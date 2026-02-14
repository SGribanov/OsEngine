using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Entity;
using OsEngine.OsOptimizer;
using OsEngine.OsOptimizer.OptimizerEntity;
using OsEngine.OsOptimizer.OptEntity;
using OsEngine.OsTrader.Panels.Tab.Internal;
using Xunit;

namespace OsEngine.Tests;

public class OptimizerRefactorTests
{
    private static readonly object SettingsFileLock = new object();

    [Fact]
    public void OptimizerReportSerializer_V2AndLegacyRoundTrip_ShouldPreserveData()
    {
        OptimizerReport source = BuildSampleReport();

        string v2 = source.GetSaveString().ToString();
        Assert.StartsWith("V2|", v2);

        OptimizerReport loadedFromV2 = new OptimizerReport();
        loadedFromV2.LoadFromString(v2);

        Assert.Equal(source.BotName, loadedFromV2.BotName);
        Assert.Equal(source.TotalProfit, loadedFromV2.TotalProfit);
        Assert.Equal(source.StrategyParameters.Count, loadedFromV2.StrategyParameters.Count);
        Assert.Equal(source.TabsReports.Count, loadedFromV2.TabsReports.Count);

        string legacy = v2.Substring("V2|".Length);
        OptimizerReport loadedFromLegacy = new OptimizerReport();
        loadedFromLegacy.LoadFromString(legacy);

        Assert.Equal(source.BotName, loadedFromLegacy.BotName);
        Assert.Equal(source.TotalProfitPercent, loadedFromLegacy.TotalProfitPercent);
        Assert.Equal(source.SharpRatio, loadedFromLegacy.SharpRatio);
    }

    [Fact]
    public void BruteForceStrategy_EstimateBotCount_ShouldMatchGridSize()
    {
        ParameterIterator iterator = new ParameterIterator();
        BruteForceStrategy strategy = new BruteForceStrategy(iterator);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("Length", 2, 1, 3, 1),
            new StrategyParameterDecimal("Threshold", 0.1m, 0.1m, 0.3m, 0.1m),
            new StrategyParameterBool("UseFilter", true)
        };

        List<bool> parametersToOptimization = new List<bool> { true, true, false };

        int estimated = strategy.EstimateBotCount(allParameters, parametersToOptimization);

