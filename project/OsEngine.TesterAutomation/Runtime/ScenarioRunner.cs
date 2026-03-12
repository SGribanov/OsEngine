using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using OsEngine.TesterAutomation.Models;

namespace OsEngine.TesterAutomation.Runtime;

internal sealed class ScenarioRunner
{
    private readonly TimeProvider _timeProvider;
    private readonly ProcessTreeSnapshotter _processTreeSnapshotter = new();
    private readonly UiAutomationDriver _uiAutomationDriver = new();

    public ScenarioRunner(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public async Task<RunSummary> RunAsync(
        HarnessScenario scenario,
        string scenarioPath,
        CancellationToken cancellationToken)
    {
        string workingDirectory = Path.GetFullPath(scenario.WorkingDirectory);
        string executablePath = Path.GetFullPath(scenario.ExecutablePath);
        string logDirectory = Path.Combine(workingDirectory, "Engine", "Log");
        DateTimeOffset startedAtUtc = _timeProvider.GetUtcNow();
        string summaryPath = ResolveSummaryPath(scenario, workingDirectory, startedAtUtc);

        PrepareWorkingDirectory(scenario, workingDirectory, logDirectory);

        RunSummary summary = new()
        {
            ScenarioName = scenario.Name,
            ScenarioPath = Path.GetFullPath(scenarioPath),
            WorkingDirectory = workingDirectory,
            ExecutablePath = executablePath,
            Arguments = scenario.Arguments.ToList(),
            StartedAtUtc = startedAtUtc,
            SummaryPath = summaryPath,
            Status = RunStatus.Failed
        };

        Regex? completionRegex = CreateOptionalRegex(scenario.Completion.Type, scenario.Completion.Value);
        Regex[] failureRegexes = scenario.Failure.LogRegexes
            .Where(static pattern => !string.IsNullOrWhiteSpace(pattern))
            .Select(pattern => new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase))
            .ToArray();

        LogMonitor logMonitor = new(logDirectory);
        Process? process = null;
        List<ProcessTreeEntry> previousTree = new();

        try
        {
            process = StartProcess(executablePath, workingDirectory, scenario.Arguments);
            summary.RootProcessId = process.Id;
            RecordProcessObservation(summary, "root-started", process.Id, 0, process.ProcessName);

            previousTree = _processTreeSnapshotter.CaptureProcessTree(process.Id).ToList();
            RecordTreeDiff(summary, Array.Empty<ProcessTreeEntry>(), previousTree);

            await ExecuteActionsAsync(scenario.Actions, summary, cancellationToken);

            DateTimeOffset deadline = startedAtUtc.AddSeconds(scenario.TimeoutSeconds);

            while (_timeProvider.GetUtcNow() <= deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();

                IReadOnlyList<ObservedLogLine> newLines = logMonitor.Poll();
                summary.TotalObservedLogLines += newLines.Count;
                summary.ObservedLogFiles = logMonitor.GetTrackedFiles().ToList();

                foreach (ObservedLogLine line in newLines)
                {
                    if (completionRegex is not null && completionRegex.IsMatch(line.Text))
                    {
                        summary.CompletionMatches.Add(line);
                    }

                    if (failureRegexes.Any(regex => regex.IsMatch(line.Text)))
                    {
                        summary.ErrorMatches.Add(line);
                    }
                }

                List<ProcessTreeEntry> currentTree = _processTreeSnapshotter.CaptureProcessTree(process.Id).ToList();
                RecordTreeDiff(summary, previousTree, currentTree);
                previousTree = currentTree;

                if (summary.ErrorMatches.Count > 0)
                {
                    summary.Status = RunStatus.Failed;
                    summary.FailureReason = "Failure log pattern matched.";
                    break;
                }

                if (IsCompletionSatisfied(scenario.Completion, completionRegex, summary, process))
                {
                    summary.Status = RunStatus.Passed;
                    break;
                }

                if (process.HasExited)
                {
                    summary.RootExitCode = process.ExitCode;
                    RecordProcessObservation(summary, "root-exited", process.Id, 0, process.ProcessName);

                    if (scenario.Failure.FailOnProcessExit)
                    {
                        summary.Status = RunStatus.Failed;
                        summary.FailureReason = $"Root process exited before completion. ExitCode={process.ExitCode}.";
                        break;
                    }
                }

                await Task.Delay(
                    TimeSpan.FromMilliseconds(scenario.PollIntervalMilliseconds),
                    _timeProvider,
                    cancellationToken);
            }

            if (summary.Status == RunStatus.Failed && string.IsNullOrWhiteSpace(summary.FailureReason) && summary.CompletionMatches.Count == 0)
            {
                summary.Status = RunStatus.TimedOut;
                summary.FailureReason = "Scenario timed out before completion condition matched.";
            }
        }
        catch (Exception ex)
        {
            summary.Status = RunStatus.Failed;
            summary.FailureReason = ex.Message;
        }
        finally
        {
            if (process is not null)
            {
                if (process.HasExited == false && scenario.KillProcessTreeOnExit)
                {
                    process.Kill(entireProcessTree: true);
                    RecordProcessObservation(summary, "root-killed", process.Id, 0, process.ProcessName);
                    process.WaitForExit(5000);
                }

                if (process.HasExited)
                {
                    summary.RootExitCode ??= process.ExitCode;
                }
            }

            summary.FinishedAtUtc = _timeProvider.GetUtcNow();
            WriteSummary(summaryPath, summary);
        }

        return summary;
    }

