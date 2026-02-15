/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms.Integration;
using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Logging;
using OsEngine.Market.Servers.Optimizer;
using OsEngine.Market.Servers.Tester;
using OsEngine.OsTrader.Panels;
using OsEngine.Robots;
using OsEngine.OsTrader.Panels.Tab.Internal;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.Market;
using OsEngine.OsOptimizer.OptEntity;

namespace OsEngine.OsOptimizer
{
    public class OptimizerMaster
    {
        #region Service

        public OptimizerMaster()
        {
            _log = new Log("OptimizerLog", StartProgram.IsTester);
            _log.Listen(this);

            Settings = new OptimizerSettings();
            Settings.LogMessageEvent += SendLogMessage;
            Settings.CommissionChanged += UpdateBotManualControlSettings;
            Settings.DateTimeStartEndChange += OnDateTimeStartEndChange;

            _filterManager = new OptimizerFilterManager(Settings);
            _filterManager.LogMessageEvent += SendLogMessage;

            _phaseCalculator = new PhaseCalculator();
            _phaseCalculator.LogMessageEvent += SendLogMessage;

            Storage = new OptimizerDataStorage("Prime", true);
            Storage.SecuritiesChangeEvent += _storage_SecuritiesChangeEvent;
            Storage.TimeChangeEvent += _storage_TimeChangeEvent;

            ManualControl = new BotManualControl("OptimizerManualControl", null, StartProgram.IsOsTrader);

            CreateBot();

            _optimizerExecutor = new OptimizerExecutor(this);
            _optimizerExecutor.LogMessageEvent += SendLogMessage;
            _optimizerExecutor.TestingProgressChangeEvent += _optimizerExecutor_TestingProgressChangeEvent;
            _optimizerExecutor.PrimeProgressChangeEvent += _optimizerExecutor_PrimeProgressChangeEvent;
            _optimizerExecutor.TestReadyEvent += _optimizerExecutor_TestReadyEvent;
            _optimizerExecutor.NeedToMoveUiToEvent += _optimizerExecutor_NeedToMoveUiToEvent;
            _optimizerExecutor.TimeToEndChangeEvent += _optimizerExecutor_TimeToEndChangeEvent;
            ProgressBarStatuses = new List<ProgressBarStatus>();
            PrimeProgressBarStatus = new ProgressBarStatus();
        }

        public readonly OptimizerSettings Settings;

        private readonly OptimizerFilterManager _filterManager;

        private readonly PhaseCalculator _phaseCalculator;

        public int GetMaxBotsCount()
        {
            if (_parameters == null ||
                _parametersOn == null)
            {
                return 0;
            }

            int botCountRaw = _optimizerExecutor.BotCountOneFaze(_parameters, _parametersOn);
            int botCount = Math.Max(0, botCountRaw);
            int iterationCount = Math.Max(0, Settings.IterationCount);

            long valueLong = (long)botCount * iterationCount * 2L;

            if (Settings.LastInSample && valueLong > 0)
            {
                valueLong -= botCount;
            }

            if (valueLong <= 0)
            {
                return 0;
            }

            if (valueLong > int.MaxValue)
            {
                return int.MaxValue;
            }

            return (int)valueLong;
        }

        #endregion

        #region Forwarding properties for UI compatibility

        // These properties delegate to Settings to maintain backward compatibility
        // with OptimizerUi and other consumers.

        public int ThreadsCount
        {
            get => Settings.ThreadsCount;
            set => Settings.ThreadsCount = value;
        }

        public string StrategyName
        {
            get => Settings.StrategyName;
            set => Settings.StrategyName = value;
        }

        public bool IsScript
        {
            get => Settings.IsScript;
            set => Settings.IsScript = value;
        }

        public decimal StartDeposit
        {
            get => Settings.StartDeposit;
            set => Settings.StartDeposit = value;
        }

        public OrderExecutionType OrderExecutionType
        {
            get => Settings.OrderExecutionType;
            set => Settings.OrderExecutionType = value;
        }

        public int SlippageToSimpleOrder
        {
            get => Settings.SlippageToSimpleOrder;
            set => Settings.SlippageToSimpleOrder = value;
        }

        public int SlippageToStopOrder
        {
            get => Settings.SlippageToStopOrder;
            set => Settings.SlippageToStopOrder = value;
        }

        public CommissionType CommissionType
        {
            get => Settings.CommissionType;
            set => Settings.CommissionType = value;
        }

        public decimal CommissionValue
        {
            get => Settings.CommissionValue;
            set => Settings.CommissionValue = value;
        }

        public decimal FilterProfitValue
        {
            get => Settings.FilterProfitValue;
            set => Settings.FilterProfitValue = value;
        }

        public bool FilterProfitIsOn
        {
            get => Settings.FilterProfitIsOn;
            set => Settings.FilterProfitIsOn = value;
        }

        public decimal FilterMaxDrawDownValue
        {
            get => Settings.FilterMaxDrawDownValue;
            set => Settings.FilterMaxDrawDownValue = value;
        }