        // Int: 1..3 => 3 values, Decimal: 0.1..0.3 => 3 values, total 9 combinations.
        Assert.Equal(9, estimated);
    }

    [Fact]
    public void BruteForceStrategy_EstimateBotCount_WithMismatchedFlags_ShouldThrowArgumentException()
    {
        ParameterIterator iterator = new ParameterIterator();
        BruteForceStrategy strategy = new BruteForceStrategy(iterator);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 2, 1),
            new StrategyParameterInt("B", 1, 1, 2, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true };

        Assert.Throws<ArgumentException>(() =>
            strategy.EstimateBotCount(allParameters, parametersToOptimization));
    }

    [Fact]
    public async Task BruteForceStrategy_OptimizeInSampleAsync_ShouldEvaluateAllCombinations()
    {
        ParameterIterator iterator = new ParameterIterator();
        int calls = 0;

        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
        {
            Interlocked.Increment(ref calls);

            OptimizerReport report = new OptimizerReport(new List<IIStrategyParameter>());
            report.BotName = "bot_" + calls;
            report.TotalProfit = optimized
                .OfType<StrategyParameterInt>()
                .Select(x => (decimal)x.ValueInt)
                .DefaultIfEmpty(0m)
                .Sum();

            return Task.FromResult(report);
        });

        BruteForceStrategy strategy = new BruteForceStrategy(iterator, evaluator, maxParallel: 2);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("Fast", 2, 1, 2, 1),
            new StrategyParameterInt("Slow", 4, 3, 4, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        List<OptimizerReport> reports =
            await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None);

        Assert.Equal(4, calls);
        Assert.Equal(4, reports.Count);
        Assert.All(reports, r => Assert.StartsWith("bot_", r.BotName));
    }

    [Fact]
    public async Task BruteForceStrategy_OptimizeInSampleAsync_ShouldRespectMaxParallel()
    {
        ParameterIterator iterator = new ParameterIterator();
        int current = 0;
        int maxObserved = 0;

        IBotEvaluator evaluator = new BotEvaluator(async (all, optimized, ct) =>
        {
            int now = Interlocked.Increment(ref current);
            while (true)
            {
                int snapshot = maxObserved;
                if (now <= snapshot)
                {
                    break;
                }
                if (Interlocked.CompareExchange(ref maxObserved, now, snapshot) == snapshot)
                {
                    break;
                }
            }

            try
            {
                await Task.Delay(30, ct);
                OptimizerReport report = new OptimizerReport(new List<IIStrategyParameter>());
                report.BotName = "parallel";
                return report;
            }
            finally
            {
                Interlocked.Decrement(ref current);
            }
        });

        BruteForceStrategy strategy = new BruteForceStrategy(iterator, evaluator, maxParallel: 2);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 3, 1),
            new StrategyParameterInt("B", 1, 1, 3, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        List<OptimizerReport> reports =
            await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None);

        Assert.Equal(9, reports.Count);
        Assert.True(maxObserved <= 2, $"Observed parallelism {maxObserved} exceeds configured max 2");
    }

    [Fact]
    public async Task BruteForceStrategy_OptimizeInSampleAsync_CanceledBeforeStart_ShouldReturnEmpty()
    {
        ParameterIterator iterator = new ParameterIterator();
        int calls = 0;

        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult(new OptimizerReport(new List<IIStrategyParameter>()));
        });

        BruteForceStrategy strategy = new BruteForceStrategy(iterator, evaluator, maxParallel: 2);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 2, 1),
            new StrategyParameterInt("B", 1, 1, 2, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        CancellationTokenSource cts = new CancellationTokenSource();
        cts.Cancel();

        List<OptimizerReport> reports =
            await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, cts.Token);

        Assert.Empty(reports);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task BruteForceStrategy_OptimizeInSampleAsync_WithoutEvaluator_ShouldThrow()
    {
        ParameterIterator iterator = new ParameterIterator();
        BruteForceStrategy strategy = new BruteForceStrategy(iterator);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 2, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true };

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None));
    }

    [Fact]
    public async Task BruteForceStrategy_OptimizeInSampleAsync_WithMismatchedFlags_ShouldThrowArgumentException()
    {
        ParameterIterator iterator = new ParameterIterator();
        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
            Task.FromResult(new OptimizerReport(new List<IIStrategyParameter>())));
        BruteForceStrategy strategy = new BruteForceStrategy(iterator, evaluator);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 2, 1),
            new StrategyParameterInt("B", 1, 1, 2, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true };

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None));
    }

    [Fact]
    public void OptimizerFazeReport_SortResults_ShouldSortDescendingByMetric()
    {
        List<OptimizerReport> reports = new List<OptimizerReport>
        {
            new OptimizerReport { BotName = "1", TotalProfit = 10, PositionsCount = 2 },
            new OptimizerReport { BotName = "2", TotalProfit = 30, PositionsCount = 1 },
            new OptimizerReport { BotName = "3", TotalProfit = 20, PositionsCount = 3 }
        };

        OptimizerFazeReport.SortResults(reports, SortBotsType.TotalProfit);
        Assert.Equal(new[] { "2", "3", "1" }, reports.Select(r => r.BotName).ToArray());

        OptimizerFazeReport.SortResults(reports, SortBotsType.PositionCount);
        Assert.Equal(new[] { "3", "1", "2" }, reports.Select(r => r.BotName).ToArray());
    }

    [Fact]
    public void OptimizerReportSerializer_DeserializeMalformed_ShouldNotThrowAndKeepObjectUsable()
    {
        OptimizerReport report = new OptimizerReport();
        Exception ex = Record.Exception(() => report.LoadFromString("V2|broken_payload"));
        Assert.Null(ex);

        // Object remains usable after malformed load attempt.
        report.BotName = "ok";
        string save = report.GetSaveString().ToString();
        Assert.StartsWith("V2|", save);
    }

    [Fact]
    public async Task BruteForceStrategy_OptimizeInSampleAsync_EvaluatorMutationsMustNotCorruptEnumeration()
    {
        ParameterIterator iterator = new ParameterIterator();
        ConcurrentBag<string> seen = new ConcurrentBag<string>();

        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
        {
            StrategyParameterInt p1 = (StrategyParameterInt)optimized[0];
            StrategyParameterInt p2 = (StrategyParameterInt)optimized[1];
            string originalPair = p1.ValueInt + "_" + p2.ValueInt;
            seen.Add(originalPair);

            // Mutate received parameter intentionally; strategy should pass snapshots.
            p1.ValueInt = 999;

            OptimizerReport report = new OptimizerReport(new List<IIStrategyParameter>())
            {
                BotName = originalPair
            };
            return Task.FromResult(report);
        });

        BruteForceStrategy strategy = new BruteForceStrategy(iterator, evaluator, maxParallel: 3);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("X", 1, 1, 2, 1),
            new StrategyParameterInt("Y", 1, 1, 2, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        List<OptimizerReport> reports =
            await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None);

        Assert.Equal(4, reports.Count);
        Assert.Equal(new[] { "1_1", "1_2", "2_1", "2_2" }, seen.Distinct().OrderBy(x => x).ToArray());
        Assert.Equal(1, ((StrategyParameterInt)allParameters[0]).ValueInt);
        Assert.Equal(1, ((StrategyParameterInt)allParameters[1]).ValueInt);
    }

    [Fact]
    public void OptimizerReportTab_LoadFromSaveString_LegacyWithoutSharpRatio_ShouldLoad()
    {
        // Legacy format had 11 fields without SharpRatio.
        string legacyTab =
            "BotTabSimple*BTCUSDT*5*100* -2*20*1.2*1.5*1.1*2.0*5.0*".Replace(" ", "");

        OptimizerReportTab tab = new OptimizerReportTab();
        tab.LoadFromSaveString(legacyTab);

        Assert.Equal("BotTabSimple", tab.TabType);
        Assert.Equal("BTCUSDT", tab.SecurityName);
        Assert.Equal(5, tab.PositionsCount);
        Assert.Equal(100m, tab.TotalProfit);
        Assert.Equal(0m, tab.SharpRatio);
    }

    [Fact]
    public async Task BruteForceStrategy_OptimizeInSampleAsync_ShouldPreserveDecimalCheckBoxSnapshot()
    {
        ParameterIterator iterator = new ParameterIterator();
        ConcurrentBag<string> seen = new ConcurrentBag<string>();

        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
        {
            StrategyParameterInt i = (StrategyParameterInt)optimized[0];
            StrategyParameterDecimalCheckBox d = (StrategyParameterDecimalCheckBox)optimized[1];

            string key = i.ValueInt + "_" + d.ValueDecimal + "_" + d.CheckState;
            seen.Add(key);

            // Mutate received objects intentionally.
            i.ValueInt = 999;
            d.ValueDecimal = 999m;
            d.CheckState = System.Windows.Forms.CheckState.Unchecked;

            OptimizerReport report = new OptimizerReport(new List<IIStrategyParameter>())
            {
                BotName = key
            };
            return Task.FromResult(report);
        });

        BruteForceStrategy strategy = new BruteForceStrategy(iterator, evaluator, maxParallel: 2);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("I", 1, 1, 2, 1),
            new StrategyParameterDecimalCheckBox("D", 1m, 1m, 2m, 1m, true)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        List<OptimizerReport> reports =
            await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None);

        Assert.Equal(4, reports.Count);
        Assert.Equal(
            new[] { "1_1_Checked", "1_2_Checked", "2_1_Checked", "2_2_Checked" },
            seen.Distinct().OrderBy(x => x).ToArray());

        Assert.Equal(1, ((StrategyParameterInt)allParameters[0]).ValueInt);
        Assert.Equal(1m, ((StrategyParameterDecimalCheckBox)allParameters[1]).ValueDecimal);
        Assert.Equal(System.Windows.Forms.CheckState.Checked, ((StrategyParameterDecimalCheckBox)allParameters[1]).CheckState);
    }

    [Fact]
    public void OptimizationStrategyFactory_BruteForce_ShouldReturnBruteForceWithoutMessage()
    {
        ParameterIterator iterator = new ParameterIterator();

        IOptimizationStrategy strategy = OptimizationStrategyFactory.CreateInSampleStrategy(
            OptimizationMethodType.BruteForce,
            iterator,
            evaluator: null,
            maxParallel: 1,
            objectiveMetric: SortBotsType.TotalProfit,
            objectiveDirection: ObjectiveDirectionType.Maximize,
            bayesianInitialSamples: 10,
            bayesianMaxIterations: 50,
            bayesianBatchSize: 2,
            bayesianAcquisitionMode: BayesianAcquisitionModeType.Ucb,
            bayesianAcquisitionKappa: 0.25m,
            bayesianUseTailPass: true,
            bayesianTailSharePercent: 20,
            out string infoMessage);

        Assert.IsType<BruteForceStrategy>(strategy);
        Assert.True(string.IsNullOrEmpty(infoMessage));
    }

    [Fact]
    public void OptimizationStrategyFactory_Bayesian_ShouldReturnBayesianSkeletonWithMessage()
    {
        ParameterIterator iterator = new ParameterIterator();

        IOptimizationStrategy strategy = OptimizationStrategyFactory.CreateInSampleStrategy(
            OptimizationMethodType.Bayesian,
            iterator,
            evaluator: null,
            maxParallel: 2,
            objectiveMetric: SortBotsType.SharpRatio,
            objectiveDirection: ObjectiveDirectionType.Maximize,
            bayesianInitialSamples: 12,
            bayesianMaxIterations: 77,
            bayesianBatchSize: 3,
            bayesianAcquisitionMode: BayesianAcquisitionModeType.Ucb,
            bayesianAcquisitionKappa: 0.5m,
            bayesianUseTailPass: true,
            bayesianTailSharePercent: 15,
            out string infoMessage);

        BayesianOptimizationStrategy bayesian = Assert.IsType<BayesianOptimizationStrategy>(strategy);
        Assert.Equal(SortBotsType.SharpRatio, bayesian.ObjectiveMetric);
        Assert.Equal(ObjectiveDirectionType.Maximize, bayesian.ObjectiveDirection);
        Assert.Equal(12, bayesian.InitialSamples);
        Assert.Equal(77, bayesian.MaxIterations);
        Assert.Equal(3, bayesian.BatchSize);
        Assert.Equal(BayesianAcquisitionModeType.Ucb, bayesian.AcquisitionMode);
        Assert.Equal(0.5m, bayesian.AcquisitionKappa);
        Assert.True(bayesian.UseExploitationTailPass);
        Assert.Equal(15, bayesian.TailSharePercent);
        Assert.False(string.IsNullOrEmpty(infoMessage));
        Assert.Contains("skeleton", infoMessage);
        Assert.Contains("AcquisitionMode=Ucb", infoMessage);
        Assert.Contains("TailSharePercent=15", infoMessage);
    }

    [Fact]
    public async Task BayesianOptimizationStrategy_OptimizeInSampleAsync_ShouldUseCurrentBruteForceBackend()
    {
        ParameterIterator iterator = new ParameterIterator();
        int calls = 0;

        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult(new OptimizerReport(new List<IIStrategyParameter>())
            {
                BotName = "bayes"
            });
        });

        BayesianOptimizationStrategy strategy = new BayesianOptimizationStrategy(
            iterator,
            evaluator,
            maxParallel: 2,
            objectiveMetric: SortBotsType.TotalProfit,
            objectiveDirection: ObjectiveDirectionType.Maximize,
            initialSamples: 5,
            maxIterations: 20,
            batchSize: 2,
            acquisitionMode: BayesianAcquisitionModeType.Ucb,
            acquisitionKappa: 0.25m,
            useExploitationTailPass: true,
            tailSharePercent: 20);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 2, 1),
            new StrategyParameterInt("B", 1, 1, 2, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        List<OptimizerReport> reports =
            await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None);

        Assert.Equal(4, calls);
        Assert.Equal(4, reports.Count);
    }

    [Fact]
    public void BayesianOptimizationStrategy_EstimateBotCount_WithMismatchedFlags_ShouldThrowArgumentException()
    {
        ParameterIterator iterator = new ParameterIterator();
        BayesianOptimizationStrategy strategy = new BayesianOptimizationStrategy(
            iterator, botEvaluator: null, maxParallel: 1,
            objectiveMetric: SortBotsType.TotalProfit,
            objectiveDirection: ObjectiveDirectionType.Maximize,
            initialSamples: 1, maxIterations: 5, batchSize: 1,
            acquisitionMode: BayesianAcquisitionModeType.Ucb,
            acquisitionKappa: 0.1m,
            useExploitationTailPass: true,
            tailSharePercent: 20);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 2, 1),
            new StrategyParameterInt("B", 1, 1, 2, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true };

        Assert.Throws<ArgumentException>(() =>
            strategy.EstimateBotCount(allParameters, parametersToOptimization));
    }

    [Fact]
    public void BayesianOptimizationStrategy_EstimateBotCount_ShouldRespectBudgetCap()
    {
        ParameterIterator iterator = new ParameterIterator();
        BayesianOptimizationStrategy strategy = new BayesianOptimizationStrategy(
            iterator, botEvaluator: null, maxParallel: 1,
            objectiveMetric: SortBotsType.TotalProfit,
            objectiveDirection: ObjectiveDirectionType.Maximize,
            initialSamples: 2, maxIterations: 3, batchSize: 2,
            acquisitionMode: BayesianAcquisitionModeType.Ucb,
            acquisitionKappa: 0.25m,
            useExploitationTailPass: true,
            tailSharePercent: 20);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 4, 1),
            new StrategyParameterInt("B", 1, 1, 4, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        int estimated = strategy.EstimateBotCount(allParameters, parametersToOptimization);

        // Grid has 16 combinations, budget is 2 + 3 = 5.
        Assert.Equal(5, estimated);
    }

    [Fact]
    public void BayesianOptimizationStrategy_EstimateBotCount_WhenBudgetExceedsGrid_ShouldReturnGridSize()
    {
        ParameterIterator iterator = new ParameterIterator();
        BayesianOptimizationStrategy strategy = new BayesianOptimizationStrategy(
            iterator, botEvaluator: null, maxParallel: 1,
            objectiveMetric: SortBotsType.TotalProfit,
            objectiveDirection: ObjectiveDirectionType.Maximize,
            initialSamples: 20, maxIterations: 50, batchSize: 2,
            acquisitionMode: BayesianAcquisitionModeType.Ucb,
            acquisitionKappa: 0.25m,
            useExploitationTailPass: true,
            tailSharePercent: 20);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 2, 1),
            new StrategyParameterInt("B", 1, 1, 2, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        int estimated = strategy.EstimateBotCount(allParameters, parametersToOptimization);

        // Grid has 4 combinations, budget is 70.
        Assert.Equal(4, estimated);
    }

    [Fact]
    public async Task BayesianOptimizationStrategy_OptimizeInSampleAsync_WithNullFlags_ShouldThrowArgumentNullException()
    {
        ParameterIterator iterator = new ParameterIterator();
        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
            Task.FromResult(new OptimizerReport(new List<IIStrategyParameter>())));

        BayesianOptimizationStrategy strategy = new BayesianOptimizationStrategy(
            iterator, evaluator, 2, SortBotsType.TotalProfit, ObjectiveDirectionType.Maximize,
            initialSamples: 2, maxIterations: 5, batchSize: 2,
            acquisitionMode: BayesianAcquisitionModeType.Ucb, acquisitionKappa: 0.25m,
            useExploitationTailPass: true, tailSharePercent: 20);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 2, 1)
        };

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await strategy.OptimizeInSampleAsync(allParameters, null, CancellationToken.None));
    }

    [Fact]
    public async Task BayesianOptimizationStrategy_OptimizeInSampleAsync_ShouldRespectIterationBudget()
    {
        ParameterIterator iterator = new ParameterIterator();
        int calls = 0;

        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult(new OptimizerReport(new List<IIStrategyParameter>())
            {
                BotName = "iter"
            });
        });

        BayesianOptimizationStrategy strategy = new BayesianOptimizationStrategy(
            iterator,
            evaluator,
            maxParallel: 2,
            objectiveMetric: SortBotsType.TotalProfit,
            objectiveDirection: ObjectiveDirectionType.Maximize,
            initialSamples: 2,
            maxIterations: 3,
            batchSize: 2,
            acquisitionMode: BayesianAcquisitionModeType.Ucb,
            acquisitionKappa: 0.25m,
            useExploitationTailPass: true,
            tailSharePercent: 20);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 3, 1),
            new StrategyParameterInt("B", 1, 1, 3, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        List<OptimizerReport> reports =
            await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None);

        // 9 grid points total, but staged budget is 2 initial + 3 iterative = 5 evaluations.
        Assert.Equal(5, calls);
        Assert.Equal(5, reports.Count);
    }

    [Fact]
    public async Task BayesianOptimizationStrategy_GreedyMode_ShouldPlanZeroTailBudget()
    {
        ParameterIterator iterator = new ParameterIterator();

        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
            Task.FromResult(new OptimizerReport(new List<IIStrategyParameter>())));

        BayesianOptimizationStrategy strategy = new BayesianOptimizationStrategy(
            iterator,
            evaluator,
            maxParallel: 2,
            objectiveMetric: SortBotsType.TotalProfit,
            objectiveDirection: ObjectiveDirectionType.Maximize,
            initialSamples: 2,
            maxIterations: 12,
            batchSize: 3,
            acquisitionMode: BayesianAcquisitionModeType.Greedy,
            acquisitionKappa: 0.25m,
            useExploitationTailPass: true,
            tailSharePercent: 20);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 4, 1),
            new StrategyParameterInt("B", 1, 1, 4, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None);

        Assert.Equal(0, strategy.LastTailBudgetPlanned);
    }

    [Fact]
    public async Task BayesianOptimizationStrategy_UcbMode_ShouldPlanPositiveTailBudgetWhenEnabled()
    {
        ParameterIterator iterator = new ParameterIterator();

        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
            Task.FromResult(new OptimizerReport(new List<IIStrategyParameter>())));

        BayesianOptimizationStrategy strategy = new BayesianOptimizationStrategy(
            iterator,
            evaluator,
            maxParallel: 2,
            objectiveMetric: SortBotsType.TotalProfit,
            objectiveDirection: ObjectiveDirectionType.Maximize,
            initialSamples: 2,
            maxIterations: 12,
            batchSize: 3,
            acquisitionMode: BayesianAcquisitionModeType.Ucb,
            acquisitionKappa: 0.25m,
            useExploitationTailPass: true,
            tailSharePercent: 20);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 4, 1),
            new StrategyParameterInt("B", 1, 1, 4, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None);

        Assert.True(strategy.LastTailBudgetPlanned > 0);
    }

    [Fact]
    public async Task BayesianOptimizationStrategy_CanceledRun_ShouldResetLastTailBudgetPlanned()
    {
        ParameterIterator iterator = new ParameterIterator();
        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
            Task.FromResult(new OptimizerReport(new List<IIStrategyParameter>())));

        BayesianOptimizationStrategy strategy = new BayesianOptimizationStrategy(
            iterator,
            evaluator,
            maxParallel: 2,
            objectiveMetric: SortBotsType.TotalProfit,
            objectiveDirection: ObjectiveDirectionType.Maximize,
            initialSamples: 2,
            maxIterations: 12,
            batchSize: 3,
            acquisitionMode: BayesianAcquisitionModeType.Ucb,
            acquisitionKappa: 0.25m,
            useExploitationTailPass: true,
            tailSharePercent: 20);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 4, 1),
            new StrategyParameterInt("B", 1, 1, 4, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None);
        Assert.True(strategy.LastTailBudgetPlanned > 0);

        using CancellationTokenSource cts = new CancellationTokenSource();
        cts.Cancel();
        await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, cts.Token);

        Assert.Equal(0, strategy.LastTailBudgetPlanned);
    }

    [Fact]
    public async Task BayesianOptimizationStrategy_TailSharePercent_ShouldAffectPlannedTailBudget()
    {
        ParameterIterator iterator = new ParameterIterator();
        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
            Task.FromResult(new OptimizerReport(new List<IIStrategyParameter>())));

        BayesianOptimizationStrategy lowShare = new BayesianOptimizationStrategy(
            iterator, evaluator, 2, SortBotsType.TotalProfit, ObjectiveDirectionType.Maximize,
            initialSamples: 2, maxIterations: 20, batchSize: 10,
            acquisitionMode: BayesianAcquisitionModeType.Ucb, acquisitionKappa: 0.25m,
            useExploitationTailPass: true, tailSharePercent: 5);

        BayesianOptimizationStrategy highShare = new BayesianOptimizationStrategy(
            iterator, evaluator, 2, SortBotsType.TotalProfit, ObjectiveDirectionType.Maximize,
            initialSamples: 2, maxIterations: 20, batchSize: 10,
            acquisitionMode: BayesianAcquisitionModeType.Ucb, acquisitionKappa: 0.25m,
            useExploitationTailPass: true, tailSharePercent: 40);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 4, 1),
            new StrategyParameterInt("B", 1, 1, 4, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        await lowShare.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None);
        await highShare.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None);

        Assert.True(highShare.LastTailBudgetPlanned > lowShare.LastTailBudgetPlanned);
    }

    [Fact]
    public async Task BayesianOptimizationStrategy_TailPassDisabled_ShouldPlanZeroTailBudget()
    {
        ParameterIterator iterator = new ParameterIterator();
        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
            Task.FromResult(new OptimizerReport(new List<IIStrategyParameter>())));

        BayesianOptimizationStrategy strategy = new BayesianOptimizationStrategy(
            iterator, evaluator, 2, SortBotsType.TotalProfit, ObjectiveDirectionType.Maximize,
            initialSamples: 2, maxIterations: 20, batchSize: 10,
            acquisitionMode: BayesianAcquisitionModeType.Ucb, acquisitionKappa: 0.25m,
            useExploitationTailPass: false, tailSharePercent: 40);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 4, 1),
            new StrategyParameterInt("B", 1, 1, 4, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None);

        Assert.Equal(0, strategy.LastTailBudgetPlanned);
    }

    [Fact]
    public void BayesianOptimizationStrategy_TailSharePercent_ShouldClampToRange()
    {
        ParameterIterator iterator = new ParameterIterator();

        BayesianOptimizationStrategy low = new BayesianOptimizationStrategy(
            iterator, botEvaluator: null, maxParallel: 1,
            objectiveMetric: SortBotsType.TotalProfit,
            objectiveDirection: ObjectiveDirectionType.Maximize,
            initialSamples: 1, maxIterations: 5, batchSize: 1,
            acquisitionMode: BayesianAcquisitionModeType.Ucb,
            acquisitionKappa: 0.1m,
            useExploitationTailPass: true,
            tailSharePercent: 0);

        BayesianOptimizationStrategy high = new BayesianOptimizationStrategy(
            iterator, botEvaluator: null, maxParallel: 1,
            objectiveMetric: SortBotsType.TotalProfit,
            objectiveDirection: ObjectiveDirectionType.Maximize,
            initialSamples: 1, maxIterations: 5, batchSize: 1,
            acquisitionMode: BayesianAcquisitionModeType.Ucb,
            acquisitionKappa: 0.1m,
            useExploitationTailPass: true,
            tailSharePercent: 999);

        Assert.Equal(1, low.TailSharePercent);
        Assert.Equal(50, high.TailSharePercent);
    }

    [Fact]
    public async Task BayesianOptimizationStrategy_WithTailPass_ShouldRespectTotalEvaluationBudget()
    {
        ParameterIterator iterator = new ParameterIterator();
        int calls = 0;
        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
        {
            Interlocked.Increment(ref calls);
            return Task.FromResult(new OptimizerReport(new List<IIStrategyParameter>()));
        });

        BayesianOptimizationStrategy strategy = new BayesianOptimizationStrategy(
            iterator, evaluator, 2, SortBotsType.TotalProfit, ObjectiveDirectionType.Maximize,
            initialSamples: 2, maxIterations: 10, batchSize: 3,
            acquisitionMode: BayesianAcquisitionModeType.Ucb, acquisitionKappa: 0.25m,
            useExploitationTailPass: true, tailSharePercent: 20);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 4, 1),
            new StrategyParameterInt("B", 1, 1, 4, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None);

        // Total planned evaluations should remain InitialSamples + MaxIterations.
        Assert.Equal(12, calls);
    }

    [Fact]
    public async Task BayesianOptimizationStrategy_ShouldNotEvaluateDuplicateCandidates()
    {
        ParameterIterator iterator = new ParameterIterator();
        ConcurrentBag<string> seen = new ConcurrentBag<string>();

        IBotEvaluator evaluator = new BotEvaluator((all, optimized, ct) =>
        {
            StrategyParameterInt a = (StrategyParameterInt)optimized[0];
            StrategyParameterInt b = (StrategyParameterInt)optimized[1];
            seen.Add(a.ValueInt + "_" + b.ValueInt);
            return Task.FromResult(new OptimizerReport(new List<IIStrategyParameter>()));
        });

        BayesianOptimizationStrategy strategy = new BayesianOptimizationStrategy(
            iterator, evaluator, 3, SortBotsType.TotalProfit, ObjectiveDirectionType.Maximize,
            initialSamples: 2, maxIterations: 10, batchSize: 3,
            acquisitionMode: BayesianAcquisitionModeType.Ucb, acquisitionKappa: 0.5m,
            useExploitationTailPass: true, tailSharePercent: 20);

        List<IIStrategyParameter> allParameters = new List<IIStrategyParameter>
        {
            new StrategyParameterInt("A", 1, 1, 4, 1),
            new StrategyParameterInt("B", 1, 1, 4, 1)
        };
        List<bool> parametersToOptimization = new List<bool> { true, true };

        await strategy.OptimizeInSampleAsync(allParameters, parametersToOptimization, CancellationToken.None);

        string[] distinct = seen.Distinct().ToArray();
        Assert.Equal(seen.Count, distinct.Length);
    }

    [Fact]
    public void BayesianCandidateSelector_SelectInitialBatch_ShouldSpreadAndFill()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 3);
        HashSet<int> evaluated = new HashSet<int> { 0 };

        List<int> batch = selector.SelectInitialBatch(totalCount: 10, evaluated, take: 4);

        Assert.Equal(4, batch.Count);
        Assert.DoesNotContain(0, batch);
        Assert.Equal(4, batch.Distinct().Count());
    }

    [Fact]
    public void BayesianCandidateSelector_SelectInitialBatch_WithNullEvaluated_ShouldTreatAsEmpty()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 3);

        List<int> batch = selector.SelectInitialBatch(totalCount: 5, evaluated: null, take: 3);

        Assert.Equal(3, batch.Count);
        Assert.Equal(3, batch.Distinct().Count());
    }

    [Fact]
    public void BayesianCandidateSelector_SelectNextBatch_ShouldPreferNeighborsOfTopScores()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 3);
        HashSet<int> evaluated = new HashSet<int> { 4 };
        List<BayesianCandidateSelector.CandidateScore> scored = new List<BayesianCandidateSelector.CandidateScore>
        {
            new BayesianCandidateSelector.CandidateScore { Index = 4, Score = 100m }
        };

        List<int> batch = selector.SelectNextBatch(totalCount: 10, evaluated, scored, batchSize: 3);

        Assert.Equal(3, batch.Count);
        Assert.Contains(3, batch);
        Assert.Contains(5, batch);
        Assert.DoesNotContain(4, batch);
    }

    [Fact]
    public void BayesianCandidateSelector_SelectNextBatch_WithNullScored_ShouldFallbackToSequentialFill()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 3);
        HashSet<int> evaluated = new HashSet<int> { 0, 1 };

        List<int> batch = selector.SelectNextBatch(totalCount: 6, evaluated, scored: null, batchSize: 3);

        Assert.Equal(new[] { 2, 3, 4 }, batch);
    }

    [Fact]
    public void BayesianCandidateSelector_SelectNextBatch_WithNullEvaluated_ShouldTreatAsEmpty()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 3);
        List<BayesianCandidateSelector.CandidateScore> scored = new List<BayesianCandidateSelector.CandidateScore>
        {
            new BayesianCandidateSelector.CandidateScore { Index = 2, Score = 10m }
        };

        List<int> batch = selector.SelectNextBatch(totalCount: 6, evaluated: null, scored, batchSize: 2);

        Assert.Equal(2, batch.Count);
        Assert.Contains(1, batch);
        Assert.Contains(3, batch);
    }

    [Fact]
    public void BayesianAcquisitionPolicy_SelectNextBatch_WithoutScores_ShouldFallbackToSelector()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 3);
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy();
        HashSet<int> evaluated = new HashSet<int> { 0 };

        List<int> batch = policy.SelectNextBatch(
            totalCount: 10,
            evaluated,
            scored: new List<BayesianCandidateSelector.CandidateScore>(),
            batchSize: 3,
            fallbackSelector: selector,
            candidates: BuildIntCandidates(10, "X", 1, 10, 1),
            mode: BayesianAcquisitionModeType.Ucb,
            kappa: 0.25m);

        Assert.Equal(3, batch.Count);
        Assert.DoesNotContain(0, batch);
    }

    [Fact]
    public void BayesianAcquisitionPolicy_SelectNextBatch_WithEqualMeans_ShouldPreferHigherUncertainty()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 2);
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy(kappa: 1m);
        HashSet<int> evaluated = new HashSet<int> { 1, 2 };
        List<BayesianCandidateSelector.CandidateScore> scored = new List<BayesianCandidateSelector.CandidateScore>
        {
            new BayesianCandidateSelector.CandidateScore { Index = 1, Score = 10m },
            new BayesianCandidateSelector.CandidateScore { Index = 2, Score = 10m }
        };

        List<int> batch = policy.SelectNextBatch(
            totalCount: 8,
            evaluated,
            scored,
            batchSize: 1,
            fallbackSelector: selector,
            candidates: BuildIntCandidates(8, "X", 1, 8, 1),
            mode: BayesianAcquisitionModeType.Ucb,
            kappa: 1m);

        // With equal means, farthest candidate in parameter-space gets max uncertainty.
        Assert.Single(batch);
        Assert.Equal(7, batch[0]);
    }

    [Fact]
    public void BayesianAcquisitionPolicy_Modes_ShouldChangeExplorationBehavior()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 1);
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy();
        HashSet<int> evaluated = new HashSet<int> { 0, 2 };
        List<BayesianCandidateSelector.CandidateScore> scored = new List<BayesianCandidateSelector.CandidateScore>
        {
            new BayesianCandidateSelector.CandidateScore { Index = 0, Score = 10m },
            new BayesianCandidateSelector.CandidateScore { Index = 2, Score = 0m }
        };
        List<List<IIStrategyParameter>> candidates = BuildIntCandidates(10, "X", 1, 10, 1);

        List<int> greedy = policy.SelectNextBatch(
            totalCount: 10,
            evaluated,
            scored,
            batchSize: 1,
            fallbackSelector: selector,
            candidates: candidates,
            mode: BayesianAcquisitionModeType.Greedy,
            kappa: 0.25m);

        List<int> ucb = policy.SelectNextBatch(
            totalCount: 10,
            evaluated,
            scored,
            batchSize: 1,
            fallbackSelector: selector,
            candidates: candidates,
            mode: BayesianAcquisitionModeType.Ucb,
            kappa: 50m);

        Assert.Single(greedy);
        Assert.Single(ucb);
        Assert.Equal(1, greedy[0]);
        Assert.Equal(9, ucb[0]);
    }

    [Fact]
    public void BayesianAcquisitionPolicy_ExpectedImprovement_ShouldUseOptimisticMean()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 1);
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy();
        HashSet<int> evaluated = new HashSet<int> { 0, 2 };
        List<BayesianCandidateSelector.CandidateScore> scored = new List<BayesianCandidateSelector.CandidateScore>
        {
            new BayesianCandidateSelector.CandidateScore { Index = 0, Score = 1m },
            new BayesianCandidateSelector.CandidateScore { Index = 2, Score = 0m }
        };
        List<List<IIStrategyParameter>> candidates = BuildIntCandidates(10, "X", 1, 10, 1);

        List<int> ei = policy.SelectNextBatch(
            totalCount: 10,
            evaluated,
            scored,
            batchSize: 1,
            fallbackSelector: selector,
            candidates: candidates,
            mode: BayesianAcquisitionModeType.ExpectedImprovement,
            kappa: 10m);

        Assert.Single(ei);
        Assert.Equal(9, ei[0]);
    }

    [Fact]
    public void BayesianAcquisitionPolicy_NegativeKappa_ShouldBeTreatedAsZero()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 1);
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy();
        HashSet<int> evaluated = new HashSet<int> { 0, 2 };
        List<BayesianCandidateSelector.CandidateScore> scored = new List<BayesianCandidateSelector.CandidateScore>
        {
            new BayesianCandidateSelector.CandidateScore { Index = 0, Score = 10m },
            new BayesianCandidateSelector.CandidateScore { Index = 2, Score = 0m }
        };
        List<List<IIStrategyParameter>> candidates = BuildIntCandidates(10, "X", 1, 10, 1);

        List<int> ucbNegative = policy.SelectNextBatch(
            totalCount: 10,
            evaluated,
            scored,
            batchSize: 1,
            fallbackSelector: selector,
            candidates: candidates,
            mode: BayesianAcquisitionModeType.Ucb,
            kappa: -100m);

        List<int> greedy = policy.SelectNextBatch(
            totalCount: 10,
            evaluated,
            scored,
            batchSize: 1,
            fallbackSelector: selector,
            candidates: candidates,
            mode: BayesianAcquisitionModeType.Greedy,
            kappa: 0m);

        Assert.Single(ucbNegative);
        Assert.Single(greedy);
        Assert.Equal(greedy[0], ucbNegative[0]);
    }

    [Fact]
    public void BayesianAcquisitionPolicy_InvalidCandidates_ShouldFallbackToSelectorPath()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 2);
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy();
        HashSet<int> evaluated = new HashSet<int> { 0 };
        List<BayesianCandidateSelector.CandidateScore> scored = new List<BayesianCandidateSelector.CandidateScore>
        {
            new BayesianCandidateSelector.CandidateScore { Index = 0, Score = 1m }
        };

        List<int> fromNull = policy.SelectNextBatch(
            totalCount: 6,
            evaluated,
            scored,
            batchSize: 2,
            fallbackSelector: selector,
            candidates: null,
            mode: BayesianAcquisitionModeType.Ucb,
            kappa: 0.5m);

        List<int> fromWrongSize = policy.SelectNextBatch(
            totalCount: 6,
            evaluated,
            scored,
            batchSize: 2,
            fallbackSelector: selector,
            candidates: BuildIntCandidates(5, "X", 1, 5, 1),
            mode: BayesianAcquisitionModeType.Ucb,
            kappa: 0.5m);

        Assert.Equal(2, fromNull.Count);
        Assert.Equal(2, fromWrongSize.Count);
        Assert.Equal(fromNull, fromWrongSize);
        Assert.DoesNotContain(0, fromNull);
    }

    [Fact]
    public void BayesianAcquisitionPolicy_NullEvaluated_ShouldTreatAsEmptySet()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 2);
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy();
        List<BayesianCandidateSelector.CandidateScore> scored = new List<BayesianCandidateSelector.CandidateScore>
        {
            new BayesianCandidateSelector.CandidateScore { Index = 1, Score = 1m }
        };

        List<int> batch = policy.SelectNextBatch(
            totalCount: 6,
            evaluated: null,
            scored: scored,
            batchSize: 2,
            fallbackSelector: selector,
            candidates: BuildIntCandidates(6, "X", 1, 6, 1),
            mode: BayesianAcquisitionModeType.Ucb,
            kappa: 0.5m);

        Assert.Equal(2, batch.Count);
    }

    [Fact]
    public void BayesianAcquisitionPolicy_NullFallback_WhenFallbackPathNeeded_ShouldThrowArgumentNullException()
    {
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy();

        Assert.Throws<ArgumentNullException>(() =>
            policy.SelectNextBatch(
                totalCount: 5,
                evaluated: null,
                scored: null,
                batchSize: 2,
                fallbackSelector: null,
                candidates: null,
                mode: BayesianAcquisitionModeType.Ucb,
                kappa: 0.25m));
    }

    [Fact]
    public void BayesianAcquisitionPolicy_NonPositiveBudgetOrUniverse_ShouldReturnEmpty()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 2);
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy();
        List<BayesianCandidateSelector.CandidateScore> scored = new List<BayesianCandidateSelector.CandidateScore>
        {
            new BayesianCandidateSelector.CandidateScore { Index = 0, Score = 1m }
        };

        List<int> emptyByBatch = policy.SelectNextBatch(
            totalCount: 10,
            evaluated: new HashSet<int>(),
            scored: scored,
            batchSize: 0,
            fallbackSelector: selector,
            candidates: BuildIntCandidates(10, "X", 1, 10, 1),
            mode: BayesianAcquisitionModeType.Ucb,
            kappa: 0.25m);

        List<int> emptyByUniverse = policy.SelectNextBatch(
            totalCount: 0,
            evaluated: new HashSet<int>(),
            scored: scored,
            batchSize: 2,
            fallbackSelector: selector,
            candidates: new List<List<IIStrategyParameter>>(),
            mode: BayesianAcquisitionModeType.Ucb,
            kappa: 0.25m);

        Assert.Empty(emptyByBatch);
        Assert.Empty(emptyByUniverse);
    }

    [Fact]
    public void BayesianAcquisitionPolicy_AllCandidatesEvaluated_ShouldReturnEmpty()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 2);
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy();

        List<int> result = policy.SelectNextBatch(
            totalCount: 4,
            evaluated: new HashSet<int> { 0, 1, 2, 3 },
            scored: new List<BayesianCandidateSelector.CandidateScore>
            {
                new BayesianCandidateSelector.CandidateScore { Index = 1, Score = 0.5m }
            },
            batchSize: 2,
            fallbackSelector: selector,
            candidates: BuildIntCandidates(4, "X", 1, 4, 1),
            mode: BayesianAcquisitionModeType.Ucb,
            kappa: 0.25m);

        Assert.Empty(result);
    }

    [Fact]
    public void BayesianAcquisitionPolicy_InvalidScoredIndices_ShouldFallbackToSelectorPath()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 2);
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy();
        HashSet<int> evaluated = new HashSet<int> { 0 };
        List<BayesianCandidateSelector.CandidateScore> invalidScored = new List<BayesianCandidateSelector.CandidateScore>
        {
            new BayesianCandidateSelector.CandidateScore { Index = -1, Score = 5m },
            new BayesianCandidateSelector.CandidateScore { Index = 999, Score = 6m }
        };

        List<int> fromInvalid = policy.SelectNextBatch(
            totalCount: 6,
            evaluated,
            scored: invalidScored,
            batchSize: 2,
            fallbackSelector: selector,
            candidates: BuildIntCandidates(6, "X", 1, 6, 1),
            mode: BayesianAcquisitionModeType.Ucb,
            kappa: 0.25m);

        List<int> fromFallback = selector.SelectNextBatch(
            totalCount: 6,
            evaluated,
            scored: invalidScored,
            batchSize: 2);

        Assert.Equal(fromFallback, fromInvalid);
    }

    [Fact]
    public void BayesianAcquisitionPolicy_MixedScoredIndices_ShouldIgnoreInvalidAndUseValid()
    {
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 2);
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy();
        HashSet<int> evaluated = new HashSet<int> { 4 };
        List<BayesianCandidateSelector.CandidateScore> validOnly = new List<BayesianCandidateSelector.CandidateScore>
        {
            new BayesianCandidateSelector.CandidateScore { Index = 4, Score = 100m }
        };
        List<BayesianCandidateSelector.CandidateScore> mixed = new List<BayesianCandidateSelector.CandidateScore>
        {
            new BayesianCandidateSelector.CandidateScore { Index = 4, Score = 100m }, // valid
            new BayesianCandidateSelector.CandidateScore { Index = 999, Score = 1000m } // invalid
        };

        List<int> batchFromValid = policy.SelectNextBatch(
            totalCount: 10,
            evaluated,
            scored: validOnly,
            batchSize: 2,
            fallbackSelector: selector,
            candidates: BuildIntCandidates(10, "X", 1, 10, 1),
            mode: BayesianAcquisitionModeType.Greedy,
            kappa: 0m);

        List<int> batchFromMixed = policy.SelectNextBatch(
            totalCount: 10,
            evaluated,
            scored: mixed,
            batchSize: 2,
            fallbackSelector: selector,
            candidates: BuildIntCandidates(10, "X", 1, 10, 1),
            mode: BayesianAcquisitionModeType.Greedy,
            kappa: 0m);

        Assert.Equal(batchFromValid, batchFromMixed);
    }

    [Fact]
    public void BayesianAcquisitionPolicy_NullFallback_OnDirectAcquisitionPath_ShouldNotThrow()
    {
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy();
        HashSet<int> evaluated = new HashSet<int> { 4 };
        List<BayesianCandidateSelector.CandidateScore> scored = new List<BayesianCandidateSelector.CandidateScore>
        {
            new BayesianCandidateSelector.CandidateScore { Index = 4, Score = 100m }
        };

        List<int> batch = policy.SelectNextBatch(
            totalCount: 10,
            evaluated,
            scored,
            batchSize: 2,
            fallbackSelector: null,
            candidates: BuildIntCandidates(10, "X", 1, 10, 1),
            mode: BayesianAcquisitionModeType.Greedy,
            kappa: 0m);

        Assert.Equal(2, batch.Count);
    }

    [Fact]
    public void BayesianAcquisitionPolicy_ScoredContainsNullEntries_ShouldIgnoreNulls()
    {
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy();
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 2);
        HashSet<int> evaluated = new HashSet<int> { 4 };
        List<BayesianCandidateSelector.CandidateScore> scored = new List<BayesianCandidateSelector.CandidateScore>
        {
            null!,
            new BayesianCandidateSelector.CandidateScore { Index = 4, Score = 100m }
        };

        List<int> batch = policy.SelectNextBatch(
            totalCount: 10,
            evaluated,
            scored,
            batchSize: 2,
            fallbackSelector: selector,
            candidates: BuildIntCandidates(10, "X", 1, 10, 1),
            mode: BayesianAcquisitionModeType.Greedy,
            kappa: 0m);

        Assert.Equal(2, batch.Count);
    }

    [Fact]
    public void BayesianAcquisitionPolicy_DuplicateScoredIndex_ShouldUseMaxScorePerIndex()
    {
        BayesianAcquisitionPolicy policy = new BayesianAcquisitionPolicy();
        BayesianCandidateSelector selector = new BayesianCandidateSelector(defaultBatchSize: 1);
        HashSet<int> evaluated = new HashSet<int> { 0, 9 };
        List<BayesianCandidateSelector.CandidateScore> scored = new List<BayesianCandidateSelector.CandidateScore>
        {
            new BayesianCandidateSelector.CandidateScore { Index = 0, Score = 0m },
            new BayesianCandidateSelector.CandidateScore { Index = 0, Score = 1m },
            new BayesianCandidateSelector.CandidateScore { Index = 9, Score = 0.5m }
        };

        List<int> batch = policy.SelectNextBatch(
            totalCount: 10,
            evaluated,
            scored,
            batchSize: 1,
            fallbackSelector: selector,
            candidates: BuildIntCandidates(10, "X", 1, 10, 1),
            mode: BayesianAcquisitionModeType.Greedy,
            kappa: 0m);

        Assert.Single(batch);
        Assert.Equal(4, batch[0]);
    }

    [Fact]
    public void OptimizerSettings_SaveLoad_ShouldPersistOptimizationMethodFields()
    {
        lock (SettingsFileLock)
        {
            using SettingsFileScope _ = new SettingsFileScope();

            OptimizerSettings writer = new OptimizerSettings
            {
                OptimizationMethod = OptimizationMethodType.Bayesian,
                ObjectiveMetric = SortBotsType.SharpRatio,
                BayesianInitialSamples = 33,
                BayesianMaxIterations = 77,
                BayesianBatchSize = 4,
                ObjectiveDirection = ObjectiveDirectionType.Minimize,
                BayesianAcquisitionMode = BayesianAcquisitionModeType.ExpectedImprovement,
                BayesianAcquisitionKappa = 0.9m,
                BayesianUseTailPass = false,
                BayesianTailSharePercent = 33
            };

            OptimizerSettings reader = new OptimizerSettings();

            Assert.Equal(OptimizationMethodType.Bayesian, reader.OptimizationMethod);
            Assert.Equal(SortBotsType.SharpRatio, reader.ObjectiveMetric);
            Assert.Equal(33, reader.BayesianInitialSamples);
            Assert.Equal(77, reader.BayesianMaxIterations);
            Assert.Equal(4, reader.BayesianBatchSize);
            Assert.Equal(ObjectiveDirectionType.Minimize, reader.ObjectiveDirection);
            Assert.Equal(BayesianAcquisitionModeType.ExpectedImprovement, reader.BayesianAcquisitionMode);
            Assert.Equal(0.9m, reader.BayesianAcquisitionKappa);
            Assert.False(reader.BayesianUseTailPass);
            Assert.Equal(33, reader.BayesianTailSharePercent);
        }
    }

    [Fact]
    public void OptimizerSettings_LoadLegacyWithoutV2Fields_ShouldKeepDefaultsForMethodSettings()
    {
        lock (SettingsFileLock)
        {
            using SettingsFileScope scope = new SettingsFileScope();

            // Create a full modern settings file first.
            _ = new OptimizerSettings
            {
                OptimizationMethod = OptimizationMethodType.Bayesian,
                ObjectiveMetric = SortBotsType.Recovery,
                BayesianInitialSamples = 99,
                BayesianMaxIterations = 199,
                BayesianBatchSize = 7,
                ObjectiveDirection = ObjectiveDirectionType.Minimize,
                BayesianAcquisitionMode = BayesianAcquisitionModeType.Greedy,
                BayesianAcquisitionKappa = 0.77m,
                BayesianUseTailPass = false,
                BayesianTailSharePercent = 7
            };

            string[] fullLines = File.ReadAllLines(scope.SettingsPath);
            Assert.True(fullLines.Length >= 5);

            // Simulate legacy file by removing appended method-setting lines from the tail.
            string[] legacyLines = fullLines.Take(fullLines.Length - 10).ToArray();
            File.WriteAllLines(scope.SettingsPath, legacyLines);

            OptimizerSettings reader = new OptimizerSettings();

            Assert.Equal(OptimizationMethodType.BruteForce, reader.OptimizationMethod);
            Assert.Equal(SortBotsType.TotalProfit, reader.ObjectiveMetric);
            Assert.Equal(20, reader.BayesianInitialSamples);
            Assert.Equal(100, reader.BayesianMaxIterations);
            Assert.Equal(5, reader.BayesianBatchSize);
            Assert.Equal(ObjectiveDirectionType.Maximize, reader.ObjectiveDirection);
            Assert.Equal(BayesianAcquisitionModeType.Ucb, reader.BayesianAcquisitionMode);
            Assert.Equal(0.25m, reader.BayesianAcquisitionKappa);
            Assert.True(reader.BayesianUseTailPass);
            Assert.Equal(20, reader.BayesianTailSharePercent);
        }
    }

    [Fact]
    public void OptimizerSettings_MethodFields_ShouldClampInvalidValues()
    {
        lock (SettingsFileLock)
        {
            using SettingsFileScope _ = new SettingsFileScope();

            OptimizerSettings settings = new OptimizerSettings
            {
                BayesianInitialSamples = 0,
                BayesianMaxIterations = -5,
                BayesianBatchSize = 0,
                BayesianAcquisitionKappa = -10m,
                BayesianTailSharePercent = 0
            };

            Assert.Equal(1, settings.BayesianInitialSamples);
            Assert.Equal(1, settings.BayesianMaxIterations);
            Assert.Equal(1, settings.BayesianBatchSize);
            Assert.Equal(0m, settings.BayesianAcquisitionKappa);
            Assert.Equal(1, settings.BayesianTailSharePercent);

            settings.BayesianTailSharePercent = 999;
            Assert.Equal(50, settings.BayesianTailSharePercent);
        }
    }

    [Fact]
    public void OptimizerSettings_LoadFromFile_WithInvalidBayesianValues_ShouldClampOnLoad()
    {
        lock (SettingsFileLock)
        {
            using SettingsFileScope scope = new SettingsFileScope();

            // Create baseline settings file with all modern fields.
            _ = new OptimizerSettings
            {
                OptimizationMethod = OptimizationMethodType.Bayesian,
                ObjectiveMetric = SortBotsType.TotalProfit,
                BayesianInitialSamples = 10,
                BayesianMaxIterations = 20,
                BayesianBatchSize = 3,
                ObjectiveDirection = ObjectiveDirectionType.Maximize,
                BayesianAcquisitionMode = BayesianAcquisitionModeType.Ucb,
                BayesianAcquisitionKappa = 0.5m,
                BayesianUseTailPass = true,
                BayesianTailSharePercent = 20
            };

            string[] lines = File.ReadAllLines(scope.SettingsPath);
            Assert.True(lines.Length >= 10);

            // Patch last settings block with invalid numeric values.
            lines[^8] = "0";     // BayesianInitialSamples
            lines[^7] = "-10";   // BayesianMaxIterations
            lines[^6] = "0";     // BayesianBatchSize
            lines[^3] = "-7.5";  // BayesianAcquisitionKappa
            lines[^1] = "999";   // BayesianTailSharePercent
            File.WriteAllLines(scope.SettingsPath, lines);

            OptimizerSettings reader = new OptimizerSettings();

            Assert.Equal(1, reader.BayesianInitialSamples);
            Assert.Equal(1, reader.BayesianMaxIterations);
            Assert.Equal(1, reader.BayesianBatchSize);
            Assert.Equal(0m, reader.BayesianAcquisitionKappa);
            Assert.Equal(50, reader.BayesianTailSharePercent);
        }
    }

    [Fact]
    public void OptimizerSettings_LoadFromFile_WithInvalidEnumNumbers_ShouldKeepDefaults()
    {
        lock (SettingsFileLock)
        {
            using SettingsFileScope scope = new SettingsFileScope();

            _ = new OptimizerSettings
            {
                OptimizationMethod = OptimizationMethodType.Bayesian,
                ObjectiveMetric = SortBotsType.SharpRatio,
                BayesianInitialSamples = 10,
                BayesianMaxIterations = 20,
                BayesianBatchSize = 3,
                ObjectiveDirection = ObjectiveDirectionType.Minimize,
                BayesianAcquisitionMode = BayesianAcquisitionModeType.ExpectedImprovement,
                BayesianAcquisitionKappa = 0.5m,
                BayesianUseTailPass = true,
                BayesianTailSharePercent = 20
            };

            string[] lines = File.ReadAllLines(scope.SettingsPath);
            Assert.True(lines.Length >= 10);

            lines[^10] = "999"; // OptimizationMethod
            lines[^9] = "999";  // ObjectiveMetric
            lines[^5] = "999";  // ObjectiveDirection
            lines[^4] = "999";  // BayesianAcquisitionMode
            File.WriteAllLines(scope.SettingsPath, lines);

            OptimizerSettings reader = new OptimizerSettings();

            Assert.Equal(OptimizationMethodType.BruteForce, reader.OptimizationMethod);
            Assert.Equal(SortBotsType.TotalProfit, reader.ObjectiveMetric);
            Assert.Equal(ObjectiveDirectionType.Maximize, reader.ObjectiveDirection);
            Assert.Equal(BayesianAcquisitionModeType.Ucb, reader.BayesianAcquisitionMode);
        }
    }

    [Fact]
    public void OptimizerSettings_LoadFromFile_WithPartiallyInvalidV2Numbers_ShouldLoadRemainingFields()
    {
        lock (SettingsFileLock)
        {
            using SettingsFileScope scope = new SettingsFileScope();

            _ = new OptimizerSettings
            {
                OptimizationMethod = OptimizationMethodType.Bayesian,
                ObjectiveMetric = SortBotsType.TotalProfit,
                BayesianInitialSamples = 10,
                BayesianMaxIterations = 20,
                BayesianBatchSize = 3,
                ObjectiveDirection = ObjectiveDirectionType.Maximize,
                BayesianAcquisitionMode = BayesianAcquisitionModeType.Ucb,
                BayesianAcquisitionKappa = 0.5m,
                BayesianUseTailPass = true,
                BayesianTailSharePercent = 20
            };

            string[] lines = File.ReadAllLines(scope.SettingsPath);
            Assert.True(lines.Length >= 10);

            lines[^8] = "broken-int"; // BayesianInitialSamples
            lines[^7] = "33";         // BayesianMaxIterations
            lines[^6] = "4";          // BayesianBatchSize
            lines[^5] = "Minimize";   // ObjectiveDirection
            lines[^4] = "Greedy";     // BayesianAcquisitionMode
            lines[^3] = "0.9";        // BayesianAcquisitionKappa
            lines[^2] = "False";      // BayesianUseTailPass
            lines[^1] = "17";         // BayesianTailSharePercent
            File.WriteAllLines(scope.SettingsPath, lines);

            OptimizerSettings reader = new OptimizerSettings();

            Assert.Equal(20, reader.BayesianInitialSamples);
            Assert.Equal(33, reader.BayesianMaxIterations);
            Assert.Equal(4, reader.BayesianBatchSize);
            Assert.Equal(ObjectiveDirectionType.Minimize, reader.ObjectiveDirection);
            Assert.Equal(BayesianAcquisitionModeType.Greedy, reader.BayesianAcquisitionMode);
            Assert.Equal(0.9m, reader.BayesianAcquisitionKappa);
            Assert.False(reader.BayesianUseTailPass);
            Assert.Equal(17, reader.BayesianTailSharePercent);
        }
    }

    [Fact]
    public void OptimizerSettings_LoadFromFile_WithCommaDecimalKappa_ShouldParseCorrectly()
    {
        lock (SettingsFileLock)
        {
            using SettingsFileScope scope = new SettingsFileScope();

            _ = new OptimizerSettings
            {
                OptimizationMethod = OptimizationMethodType.Bayesian,
                ObjectiveMetric = SortBotsType.TotalProfit,
                BayesianInitialSamples = 10,
                BayesianMaxIterations = 20,
                BayesianBatchSize = 3,
                ObjectiveDirection = ObjectiveDirectionType.Maximize,
                BayesianAcquisitionMode = BayesianAcquisitionModeType.Ucb,
                BayesianAcquisitionKappa = 0.5m,
                BayesianUseTailPass = true,
                BayesianTailSharePercent = 20
            };

            string[] lines = File.ReadAllLines(scope.SettingsPath);
            Assert.True(lines.Length >= 10);

            lines[^3] = "0,9"; // BayesianAcquisitionKappa
            File.WriteAllLines(scope.SettingsPath, lines);

            OptimizerSettings reader = new OptimizerSettings();

            Assert.Equal(0.9m, reader.BayesianAcquisitionKappa);
        }
    }

    [Fact]
    public void OptimizerSettings_LoadFromFile_WithInvalidBoolTailPass_ShouldKeepValueAndLoadFollowingFields()
    {
        lock (SettingsFileLock)
        {
            using SettingsFileScope scope = new SettingsFileScope();

            _ = new OptimizerSettings
            {
                OptimizationMethod = OptimizationMethodType.Bayesian,
                ObjectiveMetric = SortBotsType.TotalProfit,
                BayesianInitialSamples = 10,
                BayesianMaxIterations = 20,
                BayesianBatchSize = 3,
                ObjectiveDirection = ObjectiveDirectionType.Maximize,
                BayesianAcquisitionMode = BayesianAcquisitionModeType.Ucb,
                BayesianAcquisitionKappa = 0.5m,
                BayesianUseTailPass = true,
                BayesianTailSharePercent = 20
            };

            string[] lines = File.ReadAllLines(scope.SettingsPath);
            Assert.True(lines.Length >= 10);

            lines[^2] = "not-a-bool"; // BayesianUseTailPass
            lines[^1] = "17";         // BayesianTailSharePercent
            File.WriteAllLines(scope.SettingsPath, lines);

            OptimizerSettings reader = new OptimizerSettings();

            Assert.True(reader.BayesianUseTailPass);
            Assert.Equal(17, reader.BayesianTailSharePercent);
        }
    }

    [Fact]
    public void OptimizerSettings_SaveLoad_TailSharePercentBoundaries_ShouldRoundTrip()
    {
        lock (SettingsFileLock)
        {
            using SettingsFileScope _ = new SettingsFileScope();

            OptimizerSettings writer = new OptimizerSettings
            {
                BayesianTailSharePercent = 1
            };

            OptimizerSettings readerLow = new OptimizerSettings();
            Assert.Equal(1, readerLow.BayesianTailSharePercent);

            writer.BayesianTailSharePercent = 50;

            OptimizerSettings readerHigh = new OptimizerSettings();
            Assert.Equal(50, readerHigh.BayesianTailSharePercent);
        }
    }

    [Fact]
    public void AsyncBotFactory_GetBot_WithInvalidKeys_ShouldReturnNull()
    {
        AsyncBotFactory factory = new AsyncBotFactory();

        var byEmptyType = factory.GetBot("", "bot");
        var byEmptyName = factory.GetBot("type", "");
        var byWhitespace = factory.GetBot(" ", "   ");

        Assert.Null(byEmptyType);
        Assert.Null(byEmptyName);
        Assert.Null(byWhitespace);
    }

    [Fact]
    public void AsyncBotFactory_CreateNewBots_WithInvalidInput_ShouldNotThrow()
    {
        AsyncBotFactory factory = new AsyncBotFactory();

        Exception ex = Record.Exception(() =>
        {
            factory.CreateNewBots(null, "type", isScript: false, StartProgram.IsOsOptimizer);
            factory.CreateNewBots(new List<string>(), "type", isScript: false, StartProgram.IsOsOptimizer);
            factory.CreateNewBots(new List<string> { "bot" }, "", isScript: false, StartProgram.IsOsOptimizer);
            factory.CreateNewBots(
                new List<string> { "", "   " },
                "type",
                isScript: false,
                StartProgram.IsOsOptimizer);
        });

        Assert.Null(ex);
    }

    [Fact]
    public void AsyncBotFactory_GetBot_WithCanceledToken_ShouldReturnNull()
    {
        AsyncBotFactory factory = new AsyncBotFactory();
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.Cancel();

        var bot = factory.GetBot("type", "bot", cts.Token);

        Assert.Null(bot);
    }

    [Fact]
    public void BotConfigurator_CreateAndConfigureBot_WhenFactoryReturnsNull_ShouldReturnNull()
    {
        lock (SettingsFileLock)
        {
            using SettingsFileScope _ = new SettingsFileScope();

            OptimizerSettings settings = new OptimizerSettings
            {
                StrategyName = string.Empty
            };
            AsyncBotFactory factory = new AsyncBotFactory();
            BotManualControl manualControl = new BotManualControl("test_manual", null, StartProgram.IsOsOptimizer);
            BotConfigurator configurator = new BotConfigurator(settings, factory, manualControl);

            Exception ex = Record.Exception(() =>
            {
                var bot = configurator.CreateAndConfigureBot(
                    botName: "bot",
                    parameters: new List<IIStrategyParameter>(),
                    parametersOptimized: null,
                    server: null,
                    regime: StartProgram.IsOsOptimizer,
                    cancellationToken: CancellationToken.None);

                Assert.Null(bot);
            });

            Assert.Null(ex);
        }
    }

    [Fact]
    public void BotConfigurator_CreateAndConfigureBot_WithNullParameters_ShouldReturnNull()
    {
        lock (SettingsFileLock)
        {
            using SettingsFileScope _ = new SettingsFileScope();

            OptimizerSettings settings = new OptimizerSettings
            {
                StrategyName = string.Empty
            };
            AsyncBotFactory factory = new AsyncBotFactory();
            BotManualControl manualControl = new BotManualControl("test_manual2", null, StartProgram.IsOsOptimizer);
            BotConfigurator configurator = new BotConfigurator(settings, factory, manualControl);

            var bot = configurator.CreateAndConfigureBot(
                botName: "bot",
                parameters: null,
                parametersOptimized: null,
                server: null,
                regime: StartProgram.IsOsOptimizer,
                cancellationToken: CancellationToken.None);

            Assert.Null(bot);
        }
    }

    [Fact]
    public void BotConfigurator_CreateAndConfigureBot_WithEmptyBotName_ShouldReturnNull()
    {
        lock (SettingsFileLock)
        {
            using SettingsFileScope _ = new SettingsFileScope();

            OptimizerSettings settings = new OptimizerSettings
            {
                StrategyName = string.Empty
            };
            AsyncBotFactory factory = new AsyncBotFactory();
            BotManualControl manualControl = new BotManualControl("test_manual3", null, StartProgram.IsOsOptimizer);
            BotConfigurator configurator = new BotConfigurator(settings, factory, manualControl);

            var bot = configurator.CreateAndConfigureBot(
                botName: " ",
                parameters: new List<IIStrategyParameter>(),
                parametersOptimized: null,
                server: null,
                regime: StartProgram.IsOsOptimizer,
                cancellationToken: CancellationToken.None);

            Assert.Null(bot);
        }
    }

    private sealed class SettingsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;

        public SettingsFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "OptimizerSettings.txt");
            _settingsBackup = Path.Combine(_engineDirPath, "OptimizerSettings.txt.codex.bak");

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _settingsFileExisted = File.Exists(SettingsPath);
            if (_settingsFileExisted)
            {
                File.Copy(SettingsPath, _settingsBackup, overwrite: true);
            }
            else if (File.Exists(_settingsBackup))
            {
                File.Delete(_settingsBackup);
            }
        }

        public string SettingsPath { get; }

        public void Dispose()
        {
            if (_settingsFileExisted)
            {
                if (File.Exists(_settingsBackup))
                {
                    File.Copy(_settingsBackup, SettingsPath, overwrite: true);
                    File.Delete(_settingsBackup);
                }
            }
            else
            {
                if (File.Exists(SettingsPath))
                {
                    File.Delete(SettingsPath);
                }

                if (File.Exists(_settingsBackup))
                {
                    File.Delete(_settingsBackup);
                }
            }

            if (!_engineDirExisted && Directory.Exists(_engineDirPath))
            {
                if (!Directory.EnumerateFileSystemEntries(_engineDirPath).Any())
                {
                    Directory.Delete(_engineDirPath);
                }
            }
        }
    }

    private static OptimizerReport BuildSampleReport()
    {
        OptimizerReport report = new OptimizerReport(new List<IIStrategyParameter>
        {
            new StrategyParameterInt("Length", 5, 1, 10, 1),
            new StrategyParameterDecimal("Threshold", 0.5m, 0.1m, 1.0m, 0.1m)
        });

        report.BotName = "123 OpT InSample";
        report.PositionsCount = 7;
        report.TotalProfit = 1500.25m;
        report.MaxDrawDawn = -3.2m;
        report.AverageProfit = 210.5m;
        report.AverageProfitPercentOneContract = 1.7m;
        report.ProfitFactor = 1.3m;
        report.PayOffRatio = 1.1m;
        report.Recovery = 2.5m;
        report.TotalProfitPercent = 12.4m;
        report.SharpRatio = 0.9m;

        report.TabsReports.Add(new OptimizerReportTab
        {
            TabType = "BotTabSimple",
            SecurityName = "BTCUSDT",
            PositionsCount = 7,
            TotalProfit = 1500.25m,
            MaxDrawDawn = -3.2m,
            AverageProfit = 210.5m,
            AverageProfitPercentOneContract = 1.7m,
            ProfitFactor = 1.3m,
            PayOffRatio = 1.1m,
            Recovery = 2.5m,
            TotalProfitPercent = 12.4m,
            SharpRatio = 0.9m
        });

        return report;
    }

    private static List<List<IIStrategyParameter>> BuildIntCandidates(
        int count,
        string name,
        int start,
        int stop,
        int step)
    {
        List<List<IIStrategyParameter>> result = new List<List<IIStrategyParameter>>();

        for (int i = 0; i < count; i++)
        {
            int value = start + i * step;
            StrategyParameterInt p = new StrategyParameterInt(name, value, start, stop, step)
            {
                ValueInt = value
            };
            result.Add(new List<IIStrategyParameter> { p });
        }

        return result;
    }
}
