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
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexPriceAlertJson";
        using AlertToPriceFileScope scope = new AlertToPriceFileScope(name);

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

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

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
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexPriceAlertLegacy";
        using AlertToPriceFileScope scope = new AlertToPriceFileScope(name);

        File.WriteAllLines(scope.SettingsPath, new[]
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
    }

    private sealed class AlertToPriceFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;

        public AlertToPriceFileScope(string name)
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, name + "Alert.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _settingsFileExisted = File.Exists(SettingsPath);
            if (_settingsFileExisted)
            {
                File.Copy(SettingsPath, _settingsBackup, overwrite: true);
            }
            else if (File.Exists(_settingsBackup))
            {
                File.Delete(_settingsBackup);
            }
        }

        public string SettingsPath { get; }

        public void Dispose()
        {
            if (_settingsFileExisted)
            {
                if (File.Exists(_settingsBackup))
                {
                    File.Copy(_settingsBackup, SettingsPath, overwrite: true);
                    File.Delete(_settingsBackup);
                }
            }
            else
            {
                if (File.Exists(SettingsPath))
                {
                    File.Delete(SettingsPath);
                }

                if (File.Exists(_settingsBackup))
                {
                    File.Delete(_settingsBackup);
                }
            }

            if (!_engineDirExisted
                && Directory.Exists(_engineDirPath)
                && !Directory.EnumerateFileSystemEntries(_engineDirPath).Any())
            {
                Directory.Delete(_engineDirPath);
            }
        }
    }
}
