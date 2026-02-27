#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Globalization;
using System.Linq;
using OsEngine.Entity;
using OsEngine.Market;
using Xunit;

namespace OsEngine.Tests;

public class PositionPersistenceTests
{
    [Fact]
    public void SetDealFromString_RoundTripWithOrders_ShouldRestoreCoreFields()
    {
        Position source = new Position
        {
            Direction = Side.Buy,
            State = PositionStateType.Open,
            NameBot = "bot-1",
            ProfitOperationPercent = 1.5m,
            ProfitOperationAbs = 15m,
            Number = 77,
            Comment = "comment",
            SignalTypeStop = "stop-signal",
            SignalTypeProfit = "profit-signal",
            StopOrderIsActive = true,
            StopOrderPrice = 100.25m,
            StopOrderRedLine = 99.75m,
            ProfitOrderIsActive = true,
            ProfitOrderPrice = 110.5m,
            Lots = 2m,
            MarginBuy = 1.1m,
            MarginSell = 1.2m,
            PriceStepCost = 0.01m,
            PriceStep = 0.01m,
            PortfolioValueOnOpenPosition = 10000m,
            ProfitOrderRedLine = 109.5m,
            SignalTypeOpen = "open-signal",
            SignalTypeClose = "close-signal",
            CommissionValue = 0.15m,
            CommissionType = CommissionType.Percent,
            StopIsMarket = true,
            ProfitIsMarket = false,
            SecurityName = "SBER"
        };

        source.AddNewOpenOrder(CreateOrder(
            marketId: "open-1",
            side: Side.Buy,
            timeCreate: new DateTime(2026, 2, 26, 10, 0, 0),
            tradeId: "t-open"));

        source.AddNewCloseOrder(CreateOrder(
            marketId: "close-1",
            side: Side.Sell,
            timeCreate: new DateTime(2026, 2, 26, 10, 5, 0),
            tradeId: "t-close"));

        string save = source.GetStringForSave().ToString();

        Position loaded = new Position();
        loaded.SetDealFromString(save);

        Assert.Equal(source.Direction, loaded.Direction);
        Assert.Equal(source.State, loaded.State);
        Assert.Equal(source.NameBot, loaded.NameBot);
        Assert.Equal(source.Number, loaded.Number);
        Assert.Equal(source.CommissionType, loaded.CommissionType);
        Assert.Equal(source.CommissionValue, loaded.CommissionValue);
        Assert.Equal(source.StopIsMarket, loaded.StopIsMarket);
        Assert.Equal(source.ProfitIsMarket, loaded.ProfitIsMarket);
        Assert.Equal(source.SecurityName, loaded.SecurityName);
        Assert.NotNull(loaded.OpenOrders);
        Assert.NotNull(loaded.CloseOrders);
        Assert.Single(loaded.OpenOrders);
        Assert.Single(loaded.CloseOrders);
        Assert.Equal("open-1", loaded.OpenOrders[0].NumberMarket);
        Assert.Equal("close-1", loaded.CloseOrders[0].NumberMarket);
    }

    [Fact]
    public void SetDealFromString_ShouldParseLegacyRuOrderDateInsidePosition()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            Position source = new Position
            {
                Direction = Side.Buy,
                State = PositionStateType.Open,
                NameBot = "bot-legacy",
                Number = 10,
                SecurityName = "SBER"
            };

            source.AddNewOpenOrder(CreateOrder(
                marketId: "legacy-order",
                side: Side.Buy,
                timeCreate: new DateTime(2026, 2, 26, 11, 0, 0),
                tradeId: "legacy-trade"));

            string[] positionFields = source.GetStringForSave().ToString().Split('#');
            string openOrder = positionFields[5].TrimEnd('^');
            string[] orderFields = openOrder.Split('@');

