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
