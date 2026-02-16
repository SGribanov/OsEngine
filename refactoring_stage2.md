# OsEngine Stage 2 Audit Report

**Date:** 2026-02-15
**Scope:** Comprehensive code audit after .NET 10 / C# 14 migration
**Approach:** Research-only; no code changes applied

---

## Table of Contents

1. [Technology Assessment](#1-technology-assessment)
2. [Data Persistence](#2-data-persistence)
3. [Security](#3-security)
4. [Error Handling](#4-error-handling)
5. [Cross-Platform Readiness](#5-cross-platform-readiness)
6. [Optimizer Caching](#6-optimizer-caching)
7. [UI Modernization](#7-ui-modernization)
8. [Weak Spots Summary](#8-weak-spots-summary)

---

## 1. Technology Assessment

### 1.1 Current Stack

| Property | Value |
|----------|-------|
| Target Framework | `net10.0-windows` |
| LangVersion | 14 (C# 14) |
| Nullable | `disable` |
| Platform | x64 only |
| UI | WPF + WinForms interop |
| Unsafe blocks | Enabled (`AllowUnsafeBlocks`) |

### 1.2 NuGet Dependencies

| Package | Version | Notes |
|---------|---------|-------|
| Newtonsoft.Json | 13.0.4 | Primary JSON serializer |
| Google.Protobuf | 3.33.5 | gRPC support (T-Invest) |
| Grpc.Net.Client | 2.76.0 | gRPC transport |
| Microsoft.CodeAnalysis.CSharp | 5.0.0 | Roslyn runtime compilation |
| WTelegramClient | 4.4.1 | Telegram integration |
| WinForms.DataVisualization | 1.10.0 | Charts (WinForms) |
| System.ServiceModel.Duplex | 6.0.0 | Max available version |
| System.ServiceModel.Security | 6.0.0 | Max available version |
| System.ServiceModel.Primitives | 10.0.652802 | .NET 10 version |

### 1.3 Legacy DLL References (HintPath to bin/Debug)

12 pre-compiled DLLs referenced via `<Reference>` with HintPath:

| DLL | Purpose |
|-----|---------|
| `cgate_net64.dll` | Moscow Exchange P2 gateway |
| `QuikSharp.dll` | QUIK terminal connector |
| `MtApi5.dll` / `MtClient.dll` | MetaTrader 5 connector |
| `LiteDB.dll` | Embedded NoSQL database (v5.0.19) |
| `RestSharp.dll` | HTTP client library |
| `FinamApi.dll` | Finam broker API |
| `OpenFAST.dll` | FIX Adapted for STreaming |
| `BytesRoad.Net.Ftp.dll` / `BytesRoad.Net.Sockets.dll` | FTP/socket communication |
| `Jayrock.Json.dll` | Alternative JSON parser |
| `TInvestApi.dll` | T-Invest (Tinkoff) generated gRPC stubs |

**Risk:** These DLLs have no source in the repo, no NuGet provenance, no version tracking. If any DLL is corrupted or outdated, there is no rebuild path.

### 1.4 Unused .NET 10 / C# 14 Features

The migration to .NET 10 and C# 14 is complete at the project level, but the codebase does not yet leverage:

- **Extension members** (C# 14) - could replace static helper classes
- **`field` keyword** (C# 14) - simplify backing-field properties
- **Null-conditional assignment** (`??=` improvements) - `Nullable` is currently `disable`
- **Lambda modifiers** - cleaner delegate expressions
- **NativeAOT** (.NET 10) - startup time optimization for CLI tools
- **`dotnet package update --vulnerable`** (.NET 10) - automated vulnerability scanning
- **Frozen collections** (.NET 8+) - for read-only dictionaries (e.g., bot/indicator registries)
- **`Lock` type** (.NET 9+) - already used in some places (BotFactory), but inconsistently; many files still use `object` locks

### 1.5 App.config

`App.config` still references `.NETFramework,Version=v4.8` and contains assembly binding redirects for `System.Buffers`, `Newtonsoft.Json` (v12), `System.Memory`, etc. This file is a **leftover from the Framework era** and should be cleaned up or removed, as .NET 10 does not use these binding redirects.

---

## 2. Data Persistence

### 2.1 Current Architecture

All application settings are persisted as **plain-text files** in the `Engine/` directory using `StreamWriter`/`StreamReader`. The codebase has **121+ files** using this pattern.

**Typical pattern:**
```csharp
// Save
using (StreamWriter writer = new StreamWriter(@"Engine\" + name + ".txt", false))
{
    writer.WriteLine(field1);
    writer.WriteLine(field2);
}

// Load
using (StreamReader reader = new StreamReader(@"Engine\" + name + ".txt"))
{
    field1 = reader.ReadLine();
    field2 = reader.ReadLine();
}
```

### 2.2 Delimiter-Based Serialization

More complex objects use custom delimiters with no formal schema:

| Delimiter | Usage | Example Files |
|-----------|-------|---------------|
| `@` | Primary field separator | `OptimizerReportSerializer.cs` (line 53), `Position.cs`, `Order.cs` |
| `&` | Sub-field separator | `OptimizerReportSerializer.cs` (line 70, 80) |
| `#` | Tertiary separator | `MarketDepth.cs`, `BotPanel.cs` |
| `^` | Parameter separator | `ServerParameter.cs` (line 298) |

**53 files** use `Split('@')`, `Split('&')`, or `Split('#')` for deserialization.

### 2.3 Versioned Format (OptimizerReportSerializer)

`OptimizerReportSerializer.cs` (lines 17-46) implements a `V2|` prefix for forward compatibility:
- `Serialize()` prepends `"V2|"` to the legacy body
- `Deserialize()` detects prefix and routes to V2 or legacy parser
- Legacy parser at line 90: `string[] str = saveStr.Split('@');`

### 2.4 JSON Usage (Inconsistent)

`AServer.cs` uses `Newtonsoft.Json` for leverage data (lines 5208-5299):
- Load: `JsonConvert.DeserializeObject<Dictionary<string, ClassLeverageData>>(json)` (line 5223)
- Save: `JsonConvert.SerializeObject(_listLeverageData, Formatting.Indented)` (line 5292)
- Files stored in `Engine/ServerDopSettings/*.json`

54 files use `JsonConvert` across exchange connectors, but primarily for REST API payloads, not local persistence.

### 2.5 Problems

| Problem | Impact | Severity |
|---------|--------|----------|
| **No transactional writes** | Crash during save = corrupted/empty file | HIGH |
| **No schema validation** | `Split('@')` can throw `IndexOutOfRangeException` on corrupted data | HIGH |
| **No compression** | Large candle/trade history files consume excessive disk space | MEDIUM |
| **Mixed formats** | Text files + JSON in different subsystems | LOW |
| **No migration system** | Adding new fields requires backward-compat hacks | MEDIUM |
| **Culture-dependent parsing** | `decimal.Parse()` without `CultureInfo.InvariantCulture` in some places | HIGH |

### 2.6 Alternatives Analysis

| Format | Read Speed | Write Speed | Size | Schema | Ecosystem |
|--------|-----------|------------|------|--------|-----------|
| Current (TXT) | Slow | Slow | Large | None | None |
| **MessagePack** | 10x JSON | 10x JSON | ~50% JSON | Contractless option | NuGet, mature |
| **MemoryPack** | 3-50x MsgPack | 3-50x MsgPack | Smallest | C# codegen | NuGet, .NET 7+ |
| **Parquet** | Column-optimal | Good | Very compact | Schema-embedded | NuGet, analytics |
| **System.Text.Json** | ~2x Newtonsoft | ~2x Newtonsoft | Same as JSON | Built-in .NET | Built-in |

**Recommendation:** Migrate settings to `System.Text.Json` (built-in, no extra dependency). For large datasets (candle history), consider MemoryPack or Parquet for columnar access patterns.

---

## 3. Security

### 3.1 Credential Storage (CRITICAL)

**API keys are stored as plain text** in `Engine/` settings files.

`ServerParameter.cs` (lines 295-298):
```csharp
public string GetStringToSave()
{
    return Type + "^" + Name + "^" + Value;  // Plain text
}
```

Example file content (`Engine/BybitParams.txt`):
```
String^Public key^2dUPcxm2tOrOUWF3dR
Password^Secret key^3Xkju5snIGxSTyniKNq5WRxMiBDO5Kas53Da
```

**21+ exchange connectors** use `CreateParameterPassword()` which stores credentials via this mechanism. Any process with file system access can harvest all exchange API keys.

**Recommended fix:** Use Windows DPAPI (`ProtectedData.Protect()`) for at-rest encryption, or integrate with Windows Credential Manager.

### 3.2 HttpClient Management (MEDIUM)

**Per-request HttpClient creation** causes socket exhaustion risk:

`OkxServer.cs` creates new `HttpClient` instances per API call (5 locations: lines 1338, 3173, 3237, 3305, 3860):
```csharp
using HttpClient client = new HttpClient(
    new HttpInterceptor(_publicKey, _secretKey, _password, bodyStr, _demoMode, _myProxy));
```

The `HttpInterceptor` is a `DelegatingHandler` that signs each request, but because the signing data is passed via constructor, a new handler (and thus new `HttpClient`) must be created per request. This is an **architectural issue** - the signing should be done in `SendAsync()` using request-specific data, not constructor arguments.

**4 connectors use correct shared-instance pattern:** Deribit, Finam, TraderNet, Polygon.

**Recommended fix:** Refactor `HttpInterceptor` to accept signing data per-request (e.g., via `HttpRequestMessage.Options`), enabling a single shared `HttpClient` instance. Alternatively, use `IHttpClientFactory`.

### 3.3 SSL/TLS Certificate Bypass (HIGH)

`WebSocketOsEngine.cs` (lines 52-60):
```csharp
public bool IgnoreSslErrors { get; set; } = false;

// When true:
_client.Options.RemoteCertificateValidationCallback =
    (sender, cert, chain, errors) => true;  // Accepts ALL certificates
```

Default is `false`, but the property is public and settable. Any connector can enable it. No logging or warning when SSL validation is disabled.

### 3.4 MOEX Basic Auth (HIGH)

`MoexAlgopackAuth.cs` (lines 37-41) sends credentials via HTTP Basic authentication (Base64-encoded, not encrypted). If the connection is not HTTPS, credentials are exposed in plain text.

### 3.5 OKX Credential Headers (MEDIUM)

`HttpInterceptor.cs` (lines 49-66) adds API key and passphrase as plain-text HTTP headers:
```csharp
request.Headers.Add("OK-ACCESS-KEY", this._apiKey);
request.Headers.Add("OK-ACCESS-PASSPHRASE", this._passPhrase);
```

HMAC-SHA256 signing is properly implemented, but passphrase transmission relies entirely on HTTPS.

---

## 4. Error Handling

### 4.1 Silent Catches

Multiple subsystems swallow exceptions without logging:

| File | Location | Pattern |
|------|----------|---------|
| `OptimizerSettings.cs` | Lines 665-668 | `catch { // ignore - backward compatibility }` |
| `OptimizerDataStorage.cs` | Lines 112-115, 130-133 | `catch (Exception) { // ignored }` |
| `HorizontalVolume.cs` | Lines 45-49, 71-75 | `catch (Exception) { // send to log }` (but doesn't actually log) |
| `NonTradePeriods.cs` | Lines 54-57, 88-91 | `catch { // ignore }` |
| `OptimizerReportSerializer.cs` | Lines 42-45 | `catch { // keep backward-compatible behavior }` |

### 4.2 Inconsistent Error Handling Depth

- **Recently refactored optimizer code** has good error handling with `SendLogMessage()` and proper exception typing
- **Exchange connectors** have varying quality - some log everything, others silently retry
- **Entity classes** (`Order.cs`, `Position.cs`) have minimal error handling in serialization/deserialization

### 4.3 Recommendations

1. **Replace silent catches** with structured logging via `SendLogMessage()`
2. **Add file I/O retry logic** for transient errors (file locked by antivirus, etc.)
3. **Implement global exception handler** for unhandled exceptions in background threads
4. **Use result types** or explicit error returns instead of swallowing exceptions

---

## 5. Cross-Platform Readiness

### 5.1 Current State: NOT CROSS-PLATFORM

| Barrier | Count | Severity |
|---------|-------|----------|
| WPF Framework | Entire UI (152 XAML files) | CRITICAL |
| WindowsFormsHost | Used in charts, grids, panels | CRITICAL |
| WinForms.DataVisualization | Charting library | CRITICAL |
| Native P/Invoke DLLs | `mtesrl64.dll`, `txmlconnector64.dll` | CRITICAL |
| `RuntimeInformation.IsOSPlatform` checks | **0 occurrences** | CRITICAL |
| MVVM / UI abstraction | **None** (0 INotifyPropertyChanged, 0 ICommand) | HIGH |
| `Dispatcher.Invoke` calls | 148 across 49 files | HIGH |

### 5.2 Native Dependencies

**AstsBridgeWrapper.cs:** 20+ `DllImport` declarations to `mtesrl64.dll`, 11 `unsafe` methods (lines 83-634). Required for Moscow Exchange ASTS bridge - Windows-only.

**TransaqServer.cs:** 12 `DllImport` declarations to `txmlconnector64.dll` / `TXmlConnector64.dll`. Windows-only Transaq terminal connector.

**QuikSharp, MtApi5:** Pre-compiled Windows DLLs for QUIK and MetaTrader integration.

### 5.3 Avalonia Migration Path

[Avalonia UI](https://avaloniaui.net/) is the most viable WPF replacement for cross-platform:

| Aspect | WPF (Current) | Avalonia |
|--------|---------------|----------|
| Platform | Windows only | Windows, Linux, macOS, WebAssembly |
| XAML | WPF XAML | Avalonia XAML (similar syntax) |
| Code-behind | Supported | Supported |
| MVVM | Optional | First-class with ReactiveUI |
| Charts | WinForms.DataVisualization | OxyPlot.Avalonia, ScottPlot.Avalonia, LiveChartsCore |
| Maturity | Production | Production (v11+) |

**Migration effort:** Very large. 152 XAML files, 148 Dispatcher.Invoke calls, all WinForms chart code must be replaced.

### 5.4 Recommended Approach

1. **Phase 1:** Extract business logic from code-behind into view-independent classes
2. **Phase 2:** Introduce MVVM for new features (data binding, commands)
3. **Phase 3:** Replace WinForms charting with cross-platform alternative (OxyPlot/ScottPlot)
4. **Phase 4:** Migrate XAML from WPF to Avalonia incrementally
5. **Phase 5:** Wrap native DLLs behind platform abstraction (conditional loading)

---

## 6. Optimizer Caching

### 6.1 Current Flow (No Caching)

```
For each parameter combination:
  1. Create new BotPanel instance (BotConfigurator.CreateAndConfigureBot)
  2. Create new indicator instances (IndicatorsFactory.CreateIndicatorByName)
  3. Process ALL candles through indicators from scratch (Aindicator.ProcessAll)
  4. Run strategy logic
  5. Collect results
  6. Dispose bot completely
```

**Key code locations:**
- `OptimizerExecutor.cs:2106-2151` - `CreateNewBot()` creates fresh bot per iteration
- `Aindicator.cs:649-665` - `ProcessAll()` clears all values and recomputes from scratch
- `BotTabSimple.cs:622-650` - Each bot creates its own indicator instances
- `BayesianOptimizationStrategy.cs:376-391` - `EvaluateCandidateAsync()` creates fresh bot per candidate

### 6.2 Wasted Computation

In a typical optimization run with 1000 parameter combinations and 3 indicators:
- The **same candle data** is processed 1000 times
- Indicators with **identical parameters** across combinations are recalculated fully
- If only 1 of 3 indicator parameters changes, the other 2 are still recomputed

### 6.3 Bot Compilation Cache Bug

`BotFactory.cs` defines `_compiledBotTypesCache` (line 220) but **never writes to it after compilation**. The cache `TryGetValue` at line 296 always misses. This means Roslyn-compiled bot scripts are recompiled on every `CreateBot()` call.

In contrast, `IndicatorsFactory.cs` correctly populates its cache at line 338.

### 6.4 Proposed Caching Architecture

```
IndicatorCache (optimizer-only):
  Key: (IndicatorType, ParameterHash, CandleSeriesHash)
  Value: List<decimal>[] (DataSeries values)

Strategy:
  1. Before creating bot, compute cache key for each indicator config
  2. If cache hit: inject pre-computed values, skip ProcessAll
  3. If cache miss: compute normally, store result in cache
  4. Cache is valid only within a single optimization run
  5. MUST NOT apply in live trading or tester mode
```

**Expected speedup:** 2-10x for typical optimization runs where indicator parameters overlap between combinations.

### 6.5 Quick Wins

1. **Fix bot compilation cache** - add `_compiledBotTypesCache[nameClass] = botType;` after compilation in `BotFactory.cs`
2. **Share candle data** - pass same `List<Candle>` reference to all bots (already partially done via `OptimizerServer`)
3. **Lazy indicator creation** - only create indicators that have parameter changes vs. previous iteration

---

## 7. UI Modernization

### 7.1 Current Architecture

- **152 XAML files** with pure code-behind (no MVVM)
- **0 uses of INotifyPropertyChanged, ICommand, or RelayCommand**
- **148 Dispatcher.Invoke calls** across 49 files for thread-safe UI updates
- Charting via `System.Windows.Forms.DataVisualization` embedded in `WindowsFormsHost`

### 7.2 Code-Behind Issues

All UI logic resides in `.xaml.cs` files:
- Business logic mixed with UI manipulation
- No unit testability of view logic
- Tight coupling between data processing and presentation
- Thread marshalling spread across all layers

### 7.3 Charting Dependency

`WinFormsChartPainter.cs` (lines 12-14) uses:
```csharp
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.Integration;
```

`BotPanelChartUI.xaml` contains **8 WindowsFormsHost** elements for charts, grids, alerts, positions, etc.

### 7.4 Modernization Options

| Approach | Effort | Risk | Benefit |
|----------|--------|------|---------|
| **Add MVVM incrementally** | Low per feature | Low | Testability, separation |
| **Replace charts with OxyPlot** | Medium | Medium | Cross-platform, pure WPF |
| **Replace charts with ScottPlot** | Medium | Medium | Better performance |
| **Full Avalonia migration** | Very High | High | Full cross-platform |
| **MAUI migration** | Very High | Very High | Mobile support too |

### 7.5 Recommended Path

1. Introduce `CommunityToolkit.Mvvm` for `ObservableObject`, `RelayCommand` - minimal overhead
2. New features should use MVVM pattern
3. Replace WinForms charts with OxyPlot.Wpf (stays on WPF, removes WinForms dependency)
4. Evaluate Avalonia migration as separate project after MVVM is established

---

## 8. Weak Spots Summary

### Priority Matrix

| # | Issue | Severity | Effort | Priority |
|---|-------|----------|--------|----------|
| 1 | **Plain-text API key storage** | CRITICAL | Low | **P0** |
| 2 | **No transactional file writes** | HIGH | Medium | **P1** |
| 3 | **Silent exception swallowing** | HIGH | Low | **P1** |
| 4 | **HttpClient per-request creation (OKX)** | MEDIUM | Medium | **P1** |
| 5 | **Bot compilation cache not populated** | MEDIUM | Trivial | **P1** |
| 6 | **SSL validation bypass option** | HIGH | Low | **P2** |
| 7 | **App.config .NET Framework leftovers** | LOW | Trivial | **P2** |
| 8 | **Culture-dependent decimal parsing** | HIGH | Medium | **P2** |
| 9 | **No indicator caching in optimizer** | MEDIUM | High | **P3** |
| 10 | **Legacy DLLs without provenance** | MEDIUM | High | **P3** |
| 11 | **No MVVM separation** | LOW | Very High | **P4** |
| 12 | **WPF-only, no cross-platform** | LOW | Very High | **P4** |

### Quick Wins (< 1 day each)

1. Fix bot compilation cache (`BotFactory.cs` - add 1 line)
2. Remove or update `App.config`
3. Add logging to silent catch blocks
4. Add `[Obsolete]` warning to `IgnoreSslErrors` property

### Medium Term (1-4 weeks)

5. Encrypt API keys with DPAPI
6. Implement atomic file writes (write to temp, then rename)
7. Refactor OKX `HttpInterceptor` for shared `HttpClient`
8. Standardize `CultureInfo.InvariantCulture` across all numeric parsing

### Long Term (1-6 months)

9. Implement indicator caching for optimizer
10. Migrate settings from TXT to `System.Text.Json`
11. Introduce MVVM with CommunityToolkit.Mvvm
12. Replace WinForms charts with OxyPlot.Wpf

---

*Report generated by automated code audit. All file paths and line numbers verified against current codebase.*
