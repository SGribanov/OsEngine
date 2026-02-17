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
