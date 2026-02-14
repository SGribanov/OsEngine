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
104. Stabilization: upgraded `AsyncBotFactory.CancelAllWaiters()` to drain the waiter map fully (loop until stable empty) for more reliable cleanup under concurrent add/remove races.
105. Stabilization: added bounded-pass safeguard and diagnostics to `AsyncBotFactory.CancelAllWaiters()` to prevent potential infinite cleanup loop under sustained concurrent waiter additions.
106. Operability/Stabilization: aligned progress accounting for compensated out-of-sample skips/tail by incrementing `_countAllServersEndTest` and emitting `PrimeProgressChangeEvent` when countdown is signaled without actual bot run.

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

## Stabilization Update (2026-02-14) - Progress Clamp
### What changed
- Added upper bound clamp in compensated out-of-sample progress path to prevent progress counters from exceeding total planned work under race/over-compensation conditions.
- `AddCompensatedOutOfSampleProgress(int count)` now enforces:
  - `_countAllServersEndTest += count`;
  - if `_countAllServersEndTest > _countAllServersMax`, set `_countAllServersEndTest = _countAllServersMax`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Clamp is intentionally local to compensation path; normal per-bot completion accounting remains unchanged.

## Stabilization Update (2026-02-14) - Thread-Safe Prime Progress Increment
### What changed
- In `server_TestingEndEvent`, moved `_countAllServersEndTest` increment into `_serverRemoveLocker` critical section.
- Added upper-bound clamp there as well (`_countAllServersEndTest <= _countAllServersMax`).
- Progress event payload is now captured from synchronized snapshots (`progressEnd`, `progressMax`) and emitted after lock release.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- `PrimeProgressChangeEvent` remains outside lock to avoid re-entrancy/deadlock risk in subscribers.

## Stabilization Update (2026-02-14) - Safe Phase Signal On Not-Started Bot Finalization
### What changed
- Hardened `FinalizeNotStartedBot` phase completion signaling against shutdown races.
- Replaced direct `_phaseCompletion.Signal()` with local snapshot and guarded call:
  - catches `ObjectDisposedException`;
  - catches `InvalidOperationException` (already-set/over-signaled race).

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior is unchanged on normal path; only race-failure mode is neutralized.

## Stabilization Update (2026-02-14) - Safe Phase Signal On Testing-End Cleanup
### What changed
- Hardened final phase signal in `server_TestingEndEvent` against disposal/over-signal races.
- Replaced direct `_phaseCompletion.Signal()` with guarded snapshot-based call and exception-safe handling for:
  - `ObjectDisposedException`;
  - `InvalidOperationException`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional change in successful path; only shutdown race resilience improved.

## Stabilization Update (2026-02-14) - Atomic Progress Snapshot In Compensation Path
### What changed
- Updated `AddCompensatedOutOfSampleProgress` to publish `PrimeProgressChangeEvent` from lock-captured snapshots.
- Event now uses local `progressEnd/progressMax` values captured under `_serverRemoveLocker`, avoiding unsynchronized field reads.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavioral change expected; this is consistency hardening for concurrent observers.

## Stabilization Update (2026-02-14) - Cancel-Aware Early Exit After Slot Acquisition
### What changed
- Added cancellation race guard in `StartNewBotForEvaluationAsync`.
- If evaluation cancellation is already requested after slot acquisition but before `StartNewBot(...)`, method now:
  - skips bot start;
  - releases acquired `_serverSlots` permit;
  - returns the already-canceled `completion.Task`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents unnecessary server/bot allocation on canceled evaluations and avoids temporary slot starvation in cancellation races.

## Stabilization Update (2026-02-14) - Exception-Safe Evaluation Start After Slot Wait
### What changed
- Wrapped `StartNewBot(...)` call in `StartNewBotForEvaluationAsync` with `try/catch`.
- On exception during evaluation start:
  - set exception on evaluation completion (`completion.TrySetException(ex)`);
  - emit error log with exception details;
  - release previously acquired `_serverSlots` permit.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents slot leaks when startup path throws unexpectedly; keeps async caller informed via faulted task.

## Stabilization Update (2026-02-14) - Snapshot-Based Phase Access In Skip Compensation
### What changed
- Replaced direct field re-reads of `_phaseCompletion` in `CompensateSkippedOutOfSampleSlot` with a local snapshot (`phase`).
- Signal call now targets the snapshot object, preserving existing exception guards.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Reduces race window where `_phaseCompletion` could become `null` between condition check and `Signal()` call during concurrent cleanup.

## Stabilization Update (2026-02-14) - Token Snapshot Consistency In Single-Bot Test Loop
### What changed
- In `TestBot(...)`, introduced local cancellation token snapshot:
  - `CancellationToken token = _stopCts?.Token ?? CancellationToken.None;`
- Replaced direct `_stopCts` token reads in `WaitHandle.WaitOne(...)` calls with `token.WaitHandle`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change expected; improves consistency of cancellation observation within single `TestBot` execution.

## Stabilization Update (2026-02-14) - Dispose Await Object On TestBot Early-Exit
### What changed
- Fixed `TestBot(...)` early-exit branch for `!isConnected || IsStopRequested`.
- Added missing `awaitObj.Dispose()` before returning `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents leaked/pending wait indicator when single-bot test fails to connect or is canceled before run start.

## Stabilization Update (2026-02-14) - Full Cleanup On TestBot Failure Paths
### What changed
- Hardened `TestBot(...)` early-failure branches with missing cleanup:
  - when `_primeThreadWorker != null`: `awaitObj?.Dispose()` before return;
  - when bot creation returns `null`: dispose await object and remove created optimizer server;
  - when connect wait fails or stop is requested: clear/delete bot (best-effort), remove optimizer server, dispose await object.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Reduces resource leaks (server/bot/await UI object) in single-bot test failure and cancellation paths.

## Stabilization Update (2026-02-14) - UI Null Guard Before Chart Dialog
### What changed
- Added missing `null` guard in optimizer main UI before opening chart dialog from single-bot test result.
- `OptimizerUi` now mirrors existing guard already present in `OptimizerReportUi`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents `NullReferenceException` when `TestBot(...)` returns `null` on early failure/cancel path.

## Stabilization Update (2026-02-14) - UI RowIndex Boundary Fix (Off-By-One)
### What changed
- Fixed phase grid row boundary checks in optimizer UI dialogs:
  - replaced `RowIndex > _reports.Count` with `RowIndex >= _reports.Count`.
- Applied consistently in both chart-open and parameters-open handlers.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
- `project/OsEngine/OsOptimizer/OptimizerReportUi.xaml.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents out-of-range access when selected row index equals report count boundary.

## Stabilization Update (2026-02-14) - UI Empty Reports Guard In Dialog Actions
### What changed
- Added defensive guard in optimizer result dialog actions:
  - return early when `_reports == null || _reports.Count == 0`.
- Applied in both UI windows and both handlers:
  - `ShowBotChartDialog(...)`
  - `ShowParametersDialog(...)`

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
- `project/OsEngine/OsOptimizer/OptimizerReportUi.xaml.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents rare null/out-of-range access on stale UI events when reports list is empty.

## Stabilization Update (2026-02-14) - Input Guard In OptimizerMaster.TestBot
### What changed
- Added early input validation in `OptimizerMaster.TestBot(...)`.
- If `faze` or `report` is `null`, method now logs an error and returns `null` without starting async single-bot test workflow.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Avoids avoidable background task failures and UI wait lifecycle on invalid caller input.

## Stabilization Update (2026-02-14) - Input Guard In OptimizerExecutor.TestBot
### What changed
- Added early validation in executor-level `TestBot(...)`.
- If `reportFaze`, `reportToBot`, or `awaitObj` is `null`, method logs and returns `null` immediately.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null-driven failures in lower-level single-bot test path, complementing master-level guard.

## Stabilization Update (2026-02-14) - UI Negative RowIndex Guard In Result Actions
### What changed
- Added explicit `e.RowIndex < 0` guards to result action handlers in optimizer UIs.
- Applied in both files and both handlers:
  - `ShowBotChartDialog(...)`
  - `ShowParametersDialog(...)`

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
- `project/OsEngine/OsOptimizer/OptimizerReportUi.xaml.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents negative-index access when DataGridView header/invalid rows trigger click events.

## Stabilization Update (2026-02-14) - UI Negative ColumnIndex Guard In Result Click Dispatcher
### What changed
- Hardened result click dispatcher in both optimizer UI windows.
- `_gridResults_CellMouseClick(...)` now returns early when `e.ColumnIndex < 0` (together with existing row guard).

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
- `project/OsEngine/OsOptimizer/OptimizerReportUi.xaml.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents dispatch attempts from DataGridView header/invalid column clicks.

## Stabilization Update (2026-02-14) - UI Null Guard For Phase Reports Collection
### What changed
- Hardened result action handlers against `fazeReport.Reports == null`.
- Updated row access checks in both dialogs and both handlers to:
  - `if (fazeReport.Reports == null || e.RowIndex >= fazeReport.Reports.Count) return;`

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
- `project/OsEngine/OsOptimizer/OptimizerReportUi.xaml.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null dereference on stale/partially initialized phase report data.

## Stabilization Update (2026-02-14) - UI Null Guard For Chart Dialog Instance
### What changed
- Added defensive check after `bot.ShowChartDialog()` in optimizer UIs.
- If `ui == null`, handler now exits without subscribing to `ui.Closed`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
- `project/OsEngine/OsOptimizer/OptimizerReportUi.xaml.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null-reference crashes if chart dialog creation fails or returns null under edge conditions.

## Stabilization Update (2026-02-14) - Diagnostic Log On Concurrent Single-Bot Test Request
### What changed
- Added explicit diagnostic log when `OptimizerMaster.TestBot(...)` receives a request while previous single-bot test is still running (`_aloneTestIsOver == false`).

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves operator visibility of ignored concurrent test requests.

## Stabilization Update (2026-02-14) - Null Parameter Guard In Executor TestBot
### What changed
- Added defensive guard in `OptimizerExecutor.TestBot(...)` for `reportToBot.GetParameters() == null`.
- On null parameters, method now:
  - logs error;
  - removes created optimizer server;
  - disposes await object;
  - returns `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null-parameter path from flowing into bot creation and leaving partially initialized single-test resources.

## Stabilization Update (2026-02-14) - Null Server Guard In Executor TestBot
### What changed
- Added guard for `CreateNewServer(...)` failure in `OptimizerExecutor.TestBot(...)`.
- If server creation returns `null`, method now logs and disposes await object before returning.
- Also made server removal in bot-creation failure path conditional (`server != null`) for defensive safety.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null-server cascades and keeps single-bot test cleanup deterministic under server creation failures.

## Stabilization Update (2026-02-14) - Strategy Name Guard In Executor TestBot
### What changed
- Added early validation for strategy identity in `OptimizerExecutor.TestBot(...)`.
- If `_master.StrategyName` is null/empty/whitespace, method now:
  - logs error;
  - disposes await object;
  - returns `null` before bot queue/server setup.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents invalid async bot creation requests with empty strategy key in single-bot test path.