        public bool FilterMaxDrawDownIsOn
        {
            get => Settings.FilterMaxDrawDownIsOn;
            set => Settings.FilterMaxDrawDownIsOn = value;
        }

        public decimal FilterMiddleProfitValue
        {
            get => Settings.FilterMiddleProfitValue;
            set => Settings.FilterMiddleProfitValue = value;
        }

        public bool FilterMiddleProfitIsOn
        {
            get => Settings.FilterMiddleProfitIsOn;
            set => Settings.FilterMiddleProfitIsOn = value;
        }

        public decimal FilterProfitFactorValue
        {
            get => Settings.FilterProfitFactorValue;
            set => Settings.FilterProfitFactorValue = value;
        }

        public bool FilterProfitFactorIsOn
        {
            get => Settings.FilterProfitFactorIsOn;
            set => Settings.FilterProfitFactorIsOn = value;
        }

        public int FilterDealsCountValue
        {
            get => Settings.FilterDealsCountValue;
            set => Settings.FilterDealsCountValue = value;
        }

        public bool FilterDealsCountIsOn
        {
            get => Settings.FilterDealsCountIsOn;
            set => Settings.FilterDealsCountIsOn = value;
        }

        public DateTime TimeStart
        {
            get => Settings.TimeStart;
            set => Settings.TimeStart = value;
        }

        public DateTime TimeEnd
        {
            get => Settings.TimeEnd;
            set => Settings.TimeEnd = value;
        }

        public decimal PercentOnFiltration
        {
            get => Settings.PercentOnFiltration;
            set => Settings.PercentOnFiltration = value;
        }

        public int IterationCount
        {
            get => Settings.IterationCount;
            set => Settings.IterationCount = value;
        }

        public bool LastInSample
        {
            get => Settings.LastInSample;
            set => Settings.LastInSample = value;
        }

        public OptimizationMethodType OptimizationMethod
        {
            get => Settings.OptimizationMethod;
            set => Settings.OptimizationMethod = value;
        }

        public SortBotsType ObjectiveMetric
        {
            get => Settings.ObjectiveMetric;
            set => Settings.ObjectiveMetric = value;
        }

        public int BayesianInitialSamples
        {
            get => Settings.BayesianInitialSamples;
            set => Settings.BayesianInitialSamples = value;
        }

        public int BayesianMaxIterations
        {
            get => Settings.BayesianMaxIterations;
            set => Settings.BayesianMaxIterations = value;
        }

        public int BayesianBatchSize
        {
            get => Settings.BayesianBatchSize;
            set => Settings.BayesianBatchSize = value;
        }

        public ObjectiveDirectionType ObjectiveDirection
        {
            get => Settings.ObjectiveDirection;
            set => Settings.ObjectiveDirection = value;
        }

        public BayesianAcquisitionModeType BayesianAcquisitionMode
        {
            get => Settings.BayesianAcquisitionMode;
            set => Settings.BayesianAcquisitionMode = value;
        }

        public decimal BayesianAcquisitionKappa
        {
            get => Settings.BayesianAcquisitionKappa;
            set => Settings.BayesianAcquisitionKappa = value;
        }

        public bool BayesianUseTailPass
        {
            get => Settings.BayesianUseTailPass;
            set => Settings.BayesianUseTailPass = value;
        }

        public int BayesianTailSharePercent
        {
            get => Settings.BayesianTailSharePercent;
            set => Settings.BayesianTailSharePercent = value;
        }

        public List<OrderClearing> ClearingTimes => Settings.ClearingTimes;

        public List<NonTradePeriod> NonTradePeriods => Settings.NonTradePeriods;

        public void SaveClearingInfo() => Settings.SaveClearingInfo();

        public void CreateNewClearing() => Settings.CreateNewClearing();

        public void RemoveClearing(int num) => Settings.RemoveClearing(num);

        public void SaveNonTradePeriods() => Settings.SaveNonTradePeriods();

        public void CreateNewNonTradePeriod() => Settings.CreateNewNonTradePeriod();

        public void RemoveNonTradePeriod(int num) => Settings.RemoveNonTradePeriod(num);

        #endregion

        #region Progress of the optimization process

        private void _optimizerExecutor_PrimeProgressChangeEvent(int curVal, int maxVal)
        {
            if (PrimeProgressBarStatus.CurrentValue != curVal)
            {
                PrimeProgressBarStatus.CurrentValue = curVal;
            }

            if (PrimeProgressBarStatus.MaxValue != maxVal)
            {
                PrimeProgressBarStatus.MaxValue = maxVal;
            }
        }

        private void _optimizerExecutor_TestReadyEvent(List<OptimizerFazeReport> reports)
        {
            if (PrimeProgressBarStatus.CurrentValue != PrimeProgressBarStatus.MaxValue)
            {
                PrimeProgressBarStatus.CurrentValue = PrimeProgressBarStatus.MaxValue;
            }

            TestReadyEvent?.Invoke(reports);
        }

