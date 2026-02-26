using System;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class CandleCoreTests
{
    [Fact]
    public void GetPoint_ShouldReturnExpectedValues()
    {
        Candle candle = new Candle
        {
            Open = 10m,
            High = 16m,
            Low = 8m,
            Close = 14m
        };

        Assert.Equal(14m, candle.GetPoint("Close"));
        Assert.Equal(16m, candle.GetPoint("High"));
        Assert.Equal(8m, candle.GetPoint("Low"));
        Assert.Equal(10m, candle.GetPoint("Open"));
        Assert.Equal(12m, candle.GetPoint("Median"));
        Assert.Equal((16m + 8m + 14m) / 3m, candle.GetPoint("Typical"));
    }

    [Fact]
    public void ShapeProperties_ShouldBeCalculatedCorrectly_ForUpAndDownCandles()
    {
        Candle up = new Candle
        {
            Open = 100m,
            High = 120m,
            Low = 90m,
            Close = 110m
        };

        Assert.True(up.IsUp);
        Assert.False(up.IsDown);
        Assert.False(up.IsDoji);
        Assert.Equal(10m, up.ShadowTop);
        Assert.Equal(10m, up.ShadowBottom);
        Assert.Equal(10m, up.Body);
        Assert.Equal(10m, up.BodyPercent);
        Assert.Equal(105m, up.Center);
        Assert.Equal((120m - 105m) / 105m * 100m, up.Volatility);

        Candle down = new Candle
        {
            Open = 120m,
            High = 125m,
            Low = 100m,
            Close = 105m
        };

        Assert.False(down.IsUp);
        Assert.True(down.IsDown);
        Assert.False(down.IsDoji);
        Assert.Equal(5m, down.ShadowTop);
        Assert.Equal(5m, down.ShadowBottom);
        Assert.Equal(15m, down.Body);
    }

    [Fact]
    public void SetCandleFromString_ShouldFallbackVolumeAndIgnoreInvalidOpenInterest()
    {
        Candle candle = new Candle();

        candle.SetCandleFromString("20240102,153045,10,12,9,11,not_a_number,not_a_number");

        Assert.Equal(new DateTime(2024, 1, 2, 15, 30, 45), candle.TimeStart);
        Assert.Equal(10m, candle.Open);
        Assert.Equal(12m, candle.High);
        Assert.Equal(9m, candle.Low);
        Assert.Equal(11m, candle.Close);
        Assert.Equal(0m, candle.Volume);
        Assert.Equal(0m, candle.OpenInterest);
    }

    [Fact]
    public void StringToSave_ShouldUpdate_WhenCloseChanges()
    {
        Candle candle = new Candle
        {
            TimeStart = new DateTime(2024, 1, 2, 15, 30, 45),
            Open = 10m,
            High = 12m,
            Low = 9m,
            Close = 11m,
            Volume = 5m,
            OpenInterest = 7m
        };

        string first = candle.StringToSave;
        string second = candle.StringToSave;
        Assert.Equal(first, second);
        Assert.Contains(",11,", first, StringComparison.Ordinal);

        candle.Close = 13m;
        string afterChange = candle.StringToSave;

        Assert.NotEqual(first, afterChange);
        Assert.Contains(",13,", afterChange, StringComparison.Ordinal);
    }
}
