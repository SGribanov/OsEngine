#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Globalization;
using OsEngine.Entity;
using OsEngine.Market;
using Xunit;

namespace OsEngine.Tests;

public class OrderPersistenceTests
{
    [Fact]
    public void SetOrderFromString_RoundTripWithTradesAndCancelInfo_ShouldRestoreFields()
    {
        Order source = new Order
        {
            NumberUser = 42,
            ServerType = ServerType.Binance,
            NumberMarket = "ord-1001",
            Side = Side.Buy,
            Price = 123.45m,
            Volume = 2.5m,
            VolumeExecute = 1.25m,
            State = OrderStateType.Partial,
            TypeOrder = OrderPriceType.Limit,
            SecurityNameCode = "BTCUSDT",
            PortfolioNumber = "main",
            TimeCreate = new DateTime(2026, 2, 26, 10, 0, 0),
            TimeCallBack = new DateTime(2026, 2, 26, 10, 0, 1),
            TimeCancel = new DateTime(2026, 2, 26, 10, 3, 0),
            TimeDone = new DateTime(2026, 2, 26, 10, 5, 0),
            LifeTime = TimeSpan.FromMinutes(15),
            Comment = "test-order",
            OrderTypeTime = OrderTypeTime.Day,
            ServerName = "binance-main",
            IsSendToCancel = true,
            CancellingTryCount = 3,
            LastCancelTryLocalTime = new DateTime(2026, 2, 26, 10, 4, 0, DateTimeKind.Utc)
        };

        source.SetTrade(new MyTrade
        {
            NumberOrderParent = source.NumberMarket,
            NumberTrade = "trade-1",
            SecurityNameCode = source.SecurityNameCode,
            Side = source.Side,
            Price = 123.45m,
            Volume = 1.25m,
            Time = new DateTime(2026, 2, 26, 10, 0, 2)
        });

        string save = source.GetStringForSave().ToString();

        Order loaded = new Order();
        loaded.SetOrderFromString(save);

        Assert.Equal(source.NumberUser, loaded.NumberUser);
        Assert.Equal(source.NumberMarket, loaded.NumberMarket);
        Assert.Equal(source.SecurityNameCode, loaded.SecurityNameCode);
        Assert.Equal(source.LifeTime, loaded.LifeTime);
        Assert.Equal(source.CancellingTryCount, loaded.CancellingTryCount);
        Assert.Equal(source.IsSendToCancel, loaded.IsSendToCancel);
        Assert.Equal(source.LastCancelTryLocalTime, loaded.LastCancelTryLocalTime);
        Assert.NotNull(loaded.MyTrades);
        Assert.Single(loaded.MyTrades);
        Assert.Equal("trade-1", loaded.MyTrades[0].NumberTrade);
    }

    [Fact]
    public void SetOrderFromString_ShouldParseLegacyRuDateFormat()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            Order source = new Order
            {
                NumberUser = 1,
                ServerType = ServerType.None,
                NumberMarket = "legacy",
                Side = Side.Sell,
                Price = 10m,
                Volume = 1m,
                VolumeExecute = 0m,
                State = OrderStateType.Active,
                TypeOrder = OrderPriceType.Limit,
                SecurityNameCode = "SEC",
                PortfolioNumber = "pf",
                LifeTime = TimeSpan.FromMinutes(1),
                Comment = "legacy",
                OrderTypeTime = OrderTypeTime.Specified,
                ServerName = "srv",
                IsSendToCancel = true,
                CancellingTryCount = 2,
                LastCancelTryLocalTime = DateTime.MinValue
            };

            string[] fields = source.GetStringForSave().ToString().Split('@');

            fields[10] = "15.02.2026 13:45:10";
            fields[13] = "15.02.2026 13:40:10";
            fields[14] = "15.02.2026 13:46:10";
            fields[15] = "15.02.2026 13:45:10";
            fields[19] = "15.02.2026 13:50:10";
            fields[22] = "True&2&15.02.2026 13:51:10";

            string legacySave = string.Join("@", fields);

            Order loaded = new Order();
            loaded.SetOrderFromString(legacySave);

            Assert.Equal(new DateTime(2026, 2, 15, 13, 45, 10), loaded.TimeCallBack);
            Assert.Equal(new DateTime(2026, 2, 15, 13, 40, 10), loaded.TimeCreate);
            Assert.Equal(new DateTime(2026, 2, 15, 13, 46, 10), loaded.TimeCancel);
            Assert.Equal(new DateTime(2026, 2, 15, 13, 50, 10), loaded.TimeDone);
            Assert.Equal(new DateTime(2026, 2, 15, 13, 51, 10), loaded.LastCancelTryLocalTime);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void SetOrderFromString_ShouldSupportLegacyPayloadWithoutOptionalTailFields()
    {
        Order source = new Order
        {
            NumberUser = 7,
            ServerType = ServerType.None,
            NumberMarket = "legacy-short",
            Side = Side.Buy,
            Price = 100m,
            Volume = 2m,
            VolumeExecute = 0m,
            State = OrderStateType.Active,
            TypeOrder = OrderPriceType.Limit,
            SecurityNameCode = "SEC",
            PortfolioNumber = "PF",
            TimeCallBack = new DateTime(2026, 2, 27, 10, 10, 10),
            TimeCreate = new DateTime(2026, 2, 27, 10, 10, 0),
            TimeCancel = DateTime.MinValue,
            LifeTime = TimeSpan.FromMinutes(3),
            Comment = "legacy-short",
            TimeDone = DateTime.MinValue,
            OrderTypeTime = OrderTypeTime.Specified,
            ServerName = "server-x",
            IsSendToCancel = true,
            CancellingTryCount = 5,
            LastCancelTryLocalTime = new DateTime(2026, 2, 27, 10, 11, 0)
        };

        string full = source.GetStringForSave().ToString();
        string shortLegacy = string.Join("@", full.Split('@')[..20]);

        Order loaded = new Order();
        loaded.SetOrderFromString(shortLegacy);

        Assert.Equal(source.NumberUser, loaded.NumberUser);
        Assert.Equal(source.NumberMarket, loaded.NumberMarket);
        Assert.Equal(OrderTypeTime.Specified, loaded.OrderTypeTime);
        Assert.Equal(string.Empty, loaded.ServerName);
        Assert.False(loaded.IsSendToCancel);
        Assert.Equal(0, loaded.CancellingTryCount);
        Assert.Equal(DateTime.MinValue, loaded.LastCancelTryLocalTime);
    }
}