        private void _optimizerExecutor_TimeToEndChangeEvent(TimeSpan timeToEnd)
        {
            TimeToEndChangeEvent?.Invoke(timeToEnd);
        }

        public event Action<TimeSpan> TimeToEndChangeEvent;

        public event Action<List<OptimizerFazeReport>> TestReadyEvent;

        private void _optimizerExecutor_TestingProgressChangeEvent(int curVal, int maxVal, int numServer)
        {
            ProgressBarStatus status;
            try
            {
                status = ProgressBarStatuses.Find(st => st.Num == numServer);
            }
            catch
            {
                return;
            }

            if (status == null)
            {
                status = new ProgressBarStatus();
                status.Num = numServer;
                ProgressBarStatuses.Add(status);
            }

            status.CurrentValue = curVal;
            status.MaxValue = maxVal;
        }

        public List<ProgressBarStatus> ProgressBarStatuses;

        public ProgressBarStatus PrimeProgressBarStatus;

        #endregion

        #region Data store

        public bool ShowDataStorageDialog()
        {
            TesterSourceDataType storageSource = Storage.SourceDataType;
            string folder = Storage.PathToFolder;
            TesterDataType storageDataType = Storage.TypeTesterData;
            string setName = Storage.ActiveSet;

            Storage.ShowDialog(this);

            if (storageSource != Storage.SourceDataType
                || folder != Storage.PathToFolder
                || storageDataType != Storage.TypeTesterData
                || setName != Storage.ActiveSet)
            {
                return true;
            }

            return false;
        }

        public OptimizerDataStorage Storage;

        private void _storage_TimeChangeEvent(DateTime timeStart, DateTime timeEnd)
        {
            TimeStart = timeStart;
            TimeEnd = timeEnd;
        }

        private void _storage_SecuritiesChangeEvent(List<Security> securities)
        {
            NewSecurityEvent?.Invoke(securities);

            TimeStart = Storage.TimeStart;
            TimeEnd = Storage.TimeEnd;
        }

        public event Action<List<Security>> NewSecurityEvent;

        #endregion

        #region Management

        public List<SecurityTester> SecurityTester
        {
            get { return Storage.SecuritiesTester; }
        }

        public BotManualControl ManualControl;

        public BotPanel BotToTest;

        public OptimizerServer ServerToTestBot;

        public void ShowManualControlDialog()
        {
            ManualControl.ShowDialog(StartProgram.IsOsOptimizer);
        }

        public void UpdateBotManualControlSettings()
        {
            if (string.IsNullOrEmpty(Settings.StrategyName))
            {
                return;
            }

            if (BotToTest == null)
            {
                string botName = "OptimizerBot" + Settings.StrategyName.RemoveExcessFromSecurityName();

                BotToTest = BotFactory.GetStrategyForName(Settings.StrategyName, botName, StartProgram.IsTester, Settings.IsScript);
            }

            List<IIBotTab> sources = BotToTest.GetTabs();

            for (int i = 0; i < sources.Count; i++)
            {
                IIBotTab curTab = sources[i];

                if (curTab.TabType == BotTabType.Simple)
                {
                    BotTabSimple simpleTab = (BotTabSimple)curTab;
                    simpleTab.Connector.ServerType = Market.ServerType.Optimizer;
                    simpleTab.Connector.ServerUid = -1;
                    simpleTab.CommissionType = Settings.CommissionType;
                    simpleTab.CommissionValue = Settings.CommissionValue;

                    CopyManualSupportSettings(simpleTab.ManualPositionSupport);
                }
                if (curTab.TabType == BotTabType.Screener)
                {
                    BotTabScreener screenerTab = (BotTabScreener)curTab;
                    screenerTab.ServerType = Market.ServerType.Optimizer;
                    screenerTab.ServerUid = -1;
                    screenerTab.CommissionType = Settings.CommissionType;
                    screenerTab.CommissionValue = Settings.CommissionValue;
                }
            }

            UpdateServerToSettings();
        }

        public void CreateBot()
        {
            try
            {
                if (string.IsNullOrEmpty(Settings.StrategyName))
                {
                    return;
                }

                string botName = "OptimizerBot" + Settings.StrategyName.RemoveExcessFromSecurityName();

                if (Storage.SourceDataType == TesterSourceDataType.Set
                    && string.IsNullOrEmpty(Storage.ActiveSet) == false)
                {
                    string[] setNameArray = Storage.ActiveSet.Split('_');

                    botName += setNameArray[setNameArray.Length - 1];
                }

                BotToTest = BotFactory.GetStrategyForName(Settings.StrategyName, botName, StartProgram.IsTester, Settings.IsScript);

                if(BotToTest == null)
                {
                    return;
                }

                List<IIBotTab> sources = BotToTest.GetTabs();

                for (int i = 0; i < sources.Count; i++)
                {
                    IIBotTab curTab = sources[i];

                    if (curTab.TabType == BotTabType.Simple)
                    {
                        BotTabSimple simpleTab = (BotTabSimple)curTab;
                        simpleTab.Connector.ServerType = Market.ServerType.Optimizer;
                        simpleTab.Connector.ServerUid = -1;
                        simpleTab.CommissionType = Settings.CommissionType;
                        simpleTab.CommissionValue = Settings.CommissionValue;

                        CopyManualSupportSettings(simpleTab.ManualPositionSupport);
                    }
                    if (curTab.TabType == BotTabType.Screener)
                    {
                        BotTabScreener screenerTab = (BotTabScreener)curTab;
                        screenerTab.ServerType = Market.ServerType.Optimizer;
                        screenerTab.ServerUid = -1;
                        screenerTab.CommissionType = Settings.CommissionType;
                        screenerTab.CommissionValue = Settings.CommissionValue;
                        screenerTab.ManualPositionSupportFromOptimizer = ManualControl;
                        screenerTab.TryLoadTabs();
                        screenerTab.NeedToReloadTabs = true;
                        screenerTab.TryReLoadTabs();
                    }
                }

                UpdateServerToSettings();
            }
            catch (Exception ex)
            {
                SendLogMessage("Can`t create bot " + Settings.StrategyName + " Exception: " + ex.ToString(), LogMessageType.Error);
            }
        }

