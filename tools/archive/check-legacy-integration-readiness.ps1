param(
    [string]$Remote = "origin",
    [string]$IntegrationBranch = "codex/integration-mvp",
    [string[]]$SourceBranches = @(
        "codex/mobile-mvp",
        "codex/cms-mvp",
        "codex/analysis-mvp"
    )
)

$ErrorActionPreference = "Stop"

function Invoke-Git {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $output = & git @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "git $($Arguments -join ' ') failed.`n$output"
    }

    return $output
}

Write-Host "Fetching latest remote refs from $Remote..."
Invoke-Git -Arguments @("fetch", $Remote, "--prune") | Out-Null

$integrationRef = "$Remote/$IntegrationBranch"
Invoke-Git -Arguments @("rev-parse", "--verify", $integrationRef) | Out-Null

$results = foreach ($branch in $SourceBranches) {
    $sourceRef = "$Remote/$branch"
    Invoke-Git -Arguments @("rev-parse", "--verify", $sourceRef) | Out-Null

    $counts = (Invoke-Git -Arguments @("rev-list", "--left-right", "--count", "$integrationRef...$sourceRef")).Trim() -split "\s+"
    $integrationOnly = [int]$counts[0]
    $sourceOnly = [int]$counts[1]

    $mergeBase = (Invoke-Git -Arguments @("merge-base", $integrationRef, $sourceRef)).Trim()
    $integrationHead = (Invoke-Git -Arguments @("rev-parse", $integrationRef)).Trim()
    $sourceHead = (Invoke-Git -Arguments @("rev-parse", $sourceRef)).Trim()

    $status =
        if ($sourceOnly -eq 0) {
            "WAITING"
        }
        elseif ($integrationOnly -eq 0) {
            "READY"
        }
        else {
            "DIVERGED"
        }

    [pscustomobject]@{
        Branch = $branch
        Status = $status
        CommitsOnlyInIntegration = $integrationOnly
        CommitsOnlyInSource = $sourceOnly
        MergeBase = $mergeBase
        IntegrationHead = $integrationHead
        SourceHead = $sourceHead
    }
}

$results | Format-Table -AutoSize

$notReady = $results | Where-Object { $_.Status -eq "WAITING" }
if ($notReady.Count -gt 0) {
    Write-Warning "Some source branches do not have remote-only commits yet. Merge should wait."
}

$diverged = $results | Where-Object { $_.Status -eq "DIVERGED" }
if ($diverged.Count -gt 0) {
    Write-Warning "Some source branches have diverged from integration. Merge is possible, but conflict review will likely be needed."
}
