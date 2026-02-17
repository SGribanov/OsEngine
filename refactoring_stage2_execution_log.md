# Refactoring Stage 2 Execution Log

**Started:** 2026-02-15
**Repository:** `C:\Repos\MyCloneOsEngine`
**Branch:** `master`

## 2026-02-15

### Step 0.1 - Fix Bot Compilation Cache

- **Status:** Done
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.1
- **Changes:**
  - Added cache write path for compiled bot type in `CompileAndInstantiateBotScript()` under `_compiledTypesCacheLock`.
  - File: `project/OsEngine/Robots/BotFactory.cs`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore` succeeded, 0 errors.
  - Full solution build in this environment failed on restore/workload resolver, not on compile errors.
- **Commit:** `1f648f5e9`
- **Push:** yes (`origin/master`, SSH)

### Step 0.2 - Clean Up App.config

- **Status:** Done
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.2
- **Changes:**
  - Removed legacy `.NET Framework` config file.
  - File removed: `project/OsEngine/App.config`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore` succeeded, 0 errors.
- **Commit:** `3cd1705c3`
- **Push:** yes (`origin/master`, SSH)

### Step 0.3 - Add Logging to Silent Catches

- **Status:** Done
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Replaced targeted silent catches with explicit logging while keeping non-throwing behavior.
  - Files:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
    - `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorage.cs`
    - `project/OsEngine/Entity/HorizontalVolume.cs`
    - `project/OsEngine/Entity/NonTradePeriods.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerReportSerializer.cs`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore` succeeded, 0 errors.
- **Commit:** `f31327e27`
- **Push:** yes (`origin/master`, SSH)

### Step 1.1 - Encrypt API Keys at Rest

- **Status:** Done
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 1 / Step 1.1
- **Changes:**
  - Added DPAPI helper with `dpapi:` prefix marker and safe fallback:
    - `project/OsEngine/Entity/CredentialProtector.cs`
  - Updated password parameter persistence:
    - encrypt on save, decrypt on load
    - migration flag for legacy plain-text values
    - file: `project/OsEngine/Market/Servers/Entity/ServerParameter.cs`
  - Added auto-resave trigger after legacy password load:
    - file: `project/OsEngine/Market/Servers/AServer.cs`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore` succeeded, 0 errors.
- **Commit:** `48e2f71f0`
- **Push:** yes (`origin/master`, SSH)

## 2026-02-16

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #1)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Added generic JSON settings utility:
    - `project/OsEngine/Entity/SettingsManager.cs`
  - Added tests for JSON manager:
    - `project/OsEngine.Tests/SettingsManagerTests.cs`
  - Migrated optimizer storage settings persistence to JSON with backward-compatible legacy fallback parser:
    - `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorage.cs`
  - Added regression tests:
    - `OptimizerDataStorage_Load_ShouldReadLegacyTextSettings`
    - `OptimizerDataStorage_Save_ShouldPersistJsonAndRoundTrip`
    - additional `ParameterIterator` guard tests for non-positive/overshoot step handling
    - file: `project/OsEngine.Tests/OptimizerRefactorTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`90/90`).
- **Commit:** `5e94ce9e6`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #2)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated `NonTradePeriods` persistence to JSON with legacy fallback:
    - `project/OsEngine/Entity/NonTradePeriods.cs`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/NonTradePeriodsPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`92/92`).
- **Commit:** `3f865ce92`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #3)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated `HorizontalVolume` persistence to JSON with legacy fallback:
    - `project/OsEngine/Entity/HorizontalVolume.cs`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/HorizontalVolumePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`94/94`).
- **Commit:** `8505c651f`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #4)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated `NumberGen` persistence to JSON with legacy fallback:
    - `project/OsEngine/Entity/NumberGen.cs`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/NumberGenPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`96/96`).
- **Commit:** `358e6cb8e`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #5)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated `ComparePositionsModule` persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Servers/ComparePositionsModule.cs`
  - Covered both persisted files:
    - `CompareModule.txt` (main settings)
    - `CompareModule_IgnoreSec.txt` (ignored securities)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ComparePositionsModulePersistenceTests.cs`
      - `SaveLoad_ShouldPersistJsonForMainSettings`
      - `Load_ShouldSupportLegacyLineBasedMainSettings`
      - `SaveLoadIgnoredSecurities_ShouldPersistJson`
      - `LoadIgnoredSecurities_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`100/100`).
- **Commit:** `0fabce56e`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #6)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Interactive Brokers securities watchlist persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Servers/InteractiveBrokers/InteractiveBrokersServer.cs`
  - Covered persisted file:
    - `IbSecuritiesToWatch.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/InteractiveBrokersSecuritiesPersistenceTests.cs`
      - `SaveIbSecurities_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadIbSecurities_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`102/102`).
- **Commit:** `aafd4bc8c`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #7)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated AServer core settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Servers/AServer.cs`
  - Covered persisted file:
    - `<ServerNameUnique>ServerSettings.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/AServerSettingsPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`104/104`).
- **Commit:** `6cd0f6857`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #8)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated SMS server settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Logging/ServerSms.cs`
  - Covered persisted file:
    - `Engine\\smsSet.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ServerSmsPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`106/106`).
- **Commit:** `2f2114867`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #9)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated mail server settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Logging/ServerMail.cs`
  - Covered persisted file:
    - `Engine\\mailSet.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ServerMailPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`108/108`).
- **Commit:** `c5dd4220a`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #10)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated webhook server settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Logging/ServerWebhook.cs`
  - Covered persisted file:
    - `Engine\\webhookSet.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ServerWebhookPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`110/110`).
- **Commit:** `7949ce1ab`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #11)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated message sender settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Logging/MessageSender.cs`
  - Covered persisted file:
    - `Engine\\<name>MessageSender.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/MessageSenderPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`112/112`).
- **Commit:** `077153161`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #12)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Telegram server settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Logging/ServerTelegram.cs`
  - Covered persisted file:
    - `Engine\\telegramSet.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ServerTelegramPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`114/114`).
- **Commit:** `4dd26a36a`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #13)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated timeframe builder settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Candles/TimeFrameBuilder.cs`
  - Covered persisted file:
    - `Engine\\<name>TimeFrameBuilder.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/TimeFrameBuilderPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`116/116`).
- **Commit:** `14f21b9ef`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #14)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated price alert settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Alerts/AlertToPrice.cs`
  - Covered persisted file:
    - `Engine\\<Name>Alert.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/AlertToPricePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`118/118`).
- **Commit:** `21d9d1a3c`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #15)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated prime settings persistence to JSON with legacy fallback:
    - `project/OsEngine/PrimeSettings/PrimeSettingsMaster.cs`
  - Covered persisted file:
    - `Engine\\PrimeSettings.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/PrimeSettingsMasterPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`120/120`).
- **Commit:** `b757cbe27`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #16)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated candle converter settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Candles/CandleConverter.cs`
  - Covered persisted file:
    - `Engine\\CandleConverter.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/CandleConverterPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`122/122`).
- **Commit:** `bc9a0d013`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #17)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated cluster chart master settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/ClusterChart/ChartClusterMaster.cs`
  - Covered persisted file:
    - `Engine\\<name>ClusterChartMasterSet.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ChartClusterMasterPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`124/124`).
- **Commit:** `5c0f3023f`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #18)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated cluster tab on/off settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabCluster.cs`
  - Covered persisted file:
    - `Engine\\<TabName>ClusterOnOffSet.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/BotTabClusterPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`126/126`).
- **Commit:** `2e53efd1e`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #19)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated chart alert settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Alerts/AlertToChart.cs`
  - Covered persisted file:
    - `Engine\\<Name>Alert.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/AlertToChartPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`128/128`).
- **Commit:** `a2562fca7`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #20)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated chart color keeper settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/ColorKeeper/ChartMasterColorKeeper.cs`
  - Covered persisted file:
    - `Engine\\Color\\<name>Color.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ChartMasterColorKeeperPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`130/130`).
- **Commit:** `d1a49f148`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #21)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated manual control settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/Panels/Tab/Internal/BotManualControl.cs`
  - Covered persisted file:
    - `Engine\\<name>StrategSettings.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/BotManualControlPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`132/132`).
- **Commit:** `37a914336`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #22)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated index auto-formula settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs` (`IndexFormulaBuilder`)
  - Covered persisted file:
    - `Engine\\<botUniqName>IndexAutoFormulaSettings.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/IndexFormulaBuilderPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`134/134`).
- **Commit:** `9cc3febbf`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #23)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated index spread settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs` (`SpreadSet.txt`)
  - Covered persisted file:
    - `Engine\\<TabName>SpreadSet.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/BotTabIndexSpreadPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Stabilized server settings tests to avoid constructor side effects:
    - `project/OsEngine.Tests/AServerSettingsPersistenceTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`136/136`).
