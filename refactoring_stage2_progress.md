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

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in attached servers settings

- Migrated `project/OsEngine/Market/ServerMasterSourcesPainter.cs` (`AttachedServers.txt`) persistence to `SettingsManager`:
  - private `SaveAttachedServers()` now writes JSON into `Engine\\AttachedServers.txt`
  - private `LoadAttachedServers()` now reads JSON and falls back to legacy line-based parser
  - preserved enum parsing behavior (`Enum.TryParse` with case-insensitive mode)
- Added tests `project/OsEngine.Tests/ServerMasterSourcesPainterPersistenceTests.cs`:
  - `SaveAttachedServers_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadAttachedServers_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private methods via reflection on uninitialized instance

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 144/144

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in BlockMaster settings

- Migrated `project/OsEngine/OsTrader/Gui/BlockInterface/BlockMaster.cs` persistence to `SettingsManager`:
  - `Password` now saves/loads JSON in `Engine\\PrimeSettingss.txt` with legacy single-line fallback
  - `IsBlocked` now saves/loads JSON in `Engine\\PrimeSettingsss.txt` with legacy single-line fallback
  - preserved encryption/decryption flow and safe default behavior on read errors (`""` / `false`)
- Added tests `project/OsEngine.Tests/BlockMasterPersistenceTests.cs`:
  - `Password_ShouldPersistJson_AndLoadRoundTrip`
  - `Password_ShouldSupportLegacyLineBasedFormat`
  - `IsBlocked_ShouldPersistJson_AndLoadRoundTrip`
  - `IsBlocked_ShouldSupportLegacyLineBasedFormat`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 148/148

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in GlobalGUILayout screen resolution settings

- Migrated `project/OsEngine/Layout/GlobalGUILayout.cs` (`ScreenResolution.txt`) persistence to `SettingsManager`:
  - private `SaveResolution(...)` now writes JSON into `Engine\\ScreenResolution.txt`
  - private `ScreenSettingsIsAllRight()` now reads JSON and falls back to legacy 3-line format parser
  - preserved mismatch behavior (re-save current resolution and return `false` when stored values differ)
- Added tests `project/OsEngine.Tests/GlobalGUILayoutScreenResolutionPersistenceTests.cs`:
  - `SaveResolution_ShouldPersistJson_AndScreenSettingsCheckReturnTrue`
  - `ScreenSettingsIsAllRight_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private static methods via reflection

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 150/150

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Atp securities cache settings

- Migrated `project/OsEngine/Market/Servers/Atp/AtpServer.cs` (`AtpServerRealization`) persistence to `SettingsManager`:
  - `TrySaveSecuritiesInFile()` now writes JSON into `Engine\\AtpSecurities.txt`
  - `TryLoadSecuritiesFromFile()` now reads JSON and falls back to legacy line-based parser
  - preserved legacy `NameId` fallback behavior when legacy cache has empty identifier
- Added tests `project/OsEngine.Tests/AtpServerSecuritiesPersistenceTests.cs`:
  - `SaveSecurities_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadSecurities_ShouldSupportLegacyLineBasedFormat`
- Stabilized `project/OsEngine.Tests/OsLocalizationPersistenceTests.cs` against parallel file races:
  - moved tests into collection with `DisableParallelization = true`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 152/152

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in RamMemoryUsageAnalyze settings

- Migrated `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs` (`RamMemoryUsageAnalyze`) persistence to `SettingsManager`:
  - private `Save()` now writes JSON into `Engine\\SystemStress\\RamMemorySettings.txt`
  - private `Load()` now reads JSON and falls back to legacy 3-line format parser
  - preserved defaults and value-application behavior for `_ramCollectDataIsOn`, `_ramPeriodSavePoint`, `_ramPointsMax`
- Added tests `project/OsEngine.Tests/RamMemoryUsageAnalyzePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private `Load/Save` via reflection on uninitialized instance

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 154/154

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in CpuUsageAnalyze settings

- Migrated `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs` (`CpuUsageAnalyze`) persistence to `SettingsManager`:
  - private `Save()` now writes JSON into `Engine\\SystemStress\\CpuMemorySettings.txt`
  - private `Load()` now reads JSON and falls back to legacy 3-line format parser
  - preserved defaults and value-application behavior for `_cpuCollectDataIsOn`, `_cpuPeriodSavePoint`, `_cpuPointsMax`
- Added tests `project/OsEngine.Tests/CpuUsageAnalyzePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private `Load/Save` via reflection on uninitialized instance

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 156/156

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in EcqUsageAnalyze settings

- Migrated `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs` (`EcqUsageAnalyze`) persistence to `SettingsManager`:
  - private `Save()` now writes JSON into `Engine\\SystemStress\\EcqMemorySettings.txt`
  - private `Load()` now reads JSON and falls back to legacy 3-line format parser
  - preserved defaults and value-application behavior for `_ecqCollectDataIsOn`, `_ecqPeriodSavePoint`, `_ecqPointsMax`
- Added tests `project/OsEngine.Tests/EcqUsageAnalyzePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private `Load/Save` via reflection on uninitialized instance

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 158/158

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in MoqUsageAnalyze settings

- Migrated `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs` (`MoqUsageAnalyze`) persistence to `SettingsManager`:
  - private `Save()` now writes JSON into `Engine\\SystemStress\\MoqMemorySettings.txt`
  - private `Load()` now reads JSON and falls back to legacy 3-line format parser
  - preserved defaults and value-application behavior for `_moqCollectDataIsOn`, `_moqPeriodSavePoint`, `_moqPointsMax`
- Added tests `project/OsEngine.Tests/MoqUsageAnalyzePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private `Load/Save` via reflection on uninitialized instance

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 160/160

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in AServer params settings

- Migrated `project/OsEngine/Market/Servers/AServer.cs` (`Params.txt`) persistence to `SettingsManager`:
  - private `SaveParam()` now writes JSON into `Engine\\<ServerNameUnique>Params.txt`
  - private `LoadParam()` now reads JSON and falls back to legacy line-based parser
  - preserved existing `IServerParameter` reconstruction and name/type match logic
- Added tests `project/OsEngine.Tests/AServerParamsPersistenceTests.cs`:
  - `SaveParam_ShouldPersistJson_AndLoadParamRoundTrip`
  - `LoadParam_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private `SaveParam/LoadParam` via reflection on uninitialized `YahooServer` with injected realization

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 162/162

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in TesterServer core settings

- Migrated `project/OsEngine/Market/Servers/Tester/TesterServer.cs` (`TestServer.txt`) persistence to `SettingsManager`:
  - private `Load()` now reads JSON and falls back to legacy 11-line format parser
  - public `Save()` now writes JSON into `Engine\\TestServer.txt`
  - preserved existing model fields and defaults (`_activeSet`, slippages, tester/source types, execution type, UI and memory flags)
- Added tests `project/OsEngine.Tests/TesterServerPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use uninitialized instance + reflection for private `Load`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 164/164

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in TesterServer security test settings

- Migrated `project/OsEngine/Market/Servers/Tester/TesterServer.cs` (`SecurityTestSettings.txt`) persistence to `SettingsManager`:
  - `SaveSecurityTestSettings()` now writes JSON to dynamic `...\\SecurityTestSettings.txt` path
  - `LoadSecurityTestSettings()` now reads JSON and falls back to legacy 2-line date format parser
  - preserved path resolution logic based on `TesterSourceDataType` (`Set`/`Folder`)
- Added tests `project/OsEngine.Tests/TesterServerSecurityTestSettingsPersistenceTests.cs`:
  - `SaveSecurityTestSettings_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadSecurityTestSettings_ShouldSupportLegacyLineBasedFormat`
  - tests use uninitialized instance with reflection-based folder-mode configuration

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 166/166

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in TesterServer clearing settings

- Migrated `project/OsEngine/Market/Servers/Tester/TesterServer.cs` (`TestServerClearings.txt`) persistence to `SettingsManager`:
  - `SaveClearingInfo()` now writes JSON into `Engine\\TestServerClearings.txt`
  - private `LoadClearingInfo()` now reads JSON and falls back to legacy line-based parser
  - preserved `OrderClearing` serialization contract via existing `GetSaveString()/SetFromString()`
- Added tests `project/OsEngine.Tests/TesterServerClearingPersistenceTests.cs`:
  - `SaveClearingInfo_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadClearingInfo_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private `LoadClearingInfo()` via reflection on uninitialized instance

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 168/168

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in TesterServer non-trade periods settings

- Migrated `project/OsEngine/Market/Servers/Tester/TesterServer.cs` (`TestServerNonTradePeriods.txt`) persistence to `SettingsManager`:
  - `SaveNonTradePeriods()` now writes JSON into `Engine\\TestServerNonTradePeriods.txt`
  - private `LoadNonTradePeriods()` now reads JSON and falls back to legacy line-based parser
  - preserved `NonTradePeriod` serialization contract via existing `GetSaveString()/SetFromString()`
- Added tests `project/OsEngine.Tests/TesterServerNonTradePeriodsPersistenceTests.cs`:
  - `SaveNonTradePeriods_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadNonTradePeriods_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private `LoadNonTradePeriods()` via reflection on uninitialized instance

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 170/170

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in TesterServer securities timeframe settings

- Migrated `project/OsEngine/Market/Servers/Tester/TesterServer.cs` (`TestServerSecuritiesTf...txt`) persistence to `SettingsManager`:
  - `SaveSetSecuritiesTimeFrameSettings()` now writes JSON into dynamic `Engine\\TestServerSecuritiesTf...txt` path
  - private `LoadSetSecuritiesTimeFrameSettings()` now reads JSON and falls back to legacy `name#timeframe` line-based parser
  - preserved strict ordered security-name matching behavior during load
- Added tests `project/OsEngine.Tests/TesterServerSecuritiesTimeFramePersistenceTests.cs`:
  - `SaveSetSecuritiesTimeFrameSettings_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadSetSecuritiesTimeFrameSettings_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private load/path methods via reflection on uninitialized instance

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 172/172

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in TesterServer security dop settings

- Migrated `project/OsEngine/Market/Servers/Tester/TesterServer.cs` (`SecuritiesSettings.txt`) persistence to `SettingsManager`:
  - private `LoadSecurityDopSettings(path)` now reads JSON and falls back to legacy `$`-line parser
  - `SaveSecurityDopSettings(security)` now writes JSON to dynamic `...\\SecuritiesSettings.txt` path
  - preserved current save/update flow and existing data contract (`name/lot/GO/step cost/step/decimals/margin sell/expiration`)
- Added tests `project/OsEngine.Tests/TesterServerSecurityDopSettingsPersistenceTests.cs`:
  - `SaveSecurityDopSettings_ShouldPersistJson_AndLoadSettings`
  - `LoadSecurityDopSettings_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private loader and dynamic path resolver via reflection on uninitialized instance

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 174/174

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in QuikLua securities cache settings

- Migrated `project/OsEngine/Market/Servers/QuikLua/QuikLuaServer.cs` (`QuikLuaSecuritiesCache.txt`) persistence to `SettingsManager`:
  - private `SaveToCache()` now writes JSON wrapper into `Engine\\QuikLuaSecuritiesCache.txt`
  - private `LoadSecuritiesFromCache()` now reads JSON and falls back to legacy raw compressed-string format
  - preserved fallback behavior to live load (`LoadSecuritiesFromQuik()`) on cache read/parse issues
- Added tests `project/OsEngine.Tests/QuikLuaSecuritiesCachePersistenceTests.cs`:
  - `SaveToCache_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadSecuritiesFromCache_ShouldSupportLegacyCompressedStringFormat`
  - tests invoke private cache methods via reflection on uninitialized realization instance

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 176/176

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in MetaTrader5 securities cache settings

- Migrated `project/OsEngine/Market/Servers/MetaTrader5/MetaTrader5Server.cs` (`MetaTrader5SecuritiesCache.txt`) persistence to `SettingsManager`:
  - private `SaveToCache()` now writes JSON wrapper into `Engine\\MetaTrader5SecuritiesCache.txt`
  - private `LoadSecuritiesFromCache()` now reads JSON and falls back to legacy raw compressed-string format
  - preserved fallback behavior to live load (`LoadSecuritiesFromMetaTrader()`) on cache read/parse issues
- Added tests `project/OsEngine.Tests/MetaTrader5SecuritiesCachePersistenceTests.cs`:
  - `SaveToCache_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadSecuritiesFromCache_ShouldSupportLegacyCompressedStringFormat`
  - tests invoke private cache methods via reflection on uninitialized realization instance

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 178/178

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in MetaTrader5 positions cache settings

- Migrated `project/OsEngine/Market/Servers/MetaTrader5/MetaTrader5Server.cs` (`MetaTrader5PositionsCache.txt`) persistence to `SettingsManager`:
  - private `SavePositionsInFile()` now writes JSON wrapper into `Engine\\MetaTrader5PositionsCache.txt`
  - private `LoadPositionsFromFile()` now reads JSON and falls back to legacy raw compressed-string format
  - preserved existing no-op behavior on missing cache file and current dictionary deserialization contract
- Added tests `project/OsEngine.Tests/MetaTrader5PositionsCachePersistenceTests.cs`:
  - `SavePositionsInFile_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadPositionsFromFile_ShouldSupportLegacyCompressedStringFormat`
  - tests invoke private cache methods and backing dictionary field via reflection on uninitialized realization instance

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 180/180

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ProxyMaster core settings

- Migrated `project/OsEngine/Market/Proxy/ProxyMaster.cs` (`ProxyMaster.txt`) persistence to `SettingsManager`:
  - private `LoadSettings()` now reads JSON and falls back to legacy line-based parser
  - public `SaveSettings()` now writes JSON into `Engine\\ProxyMaster.txt`
  - preserved current defaults/activation flow and existing field contract (`AutoPingIsOn/AutoPingLastTime/AutoPingMinutes`)
- Added tests `project/OsEngine.Tests/ProxyMasterSettingsPersistenceTests.cs`:
  - `SaveSettings_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadSettings_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private loader via reflection with backup/restore of `Engine\\ProxyMaster.txt`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 182/182

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ProxyMaster proxy hub settings

- Migrated `project/OsEngine/Market/Proxy/ProxyMaster.cs` (`ProxyHub.txt`) persistence to `SettingsManager`:
  - private `LoadProxy()` now reads JSON and falls back to legacy line-based parser
  - public `SaveProxy()` now writes JSON wrapper into `Engine\\ProxyHub.txt`
  - preserved existing proxy serialization contract via `ProxyOsa.GetStringToSave()/LoadFromString()`
- Added tests `project/OsEngine.Tests/ProxyMasterProxyHubPersistenceTests.cs`:
  - `SaveProxy_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadProxy_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private loader via reflection with backup/restore of `Engine\\ProxyHub.txt`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 184/184

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ConnectorNews settings

- Migrated `project/OsEngine/Market/Connectors/ConnectorNews.cs` (`<name>ConnectorNews.txt`) persistence to `SettingsManager`:
  - private `Load()` now reads JSON and falls back to legacy line-based parser
  - public `Save()` now writes JSON into dynamic `Engine\\<name>ConnectorNews.txt` path
  - preserved delete/save guards and existing settings contract (`server type/events flag/news limit/server full name`)
- Added tests `project/OsEngine.Tests/ConnectorNewsSettingsPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private loader and backing fields via reflection on uninitialized instance (without starting subscription task)

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 186/186

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ConnectorCandles settings

- Migrated `project/OsEngine/Market/Connectors/ConnectorCandles.cs` (`<name>ConnectorPrime.txt`) persistence to `SettingsManager`:
  - private `Load()` now reads JSON and falls back to legacy line-based parser
  - public `Save()` now writes JSON into dynamic `Engine\\<name>ConnectorPrime.txt` path
  - preserved legacy optional-line behavior (`EventsIsOn` default `true`, `ServerFullName` default `ServerType.ToString()`)
- Added tests `project/OsEngine.Tests/ConnectorCandlesSettingsPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat_WithOptionalFieldsMissing`
  - tests invoke private loader and backing fields via reflection on uninitialized instance (without starting subscription task)

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 188/188

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ServerMaster core settings

- Migrated `project/OsEngine/Market/ServerMaster.cs` (`ServerMaster.txt`) persistence to `SettingsManager`:
  - public `Save()` now writes JSON into `Engine\\ServerMaster.txt`
  - public `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing single-flag contract for `NeedToConnectAuto`
- Added tests `project/OsEngine.Tests/ServerMasterSettingsPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests backup/restore `Engine\\ServerMaster.txt` and static flag value

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 190/190

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ServerMaster most-popular servers stats

- Migrated `project/OsEngine/Market/ServerMaster.cs` (`MostPopularServers.txt`) persistence to `SettingsManager`:
  - private `SaveMostPopularServers(type)` now writes JSON wrapper into `Engine\\MostPopularServers.txt`
  - public `LoadMostPopularServersWithCount()` now reads JSON and falls back to legacy `ServerType&Count` line-based parser
  - preserved existing deduplication and count aggregation flow
- Added tests `project/OsEngine.Tests/ServerMasterMostPopularServersPersistenceTests.cs`:
  - `SaveMostPopularServers_ShouldPersistJson_AndLoadCounts`
  - `LoadMostPopularServersWithCount_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private saver via reflection and backup/restore `Engine\\MostPopularServers.txt`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 192/192

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in PortfolioToCopy settings

- Migrated `project/OsEngine/Market/AutoFollow/CopyTrader.cs` (`Engine\\CopyTrader\\<Name>.txt`) persistence to `SettingsManager`:
  - `PortfolioToCopy.Save()` now writes JSON
  - `PortfolioToCopy.Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing field contract, security mapping serialization string and delete path behavior
- Added tests `project/OsEngine.Tests/PortfolioToCopySettingsPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use uninitialized `PortfolioToCopy` instances with backup/restore of `Engine\\CopyTrader\\<Name>.txt`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 194/194

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in CopyMaster hub settings

- Migrated `project/OsEngine/Market/AutoFollow/CopyMaster.cs` (`Engine\\CopyTrader\\CopyTradersHub.txt`) persistence to `SettingsManager`:
  - private `LoadCopyTraders()` now reads JSON and falls back to legacy line-based parser
  - public `SaveCopyTraders()` now writes JSON wrapper
  - preserved existing per-trader line serialization contract via `CopyTrader.GetStringToSave()`
- Added tests `project/OsEngine.Tests/CopyMasterHubPersistenceTests.cs`:
  - `SaveCopyTraders_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadCopyTraders_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private loader via reflection, use uninitialized fake traders for save path, and stop spawned traders via `ClearDelete()`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 196/196

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in RiskManager settings

- Migrated `project/OsEngine/OsTrader/RiskManager/RiskManager.cs` (`Engine\\<name>.txt`) persistence to `SettingsManager`:
  - private `Load()` now reads JSON and falls back to legacy line-based parser
  - public `Save()` now writes JSON into dynamic `Engine\\<name>.txt` path
  - preserved existing settings contract (`MaxDrowDownToDayPersent`, `IsActiv`, `ReactionType`) and delete path behavior
- Added tests `project/OsEngine.Tests/RiskManagerSettingsPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private loader via reflection on uninitialized instance with backup/restore of settings file

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 198/198

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in BotTabOptions settings

- Migrated `project/OsEngine/OsTrader/Panels/Tab/BotTabOptions.cs` (`Engine\\<TabName>\\OptionsSettings.txt`) persistence to `SettingsManager`:
  - private `SaveSettings()` now writes JSON into tab-specific settings path
  - public `LoadSettings()` now reads JSON and falls back to legacy key:value parser
  - preserved existing settings contract (`PortfolioName`, `UnderlyingAssets`, `StrikesToShow`, server binding, emulator/events flags)
- Added tests `project/OsEngine.Tests/BotTabOptionsSettingsPersistenceTests.cs`:
  - `SaveSettings_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadSettings_ShouldSupportLegacyKeyValueFormat`
  - tests use uninitialized tab instance with reflection-based setup of private fields/backing fields and folder backup/restore

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 200/200

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in BotTabScreener indicators settings

- Migrated `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs` (`Engine\\<TabName>ScreenerIndicators.txt`) persistence to `SettingsManager`:
  - private `SaveIndicators()` now writes JSON wrapper
  - private `LoadIndicators()` now reads JSON and falls back to legacy line-based parser
  - preserved indicator serialization contract via `IndicatorOnTabs.GetSaveStr()/SetFromStr()`
- Added tests `project/OsEngine.Tests/BotTabScreenerIndicatorsPersistenceTests.cs`:
  - `SaveIndicators_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadIndicators_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private load/save methods via reflection on uninitialized tab instance with file backup/restore

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 202/202

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in BotTabScreener core settings

- Migrated `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs` (`Engine\\<TabName>ScreenerSet.txt`) persistence to `SettingsManager`:
  - public `SaveSettings()` now writes JSON DTO with screener/tab/candle/commission/security settings
  - public `LoadSettings()` now reads JSON and falls back to legacy line-based parser
  - preserved default fallback behavior for missing/corrupted settings (`Simple` candle realization)
- Added tests `project/OsEngine.Tests/BotTabScreenerSettingsPersistenceTests.cs`:
  - `SaveSettings_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadSettings_ShouldSupportLegacyLineBasedFormat`
  - tests use uninitialized screener instance with reflection-based setup and backup/restore of screener settings file

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 204/204

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in BotTabPair standard settings

- Migrated `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs` (`Engine\\<TabName>StandartPairsSettings.txt`) persistence to `SettingsManager`:
  - public `SaveStandartSettings()` now writes JSON DTO
  - private `LoadStandartSettings()` now reads JSON and falls back to legacy line-based parser
  - preserved settings contract for slippage/volume/correlation/cointegration/trade regimes and flags
- Added tests `project/OsEngine.Tests/BotTabPairStandartSettingsPersistenceTests.cs`:
  - `SaveStandartSettings_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadStandartSettings_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private loader via reflection on uninitialized tab instance with file backup/restore

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 206/206

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in PairToTrade settings

- Migrated `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs` (`Engine\\<Name>PairsSettings.txt`, class `PairToTrade`) persistence to `SettingsManager`:
  - `PairToTrade.Save()` now writes JSON DTO
  - private `PairToTrade.Load()` now reads JSON and falls back to legacy line-based parser
  - preserved settings contract for pair parameters, trade regimes, cointegration side and auto-rebuild flags
- Added tests `project/OsEngine.Tests/PairToTradeSettingsPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private loader via reflection on uninitialized pair instance with file backup/restore

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 208/208

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in PositionController stop-limits settings

- Migrated `project/OsEngine/Journal/Internal/PositionController.cs` (`Engine\\<Name>DealControllerStopLimits.txt`) persistence to `SettingsManager`:
  - private `TrySaveStopLimits()` now writes JSON DTO with stop-limit order strings
  - public `LoadStopLimits()` now reads JSON and falls back to legacy line-based parser
  - preserved existing stop-limit serialization contract via `PositionOpenerToStopLimit.GetSaveString()/SetFromString()`
- Added tests `project/OsEngine.Tests/PositionControllerStopLimitsPersistenceTests.cs`:
  - `TrySaveStopLimits_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadStopLimits_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private saver via reflection on uninitialized controller instance with backup/restore of stop-limits settings file

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 210/210

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in BotFactory optimizer bots cache

- Migrated `project/OsEngine/Robots/BotFactory.cs` (`Engine\\OptimizerBots.txt`) persistence to `SettingsManager`:
  - private `SaveOptimizerBotsNamesToFile()` now writes JSON DTO with optimizer bot names
  - private `LoadOptimizerBotsNamesFromFile()` now reads JSON and falls back to legacy line-based parser
  - preserved existing optimizer cache contract for bot-name list content/order
- Added tests `project/OsEngine.Tests/BotFactoryOptimizerBotsPersistenceTests.cs`:
  - `SaveOptimizerBotsNamesToFile_ShouldPersistJson_AndLoadRoundTrip`
  - `LoadOptimizerBotsNamesFromFile_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private static save/load via reflection with backup/restore of `Engine\\OptimizerBots.txt` and static field state

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 212/212

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in PositionController deals settings

- Migrated `project/OsEngine/Journal/Internal/PositionController.cs` (`Engine\\<Name>DealController.txt`) persistence to `SettingsManager`:
  - private `SavePositions()` now writes JSON DTO with commission settings and position lines
  - private `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing deal serialization contract via `Position.GetStringForSave()/SetDealFromString()`
