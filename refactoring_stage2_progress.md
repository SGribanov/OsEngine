# Refactoring Stage 2 Progress Log

## 2026-02-15 - Step 2.1 (Atomic File Writes) - In Progress

- Added `project/OsEngine/Entity/SafeFileWriter.cs`:
  - temp file in same directory
  - `Flush(true)` before replace
  - `File.Replace(temp, target, backup, true)` when target exists
  - fallback `File.Move(temp, target)` for first write
  - cleanup of stale `.tmp`
- Migrated save paths to atomic writes:
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
    - `SaveClearingInfo`
    - `SaveNonTradePeriods`
    - `Save`
  - `project/OsEngine/OsTrader/Panels/BotPanel.cs`
    - `SaveParameters`
  - `project/OsEngine/Market/Servers/AServer.cs`
    - `SaveParam`
    - `Save`
    - `SaveLeverageToFile`
- Added tests:
  - `project/OsEngine.Tests/SafeFileWriterTests.cs`
    - creates new file without `.tmp` leftovers
    - overwrite creates `.bak` with previous content

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj` -> success, 0 warnings, 0 errors
- `dotnet build project/OsEngine.Tests/OsEngine.Tests.csproj` -> success, 0 warnings, 0 errors
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj` -> passed 78/78

### Notes

- `dotnet build project/OsEngine.sln` currently fails on restore path with `MSB4276` in this environment (existing toolchain issue), while project-level builds and tests pass.

## 2026-02-15 - Step 2.2 (InvariantCulture in persistence) - In Progress

- Migrated persistence serialization to `CultureInfo.InvariantCulture` in core entity files:
  - `project/OsEngine/Entity/Order.cs`
    - save formatting for decimal/date fields switched to invariant
    - added backward-compatible date parser: invariant first, legacy `ru-RU` fallback
  - `project/OsEngine/Entity/MyTrade.cs`
    - save formatting switched to invariant
    - added backward-compatible date parser: invariant first, legacy `ru-RU` fallback
  - `project/OsEngine/Entity/Position.cs`
    - save formatting for decimal persistence fields switched to invariant
- Checked `project/OsEngine/Entity/Trade.cs` and `project/OsEngine/Candles/Candle.cs`:
  - both already persist decimal values in invariant format
- Added tests:
  - `project/OsEngine.Tests/PersistenceCultureTests.cs`
    - `Order` save uses dot decimal separator under `ru-RU` current culture
    - `MyTrade` load parses legacy `ru-RU` date format
    - `Position` save uses dot decimal separator under `ru-RU` current culture

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj` -> success, 0 warnings, 0 errors
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj` -> passed 81/81

## 2026-02-15 - Step 2.3 (JSON settings subsystem) - Initial Implementation

- Added `project/OsEngine/Entity/SettingsManager.cs`:
  - `Save<T>(path, settings)` via `System.Text.Json`
  - atomic write via `SafeFileWriter.WriteAllText`
  - `Load<T>(path, defaultValue, legacyLoader)` with:
    - default fallback on missing/unreadable file
    - JSON deserialize path
    - optional legacy parser fallback on JSON parse failure
- Added tests `project/OsEngine.Tests/SettingsManagerTests.cs`:
  - JSON save/load roundtrip
  - invalid JSON fallback through legacy loader
  - missing file returns provided default value

## 2026-02-16 - Session Resume / Snapshot Commit

- Resumed interrupted session without MCP resume and restored working context from local git state.
- Verified snapshot state:
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 84/84
- Fixed nullable warning in tests:
  - `project/OsEngine.Tests/SettingsManagerTests.cs` (`TestSettings.Name` initialized with `string.Empty`)
- Created snapshot commit:
  - `bf7d8ea1a` — `refactor(stage2): add JSON settings manager and harden optimizer flow`
- Push attempt from current environment failed due network restrictions:
  - SSH (`github.com:22`) blocked
  - HTTPS (`github.com:443`) blocked

