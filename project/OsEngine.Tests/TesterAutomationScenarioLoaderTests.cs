#nullable enable

using System.IO;
using OsEngine.TesterAutomation.Runtime;
using Xunit;

namespace OsEngine.Tests;

public sealed class TesterAutomationScenarioLoaderTests
{
    [Fact]
    public void Load_ShouldResolveSampleScenarioPaths_ToRepositoryLocations()
    {
        string repoRoot = FindRepositoryRoot();
        string scenarioPath = Path.Combine(
            repoRoot,
            "project",
            "OsEngine.TesterAutomation",
            "Scenarios",
            "tester-start-smoke.sample.json");

        var scenario = HarnessScenarioLoader.Load(scenarioPath);

        Assert.Equal(
            Path.Combine(repoRoot, "project", "OsEngine", "bin", "Debug"),
            scenario.WorkingDirectory);
        Assert.Equal(
            Path.Combine(repoRoot, "project", "OsEngine", "bin", "Debug", "OsEngine.exe"),
            scenario.ExecutablePath);
        Assert.Equal(
            Path.Combine(repoRoot, "docs", "samples", "tester-smoke-engine-seed"),
            scenario.EngineSeedPath);
        Assert.Equal(
            Path.Combine(repoRoot, "reports", "tester_gui_harness_smoke.json"),
            scenario.SummaryOutputPath);
        Assert.Equal(
            ["D:\\HistoryData\\OsEngineDataLoad\\Debug\\Data\\Set_BinanceFTP\\BTCUSD_PERP\\Min1\\SecurityTestSettings.txt"],
            scenario.RestoreFiles);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            string solutionPath = Path.Combine(current.FullName, "project", "OsEngine.sln");
            if (File.Exists(solutionPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root could not be resolved from test base directory.");
    }
}
