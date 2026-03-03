param(
    [string]$Configuration = "Release",
    [switch]$NoBuild = $true,
    [switch]$EnforceThresholds
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$reportsDir = Join-Path $repoRoot "reports"
$metricsPath = Join-Path $reportsDir "stage2_perf_metrics.jsonl"
$summaryPath = Join-Path $reportsDir "stage2_perf_summary.json"
$thresholdsPath = Join-Path $PSScriptRoot "perf-thresholds.json"

if (-not (Test-Path $reportsDir)) {
    New-Item -ItemType Directory -Path $reportsDir | Out-Null
}

if (Test-Path $metricsPath) {
    Remove-Item $metricsPath -Force
}

$testArgs = @(
    "test",
    "project/OsEngine.Tests/OsEngine.Tests.csproj",
    "--configuration", $Configuration,
    "--nologo",
    "--filter", "FullyQualifiedName~Stage2Perf_"
)

if ($NoBuild) {
    $testArgs += "--no-build"
}

Write-Host "Running Stage2 performance scenarios..."
& dotnet @testArgs
if ($LASTEXITCODE -ne 0) {
    throw "dotnet test failed for Stage2 performance scenarios."
}

if (-not (Test-Path $metricsPath)) {
    throw "Metrics file not found: $metricsPath"
}

$metrics = @()
Get-Content $metricsPath | ForEach-Object {
    $line = $_.Trim()
    if (-not [string]::IsNullOrWhiteSpace($line)) {
        $metrics += ($line | ConvertFrom-Json)
    }
}

if ($metrics.Count -eq 0) {
    throw "No metrics were captured."
}

$summary = [ordered]@{
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    metrics_file = $metricsPath
    scenarios = $metrics
}

$summary | ConvertTo-Json -Depth 10 | Set-Content -Path $summaryPath -Encoding UTF8

Write-Host ""
Write-Host "Stage2 perf summary:"
$metrics | Sort-Object scenario | Format-Table scenario, iterations, elapsedMsTotal, nanosecondsPerOp, allocatedBytesPerOp, gen0Collections -AutoSize

if ($EnforceThresholds) {
    if (-not (Test-Path $thresholdsPath)) {
        throw "Threshold file not found: $thresholdsPath"
    }

    $thresholds = Get-Content $thresholdsPath -Raw | ConvertFrom-Json
    $thresholdItems = @($thresholds.scenarios)
    $failed = @()

    foreach ($threshold in $thresholdItems) {
        $scenario = $metrics | Where-Object { $_.scenario -eq $threshold.name } | Select-Object -Last 1
        if ($null -eq $scenario) {
            $failed += "Missing scenario metrics: $($threshold.name)"
            continue
        }

        if ($scenario.elapsedMsTotal -gt $threshold.max_elapsed_ms_total) {
            $failed += "$($threshold.name): elapsedMsTotal=$($scenario.elapsedMsTotal) > $($threshold.max_elapsed_ms_total)"
        }

        if ($scenario.allocatedBytesPerOp -gt $threshold.max_allocated_bytes_per_op) {
            $failed += "$($threshold.name): allocatedBytesPerOp=$($scenario.allocatedBytesPerOp) > $($threshold.max_allocated_bytes_per_op)"
        }

        if ($scenario.gen0Collections -gt $threshold.max_gen0_collections) {
            $failed += "$($threshold.name): gen0Collections=$($scenario.gen0Collections) > $($threshold.max_gen0_collections)"
        }
    }

    if ($failed.Count -gt 0) {
        Write-Host ""
        Write-Host "Threshold check failed:"
        $failed | ForEach-Object { Write-Host "- $_" }
        throw "Stage2 perf thresholds are violated."
    }

    Write-Host ""
    Write-Host "Threshold check passed."
}

Write-Host ""
Write-Host "Metrics JSONL: $metricsPath"
Write-Host "Summary JSON: $summaryPath"