## 2026-02-16 - Optimizer Hardening Follow-up (Regression tests)

- Added regression tests in `project/OsEngine.Tests/OptimizerRefactorTests.cs` for recent `ParameterIterator` guards:
  - `CountCombinations` returns `0` when step is non-positive
  - `EnumerateCombinations` with non-positive step yields start value once and terminates
  - int overshoot step clamps to stop value during enumeration
  - decimal overshoot step clamps to stop value during enumeration

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 88/88

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in OptimizerDataStorage

- Migrated `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorage.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON via `SettingsManager.Save(...)`
  - `Load()` now reads via `SettingsManager.Load(...)`
  - added legacy fallback parser for old line-based `.txt` content in the same file path
  - preserved enum-safety checks (`Enum.IsDefined`) while applying loaded values
- Added coverage in `project/OsEngine.Tests/OptimizerRefactorTests.cs`:
  - `OptimizerDataStorage_Load_ShouldReadLegacyTextSettings`
  - `OptimizerDataStorage_Save_ShouldPersistJsonAndRoundTrip`
- Added `ParameterIterator` regression tests (from hardening follow-up) and kept them green in same run.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 90/90

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in NonTradePeriods

- Migrated `project/OsEngine/Entity/NonTradePeriods.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON via `SettingsManager.Save(...)`
  - `Load()` now reads via `SettingsManager.Load(...)`
  - added legacy fallback parser for old line-based format (`GetFullSaveArray`-compatible)
  - preserved existing in-memory model and `LoadFromString...` application flow
- Added tests `project/OsEngine.Tests/NonTradePeriodsPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 92/92

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in HorizontalVolume

- Migrated `project/OsEngine/Entity/HorizontalVolume.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON via `SettingsManager.Save(...)`
  - `Load()` now reads via `SettingsManager.Load(...)`
  - added legacy fallback parser for the old single-line decimal format
- Added tests `project/OsEngine.Tests/HorizontalVolumePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 94/94

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in NumberGen

- Migrated `project/OsEngine/Entity/NumberGen.cs` persistence to `SettingsManager`:
  - private `Save()` now writes JSON via `SettingsManager.Save(...)`
  - private `Load()` now reads via `SettingsManager.Load(...)`
  - added legacy fallback parser for old two-line integer format
- Added tests `project/OsEngine.Tests/NumberGenPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip` (reflection-based call to private static `Save/Load`)
  - `Load_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 96/96

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ComparePositionsModule

- Migrated `project/OsEngine/Market/Servers/ComparePositionsModule.cs` persistence to `SettingsManager`:
  - main settings (`CompareModule.txt`) now save/load as JSON
  - ignored-securities settings (`CompareModule_IgnoreSec.txt`) now save/load as JSON
  - added legacy fallback parsers for both old line-based formats
- Added tests `project/OsEngine.Tests/ComparePositionsModulePersistenceTests.cs`:
  - `SaveLoad_ShouldPersistJsonForMainSettings`
  - `Load_ShouldSupportLegacyLineBasedMainSettings`
  - `SaveLoadIgnoredSecurities_ShouldPersistJson`
  - `LoadIgnoredSecurities_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 100/100

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in InteractiveBrokers securities watchlist

- Migrated `project/OsEngine/Market/Servers/InteractiveBrokers/InteractiveBrokersServer.cs` persistence to `SettingsManager`:
  - `SaveIbSecurities()` now saves structured JSON (`IbSecuritiesToWatch.txt`)
  - `LoadIbSecurities()` now loads JSON and falls back to legacy line-based parser
  - preserved fallback to `LoadStartSecurities()` on missing/empty/invalid persisted content
- Added tests `project/OsEngine.Tests/InteractiveBrokersSecuritiesPersistenceTests.cs`:
  - `SaveIbSecurities_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadIbSecurities_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 102/102

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in AServer core settings

