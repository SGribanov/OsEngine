#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Entity;
using OsEngine.Market.Connectors;
using OsEngine.Market.Servers;
using OsEngine.Market.Servers.Tester;
using OsEngine.OsOptimizer.OptEntity;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Grids;
using OsEngine.OsTrader.Panels.Tab;
using Xunit;

namespace OsEngine.Tests;

[Collection("Stage2PerfSerial")]
public class Stage2PerformanceBaselineTests
{
    [Fact]
    public void Stage2Perf_TradeGrid_QueryCollectionsHotPath_ShouldEmitMetricsAndDeterministicChecksum()
    {
        const int warmupIterations = 128;
        const int iterations = 4096;

        TradeGrid grid = CreateBareGrid();
        AttachReadyTabForQueries(grid);
        SeedGridLines(grid, 64);

        for (int i = 0; i < warmupIterations; i++)
        {
            _ = RunTradeGridQueryPass(grid);
        }

        ForceGc();

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int gen0Before = GC.CollectionCount(0);
        Stopwatch stopwatch = Stopwatch.StartNew();

        long checksum = 0;

        for (int i = 0; i < iterations; i++)
        {
            checksum += RunTradeGridQueryPass(grid);
        }

        stopwatch.Stop();

        long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
        int gen0After = GC.CollectionCount(0);

        long allocatedBytes = allocatedAfter - allocatedBefore;
        double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
        double nsPerOp = elapsedMs * 1_000_000d / iterations;
        double allocatedBytesPerOp = (double)allocatedBytes / iterations;

        Assert.True(checksum > 0);

        Stage2PerfReportWriter.Append(new Stage2PerfMetric
        {
            Scenario = "tradegrid_query_collections_hotpath",
            Iterations = iterations,
            ElapsedMsTotal = elapsedMs,
            NanosecondsPerOp = nsPerOp,
            AllocatedBytesTotal = allocatedBytes,
            AllocatedBytesPerOp = allocatedBytesPerOp,
            Gen0Collections = gen0After - gen0Before,
            Checksum = checksum
        });
    }

