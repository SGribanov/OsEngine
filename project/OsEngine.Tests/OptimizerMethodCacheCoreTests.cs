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
    public void OptimizerMethodCacheKey_HashedCtor_ShouldMatchStringCtor()
    {
        string resultTypeName = typeof(decimal).FullName!;

        OptimizerMethodCacheKey stringKey = new OptimizerMethodCacheKey(
            securityName: "BTCUSDT",
            timeframeTicks: 60,
            firstTimeTicks: 1,
            lastTimeTicks: 2,
            candleCount: 100,
            calculationName: "Calc",
            parametersHash: "p1",
            sourceId: 12345,
            dataFingerprint: 42,
            resultTypeName: resultTypeName);

        OptimizerMethodCacheKey hashedKey = new OptimizerMethodCacheKey(
            securityName: new OrdinalHashedString("BTCUSDT"),
            timeframeTicks: 60,
            firstTimeTicks: 1,
            lastTimeTicks: 2,
            candleCount: 100,
            calculationName: new OrdinalHashedString("Calc"),
            parametersHash: new OrdinalHashedString("p1"),
            sourceId: 12345,
            dataFingerprint: 42,
            resultTypeName: new OrdinalHashedString(resultTypeName));

        Assert.Equal(stringKey, hashedKey);
        Assert.Equal(stringKey.GetHashCode(), hashedKey.GetHashCode());
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

    [Fact]
    public void OptimizerMethodCache_AddingNewKeyAtCapacity_ShouldEvictSingleEntryInsteadOfClearingAll()
    {
        OptimizerMethodCache cache = new OptimizerMethodCache(maxEntries: 2);
        OptimizerMethodCacheKey first = BuildKey("first");
        OptimizerMethodCacheKey second = BuildKey("second");
        OptimizerMethodCacheKey third = BuildKey("third");

        cache.Set(first, 1);
        cache.Set(second, 2);
        cache.Set(third, 3);

        bool firstFound = cache.TryGet(first, out int firstValue);
        bool secondFound = cache.TryGet(second, out int secondValue);
        bool thirdFound = cache.TryGet(third, out int thirdValue);

        Assert.True(thirdFound);
        Assert.Equal(3, thirdValue);
        Assert.True(firstFound || secondFound);
        Assert.False(firstFound && secondFound);
        Assert.True((firstFound && firstValue == 1) || (secondFound && secondValue == 2));

        OptimizerMethodCacheStatistics stats = cache.GetStatisticsSnapshot();
        Assert.Equal(2, stats.EntriesCount);
        Assert.Equal(1, stats.Evictions);
    }

    [Fact]
    public void OptimizerMethodCache_SetExistingAtCapacity_ShouldOverwriteWithoutEviction()
    {
        OptimizerMethodCache cache = new OptimizerMethodCache(maxEntries: 1);
        OptimizerMethodCacheKey key = BuildKey("stable");

        cache.Set(key, 1);
        cache.Set(key, 2);

        Assert.True(cache.TryGet(key, out int value));
        Assert.Equal(2, value);

        OptimizerMethodCacheStatistics stats = cache.GetStatisticsSnapshot();
        Assert.Equal(0, stats.Evictions);
        Assert.Equal(1, stats.EntriesCount);
        Assert.Equal(2, stats.Writes);
    }

    [Fact]
    public void OptimizerMethodCache_RepeatedChurn_ShouldStayWithinCapacityAndKeepNewestKey()
    {
        OptimizerMethodCache cache = new OptimizerMethodCache(maxEntries: 3);
        OptimizerMethodCacheKey newestKey = default;

        for (int i = 0; i < 16; i++)
        {
            newestKey = BuildKey("churn-" + i);
            cache.Set(newestKey, i);
        }

        Assert.True(cache.TryGet(newestKey, out int newestValue));
        Assert.Equal(15, newestValue);

        OptimizerMethodCacheStatistics stats = cache.GetStatisticsSnapshot();
        Assert.Equal(3, stats.EntriesCount);
        Assert.True(stats.Evictions >= 13);
    }

    private static OptimizerMethodCacheKey BuildKey(string sourceId)
    {
        return new OptimizerMethodCacheKey(
            securityName: "SEC",
            timeframeTicks: 60,
            firstTimeTicks: 1,
            lastTimeTicks: 2,
            candleCount: 100,
            calculationName: "Calc",
            parametersHash: "p",
            sourceId: sourceId,
            dataFingerprint: 1,
            resultTypeName: typeof(int).FullName!);
    }
}
