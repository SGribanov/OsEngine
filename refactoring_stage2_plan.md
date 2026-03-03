# OsEngine Stage 2 Refactoring Plan

**Date:** 2026-02-15
**Based on:** [refactoring_stage2.md](refactoring_stage2.md) audit report
**Principle:** Each step must compile and pass manual verification before proceeding

---

## 2026-03-03 Plan Refresh (Reliability + Throughput + Low Allocation First)

This section supersedes execution priority of the original phase order.
The original phase descriptions remain as implementation backlog/details.

### Why we are replanning

1. Current execution has strong safety coverage but weak runtime-impact density.
2. Build/test-only gates are insufficient for a "fast + low-allocation" target.
3. Several hot paths still allocate on each cycle (`TradeGrid` query/cancel helpers and optimizer cache cloning paths).

### New primary objective

Deliver measurable runtime improvements first, while preserving behavior and compatibility:
1. No reliability regressions.
2. Lower tail latency in hot loops.
3. Lower allocations/op and lower GC pressure.

### Mandatory KPI gates (new Definition of Done)

Every runtime-affecting step must ship with baseline and after-metrics for a fixed scenario set.

1. Correctness gate:
   - existing functional tests pass;
   - new/changed behavior has focused regression tests.
2. Performance gate:
   - no latency regression in target scenarios;
   - target path median or p95 latency improves by >= 10%, or change is rejected.
3. Allocation gate:
   - target path allocated bytes/op improves by >= 20%, or change is rejected;
   - Gen0 collections per scenario do not increase.
4. Reliability gate:
   - negative-path behavior remains deterministic (no modal/UI side effects in tests, no silent failures).

### Required baseline harness before next runtime changes

Create and check in deterministic perf harnesses (repeatable on developer machine):
1. `TradeGrid` process-loop scenario with fixed candles/orders/positions workload.
2. Optimizer indicator-cache hit-path scenario.
3. Settings save/load scenario for high-frequency write/read components.

Minimum artifacts to store per run:
1. elapsed time summary (median, p95 when applicable),
2. allocated bytes/op,
3. GC collections by generation for scenario run,
4. scenario checksum/assertion proving equivalent functional result.

### Re-prioritized execution waves

#### Wave P0 (Immediate): Measurement and guardrails

**Goal:** Make performance/allocation regressions visible before further refactors.

**Actions:**
1. Add perf regression test project area (`project/OsEngine.Tests/Performance/*`) for deterministic scenario runners.
2. Add runner script (`tools/run-stage2-perf.ps1`) that emits comparable metrics.
3. Add acceptance thresholds file (`tools/perf-thresholds.json`) and fail-fast check mode.

**Exit criteria:**
1. Baseline snapshot committed.
2. CI/local command can reproduce metric report on demand.

#### Wave P1: TradeGrid hot-path allocation reduction

**Goal:** Remove avoidable allocations in `TradeGrid` runtime cycle.

**Target files (first pass):**
- `project/OsEngine/OsTrader/Grids/TradeGrid.cs`

**Actions:**
1. Replace repeated "build new list and return" patterns in high-frequency query methods with reusable buffers (`Fill...` pattern).
2. Eliminate duplicate intermediate collections in order-cancel/selection logic.
3. Keep external behavior and ordering stable; isolate refactor behind deterministic tests.

**Exit criteria:**
1. `TradeGrid` scenario allocations/op reduced by at least 30% from P0 baseline.
2. No functional regressions in existing grid tests.

#### Wave P2: Optimizer cache overhead reduction

**Goal:** Keep cache hit-rate benefits while cutting clone/boxing/string overhead.

**Target files (first pass):**
- `project/OsEngine/OsOptimizer/OptEntity/IndicatorCache.cs`
- `project/OsEngine/Indicators/Aindicator.cs`
- `project/OsEngine/OsOptimizer/OptEntity/OptimizerMethodCache.cs`

