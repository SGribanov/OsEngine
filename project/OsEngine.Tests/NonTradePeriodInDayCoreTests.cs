#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class NonTradePeriodInDayCoreTests
{
    [Fact]
    public void GetSaveString_LoadFromString_ShouldRoundTripAllPeriods()
    {
        NonTradePeriodInDay source = new NonTradePeriodInDay
        {
            NonTradePeriod1OnOff = true,
            NonTradePeriod2OnOff = false,
            NonTradePeriod3OnOff = true,
            NonTradePeriod4OnOff = true,
            NonTradePeriod5OnOff = false
        };

        source.NonTradePeriod1Start.LoadFromString("1:2:3:4");
        source.NonTradePeriod1End.LoadFromString("5:6:7:8");
        source.NonTradePeriod2Start.LoadFromString("9:10:11:12");
        source.NonTradePeriod2End.LoadFromString("13:14:15:16");
        source.NonTradePeriod3Start.LoadFromString("17:18:19:20");
        source.NonTradePeriod3End.LoadFromString("21:22:23:24");
        source.NonTradePeriod4Start.LoadFromString("2:3:4:5");
        source.NonTradePeriod4End.LoadFromString("6:7:8:9");
        source.NonTradePeriod5Start.LoadFromString("10:11:12:13");
        source.NonTradePeriod5End.LoadFromString("14:15:16:17");

        string save = source.GetSaveString();

        NonTradePeriodInDay loaded = new NonTradePeriodInDay();
        loaded.LoadFromString(save);

        Assert.Equal(source.NonTradePeriod1OnOff, loaded.NonTradePeriod1OnOff);
        Assert.Equal(source.NonTradePeriod2OnOff, loaded.NonTradePeriod2OnOff);
        Assert.Equal(source.NonTradePeriod3OnOff, loaded.NonTradePeriod3OnOff);
        Assert.Equal(source.NonTradePeriod4OnOff, loaded.NonTradePeriod4OnOff);
        Assert.Equal(source.NonTradePeriod5OnOff, loaded.NonTradePeriod5OnOff);

        Assert.Equal("1:2:3:4", loaded.NonTradePeriod1Start.ToString());
        Assert.Equal("5:6:7:8", loaded.NonTradePeriod1End.ToString());
        Assert.Equal("9:10:11:12", loaded.NonTradePeriod2Start.ToString());
        Assert.Equal("13:14:15:16", loaded.NonTradePeriod2End.ToString());
        Assert.Equal("17:18:19:20", loaded.NonTradePeriod3Start.ToString());
        Assert.Equal("21:22:23:24", loaded.NonTradePeriod3End.ToString());
        Assert.Equal("2:3:4:5", loaded.NonTradePeriod4Start.ToString());
        Assert.Equal("6:7:8:9", loaded.NonTradePeriod4End.ToString());
        Assert.Equal("10:11:12:13", loaded.NonTradePeriod5Start.ToString());
        Assert.Equal("14:15:16:17", loaded.NonTradePeriod5End.ToString());
    }

    [Fact]
    public void LoadFromString_ShouldIgnoreReservedTailFields()
    {
        NonTradePeriodInDay loaded = new NonTradePeriodInDay();

        string legacy =
            "True@1:0:0:0@2:0:0:0@False@3:0:0:0@4:0:0:0@True@5:0:0:0@6:0:0:0@False@7:0:0:0@8:0:0:0@True@9:0:0:0@10:0:0:0@reserved1@reserved2@reserved3@reserved4@reserved5";

        loaded.LoadFromString(legacy);

        Assert.True(loaded.NonTradePeriod1OnOff);
        Assert.False(loaded.NonTradePeriod2OnOff);
        Assert.True(loaded.NonTradePeriod3OnOff);
        Assert.False(loaded.NonTradePeriod4OnOff);
        Assert.True(loaded.NonTradePeriod5OnOff);
        Assert.Equal("9:0:0:0", loaded.NonTradePeriod5Start.ToString());
        Assert.Equal("10:0:0:0", loaded.NonTradePeriod5End.ToString());
    }

    [Fact]
    public void LoadFromString_MalformedPayload_ShouldNotThrowAndKeepDefaults()
    {
        NonTradePeriodInDay loaded = new NonTradePeriodInDay();

        Exception? error = Record.Exception(() => loaded.LoadFromString("bad"));

        Assert.Null(error);
        Assert.False(loaded.NonTradePeriod1OnOff);
        Assert.Equal("0:0:0:0", loaded.NonTradePeriod1Start.ToString());
        Assert.Equal("7:0:0:0", loaded.NonTradePeriod1End.ToString());
    }
}
