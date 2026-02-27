#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Globalization;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class SecurityCoreTests
{
    [Fact]
    public void GetSaveStr_LoadFromString_ShouldRoundTripCoreFields()
    {
        Security source = new Security
        {
            Name = "SBER",
            NameClass = "TQBR",
            NameFull = "Sberbank",
            NameId = "id-123",
            State = SecurityStateType.Activ,
            PriceStep = 0.01m,
            Lot = 10m,
            PriceStepCost = 0.01m,
            MarginBuy = 1000m,
            SecurityType = SecurityType.Stock,
            Decimals = 2,
            PriceLimitLow = 90m,
            PriceLimitHigh = 150m,
            OptionType = OptionType.None,
            Strike = 0m,
            Expiration = new DateTime(2026, 12, 31, 18, 0, 0, DateTimeKind.Utc),
            DecimalsVolume = 3,
            MinTradeAmount = 1.25m,
            VolumeStep = 0.01m,
            MinTradeAmountType = MinTradeAmountType.Contract,
            MarginSell = 1200m
        };

        string save = source.GetSaveStr();

        Security loaded = new Security();
        loaded.LoadFromString(save);

        Assert.Equal(source.Name, loaded.Name);
        Assert.Equal(source.NameClass, loaded.NameClass);
        Assert.Equal(source.NameFull, loaded.NameFull);
        Assert.Equal(source.NameId, loaded.NameId);
        Assert.Equal(source.State, loaded.State);
        Assert.Equal(source.PriceStep, loaded.PriceStep);
        Assert.Equal(source.Lot, loaded.Lot);
        Assert.Equal(source.PriceStepCost, loaded.PriceStepCost);
        Assert.Equal(source.MarginBuy, loaded.MarginBuy);
        Assert.Equal(source.SecurityType, loaded.SecurityType);
        Assert.Equal(source.Decimals, loaded.Decimals);
        Assert.Equal(source.PriceLimitLow, loaded.PriceLimitLow);
        Assert.Equal(source.PriceLimitHigh, loaded.PriceLimitHigh);
        Assert.Equal(source.OptionType, loaded.OptionType);
        Assert.Equal(source.Strike, loaded.Strike);
        Assert.Equal(source.Expiration, loaded.Expiration);
        Assert.Equal(source.DecimalsVolume, loaded.DecimalsVolume);
        Assert.Equal(source.MinTradeAmount, loaded.MinTradeAmount);
        Assert.Equal(source.VolumeStep, loaded.VolumeStep);
        Assert.Equal(source.MinTradeAmountType, loaded.MinTradeAmountType);
        Assert.Equal(source.MarginSell, loaded.MarginSell);
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
                "GAZP\nTQBR\nGazprom\nid-987\nActiv\n0.01\n10\n0.01\n500\nStock\n2\n100.25\n220.75\nCall\n150\n27.02.2026 15:30:45\n3\n1.5\n0.01\nContract\n600";

            Security loaded = new Security();
            loaded.LoadFromString(save);

            Assert.Equal(0.01m, loaded.PriceStep);
            Assert.Equal(100.25m, loaded.PriceLimitLow);
            Assert.Equal(220.75m, loaded.PriceLimitHigh);
            Assert.Equal(1.5m, loaded.MinTradeAmount);
            Assert.Equal(new DateTime(2026, 2, 27, 15, 30, 45), loaded.Expiration);
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
            "LKOH\nTQBR\nLukoil\nid-555\nActiv\n0.05\n1\n0.05\n700\nStock\n2\n5000\n9000\nNone\n0\n2026-02-27T15:30:45.0000000Z\n0\n1";

        Security loaded = new Security();
        loaded.LoadFromString(legacyShort);

        Assert.Equal("LKOH", loaded.Name);
        Assert.Equal(0.05m, loaded.PriceStep);
        Assert.Equal(1m, loaded.Lot);
        Assert.Equal(700m, loaded.MarginBuy);
        Assert.Equal(1m, loaded.MinTradeAmount);
        Assert.Equal(0m, loaded.VolumeStep);
        Assert.Equal(MinTradeAmountType.Contract, loaded.MinTradeAmountType);
        Assert.Equal(0m, loaded.MarginSell);
    }
}