- **Commit:** `6729c267e`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #24)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated converter settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsConverter/OsConverterMaster.cs`
  - Covered persisted file:
    - `Engine\\Converter.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/OsConverterMasterPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`138/138`).
- **Commit:** `f55fe2ebb`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #25)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated localization settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Language/OsLocalization.cs`
  - Covered persisted file:
    - `Engine\\local.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/OsLocalizationPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`140/140`).
- **Commit:** `d01c06dc4`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #26)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated layout settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Layout/GlobalGUILayout.cs` (`LayoutGui.txt`)
  - Covered persisted file:
    - `Engine\\LayoutGui.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/GlobalGUILayoutPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`142/142`).
- **Commit:** `b658b2e06`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #27)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated attached servers settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/ServerMasterSourcesPainter.cs`
  - Covered persisted file:
    - `Engine\\AttachedServers.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ServerMasterSourcesPainterPersistenceTests.cs`
      - `SaveAttachedServers_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadAttachedServers_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`144/144`).
- **Commit:** `108e3d16e`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #28)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated block interface settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/Gui/BlockInterface/BlockMaster.cs`
  - Covered persisted files:
    - `Engine\\PrimeSettingss.txt`
    - `Engine\\PrimeSettingsss.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/BlockMasterPersistenceTests.cs`
      - `Password_ShouldPersistJson_AndLoadRoundTrip`
      - `Password_ShouldSupportLegacyLineBasedFormat`
      - `IsBlocked_ShouldPersistJson_AndLoadRoundTrip`
      - `IsBlocked_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`148/148`).
- **Commit:** `648bca710`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #29)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated screen resolution settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Layout/GlobalGUILayout.cs`
  - Covered persisted file:
    - `Engine\\ScreenResolution.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/GlobalGUILayoutScreenResolutionPersistenceTests.cs`
      - `SaveResolution_ShouldPersistJson_AndScreenSettingsCheckReturnTrue`
      - `ScreenSettingsIsAllRight_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`150/150`).
- **Commit:** `6c391aaf1`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #30)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated ATP securities cache persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Servers/Atp/AtpServer.cs` (`AtpServerRealization`)
  - Covered persisted file:
    - `Engine\\AtpSecurities.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/AtpServerSecuritiesPersistenceTests.cs`
      - `SaveSecurities_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadSecurities_ShouldSupportLegacyLineBasedFormat`
  - Stabilized localization persistence tests against parallel file races:
    - `project/OsEngine.Tests/OsLocalizationPersistenceTests.cs` (`DisableParallelization` collection)
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`152/152`).
- **Commit:** `13857cc54`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #31)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated RAM usage analyze settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs` (`RamMemoryUsageAnalyze`)
  - Covered persisted file:
    - `Engine\\SystemStress\\RamMemorySettings.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/RamMemoryUsageAnalyzePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`154/154`).
- **Commit:** `d479ec569`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #32)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated CPU usage analyze settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs` (`CpuUsageAnalyze`)
  - Covered persisted file:
    - `Engine\\SystemStress\\CpuMemorySettings.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/CpuUsageAnalyzePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`156/156`).
- **Commit:** `dc4af9143`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #33)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated ECQ usage analyze settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs` (`EcqUsageAnalyze`)
  - Covered persisted file:
    - `Engine\\SystemStress\\EcqMemorySettings.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/EcqUsageAnalyzePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`158/158`).
- **Commit:** `94aaf1e0a`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #34)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated MOQ usage analyze settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs` (`MoqUsageAnalyze`)
  - Covered persisted file:
    - `Engine\\SystemStress\\MoqMemorySettings.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/MoqUsageAnalyzePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`160/160`).
- **Commit:** `1ddbfae5c`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #35)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated AServer parameter settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Servers/AServer.cs`
  - Covered persisted file:
    - `Engine\\<ServerNameUnique>Params.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/AServerParamsPersistenceTests.cs`
      - `SaveParam_ShouldPersistJson_AndLoadParamRoundTrip`
      - `LoadParam_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`162/162`).
- **Commit:** `63ad7e616`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #36)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated tester core settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Covered persisted file:
    - `Engine\\TestServer.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/TesterServerPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`164/164`).
- **Commit:** `fe10b4b34`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #37)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated tester security test settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Covered persisted file:
    - dynamic `...\\SecurityTestSettings.txt` path (set/folder modes)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/TesterServerSecurityTestSettingsPersistenceTests.cs`
      - `SaveSecurityTestSettings_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadSecurityTestSettings_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`166/166`).
- **Commit:** `d2e35fbac`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #38)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated tester clearing settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Covered persisted file:
    - `Engine\\TestServerClearings.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/TesterServerClearingPersistenceTests.cs`
      - `SaveClearingInfo_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadClearingInfo_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`168/168`).
- **Commit:** `0d0ca36f1`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #39)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated tester non-trade periods settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Covered persisted file:
    - `Engine\\TestServerNonTradePeriods.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/TesterServerNonTradePeriodsPersistenceTests.cs`
      - `SaveNonTradePeriods_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadNonTradePeriods_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`170/170`).
- **Commit:** `a16635e96`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #40)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated tester securities timeframe settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Covered persisted file:
    - dynamic `Engine\\TestServerSecuritiesTf...txt` path
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/TesterServerSecuritiesTimeFramePersistenceTests.cs`
      - `SaveSetSecuritiesTimeFrameSettings_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadSetSecuritiesTimeFrameSettings_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`172/172`).
- **Commit:** `b7e79172f`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #41)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated tester security dop settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Covered persisted file:
    - dynamic `...\\SecuritiesSettings.txt` path
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/TesterServerSecurityDopSettingsPersistenceTests.cs`
      - `SaveSecurityDopSettings_ShouldPersistJson_AndLoadSettings`
      - `LoadSecurityDopSettings_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`174/174`).
- **Commit:** `0be3d609e`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #42)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated QuikLua securities cache persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Servers/QuikLua/QuikLuaServer.cs`
  - Covered persisted file:
    - `Engine\\QuikLuaSecuritiesCache.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/QuikLuaSecuritiesCachePersistenceTests.cs`
      - `SaveToCache_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadSecuritiesFromCache_ShouldSupportLegacyCompressedStringFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`176/176`).
- **Commit:** `de6aa5f0a`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #43)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated MetaTrader5 securities cache persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Servers/MetaTrader5/MetaTrader5Server.cs`
  - Covered persisted file:
    - `Engine\\MetaTrader5SecuritiesCache.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/MetaTrader5SecuritiesCachePersistenceTests.cs`
      - `SaveToCache_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadSecuritiesFromCache_ShouldSupportLegacyCompressedStringFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`178/178`).
- **Commit:** `975cf9022`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #44)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated MetaTrader5 positions cache persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Servers/MetaTrader5/MetaTrader5Server.cs`
  - Covered persisted file:
    - `Engine\\MetaTrader5PositionsCache.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/MetaTrader5PositionsCachePersistenceTests.cs`
      - `SavePositionsInFile_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadPositionsFromFile_ShouldSupportLegacyCompressedStringFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`180/180`).
- **Commit:** `2f8e072d5`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #45)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated proxy master core settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Proxy/ProxyMaster.cs`
  - Covered persisted file:
    - `Engine\\ProxyMaster.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ProxyMasterSettingsPersistenceTests.cs`
      - `SaveSettings_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadSettings_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`182/182`).
- **Commit:** `e71c81f72`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #46)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated proxy hub settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Proxy/ProxyMaster.cs`
  - Covered persisted file:
    - `Engine\\ProxyHub.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ProxyMasterProxyHubPersistenceTests.cs`
      - `SaveProxy_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadProxy_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`184/184`).
- **Commit:** `d8eda90b6`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #47)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated connector news settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Connectors/ConnectorNews.cs`
  - Covered persisted file:
    - dynamic `Engine\\<name>ConnectorNews.txt` path
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ConnectorNewsSettingsPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`186/186`).
- **Commit:** `2c6ec95f9`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #48)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated connector candles settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/Connectors/ConnectorCandles.cs`
  - Covered persisted file:
    - dynamic `Engine\\<name>ConnectorPrime.txt` path
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ConnectorCandlesSettingsPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat_WithOptionalFieldsMissing`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`188/188`).
- **Commit:** `5521ddf82`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #49)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated server master core settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/ServerMaster.cs`
  - Covered persisted file:
    - `Engine\\ServerMaster.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ServerMasterSettingsPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`190/190`).
- **Commit:** `0b0d5564f`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #50)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated server popularity stats persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/ServerMaster.cs`
  - Covered persisted file:
    - `Engine\\MostPopularServers.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ServerMasterMostPopularServersPersistenceTests.cs`
      - `SaveMostPopularServers_ShouldPersistJson_AndLoadCounts`
      - `LoadMostPopularServersWithCount_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`192/192`).
- **Commit:** `f4e098eee`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #51)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated auto-follow portfolio copier settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/AutoFollow/CopyTrader.cs` (`PortfolioToCopy`)
  - Covered persisted file:
    - dynamic `Engine\\CopyTrader\\<Name>.txt` path
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/PortfolioToCopySettingsPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`194/194`).
- **Commit:** `423316ec7`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #52)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated auto-follow copy trader hub persistence to JSON with legacy fallback:
    - `project/OsEngine/Market/AutoFollow/CopyMaster.cs`
  - Covered persisted file:
    - `Engine\\CopyTrader\\CopyTradersHub.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/CopyMasterHubPersistenceTests.cs`
      - `SaveCopyTraders_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadCopyTraders_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`196/196`).
- **Commit:** `31c6c4cd2`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #53)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated trader risk manager settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/RiskManager/RiskManager.cs`
  - Covered persisted file:
    - dynamic `Engine\\<name>.txt` path
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/RiskManagerSettingsPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`198/198`).
- **Commit:** `afde27baf`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #54)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated options tab settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabOptions.cs`
  - Covered persisted file:
    - dynamic `Engine\\<TabName>\\OptionsSettings.txt` path
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/BotTabOptionsSettingsPersistenceTests.cs`
      - `SaveSettings_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadSettings_ShouldSupportLegacyKeyValueFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`200/200`).
- **Commit:** `70186abfe`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #55)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated screener indicators settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`
  - Covered persisted file:
    - dynamic `Engine\\<TabName>ScreenerIndicators.txt` path
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/BotTabScreenerIndicatorsPersistenceTests.cs`
      - `SaveIndicators_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadIndicators_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`202/202`).
- **Commit:** `0d28735a0`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #56)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated screener core settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`
  - Covered persisted file:
    - dynamic `Engine\\<TabName>ScreenerSet.txt` path
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/BotTabScreenerSettingsPersistenceTests.cs`
      - `SaveSettings_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadSettings_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`204/204`).