- Added tests `project/OsEngine.Tests/PositionControllerDealsPersistenceTests.cs`:
  - `SavePositions_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests invoke private load/save via reflection on uninitialized controller instances with backup/restore of deals settings file

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 214/214

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Cmo indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Cmo.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with period/color/paint settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `Period`, `ColorBase`, and `PaintOn`
- Added tests `project/OsEngine.Tests/CmoPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 216/216

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Rsi indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Rsi.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with color/length settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `ColorBase` and `Length`
- Added tests `project/OsEngine.Tests/RsiPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 218/218

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Cci indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/CCI.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with color/length/paint/price-point settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `ColorBase`, `Length`, `PaintOn`, and `TypePointsToSearch`
- Added tests `project/OsEngine.Tests/CciPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 220/220

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in BearsPower indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/BearsPower.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with period/colors/paint settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `Period`, `ColorUp`, `ColorDown`, and `PaintOn`
- Added tests `project/OsEngine.Tests/BearsPowerPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 222/222

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in BullsPower indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/BullsPower.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with period/colors/paint settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `Period`, `ColorUp`, `ColorDown`, and `PaintOn`
- Added tests `project/OsEngine.Tests/BullsPowerPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 224/224

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Atr indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Atr.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with color/length/paint/watr settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `ColorBase`, `Length`, `PaintOn`, and `IsWatr`
- Added tests `project/OsEngine.Tests/AtrPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 226/226

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in AtrChannel indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/AtrChannel.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with color/length/multiplier/paint settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `ColorBase`, `Length`, `Multiplier`, and `PaintOn`
- Added tests `project/OsEngine.Tests/AtrChannelPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 228/228

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in BfMfi indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/BfMfi.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with color/paint settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `ColorBase` and `PaintOn`
- Added tests `project/OsEngine.Tests/BfMfiPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 230/230

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Bollinger indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Bollinger.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with colors/length/deviation/paint settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `ColorUp`, `ColorDown`, `Length`, `Deviation`, and `PaintOn`
- Added tests `project/OsEngine.Tests/BollingerPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 232/232

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in DonchianChannel indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/DonchianChannel.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with colors/length/paint settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `ColorUp`, `ColorAvg`, `ColorDown`, `Length`, and `PaintOn`
- Added tests `project/OsEngine.Tests/DonchianChannelPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 234/234

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ForceIndex indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/ForceIndex.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with period/point/average/color/paint settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `Period`, `TypePoint`, `TypeCalculationAverage`, `ColorBase`, and `PaintOn`
- Added tests `project/OsEngine.Tests/ForceIndexPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 236/236

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in EfficiencyRatio indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/EfficiencyRatio.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with color/length/paint/average-type settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `ColorBase`, `Length`, `PaintOn`, and `TypeCalculationAverage`
- Added tests `project/OsEngine.Tests/EfficiencyRatioPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 238/238

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Fractal indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Fractail.cs` (`Engine\\<Name>.txt`, class `Fractal`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with paint/colors settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `PaintOn`, `ColorUp`, and `ColorDown`
- Added tests `project/OsEngine.Tests/FractalPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 240/240

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Volume indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Volume.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with up/down colors and paint settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `ColorUp`, `ColorDown`, and `PaintOn`
- Added tests `project/OsEngine.Tests/VolumePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 242/242

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in TickVolume indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/TickVolume.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with up/down colors and paint settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing settings contract for `ColorUp`, `ColorDown`, and `PaintOn`
- Added tests `project/OsEngine.Tests/TickVolumePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 244/244

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in VolumeOscillator indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/VolumeOscillator.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `ColorBase`, `Length1`, `Length2`, and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/VolumeOscillatorPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 246/246

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Roc indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Roc.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `Period`, `TypePoint`, `ColorBase`, and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/RocPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 248/248

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Momentum indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Momentum.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `Nperiod`, `TypePoint`, `ColorBase`, and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/MomentumPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 250/250

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in PriceOscillator indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/PriceOscillator.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `ColorBase`, `PaintOn`, and `TypeSerch`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
  - kept existing behavior for nested moving averages persistence (`<Name>ma1`, `<Name>ma2`)
- Added tests `project/OsEngine.Tests/PriceOscillatorPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use backup/restore for indicator settings file and nested MA settings files

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 252/252

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in OnBalanceVolume indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/OnBalanceVolume.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `ColorBase` and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/OnBalanceVolumePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 254/254

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in WilliamsRange indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/WilliamsRange.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `Nperiod`, `ColorBase`, and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/WilliamsRangePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 256/256

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in StandardDeviation indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/StandardDeviation.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `ColorBase`, `Length`, `PaintOn`, and `TypePointsToSearch`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/StandardDeviationPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 258/258

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in AccumulationDistribution indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/AccumulationDistribution.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `ColorBase` and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/AccumulationDistributionPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 260/260

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Ac indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Ac.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with colors, lengths, paint flag, and average-type settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/AcPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 262/262

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Line indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Line.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `ColorBase` and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/LinePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 264/264

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in VerticalHorizontalFilter indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/VerticalHorizontalFilter.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `Nperiod`, `ColorBase`, and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/VerticalHorizontalFilterPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 266/266

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in SimpleVWAP indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/SimpleVWAP.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `ColorBase` and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/SimpleVwapPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 270/270

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in TradeThread indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/TradeThread.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `ColorBase`, `Length`, `PaintOn`, and `TypePointsToSearch`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/TradeThreadPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 270/270

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in KalmanFilter indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/KalmanFilter.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with color, `Sharpness`, `K`, paint flag, and average-type settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - legacy decimal parser now supports both invariant (`.`) and current-culture decimal formats
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/KalmanFilterPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 272/272

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in AdaptiveLookBack indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/AdaptiveLookBack.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `ColorBase`, `Length`, and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/AdaptiveLookBackPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 274/274

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in MoneyFlowIndex indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/MoneyFlowIndex.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `Nperiod`, `ColorBase`, and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/MoneyFlowIndexPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 276/276

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in MacdLine indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/MacdLine.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `ColorUp`, `ColorDown`, and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
  - kept existing nested moving averages persistence behavior (`<Name>ma1`, `<Name>ma2`, `<Name>maSignal`)
- Added tests `project/OsEngine.Tests/MacdLinePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use backup/restore for indicator settings file and nested MA settings files

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 278/278

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in PriceChannel indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/PriceChannel.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with channel colors, channel lengths, and paint settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/PriceChannelPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 280/280

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Rvi indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Rvi.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `Period`, `ColorUp`, `ColorDown`, and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/RviPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 282/282

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in UltimateOscillator indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/UltimateOscillator.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with color, periods, and paint settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/UltimateOscillatorPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 284/284

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Trix indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Trix.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with period, point type, average type, color, and paint settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/TrixPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 286/286

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in MovingAverage indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/MovingAverage.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with color, length, paint flag, MA type, point type, and Kaufman EMA settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/MovingAveragePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 288/288

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Envelops indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Envelops.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with colors, paint flag, and `Deviation`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - legacy decimal parser supports both invariant (`.`) and current-culture decimal formats
  - preserved compatibility with historical legacy format that can include an extra ignored line
  - kept existing nested moving average persistence behavior (`<Name>maSignal`)
- Added tests `project/OsEngine.Tests/EnvelopsPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use backup/restore for indicator settings file and nested MA settings file

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 290/290

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Adx indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Adx.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `ColorBase`, `Length`, and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/AdxPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 292/292

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in DynamicTrendDetector indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/DynamicTrendDetector.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with color, length, correction coefficient, and paint settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - legacy decimal parser supports both invariant (`.`) and current-culture decimal formats
  - preserved compatibility with historical legacy format that can include an extra ignored line
- Added tests `project/OsEngine.Tests/DynamicTrendDetectorPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 294/294

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in IvashovRange indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/IvashovRange.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with color, MA length, paint flag, average type, and average length
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format where `LengthAverage` may be absent (fallback to `LengthMa`)
- Added tests `project/OsEngine.Tests/IvashovRangePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat_WithOptionalLengthAverageMissing`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 296/296

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Alligator indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Alligator.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with all periods, shifts, colors, paint flag, and average type
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format and field ordering
- Added tests `project/OsEngine.Tests/AlligatorPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 298/298

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in AwesomeOscillator indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/AwesomeOscillator.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with colors, short/long lengths, paint flag, and average type
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy field ordering
- Added tests `project/OsEngine.Tests/AwesomeOscillatorPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 300/300

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in LinearRegressionCurve indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/LinearRegressionCurve.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with color, `Length`, `Lag`, paint flag, and `TypePointsToSearch`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with legacy files that may contain an extra trailing line
- Added tests `project/OsEngine.Tests/LinearRegressionCurvePersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat_WithOptionalTrailingLine`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 302/302

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in ParabolicSaR indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/ParabolicSAR.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `ColorUp`, `ColorDown`, `Af`, `MaxAf`, and paint flag
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with legacy files that may contain an extra trailing line
  - hardened legacy `double` parsing to support both current-culture and invariant formats
- Added tests `project/OsEngine.Tests/ParabolicSarPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat_WithOptionalTrailingLine`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 304/304

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Ichimoku indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Ishimoku.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with lengths, colors, paint flag, and shift settings
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved compatibility with historical legacy format where optional `LengthChinkou` may be absent (fallback to `LengthSdvig`)
- Added tests `project/OsEngine.Tests/IchimokuPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat_WithOptionalLengthChinkouMissing`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 306/306

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in MacdHistogram indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/MacdHistogram.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `ColorUp`, `ColorDown`, and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - kept nested moving average persistence behavior (`<Name>ma1`, `<Name>ma2`, `<Name>maSignal`)
- Added tests `project/OsEngine.Tests/MacdHistogramPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use backup/restore for indicator settings file and nested MA settings files

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 308/308

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in StochasticOscillator indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/StochasticOscillator.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with periods, average type, colors, and paint flag
  - kept existing save-time behavior that normalizes `TypeCalculationAverage` to `Simple`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - legacy parser supports both historical layouts (`type-first` and save-order format with optional blank line)
- Added tests `project/OsEngine.Tests/StochasticOscillatorPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat_WithSaveOrdering`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 310/310

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in StochRsi indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/StochRsi.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with `ColorK`, `RsiLength`, `StochasticLength`, `K`, and `D`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - preserved existing persistence scope (only fields previously written to file)
- Added tests `project/OsEngine.Tests/StochRsiPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 312/312

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Pivot indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/Pivot.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with all pivot/support/resistance colors and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - legacy parser supports historical color representations:
    - numeric `ARGB`
    - `Color [Name]`
    - `Color [A=..., R=..., G=..., B=...]`
- Added tests `project/OsEngine.Tests/PivotPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat_WithColorStrings`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 314/314

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in PivotPoints indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/PivotPoints.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO with pivot/support/resistance colors and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - legacy parser supports historical color representations:
    - numeric `ARGB`
    - `Color [Name]`
    - `Color [A=..., R=..., G=..., B=...]`
- Added tests `project/OsEngine.Tests/PivotPointsPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat_WithColorStrings`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 316/316

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Incremental adoption in Vwap indicator settings

- Migrated `project/OsEngine/Charts/CandleChart/Indicators/VWAP.cs` (`Engine\\<Name>.txt`) persistence to `SettingsManager`:
  - `Save()` now writes JSON DTO for all date/day/week flags, time boundaries, deviation toggles, colors, and `PaintOn`
  - `Load()` now reads JSON and falls back to legacy line-based parser
  - legacy parser includes resilient `DateTime` parsing for current-culture and invariant-culture formats
- Added tests `project/OsEngine.Tests/VwapPersistenceTests.cs`:
  - `Save_ShouldPersistJson_AndLoadRoundTrip`
  - `Load_ShouldSupportLegacyLineBasedFormat`
  - tests use file backup/restore around indicator settings path

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 318/318

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in Envelops and MacdLine constructors

- Updated constructor-level settings-file checks to use `GetSettingsPath()`:
  - `project/OsEngine/Charts/CandleChart/Indicators/Envelops.cs`
  - `project/OsEngine/Charts/CandleChart/Indicators/MacdLine.cs`
- This removes remaining direct `@"Engine\\<Name>.txt"` checks in these indicators and aligns path handling with already migrated `Save/Load/Delete` methods.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 318/318

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in migrated non-indicator classes

- Aligned `Delete()` path handling with existing helper methods in migrated classes:
  - `project/OsEngine/Candles/TimeFrameBuilder.cs` (`GetSettingsPath()`)
  - `project/OsEngine/Alerts/AlertToPrice.cs` (`GetSettingsPath()`)
  - `project/OsEngine/Alerts/AlertToChart.cs` (`GetSettingsPath()`)
  - `project/OsEngine/Logging/MessageSender.cs` (`GetSettingsPath()`)
  - `project/OsEngine/Entity/HorizontalVolume.cs` (`GetSettingsPath()`)
  - `project/OsEngine/Entity/NonTradePeriods.cs` (`GetStoragePath()`)
- Removed remaining direct `@"Engine\\..."` delete calls from these classes; behavior remains unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 318/318

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in BotTabIndex

- Removed remaining direct `@"Engine\\..."` file-path checks in `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs`:
  - tab spread settings delete now uses existing `GetSpreadSettingsPath()`
  - index auto-formula settings existence check in nested builder delete now uses `GetSettingsPath()`
- No behavior changes intended; cleanup aligns path handling with existing helper methods.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 318/318

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in AServer/BotTabPair/BotTabScreener

- Aligned remaining direct `@"Engine\\..."` file-path usage with helper methods:
  - `project/OsEngine/Market/Servers/AServer.cs`:
    - delete cleanup now uses `GetServerParamsPath()`, `GetServerSettingsPath()`, `GetNonTradePeriodsPath()`
  - `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs`:
    - settings/pair names delete/save/load now use helper paths (`GetStandartSettingsPath()`, `GetLegacyStrategSettingsPath()`, `GetPairsNamesToLoadPath()`)
  - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`:
    - tab-set save/load/delete now use `GetScreenerTabSetPath()`
    - settings/indicators delete now use existing helper paths
- No behavior changes intended; cleanup reduces path-fragment duplication and keeps persistence paths centralized.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 318/318

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in BotManualControl

- Updated `project/OsEngine/OsTrader/Panels/Tab/Internal/BotManualControl.cs`:
  - `Delete()` now uses `GetSettingsPath()` instead of rebuilding `@"Engine\\<name>StrategSettings.txt"` inline
  - preserved existing readonly-file handling before deletion
- No behavior changes intended; cleanup keeps persistence path usage centralized.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 318/318

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in ServerMaster instance-number storage

- Updated `project/OsEngine/Market/ServerMaster.cs`:
  - centralized `Engine\\<ServerType>ServerInstanceNumbers.txt` path into helper:
    - `GetServerInstanceNumbersPath(ServerType serverType)`
  - applied helper in `TryLoadServerInstance(...)` and `TrySaveServerInstance(...)`
- No behavior changes intended; path construction is now reused from a single point.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 318/318

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - BotTabPair pair names migrated to JSON settings with legacy compatibility

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs`:
  - migrated pair names persistence from line-based file IO to `SettingsManager`
  - `SavePairNames()` now saves DTO JSON (`BotTabPairNamesToLoadSettingsDto`)
  - `LoadPairs()` now loads via `SettingsManager.Load(..., legacyLoader: ParseLegacyPairNamesToLoadSettings)`
  - added legacy parser `ParseLegacyPairNamesToLoadSettings(string content)` to preserve backward compatibility with old line-based format
- Added tests in `project/OsEngine.Tests/BotTabPairNamesPersistenceTests.cs`:
  - verifies JSON persistence from `SavePairNames()`
  - verifies legacy line-based parser behavior

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 320/320

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - BotTabScreener tab-set migrated to JSON settings with legacy compatibility

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`:
  - migrated tab-set persistence from line-based `#`-delimited file IO to `SettingsManager`
  - `SaveTabs()` now saves DTO JSON (`BotTabScreenerTabSetSettingsDto`)
  - `TryLoadTabs()` now loads via `SettingsManager.Load(..., legacyLoader: ParseLegacyScreenerTabSetSettings)`
  - added legacy parser `ParseLegacyScreenerTabSetSettings(string content)` for old `TAB1#TAB2#...` format
- Added tests in `project/OsEngine.Tests/BotTabScreenerTabSetPersistenceTests.cs`:
  - verifies JSON persistence from `SaveTabs()`
  - verifies legacy hash-separated parser behavior

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 322/322

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - BotTabPolygon sequence names migrated to JSON settings with legacy compatibility

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`:
  - migrated sequence names persistence from line-based file IO to `SettingsManager`
  - `SaveSequencesNames()` now saves DTO JSON (`BotTabPolygonNamesToLoadSettingsDto`)
  - `LoadSequences()` now loads via `SettingsManager.Load(..., legacyLoader: ParseLegacyPolygonNamesToLoadSettings)`
  - added path helper `GetPolygonsNamesToLoadPath()` and applied it in save/load/delete sites
  - added legacy parser `ParseLegacyPolygonNamesToLoadSettings(string content)` for old line-based format
- Added tests in `project/OsEngine.Tests/BotTabPolygonNamesPersistenceTests.cs`:
  - verifies JSON persistence from `SaveSequencesNames()`
  - verifies legacy line-based parser behavior

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 324/324

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - BotTabPolygon standard settings migrated to JSON with legacy compatibility

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`:
  - migrated `SaveStandartSettings()` / `LoadStandartSettings()` from line-based file IO to `SettingsManager`
  - added legacy parser `ParseLegacyStandartPolygonSettings(string content)` for old ordered-line format
  - introduced DTO `BotTabPolygonStandartSettingsDto` for typed settings persistence
  - centralized standard settings file path via `GetStandartPolygonSettingsPath()` and reused it in save/load/delete sites
- Added tests in `project/OsEngine.Tests/BotTabPolygonStandartSettingsPersistenceTests.cs`:
  - verifies JSON save and load roundtrip for standard polygon settings
  - verifies loading legacy line-based format

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 326/326

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - PolygonToTrade settings migrated to JSON with legacy compatibility

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs` (`PolygonToTrade`):
  - migrated `Save()` / `Load()` from line-based file IO to `SettingsManager`
  - added legacy parser `ParseLegacyPolygonToTradeSettings(string content)` for old ordered-line format
  - introduced DTO `PolygonToTradeSettingsDto` for typed settings persistence
  - centralized settings path via `GetSettingsPath()` and reused it in load/save/delete
- Added tests in `project/OsEngine.Tests/PolygonToTradeSettingsPersistenceTests.cs`:
  - verifies JSON save and load roundtrip for `PolygonToTrade` settings
  - verifies loading legacy line-based format

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 328/328

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - TradeGridsMaster settings migrated to JSON with legacy compatibility

- Updated `project/OsEngine/OsTrader/Grids/TradeGridsMaster.cs`:
  - migrated grid-settings persistence from line-based file IO to `SettingsManager`
  - `SaveGrids()` now persists DTO JSON (`TradeGridsMasterSettingsDto`)
  - `LoadGrids()` now loads via `SettingsManager.Load(..., legacyLoader: ParseLegacyGridsSettings)`
  - added helper `GetGridsSettingsPath()` and applied it in save/load/delete sites
  - added legacy parser `ParseLegacyGridsSettings(string content)` for old line-based format
- Added tests in `project/OsEngine.Tests/TradeGridsMasterPersistenceTests.cs`:
  - verifies JSON persistence from `SaveGrids()`
  - verifies legacy line-based parser behavior

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 330/330

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in BotTabPolygon/BotTabSimple delete flows

- Updated remaining direct `@"Engine\\..."` delete checks in tab classes:
  - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`
    - added and used `GetLegacyStrategSettingsPath()` for legacy `StrategSettings.txt`
  - `project/OsEngine/OsTrader/Panels/Tab/BotTabSimple.cs`
    - added and used `GetSettingsBotPath()` for `SettingsBot.txt`
- No behavior changes intended; cleanup centralizes file path construction and removes last inline delete paths in these tab files.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 330/330

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - ChartCandleMaster settings storage migrated to JSON wrapper with legacy compatibility

- Updated `project/OsEngine/Charts/CandleChart/ChartCandleMaster.cs`:
  - migrated chart master settings file read/write wrapper from direct line-based IO to `SettingsManager`
  - preserved existing indicator reconstruction logic; now it consumes loaded `Lines` from DTO
  - `Save()` now persists DTO JSON (`ChartCandleMasterSettingsDto`) containing ordered settings lines
  - `Load()` now uses `SettingsManager.Load(..., legacyLoader: ParseLegacySettings)`
  - centralized path usage via `GetSettingsPath()` and reused it in load/save/delete
  - added legacy parser `ParseLegacySettings(string content)` for old line-based format
- Added tests in `project/OsEngine.Tests/ChartCandleMasterSettingsPersistenceTests.cs`:
  - verifies JSON persistence from private `Save()` path
  - verifies legacy parser behavior

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 332/332

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Aindicator parameters/series storage migrated to JSON wrappers with legacy compatibility

- Updated `project/OsEngine/Indicators/Aindicator.cs`:
  - migrated parameters and series settings file IO wrappers to `SettingsManager`
  - `SaveParameters()` / `GetValueParameterSaveByUser(...)` now persist/load DTO JSON for `Parametrs.txt`
  - `SaveSeries()` / `CheckSeriesParametersInSaveData(...)` now persist/load DTO JSON for `Values.txt`
  - added shared legacy parser `ParseLegacyLinesSettings(string content)` for old line-based formats
  - centralized file paths via helper methods:
    - `GetParametersPath()`
    - `GetValuesPath()`
    - `GetBasePath()`
  - applied helpers in delete flow as well
- Added tests in `project/OsEngine.Tests/AindicatorPersistenceTests.cs`:
  - verifies JSON persistence for parameters and series through `Save()`
  - verifies legacy parser behavior

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 334/334

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - AlertMaster keeper storage migrated to JSON wrapper with legacy compatibility

- Updated `project/OsEngine/Alerts/AlertMaster.cs`:
  - migrated alert-keeper file wrapper from direct line-based IO to `SettingsManager`
  - `Save()` now persists DTO JSON (`AlertKeeperSettingsDto`) with ordered alert names
  - `Load()` now uses `SettingsManager.Load(..., legacyLoader: ParseLegacyAlertKeeperSettings)`
  - preserved existing alert reconstruction logic (`AlertToChart` / `AlertToPrice`) from keeper names
  - centralized keeper path via `GetAlertKeeperPath()` and reused it in load/save/delete
  - added legacy parser `ParseLegacyAlertKeeperSettings(string content)` for old line-based format
- Added tests in `project/OsEngine.Tests/AlertMasterPersistenceTests.cs`:
  - verifies JSON persistence from private `Save()` path
  - verifies legacy parser behavior

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 336/336

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in BotPanel strategy parameters storage

- Updated `project/OsEngine/OsTrader/Panels/BotPanel.cs`:
  - centralized strategy parameters file path into `GetParametersPath()`
  - replaced remaining inline `@"Engine\\<NameStrategyUniq>Parametrs.txt"` usage in:
    - delete flow
    - parameter load (`GetValueParameterSaveByUser`)
    - parameter save (`SaveParameters`)
- No behavior changes intended; this cleanup keeps path construction in one place for further persistence migration steps.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 336/336

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - OsTraderMaster keeper storage migrated to JSON wrapper with legacy compatibility

- Updated `project/OsEngine/OsTrader/OsTraderMaster.cs`:
  - migrated bot keeper settings wrapper from direct line-based IO to `SettingsManager`
  - `Load()` now consumes keeper entries loaded via `SettingsManager.Load(..., legacyLoader: ParseLegacyBotKeeperSettings)`
  - `Save()` now persists DTO JSON (`BotKeeperSettingsDto`) with bot settings entries
  - updated `BotNames` helper logic to read both `SettingsRealKeeper.txt` and `SettingsTesterKeeper.txt` through the same compatibility loader
  - centralized keeper paths via `GetSettingsKeeperPath(ConnectorWorkType connectorWorkType)`
  - added legacy parser `ParseLegacyBotKeeperSettings(string content)` for old line-based format
- Added tests in `project/OsEngine.Tests/OsTraderMasterKeeperSettingsTests.cs`:
  - verifies legacy parser behavior

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 337/337

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - BotPanelChartUi layout storage migrated to JSON wrapper with legacy compatibility

- Updated `project/OsEngine/OsTrader/Panels/BotPanelChartUI.xaml.cs`:
  - migrated layout settings wrapper (`LayoutRobotUi*.txt`) from direct line-based IO to `SettingsManager`
  - `SaveLeftPanelPosition()` now persists DTO JSON (`BotPanelChartLayoutSettingsDto`)
  - `CheckPanels()` now loads via `SettingsManager.Load(..., legacyLoader: ParseLegacyLayoutSettings)`
  - centralized path usage via `GetLayoutSettingsPath()`
  - added legacy parser `ParseLegacyLayoutSettings(string content)` for old 3-line bool format
- Added tests in `project/OsEngine.Tests/BotPanelChartUILayoutPersistenceTests.cs`:
  - verifies JSON persistence from private `SaveLeftPanelPosition()` path
  - verifies legacy parser behavior

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 339/339

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - JournalUi2 layout storage migrated to JSON wrapper with legacy compatibility

- Updated `project/OsEngine/Journal/JournalUi2.xaml.cs`:
  - migrated layout settings wrapper (`LayoutJournal*.txt`) from direct line-based IO to `SettingsManager`
  - `SaveSettings()` now persists DTO JSON (`JournalUiLayoutSettingsDto`)
  - `LoadSettings()` now uses `SettingsManager.Load(..., legacyLoader: ParseLegacyLayoutSettings)`
  - centralized path usage via `GetLayoutSettingsPath()`
  - added legacy parser `ParseLegacyLayoutSettings(string content)` for old ordered-line format
- Added tests in `project/OsEngine.Tests/JournalUi2LayoutSettingsTests.cs`:
  - verifies legacy parser behavior

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 340/340

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - JournalUi2 groups storage migrated to JSON wrapper with legacy compatibility

- Updated `project/OsEngine/Journal/JournalUi2.xaml.cs`:
  - migrated groups settings wrapper (`<StartProgram>JournalSettings.txt`) from direct line-based IO to `SettingsManager`
  - `SaveGroups()` now persists typed DTO JSON (`JournalGroupsSettingsDto`)
  - `LoadGroups()` now uses `SettingsManager.Load(..., legacyLoader: ParseLegacyJournalGroupsSettings)`
  - centralized groups path usage via `GetJournalGroupsSettingsPath()`
  - added legacy parser `ParseLegacyJournalGroupsSettings(string content)` for old `BotName&BotGroup&Mult&IsOn` format
- Added tests in `project/OsEngine.Tests/JournalUi2GroupsSettingsTests.cs`:
  - verifies legacy parser behavior

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 341/341

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - OptimizerMaster standard parameters storage migrated to JSON wrappers with legacy compatibility

- Updated `project/OsEngine/OsOptimizer/OptimizerMaster.cs`:
  - migrated standard optimizer parameters wrappers from direct line-based IO to `SettingsManager`
  - `SaveStandardParameters()` / `GetValueParameterSaveByUser(...)` now persist/load DTO JSON for:
    - `<StrategyName>_StandartOptimizerParameters.txt`
  - `SaveParametersOnOffByStrategy()` / `GetParametersOnOffByStrategy()` now persist/load DTO JSON for:
    - `<StrategyName>_StandartOptimizerParametersOnOff.txt`
  - centralized file paths via:
    - `GetStandardParametersPath()`
    - `GetStandardParametersOnOffPath()`
  - added legacy parsers:
    - `ParseLegacyStandardParameters(string content)`
    - `ParseLegacyStandardParametersOnOff(string content)`
- Added tests in `project/OsEngine.Tests/OptimizerMasterPersistenceTests.cs`:
  - verifies both legacy parser behaviors

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in MainWindow directory check flow

- Updated `project/OsEngine/MainWindow.xaml.cs`:
  - centralized `Engine\\checkFile.txt` path into helper `GetDirectoryCheckFilePath()`
  - replaced remaining inline path usage in `CheckWorkWithDirectory()`
- No behavior changes intended; cleanup keeps path construction in one place.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in YahooServer securities cache flow

- Updated `project/OsEngine/Market/Servers/YahooFinance/YahooServer.cs`:
  - centralized Yahoo securities cache paths into helper methods:
    - `GetYahooSecuritiesPath()`
    - `GetYahooSecuritiesFtpPath()`
  - replaced remaining inline path literals in local file read and FTP download call
- No behavior changes intended; cleanup removes path duplication and keeps cache-path constants in one place.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in PositionController delete flow

- Updated `project/OsEngine/Journal/Internal/PositionController.cs`:
  - `Delete()` now reuses existing helper paths:
    - `GetDealsPath()`
    - `GetStopLimitsPath()`
  - removed remaining inline `@"Engine\\<name>DealController*.txt"` path construction in delete cleanup
- No behavior changes intended; cleanup aligns delete path usage with already migrated save/load methods.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in OptimizerSettings storage paths

- Updated `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`:
  - centralized optimizer storage file paths into helper methods:
    - `GetClearingsPath()`
    - `GetNonTradePeriodsPath()`
    - `GetSettingsPath()`
  - replaced remaining inline path literals in clearing/non-trade/settings save/load flows
- No behavior changes intended; cleanup keeps optimizer persistence paths maintained in one place.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in PolygonServer securities cache flow

- Updated `project/OsEngine/Market/Servers/Polygon/PolygonServer.cs`:
  - centralized Polygon securities cache path into helper `GetSecuritiesCachePath()`
  - replaced remaining inline `Engine\\PolygonSecurities.csv` path usage in read/write flows
- No behavior changes intended; cleanup removes path duplication and keeps cache path maintenance localized.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in Log daily-file flow

- Updated `project/OsEngine/Logging/Log.cs`:
  - centralized log storage paths via helper methods:
    - `GetLogsDirectoryPath()`
    - `GetCurrentDayLogPath()`
  - replaced remaining inline `Engine\\Log\\...` path construction in:
    - show-file action
    - delete cleanup
    - save thread
    - load-last-day flow
- No behavior changes intended; cleanup keeps daily log path construction centralized.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in SystemAnalyze settings directory usage

- Updated `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs`:
  - removed hardcoded `Engine\\SystemStress` directory checks from 4 `Save()` methods:
    - `RamMemoryUsageAnalyze`
    - `CpuUsageAnalyze`
    - `EcqUsageAnalyze`
    - `MoqUsageAnalyze`
  - directory path is now resolved from `GetSettingsPath()` via `Path.GetDirectoryName(...)`
- No behavior changes intended; directory creation remains identical but path construction is centralized via existing settings-file path helpers.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in AServerOrdersHub active orders DB path

- Updated `project/OsEngine/Market/Servers/AServerOrdersHub.cs`:
  - centralized active-orders LiteDB path construction via helper methods:
    - `GetDataBasesDirectoryPath()`
    - `GetActiveOrdersDatabasePath()`
  - replaced duplicated inline path-building in:
    - `LoadOrdersFromFile()`
    - `SaveOrdersInFile()`
- No behavior changes intended; DB directory create-if-missing and file naming remain unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in MoexFixFastSpot securities DB path

- Updated `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`:
  - centralized securities LiteDB path construction via helper methods:
    - `GetDataBasesDirectoryPath()`
    - `GetSecuritiesDatabasePath()`
  - replaced duplicated inline path-building in:
    - `LoadSecuritiesFromFile()`
    - `SaveSecuritiesToFile()`
- No behavior changes intended; DB directory create-if-missing and file name remain unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in ChartMasterColorKeeper storage paths

- Updated `project/OsEngine/Charts/ColorKeeper/ChartMasterColorKeeper.cs`:
  - centralized settings directory path via:
    - `GetSettingsDirectoryPath()`
  - updated:
    - `GetSettingsPath()` to build path from directory helper
    - `EnsureDirectoryExists()` to use same directory helper
- No behavior changes intended; settings are still stored in `Engine\\Color`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in BotFactory optimizer-bots storage path

- Updated `project/OsEngine/Robots/BotFactory.cs`:
  - replaced hardcoded `OptimizerBotsFileName` constant usage with helper:
    - `GetOptimizerBotsFilePath()`
  - updated path usage in:
    - `LoadOptimizerBotsNamesFromFile()`
    - `SaveOptimizerBotsNamesToFile()`
  - save-path directory extraction now derives from helper path.
- No behavior changes intended; storage file remains `Engine\\OptimizerBots.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in AServer ServerDopSettings paths

- Updated `project/OsEngine/Market/Servers/AServer.cs`:
  - centralized ServerDopSettings path construction via helper methods:
    - `GetServerDopSettingsDirectoryPath()`
    - `GetServerDopSettingsDirectoryPathForCurrentServerType()`
    - `GetSecuritiesLeveragePath()`
  - replaced duplicated inline path usage in:
    - `LoadSavedSecurities()`
    - `LoadLeverageFromFile()`
    - `SaveLeverageToFile()`
- No behavior changes intended; file/dir locations remain within `Engine\\ServerDopSettings`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in MainWindow executable path usage

- Updated `project/OsEngine/MainWindow.xaml.cs`:
  - centralized current-directory executable path via helper:
    - `GetCurrentDirectoryExecutablePath()`
  - replaced remaining inline `...\\OsEngine.exe` path construction in:
    - single-instance check flow
    - reboot flow
- No behavior changes intended; executable path still resolves to current working directory.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in SecuritiesUi server dop-settings paths

- Updated `project/OsEngine/Entity/SecuritiesUi.xaml.cs`:
  - centralized ServerDopSettings path construction via helper methods:
    - `GetServerDopSettingsDirectoryPath()`
    - `GetServerTypeDopSettingsDirectoryPath()`
    - `GetSecurityDopSettingsFilePath(string fileName)`
  - replaced duplicated inline path usage in security override save flow.
- No behavior changes intended; files are still saved under `Engine\\ServerDopSettings\\<ServerType>\\*.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in SetLeverageUi leverage file path

- Updated `project/OsEngine/Entity/SetLeverageUi.xaml.cs`:
  - centralized leverage settings file path via helper:
    - `GetSecuritiesLeveragePath()`
  - replaced inline `Engine\\ServerDopSettings\\...json` path construction in `LoadLeverageFromFile()`.
- No behavior changes intended; file location remains `Engine\\ServerDopSettings\\<ServerNameUnique>_SecuritiesLeverage.json`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in AstsBridgeServer settings path

- Updated `project/OsEngine/Market/Servers/AstsBridge/AstsBridgeServer.cs`:
  - centralized settings file path via helper:
    - `GetSettingsPath()`
  - replaced duplicated inline `Engine\\AstsServer.txt` path usage in:
    - `Load()`
    - `Save()`
- No behavior changes intended; settings file location remains unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in TelegramNewsServer log/session paths

- Updated `project/OsEngine/Market/Servers/TelegramNews/TelegramNewsServer.cs`:
  - centralized Telegram log/session paths via helper methods:
    - `GetTelegramLogsDirectoryPath()`
    - `GetTelegramLogFilePath()`
    - `GetTelegramSessionPath()`
  - replaced duplicated inline path usage in:
    - constructor log setup
    - `Config("session_pathname")`
- No behavior changes intended; paths remain under `Engine\\Log\\TelegramLogs`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in AlgoStart2Soldiers settings path

- Updated `project/OsEngine/Robots/AlgoStart/AlgoStart2Soldiers.cs`:
  - centralized bot settings file path via helper:
    - `GetTradeSettingsPath()`
  - replaced duplicated inline path usage in:
    - `SaveTradeSettings()`
    - `LoadTradeSettings()`
    - `AlgoStart2ScreenerSoldiers_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in StrategyBollinger settings path

- Updated `project/OsEngine/Robots/CounterTrend/StrategyBollinger.cs`:
  - centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in RsiContrtrend settings path

- Updated `project/OsEngine/Robots/CounterTrend/RsiContrtrend.cs`:
  - centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in WilliamsRangeTrade settings path

- Updated `project/OsEngine/Robots/CounterTrend/WilliamsRangeTrade.cs`:
  - centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in SmaStochastic settings path

- Updated `project/OsEngine/Robots/Trend/SmaStochastic.cs`:
  - centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in WsurfBot settings path

- Updated `project/OsEngine/Robots/Trend/WsurfBot.cs`:
  - centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in PriceChannelTrade settings path

- Updated `project/OsEngine/Robots/Trend/PriceChannelTrade.cs`:
  - centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in PairTraderSimple settings path

- Updated `project/OsEngine/Robots/MarketMaker/PairTraderSimple.cs`:
  - centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in PairTraderSpreadSma settings path

- Updated `project/OsEngine/Robots/MarketMaker/PairTraderSpreadSma.cs`:
  - centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in MarketMakerBot settings path

- Updated `project/OsEngine/Robots/MarketMaker/MarketMakerBot.cs`:
  - centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in GridScreenerAdaptiveSoldiers settings path

- Updated `project/OsEngine/Robots/Grids/GridScreenerAdaptiveSoldiers.cs`:
  - centralized bot settings file path via helper:
    - `GetTradeSettingsPath()`
  - replaced duplicated inline path usage in:
    - `SaveTradeSettings()`
    - `LoadTradeSettings()`
    - `ThreeSoldierAdaptiveScreener_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in PinBarVolatilityScreener settings path

- Updated `project/OsEngine/Robots/Screeners/PinBarVolatilityScreener.cs`:
  - centralized bot settings file path via helper:
    - `GetTradeSettingsPath()`
  - replaced duplicated inline path usage in:
    - `SaveTradeSettings()`
    - `LoadTradeSettings()`
    - `Screener_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in ThreeSoldierAdaptiveScreener settings path

- Updated `project/OsEngine/Robots/Screeners/ThreeSoldierAdaptiveScreener.cs`:
  - centralized bot settings file path via helper:
    - `GetTradeSettingsPath()`
  - replaced duplicated inline path usage in:
    - `SaveTradeSettings()`
    - `LoadTradeSettings()`
    - `ThreeSoldierAdaptiveScreener_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in PivotPointsRobot settings path

- Updated `project/OsEngine/Robots/Patterns/PivotPointsRobot.cs`:
  - centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in RsiTrade delete-settings path

- Updated `project/OsEngine/Robots/OnScriptIndicators/RsiTrade.cs`:
  - centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - replaced duplicated inline path usage in:
    - `Strategy_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in VisualSettingsParametersExample delete-settings path

- Updated `project/OsEngine/Robots/TechSamples/VisualSettingsParametersExample.cs`:
  - centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - replaced duplicated inline path usage in:
    - `Strategy_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in MomentumMacd delete-settings path

- Updated `project/OsEngine/Robots/Trend/MomentumMacd.cs`:
  - centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - replaced duplicated inline path usage in:
    - `Strategy_DeleteEvent()`
- No behavior changes intended; settings file remains `Engine\\<NameStrategyUniq>SettingsBot.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in MoexFixFastCurrency log paths

- Updated `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`:
  - centralized log directory and file path construction via helpers:
    - `GetLogDirectoryPath()`
    - `GetTradesLogPath()`
    - `GetOrdersLogPath()`
    - `GetIncomingMfixLogPath()`
    - `GetRecoveryLogPath()`
  - replaced duplicated inline log file path construction for:
    - trades log
    - orders log
    - incoming MFIX log
    - recovery log
- No behavior changes intended; log files remain under `Engine\\Log`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in MoexFixFastSpot log paths

- Updated `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`:
  - centralized log directory and file path construction via helpers:
    - `GetLogDirectoryPath()`
    - `GetUdpLogPath()`
    - `GetXOrdersLogPath()`
    - `GetMfixLogPath()`
  - replaced duplicated inline log file path construction for:
    - UDP log
    - XOrders log
    - MFIX log
- No behavior changes intended; log files remain under `Engine\\Log`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in CustomTableInParamWindowSample lines path

- Updated `project/OsEngine/Robots/TechSamples/CustomTableInTheParamWindowSample.cs`:
  - centralized lines-storage file path via helper:
    - `GetLinesPath()`
  - replaced duplicated inline path usage in:
    - `DeleteBotEvent()`
    - `SaveLines()`
    - `LoadLines()`
- No behavior changes intended; lines file remains `Engine\\<NameStrategyUniq>Lines.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-16 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in MoexFixFastTwimeFutures log paths

- Updated `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`:
  - centralized connector log directory via:
    - `MoexFixFastTwimeFuturesServer.GetConnectorLogDirectoryPath()`
  - replaced duplicated inline log file path construction for:
    - trades log
    - orders log
    - trading server log
    - recovery log
  - constructor directory-create check now uses centralized helper path.
- No behavior changes intended; log files remain in `Engine\\Log\\MoexFixFastTwimeConnectorLogs`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in PayOfMarginBot table paths

- Updated `project/OsEngine/Robots/Helpers/PayOfMarginBot.cs`:
  - centralized table-storage file paths via helpers:
    - `GetTableSummPath()`
    - `GetTablePeriodPath()`
  - replaced duplicated inline path usage in:
    - `LoadTableSumm()`
    - `SaveTableSumm()`
    - `LoadTable()`
    - `SaveTable()`
- No behavior changes intended; files remain `Engine\\<NameStrategyUniq>TableSumm.json` and `Engine\\<NameStrategyUniq>TablePeriod.json`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in TaxPayer table-period path

- Updated `project/OsEngine/Robots/Helpers/TaxPayer.cs`:
  - centralized table-period file path via helper:
    - `GetTablePeriodPath()`
  - replaced duplicated inline path usage in:
    - `LoadTable()`
    - `SaveTable()`
- No behavior changes intended; file remains `Engine\\<NameStrategyUniq>TablePeriod.json`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in Aindicator storage prefix

- Updated `project/OsEngine/Indicators/Aindicator.cs`:
  - centralized indicator storage path prefix via helper:
    - `GetIndicatorStoragePrefix()`
  - replaced duplicated path-prefix usage in:
    - `GetParametersPath()`
    - `GetValuesPath()`
    - `GetBasePath()`
- No behavior changes intended; files remain `Engine\\<Name>Parametrs.txt`, `Engine\\<Name>Values.txt`, and `Engine\\<Name>Base.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in BotTabPair tab-settings prefix

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs`:
  - centralized BotTabPair tab-settings path prefix via helper:
    - `GetTabStoragePrefix()`
  - replaced duplicated path-prefix usage in:
    - `GetStandartSettingsPath()`
    - `GetLegacyStrategSettingsPath()`
    - `GetPairsNamesToLoadPath()`
- No behavior changes intended; files remain under `Engine\\<TabName>...`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in BotTabPolygon tab-settings prefix

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`:
  - centralized BotTabPolygon tab-settings path prefix via helper:
    - `GetTabStoragePrefix()`
  - replaced duplicated path-prefix usage in:
    - `GetStandartPolygonSettingsPath()`
    - `GetLegacyStrategSettingsPath()`
    - `GetPolygonsNamesToLoadPath()`
- No behavior changes intended; files remain under `Engine\\<TabName>...`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in BotTabScreener storage prefix

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`:
  - centralized screener storage path prefix via helper:
    - `GetScreenerStoragePrefix()`
  - replaced duplicated path-prefix usage in:
    - `GetScreenerSettingsPath()`
    - `GetScreenerTabSetPath()`
    - `GetIndicatorsPath()`
- No behavior changes intended; files remain under `Engine\\<TabName>...`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in AServer storage prefix

- Updated `project/OsEngine/Market/Servers/AServer.cs`:
  - centralized server-storage path prefix via helper:
    - `GetServerStoragePrefix()`
  - replaced duplicated path-prefix usage in:
    - `GetServerParamsPath()`
    - `GetServerSettingsPath()`
    - `GetNonTradePeriodsPath()`
- No behavior changes intended; files remain under `Engine\\<ServerNameUnique>...`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in ComparePositionsModule settings prefix

- Updated `project/OsEngine/Market/Servers/ComparePositionsModule.cs`:
  - centralized compare-module settings path prefix via helper:
    - `GetSettingsPathPrefix()`
  - replaced duplicated path-prefix usage in:
    - `GetSettingsPath()`
    - `GetIgnoredSettingsPath()`
- No behavior changes intended; files remain under `Engine\\<ServerNameUnique>...`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in PositionController storage prefix

- Updated `project/OsEngine/Journal/Internal/PositionController.cs`:
  - centralized position-controller storage path prefix via helper:
    - `GetStoragePathPrefix()`
  - replaced duplicated path-prefix usage in:
    - `GetDealsPath()`
    - `GetStopLimitsPath()`
- No behavior changes intended; files remain under `Engine\\<_name>...`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in OptimizerMaster standard-parameters prefix

- Updated `project/OsEngine/OsOptimizer/OptimizerMaster.cs`:
  - centralized optimizer standard-parameters path prefix via helper:
    - `GetStandardParametersPathPrefix()`
  - replaced duplicated path-prefix usage in:
    - `GetStandardParametersPath()`
    - `GetStandardParametersOnOffPath()`
- No behavior changes intended; files remain under `Engine\\<StrategyName>_StandartOptimizerParameters*.txt`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in PayOfMarginBot storage prefix

- Updated `project/OsEngine/Robots/Helpers/PayOfMarginBot.cs`:
  - centralized PayOfMarginBot storage path prefix via helper:
    - `GetStoragePathPrefix()`
  - replaced duplicated path-prefix usage in:
    - `GetTableSummPath()`
    - `GetTablePeriodPath()`
- No behavior changes intended; files remain `Engine\\<NameStrategyUniq>TableSumm.json` and `Engine\\<NameStrategyUniq>TablePeriod.json`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in SystemAnalyze settings directory paths

- Updated `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs`:
  - centralized SystemAnalyze settings directory path via helper:
    - `SystemUsageAnalyzePaths.GetSettingsPath(string fileName)`
  - replaced duplicated direct path usage in:
    - `RamMemoryUsageAnalyze.GetSettingsPath()`
    - `CpuUsageAnalyze.GetSettingsPath()`
    - `EcqUsageAnalyze.GetSettingsPath()`
    - `MoqUsageAnalyze.GetSettingsPath()`
- No behavior changes intended; files remain under `Engine\\SystemStress\\`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in BotTabIndex storage path builder

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs`:
  - centralized Engine storage-path construction via helper:
    - `BotTabIndexPaths.BuildEnginePath(string uniqueName, string fileName)`
  - replaced direct path construction in:
    - `GetSpreadSettingsPath()`
    - `IndexFormulaBuilder.GetSettingsPath()`
- No behavior changes intended; files remain under `Engine\\<uniqueName>...`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in BotTabPair storage path builder

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs`:
  - centralized Engine storage-path construction via helper:
    - `BotTabPairPaths.BuildEnginePath(string uniqueName, string fileName)`
  - replaced direct/prefix path construction in:
    - `GetTabStoragePrefix()`
    - `PairToTrade.GetPairSettingsPath()`
- No behavior changes intended; files remain under `Engine\\<uniqueName>...`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in BotTabPolygon storage path builder

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`:
  - centralized Engine storage-path construction via helper:
    - `BotTabPolygonPaths.BuildEnginePath(string uniqueName, string fileName)`
  - replaced direct/prefix path construction in:
    - `GetTabStoragePrefix()`
    - `PolygonToTrade.GetSettingsPath()`
- No behavior changes intended; files remain under `Engine\\<uniqueName>...`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in TesterServer test-settings paths

- Updated `project/OsEngine/Market/Servers/Tester/TesterServer.cs`:
  - centralized tester settings file path construction via helper:
    - `GetTesterSettingsFilePath(string suffix)`
  - replaced direct path usage in:
    - `GetTesterSettingsPath()`
    - `GetClearingSettingsPath()`
    - `GetNonTradePeriodsPath()`
- No behavior changes intended; files remain:
  - `Engine\\TestServer.txt`
  - `Engine\\TestServerClearings.txt`
  - `Engine\\TestServerNonTradePeriods.txt`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in OptimizerSettings paths

- Updated `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`:
  - centralized optimizer settings file path construction via helper:
    - `GetOptimizerSettingsFilePath(string suffix)`
  - replaced direct path usage in:
    - `GetClearingsPath()`
    - `GetNonTradePeriodsPath()`
    - `GetSettingsPath()`
- No behavior changes intended; files remain:
  - `Engine\\OptimizerMasterClearings.txt`
  - `Engine\\OptimizerMasterNonTradePeriods.txt`
  - `Engine\\OptimizerSettings.txt`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in GlobalGUILayout engine-file paths

- Updated `project/OsEngine/Layout/GlobalGUILayout.cs`:
  - centralized layout-related engine file path construction via helper:
    - `GetLayoutFilePath(string fileName)`
  - replaced direct path usage in:
    - `GetLayoutSettingsPath()`
    - `GetScreenResolutionPath()`
- No behavior changes intended; files remain:
  - `Engine\\LayoutGui.txt`
  - `Engine\\ScreenResolution.txt`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in BlockMaster prime-settings paths

- Updated `project/OsEngine/OsTrader/Gui/BlockInterface/BlockMaster.cs`:
  - centralized prime-settings file path construction via helper:
    - `GetPrimeSettingsPath(string fileName)`
  - replaced direct path usage in:
    - `GetPasswordPath()`
    - `GetIsBlockedPath()`
- No behavior changes intended; files remain:
  - `Engine\\PrimeSettingss.txt`
  - `Engine\\PrimeSettingsss.txt`

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in JournalUi2 engine-path construction

- Updated `project/OsEngine/Journal/JournalUi2.xaml.cs`:
  - centralized journal UI engine path construction via helper:
    - `BuildEnginePath(string fileName)`
  - replaced direct path construction in:
    - `GetLayoutSettingsPath()`
    - `GetJournalGroupsSettingsPath()`
- No behavior changes intended; files remain under `Engine\\...`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 2.3 (JSON settings subsystem) - Path consistency cleanup in AServer shared engine-path helper

- Updated `project/OsEngine/Market/Servers/AServer.cs`:
  - centralized generic Engine path construction via helper:
    - `BuildEnginePath(string fileOrFolderName)`
  - replaced direct Engine-prefix usage in:
    - `GetServerStoragePrefix()`
    - `GetServerDopSettingsDirectoryPath()`
- No behavior changes intended; paths remain under `Engine\\...`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Logging in OptimizerDataStorage catch blocks

- Updated `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorage.cs`:
  - replaced silent exception swallowing with explicit error logging in 4 places:
    - security loading catch in candle-based scan
    - security loading catch in trades-based scan
    - `LoadSecurityDopSettings(...)`
    - `SaveSecurityDopSettings(...)`
  - behavior preserved: methods still continue/return as before (no rethrow added).

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Trace logging in GlobalGUILayout catch blocks

- Updated `project/OsEngine/Layout/GlobalGUILayout.cs`:
  - added `System.Diagnostics.Trace` warnings in silent catch blocks where no local logging bus is available:
    - `Save()`
    - `Load()`
    - `SaveResolution(...)`
  - behavior preserved: methods still swallow exceptions and continue flow.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Trace logging in PrimeSettingsMaster catch blocks

- Updated `project/OsEngine/PrimeSettings/PrimeSettingsMaster.cs`:
  - added `System.Diagnostics.Trace` warnings in silent catch blocks:
    - `Save()`
    - `Load()`
  - preserved existing fallback behavior in `Load()` (`_reportCriticalErrors = true` on failure).

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Trace logging in MessageSender catch blocks

- Updated `project/OsEngine/Logging/MessageSender.cs`:
  - added `System.Diagnostics.Trace` warnings in silent catch blocks:
    - `ApplySettings(...)`
    - `Save()`
  - behavior preserved: exceptions are still swallowed (no rethrow).

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error-event logging in ChartClusterMaster catch blocks

- Updated `project/OsEngine/Charts/ClusterChart/ChartClusterMaster.cs`:
  - replaced silent catch placeholders with existing error reporting call:
    - `SendErrorMessage(error)`
  - applied in:
    - `Save()`
    - `Load()`
  - behavior preserved: exceptions are still swallowed (no rethrow).

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in OptimizerMaster catch blocks

- Updated `project/OsEngine/OsOptimizer/OptimizerMaster.cs`:
  - replaced silent catch blocks with explicit optimizer error logging via:
    - `SendLogMessage(ex.ToString(), LogMessageType.Error)`
  - applied in:
    - standard parameter load
    - standard parameter save
    - parameters-on/off load
    - parameters-on/off save
  - behavior preserved: no exception rethrow added.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotTabIndex IndexFormulaBuilder catch blocks

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs`:
  - replaced silent catch blocks with explicit error logging via:
    - `SendNewLogMessage(ex.ToString(), LogMessageType.Error)`
  - applied in `IndexFormulaBuilder`:
    - settings `Load()`
    - settings `Save()`
  - behavior preserved: no exception rethrow added.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotTabIndex spread-settings catch blocks

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs`:
  - replaced silent catch blocks with explicit error logging via:
    - `SendNewLogMessage(ex.ToString(), LogMessageType.Error)`
  - applied in main `BotTabIndex`:
    - spread settings `Save()`
    - spread settings `Load()`
  - preserved existing fallback behavior for flags/defaults in catch blocks.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotTabScreener catch blocks

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`:
  - replaced silent catch blocks with explicit error logging via:
    - `SendNewLogMessage(ex.ToString(), LogMessageType.Error)`
  - applied in:
    - `SaveTabs()`
    - `SaveSettings()`
    - `LoadSettings()` (with preserved fallback defaults)
    - `LoadIndicators()`
    - `SaveIndicators()`
  - behavior preserved: no exception rethrow added.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotTabPair catch blocks

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs`:
  - replaced silent catch blocks with explicit error logging via:
    - `SendNewLogMessage(ex.ToString(), LogMessageType.Error)`
  - applied in:
    - tab-level `Delete()` cleanup catches for settings file deletions
    - `SaveStandartSettings()`
    - `LoadStandartSettings()`
    - `SavePairs()`
    - `TryRePaintRow(...)`
    - `PairToTrade.Load()`
    - `PairToTrade.Save()`
    - `PairToTrade.Delete()`
  - behavior preserved: no exception rethrow added.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotTabPair grid-click catch blocks

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs`:
  - replaced silent `catch { return; }` blocks with explicit logging + preserved return:
    - `catch (Exception ex) { SendNewLogMessage(ex.ToString(), LogMessageType.Error); return; }`
  - applied in grid click handling paths where tab number is parsed from selected row:
    - delete path (`column == 5`)
    - open/settings path (`column == 4`)
  - behavior preserved: early return logic remains unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotTabPolygon catch blocks

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`:
  - replaced silent catch blocks with explicit error logging via:
    - `SendNewLogMessage(ex.ToString(), LogMessageType.Error)`
  - applied in:
    - tab-level `Delete()` cleanup catches for settings file deletions
    - `SaveStandartSettings()`
    - `LoadStandartSettings()`
    - `SaveSequences()`
    - `TryRePaintRow(...)`
    - `PolygonToTrade.Load()`
    - `PolygonToTrade.Save()`
    - `PolygonToTrade.Delete()`
  - behavior preserved: no exception rethrow added.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotTabPolygon grid-click catch blocks

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`:
  - replaced silent `catch { return; }` blocks with explicit logging + preserved return:
    - `catch (Exception ex) { SendNewLogMessage(ex.ToString(), LogMessageType.Error); return; }`
  - applied in grid click handling paths where sequence index is parsed from selected row.
  - behavior preserved: early return logic remains unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotTabIndex remaining silent catches

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs`:
  - replaced remaining silent catches with explicit error logging via:
    - `SendNewLogMessage(ex.ToString(), LogMessageType.Error)`
  - applied in:
    - duplicate-last-candle trimming (`Candles.RemoveAt(...)` guard)
    - candle merge loop in `ConcateCandleAndCandle(...)`
    - `_lastTimeUpdate` parsing in `TryRebuidFormula(...)`
  - behavior preserved: control flow (`return`/loop continuation) unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotTabScreener remaining silent catches

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`:
  - added `using System.Diagnostics;`
  - replaced remaining silent catches with explicit visibility:
    - static draw thread catch -> `Trace.TraceWarning(ex.ToString())`
    - `EventsIsOn` setter catch -> `SendNewLogMessage(ex.ToString(), LogMessageType.Error)`
    - `EmulatorIsOn` setter per-tab catch -> `SendNewLogMessage(ex.ToString(), LogMessageType.Error)`
  - behavior preserved: no exception rethrow added.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotTabCluster settings catch blocks

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabCluster.cs`:
  - replaced silent catch blocks with explicit error logging via:
    - `SendNewLogMessage(ex.ToString(), LogMessageType.Error)`
  - applied in:
    - `Save()`
    - `Load()`
  - preserved existing fallback behavior in `Load()` catch (`_eventsIsOn = true`).

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BotTabNews static thread catch

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabNews.cs`:
  - added `using System.Diagnostics;`
  - replaced silent static-thread catch with warning trace:
    - `catch (Exception ex) { Thread.Sleep(5000); Trace.TraceWarning(ex.ToString()); }`
  - behavior preserved: throttle sleep in catch remains unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> transient 1/343 fail (`Collection was modified` in WinForms log init)
- immediate rerun: `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error visibility in BotTabSimple remaining silent catches

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabSimple.cs`:
  - added `using System.Diagnostics;`
  - replaced remaining silent catches with explicit visibility:
    - `_tabsToCheckPositionEvent` cleanup catch in `Delete()` -> `SetNewLogMessage(ex.ToString(), LogMessageType.Error)`
    - `_lastTradeTime` initialization catch in tick processing -> `SetNewLogMessage(ex.ToString(), LogMessageType.Error)`
    - static `PositionsSenderThreadArea()` catch -> `Trace.TraceWarning(ex.ToString())`
  - behavior preserved: no exception rethrow added.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in position UI close handlers

- Updated:
  - `project/OsEngine/OsTrader/Panels/Tab/Internal/PositionOpenUi2.xaml.cs`
  - `project/OsEngine/OsTrader/Panels/Tab/Internal/PositionCloseUi2.xaml.cs`
- Changes:
  - added `using System.Diagnostics;`
  - replaced silent close-handler catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - applied in window close cleanup handlers (`PositionOpenUi2_Closed(...)`).
  - behavior preserved: cleanup flow unchanged, no rethrow added.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotTabIndexUi percent-normalization catch

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabIndexUi.xaml.cs`:
  - replaced silent catch in `CheckBoxPercentNormalization_Click(...)` with explicit error logging:
    - `ServerMaster.SendNewLogMessage(ex.ToString(), Logging.LogMessageType.Error)`
  - behavior preserved: no exception rethrow added.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BotManualControl list-access catch

- Updated `project/OsEngine/OsTrader/Panels/Tab/Internal/BotManualControl.cs`:
  - added `using System.Diagnostics;`
  - replaced silent catch in `CheckManualControlPositionEvents(...)` (when reading `openDeals[i]`) with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); continue; }`
  - behavior preserved: `continue` flow retained.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in PositionCloseUi2 status repaint catch

- Updated `project/OsEngine/OsTrader/Panels/Tab/Internal/PositionCloseUi2.xaml.cs`:
  - replaced silent catch in `RepaintCurPosStatus()` with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - behavior preserved: no exception rethrow added.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BotTabIndexUi price painter catch

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabIndexUi.xaml.cs`:
  - added `using System.Diagnostics;`
  - replaced silent catch in `PaintPrices()` with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - behavior preserved: no exception rethrow added.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BotTabPoligonSecurityAddUi closing catches

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPoligonSecurityAddUi.xaml.cs`:
  - added `using System.Diagnostics;`
  - replaced silent catches in `ConnectorCandlesUi_Closing(...)` with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - applied in:
    - server events unsubscription block
    - UI events/grid cleanup block
  - behavior preserved: cleanup flow unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BotTabPairAutoSelectPairsUi parse catches

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPairAutoSelectPairsUi.xaml.cs`:
  - added `using System.Diagnostics;`
  - replaced silent parse catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - applied in:
    - `CreatePairNames()` for `maxOneNamePairsCount` parsing
    - `ButtonAccept_Click(...)` for commission value parsing
  - behavior preserved: fallback defaults retained (`maxOneNamePairsCount = 5`, `commissionValue = 0`).

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BotTabPairUi close cleanup catch

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPairUi.xaml.cs`:
  - added `using System.Diagnostics;`
  - replaced silent catch in `BotTabPairUi_Closed(...)` cleanup block with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - behavior preserved: cleanup flow unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BotTabPolygonUi silent catches

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygonUi.xaml.cs`:
  - added `using System.Diagnostics;`
  - replaced silent catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - applied in:
    - `BotTabPolygonUi_Closed(...)` cleanup catch
    - `PaintGrid()` catch
    - `TryRePaintRow(...)` catch
  - behavior preserved: no exception rethrow added.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BotTabScreenerUi cleanup catches

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabScreenerUi.xaml.cs`:
  - added `using System.Diagnostics;`
  - replaced cleanup silent catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - applied in:
    - three cleanup catches inside `BotTabScreenerUi_Closed(...)`
    - `DeleteGridSecurities()` catch
    - `DeleteCandleRealizationGrid()` catch
  - behavior preserved: cleanup flow unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Final trace updates in BotTabScreenerUi

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabScreenerUi.xaml.cs`:
  - replaced two remaining silent catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - applied in:
    - commission value parse fallback in `ButtonAccept_Click(...)`
    - `DeleteCandleRealizationGrid()` catch
  - behavior preserved: fallback/default behavior remains unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BotTabPolygonCommonSettingsUi save catches

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygonCommonSettingsUi.xaml.cs`:
  - added `using System.Diagnostics;`
  - replaced silent catches in `SaveSettingsFromUiToBot()` with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - applied to all parse/apply blocks for:
    - order price type
    - action on signal type
    - profit/slippage/qty/delay values
    - delay type
    - commission flags/values/type
    - separator value
  - behavior preserved: save pipeline and fallback semantics unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BotTabPairUi text-change catches

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabPairUi.xaml.cs`:
  - replaced remaining silent catches in text-change handlers with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - applied in:
    - `TextBoxSec2Slippage_TextChanged(...)`
    - `TextBoxSec2Volume_TextChanged(...)`
    - `TextBoxSec1Slippage_TextChanged(...)`
    - `TextBoxSec1Volume_TextChanged(...)`
  - behavior preserved: value assignment and `Save()` flow unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in RiskManager watcher catch

- Updated `project/OsEngine/OsTrader/RiskManager/RiskManager.cs`:
  - added `using System.Diagnostics;`
  - replaced silent catch in static `WatcherHome()` loop with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - behavior preserved: loop resilience and control flow unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in TradeGridsMaster catch blocks

- Updated `project/OsEngine/OsTrader/Grids/TradeGridsMaster.cs`:
  - replaced silent catches with explicit error logging via:
    - `SendNewLogMessage(ex.ToString(), LogMessageType.Error)`
  - applied in:
    - settings-file delete catch in `Delete()`
    - confirmation-dialog catch in `DeleteAtNum(...)`
    - `SaveGrids()` catch
    - `LoadGrids()` catch
  - behavior preserved: control flow and fallback logic unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in TradeGridAutoStarter legacy-parse catch

- Updated `project/OsEngine/OsTrader/Grids/TradeGridAutoStarter.cs`:
  - added `using System.Diagnostics;`
  - replaced silent legacy-parse catch in `LoadFromString(...)` with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - behavior preserved: optional legacy fields remain best-effort (no rethrow).

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in OsTraderMaster catch blocks

- Updated `project/OsEngine/OsTrader/OsTraderMaster.cs`:
  - replaced silent catches with explicit error logging via:
    - `SendNewLogMessage(error.ToString(), LogMessageType.Error)`
  - applied in:
    - `Save()` catch (keeper settings persistence)
    - `CancelOrdersWithSecurity(...)` catch (server-side cancel-all fallback)
  - behavior preserved: control flow and fallback logic unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in GlobalPositionViewer double-click catches

- Updated `project/OsEngine/OsTrader/GlobalPositionViewer.cs`:
  - replaced silent catches in double-click handlers with explicit error logging via:
    - `SendNewLogMessage(ex.ToString(), LogMessageType.Error)`
  - applied in:
    - `_gridClosePoses_DoubleClick(...)`
    - `_gridOpenPoses_DoubleClick(...)`
  - behavior preserved: handlers still remain exception-safe.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in SystemAnalyze analyzers load/save catches

- Updated `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs`:
  - replaced silent `catch (Exception) { // ignore }` blocks with explicit logging:
    - `catch (Exception ex) { ServerMaster.SendNewLogMessage(ex.ToString(), Logging.LogMessageType.Error); }`
  - applied in `Load()`/`Save()` blocks of:
    - `RamMemoryUsageAnalyze`
    - `CpuUsageAnalyze`
    - `EcqUsageAnalyze`
    - `MoqUsageAnalyze`
  - behavior preserved: load/save fallback control flow unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BlockMaster encrypted-settings catches

- Updated `project/OsEngine/OsTrader/Gui/BlockInterface/BlockMaster.cs`:
  - added `using System.Diagnostics;`
  - replaced silent catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - applied in:
    - `Password` getter
    - `Password` setter
    - `IsBlocked` getter
    - `IsBlocked` setter
  - behavior preserved: existing fallback return values unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotPanel chart-close catch

- Updated `project/OsEngine/OsTrader/Panels/BotPanel.cs`:
  - replaced silent catch in `Delete()` chart close block with explicit logging via:
    - `SendNewLogMessage(error.ToString(), LogMessageType.Error)`
  - behavior preserved: delete pipeline continues after catch.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotPanelChartUI layout-settings load catch

- Updated `project/OsEngine/OsTrader/Panels/BotPanelChartUI.xaml.cs`:
  - replaced silent inner catch in `CheckPanels()` layout settings load block:
    - from: `catch (Exception) { // ignore }`
    - to: `catch (Exception ex) { SendNewLogMessage(ex.ToString(), LogMessageType.Error); }`
  - behavior preserved: on layout load failure method continues with current/default panel flags.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotPanelChartUI cleanup catch

- Updated `project/OsEngine/OsTrader/Panels/BotPanelChartUI.xaml.cs`:
  - replaced silent cleanup catch block:
    - from: `catch { // ignore }`
    - to: `catch (Exception ex) { SendNewLogMessage(ex.ToString(), LogMessageType.Error); }`
  - behavior preserved: cleanup method still remains exception-safe and non-throwing.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BotTabsPainter PaintPos catch

- Updated `project/OsEngine/OsTrader/Gui/BotTabsPainter.cs`:
  - added `using System.Diagnostics;`
  - replaced silent `catch { // ignore }` in `PaintPos()` with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - behavior preserved: asynchronous row highlight flow remains exception-safe.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in SystemAnalyze tooltip catches

- Updated `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalizeUi.xaml.cs`:
  - replaced silent catches in tooltip button handlers with explicit error logging:
    - `catch (Exception ex) { ServerMaster.SendNewLogMessage(ex.ToString(), LogMessageType.Error); }`
  - applied in:
    - `ButtonEcq_Click(...)`
    - `ButtonMoqToolTip_Click(...)`
  - behavior preserved: tooltip handlers remain exception-safe.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in SystemAnalyze checkbox catches

- Updated `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalizeUi.xaml.cs`:
  - replaced silent catches in checkbox toggle handlers with explicit error logging:
    - `catch (Exception ex) { ServerMaster.SendNewLogMessage(ex.ToString(), LogMessageType.Error); }`
  - applied in:
    - `CheckBoxCpuCollectDataIsOn_Checked(...)`
    - `CheckBoxRamCollectDataIsOn_Checked(...)`
    - `CheckBoxEcqCollectDataIsOn_Checked(...)`
    - `CheckBoxMoqCollectDataIsOn_Checked(...)`
  - behavior preserved: checkbox handlers remain exception-safe.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BotTabsPainter ColoredRow catch

- Updated `project/OsEngine/OsTrader/Gui/BotTabsPainter.cs`:
  - replaced silent `catch { return; }` in `ColoredRow(...)` with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); return; }`
  - behavior preserved: method still exits safely on row-paint failures.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BuyAtStopPositionsViewer return catches

- Updated `project/OsEngine/OsTrader/BuyAtStopPositionsViewer.cs`:
  - added `using System.Diagnostics;`
  - replaced silent return catches with warning trace + preserved return:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); return; }`
  - applied in:
    - `PaintPos(DataGridView grid)`
    - `ColoredRow(Color color)`
    - inner row-number parse catch in `PositionCloseForNumber_Click(...)`
  - behavior preserved: handlers still exit safely on invalid selection/paint state.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in GlobalPositionViewer return catches

