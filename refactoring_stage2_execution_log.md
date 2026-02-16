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
