# Optimizer Refactor Report

Last updated: 2026-02-14

## Scope
This report tracks all optimizer-related refactoring done in the current workstream and must be kept up to date after each meaningful change.

## Summary
Implemented and committed:
1. Phase 1 foundation (SRP decomposition + wiring in master/executor).
2. Phase 2 partial threading improvements (removed core busy-wait loops in optimizer execution and async bot factory path).
3. Phase 2 continuation (in progress): stop/cancel path in `OptimizerExecutor` moved to `CancellationTokenSource`.
4. Phase 3 started: `OptimizerReport` serialization extracted to dedicated serializer with version prefix and legacy fallback.
5. Phase 4 started: optimization strategy abstraction interfaces and brute-force strategy scaffold added.
6. Phase 5 started: bayesian strategy skeleton added and wired via factory with safe brute-force backend delegation.
7. Phase 5 continued: bayesian staged candidate loop implemented (`initial sampling + iterative batches`) with limits by `InitialSamples/MaxIterations/BatchSize`.
8. Phase 5 continued: optimizer UI now exposes and persists optimization method/objective/bayesian numeric settings.
9. Stabilization continued: added persistence tests for optimizer method settings (`OptimizationMethod`, `ObjectiveMetric`, `Bayesian*`) including legacy file compatibility.
10. Phase 5 continued: candidate selection policy extracted from strategy runtime into dedicated `BayesianCandidateSelector`.

## Commits
- `b1e5eabe3` — `Optimizer: persist Phase1 extraction and wiring state`
- `004fef95c` — `Optimizer: remove busy-wait in executor and async bot factory`

## Phase 1 Changes (SRP Decomposition)
### New files
- `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
- `project/OsEngine/OsOptimizer/OptEntity/OptimizerFilterManager.cs`
- `project/OsEngine/OsOptimizer/OptEntity/PhaseCalculator.cs`
- `project/OsEngine/OsOptimizer/OptEntity/ParameterIterator.cs`
- `project/OsEngine/OsOptimizer/OptEntity/BotConfigurator.cs`
- `project/OsEngine/OsOptimizer/OptEntity/ServerLifecycleManager.cs`
- `project/OsEngine/OsOptimizer/OptEntity/OptimizerEnums.cs`

### Core wiring
- `OptimizerMaster` now delegates persistent settings/filtering/phase calculations via extracted services.
- Backward-compatible forwarding properties preserved in `OptimizerMaster` for existing UI usage.
- `OptimizerExecutor` now uses:
  - `ParameterIterator` for combination count/enumeration/copy/reload logic.
  - `BotConfigurator` for bot creation and tab/source configuration.

### Enum consolidation
- `SortBotsType` canonicalized in `OptimizerEnums.cs` (namespace `OsEngine.OsOptimizer`).
- Duplicate enum removed from `OptimizerUi.xaml.cs`.

## Phase 2 Changes (Threading Improvements, Partial)
### `OptimizerExecutor.cs`
- Introduced `SemaphoreSlim` slot throttling for test server concurrency.
- Introduced `CountdownEvent` for phase completion synchronization.
- Replaced main optimization-thread busy waits (`while + Thread.Sleep(1/50)`) with synchronization-based waits.
- Added safe failure finalization for bots/servers that fail before test start.
- Replaced connection polling in optimizer-start path with `SpinWait.SpinUntil` timeout.
- Added cancellation token model for stop flow:
  - `Start()` recreates `CancellationTokenSource`.
  - `Stop()` cancels token instead of only toggling flag.
  - Slot acquisition and phase waits now observe cancellation token and exit deterministically.
  - Connection wait path also checks cancellation token state.
- Single-bot test loop now respects stop cancellation (`IsStopRequested`) to avoid unnecessary wait continuation after stop.
- Added run-scope synchronization cleanup in `finally` of prime worker:
  - dispose/reset `CountdownEvent`,
  - dispose/reset `SemaphoreSlim`,
  - dispose/reset `CancellationTokenSource`.
- Replaced `Thread.Sleep(1000)` cadence in `TestBot` progression loop with cancellation-aware wait (`token.WaitHandle.WaitOne(1000)`).

### `OptEntity/AsyncBotFactory.cs`
- Reworked from mutable shared lists + polling loops to:
  - `ConcurrentQueue<BotCreateRequest>`
  - `SemaphoreSlim` signal
  - `ConcurrentDictionary<string, TaskCompletionSource<BotPanel>>`
- Removed active waiting in bot retrieval path.
- Added cancellation-aware wait in `GetBot(..., CancellationToken)` so stop requests can break bot retrieval wait.

### `OptEntity/BotConfigurator.cs`
- `CreateAndConfigureBot(...)` now accepts optional `CancellationToken` and forwards it to async bot retrieval.

### `OptEntity/ServerLifecycleManager.cs`
- Added missing `using System.Threading;` for `Lock` type usage.

## Validation
### Build
Command:
- `dotnet build project/OsEngine.sln`

Result:
- Current `dotnet build project/OsEngine.sln` is successful.
- Related external blocker in `OKX` server implementation was resolved by adding missing interface method stubs in `OkxServerRealization`.

### Tests
Added dedicated unit-test project:
- `project/OsEngine.Tests/OsEngine.Tests.csproj` (`xUnit`, `net10.0-windows`, reference to `OsEngine.csproj`)

Added tests:
- `project/OsEngine.Tests/OptimizerRefactorTests.cs`
  - `OptimizerReportSerializer_V2AndLegacyRoundTrip_ShouldPreserveData`
  - `BruteForceStrategy_EstimateBotCount_ShouldMatchGridSize`
  - `BruteForceStrategy_OptimizeInSampleAsync_ShouldEvaluateAllCombinations`
  - `BruteForceStrategy_OptimizeInSampleAsync_ShouldRespectMaxParallel`
  - `BruteForceStrategy_OptimizeInSampleAsync_CanceledBeforeStart_ShouldReturnEmpty`
  - `BruteForceStrategy_OptimizeInSampleAsync_WithoutEvaluator_ShouldThrow`
  - `OptimizerFazeReport_SortResults_ShouldSortDescendingByMetric`
  - `OptimizerReportSerializer_DeserializeMalformed_ShouldNotThrowAndKeepObjectUsable`
  - `BruteForceStrategy_OptimizeInSampleAsync_EvaluatorMutationsMustNotCorruptEnumeration`
  - `OptimizerReportTab_LoadFromSaveString_LegacyWithoutSharpRatio_ShouldLoad`
  - `BruteForceStrategy_OptimizeInSampleAsync_ShouldPreserveDecimalCheckBoxSnapshot`
  - `OptimizationStrategyFactory_BruteForce_ShouldReturnBruteForceWithoutMessage`
  - `OptimizationStrategyFactory_Bayesian_ShouldReturnBayesianSkeletonWithMessage`
  - `BayesianOptimizationStrategy_OptimizeInSampleAsync_ShouldUseCurrentBruteForceBackend`
  - `BayesianOptimizationStrategy_OptimizeInSampleAsync_ShouldRespectIterationBudget`
  - `BayesianCandidateSelector_SelectInitialBatch_ShouldSpreadAndFill`
  - `BayesianCandidateSelector_SelectNextBatch_ShouldPreferNeighborsOfTopScores`
  - `OptimizerSettings_SaveLoad_ShouldPersistOptimizationMethodFields`
  - `OptimizerSettings_LoadLegacyWithoutV2Fields_ShouldKeepDefaultsForMethodSettings`

Command:
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`