- Migrated `project/OsEngine/Market/Servers/AServer.cs` persistence to `SettingsManager`:
  - `Save()` now stores `ServerPrefix` in JSON (`<ServerNameUnique>ServerSettings.txt`)
  - `Load()` now reads JSON and falls back to legacy single-line format parser
  - preserved existing behavior for missing/empty/invalid settings file
- Added tests `project/OsEngine.Tests/AServerSettingsPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 104/104

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ServerSms settings

- Migrated `project/OsEngine/Logging/ServerSms.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\smsSet.txt`
  - `Load()` now reads JSON and falls back to legacy 3-line format parser
  - preserved default values behavior when settings file is missing
- Added tests `project/OsEngine.Tests/ServerSmsPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 106/106

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ServerMail settings

- Migrated `project/OsEngine/Logging/ServerMail.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\mailSet.txt`
  - `Load()` now reads JSON and falls back to legacy line-based format parser
  - preserved `IsReady` calculation semantics from stored recipients
- Added tests `project/OsEngine.Tests/ServerMailPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 108/108

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ServerWebhook settings

- Migrated `project/OsEngine/Logging/ServerWebhook.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\webhookSet.txt`
  - `Load()` now reads JSON and falls back to legacy line-based format parser
  - preserved `IsReady` calculation semantics from stored webhook list
- Added tests `project/OsEngine.Tests/ServerWebhookPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 110/110

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in MessageSender settings

- Migrated `project/OsEngine/Logging/MessageSender.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\<name>MessageSender.txt`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - legacy parser supports truncated historical format (missing trailing fields -> default `false`)
- Added tests `project/OsEngine.Tests/MessageSenderPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 112/112

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ServerTelegram settings

- Migrated `project/OsEngine/Logging/ServerTelegram.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\telegramSet.txt`
  - `Load()` now reads JSON and falls back to legacy 3-line format parser
  - preserved readiness behavior (`_isReady`) on successful load/save
- Added tests `project/OsEngine.Tests/ServerTelegramPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use uninitialized instance to avoid constructor side effects (network polling threads)

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 114/114

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in TimeFrameBuilder settings

- Migrated `project/OsEngine/Candles/TimeFrameBuilder.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\<name>TimeFrameBuilder.txt`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - legacy parser keeps compatibility with optional trailing spread settings
- Added tests `project/OsEngine.Tests/TimeFrameBuilderPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 116/116

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in AlertToPrice settings

- Migrated `project/OsEngine/Alerts/AlertToPrice.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\<Name>Alert.txt`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing alert model fields and behavior
- Added tests `project/OsEngine.Tests/AlertToPricePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 118/118

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in PrimeSettingsMaster

- Migrated `project/OsEngine/PrimeSettings/PrimeSettingsMaster.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\PrimeSettings.txt`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility rule for legacy bool-like header label values
- Added tests `project/OsEngine.Tests/PrimeSettingsMasterPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 120/120

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in CandleConverter settings

- Migrated `project/OsEngine/Candles/CandleConverter.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\CandleConverter.txt`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing `TimeFrame`/source/exit file model
- Added tests `project/OsEngine.Tests/CandleConverterPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use uninitialized instance to avoid UI constructor dependencies

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 122/122

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ChartClusterMaster settings

- Migrated `project/OsEngine/Charts/ClusterChart/ChartClusterMaster.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\<name>ClusterChartMasterSet.txt`
  - `Load()` now reads JSON and falls back to legacy single-line enum parser
  - made load-side chart assignment null-safe (`_chart != null`) for compatibility with non-constructor test instances
- Added tests `project/OsEngine.Tests/ChartClusterMasterPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use uninitialized instance to avoid UI constructor dependencies

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 124/124

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in BotTabCluster on/off settings

- Migrated `project/OsEngine/OsTrader/Panels/Tab/BotTabCluster.cs` persistence to `SettingsManager`:
  - private `Save()` now writes JSON into `Engine\\<TabName>ClusterOnOffSet.txt`
  - private `Load()` now reads JSON and falls back to legacy single-line bool parser
  - preserved fallback behavior (`_eventsIsOn = true` on load exceptions)
- Added tests `project/OsEngine.Tests/BotTabClusterPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private `Save/Load` via reflection on uninitialized instance

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 126/126

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in AlertToChart settings

