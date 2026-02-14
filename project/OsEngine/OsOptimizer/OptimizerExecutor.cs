/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Logging;
using OsEngine.Market;
using OsEngine.Market.Servers.Optimizer;
using OsEngine.Market.Servers.Tester;
using OsEngine.OsTrader.Panels;
using OsEngine.OsOptimizer.OptimizerEntity;
using OsEngine.OsOptimizer.OptEntity;
using OsEngine.OsTrader.Panels.Tab;

namespace OsEngine.OsOptimizer
{
    public class OptimizerExecutor
    {
        #region Service

        public OptimizerExecutor(OptimizerMaster master)
        {
            _master = master;

            _asyncBotFactory = new AsyncBotFactory();
            _asyncBotFactory.LogMessageEvent += SendLogMessage;
            _parameterIterator = new ParameterIterator();
            _parameterIterator.LogMessageEvent += SendLogMessage;
            _botConfigurator = new BotConfigurator(_master.Settings, _asyncBotFactory, _master.ManualControl);
            _botConfigurator.LogMessageEvent += SendLogMessage;
        }

        private OptimizerMaster _master;

        private AsyncBotFactory _asyncBotFactory;

        private readonly ParameterIterator _parameterIterator;

        private readonly BotConfigurator _botConfigurator;

        private SemaphoreSlim _serverSlots;

        private CountdownEvent _phaseCompletion;

        private CancellationTokenSource _stopCts;

        private readonly ConcurrentDictionary<int, TaskCompletionSource<OptimizerReport>> _pendingEvaluationByServer =
            new ConcurrentDictionary<int, TaskCompletionSource<OptimizerReport>>();

        public bool Start(List<bool> parametersOn, List<IIStrategyParameter> parameters)
        {
            if (_primeThreadWorker != null)
            {
                SendLogMessage(OsLocalization.Optimizer.Message1, LogMessageType.System);
                return false;
            }
            _parametersOn = parametersOn;
            _parameters = parameters;

            SendLogMessage(OsLocalization.Optimizer.Message2, LogMessageType.System);

            _stopCts?.Dispose();
            _stopCts = new CancellationTokenSource();
            _servers = new List<OptimizerServer>();
            _countAllServersMax = 0;
            _countAllServersEndTest = 0;
            _serverNum = 1;
            _testBotsTime.Clear();
            _serverSlots = new SemaphoreSlim(Math.Max(1, _master.ThreadsCount), Math.Max(1, _master.ThreadsCount));
            _phaseCompletion = null;

            _primeThreadWorker = new Thread(PrimeThreadWorkerPlace);
            _primeThreadWorker.Name = "OptimizerExecutorThread";
            _primeThreadWorker.IsBackground = true;
            _primeThreadWorker.Start();

            return true;
        }

        public void Stop()
        {
            _stopCts?.Cancel();
            SendLogMessage(OsLocalization.Optimizer.Message3, LogMessageType.System);
        }

        private bool IsStopRequested => _stopCts != null && _stopCts.IsCancellationRequested;

        #endregion

        #region Optimization algorithm