Result:
- Passed: 19
- Failed: 0

### Stabilization fixes from new tests
- Fixed `BruteForceStrategy` combination snapshot bug:
  - prior implementation used `ParameterIterator.CopyParameters(...)` directly, which reset numeric optimized values to start values;
  - evaluator path could repeatedly receive first combination values.
- Added explicit snapshot cloning that preserves current optimized values (`Int`, `Decimal`, `DecimalCheckBox`) before async evaluation.

## Current Status
- Phase 1: largely integrated (core extraction + wiring done).
- Phase 2: in progress (major busy-wait removal done; cancellation propagation now wired through `OptimizerExecutor -> BotConfigurator -> AsyncBotFactory`, remaining cleanup still pending).
- Phase 3: started (serializer extracted; legacy fallback preserved; further cleanup and adoption still pending).
- Phase 4: in progress (core abstraction contracts added; executor switched to strategy-based in-sample pipeline).
- Phase 5 (Bayesian optimization + UI controls): in progress (staged bayesian loop added; still grid-backed and not full probabilistic BO model).
- Phase 5 (Bayesian optimization + UI controls): in progress (staged bayesian loop + UI wiring complete; probabilistic surrogate/acquisition not implemented yet).

## Phase 4 Changes (Started)
### New files
- `project/OsEngine/OsOptimizer/OptEntity/IOptimizationStrategy.cs`
- `project/OsEngine/OsOptimizer/OptEntity/IBotEvaluator.cs`
- `project/OsEngine/OsOptimizer/OptEntity/BotEvaluator.cs`
- `project/OsEngine/OsOptimizer/OptEntity/BruteForceStrategy.cs`
- `project/OsEngine/OsOptimizer/OptEntity/BayesianOptimizationStrategy.cs`
- `project/OsEngine/OsOptimizer/OptEntity/BayesianCandidateSelector.cs`
- `project/OsEngine/OsOptimizer/OptEntity/OptimizationStrategyFactory.cs`

