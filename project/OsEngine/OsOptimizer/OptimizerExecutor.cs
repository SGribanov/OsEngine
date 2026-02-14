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
        private readonly object _reportsSync = new object();

        private SemaphoreSlim _serverSlots;

        private CountdownEvent _phaseCompletion;

        private CancellationTokenSource _stopCts;

        private readonly ConcurrentDictionary<int, TaskCompletionSource<OptimizerReport>> _pendingEvaluationByServer =
            new ConcurrentDictionary<int, TaskCompletionSource<OptimizerReport>>();

        private readonly object _testBotsTimeSync = new object();
        private readonly object _startSync = new object();

        private bool IsPrimeWorkerActive()
        {
            return Volatile.Read(ref _primeThreadWorker) != null;
        }

        public bool Start(List<bool> parametersOn, List<IIStrategyParameter> parameters)
        {
            if (_master == null)
            {
                SendLogMessage("Optimizer start skipped: master context is null.", LogMessageType.Error);
                return false;
            }

            if (_master.Fazes == null || _master.Fazes.Count == 0)
            {
                SendLogMessage("Optimizer start skipped: faze configuration is empty.", LogMessageType.Error);
                return false;
            }

            if (_master.Storage == null)
            {
                SendLogMessage("Optimizer start skipped: storage context is null.", LogMessageType.Error);
                return false;
            }

            if (_master.BotToTest == null)
            {
                SendLogMessage("Optimizer start skipped: bot-to-test context is null.", LogMessageType.Error);
                return false;
            }

            if (_master.Settings == null)
            {
                SendLogMessage("Optimizer start skipped: settings context is null.", LogMessageType.Error);
                return false;
            }

            List<IIBotTab> botTabs = null;
            try
            {
                botTabs = _master.BotToTest.GetTabs();
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer start skipped: bot tabs retrieval failed. " + ex, LogMessageType.Error);
                return false;
            }

            if (botTabs == null || botTabs.Count == 0)
            {
                SendLogMessage("Optimizer start skipped: bot tabs collection is empty.", LogMessageType.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(_master.StrategyName))
            {
                SendLogMessage("Optimizer start skipped: strategy name is empty.", LogMessageType.Error);
                return false;
            }

            if (_master.ThreadsCount <= 0)
            {
                SendLogMessage(
                    "Optimizer start skipped: threads count must be positive (value " + _master.ThreadsCount + ").",
                    LogMessageType.Error);
                return false;
            }

            if (_master.IterationCount < 0)
            {
                SendLogMessage(
                    "Optimizer start skipped: iteration count cannot be negative (value " + _master.IterationCount + ").",
                    LogMessageType.Error);
                return false;
            }

            if (!Enum.IsDefined(typeof(OptimizationMethodType), _master.OptimizationMethod))
            {
                SendLogMessage(
                    "Optimizer start skipped: optimization method is invalid (value " + _master.OptimizationMethod + ").",
                    LogMessageType.Error);
                return false;
            }

            if (!Enum.IsDefined(typeof(SortBotsType), _master.ObjectiveMetric))
            {
                SendLogMessage(
                    "Optimizer start skipped: objective metric is invalid (value " + _master.ObjectiveMetric + ").",
                    LogMessageType.Error);
                return false;
            }

            if (!Enum.IsDefined(typeof(ObjectiveDirectionType), _master.ObjectiveDirection))
            {
                SendLogMessage(
                    "Optimizer start skipped: objective direction is invalid (value " + _master.ObjectiveDirection + ").",
                    LogMessageType.Error);
                return false;
            }

            if (!Enum.IsDefined(typeof(BayesianAcquisitionModeType), _master.BayesianAcquisitionMode))
            {
                SendLogMessage(
                    "Optimizer start skipped: bayesian acquisition mode is invalid (value " + _master.BayesianAcquisitionMode + ").",
                    LogMessageType.Error);
                return false;
            }

            if (parametersOn == null)
            {
                SendLogMessage("Optimizer start skipped: parametersOn is null.", LogMessageType.Error);
                return false;
            }

            if (parameters == null)
            {
                SendLogMessage("Optimizer start skipped: parameters is null.", LogMessageType.Error);
                return false;
            }

            if (parametersOn.Count != parameters.Count)
            {
                SendLogMessage(
                    "Optimizer start skipped: parametersOn count (" + parametersOn.Count +
                    ") does not match parameters count (" + parameters.Count + ").",
                    LogMessageType.Error);
                return false;
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                if (parameters[i] == null)
                {
                    SendLogMessage(
                        "Optimizer start skipped: parameter is null at index " + i + ".",
                        LogMessageType.Error);
                    return false;
                }
            }

            lock (_startSync)
            {
                if (IsPrimeWorkerActive())
                {
                    SendLogMessage(OsLocalization.Optimizer.Message1, LogMessageType.System);
                    return false;
                }

                _parametersOn = new List<bool>(parametersOn);
                _parameters = new List<IIStrategyParameter>(parameters);

                SendLogMessage(OsLocalization.Optimizer.Message2, LogMessageType.System);

                CancellationTokenSource previousStopCts = Interlocked.Exchange(ref _stopCts, new CancellationTokenSource());
                try
                {
                    previousStopCts?.Dispose();
                }
                catch (Exception ex)
                {
                    SendLogMessage("Optimizer start cleanup failed: previous stop token dispose. " + ex, LogMessageType.Error);
                }

                CountdownEvent previousPhase = Interlocked.Exchange(ref _phaseCompletion, null);
                try
                {
                    previousPhase?.Dispose();
                }
                catch (Exception ex)
                {
                    SendLogMessage("Optimizer start cleanup failed: previous phase completion dispose. " + ex, LogMessageType.Error);
                }

                lock (_serverRemoveLocker)
                {
                    _servers = new List<OptimizerServer>();
                    _countAllServersMax = 0;
                    _countAllServersEndTest = 0;
                    _serverNum = 1;
                }
                lock (_testBotsTimeSync)
                {
                    _testBotsTime.Clear();
                }
                lock (_reportsSync)
                {
                    ReportsToFazes = new List<OptimizerFazeReport>();
                }

                CancelPendingEvaluations("Optimizer start cleanup canceled stale pending evaluations: ");

                SemaphoreSlim previousServerSlots = Interlocked.Exchange(
                    ref _serverSlots,
                    new SemaphoreSlim(Math.Max(1, _master.ThreadsCount), Math.Max(1, _master.ThreadsCount)));

                try
                {
                    previousServerSlots?.Dispose();
                }
                catch (Exception ex)
                {
                    SendLogMessage("Optimizer start cleanup failed: previous server slots dispose. " + ex, LogMessageType.Error);
                }

                Thread primeWorker = new Thread(PrimeThreadWorkerPlace);
                primeWorker.Name = "OptimizerExecutorThread";
                primeWorker.IsBackground = true;
                Volatile.Write(ref _primeThreadWorker, primeWorker);
                primeWorker.Start();

                return true;
            }
        }

        public void Stop()
        {
            lock (_startSync)
            {
                CancellationTokenSource stopCts = _stopCts;
                if (stopCts != null)
                {
                    try
                    {
                        stopCts.Cancel();
                    }
                    catch (ObjectDisposedException)
                    {
                        // already disposed during concurrent cleanup
                    }
                    catch (Exception ex)
                    {
                        SendLogMessage("Optimizer stop cancellation failed: " + ex, LogMessageType.Error);
                    }
                }
            }

            SendLogMessage(OsLocalization.Optimizer.Message3, LogMessageType.System);
        }

        private bool IsStopRequested => GetStopTokenOrNone().IsCancellationRequested;

        private CancellationToken GetStopTokenOrNone()
        {
            CancellationTokenSource stopCts = _stopCts;
            if (stopCts == null)
            {
                return CancellationToken.None;
            }

            try
            {
                return stopCts.Token;
            }
            catch (ObjectDisposedException)
            {
                return CancellationToken.None;
            }
        }

        #endregion

        #region Optimization algorithm

        private void PrimeThreadWorkerPlace()
        {
            try
            {
                if (_master == null)
                {
                    SendLogMessage("Optimizer prime worker skipped: master context is null at runtime.", LogMessageType.Error);
                    PublishTestReadySnapshot();
                    return;
                }

                lock (_reportsSync)
                {
                    ReportsToFazes = new List<OptimizerFazeReport>();
                }
                List<OptimizerFaze> fazesSource = _master.Fazes;
                if (fazesSource == null || fazesSource.Count == 0)
                {
                    SendLogMessage("Optimizer prime worker skipped: faze configuration is unavailable at runtime.", LogMessageType.Error);
                    PublishTestReadySnapshot();
                    return;
                }

                List<OptimizerFaze> fazesSnapshot = new List<OptimizerFaze>(fazesSource);
                if (fazesSnapshot.Count == 0)
                {
                    SendLogMessage("Optimizer prime worker skipped: faze snapshot is empty at runtime.", LogMessageType.Error);
                    PublishTestReadySnapshot();
                    return;
                }

                string strategyName = _master.StrategyName;
                bool isScript = _master.IsScript;
                int iterationCount = _master.IterationCount;
                int threadsCount = _master.ThreadsCount;
                bool lastInSample = _master.LastInSample;
                OptimizationMethodType optimizationMethod = _master.OptimizationMethod;
                SortBotsType objectiveMetric = _master.ObjectiveMetric;
                ObjectiveDirectionType objectiveDirection = _master.ObjectiveDirection;
                BayesianAcquisitionModeType bayesianAcquisitionMode = _master.BayesianAcquisitionMode;
                List<bool> parametersOnSnapshot = _parametersOn == null ? null : new List<bool>(_parametersOn);
                List<IIStrategyParameter> parametersSnapshot = _parameters == null ? null : new List<IIStrategyParameter>(_parameters);

                if (string.IsNullOrWhiteSpace(strategyName))
                {
                    SendLogMessage("Optimizer prime worker skipped: strategy name is unavailable at runtime.", LogMessageType.Error);
                    PublishTestReadySnapshot();
                    return;
                }

                if (parametersOnSnapshot == null || parametersSnapshot == null)
                {
                    SendLogMessage("Optimizer prime worker skipped: parameters snapshot is unavailable at runtime.", LogMessageType.Error);
                    PublishTestReadySnapshot();
                    return;
                }

                if (iterationCount < 0)
                {
                    SendLogMessage(
                        "Optimizer prime worker skipped: iteration count is invalid at runtime (value " + iterationCount + ").",
                        LogMessageType.Error);
                    PublishTestReadySnapshot();
                    return;
                }

                if (threadsCount <= 0)
                {
                    SendLogMessage(
                        "Optimizer prime worker skipped: threads count is invalid at runtime (value " + threadsCount + ").",
                        LogMessageType.Error);
                    PublishTestReadySnapshot();
                    return;
                }

                if (!Enum.IsDefined(typeof(OptimizationMethodType), optimizationMethod))
                {
                    SendLogMessage(
                        "Optimizer prime worker skipped: optimization method is invalid at runtime (value " + optimizationMethod + ").",
                        LogMessageType.Error);
                    PublishTestReadySnapshot();
                    return;
                }

                if (!Enum.IsDefined(typeof(SortBotsType), objectiveMetric))
                {
                    SendLogMessage(
                        "Optimizer prime worker skipped: objective metric is invalid at runtime (value " + objectiveMetric + ").",
                        LogMessageType.Error);
                    PublishTestReadySnapshot();
                    return;
                }

                if (!Enum.IsDefined(typeof(ObjectiveDirectionType), objectiveDirection))
                {
                    SendLogMessage(
                        "Optimizer prime worker skipped: objective direction is invalid at runtime (value " + objectiveDirection + ").",
                        LogMessageType.Error);
                    PublishTestReadySnapshot();
                    return;
                }

                if (!Enum.IsDefined(typeof(BayesianAcquisitionModeType), bayesianAcquisitionMode))
                {
                    SendLogMessage(
                        "Optimizer prime worker skipped: bayesian acquisition mode is invalid at runtime (value " + bayesianAcquisitionMode + ").",
                        LogMessageType.Error);
                    PublishTestReadySnapshot();
                    return;
                }

                if (parametersOnSnapshot.Count != parametersSnapshot.Count)
                {
                    SendLogMessage(
                        "Optimizer prime worker skipped: parameters snapshot count mismatch (flags " + parametersOnSnapshot.Count +
                        ", params " + parametersSnapshot.Count + ").",
                        LogMessageType.Error);
                    PublishTestReadySnapshot();
                    return;
                }

                for (int i = 0; i < parametersSnapshot.Count; i++)
                {
                    if (parametersSnapshot[i] == null)
                    {
                        SendLogMessage(
                            "Optimizer prime worker skipped: parameter snapshot contains null at index " + i + ".",
                            LogMessageType.Error);
                        PublishTestReadySnapshot();
                        return;
                    }
                }

                int countBotsRaw = BotCountOneFaze(parametersSnapshot, parametersOnSnapshot);
                int countBots = Math.Max(0, countBotsRaw);
                if (countBotsRaw < 0)
                {
                    SendLogMessage(
                        "Optimizer bot count estimate was negative and was clamped to zero: " + countBotsRaw + ".",
                        LogMessageType.Error);
                }

                long estimatedMaxTestsLong = (long)countBots * Math.Max(0, iterationCount) * 2L;

                if (lastInSample && estimatedMaxTestsLong > 0)
                {
                    estimatedMaxTestsLong -= countBots;
                }

                int estimatedMaxTests = (int)Math.Min(int.MaxValue, Math.Max(0L, estimatedMaxTestsLong));

                SendLogMessage(OsLocalization.Optimizer.Message4 + estimatedMaxTests, LogMessageType.System);

                DateTime timeStart = DateTime.Now;
                OptimizerFazeReport latestInSampleReport = null;

                for (int i = 0; i < fazesSnapshot.Count; i++)
                {
                    if (IsStopRequested)
                    {
                        PublishTestReadySnapshot();
                        return;
                    }

                    OptimizerFaze currentFaze = fazesSnapshot[i];
                    if (currentFaze == null)
                    {
                        SendLogMessage("Optimizer phase skipped: faze entry is null at index " + i + ".", LogMessageType.Error);
                        continue;
                    }

                    if (currentFaze.TimeEnd <= currentFaze.TimeStart)
                    {
                        SendLogMessage(
                            "Optimizer phase skipped: invalid faze time range at index " + i +
                            " (start " + currentFaze.TimeStart + ", end " + currentFaze.TimeEnd + ").",
                            LogMessageType.Error);
                        continue;
                    }

                    if (currentFaze.TypeFaze == OptimizerFazeType.InSample)
                    {
                        OptimizerFazeReport report = new OptimizerFazeReport();
                        report.Faze = currentFaze;

                        lock (_reportsSync)
                        {
                            ReportsToFazes.Add(report);
                        }

                        StartAsuncBotFactoryInSample(countBots, strategyName, isScript, "InSample");

                        StartOptimizeFazeInSample(report, parametersSnapshot, parametersOnSnapshot, countBots);

                        EndOfFazeFiltration(report);
                        if (report == null)
                        {
                            SendLogMessage("InSample phase produced null report container at index " + i + ".", LogMessageType.Error);
                            latestInSampleReport = null;
                        }
                        else if (report.Reports == null)
                        {
                            SendLogMessage("InSample phase produced null report list at index " + i + ".", LogMessageType.Error);
                            latestInSampleReport = null;
                        }
                        else
                        {
                            latestInSampleReport = report;
                        }
                    }
                    else
                    {
                        OptimizerFazeReport inSampleReport = latestInSampleReport;
                        if (inSampleReport == null)
                        {
                            SendLogMessage("OutOfSample phase skipped: no previous in-sample phase reports are available.", LogMessageType.Error);
                            continue;
                        }

                        if (inSampleReport.Faze == null
                            || inSampleReport.Faze.TypeFaze != OptimizerFazeType.InSample)
                        {
                            SendLogMessage("OutOfSample phase skipped: previous source report is not an InSample phase.", LogMessageType.Error);
                            continue;
                        }

                        int inSampleCount = inSampleReport?.Reports?.Count ?? 0;
                        SendLogMessage("ReportsCount " + inSampleCount.ToString(), LogMessageType.System);

                        OptimizerFazeReport report = new OptimizerFazeReport();
                        report.Faze = currentFaze;

                        lock (_reportsSync)
                        {
                            ReportsToFazes.Add(report);
                        }

                        StartAsuncBotFactoryOutOfSample(inSampleReport, strategyName, isScript, "OutOfSample");

                        StartOptimizeFazeOutOfSample(report, inSampleReport);
                    }
                }

                GC.Collect(2, GCCollectionMode.Optimized, blocking: false);

                TimeSpan time = DateTime.Now - timeStart;

                SendLogMessage(OsLocalization.Optimizer.Message7, LogMessageType.System);
                SendLogMessage("Total test time = " + time.ToString(), LogMessageType.System);

                PublishTestReadySnapshot();
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer prime worker failed: " + ex, LogMessageType.Error);
                PublishTestReadySnapshot();
            }
            finally
            {
                Interlocked.Exchange(ref _primeThreadWorker, null);
                DisposeRunSynchronization();
            }
        }

        private void StartAsuncBotFactoryInSample(int botCount, string botType, bool isScript, string faze)
        {
            string normalizedFaze = string.IsNullOrWhiteSpace(faze) ? "InSample" : faze.Trim();
            string normalizedBotType = string.IsNullOrWhiteSpace(botType) ? null : botType.Trim();

            if (botCount <= 0)
            {
                SendLogMessage(
                    "Async bot factory start skipped (InSample): bot count is non-positive (count " + botCount +
                    ", bot type '" + normalizedBotType + "', faze '" + normalizedFaze +
                    "', isScript " + isScript + ").",
                    LogMessageType.System);
                return;
            }

            if (string.IsNullOrWhiteSpace(normalizedBotType))
            {
                SendLogMessage("Async bot factory start skipped (InSample): bot type is empty.", LogMessageType.Error);
                return;
            }

            int expectedNamesCount = Math.Max(0, botCount);
            List<string> botNames = new List<string>(expectedNamesCount);
            HashSet<string> uniqueBotNames = new HashSet<string>(expectedNamesCount, StringComparer.Ordinal);
            int startServerIndex;
            lock (_serverRemoveLocker)
            {
                startServerIndex = _serverNum;
            }
            string fazeSuffix = " " + normalizedFaze;

            for (int i = 0; i < botCount; i++)
            {
                string botNameBase = (startServerIndex + i) + " OpT";
                string botName = botNameBase.EndsWith(fazeSuffix, StringComparison.Ordinal)
                    ? botNameBase
                    : botNameBase + fazeSuffix;
                if (!uniqueBotNames.Add(botName))
                {
                    SendLogMessage(
                        "Async bot factory start skipped duplicate bot name (InSample): " + botName +
                        " at index " + i + " of " + expectedNamesCount +
                        " (bot type '" + normalizedBotType + "', faze '" + normalizedFaze +
                        "', isScript " + isScript + ").",
                        LogMessageType.Error);
                    continue;
                }

                botNames.Add(botName);
            }

            if (botNames.Count == 0)
            {
                SendLogMessage(
                    "Async bot factory start skipped (InSample): no bot names generated (expected " +
                    expectedNamesCount + ", bot type '" + normalizedBotType + "', faze '" + normalizedFaze +
                    "', isScript " + isScript + ").",
                    LogMessageType.System);
                return;
            }

            try
            {
                _asyncBotFactory.CreateNewBots(botNames, normalizedBotType, isScript, StartProgram.IsOsOptimizer);
            }
            catch (Exception ex)
            {
                SendLogMessage(
                    "Async bot factory start failed (InSample, count " + botNames.Count +
                    ", bot type '" + normalizedBotType + "', faze '" + normalizedFaze +
                    "', isScript " + isScript + "): " + ex,
                    LogMessageType.Error);
                throw;
            }
        }

        private void StartAsuncBotFactoryOutOfSample(OptimizerFazeReport reportFiltered, string botType, bool isScript, string faze)
        {
            string normalizedFaze = string.IsNullOrWhiteSpace(faze) ? "OutOfSample" : faze.Trim();
            string normalizedBotType = string.IsNullOrWhiteSpace(botType) ? null : botType.Trim();

            if (string.IsNullOrWhiteSpace(normalizedBotType))
            {
                SendLogMessage("Async bot factory start skipped (OutOfSample): bot type is empty.", LogMessageType.Error);
                return;
            }

            if (reportFiltered?.Reports == null)
            {
                SendLogMessage("Async bot factory start skipped (OutOfSample): source reports are unavailable.", LogMessageType.Error);
                return;
            }

            List<OptimizerReport> reports = new List<OptimizerReport>(reportFiltered.Reports);
            if (reports.Count == 0)
            {
                SendLogMessage(
                    "Async bot factory start skipped (OutOfSample): source reports snapshot is empty (count " +
                    reports.Count + ", bot type '" + normalizedBotType + "', faze '" + normalizedFaze +
                    "', isScript " + isScript + ").",
                    LogMessageType.System);
                return;
            }

            int expectedNamesCount = Math.Max(0, reports.Count);
            List<string> botNames = new List<string>(expectedNamesCount);
            HashSet<string> uniqueBotNames = new HashSet<string>(expectedNamesCount, StringComparer.Ordinal);
            const string inSampleSuffix = " InSample";
            string fazeSuffix = " " + normalizedFaze;
            int reportsCount = reports.Count;

            for (int i = 0; i < reportsCount; i++)
            {
                OptimizerReport sourceReport = reports[i];
                if (sourceReport == null)
                {
                    SendLogMessage(
                        "Async bot factory start skipped (OutOfSample): source report is null at index " + i +
                        " of " + reportsCount + " (bot type '" + normalizedBotType +
                        "', faze '" + normalizedFaze + "', isScript " + isScript + ").",
                        LogMessageType.Error);
                    continue;
                }

                string sourceBotName = sourceReport.BotName;
                if (string.IsNullOrWhiteSpace(sourceBotName))
                {
                    SendLogMessage(
                        "Async bot factory start skipped (OutOfSample): source report bot name is empty at index " + i +
                        " of " + reportsCount + " (bot type '" + normalizedBotType +
                        "', faze '" + normalizedFaze + "', isScript " + isScript + ").",
                        LogMessageType.Error);
                    continue;
                }

                string transformedBotName = sourceBotName.Trim();
                if (transformedBotName.EndsWith(inSampleSuffix, StringComparison.Ordinal))
                {
                    transformedBotName = transformedBotName.Substring(0, transformedBotName.Length - inSampleSuffix.Length);
                }

                transformedBotName = transformedBotName.Trim();
                if (string.IsNullOrWhiteSpace(transformedBotName))
                {
                    SendLogMessage(
                        "Async bot factory start skipped (OutOfSample): transformed bot name is empty at index " + i +
                        " of " + reportsCount + " (bot type '" + normalizedBotType +
                        "', faze '" + normalizedFaze + "', isScript " + isScript + ").",
                        LogMessageType.Error);
                    continue;
                }

                string botName = transformedBotName.EndsWith(fazeSuffix, StringComparison.Ordinal)
                    ? transformedBotName
                    : transformedBotName + fazeSuffix;
                if (!uniqueBotNames.Add(botName))
                {
                    SendLogMessage(
                        "Async bot factory start skipped duplicate bot name (OutOfSample): " + botName +
                        " at index " + i + " of " + reportsCount +
                        " (bot type '" + normalizedBotType + "', faze '" + normalizedFaze +
                        "', isScript " + isScript + ").",
                        LogMessageType.Error);
                    continue;
                }

                botNames.Add(botName);
            }

            if (botNames.Count == 0)
            {
                SendLogMessage(
                    "Async bot factory start skipped (OutOfSample): no bot names generated (expected " +
                    expectedNamesCount + ", bot type '" + normalizedBotType + "', faze '" + normalizedFaze +
                    "', isScript " + isScript + ").",
                    LogMessageType.System);
                return;
            }

            try
            {
                _asyncBotFactory.CreateNewBots(botNames, normalizedBotType, isScript, StartProgram.IsOsOptimizer);
            }
            catch (Exception ex)
            {
                SendLogMessage(
                    "Async bot factory start failed (OutOfSample, count " + botNames.Count +
                    ", bot type '" + normalizedBotType + "', faze '" + normalizedFaze +
                    "', isScript " + isScript + "): " + ex,
                    LogMessageType.Error);
                throw;
            }
        }

        private Thread _primeThreadWorker;

        public int BotCountOneFaze(List<IIStrategyParameter> parameters, List<bool> parametersOn)
        {
            IOptimizationStrategy strategy = GetInSampleOptimizationStrategy(null);
            if (strategy == null)
            {
                SendLogMessage("Optimizer bot-count estimate fallback: strategy is unavailable.", LogMessageType.Error);
                return 0;
            }

            return strategy.EstimateBotCount(parameters, parametersOn);
        }

        private IOptimizationStrategy GetInSampleOptimizationStrategy(IBotEvaluator evaluator)
        {
            if (_master == null)
            {
                SendLogMessage("Optimizer strategy creation skipped: master context is null.", LogMessageType.Error);
                return null;
            }

            if (!Enum.IsDefined(typeof(OptimizationMethodType), _master.OptimizationMethod)
                || !Enum.IsDefined(typeof(SortBotsType), _master.ObjectiveMetric)
                || !Enum.IsDefined(typeof(ObjectiveDirectionType), _master.ObjectiveDirection)
                || !Enum.IsDefined(typeof(BayesianAcquisitionModeType), _master.BayesianAcquisitionMode))
            {
                SendLogMessage("Optimizer strategy creation skipped: runtime strategy settings are invalid.", LogMessageType.Error);
                return null;
            }

            if (_master.ThreadsCount <= 0)
            {
                SendLogMessage(
                    "Optimizer strategy creation skipped: threads count is invalid at runtime (value " +
                    _master.ThreadsCount + ").",
                    LogMessageType.Error);
                return null;
            }

            int parallel = _master.ThreadsCount;

            IOptimizationStrategy strategy = null;
            string infoMessage = null;
            try
            {
                strategy = OptimizationStrategyFactory.CreateInSampleStrategy(
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
                    out infoMessage);
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer strategy creation failed: " + ex, LogMessageType.Error);
                return null;
            }

            if (!string.IsNullOrEmpty(infoMessage))
            {
                SendLogMessage(infoMessage, LogMessageType.System);
            }

            return strategy;
        }

        public List<OptimizerFazeReport> ReportsToFazes = new List<OptimizerFazeReport>();

        private void StartOptimizeFazeInSample(OptimizerFazeReport report,
            List<IIStrategyParameter> allParameters, List<bool> parametersToOptimization, int inSampleBotsCount)
        {
            ReloadAllParam(allParameters);
            ReplacePhaseCompletion(inSampleBotsCount);

            if (inSampleBotsCount > 0)
            {
                int progressEnd;
                int progressMax;

                lock (_serverRemoveLocker)
                {
                    _countAllServersMax += inSampleBotsCount;
                    progressEnd = _countAllServersEndTest;
                    progressMax = _countAllServersMax;
                }

                SafeInvokePrimeProgress(progressEnd, progressMax);
            }

            // 2 проходим первую фазу, когда нужно обойти все варианты

            IBotEvaluator evaluator = new BotEvaluator(async (all, optimized, token) =>
            {
                return await StartNewBotForEvaluationAsync(all, optimized, report, " OpT InSample", token)
                    .ConfigureAwait(false);
            });

            IOptimizationStrategy strategy = GetInSampleOptimizationStrategy(evaluator);
            if (strategy == null)
            {
                SendLogMessage("InSample phase skipped: optimization strategy is unavailable.", LogMessageType.Error);
                CompensateSkippedInSamplePhase(inSampleBotsCount);
                WaitCurrentPhaseToComplete();
                return;
            }

            List<OptimizerReport> reports =
                strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, GetStopTokenOrNone())
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
            ReplacePhaseCompletion(outOfSampleBotsCount);

            if (outOfSampleBotsCount > 0)
            {
                int progressEnd;
                int progressMax;

                lock (_serverRemoveLocker)
                {
                    _countAllServersMax += outOfSampleBotsCount;
                    progressEnd = _countAllServersEndTest;
                    progressMax = _countAllServersMax;
                }

                SafeInvokePrimeProgress(progressEnd, progressMax);
            }

            if (outOfSampleBotsCount == 0)
            {
                int progressEnd;
                int progressMax;

                lock (_serverRemoveLocker)
                {
                    progressEnd = _countAllServersEndTest;
                    progressMax = _countAllServersMax;
                }

                SendLogMessage("OutOfSample has no valid source reports to process.", LogMessageType.System);
                SafeInvokePrimeProgress(progressEnd, progressMax);
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
                    PublishTestReadySnapshot();
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
                AddCompensatedProgress(1);
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
                AddCompensatedProgress(signaledCount);
            }
        }

        private void CompensateSkippedInSamplePhase(int skippedCount)
        {
            if (skippedCount <= 0)
            {
                return;
            }

            CountdownEvent phase = _phaseCompletion;
            if (phase == null)
            {
                return;
            }

            int signaledCount = 0;

            while (skippedCount > 0)
            {
                if (!SafeTrySignalPhase(phase))
                {
                    break;
                }

                signaledCount++;
                skippedCount--;
            }

            if (signaledCount > 0)
            {
                AddCompensatedProgress(signaledCount);
            }
        }

        private void AddCompensatedProgress(int count)
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

            SafeInvokePrimeProgress(progressEnd, progressMax);
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
            if (server == null)
            {
                SendLogMessage("StartNewBot skipped: optimizer server was not created.", LogMessageType.Error);
                SafeTrySetCanceled(completionSource);
                FinalizeNotStartedBot(null, null);
                return;
            }

            if (completionSource != null)
            {
                if (!_pendingEvaluationByServer.TryAdd(server.NumberServer, completionSource))
                {
                    SendLogMessage(
                        "StartNewBot skipped: pending evaluation entry already exists for server " + server.NumberServer + ".",
                        LogMessageType.Error);
                    SafeTrySetCanceled(completionSource);
                    FinalizeNotStartedBot(server, null);
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(botName))
            {
                SendLogMessage("StartNewBot skipped empty bot name prefix check: generated name is empty.", LogMessageType.Error);
                botName = server.NumberServer.ToString();
            }
            else if (!char.IsDigit(botName[0]))
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

            try
            {
                server.TestingStart();
            }
            catch (Exception ex)
            {
                SendLogMessage("StartNewBot failed to start server testing: " + ex, LogMessageType.Error);
                FinalizeNotStartedBot(server, bot);
            }
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
                SafeTrySetCanceled(completion, cancellationToken);
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
            CancellationToken token = GetStopTokenOrNone();
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
            catch (Exception ex)
            {
                SendLogMessage("Optimizer server slot acquire failed: " + ex, LogMessageType.Error);
                return false;
            }
        }

        private void WaitCurrentPhaseToComplete()
        {
            CancellationToken token = GetStopTokenOrNone();
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
            catch (Exception ex)
            {
                SendLogMessage("Optimizer phase wait failed: " + ex, LogMessageType.Error);
                return;
            }
        }

        private void ReplacePhaseCompletion(int participantsCount)
        {
            CountdownEvent nextPhase = new CountdownEvent(participantsCount);
            CountdownEvent previousPhase = Interlocked.Exchange(ref _phaseCompletion, nextPhase);

            if (previousPhase == null)
            {
                return;
            }

            try
            {
                previousPhase.Dispose();
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer phase replacement dispose failed: " + ex, LogMessageType.Error);
            }
        }

        private void DisposeRunSynchronization()
        {
            CancelPendingEvaluations("Optimizer cleanup canceled pending evaluations: ");

            CountdownEvent phaseCompletion = Interlocked.Exchange(ref _phaseCompletion, null);
            try
            {
                phaseCompletion?.Dispose();
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer sync cleanup failed: phase completion dispose. " + ex, LogMessageType.Error);
            }

            SemaphoreSlim serverSlots = Interlocked.Exchange(ref _serverSlots, null);
            try
            {
                serverSlots?.Dispose();
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer sync cleanup failed: server slots dispose. " + ex, LogMessageType.Error);
            }

            CancellationTokenSource stopCts = Interlocked.Exchange(ref _stopCts, null);
            try
            {
                stopCts?.Dispose();
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer sync cleanup failed: stop token source dispose. " + ex, LogMessageType.Error);
            }
        }

        private void CancelPendingEvaluations(string messagePrefix)
        {
            int canceled = 0;

            foreach (KeyValuePair<int, TaskCompletionSource<OptimizerReport>> pending in _pendingEvaluationByServer)
            {
                if (_pendingEvaluationByServer.TryRemove(pending.Key, out TaskCompletionSource<OptimizerReport> completion))
                {
                    SafeTrySetCanceled(completion);
                    canceled++;
                }
            }

            if (canceled > 0)
            {
                SendLogMessage(messagePrefix + canceled + ".", LogMessageType.System);
            }
        }

        private void FinalizeNotStartedBot(OptimizerServer server, BotPanel bot)
        {
            SafeRemoveBotFromInTest(bot);
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

            if (report.Faze.TimeEnd <= report.Faze.TimeStart)
            {
                SendLogMessage("CreateNewServer skipped: invalid faze time range.", LogMessageType.Error);
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

            int serverNumber;
            lock (_serverRemoveLocker)
            {
                serverNumber = _serverNum;
                _serverNum++;
            }

            // 1. Create a new server for optimization. And one thread respectively
            // 1. создаём новый сервер для оптимизации. И один поток соответственно
            OptimizerServer server = null;
            try
            {
                server = ServerMaster.CreateNextOptimizerServer(_master.Storage, serverNumber,
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
                _servers.Add(server);
            }

            if (needToDelete)
            {
                server.TestingEndEvent += server_TestingEndEvent;
            }

            server.TypeTesterData = _master.Storage.TypeTesterData;
            server.TestingProgressChangeEvent += server_TestingProgressChangeEvent;

            List<IIBotTab> sources = null;
            try
            {
                sources = _master.BotToTest.GetTabs();
            }
            catch (Exception ex)
            {
                SendLogMessage("CreateNewServer skipped: bot tabs retrieval failed. " + ex, LogMessageType.Error);
                SafeRemoveOptimizerServer(server);
                return null;
            }

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
                    BotTabSimple simple = sources[i] as BotTabSimple;
                    if (simple == null)
                    {
                        SendLogMessage("CreateNewServer skipped simple tab bind: invalid tab instance at source index " + i + ".", LogMessageType.Error);
                        continue;
                    }

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
                    BotTabIndex index = sources[i] as BotTabIndex;
                    if (index == null)
                    {
                        SendLogMessage("CreateNewServer skipped index bind: invalid tab instance at source index " + i + ".", LogMessageType.Error);
                        continue;
                    }

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
                    BotTabScreener screener = sources[i] as BotTabScreener;
                    if (screener == null)
                    {
                        SendLogMessage("CreateNewServer skipped screener bind: invalid tab instance at source index " + i + ".", LogMessageType.Error);
                        continue;
                    }

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
                else
                {
                    SendLogMessage(
                        "CreateNewServer skipped unsupported tab type at source index " + i + ": " + sources[i].TabType + ".",
                        LogMessageType.Error);
                }
            }

            return server;
        }

        private BotPanel CreateNewBot(string botName,
            List<IIStrategyParameter> parameters,
            List<IIStrategyParameter> parametersOptimized,
            OptimizerServer server, StartProgram regime)
        {
            if (_master == null || _master.BotToTest == null)
            {
                SendLogMessage("CreateNewBot skipped: optimizer master context is not initialized.", LogMessageType.Error);
                return null;
            }

            if (server == null)
            {
                SendLogMessage("CreateNewBot skipped: optimizer server is null.", LogMessageType.Error);
                return null;
            }

            if (string.IsNullOrWhiteSpace(botName))
            {
                SendLogMessage("CreateNewBot skipped: bot name is empty.", LogMessageType.Error);
                return null;
            }

            if (parameters == null || parametersOptimized == null)
            {
                SendLogMessage("CreateNewBot skipped: bot parameter sets are null.", LogMessageType.Error);
                return null;
            }

            try
            {
                _botConfigurator.BotToTest = _master.BotToTest;
                return _botConfigurator.CreateAndConfigureBot(
                    botName,
                    parameters,
                    parametersOptimized,
                    server,
                    regime,
                    GetStopTokenOrNone());
            }
            catch (Exception ex)
            {
                SendLogMessage("CreateNewBot failed: " + ex, LogMessageType.Error);
                return null;
            }
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

            if (_master == null)
            {
                SendLogMessage("Single-bot test skipped: optimizer master context is not initialized.", LogMessageType.Error);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            if (IsPrimeWorkerActive())
            {
                SendLogMessage("Single-bot test request ignored: previous test worker is still active.", LogMessageType.System);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

            CancellationToken token = GetStopTokenOrNone();

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

            BotPanel bot = null;
            try
            {
                bot = CreateNewBot(botName,
                    parametrs, parametrs, server, startProgram);
            }
            catch (Exception ex)
            {
                SendLogMessage("Single-bot test skipped due to bot creation error: " + ex, LogMessageType.Error);
                SafeRemoveOptimizerServer(server);
                SafeDisposeAwaitObject(awaitObj);
                return null;
            }

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

        private void SafeRemoveBotFromInTest(BotPanel bot)
        {
            if (bot == null)
            {
                return;
            }

            try
            {
                lock (_serverRemoveLocker)
                {
                    _botsInTest.Remove(bot);
                }
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer bot list cleanup failed: " + ex, LogMessageType.Error);
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
            if (phase == null)
            {
                return false;
            }

            try
            {
                if (phase.IsSet)
                {
                    return false;
                }

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
            SemaphoreSlim slots = _serverSlots;
            if (slots == null)
            {
                return;
            }

            try
            {
                slots.Release();
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
                OptimizerFazeReport lastFaze;
                lock (_reportsSync)
                {
                    if (ReportsToFazes == null || ReportsToFazes.Count == 0)
                    {
                        SendLogMessage("Optimizer report load skipped: faze collection is empty.", LogMessageType.Error);
                        return;
                    }

                    lastFaze = ReportsToFazes[ReportsToFazes.Count - 1];
                }

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
                string tabPart = tabIndex >= 0 ? (", tab index " + tabIndex) : string.Empty;
                string serverPart = server == null ? "server is null" : "server is set";
                string securityPart = security == null ? "security is null" : "security is set";
                SendLogMessage(
                    "CreateNewServer security bind skipped (" + sourceKind + ", source index " + sourceIndex + tabPart +
                    "): " + serverPart + ", " + securityPart + ".",
                    LogMessageType.Error);
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

            string normalizedSecurityName = securityName.Trim();

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
                    if (candidate != null
                        && !string.IsNullOrWhiteSpace(candidate.Name)
                        && string.Equals(candidate.Name.Trim(), normalizedSecurityName, StringComparison.Ordinal))
                    {
                        security = candidate;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                SendLogMessage("CreateNewServer security lookup failed for '" + normalizedSecurityName + "': " + ex, LogMessageType.Error);
            }

            SendLogMessage("CreateNewServer security lookup: security '" + normalizedSecurityName + "' was not found.", LogMessageType.Error);
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
            SafeInvokeTestingProgress(100, 100, serverNum);

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
                else
                {
                    SendLogMessage("Optimizer end-event: bot was not found for server " + serverNum + ".", LogMessageType.Error);

                    if (_pendingEvaluationByServer.TryRemove(serverNum, out TaskCompletionSource<OptimizerReport> completion))
                    {
                        SafeTrySetCanceled(completion);
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

                int threadsCount = Math.Max(1, _master?.ThreadsCount ?? 1);
                TimeSpan? timeToEndValue = null;

                lock (_testBotsTimeSync)
                {
                    _testBotsTime.Add(testTime);

                    if (_testBotsTime.Count >= threadsCount)
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

                        decimal secondsToEndDivideThreads = secondsToEndAllTests / threadsCount;

                        TimeSpan timeToEnd = TimeSpan.FromSeconds(Convert.ToInt32(secondsToEndDivideThreads));

                        if (timeToEnd.TotalSeconds != 0)
                        {
                            timeToEndValue = timeToEnd;
                        }
                    }
                }

                Action<TimeSpan> etaHandler = TimeToEndChangeEvent;

                if (etaHandler != null
                    && timeToEndValue.HasValue)
                {
                    try
                    {
                        etaHandler(timeToEndValue.Value);
                    }
                    catch (Exception ex)
                    {
                        SendLogMessage("Optimizer ETA event dispatch failed: " + ex, LogMessageType.Error);
                    }
                }
            }

            SafeInvokePrimeProgress(progressEnd, progressMax);

            if (bot != null)
            {
                // уничтожаем робота
                SafeDisposeBotPanel(bot);
            }

            if (server != null)
            {
                SafeRemoveOptimizerServer(server);
            }
            else
            {
                SendLogMessage("Optimizer end-event: server was not found in active list for server " + serverNum + ".", LogMessageType.Error);
            }

            CountdownEvent phase = _phaseCompletion;
            SafeTrySignalPhase(phase);

            SafeReleaseServerSlot();
        }

        public event Action<TimeSpan> TimeToEndChangeEvent;

        public event Action<List<OptimizerFazeReport>> TestReadyEvent;

        private void PublishTestReadySnapshot()
        {
            SafeInvokeTestReady(GetReportsSnapshotForPublish());
        }

        private List<OptimizerFazeReport> GetReportsSnapshotForPublish()
        {
            lock (_reportsSync)
            {
                List<OptimizerFazeReport> reports = ReportsToFazes;
                if (reports == null)
                {
                    return new List<OptimizerFazeReport>();
                }

                return new List<OptimizerFazeReport>(reports);
            }
        }

        private void SafeInvokePrimeProgress(int progressEnd, int progressMax)
        {
            Action<int, int> handler = PrimeProgressChangeEvent;

            if (handler == null)
            {
                return;
            }

            try
            {
                handler(progressEnd, progressMax);
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer prime progress event dispatch failed: " + ex, LogMessageType.Error);
            }
        }

        private void SafeInvokeTestReady(List<OptimizerFazeReport> reports)
        {
            Action<List<OptimizerFazeReport>> handler = TestReadyEvent;

            if (handler == null)
            {
                return;
            }

            try
            {
                handler(reports);
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer test-ready event dispatch failed: " + ex, LogMessageType.Error);
            }
        }

        private void server_TestingProgressChangeEvent(int curVal, int maxVal, int numServer)
        {
            SafeInvokeTestingProgress(curVal, maxVal, numServer);
        }

        public event Action<int, int, int> TestingProgressChangeEvent;

        private void SafeInvokeTestingProgress(int curVal, int maxVal, int numServer)
        {
            Action<int, int, int> handler = TestingProgressChangeEvent;

            if (handler == null)
            {
                return;
            }

            try
            {
                handler(curVal, maxVal, numServer);
            }
            catch (Exception ex)
            {
                SendLogMessage("Optimizer testing progress event dispatch failed: " + ex, LogMessageType.Error);
            }
        }

        #endregion

        #region Log

        private void SendLogMessage(string message, LogMessageType type)
        {
            Action<string, LogMessageType> handler = LogMessageEvent;

            if (handler == null)
            {
                return;
            }

            try
            {
                handler(message, type);
            }
            catch
            {
                // Avoid recursive logging failures from event subscribers.
            }
        }

        public event Action<string, LogMessageType> LogMessageEvent;

        #endregion
    }
}