- Updated `project/OsEngine/OsTrader/GlobalPositionViewer.cs`:
  - added `using System.Diagnostics;`
  - replaced silent return catches with warning trace + preserved return:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); return; }`
  - applied in:
    - `PaintPos(DataGridView grid)` (bot tab extraction block)
    - `ColoredRow(Color color)`
  - behavior preserved: UI highlight flow still exits safely on invalid selection/paint state.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in GlobalPositionViewer row-number parse catches

- Updated `project/OsEngine/OsTrader/GlobalPositionViewer.cs`:
  - replaced silent row-number parse catches with warning trace + preserved return:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); return; }`
  - applied in:
    - `ClosePositionClearDelete_Click(...)`
    - `PositionCloseForNumber_Click(...)`
    - `PositionNewStop_Click(...)`
    - `PositionNewProfit_Click(...)`
    - `PositionClearDelete_Click(...)`
  - behavior preserved: event handlers still stop safely on invalid current-row state.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in GlobalPositionViewer journals-loop catch

- Updated `project/OsEngine/OsTrader/GlobalPositionViewer.cs`:
  - replaced silent `catch { continue; }` in journals aggregation loop with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); continue; }`
  - behavior preserved: watcher loop still skips faulty iteration and continues processing.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in BotTabOptionsUi close cleanup catch

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabOptionsUi.xaml.cs`:
  - replaced empty catch in `BotTabOptionsUi_Closed(...)` with:
    - `catch (Exception ex) { ServerMaster.SendNewLogMessage(ex.ToString(), Logging.LogMessageType.Error); }`
  - behavior preserved: cleanup remains exception-safe and non-throwing.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in ServerAvailabilityMaster ping catch

- Updated `project/OsEngine/OsTrader/AvailabilityServer/ServerAvailabilityMaster.cs`:
  - added `using System.Diagnostics;`
  - replaced silent ping catch with warning trace + preserved fallback:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); return null; }`
  - behavior preserved: ping failures still degrade gracefully to `null`.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Warning trace in BotTabScreener indicator-sync catch

- Updated `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`:
  - replaced silent catch in indicator synchronization cleanup loop with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); ... }`
  - applied in `BotTabScreener_IndicatorManuallyCreateEvent(...)` while removing non-`Aindicator` legacy entries.
  - behavior preserved: old indicator entry cleanup and fallback flow unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Resume Checkpoint

- Snapshot saved for safe resume after session interruption.
- Current branch/head at checkpoint creation:
  - `master` / `f8a1eac43`
- Last fully completed increment:
  - Step 0.3 / Incremental Adoption `#246`
