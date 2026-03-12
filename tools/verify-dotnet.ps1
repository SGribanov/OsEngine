param(
    [string]$Configuration = "Release",
    [switch]$NoRestore,
    [switch]$SkipBuild,
    [switch]$SkipTest
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repoRoot "project/OsEngine.sln"
$projectPath = Join-Path $repoRoot "project/OsEngine/OsEngine.csproj"
$testerAutomationProjectPath = Join-Path $repoRoot "project/OsEngine.TesterAutomation/OsEngine.TesterAutomation.csproj"
$testProjectPath = Join-Path $repoRoot "project/OsEngine.Tests/OsEngine.Tests.csproj"

function Invoke-Step {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    Write-Host "==> $Name"
    & dotnet @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "$Name failed with exit code $LASTEXITCODE"
    }
}

Push-Location $repoRoot

try {
    Invoke-Step -Name "dotnet build-server shutdown (pre)" -Arguments @("build-server", "shutdown")

    if (-not $NoRestore) {
        Invoke-Step -Name "dotnet restore" -Arguments @("restore", $solutionPath)
    }

    if (-not $SkipBuild) {
        $buildArgs = @(
            "build",
            $projectPath,
            "--configuration", $Configuration,
            "--disable-build-servers"
        )

        if ($NoRestore) {
            $buildArgs += "--no-restore"
        }

        Invoke-Step -Name "dotnet build" -Arguments $buildArgs

        if (Test-Path $testerAutomationProjectPath) {
            $testerAutomationBuildArgs = @(
                "build",
                $testerAutomationProjectPath,
                "--configuration", $Configuration,
                "--disable-build-servers"
            )

            if ($NoRestore) {
                $testerAutomationBuildArgs += "--no-restore"
            }

            Invoke-Step -Name "dotnet build tester automation" -Arguments $testerAutomationBuildArgs
        }

        $testBuildArgs = @(
            "build",
            $testProjectPath,
            "--configuration", $Configuration,
            "--disable-build-servers"
        )

        if ($NoRestore) {
            $testBuildArgs += "--no-restore"
        }
        else {
            $testBuildArgs += "--no-dependencies"
        }

        Invoke-Step -Name "dotnet build tests" -Arguments $testBuildArgs
    }

    if (-not $SkipTest) {
        $testArgs = @(
            "test",
            $testProjectPath,
            "--configuration", $Configuration,
            "--no-build",
            "--disable-build-servers"
        )

        if ($NoRestore) {
            $testArgs += "--no-restore"
        }

        Invoke-Step -Name "dotnet test" -Arguments $testArgs
    }
}
finally {
    try {
        Invoke-Step -Name "dotnet build-server shutdown (post)" -Arguments @("build-server", "shutdown")
    }
    catch {
        Write-Warning $_
    }

    Pop-Location
}
