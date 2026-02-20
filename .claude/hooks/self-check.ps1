param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
    [switch]$KeepArtifacts
)

$ErrorActionPreference = "Stop"

function Require-File([string]$path) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required file not found: $path"
    }
}

function Invoke-Hook([string]$hookPath, [hashtable]$payload) {
    $payloadJson = $payload | ConvertTo-Json -Compress -Depth 20
    $output = $payloadJson | & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $hookPath 2>&1
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        $outText = ($output -join [Environment]::NewLine)
        throw "Hook failed ($hookPath), exit code $exitCode. Output: $outText"
    }
    return ($output -join [Environment]::NewLine)
}

function Assert-Contains([string]$haystack, [string]$needle, [string]$label) {
    if ($haystack -notlike "*$needle*") {
        throw "Assertion failed: '$label' does not contain '$needle'."
    }
}

$hooksDir = Join-Path $ProjectRoot ".claude/hooks"
$sessionStartHook = Join-Path $hooksDir "session-start.ps1"
$userPromptHook = Join-Path $hooksDir "user-prompt-submit.ps1"
$stopHook = Join-Path $hooksDir "stop.ps1"

Require-File $sessionStartHook
Require-File $userPromptHook
Require-File $stopHook

$tmpRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("claude-hook-selfcheck-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tmpRoot -Force | Out-Null

$sessionId = "selfcheck-" + [Guid]::NewGuid().ToString("N").Substring(0, 10)
$userToken = "SELF_CHECK_USER_" + [Guid]::NewGuid().ToString("N").Substring(0, 8)
$assistantToken = "SELF_CHECK_ASSISTANT_" + [Guid]::NewGuid().ToString("N").Substring(0, 8)

$cleanupNeeded = $true
try {
    Invoke-Hook -hookPath $userPromptHook -payload @{
        cwd = $tmpRoot
        prompt = $userToken
    } | Out-Null

    Invoke-Hook -hookPath $stopHook -payload @{
        cwd = $tmpRoot
        response = $assistantToken
        session_id = $sessionId
        hook_event_name = "SessionEnd"
    } | Out-Null

    $startOutput = Invoke-Hook -hookPath $sessionStartHook -payload @{
        cwd = $tmpRoot
        session_id = $sessionId
    }

    $memoryDir = Join-Path $tmpRoot ".claude/session-memory"
    $stateFile = Join-Path $memoryDir "state.json"
    $contextFile = Join-Path $memoryDir "context.md"
    $dialogFile = Join-Path $memoryDir "dialog.md"
    $eventsFile = Join-Path $memoryDir "events.jsonl"

    Require-File $stateFile
    Require-File $contextFile
    Require-File $dialogFile
    Require-File $eventsFile

    $dialog = Get-Content -LiteralPath $dialogFile -Raw
    Assert-Contains -haystack $dialog -needle $userToken -label "dialog.md"
    Assert-Contains -haystack $dialog -needle $assistantToken -label "dialog.md"

    $stateRaw = Get-Content -LiteralPath $stateFile -Raw
    $state = $stateRaw | ConvertFrom-Json
    if ([string]$state.session_id -ne $sessionId) {
        throw "Assertion failed: state.json session_id mismatch. Expected '$sessionId', got '$($state.session_id)'."
    }
    if ([string]$state.hook_event -ne "SessionEnd") {
        throw "Assertion failed: state.json hook_event mismatch. Expected 'SessionEnd', got '$($state.hook_event)'."
    }

    $events = Get-Content -LiteralPath $eventsFile -Raw
    Assert-Contains -haystack $events -needle '"hook":"UserPromptSubmit"' -label "events.jsonl"
    Assert-Contains -haystack $events -needle '"hook":"SessionEnd"' -label "events.jsonl"

    Assert-Contains -haystack $startOutput -needle "=== RESTORED STATE ===" -label "session-start output"
    Assert-Contains -haystack $startOutput -needle "=== RESTORED CONTEXT ===" -label "session-start output"
    Assert-Contains -haystack $startOutput -needle "=== LAST DIALOG ===" -label "session-start output"
    Assert-Contains -haystack $startOutput -needle $assistantToken -label "session-start output"

    Write-Output "HOOK SELF-CHECK: PASS"
    Write-Output "Session ID: $sessionId"
    Write-Output "Artifacts: $memoryDir"

    if ($KeepArtifacts) {
        $cleanupNeeded = $false
        Write-Output "Artifacts retained due to -KeepArtifacts switch."
    }
}
finally {
    if ($cleanupNeeded -and (Test-Path -LiteralPath $tmpRoot)) {
        Remove-Item -LiteralPath $tmpRoot -Recurse -Force
    }
}