        private void PrimeThreadWorkerPlace()
        {
            try
            {
                ReportsToFazes = new List<OptimizerFazeReport>();

                int countBots = BotCountOneFaze(_parameters, _parametersOn);

                int estimatedMaxTests = countBots * (_master.IterationCount * 2);

                if (_master.LastInSample)
                {
                    estimatedMaxTests = estimatedMaxTests - countBots;
                }

                SendLogMessage(OsLocalization.Optimizer.Message4 + estimatedMaxTests, LogMessageType.System);

                DateTime timeStart = DateTime.Now;

                for (int i = 0; i < _master.Fazes.Count; i++)
                {
                    if (IsStopRequested)
                    {
                        TestReadyEvent?.Invoke(ReportsToFazes);
                        return;
                    }

                    if (_master.Fazes[i].TypeFaze == OptimizerFazeType.InSample)
                    {
                        OptimizerFazeReport report = new OptimizerFazeReport();
                        report.Faze = _master.Fazes[i];

                        ReportsToFazes.Add(report);

                        StartAsuncBotFactoryInSample(countBots, _master.StrategyName, _master.IsScript, "InSample");

                        StartOptimizeFazeInSample(_master.Fazes[i], report, _parameters, _parametersOn, countBots);

                        EndOfFazeFiltration(ReportsToFazes[ReportsToFazes.Count - 1]);
                    }
                    else
                    {

                        SendLogMessage("ReportsCount " + ReportsToFazes[ReportsToFazes.Count - 1].Reports.Count.ToString(), LogMessageType.System);

                        OptimizerFazeReport report = new OptimizerFazeReport();
                        report.Faze = _master.Fazes[i];

                        ReportsToFazes.Add(report);

                        StartAsuncBotFactoryOutOfSample(ReportsToFazes[ReportsToFazes.Count - 2], _master.StrategyName, _master.IsScript, "OutOfSample");

                        StartOptimizeFazeOutOfSample(report, ReportsToFazes[ReportsToFazes.Count - 2]);
                    }
                }

                GC.Collect(2, GCCollectionMode.Optimized, blocking: false);

                TimeSpan time = DateTime.Now - timeStart;

                SendLogMessage(OsLocalization.Optimizer.Message7, LogMessageType.System);
                SendLogMessage("Total test time = " + time.ToString(), LogMessageType.System);

                TestReadyEvent?.Invoke(ReportsToFazes);
            }
            finally
            {
                _primeThreadWorker = null;
                DisposeRunSynchronization();
            }
        }

        private void StartAsuncBotFactoryInSample(int botCount, string botType, bool isScript, string faze)
        {
            List<string> botNames = new List<string>();
            int startServerIndex = _serverNum;

            for (int i = 0; i < botCount; i++)
            {
                string botName = (startServerIndex + i) + " OpT " + faze;
                botNames.Add(botName);
            }

            _asyncBotFactory.CreateNewBots(botNames, botType, isScript, StartProgram.IsOsOptimizer);
        }

        private void StartAsuncBotFactoryOutOfSample(OptimizerFazeReport reportFiltered, string botType, bool isScript, string faze)
        {
            List<string> botNames = new List<string>();

            for (int i = 0; i < reportFiltered.Reports.Count; i++)
            {
                if (reportFiltered.Reports[i] == null)
                {
                    reportFiltered.Reports.RemoveAt(i);
                    i--;
                    continue;
                }

                string botName = reportFiltered.Reports[i].BotName.Replace(" InSample", "") + " OutOfSample";
                botNames.Add(botName);
            }

            _asyncBotFactory.CreateNewBots(botNames, botType, isScript, StartProgram.IsOsOptimizer);
        }

        private Thread _primeThreadWorker;

        public int BotCountOneFaze(List<IIStrategyParameter> parameters, List<bool> parametersOn)
        {
            IOptimizationStrategy strategy = GetInSampleOptimizationStrategy(null);
            return strategy.EstimateBotCount(parameters, parametersOn);
        }

        private IOptimizationStrategy GetInSampleOptimizationStrategy(IBotEvaluator evaluator)
        {
            int parallel = Math.Max(1, _master.ThreadsCount);

            IOptimizationStrategy strategy = OptimizationStrategyFactory.CreateInSampleStrategy(
                _master.OptimizationMethod,
                _parameterIterator,
                evaluator,
                parallel,
                _master.ObjectiveMetric,
                _master.ObjectiveDirection,
                _master.BayesianInitialSamples,
                _master.BayesianMaxIterations,
                _master.BayesianBatchSize,
                _master.BayesianAcquisitionMode,
                _master.BayesianAcquisitionKappa,
                _master.BayesianUseTailPass,
                _master.BayesianTailSharePercent,
                out string infoMessage);

            if (!string.IsNullOrEmpty(infoMessage))
            {
                SendLogMessage(infoMessage, LogMessageType.System);
            }

            return strategy;
        }

        public List<OptimizerFazeReport> ReportsToFazes = new List<OptimizerFazeReport>();