- **Commit:** `1002256b4`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #57)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated pair-trading standard settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs`
  - Covered persisted file:
    - dynamic `Engine\\<TabName>StandartPairsSettings.txt` path
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/BotTabPairStandartSettingsPersistenceTests.cs`
      - `SaveStandartSettings_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadStandartSettings_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`206/206`).
- **Commit:** `37ee3f644`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #58)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated pair instance settings persistence to JSON with legacy fallback:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs` (`PairToTrade`)
  - Covered persisted file:
    - dynamic `Engine\\<Name>PairsSettings.txt` path
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/PairToTradeSettingsPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`208/208`).
- **Commit:** `d320644cf`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #59)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated position controller stop-limit settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Journal/Internal/PositionController.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>DealControllerStopLimits.txt` path
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/PositionControllerStopLimitsPersistenceTests.cs`
      - `TrySaveStopLimits_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadStopLimits_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`210/210`).
- **Commit:** `117e637c1`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #60)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated optimizer bot cache persistence to JSON with legacy fallback:
    - `project/OsEngine/Robots/BotFactory.cs`
  - Covered persisted file:
    - static `Engine\\OptimizerBots.txt` path
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/BotFactoryOptimizerBotsPersistenceTests.cs`
      - `SaveOptimizerBotsNamesToFile_ShouldPersistJson_AndLoadRoundTrip`
      - `LoadOptimizerBotsNamesFromFile_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`212/212`).
- **Commit:** `f360c8d47`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #61)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated position controller deals persistence to JSON with legacy fallback:
    - `project/OsEngine/Journal/Internal/PositionController.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>DealController.txt` path
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/PositionControllerDealsPersistenceTests.cs`
      - `SavePositions_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`214/214`).
- **Commit:** `f8f43d786`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #62)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Cmo indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Cmo.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Cmo` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/CmoPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`216/216`).
- **Commit:** `894d83a8c`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #63)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Rsi indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Rsi.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Rsi` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/RsiPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`218/218`).
- **Commit:** `1a59353a2`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #64)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Cci indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/CCI.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Cci` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/CciPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`220/220`).
- **Commit:** `919bf6f32`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #65)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated BearsPower indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/BearsPower.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`BearsPower` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/BearsPowerPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`222/222`).
- **Commit:** `303ecd3d7`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #66)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated BullsPower indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/BullsPower.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`BullsPower` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/BullsPowerPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`224/224`).
- **Commit:** `20437b16b`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #67)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Atr indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Atr.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Atr` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/AtrPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`226/226`).
- **Commit:** `cc818a5cc`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #68)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated AtrChannel indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/AtrChannel.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`AtrChannel` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/AtrChannelPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`228/228`).
- **Commit:** `120fe88f4`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #69)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated BfMfi indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/BfMfi.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`BfMfi` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/BfMfiPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`230/230`).
- **Commit:** `834524ee6`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #70)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Bollinger indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Bollinger.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Bollinger` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/BollingerPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`232/232`).
- **Commit:** `8ce7d4f38`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #71)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated DonchianChannel indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/DonchianChannel.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`DonchianChannel` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/DonchianChannelPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`234/234`).
- **Commit:** `e544032d2`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #72)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated ForceIndex indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/ForceIndex.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`ForceIndex` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ForceIndexPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`236/236`).
- **Commit:** `11575aba9`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #73)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated EfficiencyRatio indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/EfficiencyRatio.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`EfficiencyRatio` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/EfficiencyRatioPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`238/238`).
- **Commit:** `c76e9f543`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #74)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Fractal indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Fractail.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Fractal` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/FractalPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`240/240`).
- **Commit:** `1d1c8bdac`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #75)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Volume indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Volume.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Volume` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/VolumePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`242/242`).
- **Commit:** `8f56e48b2`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #76)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated TickVolume indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/TickVolume.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`TickVolume` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/TickVolumePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`244/244`).
- **Commit:** `e3ef0e217`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #77)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated VolumeOscillator indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/VolumeOscillator.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`VolumeOscillator` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/VolumeOscillatorPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`246/246`).
- **Commit:** `de14af676`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #78)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Roc indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Roc.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Roc` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/RocPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`248/248`).
- **Commit:** `6a70cb00f`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #79)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Momentum indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Momentum.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Momentum` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/MomentumPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`250/250`).
- **Commit:** `3e5e427c1`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #80)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated PriceOscillator indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/PriceOscillator.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`PriceOscillator` indicator)
  - Kept existing nested moving averages persistence behavior:
    - `Engine\\<Name>ma1.txt`
    - `Engine\\<Name>ma2.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/PriceOscillatorPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`252/252`).
- **Commit:** `64e1e006d`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #81)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated OnBalanceVolume indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/OnBalanceVolume.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`OnBalanceVolume` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/OnBalanceVolumePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`254/254`).
- **Commit:** `3c33432d9`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #82)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated WilliamsRange indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/WilliamsRange.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`WilliamsRange` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/WilliamsRangePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`256/256`).
- **Commit:** `5fa987fc6`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #83)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated StandardDeviation indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/StandardDeviation.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`StandardDeviation` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/StandardDeviationPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`258/258`).
- **Commit:** `5dd88635c`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #84)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated AccumulationDistribution indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/AccumulationDistribution.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`AccumulationDistribution` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/AccumulationDistributionPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`260/260`).
- **Commit:** `0dd159cc4`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #85)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Ac indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Ac.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Ac` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/AcPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`262/262`).
- **Commit:** `93b274a3c`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #86)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Line indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Line.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Line` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/LinePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`264/264`).
- **Commit:** `f1025ada2`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #87)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated VerticalHorizontalFilter indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/VerticalHorizontalFilter.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`VerticalHorizontalFilter` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/VerticalHorizontalFilterPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`266/266`).
- **Commit:** `c50c4f237`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #88)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated SimpleVWAP indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/SimpleVWAP.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`SimpleVWAP` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/SimpleVwapPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`270/270`).
- **Commit:** `a811457ef`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #89)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated TradeThread indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/TradeThread.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`TradeThread` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/TradeThreadPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`270/270`).
- **Commit:** `314b9b2bf`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #90)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated KalmanFilter indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/KalmanFilter.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`KalmanFilter` indicator)
  - Hardened legacy decimal parsing in fallback loader:
    - supports invariant and current-culture decimal formats
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/KalmanFilterPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`272/272`).
- **Commit:** `07511e635`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #91)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated AdaptiveLookBack indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/AdaptiveLookBack.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`AdaptiveLookBack` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/AdaptiveLookBackPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`274/274`).
- **Commit:** `0c2b51a43`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #92)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated MoneyFlowIndex indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/MoneyFlowIndex.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`MoneyFlowIndex` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/MoneyFlowIndexPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`276/276`).
- **Commit:** `776dfa9e1`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #93)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated MacdLine indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/MacdLine.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`MacdLine` indicator)
  - Kept existing nested moving averages persistence behavior:
    - `Engine\\<Name>ma1.txt`
    - `Engine\\<Name>ma2.txt`
    - `Engine\\<Name>maSignal.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/MacdLinePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`278/278`).
- **Commit:** `bd5797012`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #94)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated PriceChannel indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/PriceChannel.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`PriceChannel` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/PriceChannelPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`280/280`).
- **Commit:** `b66746a5b`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #95)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Rvi indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Rvi.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Rvi` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/RviPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`282/282`).
- **Commit:** `09defec91`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #96)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated UltimateOscillator indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/UltimateOscillator.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`UltimateOscillator` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/UltimateOscillatorPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`284/284`).
- **Commit:** `f0d4bab51`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #97)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Trix indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Trix.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Trix` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/TrixPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`286/286`).
- **Commit:** `e6e061046`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #98)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated MovingAverage indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/MovingAverage.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`MovingAverage` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/MovingAveragePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`288/288`).
- **Commit:** `deb4f32e4`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #99)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Envelops indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Envelops.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Envelops` indicator)
  - Kept existing nested moving average persistence behavior:
    - `Engine\\<Name>maSignal.txt`
  - Hardened legacy decimal parsing in fallback loader:
    - supports invariant and current-culture decimal formats
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/EnvelopsPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`290/290`).
- **Commit:** `b85fb5fab`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #100)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Adx indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Adx.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Adx` indicator)
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/AdxPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`292/292`).
- **Commit:** `c3c2f2320`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #101)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated DynamicTrendDetector indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/DynamicTrendDetector.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`DynamicTrendDetector` indicator)
  - Hardened legacy decimal parsing in fallback loader:
    - supports invariant and current-culture decimal formats
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/DynamicTrendDetectorPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`294/294`).
- **Commit:** `7d5e0c010`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #102)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated IvashovRange indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/IvashovRange.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`IvashovRange` indicator)
  - Preserved legacy compatibility when optional `LengthAverage` line is missing:
    - fallback uses `LengthMa`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/IvashovRangePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat_WithOptionalLengthAverageMissing`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`296/296`).
- **Commit:** `205b9caa7`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #103)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Alligator indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Alligator.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Alligator` indicator)
  - Preserved legacy compatibility for historical field ordering:
    - lengths, shifts, colors, `PaintOn`, `TypeCalculationAverage`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/AlligatorPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`298/298`).
- **Commit:** `fcc2d80c4`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #104)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated AwesomeOscillator indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/AwesomeOscillator.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`AwesomeOscillator` indicator)
  - Preserved legacy compatibility for historical field ordering:
    - `ColorUp`, `ColorDown`, short/long lengths, `PaintOn`, `TypeCalculationAverage`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/AwesomeOscillatorPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`300/300`).