### Notes
- Interfaces are introduced with async contracts.
- `BruteForceStrategy` currently enumerates combinations via `ParameterIterator` and delegates bot evaluation through `IBotEvaluator`.
- `OptimizerExecutor` now uses strategy abstraction in two places:
  - bot count estimation (`EstimateBotCount`),
  - in-sample runtime execution via `IBotEvaluator` + `IOptimizationStrategy`.
- In-sample runtime now maps server completion to evaluator tasks (`ConcurrentDictionary<int, TaskCompletionSource<OptimizerReport>>`) and collects reports from strategy output.
- `OptimizerMaster` now forwards strategy-related settings from `OptimizerSettings`:
  - `OptimizationMethod`
  - `ObjectiveMetric`
  - `BayesianInitialSamples`
  - `BayesianMaxIterations`
  - `BayesianBatchSize`
- Method-selection hook added in `OptimizerExecutor` for in-sample strategy resolution:
  - `Bayesian` now resolves to `BayesianOptimizationStrategy` skeleton.
  - bayesian strategy now performs staged search on grid candidates:
    - initial spread sampling;
    - iterative neighborhood batches around top-scored candidates;
    - respects `InitialSamples`, `MaxIterations`, `BatchSize`, and cancellation token.
  - safety fallback remains for very large candidate pools (delegates to brute-force backend).
  - selection is centralized via `OptimizationStrategyFactory`.
  - candidate selection logic (initial + iterative) is now isolated in `BayesianCandidateSelector`, reducing strategy complexity and preparing surrogate/acquisition swap-in.

## Phase 5 UI Wiring (Continued)
### Updated files
- `project/OsEngine/OsOptimizer/OptimizerUi.xaml`
- `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`

### Added controls and behavior
- Added method/objective selectors:
  - `ComboBoxOptimizationMethod`
  - `ComboBoxObjectiveMetric`
- Added bayesian numeric settings editors:
  - `TextBoxBayesianInitialSamples`
  - `TextBoxBayesianMaxIterations`
  - `TextBoxBayesianBatchSize`
- Added binding and validation in UI code-behind:
  - values are loaded from `OptimizerMaster` at startup;
  - edits are persisted back to `OptimizerMaster` (`OptimizationMethod`, `ObjectiveMetric`, `BayesianInitialSamples`, `BayesianMaxIterations`, `BayesianBatchSize`);
  - bayesian numeric fields require positive integers.
- Added dynamic enable/disable:
  - bayesian numeric fields are enabled only when `OptimizationMethod = Bayesian`;
  - disabled during optimization run and restored after completion.

## Stabilization Tests (Settings Persistence)
### Updated files
- `project/OsEngine.Tests/OptimizerRefactorTests.cs`

### Notes
- Added safe test fixture scope `SettingsFileScope` with backup/restore for `Engine/OptimizerSettings.txt`.
- New tests verify:
  - full save/load roundtrip for method settings;
  - legacy settings file (without last 5 V2 lines) keeps method defaults and does not break load.

## Phase 3 Changes (Started)
### New file
- `project/OsEngine/OsOptimizer/OptEntity/OptimizerReportSerializer.cs`

### `OptimizerReport.cs`
- `GetSaveString()` now delegates to `OptimizerReportSerializer.Serialize(this)`.
- `LoadFromString()` now delegates to `OptimizerReportSerializer.Deserialize(this, saveStr)`.
- New versioned format prefix: `V2|`.
- Legacy format (without prefix) is still auto-detected and parsed.
- `SortResults()` switched from bubble sort to `List.Sort()` with comparator preserving existing ordering rules.

## Next Recommended Steps
1. Finish Phase 2 remaining items:
   - cancellation token propagation through executor/factory lifecycle;
   - full replacement of remaining polling in single-bot test flow where appropriate.
2. Start Phase 3 (`OptimizerReport` serializer extraction with legacy fallback).
3. Start Phase 4 strategy abstraction (`IOptimizationStrategy`, `IBotEvaluator`, brute-force extraction).
4. Continue Phase 5 with surrogate model + acquisition policy integration on top of `BayesianCandidateSelector`.

## Update Rule
After each optimizer-related change, update this file with:
- what changed,
- files touched,
- validation result,
- blockers/risk.
