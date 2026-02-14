using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Entity;
using OsEngine.OsOptimizer;
using OsEngine.OsOptimizer.OptEntity;
using Xunit;

namespace OsEngine.Tests;

public class OptimizerRefactorTests
{
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
}