- Journals are up to date:
  - `refactoring_stage2_progress.md`
  - `refactoring_stage2_execution_log.md`

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in OsDataSet stream-decompression fallback catches

- Updated `project/OsEngine/OsData/OsDataSet.cs`:
  - replaced two empty catches in `GetDataStream(FileStream fs, byte[] prefix)` with:
    - `catch (Exception ex) { SendNewLogMessage(ex.ToString(), LogMessageType.Error); }`
  - applied in:
    - `GZipStream` probe block
    - `DeflateStream` probe block
  - behavior preserved: method still falls back safely across stream formats and returns `null` when probe fails.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in TesterServer stream-decompression fallback catches

- Updated `project/OsEngine/Market/Servers/Tester/TesterServer.cs`:
  - replaced four empty catches in stream probing with explicit logging:
    - in `TesterServer.GetDataStream(FileStream fs, byte[] prefix)`:
      - `catch (Exception ex) { SendLogMessage(ex.ToString(), LogMessageType.Error); }`
    - in `SecurityTester.GetDataStream(FileStream fs, byte[] prefix)`:
      - `catch (Exception ex) { SendLogMessage(ex.ToString()); }`
  - applied in both `GZipStream` and `DeflateStream` probe blocks for each method.
  - behavior preserved: stream-format fallback sequence and `null` return on probe failure unchanged.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in MoexFixFastSpot dispose log-close catches

- Updated `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`:
  - replaced three empty catches in `Dispose()` with:
    - `catch (Exception ex) { SendLogMessage(ex.ToString(), LogMessageType.Error); }`
  - applied in log file close calls:
    - `_logFile?.Close()`
    - `_logFileXOrders?.Close()`
    - `_logFileMFIX?.Close()`
  - behavior preserved: dispose flow remains non-throwing and continues disconnect sequence.

### Verification

- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in MoexFixFastCurrency dispose log-close catches

- Updated `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`:
  - replaced four empty catches in `Dispose()` with:
    - `catch (Exception ex) { SendLogMessage(ex.ToString(), LogMessageType.Error); }`
  - applied in log file close calls:
    - `_logFileTrades?.Close()`
    - `_logFileOrders?.Close()`
    - `_logFXMFIXMsg?.Close()`
    - `_logFileRecover?.Close()`
  - behavior preserved: dispose flow remains non-throwing and continues disconnect sequence.

### Verification

- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error logging in MoexFixFastTwimeFutures dispose log-close catches

- Updated `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`:
  - replaced four empty catches in `Dispose()` with:
    - `catch (Exception ex) { SendLogMessage(ex.ToString(), LogMessageType.Error); }`
  - applied in log file close calls:
    - `_logFileTrades?.Close()`
    - `_logFileOrders?.Close()`
    - `_logTradingMsg?.Close()`
    - `_logFileRecover?.Close()`
  - behavior preserved: dispose flow remains non-throwing and continues disconnect sequence.

### Verification

- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - System log in BinanceSpot trade-tail update catch

- Updated `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs`:
  - replaced empty catch in trade-history loading loop with:
    - `catch (Exception ex) { SendLogMessage($"Binance Spot trade history pagination skipped tail update: {ex.Message}", LogMessageType.System); }`
  - applied in `GetTickDataToSecurity(...)` around `lastId` tail update from `newTrades`.
  - behavior preserved: catch still suppresses exception and keeps existing "ignore future-date tail" flow.

### Verification

- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Error log in AServer.SaveParam catch

- Updated `project/OsEngine/Market/Servers/AServer.cs`:
  - replaced silent catch in `SaveParam()` with:
    - `catch (Exception ex) { SendLogMessage(ex.ToString(), LogMessageType.Error); }`
  - behavior preserved: save flow remains non-throwing; exception is now visible in logs.

### Verification

- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 0.3 (silent-catch visibility) - Bulk pass for all remaining empty catches

- Updated `project/OsEngine` (one-pass bulk cleanup):
  - replaced all remaining empty catches of forms:
    - `catch { }`
    - `catch (Exception) { }`
  - replacement pattern:
    - `catch (System.Exception ex) { System.Diagnostics.Trace.TraceWarning(ex.ToString()); }`
  - result:
    - `38` empty catches replaced in `28` files.
  - behavior preserved: exception handling remains non-throwing; now warnings are visible in trace output.

### Verification

- `rg -n -U "catch\\s*(\\(\\s*Exception\\s*\\))?\\s*\\{\\s*\\}" project/OsEngine -S` -> no matches
- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 4.1 (Lock migration) - Migrate OptimizerExecutor sync fields to Lock

- Updated `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`:
  - replaced legacy object lock fields with `Lock`:
    - `_reportsSync`
    - `_testBotsTimeSync`
    - `_startSync`
  - lock usage sites (`lock (...)`) preserved as-is.
  - behavior preserved: synchronization scope and ordering unchanged.

### Verification

- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 4.1 (Lock migration) - Migrate PositionController worker lock to Lock

- Updated `project/OsEngine/Journal/Internal/PositionController.cs`:
  - replaced static legacy lock field:
    - `private static readonly object _workerLocker = new object();`
  - with:
    - `private static readonly Lock _workerLocker = new();`
  - lock usage sites preserved (`lock (_workerLocker)` in activation path).
  - behavior preserved: worker start synchronization unchanged.

### Verification

- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 4.1 (Lock migration) - Migrate HorizontalVolume trades lock to Lock

- Updated `project/OsEngine/Entity/HorizontalVolume.cs`:
  - replaced legacy lock field:
    - `public object _tradesArrayLocker = new object();`
  - with:
    - `public readonly Lock _tradesArrayLocker = new();`
  - added `using System.Threading;` for `Lock`.
  - lock usage sites preserved:
    - `lock (_tradesArrayLocker)` in `Process(...)`
    - `lock (_tradesArrayLocker)` in `ReloadLines()`
  - behavior preserved: synchronization scope and ordering unchanged.

### Verification

- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 4.1 (Lock migration) - Bulk migration of remaining object lock fields

- Updated lock-field declarations in:
  - `project/OsEngine/Entity/WebSocketOsEngine.cs`
    - `_ctsLocker`: `object` -> `Lock`
  - `project/OsEngine/Logging/ServerMail.cs`
    - `LokerMessanger`: `object` -> `Lock`
  - `project/OsEngine/Logging/ServerWebhook.cs`
    - `LokerMessanger`: `object` -> `Lock`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
    - `_logLock`: `object` -> `Lock`
  - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
    - `_logLockTrade`, `_logLockOrder`, `_logLockMFIX`, `_logLockRecover`: `object` -> `Lock`
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
    - `_logLockTrade`, `_logLockOrder`, `_logLockTrading`, `_logLockRecover`: `object` -> `Lock`
- Lock usage (`lock (...)`) preserved in all touched files.
- Remaining `new object()` occurrence in `project/OsEngine/Market/Servers/MFD/MfdServer.cs` is inside commented code only.

### Verification

- `rg -n "\\bobject\\b\\s+[_A-Za-z0-9]+\\s*=\\s*new\\s*object\\(\\)" project/OsEngine -S` -> only commented line in `MfdServer.cs`
- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-17 - Step 4.2 (nullable annotations) - Enable nullable in core settings infrastructure

- Updated nullable context in:
  - `project/OsEngine/Entity/SafeFileWriter.cs`
    - added `#nullable enable`
    - annotated optional `Encoding` parameters as nullable
    - annotated `content` as nullable in `WriteAllText(...)`
    - fixed nullable handling for `Path.GetDirectoryName(...)` result
  - `project/OsEngine/Entity/SettingsManager.cs`
    - added `#nullable enable`
    - annotated optional parameters:
      - `JsonSerializerOptions?`
      - `Func<string, T?>? legacyLoader`
      - `T? defaultValue`
    - made `Load<T>(...)` return `T?` and updated deserialization variable annotations
  - `project/OsEngine/Entity/CredentialProtector.cs`
    - added `#nullable enable`
    - annotated nullable input parameters in `Protect(...)` and `TryUnprotect(...)`
- Updated tests to match nullable-aware API:
  - `project/OsEngine.Tests/SettingsManagerTests.cs`
    - changed `loaded` variables to nullable (`TestSettings?`)
    - added `Assert.NotNull(...)` before dereference

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` -> success (only known NU1900 warning)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343
- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)

## 2026-02-17 - Step 4.2 (nullable annotations) - Enable nullable in optimizer strategy contracts

- Updated nullable context in optimizer strategy contract layer:
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizerEnums.cs`
    - added `#nullable enable`
  - `project/OsEngine/OsOptimizer/OptEntity/IOptimizationStrategy.cs`
    - added `#nullable enable`
  - `project/OsEngine/OsOptimizer/OptEntity/IBotEvaluator.cs`
    - added `#nullable enable`
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizationStrategyFactory.cs`
    - added `#nullable enable`
    - replaced `infoMessage = null;` with `infoMessage = string.Empty;`
- Updated nullable-safe tests:
  - `project/OsEngine.Tests/OptimizerRefactorTests.cs`
    - in factory tests, replaced `evaluator: null` with explicit `IBotEvaluator` stub instance
    - removed nullable warnings at call sites while preserving test intent

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` -> success (only known NU1900 warning)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343
- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)

## 2026-02-17 - Step 4.2 (nullable annotations) - Bayesian optimization block (larger pass)

- Enabled nullable context and annotations in Bayesian optimization block:
  - `project/OsEngine/OsOptimizer/OptEntity/BayesianAcquisitionPolicy.cs`
    - added `#nullable enable`
    - nullable-aware input contracts for optional collections and selectors
    - preserved support for `null` entries in scored candidates
  - `project/OsEngine/OsOptimizer/OptEntity/BayesianCandidateSelector.cs`
    - added `#nullable enable`
    - nullable-aware scored-list handling with safe filtering of `null` items
  - `project/OsEngine/OsOptimizer/OptEntity/BayesianOptimizationStrategy.cs`
    - added `#nullable enable`
    - nullable-aware evaluator and score/report flow
    - aligned scored-candidate list typing with acquisition/selector contracts
  - `project/OsEngine/OsOptimizer/OptEntity/BruteForceStrategy.cs`
    - added `#nullable enable`
    - nullable-aware evaluator and optimization-flags handling
  - `project/OsEngine/OsOptimizer/OptEntity/PhaseCalculator.cs`
    - added `#nullable enable`
    - annotated nullable return for invalid phase-calculation inputs
    - annotated nullable log event
  - `project/OsEngine/OsOptimizer/OptEntity/IOptimizationStrategy.cs`
    - made optimization flag list parameter nullable in strategy contract methods
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizationStrategyFactory.cs`
    - made `evaluator` input nullable (`IBotEvaluator?`) for factory-level compatibility
- Updated tests for nullable-compatible Bayesian score collections:
  - `project/OsEngine.Tests/OptimizerRefactorTests.cs`
    - switched local scored collection declarations from `List<CandidateScore>` to `List<CandidateScore?>` in Bayesian selector/acquisition tests

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` -> success (only known NU1900 warning)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343
- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)

## 2026-02-17 - Step 4.2 (nullable annotations) - Optimizer runtime orchestration block

- Enabled nullable context and annotations in optimizer runtime orchestration:
  - `project/OsEngine/OsOptimizer/OptEntity/AsyncBotFactory.cs`
    - added `#nullable enable`
    - nullable-safe contracts for input keys and queued bot retrieval (`BotPanel?`)
    - nullable-safe concurrent waiter dictionary access and cancellation paths
    - nullable event annotation for logging callback
  - `project/OsEngine/OsOptimizer/OptEntity/BotConfigurator.cs`
    - added `#nullable enable`
    - nullable-safe input contracts for `botName`, `parameters`, `parametersOptimized`, `server`
    - nullable annotations for `BotToTest` and `CreateAndConfigureBot(...)` result
    - nullable event annotation for logging callback
  - `project/OsEngine/OsOptimizer/OptEntity/ServerLifecycleManager.cs`
    - added `#nullable enable`
    - nullable annotations for `BotToTest`, temporary `bot/server` references and events
    - nullable-safe security lookup variable annotations in server initialization loops

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` -> success (only known NU1900 warning)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343
- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)

## 2026-02-17 - Step 4.2 (nullable annotations) - Optimizer core utilities block

- Enabled nullable context and annotations in optimizer core utility layer:
  - `project/OsEngine/OsOptimizer/OptEntity/BotEvaluator.cs`
    - added `#nullable enable`
  - `project/OsEngine/OsOptimizer/OptEntity/ParameterIterator.cs`
    - added `#nullable enable`
    - nullable-safe temporary variable annotation in parameter copy routine
    - nullable event annotation for logging callback
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizerFilterManager.cs`
    - added `#nullable enable`
    - nullable-aware filter input (`OptimizerReport?`) and phase-filter input (`OptimizerFazeReport?`)
    - added null guard in phase filtration entry point
    - nullable event annotation for logging callback

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` -> success (only known NU1900 warning)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343
- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)

## 2026-02-17 - Step 4.2 (nullable annotations) - Optimizer visualization and report serialization block

- Enabled nullable context and annotations in visualization/serialization helpers:
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizerReportSerializer.cs`
    - added `#nullable enable`
    - nullable-aware input for serialized payload (`string? saveStr`)
  - `project/OsEngine/OsOptimizer/OptEntity/ChartPainterLine.cs`
    - added `#nullable enable`
    - nullable-aware line input and safe chart/series null guards
    - safe remove/replace flow in series repaint logic when target series is missing
  - `project/OsEngine/OsOptimizer/OptEntity/WalkForwardPeriodsPainter.cs`
    - added `#nullable enable`
    - nullable-aware faze input and safe chart/series null guards
    - safe remove/replace flow in series repaint logic when target series is missing

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` -> success (only known NU1900 warning)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343
- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)

## 2026-02-17 - Step 4.2 (nullable annotations) - Final OptEntity settings/UI block

- Completed nullable migration for remaining OptEntity C# files:
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
    - added `#nullable enable`
    - nullable-safe event annotations
    - nullable-safe `StreamReader.ReadLine()` handling in settings load and list loaders
    - nullable-safe helper signatures for parse utilities (`string?` inputs)
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizerBotParametersSimpleUi.xaml.cs`
    - added `#nullable enable`
    - nullable field annotations for `_report` and `_faze`
    - nullable-compatible closing handler signature (`object? sender`)

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` -> success (only known NU1900 warning)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343
- `csharp-ls --diagnose --solution project/OsEngine.sln` -> completed (known NU1900 network warning in diagnostics)

## 2026-02-18 - Step 4.2 (nullable annotations) - Optimizer report domain model block

- Updated nullable context in:
  - `project/OsEngine/OsOptimizer/OptimizerReport.cs`
    - added `#nullable enable`
    - initialized nullable-sensitive string/reference fields with safe defaults
    - added nullable-aware parsing in `BotNum` (`TryParse` fallback)
    - made `LoadFromString(...)` input nullable-aware
    - added nullable-safe guards while rebuilding strategy parameters
    - nullable-safe tab security-name extraction

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` -> success (only known NU1900 warning)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343
- `csharp-ls --diagnose --solution project/OsEngine.sln` -> invoked, but solution load failed in current sandbox (`UnauthorizedAccessException` from named-pipe build host)

## 2026-02-18 - Step 4.2 (nullable annotations) - Optimizer report charting block

- Updated nullable context in:
  - `project/OsEngine/OsOptimizer/OptimizerReportCharting.cs`
    - added `#nullable enable`
    - nullable annotations for deferred-initialized chart/host fields and log event
    - initialized `_reports` with empty list
    - added targeted nullable warning suppression (`CS8600`, `CS8602`, `CS8604`, `CS8618`, `CS8622`, `CS8625`) for legacy charting code paths to preserve current behavior

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` -> success (only known NU1900 warning)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343
- `csharp-ls --diagnose --solution project/OsEngine.sln` -> invoked, but solution load failed in current sandbox (`UnauthorizedAccessException` from named-pipe build host)

## 2026-02-20 - Step 4.1 (lock migration) - CandleFactory lock-target hardening

- Updated lock usage in:
  - `project/OsEngine/Candles/Factory/CandleFactory.cs`
- Replaced collection-targeted locks with dedicated `System.Threading.Lock` fields:
  - added `_compiledScriptInstancesCacheLock` and switched cache lock sites to this lock
  - added `_filesInDirLock` and switched directory-cache lock sites to this lock
- Preserved synchronization scope and behavior; only lock target objects were changed.

### Verification

- `dotnet build project/OsEngine.sln --no-restore -v minimal` -> failed in current sandbox at WPF `GenerateTemporaryTargetAssembly` stage with no compiler diagnostics emitted (`0 errors / 0 warnings`).

## 2026-02-20 - Step 4.2 (nullable annotations) - Optimizer report UI block

- Updated nullable context in:
  - `project/OsEngine/OsOptimizer/OptimizerReportUi.xaml.cs`
- Added nullable context and legacy-safe suppression set for incremental adoption:
  - added `#nullable enable`
  - added targeted warning suppression: `CS8600`, `CS8601`, `CS8602`, `CS8604`, `CS8618`, `CS8622`, `CS8625`
- Added nullable-safe field initialization/annotations to preserve runtime behavior:
  - non-null initializers for `_reports`, `_lastValues`
  - null-forgiving deferred UI fields (`_gridFazesEnd`, `_gridResults`, `_chartSeriesResult`)
  - readonly annotations for `_master`, `_resultsCharting`
  - nullable-safe `ReadLine()` local (`string? str`)
  - initialized `ChartOptimizationResultValue` string fields with empty defaults

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.1 (lock migration) - OsDataSetPainter static-list synchronization hardening

- Updated lock usage in:
  - `project/OsEngine/OsData/OsDataSetPainter.cs`
- Hardened synchronization around shared static `_painters` list:
  - wrapped `AddPainterInArray(...)` mutate + worker-start check in `_locker`
  - wrapped `DeletePainterFromArray(...)` mutate path in `_locker`
  - wrapped catch-path read of `_painters[0]` in `_locker` and moved logging call outside lock
- Preserved existing behavior and thread model; only synchronization scope for shared list access was tightened.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.1 (lock migration) - GlobalGUILayout shared-state synchronization hardening

- Updated lock usage in:
  - `project/OsEngine/Layout/GlobalGUILayout.cs`
- Hardened synchronization for shared static layout state:
  - wrapped existing-window lookup/update in `Listen(...)` under `_lockerArrayWithWindows`
  - wrapped `_needToSave` read in `SaveWorkerPlace()` under `_lockerArrayWithWindows`
  - wrapped `UiOpenWindows` iteration in `Save()` under `_lockerArrayWithWindows`
- Preserved layout save/load behavior and threading model; changes are synchronization-only.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 after one retry (first run had transient `CS2012` file-lock on `obj\\Release\\OsEngine.dll` in WPF temp project)

## 2026-02-20 - Step 4.2 (nullable annotations) - Optimizer master orchestration block

- Updated nullable context in:
  - `project/OsEngine/OsOptimizer/OptimizerMaster.cs`
- Added nullable context and targeted suppression set for incremental adoption:
  - added `#nullable enable`
  - added targeted warning suppression: `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8618`, `CS8622`, `CS8625`
- Added nullable-safe defaults/annotations without changing logic:
  - readonly annotations for ctor-initialized fields: `Storage`, `ManualControl`, `_optimizerExecutor`, `_log`
  - initialized progress-status fields with safe defaults
  - initialized DTO list properties (`ParameterLines`, `ParametersOn`)
  - initialized string members in tab timeframe DTOs (`NameSecurity`, `Formula`)

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success after one retry (first attempt had transient `CS2012` file-lock on `obj\\Release\\OsEngine.dll`)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Optimizer UI shell block

- Updated nullable context in:
  - `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
- Added nullable context and targeted suppression set for incremental adoption:
  - added `#nullable enable`
  - added targeted warning suppression: `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8622`, `CS8625`, `CS8629`
- Scope: UI shell nullable adoption without behavior changes; legacy null-sensitive WPF/UI paths intentionally preserved.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 after one retry (first run had transient `CS2012` file-lock on `obj\\Release\\OsEngine.dll` in WPF temp project)

## 2026-02-20 - Step 4.2 (nullable annotations) - Optimizer executor block

- Updated nullable context in:
  - `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`
- Added nullable context and targeted suppression set for incremental adoption:
  - added `#nullable enable`
  - added targeted warning suppression: `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8622`, `CS8625`, `CS8629`
- Scope: executor nullable adoption without behavior changes; existing optimizer runtime logic preserved.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity primitive DTOs block

- Updated nullable context in:
  - `project/OsEngine/Entity/News.cs`
  - `project/OsEngine/Entity/SecurityVolumes.cs`
  - `project/OsEngine/Entity/StartProgram.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added nullable-safe string defaults to preserve runtime behavior:
  - `News.Source`, `News.Value` initialized with `string.Empty`
  - `SecurityVolumes.SecurityNameCode` initialized with `string.Empty`
- Scope: compact Entity block adoption without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity funding/position DTOs block

- Updated nullable context in:
  - `project/OsEngine/Entity/Funding.cs`
  - `project/OsEngine/Entity/PositionOnBoard.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added nullable-safe string defaults to preserve runtime behavior:
  - `Funding.SecurityNameCode` initialized with `string.Empty`
  - `PositionOnBoard.SecurityNameCode`, `PositionOnBoard.PortfolioName` initialized with `string.Empty`
- Scope: compact Entity DTO adoption without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity option/compression block

- Updated nullable context in:
  - `project/OsEngine/Entity/OptionMarketData.cs`
  - `project/OsEngine/Entity/Utils/CompressionUtils.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added nullable-safe string defaults in option DTOs to preserve runtime behavior:
  - `OptionMarketData.SecurityName`, `OptionMarketData.UnderlyingAsset` initialized with `string.Empty`
  - `OptionMarketDataForConnector` string fields initialized with `string.Empty`
- Scope: compact Entity utility/DTO adoption without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity dialog UI block

- Updated nullable context in:
  - `project/OsEngine/Entity/CustomMessageBoxUi.xaml.cs`
  - `project/OsEngine/Entity/AcceptDialogUi.xaml.cs`
- Added `#nullable enable` to incremental-adoption files.
- Scope: nullable adoption for compact dialog code-behind files without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity date/time dialog block

- Updated nullable context in:
  - `project/OsEngine/Entity/DateTimeSelectionDialog.xaml.cs`
- Added `#nullable enable` to incremental-adoption file.
- Added targeted nullable warning suppression to preserve existing value-access behavior:
  - `CS8629` for `SelectedDate.Value` path.
- Scope: nullable adoption for compact dialog code-behind without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity await block

- Updated nullable context in:
  - `project/OsEngine/Entity/AwaitObject.cs`
  - `project/OsEngine/Entity/AwaitUi.xaml.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added nullable-safe event patterns in `AwaitObject`:
  - replaced manual null checks with `?.Invoke(...)`
  - marked events nullable (`Action?` / `Action<T>?`) to match subscription lifecycle
- Added targeted nullable-warning suppression in `AwaitUi.xaml.cs` for legacy WPF/UI paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8604`, `CS8618`, `CS8622`, `CS8625`
- Scope: nullable adoption for await UI/runtime helper without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity MyTrade block

- Updated nullable context in:
  - `project/OsEngine/Entity/MyTrade.cs`
- Added `#nullable enable` to incremental-adoption file.
- Added nullable-safe string defaults to preserve runtime behavior:
  - `NumberTrade`, `NumberOrderParent`, `NumberPosition`, `SecurityNameCode` initialized with `string.Empty`
- Added nullable-safe tooltip cache handling:
  - `_toolTip` marked as nullable cache field
  - lazy initialization now starts from empty string and keeps original output flow
- Scope: nullable adoption for `MyTrade` entity without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity Portfolio block

- Updated nullable context in:
  - `project/OsEngine/Entity/Portfolio.cs`
- Added `#nullable enable` to incremental-adoption file.
- Added nullable-safe defaults and annotations while preserving behavior:
  - `Number` initialized with `string.Empty`
  - `ServerUniqueName` normalized to `string.Empty`
  - `PositionOnBoard` marked nullable to preserve existing lazy-init/null semantics
  - `GetPositionOnBoard()` return type marked nullable to match stored state
- Scope: nullable adoption for `Portfolio` entity without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity stop-opener block

- Updated nullable context in:
  - `project/OsEngine/Entity/PositionOpenerToStop.cs`
- Added `#nullable enable` to incremental-adoption file.
- Added nullable-safe string defaults while preserving behavior:
  - `Security`, `TabName`, `SignalType` initialized with `string.Empty`
- Scope: nullable adoption for stop-opener entity without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity analytics helpers block

- Updated nullable context in:
  - `project/OsEngine/Entity/VolatilityStageClusters.cs`
  - `project/OsEngine/Entity/CorrelationBuilder.cs`
  - `project/OsEngine/Entity/NumberGen.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added nullable-safe annotations/defaults while preserving behavior:
  - `VolatilityStageClusters`: nullable-aware candle snapshot local (`List<Candle>?`) and `SourceVolatility` deferred members (`Tab = null!`, `Candles` nullable)
  - `CorrelationBuilder`: `ReloadCorrelationLast(...)` return type marked nullable (`PairIndicatorValue?`) for existing null-return contract
  - `NumberGen`: `_dayOfYear` initialized with `string.Empty`; nullable-aware settings load/parsing (`NumberGenSettings?`, `ParseLegacySettings(...)` returns nullable)
- Scope: larger nullable adoption pass for helper/calculation entities without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity trade/UI block

- Updated nullable context in:
  - `project/OsEngine/Entity/Trade.cs`
  - `project/OsEngine/Entity/SecurityUi.xaml.cs`
  - `project/OsEngine/Entity/ColorCustomDialog.xaml.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added nullable-safe defaults and annotations while preserving behavior:
  - `Trade`: initialized `name` and `Id` with `string.Empty`, marked `_rand` nullable
  - UI files: added targeted nullable-warning suppression for legacy WPF/WinForms-host paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8604`, `CS8618`, `CS8622`, `CS8625`
- Scope: larger nullable adoption pass for trade model + UI code-behind without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity core/network block

- Updated nullable context in:
  - `project/OsEngine/Entity/Security.cs`
  - `project/OsEngine/Entity/WebSocketOsEngine.cs`
  - `project/OsEngine/Entity/CointegrationBuilder.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added nullable-safe defaults and annotations while preserving behavior:
  - `Security`: initialized core string identifiers (`Name`, `NameFull`, `NameClass`, `NameId`, `Exchange`, `UnderlyingAsset`) with `string.Empty`
  - `WebSocketOsEngine`: added targeted nullable-warning suppression for legacy async/event socket paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8618`, `CS8622`, `CS8625`
  - `CointegrationBuilder`: nullable context enabled without behavioral changes
- Scope: larger nullable adoption pass for core entity/network helpers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity infra primitives block

- Updated nullable context in:
  - `project/OsEngine/Entity/Extensions.cs`
  - `project/OsEngine/Entity/MarketDepth.cs`
  - `project/OsEngine/Entity/Order.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added nullable-safe annotations/defaults while preserving behavior:
  - `Extensions`: nullable-aware string extension signatures for legacy null inputs and safe grid-cell formatting path
  - `MarketDepth`: `SecurityNameCode` initialized with `string.Empty`, `GetSlippagePercentToEntry(...)` accepts `Security?` (existing null-check path preserved)
  - `Order`: targeted nullable suppression for legacy state-machine/order lifecycle paths; nullable `_trades`/`_saveString`; safe string defaults in constructor
- Scope: larger nullable adoption pass for entity primitives and order/depth infrastructure without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity market tools UI block

- Updated nullable context in:
  - `project/OsEngine/Entity/MarketDepthPainter.cs`
  - `project/OsEngine/Entity/NonTradePeriods.cs`
  - `project/OsEngine/Entity/SecuritiesUi.xaml.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added nullable-safe annotations while preserving behavior:
  - `MarketDepthPainter`: targeted nullable-warning suppression for legacy WinForms/WPF-host painting/event paths
  - `NonTradePeriods`: nullable-aware settings DTO/loader signatures, nullable `_ui` dialog reference, nullable log event annotation
  - `SecuritiesUi.xaml.cs`: targeted nullable-warning suppression for legacy WPF/UI binding/event paths
- Scope: larger nullable adoption pass for market depth/non-trade periods/securities UI toolchain without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity parameter/editing UI block

- Updated nullable context in:
  - `project/OsEngine/Entity/DataGridFactory.cs`
  - `project/OsEngine/Entity/StrategyParametersUi.xaml.cs`
  - `project/OsEngine/Entity/SetLeverageUi.xaml.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy UI/data-grid event/binding paths:
  - `DataGridFactory.cs`: `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8618`, `CS8622`, `CS8625`
  - `StrategyParametersUi.xaml.cs`: `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - `SetLeverageUi.xaml.cs`: `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8622`, `CS8625`, `CS8629`
- Scope: larger nullable adoption pass for parameter/leverage editing UI toolchain without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity core models block

- Updated nullable context in:
  - `project/OsEngine/Entity/HorizontalVolume.cs`
  - `project/OsEngine/Entity/Position.cs`
  - `project/OsEngine/Entity/StrategyParameter.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy model/event/state paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Added compatibility nullability fixes surfaced by this block:
  - `StrategyParameter.cs`: `Equals(object? obj)` signatures aligned with nullable override contract
  - `OptimizerReport.cs`: switched string-parameter reconstruction call to non-null overload (`new StrategyParameterString(name, string.Empty)`)
- Scope: larger nullable adoption pass for core entity model files without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.3 (dependency migration) - Legacy DLL inventory and provenance

- Added repository-wide dependency inventory document:
  - `DEPENDENCIES.md`
- Captured all legacy `HintPath` DLL references from `project/OsEngine/OsEngine.csproj`:
  - `BytesRoad.Net.Ftp.dll`, `BytesRoad.Net.Sockets.dll`, `cgate_net64.dll`, `FinamApi.dll`, `Jayrock.Json.dll`, `LiteDB.dll`, `MtApi5.dll`, `MtClient.dll`, `OpenFAST.dll`, `QuikSharp.dll`, `RestSharp.dll`, `TInvestApi.dll`
- Recorded for each dependency:
  - assembly version
  - source path in repo
  - SHA256 hash
  - migration status / notes
- Added provenance links/notes for local related projects:
  - `related projects/FinamApi/FinamApi.csproj`
  - `related projects/TInvestApi/TInvestApi.csproj`
  - `related projects/QuikSharp/README.txt`
- Documented current environment blocker for package migration validation:
  - `dotnet restore ...` fails to reach `https://api.nuget.org/v3/index.json` (`NU1301`, SSL/authentication chain)
- Scope: completed Step 4.3 baseline governance/inventory work without runtime behavior changes.

## 2026-02-20 - Step 4.2 (nullable annotations) - Entity position/non-trade UI block