        private void StartOptimizeFazeInSample(OptimizerFaze faze, OptimizerFazeReport report,
            List<IIStrategyParameter> allParameters, List<bool> parametersToOptimization, int inSampleBotsCount)
        {
            ReloadAllParam(allParameters);
            _phaseCompletion = new CountdownEvent(inSampleBotsCount);

            if (inSampleBotsCount > 0)
            {
                lock (_serverRemoveLocker)
                {
                    _countAllServersMax += inSampleBotsCount;
                }

                PrimeProgressChangeEvent?.Invoke(_countAllServersEndTest, _countAllServersMax);
            }

            // 2 проходим первую фазу, когда нужно обойти все варианты

            IBotEvaluator evaluator = new BotEvaluator(async (all, optimized, token) =>
            {
                return await StartNewBotForEvaluationAsync(all, optimized, report, " OpT InSample", token)
                    .ConfigureAwait(false);
            });

            IOptimizationStrategy strategy = GetInSampleOptimizationStrategy(evaluator);
            List<OptimizerReport> reports =
                strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, _stopCts?.Token ?? CancellationToken.None)
                    .GetAwaiter().GetResult();

            if (reports != null && reports.Count > 0)
            {
                report.Reports.AddRange(reports);
            }

            WaitCurrentPhaseToComplete();

            SendLogMessage(OsLocalization.Optimizer.Message5, LogMessageType.System);
        }

        private void StartOptimizeFazeOutOfSample(OptimizerFazeReport report, OptimizerFazeReport reportInSample)
        {
            SendLogMessage(OsLocalization.Optimizer.Message6, LogMessageType.System);

            if (report == null)
            {
                SendLogMessage("OutOfSample phase skipped: target report container is null.", LogMessageType.Error);
                return;
            }

            if (reportInSample == null)
            {
                SendLogMessage("OutOfSample phase skipped: source in-sample report is null.", LogMessageType.System);
            }

            List<OptimizerReport> inSampleReports = reportInSample?.Reports;
            int sourceCount = inSampleReports?.Count ?? 0;
            if (inSampleReports != null)
            {
                inSampleReports = inSampleReports
                    .FindAll(r => r != null && !string.IsNullOrWhiteSpace(r.BotName));
            }

            int droppedInvalid = sourceCount - (inSampleReports?.Count ?? 0);
            if (droppedInvalid > 0)
            {
                SendLogMessage("OutOfSample skipped invalid source reports: " + droppedInvalid, LogMessageType.System);
            }

            int outOfSampleBotsCount = inSampleReports?.Count ?? 0;
            _phaseCompletion = new CountdownEvent(outOfSampleBotsCount);

            if (outOfSampleBotsCount > 0)
            {
                lock (_serverRemoveLocker)
                {
                    _countAllServersMax += outOfSampleBotsCount;
                }

                PrimeProgressChangeEvent?.Invoke(_countAllServersEndTest, _countAllServersMax);
            }

            if (outOfSampleBotsCount == 0)
            {
                WaitCurrentPhaseToComplete();
                return;
            }

            for (int i = 0; i < inSampleReports.Count; i++)
            {
                if (!TryAcquireServerSlot())
                {
                    WaitCurrentPhaseToComplete();
                    TestReadyEvent?.Invoke(ReportsToFazes);
                    _primeThreadWorker = null;
                    return;
                }

                // SendLogMessage("Bot Out of Sample", LogMessageType.System);
                List<IIStrategyParameter> parameters = inSampleReports[i].GetParameters();
                if (parameters == null)
                {
                    SendLogMessage("OutOfSample skipped source report with null parameters: " + inSampleReports[i].BotName, LogMessageType.System);

                    if (_phaseCompletion != null && !_phaseCompletion.IsSet)
                    {
                        _phaseCompletion.Signal();
                    }

                    try
                    {
                        _serverSlots?.Release();
                    }
                    catch
                    {
                        // ignored
                    }

                    continue;
                }

                StartNewBot(parameters, null, report,
                    inSampleReports[i].BotName.Replace(" InSample", "") + " OutOfSample");
            }

            WaitCurrentPhaseToComplete();
        }

        private List<bool> _parametersOn;

        public List<IIStrategyParameter> _parameters;

        private void ReloadAllParam(List<IIStrategyParameter> parameters)
        {
            _parameterIterator.ReloadAllParam(parameters);
        }

        private void ReloadParam(IIStrategyParameter parameters)
        {
            _parameterIterator.ReloadParam(parameters);
        }

        private List<IIStrategyParameter> CopyParameters(List<IIStrategyParameter> parametersToCopy)
        {
            return _parameterIterator.CopyParameters(parametersToCopy);
        }

