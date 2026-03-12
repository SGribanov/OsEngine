using System.Text.Json.Serialization;

namespace OsEngine.TesterAutomation.Models;

public sealed class HarnessScenario
{
    public string Name { get; set; } = "tester-automation";

    public string WorkingDirectory { get; set; } = ".";

    public string ExecutablePath { get; set; } = "OsEngine.exe";

    public List<string> Arguments { get; set; } = new() { "-tester" };

    public string? EngineSeedPath { get; set; }

    public bool ClearLogDirectory { get; set; } = true;

    public bool KillProcessTreeOnExit { get; set; } = true;

    public int TimeoutSeconds { get; set; } = 300;

    public int PollIntervalMilliseconds { get; set; } = 1000;

    public string? SummaryOutputPath { get; set; }

    public List<UiActionDefinition> Actions { get; set; } = new();

    public CompletionCondition Completion { get; set; } = new();

    public FailureRules Failure { get; set; } = new();
}

public sealed class UiActionDefinition
{
    public string Type { get; set; } = string.Empty;

    public string? WindowAutomationId { get; set; }

    public string? WindowName { get; set; }

    public string? AutomationId { get; set; }

    public string? Value { get; set; }

    public int TimeoutSeconds { get; set; } = 30;

    public int DelayMilliseconds { get; set; }
}

public sealed class CompletionCondition
{
    public string Type { get; set; } = "logRegex";

    public string? Value { get; set; }

    public string? WindowAutomationId { get; set; }

    public string? WindowName { get; set; }
}

public sealed class FailureRules
{
    public List<string> LogRegexes { get; set; } = new() { ";Error;" };

    public bool FailOnProcessExit { get; set; } = true;
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(HarnessScenario))]
[JsonSerializable(typeof(RunSummary))]
internal sealed partial class HarnessJsonSerializerContext : JsonSerializerContext;