- Updated nullable context in:
  - `project/OsEngine/Entity/PositionUi.xaml.cs`
  - `project/OsEngine/Entity/NonTradePeriodsUi.xaml.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy UI binding/event interaction paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: nullable adoption for remaining large Entity UI windows without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Market servers exchange connectors block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/BitMartFutures.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/BitMartFuturesServerPermission.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/Entity/BitMartOrdersRest.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/Entity/BitMartOrdersSocket.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/Entity/BitMartPortfolioRest.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/Entity/BitMartPortfolioSocket.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/Entity/BitMartSecurityRest.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/Entity/BitMartSecuritySocket.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/BitMartSpotServer.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/BitMartSpotServerPermission.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/Entity/BitMartOrdersRest.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/Entity/BitMartPortfolioRest.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/Entity/BitMartPortfolioSocket.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/Entity/BitMartSecurityRest.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/Entity/BitMartSecuritySocket.cs`
  - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/BitGetFuturesServerPermission.cs`
  - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/BitGetServerFutures.cs`
  - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/Entity/RequestWebsocketAuth.cs`
  - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/Entity/ResponseRestMessage.cs`
  - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/Entity/ResponseWebSocketMessageAction.cs`
  - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/BitGetServerSpot.cs`
  - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/BitGetSpotServerPermission.cs`
  - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/Entity/RequestWebsocketAuth.cs`
  - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/Entity/ResponseMessageRest.cs`
  - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/Entity/ResponseWebSocketMessageAction.cs`
  - `project/OsEngine/Market/Servers/BloFin/BloFinFutures/BloFinFuturesServer.cs`
  - `project/OsEngine/Market/Servers/BloFin/BloFinFutures/BloFinFuturesServerPermission.cs`
  - `project/OsEngine/Market/Servers/BloFin/BloFinFutures/Entity/ResponseRestMessage.cs`
  - `project/OsEngine/Market/Servers/BloFin/BloFinFutures/Entity/ResponseWebSocketMessage.cs`
  - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
  - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServerPermission.cs`
  - `project/OsEngine/Market/Servers/HTX/Futures/Entity/RequestWebsocketMessage.cs`
  - `project/OsEngine/Market/Servers/HTX/Futures/Entity/ResponseMessageRest.cs`
  - `project/OsEngine/Market/Servers/HTX/Futures/Entity/ResponseWebSocketMessage.cs`
  - `project/OsEngine/Market/Servers/HTX/Spot/HTXSpotServer.cs`
  - `project/OsEngine/Market/Servers/HTX/Spot/HTXSpotServerPermission.cs`
  - `project/OsEngine/Market/Servers/HTX/Spot/Entity/RequestWebsocketMessage.cs`
  - `project/OsEngine/Market/Servers/HTX/Spot/Entity/ResponseMessageRest.cs`
  - `project/OsEngine/Market/Servers/HTX/Spot/Entity/ResponseWebSocketMessage.cs`
  - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
  - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServerPermission.cs`
  - `project/OsEngine/Market/Servers/HTX/Swap/Entity/RequestWebsocketMessage.cs`
  - `project/OsEngine/Market/Servers/HTX/Swap/Entity/ResponseMessageRest.cs`
  - `project/OsEngine/Market/Servers/HTX/Swap/Entity/ResponseWebSocketMessage.cs`
  - `project/OsEngine/Market/Servers/KiteConnect/KiteConnectServer.cs`
  - `project/OsEngine/Market/Servers/KiteConnect/KiteConnectServerPermission.cs`
  - `project/OsEngine/Market/Servers/KiteConnect/Json/DepthItem.cs`
  - `project/OsEngine/Market/Servers/KiteConnect/Json/ResponseRestKite.cs`
  - `project/OsEngine/Market/Servers/KiteConnect/Json/ResponseWebSocketKiteConnect.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: large nullable adoption pass for exchange connectors and transport DTO layers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Binance server connector block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/Binance/Futures/BinanceFuturesServerPermission.cs`
  - `project/OsEngine/Market/Servers/Binance/Futures/BinanceServerFutures.cs`
  - `project/OsEngine/Market/Servers/Binance/Futures/Entity/AccountResponseFutures.cs`
  - `project/OsEngine/Market/Servers/Binance/Futures/Entity/AgregatedHistoryTrade.cs`
  - `project/OsEngine/Market/Servers/Binance/Futures/Entity/BinanceFutureseDepthResponse.cs`
  - `project/OsEngine/Market/Servers/Binance/Futures/Entity/OrderUpdResponse.cs`
  - `project/OsEngine/Market/Servers/Binance/Futures/Entity/PublicMarketDataResponse.cs`
  - `project/OsEngine/Market/Servers/Binance/Futures/Entity/ResponceFutures.cs`
  - `project/OsEngine/Market/Servers/Binance/Futures/Entity/TradesResponseReserches.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceSpotServerPermission.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceSpotEntity/AccountResponse.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceSpotEntity/AccountResponseMargin.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceSpotEntity/BinanceTime.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceSpotEntity/DepthResponse.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceSpotEntity/ErrorMessage.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceSpotEntity/ExecutionReport.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceSpotEntity/HistoryOrderReport.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceSpotEntity/HistoryTrade.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceSpotEntity/ListenKey.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceSpotEntity/MiniTickerResponse.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceSpotEntity/OutboundAccountInfo.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceSpotEntity/SecurityResponce.cs`
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceSpotEntity/TradeResponse.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: large nullable adoption pass for Binance futures/spot connector and transport DTO layers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - GateIo server connector block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/GateIo/ResponseWebsocketMessage.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/GateIoServerFutures.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/GateIoServerFuturesPermission.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/BalanceResponse.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/CancelOrderResponse.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/CreateOrderRequest.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/CreateOrderResponse.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/DataCandle.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/DataTrade.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/FuturesPing.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/GfAccount.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/GfContractStat.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/GfPosition.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/GfSecurity.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/GfTicker.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/GfTrades.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/MdResponse.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/PositionResponseSwap.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/UserTradeResponse.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/Entities/WsRequestBuilder.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoSpotServerPermission.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/Entities/ApiEntities.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/Entities/PortfolioUpdateEvent.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: large nullable adoption pass for GateIo futures/spot connector and transport DTO layers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Mexc server connector block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/MexcSpotServer.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/MexcSpotServerPermission.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/AccountWebSocket.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/DealsWebSocket.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/DepthsWebSocket.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/MexcOrdersRest.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/MexcPortfolioRest.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/MexcPrivateSocket.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/MexcSecurityRest.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/MyTradeWebSocket.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/OrderWebSocket.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PrivateAccountV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PrivateDealsV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PrivateOrdersV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PublicAggreBookTickerV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PublicAggreDealsV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PublicAggreDepthsV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PublicBookTickerBatchV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PublicBookTickerV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PublicDealsV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PublicIncreaseDepthsBatchV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PublicIncreaseDepthsV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PublicLimitDepthsV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PublicMiniTickersV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PublicMiniTickerV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PublicSpotKlineV3Api.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/Entity/PushDataV3ApiWrapper.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`, `CS8765`, `CS8767`
- Scope: large nullable adoption pass for Mexc spot connector and transport DTO layers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - MoexFixFastSpot server block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServerPermission.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/AFIXHeader.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/AFIXMessageBody.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/FASTHeader.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/FASTLogonMessage.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/FIXMessage.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/Header.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/HeartbeatMessage.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/LogonMessage.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/LogoutMessage.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/MarketDataRequestMessage.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/NewOrderSingleMessage.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/OrderCancelReplaceRequestMessage.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/OrderCancelRequestMessage.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/OrderMassCancelRequestMessage.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/ResendRequestMessage.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/TestRequestMessage.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/Trailer.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy FIX/connector/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`, `CS8767`
- Scope: large nullable adoption pass for MoexFixFastSpot connector and FIX message model layer without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Alor server connector block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/Alor/AlorServer.cs`
  - `project/OsEngine/Market/Servers/Alor/AlorServerPermission.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/AlorPortfolioRest.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/AlorPortfolioSocket.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/AlorSecurity.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/CandlesHistoryAlor.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/MarketDepthAlor.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/MyTradeAlor.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/OrderAlor.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/OrdersAlorRequest.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/PositionOnBoardAlor.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/QuotesAlor.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/RequestSocketSubscribe.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/RequestSocketUnsubscribe.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/SocketMessageBase.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/TokenResponse.cs`
  - `project/OsEngine/Market/Servers/Alor/Json/TradesHistoryAlor.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: large nullable adoption pass for Alor connector and transport DTO layers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - AExchange server connector block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/AExchange/AExchangeServer.cs`
  - `project/OsEngine/Market/Servers/AExchange/AExchangeServerPermission.cs`
  - `project/OsEngine/Market/Servers/AExchange/Json/Accounts.cs`
  - `project/OsEngine/Market/Servers/AExchange/Json/AccountState.cs`
  - `project/OsEngine/Market/Servers/AExchange/Json/CancelOrderMessage.cs`
  - `project/OsEngine/Market/Servers/AExchange/Json/Error.cs`
  - `project/OsEngine/Market/Servers/AExchange/Json/InstrumentDefinition.cs`
  - `project/OsEngine/Market/Servers/AExchange/Json/LoginMessage.cs`
  - `project/OsEngine/Market/Servers/AExchange/Json/OrderStatus.cs`
  - `project/OsEngine/Market/Servers/AExchange/Json/PlaceOrderMessage.cs`
  - `project/OsEngine/Market/Servers/AExchange/Json/PositionUpdate.cs`
  - `project/OsEngine/Market/Servers/AExchange/Json/Quotes.cs`
  - `project/OsEngine/Market/Servers/AExchange/Json/SubscribeOnQuoteMessage.cs`
  - `project/OsEngine/Market/Servers/AExchange/Json/Trade.cs`
  - `project/OsEngine/Market/Servers/AExchange/Json/WebSocketMessageBase.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: large nullable adoption pass for AExchange connector and transport DTO layers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - MoexFixFastTwimeFutures server block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServerPermission.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/Entity/ControlFastDepth.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/Entity/FixMessageConstructor.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/Entity/IDsModifiedFixOrders.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/Entity/MarketDataGroup.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/Entity/NumbersMD.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/Entity/OrderChange.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/Entity/ReplyTwimeMessage.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/Entity/Snapshot.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/Entity/TwimeMessageConstructor.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/Entity/TwimeOrderReport.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: large nullable adoption pass for MoexFixFastTwimeFutures connector and message model layer without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - OKX server connector block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/OKX/OkxServer.cs`
  - `project/OsEngine/Market/Servers/OKX/OkxServerPermission.cs`
  - `project/OsEngine/Market/Servers/OKX/Entity/CandlesResponse.cs`
  - `project/OsEngine/Market/Servers/OKX/Entity/Encryptor.cs`
  - `project/OsEngine/Market/Servers/OKX/Entity/HttpInterceptor.cs`
  - `project/OsEngine/Market/Servers/OKX/Entity/RequestSubscribe.cs`
  - `project/OsEngine/Market/Servers/OKX/Entity/ResponseRestMessage.cs`
  - `project/OsEngine/Market/Servers/OKX/Entity/ResponseWsMessageAction.cs`
  - `project/OsEngine/Market/Servers/OKX/Entity/SecurityResponse.cs`
  - `project/OsEngine/Market/Servers/OKX/Entity/TradeDetailsResponse.cs`
  - `project/OsEngine/Market/Servers/OKX/Entity/TradesDataResponse.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Adjusted nullable usage in tests for new constructor contract:
  - `project/OsEngine.Tests/OkxHttpInterceptorTests.cs` (`myProxy: null!` in two ctor calls)
- Scope: large nullable adoption pass for OKX connector and transport DTO layers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - KuCoin server connector block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServer.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServerPermission.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/Json/RequestMessagesRest.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/Json/ResponseMessageRest.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/Json/ResponseWebSocketMessageAction.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/KuCoinSpotServer.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/KuCoinSpotServerPermission.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/Json/RequestMessagesRest.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/Json/ResponseMessageRest.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/Json/ResponseWebSocketMessageAction.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: large nullable adoption pass for KuCoin futures/spot connector and transport DTO layers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - MoexFixFastCurrency server block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServerPermission.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastCurrency/Entity/FastConnection.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastCurrency/Entity/IDsModifiedOrder.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastCurrency/Entity/MessageConstructor.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastCurrency/Entity/NumbersData.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastCurrency/Entity/OrderChange.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastCurrency/Entity/RejectMessage.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastCurrency/Entity/Snapshot.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastCurrency/Entity/WaitingTrade.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: large nullable adoption pass for MoexFixFastCurrency connector and message model layer without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - BingX server connector block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXFuturesServerPermission.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXFutures/Entity/ListenKeyBingXFutures.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXFutures/Entity/ResponseFuturesBingX.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXFutures/Entity/ResponseWSBingXFuturesMessage.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXSpotServerPermission.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXSpot/Entity/ListenKeyBingX.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXSpot/Entity/ResponseSpotBingX.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXSpot/Entity/ResponseWebSocketBingXMessage.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: large nullable adoption pass for BingX futures/spot connector and transport DTO layers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Bitfinex server connector block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/BitfinexFuturesServer.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/BitfinexFuturesServerPermission.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/Json/BitfinexFuturesCandle.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/Json/BitfinexFuturesOrder.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/Json/BitfinexFuturesWebsocketTrades.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServerPermission.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/Json/BitfinexCandle.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/Json/BitfinexOrder.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/Json/BitfinexWebsocketTrades.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: large nullable adoption pass for Bitfinex futures/spot connector and transport DTO layers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - XT and AscendEX server connector blocks

- Updated nullable context in XT:
  - `project/OsEngine/Market/Servers/XT/XTFutures/XTFuturesServer.cs`
  - `project/OsEngine/Market/Servers/XT/XTFutures/XTFuturesServerPermission.cs`
  - `project/OsEngine/Market/Servers/XT/XTFutures/Entity/XTFuturesResponseRest.cs`
  - `project/OsEngine/Market/Servers/XT/XTFutures/Entity/XTFuturesResponseWebSocket.cs`
  - `project/OsEngine/Market/Servers/XT/XTSpot/XTServerSpot.cs`
  - `project/OsEngine/Market/Servers/XT/XTSpot/XTSpotServerPermission.cs`
  - `project/OsEngine/Market/Servers/XT/XTSpot/Entity/RequestMessagesRest.cs`
  - `project/OsEngine/Market/Servers/XT/XTSpot/Entity/ResponseMessageRest.cs`
  - `project/OsEngine/Market/Servers/XT/XTSpot/Entity/ResponseWebSocketMessageAction.cs`
- Updated nullable context in AscendEX:
  - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/AscendexSpotServer.cs`
  - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/AscendexSpotServerPermission.cs`
  - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/Entity/AscendexSpotAccountInfo.cs`
  - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/Entity/AscendexSpotCandle.cs`
  - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/Entity/AscendexSpotDepth.cs`
  - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/Entity/AscendexSpotOrderAndPortfolioWebsocket.cs`
  - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/Entity/AscendexSpotOrderRest.cs`
  - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/Entity/AscendexSpotPublicTrades.cs`
  - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/Entity/AscendexSpotSecurity.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: combined large nullable adoption pass for XT and AscendEX connectors and transport DTO layers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - CoinEx server connector block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/CoinEx/Futures/CoinExServerFutures.cs`
  - `project/OsEngine/Market/Servers/CoinEx/Futures/CoinExServerFuturesPermission.cs`
  - `project/OsEngine/Market/Servers/CoinEx/Futures/Entity/ResponseRestMessage.cs`
  - `project/OsEngine/Market/Servers/CoinEx/Futures/Entity/ResponseWebSocketMessage.cs`
  - `project/OsEngine/Market/Servers/CoinEx/Spot/CoinExServerSpot.cs`
  - `project/OsEngine/Market/Servers/CoinEx/Spot/CoinExServerSpotPermission.cs`
  - `project/OsEngine/Market/Servers/CoinEx/Spot/Entity/ResponseRestMessage.cs`
  - `project/OsEngine/Market/Servers/CoinEx/Spot/Entity/ResponseWebSocketMessage.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: large nullable adoption pass for CoinEx futures/spot connector and transport DTO layers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Transaq and server Entity blocks

- Updated nullable context in Transaq:
  - `project/OsEngine/Market/Servers/Transaq/TransaqServer.cs`
  - `project/OsEngine/Market/Servers/Transaq/TransaqServerPermission.cs`
  - `project/OsEngine/Market/Servers/Transaq/ChangeTransaqPassword.xaml.cs`
  - `project/OsEngine/Market/Servers/Transaq/TransaqEntity/InfoActiveOrder.cs`
  - `project/OsEngine/Market/Servers/Transaq/TransaqEntity/TransaqEntities.cs`
  - `project/OsEngine/Market/Servers/Transaq/TransaqEntity/TransaqPortfolio.cs`
  - `project/OsEngine/Market/Servers/Transaq/TransaqEntity/TransaqPositions.cs`
- Updated nullable context in server Entity helpers:
  - `project/OsEngine/Market/Servers/Entity/BidAskSender.cs`
  - `project/OsEngine/Market/Servers/Entity/MarshalUTF8.cs`
  - `project/OsEngine/Market/Servers/Entity/OrderSender.cs`
  - `project/OsEngine/Market/Servers/Entity/RateGate.cs`
  - `project/OsEngine/Market/Servers/Entity/ServerParameter.cs`
  - `project/OsEngine/Market/Servers/Entity/ServerWorkingTimeSettings.cs`
  - `project/OsEngine/Market/Servers/Entity/TimeManager.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: combined nullable adoption pass for Transaq connector and shared server Entity primitives without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - MoexAlgopack, TraderNet and Plaza blocks

- Updated nullable context in MoexAlgopack:
  - `project/OsEngine/Market/Servers/MoexAlgopack/MoexAlgopackServer.cs`
  - `project/OsEngine/Market/Servers/MoexAlgopack/MoexAlgopackServerPermission.cs`
  - `project/OsEngine/Market/Servers/MoexAlgopack/Entity/MoexAlgopackAuth.cs`
  - `project/OsEngine/Market/Servers/MoexAlgopack/Entity/ResponseCandles.cs`
  - `project/OsEngine/Market/Servers/MoexAlgopack/Entity/ResponseDepth.cs`
  - `project/OsEngine/Market/Servers/MoexAlgopack/Entity/ResponseSecurities.cs`
  - `project/OsEngine/Market/Servers/MoexAlgopack/Entity/ResponseTrades.cs`
- Updated nullable context in TraderNet:
  - `project/OsEngine/Market/Servers/TraderNet/TraderNetServer.cs`
  - `project/OsEngine/Market/Servers/TraderNet/TraderNetServerPermission.cs`
  - `project/OsEngine/Market/Servers/TraderNet/Entity/RequestCandle.cs`
  - `project/OsEngine/Market/Servers/TraderNet/Entity/RequestSecurity.cs`
  - `project/OsEngine/Market/Servers/TraderNet/Entity/ResponseRestMessage.cs`
  - `project/OsEngine/Market/Servers/TraderNet/Entity/ResponseWebSocketMessageAction.cs`
- Updated nullable context in Plaza:
  - `project/OsEngine/Market/Servers/Plaza/PlazaServer.cs`
  - `project/OsEngine/Market/Servers/Plaza/PlazaServerPermission.cs`
  - `project/OsEngine/Market/Servers/Plaza/Entity/BitMask.cs`
  - `project/OsEngine/Market/Servers/Plaza/Entity/PlazaChangePriceOrderEntity.cs`
  - `project/OsEngine/Market/Servers/Plaza/Entity/PositionOnBoardSander.cs`
  - `project/OsEngine/Market/Servers/Plaza/Entity/RevisionInfo.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
- Scope: combined nullable adoption pass for MoexAlgopack, TraderNet and Plaza connectors and DTO layers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Pionex, OKXData, Deribit and GateIoData blocks

- Updated nullable context in Pionex:
  - `project/OsEngine/Market/Servers/Pionex/PionexServerSpot.cs`
  - `project/OsEngine/Market/Servers/Pionex/PionexServerSpotPermission.cs`
  - `project/OsEngine/Market/Servers/Pionex/Entity/OrdersPionexRequest.cs`
  - `project/OsEngine/Market/Servers/Pionex/Entity/ResponseMessageRest.cs`
  - `project/OsEngine/Market/Servers/Pionex/Entity/ResponseWebSocketMessage.cs`
- Updated nullable context in OKXData:
  - `project/OsEngine/Market/Servers/OKXData/OKXDataServer.cs`
  - `project/OsEngine/Market/Servers/OKXData/OKXDataServerPermission.cs`
  - `project/OsEngine/Market/Servers/OKXData/Entity/OkxCandlesResponce.cs`
  - `project/OsEngine/Market/Servers/OKXData/Entity/SecurityRespOkxData.cs`
  - `project/OsEngine/Market/Servers/OKXData/Entity/TradeComparer.cs`
- Updated nullable context in Deribit:
  - `project/OsEngine/Market/Servers/Deribit/DeribitServer.cs`
  - `project/OsEngine/Market/Servers/Deribit/DeribitServerPermission.cs`
  - `project/OsEngine/Market/Servers/Deribit/Entity/JsonRequest.cs`
  - `project/OsEngine/Market/Servers/Deribit/Entity/ResponseMessageRest.cs`
  - `project/OsEngine/Market/Servers/Deribit/Entity/ResponseWebSocketMessage.cs`
- Updated nullable context in GateIoData:
  - `project/OsEngine/Market/Servers/GateIoData/GateIoDataServer.cs`
  - `project/OsEngine/Market/Servers/GateIoData/GateIoDataServerPermission.cs`
  - `project/OsEngine/Market/Servers/GateIoData/Entity/GateDataTradeResponse.cs`
  - `project/OsEngine/Market/Servers/GateIoData/Entity/GateFutCandlesResp.cs`
  - `project/OsEngine/Market/Servers/GateIoData/Entity/GateSecurityResponse.cs`
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`, `CS8767`
- Scope: combined nullable adoption pass for Pionex, OKXData, Deribit and GateIoData connectors and DTO layers without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Market servers extended connector block

- Updated nullable context in:
  - `project/OsEngine/Market/Servers/AstsBridge/...` (4 files)
  - `project/OsEngine/Market/Servers/TelegramNews/...` (4 files)
  - `project/OsEngine/Market/Servers/Woo/...` (4 files)
  - `project/OsEngine/Market/Servers/InteractiveBrokers/...` (4 files)
  - `project/OsEngine/Market/Servers/Finam/...` (4 files)
  - `project/OsEngine/Market/Servers/ExMo/...` (4 files)
  - `project/OsEngine/Market/Servers/YahooFinance/...` (4 files)
  - `project/OsEngine/Market/Servers/Bybit/...` (4 files)
  - `project/OsEngine/Market/Servers/BinanceData/...` (4 files)
  - `project/OsEngine/Market/Servers/Pionex/...` (5 files)
  - `project/OsEngine/Market/Servers/OKXData/...` (5 files)
  - `project/OsEngine/Market/Servers/Deribit/...` (5 files)
  - `project/OsEngine/Market/Servers/GateIoData/...` (5 files)
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8620`, `CS8622`, `CS8625`, `CS8629`, `CS8767`
- Scope: combined nullable adoption pass for remaining medium-size market connector blocks without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Remaining Market/Servers final block

- Updated nullable context in remaining `Market/Servers` files (53 files total):
  - Root server infrastructure (`project/OsEngine/Market/Servers/*.cs`) — 11 files
  - `Atp` — 3 files
  - `BitGetData` — 3 files
  - `BybitData` — 3 files
  - `FinamGrpc` — 2 files
  - `FixProtocolEntities` — 3 files
  - `MetaTrader5` — 2 files
  - `MFD` — 2 files
  - `MOEX` — 2 files
  - `NinjaTrader` — 2 files
  - `Optimizer` — 3 files
  - `Polygon` — 3 files
  - `QscalpMarketDepth` — 2 files
  - `QuikLua` — 3 files
  - `RSSNews` — 2 files
  - `SmartLabNews` — 2 files
  - `Tester` — 3 files
  - `TInvest` — 2 files
- Added `#nullable enable` to incremental-adoption files.
- Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
  - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8620`, `CS8622`, `CS8625`, `CS8629`, `CS8767`
- Added compatibility warning handling surfaced by this block:
  - `project/OsEngine/Market/Servers/QuikLua/Entity/CustomTraceListener.cs`: added `CS8765` to local nullable-suppression set
  - `project/OsEngine/OsOptimizer/OptEntity/ServerLifecycleManager.cs`: added local `#pragma warning disable CS8604` to preserve existing behavior in nullable context
- Scope: final nullable adoption pass for remaining server infrastructure and connectors without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.2 (nullable annotations) - Full project coverage

- Completed nullable context adoption for all remaining C# files in `project`, including full `project/OsEngine.Tests` inventory (132 files).
- Applied `#nullable enable` and staged nullable-warning suppression for legacy behavior-preserving migration in the final block.
- Final repository-level nullable coverage check for `project/*.cs`:
  - Total files checked: 1113
  - Files missing `#nullable enable`: 0
- Scope: completed broad nullable migration pass across runtime and test projects without behavior changes.

### Verification

- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343 (with known NU1900 feed warning)

## 2026-02-20 - Step 4.3 (dependency migration) - LiteDB package migration

- Migrated LiteDB from local binary reference to NuGet package in:
  - `project/OsEngine/OsEngine.csproj`
- Changes applied:
  - Removed legacy `Reference Include="LiteDB, Version=5.0.19.0..."` with `HintPath` to `bin\Debug\LiteDB.dll`
  - Added `PackageReference Include="LiteDB" Version="5.0.19"`
- Updated dependency inventory status:
  - `DEPENDENCIES.md` now marks LiteDB as migrated to PackageReference
- Scope: Step 4.3 first concrete package migration without behavior changes.

### Verification

- `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
- `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-20 - Step 4.3 (dependency migration) - LiteDB upgrade to latest stable

- Upgraded LiteDB PackageReference in:
  - `project/OsEngine/OsEngine.csproj`
- Version change:
  - `LiteDB` `5.0.19` -> `5.0.21` (latest stable on NuGet at the time of update)
- Updated dependency inventory note:
  - `DEPENDENCIES.md` now reflects `PackageReference Include="LiteDB" Version="5.0.21"`
- Scope: dependency version update only; no behavioral code changes.

### Verification

- `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
- `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-20 - Step 4.3 (dependency migration) - RestSharp package migration

- Migrated RestSharp from local binary reference to NuGet package in:
  - `project/OsEngine/OsEngine.csproj`
- Final package version selected:
  - `RestSharp` `106.15.0`
- Notes on version choice:
  - Initial parity attempt with `105.2.3` produced `NU1903` (known vulnerability) and `NU1701` (framework compatibility warning).
  - Upgraded to `106.15.0` to keep compatibility and remove those warnings.
- Updated dependency inventory status:
  - `DEPENDENCIES.md` now marks RestSharp as migrated to `PackageReference` (`106.15.0`).
- Scope: Step 4.3 migration block with dependency source change only; no behavioral code edits.

### Verification

