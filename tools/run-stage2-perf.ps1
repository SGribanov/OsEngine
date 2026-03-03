param(
    [string]$Configuration = "Release",
    [switch]$NoBuild = $true,
    [int]$Repeat = 3,
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

if ($Repeat -lt 1) {
    throw "Repeat must be >= 1."
}

function Get-Median {
    param([double[]]$Values)

    if ($null -eq $Values -or $Values.Count -eq 0) {
        throw "Cannot compute median for empty values."
    }

    $sorted = $Values | Sort-Object
    $count = $sorted.Count
    $mid = [int][Math]::Floor($count / 2.0)

    if ($count % 2 -eq 1) {
        return [double]$sorted[$mid]
    }

    return ([double]$sorted[$mid - 1] + [double]$sorted[$mid]) / 2.0
}

function Read-RunMetrics {
    param(
        [string]$Path,
        [int]$RunIndex
    )

    if (-not (Test-Path $Path)) {
        throw "Metrics file not found: $Path"
    }

    $runMetrics = @()

    Get-Content $Path | ForEach-Object {
        $line = $_.Trim()
        if (-not [string]::IsNullOrWhiteSpace($line)) {
            $raw = $line | ConvertFrom-Json
            $recordedAtUtc = [string]$raw.RecordedAtUtc
            if ($raw.RecordedAtUtc -is [DateTime]) {
                $recordedAtUtc = $raw.RecordedAtUtc.ToUniversalTime().ToString("o", [System.Globalization.CultureInfo]::InvariantCulture)
            }
            $runMetrics += [pscustomobject]([ordered]@{
                    run_index = $RunIndex
                    scenario = [string]$raw.Scenario
                    iterations = [int]$raw.Iterations
                    elapsedMsTotal = [double]$raw.ElapsedMsTotal
                    nanosecondsPerOp = [double]$raw.NanosecondsPerOp
                    allocatedBytesTotal = [double]$raw.AllocatedBytesTotal
                    allocatedBytesPerOp = [double]$raw.AllocatedBytesPerOp
                    gen0Collections = [int]$raw.Gen0Collections
                    checksum = [double]$raw.Checksum
                    recordedAtUtc = $recordedAtUtc
                })
        }
    }

    return ,$runMetrics
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

$allRunMetrics = @()

for ($run = 1; $run -le $Repeat; $run++) {
    if (Test-Path $metricsPath) {
        Remove-Item $metricsPath -Force
    }

    Write-Host "Running Stage2 performance scenarios (run $run/$Repeat)..."
    & dotnet @testArgs
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed for Stage2 performance scenarios (run $run)."
    }

    $runMetrics = Read-RunMetrics -Path $metricsPath -RunIndex $run
    if ($runMetrics.Count -eq 0) {
        throw "No metrics were captured on run $run."
    }

    $allRunMetrics += $runMetrics
}

if ($allRunMetrics.Count -eq 0) {
    throw "No metrics were captured."
}

if (Test-Path $metricsPath) {
    Remove-Item $metricsPath -Force
}

foreach ($metric in $allRunMetrics) {
    ($metric | ConvertTo-Json -Compress) | Add-Content -Path $metricsPath -Encoding UTF8
}

$medianMetrics = @()
$scenarioGroups = $allRunMetrics | Group-Object scenario
foreach ($group in $scenarioGroups) {
    $rows = @($group.Group)
    $sample = $rows[0]

    $medianMetrics += [pscustomobject]([ordered]@{
            scenario = $group.Name
            iterations = [int]$sample.iterations
            repeat_count = $rows.Count
            elapsedMsTotal = [double](Get-Median -Values ($rows | ForEach-Object { [double]$_.elapsedMsTotal }))
            nanosecondsPerOp = [double](Get-Median -Values ($rows | ForEach-Object { [double]$_.nanosecondsPerOp }))
            allocatedBytesTotal = [double](Get-Median -Values ($rows | ForEach-Object { [double]$_.allocatedBytesTotal }))
            allocatedBytesPerOp = [double](Get-Median -Values ($rows | ForEach-Object { [double]$_.allocatedBytesPerOp }))
            gen0Collections = [int][Math]::Round((Get-Median -Values ($rows | ForEach-Object { [double]$_.gen0Collections })))
            checksum = [double](Get-Median -Values ($rows | ForEach-Object { [double]$_.checksum }))
        })
}

if ($medianMetrics.Count -eq 0) {
    throw "No median metrics were calculated."
}

$summary = [ordered]@{
    generated_at_utc = (Get-Date).ToUniversalTime().ToString("o")
    repeat_count = $Repeat
    metrics_file = $metricsPath
    scenarios_median = ($medianMetrics | Sort-Object scenario)
    scenarios_runs = ($allRunMetrics | Sort-Object scenario, run_index)
}

$summary | ConvertTo-Json -Depth 10 | Set-Content -Path $summaryPath -Encoding UTF8

Write-Host ""
Write-Host "Stage2 perf summary (median across runs):"
($medianMetrics | Sort-Object scenario) | Format-Table scenario, repeat_count, iterations, elapsedMsTotal, nanosecondsPerOp, allocatedBytesPerOp, gen0Collections -AutoSize

if ($EnforceThresholds) {
    if (-not (Test-Path $thresholdsPath)) {
        throw "Threshold file not found: $thresholdsPath"
    }

    $thresholds = Get-Content $thresholdsPath -Raw | ConvertFrom-Json
    $thresholdItems = @($thresholds.scenarios)
    $failed = @()

    foreach ($threshold in $thresholdItems) {
        $scenario = $medianMetrics | Where-Object { $_.scenario -eq $threshold.name } | Select-Object -First 1
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