## Stabilization Update (2026-02-14) - Null Phase Guard In Executor TestBot
### What changed
- Added early validation in `OptimizerExecutor.TestBot(...)` for `reportFaze.Faze == null`.
- On null phase configuration, method now logs error, disposes await object, and returns `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null-phase dereference in single-bot test loop condition (`reportFaze.Faze.TimeEnd`).

## Stabilization Update (2026-02-14) - Executor Initialization Guard In Master TestBot
### What changed
- Added early guard in `OptimizerMaster.TestBot(...)` for `_optimizerExecutor == null`.
- If executor is not initialized, method logs error and returns `null` without starting async single-bot workflow.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null-executor failures on edge lifecycle transitions (startup/disposal races).

## Stabilization Update (2026-02-14) - Timeout Recovery In Master Single-Bot Wait
### What changed
- Hardened timeout branch in `OptimizerMaster.TestBot(...)`.
- If wait for `_aloneTestDoneSignal` exceeds 30 seconds, code now:
  - logs timeout;
  - restores `_aloneTestIsOver = true`;
  - sets `_aloneTestDoneSignal` to avoid stuck "test running" state.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Improves recovery after rare completion-signal loss; may allow new test request while delayed background completion arrives later.

## Stabilization Update (2026-02-14) - Executor Snapshot Guard In Async Single-Bot Runner
### What changed
- Hardened `RunAloneBotTestAsync()` against lifecycle race after initial delay.
- Captures `_optimizerExecutor` into local snapshot and verifies it is not `null` before invoking `TestBot(...)`.
- On missing executor, logs diagnostic message and exits async run safely.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents delayed single-bot task from dereferencing disposed/unavailable executor after UI-triggered wait window.

## Stabilization Update (2026-02-14) - Exception Guard For Parameter Extraction In Executor TestBot
### What changed
- Hardened `OptimizerExecutor.TestBot(...)` around `reportToBot.GetParameters()`.
- Added `try/catch` for parameter extraction exceptions with deterministic cleanup:
  - error log with exception;
  - remove created optimizer server;
  - dispose await object;
  - return `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents single-bot flow from crashing on malformed report payloads and keeps resource cleanup consistent.

## Stabilization Update (2026-02-14) - Await Object Guard In Async Single-Bot Runner
### What changed
- Added `await` object availability check in `RunAloneBotTestAsync()`.
- After executor snapshot validation, method now verifies `_awaitUiMasterAloneTest` is not null before calling `executor.TestBot(...)`.
- On missing await object, logs diagnostic message and exits safely.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents edge-case null await-object propagation into executor single-test path.

## Stabilization Update (2026-02-14) - Snapshot Inputs For Async Single-Bot Runner
### What changed
- Removed mutable field dependency for single-bot async runner inputs.
- `RunAloneBotTestAsync(...)` now accepts `faze/report/awaitUi` as explicit parameters.
- `TestBot(...)` schedules async run with captured snapshots:
  - `Task.Run(() => RunAloneBotTestAsync(faze, report, awaitUi));`
- This prevents late async execution from reading overwritten request fields after timeout/retry scenarios.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Reduces cross-request data races in overlapped single-bot test flows.

## Stabilization Update (2026-02-14) - RunId Isolation For Async Single-Bot Completion
### What changed
- Added per-request `runId` in `OptimizerMaster.TestBot(...)` using `Interlocked.Increment`.
- `RunAloneBotTestAsync(...)` now receives `runId` and publishes completion only for current run:
  - result assignment;
  - error-null assignment;
  - `_aloneTestIsOver` transition;
  - `_aloneTestDoneSignal.Set()`.
- Guard check uses `runId == Volatile.Read(ref _aloneTestRunId)`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents late completion of stale async run from clobbering state/result of a newer single-bot request.

## Stabilization Update (2026-02-14) - Remove Unused Shared Await Field
### What changed
- Removed obsolete shared field `_awaitUiMasterAloneTest` from `OptimizerMaster`.
- After snapshot refactor, single-bot async path already passes `AwaitObject` explicitly, so shared field assignment was redundant.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Reduces mutable shared state and future race surface in single-bot test lifecycle.

## Stabilization Update (2026-02-14) - Exception-Safe Server Testing Start In Executor TestBot
### What changed
- Wrapped `server.TestingStart()` in `OptimizerExecutor.TestBot(...)` with `try/catch`.
- On start exception, method now performs deterministic cleanup:
  - logs error with exception;
  - clears/deletes bot (best-effort);
  - removes optimizer server;
  - disposes await object;
  - returns `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents leaked single-bot test resources when server test start fails unexpectedly.

## Stabilization Update (2026-02-14) - Phase Time Range Guard In Executor TestBot
### What changed
- Added early validation in `OptimizerExecutor.TestBot(...)` for phase time range integrity.
- If `reportFaze.Faze.TimeEnd <= reportFaze.Faze.TimeStart`, method now logs error, disposes await object, and exits early.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents invalid/degenerate single-bot phase windows from entering runtime loop.

## Stabilization Update (2026-02-14) - Timeout Invalidates Active Single-Bot RunId
### What changed
- Hardened timeout branch in `OptimizerMaster.TestBot(...)` by invalidating active run id.
- On `Wait(30s)` timeout, now executes:
  - `Interlocked.Increment(ref _aloneTestRunId);`
  - followed by existing state recovery (`_aloneTestIsOver = true`, `_aloneTestDoneSignal.Set()`).

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents delayed completion of timed-out run from publishing stale result/state updates.

## Stabilization Update (2026-02-14) - Volatile Access For Single-Bot Running Flag
### What changed
- Switched `_aloneTestIsOver` access in `OptimizerMaster` to volatile operations:
  - `Volatile.Read(ref _aloneTestIsOver)` in request gate;
  - `Volatile.Write(ref _aloneTestIsOver, false/true)` on start/timeout/finalization transitions.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Improves cross-thread visibility of single-bot run-state flag between UI path and async runner.

## Stabilization Update (2026-02-14) - Exception Guard Around AwaitUi In Master TestBot
### What changed
- Wrapped `AwaitUi` creation/show path in `OptimizerMaster.TestBot(...)` with `try/catch`.
- On UI wait failure, method now:
  - logs error with exception;
  - invalidates active run id (`Interlocked.Increment`);
  - restores running flag (`Volatile.Write(..., true)`);
  - signals completion event (`_aloneTestDoneSignal.Set()`);
  - returns `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents stuck single-bot state when UI wait window cannot be created/displayed.

## Stabilization Update (2026-02-14) - Exception Guard In Single-Bot Setup Stage
### What changed
- Hardened setup stage in `OptimizerMaster.TestBot(...)`:
  - `AwaitObject` creation and `Task.Run(...)` launch are now wrapped in `try/catch`.
- On setup failure, code now:
  - logs setup error;
  - invalidates active run id;
  - restores running flag;
  - signals `_aloneTestDoneSignal`;
  - returns `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents orphaned "running" state if single-bot test fails before UI wait dialog stage.

## Stabilization Update (2026-02-14) - Generated Bot Name Guard In Executor TestBot
### What changed
- Added defensive validation for generated single-test bot name in `OptimizerExecutor.TestBot(...)`.
- If generated `botName` is null/empty/whitespace, method now logs error, disposes await object, and exits before bot queue/server setup.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents invalid bot-creation request construction in rare number-generation edge cases.

## Stabilization Update (2026-02-14) - Exception Guard For Async Bot Queue In Executor TestBot
### What changed
- Wrapped `_asyncBotFactory.CreateNewBots(...)` in `OptimizerExecutor.TestBot(...)` with `try/catch`.
- On queueing exception, method now:
  - logs error with exception details;
  - disposes await object;
  - exits early with `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents single-bot test UI wait path from hanging on unexpected async bot factory queue failures.

## Stabilization Update (2026-02-14) - Exception Guard For Server Creation In Executor TestBot
### What changed
- Wrapped `CreateNewServer(...)` call in `OptimizerExecutor.TestBot(...)` with `try/catch`.
- On server creation exception, method now:
  - logs detailed error;
  - disposes await object;
  - exits early with `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents propagation of server-construction exceptions into upper single-bot UI flow.

## Stabilization Update (2026-02-14) - Exception Guard For Bot Name Generation In Executor TestBot
### What changed
- Wrapped bot-name generation (`NumberGen.GetNumberDeal(...).ToString()`) in `OptimizerExecutor.TestBot(...)` with `try/catch`.
- On generation exception, method now:
  - logs error with exception details;
  - disposes await object;
  - exits early with `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents rare number-generator failures from breaking single-bot test UI flow.

## Stabilization Update (2026-02-14) - Safe AwaitObject Disposal Helper In Executor TestBot
### What changed
- Introduced `SafeDisposeAwaitObject(AwaitObject awaitObj)` helper in `OptimizerExecutor`.
- Replaced direct `awaitObj.Dispose()` calls in `TestBot(...)` with safe helper usage.
- Helper catches disposal exceptions and logs diagnostic message instead of allowing cleanup path failure.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Improves robustness of error/early-return branches by preventing secondary exceptions during await object cleanup.

## Stabilization Update (2026-02-14) - Phase End Snapshot In Executor Single-Bot Loop
### What changed
- Captured `reportFaze.Faze.TimeEnd` into local snapshot (`phaseTimeEnd`) after validation in `OptimizerExecutor.TestBot(...)`.
- Single-bot wait loop now compares `bot.TimeServer < phaseTimeEnd` instead of repeatedly reading nested phase field.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Reduces sensitivity to concurrent phase-object mutations during long-running single-bot test loop.

## Stabilization Update (2026-02-14) - Full Phase Time Snapshot Validation In Executor TestBot
### What changed
- Extended single-bot phase-time snapshot approach in `OptimizerExecutor.TestBot(...)`:
  - captures both `phaseTimeStart` and `phaseTimeEnd` once;
  - validates range using snapshot values;
  - keeps runtime loop bound to snapshot end time.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Removes repeated nested phase reads and keeps validation/loop limits consistent for full method execution.

## Stabilization Update (2026-02-14) - Safe Optimizer Server Removal Helper In Executor TestBot
### What changed
- Added `SafeRemoveOptimizerServer(OptimizerServer server)` helper in `OptimizerExecutor`.
- Replaced direct `ServerMaster.RemoveOptimizerServer(server)` calls in `TestBot(...)` failure paths with safe helper usage.
- Helper catches cleanup exceptions and logs diagnostics instead of breaking error-handling flow.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Improves resilience of cleanup branches when server removal itself fails.

## Stabilization Update (2026-02-14) - Safe Done-Signal Helper In Master Single-Bot Flow
### What changed
- Added `SafeSignalAloneTestDone()` helper in `OptimizerMaster`.
- Replaced direct `_aloneTestDoneSignal.Set()` calls in single-bot flow with safe helper usage:
  - setup-failure branch;
  - UI wait failure branch;
  - timeout recovery branch;
  - async runner finalization (current run only).
- Helper catches signal exceptions and logs diagnostics.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents cleanup/finalization branch failures if done-signal set operation throws unexpectedly.

## Stabilization Update (2026-02-14) - Unified Recovery Helper In Master Single-Bot Fail Paths
### What changed
- Added `RecoverSingleBotStateAfterFailure(bool invalidateRunId)` helper in `OptimizerMaster`.
- Replaced duplicated recovery logic in setup/UI/timeout fail branches with unified helper call.
- Helper responsibilities:
  - optional run-id invalidation (`Interlocked.Increment`);
  - restore running flag (`Volatile.Write(..., true)`);
  - safe done-signal emit via `SafeSignalAloneTestDone()`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Reduces divergence risk between failure branches and keeps single-bot recovery behavior consistent.