        public void UpdateServerToSettings()
        {
            List<Market.Servers.IServer> servers = ServerMaster.GetServers();

            for (int i = 0; servers != null && i < servers.Count; i++)
            {
                if (servers[i].ServerType != ServerType.Optimizer)
                {
                    continue;
                }
                OptimizerServer curServer = (OptimizerServer)servers[i];

                if (curServer.NumberServer == -1)
                {

                    ServerMaster.RemoveOptimizerServer(curServer);
                    break;
                }
            }

            ServerToTestBot = ServerMaster.CreateNextOptimizerServer(Storage, -1, 10000);
        }

        public void CopyManualSupportSettings(BotManualControl manualControlTo)
        {

            manualControlTo.DoubleExitIsOn = ManualControl.DoubleExitIsOn;
            manualControlTo.DoubleExitSlippage = ManualControl.DoubleExitSlippage;
            manualControlTo.OrderTypeTime = ManualControl.OrderTypeTime;
            manualControlTo.ProfitDistance = ManualControl.ProfitDistance;
            manualControlTo.ProfitIsOn = ManualControl.ProfitIsOn;
            manualControlTo.ProfitSlippage = ManualControl.ProfitSlippage;
            manualControlTo.SecondToClose = ManualControl.SecondToClose;
            manualControlTo.SecondToCloseIsOn = ManualControl.SecondToCloseIsOn;
            manualControlTo.SecondToOpen = ManualControl.SecondToOpen;
            manualControlTo.SecondToOpenIsOn = ManualControl.SecondToOpenIsOn;
            manualControlTo.SetbackToCloseIsOn = ManualControl.SetbackToCloseIsOn;
            manualControlTo.SetbackToClosePosition = ManualControl.SetbackToClosePosition;
            manualControlTo.SetbackToOpenIsOn = ManualControl.SetbackToOpenIsOn;
            manualControlTo.SetbackToOpenPosition = ManualControl.SetbackToOpenPosition;
            manualControlTo.StopDistance = ManualControl.StopDistance;
            manualControlTo.StopIsOn = ManualControl.StopIsOn;
            manualControlTo.StopSlippage = ManualControl.StopSlippage;
            manualControlTo.TypeDoubleExitOrder = ManualControl.TypeDoubleExitOrder;
            manualControlTo.ValuesType = ManualControl.ValuesType;

        }

        #endregion

        #region Optimization phases

        public bool IsAcceptedByFilter(OptimizerReport report)
        {
            return _filterManager.IsAcceptedByFilter(report);
        }

        public List<OptimizerFaze> Fazes;

        private void OnDateTimeStartEndChange()
        {
            DateTimeStartEndChange?.Invoke();
        }

        public void ReloadFazes()
        {
            Fazes = _phaseCalculator.CalculatePhases(
                Settings.TimeStart,
                Settings.TimeEnd,
                Settings.IterationCount,
                Settings.PercentOnFiltration,
                Settings.LastInSample);
        }

        public event Action DateTimeStartEndChange;

        #endregion

        #region Optimization parameters

        public List<IIStrategyParameter> Parameters
        {
            get
            {
                if (string.IsNullOrEmpty(Settings.StrategyName))
                {
                    return null;
                }

                BotPanel bot = BotFactory.GetStrategyForName(Settings.StrategyName, "", StartProgram.IsOsOptimizer, Settings.IsScript);

                if (bot == null)
                {
                    return null;
                }

                if (bot.Parameters == null ||
                    bot.Parameters.Count == 0)
                {
                    return null;
                }

                if (_parameters != null)
                {
                    _parameters.Clear();
                    _parameters = null;
                }

                _parameters = new List<IIStrategyParameter>();

                for (int i = 0; i < bot.Parameters.Count; i++)
                {
                    _parameters.Add(bot.Parameters[i]);
                }

                for (int i = 0; i < _parameters.Count; i++)
                {
                    GetValueParameterSaveByUser(_parameters[i]);
                }

                bot.Delete();

                return _parameters;
            }
        }

