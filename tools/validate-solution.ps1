param(
    [switch]$SkipFetch
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$mobileProjectPath = Join-Path $repoRoot "src\GeoGuide.Mobile\GeoGuide.Mobile.csproj"
$cmsProjectPath = Join-Path $repoRoot "src\GeoGuide.Cms\GeoGuide.Cms.csproj"
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

    Invoke-Step -Name "Restore CMS project" -Action {
        Invoke-CheckedCommand -FilePath "dotnet" -Arguments @("restore", $cmsProjectPath)
    }

    Invoke-Step -Name "Build CMS project" -Action {
        Invoke-CheckedCommand -FilePath "dotnet" -Arguments @("build", $cmsProjectPath, "--no-restore")
    }

    Invoke-Step -Name "Restore mobile project" -Action {
        Invoke-CheckedCommand -FilePath "dotnet" -Arguments @("restore", $mobileProjectPath)
    }

    Invoke-Step -Name "Build mobile Windows target" -Action {
        Invoke-CheckedCommand -FilePath "dotnet" -Arguments @("build", $mobileProjectPath, "-f", $windowsTarget, "--no-restore")
    }
}
finally {
    Pop-Location
}