- Migrated `project/OsEngine/Alerts/AlertToChart.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\<Name>Alert.txt`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved chart-line model and signal/slippage fields
- Added tests `project/OsEngine.Tests/AlertToChartPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 128/128

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ChartMasterColorKeeper settings

- Migrated `project/OsEngine/Charts/ColorKeeper/ChartMasterColorKeeper.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\Color\\<name>Color.txt`
  - `Load()` now reads JSON and falls back to legacy 10-line format parser
  - preserved default color scheme behavior when settings file is missing
- Added tests `project/OsEngine.Tests/ChartMasterColorKeeperPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 130/130

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in BotManualControl settings

- Migrated `project/OsEngine/OsTrader/Panels/Tab/Internal/BotManualControl.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\<name>StrategSettings.txt`
  - private `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved original `Load()` contract (`false` only when file is missing)
- Added tests `project/OsEngine.Tests/BotManualControlPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use uninitialized instance + reflection to avoid constructor side effects

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 132/132

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in IndexFormulaBuilder settings

- Migrated `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs` (`IndexFormulaBuilder`) persistence to `SettingsManager`:
  - private `Save()` now writes JSON into `Engine\\<botUniqName>IndexAutoFormulaSettings.txt`
  - private `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved optimizer bypass behavior (`StartProgram.IsOsOptimizer`)
- Added tests `project/OsEngine.Tests/IndexFormulaBuilderPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use uninitialized instance + reflection for private `Save/Load`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 134/134

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in BotTabIndex spread settings

- Migrated `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs` (`SpreadSet.txt`) persistence to `SettingsManager`:
  - public `Save()` now writes structured JSON into `Engine\\<TabName>SpreadSet.txt`
  - public `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved defaults and connector reconstruction behavior from stored unique names
  - made formula apply path null-safe for non-constructor test instances (`FullRecalculateIndex()` only when chart is initialized)
- Added tests `project/OsEngine.Tests/BotTabIndexSpreadPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
- Stabilized `project/OsEngine.Tests/AServerSettingsPersistenceTests.cs`:
  - switched to uninitialized `YahooServer` instances with explicit `_serverRealization` injection
  - invoke private `AServer.Load()` via reflection to avoid constructor side effects (UI/file-lock flakiness)

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 136/136

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in OsConverterMaster settings

- Migrated `project/OsEngine/OsConverter/OsConverterMaster.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\Converter.txt`
  - `Load()` now reads JSON and falls back to legacy 3-line format parser
  - preserved original model (`TimeFrame`, source file, output file)
- Added tests `project/OsEngine.Tests/OsConverterMasterPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use uninitialized instance + reflection to avoid constructor/UI side effects

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 138/138

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in OsLocalization settings

- Migrated `project/OsEngine/Language/OsLocalization.cs` persistence to `SettingsManager`:
  - `Save()` now writes JSON into `Engine\\local.txt`
  - private `Load()` now reads JSON and falls back to legacy 3-line format parser
  - preserved default date/time format fallback behavior when settings file is missing or incomplete
- Added tests `project/OsEngine.Tests/OsLocalizationPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests isolate and restore static localization state via reflection

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 140/140

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in GlobalGUILayout layout settings

- Migrated `project/OsEngine/Layout/GlobalGUILayout.cs` (`LayoutGui.txt`) persistence to `SettingsManager`:
  - private `Save()` now writes JSON into `Engine\\LayoutGui.txt`
  - private `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing layout-filtering rules for invalid/minimized window coordinates
- Added tests `project/OsEngine.Tests/GlobalGUILayoutPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private static `Save/Load` via reflection and isolate static `UiOpenWindows` state

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 142/142