        public List<IIStrategyParameter> ParametersStandard
        {
            get
            {
                if (string.IsNullOrEmpty(Settings.StrategyName))
                {
                    return null;
                }

                BotPanel bot = BotFactory.GetStrategyForName(Settings.StrategyName, "", StartProgram.IsOsOptimizer, Settings.IsScript);

                if (bot == null)
                {
                    return null;
                }

                if (bot.Parameters == null ||
                    bot.Parameters.Count == 0)
                {
                    return null;
                }

                if (_parameters != null)
                {
                    _parameters.Clear();
                    _parameters = null;
                }

                _parameters = new List<IIStrategyParameter>();

                for (int i = 0; i < bot.Parameters.Count; i++)
                {
                    _parameters.Add(bot.Parameters[i]);
                }

                return _parameters;
            }
        }

        private List<IIStrategyParameter> _parameters;

        private void GetValueParameterSaveByUser(IIStrategyParameter parameter)
        {
            if (!File.Exists(@"Engine\" + Settings.StrategyName + @"_StandartOptimizerParameters.txt"))
            {
                return;
            }
            try
            {
                using (StreamReader reader = new StreamReader(@"Engine\" + Settings.StrategyName + @"_StandartOptimizerParameters.txt"))
                {
                    while (!reader.EndOfStream)
                    {
                        string[] save = reader.ReadLine().Split('#');

                        if (save[0] == parameter.Name)
                        {
                            parameter.LoadParamFromString(save);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public void SaveStandardParameters()
        {
            if (_parameters == null ||
                _parameters.Count == 0)
            {
                return;
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(@"Engine\" + Settings.StrategyName + @"_StandartOptimizerParameters.txt", false)
                    )
                {
                    for (int i = 0; i < _parameters.Count; i++)
                    {
                        writer.WriteLine(_parameters[i].GetStringToSave());
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }

            SaveParametersOnOffByStrategy();
        }

        public List<bool> ParametersOn
        {
            get
            {

                _parametersOn = new List<bool>();
                for (int i = 0; _parameters != null && i < _parameters.Count; i++)
                {
                    _parametersOn.Add(false);
                }

                List<bool> parametersOnSaveBefore = GetParametersOnOffByStrategy();

                if (parametersOnSaveBefore != null &&
                    parametersOnSaveBefore.Count == _parametersOn.Count)
                {
                    _parametersOn = parametersOnSaveBefore;
                }

                return _parametersOn;
            }
        }
        private List<bool> _parametersOn;

        private List<bool> GetParametersOnOffByStrategy()
        {
            List<bool> result = new List<bool>();

            if (!File.Exists(@"Engine\" + Settings.StrategyName + @"_StandartOptimizerParametersOnOff.txt"))
            {
                return result;
            }
            try
            {
                using (StreamReader reader = new StreamReader(@"Engine\" + Settings.StrategyName + @"_StandartOptimizerParametersOnOff.txt"))
                {
                    while (!reader.EndOfStream)
                    {
                        result.Add(Convert.ToBoolean(reader.ReadLine()));
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }

            return result;
        }

        private void SaveParametersOnOffByStrategy()
        {
            if (_parametersOn == null ||
               _parametersOn.Count == 0)
            {
                return;
            }

            try
            {
                using (StreamWriter writer = new StreamWriter(@"Engine\" + Settings.StrategyName + @"_StandartOptimizerParametersOnOff.txt", false)
                    )
                {
                    for (int i = 0; i < _parametersOn.Count; i++)
                    {
                        writer.WriteLine(_parametersOn[i].ToString());
                    }
                }
            }
            catch (Exception)
            {
                // ignore
            }

        }

        #endregion

        #region Start optimization algorithm

        public OptimizerExecutor _optimizerExecutor;

        public bool Start()
        {
            if (CheckReadyData() == false)
            {
                return false;
            }

            if (_optimizerExecutor.Start(_parametersOn, _parameters))
            {
                ProgressBarStatuses = new List<ProgressBarStatus>();
                PrimeProgressBarStatus = new ProgressBarStatus();
            }
            return true;
        }

        public void Stop()
        {
            _optimizerExecutor.Stop();
        }

        private bool CheckReadyData()
        {
            if (Fazes == null || Fazes.Count == 0)
            {
                CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Optimizer.Message14);
                ui.ShowDialog();
                SendLogMessage(OsLocalization.Optimizer.Message14, LogMessageType.System);
                if (NeedToMoveUiToEvent != null)
                {
                    NeedToMoveUiToEvent(NeedToMoveUiTo.Fazes);
                }
                return false;
            }

            List<IIBotTab> sources = BotToTest.GetTabs();

            // проверяем наличие данных в источниках

            bool noDataFull = true;
            bool noDataInOneSource = false;

            for (int i = 0; i < sources.Count; i++)
            {
                if (sources[i].TabType == BotTabType.Simple)
                {// BotTabSimple
                    BotTabSimple simple = (BotTabSimple)sources[i];

                    if (string.IsNullOrEmpty(simple.Connector.SecurityName) == false)
                    {
                        noDataFull = false;
                        if (HaveSecurityAndTfInStorage(
                            simple.Connector.SecurityName, simple.Connector.TimeFrame) == false)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        noDataInOneSource = true;
                    }
                }
                else if (sources[i].TabType == BotTabType.Index)
                {// BotTabIndex
                    BotTabIndex index = (BotTabIndex)sources[i];

                    if (index.Tabs == null ||
                        index.Tabs.Count == 0)
                    {
                        noDataInOneSource = true;
                    }
                    else
                    {
                        noDataFull = false;

                        for (int i2 = 0; i2 < index.Tabs.Count; i2++)
                        {
                            if (HaveSecurityAndTfInStorage(
                                index.Tabs[i2].SecurityName, index.Tabs[i2].TimeFrame) == false)
                            {
                                return false;
                            }
                        }
                    }
                }
                else if (sources[i].TabType == BotTabType.Screener)
                {// BotTabScreener
                    BotTabScreener screener = (BotTabScreener)sources[i];

                    if (screener.Tabs == null ||
                        screener.Tabs.Count == 0)
                    {
                        noDataInOneSource = true;
                    }
                    else
                    {
                        noDataFull = false;

                        for (int i2 = 0; i2 < screener.Tabs.Count; i2++)
                        {
                            if (HaveSecurityAndTfInStorage(
                                screener.Tabs[i2].Connector.SecurityName, screener.Tabs[i2].Connector.TimeFrame) == false)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (noDataFull == true)
            { // данные полность не подключены
                CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Optimizer.Message15);
                ui.ShowDialog();
                SendLogMessage(OsLocalization.Optimizer.Message15, LogMessageType.System);
                if (NeedToMoveUiToEvent != null)
                {
                    NeedToMoveUiToEvent(NeedToMoveUiTo.TabsAndTimeFrames);
                }
                return false;
            }

            if(noDataInOneSource == true)
            {
                AcceptDialogUi ui = new AcceptDialogUi(OsLocalization.Market.Message103);

                ui.ShowDialog();

                if (ui.UserAcceptAction == false)
                {
                    return false;
                }
            }

            if ((string.IsNullOrEmpty(Storage.ActiveSet)
                && Storage.SourceDataType == TesterSourceDataType.Set)
                ||
                Storage.SecuritiesTester == null
                ||
                Storage.SecuritiesTester.Count == 0)
            {
                CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Optimizer.Message16);
                ui.ShowDialog();
                SendLogMessage(OsLocalization.Optimizer.Message16, LogMessageType.System);

                if (NeedToMoveUiToEvent != null)
                {
                    NeedToMoveUiToEvent(NeedToMoveUiTo.Storage);
                }
                return false;
            }

            if (string.IsNullOrEmpty(Settings.StrategyName))
            {
                CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Optimizer.Message17);
                ui.ShowDialog();
                SendLogMessage(OsLocalization.Optimizer.Message17, LogMessageType.System);
                if (NeedToMoveUiToEvent != null)
                {
                    NeedToMoveUiToEvent(NeedToMoveUiTo.NameStrategy);
                }
                return false;
            }

            bool onParametersReady = false;

            if(_parametersOn == null)
            {
                CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Optimizer.Message44);
                ui.ShowDialog();
                return false;
            }

            for (int i = 0; i < _parametersOn.Count; i++)
            {
                if (_parametersOn[i])
                {
                    onParametersReady = true;
                    break;
                }
            }

            if (onParametersReady == false)
            {
                CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Optimizer.Message18);
                ui.ShowDialog();
                SendLogMessage(OsLocalization.Optimizer.Message18, LogMessageType.System);
                if (NeedToMoveUiToEvent != null)
                {

                    NeedToMoveUiToEvent(NeedToMoveUiTo.Parameters);
                }
                return false;
            }


            // проверка наличия и состояния параметра Regime
            bool onRgimeOff = false;

            for (int i = 0; i < _parameters.Count; i++)
            {
                if (_parameters[i].Name == "Regime" && _parameters[i].Type == StrategyParameterType.String)
                {
                    if (((StrategyParameterString)_parameters[i]).ValueString == "Off")
                    {
                        onRgimeOff = true;
                    }
                }

                else if (_parameters[i].Name == "Regime" && _parameters[i].Type == StrategyParameterType.CheckBox)
                {
                    if (((StrategyParameterCheckBox)_parameters[i]).CheckState == System.Windows.Forms.CheckState.Unchecked)
                    {
                        onRgimeOff = true;
                    }
                }
            }

            if (onRgimeOff == true)
            {
                CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Optimizer.Message41);
                ui.ShowDialog();
                SendLogMessage(OsLocalization.Optimizer.Message41, LogMessageType.System);
                if (NeedToMoveUiToEvent != null)
                {
                    NeedToMoveUiToEvent(NeedToMoveUiTo.RegimeRow);
                }
                return false;
            }
            // Regime / конец

            return true;
        }

        private bool HaveSecurityAndTfInStorage(string secName, TimeFrame timeFrame)
        {
            // проверяем наличие тайм-фрейма в обойме

            bool isInArray = false;

            for (int j = 0; j < Storage.SecuritiesTester.Count; j++)
            {
                if (Storage.SecuritiesTester[j].Security.Name == secName
                    &&
                    (Storage.SecuritiesTester[j].TimeFrame == timeFrame
                    || Storage.SecuritiesTester[j].TimeFrame == TimeFrame.Sec1
                    || Storage.SecuritiesTester[j].TimeFrame == TimeFrame.Tick))
                {
                    isInArray = true;
                }
            }

            if (isInArray == false)
            {
                CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Optimizer.Message43);
                ui.ShowDialog();
                SendLogMessage(OsLocalization.Optimizer.Message43, LogMessageType.System);

                if (NeedToMoveUiToEvent != null)
                {
                    NeedToMoveUiToEvent(NeedToMoveUiTo.NameStrategy);
                }
                return false;
            }

            return true;
        }

        private void _optimizerExecutor_NeedToMoveUiToEvent(NeedToMoveUiTo moveUiTo)
        {
            NeedToMoveUiToEvent?.Invoke(moveUiTo);
        }

        public event Action<NeedToMoveUiTo> NeedToMoveUiToEvent;

        #endregion

        #region One bot test

        public BotPanel TestBot(OptimizerFazeReport faze, OptimizerReport report)
        {
            if (_optimizerExecutor == null)
            {
                SendLogMessage("Single-bot test skipped: optimizer executor is not initialized.", LogMessageType.Error);
                return null;
            }

            if (faze == null || report == null)
            {
                SendLogMessage("Single-bot test skipped due to null faze/report input.", LogMessageType.Error);
                return null;
            }

            if (Volatile.Read(ref _aloneTestIsOver) == false)
            {
                SendLogMessage("Single-bot test request ignored: previous test is still running.", LogMessageType.System);
                return null;
            }

            _resultBotAloneTest = null;

            Volatile.Write(ref _aloneTestIsOver, false);
            _aloneTestDoneSignal.Reset();
            int runId = Interlocked.Increment(ref _aloneTestRunId);

            AwaitObject awaitUi;
            try
            {
                awaitUi = new AwaitObject(OsLocalization.Optimizer.Label52, 100, 0, true);
                Task.Run(() => RunAloneBotTestAsync(faze, report, awaitUi, runId));
            }
            catch (Exception ex)
            {
                SendLogMessage("Single-bot test setup failed: " + ex, LogMessageType.Error);
                RecoverSingleBotStateAfterFailure(invalidateRunId: true);
                return null;
            }

            try
            {
                AwaitUi ui = new AwaitUi(awaitUi);
                ui.ShowDialog();
            }
            catch (Exception ex)
            {
                SendLogMessage("Single-bot test UI wait failed: " + ex, LogMessageType.Error);
                RecoverSingleBotStateAfterFailure(invalidateRunId: true);
                return null;
            }

            if (!SafeWaitAloneTestDone(TimeSpan.FromSeconds(30)))
            {
                SendLogMessage("Single-bot test completion wait timed out.", LogMessageType.Error);
                RecoverSingleBotStateAfterFailure(invalidateRunId: true);
            }

            return _resultBotAloneTest;
        }

        private BotPanel _resultBotAloneTest;

        private bool _aloneTestIsOver = true;

        private int _aloneTestRunId;

        private readonly ManualResetEventSlim _aloneTestDoneSignal = new ManualResetEventSlim(true);

        private async Task RunAloneBotTestAsync(
            OptimizerFazeReport fazeToTest,
            OptimizerReport reportToTest,
            AwaitObject awaitUi,
            int runId)
        {
            try
            {
                if (!IsCurrentSingleBotRun(runId))
                {
                    return;
                }

                await Task.Delay(2000);

                if (!IsCurrentSingleBotRun(runId))
                {
                    return;
                }

                OptimizerExecutor executor = _optimizerExecutor;
                if (executor == null)
                {
                    SendLogMessage("Single-bot test canceled: optimizer executor became unavailable.", LogMessageType.Error);
                    TrySetSingleBotResult(runId, null);
                    return;
                }

                if (awaitUi == null)
                {
                    SendLogMessage("Single-bot test canceled: await object is unavailable.", LogMessageType.Error);
                    TrySetSingleBotResult(runId, null);
                    return;
                }

                BotPanel result =
                    executor.TestBot(fazeToTest, reportToTest, StartProgram.IsTester, awaitUi);

                TrySetSingleBotResult(runId, result);
            }
            catch (Exception ex)
            {
                SendLogMessage("Single-bot test failed: " + ex, LogMessageType.Error);
                TrySetSingleBotResult(runId, null);
            }
            finally
            {
                if (IsCurrentSingleBotRun(runId))
                {
                    Volatile.Write(ref _aloneTestIsOver, true);
                    SafeSignalAloneTestDone();
                }
            }
        }

        private void SafeSignalAloneTestDone()
        {
            try
            {
                _aloneTestDoneSignal.Set();
            }
            catch (Exception ex)
            {
                SendLogMessage("Single-bot test done-signal set failed: " + ex, LogMessageType.Error);
            }
        }

        private void RecoverSingleBotStateAfterFailure(bool invalidateRunId)
        {
            if (invalidateRunId)
            {
                Interlocked.Increment(ref _aloneTestRunId);
            }

            Volatile.Write(ref _aloneTestIsOver, true);
            SafeSignalAloneTestDone();
        }

        private bool IsCurrentSingleBotRun(int runId)
        {
            return runId == Volatile.Read(ref _aloneTestRunId);
        }

        private bool SafeWaitAloneTestDone(TimeSpan timeout)
        {
            try
            {
                return _aloneTestDoneSignal.Wait(timeout);
            }
            catch (Exception ex)
            {
                SendLogMessage("Single-bot test done-signal wait failed: " + ex, LogMessageType.Error);
                return false;
            }
        }

        private void TrySetSingleBotResult(int runId, BotPanel result)
        {
            if (IsCurrentSingleBotRun(runId))
            {
                _resultBotAloneTest = result;
            }
        }

        #endregion

        #region Log

        private Log _log;

        public void StartPaintLog(WindowsFormsHost logHost)
        {
            _log.StartPaint(logHost);
        }

        public void SendLogMessage(string message, LogMessageType type)
        {
            LogMessageEvent?.Invoke(message, type);
        }

        public event Action<string, LogMessageType> LogMessageEvent;

        #endregion
    }

    public class ProgressBarStatus
    {
        public int CurrentValue;

        public int MaxValue;

        public int Num;

        public bool IsFinalized;
    }

    public class OptimizerFaze
    {
        public OptimizerFazeType TypeFaze;

        public DateTime TimeStart
        {
            get { return _timeStart; }
            set
            {
                _timeStart = value;
                Days = Convert.ToInt32((TimeEnd - _timeStart).TotalDays);
            }
        }
        private DateTime _timeStart;

        public DateTime TimeEnd
        {
            get { return _timeEnd; }
            set
            {
                _timeEnd = value;
                Days = Convert.ToInt32((value - TimeStart).TotalDays);
            }
        }
        private DateTime _timeEnd;

        public int Days;

        public string GetSaveString()
        {
            string result = "";

            result += TypeFaze.ToString() + "%";

            result += _timeStart.ToString(CultureInfo.InvariantCulture) + "%";

            result += _timeEnd.ToString(CultureInfo.InvariantCulture) + "%";

            result += Days.ToString() + "%";

            return result;
        }

        public void LoadFromString(string saveStr)
        {
            string[] str = saveStr.Split('%');

            Enum.TryParse(str[0], out TypeFaze);

            _timeStart = Convert.ToDateTime(str[1], CultureInfo.InvariantCulture);

            _timeEnd = Convert.ToDateTime(str[2], CultureInfo.InvariantCulture);

            Days = Convert.ToInt32(str[3]);
        }

    }

    public enum OptimizerFazeType
    {
        InSample,

        OutOfSample
    }

    public class TabSimpleEndTimeFrame
    {
        public int NumberOfTab;

        public string NameSecurity;

        public TimeFrame TimeFrame;

        public string GetSaveString()
        {
            string result = "";
            result += NumberOfTab + "%";
            result += NameSecurity + "%";
            result += TimeFrame;

            return result;
        }

        public void SetFromString(string saveStr)
        {
            string[] str = saveStr.Split('%');

            NumberOfTab = Convert.ToInt32(str[0]);
            NameSecurity = str[1];
            Enum.TryParse(str[2], out TimeFrame);
        }
    }

    public class TabIndexEndTimeFrame
    {
        public int NumberOfTab;

        public List<string> NamesSecurity = new List<string>();

        public TimeFrame TimeFrame;

        public string Formula;

        public string GetSaveString()
        {
            string result = "";
            result += NumberOfTab + "%";
            result += TimeFrame + "%";
            result += Formula + "%";

            for (int i = 0; i < NamesSecurity.Count; i++)
            {
                result += NamesSecurity[i];

                if (i + 1 != NamesSecurity.Count)
                {
                    result += "^";
                }
            }

            return result;
        }

        public void SetFromString(string saveStr)
        {
            string[] str = saveStr.Split('%');

            NumberOfTab = Convert.ToInt32(str[0]);
            Enum.TryParse(str[1], out TimeFrame);
            Formula = str[2];

            if (str.Length > 2)
            {
                string[] secs = str[3].Split('^');

                for (int i = 0; i < secs.Length; i++)
                {
                    string sec = secs[i];
                    NamesSecurity.Add(sec);
                }
            }
        }
    }

    public enum NeedToMoveUiTo
    {
        NameStrategy,

        Fazes,

        Storage,

        TabsAndTimeFrames,

        Parameters,

        Filters,

        RegimeRow
    }
}