## Stabilization Update (2026-02-14) - Unified Current-Run Check Helper In Async Single-Bot Runner
### What changed
- Added `IsCurrentSingleBotRun(int runId)` helper in `OptimizerMaster`.
- Replaced duplicated `runId == Volatile.Read(ref _aloneTestRunId)` checks in `RunAloneBotTestAsync(...)` with helper calls across:
  - cancel branches;
  - result publish path;
  - exception fallback path;
  - finalization path.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Keeps current-run gating logic centralized and reduces accidental divergence in future edits.

## Stabilization Update (2026-02-14) - Early Stale-Run Exit In Async Single-Bot Runner
### What changed
- Added early `runId` staleness checks in `RunAloneBotTestAsync(...)`:
  - before initial delay;
  - immediately after delay.
- If run is no longer current, async method exits before executor/state work.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Reduces redundant work and side-effect surface for already invalidated single-bot runs.

## Stabilization Update (2026-02-14) - Unified Result Publish Helper For Async Single-Bot Runner
### What changed
- Added `TrySetSingleBotResult(int runId, BotPanel result)` helper in `OptimizerMaster`.
- Replaced duplicated guarded assignments to `_resultBotAloneTest` in `RunAloneBotTestAsync(...)` with helper calls.
- Helper enforces current-run check via `IsCurrentSingleBotRun(runId)` before writing shared result.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Keeps async result publication logic centralized and consistent across success/error/cancel branches.

## Stabilization Update (2026-02-14) - Safe Done-Signal Wait Helper In Master Single-Bot Flow
### What changed
- Added `SafeWaitAloneTestDone(TimeSpan timeout)` helper in `OptimizerMaster`.
- Replaced direct `_aloneTestDoneSignal.Wait(...)` call in `TestBot(...)` with safe helper usage.
- Helper catches wait exceptions, logs diagnostics, and returns `false` to trigger standard timeout-style recovery path.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerMaster.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents unexpected wait exceptions from breaking single-bot control flow without recovery.

## Stabilization Update (2026-02-14) - Safe Cancellation Token Wait Helper In Executor TestBot
### What changed
- Added `SafeWaitCancellationToken(CancellationToken token, TimeSpan timeout)` helper in `OptimizerExecutor`.
- Replaced direct `token.WaitHandle.WaitOne(...)` calls in `TestBot(...)` with safe helper usage.
- Helper handles `ObjectDisposedException` by returning `true` (treat as stop signal), preserving deterministic loop exit.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents token wait-handle disposal race from crashing single-bot wait loop.

## Stabilization Update (2026-02-14) - Safe Bot Cleanup Helper In Executor TestBot
### What changed
- Added `SafeDisposeBotPanel(BotPanel bot)` helper in `OptimizerExecutor`.
- Replaced duplicated `bot.Clear()/bot.Delete()` try-catch blocks in `TestBot(...)` failure paths with helper call.
- Helper catches cleanup exceptions and logs diagnostics.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Keeps bot cleanup logic centralized and resilient under error-path exceptions.

## Stabilization Update (2026-02-14) - Reuse Safe Bot/Server Cleanup In Shared Executor Paths
### What changed
- Extended cleanup hardening beyond single-bot branch:
  - `FinalizeNotStartedBot(...)` now uses `SafeDisposeBotPanel(...)` and `SafeRemoveOptimizerServer(...)`;
  - `server_TestingEndEvent(...)` now uses same safe helpers for bot/server cleanup.
- Updated bot cleanup diagnostic text to generic executor scope (`Optimizer bot cleanup failed`).

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents cleanup exceptions in shared executor event/finalization paths from breaking optimization lifecycle flow.

## Stabilization Update (2026-02-14) - Safe Completion Cancellation Helper In Executor Finalization
### What changed
- Added `SafeTrySetCanceled(TaskCompletionSource<OptimizerReport> completion)` helper in `OptimizerExecutor`.
- Replaced direct `completion.TrySetCanceled()` in `FinalizeNotStartedBot(...)` with safe helper.
- Helper catches and logs cancellation completion exceptions to keep finalization flow resilient.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents rare task-completion cancellation exceptions from interrupting not-started bot finalization.

## Stabilization Update (2026-02-14) - Safe Completion Result Publish Helper In Server End Event
### What changed
- Added `SafeTrySetResult(TaskCompletionSource<OptimizerReport> completion, OptimizerReport report)` helper in `OptimizerExecutor`.
- Replaced direct `completion.TrySetResult(report)` in `server_TestingEndEvent(...)` with safe helper usage.
- Helper catches and logs result-publication exceptions to preserve server-end cleanup flow.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents rare task completion publication failures from breaking end-of-test processing path.

## Stabilization Update (2026-02-14) - Safe Cancel-Callback Helper With Token In Evaluation Startup
### What changed
- Added overload `SafeTrySetCanceled(TaskCompletionSource<OptimizerReport>, CancellationToken)` in `OptimizerExecutor`.
- Replaced direct cancel registration callback body in `StartNewBotForEvaluationAsync(...)`:
  - from `completion.TrySetCanceled(cancellationToken)`
  - to safe helper invocation.
- Helper catches and logs cancellation publication exceptions.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents callback-side exceptions during evaluation cancellation propagation from leaking into scheduler path.

## Stabilization Update (2026-02-14) - Safe Exception Publish Helper In Evaluation Startup
### What changed
- Added `SafeTrySetException(TaskCompletionSource<OptimizerReport>, Exception)` helper in `OptimizerExecutor`.
- Replaced direct `completion.TrySetException(ex)` in `StartNewBotForEvaluationAsync(...)` with safe helper usage.
- Helper catches and logs exception-publication failures.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents rare exception-publication failures from disrupting evaluation startup error handling.

## Stabilization Update (2026-02-14) - Safe Server Slot Release Helper In Executor Paths
### What changed
- Added `SafeReleaseServerSlot()` helper in `OptimizerExecutor`.
- Replaced duplicated `_serverSlots?.Release()` try-catch blocks with helper usage in:
  - out-of-sample slot compensation;
  - evaluation startup canceled-before-start path;
  - evaluation startup exception path;
  - not-started bot finalization;
  - server testing-end cleanup.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Keeps server-slot release behavior centralized and consistent across cleanup branches.

## Stabilization Update (2026-02-14) - Safe Phase Signal Helper In Executor Paths
### What changed
- Added `SafeTrySignalPhase(CountdownEvent phase)` helper in `OptimizerExecutor`.
- Replaced repeated guarded `phase.Signal()` blocks with helper usage in:
  - out-of-sample skip compensation;
  - out-of-sample unscheduled compensation loop;
  - not-started bot finalization;
  - server testing-end cleanup.
- Helper returns `bool` to preserve compensation accounting semantics.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Keeps phase-signal race handling centralized and reduces duplication in cleanup/compensation flow.

## Stabilization Update (2026-02-14) - Server Event Detach Helper In Executor Cleanup
### What changed
- Added `DetachServerEvents(OptimizerServer server)` helper in `OptimizerExecutor`.
- Replaced duplicated event-unsubscribe code in:
  - `FinalizeNotStartedBot(...)`;
  - `server_TestingEndEvent(...)`.
- Helper catches and logs detach exceptions.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Keeps server event-detach behavior centralized and reduces risk of inconsistent unsubscribe logic.

## Stabilization Update (2026-02-14) - Safe Load-To-Last-Faze Helper In Server End Event
### What changed
- Added `SafeLoadBotToLastFaze(BotPanel bot)` helper in `OptimizerExecutor`.
- Replaced direct `ReportsToFazes[ReportsToFazes.Count - 1].Load(bot)` call in `server_TestingEndEvent(...)` with safe helper usage.
- Helper validates collection state (`null`/empty, null last faze) and logs diagnostics instead of throwing.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents end-of-test processing from failing on transient/edge faze collection state.

## Stabilization Update (2026-02-14) - Safe Report Build Helper In Server End Completion Path
### What changed
- Added `TryBuildOptimizerReportFromBot(BotPanel bot, out OptimizerReport report)` helper in `OptimizerExecutor`.
- In `server_TestingEndEvent(...)`, replaced direct report construction/load with helper:
  - on success -> `SafeTrySetResult(...)`;
  - on failure -> `SafeTrySetCanceled(...)` fallback.
- Helper catches and logs report-build failures.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents malformed bot-state payload from breaking evaluation completion publication path.

## Stabilization Update (2026-02-14) - Refined Exception Handling In Safe Server Slot Release
### What changed
- Improved `SafeReleaseServerSlot()` exception handling in `OptimizerExecutor`:
  - explicitly ignores expected races (`ObjectDisposedException`, `SemaphoreFullException`);
  - logs unexpected failures as error.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Keeps common shutdown/over-release races quiet while improving diagnostics for non-expected release failures.

## Stabilization Update (2026-02-14) - Refined Exception Handling In Safe Phase Signal
### What changed
- Improved `SafeTrySignalPhase(...)` in `OptimizerExecutor`:
  - expected race exceptions (`ObjectDisposedException`, `InvalidOperationException`) remain silent with `false` result;
  - added generic `catch (Exception ex)` with error log for non-expected failures.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Preserves race-safe behavior while improving visibility of unexpected phase-signaling failures.

## Stabilization Update (2026-02-14) - Include Exception Details In Safe Slot-Release Error Log
### What changed
- Improved diagnostics in `SafeReleaseServerSlot()`:
  - unexpected catch path now logs full exception details instead of generic message.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves post-mortem analysis when non-expected slot-release failures occur.

## Stabilization Update (2026-02-14) - Unexpected Exception Logging In Safe Token Wait Helper
### What changed
- Extended `SafeWaitCancellationToken(...)` with generic exception handling.
- Besides `ObjectDisposedException`, helper now logs any other wait failure and returns `true` (safe stop behavior).

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Improves diagnostics for rare wait-handle failures while preserving conservative shutdown path semantics.

## Stabilization Update (2026-02-14) - Log Message Normalization In Eval Completion Safe Helpers
### What changed
- Normalized diagnostic prefixes in eval completion safe helpers for consistency:
  - `SafeTrySetCanceled(...)`
  - `SafeTrySetCanceled(..., CancellationToken)`
  - `SafeTrySetResult(...)`
  - `SafeTrySetException(...)`
- Message wording now consistently uses `Optimizer eval completion ... publish failed`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves operational readability of failure logs.

## Stabilization Update (2026-02-14) - Bot Parameters Guard In Safe Load-To-Last-Faze Helper
### What changed
- Added pre-check in `SafeLoadBotToLastFaze(BotPanel bot)`:
  - if `bot.Parameters == null`, load is skipped with error log.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents last-faze load attempts on clearly incomplete bot state payload.

## Stabilization Update (2026-02-14) - Bot Parameters Guard In Safe Report-Build Helper
### What changed
- Added pre-check in `TryBuildOptimizerReportFromBot(...)`:
  - if `bot.Parameters == null`, report build is skipped with error log and `false` return.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Aligns completion-report build path with existing last-faze load guard for null bot parameters.

## Stabilization Update (2026-02-14) - Log Message Harmonization For Null Bot Parameters
### What changed
- Normalized diagnostic text for null `bot.Parameters` checks in both helpers:
  - `SafeLoadBotToLastFaze(...)`
  - `TryBuildOptimizerReportFromBot(...)`
- Unified message: `Optimizer report build/load skipped: bot parameters are null.`

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves log consistency for alerting/filtering.

## Stabilization Update (2026-02-14) - Null Server Guard In FinalizeNotStartedBot
### What changed
- Added early null-server guard in `FinalizeNotStartedBot(...)`.
- If `server == null`, method now:
  - logs diagnostic error;
  - compensates phase completion via safe signal helper;
  - releases server slot via safe release helper;
  - returns without touching server-indexed structures.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents potential null dereference on `server.NumberServer` in rare not-started bot edge paths.