    private async Task ExecuteActionsAsync(
        IReadOnlyList<UiActionDefinition> actions,
        RunSummary summary,
        CancellationToken cancellationToken)
    {
        foreach (UiActionDefinition action in actions)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            ActionExecutionRecord record = new()
            {
                Type = action.Type,
                WindowAutomationId = action.WindowAutomationId,
                AutomationId = action.AutomationId,
                Value = action.Value
            };

            try
            {
                UiTarget target = new()
                {
                    WindowAutomationId = action.WindowAutomationId,
                    WindowName = action.WindowName,
                    AutomationId = action.AutomationId
                };

                TimeSpan timeout = TimeSpan.FromSeconds(action.TimeoutSeconds);

                switch (action.Type.Trim().ToLowerInvariant())
                {
                    case "waitwindow":
                        _uiAutomationDriver.WaitForWindow(target, timeout);
                        break;

                    case "waitelementenabled":
                        _uiAutomationDriver.WaitForElementEnabled(target, timeout);
                        break;

                    case "click":
                        _uiAutomationDriver.Click(target, timeout);
                        break;

                    case "settext":
                        _uiAutomationDriver.SetValue(
                            target,
                            action.Value ?? throw new InvalidOperationException("setText action requires 'value'."),
                            timeout);
                        break;

                    case "selectcomboboxitem":
                        _uiAutomationDriver.SelectComboBoxItem(
                            target,
                            action.Value ?? throw new InvalidOperationException("selectComboBoxItem action requires 'value'."),
                            timeout);
                        break;

                    case "closewindow":
                        _uiAutomationDriver.CloseWindow(target, timeout);
                        break;

                    case "sleep":
                        int delay = action.DelayMilliseconds > 0
                            ? action.DelayMilliseconds
                            : int.TryParse(action.Value, out int parsedDelay)
                                ? parsedDelay
                                : 1000;

                        await Task.Delay(TimeSpan.FromMilliseconds(delay), _timeProvider, cancellationToken);
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported action type '{action.Type}'.");
                }

                record.Success = true;
            }
            catch (Exception ex)
            {
                record.Success = false;
                record.Message = ex.Message;
                summary.Actions.Add(FinalizeRecord(record, stopwatch));
                throw;
            }

            summary.Actions.Add(FinalizeRecord(record, stopwatch));
        }
    }

    private static ActionExecutionRecord FinalizeRecord(ActionExecutionRecord record, Stopwatch stopwatch)
    {
        stopwatch.Stop();
        record.DurationMilliseconds = stopwatch.ElapsedMilliseconds;
        return record;
    }

    private bool IsCompletionSatisfied(
        CompletionCondition completion,
        Regex? completionRegex,
        RunSummary summary,
        Process process)
    {
        string completionType = completion.Type.Trim().ToLowerInvariant();

        return completionType switch
        {
            "logregex" => completionRegex is not null && summary.CompletionMatches.Count > 0,
            "processexit" => process.HasExited,
            "windowclosed" => !_uiAutomationDriver.IsWindowPresent(new UiTarget
            {
                WindowAutomationId = completion.WindowAutomationId,
                WindowName = completion.WindowName
            }),
            _ => throw new InvalidOperationException($"Unsupported completion type '{completion.Type}'.")
        };
    }

    private static Regex? CreateOptionalRegex(string completionType, string? value)
    {
        if (!completionType.Equals("logRegex", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Completion type 'logRegex' requires 'completion.value'.");
        }

        return new Regex(value, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }

    private static Process StartProcess(string executablePath, string workingDirectory, IReadOnlyList<string> arguments)
    {
        if (File.Exists(executablePath) == false)
        {
            throw new FileNotFoundException("Executable file was not found.", executablePath);
        }

        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                Arguments = BuildArguments(arguments)
            }
        };

        if (process.Start() == false)
        {
            throw new InvalidOperationException($"Process '{executablePath}' could not be started.");
        }

        return process;
    }

    private static string BuildArguments(IReadOnlyList<string> arguments)
    {
        return string.Join(" ", arguments.Select(QuoteArgument));
    }

    private static string QuoteArgument(string argument)
    {
        if (string.IsNullOrEmpty(argument))
        {
            return "\"\"";
        }

        return argument.Contains(' ') || argument.Contains('"')
            ? $"\"{argument.Replace("\"", "\\\"", StringComparison.Ordinal)}\""
            : argument;
    }

    private static void PrepareWorkingDirectory(HarnessScenario scenario, string workingDirectory, string logDirectory)
    {
        if (!string.IsNullOrWhiteSpace(scenario.EngineSeedPath))
        {
            DirectorySyncHelper.CopyRecursive(
                scenario.EngineSeedPath!,
                Path.Combine(workingDirectory, "Engine"));
        }

        if (scenario.ClearLogDirectory)
        {
            Directory.CreateDirectory(logDirectory);

            foreach (string file in Directory.GetFiles(logDirectory, "*.txt", SearchOption.TopDirectoryOnly))
            {
                File.Delete(file);
            }
        }
    }

    private static string ResolveSummaryPath(
        HarnessScenario scenario,
        string workingDirectory,
        DateTimeOffset startedAtUtc)
    {
        if (!string.IsNullOrWhiteSpace(scenario.SummaryOutputPath))
        {
            string fullPath = Path.GetFullPath(scenario.SummaryOutputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            return fullPath;
        }

        string fullReportsDirectory = Path.GetFullPath(
            Path.Combine(workingDirectory, "..", "..", "..", "..", "reports"));
        Directory.CreateDirectory(fullReportsDirectory);
        return Path.Combine(
            fullReportsDirectory,
            $"tester_gui_harness_{startedAtUtc:yyyy-MM-dd_HH-mm-ss}.json");
    }

    private static void WriteSummary(string summaryPath, RunSummary summary)
    {
        string json = JsonSerializer.Serialize(summary, HarnessJsonSerializerContext.Default.RunSummary);
        File.WriteAllText(summaryPath, json);
    }

    private void RecordTreeDiff(
        RunSummary summary,
        IReadOnlyList<ProcessTreeEntry> previousTree,
        IReadOnlyList<ProcessTreeEntry> currentTree)
    {
        Dictionary<int, ProcessTreeEntry> previousById = previousTree.ToDictionary(static item => item.ProcessId);
        Dictionary<int, ProcessTreeEntry> currentById = currentTree.ToDictionary(static item => item.ProcessId);

        foreach (ProcessTreeEntry added in currentTree.Where(item => previousById.ContainsKey(item.ProcessId) == false))
        {
            RecordProcessObservation(summary, "process-added", added.ProcessId, added.ParentProcessId, added.ProcessName);
        }

        foreach (ProcessTreeEntry removed in previousTree.Where(item => currentById.ContainsKey(item.ProcessId) == false))
        {
            RecordProcessObservation(summary, "process-removed", removed.ProcessId, removed.ParentProcessId, removed.ProcessName);
        }
    }

    private void RecordProcessObservation(
        RunSummary summary,
        string kind,
        int processId,
        int parentProcessId,
        string processName)
    {
        summary.ProcessEvents.Add(new ProcessObservation
        {
            TimestampUtc = _timeProvider.GetUtcNow(),
            Kind = kind,
            ProcessId = processId,
            ParentProcessId = parentProcessId,
            ProcessName = processName
        });
    }
}