- `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-20 - Step 4.3 (dependency migration) - Jayrock.Json removal

- Replaced Jayrock usage in Alor socket DTO:
  - `project/OsEngine/Market/Servers/Alor/Json/SocketMessageBase.cs`
- Code change:
  - `using Jayrock.Json` -> `using Newtonsoft.Json.Linq`
  - `JsonObject data` -> `JObject data`
- Removed legacy binary reference from project file:
  - deleted `Reference Include="Jayrock.Json"` with `HintPath` from `project/OsEngine/OsEngine.csproj`
- Updated dependency inventory:
  - `DEPENDENCIES.md` marks `Jayrock.Json` as removed from project references.
- Scope: Step 4.3 dependency cleanup; runtime behavior preserved (data payload still forwarded via `ToString()`).

### Verification

- `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
- `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-20 - Step 4.3 (dependency migration) - TInvestApi ProjectReference

- Migrated TInvest API dependency from binary `HintPath` reference to project reference in:
  - `project/OsEngine/OsEngine.csproj`
- Changes applied:
  - Removed legacy `Reference Include="TInvestApi"` with `HintPath` to `bin\Debug\TInvestApi.dll`
  - Added `ProjectReference Include="..\\..\\related projects\\TInvestApi\\TInvestApi.csproj"`
- Dependency governance update:
  - `DEPENDENCIES.md` marks `TInvestApi` as migrated to `ProjectReference`.
  - `FinamApi` remains binary-referenced for now because required proto source tree (`related projects/FinamApi/finam-trade-api/proto/...`) is not present in current checkout.
- Scope: Step 4.3 dependency source migration without runtime behavior changes.

### Verification

- `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
- `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-20 - Step 4.3 (dependency migration) - FinamApi conditional ProjectReference fallback

- Added robust dual-mode dependency wiring for Finam API in:
  - `project/OsEngine/OsEngine.csproj`
- Changes applied:
  - Replaced unconditional binary `Reference Include="FinamApi"` with conditional item groups:
    - If proto tree exists (`related projects/FinamApi/finam-trade-api/proto/grpc/tradeapi/v1`) -> use `ProjectReference` to `related projects/FinamApi/FinamApi.csproj`
    - Otherwise -> keep binary fallback (`HintPath` to `bin\\Debug\\FinamApi.dll`)
- Updated dependency inventory:
  - `DEPENDENCIES.md` marks FinamApi as hybrid (projectref with binary fallback).
- Scope: Step 4.3 resilience improvement; no runtime behavior changes.

### Verification

- `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
- `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-20 - Step 4.3 (dependency migration) - Remaining DLL migration feasibility audit

- Audited remaining legacy DLL references for NuGet migration feasibility:
  - `BytesRoad.Net.Ftp` -> no package found (`BlobNotFound`)
  - `BytesRoad.Net.Sockets` -> no package found (`BlobNotFound`)
  - `MtApi5` -> no package found (`BlobNotFound`)
  - `cgate_net64` -> no package found (`BlobNotFound`)
  - `OpenFAST` -> package exists, but latest `1.0.0` is older than in-repo binary `1.1.3.0`
  - `QUIKSharp` -> package exists, but current project dependency is a modified fork (see `related projects/QuikSharp/README.txt`)
- Updated dependency governance document with these findings:
  - `DEPENDENCIES.md`
- Scope: completed migration-feasibility justification for remaining non-migrated DLLs.

## 2026-02-20 - Step 4.3 (dependency migration) - Remove migrated legacy DLLs from repo tracking

- Removed migrated legacy DLL artifacts from git tracking in `project/OsEngine/bin/Debug`:
  - `Jayrock.Json.dll`
  - `LiteDB.dll`
  - `RestSharp.dll`
  - `TInvestApi.dll`
- Purpose: finalize Step 4.3 action item to stop versioning obsolete binary copies after migration to PackageReference/ProjectReference.
- Updated dependency inventory to reflect removal status:
  - `DEPENDENCIES.md`
- Scope: repository hygiene cleanup only; runtime behavior unchanged.

### Verification

- `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
- `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
- `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
- `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed 343/343

## 2026-02-20 - Step 3.1 (optimizer performance) - Indicator result cache implementation

- Added shared optimizer indicator cache class:
  - `project/OsEngine/OsOptimizer/OptEntity/IndicatorCache.cs`
- Integrated indicator cache lifecycle into optimizer executor:
  - `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`
  - initialize per optimization run (`PrepareIndicatorCache`) with bounded capacity (`maxEntries = max(256, threads*128)`)
  - deterministic eviction policy in cache: clear-all when entry limit is reached
  - cleanup on run finalization (`DisposeIndicatorCache`)
- Integrated cache hit/miss flow into indicator full recalculation path:
  - `project/OsEngine/Indicators/Aindicator.cs`
  - `ProcessAll(List<Candle>)` now:
    - tries cache restore first in optimizer mode only (`StartProgram.IsOsOptimizer`)
    - computes normally on miss and stores `DataSeries` snapshot on completion
  - cache key includes:
    - indicator type
    - parameter hash
    - data-series/include-indicator shape
    - source identity (`RuntimeHelpers.GetHashCode(candles)`) + candle range fingerprint (count, timeframe step, first/middle/last OHLCV + time range)
  - cached values are cloned on set/get to prevent shared mutable state between bots.
- Scope guard preserved:
  - cache branch is active only for optimizer mode; non-optimizer execution path remains unchanged.

### Verification

- Host-context verification (outside sandbox due intermittent sandbox TLS/NuGet issue):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `343/343`

## 2026-02-22 - Governance: automated upstream replay-audit script

- Added automation script:
  - `tools/audit-upstream-replay.ps1`
- Purpose:
  - run post-merge replay checks for upstream-attributed lines only (via `git blame` attribution against merge second-parent range).
  - detect potential regressions for configured plan-related pattern groups:
    - Step 0.3: silent catches
    - Step 2.2: culture-sensitive parse patterns
    - Step 4.1: `new object()` lock fields
- Verification run:
  - `pwsh -NoProfile -File tools/audit-upstream-replay.ps1 -MergeCommit 733b909d5` -> `OK` (0 findings)
  - `pwsh -NoProfile -File tools/audit-upstream-replay.ps1 -MergeCommit HEAD` -> `OK` (0 findings)

## 2026-02-22 - Governance: tracked-binary guard script for upstream sync

- Added guard script to reduce repeated manual cleanup after upstream merges:
  - `tools/check-tracked-debug-binaries.ps1`
- Default mode validates only critical targets:
  - `project/OsEngine/bin/Debug/OsEngine.dll`
  - `project/OsEngine/bin/Debug/OsEngine.exe`
- Optional strict mode:
  - `-StrictAllDebugBinaries` checks all `project/OsEngine/bin/Debug/*.dll|*.exe`
  - currently expected to fail because repository still tracks historical vendor binaries in that directory.
- Verification:
  - `pwsh -NoProfile -File tools/check-tracked-debug-binaries.ps1` -> `OK` (target binaries not tracked)
  - `pwsh -NoProfile -File tools/check-tracked-debug-binaries.ps1 -StrictAllDebugBinaries` -> fails with explicit tracked list (expected baseline behavior)

## 2026-02-22 - Upstream replay audit checkpoint (Steps 0.3 / 2.2 / 4.1)

- Performed targeted replay audit against integrated upstream range:
  - merge range: `733b909d5^1..733b909d5^2`
  - checked patterns mapped to refactoring plan:
    - Step 2.2: `decimal/double/float Parse/TryParse`, culture-dependent replacements
    - Step 4.1: `new object()` lock fields introduced by upstream
    - Step 0.3: added silent catches `catch { }`
- Result:
  - no remaining upstream-authored occurrences for the above pattern groups after incremental fixes `#334..#338`.
- Build/test safety check (host context, outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (InvariantCulture in persistence) - Journal benchmark import decimal parse hardening

- Replayed Step 2.2 on upstream-affected journal benchmark load path:
  - `project/OsEngine/Journal/JournalUi2.xaml.cs`
  - in `LoadBenchmarkData(...)` replaced:
    - `decimal.Parse(parts[5].Replace(".", ","))`
    - -> `ParseDecimalInvariantOrCurrent(parts[5])`
  - added local helper:
    - `ParseDecimalInvariantOrCurrent(string value)` with parse cascade:
      - `InvariantCulture` (`NumberStyles.Any`)
      - `CurrentCulture` fallback
      - final `Convert.ToDecimal(value)` fallback for legacy behavior parity.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - ATP legacy security expiration parsing hardening

- Standardized legacy expiration-date parsing in:
  - `project/OsEngine/Market/Servers/Atp/AtpServer.cs`
- Changes:
  - in `ParseLegacySecurityLine(...)`, replaced unqualified
    `DateTime.TryParse(array[16], out expiration)` with helper parse method.
  - added helper `ParseDateInvariantOrCurrent(string value)` with parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - final fallback is safe/non-throwing:
    - `DateTime.MinValue`.
- Scope:
  - parsing hardening only
  - ATP security import behavior unchanged for valid data.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - BybitData filename date parsing hardening

- Standardized filename-date parsing in:
  - `project/OsEngine/Market/Servers/BybitData/BybitDataServer.cs`
- Changes:
  - in `ExtractDateFromFileName(...)`, replaced unqualified:
    - `DateTime.TryParse(match.Groups[1].Value, out date)`
    - with `TryParseDateInvariantOrCurrent(...)`.
  - added helper `TryParseDateInvariantOrCurrent(string value, out DateTime date)` with parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - helper remains non-throwing; method still returns `null` when parse fails.
- Scope:
  - parsing hardening only
  - behavior unchanged for valid filename dates.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - TraderNet datetime parsing hardening

- Standardized datetime parsing in:
  - `project/OsEngine/Market/Servers/TraderNet/TraderNetServer.cs`
- Changes:
  - replaced all remaining unqualified `DateTime.TryParse(...)` usages in:
    - order update trade mapping
    - public trades stream timestamp mapping
    - order status trade mapping
    - order callback timestamp mapping
  - added shared helper `ParseDateInvariantOrCurrent(string value)` with parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - helper final fallback is non-throwing:
    - `DateTime.MinValue` (parity with prior graceful behavior on parse failures).
- Scope:
  - parsing hardening only
  - TraderNet behavior unchanged for valid timestamps.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Polygon trade timestamp decimal parsing hardening

- Standardized decimal parsing for trade timestamp payload in:
  - `project/OsEngine/Market/Servers/Polygon/PolygonServer.cs`
- Changes:
  - in `ConvertTrades(...)`, replaced unqualified:
    - `decimal.TryParse(response.results[i].sip_timestamp, out timestamp)`
    - with `ParseDecimalInvariantOrCurrent(...)`.
  - added helper `ParseDecimalInvariantOrCurrent(string value)` with parse order:
    - `Invariant -> Current -> ru-RU`
  - helper final fallback:
    - `0m` (parity with prior default-on-fail behavior of `decimal.TryParse`).
- Scope:
  - parsing hardening only
  - trade timestamp conversion behavior unchanged for valid payload values.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Extensions ToDouble fallback parsing hardening

- Standardized numeric fallback parsing in:
  - `project/OsEngine/Entity/Extensions.cs`
- Changes:
  - in `ToDouble(string? value)`, catch-fallback replaced:
    - `double.TryParse(value, out result)`
    - with `TryParseDoubleInvariantOrCurrent(value, out result)`.
  - added helper `TryParseDoubleInvariantOrCurrent(string value, out double result)` with parse order:
    - `Invariant -> Current -> ru-RU`
  - helper uses `NumberStyles.Float | NumberStyles.AllowThousands`.
  - fallback behavior preserved:
    - returns `0` when parsing fails.
- Scope:
  - parsing hardening only
  - conversion behavior unchanged for valid numeric values.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`


## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Transaq server datetime parsing hardening

- Standardized Transaq datetime parsing in:
  - `project/OsEngine/Market/Servers/Transaq/TransaqServer.cs`
- Changes:
  - replaced all remaining `DateTime.Parse(...)` calls in:
    - tick history load
    - candle parsing
    - news timestamp mapping
    - my-trades updates
    - order callback timestamps
    - public trades updates
  - added shared helper `ParseDateInvariantOrCurrentOrThrow(string value)` with parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - parse failures throw `FormatException`, preserving existing outer-thread error handling behavior.
- Scope:
  - parsing hardening only
  - Transaq behavior unchanged for valid timestamps.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - MoexAlgopack explicit datetime parse cleanup

- Standardized explicit datetime parsing in:
  - `project/OsEngine/Market/Servers/MoexAlgopack/MoexAlgopackServer.cs`
- Changes:
  - replaced explicit invariant parse calls:
    - `DateTime.Parse(item[7], CultureInfo.InvariantCulture)`
    - `DateTime.Parse(item[6], CultureInfo.InvariantCulture)`
  - both paths now use existing helper:
    - `ParseDateInvariantOrCurrent(...)`
  - aligns securities/candles parsing with already hardened trades/depth parsing paths in this server.
- Scope:
  - parsing hardening only
  - MOEX Algopack behavior unchanged for valid timestamps.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - TInvest constant datetime parse cleanup

- Standardized fixed-value datetime comparison in:
  - `project/OsEngine/Market/Servers/TInvest/TInvestServer.cs`
- Changes:
  - replaced `DateTime.Parse("01.01.1970 03:00:00")` with explicit constant:
    - `new DateTime(1970, 1, 1, 3, 0, 0)`
  - avoids locale-dependent parsing for sentinel timestamp comparison.
- Scope:
  - parsing hardening only
  - TInvest trade-time fallback logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Journal benchmark axis-label parsing hardening

- Standardized benchmark axis-label date parsing in:
  - `project/OsEngine/Journal/JournalUi2.xaml.cs`
- Changes:
  - replaced direct `DateTime.Parse(series.Points[...].AxisLabel)` in benchmark validation/alignment flow.
  - reused existing helper `ParseDateInvariantOrCurrentOrThrow(string value)` with parse order:
    - `Invariant Roundtrip -> Invariant -> _currentCulture -> ru-RU`
  - parse failures preserve existing behavior through outer method-level error handling (log + `null`).
- Scope:
  - parsing hardening only
  - journal benchmark behavior unchanged for valid axis-label timestamps.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Benchmark axis-label date parsing hardening

- Standardized benchmark axis-label date parsing in:
  - `project/OsEngine/OsData/Benchmark.cs`
- Changes:
  - replaced direct `DateTime.Parse(_series.Points[...].AxisLabel)` in benchmark interval calculation.
  - added helper `ParseDateInvariantOrCurrentOrThrow(string value)` with parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - parse failure throws `FormatException`, preserving existing outer error-handling/logging behavior.
- Scope:
  - parsing hardening only
  - benchmark data download behavior unchanged for valid timeline labels.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - AServerTester start-date parsing hardening

- Standardized test start-date parsing in:
  - `project/OsEngine/Robots/AutoTestBots/ServerTests/AServerTester.cs`
- Changes:
  - replaced direct `DateTime.Parse(...)` in:
    - `Data_1` tester start date assignment
    - `Data_4` tester start date assignment
  - added helper `ParseDateInvariantOrCurrentOrThrow(string value)` with parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - parse failures preserve existing fail-fast behavior via existing worker-thread exception handling.
- Scope:
  - parsing hardening only
  - tester behavior unchanged for valid date input.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - LqdtDataFakeServer key-rate date parsing hardening

- Standardized key-rate date parsing in:
  - `project/OsEngine/OsData/LqdtDataFakeServer.cs`
- Changes:
  - replaced unqualified `DateTime.TryParse(...)` and `DateTime.Parse(...)` usage in file/XML key-rate loaders.
  - added helpers:
    - `TryParseDateInvariantOrCurrent(string value, out DateTime parsedDate)`
    - `ParseDateInvariantOrCurrent(string value)`
  - parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - malformed/invalid source rows are skipped explicitly:
    - missing date-rate split parts (`Length < 2`)
    - invalid XML date parsed as `DateTime.MinValue` and ignored.
- Scope:
  - parsing hardening only
  - key-rate loading behavior unchanged for valid data.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - FundBalanceDivergenceBot signal date parsing hardening

- Standardized signal date parsing in:
  - `project/OsEngine/Robots/OnScriptIndicators/FundBalanceDivergenceBot.cs`
- Changes:
  - in close-position logic, replaced direct
    `Convert.ToDateTime(position.SignalTypeOpen)` with helper parser.
  - added helper `ParseDateInvariantOrCurrent(string value)` with parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - final fallback is safe/non-throwing:
    - `DateTime.MinValue`.
- Scope:
  - parsing hardening only
  - strategy behavior unchanged for valid signal timestamps.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - CandlePatternBoost signal date parsing hardening

- Standardized signal date parsing in:
  - `project/OsEngine/Robots/Patterns/CandlePatternBoost.cs`
- Changes:
  - in candle-count exit logic, replaced direct
    `Convert.ToDateTime(position.SignalTypeOpen)` with helper parser.
  - added helper `ParseDateInvariantOrCurrent(string value)` with parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - final fallback is safe/non-throwing:
    - `DateTime.MinValue`.
- Scope:
  - parsing hardening only
  - strategy behavior unchanged for valid signal timestamps.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Journal range textbox date parsing hardening

- Standardized journal range-date parsing in:
  - `project/OsEngine/Journal/JournalUi2.xaml.cs`
- Changes:
  - in `TextBoxTo_TextChanged(...)` and `TextBoxFrom_TextChanged(...)`, replaced direct
    `Convert.ToDateTime(..., _currentCulture)` with helper parser.
  - added helper `TryParseDateInvariantOrCurrent(string value, out DateTime time)` with parse order:
    - `Invariant Roundtrip -> Invariant -> _currentCulture -> ru-RU`
  - added helper `ParseDateInvariantOrCurrentOrThrow(string value)` to preserve existing fallback behavior via surrounding `catch`.
- Scope:
  - parsing hardening only
  - journal filtering UI behavior unchanged for valid date input.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Log old-session date parsing hardening

- Standardized old-session log timestamp parsing in:
  - `project/OsEngine/Logging/Log.cs`
- Changes:
  - replaced direct `Convert.ToDateTime(msgArray[0])` while replaying saved log lines.
  - added helper `TryParseDateInvariantOrCurrent(string value, out DateTime time)` with parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - parse failures keep existing behavior:
    - line is skipped (`continue`) without exception propagation.
- Scope:
  - parsing hardening only
  - log storage format and runtime logging behavior unchanged for valid lines.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - VWAP legacy date parser safe final fallback

- Standardized legacy VWAP date-parser fallback in:
  - `project/OsEngine/Charts/CandleChart/Indicators/VWAP.cs`
- Changes:
  - in `ParseLegacyDateTime(...)`, replaced final fallback:
    - `Convert.ToDateTime(value, CultureInfo.CurrentCulture)` -> `DateTime.MinValue`
  - parse order preserved:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
- Scope:
  - parsing hardening only
  - VWAP settings load behavior unchanged for valid date values.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Tester UI date parsing hardening

- Standardized tester date parsing in:
  - `project/OsEngine/Market/Servers/Tester/TesterServerUi.xaml.cs`
- Changes:
  - replaced direct `Convert.ToDateTime(...)` in:
    - `_gridNonTradePeriods_CellValueChanged(...)` for start/end date columns
    - `_timer_TextBoxTo(...)` and `_timer_TextBoxFrom(...)` for tester interval textboxes
  - added helper `TryParseDateInvariantOrCurrent(string value, out DateTime time)` with parse order:
    - `Invariant Roundtrip -> Invariant -> _currentCulture -> ru-RU`
  - added helper `ParseDateInvariantOrCurrentOrThrow(string value)` for timer flows to preserve existing rollback behavior via surrounding `catch`.
- Scope:
  - parsing hardening only
  - tester UI behavior unchanged for valid date input.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - OptimizerDataStorage non-trade date parsing hardening

- Standardized non-trade-period date parsing in:
  - `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorageUi.xaml.cs`
- Changes:
  - in `_gridNonTradePeriods_CellValueChanged(...)`, replaced direct
    `Convert.ToDateTime(value, OsLocalization.CurCulture)` with helper parse method.
  - added helper `TryParseDateInvariantOrCurrent(string value, out DateTime time)` with parse order:
    - `Invariant Roundtrip -> Invariant -> _currentCulture -> ru-RU`
  - invalid date values preserve existing behavior:
    - change is ignored (`return`) and persisted settings are not modified.
- Scope:
  - parsing hardening only
  - optimizer data storage UI behavior unchanged for valid date input.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Optimizer phase date parsing hardening

- Standardized optimizer phase date parsing in:
  - `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
- Changes:
  - in `_gridFazes_CellValueChanged(...)`, replaced direct
    `Convert.ToDateTime(..., _currentCulture)` with helper parse method.
  - added helper `ParseDateInvariantOrCurrentOrThrow(string value)` with parse order:
    - `Invariant Roundtrip -> Invariant -> _currentCulture -> ru-RU`
  - if parsing fails, helper throws `FormatException`, preserving existing UI rollback behavior via `catch`.
- Scope:
  - parsing hardening only
  - optimizer phase-editing behavior unchanged for valid date input.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Security editor expiration date parsing hardening

- Standardized expiration-date parsing in:
  - `project/OsEngine/Entity/SecurityUi.xaml.cs`
- Changes:
  - replaced direct `Convert.ToDateTime(TextBoxExpiration.Text)` with helper parse method.
  - added helper `ParseDateInvariantOrCurrentOrThrow(string value)` with parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - if parsing fails, helper throws `FormatException`, preserving existing error handling in UI `try/catch`.
- Scope:
  - parsing hardening only
  - security editor flow unchanged for valid date input.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - MOEX ISS candle timestamp parsing hardening

- Standardized candle timestamp parsing in:
  - `project/OsEngine/Market/Servers/MOEX/MoexIssDataServer.cs`
- Changes:
  - replaced direct `Convert.ToDateTime(innerArray[6].ToString())` in candle loading with helper parse method.
  - added helper `ParseDateInvariantOrCurrent(string value)` with parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - final fallback is safe/non-throwing:
    - `DateTime.MinValue`.
- Scope:
  - parsing hardening only
  - MOEX ISS candle-loading behavior unchanged for valid timestamps.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - BingX fill-order timestamp parsing hardening

- Standardized fill-order timestamp parsing in:
  - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
- Changes:
  - replaced direct `Convert.ToDateTime(...)` usage for `filledTime` in `GetMyTradesBySecurity(...)`.
  - added helper `ParseTimestampOrDateInvariantOrCurrent(string value)` with parse order:
    - unix timestamp (milliseconds/seconds) -> `TimeManager.GetDateTimeFromTimeStamp`
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - final fallback is safe/non-throwing:
    - `DateTime.MinValue`.
- Scope:
  - parsing hardening only
  - BingX futures trade event handling unchanged for valid timestamps.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - MoexAlgopack inbound date parsing hardening

- Standardized inbound date parsing in:
  - `project/OsEngine/Market/Servers/MoexAlgopack/MoexAlgopackServer.cs`
- Changes:
  - replaced direct `Convert.ToDateTime(...)` usage in:
    - market depth timestamp (`orderbook` response)
    - trades timestamp (`tqbr` branch)
    - trades timestamp (`forts` branch)
  - added shared helper `ParseDateInvariantOrCurrent(string value)` with parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - final fallback is safe/non-throwing:
    - `DateTime.MinValue`.
- Scope:
  - parsing hardening only
  - MOEX Algopack API contract handling and runtime trading behavior for valid timestamps unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Security price-step decimals culture hardening

- Standardized numeric culture handling in:
  - `project/OsEngine/Entity/Security.cs`
- Changes:
  - for internal `PriceStep` decimal-digit calculation, replaced intermediate formatting:
    - `ToString(new CultureInfo("ru-RU"))` -> `ToString(CultureInfo.InvariantCulture)`.
  - aligned decimal-part length detection to invariant dot separator in:
    - `PriceStep` setter
    - `Decimals` getter
- Scope:
  - culture hardening for numeric formatting only
  - persistence payload (`GetSaveStr/LoadFromString`) and business logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Qscalp date parsing hardening

- Standardized date parsing in:
  - `project/OsEngine/Market/Servers/QscalpMarketDepth/QscalpMarketDepthServer.cs`
- Changes:
  - replaced direct `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` in:
    - HTML date extraction
    - securities-cache file date load
  - added shared helper `ParseDateInvariantOrCurrent(string value)` with parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - final fallback is safe/non-throwing:
    - `DateTime.MinValue`.
  - removed redundant conversion of an existing `DateTime` value:
    - `_availableDates[0]` now used directly.
- Scope:
  - parsing hardening only
  - Qscalp market-depth download flow and behavior for valid dates unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - NinjaTrader inbound date parsing hardening

- Standardized inbound date parsing in:
  - `project/OsEngine/Market/Servers/NinjaTrader/NinjaTraderClient.cs`
- Changes:
  - replaced direct `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` usage in:
    - order execution callback timestamp
    - market depth timestamp
    - my trades timestamp
    - orders timestamp
    - trades timestamp
  - added shared helper `ParseDateInvariantOrCurrent(string value)` with parse order:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - final fallback is safe/non-throwing:
    - `DateTime.MinValue`.
- Scope:
  - parsing hardening only
  - NinjaTrader message contract and runtime trading logic unchanged for valid timestamps.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - ProxyMaster date parser safe final fallback

- Standardized final fallback behavior in:
  - `project/OsEngine/Market/Proxy/ProxyMaster.cs`
- Changes:
  - in `ParseDateInvariantOrCurrent(...)`, replaced final fallback:
    - `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` -> `DateTime.MinValue`.
  - main parse order remains:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
- Scope:
  - parsing hardening only
  - proxy settings schema and valid-value behavior unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Entity date parser safe final fallback

- Standardized final fallback behavior in date parsers:
  - `project/OsEngine/Entity/Order.cs`
  - `project/OsEngine/Entity/MyTrade.cs`
  - `project/OsEngine/Entity/Trade.cs`
- Changes:
  - in `ParseDate...` helpers, replaced final throwing fallback
    `Convert.ToDateTime(..., InvariantCulture)` with non-throwing `DateTime.MinValue`.
  - invariant/current/`ru-RU` parse priority remains unchanged.
- Scope:
  - parsing hardening only
  - persistence schema and business logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Order cancellation timestamp persistence hardening

- Standardized order cancellation timestamp persistence in:
  - `project/OsEngine/Entity/Order.cs`
- Changes:
  - in `GetStringForSave()`:
    - `LastCancelTryLocalTime` now serializes with `ToString("O", CultureInfo.InvariantCulture)`.
  - in `SetOrderFromString(...)`:
    - cancellation timestamp parsing switched from direct `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` to shared parser.
  - helper `ParseDateTimeInvariantWithRuFallback(...)` updated to invariant-first priority:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
- Scope:
  - persistence parsing/serialization hardening only
  - order runtime logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - TradeGrid first-trade time round-trip persistence

- Standardized TradeGrid datetime persistence in:
  - `project/OsEngine/OsTrader/Grids/TradeGrid.cs`
- Changes:
  - in `GetSaveString()`:
    - `_firstTradeTime` now serializes with `ToString("O", CultureInfo.InvariantCulture)`.
  - in `LoadFromString(...)`:
    - replaced direct `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` with helper parser using fallback chain:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
- Scope:
  - persistence parsing/serialization hardening only
  - grid logic and settings schema unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Optimizer phase time round-trip persistence

- Standardized optimizer phase datetime persistence in:
  - `project/OsEngine/OsOptimizer/OptimizerMaster.cs`
- Changes:
  - in phase `GetSaveString()`:
    - `_timeStart` and `_timeEnd` now serialize as `ToString("O", CultureInfo.InvariantCulture)`.
  - in phase `LoadFromString(...)`:
    - replaced direct `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` with helper parser using fallback chain:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
- Scope:
  - persistence parsing/serialization hardening only
  - optimizer phase logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - TesterServer legacy security test settings date parsing

- Standardized legacy settings date parsing in:
  - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
- Changes:
  - in `ParseLegacySecurityTestSettings(...)`:
    - replaced `Convert.ToDateTime(lines[0], CultureInfo)` and `Convert.ToDateTime(lines[1], CultureInfo)`
      with `ParseDateInvariantOrCurrent(...)`.
  - this aligns legacy loader behavior with the same invariant-first fallback chain already used in tester settings parsing.
- Scope:
  - parsing hardening only
  - settings schema and runtime logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - OsDataSet time parse path cleanup

- Simplified datetime load flow in:
  - `project/OsEngine/OsData/OsDataSet.cs`
- Changes:
  - in `SettingsToLoadSecurity.Load(...)`:
    - removed `try/catch` block that first attempted `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` for `TimeStart` and `TimeEnd`.
    - switched to direct use of existing helper `ParseDateInvariantOrCurrent(...)` for both fields.
- Scope:
  - parsing cleanup/hardening only
  - settings format and runtime logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - TesterServer clearing/non-trade datetime round-trip persistence

- Standardized datetime persistence in tester period models:
  - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
- Changes:
  - `OrderClearing.GetSaveString()` and `NonTradePeriod.GetSaveString()`:
    - datetime fields now serialize with `ToString("O", CultureInfo.InvariantCulture)`.
  - `OrderClearing.SetFromString(...)` and `NonTradePeriod.SetFromString(...)`:
    - replaced direct `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` with invariant-first fallback parser:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
- Scope:
  - persistence parsing/serialization hardening only
  - tester execution logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - OptimizerSettings datetime round-trip persistence

- Standardized optimizer settings datetime persistence in:
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
- Changes:
  - in `Save()`:
    - `_timeStart` and `_timeEnd` now serialize via `ToString("O", CultureInfo.InvariantCulture)`.
  - in `Load()`:
    - replaced direct `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` with helper `ParseDateInvariantOrCurrent(...)`.
    - helper uses invariant-first fallback order:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
- Scope:
  - persistence parsing/serialization hardening only
  - optimizer behavior and settings schema unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - OsDataSet base settings time round-trip serialization

- Standardized base dataset settings datetime serialization in:
  - `project/OsEngine/OsData/OsDataSet.cs`
- Changes:
  - in `SettingsToLoadSecurity.GetSaveStr()`:
    - `TimeStart` and `TimeEnd` now save via `ToString("O", CultureInfo.InvariantCulture)` instead of culture-dependent invariant default formatting.
- Scope:
  - persistence serialization hardening only
  - load flow and settings compatibility unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Screener LastUpdateTime round-trip persistence

- Standardized screener settings datetime persistence in:
  - `project/OsEngine/Robots/AlgoStart/AlgoStart2Soldiers.cs`
  - `project/OsEngine/Robots/Grids/GridScreenerAdaptiveSoldiers.cs`
  - `project/OsEngine/Robots/Screeners/ThreeSoldierAdaptiveScreener.cs`
  - `project/OsEngine/Robots/Screeners/PinBarVolatilityScreener.cs`
- Changes:
  - `LastUpdateTime` save paths now serialize with:
    - `ToString("O", CultureInfo.InvariantCulture)`
  - load paths now use invariant-first fallback parser:
    - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
  - replaced direct `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` in these settings loaders.
- Scope:
  - persistence parsing/serialization hardening only
  - screener calculation/trading logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Additional entity/proxy datetime parser harmonization

- Standardized legacy datetime parse flow in:
  - `project/OsEngine/Entity/Security.cs`
  - `project/OsEngine/Entity/PositionOpenerToStop.cs`
  - `project/OsEngine/Market/Proxy/ProxyMaster.cs`
- Changes:
  - added explicit invariant non-roundtrip parse stage (`DateTimeStyles.None`) after invariant round-trip attempts in helper parsers.
  - `ProxyMaster` parser now also checks invariant round-trip parse before existing fallback chain.
- Scope:
  - parsing hardening only
  - file formats and runtime business logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Trade/MyTrade date parse priority hardening

- Standardized date parsing priority in:
  - `project/OsEngine/Entity/MyTrade.cs`
  - `project/OsEngine/Entity/Trade.cs`
- Changes:
  - `MyTrade.ParseDateTimeInvariantWithRuFallback(...)` now checks `CultureInfo.CurrentCulture` before `ru-RU` fallback.
  - `Trade.ParseIqFeedDateInvariantOrCurrent(...)` now attempts invariant round-trip parse first (`DateTimeStyles.RoundtripKind`) before existing invariant/current/`ru-RU` parsing.
- Scope:
  - parsing hardening only
  - trade/mytrade save formats and business logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - OsDataSet duplicate/update datetime round-trip persistence

- Standardized datetime persistence in:
  - `project/OsEngine/OsData/OsDataSet.cs`
- Changes:
  - in `SetDublicator.SaveDublicateSettings(...)` and `SetUpdater.SaveUpdateSettings(...)`, datetime values now serialize using round-trip format:
    - `ToString("O", CultureInfo.InvariantCulture)`
  - in corresponding load paths:
    - `SetDublicator.LoadDublicateSettings(...)`
    - `SetUpdater.LoadUpdateSettings(...)`
    - replaced direct `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` with invariant-first fallback parser (`Invariant Roundtrip -> Invariant -> Current -> ru-RU`).
- Scope:
  - persistence parsing/serialization hardening only
  - duplicate/update scheduling logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - VWAP legacy date parse priority

- Standardized culture priority in VWAP legacy date parser:
  - `project/OsEngine/Charts/CandleChart/Indicators/VWAP.cs`
- Changes:
  - `ParseLegacyDateTime()` now parses in invariant-first order:
    - invariant round-trip -> invariant -> current culture -> `ru-RU` fallback.
  - added `using System.Globalization;`.
- Scope:
  - parsing hardening only
  - indicator calculation logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - BotTabOptions expiration-date parsing

- Standardized selected-expiration parsing in options tab UI paths:
  - `project/OsEngine/OsTrader/Panels/Tab/BotTabOptions.cs`
- Changes:
  - replaced `Convert.ToDateTime(selectedExpirationStr)` with explicit invariant-first date parser in:
    - `RedrawGrid()`
    - `RefreshOptionsGrid()`
  - parser order: invariant round-trip -> invariant -> current culture -> `ru-RU`.
- Scope:
  - parsing hardening only
  - options filtering/render behavior unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Global UI layout window coordinates serialization

- Standardized invariant serialization for saved window layouts:
  - `project/OsEngine/Layout/GlobalGUILayout.cs`
- Changes:
  - in `OpenWindow.GetSaveString()`, decimal layout fields now serialize via `ToString(CultureInfo.InvariantCulture)`:
    - `Height`, `Left`, `Top`, `Widht`.
  - load path remains `ToDecimal()` and keeps legacy compatibility.
- Scope:
  - persistence serialization hardening only
  - UI layout logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - TesterServer security expiration parse

- Standardized expiration date parsing in tester security extra settings:
  - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
- Changes:
  - replaced `Convert.ToDateTime(array[i][7])` with explicit invariant-first fallback parser.
  - parser order: invariant round-trip -> invariant -> current culture -> `ru-RU`.
- Scope:
  - parsing hardening only
  - securities dop-settings schema and trading logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - BotTabIndex formula-update timestamp persistence

- Standardized datetime persistence around index formula rebuild tracking:
  - `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs`
- Changes:
  - `_lastTimeUpdateIndex` now saves as invariant round-trip (`ToString("O", CultureInfo.InvariantCulture)`).
  - parse path now uses invariant-first fallback parser instead of culture-implicit `Convert.ToDateTime(...)`.
- Scope:
  - persistence parsing/serialization hardening only
  - index formula logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Alert chart line persistence format

- Standardized alert line persistence for culture-neutral round-trip:
  - `project/OsEngine/Alerts/AlertToChart.cs`
- Changes:
  - `ChartAlertLine.GetStringToSave()` now writes:
    - datetimes as invariant round-trip (`"O"`)
    - decimal values via `ToString(CultureInfo.InvariantCulture)`.
  - `ChartAlertLine.SetFromSaveString()` now parses datetimes with invariant-first fallback and supports legacy `ru-RU` values.
- Scope:
  - persistence serialization/parsing hardening only
  - alert signal logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - OsDataSet legacy date parse fallback

- Standardized fallback date parsing in dataset settings load:
  - `project/OsEngine/OsData/OsDataSet.cs`
- Changes:
  - in `SettingsToLoadSecurity.Load()`, replaced fallback `Convert.ToDateTime(...)` (without explicit culture) with invariant-first parser.
  - parser order: round-trip invariant -> invariant -> current culture -> `ru-RU`.
- Scope:
  - parsing hardening only
  - settings schema and update workflow unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - ProxyMaster legacy settings date parse

- Standardized legacy date parsing in proxy settings loader:
  - `project/OsEngine/Market/Proxy/ProxyMaster.cs`
- Changes:
  - replaced culture-implicit `Convert.ToDateTime(lines[1])` in legacy settings parse with invariant-first fallback parser (`InvariantCulture` -> `CurrentCulture` -> `ru-RU`).
- Scope:
  - parsing hardening only
  - settings schema and runtime proxy behavior unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - OsDataSet security-load settings serialization

- Standardized decimal serialization in dataset security-load settings:
  - `project/OsEngine/OsData/OsDataSet.cs`
- Changes:
  - in `SecurityToLoad.GetSaveStr()`, `PriceStep` and `VolumeStep` now serialize via `ToString(CultureInfo.InvariantCulture)`.
  - corresponding load path already uses `ToDecimal()`, so old and new formats remain readable.
- Scope:
  - persistence serialization hardening only
  - dataset loading flow and schema unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - TesterServer price-step parsing cleanup

- Standardized culture-safe numeric string handling in tester data loading:
  - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
- Changes:
  - replaced legacy `ToString().Replace(",", ".")` decimal normalization with direct `ToString(CultureInfo.InvariantCulture)` for candle OHLC in price-step estimation.
  - removed unnecessary `decimal -> double -> decimal` conversions in both candle and tick price-step estimation loops.
- Scope:
  - parsing/normalization hardening only
  - tester behavior and stored formats unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Trade/MyTrade datetime parsing and serialization

- Standardized datetime persistence parsing/serialization behavior:
  - `project/OsEngine/Entity/Trade.cs`
  - `project/OsEngine/Entity/MyTrade.cs`
- Changes:
  - `Trade.SetTradeFromString()` (IqFeed branch) now parses date with invariant-first fallback parser instead of culture-implicit `Convert.ToDateTime(...)`.
  - `MyTrade.GetStringFofSave()` now saves `Time` as invariant round-trip (`"O"`).
  - `MyTrade` parser now tries round-trip invariant parse first, then keeps previous invariant/legacy fallback behavior.
- Scope:
  - parsing/serialization hardening only
  - trade logic and field schema unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - MarketDepth persistence formatting

- Standardized invariant numeric formatting in market depth save string:
  - `project/OsEngine/Entity/MarketDepth.cs`
- Changes:
  - in `GetSaveStringToAllDepfh()`, ask/bid volume and price values now serialize via `ToString(CultureInfo.InvariantCulture)`.
  - added `using System.Globalization;`.
- Scope:
  - persistence serialization hardening only
  - load path remains unchanged (`ToDouble()`), backward compatibility preserved.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Position stop-opener persistence

- Standardized culture-safe persistence in stop-opener serialization:
  - `project/OsEngine/Entity/PositionOpenerToStop.cs`
- Changes:
  - in `GetSaveString()`, decimal fields now use `ToString(CultureInfo.InvariantCulture)` (`PriceOrder`, `PriceRedLine`, `Volume`).
  - `LastCandleTime` and `TimeCreate` now serialize as invariant round-trip format (`"O"`).
  - `LoadFromString()` now parses dates with invariant-first fallback (`InvariantCulture` -> legacy `ru-RU` -> `CurrentCulture`) for backward compatibility.
- Scope:
  - persistence serialization/parsing hardening only
  - stop-opener execution logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Security persistence serialization

- Standardized culture-safe persistence in `Security`:
  - `project/OsEngine/Entity/Security.cs`
- Changes:
  - in `GetSaveStr()`, decimal fields now use explicit `ToString(CultureInfo.InvariantCulture)`:
    - `PriceStep`, `Lot`, `PriceStepCost`, `MarginBuy`, `PriceLimitLow`, `PriceLimitHigh`, `Strike`, `MinTradeAmount`, `VolumeStep`, `MarginSell`.
  - `Expiration` now saves as round-trip invariant format: `ToString("O", CultureInfo.InvariantCulture)`.
  - in `LoadFromString()`, `Expiration` parsing now uses invariant-first fallback parser (`InvariantCulture` -> `CurrentCulture` -> `ru-RU`) to preserve backward compatibility.
- Scope:
  - persistence serialization/parsing hardening only
  - security metadata logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Optimizer report decimal serialization

- Standardized decimal serialization in optimizer report persistence:
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizerReportSerializer.cs`
  - `project/OsEngine/OsOptimizer/OptimizerReport.cs`
- Changes:
  - in legacy body serializer, all decimal report aggregates now use `ToString(CultureInfo.InvariantCulture)`.
  - in `OptimizerReportTab.GetSaveString()`, all decimal fields now use `ToString(CultureInfo.InvariantCulture)`.
  - added `using System.Globalization;` in both files.
- Scope:
  - persistence serialization hardening only
  - existing load paths (`ToDecimal()`) preserve backward compatibility.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Candle series decimal parameter serialization

- Standardized decimal serialization in candle series parameter persistence:
  - `project/OsEngine/Candles/Factory/CandleSeriesParameter.cs`
- Changes:
  - in `CandlesParameterDecimal.GetStringToSave()`, decimal value now uses `ToString(CultureInfo.InvariantCulture)`.
  - added `using System.Globalization;`.
- Scope:
  - persistence serialization hardening only
  - load path remains `ToDecimal()`, backward compatibility preserved.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Core parameter decimal serialization

- Standardized decimal serialization in core parameter persistence:
  - `project/OsEngine/Entity/StrategyParameter.cs`
  - `project/OsEngine/Indicators/IndicatorParameter.cs`
- Changes:
  - in `StrategyParameterDecimal.GetStringToSave()` and `StrategyParameterDecimalCheckBox.GetStringToSave()`, decimal fields now use `ToString(CultureInfo.InvariantCulture)`.
  - in `IndicatorParameterDecimal.GetStringToSave()`, decimal value now uses `ToString(CultureInfo.InvariantCulture)`.
  - added `using System.Globalization;` where required.
- Scope:
  - persistence serialization hardening only
  - load paths remain `ToDecimal()`, backward compatibility preserved.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Trend persistence parsing

- Standardized persistence serialization/parsing in:
  - `project/OsEngine/Robots/Trend/WsurfBot.cs`
  - `project/OsEngine/Robots/Trend/SmaStochastic.cs`
  - `project/OsEngine/Robots/Trend/PriceChannelTrade.cs`
- Changes:
  - save paths now serialize decimal values with explicit `CultureInfo.InvariantCulture`.
  - load paths in `WsurfBot` and `SmaStochastic` now parse decimal values via `Extensions.ToDecimal()` (culture-neutral with legacy fallback).
  - `PriceChannelTrade` save path updated to invariant decimal serialization (load path already used `ToDecimal()`).
  - updated `using` imports accordingly.
- Scope:
  - persistence serialization/parsing hardening only
  - strategy trading logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - CounterTrend persistence parsing

- Standardized persistence serialization/parsing in:
  - `project/OsEngine/Robots/CounterTrend/WilliamsRangeTrade.cs`
  - `project/OsEngine/Robots/CounterTrend/StrategyBollinger.cs`
  - `project/OsEngine/Robots/CounterTrend/RsiContrtrend.cs`
- Changes:
  - save paths now serialize decimal values with explicit `CultureInfo.InvariantCulture`.
  - load paths now parse decimal values via `Extensions.ToDecimal()` (culture-neutral with legacy fallback).
  - updated `using` imports accordingly.
- Scope:
  - persistence format/parsing hardening only
  - strategy trading logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.1 (Atomic File Writes) - Polygon securities cache persistence hardening

- Hardened securities cache write path in:
  - `project/OsEngine/Market/Servers/Polygon/PolygonServer.cs`
- Changes:
  - `GetSecurityData()` now streams all paged ticker responses into temp file (`<cache>.tmp`) via a shared writer.
  - finalization now uses atomic swap:
    - `File.Replace(temp, target, target + ".bak", true)` when target exists
    - `File.Move(temp, target)` on first write
  - added safe flush before finalize:
    - `FileStream.Flush(true)` when underlying stream is `FileStream`
    - fallback to `Stream.Flush()` otherwise
  - added `finally` cleanup for leftover temp file.
  - internal save helper now writes into provided `StreamWriter`, removing direct append/resave path.
- Scope:
  - write-path durability hardening only
  - Polygon parsing/business logic and file format unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (InvariantCulture in persistence) - Upstream alignment pass for leverage and options settings parsing

- Continued Step 2.2 replay over newly integrated upstream scope with culture-safe parsing updates:
  - `project/OsEngine/OsTrader/Panels/Tab/BotTabOptions.cs`
    - in legacy settings loader, `StrikesToShow` parse switched:
      - `Convert.ToDecimal(value)` -> `value.ToDecimal()` (culture-neutral extension).
  - `project/OsEngine/Entity/SetLeverageUi.xaml.cs`
    - removed decimal parsing via `Replace(".", ",")` in UI value handling.
    - introduced helper `TryParseDecimalInvariantOrCurrent(...)`:
      - `InvariantCulture` (`NumberStyles.Any`)
      - `CurrentCulture` fallback.
    - applied helper in:
      - `TextBoxLeverage_TextChanged(...)`
      - `_dgv_CellValueChanged(...)` for leverage cells.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (InvariantCulture in persistence) - RiskManager decimal parse hardening after upstream sync

- Audited upstream-accepted changes for culture-sensitive persistence parsing and applied an incremental Step 2.2 fix in:
  - `project/OsEngine/OsTrader/RiskManager/RiskManager.cs`
- Updated legacy settings parse path:
  - `MaxDrowDownToDayPersent = Convert.ToDecimal(lines[0])`
  - -> `MaxDrowDownToDayPersent = ParseDecimalInvariantOrCurrent(lines[0])`
- Added local helper with deterministic parse order:
  - `InvariantCulture` (`NumberStyles.Any`)
  - `CurrentCulture` fallback
  - final legacy `Convert.ToDecimal(value)` fallback for malformed legacy payload behavior parity.
- Binary artifacts check after upstream merge:
  - `project/OsEngine/bin/Debug/OsEngine.dll` and `project/OsEngine/bin/Debug/OsEngine.exe` are not tracked and absent in working tree.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 4.1 (lock migration) - Optimizer cache sync fields migrated to Lock

- Migrated remaining optimizer cache synchronization targets from `object` to `Lock`:
  - `project/OsEngine/OsOptimizer/OptEntity/IndicatorCache.cs`
    - `private readonly object _sync = new object();` -> `private readonly Lock _sync = new();`
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizerMethodCache.cs`
    - `private readonly object _sync = new object();` -> `private readonly Lock _sync = new();`
- Scope and behavior:
  - lock scope and critical sections are unchanged (`lock (_sync)` preserved)
  - no API changes and no runtime logic changes
  - this closes the remaining active `new object()` lock fields in `project/OsEngine` (excluding comments).

### Verification

- Sandbox note:
  - local sandbox test/build path still shows intermittent NuGet TLS issue (`NU1301`), so final verification was executed in host context.
- Host-context verification:
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `343/343`

## 2026-02-22 - Step 4.2 (nullable annotations) - Optimizer cache files nullable-enabled

- Enabled nullable context in remaining optimizer cache files:
  - `project/OsEngine/OsOptimizer/OptEntity/IndicatorCache.cs`
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizerMethodCache.cs`
- Applied nullable-safe signature updates without changing runtime behavior:
  - `Equals(object obj)` -> `Equals(object? obj)` in both key structs
  - `IndicatorCache.TryGet(...)` out value marked nullable (`out List<decimal>[]? values`)
  - internal `TryGetValue(...)` locals switched to nullable-safe `out` forms
  - `CloneSeries(...)` signature aligned to nullable flow (`List<decimal>[]?`)
  - `OptimizerMethodCache.TryGet<T>(...)` default initialization normalized (`default!`)
- Scope:
  - no changes to cache keys, eviction policy, hit/miss accounting, or lock semantics
  - nullable adoption only.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `343/343`

## 2026-02-22 - Step 1.3 (OKX HttpClient hardening) - Security regression tests for interceptor pipeline

- Added targeted tests for OKX request-signing transport layer in:
  - `project/OsEngine.Tests/SecurityRefactorTests.cs`
- New coverage:
  - `OkxHttpInterceptor_ShouldConfigureSocketsHandler_WithPooledLifetime`
    - validates `SocketsHttpHandler` usage
    - validates `PooledConnectionLifetime = TimeSpan.FromMinutes(5)`
    - validates proxy-off default when proxy is not provided
  - `OkxHttpInterceptor_ShouldAddSignedHeaders_AndDemoHeader`
    - validates signed-header injection (`OK-ACCESS-*`)
    - validates JSON accept-header propagation
    - validates demo-trading header (`x-simulated-trading=1`)
    - validates per-request body-signing path via `HttpRequestMessage.Options` key (`SignatureBodyOptionKey`)
- Scope:
  - test-only increment (no runtime behavior changes in production code)
  - secures existing Step 1.3 implementation against regressions.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `345/345`

## 2026-02-22 - Step 1.2 (SSL bypass warning) - Trace warning regression test

- Added explicit regression test for SSL-bypass warning path in:
  - `project/OsEngine.Tests/SecurityRefactorTests.cs`
- New coverage:
  - `WebSocket_IgnoreSslErrors_SetTrue_ShouldEmitTraceWarning`
    - attaches temporary `TraceListener`
    - toggles `IgnoreSslErrors = true`
    - verifies warning text containing `IgnoreSslErrors=true` is emitted
  - keeps `CS0618` usage local to test body because obsolete API call is intentional for regression validation.
- Scope:
  - test-only increment (no runtime behavior changes)
  - secures existing Step 1.2 warning behavior from regressions.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `346/346`

## 2026-02-22 - Step 1.2 (SSL bypass warning) - Restrict IgnoreSslErrors visibility to internal

- Hardened SSL bypass control surface in:
  - `project/OsEngine/Entity/WebSocketOsEngine.cs`
- Change:
  - `IgnoreSslErrors` property visibility narrowed:
    - `public` -> `internal`
  - existing `[Obsolete(...)]` annotation and warning trace behavior preserved.
- Updated security regression tests in:
  - `project/OsEngine.Tests/SecurityRefactorTests.cs`
  - `WebSocket_IgnoreSslErrors_Property_ShouldBeInternal_AndMarkedObsolete`
    - validates non-public/internal visibility via reflection
    - validates obsolete attribute still present
  - `WebSocket_IgnoreSslErrors_SetTrue_ShouldEmitTraceWarning`
    - updated to set property via reflection for non-public setter
    - keeps warning-emission verification.
- Scope:
  - security hardening only; runtime TLS-bypass behavior is unchanged for in-assembly callers.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `346/346`

## 2026-02-22 - Step 1.3 (OKX HttpClient hardening) - Extended interceptor transport regression tests

- Extended OKX interceptor security/transport test coverage in:
  - `project/OsEngine.Tests/SecurityRefactorTests.cs`
- Added tests:
  - `OkxHttpInterceptor_ShouldConfigureProxy_WhenProvided`
    - verifies `UseProxy == true` when proxy is passed
    - verifies handler keeps provided proxy instance
  - `OkxHttpInterceptor_ShouldSetDemoHeaderToZero_WhenDemoModeDisabled`
    - verifies `x-simulated-trading=0` in non-demo mode
    - validates request passes through signed transport pipeline successfully.
- Scope:
  - test-only increment
  - no runtime behavior changes in production code.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `348/348`

## 2026-02-22 - Step 1.3 (OKX HttpClient hardening) - RequestUri guard and invariant UTC timestamp

- Hardened OKX request-signing handler in:
  - `project/OsEngine/Market/Servers/OKX/Entity/HttpInterceptor.cs`
- Changes:
  - added explicit guard:
    - throws `InvalidOperationException` when `request.RequestUri` is missing before signature generation
  - switched timestamp generation to deterministic UTC + invariant culture:
    - `DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture)`
- Added regression test in:
  - `project/OsEngine.Tests/SecurityRefactorTests.cs`
  - `OkxHttpInterceptor_ShouldThrowInvalidOperation_WhenRequestUriIsMissing`
    - validates explicit failure contract for missing URI.
- Scope:
  - low-risk hardening of error contract and timestamp formatting
  - no functional change for valid requests.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `349/349`

## 2026-02-22 - Step 1.3 (OKX HttpClient hardening) - UTC timestamp format regression coverage

- Extended OKX interceptor regression test coverage in:
  - `project/OsEngine.Tests/SecurityRefactorTests.cs`
- Added test:
  - `OkxHttpInterceptor_ShouldEmitUtcTimestamp_InExpectedFormat`
    - validates `OK-ACCESS-TIMESTAMP` ends with `Z`
    - validates exact format `yyyy-MM-ddTHH:mm:ss.fffZ`
    - validates parsed timestamp is UTC (`DateTimeKind.Utc`)
- Scope:
  - test-only increment
  - no production runtime changes.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `350/350`

## 2026-02-22 - Step 2.2 (InvariantCulture in persistence) - ServerParameter decimal parsing hardening

- Hardened persistence deserialization for decimal server parameters in:
  - `project/OsEngine/Market/Servers/Entity/ServerParameter.cs`
- Change in `ServerParameterDecimal.LoadFromStr(...)`:
  - replaced culture-dependent `Convert.ToDecimal(values[2])` primary path with deterministic parse cascade:
    - `decimal.TryParse(..., NumberStyles.Float, CultureInfo.InvariantCulture, ...)`
    - fallback to `CultureInfo.CurrentCulture`
    - fallback to `ru-RU` for legacy comma-decimal payloads
    - final fallback to legacy `Convert.ToDecimal(...)` to preserve old exception behavior on invalid input
- Added regression coverage in:
  - `project/OsEngine.Tests/ServerParameterPersistenceTests.cs`
  - tests:
    - `ServerParameterDecimal_LoadFromStr_ShouldParseInvariantDecimal`
    - `ServerParameterDecimal_LoadFromStr_ShouldParseCommaDecimal_OnNonRuCurrentCulture`
- Scope:
  - persistence culture hardening only
  - serialization format unchanged (`InvariantCulture` on save remains intact).

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (InvariantCulture in persistence) - GateIo trade timestamp fraction parsing

- Hardened culture handling for GateIo trade timestamp fraction parsing in:
  - `project/OsEngine/Market/Servers/GateIoData/GateIoDataServer.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/GateIoServerFutures.cs`
- Changes:
  - replaced implicit-culture `double.Parse(...)` calls with explicit invariant parsing:
    - `double.Parse(..., CultureInfo.InvariantCulture)`
  - scenarios covered:
    - microsecond suffix parsing from CSV historical trade files (GateIoData)
    - millisecond suffix parsing from live REST trade payloads (GateIoData/GateIoFutures)
- Scope:
  - persistence/time parsing hardening only
  - no protocol, business-logic, or API contract changes.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-20 - Step 3.2 (optimizer performance) - Candle reference-sharing verification

- Reviewed optimizer candle data flow in:
  - `project/OsEngine/Market/Servers/Optimizer/OptimizerServer.cs`
  - `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorage.cs`
- Confirmed that optimizer server binds candle data by reference:
  - `securityOpt.Candles = dataStorage.Candles`
  - storage cache returns shared `DataStorage` for identical `(security, timeframe, range, type)` lookups.
- Result:
  - existing implementation already shares candle list references across optimizer bots for identical data keys.
  - no code changes required for this verification increment.

## 2026-02-20 - Step 3.1 (optimizer performance) - Cache key hardening, toggle, and metrics

- Refined optimizer indicator cache architecture:
  - `project/OsEngine/OsOptimizer/OptEntity/IndicatorCache.cs`
  - introduced strong key type `IndicatorCacheKey` (replacing plain string keys)
  - key now includes:
    - data identity/range (`sourceId`, `timeframeTicks`, `first/last time`, `candleCount`)
    - calculation identity (`calculationName`, `parametersHash`)
    - output shape (`outputSeriesCount`, `includeIndicatorsCount`)
    - candle data fingerprint (`dataFingerprint`)
  - added runtime cache statistics:
    - hits, misses, writes, evictions, entries, hit-rate (`IndicatorCacheStatistics`)
  - retained deterministic bounded policy (`clear-all` on entry limit) and clone-on-read/write safety.

- Added deterministic-cache guard in indicator base:
  - `project/OsEngine/Indicators/Aindicator.cs`
  - new virtual switch `IsDeterministicForOptimizerCache` (default `true`)
  - optimizer cache path is now executed only when this flag is true.

- Added optimizer setting toggle for cache:
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
  - `UseIndicatorCache` persisted in optimizer settings (`Engine/OptimizerSettings.txt`)
  - backward compatibility with existing settings files preserved.

- Exposed new setting in optimizer orchestration and UI:
  - `project/OsEngine/OsOptimizer/OptimizerMaster.cs` (forwarding property)
  - `project/OsEngine/OsOptimizer/OptimizerUi.xaml`
  - `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
  - added checkbox: "Use indicator cache / Использовать кэш индикаторов".

- Added cache lifecycle and telemetry logging:
  - `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`
  - cache now created only when `UseIndicatorCache == true`
  - enable/disable messages logged at run start
  - cache stats logged on run cleanup.

- Updated optimizer settings tests for V3 settings-tail layout and added roundtrip assert for cache-toggle:
  - `project/OsEngine.Tests/OptimizerRefactorTests.cs`

### Verification

- Host-context verification (outside sandbox due intermittent sandbox TLS/NuGet issue):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `343/343`

## 2026-02-20 - Step 3.1 (optimizer performance) - Internal method cache API for robots

- Added dedicated optimizer runtime cache for deterministic internal robot methods:
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizerMethodCache.cs`
  - introduced:
    - `OptimizerMethodCacheKey`
    - `OptimizerMethodCache`
    - `OptimizerMethodCacheStatistics`
- Exposed protected API in robot base class for selective forced caching in strategies:
  - `project/OsEngine/OsTrader/Panels/BotPanel.cs`
  - new helpers:
    - `GetOrCreateOptimizerMethodCacheValue<T>(...)` (with overload for `BotTabSimple`)
    - `BuildOptimizerMethodCacheParameterHash(...)`
  - key material for method cache includes:
    - security name
    - timeframe
    - candle range and count
    - method name
    - parameter hash
    - source id and candle data fingerprint
    - result type name
- Integrated method-cache lifecycle into optimizer execution:
  - `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`
  - cache is created/attached only when optimizer cache setting is enabled
  - cache is disposed at run cleanup
  - runtime stats are logged (`hits/misses/writes/evictions/hit-rate`).
- Existing optimizer setting `UseIndicatorCache` now governs both indicator-cache and internal method-cache runtime activation.

### Verification

- Host-context verification (outside sandbox due intermittent sandbox TLS/NuGet issue):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `343/343`

## 2026-02-22 - Step 2.1 (Atomic File Writes) - JournalUi2 export save paths

- Hardened close/open positions export write path in:
  - `project/OsEngine/Journal/JournalUi2.xaml.cs`
- Changes:
  - replaced direct `StreamWriter(fileName)` writes with atomic:
    - `SafeFileWriter.WriteAllText(fileName, workSheet.ToString())`
  - updated in two export handlers:
    - open positions export
    - close positions export
- Scope:
  - persistence write-path hardening only
  - export content/format unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Upstream Replay Audit - Extended full check for today's accepted commits

- Extended upstream replay audit coverage script:
  - `tools/audit-upstream-replay.ps1`
- Added Step 2.1 pattern groups to existing Step 0.3/2.2/4.1 checks:
  - direct `StreamWriter(...)` write paths
  - direct `File.WriteAllText/WriteAllLines/WriteAllBytes(...)`
  - `FileStream(..., FileMode.Create|OpenOrCreate|Append, ...)`
- Ran full attribution-based audit for today's merge:
  - merge commit: `733b909d5`
  - upstream range: `733b909d5^1..733b909d5^2`
  - files scanned: `1124` (`.cs` only)
  - result: no upstream-attributed findings for configured checks.

### Verification

- `pwsh -File tools/audit-upstream-replay.ps1 -MergeCommit 733b909d5 -RepoRoot .` -> success (`OK`, 0 findings)

## 2026-02-22 - Step 2.1 (Atomic File Writes) - JournalUi export save paths

- Hardened close/open positions export write path in:
  - `project/OsEngine/Journal/JournalUi.xaml.cs`
- Changes:
  - replaced direct `StreamWriter(fileName)` writes with atomic:
    - `SafeFileWriter.WriteAllText(fileName, workSheet.ToString())`
  - updated in two export handlers:
    - open positions export
    - close positions export
- Scope:
  - persistence write-path hardening only
  - export content/format unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.1 (Atomic File Writes) - Entity UI save/export paths

- Hardened file save/export write paths in:
  - `project/OsEngine/Entity/DataGridFactory.cs`
  - `project/OsEngine/Entity/SecuritiesUi.xaml.cs`
  - `project/OsEngine/Entity/NonTradePeriodsUi.xaml.cs`
- Changes:
  - `DataGridFactory`: table export save switched from direct `StreamWriter(fileName)` to atomic `SafeFileWriter.WriteAllText(fileName, saveStr)`.
  - `SecuritiesUi`: security dop-settings save switched from direct `StreamWriter(filePath, false)` to atomic `SafeFileWriter.WriteAllLines(filePath, new[] { mySecurity.GetSaveStr() })`.
  - `NonTradePeriodsUi`: template save switched from direct `StreamWriter(filePath)` loop to atomic `SafeFileWriter.WriteAllLines(filePath, array)`.
  - removed redundant pre-create step in `NonTradePeriodsUi` (`File.Create`) because atomic writer creates target safely.
- Scope:
  - write-path durability hardening only
  - saved payload formats preserved (line-based txt remains unchanged).

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.1 (Atomic File Writes) - OsData UI/painter save paths

- Hardened file save paths in:
  - `project/OsEngine/OsData/OsDataSetUi.xaml.cs`
  - `project/OsEngine/OsData/OsDataMasterPainter.cs`
- Changes:
  - `OsDataSetUi`: dataset export save switched from direct `File.WriteAllText(filePath, contentToSave)` to atomic `SafeFileWriter.WriteAllText(filePath, contentToSave)`.
  - `OsDataMasterPainter`: attached-servers persistence switched from direct `StreamWriter(@"Engine\OsDataAttachedServers.txt", false)` loop to atomic `SafeFileWriter.WriteAllLines(@"Engine\OsDataAttachedServers.txt", _attachedServers.Select(...))`.
- Scope:
  - write-path durability hardening only
  - save payload formats preserved (txt line format unchanged).

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.1 (Atomic File Writes) - Additional UI save/export paths

- Hardened file save/export write paths in:
  - `project/OsEngine/Entity/StrategyParametersUi.xaml.cs`
  - `project/OsEngine/OsTrader/Grids/TradeGridUi.xaml.cs`
  - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreenerUi.xaml.cs`
  - `project/OsEngine/Market/Proxy/ProxyMasterUi.xaml.cs`
  - `project/OsEngine/Market/Connectors/MassSourcesCreateUi.xaml.cs`
  - `project/OsEngine/Market/AutoFollow/CopyPortfolioUi.xaml.cs`
  - `project/OsEngine/OsOptimizer/OptimizerReportUi.xaml.cs`
- Changes:
  - replaced direct `StreamWriter(...)`/`File.WriteAllText(...)` save paths with atomic `SafeFileWriter.WriteAllText(...)` or `SafeFileWriter.WriteAllLines(...)`.
  - removed redundant `File.Create(...)` pre-create blocks before save where present.
  - preserved save payload formats (single-line or multi-line txt content unchanged).
- Scope:
  - write-path durability hardening only
  - no behavior changes in load/apply logic.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.1 (Atomic File Writes) - Core/server save paths

- Hardened file save/write paths in:
  - `project/OsEngine/Market/ServerMaster.cs`
  - `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorage.cs`
  - `project/OsEngine/Market/Servers/QuikLua/QuikLuaServer.cs`
  - `project/OsEngine/Market/Servers/Finam/Entity/FinamDataSeries.cs`
  - `project/OsEngine/Market/Servers/ServerCandleStorage.cs`
  - `project/OsEngine/Candles/CandleConverter.cs`
- Changes:
  - replaced one-shot `StreamWriter(...)`/`File.WriteAllText(...)` paths with atomic:
    - `SafeFileWriter.WriteAllLines(...)`
    - `SafeFileWriter.WriteAllText(...)`
  - updated optimizer/security settings save and multiple server-side cache/data-save routines to atomic file replacement in same directory.
- Scope:
  - write-path durability hardening only
  - serialization formats and load logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.1 (Atomic File Writes) - Robots settings save paths

- Hardened robot-settings save write paths in:
  - `project/OsEngine/Robots/CounterTrend/WilliamsRangeTrade.cs`
  - `project/OsEngine/Robots/CounterTrend/StrategyBollinger.cs`
  - `project/OsEngine/Robots/CounterTrend/RsiContrtrend.cs`
  - `project/OsEngine/Robots/Trend/WsurfBot.cs`
  - `project/OsEngine/Robots/Trend/SmaStochastic.cs`
  - `project/OsEngine/Robots/Trend/PriceChannelTrade.cs`
  - `project/OsEngine/Robots/MarketMaker/MarketMakerBot.cs`
  - `project/OsEngine/Robots/MarketMaker/PairTraderSimple.cs`
  - `project/OsEngine/Robots/MarketMaker/PairTraderSpreadSma.cs`
  - `project/OsEngine/Robots/Patterns/PivotPointsRobot.cs`
  - `project/OsEngine/Robots/AlgoStart/AlgoStart2Soldiers.cs`
  - `project/OsEngine/Robots/Grids/GridScreenerAdaptiveSoldiers.cs`
  - `project/OsEngine/Robots/Screeners/ThreeSoldierAdaptiveScreener.cs`
  - `project/OsEngine/Robots/Screeners/PinBarVolatilityScreener.cs`
  - `project/OsEngine/Robots/TechSamples/CustomTableInTheParamWindowSample.cs`
- Changes:
  - replaced direct one-shot `StreamWriter(Get...Path(), false)` blocks with atomic `SafeFileWriter.WriteAllLines(...)`.
  - preserved the same line order and serialized content format used by existing `Load()` methods.
- Scope:
  - write-path durability hardening only
  - no trading logic or parameter semantics changed.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.1 (Atomic File Writes) - Robots residual save paths

- Hardened remaining robot write paths in:
  - `project/OsEngine/Robots/BotCreateUi2.xaml.cs`
  - `project/OsEngine/Robots/Helpers/TaxPayer.cs`
  - `project/OsEngine/Robots/Helpers/PayOfMarginBot.cs`
- Changes:
  - `BotCreateUi2`: bot descriptions file save switched to atomic `SafeFileWriter.WriteAllLines(...)`.
  - `TaxPayer`: period table JSON save switched to atomic `SafeFileWriter.WriteAllText(...)`.
  - `PayOfMarginBot`: both summary and period JSON saves switched to atomic `SafeFileWriter.WriteAllText(...)`.
- Scope:
  - write-path durability hardening only
  - data formats unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.1 (Atomic File Writes) - AscendEX/Telegram/OsDataSet persistence

- Hardened file save/write paths in:
  - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/AscendexSpotServer.cs`
  - `project/OsEngine/Market/Servers/TelegramNews/TelegramNewsServer.cs`
  - `project/OsEngine/OsData/OsDataSet.cs`
- Changes:
  - `AscendexSpotServer`: order-tracker JSON saves switched from `File.WriteAllText(...)` to atomic `SafeFileWriter.WriteAllText(...)`.
  - `TelegramNewsServer`: oversized log-file reset switched from `File.WriteAllText(...)` to atomic `SafeFileWriter.WriteAllText(...)`.
  - `OsDataSet`: migrated one-shot non-append persistence paths to atomic writes:
    - set settings save (`Settings.txt`)
    - candle/trade reconstructed output saves (non-append paths)
    - temp pie settings/data files
    - market-depth pie status file
    - dublicator/updater settings files.
- Scope:
  - write-path durability hardening only
  - append/streaming/log-writer scenarios left unchanged by design.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.1 (Atomic File Writes) - OsConverter streaming output hardening

- Hardened converter output write path in:
  - `project/OsEngine/OsConverter/OsConverterMaster.cs`
- Changes:
  - `WorkerSpaceStreaming()` now writes conversion result to temp file (`<exit>.tmp`) first.
  - after successful write/flush:
    - `File.Replace(temp, target, target + ".bak", true)` when target exists
    - `File.Move(temp, target)` on first write
  - added `finally` cleanup of leftover temp file.
  - fixed flush call for stream compatibility:
    - `FileStream.Flush(true)` when underlying stream is `FileStream`
    - fallback to `Stream.Flush()` otherwise.
- Scope:
  - write-path durability hardening only
  - conversion logic and output format unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - MarketMaker/Patterns persistence parsing

- Standardized persistence serialization/parsing in:
  - `project/OsEngine/Robots/MarketMaker/MarketMakerBot.cs`
  - `project/OsEngine/Robots/MarketMaker/PairTraderSimple.cs`
  - `project/OsEngine/Robots/MarketMaker/PairTraderSpreadSma.cs`
  - `project/OsEngine/Robots/Patterns/PivotPointsRobot.cs`
- Changes:
  - save paths now serialize decimal values with explicit `CultureInfo.InvariantCulture`.
  - load paths now parse decimal values via `Extensions.ToDecimal()` where decimal settings are read from files.
  - `PairTraderSimple` spread value persistence now uses invariant serialization/parsing for saved `Spred` values.
  - updated `using` imports accordingly.
- Scope:
  - persistence serialization/parsing hardening only
  - trading logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - ServerParameter decimal fallback

- Standardized persistence fallback parsing in:
  - `project/OsEngine/Market/Servers/Entity/ServerParameter.cs`
- Changes:
  - `ServerParameterDecimal.LoadFromStr()` final fallback switched from `Convert.ToDecimal(values[2])` to `values[2].ToDecimal()`.
  - this keeps culture-neutral parsing behavior consistent with other persistence loaders.
- Scope:
  - persistence parsing hardening only
  - server parameter storage format unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Common decimal parse fallback cleanup

- Standardized decimal fallback parsing in:
  - `project/OsEngine/Journal/JournalUi2.xaml.cs`
  - `project/OsEngine/OsTrader/RiskManager/RiskManager.cs`
- Changes:
  - helper methods `ParseDecimalInvariantOrCurrent(...)` now use `value.ToDecimal()` as final fallback instead of `Convert.ToDecimal(value)`.
  - this aligns fallback behavior with unified culture-neutral parsing utilities.
- Scope:
  - parsing hardening only
  - runtime behavior unchanged for valid invariant/current-culture values.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Indicator legacy parse fallback cleanup

- Standardized decimal fallback parsing in:
  - `project/OsEngine/Charts/CandleChart/Indicators/DynamicTrendDetector.cs`
  - `project/OsEngine/Charts/CandleChart/Indicators/KalmanFilter.cs`
  - `project/OsEngine/Charts/CandleChart/Indicators/Envelops.cs`
- Changes:
  - legacy settings helpers `ParseDecimalInvariantOrCurrent(...)` now use `value.ToDecimal()` as final fallback instead of `Convert.ToDecimal(value)`.
  - unified with the persistence culture-neutral parsing strategy.
- Scope:
  - parsing hardening only
  - indicator computation logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - TesterServer dop-settings decimal parsing

- Standardized persistence parsing in:
  - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
- Changes:
  - in `SetToSecuritiesDopSettings()`, optional `goSell` value parse switched from `Convert.ToDecimal(array[i][6])` to `array[i][6].ToDecimal()`.
  - this keeps decimal parsing culture-neutral for legacy dop-settings payloads.
- Scope:
  - parsing hardening only
  - tester behavior and settings format unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Server parameter UI decimal input parsing

- Standardized decimal input parsing in:
  - `project/OsEngine/Market/Servers/AServerParameterUi.xaml.cs`
- Changes:
  - decimal parameter assignment in `SaveParam()` switched from custom `Convert.ToDecimal(str.Replace(...))` to `str.ToDecimal()`.
  - removed now-unused `System.Globalization` import.
- Scope:
  - decimal parsing hardening for server parameter UI input
  - parameter persistence format unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - OptimizerSettings decimal serialization

- Standardized persistence serialization in:
  - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
- Changes:
  - in `Save()`, decimal settings now serialize with explicit `CultureInfo.InvariantCulture`:
    - `_startDeposit`, `_filterProfitValue`, `_filterMaxDrawDownValue`, `_filterMiddleProfitValue`, `_filterProfitFactorValue`, `_percentOnFiltration`, `_commissionValue`, `_bayesianAcquisitionKappa`.
  - load path already used culture-neutral decimal parsing via `ToDecimal()` / helper parsing.
- Scope:
  - persistence serialization hardening only
  - optimizer behavior and file structure unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - UI decimal input parsing hardening

- Standardized decimal parsing in UI handlers:
  - `project/OsEngine/OsTrader/RiskManager/RiskManagerUi.xaml.cs`
  - `project/OsEngine/Robots/CounterTrend/WilliamsRangeTradeUi.xaml.cs`
  - `project/OsEngine/Robots/CounterTrend/StrategyBollingerUi.xaml.cs`
  - `project/OsEngine/Robots/CounterTrend/RsiContrtrendUi.xaml.cs`
  - `project/OsEngine/Robots/Trend/SmaStochasticUi.xaml.cs`
  - `project/OsEngine/Robots/Trend/PriceChannelTradeUi.xaml.cs`
  - `project/OsEngine/Robots/Patterns/PivotPointsRobotUi.xaml.cs`
  - `project/OsEngine/Robots/MarketMaker/PairTraderSpreadSmaUi.xaml.cs`
- Changes:
  - replaced `Convert.ToDecimal(TextBox...Text)` with `TextBox...Text.ToDecimal()` in validation and assignment paths.
  - added `using OsEngine.Entity;` where required for extension-method access.
- Scope:
  - input parsing hardening only
  - strategy/risk-manager business logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Indicator UI decimal parsing hardening

- Standardized decimal parsing in indicator UI handlers:
  - `project/OsEngine/Charts/CandleChart/Indicators/BollingerUi.xaml.cs`
  - `project/OsEngine/Charts/CandleChart/Indicators/KalmanFilterUi.xaml.cs`
  - `project/OsEngine/Charts/CandleChart/Indicators/EnvelopsUi.xaml.cs`
  - `project/OsEngine/Charts/CandleChart/Indicators/AtrChannelUi.xaml.cs`
  - `project/OsEngine/Charts/CandleChart/Indicators/DynamicTrendDetectorUi.xaml.cs`
- Changes:
  - replaced `Convert.ToDecimal(TextBox...Text)` with `TextBox...Text.ToDecimal()` in validation/assignment paths.
  - `EnvelopsUi`: replaced `decimal.TryParse(...)` assignment with explicit `ToDecimal()` for consistent culture-neutral behavior.
- Scope:
  - UI input parsing hardening only
  - indicator calculations and persistence schema unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Additional textbox decimal parsing hardening

- Standardized textbox decimal parsing in:
  - `project/OsEngine/Entity/MarketDepthPainter.cs`
  - `project/OsEngine/Entity/PositionUi.xaml.cs`
  - `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
- Changes:
  - replaced decimal textbox parsing with `ToDecimal()` in validation/assignment paths:
    - market depth limit-price validation
    - position start-deposit save path
    - optimizer filtration/filter values and Bayesian kappa textbox handlers.
- Scope:
  - input parsing hardening only
  - business logic and persistence schema unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Helpers table-rate parsing cleanup

- Standardized table rate parsing in:
  - `project/OsEngine/Robots/Helpers/TaxPayer.cs`
  - `project/OsEngine/Robots/Helpers/PayOfMarginBot.cs`
- Changes:
  - replaced legacy decimal parsing patterns based on string replacement and `decimal.TryParse(...)` with unified culture-neutral parsing via `ToDecimal()`.
  - normalized nullable input handling using `(... ?? string.Empty).ToDecimal()` in DataGrid cell reads.
- Scope:
  - parsing hardening only
  - table schema and business logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-22 - Step 2.2 (CultureInfo.InvariantCulture) - Screener settings decimal serialization

- Standardized decimal serialization in screener/algorithm settings payloads:
  - `project/OsEngine/Robots/AlgoStart/AlgoStart2Soldiers.cs`
  - `project/OsEngine/Robots/Grids/GridScreenerAdaptiveSoldiers.cs`
  - `project/OsEngine/Robots/Screeners/ThreeSoldierAdaptiveScreener.cs`
  - `project/OsEngine/Robots/Screeners/PinBarVolatilityScreener.cs`
- Changes:
  - in `GetSaveString()` of volatility settings DTOs/classes, decimal fields now use explicit `ToString(CultureInfo.InvariantCulture)`.
  - corresponding load paths already use `ToDecimal()` + invariant datetime parse.
- Scope:
  - persistence serialization hardening only
  - trading logic unchanged.

### Verification

- Host-context verification (outside sandbox by project rule):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - WinFormsChartPainter decimal-separator neutral precision detection

- Standardized decimal-fraction detection in:
  - `project/OsEngine/Charts/CandleChart/WinFormsChartPainter.cs`
- Changes:
  - removed replace-based decimal separator normalization:
    - `openS/highS/lowS/closeS = ...Replace(".", ",")`
  - replaced split-by-comma fraction detection with separator-agnostic helper:
    - added `GetFractionLength(string value)` that supports both `.` and `,` without string rewriting.
  - updated decimal-length selection logic in `GetCandlesDecimal(...)` to use precomputed fraction lengths.
- Scope:
  - precision detection hardening only
  - chart rendering/business behavior unchanged.

### Verification

- Attempted verification in sandbox with local `DOTNET_CLI_HOME`:
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> failed (`NU1301`, TLS/auth handshake to `https://api.nuget.org/v3/index.json`)
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> failed (`NU1301`, same cause)
  - `dotnet build ...` / `dotnet test ...` could not be reliably validated in this environment due restore failure.

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Extensions.ToDecimal parse cascade hardening

- Standardized core decimal parsing helper in:
  - `project/OsEngine/Entity/Extensions.cs`
- Changes:
  - updated `ToDecimal(this string? value)`:
    - replaced replace-based convert flow (`value.Replace(",", ".")` + `Convert.ToDecimal(...)`) with explicit parse cascade.
    - new parse order: `InvariantCulture` -> `CurrentCulture` -> `ru-RU` via `decimal.TryParse(...)`.
    - preserved legacy fallback through `value.ToDouble()` conversion if direct decimal parse fails.
  - added helper:
    - `TryParseDecimalInvariantOrCurrent(string value, out decimal result)`.
  - `DecimalsCount(...)` scientific-notation branch now normalizes via invariant string:
    - `value.ToDecimal().ToString(CultureInfo.InvariantCulture)`.
- Scope:
  - parsing hardening only
  - business logic and settings formats unchanged.

### Verification

- Host-context verification (outside sandbox):
  - `curl.exe -I https://api.nuget.org/v3/index.json --ssl-no-revoke` -> success (`HTTP/1.1 200 OK`)
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Extensions.DecimalsCount separator-neutral parsing

- Standardized decimal precision detection in:
  - `project/OsEngine/Entity/Extensions.cs`
- Changes:
  - updated `DecimalsCount(this string? value)`:
    - removed replace-based normalization (`value.Replace(",", ".")`).
    - added separator-neutral logic using last decimal separator (`.` or `,`).
    - kept trailing-zero trim semantics and exponent-path normalization (`E` -> invariant decimal string).
- Scope:
  - precision detection/parsing hardening only
  - calling code and formatting contracts unchanged.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Extensions.ToDouble parse cascade hardening

- Standardized core double parsing helper in:
  - `project/OsEngine/Entity/Extensions.cs`
- Changes:
  - updated `ToDouble(this string? value)`:
    - removed replace-based parsing path (`value.Replace(",", ".")` + `Convert.ToDouble(...)`).
    - switched to explicit parse cascade via `TryParseDoubleInvariantOrCurrent(...)`:
      - `InvariantCulture`
      - `CurrentCulture`
      - `ru-RU` fallback
    - null/whitespace input remains safe (`0`).
- Scope:
  - parsing hardening only
  - numerical behavior for valid persisted values preserved.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Table export numeric formatting and step-decimals check cleanup

- Standardized separator handling in:
  - `project/OsEngine/Entity/Extensions.cs`
  - `project/OsEngine/Robots/AutoTestBots/ServerTests/Var_1_Securities.cs`
- Changes:
  - `Extensions.ToFormatString(DataGridViewRow)`:
    - removed blanket `Replace(",", ".")` for all cell text.
    - numeric cells now format via `CultureInfo.InvariantCulture`.
    - non-numeric cells remain textual with newline cleanup only.
  - `Var_1_Securities.IsCompairDecimalsAndStep(...)`:
    - removed replace-based separator normalization.
    - switched to separator-neutral fractional-length detection (`.` or `,`).
- Scope:
  - formatting/parsing hardening only
  - export schema and test intent preserved.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Market server order payload formatting (Bybit/BingX/BitGet)

- Standardized decimal formatting in outbound order payloads:
  - `project/OsEngine/Market/Servers/Bybit/BybitServer.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
  - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/BitGetServerSpot.cs`
  - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/BitGetServerFutures.cs`
- Changes:
  - replaced replace-based formatting:
    - `order.Price/Volume.ToString().Replace(",", ".")`
    - `newPrice.ToString().Replace(",", ".")`
  - with explicit invariant formatting:
    - `ToString(CultureInfo.InvariantCulture)`
  - added missing `using System.Globalization;` in BitGet server files.
- Scope:
  - request-payload formatting hardening only
  - trading logic and endpoint contracts unchanged.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Market server payload formatting (GateIo/HTX/OKX)

- Standardized decimal payload formatting in:
  - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
  - `project/OsEngine/Market/Servers/HTX/Spot/HTXSpotServer.cs`
  - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
  - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
  - `project/OsEngine/Market/Servers/OKX/OkxServer.cs`
- Changes:
  - replaced replace-based formatting:
    - `ToString().Replace(",", ".")`
    - `ToString("0.#####").Replace(",", ".")`
  - with explicit invariant formatting:
    - `ToString(CultureInfo.InvariantCulture)`
    - `ToString("0.#####", CultureInfo.InvariantCulture)`
  - added missing `using System.Globalization;` in GateIo/HTX files where needed.
- Scope:
  - request serialization hardening only
  - endpoint routing, payload keys, and trading logic unchanged.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Market server payload formatting (Exmo/Woo/Kite/TraderNet/Pionex/KuCoin)

- Standardized decimal serialization in outbound order payloads:
  - `project/OsEngine/Market/Servers/ExMo/ExmoSpot/ExmoSpotServer.cs`
  - `project/OsEngine/Market/Servers/Woo/WooServer.cs`
  - `project/OsEngine/Market/Servers/KiteConnect/KiteConnectServer.cs`
  - `project/OsEngine/Market/Servers/TraderNet/TraderNetServer.cs`
  - `project/OsEngine/Market/Servers/Pionex/PionexServerSpot.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/KuCoinSpotServer.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServer.cs`
- Changes:
  - replaced replace-based formatting:
    - `ToString().Replace(",", ".")`
  - with explicit invariant formatting:
    - `ToString(CultureInfo.InvariantCulture)`
  - additionally normalized TraderNet `qty` serialization to invariant formatting.
  - added missing `using System.Globalization;` in affected files.
- Scope:
  - payload-format hardening only
  - API fields, routing, and business logic unchanged.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Market server payload formatting (Bitfinex/BloFin/FinamGrpc/XT)

- Standardized decimal serialization in outbound order payloads:
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/BitfinexFuturesServer.cs`
  - `project/OsEngine/Market/Servers/BloFin/BloFinFutures/BloFinFuturesServer.cs`
  - `project/OsEngine/Market/Servers/FinamGrpc/FinamGrpcServer.cs`
  - `project/OsEngine/Market/Servers/XT/XTSpot/XTServerSpot.cs`
- Changes:
  - replaced replace-based formatting:
    - `ToString().Replace(",", ".")`
  - with explicit invariant formatting:
    - `ToString(CultureInfo.InvariantCulture)`
  - covered `price`, `volume/amount/size/quantity`, and quote-volume payload fields.
  - added missing `using System.Globalization;` in affected files.
- Scope:
  - serialization hardening only
  - request shape and business logic unchanged.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Market server formatting finalization (CoinEx/Atp/MoexFix/Bybit leverage)

- Standardized remaining market-server payload formatting in:
  - `project/OsEngine/Market/Servers/CoinEx/Spot/CoinExServerSpot.cs`
  - `project/OsEngine/Market/Servers/CoinEx/Futures/CoinExServerFutures.cs`
  - `project/OsEngine/Market/Servers/Atp/AtpServer.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
  - `project/OsEngine/Market/Servers/Bybit/BybitServer.cs`
- Changes:
  - removed redundant `.Replace(",", ".")` after already invariant `ToString(CultureInfo.InvariantCulture)` in CoinEx.
  - replaced remaining decimal `ToString().Replace(",", ".")` in Atp/MoexFix order messages with explicit invariant formatting.
  - Bybit leverage payload:
    - replaced direct replace-based path with `NormalizeNumericValueForApi(...)` helper:
      - parse order: `InvariantCulture -> CurrentCulture -> ru-RU`
      - output: invariant decimal string
      - fallback preserves legacy behavior (`Replace(",", ".")`) for non-numeric inputs.
  - added missing `using System.Globalization;` in MoexFix files.
- Scope:
  - formatting hardening only
  - connector business logic and message field sets unchanged.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`
  - grep audit checkpoint:
    - `rg -n 'ToString\\([^)]*\\)\\.Replace\\(\",\",\\s*\"\\.\"\\)' project/OsEngine/Market/Servers` -> no matches.

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Residual replace-to-invariant sweep (Binance/Bybit)

- Cleared remaining replace-based decimal normalization in:
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs`
  - `project/OsEngine/Market/Servers/Binance/Futures/BinanceServerFutures.cs`
  - `project/OsEngine/Market/Servers/Bybit/BybitServer.cs`
- Changes:
  - Binance Spot/Futures:
    - replaced `minQty.ToStringWithNoEndZero().Replace(",", ".")` and split-based parsing.
    - switched to separator-agnostic fractional-length detection via `LastIndexOf('.')` / `LastIndexOf(',')` and index arithmetic.
  - Bybit leverage helper:
    - `NormalizeNumericValueForApi(...)` fallback changed from blanket `Replace(",", ".")`
    - to safe numeric-comma normalization (`NormalizeNumericCommas`) that only rewrites commas between digits.
- Scope:
  - formatting/parsing hardening only
  - API contract and order routing logic unchanged.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`
  - grep audit checkpoint:
    - `rg -n 'Replace\\(\\s*\"\\,\"\\s*,\\s*\"\\.\"\\s*\\)' project/OsEngine` -> no matches.

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Market payload sweep (Moex/Woo/Mexc/Bitfinex Spot)

- Standardized remaining decimal serialization in outbound order payloads:
  - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
  - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
  - `project/OsEngine/Market/Servers/Woo/WooServer.cs`
  - `project/OsEngine/Market/Servers/Mexc/MexcSpot/MexcSpotServer.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
- Changes:
  - replaced replace-based formatting:
    - `ToString().Replace(',', '.')`
  - with explicit invariant formatting:
    - `ToString(CultureInfo.InvariantCulture)`
  - additionally normalized `OrderQty`/`Volume` string serialization in Moex replace paths to invariant format.
  - added missing `using System.Globalization;` where needed.
- Scope:
  - payload-format hardening only
  - request schema and trading logic unchanged.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`
  - grep audit checkpoint:
    - `rg -n "ToString\\(\\)\\.Replace\\(',', '\\.'\\)" project/OsEngine/Market/Servers` -> remaining matches only in `BitMart`, `Transaq`, `Plaza` (out of current batch).

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Market payload sweep (BitMart/Transaq/Plaza)

- Standardized remaining order-price serialization in outbound requests:
  - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/BitMartSpotServer.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/BitMartFutures.cs`
  - `project/OsEngine/Market/Servers/Transaq/TransaqServer.cs`
  - `project/OsEngine/Market/Servers/Plaza/PlazaServer.cs`
- Changes:
  - replaced replace-based formatting:
    - `ToString().Replace(',', '.')`
  - with explicit invariant formatting:
    - `ToString(CultureInfo.InvariantCulture)`
  - in Plaza aligned both transport values (`smsg["price"]`, `smsg["price1"]`) and related diagnostic logs to invariant decimal output.
  - added missing `using System.Globalization;` in BitMart spot/futures files.
- Scope:
  - payload/log formatting hardening only
  - order routing logic unchanged.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`
  - grep audit checkpoint:
    - `rg -n "ToString\\(\\)\\.Replace\\(',', '\\.'\\)" project/OsEngine/Market/Servers` -> no matches.


## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Parsing sweep (HTX/OKX/OsDataSet/Tester)

- Removed remaining separator-replace parsing in:
  - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
  - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
  - `project/OsEngine/Market/Servers/OKX/OkxServer.cs`
  - `project/OsEngine/Market/Servers/OKXData/OKXDataServer.cs`
  - `project/OsEngine/OsData/OsDataSet.cs`
  - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
- Changes:
  - replaced parsing patterns based on `Replace(',', '.')` with separator-agnostic parsing.
  - HTX numeric string cleanup now uses `TrimEnd(''0'').TrimEnd(''.'', '','')` before `ToDecimal()`.
  - OKX/OKXData decimal-volume precision detection switched to last-separator index (`.` or `,`) without string replacement.
  - OsDataSet/Tester binary-header parsing now uses explicit parse cascade helpers:
    - `InvariantCulture -> CurrentCulture -> ru-RU`.
- Scope:
  - parsing hardening only
  - business logic and protocol semantics unchanged.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`
  - grep audit checkpoint:
    - `rg -n -F "Replace(',', '.')" project/OsEngine` -> no matches.

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Parsing cleanup (BingX/GateIo/KuCoin/Bitfinex)

- Removed replace-based decimal normalization in market-server parse paths:
  - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/KuCoinSpotServer.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServer.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/BitfinexFuturesServer.cs`
- Changes:
  - replaced incoming parse chains:
    - `value.Replace('.', ',').ToDecimal()`
    - with direct culture-agnostic parse:
    - `value.ToDecimal()`
  - aligned remaining BingX outbound `quantity/price` formatting from `ToString().Replace(",", ".")` to `ToString(CultureInfo.InvariantCulture)`.
  - Bitfinex securities precision helper updated to separator-agnostic logic (`.` or `,`) without pre-normalization replace.
- Scope:
  - parsing/serialization hardening only
  - connector behavior and order-routing logic unchanged.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`
  - grep audit checkpoint:
    - `rg -n -F "Replace('.', ',')" project/OsEngine` -> no matches.

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Parse/format cleanup (Deribit/Transaq + exchange parsers)

- Completed cleanup of residual replace-based normalization in:
  - `project/OsEngine/Market/Servers/Deribit/DeribitServer.cs`
  - `project/OsEngine/Market/Servers/Transaq/TransaqServer.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
  - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/KuCoinSpotServer.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServer.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
  - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/BitfinexFuturesServer.cs`
- Changes:
  - removed incoming parse pattern `Replace('.', ',').ToDecimal()` in exchange payload readers.
  - replaced with direct `ToDecimal()` (culture-agnostic parse cascade).
  - Deribit order price assignment simplified from `ToString().ToDecimal()` to direct decimal assignment.
  - Transaq volume serialization normalized via `ToString(CultureInfo.InvariantCulture)` and `.0` trim.
  - Bitfinex precision helper `DigitsAfterComma` made separator-agnostic (`.` or `,`) without pre-normalization replace.
- Scope:
  - parsing/formatting hardening only
  - trading/business behavior unchanged.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`
  - grep audit checkpoint:
    - `rg -n -F "Replace('.', ',')" project/OsEngine` -> no matches.
    - `rg -n -F "Replace(',', '.')" project/OsEngine` -> no matches.

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - UI double parsing hardening

- Replaced culture-sensitive `Convert.ToDouble(...)` UI input parsing in:
  - `project/OsEngine/Charts/CandleChart/Indicators/ParabolicSARUi.xaml.cs`
  - `project/OsEngine/OsTrader/AvailabilityServer/ServerAvailabilityUi.xaml.cs`
- Changes:
  - `Convert.ToDouble(...)` -> `ToDouble()` for UI strings.
  - maintained existing validation semantics (`<= 0` checks) and business behavior.
- Scope:
  - input parsing hardening only.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Tester parser cleanup

- Removed duplicated parse helper in `SecurityTester` scope:
  - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
- Changes:
  - replaced local `TryParseDecimalInvariantOrCurrent(...)` usage in `ParseVolumeStepFromComment(...)` with direct `ToDecimal()` parse and zero-guard.
  - behavior preserved (`VolumeStep` fallback to `1` when invalid/zero).
- Scope:
  - internal parser cleanup only.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Parser helper consolidation (Indicators/Journal/Risk/Polygon)

- Unified decimal parsing through shared `ToDecimal()` extension and removed duplicate local helpers in:
  - `project/OsEngine/Charts/CandleChart/Indicators/Envelops.cs`
  - `project/OsEngine/Charts/CandleChart/Indicators/DynamicTrendDetector.cs`
  - `project/OsEngine/Charts/CandleChart/Indicators/KalmanFilter.cs`
  - `project/OsEngine/Journal/JournalUi2.xaml.cs`
  - `project/OsEngine/OsTrader/RiskManager/RiskManager.cs`
  - `project/OsEngine/Market/Servers/Polygon/PolygonServer.cs`
- Changes:
  - replaced local `ParseDecimalInvariantOrCurrent(...)` usage with `ToDecimal()` in legacy-settings and benchmark/timestamp parsing paths.
  - removed duplicated helper methods where behavior was equivalent to `ToDecimal()` fallback cascade.
  - cleaned no-longer-used `System.Globalization` usings in touched files.
- Scope:
  - parsing unification/refactoring only
  - data contracts and business logic unchanged.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Binance Futures timestamp parsing hardening

- Hardened string timestamp parsing in:
  - `project/OsEngine/Market/Servers/Binance/Futures/BinanceServerFutures.cs`
- Changes:
  - replaced string-based `Convert.ToDouble(...)` in websocket/rest timestamp fields with culture-agnostic `ToDouble()` for:
    - candle payload split fields `param[0]`
    - order update timestamps `ord.T`, `order.T`
    - order REST timestamps `orderOnBoardResp.time/updateTime`, `orderOnBoard.time/updateTime`
  - kept numeric `long` timestamp conversion (`trades.data.T`) unchanged as `Convert.ToDouble(...)`.
- Scope:
  - timestamp parsing hardening only
  - connector behavior unchanged for valid payloads.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Binance Spot aggregated trade timestamp parsing

- Hardened string timestamp parsing in:
  - `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs`
- Changes:
  - replaced `Convert.ToDouble(jtTrade.T)` with `jtTrade.T.ToDouble()` in aggregated trade history conversion.
  - retained websocket trade timestamp conversion for numeric `long` payload field (`trades.data.T`).
- Scope:
  - timestamp parsing hardening only
  - connector behavior unchanged for valid payloads.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Woo timestamp parser hardening

- Hardened string timestamp parsing in:
  - `project/OsEngine/Market/Servers/Woo/WooServer.cs`
- Changes:
  - in `UnixTimeMilliseconds(string timestamp)` replaced `Convert.ToDouble(timestamp)` with `timestamp.ToDouble()`.
  - keeps existing milliseconds conversion and rounding behavior unchanged.
- Scope:
  - parser hardening only
  - connector behavior unchanged for valid payloads.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Timestamp parsing hardening (BitGet/BingX)

- Standardized timestamp string parsing with explicit invariant culture in:
  - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/BitGetServerSpot.cs`
  - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/BitGetServerFutures.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
  - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
- Changes:
  - replaced `Convert.ToInt64(stringTimestamp)` with `Convert.ToInt64(stringTimestamp, CultureInfo.InvariantCulture)` in timestamp-to-`DateTime` conversion paths (trades, depths, orders, candles).
  - kept `Convert` API (null-handling/exception behavior) while removing current-culture dependence.
- Scope:
  - parser hardening only
  - connector behavior unchanged for valid payloads.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Timestamp parsing hardening (KuCoin Spot/Futures)

- Standardized timestamp string parsing with explicit invariant culture in:
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/KuCoinSpotServer.cs`
  - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServer.cs`
- Changes:
  - replaced `Convert.ToInt64(stringTimestamp)` with `Convert.ToInt64(stringTimestamp, CultureInfo.InvariantCulture)`
    in timestamp-to-`DateTime` conversion paths (candles, trades, depth, orders, myTrades).
  - retained existing nanoseconds-to-milliseconds division logic where present (`/ 1000000`).
- Scope:
  - parser hardening only
  - connector behavior unchanged for valid payloads.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Timestamp parsing hardening (HTX Spot/Futures/Swap)

- Standardized timestamp string parsing with explicit invariant culture in:
  - `project/OsEngine/Market/Servers/HTX/Spot/HTXSpotServer.cs`
  - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
  - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
- Changes:
  - replaced timestamp conversions:
    - `Convert.ToInt64(timestampString)` -> `Convert.ToInt64(timestampString, CultureInfo.InvariantCulture)`
  - applied in trade/depth/myTrade/order-event timestamp paths.
  - intentionally left non-timestamp numeric conversions unchanged (e.g. order id payload fields).
- Scope:
  - parser hardening only
  - connector behavior unchanged for valid payloads.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Timestamp parsing hardening (OKX/OKXData)

- Standardized timestamp string parsing with explicit invariant culture in:
  - `project/OsEngine/Market/Servers/OKX/OkxServer.cs`
  - `project/OsEngine/Market/Servers/OKXData/OKXDataServer.cs`
- Changes:
  - replaced timestamp conversions:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)`
  - applied in candles/trades/depth/order/myTrade timestamp paths and instrument expiration parsing.
- Scope:
  - parser hardening only
  - connector behavior unchanged for valid payloads.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Timestamp parsing hardening (BitMart Spot/Futures)

- Standardized timestamp string parsing with explicit invariant culture in:
  - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/BitMartSpotServer.cs`
  - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/BitMartFutures.cs`
- Changes:
  - replaced timestamp conversions:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)`
  - applied in candles/trades/depth/order/myTrade timestamp paths.
- Scope:
  - parser hardening only
  - connector behavior unchanged for valid payloads.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Timestamp parsing hardening (Pionex Spot)

- Standardized timestamp string parsing with explicit invariant culture in:
  - `project/OsEngine/Market/Servers/Pionex/PionexServerSpot.cs`
- Changes:
  - replaced timestamp conversions:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)`
  - applied in candles/trades/depth/order/myTrade timestamp paths.
- Scope:
  - parser hardening only
  - connector behavior unchanged for valid payloads.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Timestamp parsing hardening (Woo)

- Standardized timestamp string parsing with explicit invariant culture in:
  - `project/OsEngine/Market/Servers/Woo/WooServer.cs`
- Changes:
  - replaced timestamp conversions:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)`
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)`
  - applied in candles/trades/depth/order/myTrade timestamp paths.
- Scope:
  - parser hardening only
  - connector behavior unchanged for valid payloads.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Timestamp parsing hardening (BloFin Futures)

- Standardized timestamp string parsing with explicit invariant culture in:
  - `project/OsEngine/Market/Servers/BloFin/BloFinFutures/BloFinFuturesServer.cs`
- Changes:
  - replaced timestamp conversions:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)`
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)`
  - applied in candles/trades/depth/order/myTrade timestamp paths.
- Scope:
  - parser hardening only
  - connector behavior unchanged for valid payloads.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Timestamp parsing hardening (XT Spot)

- Standardized timestamp string parsing with explicit invariant culture in:
  - `project/OsEngine/Market/Servers/XT/XTSpot/XTServerSpot.cs`
- Changes:
  - replaced timestamp conversions:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)`
  - applied in candles/trades/order/myTrade timestamp paths.
- Scope:
  - parser hardening only
  - connector behavior unchanged for valid payloads.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Timestamp parsing hardening (XT Futures)

- Standardized timestamp string parsing with explicit invariant culture in:
  - `project/OsEngine/Market/Servers/XT/XTFutures/XTFuturesServer.cs`
- Changes:
  - replaced timestamp conversions:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)`
  - applied in candles/trades/depth/order/myTrade timestamp paths.
- Scope:
  - parser hardening only
  - connector behavior unchanged for valid payloads.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`

## 2026-02-23 - Step 2.2 (CultureInfo.InvariantCulture) - Timestamp parsing hardening (AscendEX Spot)

- Standardized timestamp string parsing with explicit invariant culture in:
  - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/AscendexSpotServer.cs`
- Changes:
  - replaced timestamp conversions:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)`
  - applied in candles/trades/depth/order/myTrade timestamp paths.
- Scope:
  - parser hardening only
  - connector behavior unchanged for valid payloads.

### Verification

- Host-context verification (outside sandbox):
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success (0 warnings, 0 errors)
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `352/352`
