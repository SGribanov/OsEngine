$ErrorActionPreference = "Stop"

function Get-HookPayload {
    $raw = [Console]::In.ReadToEnd()
    if ([string]::IsNullOrWhiteSpace($raw)) {
        return @{ Raw = "" }
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
            $json = @{}
        }
        elseif ($parsed -is [hashtable]) {
            $json = $parsed
        }
        else {
            $json = @{}
            foreach ($p in $parsed.PSObject.Properties) {
                $json[$p.Name] = $p.Value
            }
        }
        $json["__raw"] = $raw
        return $json
    }
    catch {
        return @{ Raw = $raw }
    }
}

function Ensure-Dir([string]$path) {
    if (-not (Test-Path -LiteralPath $path)) {
        New-Item -ItemType Directory -Path $path -Force | Out-Null
    }
}

function Append-Utf8([string]$path, [string]$text) {
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::AppendAllText($path, $text, $utf8NoBom)
}

function Trim-FileTail([string]$path, [int]$maxLines, [int]$keepLines) {
    if (-not (Test-Path -LiteralPath $path)) {
        return
    }
    $lines = Get-Content -LiteralPath $path
    if ($lines.Count -le $maxLines) {
        return
    }
    $tail = $lines | Select-Object -Last $keepLines
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllLines($path, $tail, $utf8NoBom)
}

$payload = Get-HookPayload
$cwd = if ($payload.ContainsKey("cwd") -and -not [string]::IsNullOrWhiteSpace([string]$payload.cwd)) { [string]$payload.cwd } else { (Get-Location).Path }
$prompt = ""
if ($payload.ContainsKey("prompt")) { $prompt = [string]$payload.prompt }
elseif ($payload.ContainsKey("message")) { $prompt = [string]$payload.message }
elseif ($payload.ContainsKey("input")) { $prompt = [string]$payload.input }
elseif ($payload.ContainsKey("text")) { $prompt = [string]$payload.text }
elseif ($payload.ContainsKey("user_prompt")) { $prompt = [string]$payload.user_prompt }

if ([string]::IsNullOrWhiteSpace($prompt) -and $payload.ContainsKey("__raw")) {
    $prompt = "[raw payload] " + [string]$payload["__raw"]
}
elseif ([string]::IsNullOrWhiteSpace($prompt) -and $payload.ContainsKey("Raw")) {
    $prompt = "[raw payload] " + [string]$payload["Raw"]
}

$memoryDir = Join-Path $cwd ".claude/session-memory"
Ensure-Dir $memoryDir
$dialogFile = Join-Path $memoryDir "dialog.md"
$eventsFile = Join-Path $memoryDir "events.jsonl"
$timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")

if (-not [string]::IsNullOrWhiteSpace($prompt)) {
    $entry = "`r`n---`r`n**[$timestamp] USER:**`r`n$prompt`r`n"
    Append-Utf8 -path $dialogFile -text $entry
}

$eventObj = @{
    ts = (Get-Date).ToString("o")
    hook = "UserPromptSubmit"
    cwd = $cwd
    prompt = $prompt
    payload_keys = @($payload.Keys)
}
Append-Utf8 -path $eventsFile -text (($eventObj | ConvertTo-Json -Compress) + "`n")

Trim-FileTail -path $dialogFile -maxLines 2400 -keepLines 1600
Trim-FileTail -path $eventsFile -maxLines 6000 -keepLines 4000
