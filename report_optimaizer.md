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
11. Phase 5 continued: added dedicated acquisition policy (`BayesianAcquisitionPolicy`) with lightweight surrogate scoring and integrated it into bayesian iteration loop.
12. Phase 5 continued: acquisition surrogate upgraded from index-distance to parameter-space distance across strategy parameters.
13. Phase 5 continued: added objective-direction and acquisition-mode settings (`Ucb/EI/Greedy`, `kappa`) with wiring through settings, UI, factory and strategy.
14. Phase 5 continued: added score normalization and metric-aware kappa scaling before acquisition.
15. Phase 5 continued: added exploitation tail-pass (reserved budget slice from `MaxIterations`) with greedy acquisition at the end of staged run.
16. Phase 5 continued: added explicit `BayesianUseTailPass` toggle (settings + UI + strategy wiring) to enable/disable exploitation tail-pass without code changes.
17. Phase 5 tuning: tail-budget heuristic is now mode-aware (`Greedy` disables tail reservation; `EI/Ucb` use different share denominator), plus strategy exposes planned tail budget for diagnostics/tests.
18. Phase 5 tuning: added configurable `BayesianTailSharePercent` (settings + UI + strategy wiring) to control reserved tail budget share.
19. Phase 5 tuning: fixed `ExpectedImprovement` acquisition to use optimistic mean (`mean + kappa * uncertainty`) before improvement calculation.
20. Stabilization: added strict clamping for Bayesian method settings in `OptimizerSettings` (positive ints, non-negative kappa, tail share range).
21. Stabilization: added file-level load clamp coverage for invalid Bayesian values in persisted settings.
22. Stabilization: added strategy-level tail configuration edge-case coverage (tail-pass disabled and tail-share constructor clamping).
23. Operability: expanded Bayesian factory info message with full effective strategy configuration snapshot.
24. Stabilization: added test assertions for key diagnostics fields in Bayesian factory info message.
25. Stabilization: added dedicated settings roundtrip test for `BayesianTailSharePercent` boundary values (`1` and `50`).
26. Stabilization: added Bayesian strategy budget/uniqueness tests (tail-pass respects total eval budget; no duplicate candidate evaluations).
27. Stabilization: added explicit input-shape guards (`null`/length mismatch) in brute-force and bayesian strategies with dedicated tests.
28. Stabilization: hardened acquisition against negative `kappa` and extended guard coverage for `EstimateBotCount` path.
29. Stabilization: added acquisition fallback coverage for invalid candidate payload (`null`/wrong size) to ensure safe selector path.
30. Stabilization: hardened acquisition null-handling (`evaluated/scored/fallbackSelector`) and added tests for safe defaults and explicit error path.
31. Stabilization: added acquisition edge tests for non-positive `batchSize`/`totalCount` to lock empty-result contract.
32. Stabilization: added acquisition edge test for fully-evaluated candidate universe (must return empty selection).
33. Stabilization: added acquisition handling for invalid scored indices (filter valid range, fallback when all scored indices are invalid).
34. Stabilization: added mixed scored-indices coverage to ensure invalid scored entries do not affect selection when valid entries are present.
35. Stabilization: added direct-acquisition null-fallback coverage to ensure `fallbackSelector` may be `null` when fallback path is not required.
36. Stabilization: hardened acquisition against `null` entries inside `scored` list (ignore invalid entries) with dedicated coverage.
37. Stabilization: hardened `BayesianCandidateSelector.SelectNextBatch` for `null scored` and `null` scored entries with safe sequential fallback behavior.
38. Stabilization: hardened `BayesianCandidateSelector` for `null evaluated` in both initial and iterative selection paths.
39. Stabilization: hardened acquisition for duplicate scored indices by consolidating to max score per index before surrogate ranking.
40. Stabilization: hardened `OptimizerSettings.Load()` enum parsing to reject undefined numeric enum values and keep safe defaults.
41. Stabilization: removed remaining blocking sleep in single-bot test flow (`TestBot`) and made connection/min-runtime waits stop-aware through cancellation.
42. Stabilization: replaced polling waits in `OptimizerExecutor` (`TryAcquireServerSlot`, `WaitCurrentPhaseToComplete`) with direct cancellation-aware synchronization waits.
43. Stabilization: reduced optimizer bot-connect timeout in in-sample flow from 2000s to 20s to prevent prolonged stalls on failed connection.
44. Stabilization: fixed cancellation-registration lifecycle in evaluator path (`StartNewBotForEvaluationAsync`) by disposing token registration on task completion.
45. Stabilization: made optimizer UI progress painter thread stop-signal driven (`ManualResetEvent`) to avoid fixed-sleep shutdown lag in `PainterProgressArea`.
46. Stabilization: hardened V2 numeric/bool parsing in `OptimizerSettings.Load()` so partially broken values do not abort loading of remaining fields.
47. Stabilization: replaced fixed post-dialog sleep in single-bot test path (`OptimizerMaster.TestBot`) with explicit completion signal wait (`ManualResetEventSlim`) to remove race-prone timing dependency.
48. Stabilization: added regression coverage for comma-decimal parsing of `BayesianAcquisitionKappa` from settings file (`0,9` -> `0.9m`).
49. Stabilization/UI responsiveness: removed fixed `Thread.Sleep(200)` from `OptimizerUi` constructor to avoid blocking UI thread during startup.
50. Stabilization/UI responsiveness: replaced unmanaged delayed parameter-grid reselection tasks with cancellable debounced scheduling (`CancellationTokenSource`) to prevent task buildup on rapid clicks.
51. Stabilization: switched single-bot background runner from `async void` to `async Task` (`RunAloneBotTestAsync`) and replaced silent completion wait with explicit timed wait + error log on timeout.
52. Stabilization: added load-path coverage for invalid bool in V2 settings (`BayesianUseTailPass`) to ensure parser keeps prior value and continues reading trailing fields.
53. Stabilization: added explicit exception handling/logging in single-bot background runner (`RunAloneBotTestAsync`) to avoid silent fire-and-forget task failures.
54. Stabilization: hardened `AsyncBotFactory.CreateNewBots` against null/empty input and empty bot names to avoid queueing invalid creation requests.
55. Stabilization/behavior fix: corrected `BayesianOptimizationStrategy.EstimateBotCount` to return budget-capped estimate (`min(totalCombinations, InitialSamples + MaxIterations)`) instead of full grid count.
56. Stabilization: extended `BayesianOptimizationStrategy.EstimateBotCount` coverage for over-budget scenario to ensure estimate does not exceed total grid size when budget is larger.
57. Stabilization: hardened `AsyncBotFactory.GetBot` against empty `botType`/`botName` by returning early instead of creating invalid wait keys.
58. Stabilization: added regression test for `AsyncBotFactory.GetBot` invalid-key guard (`empty/whitespace` bot type/name returns `null`).
59. Stabilization: added regression coverage for `AsyncBotFactory.CreateNewBots` invalid input (`null`/empty list/whitespace names) to ensure no exceptions and safe no-op behavior.
60. Stabilization: hardened `AsyncBotFactory.GetBot` cancellation/error path to return `null` on canceled/faulted waits and added regression coverage for pre-canceled token.
61. Stabilization: hardened `AsyncBotFactory.CreateNewBots` with `botType` guard (`null/empty/whitespace`) to prevent invalid keying and queue churn.
62. Stabilization: reset `BayesianOptimizationStrategy.LastTailBudgetPlanned` at start of each run to avoid stale diagnostics after early-return paths (e.g., pre-canceled token).
63. Stabilization: added explicit null-bot guard in `BotConfigurator.CreateAndConfigureBot` after async factory retrieval to avoid noisy `NullReferenceException` on cancellation/error paths.
64. Stabilization: added regression coverage for `BotConfigurator.CreateAndConfigureBot` null-factory-result path to ensure safe null return without exception.
65. Stabilization: added early input guards in `BotConfigurator.CreateAndConfigureBot` for empty `botName` and `null parameters`.
66. Stabilization: added regression coverage for `BotConfigurator.CreateAndConfigureBot` empty-botName guard.
67. Stabilization: hardened `AsyncBotFactory.WorkerArea` error path to cancel pending waiter for failed request, preventing indefinite `GetBot` waits after bot-creation exceptions.
68. Stabilization: hardened `AsyncBotFactory` shutdown paths (worker stop/process-off) by canceling all pending waiters before worker exit.
69. Stabilization: improved `AsyncBotFactory.CancelAllWaiters` to cancel and remove waiter entries, preventing stale waiter accumulation after shutdown/error paths.
70. Stabilization: added pre-canceled-token fast path in `AsyncBotFactory.GetBot` to avoid unnecessary waiter registration/work during stop-cancel flow.
71. Stabilization/behavior fix: aligned Bayesian no-optimized-flags path to evaluate baseline once (`EstimateBotCount = 1` and single empty-candidate runtime pass).
72. Stabilization: made `AsyncBotFactory.GetBot` waiter cleanup race-safe by removing dictionary entry only when it still references the same waiter instance.
73. Stabilization: added regression coverage for `BotConfigurator.CreateAndConfigureBot` null-botName guard.
74. Stabilization: added pre-canceled-token coverage for `BayesianOptimizationStrategy.OptimizeInSampleAsync` to ensure zero evaluations and empty result on immediate cancellation.
75. Stabilization: added runtime coverage for Bayesian no-optimized-flags path to assert exactly one evaluation/report.
76. Stabilization: hardened `StartOptimizeFazeOutOfSample` against null/empty `reportInSample.Reports` by introducing safe local snapshot and early-exit path without null dereference.
77. Stabilization: added sanitization of out-of-sample source reports (`null` and empty `BotName` entries are filtered out before scheduling/countdown setup) to prevent null dereference and phase wait skew.
78. Operability: added out-of-sample diagnostic log when invalid source reports are filtered out (`OutOfSample skipped invalid source reports: N`).
79. Stabilization: added guard for null out-of-sample target report container in `StartOptimizeFazeOutOfSample` with explicit error log and safe early return.
80. Stabilization: hardened out-of-sample loop against null parameter payload from source report (`GetParameters()`), with safe skip + slot/countdown compensation + diagnostic log.
81. Operability: added explicit diagnostic log for null out-of-sample source container (`reportInSample == null`) to clarify phase-skip reason.
82. Stabilization: added guard for null target faze payload (`report.Faze == null`) in out-of-sample phase with explicit error log and safe early return.
83. Stabilization: hardened out-of-sample loop against exceptions from source `GetParameters()` with safe skip, slot/countdown compensation, and error diagnostics.
84. Operability: refined out-of-sample sanitization diagnostics by separating skip reasons (`null report` vs `empty BotName`) in logs.
85. Maintainability: extracted duplicated out-of-sample skip-compensation block into `CompensateSkippedOutOfSampleSlot()` to keep slot/countdown recovery logic consistent.
86. Stabilization: extended `AsyncBotFactory.GetBot` invalid-key regression coverage to include `null` `botType`/`botName` cases.
87. Stabilization: extended `AsyncBotFactory.CreateNewBots` invalid-input regression coverage to include `null botType`.
88. Stabilization: extended `AsyncBotFactory.CreateNewBots` invalid-input regression coverage to include whitespace-only `botType`.
89. Operability: added explicit out-of-sample info log when source report set is empty after sanitization (`no valid source reports to process`).
90. Stabilization: added defensive `BotName` guard in out-of-sample scheduling loop before name transformation (`Replace`) to prevent runtime null/whitespace edge-case failures.
91. Stabilization: fixed out-of-sample `BotName`-skip path to signal `CountdownEvent` for skipped items, preventing phase-wait skew/hang when runtime data mutates after initial sanitization.
92. Maintainability: unified out-of-sample skip compensation paths via parameterized helper (`CompensateSkippedOutOfSampleSlot(releaseServerSlot)`), reducing duplication and keeping slot/countdown semantics consistent.
93. Stabilization: added explicit precondition guards in `BotConfigurator.CreateAndConfigureBot` for `server == null` and `BotToTest == null` with clear error diagnostics and safe early return.
94. Stabilization: added regression coverage for `BotConfigurator` precondition guards (`server == null`, `BotToTest == null`).
95. Stabilization: added compensation for unscheduled out-of-sample tail when slot acquisition fails mid-loop, preventing leftover `CountdownEvent` debt for not-started items.
96. Operability: added diagnostic log for out-of-sample tail compensation count when slot acquisition fails (`compensated unscheduled tail: N`).
97. Stabilization: added defensive null-source-report guard inside out-of-sample scheduling loop (post-sanitization mutation safety) with proper compensation and diagnostics.
98. Stabilization: hardened unscheduled-tail compensation against dispose races by guarding `CountdownEvent.Signal()` with `ObjectDisposedException` handling.
99. Stabilization: hardened per-item out-of-sample skip compensation against dispose races by guarding `CountdownEvent.Signal()` in `CompensateSkippedOutOfSampleSlot(...)`.
100. Stabilization: tightened async factory error cleanup by removing failed request waiter from map (`TryRemove`) before canceling, preventing stale waiter retention on creation exceptions.
101. Stabilization: hardened out-of-sample unscheduled-tail countdown compensation against `InvalidOperationException` race (already-signaled path), making loop exit deterministic under contention.
102. Stabilization: extended per-item skip compensation guard to handle `InvalidOperationException` on `CountdownEvent.Signal()` (over-signal race) in `CompensateSkippedOutOfSampleSlot(...)`.
103. Operability: added explicit progress callback for empty out-of-sample phase (`no valid source reports`) to keep UI progress state in sync even when no bots are scheduled.

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
  - `BayesianOptimizationStrategy_OptimizeInSampleAsync_CanceledBeforeStart_ShouldReturnEmpty`
  - `BayesianOptimizationStrategy_OptimizeInSampleAsync_WithNoOptimizedFlags_ShouldEvaluateOnce`
  - `BayesianCandidateSelector_SelectInitialBatch_ShouldSpreadAndFill`
  - `BayesianCandidateSelector_SelectInitialBatch_WithNullEvaluated_ShouldTreatAsEmpty`
  - `BayesianCandidateSelector_SelectNextBatch_ShouldPreferNeighborsOfTopScores`
  - `BayesianCandidateSelector_SelectNextBatch_WithNullScored_ShouldFallbackToSequentialFill`
  - `BayesianCandidateSelector_SelectNextBatch_WithNullEvaluated_ShouldTreatAsEmpty`
  - `BayesianAcquisitionPolicy_SelectNextBatch_WithoutScores_ShouldFallbackToSelector`
  - `BayesianAcquisitionPolicy_SelectNextBatch_WithEqualMeans_ShouldPreferHigherUncertainty`
  - `BayesianAcquisitionPolicy_Modes_ShouldChangeExplorationBehavior`
  - `BayesianOptimizationStrategy_GreedyMode_ShouldPlanZeroTailBudget`
  - `BayesianOptimizationStrategy_UcbMode_ShouldPlanPositiveTailBudgetWhenEnabled`
  - `BayesianOptimizationStrategy_CanceledRun_ShouldResetLastTailBudgetPlanned`
  - `BayesianOptimizationStrategy_TailSharePercent_ShouldAffectPlannedTailBudget`
  - `BayesianAcquisitionPolicy_ExpectedImprovement_ShouldUseOptimisticMean`
  - `OptimizerSettings_MethodFields_ShouldClampInvalidValues`
  - `OptimizerSettings_LoadFromFile_WithInvalidBayesianValues_ShouldClampOnLoad`
  - `OptimizerSettings_LoadFromFile_WithInvalidEnumNumbers_ShouldKeepDefaults`
  - `OptimizerSettings_LoadFromFile_WithPartiallyInvalidV2Numbers_ShouldLoadRemainingFields`
  - `OptimizerSettings_LoadFromFile_WithCommaDecimalKappa_ShouldParseCorrectly`
  - `OptimizerSettings_LoadFromFile_WithInvalidBoolTailPass_ShouldKeepValueAndLoadFollowingFields`
  - `OptimizerSettings_SaveLoad_TailSharePercentBoundaries_ShouldRoundTrip`
  - `AsyncBotFactory_GetBot_WithInvalidKeys_ShouldReturnNull`
  - `AsyncBotFactory_CreateNewBots_WithInvalidInput_ShouldNotThrow`
  - `AsyncBotFactory_GetBot_WithCanceledToken_ShouldReturnNull`
  - `BotConfigurator_CreateAndConfigureBot_WhenFactoryReturnsNull_ShouldReturnNull`
  - `BotConfigurator_CreateAndConfigureBot_WithNullParameters_ShouldReturnNull`
  - `BotConfigurator_CreateAndConfigureBot_WithEmptyBotName_ShouldReturnNull`
  - `BotConfigurator_CreateAndConfigureBot_WithNullBotName_ShouldReturnNull`
  - `BotConfigurator_CreateAndConfigureBot_WithNullServer_ShouldReturnNull`
  - `BotConfigurator_CreateAndConfigureBot_WithNullBotToTest_ShouldReturnNull`
  - `BayesianOptimizationStrategy_WithTailPass_ShouldRespectTotalEvaluationBudget`
  - `BayesianOptimizationStrategy_ShouldNotEvaluateDuplicateCandidates`
  - `BruteForceStrategy_OptimizeInSampleAsync_WithMismatchedFlags_ShouldThrowArgumentException`
  - `BayesianOptimizationStrategy_OptimizeInSampleAsync_WithNullFlags_ShouldThrowArgumentNullException`
  - `BruteForceStrategy_EstimateBotCount_WithMismatchedFlags_ShouldThrowArgumentException`
  - `BayesianOptimizationStrategy_EstimateBotCount_WithMismatchedFlags_ShouldThrowArgumentException`
  - `BayesianOptimizationStrategy_EstimateBotCount_ShouldRespectBudgetCap`
  - `BayesianOptimizationStrategy_EstimateBotCount_WhenBudgetExceedsGrid_ShouldReturnGridSize`
  - `BayesianOptimizationStrategy_EstimateBotCount_WithNoOptimizedFlags_ShouldReturnOne`
  - `BayesianAcquisitionPolicy_NegativeKappa_ShouldBeTreatedAsZero`
  - `BayesianAcquisitionPolicy_InvalidCandidates_ShouldFallbackToSelectorPath`
  - `BayesianAcquisitionPolicy_NullEvaluated_ShouldTreatAsEmptySet`
  - `BayesianAcquisitionPolicy_NullFallback_WhenFallbackPathNeeded_ShouldThrowArgumentNullException`
  - `BayesianAcquisitionPolicy_NonPositiveBudgetOrUniverse_ShouldReturnEmpty`
  - `BayesianAcquisitionPolicy_AllCandidatesEvaluated_ShouldReturnEmpty`
  - `BayesianAcquisitionPolicy_InvalidScoredIndices_ShouldFallbackToSelectorPath`
  - `BayesianAcquisitionPolicy_MixedScoredIndices_ShouldIgnoreInvalidAndUseValid`
  - `BayesianAcquisitionPolicy_NullFallback_OnDirectAcquisitionPath_ShouldNotThrow`
  - `BayesianAcquisitionPolicy_ScoredContainsNullEntries_ShouldIgnoreNulls`
  - `BayesianAcquisitionPolicy_DuplicateScoredIndex_ShouldUseMaxScorePerIndex`
  - `BayesianOptimizationStrategy_TailPassDisabled_ShouldPlanZeroTailBudget`
  - `BayesianOptimizationStrategy_TailSharePercent_ShouldClampToRange`
  - `OptimizerSettings_SaveLoad_ShouldPersistOptimizationMethodFields`
  - `OptimizerSettings_LoadLegacyWithoutV2Fields_ShouldKeepDefaultsForMethodSettings`

