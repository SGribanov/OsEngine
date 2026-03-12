namespace OsEngine.TesterAutomation.Models;

public enum RunStatus
{
    Passed,
    Failed,
    TimedOut
}

public sealed class RunSummary
{
    public string ScenarioName { get; set; } = string.Empty;

    public string ScenarioPath { get; set; } = string.Empty;

    public string WorkingDirectory { get; set; } = string.Empty;

    public string ExecutablePath { get; set; } = string.Empty;

    public List<string> Arguments { get; set; } = new();

    public RunStatus Status { get; set; } = RunStatus.Failed;

    public string? FailureReason { get; set; }

    public DateTimeOffset StartedAtUtc { get; set; }

    public DateTimeOffset FinishedAtUtc { get; set; }

    public string SummaryPath { get; set; } = string.Empty;

    public int RootProcessId { get; set; }

    public int? RootExitCode { get; set; }

    public int TotalObservedLogLines { get; set; }

    public List<string> ObservedLogFiles { get; set; } = new();

    public List<ObservedLogLine> CompletionMatches { get; set; } = new();

    public List<ObservedLogLine> ErrorMatches { get; set; } = new();

    public List<ActionExecutionRecord> Actions { get; set; } = new();

    public List<ProcessObservation> ProcessEvents { get; set; } = new();
}

public sealed class ObservedLogLine
{
    public string FileName { get; set; } = string.Empty;

    public int LineNumber { get; set; }

    public string Text { get; set; } = string.Empty;
}

public sealed class ActionExecutionRecord
{
    public string Type { get; set; } = string.Empty;

    public string? WindowAutomationId { get; set; }

    public string? AutomationId { get; set; }

    public string? Value { get; set; }

    public bool Success { get; set; }

    public string? Message { get; set; }

    public long DurationMilliseconds { get; set; }
}

public sealed class ProcessObservation
{
    public DateTimeOffset TimestampUtc { get; set; }

    public string Kind { get; set; } = string.Empty;

    public int ProcessId { get; set; }

    public int ParentProcessId { get; set; }

    public string ProcessName { get; set; } = string.Empty;
}