        private void EndOfFazeFiltration(OptimizerFazeReport bots)
        {
            try
            {
                if (bots.Reports == null ||
                    bots.Reports.Count == 0)
                {
                    return;
                }

                OptimizerFazeReport botsFiltered = new OptimizerFazeReport();

                int startCount = bots.Reports.Count;

                for (int i = 0; i < bots.Reports.Count; i++)
                {
                    if (_master.IsAcceptedByFilter(bots.Reports[i]))
                    {
                        botsFiltered.Reports.Add(bots.Reports[i]);
                    }
                }

                if (botsFiltered.Reports.Count == 0)
                {
                    /* SendLogMessage(OsLocalization.Optimizer.Message8, LogMessageType.System);
                     MessageBox.Show(OsLocalization.Optimizer.Message8);
                     NeedToMoveUiToEvent(NeedToMoveUiTo.TabsAndTimeFrames);*/
                }
                else if (startCount != botsFiltered.Reports.Count)
                {
                    SendLogMessage(OsLocalization.Optimizer.Message9 + (startCount - botsFiltered.Reports.Count), LogMessageType.System);
                }

                bots.Reports = botsFiltered.Reports;
            }
            catch (Exception ex)
            {
                SendLogMessage(ex.ToString(), LogMessageType.Error);
            }
        }

        private void StartNewBot(List<IIStrategyParameter> parameters, List<IIStrategyParameter> parametersOptimized,
            OptimizerFazeReport report, string botName)
        {
            StartNewBot(parameters, parametersOptimized, report, botName, null);
        }

        private void StartNewBot(List<IIStrategyParameter> parameters, List<IIStrategyParameter> parametersOptimized,
            OptimizerFazeReport report, string botName, TaskCompletionSource<OptimizerReport> completionSource)
        {
            OptimizerServer server = CreateNewServer(report, true);

            if (completionSource != null)
            {
                _pendingEvaluationByServer[server.NumberServer] = completionSource;
            }

            try
            {
                decimal num = Convert.ToDecimal(botName.Substring(0, 1));
            }
            catch
            {
                botName = server.NumberServer + botName;
            }

            BotPanel bot = CreateNewBot(botName, parameters, parametersOptimized, server, StartProgram.IsOsOptimizer);

            if (bot == null)
            {
                SendLogMessage("Critical Optimizer Error. Robot cannot be created", LogMessageType.Error);
                FinalizeNotStartedBot(server, null);
                return;
            }

            // wait for the robot to connect to its data server
            // ждём пока робот подключиться к своему серверу данных

            bool isConnected = SpinWait.SpinUntil(() => bot.IsConnected || IsStopRequested, TimeSpan.FromSeconds(20));
            if (!isConnected || IsStopRequested)
            {
                SendLogMessage(
                    OsLocalization.Optimizer.Message10,
                    LogMessageType.Error);
                FinalizeNotStartedBot(server, bot);
                return;
            }

            lock (_serverRemoveLocker)
            {
                _botsInTest.Add(bot);
            }

            server.TestingStart();
        }

        private Task<OptimizerReport> StartNewBotForEvaluationAsync(
            List<IIStrategyParameter> parameters,
            List<IIStrategyParameter> parametersOptimized,
            OptimizerFazeReport report,
            string botName,
            CancellationToken cancellationToken)
        {
            if (!TryAcquireServerSlot())
            {
                return Task.FromCanceled<OptimizerReport>(cancellationToken.IsCancellationRequested
                    ? cancellationToken
                    : new CancellationToken(true));
            }

            TaskCompletionSource<OptimizerReport> completion =
                new TaskCompletionSource<OptimizerReport>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (cancellationToken.CanBeCanceled)
            {
                CancellationTokenRegistration registration =
                    cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));

                completion.Task.ContinueWith(
                    static (_, state) => ((CancellationTokenRegistration)state).Dispose(),
                    registration,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }

