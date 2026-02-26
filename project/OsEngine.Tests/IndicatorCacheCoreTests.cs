#nullable enable

using System.Collections.Generic;
using OsEngine.OsOptimizer.OptEntity;
using Xunit;

namespace OsEngine.Tests;

public class IndicatorCacheCoreTests
{
    [Fact]
    public void IndicatorCacheKey_EqualityAndHashCode_ShouldMatchForSameValues()
    {
        IndicatorCacheKey key1 = new IndicatorCacheKey(
            securityName: "BTCUSDT",
            timeframeTicks: 60,
            firstTimeTicks: 1,
            lastTimeTicks: 2,
            candleCount: 100,
            calculationName: "SMA",
            parametersHash: "p1",
            sourceId: "src",
            outputSeriesCount: 1,
            includeIndicatorsCount: 0,
            dataFingerprint: 42);

        IndicatorCacheKey key2 = new IndicatorCacheKey(
            securityName: "BTCUSDT",
            timeframeTicks: 60,
            firstTimeTicks: 1,
            lastTimeTicks: 2,
            candleCount: 100,
            calculationName: "SMA",
            parametersHash: "p1",
            sourceId: "src",
            outputSeriesCount: 1,
            includeIndicatorsCount: 0,
            dataFingerprint: 42);

        Assert.True(key1 == key2);
        Assert.True(key1.Equals(key2));
        Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
    }

    [Fact]
    public void IndicatorCache_SetAndTryGet_ShouldCloneValuesAndTrackStats()
    {
        IndicatorCache cache = new IndicatorCache(maxEntries: 8);
        IndicatorCacheKey key = BuildKey("A");

        List<decimal>[] values =
        [
            new List<decimal> { 1m, 2m, 3m },
            new List<decimal> { 10m }
        ];

        cache.Set(key, values);

        values[0][0] = 999m;

        bool found = cache.TryGet(key, out List<decimal>[]? loaded);

        Assert.True(found);
        Assert.NotNull(loaded);
        Assert.Equal(1m, loaded![0][0]);
        Assert.Equal(3, loaded[0].Count);

        loaded[0][1] = 888m;

        bool foundAgain = cache.TryGet(key, out List<decimal>[]? loadedAgain);
        Assert.True(foundAgain);
        Assert.NotNull(loadedAgain);
        Assert.Equal(2m, loadedAgain![0][1]);

        IndicatorCacheStatistics stats = cache.GetStatisticsSnapshot();
        Assert.Equal(2, stats.Hits);
        Assert.Equal(0, stats.Misses);
        Assert.Equal(1, stats.Writes);
        Assert.Equal(1, stats.EntriesCount);
        Assert.True(stats.HitRate > 0.99d);
    }

    [Fact]
    public void IndicatorCache_ShouldEvict_WhenCapacityExceeded()
    {
        IndicatorCache cache = new IndicatorCache(maxEntries: 1);

        cache.Set(BuildKey("A"), [new List<decimal> { 1m }]);
        cache.Set(BuildKey("B"), [new List<decimal> { 2m }]);

        bool oldFound = cache.TryGet(BuildKey("A"), out _);
        bool newFound = cache.TryGet(BuildKey("B"), out _);

        Assert.False(oldFound);
        Assert.True(newFound);

        IndicatorCacheStatistics stats = cache.GetStatisticsSnapshot();
        Assert.True(stats.Evictions >= 1);
    }

    [Fact]
    public void IndicatorCache_Clear_ShouldResetEntriesAndCounters()
    {
        IndicatorCache cache = new IndicatorCache(maxEntries: 4);
        IndicatorCacheKey key = BuildKey("A");

        cache.Set(key, [new List<decimal> { 1m }]);
        cache.TryGet(key, out _);
        cache.Clear();

        bool found = cache.TryGet(key, out _);
        Assert.False(found);

        IndicatorCacheStatistics stats = cache.GetStatisticsSnapshot();
        Assert.Equal(0, stats.Hits);
        Assert.Equal(1, stats.Misses);
        Assert.Equal(0, stats.Writes);
        Assert.Equal(0, stats.Evictions);
        Assert.Equal(0, stats.EntriesCount);
    }

    private static IndicatorCacheKey BuildKey(string id)
    {
        return new IndicatorCacheKey(
            securityName: id,
            timeframeTicks: 60,
            firstTimeTicks: 1,
            lastTimeTicks: 2,
            candleCount: 100,
            calculationName: "Calc",
            parametersHash: "Hash",
            sourceId: "Source",
            outputSeriesCount: 1,
            includeIndicatorsCount: 0,
            dataFingerprint: 1);
    }
}
