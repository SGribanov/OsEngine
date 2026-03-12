# GUI automation harness for Tester

`project/OsEngine.TesterAutomation` is a standalone harness for repeatable Tester GUI runs.

What it does:
- starts `OsEngine.exe` directly in tester mode via `-tester`
- optionally seeds `Engine\*` from a prepared fixture directory
- executes a small set of UI Automation actions against WPF controls with `AutomationId`
- tails `Engine\Log\*.txt`
- records process tree changes and writes a JSON summary into `reports/`

## Scenario file

Use the sample scenarios in `project/OsEngine.TesterAutomation/Scenarios/` as templates.

Available samples:
- `tester-launch-smoke.sample.json`: launches `OsEngine.exe -tester`, waits for the Tester window, closes it, confirms the shutdown dialog, and verifies clean process exit.
- `tester-start-smoke.sample.json`: opens Tester server settings and presses `Start test`; this requires a valid local seed in `docs/samples/tester-smoke-engine-seed`.

Key fields:
- `workingDirectory`: folder that contains `OsEngine.exe` and `Engine\`
- `executablePath`: usually `OsEngine.exe`
- `engineSeedPath`: optional fixture directory copied into `workingDirectory\\Engine`
- `restoreFiles`: optional absolute or scenario-relative file paths that should be restored after the run; use this for external `SecurityTestSettings.txt`-style dependencies
- `actions`: ordered UI steps such as `waitWindow`, `click`, `setText`, `selectComboBoxItem`
- `completion`: pass condition, for example a log regex or process exit
- `failure`: log regexes and process-exit behavior that should fail the run

## Local run

1. Build OsEngine in `Debug`.
2. Pick the sample scenario that matches the goal:
   - use `tester-launch-smoke.sample.json` for a fixture-free launch/automation smoke;
   - use `tester-start-smoke.sample.json` for a test-start flow with local Tester data.
3. Adjust the scenario if the target data set or dates differ on the machine.
4. Run:

```powershell
dotnet run --project project/OsEngine.TesterAutomation/OsEngine.TesterAutomation.csproj -- --scenario project/OsEngine.TesterAutomation/Scenarios/tester-launch-smoke.sample.json
```

The harness writes a JSON result to `reports/` by default, or to `summaryOutputPath` if the scenario overrides it.

## Current limits

- The harness intentionally prefers file-seeded setup plus targeted UI actions over full visual scraping.
- Completion and failure are log/process-driven; chart contents are not parsed from the GUI.
- Full test-start execution still depends on a valid local Tester data fixture in `docs/samples/tester-smoke-engine-seed`.
- The sample start-smoke fixture points to a local history folder; if that path differs on the machine, update `TestServer.txt` and the scenario `restoreFiles` entry accordingly.
