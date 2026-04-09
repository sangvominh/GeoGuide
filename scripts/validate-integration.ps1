param(
    [switch]$SkipFetch
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "MauiApp1\MauiApp1.csproj"
$windowsTarget = "net10.0-windows10.0.19041.0"

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [scriptblock]$Action
    )

    Write-Host ""
    Write-Host "==> $Name"
    & $Action
}

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$FilePath $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

Push-Location $repoRoot
try {
    if (-not $SkipFetch) {
        Invoke-Step -Name "Fetch latest remote refs" -Action {
            Invoke-CheckedCommand -FilePath "git" -Arguments @("fetch", "origin", "--prune")
        }
    }

    Invoke-Step -Name "Show working tree state" -Action {
        Invoke-CheckedCommand -FilePath "git" -Arguments @("status", "--short", "--branch")
    }

    Invoke-Step -Name "Restore MAUI project" -Action {
        Invoke-CheckedCommand -FilePath "dotnet" -Arguments @("restore", $projectPath)
    }

    Invoke-Step -Name "Build Windows target" -Action {
        Invoke-CheckedCommand -FilePath "dotnet" -Arguments @("build", $projectPath, "-f", $windowsTarget, "--no-restore")
    }
}
finally {
    Pop-Location
}
