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

public class BotTabPairStandartSettingsPersistenceTests
{
    [Fact]
    public void SaveStandartSettings_ShouldPersistToml_AndLoadRoundTrip()
    {
        using BotTabPairSettingsFileScope scope = new BotTabPairSettingsFileScope("CodexPairSettings");

        BotTabPair source = scope.CreateWithoutConstructor();
        scope.Setup(source);
        source.Sec1Slippage = 0.1m;
        source.Sec1Volume = 1.2m;
        source.Sec2Slippage = 0.3m;
        source.Sec2Volume = 2.4m;
        source.CorrelationLookBack = 42;
        source.CointegrationDeviation = 1.7m;
        source.CointegrationLookBack = 55;
        source.PairSortType = MainGridPairSortType.Correlation;
        source.Sec1SlippageType = PairTraderSlippageType.Absolute;
        source.Sec1VolumeType = PairTraderVolumeType.Currency;
        source.Sec2SlippageType = PairTraderSlippageType.Percent;
        source.Sec2VolumeType = PairTraderVolumeType.Contract;
        source.Sec1TradeRegime = PairTraderSecurityTradeRegime.Second;
        source.Sec2TradeRegime = PairTraderSecurityTradeRegime.Limit;
        source.AutoRebuildCointegration = true;
        source.AutoRebuildCorrelation = false;
        scope.SetEventsIsOn(source, false);
        scope.SetEmulatorIsOn(source, true);

        source.SaveStandartSettings();

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("Sec1Slippage = 0.1", content);

        BotTabPair target = scope.CreateWithoutConstructor();
        scope.Setup(target);
        target.Sec1Slippage = 0;
        target.Sec1Volume = 0;
        target.Sec2Slippage = 0;
        target.Sec2Volume = 0;
        scope.InvokePrivateLoadStandartSettings(target);

        Assert.Equal(0.1m, target.Sec1Slippage);
        Assert.Equal(1.2m, target.Sec1Volume);
        Assert.Equal(0.3m, target.Sec2Slippage);
        Assert.Equal(2.4m, target.Sec2Volume);
        Assert.Equal(42, target.CorrelationLookBack);
        Assert.Equal(1.7m, target.CointegrationDeviation);
        Assert.Equal(55, target.CointegrationLookBack);
        Assert.Equal(MainGridPairSortType.Correlation, target.PairSortType);
        Assert.Equal(PairTraderSlippageType.Absolute, target.Sec1SlippageType);
        Assert.Equal(PairTraderVolumeType.Currency, target.Sec1VolumeType);
        Assert.Equal(PairTraderSlippageType.Percent, target.Sec2SlippageType);
        Assert.Equal(PairTraderVolumeType.Contract, target.Sec2VolumeType);
        Assert.Equal(PairTraderSecurityTradeRegime.Second, target.Sec1TradeRegime);
        Assert.Equal(PairTraderSecurityTradeRegime.Limit, target.Sec2TradeRegime);
        Assert.True(target.AutoRebuildCointegration);
        Assert.False(target.AutoRebuildCorrelation);
        Assert.False(scope.GetEventsIsOn(target));
        Assert.True(scope.GetEmulatorIsOn(target));
    }

    [Fact]
    public void LoadStandartSettings_ShouldSupportLegacyLineBasedFormat_AndSaveToml()
    {
        using BotTabPairSettingsFileScope scope = new BotTabPairSettingsFileScope("CodexPairLegacy");

        string legacyContent = string.Join(
            Environment.NewLine,
            "0.4",
            "7",
            "0.6",
            "9",
            "30",
            "2.2",
            "70",
            "Side",
            "Percent",
            "Contract",
            "Absolute",
            "Currency",
            "True",
            "False",
            "Market",
            "Off",
            "False",
            "True");
        File.WriteAllText(scope.LegacyTxtPath, legacyContent);

        BotTabPair target = scope.CreateWithoutConstructor();
        scope.Setup(target);
        scope.InvokePrivateLoadStandartSettings(target);

        Assert.Equal(0.4m, target.Sec1Slippage);
        Assert.Equal(7m, target.Sec1Volume);
        Assert.Equal(0.6m, target.Sec2Slippage);
        Assert.Equal(9m, target.Sec2Volume);
        Assert.Equal(30, target.CorrelationLookBack);
        Assert.Equal(2.2m, target.CointegrationDeviation);
        Assert.Equal(70, target.CointegrationLookBack);
        Assert.Equal(MainGridPairSortType.Side, target.PairSortType);
        Assert.Equal(PairTraderSlippageType.Percent, target.Sec1SlippageType);
        Assert.Equal(PairTraderVolumeType.Contract, target.Sec1VolumeType);
        Assert.Equal(PairTraderSlippageType.Absolute, target.Sec2SlippageType);
        Assert.Equal(PairTraderVolumeType.Currency, target.Sec2VolumeType);
        Assert.Equal(PairTraderSecurityTradeRegime.Market, target.Sec1TradeRegime);
        Assert.Equal(PairTraderSecurityTradeRegime.Off, target.Sec2TradeRegime);
        Assert.False(target.AutoRebuildCointegration);
        Assert.True(target.AutoRebuildCorrelation);
        Assert.True(scope.GetEventsIsOn(target));
        Assert.False(scope.GetEmulatorIsOn(target));

        target.SaveStandartSettings();
        Assert.True(File.Exists(scope.CanonicalPath));
    }

    private sealed class BotTabPairSettingsFileScope : IDisposable
    {
        private readonly string _tabName;
        private readonly MethodInfo _loadStandartSettingsMethod;
        private readonly FieldInfo _eventsField;
        private readonly FieldInfo _emulatorField;
        private readonly StructuredSettingsFileScope _settingsScope;

        public BotTabPairSettingsFileScope(string tabName)
        {
            _tabName = tabName;
            _settingsScope = new StructuredSettingsFileScope(Path.Combine("Engine", $"{_tabName}StandartPairsSettings.toml"));

            _loadStandartSettingsMethod = typeof(BotTabPair).GetMethod("LoadStandartSettings", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method LoadStandartSettings not found.");
            _eventsField = typeof(BotTabPair).GetField("_eventsIsOn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _eventsIsOn not found.");
            _emulatorField = typeof(BotTabPair).GetField("_emulatorIsOn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _emulatorIsOn not found.");

        }

        public string CanonicalPath => _settingsScope.CanonicalPath;

        public string LegacyTxtPath => _settingsScope.LegacyTxtPath;

        public BotTabPair CreateWithoutConstructor()
        {
            return (BotTabPair)RuntimeHelpers.GetUninitializedObject(typeof(BotTabPair));
        }

        public void Setup(BotTabPair pair)
        {
            pair.TabName = _tabName;
            pair.StartProgram = StartProgram.IsOsTrader;
        }

        public void InvokePrivateLoadStandartSettings(BotTabPair pair)
        {
            _loadStandartSettingsMethod.Invoke(pair, null);
        }

        public void SetEventsIsOn(BotTabPair pair, bool value)
        {
            _eventsField.SetValue(pair, value);
        }

        public bool GetEventsIsOn(BotTabPair pair)
        {
            return (bool)_eventsField.GetValue(pair)!;
        }

        public void SetEmulatorIsOn(BotTabPair pair, bool value)
        {
            _emulatorField.SetValue(pair, value);
        }

        public bool GetEmulatorIsOn(BotTabPair pair)
        {
            return (bool)_emulatorField.GetValue(pair)!;
        }

        public void Dispose()
        {
            _settingsScope.Dispose();
        }
    }
}