            orderFields[10] = "15.02.2026 13:45:10";
            orderFields[13] = "15.02.2026 13:40:10";
            orderFields[14] = "15.02.2026 13:46:10";
            orderFields[15] = "15.02.2026 13:50:10";
            orderFields[19] = "15.02.2026 13:55:10";
            orderFields[22] = "False&0&15.02.2026 13:56:10";

            positionFields[5] = string.Join("@", orderFields) + "^";
            string legacySave = string.Join("#", positionFields);

            Position loaded = new Position();
            loaded.SetDealFromString(legacySave);

            Assert.NotNull(loaded.OpenOrders);
            Assert.Single(loaded.OpenOrders);
            Assert.Equal(new DateTime(2026, 2, 15, 13, 40, 10), loaded.OpenOrders[0].TimeCreate);
            Assert.Equal(new DateTime(2026, 2, 15, 13, 50, 10), loaded.OpenOrders[0].TimeCallBack);
            Assert.Equal(new DateTime(2026, 2, 15, 13, 46, 10), loaded.OpenOrders[0].TimeCancel);
            Assert.Equal(new DateTime(2026, 2, 15, 13, 55, 10), loaded.OpenOrders[0].TimeDone);
            Assert.Equal(new DateTime(2026, 2, 15, 13, 56, 10), loaded.OpenOrders[0].LastCancelTryLocalTime);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void SetDealFromString_ShouldSupportLegacyPayloadWithoutMarketFlagFields()
    {
        Position source = new Position
        {
            Direction = Side.Sell,
            State = PositionStateType.Open,
            NameBot = "bot-no-flags",
            Number = 55,
            StopIsMarket = true,
            ProfitIsMarket = true,
            SecurityName = "GAZP"
        };

        string[] fields = source.GetStringForSave().ToString().Split('#');
        string legacyShort = string.Join("#", fields[..^3].Concat(new[] { fields[^1] }));

        Position loaded = new Position();
        loaded.SetDealFromString(legacyShort);

        Assert.Equal(source.Direction, loaded.Direction);
        Assert.Equal(source.Number, loaded.Number);
        Assert.Equal("GAZP", loaded.SecurityName);
        Assert.False(loaded.StopIsMarket);
        Assert.False(loaded.ProfitIsMarket);
    }

    [Fact]
    public void SetDealFromString_ShouldParseLowercaseMarketFlags()
    {
        Position source = new Position
        {
            Direction = Side.Buy,
            State = PositionStateType.Open,
            NameBot = "bot-lower-flags",
            Number = 56,
            StopIsMarket = false,
            ProfitIsMarket = true,
            SecurityName = "ROSN"
        };

        string[] fields = source.GetStringForSave().ToString().Split('#');
        fields[^3] = "true";
        fields[^2] = "false";
        string payload = string.Join("#", fields);

        Position loaded = new Position();
        loaded.SetDealFromString(payload);

        Assert.True(loaded.StopIsMarket);
        Assert.False(loaded.ProfitIsMarket);
    }

    [Fact]
    public void SetDealFromString_ShouldIgnoreInvalidMarketFlags()
    {
        Position source = new Position
        {
            Direction = Side.Buy,
            State = PositionStateType.Open,
            NameBot = "bot-invalid-flags",
            Number = 57,
            StopIsMarket = true,
            ProfitIsMarket = true,
            SecurityName = "LKOH"
        };

        string[] fields = source.GetStringForSave().ToString().Split('#');
        fields[^3] = "yes";
        fields[^2] = "no";
        string payload = string.Join("#", fields);

        Position loaded = new Position();
        loaded.SetDealFromString(payload);

        Assert.False(loaded.StopIsMarket);
        Assert.False(loaded.ProfitIsMarket);
        Assert.Equal("LKOH", loaded.SecurityName);
    }

