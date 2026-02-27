param(
    [string]$MergeCommit = "HEAD",
    [string]$RepoRoot = "."
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Push-Location $RepoRoot
try {
    $parent1 = git rev-parse "$MergeCommit^1" 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to resolve first parent for merge commit '$MergeCommit'."
    }

    $parent2 = git rev-parse "$MergeCommit^2" 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to resolve second parent for merge commit '$MergeCommit'. Ensure this is a merge commit."
    }

    $parent1 = $parent1.Trim()
    $parent2 = $parent2.Trim()
    $range = "$parent1..$parent2"

    $upstreamCommitSet = @{}
    git rev-list $range | ForEach-Object {
        $sha = $_.Trim()
        if ($sha) {
            $upstreamCommitSet[$sha] = $true
        }
    }

    $files = git diff --name-only $range | Where-Object { $_ -like "*.cs" }

    $checks = @(
        @{
            Name = "step0_3_silent_catch"
            Pattern = '^\s*catch\s*\{\s*\}$'
            Description = "Silent catch block"
        },
        @{
            Name = "step4_1_object_lock"
            Pattern = 'new object\(\)'
            Description = "Object lock field"
        },
        @{
            Name = "step2_2_culture_replace_parse"
            Pattern = 'decimal\.(Parse|TryParse)\(.*Replace\(\"\.\"\s*,\s*\",\"\)'
            Description = "Culture-dependent decimal parse with Replace"
        },
        @{
            Name = "step2_2_convert_decimal_reader"
            Pattern = 'Convert\.ToDecimal\(reader\.ReadLine\(\)\)'
            Description = "Convert.ToDecimal(reader.ReadLine())"
        },
        @{
            Name = "step2_1_streamwriter_write"
            Pattern = 'new StreamWriter\('
            Description = "Direct StreamWriter write path (non-atomic candidate)"
        },
        @{
            Name = "step2_1_file_writeall"
            Pattern = '\bFile\.WriteAll(Text|Lines|Bytes)\('
            Description = "Direct File.WriteAll* path (non-atomic candidate)"
        },
        @{
            Name = "step2_1_filestream_create_append"
            Pattern = 'new FileStream\(.*FileMode\.(Create|OpenOrCreate|Append)'
            Description = "Direct FileStream create/append path (non-atomic candidate)"
        }
    )

    $findings = New-Object System.Collections.Generic.List[object]

    foreach ($file in $files) {
        if (-not (Test-Path $file)) {
            continue
        }

        foreach ($check in $checks) {
            $hits = rg -n --no-heading --color never --pcre2 $check.Pattern -- $file 2>$null
            foreach ($hit in $hits) {
                if ($hit -notmatch '^.*?:(\d+):') {
                    continue
                }

                $lineNumber = [int]$matches[1]
                $blame = git blame -L "$lineNumber,$lineNumber" --porcelain -- "$file" | Select-Object -First 1
                if ([string]::IsNullOrWhiteSpace($blame)) {
                    continue
                }

                $commitSha = ($blame -split ' ')[0]
                if (-not $upstreamCommitSet.ContainsKey($commitSha)) {
                    continue
                }

                $firstColon = $hit.IndexOf(':')
                $secondColon = $hit.IndexOf(':', $firstColon + 1)
                $code = if ($secondColon -ge 0) { $hit.Substring($secondColon + 1).Trim() } else { $hit }

                $findings.Add([pscustomobject]@{
                    Check = $check.Name
                    File = $file
                    Line = $lineNumber
                    Commit = $commitSha
                    Code = $code
                })
            }
        }
    }

    Write-Host "Merge commit: $MergeCommit"
    Write-Host "Upstream range: $range"
    Write-Host "Files scanned: $($files.Count)"

    if ($findings.Count -eq 0) {
        Write-Host "OK: no upstream-attributed replay findings for configured checks." -ForegroundColor Green
        exit 0
    }

    Write-Host "Findings: $($findings.Count)" -ForegroundColor Red
    $findings |
        Sort-Object Check, File, Line |
        Format-Table Check, File, Line, Commit -AutoSize

    Write-Host ""
    Write-Host "Details:" -ForegroundColor Yellow
    $findings | Sort-Object Check, File, Line | ForEach-Object {
        Write-Host ("[{0}] {1}:{2}" -f $_.Check, $_.File, $_.Line)
        Write-Host ("  commit: {0}" -f $_.Commit)
        Write-Host ("  code:   {0}" -f $_.Code)
    }

    exit 1
}
finally {
    Pop-Location
}