## Stabilization Update (2026-02-14) - Input Guard In CreateNewServer
### What changed
- Added early input validation in `CreateNewServer(OptimizerFazeReport report, bool needToDelete)`.
- If `report == null` or `report.Faze == null`, method now logs error and returns `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null dereference in server bootstrap path when phase/report context is incomplete.

## Stabilization Update (2026-02-14) - Master Context Guard In CreateNewServer
### What changed
- Extended `CreateNewServer(...)` preconditions with master-context checks.
- Method now validates `_master`, `_master.Storage`, and `_master.BotToTest` before server creation; logs and returns `null` if context is incomplete.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents bootstrap-time null dereferences when optimizer master dependencies are unavailable.

## Stabilization Update (2026-02-14) - Storage Securities Guard In CreateNewServer
### What changed
- Added explicit guard in `CreateNewServer(...)` for `_master.Storage.Securities == null`.
- Method now logs and returns `null` before tab/security binding logic when securities collection is unavailable.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null dereference in security lookup (`Find(...)`) during server bootstrap.

## Stabilization Update (2026-02-14) - Bot Tabs Guard In CreateNewServer
### What changed
- Added guard for `_master.BotToTest.GetTabs()` result in `CreateNewServer(...)`.
- If tabs collection is `null`, method now:
  - logs error;
  - removes partially created optimizer server via safe helper;
  - returns `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null collection iteration during server bootstrap and avoids leaked partially created server instance.

## Stabilization Update (2026-02-14) - Null Tab Source Guard In CreateNewServer
### What changed
- Added guard for `null` entries in bot tabs source list inside `CreateNewServer(...)`.
- Null entries are now skipped with diagnostic log including source index.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null dereference on `sources[i].TabType` during server bootstrap when tabs list contains null gaps.

## Stabilization Update (2026-02-14) - Missing Security Guard In CreateNewServer Tab Binding
### What changed
- Added `secToStart == null` guards before `GetDataToSecurity(...)` calls in all tab binding branches:
  - `BotTabSimple`;
  - `BotTabIndex`;
  - `BotTabScreener`.
- Missing securities are now skipped with explicit error logs including missing security name.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents passing null security references into optimizer server data binding path.

## Stabilization Update (2026-02-14) - Null Connector/Tab Guards In CreateNewServer Binding Loops
### What changed
- Added null guards for tab connector structures before security/timeframe access:
  - `BotTabSimple`: `simple?.Connector != null`;
  - `BotTabIndex`: `index.Tabs[i2] != null`;
  - `BotTabScreener`: `screener.Tabs[i2]?.Connector != null`.
- Null entries are skipped with detailed index-based diagnostics.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null dereference in tab-binding loops when connector/tab metadata is partially missing.

## Stabilization Update (2026-02-14) - Null Tabs Collection Guards For Index/Screener In CreateNewServer
### What changed
- Added collection-level guards before iterating tab lists:
  - `BotTabIndex`: `index?.Tabs != null`;
  - `BotTabScreener`: `screener?.Tabs != null`.
- Null collections are now skipped with source-index diagnostics.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null list iteration in index/screener binding paths during server initialization.

## Stabilization Update (2026-02-14) - Empty Security Name Guards In CreateNewServer Binding
### What changed
- Added `string.IsNullOrWhiteSpace(...)` guards for security names before lookup:
  - `BotTabSimple`: `simple.Connector.SecurityName`;
  - `BotTabIndex`: `index.Tabs[i2].SecurityName`;
  - `BotTabScreener`: `screener.Tabs[i2].Connector.SecurityName`.
- Empty names are skipped with source/tab index diagnostics.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents invalid security lookup attempts with blank identifiers in server data-binding phase.

## Stabilization Update (2026-02-14) - Server Factory Exception/Null Guards In CreateNewServer
### What changed
- Hardened server factory call in `CreateNewServer(...)`:
  - wrapped `ServerMaster.CreateNextOptimizerServer(...)` in `try/catch`;
  - added explicit `null` result guard.
- On failure paths method now logs detailed diagnostics and returns `null` early.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents server bootstrap path from continuing with invalid server instance when factory fails.

## Stabilization Update (2026-02-14) - Safe Security Binding Helper In CreateNewServer
### What changed
- Added `SafeBindSecurityToServer(...)` helper in `OptimizerExecutor`.
- Replaced direct `server.GetDataToSecurity(...)` calls in all `CreateNewServer(...)` tab branches:
  - simple;
  - index;
  - screener.
- Helper catches per-security bind exceptions and logs detailed source context (`source kind`, `source index`, optional `tab index`, `security`).

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents one failed security bind from aborting entire server bootstrap and preserves diagnostic traceability.

## Stabilization Update (2026-02-14) - Empty Security Name Guard In SafeBindSecurityToServer
### What changed
- Added `security.Name` emptiness guard inside `SafeBindSecurityToServer(...)`.
- If security name is empty, bind is skipped with detailed source-context log.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents malformed security entities from reaching `GetDataToSecurity(...)` path and improves error context.

## Stabilization Update (2026-02-14) - Safe Security Lookup Helper In CreateNewServer
### What changed
- Added `TryFindSecurityByName(string securityName, out Security security)` helper in `OptimizerExecutor`.
- Replaced direct `List.Find(...)` lookups against `_master.Storage.Securities` in `CreateNewServer(...)` with safe helper usage.
- Helper:
  - tolerates null/empty input and null storage collection;
  - safely iterates securities list while skipping null entries;
  - logs lookup exceptions with security context.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null-entry dereference in securities list during lookup (`s => s.Name`) and keeps binding flow resilient.

## Stabilization Update (2026-02-14) - Remove Redundant Post-Lookup Null Checks In CreateNewServer
### What changed
- Simplified `CreateNewServer(...)` binding branches by removing duplicate `secToStart == null` checks after `TryFindSecurityByName(...)`.
- Behavior is unchanged because helper success path already guarantees non-null `security`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional change; reduces branching noise and keeps lookup flow easier to maintain.

## Stabilization Update (2026-02-14) - Security Lookup Diagnostics For Empty Input And Missing Storage
### What changed
- Updated `TryFindSecurityByName(...)` in `OptimizerExecutor` to split early-return guard conditions.
- Added explicit diagnostic log when lookup is skipped because `securityName` is empty.
- Added explicit diagnostic log when lookup is skipped because `_master.Storage.Securities` is unavailable.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change in control flow; improves root-cause visibility for skipped lookup cases.

## Stabilization Update (2026-02-14) - Security Lookup Diagnostics For Not Found Cases
### What changed
- Updated `TryFindSecurityByName(...)` in `OptimizerExecutor`.
- Added explicit diagnostic log when lookup completes without exceptions but security with requested name is not found.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No control-flow change; only improves post-mortem visibility when requested instrument is missing in storage.

## Stabilization Update (2026-02-14) - Remove Duplicate Missing-Security Logs In CreateNewServer
### What changed
- Cleaned up `CreateNewServer(...)` security-lookup call sites for simple/index/screener tabs.
- Removed duplicate `not found` logs after failed `TryFindSecurityByName(...)` calls.
- Kept missing-security diagnostics centralized in `TryFindSecurityByName(...)`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change; reduces log noise and avoids duplicate error lines for the same lookup miss.

## Stabilization Update (2026-02-14) - Normalize Security Name Matching In Lookup Helper
### What changed
- Updated `TryFindSecurityByName(...)` in `OptimizerExecutor` to normalize input security names via `Trim()`.
- Switched comparison to explicit `StringComparison.Ordinal` after trimming candidate names.
- Added guard to skip null/whitespace candidate names during list iteration.
- Updated diagnostics to log normalized lookup key.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Slight behavior tightening: avoids false misses caused by accidental leading/trailing spaces and culture-dependent comparison semantics.

## Stabilization Update (2026-02-14) - Add Null-Dependency Diagnostics In Safe Security Bind Helper
### What changed
- Updated `SafeBindSecurityToServer(...)` in `OptimizerExecutor`.
- Added explicit diagnostic log when bind is skipped because `server` and/or `security` is null.
- Log now includes source context (`source kind`, `source index`, optional `tab index`) and exact missing dependency state.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No control-flow change; improves visibility for unexpected null dependency paths in server-security binding.

## Stabilization Update (2026-02-14) - Guard Bot Tabs Retrieval In CreateNewServer
### What changed
- Updated `CreateNewServer(...)` in `OptimizerExecutor` to safely retrieve bot tabs.
- Wrapped `_master.BotToTest.GetTabs()` in `try/catch`.
- On retrieval exception, method now logs error details, performs `SafeRemoveOptimizerServer(server)`, and returns `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; prevents unhandled tab-source exceptions from escaping server initialization.

## Stabilization Update (2026-02-14) - Log Unsupported Bot Tab Types In CreateNewServer
### What changed
- Updated `CreateNewServer(...)` in `OptimizerExecutor`.
- Added fallback branch to log unsupported `BotTabType` values encountered in bot tab sources.
- Log includes source index and concrete tab type value.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; improves diagnostics when new/unknown tab types appear without bind handling.

## Stabilization Update (2026-02-14) - Log Busy Worker Early Exit In Single-Bot Test
### What changed
- Updated `TestBot(...)` in `OptimizerExecutor`.
- Added explicit system log when single-bot test request is ignored because previous `_primeThreadWorker` is still active.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves observability of intentional request drops under concurrent test attempts.

## Stabilization Update (2026-02-14) - Guard Tab Casts In CreateNewServer
### What changed
- Updated `CreateNewServer(...)` in `OptimizerExecutor` to use safe `as` casts for tab instances.
- Added explicit error logs and skip behavior when tab instance type does not match declared `TabType` for:
  - `BotTabType.Simple`
  - `BotTabType.Index`
  - `BotTabType.Screener`

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; prevents `InvalidCastException` from malformed tab source entries.

## Stabilization Update (2026-02-14) - Validate Faze Time Range In CreateNewServer
### What changed
- Updated `CreateNewServer(...)` in `OptimizerExecutor`.
- Added early guard for invalid phase time range (`TimeEnd <= TimeStart`).
- Method now logs and returns `null` before server creation when phase range is invalid.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior change only for invalid input; prevents unnecessary server allocation and downstream bind/start failures.

## Stabilization Update (2026-02-14) - Guard Bot Creation Exception Path In Single-Bot Test
### What changed
- Updated `TestBot(...)` in `OptimizerExecutor`.
- Wrapped `CreateNewBot(...)` call in `try/catch`.
- On bot creation exception, method now logs error details, performs `SafeRemoveOptimizerServer(server)`, disposes await object, and returns `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; prevents unhandled bot-factory exceptions from escaping single-bot execution flow.


## Stabilization Update (2026-02-14) - Guard Master Context In Single-Bot Test
### What changed
- Updated `TestBot(...)` in `OptimizerExecutor`.
- Added explicit `_master == null` guard before accessing optimizer master fields.
- On missing master context, method now logs error, disposes await object, and returns `null`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; prevents potential null-reference crash during single-bot setup.


## Stabilization Update (2026-02-14) - Guard Null Server In StartNewBot
### What changed
- Updated `StartNewBot(...)` in `OptimizerExecutor`.
- Added early guard when `CreateNewServer(...)` returns `null`.
- Method now logs a clear error, safely cancels optional completion source, and returns before using `server.NumberServer`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents null-reference crash in optimizer/evaluation startup path when server creation fails.


