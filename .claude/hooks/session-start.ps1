$ErrorActionPreference = "Stop"

function Get-HookPayload {
    $raw = [Console]::In.ReadToEnd()
    if ([string]::IsNullOrWhiteSpace($raw)) {
        return @{}
    }
    try {
        $convertFromJson = Get-Command ConvertFrom-Json -ErrorAction Stop
        if ($convertFromJson.Parameters.ContainsKey("Depth")) {
            $parsed = $raw | ConvertFrom-Json -Depth 20
        }
        else {
            $parsed = $raw | ConvertFrom-Json
        }
        if ($null -eq $parsed) {
            return @{}
        }
        if ($parsed -is [hashtable]) {
            return $parsed
        }
        $hash = @{}
        foreach ($p in $parsed.PSObject.Properties) {
            $hash[$p.Name] = $p.Value
        }
        return $hash
    }
    catch {
        return @{}
    }
}

function Ensure-Dir([string]$path) {
    if (-not (Test-Path -LiteralPath $path)) {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
    }
}

$payload = Get-HookPayload
$cwd = if ($payload.ContainsKey("cwd") -and -not [string]::IsNullOrWhiteSpace([string]$payload.cwd)) { [string]$payload.cwd } else { (Get-Location).Path }
$memoryDir = Join-Path $cwd ".claude/session-memory"
Ensure-Dir $memoryDir

$contextFile = Join-Path $memoryDir "context.md"
$dialogFile = Join-Path $memoryDir "dialog.md"
$stateFile = Join-Path $memoryDir "state.json"

if (Test-Path -LiteralPath $stateFile) {
    Write-Output "=== RESTORED STATE ==="
    Get-Content -LiteralPath $stateFile -Raw | Write-Output
    Write-Output ""
}

if (Test-Path -LiteralPath $contextFile) {
    Write-Output "=== RESTORED CONTEXT ==="
    Get-Content -LiteralPath $contextFile -Raw | Write-Output
    Write-Output ""
}

if (Test-Path -LiteralPath $dialogFile) {
    Write-Output "=== LAST DIALOG ==="
    Get-Content -LiteralPath $dialogFile -Tail 120 | Write-Output
    Write-Output ""
}