- **Commit:** `860aeeac4`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #105)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated LinearRegressionCurve indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/LinearRegressionCurve.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`LinearRegressionCurve` indicator)
  - Preserved legacy compatibility with optional trailing line in historical format
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/LinearRegressionCurvePersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat_WithOptionalTrailingLine`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`302/302`).
- **Commit:** `19162e776`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #106)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated ParabolicSaR indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/ParabolicSAR.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`ParabolicSaR` indicator)
  - Preserved legacy compatibility with optional trailing line in historical format
  - Hardened legacy decimal parsing for `Af` and `MaxAf`:
    - supports current-culture and invariant decimal formats
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/ParabolicSarPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat_WithOptionalTrailingLine`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`304/304`).
- **Commit:** `a901de033`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #107)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Ichimoku indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Ishimoku.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Ichimoku` indicator)
  - Preserved legacy compatibility where optional `LengthChinkou` may be absent:
    - fallback uses `LengthSdvig`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/IchimokuPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat_WithOptionalLengthChinkouMissing`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`306/306`).
- **Commit:** `a00527e37`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #108)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated MacdHistogram indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/MacdHistogram.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`MacdHistogram` indicator)
  - Kept existing nested moving average persistence behavior:
    - `Engine\\<Name>ma1.txt`, `Engine\\<Name>ma2.txt`, `Engine\\<Name>maSignal.txt`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/MacdHistogramPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`308/308`).
- **Commit:** `6ffe00b2e`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #109)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated StochasticOscillator indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/StochasticOscillator.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`StochasticOscillator` indicator)
  - Preserved behavior where save normalizes `TypeCalculationAverage` to `Simple`
  - Legacy parser supports historical formats:
    - `type-first` and save-order with optional blank separator line
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/StochasticOscillatorPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat_WithSaveOrdering`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`310/310`).
- **Commit:** `f5b4d9a43`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #110)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated StochRsi indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/StochRsi.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`StochRsi` indicator)
  - Preserved existing persistence scope for settings fields:
    - `ColorK`, `RsiLength`, `StochasticLength`, `K`, `D`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/StochRsiPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`312/312`).
- **Commit:** `f90c2bbdf`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #111)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Pivot indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/Pivot.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Pivot` indicator)
  - Legacy color parser supports historical formats:
    - numeric `ARGB`
    - `Color [Name]`
    - `Color [A=..., R=..., G=..., B=...]`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/PivotPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat_WithColorStrings`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`314/314`).
- **Commit:** `229aeceea`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #112)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated PivotPoints indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/PivotPoints.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`PivotPoints` indicator)
  - Legacy color parser supports historical formats:
    - numeric `ARGB`
    - `Color [Name]`
    - `Color [A=..., R=..., G=..., B=...]`
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/PivotPointsPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat_WithColorStrings`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`316/316`).
- **Commit:** `27e4a88f9`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #113)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Vwap indicator settings persistence to JSON with legacy fallback:
    - `project/OsEngine/Charts/CandleChart/Indicators/VWAP.cs`
  - Covered persisted file:
    - dynamic `Engine\\<Name>.txt` path (`Vwap` indicator)
  - Preserved legacy compatibility for line-based files:
    - boolean flags, dates/times, deviation toggles, colors, and `PaintOn`
  - Hardened legacy `DateTime` parsing:
    - supports current-culture and invariant-culture date formats
  - Added dedicated persistence tests:
    - `project/OsEngine.Tests/VwapPersistenceTests.cs`
      - `Save_ShouldPersistJson_AndLoadRoundTrip`
      - `Load_ShouldSupportLegacyLineBasedFormat`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`318/318`).
- **Commit:** `6dd4756c7`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #114)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Aligned constructor-level settings file checks with migrated path helper:
    - `project/OsEngine/Charts/CandleChart/Indicators/Envelops.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/MacdLine.cs`
  - Replaced remaining direct `@"Engine\\<Name>.txt"` checks with `GetSettingsPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`318/318`).
- **Commit:** `85e655941`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #115)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Aligned `Delete()` settings-file path handling with existing helpers in migrated classes:
    - `project/OsEngine/Candles/TimeFrameBuilder.cs`
    - `project/OsEngine/Alerts/AlertToPrice.cs`
    - `project/OsEngine/Alerts/AlertToChart.cs`
    - `project/OsEngine/Logging/MessageSender.cs`
    - `project/OsEngine/Entity/HorizontalVolume.cs`
    - `project/OsEngine/Entity/NonTradePeriods.cs`
  - Replaced remaining direct `@"Engine\\..."` delete calls with:
    - `GetSettingsPath()` / `GetStoragePath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`318/318`).
- **Commit:** `7cc346247`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #116)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Aligned remaining path checks in `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs` with existing helper methods:
    - spread settings delete uses `GetSpreadSettingsPath()`
    - nested index auto-formula builder delete check uses `GetSettingsPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`318/318`).
- **Commit:** `d3d7a812d`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #117)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Centralized remaining hardcoded settings paths via helper methods in:
    - `project/OsEngine/Market/Servers/AServer.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`
  - Added/used path helpers for delete/save/load sites to remove direct `@"Engine\\..."` path duplication.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`318/318`).
- **Commit:** `ae1cbc3f4`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #118)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Aligned `Delete()` settings path usage in:
    - `project/OsEngine/OsTrader/Panels/Tab/Internal/BotManualControl.cs`
  - Replaced inline `@"Engine\\<name>StrategSettings.txt"` path construction with `GetSettingsPath()`
  - Preserved readonly-file handling behavior before deletion.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`318/318`).
- **Commit:** `ab99494d5`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #119)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Centralized server-instance numbers file path in:
    - `project/OsEngine/Market/ServerMaster.cs`
  - Added helper:
    - `GetServerInstanceNumbersPath(ServerType serverType)`
  - Applied helper in:
    - `TryLoadServerInstance(...)`
    - `TrySaveServerInstance(...)`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`318/318`).
- **Commit:** `b08adc3cf`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #120)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated pair names persistence in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs`
  - `SavePairNames()` now persists DTO JSON via `SettingsManager.Save(...)`.
  - `LoadPairs()` now uses `SettingsManager.Load(..., legacyLoader: ParseLegacyPairNamesToLoadSettings)`.
  - Added legacy line-based parser method to preserve backward compatibility.
  - Added tests:
    - `project/OsEngine.Tests/BotTabPairNamesPersistenceTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`320/320`).
- **Commit:** `d09549b47`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #121)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated screener tab-set persistence in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`
  - `SaveTabs()` now persists DTO JSON via `SettingsManager.Save(...)`.
  - `TryLoadTabs()` now uses `SettingsManager.Load(..., legacyLoader: ParseLegacyScreenerTabSetSettings)`.
  - Added legacy parser for old hash-delimited `TAB1#TAB2#...` format.
  - Added tests:
    - `project/OsEngine.Tests/BotTabScreenerTabSetPersistenceTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`322/322`).
- **Commit:** `fbd5131c9`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #122)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated polygon sequence names persistence in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`
  - `SaveSequencesNames()` now persists DTO JSON via `SettingsManager.Save(...)`.
  - `LoadSequences()` now uses `SettingsManager.Load(..., legacyLoader: ParseLegacyPolygonNamesToLoadSettings)`.
  - Added helper path method for polygons names file and applied it in save/load/delete paths.
  - Added legacy parser for old line-based names format.
  - Added tests:
    - `project/OsEngine.Tests/BotTabPolygonNamesPersistenceTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`324/324`).
- **Commit:** `91c0d5ca6`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #123)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated standard polygon settings persistence in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`
  - `SaveStandartSettings()` and `LoadStandartSettings()` now use `SettingsManager` DTO JSON.
  - Added legacy parser for old line-based ordered format:
    - `ParseLegacyStandartPolygonSettings(string content)`
  - Centralized standard settings file path via helper and reused it in save/load/delete.
  - Added tests:
    - `project/OsEngine.Tests/BotTabPolygonStandartSettingsPersistenceTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`326/326`).
- **Commit:** `71342ca1b`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #124)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated `PolygonToTrade` settings persistence in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`
  - `Save()` and `Load()` now use `SettingsManager` DTO JSON.
  - Added legacy parser for old line-based ordered format:
    - `ParseLegacyPolygonToTradeSettings(string content)`
  - Centralized `PolygonToTrade` settings file path via helper and reused it in load/save/delete.
  - Added tests:
    - `project/OsEngine.Tests/PolygonToTradeSettingsPersistenceTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`328/328`).
- **Commit:** `a86937cc1`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #125)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated trade grids master settings persistence in:
    - `project/OsEngine/OsTrader/Grids/TradeGridsMaster.cs`
  - `SaveGrids()` and `LoadGrids()` now use `SettingsManager` DTO JSON.
  - Added legacy parser for old line-based grid-settings format:
    - `ParseLegacyGridsSettings(string content)`
  - Centralized settings file path via helper and reused it in save/load/delete.
  - Added tests:
    - `project/OsEngine.Tests/TradeGridsMasterPersistenceTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`330/330`).
- **Commit:** `bd3fb9dce`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #126)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in delete flows:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`
      - added/used `GetLegacyStrategSettingsPath()` for legacy `StrategSettings.txt`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabSimple.cs`
      - added/used `GetSettingsBotPath()` for `SettingsBot.txt`
  - Removed remaining inline `@"Engine\\..."` delete path construction in these tab files.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`330/330`).
- **Commit:** `34638af68`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #127)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated chart master settings IO wrapper in:
    - `project/OsEngine/Charts/CandleChart/ChartCandleMaster.cs`
  - `Save()` and `Load()` now use `SettingsManager` DTO JSON with `Lines` payload.
  - Preserved existing indicator reconstruction logic; source of lines changed from direct file reader to loaded DTO.
  - Added legacy parser for old line-based settings format:
    - `ParseLegacySettings(string content)`
  - Centralized settings path via helper and reused it in load/save/delete.
  - Added tests:
    - `project/OsEngine.Tests/ChartCandleMasterSettingsPersistenceTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`332/332`).