## Stabilization Update (2026-02-14) - Guard TestingStart Exception In StartNewBot
### What changed
- Updated `StartNewBot(...)` in `OptimizerExecutor`.
- Wrapped `server.TestingStart()` in `try/catch`.
- On start exception, method now logs error details and calls `FinalizeNotStartedBot(server, bot)` for centralized cleanup/signaling.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; prevents unhandled server start exceptions from escaping optimizer worker flow.


## Stabilization Update (2026-02-14) - Harden CreateNewBot With Input/Context Guards
### What changed
- Updated `CreateNewBot(...)` in `OptimizerExecutor`.
- Added explicit guards with diagnostics for:
  - missing optimizer master/bot context;
  - null optimizer server;
  - empty bot name;
  - null parameter collections.
- Wrapped `_botConfigurator.CreateAndConfigureBot(...)` in `try/catch` and now return `null` on failure with error log.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; centralizes bot creation failure handling and reduces exception leakage from factory/configurator path.


## Stabilization Update (2026-02-14) - Remove Silent Catch In StartNewBot Name Prefix Logic
### What changed
- Updated `StartNewBot(...)` bot name prefix logic in `OptimizerExecutor`.
- Replaced exception-based prefix check (`Substring/Convert` + empty `catch`) with explicit validation:
  - if `botName` is empty -> log and fallback to `server.NumberServer`;
  - if first char is non-digit -> prefix `server.NumberServer` to bot name.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; removes silent exception path and makes fallback deterministic and observable.


## Stabilization Update (2026-02-14) - Remove Stale Bot Entries During Early Finalization
### What changed
- Updated `FinalizeNotStartedBot(...)` in `OptimizerExecutor` to remove bot from `_botsInTest` before bot disposal/server cleanup.
- Added `SafeRemoveBotFromInTest(...)` helper with lock-based removal and guarded diagnostics.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Prevents stale/disposed bot references from remaining in in-test collection after early-start failure paths.


## Stabilization Update (2026-02-14) - Guard Pending Evaluation Map Collisions In StartNewBot
### What changed
- Updated `StartNewBot(...)` in `OptimizerExecutor` for evaluation path registration.
- Replaced direct assignment into `_pendingEvaluationByServer` with `TryAdd(...)`.
- On duplicate key collision for `server.NumberServer`, method now:
  - logs explicit error;
  - cancels provided completion source;
  - finalizes not-started server via `FinalizeNotStartedBot(...)`;
  - returns early.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior change only for invalid duplicate-registration state; prevents silent completion-source replacement and potential dangling waiters.


## Stabilization Update (2026-02-14) - Centralize Null-Server StartNewBot Finalization
### What changed
- Updated `StartNewBot(...)` in `OptimizerExecutor` for `CreateNewServer(...) == null` path.
- After canceling optional completion source, method now calls `FinalizeNotStartedBot(null, null)` before return.
- This reuses centralized phase signaling and server-slot release logic for early startup failure.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior change on error path only; prevents possible phase wait imbalance / slot leakage when server creation fails before registration.



## Stabilization Update (2026-02-14) - Log Missing Bot Correlation In Server End Event
### What changed
- Updated `server_TestingEndEvent(...)` in `OptimizerExecutor`.
- Added explicit error log when no bot is found in `_botsInTest` for incoming `serverNum`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves diagnostics for server/bot correlation drift during completion handling.


## Stabilization Update (2026-02-14) - Cancel Pending Evaluation When End Event Has No Bot
### What changed
- Updated `server_TestingEndEvent(...)` in `OptimizerExecutor` for missing-bot path.
- If bot is not found for `serverNum`, method now tries to remove pending completion from `_pendingEvaluationByServer` and safely cancels it.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior change on mismatch path only; prevents dangling evaluation awaiters when server completion arrives without correlated bot.


## Stabilization Update (2026-02-14) - Log Missing Server Correlation In End Event Cleanup
### What changed
- Updated `server_TestingEndEvent(...)` in `OptimizerExecutor`.
- Added explicit error log when completed `serverNum` is not found in active `_servers` list during cleanup.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves diagnostics for server list correlation drift at completion time.


## Stabilization Update (2026-02-14) - Harden End-Event ETA Against Missing/Zero Threads Count
### What changed
- Updated `server_TestingEndEvent(...)` in `OptimizerExecutor`.
- Added normalized `threadsCount = Math.Max(1, _master?.ThreadsCount ?? 1)` for ETA calculations.
- Replaced direct `_master.ThreadsCount` usage in threshold check and division with guarded `threadsCount`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; prevents null-reference and divide-by-zero failures in completion-time estimation path.


## Stabilization Update (2026-02-14) - Add Diagnostics For Synchronization Cleanup Exceptions
### What changed
- Updated `DisposeRunSynchronization()` in `OptimizerExecutor`.
- Replaced silent `catch` blocks with `catch (Exception ex)` and explicit error logs for failures during dispose of:
  - `_phaseCompletion`;
  - `_serverSlots`;
  - `_stopCts`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No control-flow change; improves observability of synchronization cleanup failures while preserving existing null-reset behavior.


## Stabilization Update (2026-02-14) - Guard Unexpected Exceptions In Server Slot Acquire
### What changed
- Updated `TryAcquireServerSlot()` in `OptimizerExecutor`.
- Added fallback `catch (Exception ex)` branch.
- On unexpected acquire failure, method now logs details and returns `false`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; prevents optimizer worker crash on unforeseen semaphore/acquire exceptions.



## Stabilization Update (2026-02-14) - Guard Unexpected Exceptions In Phase Completion Wait
### What changed
- Updated `WaitCurrentPhaseToComplete()` in `OptimizerExecutor`.
- Added fallback `catch (Exception ex)` branch.
- On unexpected wait failure, method now logs details and returns safely.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; prevents optimizer worker crash on unforeseen phase-wait exceptions.


## Stabilization Update (2026-02-14) - Explicitly Cancel Evaluation Completion On Early Cancellation
### What changed
- Updated `StartNewBotForEvaluationAsync(...)` in `OptimizerExecutor`.
- In early `cancellationToken.IsCancellationRequested` branch, method now explicitly calls `SafeTrySetCanceled(completion, cancellationToken)` before releasing slot.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; removes dependence on cancellation-registration race for completion state publication.



## Stabilization Update (2026-02-14) - Guard Phase IsSet Access In SafeTrySignalPhase
### What changed
- Updated `SafeTrySignalPhase(...)` in `OptimizerExecutor`.
- Moved `phase.IsSet` check inside `try` block.
- Method now avoids potential unhandled `ObjectDisposedException` from `IsSet` access during concurrent disposal.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; improves race-safety for phase signaling under concurrent shutdown/dispose.


## Stabilization Update (2026-02-14) - Guard Prime Progress Event Dispatch In End Event
### What changed
- Updated `server_TestingEndEvent(...)` in `OptimizerExecutor`.
- Wrapped `PrimeProgressChangeEvent?.Invoke(...)` in `try/catch`.
- On subscriber exception, method now logs error and continues cleanup flow.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; prevents event-subscriber failures from interrupting server/bot cleanup and synchronization release.


## Stabilization Update (2026-02-14) - Guard ETA Event Dispatch In Server End Flow
### What changed
- Updated `server_TestingEndEvent(...)` in `OptimizerExecutor`.
- Wrapped `TimeToEndChangeEvent(...)` invocation in `try/catch`.
- On subscriber exception, method now logs error and continues completion handling.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; prevents ETA subscriber faults from interrupting end-event workflow.


## Stabilization Update (2026-02-14) - Guard Initial Testing Progress Event In End Handler
### What changed
- Updated `server_TestingEndEvent(...)` in `OptimizerExecutor`.
- Wrapped initial `TestingProgressChangeEvent?.Invoke(100, 100, serverNum)` in `try/catch`.
- On subscriber exception, method now logs error and continues end-event cleanup flow.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; prevents progress subscriber faults from aborting completion cleanup logic.



## Stabilization Update (2026-02-14) - Guard Streaming Testing Progress Event Dispatch
### What changed
- Updated `server_TestingProgressChangeEvent(...)` in `OptimizerExecutor`.
- Added explicit `null` early return for `TestingProgressChangeEvent`.
- Wrapped subscriber invocation in `try/catch` with diagnostic logging on failure.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; prevents progress subscriber exceptions from interrupting streaming progress updates.



## Stabilization Update (2026-02-14) - Guard TestReady Event Dispatches With Safe Helper
### What changed
- Added `SafeInvokeTestReady(List<OptimizerFazeReport> reports)` helper in `OptimizerExecutor`.
- Replaced direct `TestReadyEvent?.Invoke(...)` calls with safe helper in all current dispatch points:
  - stop-request exit in prime worker;
  - normal completion of prime worker;
  - out-of-sample unscheduled-tail early return.
- Helper catches subscriber exceptions, logs diagnostics, and preserves optimizer control flow.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; prevents test-ready subscribers from aborting optimizer completion/early-exit paths.


## Stabilization Update (2026-02-14) - Centralize Safe Prime Progress Event Dispatch
### What changed
- Added `SafeInvokePrimeProgress(int progressEnd, int progressMax)` helper in `OptimizerExecutor`.
- Replaced all current direct/inline `PrimeProgressChangeEvent` invocations with helper usage in:
  - in-sample phase start;
  - out-of-sample phase start;
  - out-of-sample zero-work branch;
  - compensated progress path;
  - server end-event tail.
- Removed duplicated inline `try/catch` around prime progress dispatch in favor of shared helper.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; standardizes subscriber-fault isolation for all prime progress notifications.


## Stabilization Update (2026-02-14) - Centralize Safe Testing Progress Event Dispatch
### What changed
- Added `SafeInvokeTestingProgress(int curVal, int maxVal, int numServer)` helper in `OptimizerExecutor`.
- Replaced direct testing-progress dispatch calls with helper usage in:
  - `server_TestingEndEvent(...)` initial 100/100 publish;
  - `server_TestingProgressChangeEvent(...)` streaming progress publish.
- Unified failure logging for progress-event subscriber exceptions.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; standardizes subscriber-fault isolation for all testing-progress notifications.


## Stabilization Update (2026-02-14) - Guard Log Event Dispatch In SendLogMessage
### What changed
- Updated `SendLogMessage(...)` in `OptimizerExecutor`.
- Added explicit `null` early return for `LogMessageEvent`.
- Wrapped event dispatch in `try/catch` to prevent subscriber exceptions from breaking optimizer control flow.
- Catch block is intentionally non-logging to avoid recursive log-failure loops.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; isolates external log sink failures from optimizer runtime.


## Stabilization Update (2026-02-14) - Add Prime Worker Exception Fallback Publish
### What changed
- Updated `PrimeThreadWorkerPlace()` in `OptimizerExecutor`.
- Added `catch (Exception ex)` around main worker body.
- On unexpected failure, worker now:
  - logs detailed error;
  - triggers `SafeInvokeTestReady(ReportsToFazes)` to publish completion/fallback state.
- Existing `finally` cleanup (`_primeThreadWorker = null`, `DisposeRunSynchronization()`) remains unchanged.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior change on exception path only; improves failure signaling consistency to subscribers/UI when prime worker aborts unexpectedly.


