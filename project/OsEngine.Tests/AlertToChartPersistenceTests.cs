using System;
using System.Drawing;
using System.IO;
using System.Linq;
using OsEngine.Alerts;
using OsEngine.Entity;
using Xunit;

namespace OsEngine.Tests;

public class AlertToChartPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexChartAlertJson";
        using AlertToChartFileScope scope = new AlertToChartFileScope(name);

        AlertToChart source = new AlertToChart(name, null)
        {
            Type = ChartAlertType.Line,
            Label = "json-label",
            Message = "json-message",
            BorderWidth = 3,
            IsOn = true,
            IsMusicOn = true,
            IsMessageOn = false,
            ColorLine = Color.Red,
            ColorLabel = Color.Blue,
            Music = AlertMusic.Wolf,
            SignalType = SignalType.Sell,
            VolumeReaction = 7.5m,
            Slippage = 0.25m,
            NumberClosePosition = 2,
            OrderPriceType = OrderPriceType.Market,
            SlippageType = AlertSlippageType.PriceStep,
            Lines = new[]
            {
                new ChartAlertLine
                {
                    TimeFirstPoint = new DateTime(2026, 2, 16, 10, 0, 0),
                    ValueFirstPoint = 100.5m,
                    TimeSecondPoint = new DateTime(2026, 2, 16, 11, 0, 0),
                    ValueSecondPoint = 101.5m,
                    LastPoint = 102.5m
                }
            }
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        AlertToChart loaded = new AlertToChart(name, null);
        Assert.Equal("json-label", loaded.Label);
        Assert.Equal("json-message", loaded.Message);
        Assert.Equal(3, loaded.BorderWidth);
        Assert.True(loaded.IsOn);
        Assert.True(loaded.IsMusicOn);
        Assert.False(loaded.IsMessageOn);
        Assert.Equal(Color.Red.ToArgb(), loaded.ColorLine.ToArgb());
        Assert.Equal(Color.Blue.ToArgb(), loaded.ColorLabel.ToArgb());
        Assert.Equal(AlertMusic.Wolf, loaded.Music);
        Assert.Equal(SignalType.Sell, loaded.SignalType);
        Assert.Equal(7.5m, loaded.VolumeReaction);
        Assert.Equal(0.25m, loaded.Slippage);
        Assert.Equal(2, loaded.NumberClosePosition);
        Assert.Equal(OrderPriceType.Market, loaded.OrderPriceType);
        Assert.Equal(AlertSlippageType.PriceStep, loaded.SlippageType);
        Assert.NotNull(loaded.Lines);
        Assert.Single(loaded.Lines);
        Assert.Equal(100.5m, loaded.Lines[0].ValueFirstPoint);
        Assert.Equal(101.5m, loaded.Lines[0].ValueSecondPoint);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexChartAlertLegacy";
        using AlertToChartFileScope scope = new AlertToChartFileScope(name);

        ChartAlertLine line = new ChartAlertLine
        {
            TimeFirstPoint = new DateTime(2026, 2, 16, 12, 0, 0),
            ValueFirstPoint = 200m,
            TimeSecondPoint = new DateTime(2026, 2, 16, 13, 0, 0),
            ValueSecondPoint = 210m,
            LastPoint = 220m
        };

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "Line",
            line.GetStringToSave() + "%",
            "legacy-label",
            "legacy-message",
            "2",
            "True",
            "False",
            "True",
            Color.Green.ToArgb().ToString(),
            Color.Yellow.ToArgb().ToString(),
            "Duck",
            "Buy",
            "5.5",
            "0.4",
            "1",
            "Limit",
            "Absolute"
        });

        AlertToChart loaded = new AlertToChart(name, null);
        Assert.Equal("legacy-label", loaded.Label);
        Assert.Equal("legacy-message", loaded.Message);
        Assert.Equal(2, loaded.BorderWidth);
        Assert.True(loaded.IsOn);
        Assert.False(loaded.IsMusicOn);
        Assert.True(loaded.IsMessageOn);
        Assert.Equal(Color.Green.ToArgb(), loaded.ColorLine.ToArgb());
        Assert.Equal(Color.Yellow.ToArgb(), loaded.ColorLabel.ToArgb());
        Assert.Equal(AlertMusic.Duck, loaded.Music);
        Assert.Equal(SignalType.Buy, loaded.SignalType);
        Assert.Equal(5.5m, loaded.VolumeReaction);
        Assert.Equal(0.4m, loaded.Slippage);
        Assert.Equal(1, loaded.NumberClosePosition);
        Assert.Equal(OrderPriceType.Limit, loaded.OrderPriceType);
        Assert.Equal(AlertSlippageType.Absolute, loaded.SlippageType);
        Assert.NotNull(loaded.Lines);
        Assert.Single(loaded.Lines);
        Assert.Equal(200m, loaded.Lines[0].ValueFirstPoint);
        Assert.Equal(210m, loaded.Lines[0].ValueSecondPoint);
    }

    private sealed class AlertToChartFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;

        public AlertToChartFileScope(string name)
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
