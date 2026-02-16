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
