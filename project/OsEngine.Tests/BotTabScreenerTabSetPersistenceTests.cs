#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Candles;
using OsEngine.Entity;
using OsEngine.Market.Connectors;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.OsTrader.Panels.Tab.Internal;
using Xunit;

namespace OsEngine.Tests;

public class BotTabScreenerTabSetPersistenceTests
{
    [Fact]
    public void SaveTabs_ShouldPersistJson()
    {
        const string tabName = "CodexScreenerTabSetJson";
        using BotTabScreenerTabSetFileScope scope = new BotTabScreenerTabSetFileScope(tabName);

        BotTabScreener source = scope.CreateScreenerWithoutConstructor();
        source.Tabs.Add(scope.CreateSimpleTabStub("TAB_A"));
        source.Tabs.Add(scope.CreateSimpleTabStub("TAB_B"));

        scope.InvokeSaveTabs(source);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());
    }

    [Fact]
    public void ParseLegacyScreenerTabSetSettings_ShouldSupportHashSeparatedFormat()
    {
        const string tabName = "CodexScreenerTabSetLegacy";
        using BotTabScreenerTabSetFileScope scope = new BotTabScreenerTabSetFileScope(tabName);

        object settings = scope.ParseLegacy("TAB_X#TAB_Y#TAB_Z#");
        Assert.NotNull(settings);

        List<string> names = scope.ExtractNames(settings);
        Assert.Equal(3, names.Count);
        Assert.Equal("TAB_X", names[0]);
        Assert.Equal("TAB_Y", names[1]);
        Assert.Equal("TAB_Z", names[2]);
    }

    private sealed class BotTabScreenerTabSetFileScope : IDisposable
    {
        private readonly string _tabName;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _saveTabsMethod;
        private readonly MethodInfo _parseLegacyMethod;
        private readonly FieldInfo _screenerStartProgramField;
        private readonly FieldInfo _botTabSimpleConnectorField;

        public BotTabScreenerTabSetFileScope(string tabName)
        {
            _tabName = tabName;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, _tabName + "ScreenerTabSet.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _saveTabsMethod = typeof(BotTabScreener).GetMethod("SaveTabs", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method SaveTabs not found.");
            _parseLegacyMethod = typeof(BotTabScreener).GetMethod(
                "ParseLegacyScreenerTabSetSettings",
                BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method ParseLegacyScreenerTabSetSettings not found.");
            _screenerStartProgramField = typeof(BotTabScreener).GetField("_startProgram", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _startProgram not found.");
            _botTabSimpleConnectorField = typeof(BotTabSimple).GetField("_connector", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _connector not found.");

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

        public BotTabScreener CreateScreenerWithoutConstructor()
        {
            BotTabScreener screener = (BotTabScreener)RuntimeHelpers.GetUninitializedObject(typeof(BotTabScreener));
            screener.TabName = _tabName;
            screener.Tabs = new List<BotTabSimple>();
            _screenerStartProgramField.SetValue(screener, StartProgram.IsOsOptimizer);
            return screener;
        }

        public BotTabSimple CreateSimpleTabStub(string name)
        {
            BotTabSimple tab = (BotTabSimple)RuntimeHelpers.GetUninitializedObject(typeof(BotTabSimple));
            tab.TabName = name;

            ConnectorCandles connector = (ConnectorCandles)RuntimeHelpers.GetUninitializedObject(typeof(ConnectorCandles));
            connector.TimeFrameBuilder = (TimeFrameBuilder)RuntimeHelpers.GetUninitializedObject(typeof(TimeFrameBuilder));
            _botTabSimpleConnectorField.SetValue(tab, connector);

            BotManualControl manual = (BotManualControl)RuntimeHelpers.GetUninitializedObject(typeof(BotManualControl));
            manual._startProgram = StartProgram.IsOsOptimizer;
            tab.ManualPositionSupport = manual;

            return tab;
        }

        public void InvokeSaveTabs(BotTabScreener tab)
        {
            _saveTabsMethod.Invoke(tab, null);
        }

        public object ParseLegacy(string content)
        {
            return _parseLegacyMethod.Invoke(null, new object[] { content })!;
        }

        public List<string> ExtractNames(object settings)
        {
            PropertyInfo tabNamesProperty = settings.GetType().GetProperty("TabNames", BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Property TabNames not found.");
            IEnumerable<string> values = (IEnumerable<string>)tabNamesProperty.GetValue(settings)!;
            return values.ToList();
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
