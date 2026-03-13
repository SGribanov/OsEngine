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

public class BotTabPolygonStandartSettingsPersistenceTests
{
    [Fact]
    public void SaveStandartSettings_ShouldPersistToml_AndLoadRoundTrip()
    {
        const string tabName = "CodexPolygonStandartSettings";
        using BotTabPolygonStandartSettingsFileScope scope = new BotTabPolygonStandartSettingsFileScope(tabName);

        BotTabPolygon source = scope.CreateWithoutConstructor();
        scope.SetEventsIsOn(source, false);
        scope.SetEmulatorIsOn(source, true);
        source.SeparatorToSecurities = "/";
        source.CommissionType = CommissionPolygonType.Percent;
        source.CommissionValue = 0.3m;
        source.CommissionIsSubstract = true;
        source.DelayType = DelayPolygonType.InMLS;
        source.DelayMls = 250;
        source.QtyStart = 1.2m;
        source.SlippagePercent = 0.4m;
        source.ProfitToSignal = 2.5m;
        source.ActionOnSignalType = PolygonActionOnSignalType.All;
        source.OrderPriceType = OrderPriceType.Market;
        source.AutoCreatorSequenceBaseCurrency = "USDT";
        source.AutoCreatorSequenceSeparator = "_";
        source.SortingOnOff = false;

        source.SaveStandartSettings();

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("SeparatorToSecurities = \"/\"", content);

        BotTabPolygon target = scope.CreateWithoutConstructor();
        scope.InvokePrivateLoadStandartSettings(target);

        Assert.False(scope.GetEventsIsOn(target));
        Assert.True(scope.GetEmulatorIsOn(target));
        Assert.Equal("/", target.SeparatorToSecurities);
        Assert.Equal(CommissionPolygonType.Percent, target.CommissionType);
        Assert.Equal(0.3m, target.CommissionValue);
        Assert.True(target.CommissionIsSubstract);
        Assert.Equal(DelayPolygonType.InMLS, target.DelayType);
        Assert.Equal(250, target.DelayMls);
        Assert.Equal(1.2m, target.QtyStart);
        Assert.Equal(0.4m, target.SlippagePercent);
        Assert.Equal(2.5m, target.ProfitToSignal);
        Assert.Equal(PolygonActionOnSignalType.All, target.ActionOnSignalType);
        Assert.Equal(OrderPriceType.Market, target.OrderPriceType);
        Assert.Equal("USDT", target.AutoCreatorSequenceBaseCurrency);
        Assert.Equal("_", target.AutoCreatorSequenceSeparator);
        Assert.False(target.SortingOnOff);
    }

    [Fact]
    public void LoadStandartSettings_ShouldSupportLegacyLineBasedFormat_AndSaveToml()
    {
        const string tabName = "CodexPolygonStandartLegacy";
        using BotTabPolygonStandartSettingsFileScope scope = new BotTabPolygonStandartSettingsFileScope(tabName);

        string legacyContent = string.Join(
            Environment.NewLine,
            "True",
            "False",
            "/",
            "Percent",
            "0.15",
            "True",
            "InMLS",
            "150",
            "2.5",
            "0.25",
            "1.75",
            "Alert",
            "Limit",
            "BTC",
            "-",
            "True");
        File.WriteAllText(scope.LegacyTxtPath, legacyContent);

        BotTabPolygon target = scope.CreateWithoutConstructor();
        scope.InvokePrivateLoadStandartSettings(target);

        Assert.True(scope.GetEventsIsOn(target));
        Assert.False(scope.GetEmulatorIsOn(target));
        Assert.Equal("/", target.SeparatorToSecurities);
        Assert.Equal(CommissionPolygonType.Percent, target.CommissionType);
        Assert.Equal(0.15m, target.CommissionValue);
        Assert.True(target.CommissionIsSubstract);
        Assert.Equal(DelayPolygonType.InMLS, target.DelayType);
        Assert.Equal(150, target.DelayMls);
        Assert.Equal(2.5m, target.QtyStart);
        Assert.Equal(0.25m, target.SlippagePercent);
        Assert.Equal(1.75m, target.ProfitToSignal);
        Assert.Equal(PolygonActionOnSignalType.Alert, target.ActionOnSignalType);
        Assert.Equal(OrderPriceType.Limit, target.OrderPriceType);
        Assert.Equal("BTC", target.AutoCreatorSequenceBaseCurrency);
        Assert.Equal("-", target.AutoCreatorSequenceSeparator);
        Assert.True(target.SortingOnOff);

        target.SaveStandartSettings();
        Assert.True(File.Exists(scope.CanonicalPath));
    }

    private sealed class BotTabPolygonStandartSettingsFileScope : IDisposable
    {
        private readonly string _tabName;
        private readonly MethodInfo _loadStandartSettingsMethod;
        private readonly FieldInfo _eventsField;
        private readonly FieldInfo _emulatorField;
        private readonly StructuredSettingsFileScope _settingsScope;

        public BotTabPolygonStandartSettingsFileScope(string tabName)
        {
            _tabName = tabName;
            _settingsScope = new StructuredSettingsFileScope(Path.Combine("Engine", _tabName + "StandartPolygonSettings.toml"));

            _loadStandartSettingsMethod = typeof(BotTabPolygon).GetMethod("LoadStandartSettings", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method LoadStandartSettings not found.");
            _eventsField = typeof(BotTabPolygon).GetField("_eventsIsOn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _eventsIsOn not found.");
            _emulatorField = typeof(BotTabPolygon).GetField("_emulatorIsOn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _emulatorIsOn not found.");

        }

        public string CanonicalPath => _settingsScope.CanonicalPath;

        public string LegacyTxtPath => _settingsScope.LegacyTxtPath;

        public BotTabPolygon CreateWithoutConstructor()
        {
            BotTabPolygon tab = (BotTabPolygon)RuntimeHelpers.GetUninitializedObject(typeof(BotTabPolygon));
            tab.TabName = _tabName;
            tab.StartProgram = StartProgram.IsOsTrader;
            return tab;
        }

        public void SetEventsIsOn(BotTabPolygon tab, bool value)
        {
            _eventsField.SetValue(tab, value);
        }

        public bool GetEventsIsOn(BotTabPolygon tab)
        {
            return (bool)_eventsField.GetValue(tab)!;
        }

        public void SetEmulatorIsOn(BotTabPolygon tab, bool value)
        {
            _emulatorField.SetValue(tab, value);
        }

        public bool GetEmulatorIsOn(BotTabPolygon tab)
        {
            return (bool)_emulatorField.GetValue(tab)!;
        }

        public void InvokePrivateLoadStandartSettings(BotTabPolygon tab)
        {
            _loadStandartSettingsMethod.Invoke(tab, null);
        }

        public void Dispose()
        {
            _settingsScope.Dispose();
        }
    }
}