Command:
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`

Result:
- Passed: 70
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
- `project/OsEngine/OsOptimizer/OptEntity/BayesianAcquisitionPolicy.cs`
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
  - `ObjectiveDirection`
  - `BayesianInitialSamples`
  - `BayesianMaxIterations`
  - `BayesianBatchSize`
  - `BayesianAcquisitionMode`
  - `BayesianAcquisitionKappa`
  - `BayesianUseTailPass`
  - `BayesianTailSharePercent`
- Method-selection hook added in `OptimizerExecutor` for in-sample strategy resolution:
  - `Bayesian` now resolves to `BayesianOptimizationStrategy` skeleton.
  - bayesian strategy now performs staged search on grid candidates:
    - initial spread sampling;
    - iterative neighborhood batches around top-scored candidates;
    - respects `InitialSamples`, `MaxIterations`, `BatchSize`, and cancellation token.
  - safety fallback remains for very large candidate pools (delegates to brute-force backend).
  - selection is centralized via `OptimizationStrategyFactory`.
  - candidate selection logic (initial + iterative) is now isolated in `BayesianCandidateSelector`, reducing strategy complexity and preparing surrogate/acquisition swap-in.
  - bayesian strategy selection log now prints full effective configuration (objective, direction, samples, iterations, batch, acquisition mode/kappa, tail settings) for easier diagnostics.
  - iterative batch selection now uses `BayesianAcquisitionPolicy` with configurable acquisition:
    - local surrogate mean from nearest evaluated candidate score;
    - uncertainty proxy from normalized parameter-space distance (supports `Int`, `Decimal`, `DecimalCheckBox`, `Bool`, `CheckBox`, `String`, `TimeOfDay`);
    - acquisition mode is configurable: `Ucb`, `ExpectedImprovement`, `Greedy`;
    - `ExpectedImprovement` now computes improvement from optimistic mean (`mean + kappa * uncertainty`) vs current best surrogate mean.
    - objective-direction is configurable (`Maximize` / `Minimize`) and is applied in strategy score orientation before acquisition.
  - strategy now normalizes observed objective scores (`min-max`) before acquisition and applies metric-specific kappa scaling to stabilize exploration pressure across heterogeneous metrics.
  - strategy now reserves small exploitation budget and runs final greedy candidate pass after exploration loop.
  - tail-budget reservation is mode-aware:
    - `Greedy`: no reserved tail budget,
    - `ExpectedImprovement`: lower reserved share,
    - `Ucb`: default reserved share.

## Phase 5 UI Wiring (Continued)
### Updated files
- `project/OsEngine/OsOptimizer/OptimizerUi.xaml`
- `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`

### Added controls and behavior
- Added method/objective selectors:
  - `ComboBoxOptimizationMethod`
  - `ComboBoxObjectiveMetric`
  - `ComboBoxObjectiveDirection`
- Added acquisition selectors:
  - `ComboBoxBayesianAcquisitionMode`
  - `TextBoxBayesianAcquisitionKappa`
  - `CheckBoxBayesianTailPass`
  - `TextBoxBayesianTailSharePercent`
- Added bayesian numeric settings editors:
  - `TextBoxBayesianInitialSamples`
  - `TextBoxBayesianMaxIterations`
  - `TextBoxBayesianBatchSize`
- Added binding and validation in UI code-behind:
  - values are loaded from `OptimizerMaster` at startup;
  - edits are persisted back to `OptimizerMaster` (`OptimizationMethod`, `ObjectiveMetric`, `ObjectiveDirection`, `BayesianInitialSamples`, `BayesianMaxIterations`, `BayesianBatchSize`, `BayesianAcquisitionMode`, `BayesianAcquisitionKappa`, `BayesianUseTailPass`, `BayesianTailSharePercent`);
  - bayesian integer fields require positive integers; `kappa` requires non-negative decimal; tail share requires range `1..50`.
- Added dynamic enable/disable:
  - bayesian numeric fields are enabled only when `OptimizationMethod = Bayesian`;
  - disabled during optimization run and restored after completion.

## Stabilization Tests (Settings Persistence)
### Updated files
- `project/OsEngine.Tests/OptimizerRefactorTests.cs`

### Notes
- Added safe test fixture scope `SettingsFileScope` with backup/restore for `Engine/OptimizerSettings.txt`.
- New tests verify:
  - full save/load roundtrip for method settings, including direction/acquisition settings, tail-pass toggle, and tail-share percent;
  - legacy settings file (without appended method-setting lines) keeps defaults and does not break load.
  - invalid Bayesian setting values are clamped to safe bounds.
  - invalid persisted Bayesian values are clamped on `Load()` path as well.

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
4. Continue Phase 5 by tuning tail-budget heuristics and validating effect on optimization quality metrics.

## Update Rule
After each optimizer-related change, update this file with:
- what changed,
- files touched,
- validation result,
- blockers/risk.
