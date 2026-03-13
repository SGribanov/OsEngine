#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using OsEngine.Alerts;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class AlertToPricePersistenceTests
{
    [Fact]
    public void Save_ShouldPersistToml_AndLoadRoundTrip()
    {
        const string name = "CodexPriceAlertJson";
        using StructuredSettingsFileScope scope = new StructuredSettingsFileScope(Path.Combine("Engine", name + "Alert.toml"));

        AlertToPrice source = new AlertToPrice(name)
        {
            Message = "json-message",
            IsOn = true,
            MessageIsOn = true,
            MusicType = AlertMusic.Wolf,
            SignalType = SignalType.Buy,
            VolumeReaction = 2.5m,
            Slippage = 1.2m,
            NumberClosePosition = 3,
            OrderPriceType = OrderPriceType.Market,
            TypeActivation = PriceAlertTypeActivation.PriceLowerOrEqual,
            PriceActivation = 123.45m,
            SlippageType = AlertSlippageType.PriceStep
        };
        source.Save();

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("Message = \"json-message\"", content);

        AlertToPrice loaded = new AlertToPrice(name);
        Assert.Equal("json-message", loaded.Message);
        Assert.True(loaded.IsOn);
        Assert.True(loaded.MessageIsOn);
        Assert.Equal(AlertMusic.Wolf, loaded.MusicType);
        Assert.Equal(SignalType.Buy, loaded.SignalType);
        Assert.Equal(2.5m, loaded.VolumeReaction);
        Assert.Equal(1.2m, loaded.Slippage);
        Assert.Equal(3, loaded.NumberClosePosition);
        Assert.Equal(OrderPriceType.Market, loaded.OrderPriceType);
        Assert.Equal(PriceAlertTypeActivation.PriceLowerOrEqual, loaded.TypeActivation);
        Assert.Equal(123.45m, loaded.PriceActivation);
        Assert.Equal(AlertSlippageType.PriceStep, loaded.SlippageType);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat_AndSaveToml()
    {
        const string name = "CodexPriceAlertLegacy";
        using StructuredSettingsFileScope scope = new StructuredSettingsFileScope(Path.Combine("Engine", name + "Alert.toml"));

        File.WriteAllLines(scope.LegacyTxtPath, new[]
        {
            "legacy-message",
            "True",
            "False",
            "Duck",
            "Sell",
            "10.5",
            "0.5",
            "1",
            "Limit",
            "PriceHigherOrEqual",
            "999.99",
            "Persent"
        });

        AlertToPrice loaded = new AlertToPrice(name);
        Assert.Equal("legacy-message", loaded.Message);
        Assert.True(loaded.IsOn);
        Assert.False(loaded.MessageIsOn);
        Assert.Equal(AlertMusic.Duck, loaded.MusicType);
        Assert.Equal(SignalType.Sell, loaded.SignalType);
        Assert.Equal(10.5m, loaded.VolumeReaction);
        Assert.Equal(0.5m, loaded.Slippage);
        Assert.Equal(1, loaded.NumberClosePosition);
        Assert.Equal(OrderPriceType.Limit, loaded.OrderPriceType);
        Assert.Equal(PriceAlertTypeActivation.PriceHigherOrEqual, loaded.TypeActivation);
        Assert.Equal(999.99m, loaded.PriceActivation);
        Assert.Equal(AlertSlippageType.Persent, loaded.SlippageType);

        loaded.Save();
        Assert.True(File.Exists(scope.CanonicalPath));
        Assert.Contains("Message = \"legacy-message\"", File.ReadAllText(scope.CanonicalPath));
    }
}
