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

            if (report.Faze == null)
            {
                SendLogMessage("OutOfSample phase skipped: target report faze is null.", LogMessageType.Error);
                return;
            }

            if (reportInSample == null)
            {
                SendLogMessage("OutOfSample phase skipped: source in-sample report is null.", LogMessageType.System);
            }

            List<OptimizerReport> inSampleReports = reportInSample?.Reports;
            int sourceCount = inSampleReports?.Count ?? 0;
            int droppedNullReports = 0;
            int droppedEmptyNames = 0;
            if (inSampleReports != null)
            {
                List<OptimizerReport> filtered = new List<OptimizerReport>(inSampleReports.Count);
                for (int i = 0; i < inSampleReports.Count; i++)
                {
                    OptimizerReport sourceReport = inSampleReports[i];
                    if (sourceReport == null)
                    {
                        droppedNullReports++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(sourceReport.BotName))
                    {
                        droppedEmptyNames++;
                        continue;
                    }

                    filtered.Add(sourceReport);
                }

                inSampleReports = filtered;
            }

            int droppedInvalid = sourceCount - (inSampleReports?.Count ?? 0);
            if (droppedInvalid > 0)
            {
                SendLogMessage("OutOfSample skipped invalid source reports: " + droppedInvalid, LogMessageType.System);
                if (droppedNullReports > 0)
                {
                    SendLogMessage("OutOfSample skipped null source reports: " + droppedNullReports, LogMessageType.System);
                }
                if (droppedEmptyNames > 0)
                {
                    SendLogMessage("OutOfSample skipped source reports with empty BotName: " + droppedEmptyNames, LogMessageType.System);
                }
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
                SendLogMessage("OutOfSample has no valid source reports to process.", LogMessageType.System);
                PrimeProgressChangeEvent?.Invoke(_countAllServersEndTest, _countAllServersMax);
                WaitCurrentPhaseToComplete();
                return;
            }

            for (int i = 0; i < inSampleReports.Count; i++)
            {
                OptimizerReport sourceReport = inSampleReports[i];
                if (sourceReport == null)
                {
                    SendLogMessage("OutOfSample skipped null source report during scheduling.", LogMessageType.System);
                    CompensateSkippedOutOfSampleSlot(releaseServerSlot: false);
                    continue;
                }

                string sourceBotName = sourceReport.BotName;
                if (string.IsNullOrWhiteSpace(sourceBotName))
                {
                    SendLogMessage("OutOfSample skipped source report with empty BotName during scheduling.", LogMessageType.System);
                    CompensateSkippedOutOfSampleSlot(releaseServerSlot: false);
                    continue;
                }

                if (!TryAcquireServerSlot())
                {
                    int unscheduled = inSampleReports.Count - i;
                    CompensateUnscheduledOutOfSampleItems(unscheduled);
                    if (unscheduled > 0)
                    {
                        SendLogMessage("OutOfSample compensated unscheduled tail: " + unscheduled, LogMessageType.System);
                    }
                    WaitCurrentPhaseToComplete();
                    TestReadyEvent?.Invoke(ReportsToFazes);
                    _primeThreadWorker = null;
                    return;
                }

                // SendLogMessage("Bot Out of Sample", LogMessageType.System);
                List<IIStrategyParameter> parameters = null;
                try
                {
                    parameters = sourceReport.GetParameters();
                }
                catch (Exception ex)
                {
                    SendLogMessage("OutOfSample skipped source report due to parameter extraction error: "
                        + sourceBotName + ". " + ex, LogMessageType.Error);
                    CompensateSkippedOutOfSampleSlot(releaseServerSlot: true);
                    continue;
                }

                if (parameters == null)
                {
                    SendLogMessage("OutOfSample skipped source report with null parameters: " + sourceBotName, LogMessageType.System);
                    CompensateSkippedOutOfSampleSlot(releaseServerSlot: true);
                    continue;
                }

                StartNewBot(parameters, null, report,
                    sourceBotName.Replace(" InSample", "") + " OutOfSample");
            }

            WaitCurrentPhaseToComplete();
        }

        private void CompensateSkippedOutOfSampleSlot(bool releaseServerSlot)
        {
            bool signaled = false;
            CountdownEvent phase = _phaseCompletion;
            if (SafeTrySignalPhase(phase))
            {
                signaled = true;
            }

            if (signaled)
            {
                AddCompensatedOutOfSampleProgress(1);
            }

            if (!releaseServerSlot)
            {
                return;
            }

            SafeReleaseServerSlot();
        }

        private void CompensateUnscheduledOutOfSampleItems(int unscheduledCount)
        {
            if (unscheduledCount <= 0)
            {
                return;
            }

            CountdownEvent phase = _phaseCompletion;
            if (phase == null)
            {
                return;
            }

            int signaledCount = 0;

            while (unscheduledCount > 0)
            {
                if (!SafeTrySignalPhase(phase))
                {
                    break;
                }
                signaledCount++;
                unscheduledCount--;
            }

            if (signaledCount > 0)
            {
                AddCompensatedOutOfSampleProgress(signaledCount);
            }
        }

        private void AddCompensatedOutOfSampleProgress(int count)
        {
            if (count <= 0)
            {
                return;
            }

            int progressEnd;
            int progressMax;

            lock (_serverRemoveLocker)
            {
                _countAllServersEndTest += count;
                if (_countAllServersEndTest > _countAllServersMax)
                {
                    _countAllServersEndTest = _countAllServersMax;
                }

                progressEnd = _countAllServersEndTest;
                progressMax = _countAllServersMax;
            }

            PrimeProgressChangeEvent?.Invoke(progressEnd, progressMax);
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
                    cancellationToken.Register(() => SafeTrySetCanceled(completion, cancellationToken));

                completion.Task.ContinueWith(
                    static (_, state) => ((CancellationTokenRegistration)state).Dispose(),
                    registration,
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                SafeReleaseServerSlot();

                return completion.Task;
            }

            try
            {
                StartNewBot(parameters, parametersOptimized, report, botName, completion);
            }
            catch (Exception ex)
            {
                SafeTrySetException(completion, ex);
                SendLogMessage("Optimizer evaluation start failed. " + ex, LogMessageType.Error);
                SafeReleaseServerSlot();
            }

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
            SafeDisposeBotPanel(bot);

            if (server == null)
            {
                SendLogMessage("FinalizeNotStartedBot skipped server cleanup: server is null.", LogMessageType.Error);
                CountdownEvent nullServerPhase = _phaseCompletion;
                SafeTrySignalPhase(nullServerPhase);
                SafeReleaseServerSlot();
                return;
            }

            lock (_serverRemoveLocker)
            {
                for (int i = 0; i < _servers.Count; i++)
                {
                    if (_servers[i].NumberServer == server.NumberServer)
                    {
                        DetachServerEvents(_servers[i]);
                        _servers.RemoveAt(i);
                        break;
                    }
                }
            }

            SafeRemoveOptimizerServer(server);

            if (_pendingEvaluationByServer.TryRemove(server.NumberServer, out TaskCompletionSource<OptimizerReport> completion))
            {
                SafeTrySetCanceled(completion);
            }

            CountdownEvent phase = _phaseCompletion;
            SafeTrySignalPhase(phase);

            SafeReleaseServerSlot();
        }

        private List<BotPanel> _botsInTest = new List<BotPanel>();

        private OptimizerServer CreateNewServer(OptimizerFazeReport report, bool needToDelete)
        {
            if (report == null || report.Faze == null)
            {
                SendLogMessage("CreateNewServer skipped: report or faze is null.", LogMessageType.Error);
                return null;
            }

            if (_master == null || _master.Storage == null || _master.BotToTest == null)
            {
                SendLogMessage("CreateNewServer skipped: optimizer master context is not initialized.", LogMessageType.Error);
                return null;
            }

            if (_master.Storage.Securities == null)
            {
                SendLogMessage("CreateNewServer skipped: storage securities collection is null.", LogMessageType.Error);
                return null;
            }

            // 1. Create a new server for optimization. And one thread respectively
            // 1. создаём новый сервер для оптимизации. И один поток соответственно
            OptimizerServer server = null;
            try
            {
                server = ServerMaster.CreateNextOptimizerServer(_master.Storage, _serverNum,
                    _master.StartDeposit);
            }
            catch (Exception ex)
            {
                SendLogMessage("CreateNewServer failed: server factory threw exception. " + ex, LogMessageType.Error);
                return null;
            }

            if (server == null)
            {
                SendLogMessage("CreateNewServer failed: server factory returned null.", LogMessageType.Error);
                return null;
            }

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
            if (sources == null)
            {
                SendLogMessage("CreateNewServer skipped: bot tabs collection is null.", LogMessageType.Error);
                SafeRemoveOptimizerServer(server);
                return null;
            }

            for (int i = 0; i < sources.Count; i++)
            {
                if (sources[i] == null)
                {
                    SendLogMessage("CreateNewServer skipped null bot tab source at index " + i + ".", LogMessageType.Error);
                    continue;
                }

                if (sources[i].TabType == BotTabType.Simple)
                {// BotTabSimple
                    BotTabSimple simple = (BotTabSimple)sources[i];
                    if (simple?.Connector == null)
                    {
                        SendLogMessage("CreateNewServer skipped simple tab bind: connector is null at source index " + i + ".", LogMessageType.Error);
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(simple.Connector.SecurityName))
                    {
                        SendLogMessage("CreateNewServer skipped simple tab bind: security name is empty at source index " + i + ".", LogMessageType.Error);
                        continue;
                    }

                    Security secToStart;
                    if (!TryFindSecurityByName(simple.Connector.SecurityName, out secToStart))
                    {
                        continue;
                    }

                    SafeBindSecurityToServer(server, secToStart, simple.Connector.TimeFrame, report.Faze.TimeStart,
                        report.Faze.TimeEnd, "simple", i, -1);
                }
                else if (sources[i].TabType == BotTabType.Index)
                {// BotTabIndex
                    BotTabIndex index = (BotTabIndex)sources[i];
                    if (index?.Tabs == null)
                    {
                        SendLogMessage("CreateNewServer skipped index bind: tabs collection is null at source index " + i + ".", LogMessageType.Error);
                        continue;
                    }

                    for (int i2 = 0; i2 < index.Tabs.Count; i2++)
                    {
                        if (index.Tabs[i2] == null)
                        {
                            SendLogMessage("CreateNewServer skipped index tab bind: tab is null at source index " + i + ", tab index " + i2 + ".", LogMessageType.Error);
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(index.Tabs[i2].SecurityName))
                        {
                            SendLogMessage("CreateNewServer skipped index tab bind: security name is empty at source index " + i + ", tab index " + i2 + ".", LogMessageType.Error);
                            continue;
                        }

                        Security secToStart;
                        if (!TryFindSecurityByName(index.Tabs[i2].SecurityName, out secToStart))
                        {
                            continue;
                        }

                        SafeBindSecurityToServer(server, secToStart, index.Tabs[i2].TimeFrame, report.Faze.TimeStart,
                            report.Faze.TimeEnd, "index", i, i2);
                    }
                }
                else if (sources[i].TabType == BotTabType.Screener)
                {// BotTabScreener
                    BotTabScreener screener = (BotTabScreener)sources[i];
                    if (screener?.Tabs == null)
                    {
                        SendLogMessage("CreateNewServer skipped screener bind: tabs collection is null at source index " + i + ".", LogMessageType.Error);
                        continue;
                    }

                    for (int i2 = 0; i2 < screener.Tabs.Count; i2++)
                    {
                        if (screener.Tabs[i2]?.Connector == null)
                        {
                            SendLogMessage("CreateNewServer skipped screener tab bind: connector is null at source index " + i + ", tab index " + i2 + ".", LogMessageType.Error);
                            continue;
                        }
                        if (string.IsNullOrWhiteSpace(screener.Tabs[i2].Connector.SecurityName))
                        {
                            SendLogMessage("CreateNewServer skipped screener tab bind: security name is empty at source index " + i + ", tab index " + i2 + ".", LogMessageType.Error);
                            continue;
                        }

                        Security secToStart;
                        if (!TryFindSecurityByName(screener.Tabs[i2].Connector.SecurityName, out secToStart))
                        {
                            continue;
                        }

                        SafeBindSecurityToServer(server, secToStart, screener.Tabs[i2].Connector.TimeFrame, report.Faze.TimeStart,
                            report.Faze.TimeEnd, "screener", i, i2);
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
            if (reportFaze == null || reportToBot == null || awaitObj == null)
            {
                SendLogMessage("Single-bot test skipped due to invalid input.", LogMessageType.Error);
                return null;
            }

            if (reportFaze.Faze == null)
            {
                SendLogMessage("Single-bot test skipped due to null phase configuration.", LogMessageType.Error);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            DateTime phaseTimeStart = reportFaze.Faze.TimeStart;
            DateTime phaseTimeEnd = reportFaze.Faze.TimeEnd;
            if (phaseTimeEnd <= phaseTimeStart)
            {
                SendLogMessage("Single-bot test skipped due to invalid phase time range.", LogMessageType.Error);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            if (_primeThreadWorker != null)
            {
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            CancellationToken token = _stopCts?.Token ?? CancellationToken.None;

            DateTime startTime = DateTime.Now;

            string botName;
            try
            {
                botName = NumberGen.GetNumberDeal(StartProgram.IsOsOptimizer).ToString();
            }
            catch (Exception ex)
            {
                SendLogMessage("Single-bot test skipped due to bot name generation error: " + ex, LogMessageType.Error);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            if (string.IsNullOrWhiteSpace(botName))
            {
                SendLogMessage("Single-bot test skipped: generated bot name is empty.", LogMessageType.Error);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            if (string.IsNullOrWhiteSpace(_master.StrategyName))
            {
                SendLogMessage("Single-bot test skipped: strategy name is not set.", LogMessageType.Error);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            List<string> names = new List<string> { botName };
            try
            {
                _asyncBotFactory.CreateNewBots(names, _master.StrategyName, _master.IsScript, startProgram);
            }
            catch (Exception ex)
            {
                SendLogMessage("Single-bot test skipped due to async bot queue error: " + ex, LogMessageType.Error);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            OptimizerServer server = null;
            try
            {
                server = CreateNewServer(reportFaze, false);
            }
            catch (Exception ex)
            {
                SendLogMessage("Single-bot test skipped due to server creation error: " + ex, LogMessageType.Error);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            if (server == null)
            {
                SendLogMessage("Single-bot test skipped: optimizer server was not created.", LogMessageType.Error);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            List<IIStrategyParameter> parametrs = null;
            try
            {
                parametrs = reportToBot.GetParameters();
            }
            catch (Exception ex)
            {
                SendLogMessage("Single-bot test skipped due to parameter extraction error: " + ex, LogMessageType.Error);
                SafeRemoveOptimizerServer(server);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            if (parametrs == null)
            {
                SendLogMessage("Single-bot test skipped due to null parameter set.", LogMessageType.Error);
                SafeRemoveOptimizerServer(server);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            BotPanel bot = CreateNewBot(botName,
                parametrs, parametrs, server, startProgram);

            if (bot == null)
            {
                SendLogMessage("Test over with error. A different robot is selected in the optimizer", LogMessageType.Error);
                SafeDisposeAwaitObject(awaitObj);
                if (server != null)
                {
                    SafeRemoveOptimizerServer(server);
                }
                return null;
            }

            bool isConnected = SpinWait.SpinUntil(() => bot.IsConnected || IsStopRequested, TimeSpan.FromSeconds(20));
            if (!isConnected || IsStopRequested)
            {
                SendLogMessage(
                    OsLocalization.Optimizer.Message10,
                    LogMessageType.Error);
                SafeDisposeBotPanel(bot);
                SafeRemoveOptimizerServer(server);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            try
            {
                server.TestingStart();
            }
            catch (Exception ex)
            {
                SendLogMessage("Single-bot test failed to start server testing: " + ex, LogMessageType.Error);
                SafeDisposeBotPanel(bot);
                SafeRemoveOptimizerServer(server);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            int countSameTime = 0;
            DateTime timeServerLast = DateTime.MinValue;

            DateTime timeStartWaiting = DateTime.Now;

            while (bot.TimeServer < phaseTimeEnd)
            {
                if (IsStopRequested)
                {
                    break;
                }

                if (SafeWaitCancellationToken(token, TimeSpan.FromMilliseconds(1000)))
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
                SafeWaitCancellationToken(token, remaining);
            }

            SafeDisposeAwaitObject(awaitObj);

            return bot;
        }

        private void SafeDisposeAwaitObject(AwaitObject awaitObj)
        {
            try
            {
                awaitObj?.Dispose();
            }
            catch (Exception ex)
            {
                SendLogMessage("Single-bot test await object dispose failed: " + ex, LogMessageType.Error);
            }
        }

        private bool SafeWaitCancellationToken(CancellationToken token, TimeSpan timeout)
        {
            try
            {
                return token.WaitHandle.WaitOne(timeout);
            }
            catch (ObjectDisposedException)
            {
                return true;
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer cancellation token wait failed: " + ex, LogMessageType.Error);
                return true;
            }
        }

        private void SafeDisposeBotPanel(BotPanel bot)
        {
            try
            {
                bot?.Clear();
                bot?.Delete();
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer bot cleanup failed: " + ex, LogMessageType.Error);
            }
        }

        private void SafeRemoveOptimizerServer(OptimizerServer server)
        {
            try
            {
                if (server != null)
                {
                    ServerMaster.RemoveOptimizerServer(server);
                }
            }
            catch (Exception ex)
            {
                SendLogMessage("Single-bot test optimizer server cleanup failed: " + ex, LogMessageType.Error);
            }
        }

        private void SafeTrySetCanceled(TaskCompletionSource<OptimizerReport> completion)
        {
            try
            {
                completion?.TrySetCanceled();
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer eval completion cancel publish failed: " + ex, LogMessageType.Error);
            }
        }

        private void SafeTrySetCanceled(TaskCompletionSource<OptimizerReport> completion, CancellationToken cancellationToken)
        {
            try
            {
                completion?.TrySetCanceled(cancellationToken);
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer eval completion cancel(token) publish failed: " + ex, LogMessageType.Error);
            }
        }

        private void SafeTrySetResult(TaskCompletionSource<OptimizerReport> completion, OptimizerReport report)
        {
            try
            {
                completion?.TrySetResult(report);
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer eval completion result publish failed: " + ex, LogMessageType.Error);
            }
        }

        private void SafeTrySetException(TaskCompletionSource<OptimizerReport> completion, Exception exception)
        {
            try
            {
                completion?.TrySetException(exception);
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer eval completion exception publish failed: " + ex, LogMessageType.Error);
            }
        }

        private void DetachServerEvents(OptimizerServer server)
        {
            if (server == null)
            {
                return;
            }

            try
            {
                server.TestingEndEvent -= server_TestingEndEvent;
                server.TestingProgressChangeEvent -= server_TestingProgressChangeEvent;
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer server event detach failed: " + ex, LogMessageType.Error);
            }
        }

        private bool SafeTrySignalPhase(CountdownEvent phase)
        {
            if (phase == null || phase.IsSet)
            {
                return false;
            }

            try
            {
                phase.Signal();
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer phase signal failed: " + ex, LogMessageType.Error);
                return false;
            }
        }

        private void SafeReleaseServerSlot()
        {
            try
            {
                _serverSlots?.Release();
            }
            catch (ObjectDisposedException)
            {
                // ignored
            }
            catch (SemaphoreFullException)
            {
                // ignored
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer server slot release failed: " + ex, LogMessageType.Error);
            }
        }

        private void SafeLoadBotToLastFaze(BotPanel bot)
        {
            if (bot == null)
            {
                return;
            }

            if (bot.Parameters == null)
            {
                SendLogMessage("Optimizer report build/load skipped: bot parameters are null.", LogMessageType.Error);
                return;
            }

            try
            {
                if (ReportsToFazes == null || ReportsToFazes.Count == 0)
                {
                    SendLogMessage("Optimizer report load skipped: faze collection is empty.", LogMessageType.Error);
                    return;
                }

                OptimizerFazeReport lastFaze = ReportsToFazes[ReportsToFazes.Count - 1];
                if (lastFaze == null)
                {
                    SendLogMessage("Optimizer report load skipped: last faze is null.", LogMessageType.Error);
                    return;
                }

                lastFaze.Load(bot);
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer report load to last faze failed: " + ex, LogMessageType.Error);
            }
        }

        private bool TryBuildOptimizerReportFromBot(BotPanel bot, out OptimizerReport report)
        {
            report = null;
            if (bot == null)
            {
                return false;
            }

            if (bot.Parameters == null)
            {
                SendLogMessage("Optimizer report build/load skipped: bot parameters are null.", LogMessageType.Error);
                return false;
            }

            try
            {
                report = new OptimizerReport(bot.Parameters);
                report.LoadState(bot);
                return true;
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer report build from bot failed: " + ex, LogMessageType.Error);
                return false;
            }
        }

        private void SafeBindSecurityToServer(
            OptimizerServer server,
            Security security,
            TimeFrame timeFrame,
            DateTime timeStart,
            DateTime timeEnd,
            string sourceKind,
            int sourceIndex,
            int tabIndex)
        {
            if (server == null || security == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(security.Name))
            {
                string tabPart = tabIndex >= 0 ? (", tab index " + tabIndex) : string.Empty;
                SendLogMessage(
                    "CreateNewServer security bind skipped (" + sourceKind + ", source index " + sourceIndex + tabPart +
                    "): security name is empty.",
                    LogMessageType.Error);
                return;
            }

            try
            {
                server.GetDataToSecurity(security, timeFrame, timeStart, timeEnd);
            }
            catch (Exception ex)
            {
                string tabPart = tabIndex >= 0 ? (", tab index " + tabIndex) : string.Empty;
                SendLogMessage(
                    "CreateNewServer security bind failed (" + sourceKind + ", source index " + sourceIndex + tabPart +
                    ", security '" + security.Name + "'): " + ex,
                    LogMessageType.Error);
            }
        }

        private bool TryFindSecurityByName(string securityName, out Security security)
        {
            security = null;
            if (string.IsNullOrWhiteSpace(securityName))
            {
                SendLogMessage("CreateNewServer security lookup skipped: security name is empty.", LogMessageType.Error);
                return false;
            }

            if (_master?.Storage?.Securities == null)
            {
                SendLogMessage("CreateNewServer security lookup skipped: securities collection is unavailable.", LogMessageType.Error);
                return false;
            }

            try
            {
                List<Security> all = _master.Storage.Securities;
                for (int i = 0; i < all.Count; i++)
                {
                    Security candidate = all[i];
                    if (candidate != null && candidate.Name == securityName)
                    {
                        security = candidate;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                SendLogMessage("CreateNewServer security lookup failed for '" + securityName + "': " + ex, LogMessageType.Error);
            }

            SendLogMessage("CreateNewServer security lookup: security '" + securityName + "' was not found.", LogMessageType.Error);
            return false;
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
            int progressEnd;
            int progressMax;

            BotPanel bot = null;
            OptimizerServer server = null;

            lock (_serverRemoveLocker)
            {
                _countAllServersEndTest++;
                if (_countAllServersEndTest > _countAllServersMax)
                {
                    _countAllServersEndTest = _countAllServersMax;
                }

                progressEnd = _countAllServersEndTest;
                progressMax = _countAllServersMax;

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
                        if (TryBuildOptimizerReportFromBot(bot, out OptimizerReport report))
                        {
                            SafeTrySetResult(completion, report);
                        }
                        else
                        {
                            SafeTrySetCanceled(completion);
                        }
                    }
                    else
                    {
                        SafeLoadBotToLastFaze(bot);
                    }
                }

                for (int i = 0; i < _servers.Count; i++)
                {
                    if (_servers[i].NumberServer == serverNum)
                    {
                        DetachServerEvents(_servers[i]);
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

            PrimeProgressChangeEvent?.Invoke(progressEnd, progressMax);

            if (bot != null)
            {
                // уничтожаем робота
                SafeDisposeBotPanel(bot);
            }

            if (server != null)
            {
                SafeRemoveOptimizerServer(server);
            }

            CountdownEvent phase = _phaseCompletion;
            SafeTrySignalPhase(phase);

            SafeReleaseServerSlot();
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
