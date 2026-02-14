# Optimizer Refactor Report

Last updated: 2026-02-14

## Scope
This report tracks all optimizer-related refactoring done in the current workstream and must be kept up to date after each meaningful change.

## Summary
Implemented and committed:
1. Phase 1 foundation (SRP decomposition + wiring in master/executor).
2. Phase 2 partial threading improvements (removed core busy-wait loops in optimizer execution and async bot factory path).
3. Phase 2 continuation (in progress): stop/cancel path in `OptimizerExecutor` moved to `CancellationTokenSource`.

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
- Optimizer-related compile issues introduced by this work are resolved up to current optimizer changes.
- Build currently fails on unrelated pre-existing errors in `OKX` implementation:
  - `project/OsEngine/Market/Servers/OKX/OkxServer.cs`
  - `OkxServerRealization` does not implement newly required `IServerRealization` members:
    - `SetLeverage(string, string, string, string, string)`
    - `SetHedgeMode(string, string, string)`
    - `SetMarginMode(string, string, string)`
    - `SetCommonLeverage(string, string)`
    - `SetCommonHedgeMode(string, string)`
    - `SetCommonMarginMode(string, string)`

## Current Status
- Phase 1: largely integrated (core extraction + wiring done).
- Phase 2: in progress (major busy-wait removal done; cancellation propagation now wired through `OptimizerExecutor -> BotConfigurator -> AsyncBotFactory`, remaining cleanup still pending).
- Phase 3+ (serialization strategy abstraction, Bayesian optimization, UI for method/objective): not started in this branch segment.

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
