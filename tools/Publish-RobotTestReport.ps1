[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$InputJsonPath,

    [string]$OutputDirectory = 'reports/tests',

    [string]$RobotName,

    [string]$Ticker,

    [switch]$SkipMarkdown
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))

function Get-AbsolutePath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if ([System.IO.Path]::IsPathRooted($Path))
    {
        return [System.IO.Path]::GetFullPath($Path)
    }

    return [System.IO.Path]::GetFullPath((Join-Path $repoRoot $Path))
}

function Convert-ToHashtable {
    param(
        [object]$InputObject
    )

    if ($null -eq $InputObject)
    {
        return $null
    }

    if ($InputObject -is [System.Collections.IDictionary])
    {
        $result = [ordered]@{}

        foreach ($key in $InputObject.Keys)
        {
            $result[$key] = Convert-ToHashtable -InputObject $InputObject[$key]
        }

        return $result
    }

    if ($InputObject -is [System.Collections.IEnumerable] -and $InputObject -isnot [string])
    {
        $items = @()

        foreach ($item in $InputObject)
        {
            $items += ,(Convert-ToHashtable -InputObject $item)
        }

        return ,$items
    }

    if ($InputObject -is [pscustomobject])
    {
        $result = [ordered]@{}

        foreach ($property in $InputObject.PSObject.Properties)
        {
            $result[$property.Name] = Convert-ToHashtable -InputObject $property.Value
        }

        return $result
    }

    return $InputObject
}

function Get-NestedValue {
    param(
        [Parameter(Mandatory = $true)]
        [object]$Node,

        [Parameter(Mandatory = $true)]
        [string[]]$PathSegments
    )

    $current = $Node

    foreach ($segment in $PathSegments)
    {
        if ($null -eq $current)
        {
            return $null
        }

        if ($current -is [System.Collections.IDictionary])
        {
            if (-not $current.Contains($segment))
            {
                return $null
            }

            $current = $current[$segment]
            continue
        }

        return $null
    }

    return $current
}

function Get-ObjectEntries {
    param(
        [object]$Node
    )

    if ($null -eq $Node)
    {
        return @()
    }

    if ($Node -is [System.Collections.IDictionary])
    {
        return $Node.GetEnumerator() | Sort-Object Key
    }

    return @()
}

function Get-SafeFileSegment {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    $segment = $Value.Trim()

    if ([string]::IsNullOrWhiteSpace($segment))
    {
        return 'unknown'
    }

    foreach ($invalidCharacter in [System.IO.Path]::GetInvalidFileNameChars())
    {
        $segment = $segment.Replace([string]$invalidCharacter, '_')
    }

    $segment = [System.Text.RegularExpressions.Regex]::Replace($segment, '\s+', '_')
    $segment = $segment.Trim(' ', '.', '_')

    if ([string]::IsNullOrWhiteSpace($segment))
    {
        return 'unknown'
    }

    return $segment
}

function Get-StatisticValue {
    param(
        [Parameter(Mandatory = $true)]
        [hashtable]$Payload,

        [Parameter(Mandatory = $true)]
        [string]$Bucket,

        [Parameter(Mandatory = $true)]
        [string]$Key
    )

    $statistics = Get-NestedValue -Node $Payload -PathSegments @('Statistics', $Bucket)

    if ($statistics -is [System.Collections.IDictionary] -and $statistics.Contains($Key))
    {
        return $statistics[$Key]
    }

    return $null
}

function Add-BulletLines {
    param(
        [Parameter(Mandatory = $true)]
        [System.Text.StringBuilder]$Builder,

        [Parameter(Mandatory = $true)]
        [object]$Node
    )

    foreach ($entry in Get-ObjectEntries -Node $Node)
    {
        if ($entry.Value -is [System.Collections.IDictionary])
        {
            [void]$Builder.AppendLine("- $($entry.Key):")

            foreach ($childEntry in Get-ObjectEntries -Node $entry.Value)
            {
                [void]$Builder.AppendLine("  - $($childEntry.Key): $($childEntry.Value)")
            }

            continue
        }

        if ($entry.Value -is [System.Collections.IEnumerable] -and $entry.Value -isnot [string])
        {
            $joined = ($entry.Value | ForEach-Object { $_.ToString() }) -join ', '
            [void]$Builder.AppendLine("- $($entry.Key): $joined")
            continue
        }

        [void]$Builder.AppendLine("- $($entry.Key): $($entry.Value)")
    }
}

$inputPath = Get-AbsolutePath -Path $InputJsonPath

