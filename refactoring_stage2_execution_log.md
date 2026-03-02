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

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #247)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/OsData/OsDataSet.cs`
  - Replaced empty fallback catches in `GetDataStream(FileStream fs, byte[] prefix)` with:
    - `catch (Exception ex) { SendNewLogMessage(ex.ToString(), LogMessageType.Error); }`
  - Applied in:
    - `GZipStream` probe block
    - `DeflateStream` probe block
  - Preserved existing behavior (stream-format probing fallback and `null` return path remain unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `a185dded0`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #248)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Replaced empty fallback catches in stream probe methods with logging:
    - `TesterServer.GetDataStream(FileStream fs, byte[] prefix)`:
      - `catch (Exception ex) { SendLogMessage(ex.ToString(), LogMessageType.Error); }`
    - `SecurityTester.GetDataStream(FileStream fs, byte[] prefix)`:
      - `catch (Exception ex) { SendLogMessage(ex.ToString()); }`
  - Applied in:
    - `GZipStream` probe block (both methods)
    - `DeflateStream` probe block (both methods)
  - Preserved existing behavior (fallback sequence and `null` return on decode probe failure remain unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `c3dcc077b`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #249)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
  - Replaced empty catches in `Dispose()` log-file cleanup with:
    - `catch (Exception ex) { SendLogMessage(ex.ToString(), LogMessageType.Error); }`
  - Applied in:
    - `_logFile?.Close()`
    - `_logFileXOrders?.Close()`
    - `_logFileMFIX?.Close()`
  - Preserved existing behavior (dispose remains non-throwing and disconnect sequence unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `1a31286ae`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #250)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
  - Replaced empty catches in `Dispose()` log-file cleanup with:
    - `catch (Exception ex) { SendLogMessage(ex.ToString(), LogMessageType.Error); }`
  - Applied in:
    - `_logFileTrades?.Close()`
    - `_logFileOrders?.Close()`
    - `_logFXMFIXMsg?.Close()`
    - `_logFileRecover?.Close()`
  - Preserved existing behavior (dispose remains non-throwing and disconnect sequence unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `90f113f33`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #251)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
  - Replaced empty catches in `Dispose()` log-file cleanup with:
    - `catch (Exception ex) { SendLogMessage(ex.ToString(), LogMessageType.Error); }`
  - Applied in:
    - `_logFileTrades?.Close()`
    - `_logFileOrders?.Close()`
    - `_logTradingMsg?.Close()`
    - `_logFileRecover?.Close()`
  - Preserved existing behavior (dispose remains non-throwing and disconnect sequence unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `7e7c90f70`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #252)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit visibility in:
    - `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs`
  - Replaced empty catch in `GetTickDataToSecurity(...)` trade-tail id update block with:
    - `catch (Exception ex) { SendLogMessage($"Binance Spot trade history pagination skipped tail update: {ex.Message}", LogMessageType.System); }`
  - Preserved existing behavior (exception still ignored for control flow, loader loop behavior unchanged).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `cf7fbbacf`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #253)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit error visibility in:
    - `project/OsEngine/Market/Servers/AServer.cs`
  - Replaced silent catch in `SaveParam()` with:
    - `catch (Exception ex) { SendLogMessage(ex.ToString(), LogMessageType.Error); }`
  - Preserved existing behavior (parameter save flow remains non-throwing).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `c02d8fca9`
- **Push:** yes (`origin/master`)

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #254)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Per user request, executed one-pass bulk replacement for all remaining empty catches under:
    - `project/OsEngine/**/*.cs`
  - Replaced forms:
    - `catch { }`
    - `catch (Exception) { }`
  - With:
    - `catch (System.Exception ex) { System.Diagnostics.Trace.TraceWarning(ex.ToString()); }`
  - Result:
    - `38` empty catches replaced in `28` files.
  - Preserved existing control flow (no rethrow added); improved exception visibility via trace warnings.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `rg -n -U "catch\\s*(\\(\\s*Exception\\s*\\))?\\s*\\{\\s*\\}" project/OsEngine -S` returned no matches.
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
  - Note: one transient file-lock test failure occurred on first run; immediate rerun passed.
- **Commit:** `27b06ca81`
- **Push:** yes (`origin/master`)

### Step 4.1 - Lock Migration (Incremental Adoption #255)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.1
- **Changes:**
  - Migrated synchronization fields in:
    - `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`
  - Replaced:
    - `private readonly object _reportsSync = new object();`
    - `private readonly object _testBotsTimeSync = new object();`
    - `private readonly object _startSync = new object();`
  - With:
    - `private readonly Lock _reportsSync = new();`
    - `private readonly Lock _testBotsTimeSync = new();`
    - `private readonly Lock _startSync = new();`
  - Preserved existing lock usage and synchronization behavior.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `3def2b7e0`
- **Push:** yes (`origin/master`)

### Step 4.1 - Lock Migration (Incremental Adoption #256)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.1
- **Changes:**
  - Migrated synchronization field in:
    - `project/OsEngine/Journal/Internal/PositionController.cs`
  - Replaced:
    - `private static readonly object _workerLocker = new object();`
  - With:
    - `private static readonly Lock _workerLocker = new();`
  - Preserved existing `lock (_workerLocker)` usage and activation behavior.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `14251b5a6`
- **Push:** yes (`origin/master`)

### Step 4.1 - Lock Migration (Incremental Adoption #257)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.1
- **Changes:**
  - Migrated synchronization field in:
    - `project/OsEngine/Entity/HorizontalVolume.cs`
  - Replaced:
    - `public object _tradesArrayLocker = new object();`
  - With:
    - `public readonly Lock _tradesArrayLocker = new();`
  - Added:
    - `using System.Threading;`
  - Preserved existing `lock (_tradesArrayLocker)` usage in `Process(...)` and `ReloadLines()`.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `70bbb526e`
- **Push:** yes (`origin/master`)

### Step 4.1 - Lock Migration (Incremental Adoption #258)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.1
- **Changes:**
  - Bulk-migrated remaining runtime `object`-based lock fields to `Lock` in:
    - `project/OsEngine/Entity/WebSocketOsEngine.cs` (`_ctsLocker`)
    - `project/OsEngine/Logging/ServerMail.cs` (`LokerMessanger`)
    - `project/OsEngine/Logging/ServerWebhook.cs` (`LokerMessanger`)
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs` (`_logLock`)
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs` (`_logLockTrade`, `_logLockOrder`, `_logLockMFIX`, `_logLockRecover`)
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs` (`_logLockTrade`, `_logLockOrder`, `_logLockTrading`, `_logLockRecover`)
  - Preserved all existing `lock (...)` call sites and synchronization behavior.
  - Confirmed remaining `new object()` occurrence is commented code only in:
    - `project/OsEngine/Market/Servers/MFD/MfdServer.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `rg -n "\\bobject\\b\\s+[_A-Za-z0-9]+\\s*=\\s*new\\s*object\\(\\)" project/OsEngine -S` returned only commented code in `MfdServer.cs`.
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** `efc5ab840`
- **Push:** yes (`origin/master`)

### Step 4.2 - Nullable Annotations (Incremental Adoption #259)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Enabled nullable context and annotations in core settings infrastructure:
    - `project/OsEngine/Entity/SafeFileWriter.cs`
      - added `#nullable enable`
      - nullable annotations for optional encoding/content parameters
      - nullable-safe handling for `Path.GetDirectoryName(...)`
    - `project/OsEngine/Entity/SettingsManager.cs`
      - added `#nullable enable`
      - nullable annotations for optional options/default/legacy loader
      - `Load<T>(...)` now returns `T?`
    - `project/OsEngine/Entity/CredentialProtector.cs`
      - added `#nullable enable`
      - nullable input annotations for `Protect(...)` and `TryUnprotect(...)`
  - Updated test compatibility:
    - `project/OsEngine.Tests/SettingsManagerTests.cs`
      - switched loaded models to nullable
      - added explicit `Assert.NotNull(...)` before dereference
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` succeeded (only known NU1900 warning).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
  - Note: one transient `CS2012` file-lock error occurred on first build attempt; rerun succeeded.
- **Commit:** `0b84f0390`
- **Push:** yes (`origin/master`)

### Step 4.2 - Nullable Annotations (Incremental Adoption #260)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Enabled nullable context in optimizer strategy contract layer:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerEnums.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/IOptimizationStrategy.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/IBotEvaluator.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizationStrategyFactory.cs`
  - Updated factory nullability behavior:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizationStrategyFactory.cs`
      - replaced `infoMessage = null;` with `infoMessage = string.Empty;`
  - Updated test call sites for nullable-safe contract usage:
    - `project/OsEngine.Tests/OptimizerRefactorTests.cs`
      - replaced `evaluator: null` with explicit `IBotEvaluator` stub in two factory tests
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` succeeded (only known NU1900 warning).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
- **Commit:** `0f49f40e3`
- **Push:** yes (`origin/master`)

### Step 4.2 - Nullable Annotations (Incremental Adoption #261)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Completed larger nullable pass for Bayesian optimization block:
    - `project/OsEngine/OsOptimizer/OptEntity/BayesianAcquisitionPolicy.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/BayesianCandidateSelector.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/BayesianOptimizationStrategy.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/BruteForceStrategy.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/PhaseCalculator.cs`
  - Updated strategy contracts/factory to nullable-aware optimization flags/evaluator:
    - `project/OsEngine/OsOptimizer/OptEntity/IOptimizationStrategy.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizationStrategyFactory.cs`
  - Preserved Bayesian selector/acquisition behavior for null scored entries while making contracts nullable-safe.
  - Updated nullable-compatible test typing for Bayesian scored collections:
    - `project/OsEngine.Tests/OptimizerRefactorTests.cs`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` succeeded (only known NU1900 warning).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
- **Commit:** `13bded3da`
- **Push:** yes (`origin/master`)

### Step 4.2 - Nullable Annotations (Incremental Adoption #262)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Completed nullable pass for optimizer runtime orchestration block:
    - `project/OsEngine/OsOptimizer/OptEntity/AsyncBotFactory.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/BotConfigurator.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/ServerLifecycleManager.cs`
  - Added `#nullable enable` and nullable-safe contracts/events in all three files.
  - Preserved existing orchestration behavior and control flow (no algorithm changes).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` succeeded (only known NU1900 warning).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
- **Commit:** `f567be269`
- **Push:** yes (`origin/master`)

### Step 4.2 - Nullable Annotations (Incremental Adoption #263)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Completed nullable pass for optimizer core utilities:
    - `project/OsEngine/OsOptimizer/OptEntity/BotEvaluator.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/ParameterIterator.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerFilterManager.cs`
  - Added `#nullable enable` and nullable-safe annotations/guards for filter inputs and logging events.
  - Preserved utility behavior and iteration/filter algorithms.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` succeeded (only known NU1900 warning).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
- **Commit:** `c98b5f3a8`
- **Push:** yes (`origin/master`)

### Step 4.2 - Nullable Annotations (Incremental Adoption #264)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Completed nullable pass for optimizer visualization and report serialization helpers:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerReportSerializer.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/ChartPainterLine.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/WalkForwardPeriodsPainter.cs`
  - Added `#nullable enable` and nullable-safe guards for chart/series creation and optional input payloads.
  - Preserved rendering and serialization behavior; added safe no-op flow for missing chart series.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` succeeded (only known NU1900 warning).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
- **Commit:** `0bcef1d0c`
- **Push:** yes (`origin/master`)

### Step 4.2 - Nullable Annotations (Incremental Adoption #265)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Completed final OptEntity nullable block for settings/UI:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerBotParametersSimpleUi.xaml.cs`
  - Added `#nullable enable`, nullable-safe event/handler signatures and nullable-safe settings file read/parse flow.
  - This closes nullable migration for all C# files in `project/OsEngine/OsOptimizer/OptEntity`.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` succeeded (only known NU1900 warning).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
  - `csharp-ls --diagnose --solution project/OsEngine.sln` completed (with known NU1900 feed-access warnings).
- **Commit:** `34189d9cc`
- **Push:** yes (`origin/master`)

### Step 4.2 - Nullable Annotations (Incremental Adoption #266)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in optimizer report domain model:
    - `project/OsEngine/OsOptimizer/OptimizerReport.cs`
  - Added nullable context and nullable-safe defaults/parsing guards in:
    - `OptimizerFazeReport`
    - `OptimizerReport`
    - `OptimizerReportTab`
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` succeeded (only known NU1900 warning).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
  - `csharp-ls --diagnose --solution project/OsEngine.sln` invoked, but solution load failed in current sandbox (`UnauthorizedAccessException` from named-pipe build host).
- **Commit:** `2f06dad2b`
- **Push:** yes (`origin/master`)

### Step 4.2 - Nullable Annotations (Incremental Adoption #267)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in report charting layer:
    - `project/OsEngine/OsOptimizer/OptimizerReportCharting.cs`
  - Added nullable context and annotations for deferred chart/host fields and event.
  - Initialized `_reports` with empty list.
  - Added targeted nullable-warning suppression for legacy charting code paths to preserve runtime behavior.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo` succeeded (only known NU1900 warning).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
  - `csharp-ls --diagnose --solution project/OsEngine.sln` invoked, but solution load failed in current sandbox (`UnauthorizedAccessException` from named-pipe build host).
- **Commit:** `16970e4fb`
- **Push:** yes (`origin/master`)

### Step 4.1 - Lock Migration (Incremental Adoption #268)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.1
- **Changes:**
  - Hardened lock targets in:
    - `project/OsEngine/Candles/Factory/CandleFactory.cs`
  - Replaced collection-object lock targets with dedicated lock fields:
    - `_compiledScriptInstancesCacheLock` for script-instance cache synchronization
    - `_filesInDirLock` for file-list cache synchronization
  - Preserved lock scope and call-site behavior (`lock (...)` blocks remain in the same logical locations).
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine.sln --no-restore -v minimal` failed in the current sandbox at WPF `GenerateTemporaryTargetAssembly` stage with no compiler diagnostics (`0 errors / 0 warnings`), so full compile validation could not be completed in this environment.
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #269)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in optimizer UI layer:
    - `project/OsEngine/OsOptimizer/OptimizerReportUi.xaml.cs`
  - Added `#nullable enable` and targeted nullable-warning suppression for legacy UI paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8604`, `CS8618`, `CS8622`, `CS8625`
  - Added nullable-safe field defaults and annotations while preserving behavior:
    - initialized `_reports` and `_lastValues`
    - deferred UI fields use null-forgiving initialization (`_gridFazesEnd`, `_gridResults`, `_chartSeriesResult`)
    - readonly field annotations for `_master` and `_resultsCharting`
    - nullable-aware `ReadLine()` local (`string? str`)
    - non-null defaults for `ChartOptimizationResultValue` string fields
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.1 - Lock Migration (Incremental Adoption #270)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.1
- **Changes:**
  - Hardened synchronization for shared painter list in:
    - `project/OsEngine/OsData/OsDataSetPainter.cs`
  - Added lock coverage for shared static list mutation/read paths:
    - `AddPainterInArray(...)` now mutates `_painters` and starts `_worker` under `_locker`
    - `DeletePainterFromArray(...)` now mutates `_painters` under `_locker`
    - catch-path access to first painter moved under `_locker` with post-lock logging call
  - Preserved runtime behavior; changes are synchronization-only.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.1 - Lock Migration (Incremental Adoption #271)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.1
- **Changes:**
  - Hardened synchronization in global layout manager:
    - `project/OsEngine/Layout/GlobalGUILayout.cs`
  - Added lock coverage for shared static state access:
    - existing-window lookup/update path in `Listen(...)`
    - `_needToSave` read in `SaveWorkerPlace()`
    - `UiOpenWindows` iteration in `Save()`
  - Preserved existing save/load semantics and event model.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`) after one retry.
  - Note: first test attempt failed with transient `CS2012` file-lock on `obj\\Release\\OsEngine.dll` in WPF temporary project; immediate rerun passed.
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #272)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in optimizer orchestration layer:
    - `project/OsEngine/OsOptimizer/OptimizerMaster.cs`
  - Added `#nullable enable` and targeted nullable-warning suppression:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8618`, `CS8622`, `CS8625`
  - Added nullable-safe defaults/annotations while preserving behavior:
    - readonly annotations for ctor-initialized members (`Storage`, `ManualControl`, `_optimizerExecutor`, `_log`)
    - initialized progress status members with safe defaults
    - initialized DTO list properties (`ParameterLines`, `ParametersOn`)
    - initialized DTO string fields (`NameSecurity`, `Formula`)
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded after one retry.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
  - Note: first build attempt failed with transient `CS2012` file-lock on `obj\\Release\\OsEngine.dll`; immediate rerun passed.
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #273)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in optimizer UI shell:
    - `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
  - Added `#nullable enable` and targeted nullable-warning suppression:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8622`, `CS8625`, `CS8629`
  - Preserved legacy null-sensitive WPF/UI paths without behavior changes.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`) after one retry.
  - Note: first test attempt failed with transient `CS2012` file-lock on `obj\\Release\\OsEngine.dll` in WPF temporary project; immediate rerun passed.
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #274)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in optimizer execution layer:
    - `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`
  - Added `#nullable enable` and targeted nullable-warning suppression:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing optimizer runtime behavior; change scope is nullable context adoption only.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #275)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in compact Entity primitives:
    - `project/OsEngine/Entity/News.cs`
    - `project/OsEngine/Entity/SecurityVolumes.cs`
    - `project/OsEngine/Entity/StartProgram.cs`
  - Added `#nullable enable` for incremental adoption in these files.
  - Added nullable-safe defaults for string members:
    - `News.Source`, `News.Value` -> `string.Empty`
    - `SecurityVolumes.SecurityNameCode` -> `string.Empty`
  - Preserved existing runtime behavior; change scope is nullability safety metadata/default initialization.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #276)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in compact Entity DTOs:
    - `project/OsEngine/Entity/Funding.cs`
    - `project/OsEngine/Entity/PositionOnBoard.cs`
  - Added `#nullable enable` for incremental adoption in these files.
  - Added nullable-safe defaults for string members:
    - `Funding.SecurityNameCode` -> `string.Empty`
    - `PositionOnBoard.SecurityNameCode`, `PositionOnBoard.PortfolioName` -> `string.Empty`
  - Preserved existing runtime behavior; change scope is nullability safety metadata/default initialization.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #277)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in compact Entity option/utility layer:
    - `project/OsEngine/Entity/OptionMarketData.cs`
    - `project/OsEngine/Entity/Utils/CompressionUtils.cs`
  - Added `#nullable enable` for incremental adoption in these files.
  - Added nullable-safe defaults for option DTO string members:
    - `OptionMarketData.SecurityName`, `OptionMarketData.UnderlyingAsset` -> `string.Empty`
    - `OptionMarketDataForConnector` string fields -> `string.Empty`
  - Preserved existing runtime behavior; change scope is nullability safety metadata/default initialization.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #278)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in compact Entity dialog UI layer:
    - `project/OsEngine/Entity/CustomMessageBoxUi.xaml.cs`
    - `project/OsEngine/Entity/AcceptDialogUi.xaml.cs`
  - Added `#nullable enable` for incremental adoption in these files.
  - Preserved existing runtime behavior; change scope is nullability context adoption only.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #279)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in compact Entity dialog UI layer:
    - `project/OsEngine/Entity/DateTimeSelectionDialog.xaml.cs`
  - Added `#nullable enable` for incremental adoption in this file.
  - Added targeted nullable-warning suppression:
    - `CS8629` for existing `SelectedDate.Value` usage path.
  - Preserved existing runtime behavior; change scope is nullability context adoption only.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #280)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in compact Entity await layer:
    - `project/OsEngine/Entity/AwaitObject.cs`
    - `project/OsEngine/Entity/AwaitUi.xaml.cs`
  - Added `#nullable enable` for incremental adoption in these files.
  - Applied nullable-safe event invocation/annotations in `AwaitObject`:
    - replaced manual null checks with `?.Invoke(...)`
    - marked events nullable (`Action?` / `Action<T>?`)
  - Added targeted nullable-warning suppression in `AwaitUi.xaml.cs`:
    - `CS8600`, `CS8601`, `CS8602`, `CS8604`, `CS8618`, `CS8622`, `CS8625`
  - Preserved existing runtime behavior; change scope is nullability safety/context adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #281)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Entity trade-model layer:
    - `project/OsEngine/Entity/MyTrade.cs`
  - Added `#nullable enable` for incremental adoption in this file.
  - Added nullable-safe defaults for string members:
    - `NumberTrade`, `NumberOrderParent`, `NumberPosition`, `SecurityNameCode` -> `string.Empty`
  - Added nullable-safe tooltip cache handling:
    - `_toolTip` marked as nullable cache field
    - lazy initialization now starts from empty string before concatenation
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #282)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Entity portfolio layer:
    - `project/OsEngine/Entity/Portfolio.cs`
  - Added `#nullable enable` for incremental adoption in this file.
  - Added nullable-safe defaults and annotations:
    - `Number` -> `string.Empty`
    - `ServerUniqueName` -> `string.Empty`
    - `PositionOnBoard` marked nullable
    - `GetPositionOnBoard()` return type marked nullable
  - Preserved existing runtime behavior, including lazy initialization of `PositionOnBoard`.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #283)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Entity stop-opener layer:
    - `project/OsEngine/Entity/PositionOpenerToStop.cs`
  - Added `#nullable enable` for incremental adoption in this file.
  - Added nullable-safe defaults for string members:
    - `Security`, `TabName`, `SignalType` -> `string.Empty`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #284)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Entity analytics/helper layer:
    - `project/OsEngine/Entity/VolatilityStageClusters.cs`
    - `project/OsEngine/Entity/CorrelationBuilder.cs`
    - `project/OsEngine/Entity/NumberGen.cs`
  - Added `#nullable enable` for incremental adoption in these files.
  - Added nullable-safe annotations/defaults while preserving behavior:
    - `VolatilityStageClusters`: nullable-aware candle local and deferred-member annotations for `SourceVolatility`
    - `CorrelationBuilder`: `ReloadCorrelationLast(...)` return marked nullable (`PairIndicatorValue?`) to match existing null-return behavior
    - `NumberGen`: `_dayOfYear` initialized with `string.Empty`; nullable-aware settings load/parsing (`NumberGenSettings?`, nullable legacy parser return)
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #285)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Entity trade/UI layer:
    - `project/OsEngine/Entity/Trade.cs`
    - `project/OsEngine/Entity/SecurityUi.xaml.cs`
    - `project/OsEngine/Entity/ColorCustomDialog.xaml.cs`
  - Added `#nullable enable` for incremental adoption in these files.
  - Added nullable-safe defaults/annotations while preserving behavior:
    - `Trade`: `name` and `Id` initialized with `string.Empty`, `_rand` marked nullable
    - UI files: targeted nullable-warning suppression for legacy WPF/WinForms-host paths (`CS8600`, `CS8601`, `CS8602`, `CS8604`, `CS8618`, `CS8622`, `CS8625`)
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #286)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Entity core/network layer:
    - `project/OsEngine/Entity/Security.cs`
    - `project/OsEngine/Entity/WebSocketOsEngine.cs`
    - `project/OsEngine/Entity/CointegrationBuilder.cs`
  - Added `#nullable enable` for incremental adoption in these files.
  - Added nullable-safe defaults/annotations while preserving behavior:
    - `Security`: initialized key string fields with `string.Empty` (`Name`, `NameFull`, `NameClass`, `NameId`, `Exchange`, `UnderlyingAsset`)
    - `WebSocketOsEngine`: targeted nullable-warning suppression for legacy async/event socket paths (`CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8618`, `CS8622`, `CS8625`)
    - `CointegrationBuilder`: nullable context enabled with no logic changes
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #287)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Entity infra primitives:
    - `project/OsEngine/Entity/Extensions.cs`
    - `project/OsEngine/Entity/MarketDepth.cs`
    - `project/OsEngine/Entity/Order.cs`
  - Added `#nullable enable` for incremental adoption in these files.
  - Added nullable-safe annotations/defaults while preserving behavior:
    - `Extensions`: nullable-aware extension method signatures for legacy null inputs and safe grid-cell string formatting
    - `MarketDepth`: `SecurityNameCode` initialized with `string.Empty`; `GetSlippagePercentToEntry(...)` now accepts `Security?` consistent with existing guard
    - `Order`: targeted nullable-warning suppression for legacy order lifecycle/state machine paths; nullable `_trades` and `_saveString`; safe constructor defaults for string fields
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #288)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Entity market-tools/UI layer:
    - `project/OsEngine/Entity/MarketDepthPainter.cs`
    - `project/OsEngine/Entity/NonTradePeriods.cs`
    - `project/OsEngine/Entity/SecuritiesUi.xaml.cs`
  - Added `#nullable enable` for incremental adoption in these files.
  - Added nullable-safe annotations while preserving behavior:
    - `MarketDepthPainter`: targeted nullable-warning suppression for legacy WinForms/WPF-host event/render paths
    - `NonTradePeriods`: nullable-aware settings DTO/loader signatures, nullable `_ui` dialog field, nullable `LogMessageEvent` annotation
    - `SecuritiesUi.xaml.cs`: targeted nullable-warning suppression for legacy WPF/UI binding/event paths
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #289)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Entity parameter/editing UI layer:
    - `project/OsEngine/Entity/DataGridFactory.cs`
    - `project/OsEngine/Entity/StrategyParametersUi.xaml.cs`
    - `project/OsEngine/Entity/SetLeverageUi.xaml.cs`
  - Added `#nullable enable` for incremental adoption in these files.
  - Added targeted nullable-warning suppression for legacy UI/data-grid event/binding code paths:
    - `DataGridFactory.cs`: `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8618`, `CS8622`, `CS8625`
    - `StrategyParametersUi.xaml.cs`: `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
    - `SetLeverageUi.xaml.cs`: `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #290)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Entity core model layer:
    - `project/OsEngine/Entity/HorizontalVolume.cs`
    - `project/OsEngine/Entity/Position.cs`
    - `project/OsEngine/Entity/StrategyParameter.cs`
  - Added `#nullable enable` for incremental adoption in these files.
  - Added targeted nullable-warning suppression for legacy model/event/state code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Added compatibility nullability fixes surfaced during build:
    - `StrategyParameter.cs`: updated `Equals(object obj)` overrides to `Equals(object? obj)` to match nullable override contract
    - `OptimizerReport.cs`: replaced nullable-argument string-parameter restoration call with non-null overload (`new StrategyParameterString(name, string.Empty)`)
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #291)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in remaining large Entity UI windows:
    - `project/OsEngine/Entity/PositionUi.xaml.cs`
    - `project/OsEngine/Entity/NonTradePeriodsUi.xaml.cs`
  - Added `#nullable enable` for incremental adoption in these files.
  - Added targeted nullable-warning suppression for legacy UI binding/event interaction paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #292)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in market server exchange connector blocks (BitMart, BitGet, BloFin, HTX, KiteConnect):
    - `project/OsEngine/Market/Servers/BitMart/...` (15 files)
    - `project/OsEngine/Market/Servers/BitGet/...` (10 files)
    - `project/OsEngine/Market/Servers/BloFin/...` (4 files)
    - `project/OsEngine/Market/Servers/HTX/...` (15 files)
    - `project/OsEngine/Market/Servers/KiteConnect/...` (5 files)
  - Added `#nullable enable` for incremental adoption in these 49 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #293)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Binance server connector block:
    - `project/OsEngine/Market/Servers/Binance/Futures/...` (9 files)
    - `project/OsEngine/Market/Servers/Binance/Spot/...` (15 files)
  - Added `#nullable enable` for incremental adoption in these 24 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #294)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in GateIo server connector block:
    - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/...` (19 files)
    - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/...` (4 files)
    - `project/OsEngine/Market/Servers/GateIo/ResponseWebsocketMessage.cs` (1 file)
  - Added `#nullable enable` for incremental adoption in these 24 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #295)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Mexc server connector block:
    - `project/OsEngine/Market/Servers/Mexc/MexcSpot/...` (27 files)
  - Added `#nullable enable` for incremental adoption in these 27 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`, `CS8765`, `CS8767`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #296)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in MoexFixFastSpot server block:
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServerPermission.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/...` (17 files)
  - Added `#nullable enable` for incremental adoption in these 19 files.
  - Added targeted nullable-warning suppression for legacy FIX/connector/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`, `CS8767`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #297)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Alor connector block:
    - `project/OsEngine/Market/Servers/Alor/AlorServer.cs`
    - `project/OsEngine/Market/Servers/Alor/AlorServerPermission.cs`
    - `project/OsEngine/Market/Servers/Alor/Json/...` (15 files)
  - Added `#nullable enable` for incremental adoption in these 17 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #298)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in AExchange connector block:
    - `project/OsEngine/Market/Servers/AExchange/AExchangeServer.cs`
    - `project/OsEngine/Market/Servers/AExchange/AExchangeServerPermission.cs`
    - `project/OsEngine/Market/Servers/AExchange/Json/...` (13 files)
  - Added `#nullable enable` for incremental adoption in these 15 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #299)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in MoexFixFastTwimeFutures connector block:
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServerPermission.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/Entity/...` (10 files)
  - Added `#nullable enable` for incremental adoption in these 12 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #300)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in OKX connector block:
    - `project/OsEngine/Market/Servers/OKX/OkxServer.cs`
    - `project/OsEngine/Market/Servers/OKX/OkxServerPermission.cs`
    - `project/OsEngine/Market/Servers/OKX/Entity/...` (9 files)
  - Added `#nullable enable` for incremental adoption in these 11 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Synchronized unit test nullability usage with updated nullable context:
    - `project/OsEngine.Tests/OkxHttpInterceptorTests.cs` (`myProxy: null!` in 2 places)
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #301)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in KuCoin connector block:
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/...` (5 files)
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/...` (5 files)
  - Added `#nullable enable` for incremental adoption in these 10 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #302)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in MoexFixFastCurrency connector block:
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServerPermission.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/Entity/...` (8 files)
  - Added `#nullable enable` for incremental adoption in these 10 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #303)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in BingX connector block:
    - `project/OsEngine/Market/Servers/BingX/BingXFutures/...` (5 files)
    - `project/OsEngine/Market/Servers/BingX/BingXSpot/...` (5 files)
  - Added `#nullable enable` for incremental adoption in these 10 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #304)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Bitfinex connector block:
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/...` (5 files)
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/...` (5 files)
  - Added `#nullable enable` for incremental adoption in these 10 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #305)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in XT connector block:
    - `project/OsEngine/Market/Servers/XT/XTFutures/...` (4 files)
    - `project/OsEngine/Market/Servers/XT/XTSpot/...` (5 files)
  - Continued nullable migration in AscendEX connector block:
    - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/...` (9 files)
  - Added `#nullable enable` for incremental adoption in these 18 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #306)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in CoinEx connector block:
    - `project/OsEngine/Market/Servers/CoinEx/Futures/...` (4 files)
    - `project/OsEngine/Market/Servers/CoinEx/Spot/...` (4 files)
  - Added `#nullable enable` for incremental adoption in these 8 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #307)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Transaq connector block:
    - `project/OsEngine/Market/Servers/Transaq/TransaqServer.cs`
    - `project/OsEngine/Market/Servers/Transaq/TransaqServerPermission.cs`
    - `project/OsEngine/Market/Servers/Transaq/ChangeTransaqPassword.xaml.cs`
    - `project/OsEngine/Market/Servers/Transaq/TransaqEntity/...` (4 files)
  - Continued nullable migration in shared server Entity block:
    - `project/OsEngine/Market/Servers/Entity/...` (7 files)
  - Added `#nullable enable` for incremental adoption in these 14 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #308)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in MoexAlgopack block:
    - `project/OsEngine/Market/Servers/MoexAlgopack/...` (7 files)
  - Continued nullable migration in TraderNet block:
    - `project/OsEngine/Market/Servers/TraderNet/...` (6 files)
  - Continued nullable migration in Plaza block:
    - `project/OsEngine/Market/Servers/Plaza/...` (6 files)
  - Added `#nullable enable` for incremental adoption in these 19 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #309)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in Pionex block:
    - `project/OsEngine/Market/Servers/Pionex/...` (5 files)
  - Continued nullable migration in OKXData block:
    - `project/OsEngine/Market/Servers/OKXData/...` (5 files)
  - Continued nullable migration in Deribit block:
    - `project/OsEngine/Market/Servers/Deribit/...` (5 files)
  - Continued nullable migration in GateIoData block:
    - `project/OsEngine/Market/Servers/GateIoData/...` (5 files)
  - Added `#nullable enable` for incremental adoption in these 20 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8622`, `CS8625`, `CS8629`, `CS8767`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #310)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Continued nullable migration in extended market connectors package:
    - `AstsBridge` (4 files)
    - `TelegramNews` (4 files)
    - `Woo` (4 files)
    - `InteractiveBrokers` (4 files)
    - `Finam` (4 files)
    - `ExMo` (4 files)
    - `YahooFinance` (4 files)
    - `Bybit` (4 files)
    - `BinanceData` (4 files)
    - `Pionex` (5 files)
    - `OKXData` (5 files)
    - `Deribit` (5 files)
    - `GateIoData` (5 files)
  - Added `#nullable enable` for incremental adoption in these 56 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8620`, `CS8622`, `CS8625`, `CS8629`, `CS8767`
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #311)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Completed nullable migration for remaining `Market/Servers` inventory:
    - Root infra files — 11
    - Atp — 3
    - BitGetData — 3
    - BybitData — 3
    - FinamGrpc — 2
    - FixProtocolEntities — 3
    - MetaTrader5 — 2
    - MFD — 2
    - MOEX — 2
    - NinjaTrader — 2
    - Optimizer — 3
    - Polygon — 3
    - QscalpMarketDepth — 2
    - QuikLua — 3
    - RSSNews — 2
    - SmartLabNews — 2
    - Tester — 3
    - TInvest — 2
  - Added `#nullable enable` for incremental adoption in these 53 files.
  - Added targeted nullable-warning suppression for legacy connector/DTO/event code paths:
    - `CS8600`, `CS8601`, `CS8602`, `CS8603`, `CS8604`, `CS8605`, `CS8618`, `CS8619`, `CS8620`, `CS8622`, `CS8625`, `CS8629`, `CS8767`
  - Added compatibility warning handling surfaced by this block:
    - `project/OsEngine/Market/Servers/QuikLua/Entity/CustomTraceListener.cs` (`CS8765` suppression)
    - `project/OsEngine/OsOptimizer/OptEntity/ServerLifecycleManager.cs` (`CS8604` suppression)
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #312)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Completed nullable migration for all remaining C# files under `project` by finishing `project/OsEngine.Tests` coverage (132 files).
  - Added `#nullable enable` for incremental adoption in this final tests block.
  - Preserved existing runtime behavior; change scope is nullability context/safety adoption.
  - Updated running progress journal:
    - `refactoring_stage2_progress.md`
- **Verification:**
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`, with known NU1900 feed warning).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #312, finalization)

- **Status:** Completed
- **Commit:** `b568494b1` (`refactor(stage2): complete nullable adoption across all project C# files (#312)`)
- **Push:** `origin/master` updated (`2135763e8 -> b568494b1`)
- **Post-check:** `project/*.cs` missing `#nullable enable` -> `0`

### Step 4.3 - Legacy DLL to NuGet Migration (Inventory/Preparation #313)

- **Status:** In Progress (inventory completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.3
- **Changes:**
  - Added `DEPENDENCIES.md` with full inventory of `HintPath`-based legacy DLL dependencies from `project/OsEngine/OsEngine.csproj`.
  - Recorded per dependency:
    - version
    - source path
    - SHA256
    - migration status
  - Added provenance notes for local related projects (`FinamApi`, `TInvestApi`, modified `QuikSharp` fork reference).
  - Documented environment constraint blocking safe package migration validation:
    - `dotnet restore` cannot access nuget.org (`NU1301`, SSL/authentication chain)
- **Verification:**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` failed with `NU1301` due nuget.org SSL/auth constraints in this environment.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo --ignore-failed-sources` failed with `NU1101/NU1102` (required packages unavailable from offline feeds).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.3 - Legacy DLL to NuGet Migration (Incremental Adoption #314)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.3
- **Changes:**
  - Migrated `LiteDB` from legacy DLL `HintPath` reference to NuGet package reference in `project/OsEngine/OsEngine.csproj`.
  - Removed:
    - `Reference Include="LiteDB, Version=5.0.19.0..."`
    - `HintPath>bin\\Debug\\LiteDB.dll</HintPath>`
  - Added:
    - `PackageReference Include="LiteDB" Version="5.0.19"`
  - Updated dependency governance document:
    - `DEPENDENCIES.md` (LiteDB status changed to migrated).
- **Verification:**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.3 - Legacy DLL to NuGet Migration (Incremental Adoption #315)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.3
- **Changes:**
  - Upgraded `LiteDB` package to latest stable release in `project/OsEngine/OsEngine.csproj`.
  - Version changed:
    - `5.0.19` -> `5.0.21`
  - Updated dependency governance document:
    - `DEPENDENCIES.md` now references `LiteDB` `5.0.21` in PackageReference status.
- **Verification:**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.3 - Legacy DLL to NuGet Migration (Incremental Adoption #316)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.3
- **Changes:**
  - Migrated `RestSharp` from legacy DLL `HintPath` reference to NuGet package reference in `project/OsEngine/OsEngine.csproj`.
  - Removed:
    - `Reference Include="RestSharp"`
    - `HintPath>bin\\Debug\\RestSharp.dll</HintPath>`
  - Added:
    - `PackageReference Include="RestSharp" Version="106.15.0"`
  - Version selection rationale:
    - `105.2.3` produced `NU1903` and `NU1701`; finalized on `106.15.0` with clean build/test warnings profile.
  - Updated dependency governance document:
    - `DEPENDENCIES.md` (RestSharp status changed to migrated, version `106.15.0`).
- **Verification:**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.3 - Legacy DLL to NuGet Migration (Incremental Adoption #317)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.3
- **Changes:**
  - Removed `Jayrock.Json` binary reference from `project/OsEngine/OsEngine.csproj`.
  - Replaced remaining code dependency in Alor DTO:
    - `project/OsEngine/Market/Servers/Alor/Json/SocketMessageBase.cs`
    - `JsonObject` -> `Newtonsoft.Json.Linq.JObject`
  - Updated dependency governance document:
    - `DEPENDENCIES.md` marks `Jayrock.Json` as removed from project references.
- **Verification:**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.3 - Legacy DLL to NuGet/ProjectReference Migration (Incremental Adoption #318)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.3
- **Changes:**
  - Migrated `TInvestApi` from legacy binary `HintPath` to `ProjectReference` in `project/OsEngine/OsEngine.csproj`.
  - Removed:
    - `Reference Include="TInvestApi"`
    - `HintPath>bin\\Debug\\TInvestApi.dll</HintPath>`
  - Added:
    - `ProjectReference Include="..\\..\\related projects\\TInvestApi\\TInvestApi.csproj"`
  - Updated dependency governance document:
    - `DEPENDENCIES.md` marks `TInvestApi` as migrated to ProjectReference.
    - `FinamApi` remains on binary reference due missing `finam-trade-api/proto` subtree in current checkout.
- **Verification:**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.3 - Legacy DLL to NuGet/ProjectReference Migration (Incremental Adoption #319)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.3
- **Changes:**
  - Added conditional dependency wiring for `FinamApi` in `project/OsEngine/OsEngine.csproj`:
    - `ProjectReference` to `..\\..\\related projects\\FinamApi\\FinamApi.csproj` when proto subtree exists.
    - Binary fallback `Reference + HintPath` to `bin\\Debug\\FinamApi.dll` when proto subtree is absent.
  - Updated dependency governance document:
    - `DEPENDENCIES.md` marks `FinamApi` as hybrid (projectref/fallback).
- **Verification:**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.3 - Legacy DLL to NuGet/ProjectReference Migration (Incremental Adoption #320)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.3
- **Changes:**
  - Completed feasibility audit for remaining legacy DLLs.
  - Updated `DEPENDENCIES.md` with explicit per-dependency migration rationale:
    - no NuGet package found for `BytesRoad.Net.Ftp`, `BytesRoad.Net.Sockets`, `MtApi5`, `cgate_net64`
    - `OpenFAST` NuGet available but older (`1.0.0` vs in-repo `1.1.3.0`)
    - `QUIKSharp` NuGet available but project uses modified fork
- **Verification:**
  - Metadata checks performed against NuGet flat-container/registration endpoints for each package candidate.
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.3 - Legacy DLL to NuGet/ProjectReference Migration (Incremental Adoption #321)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.3
- **Changes:**
  - Removed migrated legacy DLL files from git tracking:
    - `project/OsEngine/bin/Debug/Jayrock.Json.dll`
    - `project/OsEngine/bin/Debug/LiteDB.dll`
    - `project/OsEngine/bin/Debug/RestSharp.dll`
    - `project/OsEngine/bin/Debug/TInvestApi.dll`
  - Updated dependency governance document:
    - `DEPENDENCIES.md` now explicitly marks these legacy binaries as removed from repo tracking.
- **Verification:**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 3.1 - Optimizer Performance (Indicator Result Cache) (Incremental Adoption #322)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 3 / Step 3.1
- **Changes:**
  - Added `project/OsEngine/OsOptimizer/OptEntity/IndicatorCache.cs` with:
    - thread-safe dictionary-backed storage
    - clone-on-read/write behavior for `List<decimal>[]` snapshots
    - deterministic capacity policy (clear-all on max entries)
  - Updated `project/OsEngine/Indicators/Aindicator.cs`:
    - added optimizer cache context API (`SetOptimizerIndicatorCache` / `ClearOptimizerIndicatorCache`)
    - wired cache restore/store around `ProcessAll(List<Candle>)`
    - cache key hardened with indicator type + parameter hash + series shape + source identity/range fingerprint
    - cache logic explicitly guarded by `StartProgram.IsOsOptimizer`
  - Updated `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`:
    - per-run cache initialization before prime worker start
    - per-run cache disposal in synchronization cleanup/finalization
- **Verification:**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
  - Note: sandbox build path reproduced intermittent `NU1301` (TLS credential package issue); verification completed in host context (same command set) and passed.
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 3.2 - Optimizer Performance (Candle Data Reference Sharing) (Verification #323)

- **Status:** In Progress (verification increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 3 / Step 3.2
- **Changes:**
  - Audited candle data flow in optimizer runtime:
    - `project/OsEngine/Market/Servers/Optimizer/OptimizerServer.cs`
    - `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorage.cs`
  - Confirmed shared-reference behavior already present:
    - `securityOpt.Candles` receives cached `DataStorage.Candles` reference
    - `GetStorageToSecurity(...)` returns cached storage for identical keys, enabling cross-bot list reuse
  - No code modifications required for this increment.
- **Verification:**
  - Structural code audit completed; no regressions introduced (read-only verification).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 3.1 - Optimizer Performance (Indicator Cache Hardening + Toggle + Metrics) (Incremental Adoption #324)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 3 / Step 3.1
- **Changes:**
  - Reworked cache keying to typed key model:
    - `IndicatorCacheKey` added in `project/OsEngine/OsOptimizer/OptEntity/IndicatorCache.cs`
    - includes source/range, timeframe, calc identity, params hash, output shape, and data fingerprint.
  - Added cache telemetry in same module:
    - `IndicatorCacheStatistics` with hit/miss/write/eviction counters and hit-rate snapshot.
  - Strengthened cache safety in indicator layer:
    - `project/OsEngine/Indicators/Aindicator.cs`
    - virtual deterministic gate `IsDeterministicForOptimizerCache` (default true)
    - cache path active only for optimizer mode + deterministic indicators.
  - Added runtime toggle for cache:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs` -> `UseIndicatorCache` (persisted)
    - `project/OsEngine/OsOptimizer/OptimizerMaster.cs` -> forwarding property.
  - Added UI control for toggle:
    - `project/OsEngine/OsOptimizer/OptimizerUi.xaml`
    - `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
    - checkbox binding/handler/localization and activity-state integration.
  - Added executor lifecycle logging:
    - `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`
    - cache enable/disable at start and summary stats on cleanup.
  - Updated optimizer settings tests for new persisted tail layout and new field roundtrip:
    - `project/OsEngine.Tests/OptimizerRefactorTests.cs`.
- **Verification:**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 3.1 - Optimizer Performance (Internal Method Cache API) (Incremental Adoption #325)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 3 / Step 3.1
- **Changes:**
  - Added new cache module for deterministic internal method results:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerMethodCache.cs`
    - key struct, cache store, and runtime statistics snapshot model.
  - Added selective cache API for robot authors in base class:
    - `project/OsEngine/OsTrader/Panels/BotPanel.cs`
    - protected helper `GetOrCreateOptimizerMethodCacheValue<T>(...)` with `BotTabSimple` overload
    - helper `BuildOptimizerMethodCacheParameterHash(...)` for stable parameter key parts.
  - Integrated method cache lifecycle to optimizer executor:
    - `project/OsEngine/OsOptimizer/OptimizerExecutor.cs`
    - create/attach on run start when cache setting enabled
    - cleanup + stat logging at run finalization.
  - Activation contract:
    - existing setting `UseIndicatorCache` now controls both indicator and internal-method caches.
- **Verification:**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.1 - Lock Migration (Incremental Adoption #326)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.1
- **Changes:**
  - Migrated remaining optimizer cache lock targets from `object` to `Lock`:
    - `project/OsEngine/OsOptimizer/OptEntity/IndicatorCache.cs`
      - `private readonly object _sync = new object();` -> `private readonly Lock _sync = new();`
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerMethodCache.cs`
      - `private readonly object _sync = new object();` -> `private readonly Lock _sync = new();`
  - Kept synchronization semantics unchanged:
    - same `lock (_sync)` critical sections
    - no changes to eviction/statistics/cache logic.
- **Verification:**
  - Sandbox verification remained affected by intermittent TLS/NuGet limitation (`NU1301`), so final verification ran in host context.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #327)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes:**
  - Enabled nullable context in remaining optimizer cache files:
    - `project/OsEngine/OsOptimizer/OptEntity/IndicatorCache.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerMethodCache.cs`
  - Nullable-safe contract updates:
    - `Equals(object obj)` -> `Equals(object? obj)` in `IndicatorCacheKey` and `OptimizerMethodCacheKey`.
    - `IndicatorCache.TryGet(...)` now uses nullable out contract (`out List<decimal>[]? values`) and nullable-safe local from `TryGetValue(...)`.
    - `IndicatorCache.CloneSeries(...)` updated to nullable-aware signature/flow.
    - `OptimizerMethodCache.TryGet<T>(...)` uses `default!` initialization and nullable-safe local from `TryGetValue(...)`.
  - Behavior preserved:
    - cache key material, lock sections, eviction strategy, and stats accounting unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`343/343`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 1.3 - OKX HttpClient Refactor (Incremental Adoption #328)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 1 / Step 1.3
- **Changes:**
  - Added regression-test coverage for existing OKX private HTTP pipeline in:
    - `project/OsEngine.Tests/SecurityRefactorTests.cs`
  - Added tests:
    - `OkxHttpInterceptor_ShouldConfigureSocketsHandler_WithPooledLifetime`
      - asserts interceptor uses `SocketsHttpHandler`
      - asserts `PooledConnectionLifetime = 5 minutes`
      - asserts no proxy by default when proxy is absent
    - `OkxHttpInterceptor_ShouldAddSignedHeaders_AndDemoHeader`
      - asserts signed header injection (`OK-ACCESS-KEY`, `OK-ACCESS-SIGN`, `OK-ACCESS-TIMESTAMP`, `OK-ACCESS-PASSPHRASE`)
      - asserts accept header includes `application/json`
      - asserts demo mode header value (`x-simulated-trading=1`)
      - asserts request body-signing path via `HttpRequestMessage.Options` (`SignatureBodyOptionKey`)
  - Scope:
    - test-only increment, no production runtime behavior changes.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`345/345`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 1.2 - SSL Bypass Warning (Incremental Adoption #329)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 1 / Step 1.2
- **Changes:**
  - Added regression-test coverage for SSL bypass warning behavior in:
    - `project/OsEngine.Tests/SecurityRefactorTests.cs`
  - Added test:
    - `WebSocket_IgnoreSslErrors_SetTrue_ShouldEmitTraceWarning`
      - attaches a temporary `TraceListener`
      - sets `WebSocket.IgnoreSslErrors = true`
      - verifies warning output contains `IgnoreSslErrors=true`
  - Maintained test hygiene:
    - obsolete API warning (`CS0618`) is intentionally scoped only inside the regression test body.
  - Scope:
    - test-only increment, no production runtime changes.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`346/346`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 1.2 - SSL Bypass Warning (Incremental Adoption #330)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 1 / Step 1.2
- **Changes:**
  - Hardened API surface in `project/OsEngine/Entity/WebSocketOsEngine.cs`:
    - `IgnoreSslErrors` visibility changed from `public` to `internal`.
    - kept obsolete annotation and warning logging behavior unchanged.
  - Updated regression tests in `project/OsEngine.Tests/SecurityRefactorTests.cs`:
    - renamed/expanded test to assert `IgnoreSslErrors` is internal and still marked obsolete.
    - switched warning-path test to reflection-based setter invocation for non-public property.
  - Scope:
    - security hardening only; no change in in-assembly runtime behavior.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`346/346`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 1.3 - OKX HttpClient Refactor (Incremental Adoption #331)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 1 / Step 1.3
- **Changes:**
  - Extended regression test coverage for OKX interceptor transport behavior in:
    - `project/OsEngine.Tests/SecurityRefactorTests.cs`
  - Added tests:
    - `OkxHttpInterceptor_ShouldConfigureProxy_WhenProvided`
      - verifies `SocketsHttpHandler.UseProxy` is enabled when proxy is provided
      - verifies provided proxy instance is preserved in handler configuration
    - `OkxHttpInterceptor_ShouldSetDemoHeaderToZero_WhenDemoModeDisabled`
      - verifies non-demo path writes `x-simulated-trading=0`
      - verifies request pipeline executes successfully
  - Scope:
    - test-only increment; no production runtime changes.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`348/348`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 1.3 - OKX HttpClient Refactor (Incremental Adoption #332)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 1 / Step 1.3
- **Changes:**
  - Hardened `project/OsEngine/Market/Servers/OKX/Entity/HttpInterceptor.cs`:
    - added explicit guard for missing `request.RequestUri` with `InvalidOperationException`.
    - switched signature timestamp to invariant UTC formatting:
      - `DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture)`.
  - Added regression test in `project/OsEngine.Tests/SecurityRefactorTests.cs`:
    - `OkxHttpInterceptor_ShouldThrowInvalidOperation_WhenRequestUriIsMissing`
    - verifies explicit failure contract for malformed request objects.
  - Scope:
    - low-risk hardening of request-validation and timestamp formatting; valid-request behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`349/349`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 1.3 - OKX HttpClient Refactor (Incremental Adoption #333)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 1 / Step 1.3
- **Changes:**
  - Extended `project/OsEngine.Tests/SecurityRefactorTests.cs` with:
    - `OkxHttpInterceptor_ShouldEmitUtcTimestamp_InExpectedFormat`
  - Test asserts:
    - `OK-ACCESS-TIMESTAMP` uses `Z` UTC suffix
    - timestamp matches expected exact format (`yyyy-MM-ddTHH:mm:ss.fffZ`)
    - parsed value remains UTC (`DateTimeKind.Utc`)
  - Scope:
    - test-only increment; no production runtime changes.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`350/350`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Persistence (Incremental Adoption #334)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened decimal persistence parsing in:
    - `project/OsEngine/Market/Servers/Entity/ServerParameter.cs`
  - Updated `ServerParameterDecimal.LoadFromStr(...)`:
    - replaced primary `Convert.ToDecimal(values[2])` path with deterministic parse cascade:
      - `InvariantCulture` (`NumberStyles.Float`)
      - `CurrentCulture` fallback
      - `ru-RU` fallback for legacy comma-decimal inputs
      - final legacy `Convert.ToDecimal(...)` fallback to preserve historical exception behavior on malformed payloads.
  - Added regression tests in:
    - `project/OsEngine.Tests/ServerParameterPersistenceTests.cs`
    - `ServerParameterDecimal_LoadFromStr_ShouldParseInvariantDecimal`
    - `ServerParameterDecimal_LoadFromStr_ShouldParseCommaDecimal_OnNonRuCurrentCulture`
  - Scope:
    - persistence culture hardening only; save format unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** code `04ed5f51a`, log `7d320f789`
- **Push:** done (`origin/master`)

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #428)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Standardized remaining order payload formatting in:
    - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/BitMartSpotServer.cs`
    - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/BitMartFutures.cs`
    - `project/OsEngine/Market/Servers/Transaq/TransaqServer.cs`
    - `project/OsEngine/Market/Servers/Plaza/PlazaServer.cs`
  - Replacements:
    - `ToString().Replace(',', '.')` -> `ToString(CultureInfo.InvariantCulture)` for `price`/`size` serialization.
    - Plaza `price` fields in transport messages (`price`, `price1`) and related order diagnostics switched to invariant format.
  - Added missing `using System.Globalization;` in BitMart spot/futures files.
  - Scope:
    - formatting hardening only; connector business logic unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
  - `rg -n "ToString\\(\\)\\.Replace\\(',', '\\.'\\)" project/OsEngine/Market/Servers` -> no matches.
- **Commit:** code `f2be6a43b`, log `e7b79760b`
- **Push:** done (`origin/master`)

### Step 2.2 - InvariantCulture Persistence (Incremental Adoption #335)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened GateIo trade timestamp-fraction parsing to be culture-invariant:
    - `project/OsEngine/Market/Servers/GateIoData/GateIoDataServer.cs`
      - `time.AddMicroseconds(double.Parse(...))` -> `double.Parse(..., CultureInfo.InvariantCulture)`
      - `time.AddMilliseconds(double.Parse(...))` -> `double.Parse(..., CultureInfo.InvariantCulture)`
    - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/GateIoServerFutures.cs`
      - `time.AddMilliseconds(double.Parse(...))` -> `double.Parse(..., CultureInfo.InvariantCulture)`
  - Scope:
    - parsing hardening only; no business logic or transport contract changes.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** code `c64b68603`, log `724ad2e5e`
- **Push:** done (`origin/master`)

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #421)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed legacy expiration-date parsing hardening in:
    - `project/OsEngine/Market/Servers/Atp/AtpServer.cs`
  - Replacements:
    - in legacy security-line parsing, replaced unqualified
      `DateTime.TryParse(array[16], out expiration)`
      with helper parser call.
    - added `ParseDateInvariantOrCurrent(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - final fallback is non-throwing: `DateTime.MinValue`.
  - Scope:
    - parsing hardening only; ATP security load behavior unchanged for valid values.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** code `f74b49656`, log `f2e93babb`
- **Push:** done (`origin/master`)

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #422)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed filename-date parsing hardening in:
    - `project/OsEngine/Market/Servers/BybitData/BybitDataServer.cs`
  - Replacements:
    - in `ExtractDateFromFileName(...)`, replaced unqualified
      `DateTime.TryParse(match.Groups[1].Value, out date)`
      with helper parser call.
    - added `TryParseDateInvariantOrCurrent(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - method remains non-throwing and preserves `null` return on parse failure.
  - Scope:
    - parsing hardening only; Bybit historical filename-date extraction behavior unchanged for valid values.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** code `79bd490ed`, log `4f7de8d23`
- **Push:** done (`origin/master`)

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #423)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed datetime parsing hardening in:
    - `project/OsEngine/Market/Servers/TraderNet/TraderNetServer.cs`
  - Replacements:
    - replaced all remaining unqualified `DateTime.TryParse(...)` usages in:
      - order update trade mapping
      - public trade stream timestamp mapping
      - order status trade mapping
      - order callback timestamp mapping
    - added shared helper `ParseDateInvariantOrCurrent(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - final fallback is non-throwing: `DateTime.MinValue` (preserves prior graceful behavior on parse failure).
  - Scope:
    - parsing hardening only; TraderNet behavior unchanged for valid timestamps.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** code `49fb1e5a3`, log `3f9f8f7f7`
- **Push:** done (`origin/master`)

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #424)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed timestamp decimal parsing hardening in:
    - `project/OsEngine/Market/Servers/Polygon/PolygonServer.cs`
  - Replacements:
    - in trade conversion, replaced unqualified
      `decimal.TryParse(response.results[i].sip_timestamp, out timestamp)`
      with helper parser call.
    - added `ParseDecimalInvariantOrCurrent(...)` with parse priority:
      - `Invariant -> Current -> ru-RU`.
    - final fallback is non-throwing: `0m` (preserves prior default-on-fail behavior).
  - Scope:
    - parsing hardening only; Polygon trade timestamp mapping unchanged for valid values.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** code `17e0e25c5`, log `01a164e2d`
- **Push:** done (`origin/master`)

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #425)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed culture-safe numeric parsing hardening in:
    - `project/OsEngine/Entity/Extensions.cs`
  - Replacements:
    - in `ToDouble(string? value)` catch fallback, replaced implicit-culture
      `double.TryParse(value, out result)`
      with helper `TryParseDoubleInvariantOrCurrent(...)`.
    - helper parse priority:
      - `Invariant -> Current -> ru-RU` with `NumberStyles.Float | AllowThousands`.
    - final behavior remains non-throwing and keeps `0` fallback on parse failure.
  - Scope:
    - parsing hardening only; conversion semantics for valid numeric values unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** code `e347ba2a5`, log `1e2640624`
- **Push:** done (`origin/master`)

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #420)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed server timestamp parsing hardening in:
    - `project/OsEngine/Market/Servers/Transaq/TransaqServer.cs`
  - Replacements:
    - replaced all remaining `DateTime.Parse(...)` usages (ticks, candles, news, my-trades, order callbacks, trades) with one shared helper.
    - added `ParseDateInvariantOrCurrentOrThrow(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - on parse failure helper throws `FormatException`, preserving existing outer-thread error handling behavior.
  - Scope:
    - parsing hardening only; Transaq flow unchanged for valid timestamps.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #497)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Robots/BotCreateUi2.xaml.cs`
    - `project/OsEngine/Market/Connectors/MassSourcesCreateUi.xaml.cs`
    - `project/OsEngine/Market/Connectors/ConnectorCandlesUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in UI textbox/config parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #498)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Connectors/ConnectorNewsUi.xaml.cs`
    - `project/OsEngine/Market/Connectors/ConnectorNews.cs`
    - `project/OsEngine/Entity/DateTimeSelectionDialog.xaml.cs`
    - `project/OsEngine/Entity/SecurityUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in textbox and legacy settings parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #419)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed explicit parse-call cleanup in:
    - `project/OsEngine/Market/Servers/MoexAlgopack/MoexAlgopackServer.cs`
  - Replacements:
    - replaced `DateTime.Parse(item[7], CultureInfo.InvariantCulture)` with existing helper:
      `ParseDateInvariantOrCurrent(item[7])`.
    - replaced `DateTime.Parse(item[6], CultureInfo.InvariantCulture)` with existing helper:
      `ParseDateInvariantOrCurrent(item[6])`.
    - unified these paths with the same server-local parse strategy used by trades/depth.
  - Scope:
    - parsing hardening only; MOEX Algopack data flow unchanged for valid timestamps.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #418)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed fixed-value datetime comparison hardening in:
    - `project/OsEngine/Market/Servers/TInvest/TInvestServer.cs`
  - Replacements:
    - replaced parsing of constant string
      `DateTime.Parse("01.01.1970 03:00:00")`
      with explicit constant construction:
      `new DateTime(1970, 1, 1, 3, 0, 0)`.
  - Scope:
    - parsing hardening only; TInvest trade-time fallback logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #417)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed benchmark axis-label date parsing hardening in:
    - `project/OsEngine/Journal/JournalUi2.xaml.cs`
  - Replacements:
    - in benchmark comparison/load flow, replaced direct
      `DateTime.Parse(series.Points[...].AxisLabel)` with shared helper parser.
    - reused `ParseDateInvariantOrCurrentOrThrow(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> _currentCulture -> ru-RU`.
    - parse failures keep existing behavior via outer method `try/catch` (error log + null result).
  - Scope:
    - parsing hardening only; benchmark chart behavior unchanged for valid axis-label timestamps.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #415)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed benchmark timeline parsing hardening in:
    - `project/OsEngine/OsData/Benchmark.cs`
  - Replacements:
    - in benchmark download interval calculation, replaced direct
      `DateTime.Parse(_series.Points[...].AxisLabel)` with helper parser.
    - added `ParseDateInvariantOrCurrentOrThrow(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - on parse failure helper throws `FormatException`, preserving existing top-level error handling/logging flow.
  - Scope:
    - parsing hardening only; benchmark download logic unchanged for valid axis-label timestamps.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #416)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed test-parameter date parsing hardening in:
    - `project/OsEngine/Robots/AutoTestBots/ServerTests/AServerTester.cs`
  - Replacements:
    - for `Data_1` and `Data_4` test launchers, replaced direct
      `DateTime.Parse(...ValueString)` with shared helper parser.
    - added `ParseDateInvariantOrCurrentOrThrow(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - on parse failure helper throws `FormatException`, preserving existing fail-fast path handled by current worker-thread `try/catch`.
  - Scope:
    - parsing hardening only; server-test scenarios unchanged for valid date inputs.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #414)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed date parsing hardening in:
    - `project/OsEngine/OsData/LqdtDataFakeServer.cs`
  - Replacements:
    - replaced generic `DateTime.TryParse(...)` and `DateTime.Parse(...)` in key-rate loaders with shared helper methods.
    - added `TryParseDateInvariantOrCurrent(...)` and `ParseDateInvariantOrCurrent(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - malformed file records are now skipped explicitly (`dateAndKey.Length < 2` guard).
    - failed XML date parse returns `DateTime.MinValue` and row is skipped.
  - Scope:
    - parsing hardening only; key-rate loading logic unchanged for valid data.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #413)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed signal timestamp parsing hardening in:
    - `project/OsEngine/Robots/OnScriptIndicators/FundBalanceDivergenceBot.cs`
  - Replacements:
    - in close-position logic, replaced direct
      `Convert.ToDateTime(position.SignalTypeOpen)` with helper parser.
    - added `ParseDateInvariantOrCurrent(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - final fallback is non-throwing: `DateTime.MinValue`.
  - Scope:
    - parsing hardening only; strategy logic unchanged for valid signal timestamps.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #412)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed signal timestamp parsing hardening in:
    - `project/OsEngine/Robots/Patterns/CandlePatternBoost.cs`
  - Replacements:
    - in candle-count exit flow, replaced direct
      `Convert.ToDateTime(position.SignalTypeOpen)` with helper parser.
    - added `ParseDateInvariantOrCurrent(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - final fallback is non-throwing: `DateTime.MinValue`.
  - Scope:
    - parsing hardening only; strategy logic unchanged for valid signal timestamps.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #411)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed UI date-input parsing hardening in:
    - `project/OsEngine/Journal/JournalUi2.xaml.cs`
  - Replacements:
    - in range textbox handlers, replaced direct
      `Convert.ToDateTime(..., _currentCulture)` with helper parser.
    - added `TryParseDateInvariantOrCurrent(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> _currentCulture -> ru-RU`.
    - added `ParseDateInvariantOrCurrentOrThrow(...)` to preserve existing rollback behavior via surrounding `try/catch`.
  - Scope:
    - parsing hardening only; journal range UI behavior unchanged for valid date input.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #410)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed log date parsing hardening in:
    - `project/OsEngine/Logging/Log.cs`
  - Replacements:
    - in old-session log replay, replaced direct
      `Convert.ToDateTime(msgArray[0])` with helper parse method.
    - added `TryParseDateInvariantOrCurrent(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - on parse failure, behavior preserved: message is skipped (`continue`).
  - Scope:
    - parsing hardening only; logging flow and persisted log file format unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #409)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed legacy date parser fallback hardening in:
    - `project/OsEngine/Charts/CandleChart/Indicators/VWAP.cs`
  - Replacements:
    - in `ParseLegacyDateTime(...)`, final fallback changed from
      `Convert.ToDateTime(value, CultureInfo.CurrentCulture)` to safe `DateTime.MinValue`.
    - existing parse priority remains:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
  - Scope:
    - parsing hardening only; VWAP settings schema and behavior unchanged for valid values.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #408)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed UI date-input parsing hardening in:
    - `project/OsEngine/Market/Servers/Tester/TesterServerUi.xaml.cs`
  - Replacements:
    - replaced direct `Convert.ToDateTime(...)` parsing in:
      - non-trade-period grid start/end date handlers
      - tester range textboxes (`TextBoxTo`, `TextBoxFrom`) timer handlers
    - added `TryParseDateInvariantOrCurrent(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> _currentCulture -> ru-RU`.
    - added `ParseDateInvariantOrCurrentOrThrow(...)` for timer handlers to preserve existing `try/catch` rollback behavior.
  - Scope:
    - parsing hardening only; Tester UI behavior unchanged for valid date values.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #407)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed UI date-input parsing hardening in:
    - `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorageUi.xaml.cs`
  - Replacements:
    - in non-trade-period grid handlers, replaced direct
      `Convert.ToDateTime(value, OsLocalization.CurCulture)` with shared helper parser.
    - added `TryParseDateInvariantOrCurrent(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> _currentCulture -> ru-RU`.
    - invalid values keep existing behavior (early `return` without persisting changes).
  - Scope:
    - parsing hardening only; OptimizerDataStorage UI behavior unchanged for valid values.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #406)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed UI date-input parsing hardening in:
    - `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
  - Replacements:
    - in `_gridFazes_CellValueChanged(...)`, replaced direct
      `Convert.ToDateTime(..., _currentCulture)` with helper parser.
    - added `ParseDateInvariantOrCurrentOrThrow(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> _currentCulture -> ru-RU`.
    - on parse failure helper throws `FormatException`, preserving existing UI fallback behavior in surrounding `catch`.
  - Scope:
    - parsing hardening only; optimizer UI behavior unchanged for valid date values.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #405)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed UI date-input parsing hardening in:
    - `project/OsEngine/Entity/SecurityUi.xaml.cs`
  - Replacements:
    - replaced direct `Convert.ToDateTime(TextBoxExpiration.Text)` with helper parser.
    - added `ParseDateInvariantOrCurrentOrThrow(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - on parse failure helper throws `FormatException`, preserving existing UI validation flow via surrounding `try/catch`.
  - Scope:
    - parsing hardening only; security-editor behavior for valid values unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #404)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed inbound candle timestamp parsing hardening in:
    - `project/OsEngine/Market/Servers/MOEX/MoexIssDataServer.cs`
  - Replacements:
    - replaced direct `Convert.ToDateTime(innerArray[6].ToString())` with shared helper parser.
    - added `ParseDateInvariantOrCurrent(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - final fallback is non-throwing: `DateTime.MinValue`.
  - Scope:
    - parsing hardening only; MOEX ISS candle transport and loading flow unchanged for valid values.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #403)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed inbound timestamp parsing hardening in:
    - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
  - Replacements:
    - in my-trades fill-order flow, replaced direct
      `Convert.ToDateTime(response.data.fill_orders[i].filledTime)`
      with shared parser helper.
    - added `ParseTimestampOrDateInvariantOrCurrent(...)` with parse priority:
      - unix timestamp (ms/sec) -> `TimeManager.GetDateTimeFromTimeStamp`
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - final fallback is non-throwing: `DateTime.MinValue`.
  - Scope:
    - parsing hardening only; BingX API contract handling and trading logic unchanged for valid input.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #402)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed inbound timestamp parsing hardening in:
    - `project/OsEngine/Market/Servers/MoexAlgopack/MoexAlgopackServer.cs`
  - Replacements:
    - replaced direct `Convert.ToDateTime(...)` parsing in market-depth/trades responses with shared helper parser.
    - added `ParseDateInvariantOrCurrent(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - final fallback is non-throwing: `DateTime.MinValue`.
  - Scope:
    - parsing hardening only; MOEX Algopack transport contract and trading logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #401)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed culture-invariant numeric formatting in:
    - `project/OsEngine/Entity/Security.cs`
  - Replacements:
    - in `PriceStep` setter and `Decimals` getter, switched intermediate step string formatting
      from `new CultureInfo("ru-RU")` to `CultureInfo.InvariantCulture`.
    - decimal-part length detection aligned to invariant dot separator.
  - Scope:
    - numeric culture hardening only; security persistence schema and trading behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #400)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed inbound/metadata date parsing hardening in:
    - `project/OsEngine/Market/Servers/QscalpMarketDepth/QscalpMarketDepthServer.cs`
  - Replacements:
    - replaced direct `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` on HTML/file date values with shared helper parser.
    - added `ParseDateInvariantOrCurrent(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - final fallback is non-throwing: `DateTime.MinValue`.
    - removed redundant `Convert.ToDateTime` over an existing `DateTime` value (`_availableDates[0]`).
  - Scope:
    - parsing hardening only; Qscalp data flow and transport contract unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #399)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed inbound timestamp parsing hardening in:
    - `project/OsEngine/Market/Servers/NinjaTrader/NinjaTraderClient.cs`
  - Replacements:
    - replaced direct `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` calls in order/depth/trade handlers with a shared parser helper.
    - added `ParseDateInvariantOrCurrent(...)` with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
    - final fallback is non-throwing: `DateTime.MinValue`.
  - Scope:
    - parsing hardening only; NinjaTrader transport format and trading logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #398)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed date parser fallback hardening in:
    - `project/OsEngine/Market/Proxy/ProxyMaster.cs`
  - Replacements:
    - in `ParseDateInvariantOrCurrent(...)`, final fallback changed from
      `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` to safe `DateTime.MinValue`.
    - invariant/current/`ru-RU` parse attempts remain unchanged.
  - Scope:
    - parsing hardening only; proxy settings schema and behavior unchanged for valid values.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #397)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed date parser fallback hardening in:
    - `project/OsEngine/Entity/Order.cs`
    - `project/OsEngine/Entity/MyTrade.cs`
    - `project/OsEngine/Entity/Trade.cs`
  - Replacements:
    - in these helper parsers, final fallback changed from throwing conversion
      `Convert.ToDateTime(..., InvariantCulture)` to safe fallback `DateTime.MinValue`.
    - invariant/current/`ru-RU` parse attempts remain unchanged and still execute first.
  - Scope:
    - parsing hardening only; persistence schema and runtime trading logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #396)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed order persistence datetime hardening in:
    - `project/OsEngine/Entity/Order.cs`
  - Replacements:
    - `LastCancelTryLocalTime` save value now uses invariant round-trip format:
      - `ToString("O", CultureInfo.InvariantCulture)`.
    - in `SetOrderFromString(...)`, cancellation timestamp load switched from direct
      `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` to shared helper parser.
    - `ParseDateTimeInvariantWithRuFallback(...)` enhanced with parse priority:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
  - Scope:
    - persistence parsing/serialization hardening only; order behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #395)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed TradeGrid datetime persistence hardening in:
    - `project/OsEngine/OsTrader/Grids/TradeGrid.cs`
  - Replacements:
    - in `GetSaveString()`, `_firstTradeTime` now serializes via:
      - `ToString("O", CultureInfo.InvariantCulture)`.
    - in `LoadFromString(...)`, replaced direct
      `Convert.ToDateTime(values[10], CultureInfo.InvariantCulture)` with invariant-first fallback parser:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`.
  - Scope:
    - persistence parsing/serialization hardening only; grid trading logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #394)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed optimizer phase datetime persistence hardening in:
    - `project/OsEngine/OsOptimizer/OptimizerMaster.cs`
  - Replacements:
    - in optimizer phase `GetSaveString()`:
      - `_timeStart`/`_timeEnd` serialization switched to `ToString("O", CultureInfo.InvariantCulture)`.
    - in `LoadFromString(...)`:
      - replaced direct `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` with invariant-first fallback parser (`Invariant Roundtrip -> Invariant -> Current -> ru-RU`).
  - Scope:
    - persistence parsing/serialization hardening only; optimizer phase logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #393)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed legacy settings date parsing hardening in:
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Replacements:
    - in `ParseLegacySecurityTestSettings(...)`, replaced direct
      `Convert.ToDateTime(lines[0/1], CultureInfo)` with existing helper
      `ParseDateInvariantOrCurrent(...)`.
  - Scope:
    - parsing hardening only; settings format and tester behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #392)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed parser cleanup in:
    - `project/OsEngine/OsData/OsDataSet.cs`
  - Replacements:
    - in `SettingsToLoadSecurity.Load(...)`, removed `try/catch` with direct `Convert.ToDateTime(..., InvariantCulture)` for `TimeStart/TimeEnd`.
    - switched to direct use of existing invariant-first fallback parser for both fields.
  - Scope:
    - parsing hardening/cleanup only; persisted schema and runtime behavior for valid values unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #391)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed datetime persistence hardening in:
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Replacements:
    - `OrderClearing.GetSaveString()` and `NonTradePeriod.GetSaveString()` now serialize datetime values in invariant round-trip format:
      - `ToString("O", CultureInfo.InvariantCulture)`
    - corresponding `SetFromString(...)` methods now use invariant-first fallback parser:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
      - instead of direct `Convert.ToDateTime(..., CultureInfo.InvariantCulture)`.
  - Scope:
    - persistence parsing/serialization hardening only; tester matching/trade logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #390)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed optimizer settings datetime persistence hardening in:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
  - Replacements:
    - `_timeStart` and `_timeEnd` save serialization now uses invariant round-trip format:
      - `ToString("O", CultureInfo.InvariantCulture)`
    - load path now uses helper parser with invariant-first fallback chain:
      - `Invariant Roundtrip -> Invariant -> Current -> ru-RU`
      - replacing direct `Convert.ToDateTime(..., CultureInfo.InvariantCulture)` usage.
  - Scope:
    - persistence parsing/serialization hardening only; optimizer logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #389)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed datetime serialization hardening in:
    - `project/OsEngine/OsData/OsDataSet.cs`
  - Replacements:
    - `SettingsToLoadSecurity.GetSaveStr()` now serializes `TimeStart` and `TimeEnd` in invariant round-trip format:
      - `ToString("O", CultureInfo.InvariantCulture)`
  - Scope:
    - persistence serialization hardening only; settings schema and load behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #388)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed screener settings datetime persistence hardening in:
    - `project/OsEngine/Robots/AlgoStart/AlgoStart2Soldiers.cs`
    - `project/OsEngine/Robots/Grids/GridScreenerAdaptiveSoldiers.cs`
    - `project/OsEngine/Robots/Screeners/ThreeSoldierAdaptiveScreener.cs`
    - `project/OsEngine/Robots/Screeners/PinBarVolatilityScreener.cs`
  - Replacements:
    - `LastUpdateTime` serialization now uses round-trip invariant format (`ToString("O", CultureInfo.InvariantCulture)`).
    - corresponding load paths now use invariant-first fallback parser (`Invariant Roundtrip -> Invariant -> Current -> ru-RU`) instead of direct `Convert.ToDateTime(..., InvariantCulture)`.
  - Scope:
    - persistence parsing/serialization hardening only; screener logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #387)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed date parser priority harmonization in:
    - `project/OsEngine/Entity/Security.cs`
    - `project/OsEngine/Entity/PositionOpenerToStop.cs`
    - `project/OsEngine/Market/Proxy/ProxyMaster.cs`
  - Replacements:
    - added explicit invariant non-roundtrip parse step (`DateTimeStyles.None`) after invariant round-trip parse in legacy datetime helpers.
    - `ProxyMaster` parser now attempts invariant round-trip parse before existing invariant/current/`ru-RU` fallback flow.
  - Scope:
    - parsing hardening only; persistence schema and business logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #386)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed date parsing priority hardening in:
    - `project/OsEngine/Entity/MyTrade.cs`
    - `project/OsEngine/Entity/Trade.cs`
  - Replacements:
    - `MyTrade` legacy parser now also checks `CultureInfo.CurrentCulture` before `ru-RU` fallback.
    - `Trade` IQFeed date parser now checks invariant round-trip parsing (`DateTimeStyles.RoundtripKind`) before standard invariant/current/`ru-RU` fallbacks.
  - Scope:
    - parsing hardening only; save formats and trade processing logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #385)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed datetime persistence hardening in:
    - `project/OsEngine/OsData/OsDataSet.cs`
  - Replacements:
    - `SetDublicator` and `SetUpdater` save paths now serialize `DateTime` with round-trip format (`"O"`, invariant culture).
    - corresponding load paths now use invariant-first fallback parser (`Invariant Roundtrip -> Invariant -> Current -> ru-RU`) instead of direct `Convert.ToDateTime(..., InvariantCulture)`.
  - Scope:
    - persistence parsing/serialization hardening only; update/duplicate business logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #384)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed date parsing hardening in:
    - `project/OsEngine/Charts/CandleChart/Indicators/VWAP.cs`
  - Replacements:
    - legacy date parser now uses invariant-first parse priority with fallback to current culture and `ru-RU`.
  - Scope:
    - parsing hardening only; indicator logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #383)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed date parsing hardening in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabOptions.cs`
  - Replacements:
    - selected-expiration date parsing now uses explicit invariant-first fallback parser instead of culture-implicit `Convert.ToDateTime(...)`.
  - Scope:
    - parsing hardening only; options tab behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #382)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed persistence serialization hardening in:
    - `project/OsEngine/Layout/GlobalGUILayout.cs`
  - Replacements:
    - `OpenWindow.GetSaveString()` now serializes window layout decimals with `ToString(CultureInfo.InvariantCulture)`.
  - Scope:
    - persistence serialization hardening only; UI behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #381)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed date parsing hardening in:
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Replacements:
    - `secu.Expiration` load from dop-settings now uses invariant-first fallback parser instead of culture-implicit `Convert.ToDateTime(...)`.
  - Scope:
    - parsing hardening only; tester settings schema/logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #380)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed datetime persistence hardening in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs`
  - Replacements:
    - `_lastTimeUpdateIndex` now saves in invariant round-trip format.
    - load/parse path for this timestamp now uses invariant-first fallback parser instead of culture-implicit conversion.
  - Scope:
    - persistence parsing/serialization hardening only; index behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #379)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed persistence serialization/parsing hardening in:
    - `project/OsEngine/Alerts/AlertToChart.cs`
  - Replacements:
    - `ChartAlertLine` now saves datetime values in invariant round-trip format and decimal values with invariant culture.
    - datetime load now uses invariant-first fallback with legacy `ru-RU` compatibility.
  - Scope:
    - persistence hardening only; alert runtime behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #378)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed date-parse hardening in:
    - `project/OsEngine/OsData/OsDataSet.cs`
  - Replacements:
    - `SettingsToLoadSecurity.Load()` fallback date parsing now uses explicit invariant-first parser instead of culture-implicit `Convert.ToDateTime(...)`.
  - Scope:
    - parsing hardening only; settings persistence schema unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #377)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed legacy date parsing hardening in:
    - `project/OsEngine/Market/Proxy/ProxyMaster.cs`
  - Replacements:
    - legacy loader date parse now uses invariant-first fallback parser instead of culture-implicit `Convert.ToDateTime(...)`.
  - Scope:
    - parsing hardening only; settings schema and proxy runtime behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #376)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed persistence serialization hardening in:
    - `project/OsEngine/OsData/OsDataSet.cs`
  - Replacements:
    - `SecurityToLoad.GetSaveStr()` now serializes `PriceStep` and `VolumeStep` with `ToString(CultureInfo.InvariantCulture)`.
  - Scope:
    - persistence serialization hardening only; loading compatibility preserved via existing `ToDecimal()`.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #377)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Finalized remaining market-server replace-based decimal formatting in:
    - `project/OsEngine/Market/Servers/CoinEx/Spot/CoinExServerSpot.cs`
    - `project/OsEngine/Market/Servers/CoinEx/Futures/CoinExServerFutures.cs`
    - `project/OsEngine/Market/Servers/Atp/AtpServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
    - `project/OsEngine/Market/Servers/Bybit/BybitServer.cs`
  - Replacements:
    - removed `.Replace(",", ".")` where values were already formatted with `CultureInfo.InvariantCulture`.
    - converted remaining decimal `ToString().Replace(",", ".")` paths to explicit invariant formatting.
  - Bybit leverage path:
    - introduced `NormalizeNumericValueForApi(string)` helper for leverage strings with parse cascade:
      - `InvariantCulture`
      - `CurrentCulture`
      - `ru-RU`
    - fallback preserves prior non-numeric handling (`Replace(",", ".")`) to avoid behavior regressions.
  - Additional:
    - added `using System.Globalization;` in MoexFix server files.
  - Scope:
    - formatting hardening only; no API contract or flow changes.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
  - `rg -n 'ToString\\([^)]*\\)\\.Replace\\(\",\",\\s*\"\\.\"\\)' project/OsEngine/Market/Servers` returned no matches.
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #375)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed culture-safe numeric normalization in:
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Replacements:
    - replaced `ToString().Replace(",", ".")` with `ToString(CultureInfo.InvariantCulture)` in candle price-step analysis.
    - removed unnecessary decimal-double-decimal roundtrip conversions in candle/tick price-step loops.
  - Scope:
    - parsing/normalization hardening only; tester business logic and data formats unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #376)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened outbound decimal payload formatting in:
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/BitfinexFuturesServer.cs`
    - `project/OsEngine/Market/Servers/BloFin/BloFinFutures/BloFinFuturesServer.cs`
    - `project/OsEngine/Market/Servers/FinamGrpc/FinamGrpcServer.cs`
    - `project/OsEngine/Market/Servers/XT/XTSpot/XTServerSpot.cs`
  - Replacements:
    - `decimal.ToString().Replace(",", ".")` -> `decimal.ToString(CultureInfo.InvariantCulture)`
  - Covered fields:
    - `price`, `size`, `amount`, `quantity`, and `quoteQty` payload values.
  - Additional:
    - added `using System.Globalization;` where needed.
  - Scope:
    - formatting hardening only; no API contract changes.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #374)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed datetime serialization/parsing hardening in:
    - `project/OsEngine/Entity/Trade.cs`
    - `project/OsEngine/Entity/MyTrade.cs`
  - Replacements:
    - removed culture-implicit `Convert.ToDateTime` in IqFeed trade parse path, replaced with invariant-first fallback parser.
    - `MyTrade` save now uses invariant round-trip datetime format (`"O"`), with backward-compatible parse fallback retained.
  - Scope:
    - persistence parsing/serialization hardening only; trade processing logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #375)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened order payload decimal formatting in:
    - `project/OsEngine/Market/Servers/ExMo/ExmoSpot/ExmoSpotServer.cs`
    - `project/OsEngine/Market/Servers/Woo/WooServer.cs`
    - `project/OsEngine/Market/Servers/KiteConnect/KiteConnectServer.cs`
    - `project/OsEngine/Market/Servers/TraderNet/TraderNetServer.cs`
    - `project/OsEngine/Market/Servers/Pionex/PionexServerSpot.cs`
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/KuCoinSpotServer.cs`
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServer.cs`
  - Replacements:
    - `decimal.ToString().Replace(",", ".")` -> `decimal.ToString(CultureInfo.InvariantCulture)`
    - TraderNet `qty` switched from culture-default `.ToString()` to invariant formatting.
  - Additional:
    - added `using System.Globalization;` in files where invariant formatting was introduced.
  - Scope:
    - formatting hardening only; no changes in API parameters set or order flow logic.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #373)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed persistence formatting hardening in:
    - `project/OsEngine/Entity/MarketDepth.cs`
  - Replacements:
    - `GetSaveStringToAllDepfh()` now serializes ask/bid `price/volume` values with `ToString(CultureInfo.InvariantCulture)`.
  - Scope:
    - persistence serialization hardening only; parsing compatibility preserved via existing `ToDouble()`.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #374)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened decimal serialization for outbound order payloads in:
    - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
    - `project/OsEngine/Market/Servers/HTX/Spot/HTXSpotServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
    - `project/OsEngine/Market/Servers/OKX/OkxServer.cs`
  - Replacements:
    - `decimal.ToString().Replace(",", ".")` -> `decimal.ToString(CultureInfo.InvariantCulture)`
    - formatted volume path `ToString("0.#####").Replace(",", ".")` -> `ToString("0.#####", CultureInfo.InvariantCulture)`
  - Additional:
    - added `using System.Globalization;` in affected GateIo/HTX files where invariant formatting was introduced.
  - Scope:
    - payload-format hardening only; connector control flow and API fields unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #372)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed persistence hardening in:
    - `project/OsEngine/Entity/PositionOpenerToStop.cs`
  - Replacements:
    - `GetSaveString()` decimal fields now serialize via `ToString(CultureInfo.InvariantCulture)`.
    - datetime fields now serialize via invariant round-trip format (`"O"`).
    - `LoadFromString()` datetime parsing now uses invariant-first fallback with legacy `ru-RU` compatibility.
  - Scope:
    - persistence serialization/parsing hardening only; runtime logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #373)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened outbound decimal payload formatting in exchange connectors:
    - `project/OsEngine/Market/Servers/Bybit/BybitServer.cs`
    - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
    - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
    - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/BitGetServerSpot.cs`
    - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/BitGetServerFutures.cs`
  - Replacements:
    - `decimal.ToString().Replace(",", ".")` -> `decimal.ToString(CultureInfo.InvariantCulture)` for `price/qty/size` request fields.
    - `newPrice` payload formatting switched to invariant string.
  - Additional:
    - added `using System.Globalization;` in BitGet spot/futures server files.
  - Scope:
    - formatting hardening only; connector behavior and API field sets unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #371)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed persistence hardening in:
    - `project/OsEngine/Entity/Security.cs`
  - Replacements:
    - `GetSaveStr()` decimal values now serialize with `ToString(CultureInfo.InvariantCulture)`.
    - `Expiration` now serializes as invariant round-trip (`"O"`).
    - `LoadFromString()` now parses `Expiration` via invariant-first fallback parser to keep compatibility with old saves.
  - Scope:
    - persistence serialization/parsing hardening only; business logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #372)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened separator handling in table export and decimal-step check:
    - `project/OsEngine/Entity/Extensions.cs`
    - `project/OsEngine/Robots/AutoTestBots/ServerTests/Var_1_Securities.cs`
  - Replacements:
    - `ToFormatString(DataGridViewRow)`:
      - removed unconditional `Replace(",", ".")` on all cell text
      - numeric cells now emit invariant-formatted values
      - non-numeric cells keep textual content (newline-stripped only)
    - `IsCompairDecimalsAndStep(...)`:
      - removed `Replace(",", ".")`
      - separator-neutral fractional precision detection by last separator index (`.` / `,`)
  - Scope:
    - parsing/formatting hardening only; no changes to business logic.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #370)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed decimal serialization hardening in optimizer report persistence:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerReportSerializer.cs`
    - `project/OsEngine/OsOptimizer/OptimizerReport.cs`
  - Replacements:
    - serializer aggregate decimals now use `ToString(CultureInfo.InvariantCulture)`.
    - `OptimizerReportTab.GetSaveString()` decimal fields now use `ToString(CultureInfo.InvariantCulture)`.
  - Scope:
    - persistence serialization hardening only; loading compatibility preserved via existing `ToDecimal()`.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #371)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened central double parse helper in:
    - `project/OsEngine/Entity/Extensions.cs`
  - Replacements:
    - `ToDouble(this string?)` no longer uses replace-based separator normalization and direct `Convert.ToDouble(...)`.
    - parse path is unified through `TryParseDoubleInvariantOrCurrent(...)` with culture cascade:
      - `InvariantCulture`
      - `CurrentCulture`
      - `ru-RU`
  - Scope:
    - parsing hardening only; no change to persisted schema or business logic.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #369)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed decimal serialization hardening in candle series parameters:
    - `project/OsEngine/Candles/Factory/CandleSeriesParameter.cs`
  - Replacements:
    - `CandlesParameterDecimal.GetStringToSave()` now serializes decimal using `ToString(CultureInfo.InvariantCulture)`.
  - Scope:
    - persistence serialization hardening only; load compatibility preserved via existing `ToDecimal()`.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #370)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened decimal precision parsing helper in:
    - `project/OsEngine/Entity/Extensions.cs`
  - Replacements:
    - `DecimalsCount(this string?)` no longer uses replace-based separator normalization (`Replace(",", ".")`).
    - added separator-neutral fraction detection (`.` / `,` via last-separator index) with preserved trailing-zero trim behavior.
    - preserved exponent normalization path (`E` -> invariant decimal string).
  - Scope:
    - parsing hardening only; external persistence formats and behavior for valid inputs unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #368)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed core decimal serialization hardening in parameter persistence:
    - `project/OsEngine/Entity/StrategyParameter.cs`
    - `project/OsEngine/Indicators/IndicatorParameter.cs`
  - Replacements:
    - decimal concatenation in `GetStringToSave()` -> explicit `ToString(CultureInfo.InvariantCulture)` for:
      - `StrategyParameterDecimal`
      - `StrategyParameterDecimalCheckBox`
      - `IndicatorParameterDecimal`
  - Scope:
    - persistence serialization hardening only; load compatibility preserved via existing `ToDecimal()`.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #427)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Standardized decimal payload serialization in:
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
    - `project/OsEngine/Market/Servers/Woo/WooServer.cs`
    - `project/OsEngine/Market/Servers/Mexc/MexcSpot/MexcSpotServer.cs`
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
  - Replacements:
    - `ToString().Replace(',', '.')` -> `ToString(CultureInfo.InvariantCulture)` for order `price` / `quantity` / `amount` fields.
    - Moex FIX replace paths: `OrderQty` serialization switched to invariant decimal string.
  - Added missing `using System.Globalization;` in files where invariant formatting was introduced.
  - Scope:
    - payload serialization hardening only; connector business logic unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
  - `rg -n "ToString\\(\\)\\.Replace\\(',', '\\.'\\)" project/OsEngine/Market/Servers` -> remaining matches only in `BitMart`, `Transaq`, `Plaza`.
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #426)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Removed residual replace-based decimal normalization in:
    - `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs`
    - `project/OsEngine/Market/Servers/Binance/Futures/BinanceServerFutures.cs`
    - `project/OsEngine/Market/Servers/Bybit/BybitServer.cs`
  - Binance Spot/Futures:
    - replaced `minQty.ToStringWithNoEndZero().Replace(",", ".")` and related split logic
    - with separator-agnostic fractional-length detection using last separator index (`.` or `,`).
  - Bybit leverage helper:
    - `NormalizeNumericValueForApi(...)` fallback changed from blanket `Replace(",", ".")`
    - to safe `NormalizeNumericCommas(...)` that rewrites only commas between digits.
  - Scope:
    - formatting/parsing hardening only; connector business logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
  - `rg -n 'Replace\\(\\s*\"\\,\"\\s*,\\s*\"\\.\"\\s*\\)' project/OsEngine` -> no matches.
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #369)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened core decimal parse helper in:
    - `project/OsEngine/Entity/Extensions.cs`
  - Replacements:
    - `ToDecimal(this string?)` no longer uses replace-based decimal separator normalization + direct `Convert.ToDecimal(...)`.
    - introduced `TryParseDecimalInvariantOrCurrent(...)` with explicit culture cascade:
      - `InvariantCulture`
      - `CurrentCulture`
      - `ru-RU` fallback
    - retained compatibility fallback through `value.ToDouble()` conversion.
  - Additional adjustment:
    - `DecimalsCount(...)` exponential-path normalization now uses invariant decimal string (`ToString(CultureInfo.InvariantCulture)`).
  - Scope:
    - parsing hardening only; persistence formats and domain logic unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `curl.exe -I https://api.nuget.org/v3/index.json --ssl-no-revoke` succeeded (`HTTP/1.1 200 OK`).
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #355)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed persistence culture hardening in Trend robots:
    - `project/OsEngine/Robots/Trend/WsurfBot.cs`
    - `project/OsEngine/Robots/Trend/SmaStochastic.cs`
    - `project/OsEngine/Robots/Trend/PriceChannelTrade.cs`
  - Replacements:
    - decimal save values: `ToString()` -> `ToString(CultureInfo.InvariantCulture)`
    - decimal load values in `WsurfBot`/`SmaStochastic`: `Convert.ToDecimal(reader.ReadLine())` -> `reader.ReadLine().ToDecimal()`
    - `PriceChannelTrade`: save serialization switched to invariant format (load already used `ToDecimal()`).
  - Scope:
    - persistence serialization/parsing hardening only; trading logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #354)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed persistence culture hardening in CounterTrend robots:
    - `project/OsEngine/Robots/CounterTrend/WilliamsRangeTrade.cs`
    - `project/OsEngine/Robots/CounterTrend/StrategyBollinger.cs`
    - `project/OsEngine/Robots/CounterTrend/RsiContrtrend.cs`
  - Replacements:
    - decimal save values: `ToString()` -> `ToString(CultureInfo.InvariantCulture)`
    - decimal load values: `Convert.ToDecimal(reader.ReadLine())` -> `reader.ReadLine().ToDecimal()`
  - Scope:
    - persistence serialization/parsing hardening only; trading logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.1 - Atomic File Writes (Incremental Adoption #353)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.1
- **Changes:**
  - Hardened Polygon securities cache persistence flow in:
    - `project/OsEngine/Market/Servers/Polygon/PolygonServer.cs`
  - `GetSecurityData()` now:
    - writes paged ticker payloads into temp cache file (`<cache>.tmp`) through shared stream writer
    - atomically finalizes cache:
      - `File.Replace(temp, target, target + ".bak", true)` when target exists
      - `File.Move(temp, target)` when target is new
    - removes temp file in `finally`.
  - `SaveSecurityToFile(...)` refactored to write into provided `StreamWriter` instead of opening target path directly.
  - Added compatible pre-finalize flush:
    - `FileStream.Flush(true)` when available
    - fallback `Stream.Flush()`.
  - Scope:
    - durability hardening only; cache payload format and business behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Persistence (Incremental Adoption #336)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened legacy decimal settings parsing in:
    - `project/OsEngine/OsTrader/RiskManager/RiskManager.cs`
  - Updated `ParseLegacySettings(...)`:
    - `MaxDrowDownToDayPersent = Convert.ToDecimal(lines[0])`
    - -> `MaxDrowDownToDayPersent = ParseDecimalInvariantOrCurrent(lines[0])`
  - Added helper `ParseDecimalInvariantOrCurrent(string value)` with parse cascade:
    - `InvariantCulture` (`NumberStyles.Any`)
    - `CurrentCulture` fallback
    - final legacy `Convert.ToDecimal(value)` fallback
  - Upstream binary artifact check:
    - confirmed `project/OsEngine/bin/Debug/OsEngine.dll` and `project/OsEngine/bin/Debug/OsEngine.exe` are not tracked by git and absent in working tree.
  - Scope:
    - persistence/culture hardening only; no runtime risk-manager algorithm changes.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Persistence (Incremental Adoption #337)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed Step 2.2 hardening over upstream-introduced settings parsing paths:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabOptions.cs`
      - `StrikesToShow` parse changed from `Convert.ToDecimal(value)` to culture-neutral `value.ToDecimal()`.
    - `project/OsEngine/Entity/SetLeverageUi.xaml.cs`
      - removed decimal parse normalization via `Replace(".", ",")`.
      - added `TryParseDecimalInvariantOrCurrent(string value, out decimal result)`:
        - `InvariantCulture` (`NumberStyles.Any`)
        - `CurrentCulture` fallback.
      - wired helper into:
        - `TextBoxLeverage_TextChanged(...)`
        - `_dgv_CellValueChanged(...)` leverage cell validation.
  - Scope:
    - culture-parsing hardening only; leverage/options business logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Persistence (Incremental Adoption #338)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened benchmark-data decimal parsing in:
    - `project/OsEngine/Journal/JournalUi2.xaml.cs`
  - Updated `LoadBenchmarkData(...)`:
    - `decimal.Parse(parts[5].Replace(".", ","))`
    - -> `ParseDecimalInvariantOrCurrent(parts[5])`
  - Added helper `ParseDecimalInvariantOrCurrent(string value)`:
    - `InvariantCulture` (`NumberStyles.Any`)
    - `CurrentCulture` fallback
    - final `Convert.ToDecimal(value)` fallback to keep legacy exception behavior.
  - Scope:
    - parsing hardening only; benchmark selection/chart logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Upstream Replay Audit Checkpoint (Incremental Adoption #339)

- **Status:** In Progress (audit checkpoint completed)
- **Plan item:** cross-check against `refactoring_stage2_plan.md` after upstream integration (`733b909d5^1..733b909d5^2`)
- **Audit scope:**
  - Step 2.2 pattern group: culture-sensitive numeric parsing (`Parse/TryParse`, replace-based decimal normalization)
  - Step 4.1 pattern group: upstream-introduced `new object()` lock fields
  - Step 0.3 pattern group: upstream-introduced silent catches (`catch { }`)
- **Method:**
  - pattern search on files touched by upstream range
  - line-level `git blame` attribution filtered to upstream commit set
- **Result:**
  - no remaining upstream-attributed matches for the above pattern groups after current replay-fix set (`#334..#338`).
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Governance - Tracked Binary Guard (Incremental Adoption #340)

- **Status:** In Progress (increment completed)
- **Plan linkage:** Execution Governance Addendum + Step 4.3 anti-regression support
- **Changes:**
  - Added helper script:
    - `tools/check-tracked-debug-binaries.ps1`
  - Behavior:
    - default mode checks only:
      - `project/OsEngine/bin/Debug/OsEngine.dll`
      - `project/OsEngine/bin/Debug/OsEngine.exe`
    - strict mode (`-StrictAllDebugBinaries`) checks all `project/OsEngine/bin/Debug/*.dll|*.exe`.
  - Purpose:
    - immediate post-merge verification that critical app binaries did not get reintroduced into git index.
    - reduce repeated manual audit effort on future upstream integrations.
- **Verification:**
  - `pwsh -NoProfile -File tools/check-tracked-debug-binaries.ps1` -> success (`OK`, critical targets not tracked).
  - `pwsh -NoProfile -File tools/check-tracked-debug-binaries.ps1 -StrictAllDebugBinaries` -> fails with tracked baseline list (expected; repository currently tracks historical vendor binaries).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Governance - Automated Upstream Replay Audit (Incremental Adoption #341)

- **Status:** In Progress (increment completed)
- **Plan linkage:** Execution Governance Addendum (post-merge replay control)
- **Changes:**
  - Added script:
    - `tools/audit-upstream-replay.ps1`
  - Script behavior:
    - accepts merge commit (`-MergeCommit`, default `HEAD`)
    - resolves upstream integration range (`merge^1..merge^2`)
    - scans changed `.cs` files only
    - performs line-level `git blame` attribution and reports only lines authored by commits from the upstream range
    - checks configured pattern groups for:
      - Step 0.3 (`catch { }`)
      - Step 2.2 (culture-sensitive parse signatures)
      - Step 4.1 (`new object()` lock signatures)
- **Verification:**
  - `pwsh -NoProfile -File tools/audit-upstream-replay.ps1 -MergeCommit 733b909d5` -> success (`OK`, 0 findings).
  - `pwsh -NoProfile -File tools/audit-upstream-replay.ps1 -MergeCommit HEAD` -> success (`OK`, 0 findings).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.1 - Atomic File Writes (Incremental Adoption #342)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.1
- **Changes:**
  - Replayed atomic-write hardening over upstream-accepted export paths in:
    - `project/OsEngine/Journal/JournalUi2.xaml.cs`
  - Replaced direct writes:
    - `using StreamWriter writer = new StreamWriter(fileName); writer.Write(workSheet);`
    - -> `SafeFileWriter.WriteAllText(fileName, workSheet.ToString());`
  - Applied in two handlers:
    - open positions export (`OpenDealSaveInFile_Click`)
    - close positions export (`CloseDealSaveInFile_Click`)
  - Scope:
    - file-write durability hardening only; export payload format unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Upstream Replay Audit - Full Check For Today's Accepted Commits (Incremental Adoption #343)

- **Status:** In Progress (audit checkpoint completed)
- **Plan item:** full replay conformance check for today's accepted upstream merge in scope of Stage 2/4 pattern groups
- **Changes:**
  - Extended `tools/audit-upstream-replay.ps1` checks with Step 2.1 pattern groups:
    - `step2_1_streamwriter_write`
    - `step2_1_file_writeall`
    - `step2_1_filestream_create_append`
  - Existing checks retained:
    - Step 0.3 silent catches
    - Step 2.2 culture-sensitive parse signatures
    - Step 4.1 object lock signatures
  - Ran audit for today's merge:
    - merge commit: `733b909d5`
    - upstream range: `733b909d5^1..733b909d5^2`
    - scanned files: `1124` (`.cs` only)
- **Result:**
  - no upstream-attributed replay findings for configured checks.
- **Verification:**
  - `pwsh -File tools/audit-upstream-replay.ps1 -MergeCommit 733b909d5 -RepoRoot .` -> success (`OK`, 0 findings)
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.1 - Atomic File Writes (Incremental Adoption #344)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.1
- **Changes:**
  - Replayed atomic-write hardening over upstream-accepted export paths in:
    - `project/OsEngine/Journal/JournalUi.xaml.cs`
  - Replaced direct writes:
    - `using StreamWriter writer = new StreamWriter(fileName); writer.Write(workSheet);`
    - -> `SafeFileWriter.WriteAllText(fileName, workSheet.ToString());`
  - Applied in two handlers:
    - open positions export (`OpenDealSaveInFile_Click`)
    - close positions export (`CloseDealSaveInFile_Click`)
  - Scope:
    - file-write durability hardening only; export payload format unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.1 - Atomic File Writes (Incremental Adoption #345)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.1
- **Changes:**
  - Replayed atomic-write hardening on additional Entity save/export paths:
    - `project/OsEngine/Entity/DataGridFactory.cs`
    - `project/OsEngine/Entity/SecuritiesUi.xaml.cs`
    - `project/OsEngine/Entity/NonTradePeriodsUi.xaml.cs`
  - Replacements:
    - `DataGridFactory`: `StreamWriter(fileName)` -> `SafeFileWriter.WriteAllText(fileName, saveStr)`
    - `SecuritiesUi`: `StreamWriter(filePath, false)` -> `SafeFileWriter.WriteAllLines(filePath, new[] { mySecurity.GetSaveStr() })`
    - `NonTradePeriodsUi`: line-by-line `StreamWriter(filePath)` -> `SafeFileWriter.WriteAllLines(filePath, array)`
  - Cleanup:
    - removed redundant `File.Create(filePath)` pre-create in `NonTradePeriodsUi` save handler.
  - Scope:
    - file-write durability hardening only; content format and UI behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.1 - Atomic File Writes (Incremental Adoption #346)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.1
- **Changes:**
  - Replayed atomic-write hardening on OsData save paths:
    - `project/OsEngine/OsData/OsDataSetUi.xaml.cs`
    - `project/OsEngine/OsData/OsDataMasterPainter.cs`
  - Replacements:
    - `OsDataSetUi`: `File.WriteAllText(filePath, contentToSave)` -> `SafeFileWriter.WriteAllText(filePath, contentToSave)`
    - `OsDataMasterPainter`: `StreamWriter(@"Engine\\OsDataAttachedServers.txt", false)` loop -> `SafeFileWriter.WriteAllLines(@"Engine\\OsDataAttachedServers.txt", _attachedServers.Select(server => server.ToString()))`
  - Scope:
    - file-write durability hardening only; content format and UI behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.1 - Atomic File Writes (Incremental Adoption #347)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.1
- **Changes:**
  - Replayed atomic-write hardening on additional UI save/export paths:
    - `project/OsEngine/Entity/StrategyParametersUi.xaml.cs`
    - `project/OsEngine/OsTrader/Grids/TradeGridUi.xaml.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreenerUi.xaml.cs`
    - `project/OsEngine/Market/Proxy/ProxyMasterUi.xaml.cs`
    - `project/OsEngine/Market/Connectors/MassSourcesCreateUi.xaml.cs`
    - `project/OsEngine/Market/AutoFollow/CopyPortfolioUi.xaml.cs`
    - `project/OsEngine/OsOptimizer/OptimizerReportUi.xaml.cs`
  - Replacements:
    - direct `StreamWriter(...)` / `File.WriteAllText(...)` -> `SafeFileWriter.WriteAllText(...)` or `SafeFileWriter.WriteAllLines(...)`
  - Cleanup:
    - removed redundant `File.Create(...)` pre-create blocks in save handlers where they preceded write.
  - Scope:
    - file-write durability hardening only; content format and UI behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.1 - Atomic File Writes (Incremental Adoption #348)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.1
- **Changes:**
  - Replayed atomic-write hardening on core/server save paths:
    - `project/OsEngine/Market/ServerMaster.cs`
    - `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorage.cs`
    - `project/OsEngine/Market/Servers/QuikLua/QuikLuaServer.cs`
    - `project/OsEngine/Market/Servers/Finam/Entity/FinamDataSeries.cs`
    - `project/OsEngine/Market/Servers/ServerCandleStorage.cs`
    - `project/OsEngine/Candles/CandleConverter.cs`
  - Replacements:
    - direct one-shot `StreamWriter(...)` / `File.WriteAllText(...)` -> `SafeFileWriter.WriteAllLines(...)` / `SafeFileWriter.WriteAllText(...)`
  - Scope:
    - file-write durability hardening only; persistence format and runtime behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.1 - Atomic File Writes (Incremental Adoption #349)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.1
- **Changes:**
  - Replayed atomic-write hardening on robot settings save paths:
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
  - Replacements:
    - direct one-shot `StreamWriter(Get...Path(), false)` save blocks -> `SafeFileWriter.WriteAllLines(...)`
    - ensured saved line order remains identical for backward-compatible `Load()` routines.
  - Scope:
    - file-write durability hardening only; strategy behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.1 - Atomic File Writes (Incremental Adoption #350)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.1
- **Changes:**
  - Replayed atomic-write hardening on remaining robot save paths:
    - `project/OsEngine/Robots/BotCreateUi2.xaml.cs`
    - `project/OsEngine/Robots/Helpers/TaxPayer.cs`
    - `project/OsEngine/Robots/Helpers/PayOfMarginBot.cs`
  - Replacements:
    - `StreamWriter(..., false)` and `File.WriteAllText(...)` -> `SafeFileWriter.WriteAllLines(...)` / `SafeFileWriter.WriteAllText(...)`
  - Scope:
    - file-write durability hardening only; JSON/txt payload formats unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.1 - Atomic File Writes (Incremental Adoption #351)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.1
- **Changes:**
  - Replayed atomic-write hardening on additional persistence paths:
    - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/AscendexSpotServer.cs`
    - `project/OsEngine/Market/Servers/TelegramNews/TelegramNewsServer.cs`
    - `project/OsEngine/OsData/OsDataSet.cs`
  - Replacements:
    - direct `File.WriteAllText(...)` -> `SafeFileWriter.WriteAllText(...)` in Ascendex/Telegram.
    - multiple one-shot non-append `StreamWriter(..., false)` / `File.WriteAllText(...)` paths in `OsDataSet` -> `SafeFileWriter.WriteAllLines(...)` / `SafeFileWriter.WriteAllText(...)`.
  - Scope:
    - durability hardening only.
    - append/streaming/log-writer paths intentionally unchanged in this increment.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.1 - Atomic File Writes (Incremental Adoption #352)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.1
- **Changes:**
  - Hardened streaming converter output persistence in:
    - `project/OsEngine/OsConverter/OsConverterMaster.cs`
  - `WorkerSpaceStreaming()` now:
    - writes to temp file (`<exit>.tmp`) during conversion
    - atomically swaps result to final file:
      - `File.Replace(temp, target, target + ".bak", true)` when target exists
      - `File.Move(temp, target)` when target is new
    - cleans temp file in `finally`.
  - Added compatible flush sequence before swap:
    - `FileStream.Flush(true)` when available
    - fallback `Stream.Flush()` otherwise.
  - Scope:
    - durability hardening only; conversion algorithm/output unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #356)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed persistence culture hardening in MarketMaker/Patterns robots:
    - `project/OsEngine/Robots/MarketMaker/MarketMakerBot.cs`
    - `project/OsEngine/Robots/MarketMaker/PairTraderSimple.cs`
    - `project/OsEngine/Robots/MarketMaker/PairTraderSpreadSma.cs`
    - `project/OsEngine/Robots/Patterns/PivotPointsRobot.cs`
  - Replacements:
    - decimal save values: `ToString()` -> `ToString(CultureInfo.InvariantCulture)`
    - decimal load values from settings files: `Convert.ToDecimal(reader.ReadLine())` -> `reader.ReadLine().ToDecimal()`
    - `PairTraderSimple` spread payload value: `Convert.ToDecimal(pos[1])` -> `pos[1].ToDecimal()`
  - Scope:
    - persistence serialization/parsing hardening only; trading logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #357)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed persistence parsing hardening in:
    - `project/OsEngine/Market/Servers/Entity/ServerParameter.cs`
  - Replacement:
    - `ServerParameterDecimal.LoadFromStr()` fallback `Convert.ToDecimal(values[2])` -> `values[2].ToDecimal()`
  - Scope:
    - persistence parsing hardening only; serialization format unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #358)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed decimal parsing fallback hardening in:
    - `project/OsEngine/Journal/JournalUi2.xaml.cs`
    - `project/OsEngine/OsTrader/RiskManager/RiskManager.cs`
  - Replacement:
    - helper fallback `Convert.ToDecimal(value)` -> `value.ToDecimal()`
  - Scope:
    - parsing hardening only; primary Invariant/Current `TryParse` flow unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #359)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed decimal parsing fallback hardening in indicator legacy-load helpers:
    - `project/OsEngine/Charts/CandleChart/Indicators/DynamicTrendDetector.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/KalmanFilter.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/Envelops.cs`
  - Replacement:
    - helper fallback `Convert.ToDecimal(value)` -> `value.ToDecimal()`
  - Scope:
    - parsing hardening only; indicator logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #360)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed persistence decimal parsing hardening in:
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Replacement:
    - optional dop-settings parse `Convert.ToDecimal(array[i][6])` -> `array[i][6].ToDecimal()`
  - Scope:
    - parsing hardening only; settings schema/behavior unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #361)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed decimal parsing hardening in:
    - `project/OsEngine/Market/Servers/AServerParameterUi.xaml.cs`
  - Replacement:
    - `SaveParam()` decimal input parse `Convert.ToDecimal(str.Replace(...))` -> `str.ToDecimal()`
  - Cleanup:
    - removed unused `using System.Globalization;`.
  - Scope:
    - UI decimal parsing hardening only; persistence format unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #362)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed persistence decimal serialization hardening in:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
  - Replacements in `Save()`:
    - decimal `.ToString()` -> `.ToString(CultureInfo.InvariantCulture)` for optimizer settings numeric fields.
  - Scope:
    - serialization hardening only; load flow and settings schema unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #363)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed decimal parse hardening across UI input handlers:
    - `project/OsEngine/OsTrader/RiskManager/RiskManagerUi.xaml.cs`
    - `project/OsEngine/Robots/CounterTrend/WilliamsRangeTradeUi.xaml.cs`
    - `project/OsEngine/Robots/CounterTrend/StrategyBollingerUi.xaml.cs`
    - `project/OsEngine/Robots/CounterTrend/RsiContrtrendUi.xaml.cs`
    - `project/OsEngine/Robots/Trend/SmaStochasticUi.xaml.cs`
    - `project/OsEngine/Robots/Trend/PriceChannelTradeUi.xaml.cs`
    - `project/OsEngine/Robots/Patterns/PivotPointsRobotUi.xaml.cs`
    - `project/OsEngine/Robots/MarketMaker/PairTraderSpreadSmaUi.xaml.cs`
  - Replacements:
    - `Convert.ToDecimal(TextBox...Text)` -> `TextBox...Text.ToDecimal()`
  - Scope:
    - UI parsing hardening only; runtime trading logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #364)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed decimal parsing hardening in indicator UI editors:
    - `project/OsEngine/Charts/CandleChart/Indicators/BollingerUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/KalmanFilterUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/EnvelopsUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/AtrChannelUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/DynamicTrendDetectorUi.xaml.cs`
  - Replacements:
    - `Convert.ToDecimal(TextBox...Text)` -> `TextBox...Text.ToDecimal()`
    - `EnvelopsUi` parse assignment `decimal.TryParse(...)` -> `ToDecimal()`
  - Scope:
    - UI input parsing hardening only; indicator logic unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #365)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed decimal textbox parsing hardening in:
    - `project/OsEngine/Entity/MarketDepthPainter.cs`
    - `project/OsEngine/Entity/PositionUi.xaml.cs`
    - `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
  - Replacements:
    - `Convert.ToDecimal(...Text...)` -> `...Text.ToDecimal()`
  - Scope:
    - UI/input parsing hardening only; runtime behavior unchanged for valid values.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #366)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed table-rate decimal parsing hardening in:
    - `project/OsEngine/Robots/Helpers/TaxPayer.cs`
    - `project/OsEngine/Robots/Helpers/PayOfMarginBot.cs`
  - Replacements:
    - `decimal.TryParse(value.Replace(".", ","), out rate)` and similar -> `(... ?? string.Empty).ToDecimal()`
  - Scope:
    - parsing hardening only; runtime logic and stored JSON structure unchanged.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #367)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Replayed decimal serialization hardening in screener/algo settings DTO save methods:
    - `project/OsEngine/Robots/AlgoStart/AlgoStart2Soldiers.cs`
    - `project/OsEngine/Robots/Grids/GridScreenerAdaptiveSoldiers.cs`
    - `project/OsEngine/Robots/Screeners/ThreeSoldierAdaptiveScreener.cs`
    - `project/OsEngine/Robots/Screeners/PinBarVolatilityScreener.cs`
  - Replacements:
    - decimal concatenation in `GetSaveString()` -> invariant formatted decimal strings.
  - Scope:
    - persistence serialization hardening only; load compatibility preserved.
- **Verification:**
  - Executed by project rule outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #368)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Removed culture-dependent decimal separator normalization in:
    - `project/OsEngine/Charts/CandleChart/WinFormsChartPainter.cs`
  - Replacements:
    - `openS/highS/lowS/closeS.Replace(".", ",")` + split-by-comma branches
    - -> separator-agnostic fraction-length calculation via new helper `GetFractionLength(string value)` used by `GetCandlesDecimal(...)`.
  - Scope:
    - decimal precision detection hardening only; chart behavior and persistence formats unchanged.
- **Verification:**
  - Attempted in current sandbox with local `DOTNET_CLI_HOME`.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` failed (`NU1301`: TLS/auth handshake failure to `https://api.nuget.org/v3/index.json`).
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` failed with same `NU1301`.
  - `dotnet build` / `dotnet test` not considered valid in this environment after restore failure.
- **Commit:** n/a (not committed in this session)
- **Push:** n/a


### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #429)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Removed residual replace-based parsing in:
    - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
    - `project/OsEngine/Market/Servers/OKX/OkxServer.cs`
    - `project/OsEngine/Market/Servers/OKXData/OKXDataServer.cs`
    - `project/OsEngine/OsData/OsDataSet.cs`
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Replacements:
    - string normalization via `Replace(',', '.')` removed from decimal parsing paths.
    - HTX parsing paths migrated to separator-neutral trim + `ToDecimal()`.
    - OKX/OKXData decimals-volume detection migrated to last-separator index logic.
    - OsDataSet/Tester parsing now uses explicit parse-cascade helpers (`Invariant -> Current -> ru-RU`).
  - Scope:
    - parsing hardening only; runtime behavior preserved for valid numeric payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
  - `rg -n -F "Replace(',', '.')" project/OsEngine` -> no matches.
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #430)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Removed replace-based decimal normalization in:
    - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
    - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
    - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/KuCoinSpotServer.cs`
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServer.cs`
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/BitfinexFuturesServer.cs`
  - Replacements:
    - `value.Replace('.', ',').ToDecimal()` -> `value.ToDecimal()` in incoming payload parsing.
    - BingX outbound numeric fields moved from `ToString().Replace(",", ".")` to invariant formatting.
    - Bitfinex digit-count helper migrated to separator-agnostic logic without pre-normalization.
  - Scope:
    - parsing/serialization hardening only; domain logic unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
  - `rg -n -F "Replace('.', ',')" project/OsEngine` -> no matches.
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #431)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Removed replace-based parse normalization in:
    - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
    - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
    - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/KuCoinSpotServer.cs`
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServer.cs`
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/BitfinexFuturesServer.cs`
  - Additional hardening:
    - `project/OsEngine/Market/Servers/Deribit/DeribitServer.cs` (`price` assignments simplified to direct decimal).
    - `project/OsEngine/Market/Servers/Transaq/TransaqServer.cs` (`volume` serialization switched to invariant string).
  - Replacements:
    - `value.Replace('.', ',').ToDecimal()` -> `value.ToDecimal()`
    - locale-sensitive outbound decimal strings -> invariant formatting where applicable.
  - Scope:
    - parsing/formatting hardening only; connector logic unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
  - `rg -n -F "Replace('.', ',')" project/OsEngine` -> no matches.
  - `rg -n -F "Replace(',', '.')" project/OsEngine` -> no matches.
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #432)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened UI double parsing in:
    - `project/OsEngine/Charts/CandleChart/Indicators/ParabolicSARUi.xaml.cs`
    - `project/OsEngine/OsTrader/AvailabilityServer/ServerAvailabilityUi.xaml.cs`
  - Replacements:
    - `Convert.ToDouble(...)` -> `ToDouble()` for user-entered values.
  - Scope:
    - UI parsing hardening only; behavior unchanged for valid input.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #433)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Internal cleanup in `project/OsEngine/Market/Servers/Tester/TesterServer.cs`:
    - removed duplicated `TryParseDecimalInvariantOrCurrent(...)` helper in `SecurityTester` scope.
    - `ParseVolumeStepFromComment(...)` switched to `ToDecimal()` + zero-guard with existing fallback logic.
  - Scope:
    - parser simplification only; runtime behavior unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #434)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Consolidated decimal parsing onto shared extension and removed duplicated local helpers in:
    - `project/OsEngine/Charts/CandleChart/Indicators/Envelops.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/DynamicTrendDetector.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/KalmanFilter.cs`
    - `project/OsEngine/Journal/JournalUi2.xaml.cs`
    - `project/OsEngine/OsTrader/RiskManager/RiskManager.cs`
    - `project/OsEngine/Market/Servers/Polygon/PolygonServer.cs`
  - Replacements:
    - local `ParseDecimalInvariantOrCurrent(...)` calls -> `ToDecimal()`.
    - removed helper method definitions whose fallback semantics duplicated `ToDecimal()`.
  - Scope:
    - parser/helper consolidation only; runtime behavior unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #435)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened culture-neutral parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/Binance/Futures/BinanceServerFutures.cs`
  - Replacements:
    - `Convert.ToDouble(...)` on string timestamp fields -> `ToDouble()`.
    - retained numeric `long` timestamp conversion (`trades.data.T`) with `Convert.ToDouble(...)`.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #436)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened culture-neutral timestamp parsing in:
    - `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs`
  - Replacements:
    - `Convert.ToDouble(jtTrade.T)` -> `jtTrade.T.ToDouble()` for string payload timestamp.
    - kept numeric `long` websocket timestamp conversion (`trades.data.T`) unchanged.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #437)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened culture-neutral timestamp parsing in:
    - `project/OsEngine/Market/Servers/Woo/WooServer.cs`
  - Replacements:
    - `Convert.ToDouble(timestamp)` -> `timestamp.ToDouble()` in `UnixTimeMilliseconds(...)`.
  - Scope:
    - parser hardening only; runtime behavior unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #438)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/BitGetServerSpot.cs`
    - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/BitGetServerFutures.cs`
    - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
    - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #439)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/KuCoinSpotServer.cs`
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServer.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #440)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/HTX/Spot/HTXSpotServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #441)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/OKX/OkxServer.cs`
    - `project/OsEngine/Market/Servers/OKXData/OKXDataServer.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #442)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/BitMartSpotServer.cs`
    - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/BitMartFutures.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #443)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/Pionex/PionexServerSpot.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #444)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/Woo/WooServer.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #445)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/BloFin/BloFinFutures/BloFinFuturesServer.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #446)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/XT/XTSpot/XTServerSpot.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #447)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/XT/XTFutures/XTFuturesServer.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #448)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/AscendexSpotServer.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #449)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/BitfinexFuturesServer.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #450)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/Bybit/BybitServer.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #451)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/ExMo/ExmoSpot/ExmoSpotServer.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #452)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/Mexc/MexcSpot/MexcSpotServer.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #453)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #454)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/GateIoServerFutures.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #455)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/HTX/Spot/HTXSpotServer.cs`
  - Replacements:
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #456)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
  - Replacements:
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #457)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
  - Replacements:
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #458)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/OKX/OkxServer.cs`
  - Replacements:
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #459)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/BitGetServerSpot.cs`
    - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/BitGetServerFutures.cs`
  - Replacements:
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #460)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/GateIoData/GateIoDataServer.cs`
  - Replacements:
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #461)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/BitGetData/BitGetDataServer.cs`
  - Replacements:
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #462)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/Polygon/PolygonServer.cs`
  - Replacements:
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #463)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/AServer.cs`
    - `project/OsEngine/Market/Servers/YahooFinance/YahooServer.cs`
    - `project/OsEngine/Market/Servers/TraderNet/TraderNetServer.cs`
  - Replacements:
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #464)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/Deribit/DeribitServer.cs`
  - Replacements:
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #465)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string timestamps in:
    - `project/OsEngine/Market/Servers/BinanceData/BinanceDataServer.cs`
    - `project/OsEngine/Market/Servers/BybitData/BybitDataServer.cs`
    - `project/OsEngine/Market/Servers/OKXData/OKXDataServer.cs`
  - Replacements:
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in timestamp parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #466)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of numeric strings in:
    - `project/OsEngine/Market/Servers/AExchange/AExchangeServer.cs`
    - `project/OsEngine/Market/Servers/BybitData/BybitDataServer.cs`
    - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/FIXMessage.cs`
  - Replacements:
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in remaining market-server parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #467)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of sequence/order identifiers in:
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
    - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/AscendexSpotServer.cs`
    - `project/OsEngine/Market/Servers/XT/XTFutures/XTFuturesServer.cs`
  - Replacements:
    - `long.Parse(value)` -> `long.Parse(value, CultureInfo.InvariantCulture)` in sequence number parsing path.
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in sequence/order id parsing paths.
    - `.ToString()` -> `.ToString(CultureInfo.InvariantCulture)` for parsed ids in XT futures.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #468)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing/formatting of ids and timestamp query params in:
    - `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs`
    - `project/OsEngine/Market/Servers/Binance/Futures/BinanceServerFutures.cs`
    - `project/OsEngine/Market/Servers/QuikLua/QuikLuaServer.cs`
    - `project/OsEngine/Market/Servers/Plaza/PlazaServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in external string id parsing paths.
    - `.ToString()` -> `.ToString(CultureInfo.InvariantCulture)` for Binance outbound timestamp/id string values.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #469)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of external FED rates timestamp in:
    - `project/OsEngine/OsData/LqdtDataFakeServer.cs`
  - Replacements:
    - `Convert.ToInt64(value)` -> `Convert.ToInt64(value, CultureInfo.InvariantCulture)` in timestamp parsing path.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #470)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for XML `port` parsing and FIX order-user id mapping (`origClOrdId`, `NumUserInSystem`, FIX tag `41`).
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #471)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Spot/HTXSpotServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for client order ids, spot precision fields (`ap`/`pp`) and swap paging fields (`total_page`/`current_page`).
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #472)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int user ids in:
    - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
    - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/GateIoServerFutures.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for values extracted from `text.Replace("t-", "")` in orders and my-trades flows.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #473)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int user ids in:
    - `project/OsEngine/Market/Servers/OKX/OkxServer.cs`
    - `project/OsEngine/Market/Servers/Woo/WooServer.cs`
    - `project/OsEngine/Market/Servers/Deribit/DeribitServer.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for OKX `clOrdId`, Woo `cid`/`clientOrderId`, and Deribit `label`.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #474)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/KuCoinSpotServer.cs`
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServer.cs`
    - `project/OsEngine/Market/Servers/Pionex/PionexServerSpot.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for KuCoin `clientOid`, Pionex `quotePrecision`/`basePrecision`, and Pionex `clientOrderId`.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #475)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
    - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
    - `project/OsEngine/Market/Servers/ExMo/ExmoSpot/ExmoSpotServer.cs`
    - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/BitGetServerSpot.cs`
    - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/BitGetServerFutures.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for:
      - BingX `clientOrderID`/`clientOrderId` and precision fields (`pricePrecision`, `quantityPrecision`).
      - ExMo `client_id` and `price_precision`.
      - BitGet `clientOid` and precision fields (`pricePrecision`, `quantityPrecision`, `pricePlace`, `priceEndStep`, `volumePlace`).
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #476)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Servers/CoinEx/Spot/CoinExServerSpot.cs`
    - `project/OsEngine/Market/Servers/CoinEx/Futures/CoinExServerFutures.cs`
    - `project/OsEngine/Market/Servers/KiteConnect/KiteConnectServer.cs`
    - `project/OsEngine/Market/Servers/Mexc/MexcSpot/MexcSpotServer.cs`
    - `project/OsEngine/Market/Servers/XT/XTSpot/XTServerSpot.cs`
    - `project/OsEngine/Market/Servers/XT/XTFutures/XTFuturesServer.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for:
      - CoinEx `client_id` and precision fields (`quote_ccy_precision`, `base_ccy_precision`).
      - KiteConnect `userNumber` mapping into order `NumberUser`.
      - Mexc `clientId`/`clientOrderId` and precision fields (`quoteAssetPrecision`, `baseAssetPrecision`).
      - XT spot/futures client order ids and precision fields (`pricePrecision`, `quantityPrecision`).
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #477)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs`
    - `project/OsEngine/Market/Servers/Binance/Futures/BinanceServerFutures.cs`
    - `project/OsEngine/Market/Servers/Bybit/BybitServer.cs`
    - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/BitMartSpotServer.cs`
    - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/BitMartFutures.cs`
    - `project/OsEngine/Market/Servers/Transaq/TransaqServer.cs`
    - `project/OsEngine/Market/Servers/QuikLua/QuikLuaServer.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for:
      - Binance spot/futures client order ids.
      - Bybit `orderLinkId` / `numUser`.
      - BitMart spot/futures `client_order_id`, `clientOrderId`, and side/action fields.
      - Transaq `Transactionid` and security decimals.
      - QuikLua `TransID` order mapping.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #478)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/BitfinexFuturesServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
    - `project/OsEngine/Market/Servers/InteractiveBrokers/IbClient.cs`
    - `project/OsEngine/Market/Servers/Plaza/PlazaServer.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for:
      - Bitfinex websocket channel ids and order/user id mapping values.
      - MoexFixFastTwimeFutures security-id conversion from string mappings.
      - InteractiveBrokers order lookup by `NumberMarket`.
      - Plaza security-id conversion from `NameId`.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #479)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
  - Replacements:
    - `int.Parse(value)` -> `int.Parse(value, CultureInfo.InvariantCulture)` for:
      - XML/FIX port values.
      - FIX message integer fields (`SessionStatus`, `SecondaryClOrdID`).
      - security decimals (`secDecimals`).
      - FAST template ids (`tmplt.Id`).
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #480)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Servers/GateIoData/GateIoDataServer.cs`
    - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
    - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/GateIoServerFutures.cs`
    - `project/OsEngine/Market/Servers/BinanceData/BinanceDataServer.cs`
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServer.cs`
    - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
    - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/BitMartFutures.cs`
    - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
    - `project/OsEngine/Market/Servers/OKX/OkxServer.cs`
    - `project/OsEngine/Market/Servers/CoinEx/Futures/CoinExServerFutures.cs`
    - `project/OsEngine/Market/Servers/Woo/WooServer.cs`
  - Replacements:
    - `int.Parse(value)` -> `int.Parse(value, CultureInfo.InvariantCulture)` for:
      - timestamp fields from external payloads.
      - funding interval fields (`Hours`, `funding_interval`, `fundingRateGranularity`, etc.).
      - BinanceData archive date/month extraction fields.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #481)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Servers/Alor/AlorServer.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for:
      - cancellation date fragments (`year/month/day`).
      - `baseMessage.comment` mapped to `order.NumberUser`.
      - date/time fragment parsing in conversion helper (`year/month/day/hour/minute/second/ms`).
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #482)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Servers/Finam/FinamServer.cs`
    - `project/OsEngine/Market/Servers/Atp/AtpServer.cs`
    - `project/OsEngine/Market/Servers/NinjaTrader/NinjaTraderClient.cs`
  - Replacements:
    - Finam: introduced `marketId = Convert.ToInt32(arrayMarkets[i], CultureInfo.InvariantCulture)` and switched repeated market-id comparisons to `marketId`.
    - Atp: `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in order id and date/time parsing paths.
    - NinjaTrader: `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for incoming `NumberUser` mappings.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #483)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Servers/QuikLua/QuikLuaServer.cs`
    - `project/OsEngine/Market/Servers/TraderNet/TraderNetServer.cs`
  - Replacements:
    - QuikLua:
      - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for expiration fragments, `Scale`, `Qty`, `Flags`, `OpenInterest`.
    - TraderNet:
      - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for `instr_type_c` and MD index fields (`del/ins/upd[*].k`).
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #484)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based numeric values in:
    - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/AscendexSpotServer.cs`
    - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/BitGetServerFutures.cs`
    - `project/OsEngine/Market/Servers/MoexAlgopack/MoexAlgopackServer.cs`
  - Replacements:
    - AscendEX: `Convert.ToInt32(priceScale/qtyScale)` -> `Convert.ToInt32(..., CultureInfo.InvariantCulture)`.
    - BitGet futures: `int.Parse(item.fundingRateInterval)` -> `int.Parse(item.fundingRateInterval, CultureInfo.InvariantCulture)`.
    - MoexAlgopack: `Convert.ToInt32(item[5|8])` -> `Convert.ToInt32(..., CultureInfo.InvariantCulture)` for security decimals.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #485)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based settings values in:
    - `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorageUi.xaml.cs`
    - `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorage.cs`
    - `project/OsEngine/Market/Servers/Tester/TesterServerUi.xaml.cs`
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
  - Replacements:
    - `Convert.ToInt32(TextBox*.Text)` -> `Convert.ToInt32(..., CultureInfo.InvariantCulture)`
    - `int.Parse(values[i])` -> `int.Parse(..., CultureInfo.InvariantCulture)`
    - `Convert.ToInt32(array[i][...])` -> `Convert.ToInt32(..., CultureInfo.InvariantCulture)`
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #486)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based values in:
    - `project/OsEngine/Market/Servers/ComparePositionsModuleUi.xaml.cs`
    - `project/OsEngine/Market/Servers/Atp/SecuritiesAtpUi.xaml.cs`
    - `project/OsEngine/Market/Servers/Entity/ServerParameter.cs`
    - `project/OsEngine/Market/Servers/MetaTrader5/MetaTrader5Server.cs`
    - `project/OsEngine/Market/Servers/MOEX/MoexIssDataServer.cs`
    - `project/OsEngine/Market/Servers/TelegramNews/TelegramNewsServer.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for UI/grid values, persisted parameter values, and string-derived id fragments.
    - `int.Parse(value)` -> `int.Parse(value, CultureInfo.InvariantCulture)` for regex-derived Telegram interval values.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #487)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Servers/AServerParameterUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(str)` -> `Convert.ToInt32(str, CultureInfo.InvariantCulture)` in server parameter UI int value mapping.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #488)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based values in:
    - `project/OsEngine/OsData/OsDataSet.cs`
  - Replacements:
    - `Convert.ToInt32(saveArray[22])` -> `Convert.ToInt32(..., CultureInfo.InvariantCulture)`
    - `Convert.ToInt32(reader.ReadLine())` -> `Convert.ToInt32(..., CultureInfo.InvariantCulture)`
    - `int.Parse(datesStr[2])` -> `int.Parse(..., CultureInfo.InvariantCulture)`
    - `Convert.ToInt32(setParts[2])` -> `Convert.ToInt32(..., CultureInfo.InvariantCulture)`
    - `int.Parse(setParts[2])` -> `int.Parse(..., CultureInfo.InvariantCulture)`
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #489)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based values in:
    - `project/OsEngine/OsData/OsDataSetUi.xaml.cs`
    - `project/OsEngine/OsData/OsDataSetPainter.cs`
    - `project/OsEngine/OsData/SetDuplicationUi.xaml.cs`
    - `project/OsEngine/OsData/SetUpdatingUi.xaml.cs`
    - `project/OsEngine/OsData/NewSecurityUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)`
    - `int.Parse(value)` -> `int.Parse(value, CultureInfo.InvariantCulture)`
    - applied to UI textbox/combo/label/grid string parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #490)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Entity/StrategyParameter.cs`
    - `project/OsEngine/Journal/Internal/PositionController.cs`
    - `project/OsEngine/OsTrader/Grids/TradeGrid.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPair.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in settings/load and grid parsing paths.
    - Applied to `save[]/array[]/values[]/lines[]` and grid cell value parsing.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #491)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/OsTrader/GlobalPositionViewer.cs`
    - `project/OsEngine/Entity/SecuritiesUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in UI/grid parsing paths.
    - Applied to row ids, selected position ids, and label/grid precision values.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #492)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Journal/JournalUi.xaml.cs`
    - `project/OsEngine/Journal/JournalUi2.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in grid/string parsing paths.
    - Applied to position ids from data-grid cells and split-string range values (`selectNum/selectNums`).
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #493)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/OsTrader/Grids/TradeGridUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in UI textbox/grid parsing paths.
    - Applied to `Fail*`, `WaitSeconds`, `LineCountStart`, `StopBy*`, `AutoStarter*`, `Max*`, `DelayInReal`, and grid row number parsing.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #494)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/OsOptimizer/OptimizerUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in optimizer UI text/grid parsing paths.
    - Applied to iteration/filter/start-deposit fields, Bayesian integer fields, and optimizer parameter grid values.
    - Kept computed non-string numeric casts unchanged (`sortedValue.Count * ...`).
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #495)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/OsTrader/Panels/Tab/Internal/PositionOpenUi2.xaml.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/Internal/BotManualControlUi.xaml.cs`
    - `project/OsEngine/OsTrader/Grids/TradeGridStopBy.cs`
    - `project/OsEngine/OsTrader/Grids/TradeGridAutoStarter.cs`
    - `project/OsEngine/OsTrader/Grids/TradeGridErrorsReaction.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for UI text parsing and `values[]` settings parsing.
    - Applied to lifetime/time fields, manual control seconds, stop/autostart/error reaction settings.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #496)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
    - `project/OsEngine/OsOptimizer/OptimizerMaster.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs`
  - Replacements:
    - OptimizerSettings: string reads from config persisted file (`reader.ReadLine()`).
    - OptimizerMaster: string-based `str[]` persisted values.
    - BotTabIndex: parsed values from `Split('A')`.
    - All updated to `Convert.ToInt32(..., CultureInfo.InvariantCulture)`.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #499)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Alerts/AlertToPriceCreateUi.xaml.cs`
    - `project/OsEngine/Alerts/AlertToPrice.cs`
    - `project/OsEngine/Alerts/AlertToChartCreateUi.xaml.cs`
    - `project/OsEngine/Alerts/AlertToChart.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in textbox and legacy alert settings parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #500)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/Proxy/ProxyOsa.cs`
    - `project/OsEngine/Market/Proxy/ProxyMaster.cs`
    - `project/OsEngine/Market/Proxy/ProxyMasterUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)`/`int.Parse(value)` -> invariant-culture `Convert.ToInt32(..., CultureInfo.InvariantCulture)` for persisted proxy settings and UI/grid values.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #501)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Entity/Order.cs`
    - `project/OsEngine/Entity/Position.cs`
    - `project/OsEngine/Entity/PositionOpenerToStop.cs`
    - `project/OsEngine/Entity/Trade.cs`
    - `project/OsEngine/Entity/Security.cs`
    - `project/OsEngine/Entity/NumberGen.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for persisted `Split(...)` values.
    - `int.TryParse(value, out ...)` -> `int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ...)` in legacy settings parsing.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #502)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalizeUi.xaml.cs`
    - `project/OsEngine/OsOptimizer/OptimizerReportCharting.cs`
    - `project/OsEngine/OsOptimizer/OptimizerReport.cs`
    - `project/OsEngine/Indicators/IndicatorParameter.cs`
    - `project/OsEngine/Indicators/IndicatorDataSeries.cs`
    - `project/OsEngine/Entity/MarketDepth.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in textbox, serialized settings, and persisted array parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #503)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/AutoFollow/CopyTrader.cs`
    - `project/OsEngine/Market/AutoFollow/CopyPortfolioUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for saved settings arrays (`save[]/lines[]`) and UI/grid parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #504)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPairCommonSettingsUi.xaml.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPairUi.xaml.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygonUi.xaml.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygonCommonSettingsUi.xaml.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabIndexUi.xaml.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPairAutoSelectPairsUi.xaml.cs`
    - `project/OsEngine/Robots/AutoTestBots/TestBotConnectionParams.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in textbox/combobox/label value parsing.
    - `Int32.TryParse(value, out ...)` -> `Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ...)` in validation path.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #505)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Robots/Trend/WsurfBot.cs`
    - `project/OsEngine/Robots/MarketMaker/PairTraderSimple.cs`
    - `project/OsEngine/Robots/CounterTrend/RsiContrtrendUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in settings load and UI validation paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #506)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Market/ServerMaster.cs`
    - `project/OsEngine/OsTrader/Grids/TradeGridsMaster.cs`
    - `project/OsEngine/Charts/CandleChart/ChartCandleMaster.cs`
    - `project/OsEngine/Robots/MarketMaker/PairTraderSimpleUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in legacy settings, grid row id parsing, tooltip parsing, and UI textbox parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #507)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Charts/CandleChart/Indicators/Adx.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/AdaptiveLookBack.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/AdxUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/AdaptiveLookBackUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in legacy indicator settings parsing and UI textbox validation/assignment.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #508)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Charts/CandleChart/Indicators/Ac.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/AcUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/Atr.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/AtrUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/AtrChannel.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/AtrChannelUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in legacy indicator settings parsing and UI textbox validation/assignment.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #509)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Charts/CandleChart/Indicators/AccumulationDistribution.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/BfMfi.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/BearsPower.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/BearsPowerUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/BullsPower.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/BullsPowerUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in legacy indicator settings parsing and UI textbox validation/assignment.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #510)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Charts/CandleChart/Indicators/Alligator.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/AlligatorUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/AwesomeOscillator.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/AwesomeOscillatorUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/Bollinger.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/BollingerUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/CCI.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/CCIUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/Cmo.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/CmoUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/DonchianChannel.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/DonchianChannelUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/DynamicTrendDetector.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/DynamicTrendDetectorUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/EfficiencyRatio.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/EfficiencyRatioUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/Envelops.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/ForceIndex.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/ForceIndexUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/Fractail.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/KalmanFilter.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/Line.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/LinearRegressionCurve.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/LinearRegressionCurveUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/MacdHistogram.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/MacdLine.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/Momentum.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/MomentumUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/MoneyFlowIndex.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/MoneyFlowIndexUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in legacy indicator settings parsing and UI textbox validation/assignment.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #511)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Charts/CandleChart/Indicators/Ishimoku.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/IshimokuUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/IvashovRange.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/IvashovRangeUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/MovingAverage.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/MovingAverageUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/OnBalanceVolume.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/ParabolicSAR.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/PriceChannel.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/PriceChannelUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/PriceOscillator.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/Roc.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/RocUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/Rsi.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/RsiUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/Rvi.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/RviUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/SimpleVWAP.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/StandardDeviation.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/StandardDeviationUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/StochasticOscillator.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/StochasticOscillatorUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/StochRsi.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/StochRsiUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/TickVolume.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/TradeThread.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/Trix.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/TrixUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/UltimateOscillator.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/UltimateOscillatorUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/VerticalHorizontalFilter.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/VerticalHorizontalFilterUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/Volume.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/VolumeOscillator.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/VolumeOscillatorUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/VWAP.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/WilliamsRange.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/WilliamsRangeUi.xaml.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/Pivot.cs`
    - `project/OsEngine/Charts/CandleChart/Indicators/PivotPoints.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in legacy indicator settings parsing and UI textbox validation/assignment.
    - `int.TryParse(value, out ...)` -> `int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ...)` in legacy numeric parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #512)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing of string-based int values in:
    - `project/OsEngine/Logging/ServerSms.cs`
    - `project/OsEngine/OsTrader/Panels/BotPanel.cs`
    - `project/OsEngine/Market/AutoFollow/CopyMasterUi.xaml.cs`
    - `project/OsEngine/Entity/PositionUi.xaml.cs`
    - `project/OsEngine/Entity/StrategyParametersUi.xaml.cs`
    - `project/OsEngine/Indicators/AIndicatorUi.xaml.cs`
    - `project/OsEngine/Candles/Factory/CandleSeriesParameter.cs`
    - `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`
    - `project/OsEngine/Charts/ColorKeeper/ChartMasterColorKeeper.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygon.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreener.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreenerUi.xaml.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabOptions.cs`
    - `project/OsEngine/OsData/DataPrunerUi.xaml.cs`
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
    - `project/OsEngine/OsTrader/SystemAnalyze/SystemAnalyzeMaster.cs`
    - `project/OsEngine/Market/Servers/ComparePositionsModule.cs`
    - `project/OsEngine/OsOptimizer/OptimizerReport.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabIndex.cs`
    - `project/OsEngine/Layout/GlobalGUILayout.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in string-based integer parsing paths.
    - `int.TryParse(value, out ...)` / `Int32.TryParse(value, out ...)` -> `TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ...)` in legacy integer parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
  - Fixup:
    - corrected accidental malformed calls introduced by mechanical replacement:
      - `ToString(, CultureInfo.InvariantCulture)` -> `ToString(), CultureInfo.InvariantCulture` in `Convert.ToInt32(...)` calls.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #513)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened legacy integer parsing by replacing bare `int/Int32.TryParse(value, out ...)` with explicit invariant overload in:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
    - `project/OsEngine/Robots/Helpers/TaxPayer.cs`
    - `project/OsEngine/Robots/Helpers/PayOfMarginBot.cs`
    - `project/OsEngine/Market/Servers/Atp/AtpServer.cs`
    - `project/OsEngine/Market/Servers/Binance/Futures/BinanceServerFutures.cs`
    - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/BitGetServerSpot.cs`
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
    - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/BitGetServerFutures.cs`
    - `project/OsEngine/Market/Servers/Bybit/BybitServer.cs`
    - `project/OsEngine/Market/Servers/BitGetData/BitGetDataServer.cs`
    - `project/OsEngine/Market/Servers/BloFin/BloFinFutures/BloFinFuturesServer.cs`
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/BitfinexFuturesServer.cs`
    - `project/OsEngine/Market/Servers/FinamGrpc/FinamGrpcServer.cs`
    - `project/OsEngine/Market/Servers/InteractiveBrokers/InteractiveBrokersServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
    - `project/OsEngine/Market/Servers/Transaq/TransaqServer.cs`
    - `project/OsEngine/Market/Servers/TraderNet/TraderNetServer.cs`
  - Replacements:
    - `int.TryParse(value, out ...)` / `Int32.TryParse(value, out ...)` -> `TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out ...)`.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #514)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing for string/object-backed integer conversions in:
    - `project/OsEngine/Entity/SetLeverageUi.xaml.cs`
    - `project/OsEngine/Market/ServerMasterSourcesPainter.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPoligonSecurityAddUi.xaml.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabPolygonAutoSelectSequenceUi.xaml.cs`
    - `project/OsEngine/Robots/Trend/SmaStochasticUi.xaml.cs`
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerReportSerializer.cs`
    - `project/OsEngine/Entity/Extensions.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/Internal/PositionCloseUi2.xaml.cs`
    - `project/OsEngine/Market/Servers/AstsBridge/AstsBridgeServer.cs`
    - `project/OsEngine/Market/Servers/AstsBridge/AstsServerUi.xaml.cs`
    - `project/OsEngine/Robots/TechSamples/CustomTableInTheParamWindowSample.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` in UI paging/input parsing and persisted string parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #515)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant parsing for UI pagination and page-size conversions in:
    - `project/OsEngine/Market/ServerMasterOrdersPainter.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for `Label.Content` and `ComboBox.SelectedValue` parsing paths.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #516)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened legacy integer string parsing by replacing bare `int.Parse/Int32.Parse` with explicit invariant overload in:
    - `project/OsEngine/Attributes/ParameterElementAttribute.cs`
    - `project/OsEngine/Attributes/Types/LabelAttribute.cs`
    - `project/OsEngine/Attributes/Types/ParameterAttribute.cs`
    - `project/OsEngine/Market/Servers/InteractiveBrokers/IbClient.cs`
    - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
  - Replacements:
    - `int.Parse(value)` / `Int32.Parse(value)` -> `Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture)`.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #517)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened remaining object-backed `Convert.ToInt32` parsing paths with explicit invariant culture in:
    - `project/OsEngine/MainWindow.xaml.cs`
    - `project/OsEngine/OsTrader/BuyAtStopPositionsViewer.cs`
    - `project/OsEngine/Market/Servers/Tester/GoToUi.xaml.cs`
  - Replacements:
    - `Convert.ToInt32(value)` -> `Convert.ToInt32(value, CultureInfo.InvariantCulture)` for registry/object/UI-value conversions.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #518)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened `Int16.Parse` with explicit invariant culture in:
    - `project/OsEngine/Market/Servers/CoinEx/Spot/CoinExServerSpot.cs`
    - `project/OsEngine/Market/Servers/CoinEx/Futures/CoinExServerFutures.cs`
  - Replacements:
    - `Int16.Parse(value)` -> `Int16.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture)`.
  - Scope:
    - parser hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #519)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened persistence formatting with explicit invariant culture in core entity/candle serialization:
    - `project/OsEngine/Entity/Order.cs`
    - `project/OsEngine/Entity/Position.cs`
    - `project/OsEngine/Entity/Trade.cs`
    - `project/OsEngine/Candles/Candle.cs`
  - Replacements:
    - object/int serialization via `ToString(InvariantCulture)` for saved numeric tokens.
    - date serialization for persisted candle/trade time via `ToString("yyyyMMdd,HHmmss", CultureInfo.InvariantCulture)`.
  - Scope:
    - persistence formatting hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #520)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened additional persistence formatting with explicit invariant culture in:
    - `project/OsEngine/Entity/StrategyParameter.cs`
    - `project/OsEngine/Entity/Position.cs`
    - `project/OsEngine/Entity/MarketDepth.cs`
  - Replacements:
    - `TimeOfDay.ToString()` numeric tokens now emit invariant digits.
    - `Position.Number` serialization to `MyTrade.NumberPosition` now uses invariant formatting.
    - market depth save-string timestamp and milliseconds use invariant formatting.
  - Scope:
    - persistence formatting hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #521)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened persistence-related date/int string formatting in:
    - `project/OsEngine/OsData/OsDataSet.cs`
  - Replacements:
    - explicit invariant formatting for saved counters and date-based file/temp identifiers:
      - `ToString()` -> `ToString(CultureInfo.InvariantCulture)` for persisted counts.
      - `ToString("yyyyMMdd")` / `ToString("yyyy-MM-dd")` -> overloads with `CultureInfo.InvariantCulture`.
  - Scope:
    - persistence formatting hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #522)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened additional optimizer persistence numeric formatting in:
    - `project/OsEngine/OsOptimizer/OptimizerMaster.cs`
    - `project/OsEngine/OsOptimizer/OptimizerReport.cs`
  - Replacements:
    - `ToString()` on persisted integer fields -> `ToString(CultureInfo.InvariantCulture)`.
  - Scope:
    - persistence formatting hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #523)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened persistence string formatting in `TradeGrid` settings serialization:
    - `project/OsEngine/OsTrader/Grids/TradeGrid.cs`
    - `project/OsEngine/OsTrader/Grids/TradeGridAutoStarter.cs`
    - `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`
    - `project/OsEngine/OsTrader/Grids/TradeGridErrorsReaction.cs`
    - `project/OsEngine/OsTrader/Grids/TradeGridStopAndProfit.cs`
    - `project/OsEngine/OsTrader/Grids/TradeGridStopBy.cs`
    - `project/OsEngine/OsTrader/Grids/TrailingUp.cs`
  - Replacements:
    - numeric persistence fields in `GetSaveString()`/`GetSaveStr()` methods now use `ToString(CultureInfo.InvariantCulture)`.
  - Scope:
    - persistence formatting hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #524)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit date/time string formatting with invariant culture in server/API and FIX message paths:
    - `project/OsEngine/Logging/ServerWebhook.cs`
    - `project/OsEngine/Market/Servers/BitGetData/BitGetDataServer.cs`
    - `project/OsEngine/Market/Servers/InteractiveBrokers/IbClient.cs`
    - `project/OsEngine/Market/Servers/BinanceData/BinanceDataServer.cs`
    - `project/OsEngine/Market/Servers/GateIoData/GateIoDataServer.cs`
    - `project/OsEngine/Market/Servers/OKXData/OKXDataServer.cs`
    - `project/OsEngine/Market/Servers/TraderNet/TraderNetServer.cs`
    - `project/OsEngine/Market/Servers/MFD/MfdServer.cs`
    - `project/OsEngine/Market/Servers/KiteConnect/KiteConnectServer.cs`
    - `project/OsEngine/Market/Servers/QscalpMarketDepth/QscalpMarketDepthServer.cs`
    - `project/OsEngine/Market/Servers/Polygon/PolygonServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Spot/HTXSpotServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/Entity/MessageConstructor.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/Entity/FixMessageConstructor.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/Header.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/FIX/FASTHeader.cs`
  - Replacements:
    - `DateTime.ToString("...")` -> `DateTime.ToString("...", CultureInfo.InvariantCulture)` for deterministic request/serialization/fix timestamps.
    - `TotalSeconds.ToString()` -> `ToString(CultureInfo.InvariantCulture)` for unix epoch payload stability.
  - Added missing `using System.Globalization;` where required.
  - Scope:
    - persistence/protocol formatting hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #525)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened non-server date/time string formatting with explicit invariant culture in:
    - `project/OsEngine/Market/Connectors/ConnectorCandlesUi.xaml.cs`
    - `project/OsEngine/Market/Connectors/MassSourcesCreateUi.xaml.cs`
    - `project/OsEngine/OsTrader/Panels/Tab/BotTabScreenerUi.xaml.cs`
    - `project/OsEngine/Robots/Patterns/CustomCandlesImpulseTrader.cs`
    - `project/OsEngine/Logging/ServerSms.cs`
    - `project/OsEngine/Robots/Helpers/PayOfMarginBot.cs`
    - `project/OsEngine/OsData/SetUpdatingUi.xaml.cs`
  - Replacements:
    - `DateTime.ToString("...")` -> `DateTime.ToString("...", CultureInfo.InvariantCulture)` for deterministic formatting.
    - `DateTime.Now.Ticks.ToString("x")` -> `ToString("x", CultureInfo.InvariantCulture)` for protocol boundary generation stability.
    - `i.ToString("00")` -> `ToString("00", CultureInfo.InvariantCulture)` for deterministic hour string formatting.
  - Consistency fix in option expiration UI:
    - unified displayed expiration format with parser format (`dd.MM.yyyy`) in connector/screener expiration comboboxes.
  - Scope:
    - formatting hardening and date format consistency only; runtime behavior unchanged for valid payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #526)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened numeric parsing with explicit invariant culture in:
    - `project/OsEngine/Logging/ServerTelegram.cs`
    - `project/OsEngine/Logging/ServerTelegramUi.xaml.cs`
    - `project/OsEngine/OsData/BinaryEntity/DealsStream.cs`
    - `project/OsEngine/Market/Servers/OKXData/Entity/TradeComparer.cs`
    - `project/OsEngine/Market/Servers/Bybit/BybitServer.cs`
  - Replacements:
    - `long.TryParse(value, out x)` -> `long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out x)`.
    - removed redundant `ToString()` before numeric parse in Bybit timestamp validation.
  - Added missing `using System.Globalization;` in files requiring parse overloads.
  - Scope:
    - parse determinism hardening only; runtime behavior unchanged for valid payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #527)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened default date parameter serialization in:
    - `project/OsEngine/Robots/AutoTestBots/ServerTests/AServerTester.cs`
  - Replacements:
    - `DateTime.Now.ToString()` -> `DateTime.Now.ToString("o", CultureInfo.InvariantCulture)` for deterministic round-trip defaults in test bot settings.
  - Scope:
    - date formatting determinism only; runtime behavior unchanged for valid payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #528)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened tick-based ID serialization with explicit invariant culture in:
    - `project/OsEngine/Robots/AutoTestBots/ServerTests/Conn_5_Screener.cs`
    - `project/OsEngine/Market/Servers/BybitData/BybitDataServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
    - `project/OsEngine/Market/Servers/TInvest/TInvestServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
  - Replacements:
    - `DateTime(...).Ticks.ToString()` -> `DateTime(...).Ticks.ToString(CultureInfo.InvariantCulture)` for deterministic numeric IDs and FIX order IDs.
  - Added missing `using System.Globalization;` where required.
  - Hardened time interval parsing in persistence/legacy settings loading:
    - `project/OsEngine/Entity/Order.cs`
      - replaced direct `TimeSpan.TryParse` with invariant-first/current/ru fallback helper.
    - `project/OsEngine/OsTrader/Panels/Tab/Internal/BotManualControl.cs`
      - replaced direct `TimeSpan.TryParse` in legacy parser with invariant-first/current helper.
  - Scope:
    - formatting/parsing determinism hardening only; runtime behavior unchanged for valid payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #529)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened exchange request payload/identifier serialization with explicit invariant culture in:
    - `project/OsEngine/Market/Servers/Binance/Futures/BinanceServerFutures.cs`
    - `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs`
    - `project/OsEngine/Market/Servers/CoinEx/Futures/CoinExServerFutures.cs`
    - `project/OsEngine/Market/Servers/CoinEx/Spot/CoinExServerSpot.cs`
    - `project/OsEngine/Market/Servers/BloFin/BloFinFutures/BloFinFuturesServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Spot/HTXSpotServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
    - `project/OsEngine/Market/Servers/Mexc/MexcSpot/MexcSpotServer.cs`
    - `project/OsEngine/Market/Servers/OKX/OKXServer.cs`
    - `project/OsEngine/Market/Servers/TraderNet/TraderNetServer.cs`
  - Replacements:
    - `order.NumberUser.ToString()` -> `order.NumberUser.ToString(CultureInfo.InvariantCulture)` in client-order-id and lookup paths.
    - request timestamp/id parameters (`startTime`, `endTime`, `fromId`, `page_index`) serialized with invariant culture.
  - Scope:
    - payload serialization determinism hardening only; runtime behavior unchanged for valid payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #530)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened timestamp and numeric query serialization with explicit invariant culture in:
    - `project/OsEngine/Market/Servers/XT/XTSpot/XTServerSpot.cs`
    - `project/OsEngine/Market/Servers/XT/XTFutures/XTFuturesServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Spot/HTXSpotServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
    - `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs`
    - `project/OsEngine/Market/Servers/Binance/Futures/BinanceServerFutures.cs`
    - `project/OsEngine/Market/Servers/Mexc/MexcSpot/MexcSpotServer.cs`
  - Replacements:
    - `ToUnixTimeMilliseconds().ToString()` -> `ToString(CultureInfo.InvariantCulture)` in websocket heartbeat/signature timestamps.
    - unix-ms/query numeric formatting (`GetUnixTimeStampMilliseconds`, `serverTime + 500`, candle range `startTime/endTime`, `limit`) switched to invariant serialization.
    - XT/Binance client-order numeric ids in payload/query paths (`NumberUser`) switched to invariant serialization.
    - Binance Futures amend-order payload numeric fields (`quantity`, `price`) switched to invariant serialization.
  - Scope:
    - payload/query serialization determinism hardening only; runtime behavior unchanged for valid payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #531)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened BingX timestamp/id serialization with explicit invariant culture in:
    - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
    - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
  - Replacements:
    - `DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()` -> `ToString(CultureInfo.InvariantCulture)` in REST signing/query paths.
    - `order.NumberUser.ToString()` -> `order.NumberUser.ToString(CultureInfo.InvariantCulture)` in spot order status lookup path.
    - `TimeManager.GetTimeStampMilliSecondsToDateTime(...).ToString()` -> invariant for generated trade ids and time-range query args in futures trade history paths.
  - Scope:
    - payload/query serialization determinism hardening only; runtime behavior unchanged for valid payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #532)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened Woo payload/signature timestamp serialization with explicit invariant culture in:
    - `project/OsEngine/Market/Servers/Woo/WooServer.cs`
  - Replacements:
    - `order.NumberUser.ToString()` -> `order.NumberUser.ToString(CultureInfo.InvariantCulture)` in client-order-id payload and order lookup paths.
    - `timestamp.ToString()` header serialization -> `timestamp.ToString(CultureInfo.InvariantCulture)` for `x-api-timestamp`.
    - signature base-string construction changed from interpolated `long timestamp` to explicit invariant string conversion via `timestamp.ToString(CultureInfo.InvariantCulture)`.
    - websocket ping payload timestamp uses invariant string variable.
  - Scope:
    - payload/signature serialization determinism hardening only; runtime behavior unchanged for valid payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #533)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened KuCoin payload/signature timestamp serialization with explicit invariant culture in:
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/KuCoinSpotServer.cs`
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServer.cs`
  - Replacements:
    - `order.NumberUser.ToString()` -> `order.NumberUser.ToString(CultureInfo.InvariantCulture)` in `clientOid` payload and futures order-status lookup path.
    - `DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()` -> `ToString(CultureInfo.InvariantCulture)` in private REST signing/query methods.
  - Scope:
    - payload/signature serialization determinism hardening only; runtime behavior unchanged for valid payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #534)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened request/signature/id serialization with explicit invariant culture in:
    - `project/OsEngine/Market/Servers/Atp/AtpServer.cs`
    - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/AscendexSpotServer.cs`
    - `project/OsEngine/Market/Servers/Alor/AlorServer.cs`
    - `project/OsEngine/Market/Servers/FinamGrpc/FinamGrpcServer.cs`
    - `project/OsEngine/Market/Servers/Bybit/BybitServer.cs`
    - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/BitGetServerSpot.cs`
    - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/BitGetServerFutures.cs`
    - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/BitMartSpotServer.cs`
    - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/BitMartFutures.cs`
    - `project/OsEngine/Market/Servers/CoinEx/Spot/CoinExServerSpot.cs`
    - `project/OsEngine/Market/Servers/CoinEx/Futures/CoinExServerFutures.cs`
    - `project/OsEngine/Market/Servers/BloFin/BloFinFutures/BloFinFuturesServer.cs`
    - `project/OsEngine/Market/Servers/Pionex/PionexServerSpot.cs`
    - `project/OsEngine/Market/Servers/ExMo/ExmoSpot/ExmoSpotServer.cs`
  - Replacements:
    - `order.NumberUser.ToString()` -> `order.NumberUser.ToString(CultureInfo.InvariantCulture)` in client-order-id and order lookup paths.
    - `DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()` -> `ToString(CultureInfo.InvariantCulture)` in REST signing/query timestamp paths.
    - `new DateTimeOffset(EndTime).ToUnixTimeMilliseconds().ToString()` -> invariant serialization for time-window query params.
  - Scope:
    - protocol/persistence serialization determinism hardening only; runtime behavior unchanged for valid payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #535)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Closed remaining `Market/Servers` hits for culture-sensitive serialization patterns:
    - `order.NumberUser.ToString()`
    - `DateTimeOffset...ToUnixTimeMilliseconds().ToString()`
  - Updated:
    - `project/OsEngine/Market/Servers/InteractiveBrokers/IbClient.cs`
    - `project/OsEngine/Market/Servers/Deribit/DeribitServer.cs`
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
    - `project/OsEngine/Market/Servers/QuikLua/QuikLuaServer.cs`
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/BitfinexFuturesServer.cs`
    - `project/OsEngine/Market/Servers/Transaq/TransaqServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
    - `project/OsEngine/Market/Servers/KiteConnect/KiteConnectServer.cs`
    - `project/OsEngine/Market/Servers/AExchange/AExchangeServer.cs`
  - Added missing `using System.Globalization;` in:
    - `project/OsEngine/Market/Servers/Deribit/DeribitServer.cs`
    - `project/OsEngine/Market/Servers/AExchange/AExchangeServer.cs`
  - Scope:
    - protocol/id/timestamp serialization determinism hardening only; runtime behavior unchanged for valid API payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #536)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened remaining unix-seconds timestamp serialization in:
    - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/BitGetServerSpot.cs`
    - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/BitGetServerFutures.cs`
  - Replacements:
    - `DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()` -> `ToString(CultureInfo.InvariantCulture)` in auth/signature timestamp paths.
  - Scope:
    - protocol timestamp serialization determinism hardening only; runtime behavior unchanged for valid payloads.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #537)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened MOEX FIX heartbeat `TestRequest` payload formatting in:
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
  - Replacements:
    - `DateTime.UtcNow.ToString("OsEngine")` -> `DateTime.UtcNow.ToString("OsEngine", CultureInfo.InvariantCulture)`.
  - Scope:
    - protocol string formatting determinism hardening only; heartbeat/test-request behavior unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #538)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened Bitfinex nonce serialization in:
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/BitfinexFuturesServer.cs`
  - Replacements:
    - `DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()` -> `ToString(CultureInfo.InvariantCulture)` in signed private-request nonce generation.
  - Scope:
    - protocol nonce serialization determinism hardening only; request semantics unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #539)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened timestamp serialization in signed/auth flows:
    - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/AscendexSpotServer.cs`
    - `project/OsEngine/Market/Servers/CoinEx/Spot/CoinExServerSpot.cs`
    - `project/OsEngine/Market/Servers/CoinEx/Futures/CoinExServerFutures.cs`
    - `project/OsEngine/Market/Servers/Bybit/BybitServer.cs`
    - `project/OsEngine/Market/Servers/OKX/Entity/Encryptor.cs`
    - `project/OsEngine/Market/Servers/OKX/Entity/HttpInterceptor.cs`
  - Replacements:
    - numeric timestamp formatting: `timestamp.ToString()` -> `timestamp.ToString(CultureInfo.InvariantCulture)` in request headers/signature payloads.
    - removed redundant `ToString()` calls where timestamp values are already strings (Bybit/OKX paths).
    - added missing `using System.Globalization;` in `Encryptor.cs`.
  - Scope:
    - protocol timestamp serialization determinism and cleanup only; request logic unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #540)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened elapsed/timeframe numeric serialization in:
    - `project/OsEngine/Market/Servers/Alor/AlorServer.cs`
    - `project/OsEngine/Market/Servers/BybitData/BybitDataServer.cs`
    - `project/OsEngine/Layout/StickyBorders.cs`
  - Replacements:
    - `TotalSeconds.ToString()` -> `TotalSeconds.ToString(CultureInfo.InvariantCulture)`.
    - `TotalMinutes.ToString()` -> `TotalMinutes.ToString(CultureInfo.InvariantCulture)`.
    - `TotalMilliseconds.ToString()` -> `TotalMilliseconds.ToString(CultureInfo.InvariantCulture)`.
  - Added missing `using System.Globalization;` in `StickyBorders.cs`.
  - Scope:
    - deterministic numeric serialization hardening only; runtime behavior unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #541)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened GateIo unix-seconds timestamp serialization in signed/public REST flows:
    - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
    - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/GateIoServerFutures.cs`
  - Replacements:
    - `TimeManager.GetUnixTimeStampSeconds().ToString()` -> `ToString(CultureInfo.InvariantCulture)` in request timestamp/signature construction paths.
  - Scope:
    - protocol timestamp serialization determinism hardening only; runtime behavior unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #542)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened GateIo WebSocket payload/signature numeric formatting in:
    - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
    - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/GateIoServerFutures.cs`
  - Replacements:
    - wrapped interpolated JSON payload strings with `FormattableString.Invariant(...)` for timestamp/id fields.
    - wrapped interpolated auth/signature parameter strings (`time=...`) with `FormattableString.Invariant(...)`.
    - replaced `string.Format`-based subscribe/unsubscribe auth params in futures with invariant interpolated strings.
  - Scope:
    - protocol payload/signature serialization determinism hardening only; runtime behavior unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #543)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened explicit invariant formatting of user-order IDs in request/signature/query strings in:
    - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
    - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
    - `project/OsEngine/Market/Servers/ExMo/ExmoSpot/ExmoSpotServer.cs`
    - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/BitGetServerFutures.cs`
    - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
    - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/GateIoServerFutures.cs`
  - Replacements:
    - used `order.NumberUser.ToString(CultureInfo.InvariantCulture)` for `newClientOrderId`/`clientOrderID`/`client_id`/`clientOid`/GateIo `text` payload fields participating in signed or persisted request strings.
  - Scope:
    - request/signature/query serialization determinism hardening only; runtime behavior unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #544)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened culture-invariant protocol string formatting in:
    - `project/OsEngine/Market/Servers/CoinEx/Spot/CoinExServerSpot.cs`
    - `project/OsEngine/Market/Servers/CoinEx/Futures/CoinExServerFutures.cs`
    - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
    - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
  - Replacements:
    - `string.Format(...)` -> `string.Format(CultureInfo.InvariantCulture, ...)` for CoinEx kline URLs.
    - standardized `clientOrderId` as invariant string for BingX signing and request parameter paths.
  - Scope:
    - protocol serialization determinism hardening only; runtime behavior unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #545)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened invariant formatting of historical-query timestamp/range protocol parameters in:
    - `project/OsEngine/Market/Servers/Deribit/DeribitServer.cs`
    - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
    - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/GateIoServerFutures.cs`
    - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
    - `project/OsEngine/Market/Servers/Woo/WooServer.cs`
    - `project/OsEngine/Market/Servers/YahooFinance/YahooServer.cs`
    - `project/OsEngine/Market/Servers/OKX/OkxServer.cs`
    - `project/OsEngine/Market/Servers/OKXData/OKXDataServer.cs`
  - Replacements:
    - explicit invariant serialization for `from/to/start/end/after/period1/period2/limit/timestamp` numeric values in URL/query/signature generation paths.
    - Deribit auth/signature timestamp string construction made explicit invariant.
  - Scope:
    - protocol URL/query/signature serialization determinism hardening only; runtime behavior unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #546)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit logging to previously silent catch blocks in:
    - `project/OsEngine/Entity/NonTradePeriods.cs`
    - `project/OsEngine/Market/Servers/Optimizer/OptimizerDataStorage.cs`
  - In `NonTradePeriods.cs`:
    - `catch { }` converted to `catch (Exception ex)` with `Trace.TraceWarning(ex.ToString())`.
  - In `OptimizerDataStorage.cs`:
    - remaining bare catches converted to `catch (Exception ex)`.
    - added `SendLogMessage(ex.ToString(), LogMessageType.Error)` while preserving existing branch behavior (`continue`, `break`, `remove`).
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - CultureInfo Invariant Persistence (Incremental Adoption #547)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Hardened Binance Spot protocol numeric string formatting in:
    - `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs`
  - Replacements:
    - `TimeManager.GetUnixTimeStampMilliseconds().ToString()` -> `ToString(CultureInfo.InvariantCulture)`.
    - `order.NumberUser.ToString()` -> `ToString(CultureInfo.InvariantCulture)` for `newClientOrderId` generation.
    - normalized legacy-order lookup to compare with precomputed invariant `oldOrderNumberUser`.
  - Scope:
    - protocol timestamp/client-order-id serialization determinism hardening only; runtime behavior unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings).
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #548)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added explicit exception logging in Binance connectors:
    - `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs`
    - `project/OsEngine/Market/Servers/Binance/Futures/BinanceServerFutures.cs`
  - Replaced remaining bare catches with `catch (Exception ex)` + `SendLogMessage(ex.ToString(), LogMessageType.Error)` while preserving existing return/continue/break semantics.
  - For expected spot history tail condition, converted silent ignore into `System` log entry.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release` blocked by environment NuGet TLS failure (`NU1301`, `SEC_E_NO_CREDENTIALS`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #549)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for previously silent catches in:
    - `project/OsEngine/Market/Servers/HTX/Spot/HTXSpotServer.cs`
    - `project/OsEngine/Market/Servers/HTX/Futures/HTXFuturesServer.cs`
  - Replaced bare `catch` / `catch (Exception)` with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved original control flow in affected branches.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` remains blocked intermittently by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #550)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/ExMo/ExmoSpot/ExmoSpotServer.cs`
    - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #551)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/GateIo/GateIoSpot/GateIoServerSpot.cs`
    - `project/OsEngine/Market/Servers/GateIo/GateIoFutures/GateIoServerFutures.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #552)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexSpot/BitfinexSpotServer.cs`
    - `project/OsEngine/Market/Servers/Bitfinex/BitfinexFutures/BitfinexFuturesServer.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #553)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/CoinEx/Spot/CoinExServerSpot.cs`
    - `project/OsEngine/Market/Servers/CoinEx/Futures/CoinExServerFutures.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #554)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/KuCoinSpotServer.cs`
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServer.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #555)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/XT/XTSpot/XTServerSpot.cs`
    - `project/OsEngine/Market/Servers/XT/XTFutures/XTFuturesServer.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #556)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/BitMartSpotServer.cs`
    - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/BitMartFutures.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #557)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/BitGet/BitGetSpot/BitGetServerSpot.cs`
    - `project/OsEngine/Market/Servers/BitGet/BitGetFutures/BitGetServerFutures.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #558)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
    - `project/OsEngine/Market/Servers/BloFin/BloFinFutures/BloFinFuturesServer.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #559)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/OKX/OkxServer.cs`
    - `project/OsEngine/Market/Servers/Pionex/PionexServerSpot.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #560)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/HTX/Swap/HTXSwapServer.cs`
    - `project/OsEngine/Market/Servers/InteractiveBrokers/IbClient.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #561)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/Woo/WooServer.cs`
    - `project/OsEngine/Market/Servers/Alor/AlorServer.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #562)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/Bybit/BybitServer.cs`
    - `project/OsEngine/Market/Servers/AscendEX/AscendEXSpot/AscendexSpotServer.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #563)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
    - `project/OsEngine/Market/Servers/Tester/TesterServerUi.xaml.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #564)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/Deribit/DeribitServer.cs`
    - `project/OsEngine/Market/Servers/Mexc/MexcSpot/MexcSpotServer.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #565)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/KiteConnect/KiteConnectServer.cs`
    - `project/OsEngine/Market/Servers/InteractiveBrokers/InteractiveBrokersServer.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet vstest project/OsEngine.Tests/bin/Release/net10.0-windows/OsEngine.Tests.dll` succeeded (`352/352`).
  - `dotnet build ... --no-restore` intermittently blocked by sandbox TLS/NuGet (`NU1301`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #566)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/ServerCandleStorage.cs`
    - `project/OsEngine/Market/Servers/MetaTrader5/MetaTrader5Server.cs`
  - Replaced bare catches with `catch (Exception ex)` and context-appropriate logging (`SendNewLogMessage` / `SendLogMessage`).
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo -p:IsTestProject=true --settings .coverage.runsettings` succeeded (`360/360`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #567)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/MoexAlgopack/MoexAlgopackServer.cs`
    - `project/OsEngine/Market/Servers/TraderNet/TraderNetServer.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo -p:IsTestProject=true --settings .coverage.runsettings` succeeded (`360/360`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #568)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for silent catches in:
    - `project/OsEngine/Market/Servers/ComparePositionsModule.cs`
    - `project/OsEngine/Market/Servers/Atp/AtpServer.cs`
  - Replaced bare catches with `catch (Exception ex)` and `SendLogMessage(ex.ToString(), LogMessageType.Error)`.
  - Preserved existing branch control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo -p:IsTestProject=true --settings .coverage.runsettings` succeeded (`360/360`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #569)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging for previously ignored catches in:
    - `project/OsEngine/Entity/WebSocketOsEngine.cs`
  - Replaced ignored catches with `catch (Exception ex)` and routed to existing error event pipeline:
    - `OnError?.Invoke(this, new ErrorEventArgs { Exception = ex });`
  - Preserved existing branch control flow and non-throwing behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #570)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging/visibility for remaining bare catches in:
    - `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs`
    - `project/OsEngine/Market/Servers/Binance/Futures/BinanceServerFutures.cs`
  - Replaced bare catches with `catch (Exception ex)` and routed details to existing log paths.
  - Added `Trace.TraceWarning(ex.ToString())` in listen-key parse guard fallback.
  - Preserved existing control flow and non-throwing behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #571)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging/visibility for remaining bare catches in:
    - `project/OsEngine/Market/Servers/BingX/BingXSpot/BingXServerSpot.cs`
    - `project/OsEngine/Market/Servers/BingX/BingXFutures/BingXServerFutures.cs`
  - Replaced bare catches with `catch (Exception ex)` and routed details to existing log paths.
  - Preserved existing control flow and non-throwing behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #572)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging/visibility for remaining bare catches in:
    - `project/OsEngine/Market/Servers/BitMart/BitMartSpot/BitMartSpotServer.cs`
    - `project/OsEngine/Market/Servers/BitMart/BitMartFutures/BitMartFutures.cs`
  - Replaced bare catches with `catch (Exception ex)` and routed details to existing log paths.
  - Preserved existing control flow and non-throwing behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #573)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging/visibility for remaining bare catches in:
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinSpot/KuCoinSpotServer.cs`
    - `project/OsEngine/Market/Servers/KuCoin/KuCoinFutures/KuCoinFuturesServer.cs`
  - Replaced bare catches with `catch (Exception ex)` and routed details to existing log paths.
  - Preserved existing control flow and non-throwing behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #574)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging/visibility for remaining bare catches in:
    - `project/OsEngine/Market/Servers/Bybit/BybitServer.cs`
    - `project/OsEngine/Market/Servers/Mexc/MexcSpot/MexcSpotServer.cs`
  - Replaced bare catches with `catch (Exception ex)` and routed details to existing log paths/trace warning for static context.
  - Preserved existing control flow and non-throwing behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #575)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging/visibility for remaining bare catches in:
    - `project/OsEngine/Market/Servers/XT/XTSpot/XTServerSpot.cs`
    - `project/OsEngine/Market/Servers/Alor/AlorServer.cs`
    - `project/OsEngine/Market/Servers/Transaq/TransaqServer.cs`
    - `project/OsEngine/Market/Servers/Finam/Entity/FinamDataSeries.cs`
  - Replaced bare catches with `catch (Exception ex)` and routed details to existing log paths.
  - Preserved existing control flow and non-throwing behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #576)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging/visibility for remaining bare catches in:
    - `project/OsEngine/Market/Servers/Atp/AtpServer.cs`
    - `project/OsEngine/Market/Servers/FixProtocolEntities/FixEntity.cs`
    - `project/OsEngine/Market/Servers/InteractiveBrokers/IbClient.cs`
  - Replaced bare catches with explicit exception handling and added diagnostics (log/trace) while preserving return defaults.
  - Preserved existing control flow and non-throwing behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #577)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging/visibility for remaining bare catches in:
    - `project/OsEngine/Market/Servers/ServerTickStorage.cs`
    - `project/OsEngine/Market/Servers/Tester/TesterServer.cs`
    - `project/OsEngine/Market/Servers/FinamGrpc/FinamGrpcServer.cs`
  - Replaced bare catches with explicit exception handling and diagnostics while preserving return defaults.
  - Preserved existing control flow and non-throwing behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #578)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging/visibility for remaining bare catches in:
    - `project/OsEngine/Market/Servers/TInvest/TInvestServer.cs`
    - `project/OsEngine/Market/Servers/QuikLua/QuikLuaServer.cs`
    - `project/OsEngine/Market/Servers/Transaq/TransaqServer.cs`
  - Replaced bare catches with explicit exception handling and diagnostics while preserving return defaults.
  - Preserved existing control flow and non-throwing behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #579)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging/visibility for remaining bare catches in:
    - `project/OsEngine/Market/Servers/Plaza/PlazaServer.cs`
  - Replaced bare catches with explicit exception handling and diagnostics while preserving existing reconnect/dispose flow.
  - Preserved existing control flow and non-throwing behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #580)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging/visibility for remaining bare catches in:
    - `project/OsEngine/Market/Servers/MoexFixFastSpot/MoexFixFastSpotServer.cs`
  - Replaced bare catches with explicit exception handling (`TraceWarning`) while preserving existing reconnect/dispose and parse-fallback behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #581)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging/visibility for remaining bare catches in:
    - `project/OsEngine/Market/Servers/MoexFixFastCurrency/MoexFixFastCurrencyServer.cs`
    - `project/OsEngine/Market/Servers/MoexFixFastTwimeFutures/MoexFixFastTwimeFuturesServer.cs`
  - Replaced bare catches with explicit exception handling (`TraceWarning`) while preserving existing reconnect/dispose and snapshot/socket fallback behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #582)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging/visibility for remaining bare catches in:
    - `project/OsEngine/Market/Servers/AServer.cs`
  - Replaced bare catches with explicit exception handling and diagnostics via base server logger.
  - Preserved existing control flow and non-throwing behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 0.3 - Silent Catch Visibility (Incremental Adoption #583)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 0 / Step 0.3
- **Changes:**
  - Added logging/visibility for remaining silent catches in:
    - `project/OsEngine/Market/Servers/FinamGrpc/FinamGrpcServer.cs`
    - `project/OsEngine/MainWindow.xaml.cs`
  - Replaced empty `catch (OperationCanceledException)` handlers in Finam stream readers with `TraceInformation` diagnostics.
  - Replaced empty process-probe catch in main window startup check with explicit exception logging (`TraceWarning`).
  - Preserved existing control flow and non-throwing behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`391/391`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #584)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Added test coverage for `Order` persistence parsing:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added roundtrip test for `GetStringForSave` / `SetOrderFromString` including trade and cancel metadata fields.
  - Added legacy RU datetime parse test to lock fallback behavior in `SetOrderFromString`.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`393/393`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #585)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Added test coverage for `MyTrade` persistence parsing:
    - `project/OsEngine.Tests/MyTradePersistenceTests.cs`
  - Added roundtrip test for `GetStringFofSave` / `SetTradeFromString` with security code escaping for `@`.
  - Added date parsing fallback test for ISO (`O`) and legacy RU datetime formats.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`395/395`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #586)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Trade` persistence parse coverage:
    - `project/OsEngine.Tests/TradeCoreTests.cs`
  - Added IQFeed legacy RU datetime parse fallback test.
  - Added standard format parse test without optional depth block.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` succeeded.
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` succeeded.
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` succeeded (0 warnings, 0 errors).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`397/397`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Stage2 Checkpoint (Post #586)

- **Status:** In Progress (checkpoint recorded)
- **Date:** 2026-02-26
- **Summary:**
  - Incremental steps completed and committed:
    - `#584` `7526d6db8` (`Order` persistence parsing tests)
    - `#585` `11e1086ee` (`MyTrade` persistence parsing tests)
    - `#586` `8b4ad02bb` (`Trade` persistence parsing tests)
  - Net result: test suite baseline increased to `397` passing tests.
- **Verification:**
  - Last host-context verification before checkpoint:
    - `dotnet restore` (OsEngine + OsEngine.Tests) succeeded.
    - `dotnet build` (Release) succeeded with `0` warnings, `0` errors.
    - `dotnet test` succeeded (`397/397`).
- **Commit:** `a3a4d504c`
- **Push:** pending

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #587)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Added core persistence parsing coverage for `Position`:
    - `project/OsEngine.Tests/PositionPersistenceTests.cs`
  - Added roundtrip test for `GetStringForSave()` / `SetDealFromString()` with open/close orders and stop/profit flags.
  - Added legacy RU datetime parse test for embedded order payload in serialized position data.
  - Locked current callback timestamp mapping behavior for embedded orders (loader uses order field index `15`).
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`399/399`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #588)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded core persistence parsing coverage for `Candle`:
    - `project/OsEngine.Tests/CandleCoreTests.cs`
  - Added roundtrip test for `StringToSave` / `SetCandleFromString` with time, OHLC, volume and open interest.
  - Added invariant-format assertion under `ru-RU` process culture to lock decimal separator behavior in serialized candles.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`400/400`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #589)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Added core persistence parsing coverage for `MarketDepth`:
    - `project/OsEngine.Tests/MarketDepthCoreTests.cs`
  - Added roundtrip test for `GetSaveStringToAllDepfh()` / `SetMarketDepthFromString()` with timestamp and level payload.
  - Added invariant-format assertion under `ru-RU` process culture for serialized market depth decimals.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`402/402`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #590)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Added core persistence parsing coverage for `PositionOpenerToStopLimit`:
    - `project/OsEngine.Tests/PositionOpenerToStopCoreTests.cs`
  - Added roundtrip test for `GetSaveString()` / `LoadFromString()` with enums, decimals and timestamps.
  - Added legacy RU datetime parse test and invariant-decimal assertions on persisted payload fields.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`404/404`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #591)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Added core persistence parsing coverage for `Security`:
    - `project/OsEngine.Tests/SecurityCoreTests.cs`
  - Added roundtrip test for `GetSaveStr()` / `LoadFromString()` including optional tail fields.
  - Added legacy RU datetime parse test for `Expiration` and invariant-decimal assertions.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`406/406`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #592)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Added core persistence parsing coverage for `NonTradePeriodInDay`:
    - `project/OsEngine.Tests/NonTradePeriodInDayCoreTests.cs`
  - Added roundtrip test for `GetSaveString()` / `LoadFromString()` for all configured period slots.
  - Added legacy payload test ensuring reserved tail fields do not affect parsing of active fields.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`408/408`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #593)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded core persistence parsing coverage for `Trade`:
    - `project/OsEngine.Tests/TradeCoreTests.cs`
  - Added roundtrip test for `GetSaveString()` / `SetTradeFromString()` with optional depth payload fields.
  - Added assertions for microseconds and trade id save/load consistency.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`409/409`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #594)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Security` parser compatibility coverage:
    - `project/OsEngine.Tests/SecurityCoreTests.cs`
  - Added test for legacy short payload without optional tail fields.
  - Added assertions for default fallback values on omitted fields.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`410/410`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #595)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for legacy short payload without optional tail fields.
  - Added assertions for fallback defaults on omitted order tail fields.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`411/411`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #596)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `PositionOpenerToStopLimit` parser compatibility coverage:
    - `project/OsEngine.Tests/PositionOpenerToStopCoreTests.cs`
  - Added test for legacy short payload without optional tail fields.
  - Added assertions for fallback defaults on omitted fields.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`412/412`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #597)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes (batch):**
  - `Position` parser: added legacy-short payload compatibility test without market-flag tail fields.
  - `MarketDepth` parser: added depth=`0` save/load compatibility test.
  - `NonTradePeriodInDay` parser: added malformed payload non-throwing compatibility test.
  - `Order` parser: added partial-tail payload compatibility test (with `OrderTypeTime` only).
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`416/416`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #598)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Position` parser compatibility coverage:
    - `project/OsEngine.Tests/PositionPersistenceTests.cs`
  - Added test for lowercase `true/false` market flag payload values.
  - Added assertions for case-insensitive parsing of market-flag tail fields.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`417/417`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #599)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for payload length `22` (`OrderTypeTime` present, later tail fields omitted).
  - Added assertions for fallback defaults on omitted tail fields.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`418/418`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #600)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for malformed cancel-info tail payload.
  - Added assertions for fallback defaults on malformed cancel-info data.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`419/419`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #601)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Position` parser compatibility coverage:
    - `project/OsEngine.Tests/PositionPersistenceTests.cs`
  - Added test for invalid market-flag tail values.
  - Added assertions for fallback defaults on unrecognized market-flag payload values.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`420/420`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #602)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Security` parser compatibility coverage:
    - `project/OsEngine.Tests/SecurityCoreTests.cs`
  - Added test for CRLF-formatted payload parsing.
  - Added assertions for parsing of optional tail fields from CRLF input.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`421/421`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #603)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes (batch):**
  - `Trade` parser: added IQFeed buy-side inference compatibility test.
  - `MarketDepth` parser: added empty-level save/load roundtrip test.
  - `PositionOpenerToStopLimit` parser: added payload compatibility test with `OrderPriceType` and missing `PositionNumber`.
  - `Order` parser: added empty-servername + valid-cancel-tail compatibility test.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`425/425`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #604)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `MyTrade` parser compatibility coverage:
    - `project/OsEngine.Tests/MyTradePersistenceTests.cs`
  - Added test for payload with empty trailing `NumberPosition` value.
  - Added assertions for stable parse of core trade fields with empty position tail.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`426/426`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #605)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Security` parser compatibility coverage:
    - `project/OsEngine.Tests/SecurityCoreTests.cs`
  - Added test for CRLF-formatted legacy-short payload without optional tail fields.
  - Added assertions for fallback defaults of omitted tail fields under CRLF input.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`427/427`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #606)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for escaped-name payload (`%` encoded values for `@`).
  - Added assertions for unescape behavior of security/portfolio names.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`428/428`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #607)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for legacy misspelled order state value (`Patrial`).
  - Added assertion for enum alias mapping to `OrderStateType.Partial`.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`429/429`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #608)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for legacy misspelled order state value (`Activ`).
  - Added assertion for enum alias mapping to `OrderStateType.Active`.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`430/430`).
- **Commit:** n/a (not committed in this session)
- **Push:** n/a

### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #609)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for legacy payload with numeric state value (`"5"`).
  - Added assertion for enum numeric mapping to `OrderStateType.Partial`.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`431/431`).
- **Commit:** `afe1c5d60`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #610)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for legacy payload with numeric active state value (`"2"`).
  - Added assertion for enum numeric mapping to `OrderStateType.Active`.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`432/432`).
- **Commit:** `1f8192398`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #611)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for legacy payload with numeric cancel state value (`"7"`).
  - Added assertion for enum numeric mapping to `OrderStateType.Cancel`.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`433/433`).
- **Commit:** `af22a1775`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #612)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for legacy payload with numeric fail state value (`"6"`).
  - Added assertion for enum numeric mapping to `OrderStateType.Fail`.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`434/434`).
- **Commit:** `b7cae7478`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #613)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for legacy payload with numeric pending state value (`"3"`).
  - Added assertion for enum numeric mapping to `OrderStateType.Pending`.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`435/435`).
- **Commit:** `6cba54489`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #614)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for legacy payload with numeric done state value (`"4"`).
  - Added assertion for enum numeric mapping to `OrderStateType.Done`.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`436/436`).
- **Commit:** `3a6787a85`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #615)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for legacy payload with numeric lost-after-active state value (`"8"`).
  - Added assertion for enum numeric mapping to `OrderStateType.LostAfterActive`.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`437/437`).
- **Commit:** `17232e0d7`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #616)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for legacy payload with numeric none state value (`"1"`).
  - Added assertion for enum numeric mapping to `OrderStateType.None`.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`438/438`).
- **Commit:** `82912aff3`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #617)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes:**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added test for legacy payload with numeric `OrderTypeTime` value (`"2"`).
  - Added assertion for enum numeric mapping to `OrderTypeTime.Day`.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`439/439`).
- **Commit:** `ca47077bc`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #618)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes (batch):**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added numeric `OrderTypeTime` parse tests for values `1` (`GTC`) and `0` (`Specified`).
  - Added lowercase enum parse test for `OrderTypeTime` value `gtc`.
  - Added invalid-value fallback test for `OrderTypeTime` (`not-a-valid-order-type-time` -> `Specified`).
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`443/443`).
- **Commit:** `9f703bdd7`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #619)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes (batch):**
  - Expanded `Order` parser compatibility coverage:
    - `project/OsEngine.Tests/OrderPersistenceTests.cs`
  - Added lowercase `State` parse tests for values `active`, `partial`, `cancel`.
  - Added invalid-value fallback test for `State` (`not-a-valid-order-state`) and fixed expected assertion to current parser behavior (underlying enum value `0`).
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`447/447`).
- **Commit:** `656f3a2dd`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #620)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes (batch, multi-agent):**
  - Expanded parser compatibility coverage in:
    - `project/OsEngine.Tests/PositionPersistenceTests.cs`
    - `project/OsEngine.Tests/SecurityCoreTests.cs`
    - `project/OsEngine.Tests/TradeCoreTests.cs`
  - Added 3 tests for `Position.SetDealFromString` legacy edge-cases.
  - Added 3 tests for `Security.LoadFromString` legacy optional-tail edge-cases.
  - Added 3 tests for `Trade.SetTradeFromString` legacy format edge-cases.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`456/456`).
- **Commit:** `d710dac06`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #621)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes (batch, multi-agent):**
  - Expanded parser compatibility coverage in:
    - `project/OsEngine.Tests/MyTradePersistenceTests.cs`
    - `project/OsEngine.Tests/MarketDepthCoreTests.cs`
    - `project/OsEngine.Tests/PositionOpenerToStopCoreTests.cs`
  - Added 3 tests for `MyTrade` legacy parser/save compatibility.
  - Added 3 tests for `MarketDepth` legacy parser compatibility.
  - Added 3 tests for `PositionOpenerToStopLimit` legacy parser compatibility.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`465/465`).
- **Commit:** `2b316635b`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #622)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes (batch, multi-agent):**
  - Expanded parser compatibility coverage in:
    - `project/OsEngine.Tests/CandleCoreTests.cs`
    - `project/OsEngine.Tests/NonTradePeriodInDayCoreTests.cs`
    - `project/OsEngine.Tests/StrategyParameterNumericTimeCoreTests.cs`
  - Added 3 tests for `Candle` legacy parser/save compatibility.
  - Added 3 tests for `NonTradePeriodInDay` legacy parser/save compatibility.
  - Added 3 tests for `StrategyParameterNumericTime` legacy parsing compatibility.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`474/474`).
- **Commit:** `5a988e5e1`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #623)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes (TradeGrid parser compatibility batch):**
  - Added parser compatibility tests in:
    - `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`
  - Added coverage for:
    - `TradeGridNonTradePeriods.LoadFromString` reserved-tail compatibility.
    - `TradeGridAutoStarter.LoadFromString` short-tail optional time-section fallback.
    - `TradeGridErrorsReaction.LoadFromString` optional-tail fallback defaults.
    - `TrailingUp.LoadFromString` legacy payload without move-flag fields.
    - `TradeGrid.LoadFromString` legacy prime-short-tail payload fallback behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`479/479`).
- **Commit:** `a3a4d504c`
- **Push:** n/a


### Step 2.2 - InvariantCulture Coverage (Incremental Adoption #624)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.2
- **Changes (optimizer settings parser compatibility batch):**
  - Added compatibility and roundtrip tests in:
    - `project/OsEngine.Tests/OptimizerSettingsCollectionsPersistenceTests.cs`
  - Added coverage for:
    - line-based legacy `OrderClearing` payloads loaded by `OptimizerSettings.LoadClearingInfo()`.
    - line-based legacy `NonTradePeriod` payloads loaded by `OptimizerSettings.LoadNonTradePeriods()`.
    - save/load roundtrip for optimizer `ClearingTimes` and `NonTradePeriods` collections.
  - Added test isolation for file-backed optimizer settings tests (disable parallelization in shared collection):
    - `project/OsEngine.Tests/OptimizerRefactorTests.cs`
    - `project/OsEngine.Tests/OptimizerSettingsCollectionsPersistenceTests.cs`
  - Runtime parser hardening (verification-driven fix):
    - fixed `TrailingUp.LoadFromString()` to safely handle short legacy payloads without `IndexOutOfRangeException`:
      - `project/OsEngine/OsTrader/Grids/TrailingUp.cs`
    - added regression test for short payload handling:
      - `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`483/483`).
- **Commit:** `c961c89d8`
- **Push:** n/a


### Step 2.1 - Atomic File Writes (Incremental Adoption #625)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.1
- **Changes (storage hotspot completion pass):**
  - Completed atomic-write migration in:
    - `project/OsEngine/Market/Servers/ServerTickStorage.cs`
  - Replaced direct append `StreamWriter(..., append: true)` path with atomic append-through-rewrite:
    - collect incremental lines for current save cycle.
    - merge with existing file content.
    - persist via `SafeFileWriter.WriteAllLines(...)`.
  - Added helper `AppendLinesAtomically(...)` in `ServerTickStorage`.
  - Scope audit result (`ServerTickStorage`, `ServerCandleStorage`, `TesterServer`, `OptimizerDataStorage`):
    - no remaining direct `StreamWriter`/`File.WriteAll*` create-append write paths in audited files.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`483/483`).
- **Commit:** `d70fd6a81`
- **Push:** n/a


### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #626)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes (optimizer settings core migration pass):**
  - Inventory check for completion pass:
    - broad JSON subsystem coverage already present.
    - remaining legacy readers mostly in strategy-specific settings paths.
    - selected high-risk core hotspot for migration: `OptimizerSettings` main settings file.
  - Migrated optimizer main settings persistence to JSON:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
    - save path now uses `SettingsManager.Save(...)`.
  - Added backward-compatible load adapter:
    - JSON-first load path with fallback to legacy line-based payload parser.
    - preserved legacy optional-tail defaults/clamping behavior for method settings fields.
  - Updated tests for explicit compatibility semantics:
    - `project/OsEngine.Tests/OptimizerRefactorTests.cs`
    - added assertion that saved `OptimizerSettings` payload is JSON.
    - converted line-index patch tests to explicit legacy payload fixtures.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`483/483`).
- **Commit:** `95ae04954`
- **Push:** n/a


### Step 3.1 - Optimizer Performance (Indicator Cache Pilot Metrics) (Incremental Adoption #627)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 3 / Step 3.1
- **Changes (pilot baseline instrumentation):**
  - Added deterministic cache pilot test:
    - `project/OsEngine.Tests/OptimizerIndicatorCachePilotMetricsTests.cs`
  - Scenario parameters:
    - unique indicator keys: `40`
    - repeated requests per key: `30`
    - total requests: `1200`
  - Measured counters:
    - baseline (without cache): `1200` computations
    - with cache: `40` computations
    - reduction: `96.67%`
    - cache stats: `hits=1160`, `misses=40`, `writes=40`, `entries=40`, `hit-rate=96.67%`
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo --filter "FullyQualifiedName~OptimizerIndicatorCachePilotMetricsTests"` succeeded (`1/1`).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `1b71ad4b6`
- **Push:** n/a


### Step 2.3 - JSON Settings Subsystem (Incremental Adoption #626, optimizer collections pass)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 2 / Step 2.3
- **Changes (high-risk optimizer settings scope):**
  - Migrated `OptimizerSettings` collections persistence to JSON with legacy fallback:
    - `project/OsEngine/OsOptimizer/OptEntity/OptimizerSettings.cs`
    - `SaveClearingInfo/LoadClearingInfo`
    - `SaveNonTradePeriods/LoadNonTradePeriods`
  - Added typed JSON wrappers for collections:
    - clearings collection DTO wrapper
    - non-trade periods collection DTO wrapper
  - Added explicit legacy parsers for line-based content to preserve backward compatibility.
  - Updated tests to assert JSON save format while retaining legacy load compatibility coverage:
    - `project/OsEngine.Tests/OptimizerSettingsCollectionsPersistenceTests.cs`
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo --filter "FullyQualifiedName~OptimizerSettingsCollectionsPersistenceTests|FullyQualifiedName~OptimizerRefactorTests"` succeeded (`79/79`).
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `370dca426`
- **Push:** n/a


### Step 4.1 - Lock Migration (Incremental Adoption #628)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.1
- **Changes (cleanup pass for remaining object locks):**
  - Inventory result in production code:
    - active `new object()` lock field found in:
      - `project/OsEngine/Market/Servers/Binance/Spot/BinanceServerSpot.cs` (`_queryHttpLocker`)
    - one commented occurrence in `MfdServer` ignored as non-runtime path.
  - Migrated active field to `Lock`:
    - `private object _queryHttpLocker = new object();`
    - -> `private readonly Lock _queryHttpLocker = new();`
  - Existing `lock (_queryHttpLocker)` usage unchanged.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `925952572`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #629)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (server storage nullable cleanup):**
  - Updated nullable contracts in:
    - `project/OsEngine/Market/Servers/ServerTickStorage.cs`
  - Runtime reference/event hardening:
    - `_server` annotated as nullable (`AServer?`) with explicit guards before use.
    - `TickLoadedEvent` and `LogMessageEvent` annotated nullable.
  - Removed lazy-null collection patterns:
    - `_securities` and `_tradeSaveInfo` converted to readonly initialized lists.
  - Simplified conditional flow after guaranteed list initialization.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `c0c8238d7`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #630)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (candle storage nullable cleanup):**
  - Updated nullable contracts in:
    - `project/OsEngine/Market/Servers/ServerCandleStorage.cs`
  - Runtime reference/event hardening:
    - `_server` annotated as nullable (`AServer?`) with explicit guard before use in saver thread.
    - `LogMessageEvent` annotated nullable.
  - Optional-load APIs clarified:
    - `TryLoadCandle(...)` -> `CandleSeriesSaveInfo?`
    - `GetCandles(...)` -> `List<Candle>?` (existing null behavior retained).
  - Collection immutability/initialization cleanup:
    - `_candleSeriesSaveInfos` converted to readonly initialized list.
  - File parser hardening:
    - nullable `ReadLine()` handling with whitespace skip guards in candle loading loops.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `19d4ef795`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #631)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (CandleSeriesSaveInfo nullable contracts):**
  - Updated `project/OsEngine/Market/Servers/ServerCandleStorage.cs`:
    - `CandleSeriesSaveInfo.AllCandlesInFile` annotated nullable (`List<Candle>?`).
    - `CandleSeriesSaveInfo.Specification` initialized with non-null default (`string.Empty`).
    - `SaveSeries(...)` safe-fallback conversion for nullable candle list before persistence.
    - `TryTrim(...)` null guard added before list count/index operations.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `dd1f8b922`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #632)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeSaveInfo nullable contracts):**
  - Updated `project/OsEngine/Market/Servers/ServerTickStorage.cs`:
    - `TradeSaveInfo.NameSecurity` initialized to `string.Empty`.
    - `TradeSaveInfo.LastTradeId` initialized to `string.Empty`.
  - Purpose: remove nullable-unsafe uninitialized string fields in storage holder without behavior changes.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `18b0d45c7`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #633)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (CandleSeriesSaveInfo signature alignment):**
  - Updated `project/OsEngine/Market/Servers/ServerCandleStorage.cs`:
    - `InsertCandles(List<Candle> candles, int maxCount)` -> `InsertCandles(List<Candle>? candles, int maxCount)`.
  - Existing null guard behavior preserved (`if (candles == null) return;`).
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `55a28ade3`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #634)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (ServerCandleStorage method signatures):**
  - Updated `project/OsEngine/Market/Servers/ServerCandleStorage.cs`:
    - `SetSeriesToSave(CandleSeries)` -> `SetSeriesToSave(CandleSeries?)`
    - `RemoveSeries(CandleSeries)` -> `RemoveSeries(CandleSeries?)`
    - `SaveSeries(CandleSeries)` -> `SaveSeries(CandleSeries?)`
  - Added early-return null guards in these methods.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `ef0cc54a3`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #635)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (ServerTickStorage method signatures):**
  - Updated `project/OsEngine/Market/Servers/ServerTickStorage.cs`:
    - `SetSecurityToSave(Security)` -> `SetSecurityToSave(Security?)`
    - `AppendLinesAtomically(string path, List<string> linesToAppend)` ->
      `AppendLinesAtomically(string path, List<string>? linesToAppend)`
  - Added early-return null guard in `SetSecurityToSave(...)`.
  - Preserved existing null-guard behavior in `AppendLinesAtomically(...)`.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `8b9a63570`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #636)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (ServerTickStorage local nullable contracts):**
  - Updated `project/OsEngine/Market/Servers/ServerTickStorage.cs`:
    - `List<Trade>[] allTrades` -> `List<Trade>[]? allTrades` in tick saver path.
    - `List<Trade>[] allTrades` -> `List<Trade>[]? allTrades` in tick loader path.
    - completion callback invoke changed to nullable-safe `TickLoadedEvent?.Invoke(allTrades)` under existing `allTrades != null` guard.
  - Purpose: align local type contracts with existing null-aware control flow.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `83961228b`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #637)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (ServerTickStorage line-read/log-event cleanup):**
  - Updated `project/OsEngine/Market/Servers/ServerTickStorage.cs`:
    - nullable-aware line read in loader loop:
      - `string? line = reader.ReadLine();`
      - skip `null`/whitespace lines before append to parse list.
    - logging callback invoke aligned to nullable-event pattern:
      - `LogMessageEvent?.Invoke(message, type)`.
      - preserved error message-box fallback when no subscribers exist.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `ba37e932d`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #638)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (ServerCandleStorage log-event dispatch):**
  - Updated `project/OsEngine/Market/Servers/ServerCandleStorage.cs`:
    - changed log callback dispatch to nullable-safe invoke: `LogMessageEvent?.Invoke(message, type)`.
    - kept existing `MessageBox` fallback for `Error` messages when no subscribers exist.
  - Purpose: align event invocation style with nullable event contract, without changing behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `326589f12`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #639)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (ServerCandleStorage series collection contract):**
  - Updated `project/OsEngine/Market/Servers/ServerCandleStorage.cs`:
    - `_series` field type aligned to nullable-aware element contract:
      - `List<CandleSeries>` -> `List<CandleSeries?>`.
    - `_series` marked `readonly` with existing eager initialization.
  - Purpose: align collection type with existing null checks in series iteration paths.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `be410eacc`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #640)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp parser/log contracts):**
  - Updated `project/OsEngine/OsTrader/Grids/TrailingUp.cs`:
    - `LoadFromString(string)` -> `LoadFromString(string?)` with existing whitespace guard preserved.
    - `LogMessageEvent` aligned to nullable event contract (`Action<string, LogMessageType>?`).
    - log dispatch updated to nullable-safe `LogMessageEvent?.Invoke(message, type)`.
    - existing `ServerMaster.SendNewLogMessage(...)` fallback for `Error` when no subscribers preserved.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `d9c207da6`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #641)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp grid lifecycle contract):**
  - Updated `project/OsEngine/OsTrader/Grids/TrailingUp.cs`:
    - `_grid` type aligned to nullable lifecycle (`TradeGrid?`) since `Delete()` nulls the field.
    - added null guards before grid dereference in:
      - `TryTrailingGrid()`
      - `MaxGridPrice` getter
      - `MinGridPrice` getter
      - `ShiftGridDownOnValue(...)`
      - `ShiftGridUpOnValue(...)`
  - Purpose: prevent null-reference failures after delete lifecycle while preserving normal runtime behavior.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `ef536a6f7`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #642)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopBy parser/log contracts):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridStopBy.cs`:
    - `LoadFromString(string)` -> `LoadFromString(string?)`.
    - added empty payload guard (`string.IsNullOrWhiteSpace(...)`).
    - added field-by-field length/empty checks before parsing `Split('@')` values to avoid index overflow on malformed payloads.
    - `LogMessageEvent` aligned to nullable event contract (`Action<string, LogMessageType>?`).
    - logging dispatch changed to nullable-safe `LogMessageEvent?.Invoke(message, type)` with fallback preserved for `Error`.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `a1e949877`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #643)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopAndProfit parser/log contracts):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridStopAndProfit.cs`:
    - `LoadFromString(string)` -> `LoadFromString(string?)`.
    - added empty payload guard (`string.IsNullOrWhiteSpace(...)`).
    - added field-by-field length/empty checks before parsing `Split('@')` values to prevent index overflow on malformed payloads.
    - `LogMessageEvent` aligned to nullable event contract (`Action<string, LogMessageType>?`).
    - logging dispatch changed to nullable-safe `LogMessageEvent?.Invoke(message, type)` with `Error` fallback preserved.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `cbdc8cba2`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #644)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridAutoStarter parser/log contracts):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridAutoStarter.cs`:
    - `LoadFromString(string)` -> `LoadFromString(string?)`.
    - added empty payload guard (`string.IsNullOrWhiteSpace(...)`).
    - added field-by-field length/empty checks before parsing `Split('@')` values to prevent index overflow on malformed payloads.
    - preserved existing nested parse-guard for optional time-of-day fields.
    - `LogMessageEvent` aligned to nullable event contract (`Action<string, LogMessageType>?`).
    - logging dispatch changed to nullable-safe `LogMessageEvent?.Invoke(message, type)` with `Error` fallback preserved.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `67588e25c`
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #645)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridErrorsReaction parser/lifecycle/log contracts):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridErrorsReaction.cs`:
    - `LoadFromString(string)` -> `LoadFromString(string?)`.
    - added empty payload guard (`string.IsNullOrWhiteSpace(...)`).
    - added field-by-field length/empty checks before parsing `Split('@')` values to prevent index overflow on malformed payloads.
    - `_myGrid` aligned to nullable lifecycle (`TradeGrid?`) and used via guarded local snapshot in no-funds reaction path.
    - `LogMessageEvent` aligned to nullable event contract (`Action<string, LogMessageType>?`).
    - logging dispatch changed to nullable-safe `LogMessageEvent?.Invoke(message, type)` with `Error` fallback preserved.
- **Verification:**
  - Executed outside sandbox.
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` succeeded (`484/484`).
- **Commit:** `83ab240be`
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #646)

- **Status:** In Progress (increment completed)
- **Plan item:** efactoring_stage2_plan.md -> Phase 4 / Step 4.2
- **Changes (TradeGridNonTradePeriods parser/log contracts):**
  - Updated project/OsEngine/OsTrader/Grids/TradeGridNonTradePeriods.cs:
    - LoadFromString(string) -> LoadFromString(string?).
    - added empty payload guard (string.IsNullOrWhiteSpace(...)).
    - added field-by-field length/empty checks before parsing Split('@') values to prevent index overflow on malformed payloads.
    - LogMessageEvent aligned to nullable event contract (Action<string, LogMessageType>?).
    - logging dispatch changed to nullable-safe LogMessageEvent?.Invoke(message, type) with Error fallback preserved.
  - Updated tests in project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs:
    - null payload keeps preconfigured defaults.
    - short payload ("CloseOnly") parses first regime and keeps second regime default without throw.
- **Verification:**
  - Sandbox build/test verification blocked by nuget network issue (NU1301 to https://api.nuget.org/v3/index.json).
  - Host-context verification: pending.
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #648)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridsMaster load-parser/log contracts):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridsMaster.cs`:
    - replaced direct `Convert.ToInt32(gridSettings.Split('@')[0], ...)` with guarded helper parsing.
    - added `TryExtractGridNumber(string? gridSettings, out int number)` for null-safe invariant parsing of legacy payload prefix.
    - malformed/empty payload entries are skipped during load instead of throwing and terminating batch.
    - `LogMessageEvent` aligned to nullable event contract (`Action<string, LogMessageType>?`).
    - logging dispatch changed to nullable-safe `LogMessageEvent?.Invoke(message, type)` with `Error` fallback preserved.
  - Updated tests in `project/OsEngine.Tests/TradeGridsMasterPersistenceTests.cs`:
    - `TryExtractGridNumber_ShouldParseValidPrefix`
    - `TryExtractGridNumber_ShouldReturnFalse_OnMalformedPrefix`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `490/490`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #649)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid core parser/log contracts):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `LoadFromString(string)` -> `LoadFromString(string?)`.
    - added empty payload guard (`string.IsNullOrWhiteSpace(...)`).
    - added length/empty checks before parsing `%`/`@` sections to avoid index overflow on malformed payloads.
    - guarded optional subsection loads by section existence (`NonTradePeriods`, `StopBy`, `GridCreator`, `StopAndProfit`, `AutoStarter`, `ErrorsReaction`, `TrailingUp`).
    - preserved legacy fallback defaults for missing/invalid prime-tail fields:
      - delay/micro-volume -> `DelayInReal = 500`, `CheckMicroVolumes = true`
      - max-distance -> `MaxDistanceToOrdersPercent = 1.5m`
      - maker-only -> `OpenOrdersMakerOnly = true`
    - `LogMessageEvent` aligned to nullable event contract (`Action<string, LogMessageType>?`).
    - logging dispatch changed to nullable-safe `LogMessageEvent?.Invoke(message, type)` with `Error` fallback preserved.
    - error log message formatting hardened for null/deleted tab state.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGrid_LoadFromString_NullPayload_ShouldKeepConfiguredDefaults`
    - `...TradeGrid_LoadFromString_ShortPayload_ShouldParsePrefixWithoutThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `492/492`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #650)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid event contracts):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `NeedToSaveEvent`, `RePaintSettingsEvent`, `FullRePaintGridEvent` aligned to nullable event contracts (`Action?`).
    - event dispatch switched to nullable-safe invoke pattern `?.Invoke()` in:
      - `Save()`
      - `RePaintGrid()`
      - `FullRePaintGrid()`
      - `Regime` setter repaint-notification path
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGrid_EventDispatchWithoutSubscribers_ShouldNotThrow`
    - `...TradeGrid_SendNewLogMessage_WithNullTab_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `494/494`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #651)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid lifecycle guards in query methods):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added lifecycle guards to return empty collections when `GridCreator`/`Tab` are unavailable:
      - `GetPositionByGrid()`
      - `GetLinesWithOpenOrdersNeed(decimal lastPrice)`
      - `GetLinesWithOpenOrdersFact()`
      - `GetLinesWithClosingOrdersFact()`
    - guarded local snapshots used for nullable-safe access in these methods.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...GetPositionByGrid_WithNullGridCreator_ShouldReturnEmpty`
    - `...GetLinesWithOpenOrdersNeed_WithNullDependencies_ShouldReturnEmpty`
    - `...GetOpenAndClosingFact_WithNullGridCreator_ShouldReturnEmpty`
    - adjusted `...SendNewLogMessage_WithNullTab...` test to event-capture path (avoids modal UI side effect).
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `497/497`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #652)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid internal lifecycle helpers):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added `GridCreator` null guards with local snapshots in:
      - `Connector_TestStartEvent()`
      - `Tab_PositionClosingFailEvent(Position position)`
      - `Tab_PositionOpeningFailEvent(Position position)`
      - `TryDeleteOpeningFailPositions()`
      - `TryDeleteDonePositions()`
    - preserves normal behavior for valid initialized state; prevents null-reference hazards in delete/race lifecycle paths.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...PrivateLifecycleMethods_WithNullGridCreator_ShouldNotThrow`
    - added reflection helper for invoking private no-arg methods in lifecycle guard tests.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `498/498`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #653)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid query/property guards for null GridCreator):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added `GridCreator` null guards in:
      - `AllVolumeInLines`
      - `HaveOrdersWithNoMarketOrders()`
      - `HaveOrdersTryToCancelLastSecond()`
      - `GetLinesWithOpenPosition()`
    - when lifecycle dependencies are unavailable, methods return safe defaults (0/false/empty) instead of risking null-reference.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...QueryProperties_WithNullGridCreator_ShouldReturnSafeDefaults`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `499/499`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #647)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator parser/log contracts):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `LoadFromString(string)` -> `LoadFromString(string?)`.
    - added empty payload guard (`string.IsNullOrWhiteSpace(...)`).
    - added field-by-field length/empty checks before parsing `Split('@')` values to prevent index overflow on malformed payloads.
    - `LogMessageEvent` aligned to nullable event contract (`Action<string, LogMessageType>?`).
    - logging dispatch changed to nullable-safe `LogMessageEvent?.Invoke(message, type)` with `Error` fallback preserved.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - null payload keeps preconfigured defaults.
    - short payload (`Buy@101.5@3`) parses available prefix and keeps tail defaults without throw.
- **Verification:**
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --no-build --configuration Release --nologo --filter "FullyQualifiedName~TradeGridPersistenceCoreTests"` -> exit code `0` in sandbox.
  - Host-context verification (outside sandbox, per dotnet-build-policy):
    - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
    - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
    - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
    - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `488/488`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #654)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid management/event lifecycle guards):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added `GridCreator` / `Tab` guards in:
      - `CreateNewGridSafe()`
      - `CreateNewLine()`
      - `DeleteGrid()`
      - `RemoveSelected(List<int>)`
    - added `GridCreator` guard in:
      - `Tab_PositionOpeningSuccesEvent(Position position)`
    - methods now exit safely when lifecycle dependencies are unavailable (e.g., after `Delete()`), preserving initialized-state behavior.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...PublicGridManagement_WithNullGridCreator_ShouldNotThrow`
    - `...PositionOpeningSuccessHandler_WithNullGridCreator_ShouldNotThrow`
    - added private reflection helper with args support (`InvokePrivateWithArgs(...)`).
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `501/501`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #655)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid save/load lifecycle guards):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - hardened `GetSaveString()` with nullable-safe subcomponent serialization via guarded snapshots for:
      - `NonTradePeriods`, `StopBy`, `GridCreator`, `StopAndProfit`, `AutoStarter`, `ErrorsReaction`, `TrailingUp`
    - hardened `LoadFromString(string? value)` by guarding per-section `LoadFromString(...)` calls when subcomponents are null (after lifecycle cleanup via `Delete()`).
    - preserves behavior in normal initialized state and existing payload format.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...SaveLoad_WithNullSubcomponents_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `502/502`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #656)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid private trading-helper lifecycle guards):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added lifecycle guards (`Tab`/`GridCreator`) in private helper methods:
      - `TryRemoveWrongOrders()`
      - `GetOpenOrdersGridHole()`
      - `TryCancelOpeningOrders()`
      - `TrySetClosingOrders(decimal lastPrice)`
      - `CheckWrongCloseOrders()`
      - `TryCancelClosingOrders()`
      - `TrySetOpenOrders()`
      - `TryFreeJournal()`
      - `TryDeletePositionsFromJournal(Position position)`
      - `TryFindPositionsInJournalAfterReconnect()`
      - `TryForcedCloseGrid()`
    - method internals switched to guarded local snapshots where required.
    - prevents null-reference entry after lifecycle cleanup (`Delete()`), while preserving behavior in valid initialized state.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...PrivateTradingHelpers_WithNullDependencies_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `503/503`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #657)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid process-loop lifecycle guards):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - in `Process()` introduced guarded local snapshots for:
      - `Tab`, `GridCreator`, `ErrorsReaction`, `AutoStarter`, `NonTradePeriods`, `StopBy`, `TrailingUp`
    - added early return when any required lifecycle dependency is null.
    - migrated direct field dereferences inside `Process()` to guarded local snapshots.
    - preserves behavior for normal initialized runtime state and prevents null-reference entry after lifecycle cleanup (`Delete()`).
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...Process_WithNullDependencies_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `504/504`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #658)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid profit helpers and market-making lifecycle guards):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added lifecycle guards in:
      - `TrySetStopAndProfit()`
      - `TrySetLimitProfit()`
      - `TryCancelWrongCloseProfitOrders()`
      - `TrySetClosingProfitOrders(decimal lastPrice)`
      - `GridTypeMarketMakingLogic(TradeGridRegime baseRegime)`
    - switched touched methods from direct `Tab` dereferences to guarded local `tab` snapshots.
    - preserves behavior for initialized runtime and prevents null-reference entry after lifecycle cleanup.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...ProfitAndMarketMakingHelpers_WithNullDependencies_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `505/505`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #659)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid fail-event/open-position lifecycle guards):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added `ErrorsReaction` guards in:
      - `Tab_PositionClosingFailEvent(Position position)`
      - `Tab_PositionOpeningFailEvent(Position position)`
    - updated `GridTypeOpenPositionLogic(TradeGridRegime baseRegime)` to use guarded local `StopAndProfit` snapshot in profit checks.
    - updated `MaxGridPrice`/`MinGridPrice` to return `0` when `TrailingUp` is null (safe default after lifecycle cleanup).
    - preserves behavior for initialized runtime while preventing null-reference paths post-`Delete()`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...FailEventsAndOpenPositionLogic_WithNullHandlers_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `506/506`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #660)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid delete path and grid-mutator snapshot guards):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `Delete()` now uses local `tab` snapshot and `tab.Connector` guard before unsubscription from `TestStartEvent`.
    - `DeleteGrid()` switched to local guarded `gridCreator` snapshot before delete call.
    - `CreateNewLine()` switched to local guarded `gridCreator` snapshot before line creation.
    - preserves behavior for initialized runtime while preventing null-reference in partially initialized/cleaned lifecycle states.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...Delete_WithUninitializedTabConnector_ShouldNotThrow`
    - added `using OsEngine.OsTrader.Panels.Tab;` for test type resolution.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `507/507`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #661)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid delete idempotency lifecycle guards):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `Delete()` reworked to snapshot all owned lifecycle dependencies (`Tab`, `NonTradePeriods`, `StopBy`, `StopAndProfit`, `AutoStarter`, `GridCreator`, `ErrorsReaction`, `TrailingUp`) into local variables.
    - fields are nulled before event unsubscription and nested cleanup calls, reducing reentrancy/race risk and making repeated `Delete()` calls safe.
    - cleanup semantics for initialized runtime state preserved.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...Delete_CalledTwice_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `508/508`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #662)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid delete fail-safe cleanup path):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - in `Delete()`, defensive cleanup `catch` blocks no longer route exceptions to UI logging fallback.
    - cleanup now silently tolerates partial-state failures in unsubscription/component-delete phases, avoiding modal popups during teardown of uninitialized state.
    - preserves normal cleanup behavior for fully initialized runtime.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...Delete_WithFaultyUninitializedSubcomponents_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `509/509`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #663)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid null-position event-handler guards):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added early null guards for event payload in:
      - `Tab_PositionOpeningSuccesEvent(Position position)`
      - `Tab_PositionOpeningFailEvent(Position position)`
      - `Tab_PositionClosingFailEvent(Position position)`
    - prevents null-reference if handler is invoked with null event payload.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...EventHandlers_WithNullPosition_ShouldNotThrow`
    - reflection invocation fixed to pass null argument explicitly as `(object?)null`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `510/510`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #664)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid RemoveSelected lifecycle null-lines guard):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `RemoveSelected(List<int> numbers)` now reads `gridCreator.Lines` into local snapshot and returns early when lines are null/empty.
    - replaced direct `gridCreator.Lines[...]` accesses with guarded local `lines[...]`.
    - prevents null-reference in partially initialized lifecycle states.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...RemoveSelected_WithNullGridCreatorLines_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `511/511`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #665)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid connector/journal lifecycle hardening block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `Process()` now guards `tab.Connector` and `connector.MyServer` in the start-connector wait branch.
    - replaced reflection-style base-type check with direct pattern match `server is AServer`.
    - added `position == null` guard in `Tab_PositionClosingSuccesEvent(Position position)`.
    - added `tab._journal == null` guard in `TryDeletePositionsFromJournal(Position position)`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TryDeletePositionsFromJournal_WithNullJournal_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `512/512`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #666)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid order-tail safety hardening block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added helper: `TryGetLastOrder(List<Order>, out Order)` for nullable/empty-safe access to tail order.
    - replaced direct `[^1]` order dereferences with guarded helper checks in:
      - `RemoveSelected(...)`
      - `TryCancelWrongCloseProfitOrders()`
      - `GetOrdersBadPriceToGrid()`
      - `GetOrdersBadLinesMaxCount()`
      - `GetOpenOrdersGridHole()`
      - `GetCloseOrdersGridHole()`
      - `TryCancelOpeningOrders()`
      - `CheckWrongCloseOrders()`
      - `TryCancelClosingOrders()`
      - `HaveOrdersWithNoMarketOrders()`
      - `HaveOrdersTryToCancelLastSecond()`
    - mitigates malformed/partial order-list crashes while preserving valid-state logic.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...OrderStateChecks_WithEmptyOrderCollections_ShouldNotThrow`
    - added private reflection helper `SetPrivateField(...)` for malformed-state setup.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `513/513`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #667)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid lines-snapshot consistency hardening block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `CreateNewGridSafe()` now uses local `lines` snapshot instead of repeated direct `gridCreator.Lines` dereference.
    - `Tab_PositionOpeningSuccesEvent`, `Tab_PositionOpeningFailEvent`, `Tab_PositionClosingFailEvent` switched to local `lines` snapshot guards before loops.
    - `CheckWrongCloseOrders()` now returns early when `linesAll` is null/empty.
    - removes remaining null-race windows around `Lines` access after lifecycle cleanup.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...EventHandlers_WithNullGridLines_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `514/514`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #668)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid entry-point/log payload hardening block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added null-payload guards in:
      - `Tab_NewTickEvent(Trade trade)`
      - `Tab_PositionStopActivateEvent(Position obj)`
    - `SendNewLogMessage(string message, LogMessageType type)` now normalizes null `message` and uses local `tab` snapshot for bot/security context.
    - preserves behavior for valid payloads while preventing edge-case null crashes.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...EntryPointsAndLog_WithNullPayload_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `515/515`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #669)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid process/journal lines-snapshot guard block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `Process()` now takes local `lines` snapshot from `gridCreator.Lines` and guards `lines == null/empty`.
    - `TryDeletePositionsFromJournal(Position position)` now returns early on null/empty `lines`.
    - cleanup targets remaining low-risk null/consistency edges in lifecycle-sensitive paths.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...Process_WithNullGridLines_ShouldNotThrow`
    - `...TryDeletePositionsFromJournal_WithNullLines_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `517/517`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #670)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid null-security guard block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added `tab.Security` null guards and local `security` snapshot usage in:
      - `GetLinesWithOpenOrdersNeed(decimal lastPrice)`
      - `TrySetOpenOrders()`
      - `TrySetClosingOrders(decimal lastPrice)`
      - `TrySetClosingProfitOrders(decimal lastPrice)`
    - removes remaining null-risk when tab exists but security isn't initialized yet.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...GetLinesWithOpenOrdersNeed_WithNullSecurity_ShouldReturnEmpty`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `518/518`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #671)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridNonTradePeriods delete null-settings hardening block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridNonTradePeriods.cs`:
    - `Delete()` switched to local snapshots for `SettingsPeriod1/SettingsPeriod2`.
    - fields are nulled before cleanup and `Delete()` calls are null-safe (`?.Delete()`).
    - fixes null-reference on teardown after partial/uninitialized lifecycle.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGridNonTradePeriods_Delete_WithNullSettings_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `519/519`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #672)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid reconnect journal null-entry guard block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - in `TryFindPositionsInJournalAfterReconnect()`, each `positions[j]` entry is now guarded for `null` before `Number` access.
    - prevents sparse-journal list null-reference without changing match semantics.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TryFindPositionsInJournalAfterReconnect_WithNullJournalEntries_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `520/520`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #673)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridNonTradePeriods service/regime null-settings guard block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridNonTradePeriods.cs`:
    - `ShowDialogPeriod1/ShowDialogPeriod2` switched to null-safe settings invocation.
    - `GetNonTradePeriodsRegime(DateTime)` now guards null settings snapshots before calling `CanTradeThisTime`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGridNonTradePeriods_ServiceAndRegime_WithNullSettings_ShouldStaySafe`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `521/521`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #674)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopBy regime guards for null runtime context block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridStopBy.cs`:
    - `GetRegime(...)` now returns `TradeGridRegime.On` when `grid` or `tab` is null.
    - added guard for null `tab.CandlesAll` before count/index access.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGridStopBy_GetRegime_WithNullGridOrTab_ShouldReturnOn`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `522/522`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #675)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopAndProfit process null-context guard block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridStopAndProfit.cs`:
    - `Process(TradeGrid grid)` now guards missing runtime dependencies (`grid`, `GridCreator`, `Tab`, `Tab.Security`) before processing.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGridStopAndProfit_Process_WithNullRuntimeContext_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `523/523`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #676)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp runtime-context null guards block):**
  - Updated `project/OsEngine/OsTrader/Grids/TrailingUp.cs`:
    - guards added for missing `Tab` and `GridCreator` across trailing and price-range/shift helpers.
    - sparse/null lines are now skipped safely in loops.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TrailingUp_RuntimeContextMissing_ShouldStaySafe`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `524/524`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #677)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridErrorsReaction no-funds/await null-safety block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridErrorsReaction.cs`:
    - null-guards added in no-funds error path for missing runtime dependencies and sparse log objects.
    - `AwaitOnStartConnector(...)` now returns false for null server.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGridErrorsReaction_AwaitOnStartConnector_WithNullServer_ShouldReturnFalse`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `525/525`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #678)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridErrorsReaction fail-event null payload/order guards block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridErrorsReaction.cs`:
    - fail-event handlers now guard null `position`; close-fail path also guards null last close-order.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGridErrorsReaction_FailEvents_WithNullPositionOrOrder_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `526/526`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #679)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopBy null-last-candle guard block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridStopBy.cs`:
    - `GetRegime(...)` now checks last candle for null before reading `Close`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGridStopBy_GetRegime_WithNullLastCandle_ShouldReturnOn`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `527/527`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #680)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopAndProfit null-last-candle trailing guard block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridStopAndProfit.cs`:
    - `SetTrailStop(...)` now checks last candle for null before reading `Close`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGridStopAndProfit_SetTrailStop_WithNullLastCandle_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `528/528`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #681)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopAndProfit private setter null-context guards block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridStopAndProfit.cs`:
    - private `SetProfit/SetStop/SetTrailStop` now guard null runtime dependencies and use local snapshots.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGridStopAndProfit_PrivateSetters_WithNullRuntimeContext_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `529/529`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #682)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopAndProfit sparse positions null-entry guard block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridStopAndProfit.cs`:
    - `SetProfit(...)`, `SetStop(...)`, `SetTrailStop(...)` now skip null entries in `positions` lists.
    - protected all touched position-iteration loops from sparse-list null-reference.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGridStopAndProfit_PrivateSetters_WithSparsePositionsList_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `530/530`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #683)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridAutoStarter runtime/sparse lifecycle guards block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridAutoStarter.cs`:
    - `HaveEventToStart(...)` now guards null `grid`/`tab` and null tail candle.
    - `GetNewGridPriceStart(...)` now guards null `grid`/`tab`/`GridCreator`, null tail candle, and missing `Security` before rounding.
    - `ShiftGridOnNewPrice(...)` now guards null `grid`/`GridCreator`, sparse/null line entries, and derives side from first non-null line.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGridAutoStarter_RuntimeContextMissing_ShouldStaySafe`
    - `...TradeGridAutoStarter_ShiftGridOnNewPrice_WithSparseLines_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `532/532`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #684)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid sparse-lines query guards block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `GetLinesWithOpenPosition()`, `GetPositionByGrid()`, `GetLinesWithOpenOrdersNeed(...)`, `GetLinesWithOpenOrdersFact()`, `GetLinesWithClosingOrdersFact()` now skip null entries in `GridCreator.Lines`.
    - protected query/selection loops from sparse-list null-reference.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGrid_QueryMethods_WithSparseLines_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `533/533`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #685)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp null-last-candle trailing guard block):**
  - Updated `project/OsEngine/OsTrader/Grids/TrailingUp.cs`:
    - `TryTrailingGrid()` now guards null last candle entry before close-price access.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TrailingUp_TryTrailingGrid_WithNullLastCandle_ShouldReturnFalse`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `534/534`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #686)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator get-volume runtime context guards block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `GetVolume(...)` now guards null `line`.
    - for `Contracts`, returns volume before any tab/security access.
    - for tab-dependent modes, now guards null `tab` and missing `Security`.
    - deposit-percent branch now guards zero `PriceBestAsk` before division.
    - touched internals use guarded local `security` snapshot.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGridCreator_GetVolume_WithNullRuntimeContext_ShouldStaySafe`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `535/535`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #687)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator sparse-lines save-string guard block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `GetSaveLinesString()` now skips null entries in `Lines`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGridCreator_GetSaveLinesString_WithSparseLines_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `536/536`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #688)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator remove-selected null/sparse guard block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `RemoveSelected(...)` now guards null/empty `numbers`.
    - `RemoveSelected(...)` now guards null/empty `Lines`.
    - sparse/null line entry is checked before `Position` log check.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGridCreator_RemoveSelected_WithNullOrSparseInputs_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `537/537`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #689)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid order-helper null-tail candle guard block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `TryRemoveWrongOrders()`, `GetOpenOrdersGridHole()`, `TrySetOpenOrders()` now guard null last candle entry.
    - `GetOpenOrdersGridHole()` now guards null first/last lines before entry-price comparison.
    - `CheckWrongCloseOrders()` now skips null sparse lines.
    - `TrySetOpenOrders()` now skips null sparse lines in opening loop.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGrid_OrderHelpers_WithNullLastCandle_ShouldStaySafe`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `538/538`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #690)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid sparse-lines journal/order-state guard block):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `TryDeletePositionsFromJournal(...)`, `TryDeleteDonePositions()`, `TryFindPositionsInJournalAfterReconnect()` now skip null sparse lines.
    - `AllVolumeInLines` now skips null sparse lines.
    - `HaveOrdersWithNoMarketOrders()` and `HaveOrdersTryToCancelLastSecond()` now skip null sparse lines before position access.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGrid_SparseLines_JournalAndOrderStatePaths_ShouldStaySafe`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `539/539`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #691)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid sparse-lines event/lifecycle guard batch):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `Connector_TestStartEvent()` now skips null sparse lines before reset.
    - `Tab_PositionOpeningSuccesEvent(...)`, `Tab_PositionOpeningFailEvent(...)`, `Tab_PositionClosingFailEvent(...)` now skip null sparse lines while matching position numbers.
    - `TryDeleteOpeningFailPositions()` now skips null sparse lines before state checks.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGrid_EventAndLifecycle_WithSparseLines_ShouldStaySafe`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `540/540`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #692)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid order-close/cancel sparse-line guard batch):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `RemoveSelected(...)` now skips null sparse lines before open-order cancel logic.
    - `TryCancelWrongCloseProfitOrders()` now uses guarded local position reference.
    - `TrySetClosingProfitOrders(...)` now skips null lines/missing positions before checks.
    - `TryCancelOpeningOrders()` and `TryCancelClosingOrders()` now use guarded local position reference.
    - `TrySetClosingOrders(...)` now skips null lines/missing positions before checks.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGrid_RemoveSelected_WithSparseLines_ShouldNotThrow`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `541/541`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #693)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid sparse-lines forced-close/property guard batch):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `TryForcedCloseGrid()` now skips null sparse lines before position checks.
    - `OpenVolumeByLines` now skips null sparse lines before volume aggregation.
    - `HaveCloseOrders` now skips null sparse lines before close-order checks.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - `...TradeGrid_ForcedCloseAndVolume_WithSparseLines_ShouldStaySafe`
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `542/542`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #694)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator deposit-percent sparse portfolio/lot-zero guards):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `GetVolume(...)` now skips null sparse entries in `Portfolio.GetPositionOnBoard()` iteration.
    - `GetVolume(...)` deposit-percent calculation now handles `security.Lot == 0` with a safe fallback path.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - extended `...TradeGridCreator_GetVolume_WithNullRuntimeContext_ShouldStaySafe` with sparse portfolio and zero-lot scenario.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `542/542`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #695)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator contract-currency zero-price guard):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `GetVolume(...)` in `ContractCurrency` mode now guards `line.PriceEnter == 0` and returns `0`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - extended `...TradeGridCreator_GetVolume_WithNullRuntimeContext_ShouldStaySafe` with zero-entry-price contract-currency scenario.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `542/542`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #696)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator load-lines null-collection guard):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `LoadLines(...)` now initializes `Lines` when collection is null before adding deserialized lines.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `...TradeGridCreator_LoadLines_WithNullLinesCollection_ShouldNotThrow`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `543/543`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #697)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator mutators null-lines/tab guard batch):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `CreateNewGrid(...)` now returns early on null `tab`.
    - `DeleteGrid()` now guards null `Lines`.
    - `CreateNewLine()` now initializes `Lines` when collection is null.
    - `CreateMarketMakingGrid(...)` now guards null `tab` and `Lines`.
    - `GetSaveLinesString()` now returns empty string for null/empty `Lines`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `...TradeGridCreator_Mutators_WithNullLinesOrTab_ShouldNotThrow`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `544/544`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #698)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (RemoveSelected negative-index guards in TradeGrid/TradeGridCreator):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `RemoveSelected(...)` now guards negative indices before access/removal.
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `RemoveSelected(...)` now guards negative indices before line lookup.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - extended creator/remove-selected sparse-input test with negative-index case.
    - extended tradegrid/remove-selected sparse-lines test with negative-index case.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `544/544`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #699)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator GetVolume decimals-safety guard batch):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - added safe `DecimalsVolume` normalization for OsTrader rounding paths in `GetVolume(...)`.
    - contract-currency and deposit-percent branches now avoid invalid `Math.Round(..., decimals)` values.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - extended get-volume runtime-context test with negative-decimals scenarios for contract-currency and deposit-percent modes.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `544/544`
- **Commit:** n/a
- **Push:** n/a


### Step 4.2 - Nullable Annotations (Incremental Adoption #700)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator LoadLines mixed-invalid payload tolerance):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `LoadLines(...)` now catches per-line parsing failures and continues loading remaining lines.
    - malformed line entries no longer abort full deserialization flow.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadLines_WithMixedInvalidPayload_ShouldKeepValidLines`.
    - attached local `LogMessageEvent` subscriber in test to avoid modal UI fallback during expected error logging branch.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `545/545`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #701)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator LoadFromString parser tolerance batch):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - replaced exception-prone numeric conversions with guarded parse checks in `LoadFromString(...)`.
    - malformed numeric fields now preserve already loaded/default state and do not break subsequent tail parsing.
    - added helper `TryParseDecimal(...)` with `InvariantCulture`/`CurrentCulture`/`ru-RU` fallback.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadFromString_WithMalformedMiddleFields_ShouldContinueTailParsing`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `546/546`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #702)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid stale-NONE order recovery hardening batch):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `HaveOrdersWithNoMarketOrders()` now uses local `position` snapshot and safe stale-order cleanup helper.
    - introduced `TryRemoveLastOrder(List<Order>)` and replaced direct `RemoveAt(Count - 1)` calls in stale-NONE cleanup branches.
    - `HaveOrdersTryToCancelLastSecond()` aligned to local-snapshot pattern.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_HaveOrdersWithNoMarketOrders_ShouldRemoveStaleNoneOpenOrder`.
    - added `Stage2Step2_2_TradeGrid_HaveOrdersWithNoMarketOrders_ShouldRemoveStaleNoneCloseOrder`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `548/548`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #703)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid MiddleEntryPrice zero-volume guard):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `MiddleEntryPrice` now returns `0` when computed aggregate trade volume equals `0` before `summ / volume`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_MiddleEntryPrice_WithZeroTradeVolume_ShouldReturnZero`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `549/549`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #704)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid LoadFromString prime parser tolerance batch):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - replaced exception-prone prime-field conversions with guarded parse checks.
    - malformed prime fields now keep existing/default state and no longer block tail-section parsing.
    - added helpers `TryParseIntInvariant(...)` and `TryParseDecimal(...)`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedPrimeFields_ShouldContinueTailParsing`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `550/550`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #705)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid last-candle access helper consolidation):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added shared helper `TryGetLastCandle(...)`.
    - switched `TryRemoveWrongOrders()`, `GetOpenOrdersGridHole()`, and `TrySetOpenOrders()` to unified safe last-candle retrieval.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_OrderHelpers_WithEmptyCandles_ShouldStaySafe`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `551/551`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #706)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid optional-tail deterministic fallback parsing):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `LoadFromString(...)` tail fields now use explicit guarded parsing with deterministic defaults on malformed inputs.
    - defaults: `DelayInReal=500`, `CheckMicroVolumes=true`, `MaxDistanceToOrdersPercent=1.5m`, `OpenOrdersMakerOnly=true`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedOptionalTail_ShouldApplyDefaults`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `552/552`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #707)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator line-parser legacy compatibility hardening):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `TradeGridLine.SetFromStr(...)` converted to guarded parse with bool success return.
    - `LoadLines(...)` now appends only successfully parsed lines.
    - supported legacy 4-field line payload with default `PositionNum = -1`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadLines_WithLegacyLineWithoutPositionNum_ShouldLoadWithDefaultPosition`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `553/553`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #708)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid flexible boolean parser compatibility batch):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added `TryParseBoolFlexible(...)`.
    - `LoadFromString(...)` now accepts `true/false`, `1/0`, `yes/no`, `on/off` for selected bool fields.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithFlexibleBooleanTail_ShouldParse`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `554/554`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #709)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid enum parser case-insensitive compatibility batch):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added `TryParseEnumFlexible<TEnum>(...)`.
    - `LoadFromString(...)` enum fields now parse case-insensitively with guarded assignment.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithLowercaseEnumFields_ShouldParse`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `555/555`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #710)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator enum parser case-insensitive compatibility batch):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - added `TryParseEnumFlexible<TEnum>(...)`.
    - `LoadFromString(...)` enum fields now parse case-insensitively with guarded assignment.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadFromString_WithLowercaseEnumFields_ShouldParse`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `556/556`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #711)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator line payload whitespace/CRLF parser compatibility batch):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `LoadLines(...)` trims line fragments and skips whitespace-only entries.
    - `TradeGridLine.SetFromStr(...)` trims per-field values prior to parsing.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadLines_WithWhitespaceAndCrLf_ShouldParseValidLines`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `557/557`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #712)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator numeric invariant guards in LoadFromString):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - guarded parsed numeric assignments with invariant checks (`>=0`/`>0` by field semantics).
    - invalid numeric payload values no longer overwrite safe existing state.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadFromString_WithInvalidNumericInvariants_ShouldKeepSafeValues`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `558/558`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #713)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid firstTradeTime parse fallback hardening):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - introduced `TryParseDateInvariantOrCurrent(...)`.
    - `LoadFromString(...)` now assigns `_firstTradeTime` only on successful parse.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedFirstTradeTime_ShouldKeepExistingValue`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `559/559`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #714)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid prime numeric invariant guards in LoadFromString):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added `>=0` guards for prime counters/limits.
    - negative `DelayInReal` now falls back to `500`.
    - negative `MaxDistanceToOrdersPercent` now falls back to `1.5m`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithNegativeNumericLimits_ShouldKeepSafeValues`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `560/560`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #715)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridLine PositionNum parser compatibility hardening):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `PositionNum` parsing switched to flexible int parser with trim and culture fallback.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadLines_WithSpacedPositionNum_ShouldParse`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `561/561`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #716)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator TradeAsset parser normalization batch):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `TradeAssetInPortfolio` payload value is trimmed before assignment.
    - empty/whitespace trimmed asset no longer overwrites current value.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadFromString_TradeAssetWithWhitespace_ShouldBeTrimmed`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `562/562`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #717)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator full-token whitespace normalization in LoadFromString):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - added pre-trim normalization pass for all payload tokens after split.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadFromString_WithMixedWhitespaceTokens_ShouldParse`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `563/563`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #718)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid full-token whitespace normalization in LoadFromString):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added pre-trim normalization for section and prime tokens before parsing.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMixedWhitespaceTokens_ShouldParse`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `564/564`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #719)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid whitespace-only section skip guards in LoadFromString):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added `IsPayloadSegmentPresent(...)`.
    - subcomponent `LoadFromString(...)` calls now run only for non-empty section tokens.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithWhitespaceOnlySections_ShouldSkipSubcomponentParsing`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `565/565`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #720)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid Number/FirstPrice invariant guards in LoadFromString):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - added `>= 0` guards for `Number` and `_firstTradePrice` assignments from payload.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithNegativeNumberAndFirstPrice_ShouldKeepSafeValues`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `566/566`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #721)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridLine non-negative invariant guard in parser):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `TradeGridLine.SetFromStr(...)` now rejects negative `PriceEnter`, `Volume`, or `PriceExit`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadLines_WithNegativeLineValues_ShouldSkipInvalidLine`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `567/567`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #722)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridLine PositionNum lower-bound invariant guard):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `PositionNum` in `SetFromStr(...)` now requires `>= -1`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadLines_WithInvalidNegativePositionNum_ShouldSkipInvalidLine`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `568/568`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #723)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridLine side invariant guard in parser):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `TradeGridLine.SetFromStr(...)` now allows only `Side.Buy`/`Side.Sell`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadLines_WithInvalidSide_ShouldSkipInvalidLine`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `569/569`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #724)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid RegimeLogicEntry range guard in LoadFromString):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `RegimeLogicEntry` assignment now allowed only for `OnTrade`/`OncePerSecond`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithOutOfRangeRegimeLogicEntry_ShouldKeepExistingValue`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `570/570`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #725)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid GridType range guard in LoadFromString):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `GridType` assignment now allowed only for `MarketMaking`/`OpenPosition`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithOutOfRangeGridType_ShouldKeepExistingValue`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `571/571`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #726)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid Regime range guard in LoadFromString):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `Regime` assignment now allowed only for valid enum values.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithOutOfRangeRegime_ShouldKeepExistingValue`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `572/572`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #727)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid DelayInReal positive-bound guard in LoadFromString):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - delay parsing now requires `DelayInReal > 0`; otherwise applies fallback `500`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithZeroDelay_ShouldApplyDefault`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `573/573`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #728)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid CheckMicroVolumes invalid-bool preservation guard):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `CheckMicroVolumes` now preserves current value when tail bool token is malformed but delay token is present.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithInvalidMicroVolumesBool_ShouldKeepExistingValue`.
    - adjusted related malformed/legacy tail expectations to match intended semantics.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `574/574`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #729)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid optional-tail malformed-vs-missing fallback alignment):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `MaxDistanceToOrdersPercent`/`OpenOrdersMakerOnly` now use distinct behavior for missing vs malformed tail tokens.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - adjusted malformed-tail expectations.
    - added missing-tail defaults test for distance/maker-only fields.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `575/575`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #730)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator enum range guards in LoadFromString):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - guarded `GridSide` assignment to `Side.Buy`/`Side.Sell`.
    - guarded `TypeStep`/`TypeProfit` assignments to valid `TradeGridValueType` members only.
    - guarded `TypeVolume` assignment to valid `TradeGridVolumeType` members only.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadFromString_WithOutOfRangeEnumFields_ShouldKeepExistingValues`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `576/576`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #731)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator FirstPrice non-negative guard in LoadFromString):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `FirstPrice` assignment now requires non-negative parsed value.
    - negative payload value keeps existing runtime `FirstPrice`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadFromString_WithNegativeFirstPrice_ShouldKeepExistingValue`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `577/577`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #732)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator LineCountStart positive-bound guard in LoadFromString):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `LineCountStart` assignment now requires strictly positive parsed value.
    - zero payload value keeps existing runtime `LineCountStart`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadFromString_WithZeroLineCountStart_ShouldKeepExistingValue`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `578/578`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #733)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator LineStep positive-bound guard in LoadFromString):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `LineStep` assignment now requires strictly positive parsed value.
    - zero payload value keeps existing runtime `LineStep`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadFromString_WithZeroLineStep_ShouldKeepExistingValue`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `579/579`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #734)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator StepMultiplicator positive-bound regression coverage):**
  - Updated `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadFromString_WithZeroStepMultiplicator_ShouldKeepExistingValue`.
  - Locked in existing `TradeGridCreator.LoadFromString(...)` behavior:
    - zero payload value keeps existing runtime `StepMultiplicator`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `580/580`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #735)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator ProfitMultiplicator positive-bound regression coverage):**
  - Updated `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadFromString_WithZeroProfitMultiplicator_ShouldKeepExistingValue`.
  - Locked in existing `TradeGridCreator.LoadFromString(...)` behavior:
    - zero payload value keeps existing runtime `ProfitMultiplicator`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `581/581`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #736)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator computed percent-step guard in grid creation):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `CreateMarketMakingGrid(...)` now returns early when computed `curStep <= 0`.
    - prevents degenerate repeated-price grid creation from zero/invalid runtime `FirstPrice` in percent-step mode.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_CreateNewGrid_WithZeroComputedPercentStep_ShouldNotCreateDegenerateLines`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `582/582`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #737)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator negative runtime step-multiplicator guard):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `CreateMarketMakingGrid(...)` now breaks when `StepMultiplicator` makes effective `curStep <= 0`.
    - prevents invalid follow-up line generation from negative runtime `StepMultiplicator` values that bypass normal UI/parser validation.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_CreateNewGrid_WithNegativeStepMultiplicatorRuntimeValue_ShouldStopAfterFirstLine`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `583/583`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #738)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator negative runtime profit-multiplicator guard):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `CreateMarketMakingGrid(...)` now breaks when `ProfitMultiplicator` makes effective `profitStep <= 0`.
    - prevents invalid follow-up exit pricing from negative runtime `ProfitMultiplicator` values that bypass normal UI/parser validation.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_CreateNewGrid_WithNegativeProfitMultiplicatorRuntimeValue_ShouldStopAfterFirstLine`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `584/584`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #739)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator negative runtime martingale guard):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `CreateMarketMakingGrid(...)` now breaks when `MartingaleMultiplicator` makes effective `volumeCurrent <= 0`.
    - prevents invalid follow-up line generation from negative runtime `MartingaleMultiplicator` values that bypass normal UI/parser validation.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_CreateNewGrid_WithNegativeMartingaleRuntimeValue_ShouldStopAfterFirstLine`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `585/585`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #740)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator non-positive runtime price guard):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - `CreateMarketMakingGrid(...)` now breaks before line creation when `priceCurrent <= 0`.
    - prevents invalid line generation from non-positive runtime `FirstPrice` values that bypass normal UI/parser validation.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_CreateNewGrid_WithNegativeFirstPriceRuntimeValue_ShouldNotCreateLines`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `586/586`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #741)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid partial optional-tail delay parsing alignment):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - `LoadFromString(...)` now parses `DelayInReal` independently from `CheckMicroVolumes`.
    - when the delay token is present and valid but the micro-volumes token is missing, the parsed delay is preserved and `CheckMicroVolumes` falls back to default `true`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMissingMicroVolumesTail_ShouldKeepParsedDelayAndApplyDefaultMicroFlag`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `587/587`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #742)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid partial optional-tail distance parsing regression coverage):**
  - Updated `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMissingMakerOnlyTail_ShouldKeepParsedDistanceAndApplyDefaultMakerFlag`.
  - Locked in existing `TradeGrid.LoadFromString(...)` behavior:
    - when `MaxDistanceToOrdersPercent` is present but `OpenOrdersMakerOnly` is missing, parsed distance is preserved and maker-only falls back to default `true`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `588/588`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #743)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid partial optional-tail distance-missing regression coverage):**
  - Updated `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMissingDistanceTail_ShouldApplyDefaultDistanceAndKeepParsedMakerFlag`.
  - Locked in existing `TradeGrid.LoadFromString(...)` behavior:
    - when `MaxDistanceToOrdersPercent` is missing but `OpenOrdersMakerOnly` is present, default distance is applied and the parsed maker-only flag is preserved.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `589/589`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #744)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid partial optional-tail malformed-distance regression coverage):**
  - Updated `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedDistanceAndMissingMakerTail_ShouldKeepDistanceAndApplyDefaultMakerFlag`.
  - Locked in existing `TradeGrid.LoadFromString(...)` behavior:
    - when `MaxDistanceToOrdersPercent` is malformed but `OpenOrdersMakerOnly` is missing, current distance is preserved and maker-only falls back to default `true`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `590/590`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #745)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid partial optional-tail malformed-delay regression coverage):**
  - Updated `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedDelayAndMissingMicroTail_ShouldApplyDefaultDelayAndDefaultMicroFlag`.
  - Locked in existing `TradeGrid.LoadFromString(...)` behavior:
    - when `DelayInReal` is malformed and `CheckMicroVolumes` is missing, delay falls back to `500` and micro-flag falls back to default `true`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `591/591`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #746)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid partial optional-tail malformed-delay parsed-micro regression coverage):**
  - Updated `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedDelayAndValidMicroTail_ShouldApplyDefaultDelayAndParseMicroFlag`.
  - Locked in existing `TradeGrid.LoadFromString(...)` behavior:
    - when `DelayInReal` is malformed but `CheckMicroVolumes` is present and valid, delay falls back to `500` and the parsed micro-flag is still applied.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `592/592`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #747)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid partial optional-tail malformed-distance parsed-maker regression coverage):**
  - Updated `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedDistanceAndValidMakerTail_ShouldKeepDistanceAndParseMakerFlag`.
  - Locked in existing `TradeGrid.LoadFromString(...)` behavior:
    - when `MaxDistanceToOrdersPercent` is malformed but `OpenOrdersMakerOnly` is present and valid, current distance is preserved and the parsed maker-only flag is applied.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `593/593`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #748)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid partial optional-tail missing-delay parsed-micro regression coverage):**
  - Updated `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMissingDelayAndValidMicroTail_ShouldApplyDefaultDelayAndParseMicroFlag`.
  - Locked in existing `TradeGrid.LoadFromString(...)` behavior:
    - when `DelayInReal` is missing but `CheckMicroVolumes` is present and valid, delay falls back to `500` and the parsed micro-flag is still applied.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `594/594`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #749)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid partial optional-tail malformed-delay invalid-micro regression coverage):**
  - Updated `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedDelayAndInvalidMicroTail_ShouldApplyDefaultDelayAndKeepMicroFlag`.
  - Locked in existing `TradeGrid.LoadFromString(...)` behavior:
    - when `DelayInReal` is malformed and `CheckMicroVolumes` is present but malformed, delay falls back to `500` and the current micro-flag is preserved.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `595/595`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #750)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid partial optional-tail missing-distance invalid-maker regression coverage):**
  - Updated `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMissingDistanceAndInvalidMakerTail_ShouldApplyDefaultDistanceAndKeepMakerFlag`.
  - Locked in existing `TradeGrid.LoadFromString(...)` behavior:
    - when `MaxDistanceToOrdersPercent` is missing and `OpenOrdersMakerOnly` is present but malformed, default distance is applied and the current maker-only flag is preserved.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `596/596`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #751)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid partial optional-tail malformed-distance invalid-maker regression coverage):**
  - Updated `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMalformedDistanceAndInvalidMakerTail_ShouldKeepDistanceAndKeepMakerFlag`.
  - Locked in existing `TradeGrid.LoadFromString(...)` behavior:
    - when `MaxDistanceToOrdersPercent` and `OpenOrdersMakerOnly` are both malformed, the current distance and current maker-only flag are both preserved.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `597/597`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #752)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid partial optional-tail missing-delay invalid-micro regression coverage):**
  - Updated `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithMissingDelayAndInvalidMicroTail_ShouldApplyDefaultDelayAndKeepMicroFlag`.
  - Locked in existing `TradeGrid.LoadFromString(...)` behavior:
    - when `DelayInReal` is missing and `CheckMicroVolumes` is present but malformed, delay falls back to `500` and the current micro-flag is preserved.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `598/598`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #753)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid malformed prime-bool regression coverage):**
  - Updated `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_LoadFromString_WithInvalidAutoClearBool_ShouldKeepExistingValue`.
  - Locked in existing `TradeGrid.LoadFromString(...)` behavior:
    - when `AutoClearJournalIsOn` token is malformed, the current bool value is preserved.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `599/599`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #754)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp malformed-bool parser hardening):**
  - Updated `project/OsEngine/OsTrader/Grids/TrailingUp.cs`:
    - `LoadFromString(...)` now uses guarded flexible bool parsing instead of `Convert.ToBoolean(...)`.
    - malformed bool tokens no longer abort the whole parse; current flag values are preserved and later fields still parse.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithInvalidBoolToken_ShouldKeepBoolAndContinueParsing`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `600/600`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #755)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp malformed-decimal parser hardening):**
  - Updated `project/OsEngine/OsTrader/Grids/TrailingUp.cs`:
    - `LoadFromString(...)` now uses guarded decimal parsing for numeric fields.
    - malformed decimal tokens no longer abort the whole parse; current numeric values are preserved and later fields still parse.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithInvalidDecimalToken_ShouldKeepDecimalAndContinueParsing`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `601/601`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #756)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp negative step parser guard):**
  - Updated `project/OsEngine/OsTrader/Grids/TrailingUp.cs`:
    - `LoadFromString(...)` now ignores negative `TrailingUpStep` payload values.
    - current step value is preserved and later fields still parse.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithNegativeTrailingUpStep_ShouldKeepExistingValue`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `602/602`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #757)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp negative down-step parser guard):**
  - Updated `project/OsEngine/OsTrader/Grids/TrailingUp.cs`:
    - `LoadFromString(...)` now ignores negative `TrailingDownStep` payload values.
    - current down-step value is preserved and later fields still parse.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithNegativeTrailingDownStep_ShouldKeepExistingValue`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `604/604`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #758)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp negative up-limit parser guard):**
  - Updated `project/OsEngine/OsTrader/Grids/TrailingUp.cs`:
    - `LoadFromString(...)` now ignores negative `TrailingUpLimit` payload values.
    - current up-limit value is preserved and later fields still parse.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithNegativeTrailingUpLimit_ShouldKeepExistingValue`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `604/604`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #759)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp negative down-limit parser guard):**
  - Updated `project/OsEngine/OsTrader/Grids/TrailingUp.cs`:
    - `LoadFromString(...)` now ignores negative `TrailingDownLimit` payload values.
    - current down-limit value is preserved and later fields still parse.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithNegativeTrailingDownLimit_ShouldKeepExistingValue`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `605/605`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #760)

- **Status:** In Progress (increment completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp move-flag partial-tail regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithInvalidUpMoveFlag_ShouldKeepValueAndParseDownMoveFlag`.
  - Locked behavior where malformed `TrailingUpCanMoveExitOrder` keeps the current value, while valid `TrailingDownCanMoveExitOrder` still parses and applies.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `606/606`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #761-#763)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp move-flag tail matrix regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithInvalidDownMoveFlag_ShouldParseUpMoveFlagAndKeepValue`.
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithMissingUpMoveFlag_ShouldKeepValueAndParseDownMoveFlag`.
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithInvalidBothMoveFlags_ShouldKeepExistingValues`.
  - Locked the remaining malformed/missing move-flag tail combinations around `TrailingUpCanMoveExitOrder` and `TrailingDownCanMoveExitOrder`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `609/609`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #764-#767)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp runtime positivity guards):**
  - Updated `project/OsEngine/OsTrader/Grids/TrailingUp.cs`:
    - `TryTrailingGrid()` now requires strictly positive step and limit values before running trailing-up or trailing-down logic.
    - this prevents negative runtime state from bypassing parser guards and shifting the grid in the wrong direction or without a valid lower bound.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TrailingUp_TryTrailingGrid_WithNegativeUpStepRuntimeValue_ShouldNotShiftGrid`.
    - added `Stage2Step2_2_TrailingUp_TryTrailingGrid_WithNegativeUpLimitRuntimeValue_ShouldNotShiftGrid`.
    - added `Stage2Step2_2_TrailingUp_TryTrailingGrid_WithNegativeDownStepRuntimeValue_ShouldNotShiftGrid`.
    - added `Stage2Step2_2_TrailingUp_TryTrailingGrid_WithNegativeDownLimitRuntimeValue_ShouldNotShiftGrid`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `613/613`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #768-#771)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp zero-value runtime regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TrailingUp_TryTrailingGrid_WithZeroUpStepRuntimeValue_ShouldNotShiftGrid`.
    - added `Stage2Step2_2_TrailingUp_TryTrailingGrid_WithZeroUpLimitRuntimeValue_ShouldNotShiftGrid`.
    - added `Stage2Step2_2_TrailingUp_TryTrailingGrid_WithZeroDownStepRuntimeValue_ShouldNotShiftGrid`.
    - added `Stage2Step2_2_TrailingUp_TryTrailingGrid_WithZeroDownLimitRuntimeValue_ShouldNotShiftGrid`.
  - Locked zero-valued runtime `step/limit` behavior across both trailing directions.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `617/617`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #772-#775)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp zero-value parser guards):**
  - Updated `project/OsEngine/OsTrader/Grids/TrailingUp.cs`:
    - `LoadFromString(...)` now ignores zero-valued `TrailingUpStep`, `TrailingUpLimit`, `TrailingDownStep`, and `TrailingDownLimit`.
    - this aligns parser behavior with the existing runtime `> 0` invariant in `TryTrailingGrid()`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithZeroTrailingUpStep_ShouldKeepExistingValue`.
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithZeroTrailingUpLimit_ShouldKeepExistingValue`.
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithZeroTrailingDownStep_ShouldKeepExistingValue`.
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithZeroTrailingDownLimit_ShouldKeepExistingValue`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `621/621`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #776-#779)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp flexible bool regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithFlexiblePrimaryBools_ShouldParse`.
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithFlexibleMoveFlagBools_ShouldParse`.
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithNumericPrimaryBools_ShouldParse`.
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithNumericMoveFlagBools_ShouldParse`.
  - Locked mixed legacy and numeric bool forms across primary trailing toggles and move-flag tail fields.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `625/625`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #780-#783)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp primary-bool partial-payload regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithMissingUpBool_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithMissingDownBool_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithInvalidUpBoolAndFlexibleDownBool_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TrailingUp_LoadFromString_WithFlexibleUpBoolAndInvalidDownBool_ShouldKeepValueAndContinueParsing`.
  - Locked missing/malformed primary-bool behavior while valid numeric fields and the opposite bool token still continue to parse.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `629/629`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #784-#789)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridErrorsReaction parser hardening):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridErrorsReaction.cs`:
    - `LoadFromString(...)` now uses flexible bool parsing and guarded positive-int parsing instead of exception-driven conversions.
    - optional tail fields now parse independently, so one malformed tail token no longer resets the whole optional tail to defaults.
    - invalid or non-positive numeric tokens now preserve current configured values.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithInvalidBoolToken_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithInvalidCountToken_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithInvalidWaitBoolAndValidTail_ShouldKeepBoolAndContinueTailParsing`.
    - added `Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithInvalidWaitSecondsAndValidReduceFlag_ShouldKeepSecondsAndParseReduceFlag`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `633/633`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #790-#793)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridErrorsReaction guarded-parser regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithZeroCounts_ShouldKeepExistingValues`.
    - added `Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithNegativeWaitSeconds_ShouldKeepExistingValue`.
    - added `Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithFlexiblePrimaryBools_ShouldParse`.
    - added `Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithFlexibleOptionalBools_ShouldParse`.
  - Locked non-positive numeric and flexible-bool behavior after the new guarded parser changes.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `637/637`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #794-#797)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridErrorsReaction optional-tail partial-payload regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithMissingWaitBool_ShouldKeepValueAndContinueTailParsing`.
    - added `Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithMissingWaitSeconds_ShouldKeepValueAndParseReduceFlag`.
    - added `Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithMissingReduceFlag_ShouldKeepValueAndPreserveParsedTailPrefix`.
    - added `Stage2Step2_2_TradeGridErrorsReaction_LoadFromString_WithInvalidWaitBoolAndMissingWaitSeconds_ShouldKeepValuesAndParseReduceFlag`.
  - Locked missing and mixed optional-tail behavior for the three optional tail fields after the parser-hardening changes.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `641/641`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #798-#805)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridAutoStarter parser hardening):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridAutoStarter.cs`:
    - `LoadFromString(...)` now uses guarded enum, decimal, bool, and range-checked int parsing instead of exception-driven conversions.
    - time-section fields now parse independently, avoiding coarse fallback behavior when one tail token is malformed.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithMalformedFields_ShouldKeepValuesAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithFlexibleTimeBools_ShouldParse`.
    - added `Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithOutOfRangeTimeFields_ShouldKeepExistingValues`.
    - added `Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithMissingTimeFlag_ShouldKeepValueAndContinueTailParsing`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `645/645`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #806-#809)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridAutoStarter time-tail partial-payload regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithMissingHour_ShouldKeepValueAndContinueTailParsing`.
    - added `Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithMissingMinute_ShouldKeepValueAndContinueTailParsing`.
    - added `Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithMissingSecond_ShouldKeepValueAndParseSingleActivation`.
    - added `Stage2Step2_2_TradeGridAutoStarter_LoadFromString_WithInvalidHourAndValidMinuteSecond_ShouldKeepHourAndContinueTailParsing`.
  - Locked missing and mixed invalid time-tail behavior after the new independent parser logic.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `649/649`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #810-#815)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopAndProfit parser hardening):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridStopAndProfit.cs`:
    - `LoadFromString(...)` now uses guarded enum parsing, guarded positive-decimal parsing, and flexible bool parsing instead of exception-driven conversions.
    - invalid or non-positive numeric values now preserve the current configured values.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithMalformedFields_ShouldKeepValuesAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithNonPositiveValues_ShouldKeepExistingValues`.
    - added `Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithFlexibleStopTradingBool_ShouldParse`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `652/652`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #816-#819)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopAndProfit late-tail regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithMissingTrailStopRegime_ShouldKeepValueAndContinueTailParsing`.
    - added `Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithInvalidTrailStopType_ShouldKeepValueAndContinueTailParsing`.
    - added `Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithMissingTrailStopValue_ShouldKeepValueAndParseStopTradingBool`.
    - added `Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithInvalidStopTradingBool_ShouldKeepValue`.
  - Locked partial and malformed behavior in the late tail of the stop/profit parser.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `656/656`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #820-#823)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopAndProfit prefix regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithMissingProfitRegime_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithInvalidProfitType_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithMissingProfitValue_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopAndProfit_LoadFromString_WithInvalidStopRegime_ShouldKeepValueAndContinueParsing`.
  - Locked partial and malformed behavior in the prefix part of the stop/profit parser.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `660/660`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #824-#831)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopBy parser hardening):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridStopBy.cs`:
    - `LoadFromString(...)` now uses flexible bool parsing, guarded positive decimal/int parsing, range-checked time parsing, and guarded enum parsing instead of exception-driven conversions.
    - invalid or non-positive numeric values now preserve current configured values.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMalformedFields_ShouldKeepValuesAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithNonPositiveValues_ShouldKeepExistingValues`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithFlexibleBools_ShouldParse`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithOutOfRangeTimeFields_ShouldKeepExistingValues`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `664/664`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #832-#835)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopBy partial-payload regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingMoveDownBool_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithInvalidPositionsReaction_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingLifeTimeSeconds_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithInvalidTimeHourAndValidTail_ShouldKeepHourAndContinueParsing`.
  - Locked missing and mixed malformed behavior across the mid/tail sections of the stop-by parser.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `668/668`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #836-#840)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopBy prefix/time-tail regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingMoveUpBool_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithInvalidMoveUpReaction_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingTimeMinute_ShouldKeepValueAndContinueTailParsing`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingTimeSecond_ShouldKeepValueAndParseReaction`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithInvalidTimeReaction_ShouldKeepValue`.
  - Locked remaining prefix and late time-tail behavior after the parser-hardening changes.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `677/677`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #841-#844)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopBy remaining parser regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingMoveUpValue_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithInvalidMoveDownReaction_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingPositionsCountValue_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingTimeFlag_ShouldKeepValueAndContinueTailParsing`.
  - Locked remaining early/mid parser gaps after the stop-by parser hardening.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `677/677`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #845-#848)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopBy symmetric parser regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingMoveDownValue_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingPositionsReaction_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithInvalidLifeTimeReaction_ShouldKeepValueAndContinueParsing`.
    - added `Stage2Step2_2_TradeGridStopBy_LoadFromString_WithMissingTimeHour_ShouldKeepValueAndContinueTailParsing`.
  - Locked another symmetric set of missing/malformed parser cases without changing production code.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `681/681`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #849-#853)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopBy runtime trigger regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithPositionsCountLimitReached_ShouldReturnConfiguredReaction`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithMoveUpLimitReached_ShouldReturnConfiguredReaction`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithMoveDownLimitReached_ShouldReturnConfiguredReaction`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithLifeTimeExpired_ShouldReturnConfiguredReaction`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithTimeOfDayReached_ShouldReturnConfiguredReaction`.
  - Locked direct runtime transition behavior for each primary stop trigger path.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `686/686`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #854-#858)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopBy runtime non-trigger regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithPositionsCountBelowLimit_ShouldReturnOn`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithMoveUpBelowLimit_ShouldReturnOn`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithMoveDownAboveLimit_ShouldReturnOn`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithLifeTimeNotExpired_ShouldReturnOn`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithTimeOfDayNotReached_ShouldReturnOn`.
  - Locked the symmetric non-trigger paths so near-threshold runtime state keeps the regime unchanged.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `691/691`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #859-#863)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopBy runtime safe-path regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithNoTriggersEnabled_ShouldReturnOn`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithEmptyCandles_ShouldReturnOn`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithZeroFirstPrice_ShouldReturnOn`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithMissingFirstTradeTime_ShouldReturnOn`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithMissingServerTime_ShouldReturnOn`.
  - Locked safe-path behavior so incomplete runtime context keeps the regime unchanged.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `696/696`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #864-#868)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopBy runtime precedence regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithPositionsAndMoveTriggersReady_ShouldPreferPositionsReaction`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithMoveAndLifeTimeTriggersReady_ShouldPreferMoveReaction`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithLifeTimeAndTimeTriggersReady_ShouldPreferLifeTimeReaction`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithOnlyLaterTriggerReady_ShouldReturnLaterReaction`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithMoveUpAndMoveDownBothReady_ShouldPreferMoveUpReaction`.
  - Locked ordering semantics across multiple simultaneously-ready runtime triggers.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `701/701`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #869-#872)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopBy runtime boundary regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithZeroLastPrice_ShouldReturnOn`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithTimeOfDayLaterMinute_ShouldReturnConfiguredReaction`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithTimeOfDayLaterHour_ShouldReturnConfiguredReaction`.
    - added `Stage2Step2_2_TradeGridStopBy_GetRegime_WithTimeOfDayFutureWithinSameDay_ShouldReturnOn`.
  - Locked remaining inner-branch boundary behavior in the move/time-of-day runtime paths.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `705/705`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #873-#877)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridsMaster helper parser regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridsMaster_ParseLegacyGridsSettings_WithWhitespaceContent_ShouldReturnNull`.
    - added `Stage2Step2_2_TradeGridsMaster_ParseLegacyGridsSettings_WithMultilineContent_ShouldCollectNonEmptyLines`.
    - added `Stage2Step2_2_TradeGridsMaster_TryExtractGridNumber_WithSeparator_ShouldParseNumber`.
    - added `Stage2Step2_2_TradeGridsMaster_TryExtractGridNumber_WithoutSeparator_ShouldParseNumber`.
    - added `Stage2Step2_2_TradeGridsMaster_TryExtractGridNumber_WithInvalidPrefix_ShouldReturnFalse`.
  - Locked private legacy/settings helper contracts without changing production code.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `710/710`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #878-#881)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridsMaster helper edge-case regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridsMaster_ParseLegacyGridsSettings_WithSingleLine_ShouldReturnSingleEntry`.
    - added `Stage2Step2_2_TradeGridsMaster_TryExtractGridNumber_WithNullInput_ShouldReturnFalse`.
    - added `Stage2Step2_2_TradeGridsMaster_TryExtractGridNumber_WithLeadingSeparator_ShouldReturnFalse`.
    - added `Stage2Step2_2_TradeGridsMaster_TryExtractGridNumber_WithWhitespaceNumberPart_ShouldReturnFalse`.
  - Extended helper coverage across the remaining invalid-prefix extraction cases.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `714/714`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #882-#887)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridsMaster runtime safe-path regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridsMaster_GetGridsSettingsPath_ShouldComposeExpectedPath`.
    - added `Stage2Step2_2_TradeGridsMaster_Clear_WithEmptyCollectionInOptimizerMode_ShouldNotThrow`.
    - added `Stage2Step2_2_TradeGridsMaster_Delete_WithOptimizerMode_ShouldClearTabAndNotThrow`.
    - added `Stage2Step2_2_TradeGridsMaster_DeleteAtNum_WithMissingNumberInOptimizerMode_ShouldNotThrow`.
    - added `Stage2Step2_2_TradeGridsMaster_StopPaint_WithNullHost_ShouldNotThrow`.
    - added `Stage2Step2_2_TradeGridsMaster_LoadAndPaint_WithSafeEarlyReturns_ShouldNotThrow`.
  - Locked runtime-safe service paths and private early-return paths without changing production behavior.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `720/720`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #888-#891)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridsMaster orchestration regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridsMaster_SaveGrids_WithOptimizerMode_ShouldNotThrow`.
    - added `Stage2Step2_2_TradeGridsMaster_ShowDialog_WithMissingGrid_ShouldNotThrow`.
    - added `Stage2Step2_2_TradeGridsMaster_UiClosed_WithNullTradeGridEntry_ShouldCleanList`.
    - added `Stage2Step2_2_TradeGridsMaster_UiClosed_WithUnknownSender_ShouldKeepOtherEntries`.
  - Extended coverage into the remaining safe orchestration branches around save/show/closed handling.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `724/724`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #892-#894)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridsMaster paint-helper regression coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridsMaster_GetGridRow_ShouldBuildExpectedCells`.
    - added `Stage2Step2_2_TradeGridsMaster_GetLastRow_ShouldBuildAddButtonRow`.
    - added `Stage2Step2_2_TradeGridsMaster_GridViewDataError_ShouldNotThrow`.
  - Locked the remaining private paint-helper contracts without changing production behavior.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `727/727`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #895-#896)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridNonTradePeriods enum parser hardening):**
  - Updated `project/OsEngine/OsTrader/Grids/TradeGridNonTradePeriods.cs`:
    - replaced raw `Enum.TryParse(...)` assignments with guarded case-insensitive enum parsing that preserves current values on invalid tokens.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridNonTradePeriods_LoadFromString_WithInvalidEnumTokens_ShouldKeepExistingValues`.
    - added `Stage2Step2_2_TradeGridNonTradePeriods_LoadFromString_WithCaseInsensitiveEnums_ShouldParse`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `729/729`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #897-#899)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridNonTradePeriods runtime priority coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridNonTradePeriods_GetRegime_WithFirstPeriodBlocked_ShouldPreferFirstRegime`.
    - added `Stage2Step2_2_TradeGridNonTradePeriods_GetRegime_WithSecondPeriodBlocked_ShouldReturnSecondRegime`.
    - added `Stage2Step2_2_TradeGridNonTradePeriods_GetRegime_WithBothPeriodsOpen_ShouldReturnOn`.
  - Locked runtime priority behavior across the first and second non-trade period buckets.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `732/732`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #900-#901)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridNonTradePeriods service contract coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridNonTradePeriods_GetSaveString_ShouldKeepReservedTailShape`.
    - added `Stage2Step2_2_TradeGridNonTradePeriods_Delete_ShouldClearSettingsAndStayIdempotent`.
  - Locked the remaining save-string and delete-idempotency contracts for the non-trade-period wrapper.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `734/734`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #902-#903)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid event dispatch positive coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_EventDispatch_WithSubscribers_ShouldInvokeEachHandlerOnce`.
    - added `Stage2Step2_2_TradeGrid_SendNewLogMessage_WithSubscriber_ShouldForwardMessage`.
  - Locked the positive subscriber paths for the public event and log-dispatch entry points.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `736/736`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #904-#905)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid helper safe defaults coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_GetOrdersBadLinesMaxCount_WithNullOpenLines_ShouldReturnEmpty`.
    - added `Stage2Step2_2_TradeGrid_GetCloseOrdersGridHole_WithNullOpenPositions_ShouldReturnEmpty`.
  - Locked two remaining helper methods to their safe empty-list defaults under null/sparse state.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `738/738`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #906)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid bad-price helper default coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_GetOrdersBadPriceToGrid_WithNullOrderLines_ShouldReturnEmpty`.
  - Locked `GetOrdersBadPriceToGrid()` to its safe empty-list default under null/sparse state.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `739/739`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #907-#908)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid open-hole direct guard coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_GetOpenOrdersGridHole_WithNullTab_ShouldReturnNull`.
    - added `Stage2Step2_2_TradeGrid_GetOpenOrdersGridHole_WithNullGridCreator_ShouldReturnNull`.
  - Locked the direct early-return guards at the `GetOpenOrdersGridHole()` entry point.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `741/741`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #909-#910)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid negative runtime limit guards):**
  - Updated runtime logic in `project/OsEngine/OsTrader/Grids/TradeGrid.cs`:
    - normalized `MaxOpenOrdersInMarket` to `Math.Max(0, ...)` inside `GetOrdersBadLinesMaxCount()`.
    - normalized `MaxCloseOrdersInMarket` to `Math.Max(0, ...)` inside `GetCloseOrdersGridHole()`.
    - blocked negative runtime values from producing negative indexing or list overrun in helper loops.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_GetOrdersBadLinesMaxCount_WithNegativeLimit_ShouldTreatAsZero`.
    - added `Stage2Step2_2_TradeGrid_GetCloseOrdersGridHole_WithNegativeLimit_ShouldTreatAsZero`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `743/743`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #911-#914)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid private helper direct return contracts):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_TryRemoveWrongOrders_WithNullDependencies_ShouldReturnZero`.
    - added `Stage2Step2_2_TradeGrid_TryCancelOpeningOrders_WithNullTab_ShouldReturnZero`.
    - added `Stage2Step2_2_TradeGrid_TryCancelClosingOrders_WithNullTab_ShouldReturnZero`.
    - added `Stage2Step2_2_TradeGrid_TrySetClosingOrders_WithNullSecurity_ShouldStayNoOp`.
  - Locked the direct early-return and no-op behavior of the private trading helpers so later refactors keep the same null-safe contracts.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `747/747`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #915-#918)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid order-state timing helper coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_HaveOrdersWithNoMarketOrders_WithFreshNoneOrder_ShouldPrimeTimestampAndReturnTrue`.
    - added `Stage2Step2_2_TradeGrid_HaveOrdersWithNoMarketOrders_WithoutNoneOrders_ShouldResetTimestampAndReturnFalse`.
    - added `Stage2Step2_2_TradeGrid_HaveOrdersTryToCancelLastSecond_WithRecentOpenCancel_ShouldReturnTrue`.
    - added `Stage2Step2_2_TradeGrid_HaveOrdersTryToCancelLastSecond_WithExpiredCloseCancel_ShouldReturnFalse`.
  - Locked the internal timing and state-transition contracts around `_lastNoneOrderTime` and the 3-second recent-cancel window.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `751/751`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #919-#922)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid property-level positive runtime coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_HaveOpenPositionsByGrid_WithOpenVolumeLine_ShouldReturnTrue`.
    - added `Stage2Step2_2_TradeGrid_HaveOrdersInMarketInGrid_WithActiveOpenOrder_ShouldReturnTrue`.
    - added `Stage2Step2_2_TradeGrid_HaveOrdersInMarketInGrid_WithActiveCloseOrder_ShouldReturnTrue`.
    - added `Stage2Step2_2_TradeGrid_HaveCloseOrders_WithActiveCloseOrder_ShouldReturnTrue`.
  - Locked the positive-path contracts of the core state-query properties so future refactors keep the same truth conditions.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `755/755`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #923-#926)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid middle-price coverage + test stabilization):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_MiddleEntryPrice_WithWeightedTrades_ShouldReturnAveragePrice`.
    - added `Stage2Step2_2_TradeGrid_MiddleEntryPrice_WithSparseOrders_ShouldIgnoreNullsAndEmptyTrades`.
    - hardened `Stage2Step2_2_TradeGridStopBy_GetRegime_WithTimeOfDayLaterMinute_ShouldReturnConfiguredReaction` against `DateTime.Now` boundary flakiness.
  - Locked the positive-path `MiddleEntryPrice` behavior and removed a known time-of-day test race.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `757/757`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #927-#928)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid volume query coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_OpenVolumeByLines_WithMultipleOpenPositions_ShouldSumOpenVolume`.
    - added `Stage2Step2_2_TradeGrid_AllVolumeInLines_WithSparseLines_ShouldSumLineVolumes`.
  - Locked the positive-path summation contracts for the two core volume-query properties.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `759/759`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #929)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid max/min wrapper coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_MaxMinGridPrice_WithSparseLines_ShouldForwardTrailingBounds`.
  - Locked the positive pass-through contract of the `TradeGrid.MaxGridPrice` / `TradeGrid.MinGridPrice` wrappers over `TrailingUp`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `760/760`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #930-#931)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid open/close line selector coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_GetLinesWithOpenOrdersNeed_WithEligibleBuyLine_ShouldReturnLine`.
    - added `Stage2Step2_2_TradeGrid_GetLinesWithClosingOrdersFact_WithActiveCloseLine_ShouldReturnLine`.
  - Locked the positive-path helper contracts for the core open/close line selectors.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `762/762`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #932-#933)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid position selector coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_GetLinesWithOpenPosition_WithSparseLines_ShouldReturnOnlyOpenVolumeLines`.
    - added `Stage2Step2_2_TradeGrid_GetPositionByGrid_WithSparseLines_ShouldReturnOnlyExistingPositions`.
  - Locked the positive-path query contracts for the two core position selectors.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `764/764`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #934-#935)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid trivial getter coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_OpenPositionsCount_ShouldReturnBackingField`.
    - added `Stage2Step2_2_TradeGrid_FirstTradeTime_ShouldReturnBackingField`.
  - Locked the direct pass-through contracts of the two core primitive getters.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `766/766`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #936-#938)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid primitive getter + regime event coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_FirstPriceReal_ShouldReturnBackingField`.
    - added `Stage2Step2_2_TradeGrid_Regime_SetSameValue_ShouldNotRaiseRepaintEvents`.
    - added `Stage2Step2_2_TradeGrid_Regime_SetNewValue_ShouldRaiseRepaintEventsOnce`.
  - Locked the direct getter contract for `FirstPriceReal` and the side-effect contract of the `Regime` property.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `769/769`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #939-#940)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TrailingUp service helper coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TrailingUp_GetSaveString_ShouldKeepReservedTailShape`.
    - added `Stage2Step2_2_TrailingUp_Delete_ShouldClearGridAndStayIdempotent`.
  - Locked the reserved save-string shape and the idempotent delete behavior of `TrailingUp`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `771/771`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #941-#942)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridErrorsReaction service helper coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridErrorsReaction_GetSaveString_ShouldKeepReservedTailShape`.
    - added `Stage2Step2_2_TradeGridErrorsReaction_Delete_ShouldClearGridAndStayIdempotent`.
  - Locked the reserved save-string shape and the idempotent delete behavior of `TradeGridErrorsReaction`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `773/773`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #943)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridStopAndProfit save-string coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopAndProfit_GetSaveString_ShouldKeepReservedTailShape`.
  - Locked the reserved save-string shape of `TradeGridStopAndProfit`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `774/774`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #944)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridAutoStarter save-string coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridAutoStarter_GetSaveString_ShouldKeepReservedTailShape`.
  - Locked the reserved save-string shape of `TradeGridAutoStarter`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `775/775`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #945-#947)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (batched save-string contract coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopBy_GetSaveString_ShouldKeepReservedTailShape`.
    - added `Stage2Step2_2_TradeGridCreator_GetSaveString_ShouldKeepReservedTailShape`.
    - added `Stage2Step2_2_TradeGridLine_GetSaveStr_ShouldUseInvariantCultureAndTrailingSeparator`.
  - Locked three remaining serialization/save-string contracts in one batch, including the exact trailing-separator shape of `TradeGridCreator`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `778/778`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #948-#949)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid top-level service coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_GetSaveString_ShouldComposePrimeAndSectionPayloads`.
    - added `Stage2Step2_2_TradeGrid_Delete_WithInitializedComponents_ShouldClearReferencesAndStayIdempotent`.
  - Locked the exact composite save-string shape of the top-level `TradeGrid` and the idempotent cascading cleanup behavior of `Delete()`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `780/780`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #950-#951)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator save-lines helper coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_GetSaveLinesString_WithEmptyLines_ShouldReturnEmptyString`.
    - added `Stage2Step2_2_TradeGridCreator_GetSaveLinesString_WithMultipleLines_ShouldConcatenateWithCaretSeparators`.
  - Locked the exact shape contracts of `GetSaveLinesString()`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `782/782`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #952-#954)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridLine parse contract coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridLine_SetFromStr_WithValidPayload_ShouldParseAllFields`.
    - added `Stage2Step2_2_TradeGridLine_SetFromStr_WithNegativeValue_ShouldReturnFalseAndKeepDefaults`.
    - added `Stage2Step2_2_TradeGridLine_SetFromStr_WithInvalidSide_ShouldReturnFalse`.
  - Locked the accept/reject contracts of `TradeGridLine.SetFromStr()`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `785/785`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #955)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator LoadLines blank-input coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_LoadLines_WithNullOrWhitespacePayload_ShouldKeepExistingLines`.
  - Locked the null/empty/whitespace early-return contract of `LoadLines()`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `786/786`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #956-#957)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridLine SetFromStr boundary coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridLine_SetFromStr_WithWhitespacePayload_ShouldReturnFalse`.
    - added `Stage2Step2_2_TradeGridLine_SetFromStr_WithInvalidPositionNum_ShouldKeepDefaultPosition`.
  - Locked two remaining boundary contracts of `SetFromStr()`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `788/788`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #958)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGrid event helper no-subscriber coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGrid_EventDispatch_WithoutSubscribers_ShouldNotThrow`.
  - Locked the no-subscriber safe path of the three simple event helpers.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `789/789`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #959-#960)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridCreator GetVolume positive-path coverage):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridCreator_GetVolume_WithContractCurrency_ShouldReturnRoundedVolume`.
    - added `Stage2Step2_2_TradeGridCreator_GetVolume_WithDepositPercentPrime_ShouldReturnRoundedVolume`.
  - Locked two positive runtime paths of `GetVolume()`.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `791/791`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #961-#962)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (TradeGridsMaster DataError logging + pending GetVolume test alignment):**
  - Updated production code in `project/OsEngine/OsTrader/Grids/TradeGridsMaster.cs`:
    - replaced `_gridViewInstances_DataError(...)` logging of `e.ToString()` with real exception-first logging.
    - added fallback diagnostic text with row, column, and `DataError` context when `e.Exception` is null.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - aligned the pending local `TradeGridCreator.GetVolume()` test with the actual tester-mode contract.
    - renamed/updated `Stage2Step2_2_TradeGridCreator_GetVolume_WithPriceStepCostModeInTester_ShouldUseStandardFormula`.
  - Outcome:
    - the UI no longer degrades to the useless `System.Windows.Forms.DataGridViewDataErrorEventArgs` message on this path.
    - the local failing test is resolved and the suite is green again.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `794/794`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #963-#965)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (remaining service log forwarding contracts):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopBy_SendNewLogMessage_WithSubscriber_ShouldForwardMessage`.
    - added `Stage2Step2_2_TradeGridStopAndProfit_SendNewLogMessage_WithSubscriber_ShouldForwardMessage`.
    - added `Stage2Step2_2_TradeGridAutoStarter_SendNewLogMessage_WithSubscriber_ShouldForwardMessage`.
  - Outcome:
    - locked simple non-UI event forwarding contracts for three remaining grid helper services.
    - verified that attached subscribers receive the original message and type without mutation.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `797/797`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #966-#968)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (no-subscriber safe paths for helper log forwarding):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridStopBy_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow`.
    - added `Stage2Step2_2_TradeGridStopAndProfit_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow`.
    - added `Stage2Step2_2_TradeGridAutoStarter_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow`.
  - Outcome:
    - locked the no-subscriber safe no-op contract for the three remaining helper `SendNewLogMessage(...)` wrappers.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - First full run hit a transient unrelated file-lock failure:
    - `AServerParamsPersistenceTests.LoadParam_ShouldSupportLegacyLineBasedFormat`
    - `System.IO.IOException` on `YahooFinanceParams.txt` in test output.
  - Immediate full rerun:
    - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
    - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
    - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
    - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `800/800`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #969-#979)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (remaining helper log wrapper contracts):**
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - added `Stage2Step2_2_TradeGridNonTradePeriods_SendNewLogMessage_WithSubscriber_ShouldForwardMessage`.
    - added `Stage2Step2_2_TradeGridNonTradePeriods_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow`.
    - added `Stage2Step2_2_TrailingUp_SendNewLogMessage_WithSubscriber_ShouldForwardMessage`.
    - added `Stage2Step2_2_TrailingUp_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow`.
    - added `Stage2Step2_2_TradeGridErrorsReaction_SendNewLogMessage_WithSubscriber_ShouldForwardMessage`.
    - added `Stage2Step2_2_TradeGridErrorsReaction_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow`.
    - added `Stage2Step2_2_TradeGridCreator_SendNewLogMessage_WithSubscriber_ShouldForwardMessage`.
    - added `Stage2Step2_2_TradeGridCreator_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow`.
    - added `Stage2Step2_2_TradeGrid_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow`.
    - added `Stage2Step2_2_TradeGridsMaster_SendNewLogMessage_WithSubscriber_ShouldForwardMessage`.
    - added `Stage2Step2_2_TradeGridsMaster_SendNewLogMessage_WithoutSubscriber_ShouldNotThrow`.
  - Outcome:
    - finished the remaining simple non-UI `SendNewLogMessage(...)` wrapper coverage in the grid service layer.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `811/811`
- **Commit:** n/a
- **Push:** n/a

### Step 4.2 - Nullable Annotations (Incremental Adoption #980-#981)

- **Status:** In Progress (increment block completed)
- **Plan item:** `refactoring_stage2_plan.md` -> Phase 4 / Step 4.2
- **Changes (suppress modal popup source for missing deposit-percent asset guard):**
  - Updated production code in `project/OsEngine/OsTrader/Grids/TradeGridCreator.cs`:
    - the missing-asset guard in `GetVolume()` now logs with `LogMessageType.System` instead of `LogMessageType.Error`.
    - behavior remains the same otherwise: message is preserved and the method still returns `0`.
  - Updated tests in `project/OsEngine.Tests/TradeGridPersistenceCoreTests.cs`:
    - extended `Stage2Step2_2_TradeGridCreator_GetVolume_WithMissingCustomAsset_ShouldReturnZero`.
    - now locks both the exact message text and the downgraded log type.
  - Outcome:
    - the `Can\`t found portfolio in Deposit Percent volume mode USDT` path no longer raises the intrusive modal popup during tests.
- **Verification (outside sandbox, per dotnet-build-policy):**
  - `dotnet restore project/OsEngine/OsEngine.csproj --nologo` -> success
  - `dotnet restore project/OsEngine.Tests/OsEngine.Tests.csproj --nologo` -> success
  - `dotnet build project/OsEngine/OsEngine.csproj --no-restore --configuration Release --nologo -p:NoWarn=NU1900` -> success, 0 warnings, 0 errors
  - `dotnet test project/OsEngine.Tests/OsEngine.Tests.csproj --no-restore --configuration Release --nologo` -> passed `811/811`
- **Commit:** n/a
- **Push:** n/a
