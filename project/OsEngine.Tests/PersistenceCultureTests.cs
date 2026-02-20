#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Globalization;
using OsEngine.Entity;
using OsEngine.Market;
using Xunit;

namespace OsEngine.Tests;

public class PersistenceCultureTests
{
    [Fact]
    public void Order_GetStringForSave_ShouldUseInvariantDecimalSeparator()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
            CultureInfo.CurrentUICulture = new CultureInfo("ru-RU");

            Order order = new Order
            {
                NumberUser = 1,
                ServerType = ServerType.None,
                NumberMarket = "42",
                Side = Side.Buy,
                Price = 1234.56m,
                Volume = 7.89m,
                VolumeExecute = 1.23m,
                State = OrderStateType.Active,
                TypeOrder = OrderPriceType.Limit,
                TimeCallBack = new DateTime(2026, 2, 15, 10, 20, 30),
                SecurityNameCode = "SEC",
                TimeCreate = new DateTime(2026, 2, 15, 10, 19, 30),
                TimeCancel = new DateTime(2026, 2, 15, 10, 21, 30),
                TimeDone = new DateTime(2026, 2, 15, 10, 22, 30)
            };

            string save = order.GetStringForSave().ToString();

            Assert.Contains("1234.56", save, StringComparison.Ordinal);
            Assert.DoesNotContain("1234,56", save, StringComparison.Ordinal);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void MyTrade_SetTradeFromString_ShouldParseLegacyRuDate()
    {
        MyTrade trade = new MyTrade();
        trade.SetTradeFromString("1.5&2.5&ord&15.02.2026 13:45:10&t1&Buy&BTCUSDT&7");

        Assert.Equal(new DateTime(2026, 2, 15, 13, 45, 10), trade.Time);
        Assert.Equal(1.5m, trade.Volume);
        Assert.Equal(2.5m, trade.Price);
    }

    [Fact]
    public void Position_GetStringForSave_ShouldUseInvariantDecimalSeparator()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
            CultureInfo.CurrentUICulture = new CultureInfo("ru-RU");

            Position position = new Position
            {
                Direction = Side.Buy,
                NameBot = "bot",
                ProfitOperationPercent = 1.25m,
                ProfitOperationAbs = 2.5m,
                StopOrderPrice = 100.5m,
                StopOrderRedLine = 99.5m,
                ProfitOrderPrice = 110.5m,
                Lots = 1.5m,
                MarginBuy = 2.5m,
                MarginSell = 3.5m,
                PriceStepCost = 0.1m,
                PriceStep = 0.01m,
                PortfolioValueOnOpenPosition = 10000.25m,
                ProfitOrderRedLine = 109.5m,
                CommissionValue = 0.25m
            };

            string save = position.GetStringForSave().ToString();

            Assert.Contains("1.25", save, StringComparison.Ordinal);
            Assert.Contains("10000.25", save, StringComparison.Ordinal);
            Assert.DoesNotContain("10000,25", save, StringComparison.Ordinal);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }
}
