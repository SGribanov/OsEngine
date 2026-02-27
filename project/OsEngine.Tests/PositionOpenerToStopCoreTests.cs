#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Globalization;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class PositionOpenerToStopCoreTests
{
    [Fact]
    public void GetSaveString_LoadFromString_ShouldRoundTripCoreFields()
    {
        PositionOpenerToStopLimit source = new PositionOpenerToStopLimit
        {
            Security = "SBER",
            TabName = "tab-1",
            Number = 15,
            LifeTimeType = PositionOpenerToStopLifeTimeType.CandlesCount,
            PriceOrder = 123.45m,
            PriceRedLine = 122.95m,
            ActivateType = StopActivateType.HigherOrEqual,
            Volume = 2.5m,
            Side = Side.Buy,
            ExpiresBars = 12,
            OrderCreateBarNumber = 345,
            LastCandleTime = new DateTime(2026, 2, 27, 16, 10, 5, DateTimeKind.Utc),
            SignalType = "signal-A",
            TimeCreate = new DateTime(2026, 2, 27, 16, 0, 0, DateTimeKind.Utc),
            OrderPriceType = OrderPriceType.Limit,
            PositionNumber = 101
        };

        string save = source.GetSaveString();

        PositionOpenerToStopLimit loaded = new PositionOpenerToStopLimit();
        loaded.LoadFromString(save);

        Assert.Equal(source.Security, loaded.Security);
        Assert.Equal(source.TabName, loaded.TabName);
        Assert.Equal(source.Number, loaded.Number);
        Assert.Equal(source.LifeTimeType, loaded.LifeTimeType);
        Assert.Equal(source.PriceOrder, loaded.PriceOrder);
        Assert.Equal(source.PriceRedLine, loaded.PriceRedLine);
        Assert.Equal(source.ActivateType, loaded.ActivateType);
        Assert.Equal(source.Volume, loaded.Volume);
        Assert.Equal(source.Side, loaded.Side);
        Assert.Equal(source.ExpiresBars, loaded.ExpiresBars);
        Assert.Equal(source.OrderCreateBarNumber, loaded.OrderCreateBarNumber);
        Assert.Equal(source.LastCandleTime, loaded.LastCandleTime);
        Assert.Equal(source.SignalType, loaded.SignalType);
        Assert.Equal(source.TimeCreate, loaded.TimeCreate);
        Assert.Equal(source.OrderPriceType, loaded.OrderPriceType);
        Assert.Equal(source.PositionNumber, loaded.PositionNumber);
    }

    [Fact]
    public void LoadFromString_ShouldParseLegacyRuDateAndInvariantDecimals()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

            string save =
                "SBER&tab-2&16&NoLifeTime&100.75&99.25&LowerOrEqual&1.5&Sell&5&77&27.02.2026 15:30:45&signal-B&27.02.2026 15:25:40&Market&202";

            PositionOpenerToStopLimit loaded = new PositionOpenerToStopLimit();
            loaded.LoadFromString(save);

            Assert.Equal(100.75m, loaded.PriceOrder);
            Assert.Equal(99.25m, loaded.PriceRedLine);
            Assert.Equal(1.5m, loaded.Volume);
            Assert.Equal(new DateTime(2026, 2, 27, 15, 30, 45), loaded.LastCandleTime);
            Assert.Equal(new DateTime(2026, 2, 27, 15, 25, 40), loaded.TimeCreate);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void LoadFromString_ShouldSupportLegacyPayloadWithoutOptionalTailFields()
    {
        string legacyShort =
            "SBER&tab-legacy&17&NoLifeTime&99.5&98.5&LowerOrEqual&1.0&Sell&3&55&27.02.2026 15:30:45&signal-C&27.02.2026 15:25:40";

        PositionOpenerToStopLimit loaded = new PositionOpenerToStopLimit();
        loaded.LoadFromString(legacyShort);

        Assert.Equal("SBER", loaded.Security);
        Assert.Equal("tab-legacy", loaded.TabName);
        Assert.Equal(17, loaded.Number);
        Assert.Equal(99.5m, loaded.PriceOrder);
        Assert.Equal(98.5m, loaded.PriceRedLine);
        Assert.Equal(1.0m, loaded.Volume);
        Assert.Equal(StopActivateType.LowerOrEqual, loaded.ActivateType);
        Assert.Equal(new DateTime(2026, 2, 27, 15, 30, 45), loaded.LastCandleTime);
        Assert.Equal(new DateTime(2026, 2, 27, 15, 25, 40), loaded.TimeCreate);
        Assert.Equal(OrderPriceType.Limit, loaded.OrderPriceType);
        Assert.Equal(0, loaded.PositionNumber);
    }
}
