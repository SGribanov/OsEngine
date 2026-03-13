#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class NonTradePeriodsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistToml_AndLoadRoundTrip()
    {
        string name = "codex_nontrade_json_" + Guid.NewGuid().ToString("N");

        using StructuredSettingsFileScope scope = new StructuredSettingsFileScope(Path.Combine("Engine", name + "nonTradePeriod.toml"));

        NonTradePeriods source = new NonTradePeriods(name);
        source.TradeInMonday = false;
        source.TradeInSunday = false;
        source.NonTradePeriodGeneral.NonTradePeriod1OnOff = true;
        source.NonTradePeriodGeneral.NonTradePeriod1Start.Hour = 1;
        source.NonTradePeriodGeneral.NonTradePeriod1Start.Minute = 2;
        source.NonTradePeriodGeneral.NonTradePeriod1End.Hour = 3;
        source.NonTradePeriodGeneral.NonTradePeriod1End.Minute = 4;
        source.Save();

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("DaysLine =", content);

        NonTradePeriods loaded = new NonTradePeriods(name);
        Assert.False(loaded.TradeInMonday);
        Assert.False(loaded.TradeInSunday);
        Assert.True(loaded.NonTradePeriodGeneral.NonTradePeriod1OnOff);
        Assert.Equal(1, loaded.NonTradePeriodGeneral.NonTradePeriod1Start.Hour);
        Assert.Equal(2, loaded.NonTradePeriodGeneral.NonTradePeriod1Start.Minute);
        Assert.Equal(3, loaded.NonTradePeriodGeneral.NonTradePeriod1End.Hour);
        Assert.Equal(4, loaded.NonTradePeriodGeneral.NonTradePeriod1End.Minute);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_AndSaveToml()
    {
        string name = "codex_nontrade_legacy_" + Guid.NewGuid().ToString("N");

        using StructuredSettingsFileScope scope = new StructuredSettingsFileScope(Path.Combine("Engine", name + "nonTradePeriod.toml"));

        NonTradePeriods legacySource = new NonTradePeriods(name);
        legacySource.TradeInTuesday = false;
        legacySource.NonTradePeriodFriday.NonTradePeriod2OnOff = true;
        legacySource.NonTradePeriodFriday.NonTradePeriod2Start.Hour = 11;
        legacySource.NonTradePeriodFriday.NonTradePeriod2Start.Minute = 15;

        File.WriteAllLines(scope.LegacyTxtPath, legacySource.GetFullSaveArray());

        NonTradePeriods loaded = new NonTradePeriods(name);
        Assert.False(loaded.TradeInTuesday);
        Assert.True(loaded.NonTradePeriodFriday.NonTradePeriod2OnOff);
        Assert.Equal(11, loaded.NonTradePeriodFriday.NonTradePeriod2Start.Hour);
        Assert.Equal(15, loaded.NonTradePeriodFriday.NonTradePeriod2Start.Minute);

        loaded.Save();
        Assert.True(File.Exists(scope.CanonicalPath));
        Assert.Contains("FridayLine =", File.ReadAllText(scope.CanonicalPath));
    }
}
