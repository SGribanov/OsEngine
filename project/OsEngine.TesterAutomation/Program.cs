using System.Text;
using OsEngine.TesterAutomation.Models;
using OsEngine.TesterAutomation.Runtime;

Console.OutputEncoding = Encoding.UTF8;

if (TryParseArguments(args, out string scenarioPath, out string? summaryOverridePath) == false)
{
    PrintUsage();
    return 2;
}

try
{
    HarnessScenario scenario = HarnessScenarioLoader.Load(scenarioPath);

    if (!string.IsNullOrWhiteSpace(summaryOverridePath))
    {
        scenario.SummaryOutputPath = summaryOverridePath;
    }

    ScenarioRunner runner = new(TimeProvider.System);
    RunSummary summary = await runner.RunAsync(scenario, scenarioPath, CancellationToken.None);

    Console.WriteLine($"Scenario: {summary.ScenarioName}");
    Console.WriteLine($"Status:   {summary.Status}");
    Console.WriteLine($"Summary:  {summary.SummaryPath}");

    if (!string.IsNullOrWhiteSpace(summary.FailureReason))
    {
        Console.WriteLine($"Reason:   {summary.FailureReason}");
    }

    return summary.Status switch
    {
        RunStatus.Passed => 0,
        RunStatus.TimedOut => 2,
        _ => 1
    };
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);
    return 1;
}

static bool TryParseArguments(string[] args, out string scenarioPath, out string? summaryOverridePath)
{
    scenarioPath = string.Empty;
    summaryOverridePath = null;

    for (int i = 0; i < args.Length; i++)
    {
        string arg = args[i];

        if (arg.Equals("--scenario", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
        {
            scenarioPath = args[++i];
            continue;
        }

        if (arg.Equals("--summary", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
        {
            summaryOverridePath = args[++i];
            continue;
        }
    }

    return !string.IsNullOrWhiteSpace(scenarioPath);
}

static void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project project/OsEngine.TesterAutomation/OsEngine.TesterAutomation.csproj -- --scenario <path> [--summary <path>]");
}
