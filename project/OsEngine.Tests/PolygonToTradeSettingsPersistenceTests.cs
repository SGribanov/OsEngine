#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.OsTrader.Panels.Tab;
using Xunit;

namespace OsEngine.Tests;

public class PolygonToTradeSettingsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexPolygonToTradeJson";
        using PolygonToTradeSettingsFileScope scope = new PolygonToTradeSettingsFileScope(name);

        PolygonToTrade source = scope.CreateWithoutConstructor();
        source.PairNum = 7;
        scope.SetShowTradePanelOnChart(source, false);
        source.BaseCurrency = "USDT";
        source.Tab1TradeSide = Side.Buy;
        source.Tab2TradeSide = Side.Sell;
        source.Tab3TradeSide = Side.Buy;
        source.SeparatorToSecurities = "/";
        source.CommissionType = CommissionPolygonType.Percent;
        source.CommissionValue = 0.25m;
        source.CommissionIsSubstract = true;
        source.DelayType = DelayPolygonType.InMLS;
        source.DelayMls = 300;
        source.QtyStart = 2.5m;
        source.SlippagePercent = 0.7m;
        source.ProfitToSignal = 1.8m;
        source.ActionOnSignalType = PolygonActionOnSignalType.All;
        source.OrderPriceType = OrderPriceType.Limit;

        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        PolygonToTrade target = scope.CreateWithoutConstructor();
        scope.InvokePrivateLoad(target);

        Assert.Equal(7, target.PairNum);
        Assert.False(scope.GetShowTradePanelOnChart(target));
        Assert.Equal("USDT", target.BaseCurrency);
        Assert.Equal(Side.Buy, target.Tab1TradeSide);
        Assert.Equal(Side.Sell, target.Tab2TradeSide);
        Assert.Equal(Side.Buy, target.Tab3TradeSide);
        Assert.Equal("/", target.SeparatorToSecurities);
        Assert.Equal(CommissionPolygonType.Percent, target.CommissionType);
        Assert.Equal(0.25m, target.CommissionValue);
        Assert.True(target.CommissionIsSubstract);
        Assert.Equal(DelayPolygonType.InMLS, target.DelayType);
        Assert.Equal(300, target.DelayMls);
        Assert.Equal(2.5m, target.QtyStart);
        Assert.Equal(0.7m, target.SlippagePercent);
        Assert.Equal(1.8m, target.ProfitToSignal);
        Assert.Equal(PolygonActionOnSignalType.All, target.ActionOnSignalType);
        Assert.Equal(OrderPriceType.Limit, target.OrderPriceType);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexPolygonToTradeLegacy";
        using PolygonToTradeSettingsFileScope scope = new PolygonToTradeSettingsFileScope(name);

        string legacyContent = string.Join(
            Environment.NewLine,
            "5",
            "True",
            "BTC",
            "Sell",
            "Buy",
            "Sell",
            "-",
            "Percent",
            "0.1",
            "False",
            "InMLS",
            "120",
            "3",
            "0.5",
            "2.2",
            "Alert",
            "Market");
        File.WriteAllText(scope.SettingsPath, legacyContent);

        PolygonToTrade target = scope.CreateWithoutConstructor();
        scope.InvokePrivateLoad(target);

        Assert.Equal(5, target.PairNum);
        Assert.True(scope.GetShowTradePanelOnChart(target));
        Assert.Equal("BTC", target.BaseCurrency);
        Assert.Equal(Side.Sell, target.Tab1TradeSide);
        Assert.Equal(Side.Buy, target.Tab2TradeSide);
        Assert.Equal(Side.Sell, target.Tab3TradeSide);
        Assert.Equal("-", target.SeparatorToSecurities);
        Assert.Equal(CommissionPolygonType.Percent, target.CommissionType);
        Assert.Equal(0.1m, target.CommissionValue);
        Assert.False(target.CommissionIsSubstract);
        Assert.Equal(DelayPolygonType.InMLS, target.DelayType);
        Assert.Equal(120, target.DelayMls);
        Assert.Equal(3m, target.QtyStart);
        Assert.Equal(0.5m, target.SlippagePercent);
        Assert.Equal(2.2m, target.ProfitToSignal);
        Assert.Equal(PolygonActionOnSignalType.Alert, target.ActionOnSignalType);
        Assert.Equal(OrderPriceType.Market, target.OrderPriceType);
    }

    private sealed class PolygonToTradeSettingsFileScope : IDisposable
    {
        private readonly string _name;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _loadMethod;
        private readonly FieldInfo _showTradePanelOnChartField;

        public PolygonToTradeSettingsFileScope(string name)
        {
            _name = name;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, _name + "PolygonSettings.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _loadMethod = typeof(PolygonToTrade).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Load not found.");
            _showTradePanelOnChartField = typeof(PolygonToTrade).GetField("_showTradePanelOnChart", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _showTradePanelOnChart not found.");

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _settingsFileExisted = File.Exists(SettingsPath);
            if (_settingsFileExisted)
            {
                File.Copy(SettingsPath, _settingsBackupPath, overwrite: true);
            }
            else if (File.Exists(_settingsBackupPath))
            {
                File.Delete(_settingsBackupPath);
            }
        }

        public string SettingsPath { get; }

        public PolygonToTrade CreateWithoutConstructor()
        {
            PolygonToTrade sequence = (PolygonToTrade)RuntimeHelpers.GetUninitializedObject(typeof(PolygonToTrade));
            sequence.Name = _name;
            return sequence;
        }

        public void SetShowTradePanelOnChart(PolygonToTrade sequence, bool value)
        {
            _showTradePanelOnChartField.SetValue(sequence, value);
        }

        public bool GetShowTradePanelOnChart(PolygonToTrade sequence)
        {
            return (bool)_showTradePanelOnChartField.GetValue(sequence)!;
        }

        public void InvokePrivateLoad(PolygonToTrade sequence)
        {
            _loadMethod.Invoke(sequence, null);
        }

        public void Dispose()
        {
            if (_settingsFileExisted)
            {
                if (File.Exists(_settingsBackupPath))
                {
                    File.Copy(_settingsBackupPath, SettingsPath, overwrite: true);
                    File.Delete(_settingsBackupPath);
                }
            }
            else
            {
                if (File.Exists(SettingsPath))
                {
                    File.Delete(SettingsPath);
                }

                if (File.Exists(_settingsBackupPath))
                {
                    File.Delete(_settingsBackupPath);
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
