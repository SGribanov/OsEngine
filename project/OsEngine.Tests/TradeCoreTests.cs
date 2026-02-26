using System;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class TradeCoreTests
{
    [Fact]
    public void GetSaveString_ShouldContainIdAndDepthBlock_WhenPresent()
    {
        Trade trade = new Trade
        {
            Time = new DateTime(2024, 1, 2, 15, 30, 45),
            Price = 123.45m,
            Volume = 2.5m,
            Side = Side.Buy,
            MicroSeconds = 321,
            Id = "abc",
            Bid = 123.4m,
            Ask = 123.5m,
            BidsVolume = 10m,
            AsksVolume = 11m
        };

        string save = trade.GetSaveString();

        Assert.Contains("20240102,153045", save, StringComparison.Ordinal);
        Assert.Contains(",123.45,2.5,Buy,321,abc,123.4,123.5,10,11", save, StringComparison.Ordinal);
    }

    [Fact]
    public void SetTradeFromString_ShouldParseStandardFormat()
    {
        Trade trade = new Trade();

        trade.SetTradeFromString("20240102,153045,100.5,7.25,Sell,999,id-42,100.4,100.6,12,13");

        Assert.Equal(new DateTime(2024, 1, 2, 15, 30, 45), trade.Time);
        Assert.Equal(100.5m, trade.Price);
        Assert.Equal(7.25m, trade.Volume);
        Assert.Equal(Side.Sell, trade.Side);
        Assert.Equal(999, trade.MicroSeconds);
        Assert.Equal("id-42", trade.Id);
        Assert.Equal(100.4m, trade.Bid);
        Assert.Equal(100.6m, trade.Ask);
        Assert.Equal(12m, trade.BidsVolume);
        Assert.Equal(13m, trade.AsksVolume);
    }

    [Fact]
    public void SetTradeFromString_ShouldParseIqFeedFormatAndInferBuySide()
    {
        Trade trade = new Trade();

        trade.SetTradeFromString("2024-01-02T15:30:45.0000000Z,100.5,7.25,100.5,100.6,C");

        Assert.Equal(new DateTime(2024, 1, 2, 15, 30, 45, DateTimeKind.Utc), trade.Time);
        Assert.Equal(100.5m, trade.Price);
        Assert.Equal(7.25m, trade.Volume);
        Assert.Equal(100.5m, trade.Bid);
        Assert.Equal(100.6m, trade.Ask);
        Assert.Equal(Side.Sell, trade.Side);
    }

    [Fact]
    public void SetTradeFromString_ShouldIgnoreEmptyInput()
    {
        Trade trade = new Trade
        {
            Time = new DateTime(2020, 1, 1)
        };

        trade.SetTradeFromString(" ");

        Assert.Equal(new DateTime(2020, 1, 1), trade.Time);
    }
}