if (-not (Test-Path -LiteralPath $inputPath))
{
    throw "Input JSON not found: $inputPath"
}

$outputDirectoryPath = Get-AbsolutePath -Path $OutputDirectory
New-Item -ItemType Directory -Path $outputDirectoryPath -Force | Out-Null

$rawJson = Get-Content -LiteralPath $inputPath -Raw -Encoding UTF8 | ConvertFrom-Json
$rawPayload = Convert-ToHashtable -InputObject $rawJson

$startedAtText = [string](Get-NestedValue -Node $rawPayload -PathSegments @('Runtime', 'StartedAt'))
$startedAt = if ([string]::IsNullOrWhiteSpace($startedAtText))
{
    [System.DateTimeOffset]::Now
}
else
{
    [System.DateTimeOffset]::Parse($startedAtText, [System.Globalization.CultureInfo]::InvariantCulture)
}

$resolvedRobot = if (-not [string]::IsNullOrWhiteSpace($RobotName))
{
    $RobotName
}
else
{
    [string](Get-NestedValue -Node $rawPayload -PathSegments @('Settings', 'Strategy'))
}

if ([string]::IsNullOrWhiteSpace($resolvedRobot))
{
    $resolvedRobot = 'UnknownRobot'
}

$resolvedTicker = if (-not [string]::IsNullOrWhiteSpace($Ticker))
{
    $Ticker
}
else
{
    [string](Get-NestedValue -Node $rawPayload -PathSegments @('Settings', 'Security'))
}

if ([string]::IsNullOrWhiteSpace($resolvedTicker))
{
    $resolvedTicker = 'UnknownTicker'
}

$resolvedTicker = [System.IO.Path]::GetFileNameWithoutExtension($resolvedTicker)
$baseName = '{0}__{1}__{2}' -f $startedAt.ToString('yyyy-MM-dd_HH-mm-ss'), (Get-SafeFileSegment -Value $resolvedRobot), (Get-SafeFileSegment -Value $resolvedTicker)
$jsonOutputPath = Join-Path $outputDirectoryPath ($baseName + '.json')
$markdownOutputPath = Join-Path $outputDirectoryPath ($baseName + '.md')

$highlights = [ordered]@{
    NetProfit = Get-NestedValue -Node $rawPayload -PathSegments @('Trades', 'NetProfit')
    FinalPortfolio = Get-NestedValue -Node $rawPayload -PathSegments @('Portfolio', 'ValueCurrent')
    ClosedPositions = Get-NestedValue -Node $rawPayload -PathSegments @('Trades', 'ClosedPositions')
    WinRatePercent = Get-NestedValue -Node $rawPayload -PathSegments @('Trades', 'WinRatePercent')
    ProfitFactor = Get-StatisticValue -Payload $rawPayload -Bucket 'All' -Key 'Profit Factor'
    MaxDrawdownPercent = Get-StatisticValue -Payload $rawPayload -Bucket 'All' -Key 'Максимальная просадка, %'
    Sharpe = Get-StatisticValue -Payload $rawPayload -Bucket 'All' -Key 'Шарп'
    Recovery = Get-StatisticValue -Payload $rawPayload -Bucket 'All' -Key 'Recovery'
}

$publishedPayload = [ordered]@{
    Status = $rawPayload['Status']
    ReportPath = $jsonOutputPath
    MarkdownReportPath = if ($SkipMarkdown) { $null } else { $markdownOutputPath }
    SourceReportPath = $inputPath
    PublishedAt = [System.DateTimeOffset]::Now.ToString('O')
    ReportIdentity = [ordered]@{
        StartedAt = $startedAt.ToString('O')
        Robot = $resolvedRobot
        Ticker = $resolvedTicker
        TimeFrame = Get-NestedValue -Node $rawPayload -PathSegments @('Settings', 'TimeFrame')
    }
    Highlights = $highlights
    Settings = $rawPayload['Settings']
    Runtime = $rawPayload['Runtime']
    Portfolio = $rawPayload['Portfolio']
    Trades = $rawPayload['Trades']
    Statistics = $rawPayload['Statistics']
    RecentLogs = $rawPayload['RecentLogs']
}

if ($rawPayload.Contains('ErrorLogs'))
{
    $publishedPayload['ErrorLogs'] = $rawPayload['ErrorLogs']
}

$jsonContent = $publishedPayload | ConvertTo-Json -Depth 100
[System.IO.File]::WriteAllText($jsonOutputPath, $jsonContent, [System.Text.UTF8Encoding]::new($false))

