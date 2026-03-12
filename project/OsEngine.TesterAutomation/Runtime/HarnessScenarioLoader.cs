using System.IO;
using System.Text.Json;
using OsEngine.TesterAutomation.Models;

namespace OsEngine.TesterAutomation.Runtime;

public static class HarnessScenarioLoader
{
    public static HarnessScenario Load(string scenarioPath)
    {
        string fullScenarioPath = Path.GetFullPath(scenarioPath);

        if (File.Exists(fullScenarioPath) == false)
        {
            throw new FileNotFoundException("Scenario file was not found.", fullScenarioPath);
        }

        HarnessScenario? scenario = JsonSerializer.Deserialize(
            File.ReadAllText(fullScenarioPath),
            HarnessJsonSerializerContext.Default.HarnessScenario);

        if (scenario is null)
        {
            throw new InvalidOperationException($"Scenario '{fullScenarioPath}' could not be parsed.");
        }

        string scenarioDirectory = Path.GetDirectoryName(fullScenarioPath)
            ?? throw new InvalidOperationException("Scenario directory could not be resolved.");

        scenario.WorkingDirectory = ResolvePath(scenarioDirectory, scenario.WorkingDirectory);

        if (!string.IsNullOrWhiteSpace(scenario.EngineSeedPath))
        {
            scenario.EngineSeedPath = ResolvePath(scenarioDirectory, scenario.EngineSeedPath);
        }

        if (!string.IsNullOrWhiteSpace(scenario.SummaryOutputPath))
        {
            scenario.SummaryOutputPath = ResolvePath(scenarioDirectory, scenario.SummaryOutputPath);
        }

        if (Path.IsPathRooted(scenario.ExecutablePath) == false)
        {
            scenario.ExecutablePath = Path.GetFullPath(
                Path.Combine(scenario.WorkingDirectory, scenario.ExecutablePath));
        }

        if (scenario.TimeoutSeconds <= 0)
        {
            throw new InvalidOperationException("Scenario timeout must be greater than zero.");
        }

        if (scenario.PollIntervalMilliseconds <= 0)
        {
            scenario.PollIntervalMilliseconds = 1000;
        }

        return scenario;
    }

    private static string ResolvePath(string baseDirectory, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return baseDirectory;
        }

        if (Path.IsPathRooted(path))
        {
            return Path.GetFullPath(path);
        }

        return Path.GetFullPath(Path.Combine(baseDirectory, path));
    }
}
