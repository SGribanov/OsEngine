param(
    [string]$RepoRoot = ".",
    [switch]$StrictAllDebugBinaries
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Push-Location $RepoRoot
try {
    $targets = @(
        "project/OsEngine/bin/Debug/OsEngine.dll",
        "project/OsEngine/bin/Debug/OsEngine.exe"
    )

    if ($StrictAllDebugBinaries) {
        $targets += @(
            "project/OsEngine/bin/Debug/*.dll",
            "project/OsEngine/bin/Debug/*.exe"
        )
    }

    $tracked = @()
    foreach ($pattern in $targets) {
        $items = git ls-files -- $pattern
        if ($LASTEXITCODE -ne 0) {
            throw "git ls-files failed for pattern: $pattern"
        }
        if ($items) {
            $tracked += $items
        }
    }

    $tracked = @($tracked | Sort-Object -Unique)

    if (@($tracked).Count -gt 0) {
        Write-Host "Tracked debug binaries found:" -ForegroundColor Red
        $tracked | ForEach-Object { Write-Host "  $_" -ForegroundColor Red }
        Write-Host ""
        Write-Host "Fix command:" -ForegroundColor Yellow
        Write-Host "  git rm --cached --ignore-unmatch project/OsEngine/bin/Debug/OsEngine.dll project/OsEngine/bin/Debug/OsEngine.exe"
        exit 1
    }

    if ($StrictAllDebugBinaries) {
        Write-Host "OK: no tracked project/OsEngine/bin/Debug/*.dll|*.exe in git index." -ForegroundColor Green
    }
    else {
        Write-Host "OK: no tracked project/OsEngine/bin/Debug/OsEngine.dll or OsEngine.exe in git index." -ForegroundColor Green
    }
    exit 0
}
finally {
    Pop-Location
}