**Actions:**
1. Remove unnecessary key/string materialization in cache keys where safe.
2. Reduce clone depth/volume on hit path while preserving isolation guarantees.
3. Revisit bounded eviction policy to avoid full-cache clear spikes when capacity is reached.

**Exit criteria:**
1. Cache hit-path allocations/op reduced by at least 40% from P0 baseline.
2. Optimizer run-time improves measurably with no result drift.

#### Wave P3: Reliability completion for persistence write paths

**Goal:** Finish high-risk atomic write migration with proof by failure-injection scenarios.

**Actions:**
1. Complete migration of remaining direct write sites (`StreamWriter`, `File.WriteAllText`, `File.AppendAllText`) in critical settings/state paths.
2. Add failure-injection tests for interrupted write and recovery behavior.
3. Keep backward-compatible read paths unchanged.

**Exit criteria:**
1. No known critical settings write path bypasses `SafeFileWriter`.
2. Crash/interruption scenarios keep previous-or-new valid file state.

#### Wave P4: Security/perf balance and cleanup

**Goal:** Close remaining high-impact security/transport risks without throughput regressions.

**Actions:**
1. Complete credential-at-rest hardening (`dpapi:` protocol marker).
2. Complete OKX shared `HttpClient` refactor.
3. Retain lock/nullable/dependency cleanups only when they do not delay P1/P2 outcomes.

**Exit criteria:**
1. Security steps merged with regression coverage.
2. Perf/allocation gates stay green after integration.

### Scope demotion (explicit)

The following items are now lower priority unless they unblock P0-P4:
1. Broad nullable sweep in low-risk/test-only areas without runtime impact.
2. UI modernization and non-critical stylistic refactors.
3. Optional format migrations not tied to reliability/performance bottlenecks.

### Governance updates

1. Each increment note must include: `Runtime impact: yes/no`.
2. If `Runtime impact: yes`, increment note must include baseline vs after metrics.
3. Increments with no measurable gain in target KPI should be split/reworked before merge.

---

## Plan Assessment and Required Additions

### What is strong already

1. Good phase decomposition by risk/impact (security, persistence, performance, quality).
2. Most steps have concrete files and a build verification rule.
3. Optional/high-risk work is isolated into later phases.

### Gaps to close before execution

1. No global entry/exit criteria per phase.
2. No measurable baseline for performance/security changes.
3. A few implementation details are unsafe as written.
4. No explicit rollback/provenance process for dependency and serialization migration.

### Critical corrections (must apply)

1. **Step 2.1 (Atomic writes):** prefer temp file in same directory + `Flush(true)` + `File.Replace(temp, target, backup)`; keep `.bak` for recovery.
2. **Step 3.1 (Indicator cache key):** `(IndicatorTypeName, ParameterHash, CandleCount)` is collision-prone; include security/timeframe/time range (or stable source id + range hash).
3. **Step 1.1 (Credential encryption):** add explicit marker prefix like `dpapi:`; avoid plain-text detection only via decrypt failure.
4. **Step 4.1 (Lock migration):** do not auto-migrate locks used with `Monitor.Wait/Pulse/TryEnter` or shared externally.

### Cross-phase Definition of Done

For each step:

1. `dotnet build project/OsEngine.sln` returns 0 errors.
2. Warning count does not increase for touched projects.
3. Backward compatibility check for persisted configs/data passes.
4. At least one negative-path test is executed and documented.
5. Completion note is recorded (date, build result, manual verification outcome).

### Baseline before Phase 0 (required)

1. Record current warning count and build duration.
2. Record optimizer baseline (fixed dataset/iterations): runtime and peak memory.
3. Save sample settings/credentials files for migration testing.
4. Freeze a smoke-test checklist for critical connectors (auth, candles, order place/cancel where applicable).

---

## Phase 0: Trivial Fixes (1-2 days)

### Step 0.1: Fix Bot Compilation Cache

**Problem:** `BotFactory._compiledBotTypesCache` is read but never written to after Roslyn compilation.

