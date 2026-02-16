using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Candles;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.Market.Connectors;
using OsEngine.OsTrader.Panels.Tab;
using Xunit;

namespace OsEngine.Tests;

public class BotTabScreenerSettingsPersistenceTests
{
    [Fact]
    public void SaveSettings_ShouldPersistJson_AndLoadRoundTrip()
    {
        using BotTabScreenerSettingsFileScope scope = new BotTabScreenerSettingsFileScope("CodexScreenerSettings");

        BotTabScreener source = scope.CreateWithoutConstructor();
        scope.Setup(source);
        source.PortfolioName = "PF_SCREENER";
        source.SecuritiesClass = "TQBR";
        source.TimeFrame = TimeFrame.Min5;
        source.ServerType = ServerType.QuikLua;
        source.ServerName = "QuikLua_Main";
        source.CandleMarketDataType = CandleMarketDataType.Tick;
        source.MarketDepthBuildMaxSpread = 0.25m;
        source.MarketDepthBuildMaxSpreadIsOn = true;
        source.CommissionType = CommissionType.None;
        source.CommissionValue = 1.5m;
        source.SaveTradesInCandles = true;
        scope.SetEmulatorIsOn(source, true);
        scope.SetEventsIsOn(source, false);
        scope.SetCandleCreateMethodType(source, "Simple");
        source.SecuritiesNames.Add(new ActivatedSecurity
        {
            SecurityName = "SBER",
            SecurityClass = "TQBR",
            IsOn = true
        });

        source.SaveSettings();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        BotTabScreener target = scope.CreateWithoutConstructor();
        scope.Setup(target);
        target.LoadSettings();

        Assert.Equal("PF_SCREENER", target.PortfolioName);
        Assert.Equal("TQBR", target.SecuritiesClass);
        Assert.Equal(TimeFrame.Min5, target.TimeFrame);
        Assert.Equal(ServerType.QuikLua, target.ServerType);
        Assert.Equal("QuikLua_Main", target.ServerName);
        Assert.Equal(CandleMarketDataType.Tick, target.CandleMarketDataType);
        Assert.Equal(0.25m, target.MarketDepthBuildMaxSpread);
        Assert.True(target.MarketDepthBuildMaxSpreadIsOn);
        Assert.Equal(CommissionType.None, target.CommissionType);
        Assert.Equal(1.5m, target.CommissionValue);
        Assert.True(target.SaveTradesInCandles);
        Assert.True(scope.GetEmulatorIsOn(target));
        Assert.False(scope.GetEventsIsOn(target));
        Assert.Equal("Simple", target.CandleCreateMethodType);
        Assert.Single(target.SecuritiesNames);
        Assert.Equal("SBER", target.SecuritiesNames[0].SecurityName);
    }

    [Fact]
    public void LoadSettings_ShouldSupportLegacyLineBasedFormat()
    {
        using BotTabScreenerSettingsFileScope scope = new BotTabScreenerSettingsFileScope("CodexScreenerLegacy");

        var simpleSeries = CandleFactory.CreateCandleSeriesRealization("Simple");
        simpleSeries.Init(StartProgram.IsOsTrader);

        ActivatedSecurity sec = new ActivatedSecurity
        {
            SecurityName = "GAZP",
            SecurityClass = "TQBR",
            IsOn = false
        };

        string legacyContent = string.Join(
            Environment.NewLine,
            "PF_LEGACY",
            "SPBFUT",
            "Min1",
            "MetaTrader5&MetaTrader5_Main",
            "False",
            $"{CandleMarketDataType.Tick}&0.5&False",
            "Simple",
            "None",
            "2",
            "False",
            "True",
            "Simple",
            simpleSeries.GetSaveString(),
            sec.GetSaveStr());
        File.WriteAllText(scope.SettingsPath, legacyContent);

        BotTabScreener target = scope.CreateWithoutConstructor();
        scope.Setup(target);
        target.LoadSettings();

        Assert.Equal("PF_LEGACY", target.PortfolioName);
        Assert.Equal("SPBFUT", target.SecuritiesClass);
        Assert.Equal(TimeFrame.Min1, target.TimeFrame);
        Assert.Equal(ServerType.MetaTrader5, target.ServerType);
        Assert.Equal("MetaTrader5_Main", target.ServerName);
        Assert.Equal(2m, target.CommissionValue);
        Assert.False(target.SaveTradesInCandles);
        Assert.False(scope.GetEmulatorIsOn(target));
        Assert.True(scope.GetEventsIsOn(target));
        Assert.Single(target.SecuritiesNames);
        Assert.Equal("GAZP", target.SecuritiesNames[0].SecurityName);
    }

    private sealed class BotTabScreenerSettingsFileScope : IDisposable
    {
        private readonly string _tabName;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly FieldInfo _startProgramField;
        private readonly FieldInfo _emulatorField;
        private readonly FieldInfo _eventsField;
        private readonly FieldInfo _candleCreateMethodTypeField;

        public BotTabScreenerSettingsFileScope(string tabName)
        {
            _tabName = tabName;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, $"{_tabName}ScreenerSet.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _startProgramField = typeof(BotTabScreener).GetField("_startProgram", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _startProgram not found.");
            _emulatorField = typeof(BotTabScreener).GetField("_emulatorIsOn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _emulatorIsOn not found.");
            _eventsField = typeof(BotTabScreener).GetField("_eventsIsOn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _eventsIsOn not found.");
            _candleCreateMethodTypeField = typeof(BotTabScreener).GetField("_candleCreateMethodType", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _candleCreateMethodType not found.");

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

        public BotTabScreener CreateWithoutConstructor()
        {
            return (BotTabScreener)RuntimeHelpers.GetUninitializedObject(typeof(BotTabScreener));
        }

        public void Setup(BotTabScreener tab)
        {
            tab.TabName = _tabName;
            _startProgramField.SetValue(tab, StartProgram.IsOsTrader);
            tab.SecuritiesNames = new List<ActivatedSecurity>();
            tab.CandleSeriesRealization = CandleFactory.CreateCandleSeriesRealization("Simple");
            tab.CandleSeriesRealization.Init(StartProgram.IsOsTrader);
            _candleCreateMethodTypeField.SetValue(tab, "Simple");
            _eventsField.SetValue(tab, true);
        }

        public void SetEmulatorIsOn(BotTabScreener tab, bool value)
        {
            _emulatorField.SetValue(tab, value);
        }

        public bool GetEmulatorIsOn(BotTabScreener tab)
        {
            return (bool)_emulatorField.GetValue(tab)!;
        }

        public void SetEventsIsOn(BotTabScreener tab, bool value)
        {
            _eventsField.SetValue(tab, value);
        }

        public bool GetEventsIsOn(BotTabScreener tab)
        {
            return (bool)_eventsField.GetValue(tab)!;
        }

        public void SetCandleCreateMethodType(BotTabScreener tab, string value)
        {
            _candleCreateMethodTypeField.SetValue(tab, value);
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