## Stabilization Update (2026-02-14) - Add Context Diagnostics For Async Bot Factory Startup
### What changed
- Updated `StartAsuncBotFactoryInSample(...)` in `OptimizerExecutor`:
  - wrapped `_asyncBotFactory.CreateNewBots(...)` in `try/catch`;
  - on failure logs phase context and bot count, then rethrows.
- Updated `StartAsuncBotFactoryOutOfSample(...)` similarly:
  - logs out-of-sample context and bot count on failure, then rethrows.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; improves root-cause visibility for async bot factory startup failures while preserving existing error propagation semantics.


## Stabilization Update (2026-02-14) - Add Input Guards For Async Bot Factory Startup Methods
### What changed
- Updated `StartAsuncBotFactoryInSample(...)` in `OptimizerExecutor`:
  - added guard for non-positive `botCount` with informational skip log;
  - added guard for empty `botType` with error log.
- Updated `StartAsuncBotFactoryOutOfSample(...)`:
  - added guard for empty `botType`;
  - added guard for missing `reportFiltered.Reports` source collection.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior change only on invalid input paths; prevents avoidable null/argument failures during async bot factory bootstrap.


## Stabilization Update (2026-02-14) - Skip Async Bot Factory Calls On Empty Name Batches
### What changed
- Updated `StartAsuncBotFactoryInSample(...)` in `OptimizerExecutor`:
  - added guard to skip factory call when generated `botNames` list is empty.
- Updated `StartAsuncBotFactoryOutOfSample(...)` similarly:
  - skip factory call when filtered/generated bot name batch is empty.
- Both paths now emit explicit system diagnostics for empty batch skip.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior change only on empty-batch paths; avoids unnecessary factory invocations and clarifies why no async bot creation was started.


## Stabilization Update (2026-02-14) - Guard Empty Source Bot Names In OutOfSample Factory Startup
### What changed
- Updated `StartAsuncBotFactoryOutOfSample(...)` in `OptimizerExecutor`.
- Added guard for empty/null `sourceReport.BotName` before `.Replace(" InSample", "")` transformation.
- Invalid entries are now skipped with indexed diagnostic log instead of risking null-reference failure.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior change only for invalid source report names; improves robustness of out-of-sample async bot batch generation.


## Stabilization Update (2026-02-14) - Avoid Source Report List Mutation In OutOfSample Factory Startup
### What changed
- Updated `StartAsuncBotFactoryOutOfSample(...)` in `OptimizerExecutor`.
- Removed in-loop mutation of `reportFiltered.Reports` (`RemoveAt` + index rewind) for null entries.
- Null source reports are now skipped with indexed diagnostic log, leaving input report collection unchanged.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; reduces side effects and keeps source optimization report list stable for downstream consumers.


## Stabilization Update (2026-02-14) - Validate Transformed OutOfSample Bot Names Before Enqueue
### What changed
- Updated `StartAsuncBotFactoryOutOfSample(...)` in `OptimizerExecutor`.
- Added normalization/validation for transformed source bot name after `Replace(" InSample", "")`:
  - now trims transformed base name;
  - skips entry with diagnostic log if transformed result is empty/whitespace.
- Only validated names are suffixed with `" OutOfSample"` and queued.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior change only on invalid naming edge-cases; prevents malformed out-of-sample bot names from reaching async factory startup.


## Stabilization Update (2026-02-14) - Normalize Faze Labels In Async Bot Factory Naming
### What changed
- Updated `StartAsuncBotFactoryInSample(...)` in `OptimizerExecutor`:
  - added normalized phase label (`normalizedFaze`) with fallback to `"InSample"` for empty input;
  - bot names now use normalized phase label.
- Updated `StartAsuncBotFactoryOutOfSample(...)` similarly:
  - added normalized phase label fallback to `"OutOfSample"`;
  - generated bot names now append normalized phase label instead of hardcoded suffix.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change for existing call sites; improves resilience and naming consistency when phase label input is empty or whitespace.


## Stabilization Update (2026-02-14) - Deduplicate Async Bot Factory Name Batches
### What changed
- Updated `StartAsuncBotFactoryInSample(...)` in `OptimizerExecutor`:
  - added `HashSet<string>`-based duplicate filtering for generated bot names;
  - duplicate names are skipped with diagnostics.
- Updated `StartAsuncBotFactoryOutOfSample(...)` similarly:
  - deduplicates transformed out-of-sample bot names before enqueue;
  - duplicates are skipped with diagnostics.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior change only when duplicate names are produced; avoids redundant/ambiguous async bot creation requests.


## Stabilization Update (2026-02-14) - Normalize Bot Type Input For Async Bot Factory Startup
### What changed
- Updated `StartAsuncBotFactoryInSample(...)` and `StartAsuncBotFactoryOutOfSample(...)` in `OptimizerExecutor`.
- Added normalized bot type (`normalizedBotType = botType?.Trim()`) with existing empty-value guards.
- Async factory calls now use normalized bot type value.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change for valid input; reduces failures from accidental leading/trailing whitespace in strategy type names.


## Stabilization Update (2026-02-14) - Make Async Name Dedup Comparison Explicit
### What changed
- Updated async bot-name dedup sets in `OptimizerExecutor` to use explicit `StringComparer.Ordinal`.
- Applied in both:
  - `StartAsuncBotFactoryInSample(...)`;
  - `StartAsuncBotFactoryOutOfSample(...)`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change expected; makes string comparison intent explicit and stable.


## Stabilization Update (2026-02-14) - Stabilize OutOfSample Name Extraction With Local Snapshot
### What changed
- Updated loop in `StartAsuncBotFactoryOutOfSample(...)` in `OptimizerExecutor`.
- Added local snapshot variables per iteration:
  - `sourceReport` for report entry;
  - `sourceBotName` for source bot name.
- Subsequent validation and transform now use snapshot values instead of repeated indexed property access.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change expected; improves readability and reduces repeated dereference/race surface in source name transformation path.


## Stabilization Update (2026-02-14) - Strip Only Terminal InSample Suffix In OutOfSample Naming
### What changed
- Updated out-of-sample bot name transform in `StartAsuncBotFactoryOutOfSample(...)`.
- Replaced global `Replace(" InSample", "")` with explicit terminal-suffix removal logic:
  - remove `" InSample"` only when it is the final suffix (`EndsWith(..., Ordinal)`);
  - keep internal occurrences untouched.
- Kept post-transform trim and empty-name guard unchanged.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior change for edge-case names containing `" InSample"` in the middle; avoids accidental over-normalization and preserves original name intent.


## Stabilization Update (2026-02-14) - Hoist OutOfSample InSample Suffix Constant Outside Loop
### What changed
- Updated `StartAsuncBotFactoryOutOfSample(...)` in `OptimizerExecutor`.
- Moved `" InSample"` suffix constant declaration out of per-item loop to method scope.
- Loop now reuses one local constant for terminal suffix stripping.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change; small readability/maintenance cleanup.


## Stabilization Update (2026-02-14) - Snapshot OutOfSample Report List For Async Factory Loop
### What changed
- Updated `StartAsuncBotFactoryOutOfSample(...)` in `OptimizerExecutor`.
- Added local list snapshot `reports = reportFiltered.Reports`.
- Loop now iterates via local `reports` reference instead of repeated property dereference on `reportFiltered.Reports`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change; small readability/maintenance improvement and reduced repeated dereference noise.



## Stabilization Update (2026-02-14) - Prevent Duplicate Phase Suffix In OutOfSample Bot Names
### What changed
- Updated out-of-sample name construction in `StartAsuncBotFactoryOutOfSample(...)`.
- Added phase-suffix check before append:
  - if transformed name already ends with target phase suffix, keep it as-is;
  - otherwise append suffix normally.
- Prevents malformed names like duplicated phase suffixes in edge-case source data.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior change only for malformed/edge-case inputs where source already includes target phase suffix; avoids duplicate suffix artifacts in queued bot names.


## Stabilization Update (2026-02-14) - Normalize InSample Name Suffix Append Semantics
### What changed
- Updated in-sample bot name construction in `StartAsuncBotFactoryInSample(...)`.
- Name is now built from base (`"<server> OpT"`) plus guarded suffix append logic:
  - append normalized phase suffix only if base name does not already end with it.
- This keeps suffix-append behavior consistent with out-of-sample naming safeguards.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change expected for current input shape; aligns naming logic and avoids accidental duplicate suffixes in edge extensions.


## Stabilization Update (2026-02-14) - Hoist InSample Phase Suffix Outside Name Loop
### What changed
- Updated `StartAsuncBotFactoryInSample(...)` in `OptimizerExecutor`.
- Moved `fazeSuffix` construction out of per-item loop to method-local precomputed variable.
- Loop now reuses one suffix value when building bot names.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change; minor allocation/readability optimization.


## Stabilization Update (2026-02-14) - Hoist OutOfSample Phase Suffix Outside Name Loop
### What changed
- Updated `StartAsuncBotFactoryOutOfSample(...)` in `OptimizerExecutor`.
- Moved `fazeSuffix` construction out of per-item loop to a method-local precomputed variable.
- Loop now reuses one suffix value for guarded append logic.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change; minor allocation/readability optimization consistent with in-sample branch.


## Stabilization Update (2026-02-14) - Trim Source Name Before InSample Suffix Detection In OOS Transform
### What changed
- Updated out-of-sample source name transform in `StartAsuncBotFactoryOutOfSample(...)`.
- Source bot name is now trimmed before terminal `" InSample"` suffix detection.
- This ensures suffix stripping works correctly even when source names contain trailing spaces.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior change only for whitespace-padded source names; improves deterministic OOS name normalization.


## Stabilization Update (2026-02-14) - Enrich Async Factory Failure Diagnostics With Type/Faze Context
### What changed
- Updated async bot factory failure logs in `OptimizerExecutor` for both in-sample and out-of-sample startup paths.
- Failure diagnostics now include:
  - batch count;
  - normalized bot type;
  - normalized faze label.
- Error propagation behavior (`throw`) remains unchanged.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No control-flow change; improves post-mortem context when async factory startup fails.


## Stabilization Update (2026-02-14) - Include Script Mode In Async Factory Failure Diagnostics
### What changed
- Updated async factory failure logs in `OptimizerExecutor` (InSample and OutOfSample paths).
- Added `isScript` flag to diagnostic payload for startup failures.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves observability by separating script/non-script failure contexts.


## Stabilization Update (2026-02-14) - Preallocate Async Bot Name Batch Capacity
### What changed
- Updated async bot-name batch allocation in `OptimizerExecutor`:
  - `StartAsuncBotFactoryInSample(...)`: `new List<string>(botCount)`;
  - `StartAsuncBotFactoryOutOfSample(...)`: `new List<string>(reports.Count)`.
- Keeps existing naming/filtering behavior unchanged while reducing potential list growth reallocations.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change; micro-optimization for batch construction paths.


## Stabilization Update (2026-02-14) - Preallocate Async Name Dedup HashSet Capacity
### What changed
- Updated dedup hash set allocations in `OptimizerExecutor`:
  - `StartAsuncBotFactoryInSample(...)`: `new HashSet<string>(botCount, StringComparer.Ordinal)`;
  - `StartAsuncBotFactoryOutOfSample(...)`: `new HashSet<string>(reports.Count, StringComparer.Ordinal)`.
- Keeps existing dedup logic unchanged while reducing potential hash-set growth reallocations.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change; micro-optimization for name dedup paths.


