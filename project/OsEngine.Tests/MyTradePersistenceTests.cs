#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class MyTradePersistenceTests
{
    [Fact]
    public void GetStringFofSave_SetTradeFromString_ShouldRoundTripWithEscapedSecurityName()
    {
        MyTrade source = new MyTrade
        {
            Volume = 1.75m,
            Price = 2450.5m,
            NumberOrderParent = "ord-99",
            Time = new DateTime(2026, 2, 26, 11, 12, 13, DateTimeKind.Utc),
            NumberTrade = "tr-77",
            Side = Side.Sell,
            SecurityNameCode = "TEST@MARKET",
            NumberPosition = "pos-11"
        };

        string saved = source.GetStringFofSave();

        MyTrade loaded = new MyTrade();
        loaded.SetTradeFromString(saved);

        Assert.Equal(source.Volume, loaded.Volume);
        Assert.Equal(source.Price, loaded.Price);
        Assert.Equal(source.NumberOrderParent, loaded.NumberOrderParent);
        Assert.Equal(source.NumberTrade, loaded.NumberTrade);
        Assert.Equal(source.Side, loaded.Side);
        Assert.Equal(source.SecurityNameCode, loaded.SecurityNameCode);
        Assert.Equal(source.NumberPosition, loaded.NumberPosition);
        Assert.Equal(source.Time, loaded.Time);
    }

    [Fact]
    public void SetTradeFromString_ShouldParseIsoAndLegacyRuDates()
    {
        MyTrade isoTrade = new MyTrade();
        isoTrade.SetTradeFromString("1&2&ord&2026-02-26T11:12:13.0000000Z&tr&Buy&SEC&1");
        Assert.Equal(new DateTime(2026, 2, 26, 11, 12, 13, DateTimeKind.Utc), isoTrade.Time);

        MyTrade ruTrade = new MyTrade();
        ruTrade.SetTradeFromString("1&2&ord&26.02.2026 11:12:13&tr&Buy&SEC&1");
        Assert.Equal(new DateTime(2026, 2, 26, 11, 12, 13), ruTrade.Time);
    }

    [Fact]
    public void SetTradeFromString_ShouldSupportLegacyPayloadWithEmptyPosition()
    {
        MyTrade trade = new MyTrade();
        trade.SetTradeFromString("1.5&2.5&ord&2026-02-26T11:12:13.0000000Z&tr&Sell&SEC&");

        Assert.Equal(1.5m, trade.Volume);
        Assert.Equal(2.5m, trade.Price);
        Assert.Equal("ord", trade.NumberOrderParent);
        Assert.Equal(new DateTime(2026, 2, 26, 11, 12, 13, DateTimeKind.Utc), trade.Time);
        Assert.Equal("tr", trade.NumberTrade);
        Assert.Equal(Side.Sell, trade.Side);
        Assert.Equal("SEC", trade.SecurityNameCode);
        Assert.Equal(string.Empty, trade.NumberPosition);
    }
}
