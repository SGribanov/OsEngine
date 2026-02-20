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

public class PairToTradeSettingsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using PairToTradeSettingsFileScope scope = new PairToTradeSettingsFileScope("CodexPairToTrade");

        PairToTrade source = scope.CreateWithoutConstructor();
        scope.Setup(source);
        source.PairNum = 8;
        source.Sec1Slippage = 0.11m;
        source.Sec1Volume = 1.1m;
        source.Sec2Slippage = 0.22m;
        source.Sec2Volume = 2.2m;
        source.CorrelationLookBack = 30;
        source.CointegrationDeviation = 1.6m;
        source.CointegrationLookBack = 40;
        source.Sec1SlippageType = PairTraderSlippageType.Absolute;
        source.Sec1VolumeType = PairTraderVolumeType.Currency;
        source.Sec2SlippageType = PairTraderSlippageType.Percent;
        source.Sec2VolumeType = PairTraderVolumeType.Contract;
        source.Sec1TradeRegime = PairTraderSecurityTradeRegime.Limit;
        source.Sec2TradeRegime = PairTraderSecurityTradeRegime.Second;
        source.AutoRebuildCointegration = false;
        source.AutoRebuildCorrelation = true;
        scope.SetLastEntryCointegrationSide(source, CointegrationLineSide.Up);
        scope.SetShowTradePanelOnChart(source, false);

        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        PairToTrade target = scope.CreateWithoutConstructor();
        scope.Setup(target);
        scope.InvokePrivateLoad(target);

        Assert.Equal(8, target.PairNum);
        Assert.Equal(0.11m, target.Sec1Slippage);
        Assert.Equal(1.1m, target.Sec1Volume);
        Assert.Equal(0.22m, target.Sec2Slippage);
        Assert.Equal(2.2m, target.Sec2Volume);
        Assert.Equal(30, target.CorrelationLookBack);
        Assert.Equal(1.6m, target.CointegrationDeviation);
        Assert.Equal(40, target.CointegrationLookBack);
        Assert.Equal(PairTraderSlippageType.Absolute, target.Sec1SlippageType);
        Assert.Equal(PairTraderVolumeType.Currency, target.Sec1VolumeType);
        Assert.Equal(PairTraderSlippageType.Percent, target.Sec2SlippageType);
        Assert.Equal(PairTraderVolumeType.Contract, target.Sec2VolumeType);
        Assert.Equal(PairTraderSecurityTradeRegime.Limit, target.Sec1TradeRegime);
        Assert.Equal(PairTraderSecurityTradeRegime.Second, target.Sec2TradeRegime);
        Assert.Equal(CointegrationLineSide.Up, scope.GetLastEntryCointegrationSide(target));
        Assert.False(scope.GetShowTradePanelOnChart(target));
        Assert.False(target.AutoRebuildCointegration);
        Assert.True(target.AutoRebuildCorrelation);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using PairToTradeSettingsFileScope scope = new PairToTradeSettingsFileScope("CodexPairToTradeLegacy");

        string legacyContent = string.Join(
            Environment.NewLine,
            "12",
            "0.5",
            "3",
            "0.7",
            "4",
            "80",
            "2.5",
            "90",
            "Percent",
            "Contract",
            "Absolute",
            "Currency",
            "Market",
            "Off",
            "Down",
            "True",
            "True",
            "False");
        File.WriteAllText(scope.SettingsPath, legacyContent);

        PairToTrade target = scope.CreateWithoutConstructor();
        scope.Setup(target);
        scope.InvokePrivateLoad(target);

        Assert.Equal(12, target.PairNum);
        Assert.Equal(0.5m, target.Sec1Slippage);
        Assert.Equal(3m, target.Sec1Volume);
        Assert.Equal(0.7m, target.Sec2Slippage);
        Assert.Equal(4m, target.Sec2Volume);
        Assert.Equal(80, target.CorrelationLookBack);
        Assert.Equal(2.5m, target.CointegrationDeviation);
        Assert.Equal(90, target.CointegrationLookBack);
        Assert.Equal(PairTraderSlippageType.Percent, target.Sec1SlippageType);
        Assert.Equal(PairTraderVolumeType.Contract, target.Sec1VolumeType);
        Assert.Equal(PairTraderSlippageType.Absolute, target.Sec2SlippageType);
        Assert.Equal(PairTraderVolumeType.Currency, target.Sec2VolumeType);
        Assert.Equal(PairTraderSecurityTradeRegime.Market, target.Sec1TradeRegime);
        Assert.Equal(PairTraderSecurityTradeRegime.Off, target.Sec2TradeRegime);
        Assert.Equal(CointegrationLineSide.Down, scope.GetLastEntryCointegrationSide(target));
        Assert.True(scope.GetShowTradePanelOnChart(target));
        Assert.True(target.AutoRebuildCointegration);
        Assert.False(target.AutoRebuildCorrelation);
    }

    private sealed class PairToTradeSettingsFileScope : IDisposable
    {
        private readonly string _pairName;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _loadMethod;
        private readonly FieldInfo _lastEntrySideField;
        private readonly FieldInfo _showTradePanelField;

        public PairToTradeSettingsFileScope(string pairName)
        {
            _pairName = pairName;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, $"{_pairName}PairsSettings.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _loadMethod = typeof(PairToTrade).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Load not found.");
            _lastEntrySideField = typeof(PairToTrade).GetField("_lastEntryCointegrationSide", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _lastEntryCointegrationSide not found.");
            _showTradePanelField = typeof(PairToTrade).GetField("_showTradePanelOnChart", BindingFlags.NonPublic | BindingFlags.Instance)
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

        public PairToTrade CreateWithoutConstructor()
        {
            return (PairToTrade)RuntimeHelpers.GetUninitializedObject(typeof(PairToTrade));
        }

        public void Setup(PairToTrade pair)
        {
            pair.Name = _pairName;
        }

        public void InvokePrivateLoad(PairToTrade pair)
        {
            _loadMethod.Invoke(pair, null);
        }

        public void SetLastEntryCointegrationSide(PairToTrade pair, CointegrationLineSide side)
        {
            _lastEntrySideField.SetValue(pair, side);
        }

        public CointegrationLineSide GetLastEntryCointegrationSide(PairToTrade pair)
        {
            return (CointegrationLineSide)_lastEntrySideField.GetValue(pair)!;
        }

        public void SetShowTradePanelOnChart(PairToTrade pair, bool value)
        {
            _showTradePanelField.SetValue(pair, value);
        }

        public bool GetShowTradePanelOnChart(PairToTrade pair)
        {
            return (bool)_showTradePanelField.GetValue(pair)!;
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