if (-not $SkipMarkdown)
{
    $builder = [System.Text.StringBuilder]::new()

    [void]$builder.AppendLine("# Отчет тестирования робота $resolvedRobot")
    [void]$builder.AppendLine()
    [void]$builder.AppendLine("- Дата запуска: $($startedAt.ToString('yyyy-MM-dd HH:mm:ss zzz'))")
    [void]$builder.AppendLine("- Робот: $resolvedRobot")
    [void]$builder.AppendLine("- Инструмент: $resolvedTicker")
    [void]$builder.AppendLine("- Таймфрейм: $(Get-NestedValue -Node $rawPayload -PathSegments @('Settings', 'TimeFrame'))")
    [void]$builder.AppendLine("- Статус: $($rawPayload['Status'])")
    [void]$builder.AppendLine("- Исходный сырой артефакт: $inputPath")
    [void]$builder.AppendLine("- Финальный JSON: $jsonOutputPath")
    [void]$builder.AppendLine()

    [void]$builder.AppendLine("## Ключевой результат")
    [void]$builder.AppendLine()
    [void]$builder.AppendLine("- Чистая прибыль: $($highlights['NetProfit'])")
    [void]$builder.AppendLine("- Итоговый портфель: $($highlights['FinalPortfolio'])")
    [void]$builder.AppendLine("- Закрытых сделок: $($highlights['ClosedPositions'])")
    [void]$builder.AppendLine("- Winrate, %: $($highlights['WinRatePercent'])")
    [void]$builder.AppendLine("- Profit Factor: $($highlights['ProfitFactor'])")
    [void]$builder.AppendLine("- Максимальная просадка, %: $($highlights['MaxDrawdownPercent'])")
    [void]$builder.AppendLine("- Sharpe: $($highlights['Sharpe'])")
    [void]$builder.AppendLine("- Recovery: $($highlights['Recovery'])")
    [void]$builder.AppendLine()

    [void]$builder.AppendLine("## Настройки")
    [void]$builder.AppendLine()
    Add-BulletLines -Builder $builder -Node $rawPayload['Settings']
    [void]$builder.AppendLine()

    [void]$builder.AppendLine("## Runtime")
    [void]$builder.AppendLine()
    Add-BulletLines -Builder $builder -Node $rawPayload['Runtime']
    [void]$builder.AppendLine()

    [void]$builder.AppendLine("## Сделки и портфель")
    [void]$builder.AppendLine()
    Add-BulletLines -Builder $builder -Node $rawPayload['Trades']
    [void]$builder.AppendLine()

    [void]$builder.AppendLine("## Статистика OsEngine")
    [void]$builder.AppendLine()
    [void]$builder.AppendLine("### All")
    [void]$builder.AppendLine()
    Add-BulletLines -Builder $builder -Node (Get-NestedValue -Node $rawPayload -PathSegments @('Statistics', 'All'))
    [void]$builder.AppendLine()
    [void]$builder.AppendLine("### Long")
    [void]$builder.AppendLine()
    Add-BulletLines -Builder $builder -Node (Get-NestedValue -Node $rawPayload -PathSegments @('Statistics', 'Long'))
    [void]$builder.AppendLine()
    [void]$builder.AppendLine("### Short")
    [void]$builder.AppendLine()
    Add-BulletLines -Builder $builder -Node (Get-NestedValue -Node $rawPayload -PathSegments @('Statistics', 'Short'))
    [void]$builder.AppendLine()

    $errorLogs = if ($rawPayload.Contains('ErrorLogs')) { @($rawPayload['ErrorLogs']) } else { @() }
    [void]$builder.AppendLine("## Ошибки и логи")
    [void]$builder.AppendLine()
    [void]$builder.AppendLine("- Количество error logs: $(@($errorLogs).Count)")
    [void]$builder.AppendLine("- Количество recent logs: $(@($rawPayload['RecentLogs']).Count)")
    [void]$builder.AppendLine()

    if (@($errorLogs).Count -gt 0)
    {
        [void]$builder.AppendLine("### Error logs")
        [void]$builder.AppendLine()

        foreach ($errorLog in $errorLogs)
        {
            [void]$builder.AppendLine("- [$($errorLog['Time'])] $($errorLog['Type']): $($errorLog['Message'])")
        }

        [void]$builder.AppendLine()
    }

    [System.IO.File]::WriteAllText($markdownOutputPath, $builder.ToString(), [System.Text.UTF8Encoding]::new($false))
}

Write-Output "JSON report: $jsonOutputPath"

if (-not $SkipMarkdown)
{
    Write-Output "Markdown report: $markdownOutputPath"
}
