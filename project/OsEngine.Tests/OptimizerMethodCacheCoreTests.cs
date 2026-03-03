#nullable enable

using OsEngine.OsOptimizer.OptEntity;
using Xunit;

namespace OsEngine.Tests;

public class OptimizerMethodCacheCoreTests
{
    [Fact]
    public void OptimizerMethodCacheKey_EqualityAndHashCode_ShouldMatchForSameValues()
    {
        OptimizerMethodCacheKey key1 = new OptimizerMethodCacheKey(
            securityName: "BTCUSDT",
            timeframeTicks: 60,
            firstTimeTicks: 1,
            lastTimeTicks: 2,
            candleCount: 100,
            calculationName: "Calc",
            parametersHash: "p1",
            sourceId: "src",
            dataFingerprint: 42,
            resultTypeName: typeof(decimal).FullName!);

        OptimizerMethodCacheKey key2 = new OptimizerMethodCacheKey(
            securityName: "BTCUSDT",
            timeframeTicks: 60,
            firstTimeTicks: 1,
            lastTimeTicks: 2,
            candleCount: 100,
            calculationName: "Calc",
            parametersHash: "p1",
            sourceId: "src",
            dataFingerprint: 42,
            resultTypeName: typeof(decimal).FullName!);

        Assert.True(key1 == key2);
        Assert.True(key1.Equals(key2));
        Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
    }

    [Fact]
    public void OptimizerMethodCache_SetAndTryGet_ShouldReturnTypedValueAndTrackStats()
    {
        OptimizerMethodCache cache = new OptimizerMethodCache(maxEntries: 8);
        OptimizerMethodCacheKey key = new OptimizerMethodCacheKey(
            securityName: "SEC",
            timeframeTicks: 60,
            firstTimeTicks: 1,
            lastTimeTicks: 2,
            candleCount: 100,
            calculationName: "Calc",
            parametersHash: "p",
            sourceId: "src",
            dataFingerprint: 1,
            resultTypeName: typeof(int).FullName!);

        cache.Set(key, 42);

        Assert.True(cache.TryGet(key, out int value));
        Assert.Equal(42, value);

        OptimizerMethodCacheStatistics stats = cache.GetStatisticsSnapshot();
        Assert.Equal(1, stats.Hits);
        Assert.Equal(0, stats.Misses);
        Assert.Equal(1, stats.Writes);
        Assert.Equal(1, stats.EntriesCount);
    }

    [Fact]
    public void OptimizerMethodCache_IntSourceIdKey_ShouldWorkInSetGet()
    {
        OptimizerMethodCache cache = new OptimizerMethodCache(maxEntries: 8);
        OptimizerMethodCacheKey key = new OptimizerMethodCacheKey(
            securityName: "SEC",
            timeframeTicks: 60,
            firstTimeTicks: 1,
            lastTimeTicks: 2,
            candleCount: 100,
            calculationName: "Calc",
            parametersHash: "p",
            sourceId: 12345,
            dataFingerprint: 1,
            resultTypeName: typeof(int).FullName!);

        cache.Set(key, 42);
        Assert.True(cache.TryGet(key, out int value));
        Assert.Equal(42, value);
    }
}