    [Fact]
    public void SetDealFromString_ShouldSupportLegacyLotsWithoutMarginValues()
    {
        Position source = new Position
        {
            Direction = Side.Buy,
            State = PositionStateType.Open,
            NameBot = "bot-legacy-lots",
            Number = 58,
            SecurityName = "SNGS"
        };

        string[] fields = source.GetStringForSave().ToString().Split('#');
        fields[13] = "3.25";
        string payload = string.Join("#", fields);

        Position loaded = new Position();
        loaded.SetDealFromString(payload);

        Assert.Equal(3.25m, loaded.Lots);
        Assert.Equal(0m, loaded.MarginBuy);
        Assert.Equal(0m, loaded.MarginSell);
        Assert.Equal("SNGS", loaded.SecurityName);
    }

    [Fact]
    public void SetDealFromString_ShouldSkipMalformedLegacyCloseOrderEntries()
    {
        Position source = new Position
        {
            Direction = Side.Sell,
            State = PositionStateType.Open,
            NameBot = "bot-close-legacy",
            Number = 59,
            SecurityName = "NVTK"
        };

        source.AddNewCloseOrder(CreateOrder(
            marketId: "close-valid",
            side: Side.Buy,
            timeCreate: new DateTime(2026, 2, 26, 12, 0, 0),
            tradeId: "close-trade"));

        string[] fields = source.GetStringForSave().ToString().Split('#');
        string[] head = fields[..^3];
        string[] tail = fields[^3..];
        string payload = string.Join("#", head.Concat(new[] { "broken-close-entry" }).Concat(tail));

        Position loaded = new Position();
        loaded.SetDealFromString(payload);

        Assert.NotNull(loaded.CloseOrders);
        Assert.Single(loaded.CloseOrders);
        Assert.Equal("close-valid", loaded.CloseOrders[0].NumberMarket);
        Assert.Equal("NVTK", loaded.SecurityName);
    }

    [Fact]
    public void SetDealFromString_ShouldIgnoreWhitespaceWrappedMarketFlags()
    {
        Position source = new Position
        {
            Direction = Side.Buy,
            State = PositionStateType.Open,
            NameBot = "bot-space-flags",
            Number = 60,
            StopIsMarket = true,
            ProfitIsMarket = true,
            SecurityName = "TATN"
        };

        string[] fields = source.GetStringForSave().ToString().Split('#');
        fields[^3] = " True ";
        fields[^2] = " false ";
        string payload = string.Join("#", fields);

        Position loaded = new Position();
        loaded.SetDealFromString(payload);

        Assert.False(loaded.StopIsMarket);
        Assert.False(loaded.ProfitIsMarket);
        Assert.Equal("TATN", loaded.SecurityName);
    }

    private static Order CreateOrder(
        string marketId,
        Side side,
        DateTime timeCreate,
        string tradeId)
    {
        Order order = new Order
        {
            NumberUser = 1,
            ServerType = ServerType.None,
            NumberMarket = marketId,
            Side = side,
            Price = 100m,
            Volume = 1m,
            VolumeExecute = 1m,
            State = OrderStateType.Done,
            TypeOrder = OrderPriceType.Limit,
            SecurityNameCode = "SBER",
            PortfolioNumber = "PF",
            TimeCallBack = timeCreate.AddSeconds(1),
            TimeCreate = timeCreate,
            TimeCancel = timeCreate.AddSeconds(2),
            TimeDone = timeCreate.AddSeconds(3),
            LifeTime = TimeSpan.FromMinutes(1),
            Comment = "order",
            OrderTypeTime = OrderTypeTime.Day,
            ServerName = "srv",
            IsSendToCancel = false,
            CancellingTryCount = 0,
            LastCancelTryLocalTime = timeCreate.AddSeconds(4)
        };

        order.SetTrade(new MyTrade
        {
            NumberOrderParent = marketId,
            NumberTrade = tradeId,
            SecurityNameCode = "SBER",
            Side = side,
            Price = 100m,
            Volume = 1m,
            Time = timeCreate.AddSeconds(5)
        });

        return order;
    }
}