            StartNewBot(parameters, parametersOptimized, report, botName, completion);
            return completion.Task;
        }

        private bool TryAcquireServerSlot()
        {
            CancellationToken token = _stopCts?.Token ?? CancellationToken.None;
            SemaphoreSlim slots = _serverSlots;
            if (slots == null)
            {
                return false;
            }

            try
            {
                slots.Wait(token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

        private void WaitCurrentPhaseToComplete()
        {
            CancellationToken token = _stopCts?.Token ?? CancellationToken.None;
            CountdownEvent phaseCompletion = _phaseCompletion;

            if (phaseCompletion == null)
            {
                return;
            }

            try
            {
                phaseCompletion.Wait(token);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }

        private void DisposeRunSynchronization()
        {
            try
            {
                _phaseCompletion?.Dispose();
            }
            catch
            {
                // ignored
            }
            finally
            {
                _phaseCompletion = null;
            }

            try
            {
                _serverSlots?.Dispose();
            }
            catch
            {
                // ignored
            }
            finally
            {
                _serverSlots = null;
            }

            try
            {
                _stopCts?.Dispose();
            }
            catch
            {
                // ignored
            }
            finally
            {
                _stopCts = null;
            }
        }

        private void FinalizeNotStartedBot(OptimizerServer server, BotPanel bot)
        {
            try
            {
                if (bot != null)
                {
                    bot.Clear();
                    bot.Delete();
                }
            }
            catch (Exception ex)
            {
                SendLogMessage(ex.ToString(), LogMessageType.Error);
            }

            lock (_serverRemoveLocker)
            {
                for (int i = 0; i < _servers.Count; i++)
                {
                    if (_servers[i].NumberServer == server.NumberServer)
                    {
                        _servers[i].TestingEndEvent -= server_TestingEndEvent;
                        _servers[i].TestingProgressChangeEvent -= server_TestingProgressChangeEvent;
                        _servers.RemoveAt(i);
                        break;
                    }
                }
            }

            ServerMaster.RemoveOptimizerServer(server);

            if (_pendingEvaluationByServer.TryRemove(server.NumberServer, out TaskCompletionSource<OptimizerReport> completion))
            {
                completion.TrySetCanceled();
            }

            if (_phaseCompletion != null && !_phaseCompletion.IsSet)
            {
                _phaseCompletion.Signal();
            }

            try
            {
                _serverSlots?.Release();
            }
            catch
            {
                // ignored
            }
        }

        private List<BotPanel> _botsInTest = new List<BotPanel>();

        private OptimizerServer CreateNewServer(OptimizerFazeReport report, bool needToDelete)
        {
            // 1. Create a new server for optimization. And one thread respectively
            // 1. создаём новый сервер для оптимизации. И один поток соответственно
            OptimizerServer server = ServerMaster.CreateNextOptimizerServer(_master.Storage, _serverNum,
                _master.StartDeposit);

            server.OrderExecutionType = _master.OrderExecutionType;
            server.SlippageToSimpleOrder = _master.SlippageToSimpleOrder;
            server.SlippageToStopOrder = _master.SlippageToStopOrder;
            server.ClearingTimes = _master.ClearingTimes;
            server.NonTradePeriods = _master.NonTradePeriods;

            lock (_serverRemoveLocker)
            {
                _serverNum++;
                _servers.Add(server);
            }

            if (needToDelete)
            {
                server.TestingEndEvent += server_TestingEndEvent;
            }

            server.TypeTesterData = _master.Storage.TypeTesterData;
            server.TestingProgressChangeEvent += server_TestingProgressChangeEvent;

            List<IIBotTab> sources = _master.BotToTest.GetTabs();

            for (int i = 0; i < sources.Count; i++)
            {
                if (sources[i].TabType == BotTabType.Simple)
                {// BotTabSimple
                    BotTabSimple simple = (BotTabSimple)sources[i];

                    Security secToStart =
                    _master.Storage.Securities.Find(s => s.Name == simple.Connector.SecurityName);

                    server.GetDataToSecurity(secToStart, simple.Connector.TimeFrame, report.Faze.TimeStart,
                        report.Faze.TimeEnd);
                }
                else if (sources[i].TabType == BotTabType.Index)
                {// BotTabIndex
                    BotTabIndex index = (BotTabIndex)sources[i];

                    for (int i2 = 0; i2 < index.Tabs.Count; i2++)
                    {
                        Security secToStart =
                          _master.Storage.Securities.Find(s => s.Name == index.Tabs[i2].SecurityName);

                        server.GetDataToSecurity(secToStart, index.Tabs[i2].TimeFrame, report.Faze.TimeStart,
                            report.Faze.TimeEnd);
                    }
                }
                else if (sources[i].TabType == BotTabType.Screener)
                {// BotTabScreener
                    BotTabScreener screener = (BotTabScreener)sources[i];

                    for (int i2 = 0; i2 < screener.Tabs.Count; i2++)
                    {
                        Security secToStart =
                          _master.Storage.Securities.Find(s => s.Name == screener.Tabs[i2].Connector.SecurityName);

                        server.GetDataToSecurity(secToStart, screener.Tabs[i2].Connector.TimeFrame, report.Faze.TimeStart,
                            report.Faze.TimeEnd);
                    }
                }
            }

            return server;
        }

        private BotPanel CreateNewBot(string botName,
            List<IIStrategyParameter> parameters,
            List<IIStrategyParameter> parametersOptimized,
            OptimizerServer server, StartProgram regime)
        {
            _botConfigurator.BotToTest = _master.BotToTest;
            return _botConfigurator.CreateAndConfigureBot(
                botName,
                parameters,
                parametersOptimized,
                server,
                regime,
                _stopCts?.Token ?? CancellationToken.None);
        }

        public event Action<int, int> PrimeProgressChangeEvent;

        public event Action<NeedToMoveUiTo> NeedToMoveUiToEvent { add { } remove { } }

        #endregion

        #region Single bot test

        public BotPanel TestBot(OptimizerFazeReport reportFaze,
            OptimizerReport reportToBot, StartProgram startProgram, AwaitObject awaitObj)
        {
            if (_primeThreadWorker != null)
            {
                return null;
            }

            DateTime startTime = DateTime.Now;

            string botName = NumberGen.GetNumberDeal(StartProgram.IsOsOptimizer).ToString();

            List<string> names = new List<string> { botName };
            _asyncBotFactory.CreateNewBots(names, _master.StrategyName, _master.IsScript, startProgram);

            OptimizerServer server = CreateNewServer(reportFaze, false);

            List<IIStrategyParameter> parametrs = reportToBot.GetParameters();

            BotPanel bot = CreateNewBot(botName,
                parametrs, parametrs, server, startProgram);

            if (bot == null)
            {
                SendLogMessage("Test over with error. A different robot is selected in the optimizer", LogMessageType.Error);
                awaitObj.Dispose();
                return null;
            }

            bool isConnected = SpinWait.SpinUntil(() => bot.IsConnected || IsStopRequested, TimeSpan.FromSeconds(20));
            if (!isConnected || IsStopRequested)
            {
                SendLogMessage(
                    OsLocalization.Optimizer.Message10,
                    LogMessageType.Error);
                return null;
            }

            server.TestingStart();

            int countSameTime = 0;
            DateTime timeServerLast = DateTime.MinValue;

            DateTime timeStartWaiting = DateTime.Now;

            while (bot.TimeServer < reportFaze.Faze.TimeEnd)
            {
                if (IsStopRequested)
                {
                    break;
                }

                if ((_stopCts?.Token ?? CancellationToken.None).WaitHandle.WaitOne(1000))
                {
                    break;
                }

                if (timeStartWaiting.AddSeconds(300) < DateTime.Now)
                {
                    break;
                }

                if (timeServerLast == bot.TimeServer)
                {
                    countSameTime++;

                    if (countSameTime >= 5)
                    { // пять раз подряд время сервера не меняется. Тест окончен
                        break;
                    }
                }
                else
                {
                    timeServerLast = bot.TimeServer;
                    countSameTime = 0;
                }
            }

            TimeSpan minRuntime = TimeSpan.FromSeconds(3);
            TimeSpan elapsed = DateTime.Now - startTime;
            if (elapsed < minRuntime)
            {
                TimeSpan remaining = minRuntime - elapsed;
                (_stopCts?.Token ?? CancellationToken.None).WaitHandle.WaitOne(remaining);
            }

            awaitObj.Dispose();

            return bot;
        }

        #endregion

        #region Server performing optimization

        private List<OptimizerServer> _servers = new List<OptimizerServer>();

        private int _serverNum = 1;

        private int _countAllServersMax;

        private int _countAllServersEndTest;

        private readonly Lock _serverRemoveLocker = new();

        private List<TimeSpan> _testBotsTime = new List<TimeSpan>();

        private void server_TestingEndEvent(int serverNum, TimeSpan testTime)
        {
            TestingProgressChangeEvent?.Invoke(100, 100, serverNum);
            _countAllServersEndTest++;
            PrimeProgressChangeEvent?.Invoke(_countAllServersEndTest, _countAllServersMax);

            BotPanel bot = null;
            OptimizerServer server = null;

            lock (_serverRemoveLocker)
            {
                for (int i = 0; i < _botsInTest.Count; i++)
                {
                    BotPanel curBot = _botsInTest[i];

                    if (curBot != null
                        && curBot.TabsSimple != null
                        && curBot.TabsSimple.Count > 0
                        && curBot.TabsSimple[0].Connector != null
                        && curBot.TabsSimple[0].Connector.ServerUid == serverNum)
                    {
                        bot = curBot;
                        _botsInTest.RemoveAt(i);
                        break;
                    }
                    else if (curBot != null
                        && curBot.TabsScreener != null
                        && curBot.TabsScreener.Count > 0
                        && curBot.TabsScreener[0].ServerUid == serverNum)
                    {
                        bot = curBot;
                        _botsInTest.RemoveAt(i);
                        break;
                    }
                }

                if (bot != null)
                {
                    if (_pendingEvaluationByServer.TryRemove(serverNum, out TaskCompletionSource<OptimizerReport> completion))
                    {
                        OptimizerReport report = new OptimizerReport(bot.Parameters);
                        report.LoadState(bot);
                        completion.TrySetResult(report);
                    }
                    else
                    {
                        ReportsToFazes[ReportsToFazes.Count - 1].Load(bot);
                    }
                }

                for (int i = 0; i < _servers.Count; i++)
                {
                    if (_servers[i].NumberServer == serverNum)
                    {
                        _servers[i].TestingEndEvent -= server_TestingEndEvent;
                        _servers[i].TestingProgressChangeEvent -= server_TestingProgressChangeEvent;
                        server = _servers[i];
                        _servers.RemoveAt(i);
                        break;
                    }
                }

                _testBotsTime.Add(testTime);

                if (_testBotsTime.Count >= _master.ThreadsCount)
                {
                    TimeSpan allTime = TimeSpan.Zero;

                    for (int i = 0; i < _testBotsTime.Count; i++)
                    {
                        allTime = TimeSpan.FromMilliseconds(allTime.TotalMilliseconds + _testBotsTime[i].TotalMilliseconds + 1000);
                    }

                    decimal secondsOnOneTest = Convert.ToDecimal(allTime.TotalSeconds / _testBotsTime.Count);

                    int testsToEndCount = _countAllServersMax - _countAllServersEndTest;

                    if (testsToEndCount < 0)
                    {
                        testsToEndCount = 0;
                    }

                    decimal secondsToEndAllTests = testsToEndCount * secondsOnOneTest;

                    decimal secondsToEndDivideThreads = secondsToEndAllTests / _master.ThreadsCount;

                    TimeSpan timeToEnd = TimeSpan.FromSeconds(Convert.ToInt32(secondsToEndDivideThreads));

                    if (TimeToEndChangeEvent != null
                        && timeToEnd.TotalSeconds != 0)
                    {
                        TimeToEndChangeEvent(timeToEnd);
                    }
                }
            }

            if (bot != null)
            {
                // уничтожаем робота
                bot.Clear();
                bot.Delete();
            }

            if (server != null)
            {
                ServerMaster.RemoveOptimizerServer(server);
            }

            if (_phaseCompletion != null && !_phaseCompletion.IsSet)
            {
                _phaseCompletion.Signal();
            }

            try
            {
                _serverSlots?.Release();
            }
            catch
            {
                // ignored
            }
        }

        public event Action<TimeSpan> TimeToEndChangeEvent;

        public event Action<List<OptimizerFazeReport>> TestReadyEvent;

        private void server_TestingProgressChangeEvent(int curVal, int maxVal, int numServer)
        {
            if (TestingProgressChangeEvent != null)
            {
                TestingProgressChangeEvent(curVal, maxVal, numServer);
            }
        }

        public event Action<int, int, int> TestingProgressChangeEvent;

        #endregion

        #region Log

        private void SendLogMessage(string message, LogMessageType type)
        {
            if (LogMessageEvent != null)
            {
                LogMessageEvent(message, type);
            }
        }

        public event Action<string, LogMessageType> LogMessageEvent;

        #endregion
    }
}