**Files:**
- `project/OsEngine/Robots/BotFactory.cs`

**Actions:**
1. In `CompileAndInstantiateBotScript()`, after successful compilation, add the compiled `Type` to `_compiledBotTypesCache` under `_compiledTypesCacheLock`
2. Mirror the pattern from `IndicatorsFactory.cs` lines 335-345

**Verification:** `dotnet build project/OsEngine.sln` succeeds. Run optimizer with a custom script bot - second iteration should skip Roslyn compilation (verify via debug logging).

---

### Step 0.2: Clean Up App.config

**Problem:** `App.config` references `.NETFramework,Version=v4.8` and contains obsolete assembly binding redirects.

**Files:**
- `project/OsEngine/App.config`

**Actions:**
1. Remove `<startup>` section (irrelevant for .NET 10)
2. Remove all `<assemblyBinding>` redirects (not used in .NET 10)
3. Keep `<system.diagnostics>` section if trace logging is needed, otherwise remove entirely
4. If file becomes empty, delete it

**Verification:** `dotnet build` succeeds. Application launches normally.

---

### Step 0.3: Add Logging to Silent Catches

**Problem:** Multiple catch blocks swallow exceptions silently.

**Files (6 files, ~12 catch blocks):**
- `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs` (line 665)
- `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorage.cs` (lines 112, 130)
- `project/OsEngine/Entity/HorizontalVolume.cs` (lines 45, 71)
- `project/OsEngine/Entity/NonTradePeriods.cs` (lines 54, 88)
- `project/OsEngine/OsOptimizer/OptEntity/OptimizerReportSerializer.cs` (line 42)

