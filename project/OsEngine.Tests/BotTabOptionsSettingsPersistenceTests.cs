#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using OsEngine.Market;
using OsEngine.OsTrader.Panels.Tab;
using Xunit;

namespace OsEngine.Tests;

public class BotTabOptionsSettingsPersistenceTests
{
    [Fact]
    public void SaveSettings_ShouldPersistToml_AndLoadRoundTrip()
    {
        using BotTabOptionsSettingsFileScope scope = new BotTabOptionsSettingsFileScope("CodexOptionsSettings");

        BotTabOptions source = scope.CreateWithoutConstructor();
        scope.Setup(source);
        scope.SetPortfolioName(source, "PF_OPT");
        scope.SetUnderlyingAssets(source, new List<string> { "RI", "Si" });
        source.ServerType = ServerType.QuikLua;
        source.ServerName = "QuikLua_Main";
        scope.SetStrikesToShow(source, 9m);
        scope.SetEmulatorIsOn(source, true);
        scope.SetEventsOn(source, false);

        scope.InvokePrivateSaveSettings(source);

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("PortfolioName = \"PF_OPT\"", content);

        BotTabOptions target = scope.CreateWithoutConstructor();
        scope.Setup(target);
        target.LoadSettings();

        Assert.Equal("PF_OPT", target.PortfolioName);
        Assert.Equal(new List<string> { "RI", "Si" }, target.UnderlyingAssets);
        Assert.Equal(ServerType.QuikLua, target.ServerType);
        Assert.Equal("QuikLua_Main", target.ServerName);
        Assert.Equal(9m, scope.GetStrikesToShow(target));
        Assert.True(scope.GetEmulatorIsOn(target));
        Assert.False(scope.GetEventsOn(target));
    }

    [Fact]
    public void LoadSettings_ShouldSupportLegacyKeyValueFormat_AndSaveToml()
    {
        using BotTabOptionsSettingsFileScope scope = new BotTabOptionsSettingsFileScope("CodexOptionsLegacy");

        string legacyContent = string.Join(
            Environment.NewLine,
            "PortfolioName:PF_LEGACY",
            "UnderlyingAssets:BR,ED",
            "StrikesToShow:7",
            "ServerType:MetaTrader5",
            "ServerName:MetaTrader5_Main",
            "EmulatorIsOn:False",
            "EventsIsOn:True");
        File.WriteAllText(scope.LegacyTxtPath, legacyContent);

        BotTabOptions target = scope.CreateWithoutConstructor();
        scope.Setup(target);
        target.LoadSettings();

        Assert.Equal("PF_LEGACY", target.PortfolioName);
        Assert.Equal(new List<string> { "BR", "ED" }, target.UnderlyingAssets);
        Assert.Equal(ServerType.MetaTrader5, target.ServerType);
        Assert.Equal("MetaTrader5_Main", target.ServerName);
        Assert.Equal(7m, scope.GetStrikesToShow(target));
        Assert.False(scope.GetEmulatorIsOn(target));
        Assert.True(scope.GetEventsOn(target));

        scope.InvokePrivateSaveSettings(target);
        Assert.True(File.Exists(scope.CanonicalPath));
    }

    private sealed class BotTabOptionsSettingsFileScope : IDisposable
    {
        private readonly string _tabName;
        private readonly MethodInfo _saveSettingsMethod;
        private readonly FieldInfo _strikesField;
        private readonly FieldInfo _emulatorField;
        private readonly FieldInfo _eventsField;
        private readonly FieldInfo _portfolioNameBackingField;
        private readonly FieldInfo _underlyingAssetsBackingField;
        private readonly StructuredSettingsFileScope _settingsScope;

        public BotTabOptionsSettingsFileScope(string tabName)
        {
            _tabName = tabName;
            _settingsScope = new StructuredSettingsFileScope(Path.Combine("Engine", _tabName, "OptionsSettings.toml"));

            _saveSettingsMethod = typeof(BotTabOptions).GetMethod("SaveSettings", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method SaveSettings not found.");
            _strikesField = typeof(BotTabOptions).GetField("_strikesToShowNumericUpDown", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _strikesToShowNumericUpDown not found.");
            _emulatorField = typeof(BotTabOptions).GetField("_emulatorIsOn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _emulatorIsOn not found.");
            _eventsField = typeof(BotTabOptions).GetField("_eventsOn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _eventsOn not found.");
            _portfolioNameBackingField = typeof(BotTabOptions).GetField("<PortfolioName>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Backing field for PortfolioName not found.");
            _underlyingAssetsBackingField = typeof(BotTabOptions).GetField("<UnderlyingAssets>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Backing field for UnderlyingAssets not found.");

        }

        public string CanonicalPath => _settingsScope.CanonicalPath;

        public string LegacyTxtPath => _settingsScope.LegacyTxtPath;

        public BotTabOptions CreateWithoutConstructor()
        {
            return (BotTabOptions)RuntimeHelpers.GetUninitializedObject(typeof(BotTabOptions));
        }

        public void Setup(BotTabOptions tab)
        {
            tab.TabName = _tabName;
            NumericUpDown numeric = new NumericUpDown
            {
                Minimum = 0,
                Maximum = 1000
            };
            _strikesField.SetValue(tab, numeric);
            _eventsField.SetValue(tab, true);
        }

        public void InvokePrivateSaveSettings(BotTabOptions tab)
        {
            _saveSettingsMethod.Invoke(tab, null);
        }

        public void SetStrikesToShow(BotTabOptions tab, decimal value)
        {
            NumericUpDown numeric = (NumericUpDown)_strikesField.GetValue(tab)!;
            numeric.Value = value;
        }

        public void SetPortfolioName(BotTabOptions tab, string value)
        {
            _portfolioNameBackingField.SetValue(tab, value);
        }

        public void SetUnderlyingAssets(BotTabOptions tab, List<string> value)
        {
            _underlyingAssetsBackingField.SetValue(tab, value);
        }

        public decimal GetStrikesToShow(BotTabOptions tab)
        {
            NumericUpDown numeric = (NumericUpDown)_strikesField.GetValue(tab)!;
            return numeric.Value;
        }

        public void SetEmulatorIsOn(BotTabOptions tab, bool value)
        {
            _emulatorField.SetValue(tab, value);
        }

        public bool GetEmulatorIsOn(BotTabOptions tab)
        {
            return (bool)_emulatorField.GetValue(tab)!;
        }

        public void SetEventsOn(BotTabOptions tab, bool value)
        {
            _eventsField.SetValue(tab, value);
        }

        public bool GetEventsOn(BotTabOptions tab)
        {
            return (bool)_eventsField.GetValue(tab)!;
        }

        public void Dispose()
        {
            _settingsScope.Dispose();
        }
    }
}
