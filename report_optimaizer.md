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

## Current Status
- Phase 1: largely integrated (core extraction + wiring done).
- Phase 2: in progress (major busy-wait removal done; cancellation propagation now wired through `OptimizerExecutor -> BotConfigurator -> AsyncBotFactory`, remaining cleanup still pending).
- Phase 3: started (serializer extracted; legacy fallback preserved; further cleanup and adoption still pending).
- Phase 4: started (core abstraction contracts and brute-force scaffold added; executor not yet switched to strategy orchestration).
- Phase 5 (Bayesian optimization + UI controls): not started in this branch segment.

## Phase 4 Changes (Started)
### New files
- `project/OsEngine/OsOptimizer/OptEntity/IOptimizationStrategy.cs`
- `project/OsEngine/OsOptimizer/OptEntity/IBotEvaluator.cs`
- `project/OsEngine/OsOptimizer/OptEntity/BotEvaluator.cs`
- `project/OsEngine/OsOptimizer/OptEntity/BruteForceStrategy.cs`

### Notes
- Interfaces are introduced with async contracts.
- `BruteForceStrategy` currently enumerates combinations via `ParameterIterator` and delegates bot evaluation through `IBotEvaluator`.
- This is an additive scaffold step; `OptimizerExecutor` now uses `IOptimizationStrategy` (`BruteForceStrategy`) for bot count estimation, while runtime test orchestration still uses existing execution flow.
- `OptimizerMaster` now forwards strategy-related settings from `OptimizerSettings`:
  - `OptimizationMethod`
  - `ObjectiveMetric`
  - `BayesianInitialSamples`
  - `BayesianMaxIterations`
  - `BayesianBatchSize`
- Method-selection hook added in `OptimizerExecutor` for in-sample strategy resolution:
  - currently `Bayesian` logs fallback and uses `BruteForceStrategy`.

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
4. Implement Phase 5 Bayesian strategy + UI settings binding.

## Update Rule
After each optimizer-related change, update this file with:
- what changed,
- files touched,
- validation result,
- blockers/risk.
