#nullable enable

using System.Collections.Generic;
using OsEngine.OsOptimizer.OptEntity;
using Xunit;

namespace OsEngine.Tests;

public class OptimizerIndicatorCachePilotMetricsTests
{
    [Fact]
    public void Stage2Step3_1_IndicatorCachePilot_ShouldReduceComputationsInRepresentativeReplay()
    {
        const int uniqueKeys = 40;
        const int repeatsPerKey = 30;
        const int operations = uniqueKeys * repeatsPerKey;

        int withoutCacheComputations = RunScenarioWithoutCache(uniqueKeys, repeatsPerKey);
        (int withCacheComputations, IndicatorCacheStatistics stats) = RunScenarioWithCache(uniqueKeys, repeatsPerKey);

        Assert.Equal(operations, withoutCacheComputations);
        Assert.Equal(uniqueKeys, withCacheComputations);

        Assert.Equal(uniqueKeys, stats.Writes);
        Assert.Equal(uniqueKeys, stats.Misses);
        Assert.Equal(operations - uniqueKeys, stats.Hits);
        Assert.Equal(uniqueKeys, stats.EntriesCount);
        Assert.True(stats.HitRate > 0.95d);
    }

    private static int RunScenarioWithoutCache(int uniqueKeys, int repeatsPerKey)
    {
        int computations = 0;

        for (int i = 0; i < uniqueKeys; i++)
        {
            for (int j = 0; j < repeatsPerKey; j++)
            {
                _ = BuildExpensiveSeries(i);
                computations++;
            }
        }

        return computations;
    }

    private static (int computations, IndicatorCacheStatistics stats) RunScenarioWithCache(int uniqueKeys, int repeatsPerKey)
    {
        int computations = 0;
        IndicatorCache cache = new IndicatorCache(maxEntries: uniqueKeys + 8);

        for (int i = 0; i < uniqueKeys; i++)
        {
            IndicatorCacheKey key = BuildKey(i);
            for (int j = 0; j < repeatsPerKey; j++)
            {
                if (!cache.TryGet(key, out _))
                {
                    List<decimal>[] computed = BuildExpensiveSeries(i);
                    cache.Set(key, computed);
                    computations++;
                }
            }
        }

        return (computations, cache.GetStatisticsSnapshot());
    }

    private static IndicatorCacheKey BuildKey(int index)
    {
        return new IndicatorCacheKey(
            securityName: "SEC_" + index,
            timeframeTicks: 60,
            firstTimeTicks: 1,
            lastTimeTicks: 2,
            candleCount: 1000,
            calculationName: "PilotCalc",
            parametersHash: "P_" + index,
            sourceId: "Source",
            outputSeriesCount: 1,
            includeIndicatorsCount: 0,
            dataFingerprint: index);
    }

    private static List<decimal>[] BuildExpensiveSeries(int seed)
    {
        List<decimal> data = new List<decimal>(256);
        decimal value = seed + 1m;

        for (int i = 0; i < 256; i++)
        {
            value = (value * 1.013m) - (i * 0.0003m);
            data.Add(value);
        }

        return [data];
    }
}