    [Fact]
    public void Stage2Perf_TradeGrid_LoadFromStringRuPayloadPath_ShouldEmitMetricsAndDeterministicChecksum()
    {
        const int warmupIterations = 200;
        const int iterations = 6000;
        const string payload =
            "1@MarketMaking@On@OnTrade@true@5@10@3@100,5@1@03.03.2026 14:20:30@500@true@1,5@true@@";

        TradeGrid grid = CreateBareGrid();

        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
        CultureInfo enUs = CultureInfo.GetCultureInfo("en-US");

        try
        {
            CultureInfo.CurrentCulture = enUs;
            CultureInfo.CurrentUICulture = enUs;

            for (int i = 0; i < warmupIterations; i++)
            {
                grid.LoadFromString(payload);
            }

            ForceGc();

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            int gen0Before = GC.CollectionCount(0);
            Stopwatch stopwatch = Stopwatch.StartNew();

            long checksum = 0;

            for (int i = 0; i < iterations; i++)
            {
                grid.LoadFromString(payload);
                checksum += grid.MaxOpenOrdersInMarket;
                checksum += (long)(grid.MaxDistanceToOrdersPercent * 100m);
            }

            stopwatch.Stop();

            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
            int gen0After = GC.CollectionCount(0);

            long allocatedBytes = allocatedAfter - allocatedBefore;
            double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
            double nsPerOp = elapsedMs * 1_000_000d / iterations;
            double allocatedBytesPerOp = (double)allocatedBytes / iterations;

            Assert.True(checksum > 0);

            Stage2PerfReportWriter.Append(new Stage2PerfMetric
            {
                Scenario = "tradegrid_load_from_string_ru_payload_path",
                Iterations = iterations,
                ElapsedMsTotal = elapsedMs,
                NanosecondsPerOp = nsPerOp,
                AllocatedBytesTotal = allocatedBytes,
                AllocatedBytesPerOp = allocatedBytesPerOp,
                Gen0Collections = gen0After - gen0Before,
                Checksum = checksum
            });
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void Stage2Perf_TradeGrid_LoadFromStringMalformedTailPath_ShouldEmitMetricsAndDeterministicChecksum()
    {
        const int warmupIterations = 200;
        const int iterations = 6000;

        string[] payloads =
        [
            "1@MarketMaking@On@OnTrade@true@5@10@3@100,5@1@03.03.2026 14:20:30@500@true@1,5@true@x@y% % %True@@11@@13@False%",
            "1@MarketMaking@On@OnTrade@maybe@5@10@3@100,5@1@bad-date@bad-delay@??@bad-distance@??%%%%%%%",
            " 1 @ MarketMaking @ On @ OncePerSecond @ true @ 5 @ 10 @ 3 @ 100,5 @ 1 @ 03.03.2026 14:20:30 @ 500 @ on @ 1,5 @ off @ @ %\t%\t%",
            "1@MarketMaking@On@OnTrade@1@5@10@3@100,5@1@2026-03-03T14:20:30Z@0@0@-1@1@%@dummy%@"
        ];

        TradeGrid grid = CreateBareGrid();

        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;
        CultureInfo enUs = CultureInfo.GetCultureInfo("en-US");

        try
        {
            CultureInfo.CurrentCulture = enUs;
            CultureInfo.CurrentUICulture = enUs;

            for (int i = 0; i < warmupIterations; i++)
            {
                grid.LoadFromString(payloads[i % payloads.Length]);
            }

            ForceGc();

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            int gen0Before = GC.CollectionCount(0);
            Stopwatch stopwatch = Stopwatch.StartNew();

            long checksum = 0;

            for (int i = 0; i < iterations; i++)
            {
                grid.LoadFromString(payloads[i % payloads.Length]);
                checksum += grid.MaxOpenOrdersInMarket;
                checksum += grid.CheckMicroVolumes ? 1 : 0;
                checksum += grid.OpenOrdersMakerOnly ? 1 : 0;
            }

            stopwatch.Stop();

            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
            int gen0After = GC.CollectionCount(0);

            long allocatedBytes = allocatedAfter - allocatedBefore;
            double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
            double nsPerOp = elapsedMs * 1_000_000d / iterations;
            double allocatedBytesPerOp = (double)allocatedBytes / iterations;

            Assert.True(checksum > 0);

            Stage2PerfReportWriter.Append(new Stage2PerfMetric
            {
                Scenario = "tradegrid_load_from_string_malformed_tail_path",
                Iterations = iterations,
                ElapsedMsTotal = elapsedMs,
                NanosecondsPerOp = nsPerOp,
                AllocatedBytesTotal = allocatedBytes,
                AllocatedBytesPerOp = allocatedBytesPerOp,
                Gen0Collections = gen0After - gen0Before,
                Checksum = checksum
            });
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void Stage2Perf_FractalAndCci_ManualSignalHotPath_ShouldEmitMetricsAndDeterministicChecksum()
    {
        const int period = 21;
        const int seedCandles = 128;
        const int warmupIterations = 512;
        const int iterations = 20000;

        List<Candle> sourceCandles = BuildCandlesForFractalSignalPath(seedCandles + warmupIterations + iterations + 4);
        FractalAndCciPerfState state = CreateFractalAndCciPerfState(sourceCandles, seedCandles, period);

        for (int i = 0; i < warmupIterations; i++)
        {
            _ = RunFractalAndCciProductionSignalPass(state, sourceCandles[seedCandles + i]);
        }

        ForceGc();

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int gen0Before = GC.CollectionCount(0);
        Stopwatch stopwatch = Stopwatch.StartNew();

        long checksum = 0;

        for (int i = 0; i < iterations; i++)
        {
            checksum += RunFractalAndCciProductionSignalPass(state, sourceCandles[seedCandles + warmupIterations + i]);
        }

        stopwatch.Stop();

        long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
        int gen0After = GC.CollectionCount(0);

        long allocatedBytes = allocatedAfter - allocatedBefore;
        double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
        double nsPerOp = elapsedMs * 1_000_000d / iterations;
        double allocatedBytesPerOp = (double)allocatedBytes / iterations;

        Assert.True(checksum != 0);

        Stage2PerfReportWriter.Append(new Stage2PerfMetric
        {
            Scenario = "fractal_and_cci_production_signal_hotpath_v2",
            Iterations = iterations,
            ElapsedMsTotal = elapsedMs,
            NanosecondsPerOp = nsPerOp,
            AllocatedBytesTotal = allocatedBytes,
            AllocatedBytesPerOp = allocatedBytesPerOp,
            Gen0Collections = gen0After - gen0Before,
            Checksum = checksum
        });
    }

    [Fact]
    public void Stage2Perf_IndicatorCache_HitPath_ShouldEmitMetricsAndStableChecksums()
    {
        const int warmupIterations = 200;
        const int iterations = 2000;

        IndicatorCache cache = new IndicatorCache(
            maxEntries: 64,
            isolationMode: IndicatorCacheIsolationMode.TrustedReferences);
        IndicatorCacheKey key = new IndicatorCacheKey(
            securityName: "PERF",
            timeframeTicks: 60,
            firstTimeTicks: 1,
            lastTimeTicks: 2,
            candleCount: 256,
            calculationName: "Stage2Perf",
            parametersHash: "P0",
            sourceId: "S0",
            outputSeriesCount: 3,
            includeIndicatorsCount: 0,
            dataFingerprint: 11);

        List<decimal>[] payload = BuildSeriesPayload();
        cache.Set(key, payload);

        for (int i = 0; i < warmupIterations; i++)
        {
            bool warmupHit = cache.TryGet(key, out _);
            Assert.True(warmupHit);
        }

        cache.Clear();
        cache.Set(key, payload);

        ForceGc();

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int gen0Before = GC.CollectionCount(0);
        Stopwatch stopwatch = Stopwatch.StartNew();

        long checksum = 0;

        for (int i = 0; i < iterations; i++)
        {
            bool hit = cache.TryGet(key, out List<decimal>[]? values);
            Assert.True(hit);
            Assert.NotNull(values);
            Assert.Equal(3, values!.Length);
            checksum += (long)(values[0][0] * 1000m);
            checksum += (long)(values[2][255] * 1000m);
        }

        stopwatch.Stop();

        long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
        int gen0After = GC.CollectionCount(0);
        IndicatorCacheStatistics stats = cache.GetStatisticsSnapshot();

        long allocatedBytes = allocatedAfter - allocatedBefore;
        double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
        double nsPerOp = elapsedMs * 1_000_000d / iterations;
        double allocatedBytesPerOp = (double)allocatedBytes / iterations;

        Assert.True(checksum > 0);
        Assert.Equal(iterations, stats.Hits);
        Assert.Equal(0, stats.Misses);

        Stage2PerfReportWriter.Append(new Stage2PerfMetric
        {
            Scenario = "indicator_cache_hit_path",
            Iterations = iterations,
            ElapsedMsTotal = elapsedMs,
            NanosecondsPerOp = nsPerOp,
            AllocatedBytesTotal = allocatedBytes,
            AllocatedBytesPerOp = allocatedBytesPerOp,
            Gen0Collections = gen0After - gen0Before,
            Checksum = checksum
        });
    }

    [Fact]
    public void Stage2Perf_OptimizerMethodCache_HitPath_ShouldEmitMetricsAndStableChecksums()
    {
        const int warmupIterations = 200;
        const int iterations = 4000;

        OptimizerMethodCache cache = new OptimizerMethodCache(maxEntries: 64);
        OptimizerMethodCacheKey key = new OptimizerMethodCacheKey(
            securityName: "PERF",
            timeframeTicks: 60,
            firstTimeTicks: 1,
            lastTimeTicks: 2,
            candleCount: 256,
            calculationName: "Stage2MethodCachePerf",
            parametersHash: "P0",
            sourceId: "S0",
            dataFingerprint: 17,
            resultTypeName: typeof(decimal).FullName ?? nameof(Decimal));

        cache.Set(key, 123.456m);

        for (int i = 0; i < warmupIterations; i++)
        {
            bool warmupHit = cache.TryGet(key, out decimal _);
            Assert.True(warmupHit);
        }

        cache.Clear();
        cache.Set(key, 123.456m);

        ForceGc();

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int gen0Before = GC.CollectionCount(0);
        Stopwatch stopwatch = Stopwatch.StartNew();

        long checksum = 0;

        for (int i = 0; i < iterations; i++)
        {
            bool hit = cache.TryGet(key, out decimal value);
            Assert.True(hit);
            checksum += (long)(value * 1000m);
        }

        stopwatch.Stop();

        long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
        int gen0After = GC.CollectionCount(0);
        OptimizerMethodCacheStatistics stats = cache.GetStatisticsSnapshot();

        long allocatedBytes = allocatedAfter - allocatedBefore;
        double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
        double nsPerOp = elapsedMs * 1_000_000d / iterations;
        double allocatedBytesPerOp = (double)allocatedBytes / iterations;

        Assert.True(checksum > 0);
        Assert.Equal(iterations, stats.Hits);
        Assert.Equal(0, stats.Misses);

        Stage2PerfReportWriter.Append(new Stage2PerfMetric
        {
            Scenario = "optimizer_method_cache_hit_path",
            Iterations = iterations,
            ElapsedMsTotal = elapsedMs,
            NanosecondsPerOp = nsPerOp,
            AllocatedBytesTotal = allocatedBytes,
            AllocatedBytesPerOp = allocatedBytesPerOp,
            Gen0Collections = gen0After - gen0Before,
            Checksum = checksum
        });
    }

    [Fact]
    public void Stage2Perf_OptimizerCacheKeyBuildPath_ShouldEmitMetricsAndStableChecksums()
    {
        const int warmupIterations = 200;
        const int iterations = 20000;

        List<Candle> candles = BuildCandlesForKeyBuild(256);
        int sourceId = RuntimeHelpers.GetHashCode(candles);
        long firstTicks = candles[0].TimeStart.Ticks;
        long lastTicks = candles[^1].TimeStart.Ticks;
        const long timeframeTicks = 60L;
        OrdinalHashedString indicatorSecurityName = OrdinalHashedString.Empty;
        OrdinalHashedString indicatorCalculationName = new OrdinalHashedString("Stage2Perf");
        OrdinalHashedString indicatorParametersHash = new OrdinalHashedString("P0");
        OrdinalHashedString methodSecurityName = new OrdinalHashedString("PERF");
        OrdinalHashedString methodCalculationName = new OrdinalHashedString("Stage2PerfMethod");
        OrdinalHashedString methodParametersHash = new OrdinalHashedString("P0");
        OrdinalHashedString methodResultTypeName = new OrdinalHashedString(typeof(decimal).FullName ?? nameof(Decimal));

        for (int i = 0; i < warmupIterations; i++)
        {
            _ = new IndicatorCacheKey(
                securityName: indicatorSecurityName,
                timeframeTicks: timeframeTicks,
                firstTimeTicks: firstTicks,
                lastTimeTicks: lastTicks,
                candleCount: candles.Count,
                calculationName: indicatorCalculationName,
                parametersHash: indicatorParametersHash,
                sourceId: sourceId,
                outputSeriesCount: 3,
                includeIndicatorsCount: 0,
                dataFingerprint: 11).GetHashCode();

            _ = new OptimizerMethodCacheKey(
                securityName: methodSecurityName,
                timeframeTicks: timeframeTicks,
                firstTimeTicks: firstTicks,
                lastTimeTicks: lastTicks,
                candleCount: candles.Count,
                calculationName: methodCalculationName,
                parametersHash: methodParametersHash,
                sourceId: sourceId,
                dataFingerprint: 17,
                resultTypeName: methodResultTypeName).GetHashCode();
        }

        ForceGc();

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int gen0Before = GC.CollectionCount(0);
        Stopwatch stopwatch = Stopwatch.StartNew();

        long checksum = 0;

        for (int i = 0; i < iterations; i++)
        {
            IndicatorCacheKey indicatorKey = new IndicatorCacheKey(
                securityName: indicatorSecurityName,
                timeframeTicks: timeframeTicks,
                firstTimeTicks: firstTicks,
                lastTimeTicks: lastTicks,
                candleCount: candles.Count,
                calculationName: indicatorCalculationName,
                parametersHash: indicatorParametersHash,
                sourceId: sourceId,
                outputSeriesCount: 3,
                includeIndicatorsCount: 0,
                dataFingerprint: 11);

            OptimizerMethodCacheKey methodKey = new OptimizerMethodCacheKey(
                securityName: methodSecurityName,
                timeframeTicks: timeframeTicks,
                firstTimeTicks: firstTicks,
                lastTimeTicks: lastTicks,
                candleCount: candles.Count,
                calculationName: methodCalculationName,
                parametersHash: methodParametersHash,
                sourceId: sourceId,
                dataFingerprint: 17,
                resultTypeName: methodResultTypeName);

            int indicatorHash = indicatorKey.GetHashCode();
            int methodHash = methodKey.GetHashCode();

            // Keep hash calculations in the hot path while preserving a deterministic checksum across processes.
            if ((indicatorHash | methodHash) == int.MinValue)
            {
                checksum--;
            }

            checksum += indicatorKey.CandleCount + methodKey.CandleCount;
        }

        stopwatch.Stop();

        long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
        int gen0After = GC.CollectionCount(0);

        long allocatedBytes = allocatedAfter - allocatedBefore;
        double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
        double nsPerOp = elapsedMs * 1_000_000d / iterations;
        double allocatedBytesPerOp = (double)allocatedBytes / iterations;

        Assert.True(checksum != 0);

        Stage2PerfReportWriter.Append(new Stage2PerfMetric
        {
            Scenario = "optimizer_cache_key_build_path",
            Iterations = iterations,
            ElapsedMsTotal = elapsedMs,
            NanosecondsPerOp = nsPerOp,
            AllocatedBytesTotal = allocatedBytes,
            AllocatedBytesPerOp = allocatedBytesPerOp,
            Gen0Collections = gen0After - gen0Before,
            Checksum = checksum
        });
    }

    [Fact]
    public void Stage2Perf_OptimizerMethodParameterHashPath_ShouldEmitMetricsAndStableChecksums()
    {
        const int warmupIterations = 400;
        const int iterations = 30000;
        const int seed = 37;

        for (int i = 0; i < warmupIterations; i++)
        {
            _ = BotPanelMethodHashPerfAccessor.BuildInt(seed + (i & 63));
        }

        ForceGc();

        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        int gen0Before = GC.CollectionCount(0);
        Stopwatch stopwatch = Stopwatch.StartNew();

        long checksum = 0;

        for (int i = 0; i < iterations; i++)
        {
            string hash = BotPanelMethodHashPerfAccessor.BuildInt(seed + (i & 63));
            checksum += hash[0];
            checksum += hash[7];
        }

        stopwatch.Stop();

        long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();
        int gen0After = GC.CollectionCount(0);

        long allocatedBytes = allocatedAfter - allocatedBefore;
        double elapsedMs = stopwatch.Elapsed.TotalMilliseconds;
        double nsPerOp = elapsedMs * 1_000_000d / iterations;
        double allocatedBytesPerOp = (double)allocatedBytes / iterations;

        Assert.True(checksum > 0);

        Stage2PerfReportWriter.Append(new Stage2PerfMetric
        {
            Scenario = "optimizer_method_parameter_hash_path",
            Iterations = iterations,
            ElapsedMsTotal = elapsedMs,
            NanosecondsPerOp = nsPerOp,
            AllocatedBytesTotal = allocatedBytes,
            AllocatedBytesPerOp = allocatedBytesPerOp,
            Gen0Collections = gen0After - gen0Before,
            Checksum = checksum
        });
    }

    private static long RunTradeGridQueryPass(TradeGrid grid)
    {
        TradeGrid.QueryCollectionsSnapshot queryCollections = grid.GetQueryCollections(100m);
        List<TradeGridLine> openPositions = queryCollections.OpenPositions;
        List<TradeGridLine> openOrdersNeed = queryCollections.OpenOrdersNeed;
        List<TradeGridLine> openOrdersFact = queryCollections.OpenOrdersFact;
        List<TradeGridLine> closeOrdersFact = queryCollections.ClosingOrdersFact;

        return openPositions.Count
               + (openOrdersNeed.Count * 10L)
               + (openOrdersFact.Count * 100L)
               + (closeOrdersFact.Count * 1000L);
    }

    private static long RunFractalAndCciProductionSignalPass(FractalAndCciPerfState state, Candle nextCandle)
    {
        state.Candles.Add(nextCandle);
        state.Cci.Process(state.Candles);
        state.Fractal.Process(state.Candles);

        int lastIndex = state.Candles.Count - 1;
        int signalIndex = lastIndex - 2;

        decimal lastCci = state.Cci.Values[lastIndex];
        decimal prevCci = state.Cci.Values[lastIndex - 1];
        decimal upFractal = GetLastConfirmedFractal(state.Fractal.ValuesUp, signalIndex);
        decimal downFractal = GetLastConfirmedFractal(state.Fractal.ValuesDown, signalIndex);
        decimal lastPrice = state.Candles[lastIndex].Close;

        long checksum = (long)(lastCci * 10m)
                        + (long)(prevCci * 10m)
                        + (long)(upFractal * 100m)
                        + (long)(downFractal * 100m)
                        + signalIndex;

        if (downFractal != 0m && downFractal < lastPrice && prevCci < -300m && lastCci > -300m)
        {
            checksum += 17;
        }

        if (upFractal != 0m && upFractal > lastPrice && prevCci > 300m && lastCci < 300m)
        {
            checksum += 31;
        }

        return checksum;
    }

    private static FractalAndCciPerfState CreateFractalAndCciPerfState(List<Candle> sourceCandles, int seedCandles, int period)
    {
        List<Candle> candles = new List<Candle>(seedCandles);

        for (int i = 0; i < seedCandles; i++)
        {
            candles.Add(sourceCandles[i]);
        }

        Cci cci = new Cci(canDelete: false)
        {
            Length = period,
            TypePointsToSearch = PriceTypePoints.Typical
        };

        Fractal fractal = new Fractal(canDelete: false);

        cci.Process(candles);
        fractal.Process(candles);

        return new FractalAndCciPerfState(candles, cci, fractal);
    }

    private static decimal GetLastConfirmedFractal(List<decimal> values, int fromIndex)
    {
        for (int i = fromIndex; i >= 0; i--)
        {
            decimal value = values[i];
            if (value != 0m)
            {
                return value;
            }
        }

        return 0m;
    }

    private static List<decimal>[] BuildSeriesPayload()
    {
        List<decimal> first = new List<decimal>(256);
        List<decimal> second = new List<decimal>(256);
        List<decimal> third = new List<decimal>(256);

        decimal value = 1m;

        for (int i = 0; i < 256; i++)
        {
            value = (value * 1.01m) + 0.1m;
            first.Add(value);
            second.Add(value * 0.7m);
            third.Add(value * 1.3m);
        }

        return new[] { first, second, third };
    }

    private static List<Candle> BuildCandlesForKeyBuild(int count)
    {
        List<Candle> candles = new List<Candle>(count);
        DateTime start = DateTime.UtcNow.AddMinutes(-count);
        decimal value = 100m;

        for (int i = 0; i < count; i++)
        {
            value = value + (i % 2 == 0 ? 0.1m : -0.05m);
            candles.Add(new Candle
            {
                TimeStart = start.AddMinutes(i),
                Open = value,
                High = value + 0.2m,
                Low = value - 0.2m,
                Close = value + 0.05m,
                Volume = 10m + i
            });
        }

        return candles;
    }

    private static List<Candle> BuildCandlesForFractalSignalPath(int count)
    {
        List<Candle> candles = new List<Candle>(count);
        DateTime start = DateTime.UtcNow.AddMinutes(-count);

        for (int i = 0; i < count; i++)
        {
            double wave = Math.Sin(i * 0.25d);
            decimal center = 100m + (decimal)(wave * 4d) + (i * 0.01m);
            decimal closeOffset = ((decimal)(i & 3) - 1.5m) * 0.1m;

            candles.Add(new Candle
            {
                TimeStart = start.AddMinutes(i),
                Open = center - 0.2m,
                High = center + 0.6m + ((i % 3) * 0.05m),
                Low = center - 0.6m - ((i & 1) * 0.05m),
                Close = center + closeOffset,
                Volume = 100m + (i % 11)
            });
        }

        return candles;
    }

    private static void SeedGridLines(TradeGrid grid, int count)
    {
        List<TradeGridLine> lines = new List<TradeGridLine>(count);

        for (int i = 0; i < count; i++)
        {
            TradeGridLine line = new TradeGridLine
            {
                PriceEnter = 90m + i,
                PriceExit = 100m + i,
                Side = i % 2 == 0 ? Side.Buy : Side.Sell,
                CanReplaceExitOrder = true,
                Volume = 1m
            };

            if (i % 4 != 0)
            {
                Position position = (Position)RuntimeHelpers.GetUninitializedObject(typeof(Position));
                OrderStateType openState = i % 3 == 0 ? OrderStateType.Active : OrderStateType.Done;
                decimal openVolume = i % 6 == 0 ? 0m : 1m;

                SetPrivateField(position, "_openOrders", new List<Order>
                {
                    new Order
                    {
                        State = openState,
                        VolumeExecute = openVolume
                    }
                });

                SetPrivateField(position, "_closeOrders", i % 5 == 0
                    ? new List<Order>
                    {
                        new Order
                        {
                            State = OrderStateType.Active,
                            VolumeExecute = 0m
                        }
                    }
                    : new List<Order>());

                line.Position = position;
            }

            lines.Add(line);
        }

        grid.GridCreator.Lines = lines;
        grid.MaxOpenOrdersInMarket = 12;
        grid.OpenOrdersMakerOnly = true;
    }

    private static TradeGrid CreateBareGrid()
    {
        TradeGrid grid = (TradeGrid)RuntimeHelpers.GetUninitializedObject(typeof(TradeGrid));
        grid.NonTradePeriods = new TradeGridNonTradePeriods("CodexPerfGrid");
        grid.StopBy = new TradeGridStopBy();
        grid.StopAndProfit = new TradeGridStopAndProfit();
        grid.AutoStarter = new TradeGridAutoStarter();
        grid.GridCreator = new TradeGridCreator();
        grid.ErrorsReaction = new TradeGridErrorsReaction(grid);
        grid.TrailingUp = new TrailingUp(grid);
        grid.StartProgram = StartProgram.IsTester;
        grid.Regime = TradeGridRegime.On;
        return grid;
    }

    private static void AttachReadyTabForQueries(TradeGrid grid)
    {
        BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
        tab.TabName = "CodexPerfTab";

        ConnectorCandles connector = (ConnectorCandles)RuntimeHelpers.GetUninitializedObject(typeof(ConnectorCandles));
        connector.StartProgram = StartProgram.IsTester;

        TimeFrameBuilder builder = new TimeFrameBuilder("CodexPerf", StartProgram.IsTester);
        CandleSeries series = new CandleSeries(builder, new Security(), StartProgram.IsTester)
        {
            CandlesAll = new List<Candle> { new Candle { TimeStart = DateTime.UtcNow, Close = 100m } }
        };

        TesterServer server = (TesterServer)RuntimeHelpers.GetUninitializedObject(typeof(TesterServer));
        server.LastStartServerTime = DateTime.UtcNow.AddMinutes(-10);

        SetPrivateField(server, "_serverConnectStatus", ServerConnectStatus.Connect);
        SetPrivateField(server, "_serverTime", DateTime.UtcNow);

        SetPrivateField(connector, "_mySeries", series);
        SetPrivateField(connector, "_myServer", server);
        SetPrivateField(connector, "_eventsIsOn", true);
        SetPrivateField(tab, "_connector", connector);

        grid.Tab = tab;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?? throw new InvalidOperationException("Field " + fieldName + " not found.");
        field.SetValue(target, value);
    }

    private static void ForceGc()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}

internal sealed class FractalAndCciPerfState
{
    public FractalAndCciPerfState(List<Candle> candles, Cci cci, Fractal fractal)
    {
        Candles = candles;
        Cci = cci;
        Fractal = fractal;
    }

    public List<Candle> Candles { get; }

    public Cci Cci { get; }

    public Fractal Fractal { get; }
}

[CollectionDefinition("Stage2PerfSerial", DisableParallelization = true)]
public class Stage2PerfSerialCollectionDefinition
{
}

internal sealed class BotPanelMethodHashPerfAccessor : BotPanel
{
    private BotPanelMethodHashPerfAccessor() : base("BotPanelMethodHashPerfAccessor", StartProgram.IsTester)
    {
    }

    internal static string BuildInt(int value)
    {
        return BuildOptimizerMethodCacheParameterHash(value);
    }
}

internal sealed class Stage2PerfMetric
{
    public string Scenario { get; init; } = string.Empty;

    public int Iterations { get; init; }

    public double ElapsedMsTotal { get; init; }

    public double NanosecondsPerOp { get; init; }

    public long AllocatedBytesTotal { get; init; }

    public double AllocatedBytesPerOp { get; init; }

    public int Gen0Collections { get; init; }

    public long Checksum { get; init; }

    public string RecordedAtUtc { get; init; } = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
}

internal static class Stage2PerfReportWriter
{
    private static readonly Lock Sync = new();
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        WriteIndented = false
    };

    private const string MetricsFileName = "stage2_perf_metrics.jsonl";
    private const string RepoMarkerFile = "refactoring_stage2_plan.md";

    public static void Append(Stage2PerfMetric metric)
    {
        if (metric == null)
        {
            return;
        }

        string repoRoot = ResolveRepoRoot();
        string reportsDir = Path.Combine(repoRoot, "reports");
        Directory.CreateDirectory(reportsDir);

        string metricsPath = Path.Combine(reportsDir, MetricsFileName);
        string payload = JsonSerializer.Serialize(metric, JsonOptions) + Environment.NewLine;

        lock (Sync)
        {
            File.AppendAllText(metricsPath, payload);
        }
    }

    private static string ResolveRepoRoot()
    {
        DirectoryInfo? current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current != null)
        {
            string markerPath = Path.Combine(current.FullName, RepoMarkerFile);

            if (File.Exists(markerPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
