#nullable enable

using System.Collections.Generic;
using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Entity;
using OsEngine.Indicators;

namespace OsEngine.Tests;

public class FractalAndCciIncrementalPathTests
{
    [Fact]
    public void Stage2P2_Cci_IncrementalProcess_ShouldMatchBatchSeries()
    {
        List<Candle> candles = BuildCandles(96);

        Cci incremental = new Cci(canDelete: false)
        {
            Length = 21,
            TypePointsToSearch = PriceTypePoints.Typical
        };

        List<Candle> growingCandles = new List<Candle>();
        for (int i = 0; i < 24; i++)
        {
            growingCandles.Add(candles[i]);
        }

        incremental.Process(growingCandles);

        for (int i = 24; i < candles.Count; i++)
        {
            growingCandles.Add(candles[i]);
            incremental.Process(growingCandles);
        }

        Cci batch = new Cci(canDelete: false)
        {
            Length = 21,
            TypePointsToSearch = PriceTypePoints.Typical
        };
        batch.Process(candles);

        Assert.Equal(batch.Values, incremental.Values);
    }

    [Fact]
    public void Stage2P2_Fractal_IncrementalProcess_ShouldMatchBatchSeriesAndConfirmedLevels()
    {
        List<Candle> candles = BuildCandles(96);

        Fractal incremental = new Fractal(canDelete: false);
        List<Candle> growingCandles = new List<Candle>();

        for (int i = 0; i < 8; i++)
        {
            growingCandles.Add(candles[i]);
        }

        incremental.Process(growingCandles);

        for (int i = 8; i < candles.Count; i++)
        {
            growingCandles.Add(candles[i]);
            incremental.Process(growingCandles);
        }

        Fractal batch = new Fractal(canDelete: false);
        batch.Process(candles);

        Assert.Equal(batch.ValuesUp, incremental.ValuesUp);
        Assert.Equal(batch.ValuesDown, incremental.ValuesDown);
        Assert.Equal(batch.LastConfirmedUp, incremental.LastConfirmedUp);
        Assert.Equal(batch.LastConfirmedDown, incremental.LastConfirmedDown);
    }

    private static List<Candle> BuildCandles(int count)
    {
        List<Candle> candles = new List<Candle>(count);
        decimal price = 100m;

        for (int i = 0; i < count; i++)
        {
            decimal wave = (i % 6) switch
            {
                0 => 1.2m,
                1 => 2.4m,
                2 => -0.8m,
                3 => -2.6m,
                4 => 0.9m,
                _ => -1.1m
            };

            decimal open = price;
            decimal close = price + wave + (i % 5 - 2) * 0.35m;
            decimal high = (open > close ? open : close) + 0.8m + (i % 4) * 0.15m;
            decimal low = (open < close ? open : close) - 0.7m - (i % 3) * 0.2m;

            candles.Add(new Candle
            {
                TimeStart = new System.DateTime(2024, 1, 1).AddMinutes(i),
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = 100 + i
            });

            price = close + ((i % 4) - 1) * 0.18m;
        }

        return candles;
    }
}