- **Commit:** `c787f59fb`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #128)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated Aindicator settings IO wrappers in:
    - `project/OsEngine/Indicators/Aindicator.cs`
  - Parameters and series storage now use `SettingsManager` DTO JSON:
    - `Parametrs.txt` wrapper (`SaveParameters` / load-by-user-parameter)
    - `Values.txt` wrapper (`SaveSeries` / series-load)
  - Added shared legacy parser for old line-based settings format:
    - `ParseLegacyLinesSettings(string content)`
  - Centralized path usage via helper methods for parameters/values/base files.
  - Added tests:
    - `project/OsEngine.Tests/AindicatorPersistenceTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`334/334`).
- **Commit:** `2b9f891c4`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #129)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated alert keeper settings wrapper in:
    - `project/OsEngine/Alerts/AlertMaster.cs`
  - `Save()` and `Load()` now use `SettingsManager` DTO JSON.
  - Preserved existing alert reconstruction flow from keeper records (`AlertToChart` / `AlertToPrice`).
  - Added legacy parser for old line-based keeper format:
    - `ParseLegacyAlertKeeperSettings(string content)`
  - Centralized keeper file path via helper and reused it in load/save/delete.
  - Added tests:
    - `project/OsEngine.Tests/AlertMasterPersistenceTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`336/336`).
- **Commit:** `b1f502fc4`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #130)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/OsTrader/Panels/BotPanel.cs`
  - Centralized strategy-parameters file path into helper:
    - `GetParametersPath()`
  - Replaced remaining inline `@"Engine\\<NameStrategyUniq>Parametrs.txt"` path usage in delete/load/save parameter flows.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`336/336`).
- **Commit:** `c7d3bcbe5`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #131)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated bot-keeper settings wrapper in:
    - `project/OsEngine/OsTrader/OsTraderMaster.cs`
  - `Load()` / `Save()` now use `SettingsManager` DTO JSON with compatibility loader.
  - Updated `BotNames` source to use same compatibility loader for:
    - `SettingsRealKeeper.txt`
    - `SettingsTesterKeeper.txt`
  - Added legacy parser for old line-based keeper format:
    - `ParseLegacyBotKeeperSettings(string content)`
  - Centralized keeper-path construction via helper method.
  - Added tests:
    - `project/OsEngine.Tests/OsTraderMasterKeeperSettingsTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`337/337`).
- **Commit:** `e74ff94f9`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #132)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated bot panel UI layout settings wrapper in:
    - `project/OsEngine/OsTrader/Panels/BotPanelChartUI.xaml.cs`
  - `SaveLeftPanelPosition()` / `CheckPanels()` now use `SettingsManager` DTO JSON.
  - Added legacy parser for old 3-line bool layout format:
    - `ParseLegacyLayoutSettings(string content)`
  - Centralized layout file path via helper:
    - `GetLayoutSettingsPath()`
  - Added tests:
    - `project/OsEngine.Tests/BotPanelChartUILayoutPersistenceTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`339/339`).
- **Commit:** `c2c7e49c5`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #133)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated journal UI layout settings wrapper in:
    - `project/OsEngine/Journal/JournalUi2.xaml.cs`
  - `SaveSettings()` / `LoadSettings()` now use `SettingsManager` DTO JSON.
  - Added legacy parser for old ordered-line layout format:
    - `ParseLegacyLayoutSettings(string content)`
  - Centralized layout path via helper:
    - `GetLayoutSettingsPath()`
  - Added tests:
    - `project/OsEngine.Tests/JournalUi2LayoutSettingsTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`340/340`).
- **Commit:** `b3b813f4e`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #134)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated journal groups settings wrapper in:
    - `project/OsEngine/Journal/JournalUi2.xaml.cs`
  - `SaveGroups()` / `LoadGroups()` now use `SettingsManager` typed DTO JSON.
  - Added legacy parser for old line-based groups format:
    - `ParseLegacyJournalGroupsSettings(string content)`
  - Centralized groups-settings path via helper:
    - `GetJournalGroupsSettingsPath()`
  - Added tests:
    - `project/OsEngine.Tests/JournalUi2GroupsSettingsTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`341/341`).