## Stabilization Update (2026-02-14) - Guard Async Batch Capacity Inputs With Non-Negative Clamp
### What changed
- Updated async batch collection preallocation in `OptimizerExecutor`.
- Added `expectedNamesCount = Math.Max(0, ...)` guard before creating list/hashset capacities in:
  - `StartAsuncBotFactoryInSample(...)` (from `botCount`);
  - `StartAsuncBotFactoryOutOfSample(...)` (from `reports.Count`).
- Collection constructors now use clamped capacity values.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No normal-path behavior change; defensive guard against unexpected negative-capacity propagation.


## Stabilization Update (2026-02-14) - Snapshot OutOfSample Reports Into Local Copy Before Iteration
### What changed
- Updated `StartAsuncBotFactoryOutOfSample(...)` in `OptimizerExecutor`.
- Switched from direct reference to source list (`reportFiltered.Reports`) to local snapshot copy:
  - `new List<OptimizerReport>(reportFiltered.Reports)`.
- Iteration now uses stable local snapshot, reducing sensitivity to external list mutations during batch build.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No intended functional behavior change; improves robustness and determinism under concurrent/report-list mutation scenarios.


## Stabilization Update (2026-02-14) - Add Early Empty-Snapshot Guard In OOS Async Factory Startup
### What changed
- Updated `StartAsuncBotFactoryOutOfSample(...)` in `OptimizerExecutor`.
- Added early-return guard after local source snapshot creation when `reports.Count == 0`.
- Method now emits explicit system diagnostic for empty snapshot and skips downstream batch construction loop.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change vs eventual empty-batch skip; improves clarity and avoids unnecessary iteration setup when source snapshot is empty.


## Stabilization Update (2026-02-14) - Enrich OOS Empty-Snapshot Skip Diagnostics
### What changed
- Updated early empty-snapshot log in `StartAsuncBotFactoryOutOfSample(...)`.
- Diagnostic now includes normalized `botType` and `faze` context in addition to skip reason.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves context quality for skip diagnostics.


## Stabilization Update (2026-02-14) - Enrich InSample Skip Diagnostics With Type/Faze Context
### What changed
- Updated in-sample async factory skip logs in `OptimizerExecutor`.
- Enriched diagnostics for:
  - non-positive bot count skip;
  - empty generated-name batch skip.
- Messages now include `botType` and `faze` context (and `count` where relevant).

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves contextual diagnostics for in-sample startup skips.



## Stabilization Update (2026-02-14) - Include Count In OOS Empty-Snapshot Skip Diagnostic
### What changed
- Updated early empty-snapshot skip log in `StartAsuncBotFactoryOutOfSample(...)`.
- Diagnostic now also includes `count` field for snapshot size (in addition to `botType` and `faze`).

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves log schema consistency across in-sample/out-of-sample skip diagnostics.


## Stabilization Update (2026-02-14) - Use Stable Local Reports Count In OOS Factory Loop
### What changed
- Updated `StartAsuncBotFactoryOutOfSample(...)` in `OptimizerExecutor`.
- Added local `reportsCount` snapshot and switched loop condition from `reports.Count` to `reportsCount`.
- Keeps loop boundaries explicit and avoids repeated property access in iteration condition.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change; minor readability/consistency optimization.


## Stabilization Update (2026-02-14) - Add Expected Batch Size To InSample Empty-Names Diagnostic
### What changed
- Updated InSample empty-batch skip log in `StartAsuncBotFactoryInSample(...)`.
- Diagnostic now includes `expectedNamesCount` alongside existing bot type/faze context.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves troubleshooting context for empty in-sample name batches.


## Stabilization Update (2026-02-14) - Add Expected Batch Size To OOS Empty-Names Diagnostic
### What changed
- Updated OutOfSample empty-batch skip log in `StartAsuncBotFactoryOutOfSample(...)`.
- Diagnostic now includes `expectedNamesCount` plus existing bot type/faze context.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves troubleshooting symmetry and context consistency between InSample/OOS name batch logs.


## Stabilization Update (2026-02-14) - Include Script Mode In Async Factory Skip Diagnostics
### What changed
- Updated async factory skip logs in `OptimizerExecutor` to include `isScript` flag for context parity with failure logs.
- Applied to:
  - InSample non-positive count skip;
  - InSample empty generated names skip;
  - OutOfSample empty source snapshot skip;
  - OutOfSample empty generated names skip.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves diagnostic consistency and faster triage across script/non-script paths.


## Stabilization Update (2026-02-14) - Add Batch-Size Context To OOS Per-Item Skip Diagnostics
### What changed
- Updated per-item OutOfSample skip logs in `StartAsuncBotFactoryOutOfSample(...)`.
- For null source report / empty source bot name / empty transformed bot name cases, diagnostics now include both:
  - current item index;
  - total snapshot size (`reportsCount`).

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves observability for batched OOS data-quality issues.


## Stabilization Update (2026-02-14) - Add Batch-Size Context To InSample Duplicate-Name Skip Log
### What changed
- Updated InSample duplicate-name skip diagnostic in `StartAsuncBotFactoryInSample(...)`.
- Log now includes:
  - duplicate item index;
  - expected batch size (`expectedNamesCount`).

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves parity and triage context for in-sample batch diagnostics.


## Stabilization Update (2026-02-14) - Add Batch Context To OOS Duplicate-Name Skip Log
### What changed
- Updated OutOfSample duplicate-name skip diagnostic in `StartAsuncBotFactoryOutOfSample(...)`.
- Log now includes:
  - duplicate item index;
  - total OOS snapshot size (`reportsCount`).

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves parity and triage context for OOS duplicate-name skips.


## Stabilization Update (2026-02-14) - Enrich Duplicate-Name Skip Diagnostics With Full Context
### What changed
- Updated duplicate-name skip logs in `OptimizerExecutor` for both InSample and OutOfSample async factory paths.
- Diagnostics now include full context payload:
  - item index and batch size;
  - normalized bot type;
  - normalized faze;
  - `isScript` flag.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves consistency and troubleshooting depth for duplicate-name skip scenarios.



## Stabilization Update (2026-02-14) - Add Full Context To OOS Per-Item Invalid-Entry Diagnostics
### What changed
- Updated OOS per-item skip logs in `StartAsuncBotFactoryOutOfSample(...)` for cases:
  - null source report;
  - empty source bot name;
  - empty transformed bot name.
- Each message now includes full context: `botType`, `faze`, `isScript` (in addition to index and batch size).

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change; improves diagnostic parity and triage depth for invalid OOS source entries.


## Stabilization Update (2026-02-14) - Use Delegate Snapshots For Event Dispatch Stability
### What changed
- Updated event dispatch in `OptimizerExecutor` to use local delegate snapshots before invocation:
  - `SafeInvokeTestingProgress(...)` now snapshots `TestingProgressChangeEvent`.
  - `SendLogMessage(...)` now snapshots `LogMessageEvent`.
- This removes a small race window between null-check and invoke under concurrent subscribe/unsubscribe.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change expected; improves thread-safety robustness of event dispatch paths.


## Stabilization Update (2026-02-14) - Align All Optimizer Event Dispatchers To Delegate Snapshot Pattern
### What changed
- Extended event dispatch stabilization in `OptimizerExecutor` to additional paths:
  - ETA dispatch in optimizer end-event flow now snapshots `TimeToEndChangeEvent`.
  - `SafeInvokePrimeProgress(...)` now snapshots `PrimeProgressChangeEvent`.
  - `SafeInvokeTestReady(...)` now snapshots `TestReadyEvent`.
- All updated paths now consistently use: local snapshot -> null guard -> try/catch invoke.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change expected; improves consistency and resilience under concurrent event subscription changes.


## Stabilization Update (2026-02-14) - Synchronize `_testBotsTime` Access In End-Event ETA Path
### What changed
- Added dedicated lock object `_testBotsTimeSync` in `OptimizerExecutor`.
- Wrapped `_testBotsTime.Clear()` in `Start(...)` with this lock.
- Wrapped `_testBotsTime` add/read ETA aggregation logic in `server_TestingEndEvent(...)` with the same lock.
- ETA value is now computed under lock and published outside lock via local snapshot to avoid holding lock during event callbacks.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change expected; removes race conditions in concurrent end-event ETA aggregation.


## Stabilization Update (2026-02-14) - Atomically Allocate Optimizer Server Numbers
### What changed
- Fixed race-prone `_serverNum` usage in `CreateNewServer(...)`.
- Server number is now reserved under `_serverRemoveLocker` before server factory call:
  - `serverNumber = _serverNum; _serverNum++;`
  - factory uses reserved `serverNumber`.
- `_servers.Add(server)` remains protected by the same lock.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Behavior is preserved; numbering may now contain gaps if creation fails, which is acceptable for unique ID allocation and safer under concurrency.


## Stabilization Update (2026-02-14) - Publish Phase-Start Progress From Locked Counter Snapshots
### What changed
- Removed unsynchronized progress counter reads in phase-start flows:
  - `StartOptimizeFazeInSample(...)`
  - `StartOptimizeFazeOutOfSample(...)`
- After updating `_countAllServersMax`, both progress values are now captured under `_serverRemoveLocker` and then passed to `SafeInvokePrimeProgress(...)`.
- Zero-item OutOfSample path now also reads progress counters from the same locked snapshot pattern.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change expected; removes minor data-race windows in progress reporting under concurrent updates.


## Stabilization Update (2026-02-14) - Make Synchronization Cleanup Atomic With `Interlocked.Exchange`
### What changed
- Updated `DisposeRunSynchronization()` in `OptimizerExecutor` to atomically detach disposable sync objects before disposal:
  - `_phaseCompletion`
  - `_serverSlots`
  - `_stopCts`
- Implemented via `Interlocked.Exchange(ref field, null)` and disposal of detached local snapshot.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change expected; reduces rare race windows where a newly assigned sync object could otherwise be affected by concurrent cleanup.


## Stabilization Update (2026-02-14) - Dispose Previous Phase Latch On Phase Replacement
### What changed
- Added `ReplacePhaseCompletion(int participantsCount)` in `OptimizerExecutor`.
- Phase startup paths now use replacement helper instead of direct assignment:
  - `StartOptimizeFazeInSample(...)`
  - `StartOptimizeFazeOutOfSample(...)`
- Helper atomically swaps `_phaseCompletion` via `Interlocked.Exchange(...)` and safely disposes previous `CountdownEvent`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change expected; improves resource hygiene and prevents phase latch accumulation across long multi-phase runs.


## Stabilization Update (2026-02-14) - Harden Stop Cancellation And Slot Release Paths
### What changed
- Updated `Stop()` in `OptimizerExecutor`:
  - uses local snapshot of `_stopCts`;
  - wraps `Cancel()` in `try/catch` to safely handle concurrent disposal (`ObjectDisposedException`);
  - logs unexpected cancellation errors.
- Updated `SafeReleaseServerSlot()`:
  - uses local snapshot of `_serverSlots` before release;
  - keeps existing defensive exception handling for disposed/full semaphore cases.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No expected behavior change in normal flow; improves resilience during concurrent stop/cleanup timing races.


## Stabilization Update (2026-02-14) - Harden Start-State Reinitialization With Atomic Swap And Dispose
### What changed
- Updated `Start(...)` in `OptimizerExecutor` to atomically reinitialize run-scoped sync state:
  - `_stopCts` now replaced via `Interlocked.Exchange(...)` with disposal of previous token source.
  - `_phaseCompletion` is atomically detached and disposed before new run.
  - `_serverSlots` now replaced via `Interlocked.Exchange(...)` with disposal of previous semaphore.
- Added guarded logging for unexpected cleanup exceptions in start reinitialization.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change expected; improves resource hygiene and reduces stale state leakage across repeated start/stop cycles.


