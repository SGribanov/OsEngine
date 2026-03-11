#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Globalization;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class MarketDepthCoreTests
{
    [Fact]
    public void GetSaveStringToAllDepfh_AndSetMarketDepthFromString_ShouldRoundTripCoreFields()
    {
        MarketDepth source = new MarketDepth
        {
            Time = new DateTime(2026, 2, 27, 14, 15, 16, 123)
        };

        source.Asks.Add(new MarketDepthLevel { Ask = 2.5, Price = 101.25 });
        source.Asks.Add(new MarketDepthLevel { Ask = 1.0, Price = 101.5 });
        source.Bids.Add(new MarketDepthLevel { Bid = 3.5, Price = 101.0 });
        source.Bids.Add(new MarketDepthLevel { Bid = 2.0, Price = 100.75 });

        string save = source.GetSaveStringToAllDepfh(10);

        MarketDepth loaded = new MarketDepth();
        loaded.SetMarketDepthFromString(save);

        Assert.Equal(source.Time, loaded.Time);
        Assert.Equal(2, loaded.Asks.Count);
        Assert.Equal(2, loaded.Bids.Count);
        Assert.Equal(source.Asks[0].Ask, loaded.Asks[0].Ask);
        Assert.Equal(source.Asks[0].Price, loaded.Asks[0].Price);
        Assert.Equal(source.Bids[0].Bid, loaded.Bids[0].Bid);
        Assert.Equal(source.Bids[0].Price, loaded.Bids[0].Price);
    }

    [Fact]
    public void GetSaveStringToAllDepfh_ShouldUseInvariantDecimalSeparator()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
            CultureInfo.CurrentUICulture = new CultureInfo("ru-RU");

            MarketDepth depth = new MarketDepth
            {
                Time = new DateTime(2026, 2, 27, 14, 15, 16, 500)
            };

            depth.Asks.Add(new MarketDepthLevel { Ask = 2.5, Price = 101.25 });
            depth.Bids.Add(new MarketDepthLevel { Bid = 3.75, Price = 101.0 });

            string save = depth.GetSaveStringToAllDepfh(1);

            Assert.Contains("2.5&101.25", save, StringComparison.Ordinal);
            Assert.Contains("3.75&101", save, StringComparison.Ordinal);
            Assert.DoesNotContain("2,5&101,25", save, StringComparison.Ordinal);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void GetSaveStringToAllDepfh_DepthZero_ShouldPersistSingleLevelAndRoundTrip()
    {
        MarketDepth source = new MarketDepth
        {
            Time = new DateTime(2026, 2, 27, 19, 0, 1, 250)
        };

        source.Asks.Add(new MarketDepthLevel { Ask = 5.0, Price = 101.1 });
        source.Asks.Add(new MarketDepthLevel { Ask = 6.0, Price = 101.2 });
        source.Bids.Add(new MarketDepthLevel { Bid = 4.0, Price = 100.9 });
        source.Bids.Add(new MarketDepthLevel { Bid = 3.0, Price = 100.8 });

        string save = source.GetSaveStringToAllDepfh(0);

        MarketDepth loaded = new MarketDepth();
        loaded.SetMarketDepthFromString(save);

        Assert.Single(loaded.Asks);
        Assert.Single(loaded.Bids);
        Assert.Equal(101.1, loaded.Asks[0].Price);
        Assert.Equal(100.9, loaded.Bids[0].Price);
    }

    [Fact]
    public void GetSaveStringToAllDepfh_EmptyLevels_ShouldRoundTripWithoutErrors()
    {
        MarketDepth source = new MarketDepth
        {
            Time = new DateTime(2026, 2, 27, 19, 30, 1, 125)
        };

        string save = source.GetSaveStringToAllDepfh(10);

        MarketDepth loaded = new MarketDepth();
        loaded.SetMarketDepthFromString(save);

        Assert.Empty(loaded.Asks);
        Assert.Empty(loaded.Bids);
        Assert.Equal(source.Time, loaded.Time);
    }

    [Fact]
    public void SetMarketDepthFromString_LegacyCommaDecimals_ShouldPreserveFractionalValues()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");

            string legacy = "20260227_141516_250_2,5&101,25*1,0&101,5*_3,75&101,0*";

            MarketDepth loaded = new MarketDepth();
            loaded.SetMarketDepthFromString(legacy);

            Assert.Equal(new DateTime(2026, 2, 27, 14, 15, 16, 250), loaded.Time);
            Assert.Equal(2, loaded.Asks.Count);
            Assert.Single(loaded.Bids);
            Assert.Equal(2.5, loaded.Asks[0].Ask);
            Assert.Equal(101.25, loaded.Asks[0].Price);
            Assert.Equal(3.75, loaded.Bids[0].Bid);
            Assert.Equal(101.0, loaded.Bids[0].Price);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void SetMarketDepthFromString_LegacyOnlyAsksSection_ShouldLoadAndKeepBidsEmpty()
    {
        string legacy = "20260227_095001_7_5&101.1*_";

        MarketDepth loaded = new MarketDepth();
        loaded.SetMarketDepthFromString(legacy);

        Assert.Equal(new DateTime(2026, 2, 27, 9, 50, 1, 7), loaded.Time);
        Assert.Single(loaded.Asks);
        Assert.Empty(loaded.Bids);
        Assert.Equal(5.0, loaded.Asks[0].Ask);
        Assert.Equal(101.1, loaded.Asks[0].Price);
    }

    [Fact]
    public void SetMarketDepthFromString_LegacyOnlyBidsSection_ShouldLoadAndKeepAsksEmpty()
    {
        string legacy = "20260227_095001_8__4&100.9*";

        MarketDepth loaded = new MarketDepth();
        loaded.SetMarketDepthFromString(legacy);

        Assert.Equal(new DateTime(2026, 2, 27, 9, 50, 1, 8), loaded.Time);
        Assert.Empty(loaded.Asks);
        Assert.Single(loaded.Bids);
        Assert.Equal(4.0, loaded.Bids[0].Bid);
        Assert.Equal(100.9, loaded.Bids[0].Price);
    }
}