- **Commit:** `6864c1206`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #135)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Migrated optimizer standard-parameters settings wrappers in:
    - `project/OsEngine/OsOptimizer/OptimizerMaster.cs`
  - Parameters and on/off settings wrappers now use `SettingsManager` DTO JSON with compatibility loaders.
  - Added legacy parsers for old line-based formats:
    - `ParseLegacyStandardParameters(string content)`
    - `ParseLegacyStandardParametersOnOff(string content)`
  - Centralized paths via:
    - `GetStandardParametersPath()`
    - `GetStandardParametersOnOffPath()`
  - Added tests:
    - `project/OsEngine.Tests/OptimizerMasterPersistenceTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `66066f1fb`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #136)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/MainWindow.xaml.cs`
  - Centralized `Engine\\checkFile.txt` path in `CheckWorkWithDirectory()` via helper:
    - `GetDirectoryCheckFilePath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `40ebec17c`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #137)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Market/Servers/YahooFinance/YahooServer.cs`
  - Centralized Yahoo securities cache file paths via:
    - `GetYahooSecuritiesPath()`
    - `GetYahooSecuritiesFtpPath()`
  - Replaced remaining inline cache path literals in read/download flow.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `72610f8c2`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #138)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Journal/Internal/PositionController.cs`
  - `Delete()` now uses existing helper methods:
    - `GetDealsPath()`
    - `GetStopLimitsPath()`
  - Removed remaining inline deal-controller delete path construction.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `a3b673c28`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #139)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
  - Centralized optimizer file paths via helper methods:
    - `GetClearingsPath()`
    - `GetNonTradePeriodsPath()`
    - `GetSettingsPath()`
  - Replaced remaining inline optimizer storage path literals in save/load methods.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `9a14ce401`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #140)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Market/Servers/Polygon/PolygonServer.cs`
  - Centralized Polygon securities cache path via helper:
    - `GetSecuritiesCachePath()`
  - Replaced remaining inline cache path literals in read/write flow.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `781da233a`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #141)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Logging/Log.cs`
  - Centralized daily log storage paths via helper methods:
    - `GetLogsDirectoryPath()`
    - `GetCurrentDayLogPath()`
  - Replaced remaining inline `Engine\\Log\\...` path construction in:
    - show-file action
    - delete cleanup
    - save thread
    - load-last-day flow
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `dde945a05`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #142)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs`
  - Removed hardcoded `Engine\\SystemStress` directory checks from 4 `Save()` methods:
    - `RamMemoryUsageAnalyze`
    - `CpuUsageAnalyze`
    - `EcqUsageAnalyze`
    - `MoqUsageAnalyze`
  - Directory path for create-if-missing now resolves from `GetSettingsPath()` via `Path.GetDirectoryName(...)`.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `63e511199`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #143)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Market/Servers/AServerOrdersHub.cs`
  - Centralized active-orders LiteDB path construction via helper methods:
    - `GetDataBasesDirectoryPath()`
    - `GetActiveOrdersDatabasePath()`
  - Replaced duplicated inline path-building in:
    - `LoadOrdersFromFile()`
    - `SaveOrdersInFile()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `9ff2134f9`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #144)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
  - Centralized securities LiteDB path construction via helper methods:
    - `GetDataBasesDirectoryPath()`
    - `GetSecuritiesDatabasePath()`
  - Replaced duplicated inline path-building in:
    - `LoadSecuritiesFromFile()`
    - `SaveSecuritiesToFile()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `2ba912695`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #145)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Charts/ColorKeeper/ChartMasterColorKeeper.cs`
  - Centralized chart-color settings directory path via:
    - `GetSettingsDirectoryPath()`
  - Updated:
    - `GetSettingsPath()` to build file path from directory helper
    - `EnsureDirectoryExists()` to use the same directory helper
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `c3d3edd25`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #146)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/BotFactory.cs`
  - Replaced hardcoded optimizer-bots file constant usage with helper:
    - `GetOptimizerBotsFilePath()`
  - Updated path usage in:
    - `LoadOptimizerBotsNamesFromFile()`
    - `SaveOptimizerBotsNamesToFile()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `29b86664e`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #147)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Market/Servers/AServer.cs`
  - Centralized ServerDopSettings path construction via helper methods:
    - `GetServerDopSettingsDirectoryPath()`
    - `GetServerDopSettingsDirectoryPathForCurrentServerType()`
    - `GetSecuritiesLeveragePath()`
  - Replaced duplicated inline path usage in:
    - `LoadSavedSecurities()`
    - `LoadLeverageFromFile()`
    - `SaveLeverageToFile()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `702af20db`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #148)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/MainWindow.xaml.cs`
  - Centralized current-directory executable path via helper:
    - `GetCurrentDirectoryExecutablePath()`
  - Replaced remaining inline `...\\OsEngine.exe` path construction in:
    - single-instance check flow
    - reboot flow
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `d7d67765c`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #149)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Entity/SecuritiesUi.xaml.cs`
  - Centralized ServerDopSettings path construction via helper methods:
    - `GetServerDopSettingsDirectoryPath()`
    - `GetServerTypeDopSettingsDirectoryPath()`
    - `GetSecurityDopSettingsFilePath(string fileName)`
  - Replaced duplicated inline path usage in security override save flow.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `0e6e16e51`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #150)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Entity/SetLeverageUi.xaml.cs`
  - Centralized leverage settings file path via helper:
    - `GetSecuritiesLeveragePath()`
  - Replaced inline `Engine\\ServerDopSettings\\...json` path construction in:
    - `LoadLeverageFromFile()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `98661e07e`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #151)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Market/Servers/AstsBridge/AstsBridgeServer.cs`
  - Centralized settings file path via helper:
    - `GetSettingsPath()`
  - Replaced duplicated inline `Engine\\AstsServer.txt` path usage in:
    - `Load()`
    - `Save()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `eed3c7982`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #152)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Market/Servers/TelegramNews/TelegramNewsServer.cs`
  - Centralized Telegram log/session paths via helper methods:
    - `GetTelegramLogsDirectoryPath()`
    - `GetTelegramLogFilePath()`
    - `GetTelegramSessionPath()`
  - Replaced duplicated inline path usage in:
    - constructor log setup
    - `Config("session_pathname")`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `f546e7862`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #153)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/AlgoStart/AlgoStart2Soldiers.cs`
  - Centralized bot settings file path via helper:
    - `GetTradeSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `SaveTradeSettings()`
    - `LoadTradeSettings()`
    - `AlgoStart2ScreenerSoldiers_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `7754eae34`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #154)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/CounterTrend/StrategyBollinger.cs`
  - Centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `f3c03056d`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #155)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/CounterTrend/RsiContrtrend.cs`
  - Centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `b753c7f0d`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #156)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/CounterTrend/WilliamsRangeTrade.cs`
  - Centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `51234598a`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #157)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/Trend/SmaStochastic.cs`
  - Centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `3729b76c8`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #158)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/Trend/WsurfBot.cs`
  - Centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `d2b13943f`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #159)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/Trend/PriceChannelTrade.cs`
  - Centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `9f47c9158`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #160)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/MarketMaker/PairTraderSimple.cs`
  - Centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `809394e8b`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #161)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/MarketMaker/PairTraderSpreadSma.cs`
  - Centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `4252a0096`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #162)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/MarketMaker/MarketMakerBot.cs`
  - Centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `e3aeeec7a`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #163)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/Grids/GridScreenerAdaptiveSoldiers.cs`
  - Centralized bot settings file path via helper:
    - `GetTradeSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `SaveTradeSettings()`
    - `LoadTradeSettings()`
    - `ThreeSoldierAdaptiveScreener_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `4f86afb7a`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #164)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/Screeners/PinBarVolatilityScreener.cs`
  - Centralized bot settings file path via helper:
    - `GetTradeSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `SaveTradeSettings()`
    - `LoadTradeSettings()`
    - `Screener_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `bfc67e01b`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #165)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/Screeners/ThreeSoldierAdaptiveScreener.cs`
  - Centralized bot settings file path via helper:
    - `GetTradeSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `SaveTradeSettings()`
    - `LoadTradeSettings()`
    - `ThreeSoldierAdaptiveScreener_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `8653106aa`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #166)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/Patterns/PivotPointsRobot.cs`
  - Centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `Save()`
    - `Load()`
    - `Strategy_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `f208029b5`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #167)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/OnScriptIndicators/RsiTrade.cs`
  - Centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `Strategy_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `be6fd56ba`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #168)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/TechSamples/VisualSettingsParametersExample.cs`
  - Centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `Strategy_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `8ad0d1ab1`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #169)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/Trend/MomentumMacd.cs`
  - Centralized bot settings file path via helper:
    - `GetSettingsPath()`
  - Replaced duplicated inline path usage in:
    - `Strategy_DeleteEvent()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `7f378d8c6`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #171)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
  - Centralized log directory and file path construction via helpers:
    - `GetLogDirectoryPath()`
    - `GetTradesLogPath()`
    - `GetOrdersLogPath()`
    - `GetIncomingMfixLogPath()`
    - `GetRecoveryLogPath()`
  - Replaced duplicated inline log file path construction for:
    - trades log
    - orders log
    - incoming MFIX log
    - recovery log
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `2c68cd253`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #172)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
  - Centralized log directory and file path construction via helpers:
    - `GetLogDirectoryPath()`
    - `GetUdpLogPath()`
    - `GetXOrdersLogPath()`
    - `GetMfixLogPath()`
  - Replaced duplicated inline log file path construction for:
    - UDP log
    - XOrders log
    - MFIX log
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `0ccb40fec`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #173)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/TechSamples/CustomTableInTheParamWindowSample.cs`
  - Centralized lines-storage file path via helper:
    - `GetLinesPath()`
  - Replaced duplicated inline path usage in:
    - `DeleteBotEvent()`
    - `SaveLines()`
    - `LoadLines()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `16fe5fb57`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #170)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
  - Centralized connector log directory via:
    - `MoexFixFastTwimeFuturesServer.GetConnectorLogDirectoryPath()`
  - Replaced duplicated inline log file path construction for:
    - trades log
    - orders log
    - trading server log
    - recovery log
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `51a4b6501`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #174)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/Helpers/PayOfMarginBot.cs`
  - Centralized table-storage file paths via helpers:
    - `GetTableSummPath()`
    - `GetTablePeriodPath()`
  - Replaced duplicated inline path usage in:
    - `LoadTableSumm()`
    - `SaveTableSumm()`
    - `LoadTable()`
    - `SaveTable()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `88f1503c9`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #175)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/Helpers/TaxPayer.cs`
  - Centralized table-period file path via helper:
    - `GetTablePeriodPath()`
  - Replaced duplicated inline path usage in:
    - `LoadTable()`
    - `SaveTable()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `386d736f0`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #176)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Indicators/Aindicator.cs`
  - Centralized indicator storage path prefix via helper:
    - `GetIndicatorStoragePrefix()`
  - Replaced duplicated path-prefix usage in:
    - `GetParametersPath()`
    - `GetValuesPath()`
    - `GetBasePath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `1cee4b529`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #177)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs`
  - Centralized BotTabPair tab-settings path prefix via helper:
    - `GetTabStoragePrefix()`
  - Replaced duplicated path-prefix usage in:
    - `GetStandartSettingsPath()`
    - `GetLegacyStrategSettingsPath()`
    - `GetPairsNamesToLoadPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `2628159d2`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #178)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`
  - Centralized BotTabPolygon tab-settings path prefix via helper:
    - `GetTabStoragePrefix()`
  - Replaced duplicated path-prefix usage in:
    - `GetStandartPolygonSettingsPath()`
    - `GetLegacyStrategSettingsPath()`
    - `GetPolygonsNamesToLoadPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `3e64215cd`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #179)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`
  - Centralized screener storage path prefix via helper:
    - `GetScreenerStoragePrefix()`
  - Replaced duplicated path-prefix usage in:
    - `GetScreenerSettingsPath()`
    - `GetScreenerTabSetPath()`
    - `GetIndicatorsPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `e5c82e4c7`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #180)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Market/Servers/AServer.cs`
  - Centralized server-storage path prefix via helper:
    - `GetServerStoragePrefix()`
  - Replaced duplicated path-prefix usage in:
    - `GetServerParamsPath()`
    - `GetServerSettingsPath()`
    - `GetNonTradePeriodsPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `cb470b2e5`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #181)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Market/Servers/ComparePositionsModule.cs`
  - Centralized compare-module settings path prefix via helper:
    - `GetSettingsPathPrefix()`
  - Replaced duplicated path-prefix usage in:
    - `GetSettingsPath()`
    - `GetIgnoredSettingsPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `975310d73`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #182)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Journal/Internal/PositionController.cs`
  - Centralized position-controller storage path prefix via helper:
    - `GetStoragePathPrefix()`
  - Replaced duplicated path-prefix usage in:
    - `GetDealsPath()`
    - `GetStopLimitsPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `f8feac292`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #183)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/OsOptimizer/OptimizerMaster.cs`
  - Centralized optimizer standard-parameters path prefix via helper:
    - `GetStandardParametersPathPrefix()`
  - Replaced duplicated path-prefix usage in:
    - `GetStandardParametersPath()`
    - `GetStandardParametersOnOffPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `a7205f384`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #184)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Robots/Helpers/PayOfMarginBot.cs`
  - Centralized PayOfMarginBot storage path prefix via helper:
    - `GetStoragePathPrefix()`
  - Replaced duplicated path-prefix usage in:
    - `GetTableSummPath()`
    - `GetTablePeriodPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `daeb7a686`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #185)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs`
  - Centralized SystemAnalyze settings directory path via helper:
    - `SystemUsageAnalyzePaths.GetSettingsPath(string fileName)`
  - Replaced duplicated direct path usage in:
    - `RamMemoryUsageAnalyze.GetSettingsPath()`
    - `CpuUsageAnalyze.GetSettingsPath()`
    - `EcqUsageAnalyze.GetSettingsPath()`
    - `MoqUsageAnalyze.GetSettingsPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `c2062cbea`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #186)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs`
  - Centralized Engine storage-path construction via helper:
    - `BotTabIndexPaths.BuildEnginePath(string uniqueName, string fileName)`
  - Replaced direct path construction in:
    - `GetSpreadSettingsPath()`
    - `IndexFormulaBuilder.GetSettingsPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `243b94e9a`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #187)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs`
  - Centralized Engine storage-path construction via helper:
    - `BotTabPairPaths.BuildEnginePath(string uniqueName, string fileName)`
  - Replaced direct/prefix path construction in:
    - `GetTabStoragePrefix()`
    - `PairToTrade.GetPairSettingsPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `1c8e31547`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #188)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`
  - Centralized Engine storage-path construction via helper:
    - `BotTabPolygonPaths.BuildEnginePath(string uniqueName, string fileName)`
  - Replaced direct/prefix path construction in:
    - `GetTabStoragePrefix()`
    - `PolygonToTrade.GetSettingsPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `f153b8892`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #189)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Centralized tester settings file path construction via helper:
    - `GetTesterSettingsFilePath(string suffix)`
  - Replaced direct path usage in:
    - `GetTesterSettingsPath()`
    - `GetClearingSettingsPath()`
    - `GetNonTradePeriodsPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `4de26c07c`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #190)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
  - Centralized optimizer settings file path construction via helper:
    - `GetOptimizerSettingsFilePath(string suffix)`
  - Replaced direct path usage in:
    - `GetClearingsPath()`
    - `GetNonTradePeriodsPath()`
    - `GetSettingsPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `f0b53d6e8`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #191)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Layout/GlobalGUILayout.cs`
  - Centralized layout-related engine file path construction via helper:
    - `GetLayoutFilePath(string fileName)`
  - Replaced direct path usage in:
    - `GetLayoutSettingsPath()`
    - `GetScreenResolutionPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `c63e32199`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #192)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/OsTrader/Gui/BlockInterface/BlockMaster.cs`
  - Centralized prime-settings file path construction via helper:
    - `GetPrimeSettingsPath(string fileName)`
  - Replaced direct path usage in:
    - `GetPasswordPath()`
    - `GetIsBlockedPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `c6c5da1e3`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #193)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Journal/JournalUi2.xaml.cs`
  - Centralized journal UI engine path construction via helper:
    - `BuildEnginePath(string fileName)`
  - Replaced direct path construction in:
    - `GetLayoutSettingsPath()`
    - `GetJournalGroupsSettingsPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `1515f08b4`
- **Push:** no (manual push by user)

### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #194)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes:**
  - Path consistency cleanup in:
    - `project/OsEngine/Market/Servers/AServer.cs`
  - Centralized generic Engine path construction via helper:
    - `BuildEnginePath(string fileOrFolderName)`
  - Replaced direct Engine-prefix usage in:
    - `GetServerStoragePrefix()`
    - `GetServerDopSettingsDirectoryPath()`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `fad8879a8`
- **Push:** no (manual push by user)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #195)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error logging in:
    - `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorage.cs`
  - Replaced silent catch blocks with `SendLogMessage(ex.ToString(), LogMessageType.Error)` in:
    - security loading catch (candle scan)
    - security loading catch (trade scan)
    - `LoadSecurityDopSettings(...)`
    - `SaveSecurityDopSettings(...)`
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `e05d9882f`
- **Push:** no (manual push by user)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #196)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added trace-based error visibility in:
    - `project/OsEngine/Layout/GlobalGUILayout.cs`
  - Replaced silent catch blocks with `Trace.TraceWarning(ex.ToString())` in:
    - `Save()`
    - `Load()`
    - `SaveResolution(...)`
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `81401dff1`
- **Push:** no (manual push by user)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #197)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added trace-based error visibility in:
    - `project/OsEngine/PrimeSettings/PrimeSettingsMaster.cs`
  - Replaced silent catch blocks with `Trace.TraceWarning(ex.ToString())` in:
    - `Save()`
    - `Load()`
  - Preserved existing fallback behavior (`_reportCriticalErrors = true` on load failure).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `a0fce1fd7`
- **Push:** no (manual push by user)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #198)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added trace-based error visibility in:
    - `project/OsEngine/Logging/MessageSender.cs`
  - Replaced silent catch blocks with `Trace.TraceWarning(ex.ToString())` in:
    - `ApplySettings(...)`
    - `Save()`
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `bad77a7eb`
- **Push:** no (manual push by user)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #199)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/Charts/ClusterChart/ChartClusterMaster.cs`
  - Replaced silent catch placeholders with `SendErrorMessage(error)` in:
    - `Save()`
    - `Load()`
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `b5b8632c7`
- **Push:** no (manual push by user)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #200)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsOptimizer/OptimizerMaster.cs`
  - Replaced silent catch blocks with `SendLogMessage(ex.ToString(), LogMessageType.Error)` in:
    - standard parameter load
    - standard parameter save
    - parameters-on/off load
    - parameters-on/off save
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `f8cd05418`
- **Push:** no (manual push by user)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #201)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs`
  - Replaced silent catch blocks with `SendNewLogMessage(ex.ToString(), LogMessageType.Error)` in `IndexFormulaBuilder`:
    - settings `Load()`
    - settings `Save()`
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `569b03b11`
- **Push:** no (manual push by user)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #202)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs`
  - Replaced silent catch blocks with `SendNewLogMessage(ex.ToString(), LogMessageType.Error)` in main `BotTabIndex`:
    - spread settings `Save()`
    - spread settings `Load()`
  - Preserved existing fallback behavior in catch blocks.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `94827d7e9`