**Actions:**
1. Replace `catch { }` / `catch (Exception) { }` with `catch (Exception ex) { SendLogMessage(ex.ToString(), LogMessageType.Error); }` where `SendLogMessage` is available
2. Where no logging infrastructure exists, use `System.Diagnostics.Trace.TraceWarning(ex.ToString())`
3. Keep backward-compatibility behavior (don't rethrow) - just add visibility

**Verification:** `dotnet build` succeeds. Intentionally corrupt a settings file, confirm error appears in logs instead of silent failure.

---

## Phase 1: Security Hardening (1-2 weeks)

### Step 1.1: Encrypt API Keys at Rest

**Problem:** Exchange API keys stored as plain text in `Engine/*.txt`.

**Files:**
- `project/OsEngine/Market/Servers/Entity/ServerParameter.cs`

**Actions:**
1. Add a helper class `CredentialProtector` using `System.Security.Cryptography.ProtectedData` (DPAPI):
   ```csharp
   public static class CredentialProtector
   {
       public static string Protect(string plainText) { ... }
       public static string Unprotect(string cipherBase64) { ... }
   }
   ```
2. In `ServerParameterPassword.GetStringToSave()`: encrypt `Value` before writing
3. In `ServerParameterPassword.LoadFromStr()`: decrypt after reading
4. Handle migration: if decryption fails, assume legacy plain-text value and re-save encrypted
5. Place `CredentialProtector` in `Entity/` namespace
6. Prefix encrypted values as `dpapi:{base64}` to make format detection explicit
7. Use `DataProtectionScope.CurrentUser` and document user/machine binding in comments and release notes

**Verification:** `dotnet build` succeeds. Connect to an exchange, verify credentials work. Check settings file contains encrypted (Base64) value, not plain text. Re-launch app, verify credentials load correctly.

**Rollback:** If decryption fails on load, fall back to plain-text read (migration path).

---

### Step 1.2: Add SSL Bypass Warning

**Problem:** `IgnoreSslErrors` can disable certificate validation without any warning.

**Files:**
- `project/OsEngine/Entity/WebSocketOsEngine.cs`

**Actions:**
1. Add `[Obsolete("SSL validation bypass is a security risk. Use only for debugging.")]` to the property
2. Log a warning when `IgnoreSslErrors` is set to `true`
3. Consider making it `internal` so only the assembly can use it

**Verification:** `dotnet build` succeeds. Any code setting `IgnoreSslErrors = true` shows compiler warning.

---

### Step 1.3: Refactor OKX HttpClient

**Problem:** `OkxServer.cs` creates new `HttpClient` per request (5 locations) due to `HttpInterceptor` constructor design.

**Files:**
- `project/OsEngine/Market/Servers/OKX/Entity/HttpInterceptor.cs`
- `project/OsEngine/Market/Servers/OKX/OkxServer.cs`

**Actions:**
1. Refactor `HttpInterceptor` to store credentials as fields (set once) and accept request body via `HttpRequestMessage.Options` dictionary
2. In `SendAsync()`, read the body/signing data from request options instead of constructor
3. Create a single `HttpClient` field in `OkxServer` initialized with the refactored handler
4. Replace all 5 `new HttpClient(new HttpInterceptor(...))` calls with the shared instance
5. Set `PooledConnectionLifetime = TimeSpan.FromMinutes(5)` via `SocketsHttpHandler`

**Verification:** `dotnet build` succeeds. Test OKX paper trading: place order, cancel order, get positions, get candles. Verify no socket exhaustion under rapid API calls.

---

## Phase 2: Data Persistence Robustness (2-3 weeks)

### Step 2.1: Implement Atomic File Writes

**Problem:** Crash during `StreamWriter` = corrupted or empty settings file.

**Files:**
- Create `project/OsEngine/Entity/SafeFileWriter.cs`
- Modify callers incrementally

**Actions:**
1. Create `SafeFileWriter` utility:
   ```csharp
   public static class SafeFileWriter
   {
       public static void WriteAllLines(string path, IEnumerable<string> lines)
       {
           string tempPath = path + ".tmp";
           string backupPath = path + ".bak";
           // write temp in the same directory, force flush, then atomically replace
           // prefer File.Replace(tempPath, path, backupPath, ignoreMetadataErrors: true)
       }
   }
   ```
2. Replace `StreamWriter` usage in highest-risk files first:
   - `OptimizerSettings.cs` (lines 517-557)
   - `BotPanel.cs` (settings save)
   - `AServer.cs` (leverage save, line 5293)
3. Gradually migrate remaining 118+ files

**Verification:** `dotnet build` succeeds. Kill application during save operation - settings file should not be corrupted.
Recovery check: after forced termination during save, resulting file must be either previous valid version or new valid version (no empty/truncated file).

---

### Step 2.2: Standardize CultureInfo.InvariantCulture

**Problem:** Decimal parsing may break on systems with comma decimal separator.

**Files:** All files using `decimal.Parse()`, `double.Parse()`, `Convert.ToDecimal()` without `CultureInfo`.

**Actions:**
1. Search for `decimal.Parse(`, `double.Parse(`, `float.Parse(`, `Convert.ToDecimal(` without `CultureInfo`
2. Add `CultureInfo.InvariantCulture` to all persistence-related parsing
3. Add `CultureInfo.InvariantCulture` to all `ToString()` calls used in persistence
4. Focus on Entity classes first: `Order.cs`, `Position.cs`, `Trade.cs`, `MyTrade.cs`, `Candle` serialization

**Verification:** `dotnet build` succeeds. Change Windows regional settings to Russian (comma decimal separator). Save and load settings - values must round-trip correctly.

---

### Step 2.3: Migrate Settings to System.Text.Json (Optional)

**Problem:** Custom delimiter-based serialization is fragile and untyped.

**Files:** New settings subsystem, gradual migration.

**Actions:**
1. Create `SettingsManager<T>` generic class using `System.Text.Json`:
   ```csharp
   public static class SettingsManager
   {
       public static void Save<T>(string path, T settings) { ... }
       public static T Load<T>(string path, T defaultValue = default) { ... }
   }
   ```
2. Apply to new features first
3. Migrate existing settings incrementally with backward-compatible readers
4. Keep old `Load()` as fallback for legacy files

**Verification:** `dotnet build` succeeds. Settings files are valid JSON. Legacy settings files still load correctly.

---

## Phase 3: Optimizer Performance (2-4 weeks)

### Step 3.1: Implement Indicator Result Cache

**Problem:** Same indicator with same parameters recalculated for every optimizer bot.

**Files:**
- Create `project/OsEngine/OsOptimizer/OptEntity/IndicatorCache.cs`
- Modify `project/OsEngine/Indicators/Aindicator.cs`
- Modify `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`

**Actions:**
1. Create `IndicatorCache` class:
   ```csharp
   public class IndicatorCache
   {
       // Key includes source identity/range to avoid collisions
       // Example: (IndicatorTypeName, ParameterHash, Security, TimeFrame, FirstTime, LastTime, CandleCount)
       // Value: DataSeries values (List<decimal>[])
       private ConcurrentDictionary<string, List<decimal>[]> _cache;

       public bool TryGet(string key, out List<decimal>[] values) { ... }
       public void Set(string key, List<decimal>[] values) { ... }
       public void Clear() { ... }
   }
   ```
2. In `OptimizerExecutor`, create cache at optimization start, pass to bot configurator
3. In `Aindicator.ProcessAll()`, check cache before computing:
   - If `StartProgram == StartProgram.IsOsOptimizer && cache != null`: check cache
   - If hit: inject cached values into `DataSeries`
   - If miss: compute normally, store result
4. Clear cache when optimization finishes
5. **Guard:** Only apply in optimizer mode (`StartProgram.IsOsOptimizer`)
6. Add cache size guard (max entries or memory threshold) with deterministic eviction/clear policy

**Verification:** `dotnet build` succeeds. Run optimization with 100+ iterations:
- Results must be identical with and without cache
- Wall-clock time should decrease measurably
- Memory usage should increase by cache size only

---

### Step 3.2: Share Candle Data References

**Problem:** Multiple bot instances may copy candle lists unnecessarily.

**Files:**
- `project/OsEngine/Market/Servers/Optimizer/OptimizerServer.cs`

**Actions:**
1. Verify that `OptimizerServer` passes the same `List<Candle>` reference to all bots (not copies)
2. If copies are made, change to shared read-only reference
3. Ensure bots don't mutate the candle list (make defensive if needed)

**Verification:** `dotnet build` succeeds. Memory profiler shows single candle list allocation per security in optimization.

---

## Phase 4: Code Quality (2-3 weeks)

### Step 4.1: Replace Legacy Lock Objects with Lock Type

**Problem:** Inconsistent locking - some files use `Lock` (.NET 9+), others use `object`.

**Files:** All files with `private readonly object _lock = new object();` pattern.

**Actions:**
1. Search for `new object()` used as lock targets
2. Replace with `private readonly Lock _lock = new();`
3. Ensure `using System.Threading;` is present
4. Skip fields used with `Monitor.Wait/Pulse/TryEnter` or any externally shared synchronization contract
5. **Do not change** lock semantics or scope

**Verification:** `dotnet build` succeeds. Multi-threaded scenarios (optimizer, live trading) work correctly.

---

### Step 4.2: Enable Nullable Warnings Incrementally

**Problem:** `Nullable` is `disable` - no null-safety analysis.

**Files:**
- `project/OsEngine/OsEngine.csproj`
- Individual `.cs` files

**Actions:**
1. Keep `<Nullable>disable</Nullable>` in csproj (global)
2. Add `#nullable enable` to new files and recently-refactored files:
   - `OsOptimizer/OptEntity/*.cs` (already clean)
   - `Entity/` core classes
3. Fix warnings file by file
4. Eventually flip global setting to `warnings` when most files are clean

**Verification:** `dotnet build` succeeds with zero new warnings in annotated files.

---

### Step 4.3: Migrate Legacy DLLs to NuGet Where Possible

**Problem:** 12 DLLs in bin/Debug with no version tracking.

**Files:**
- `project/OsEngine/OsEngine.csproj` (`<Reference>` items)

**Actions:**
1. Check NuGet for replacements:
   - `RestSharp.dll` -> `RestSharp` NuGet package (if version compatible)
   - `LiteDB.dll` -> `LiteDB` NuGet package v5.0.19
   - `Jayrock.Json.dll` -> evaluate if still needed (Newtonsoft.Json covers all use cases)
2. For DLLs without NuGet equivalent (cgate, MtApi5, QuikSharp, FinamApi, TInvestApi), document version and provenance in a `DEPENDENCIES.md`
3. Replace `<Reference>` with `<PackageReference>` for each migrated DLL
4. Remove the old `.dll` from `bin/Debug/` after migration

**Verification:** `dotnet build` succeeds. All exchange connectors that use migrated DLLs still function.

---

## Phase 5: UI Modernization (4-8 weeks, optional)

### Step 5.1: Introduce CommunityToolkit.Mvvm

**Problem:** No MVVM infrastructure - all logic in code-behind.

**Files:**
- `project/OsEngine/OsEngine.csproj` (add NuGet reference)
- New ViewModel classes

**Actions:**
1. Add `<PackageReference Include="CommunityToolkit.Mvvm" Version="8.*" />`
2. Create first ViewModel for a simple existing window (e.g., `OptimizerBotParametersSimpleUi`)
3. Document the MVVM pattern for the project in CLAUDE.md
4. Apply to new features going forward

**Verification:** `dotnet build` succeeds. Refactored window works identically to before.

---

### Step 5.2: Replace WinForms Charts (Exploratory)

**Problem:** `System.Windows.Forms.DataVisualization` requires `WindowsFormsHost`, blocking cross-platform.

**Files:**
- `project/OsEngine/Charts/CandleChart/WinFormsChartPainter.cs`
- `project/OsEngine/Charts/CandleChart/CandleChartUi.xaml`
- `project/OsEngine/OsTrader/Panels/BotPanelChartUI.xaml`

**Actions:**
1. Evaluate OxyPlot.Wpf, ScottPlot.WPF, and LiveCharts2 for candlestick chart support
2. Create proof-of-concept with one chart library
3. Implement `IChartPainter` interface to abstract chart rendering
4. Swap `WinFormsChartPainter` with new implementation behind the interface
5. Remove `WindowsFormsHost` from XAML files one by one

**Verification:** `dotnet build` succeeds. Charts render correctly with new library. Performance is acceptable for 10000+ candles.

---

## Summary Timeline

| Phase | Scope | Duration | Risk |
|-------|-------|----------|------|
| **Phase 0** | Trivial fixes | 1-2 days | Minimal |
| **Phase 1** | Security hardening | 1-2 weeks | Low |
| **Phase 2** | Data persistence | 2-3 weeks | Medium |
| **Phase 3** | Optimizer performance | 2-4 weeks | Medium |
| **Phase 4** | Code quality | 2-3 weeks | Low |
| **Phase 5** | UI modernization | 4-8 weeks | High |

**Total estimated scope:** 12-22 weeks for all phases.
**Recommended approach:** Phases 0-2 first (highest impact, lowest risk). Phase 3-4 in parallel. Phase 5 when resources allow.

---

## Build Verification Command

After every step:
```bash
dotnet build project/OsEngine.sln
```

Expected: 0 errors. Warning count should not increase.

---

## Execution Governance Addendum

1. Use dedicated branch per phase (`refactor/stage2-phaseX`) to simplify rollback.
2. Put high-risk behavior behind flags where practical (`UseSafeFileWriter`, `UseIndicatorCache`).
3. For each finished step, record date, build result, verification evidence, and regressions found/fixed.
4. Re-estimate timeline after each phase using actual throughput.

*Plan generated from automated audit. Adjust scope and ordering based on team capacity and business priorities.*
