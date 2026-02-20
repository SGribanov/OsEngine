$ErrorActionPreference = "Stop"

function Get-HookPayload {
    $raw = [Console]::In.ReadToEnd()
    if ([string]::IsNullOrWhiteSpace($raw)) {
        return @{ __raw = "" }
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
        return @{ __raw = $raw }
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
$sessionId = ""
if ($payload.ContainsKey("session_id")) { $sessionId = [string]$payload.session_id }
elseif ($payload.ContainsKey("sessionId")) { $sessionId = [string]$payload.sessionId }

$hookEvent = "Stop"
if ($payload.ContainsKey("hook_event_name")) { $hookEvent = [string]$payload.hook_event_name }
elseif ($payload.ContainsKey("hookEventName")) { $hookEvent = [string]$payload.hookEventName }

$memoryDir = Join-Path $cwd ".claude/session-memory"
Ensure-Dir $memoryDir
$stateFile = Join-Path $memoryDir "state.json"
$contextFile = Join-Path $memoryDir "context.md"
$dialogFile = Join-Path $memoryDir "dialog.md"
$eventsFile = Join-Path $memoryDir "events.jsonl"

$isGitRepo = $false
$branch = ""
$lastCommits = @()
$statusShort = @()

try {
    $inside = & git -C $cwd rev-parse --is-inside-work-tree 2>$null
    $isGitRepo = ($inside -eq "true")
}
catch {
    $isGitRepo = $false
}

if ($isGitRepo) {
    $branch = (& git -C $cwd branch --show-current 2>$null)
    $lastCommits = @(& git -C $cwd log --oneline -5 2>$null)
    $statusShort = @(& git -C $cwd status --short 2>$null)
}

$state = @{
    saved_at = (Get-Date).ToString("o")
    cwd = $cwd
    session_id = $sessionId
    hook_event = $hookEvent
    git = @{
        is_repo = $isGitRepo
        branch = $branch
        last_commits = $lastCommits
        status_short = $statusShort
    }
}

$state | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $stateFile -Encoding UTF8

$context = @()
$context += "# Session Context"
$context += ""
$context += "- Saved at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$context += "- Project path: $cwd"
if (-not [string]::IsNullOrWhiteSpace($sessionId)) {
    $context += "- Session ID: $sessionId"
}
$context += ""
$context += "## Git State"
if ($isGitRepo) {
    $context += "- Branch: $branch"
    $context += "- Recent commits:"
    $context += '```'
    if ($lastCommits.Count -gt 0) {
        $context += ($lastCommits -join [Environment]::NewLine)
    }
    else {
        $context += "(no data)"
    }
    $context += '```'
    $context += "- Modified files:"
    $context += '```'
    if ($statusShort.Count -gt 0) {
        $context += ($statusShort -join [Environment]::NewLine)
    }
    else {
        $context += "(clean)"
    }
    $context += '```'
}
else {
    $context += "Git repository not detected."
}

Set-Content -LiteralPath $contextFile -Encoding UTF8 -Value ($context -join "`r`n")

$assistantText = ""
$candidateFields = @("response", "assistant_response", "output", "completion", "message")
foreach ($field in $candidateFields) {
    if ($payload.ContainsKey($field) -and -not [string]::IsNullOrWhiteSpace([string]$payload[$field])) {
        $assistantText = [string]$payload[$field]
        break
    }
}

if ([string]::IsNullOrWhiteSpace($assistantText) -and $payload.ContainsKey("__raw")) {
    $assistantText = "[raw payload] " + [string]$payload["__raw"]
}
elseif ([string]::IsNullOrWhiteSpace($assistantText) -and $payload.ContainsKey("Raw")) {
    $assistantText = "[raw payload] " + [string]$payload["Raw"]
}

if (-not [string]::IsNullOrWhiteSpace($assistantText)) {
    $timestamp = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    $entry = "`r`n---`r`n**[$timestamp] ASSISTANT:**`r`n$assistantText`r`n"
    Append-Utf8 -path $dialogFile -text $entry
}

$eventObj = @{
    ts = (Get-Date).ToString("o")
    hook = $hookEvent
    cwd = $cwd
    session_id = $sessionId
    payload_keys = @($payload.Keys)
}
Append-Utf8 -path $eventsFile -text (($eventObj | ConvertTo-Json -Compress) + "`n")

Trim-FileTail -path $dialogFile -maxLines 2400 -keepLines 1600
Trim-FileTail -path $eventsFile -maxLines 6000 -keepLines 4000

Write-Output "Session snapshot saved: $stateFile"