- **Push:** no (manual push by user)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #203)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`
  - Replaced silent catch blocks with `SendNewLogMessage(ex.ToString(), LogMessageType.Error)` in:
    - `SaveTabs()`
    - `SaveSettings()`
    - `LoadSettings()` (with preserved fallback defaults)
    - `LoadIndicators()`
    - `SaveIndicators()`
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `908ceca73`
- **Push:** no (manual push by user)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #204)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs`
  - Replaced silent catch blocks with `SendNewLogMessage(ex.ToString(), LogMessageType.Error)` in:
    - tab-level `Delete()` cleanup catches for settings file deletions
    - `SaveStandartSettings()`
    - `LoadStandartSettings()`
    - `SavePairs()`
    - `TryRePaintRow(...)`
    - `PairToTrade.Load()`
    - `PairToTrade.Save()`
    - `PairToTrade.Delete()`
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `7b7a7b092`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #205)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs`
  - Replaced silent `catch { return; }` blocks with logging + preserved return:
    - `catch (Exception ex) { SendNewLogMessage(ex.ToString(), LogMessageType.Error); return; }`
  - Applied in grid click handling paths where tab number is parsed from selected row:
    - delete path (`column == 5`)
    - open/settings path (`column == 4`)
  - Preserved existing behavior (early return remains unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `82c56183f`
- **Push:** no (will be included in next periodic push)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #206)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`
  - Replaced silent catch blocks with `SendNewLogMessage(ex.ToString(), LogMessageType.Error)` in:
    - tab-level `Delete()` cleanup catches for settings file deletions
    - `SaveStandartSettings()`
    - `LoadStandartSettings()`
    - `SaveSequences()`
    - `TryRePaintRow(...)`
    - `PolygonToTrade.Load()`
    - `PolygonToTrade.Save()`
    - `PolygonToTrade.Delete()`
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `26cd47240`
- **Push:** no (will be included in next periodic push)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #207)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`
  - Replaced silent `catch { return; }` blocks with logging + preserved return:
    - `catch (Exception ex) { SendNewLogMessage(ex.ToString(), LogMessageType.Error); return; }`
  - Applied in grid click handling paths where sequence index is parsed from selected row.
  - Preserved existing behavior (early return remains unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `b73b6043e`
- **Push:** no (will be included in next periodic push)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #208)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs`
  - Replaced remaining silent catches with `SendNewLogMessage(ex.ToString(), LogMessageType.Error)` in:
    - duplicate-last-candle trimming (`Candles.RemoveAt(...)` guard)
    - candle merge loop in `ConcateCandleAndCandle(...)`
    - `_lastTimeUpdate` parsing in `TryRebuidFormula(...)`
  - Preserved existing behavior (control flow unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `403c56c57`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #209)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`
  - Added `using System.Diagnostics;`.
  - Replaced remaining silent catches with:
    - `Trace.TraceWarning(ex.ToString())` in static draw thread catch
    - `SendNewLogMessage(ex.ToString(), LogMessageType.Error)` in `EventsIsOn` setter catch
    - `SendNewLogMessage(ex.ToString(), LogMessageType.Error)` in `EmulatorIsOn` setter per-tab catch
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `36767cf36`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #210)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabCluster.cs`
  - Replaced silent catch blocks with `SendNewLogMessage(ex.ToString(), LogMessageType.Error)` in:
    - `Save()`
    - `Load()`
  - Preserved existing fallback behavior in `Load()` catch (`_eventsIsOn = true`).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `0d1bf3a36`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #211)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabNews.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent static-thread catch with:
    - `catch (Exception ex) { Thread.Sleep(5000); Trace.TraceWarning(ex.ToString()); }`
  - Preserved existing behavior (catch throttle sleep remains).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - First run `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` had transient single failure (`Collection was modified` in WinForms log init).
  - Immediate rerun passed (`343/343`).
- **Commit:** `d25fccb64`
- **Push:** no (will be included in next periodic push)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #212)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabSimple.cs`
  - Added `using System.Diagnostics;`.
  - Replaced remaining silent catches with:
    - `SetNewLogMessage(ex.ToString(), LogMessageType.Error)` in `_tabsToCheckPositionEvent` cleanup catch (`Delete()`)
    - `SetNewLogMessage(ex.ToString(), LogMessageType.Error)` in `_lastTradeTime` init catch
    - `Trace.TraceWarning(ex.ToString())` in static `PositionsSenderThreadArea()` catch
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `3f8562653`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #213)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/Internal/PositionOpenUi2.xaml.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/Internal/PositionCloseUi2.xaml.cs`
  - Added `using System.Diagnostics;` in both files.
  - Replaced silent close-handler catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Applied in window close cleanup handlers (`PositionOpenUi2_Closed(...)`).
  - Preserved existing behavior (cleanup flow unchanged, no rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `8ed6d3499`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #214)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabIndexUi.xaml.cs`
  - Replaced silent catch in `CheckBoxPercentNormalization_Click(...)` with:
    - `ServerMaster.SendNewLogMessage(ex.ToString(), Logging.LogMessageType.Error)`
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `e8381b976`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #215)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/Internal/BotManualControl.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent catch in `CheckManualControlPositionEvents(...)` list-access (`openDeals[i]`) with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); continue; }`
  - Preserved existing behavior (`continue` flow retained).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `0a7711fde`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #216)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/Internal/PositionCloseUi2.xaml.cs`
  - Replaced silent catch in `RepaintCurPosStatus()` with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `363f6666b`
- **Push:** no (will be included in next periodic push)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #217)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabIndexUi.xaml.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent catch in `PaintPrices()` with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `019cb5eea`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #218)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPoligonSecurityAddUi.xaml.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent catches in `ConnectorCandlesUi_Closing(...)` with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Applied in:
    - server events unsubscription block
    - UI events/grid cleanup block
  - Preserved existing behavior (cleanup flow unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `690960d03`
- **Push:** no (will be included in next periodic push)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #219)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPairAutoSelectPairsUi.xaml.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent parse catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Applied in:
    - `CreatePairNames()` (`maxOneNamePairsCount` parsing)
    - `ButtonAccept_Click(...)` (commission parsing)
  - Preserved existing behavior and fallback defaults (`maxOneNamePairsCount = 5`, `commissionValue = 0`).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `14cd34d06`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #220)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPairUi.xaml.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent catch in `BotTabPairUi_Closed(...)` cleanup block with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Preserved existing behavior (cleanup flow unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `87d30f983`
- **Push:** no (will be included in next periodic push)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #221)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygonUi.xaml.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Applied in:
    - `BotTabPolygonUi_Closed(...)` cleanup catch
    - `PaintGrid()` catch
    - `TryRePaintRow(...)` catch
  - Preserved existing behavior (no exception rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `f55630951`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #222)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreenerUi.xaml.cs`
  - Added `using System.Diagnostics;`.
  - Replaced cleanup silent catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Applied in:
    - three catches inside `BotTabScreenerUi_Closed(...)`
    - `DeleteGridSecurities()` catch
    - `DeleteCandleRealizationGrid()` catch
  - Preserved existing behavior (cleanup flow unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `9775f8759`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #223)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreenerUi.xaml.cs`
  - Replaced two remaining silent catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Applied in:
    - commission parse fallback in `ButtonAccept_Click(...)`
    - `DeleteCandleRealizationGrid()` catch
  - Preserved existing behavior and fallback defaults.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `b7f1eca6e`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #224)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygonCommonSettingsUi.xaml.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent catches in `SaveSettingsFromUiToBot()` with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Applied to all parse/apply blocks (order/action/commission/delay/separator and numeric settings).
  - Preserved existing behavior (save pipeline and fallback semantics unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `76ab09d79`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #225)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPairUi.xaml.cs`
  - Replaced remaining silent catches in text-change handlers with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Applied in:
    - `TextBoxSec2Slippage_TextChanged(...)`
    - `TextBoxSec2Volume_TextChanged(...)`
    - `TextBoxSec1Slippage_TextChanged(...)`
    - `TextBoxSec1Volume_TextChanged(...)`
  - Preserved existing behavior (assignment + `Save()` flow unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `128a849af`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #226)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/RiskManager/RiskManager.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent catch in static `WatcherHome()` with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Preserved existing behavior (loop resilience/control flow unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `907ae7b3d`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #227)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Grids/TradeGridsMaster.cs`
  - Replaced silent catches with:
    - `SendNewLogMessage(ex.ToString(), LogMessageType.Error)`
  - Applied in:
    - `Delete()` settings-file delete catch
    - confirmation-dialog catch in `DeleteAtNum(...)`
    - `SaveGrids()` catch
    - `LoadGrids()` catch
  - Preserved existing behavior (control flow/fallback unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `40d037e83`
- **Push:** no (will be included in next periodic push)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #228)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Grids/TradeGridAutoStarter.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent catch in `LoadFromString(...)` (legacy optional fields parse) with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Preserved existing behavior (best-effort legacy parsing, no rethrow).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `1e5007e8c`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #229)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/OsTraderMaster.cs`
  - Replaced silent catches with:
    - `SendNewLogMessage(error.ToString(), LogMessageType.Error)`
  - Applied in:
    - `Save()` catch (keeper settings persistence)
    - `CancelOrdersWithSecurity(...)` catch (server-side cancel-all fallback)
  - Preserved existing behavior (control flow/fallback unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `596e5809c`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #230)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/GlobalPositionViewer.cs`
  - Replaced silent catches with:
    - `SendNewLogMessage(ex.ToString(), LogMessageType.Error)`
  - Applied in:
    - `_gridClosePoses_DoubleClick(...)`
    - `_gridOpenPoses_DoubleClick(...)`
  - Preserved existing behavior (handlers remain exception-safe).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `c175dabc4`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #231)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs`
  - Replaced silent load/save catches with:
    - `catch (Exception ex) { ServerMaster.SendNewLogMessage(ex.ToString(), Logging.LogMessageType.Error); }`
  - Applied in `Load()`/`Save()` blocks of:
    - `RamMemoryUsageAnalyze`
    - `CpuUsageAnalyze`
    - `EcqUsageAnalyze`
    - `MoqUsageAnalyze`
  - Preserved existing behavior (load/save fallback control flow unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `b24187f91`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #232)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Gui/BlockInterface/BlockMaster.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Applied in encrypted settings accessors:
    - `Password` getter/setter
    - `IsBlocked` getter/setter
  - Preserved existing behavior (fallback return values unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `3ede2a84b`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #233)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/BotPanel.cs`
  - Replaced silent catch in `Delete()` chart close block with:
    - `SendNewLogMessage(error.ToString(), LogMessageType.Error)`
  - Preserved existing behavior (delete pipeline continues).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore` succeeded (`343/343`).
- **Commit:** `16e921c47`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #234)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/BotPanelChartUI.xaml.cs`
  - Replaced silent inner catch in `CheckPanels()` layout settings load block with:
    - `catch (Exception ex) { SendNewLogMessage(ex.ToString(), LogMessageType.Error); }`
  - Preserved existing behavior (on settings load failure, panel flags remain on current/default values).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `d135e91f1`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #235)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/BotPanelChartUI.xaml.cs`
  - Replaced silent cleanup catch block with:
    - `catch (Exception ex) { SendNewLogMessage(ex.ToString(), LogMessageType.Error); }`
  - Preserved existing behavior (cleanup remains non-throwing).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `387e07190`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #236)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit warning visibility in:
    - `project/OsEngine/OsTrader/Gui/BotTabsPainter.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent `PaintPos()` catch block with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); }`
  - Preserved existing behavior (async highlight flow stays exception-safe).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `769cd09ff`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #237)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalizeUi.xaml.cs`
  - Replaced silent catches in tooltip handlers with:
    - `catch (Exception ex) { ServerMaster.SendNewLogMessage(ex.ToString(), LogMessageType.Error); }`
  - Applied in:
    - `ButtonEcq_Click(...)`
    - `ButtonMoqToolTip_Click(...)`
  - Preserved existing behavior (tooltip handlers remain exception-safe).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `8d482b291`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #238)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalizeUi.xaml.cs`
  - Replaced silent catches in checkbox toggle handlers with:
    - `catch (Exception ex) { ServerMaster.SendNewLogMessage(ex.ToString(), LogMessageType.Error); }`
  - Applied in:
    - `CheckBoxCpuCollectDataIsOn_Checked(...)`
    - `CheckBoxRamCollectDataIsOn_Checked(...)`
    - `CheckBoxEcqCollectDataIsOn_Checked(...)`
    - `CheckBoxMoqCollectDataIsOn_Checked(...)`
  - Preserved existing behavior (handlers remain exception-safe).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `2a9fe06b9`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #239)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit warning visibility in:
    - `project/OsEngine/OsTrader/Gui/BotTabsPainter.cs`
  - Replaced silent `ColoredRow(...)` catch with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); return; }`
  - Preserved existing behavior (`return` on paint failure).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `c786ea31d`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #240)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit warning visibility in:
    - `project/OsEngine/OsTrader/BuyAtStopPositionsViewer.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent return catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); return; }`
  - Applied in:
    - `PaintPos(DataGridView grid)`
    - `ColoredRow(Color color)`
    - inner row-number parse catch in `PositionCloseForNumber_Click(...)`
  - Preserved existing behavior (`return` fallback on selection/paint failures).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `f4d38e55c`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #241)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit warning visibility in:
    - `project/OsEngine/OsTrader/GlobalPositionViewer.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent return catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); return; }`
  - Applied in:
    - `PaintPos(DataGridView grid)` bot-tab extraction block
    - `ColoredRow(Color color)`
  - Preserved existing behavior (`return` fallback on selection/paint failures).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `167e64796`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #242)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit warning visibility in:
    - `project/OsEngine/OsTrader/GlobalPositionViewer.cs`
  - Replaced silent row-number parse catches with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); return; }`
  - Applied in:
    - `ClosePositionClearDelete_Click(...)`
    - `PositionCloseForNumber_Click(...)`
    - `PositionNewStop_Click(...)`
    - `PositionNewProfit_Click(...)`
    - `PositionClearDelete_Click(...)`
  - Preserved existing behavior (`return` fallback on invalid current-row state).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `dbfcd2697`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #243)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit warning visibility in:
    - `project/OsEngine/OsTrader/GlobalPositionViewer.cs`
  - Replaced silent watcher-loop catch with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); continue; }`
  - Applied in journals aggregation block inside watcher thread.
  - Preserved existing behavior (`continue` fallback remains unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `eeb8655d5`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #244)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabOptionsUi.xaml.cs`
  - Replaced empty cleanup catch in `BotTabOptionsUi_Closed(...)` with:
    - `catch (Exception ex) { ServerMaster.SendNewLogMessage(ex.ToString(), Logging.LogMessageType.Error); }`
  - Preserved existing behavior (cleanup remains non-throwing).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `2c8a552e2`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #245)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit warning visibility in:
    - `project/OsEngine/OsTrader/AvailabilityServer/ServerAvailabilityMaster.cs`
  - Added `using System.Diagnostics;`.
  - Replaced silent ping fallback catch with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); return null; }`
  - Preserved existing behavior (`null` ping fallback remains unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `8127cf144`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #246)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit warning visibility in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`
  - Replaced silent indicator cleanup catch with:
    - `catch (Exception ex) { Trace.TraceWarning(ex.ToString()); ... }`
  - Applied in `BotTabScreener_IndicatorManuallyCreateEvent(...)` while removing legacy non-`Aindicator` items.
  - Preserved existing behavior (cleanup and resync fallback unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `b24769094`
- **Push:** yes (`origin/master`)

### Resume Checkpoint (Post #246)

- **Status:** Saved
- **Purpose:** fast recovery point if session/MCP hangs or disconnects.
- **Branch/Head at checkpoint creation:** `master` / `f8a1eac43`
- **Last completed increment:** Step 0.3 / `#246`
- **Tracked docs synchronized:** 
  - `refactoring_stage2_progress.md`
  - `refactoring_stage2_execution_log.md`