## Stabilization Update (2026-02-14) - Lock Server-State Reset During Start Reinitialization
### What changed
- Updated `Start(...)` in `OptimizerExecutor` to reset server-tracking state under `_serverRemoveLocker`:
  - `_servers`
  - `_countAllServersMax`
  - `_countAllServersEndTest`
  - `_serverNum`
- This aligns reset path with existing lock discipline used by server end-event and server list mutation flows.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change expected; reduces race windows during rapid stop/start cycles with late server callbacks.


## Stabilization Update (2026-02-14) - Lock `_serverNum` Snapshot In InSample Async Factory Naming
### What changed
- Updated `StartAsuncBotFactoryInSample(...)` to read `startServerIndex` under `_serverRemoveLocker`.
- This aligns read access with existing lock-protected `_serverNum` mutation path in `CreateNewServer(...)`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change expected; improves consistency of bot-name base index snapshots under concurrent server lifecycle activity.


## Stabilization Update (2026-02-14) - Remove Premature Prime-Worker State Reset In OOS Early Exit
### What changed
- Updated `StartOptimizeFazeOutOfSample(...)` early-exit branch (unscheduled tail compensation path).
- Removed direct `_primeThreadWorker = null` assignment from this branch.
- Prime worker lifecycle flag now remains reset only in `PrimeThreadWorkerPlace()` `finally`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Expected behavior is safer: avoids exposing false "idle" optimizer state while the prime thread is still unwinding.


## Stabilization Update (2026-02-14) - Guard OutOfSample Phase Against Missing Prior InSample Reports
### What changed
- Added explicit guard in `PrimeThreadWorkerPlace()` before OutOfSample branch execution.
- If no previous phase report exists, OutOfSample phase is skipped with error log instead of relying on index-based access.
- Replaced repeated index-based access (`ReportsToFazes[... - 2]`) with local `inSampleReport` snapshot for clarity and safety.
- `ReportsCount` log now uses null-safe count extraction.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Improves resilience for invalid phase sequencing configuration; prevents avoidable `IndexOutOfRange`-style failures in prime worker flow.


## Stabilization Update (2026-02-14) - Centralize Safe Stop-Token Access
### What changed
- Added helper `GetStopTokenOrNone()` in `OptimizerExecutor`:
  - returns current stop token from `_stopCts`;
  - safely falls back to `CancellationToken.None` when token source is null/disposed.
- Updated stop-token consumers to use the helper:
  - `IsStopRequested`
  - `StartOptimizeFazeInSample(...)` optimizer call
  - `TryAcquireServerSlot()`
  - `WaitCurrentPhaseToComplete()`
  - `CreateNewBot(...)`
  - `TestBot(...)`

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No intended behavior change; reduces chance of `ObjectDisposedException` races during concurrent stop/cleanup timing.


## Stabilization Update (2026-02-14) - Serialize `Start()` Critical Section
### What changed
- Added dedicated `_startSync` lock in `OptimizerExecutor`.
- Wrapped `Start(...)` lifecycle-critical sequence in this lock:
  - active worker check;
  - run-state reinitialization;
  - sync object replacement/disposal;
  - prime worker thread creation and start.
- This prevents concurrent `Start()` calls from interleaving partial state reset/start operations.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change expected for normal single-caller usage; improves correctness under accidental concurrent start requests.


## Stabilization Update (2026-02-14) - Atomic Prime-Worker State Read/Reset
### What changed
- Added `IsPrimeWorkerActive()` helper in `OptimizerExecutor` using `Volatile.Read(ref _primeThreadWorker)`.
- Updated active-worker checks to use helper:
  - `Start(...)`
  - `TestBot(...)`
- Updated worker teardown in `PrimeThreadWorkerPlace()` `finally` to atomic reset:
  - `Interlocked.Exchange(ref _primeThreadWorker, null)`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional behavior change expected; reduces residual data-race risk for prime-worker activity flag under concurrent reads/writes.


## Stabilization Update (2026-02-14) - Use `Volatile.Write` For Prime Worker Publication
### What changed
- Updated `Start(...)` in `OptimizerExecutor` to publish newly created prime worker thread reference via `Volatile.Write(ref _primeThreadWorker, primeWorker)` before `Start()`.
- This aligns publication semantics with existing `Volatile.Read` in `IsPrimeWorkerActive()`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No behavior change expected; improves memory-order consistency for concurrent worker-state observation.


## Stabilization Update (2026-02-14) - Validate And Snapshot Start Input Collections
### What changed
- Updated `Start(...)` in `OptimizerExecutor`:
  - added null guards for `parametersOn` and `parameters` with explicit error logs and early `false` return;
  - switched to list snapshots:
    - `_parametersOn = new List<bool>(parametersOn)`
    - `_parameters = new List<IIStrategyParameter>(parameters)`
- This removes dependency on caller-owned list mutability during active optimization run.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Intentional robustness change: optimizer now rejects null input lists early instead of failing later in worker flow.


## Stabilization Update (2026-02-14) - Validate Start Input Count Consistency
### What changed
- Added input consistency guard in `Start(...)`:
  - `parametersOn.Count` must equal `parameters.Count`.
- On mismatch, optimizer start is rejected early with explicit diagnostic log including both counts.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Intentional robustness change: invalid start configurations are now rejected deterministically before launching worker lifecycle.


## Stabilization Update (2026-02-14) - Validate Master/Faze Configuration Before Start
### What changed
- Added early `Start(...)` guards in `OptimizerExecutor`:
  - `_master` must be initialized;
  - `_master.Fazes` must be non-null and non-empty.
- Invalid configuration now exits before worker thread startup with explicit diagnostics.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Intentional robustness change: prevents late-stage failures caused by empty/missing phase configuration.


## Stabilization Update (2026-02-14) - Clear Stale Pending Evaluations On New Start
### What changed
- Added `CancelPendingEvaluationsForNewRun()` in `OptimizerExecutor`.
- `Start(...)` now invokes this cleanup before new run synchronization objects are used.
- Cleanup iterates `_pendingEvaluationByServer`, removes stale entries, and completes removed tasks as canceled via existing safe helper.
- Added diagnostic log with canceled stale-entry count when any were found.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No expected behavior change for healthy flow; improves run-boundary hygiene by preventing pending-evaluation leakage across restarts.


## Stabilization Update (2026-02-14) - Snapshot Faze Sequence In Prime Worker
### What changed
- Updated `PrimeThreadWorkerPlace()` in `OptimizerExecutor` to create local phase snapshot:
  - `List<OptimizerFaze> fazesSnapshot = new List<OptimizerFaze>(_master.Fazes);`
- Main faze loop now iterates over `fazesSnapshot` and uses snapshot items for phase report assignment and type branching.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No intended behavior change for normal flow; reduces race sensitivity if phase collection is mutated externally while optimization is running.


## Stabilization Update (2026-02-14) - Validate Storage And Bot Context Before Start
### What changed
- Extended early start validation in `OptimizerExecutor.Start(...)`:
  - `_master.Storage` must be initialized;
  - `_master.BotToTest` must be initialized.
- Invalid context now aborts start before worker thread lifecycle begins, with explicit diagnostics.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Intentional robustness change: prevents late create-server/create-bot failures by rejecting incomplete optimizer context at start boundary.


## Stabilization Update (2026-02-14) - Snapshot Strategy Identity Per Prime Worker Run
### What changed
- Updated `PrimeThreadWorkerPlace()` to snapshot run-local strategy identity values:
  - `strategyName = _master.StrategyName`
  - `isScript = _master.IsScript`
- InSample and OutOfSample async factory startup calls now use these run-local values.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No intended behavior change; improves per-run consistency if optimizer strategy metadata is edited externally during active execution.


## Stabilization Update (2026-02-14) - Skip Null Faze Entries In Prime Worker Loop
### What changed
- Added null-entry guard in `PrimeThreadWorkerPlace()` faze iteration:
  - each loop now snapshots `currentFaze`;
  - null entries are skipped with explicit diagnostic log including index.
- InSample/OutOfSample report assignment now uses `currentFaze` local variable.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Intentional robustness change: malformed phase arrays with null entries no longer crash run flow and are handled as skip-with-log.


## Stabilization Update (2026-02-14) - Resolve OOS Source Selection To Latest InSample Report
### What changed
- Updated `PrimeThreadWorkerPlace()` phase execution logic:
  - `StartOptimizeFazeInSample(...)` now uses run snapshot `currentFaze` (instead of direct `_master.Fazes[i]` read).
  - OutOfSample branch now resolves source report via new helper `GetLatestInSampleReport()` (searches backward for last valid InSample faze report), instead of assuming the last report is always InSample.
- Existing skip-with-log behavior is preserved when no prior InSample source is available.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Intentional robustness/logic correctness improvement for phase sequences containing consecutive OutOfSample entries or mixed ordering.


## Stabilization Update (2026-02-14) - Cancel Pending Evaluation Tasks During Run Cleanup
### What changed
- Added shared helper `CancelPendingEvaluations(string messagePrefix)` in `OptimizerExecutor`.
- `DisposeRunSynchronization()` now proactively clears `_pendingEvaluationByServer` and safely cancels remaining pending evaluation tasks before disposing sync objects.
- `Start(...)` was switched to use the same shared helper for stale pending evaluation cleanup (removing duplicate cleanup implementation).

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No expected behavior change for successful flows; improves shutdown hygiene and reduces risk of leaked/never-completing evaluation tasks after abnormal termination.


## Stabilization Update (2026-02-14) - Snapshot Iteration Settings For Max-Test Estimation
### What changed
- Extended run-local snapshotting in `PrimeThreadWorkerPlace()`:
  - `iterationCount = _master.IterationCount`
  - `lastInSample = _master.LastInSample`
- `estimatedMaxTests` computation now uses these snapshot values instead of repeated live reads from `_master`.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No intended behavior change; keeps per-run max-test estimation stable if optimizer settings are modified while a run is active.


## Stabilization Update (2026-02-14) - Clamp Max-Test Estimation Against Integer Overflow
### What changed
- Hardened `estimatedMaxTests` calculation in `PrimeThreadWorkerPlace()`:
  - switched intermediate computation to `long`;
  - guards negative iteration input with `Math.Max(0, iterationCount)`;
  - applies final clamp to `[0, int.MaxValue]`.
- `lastInSample` adjustment now applies only when intermediate estimate is positive.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No functional logic change for normal ranges; improves diagnostic correctness under extreme parameter combinations.


## Stabilization Update (2026-02-14) - Remove Unused InSample Faze Parameter
### What changed
- Simplified `StartOptimizeFazeInSample(...)` signature by removing unused `OptimizerFaze faze` argument.
- Updated call site in `PrimeThreadWorkerPlace()` accordingly.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- No runtime behavior change expected; reduces API noise and lowers risk of misleading future usage.


## Stabilization Update (2026-02-14) - Clamp Negative Bot Count Estimate To Zero
### What changed
- Hardened bot-count estimation handling in `PrimeThreadWorkerPlace()`:
  - raw estimate from `BotCountOneFaze(...)` is normalized with `Math.Max(0, ...)`;
  - when raw value is negative, explicit diagnostic log is emitted.
- Downstream phase/factory scheduling now has guaranteed non-negative base count.

### Files touched
- `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

### Validation
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --configuration Debug`
- Result: Passed 70 / Failed 0

### Risks / notes
- Intentional robustness change: prevents invalid negative estimation values from propagating into phase-size and naming logic.
