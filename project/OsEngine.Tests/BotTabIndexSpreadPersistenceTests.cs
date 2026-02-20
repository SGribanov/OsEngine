#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.Market.Connectors;
using OsEngine.OsTrader.Panels.Tab;
using Xunit;

namespace OsEngine.Tests;

public class BotTabIndexSpreadPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string tabName = "CodexBotTabIndexJson";
        using BotTabIndexSpreadFileScope scope = new BotTabIndexSpreadFileScope(tabName);

        BotTabIndex source = scope.CreateWithoutConstructor();
        scope.SetPrivateField(source, "_userFormula", "A0+A1");
        scope.SetPrivateField(source, "_eventsIsOn", false);
        source.CalculationDepth = 777;
        source.PercentNormalization = true;
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        BotTabIndex loaded = scope.CreateWithoutConstructor();
        loaded.Load();

        Assert.Equal("A0+A1", scope.GetPrivateField<string>(loaded, "_userFormula"));
        Assert.False(scope.GetPrivateField<bool>(loaded, "_eventsIsOn"));
        Assert.Equal(777, loaded.CalculationDepth);
        Assert.True(loaded.PercentNormalization);
        Assert.Empty(loaded.Tabs);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string tabName = "CodexBotTabIndexLegacy";
        using BotTabIndexSpreadFileScope scope = new BotTabIndexSpreadFileScope(tabName);

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            string.Empty,
            "LEGACY_FORMULA",
            "True",
            "555",
            "False"
        });

        BotTabIndex loaded = scope.CreateWithoutConstructor();
        loaded.Load();

        Assert.Equal("LEGACY_FORMULA", scope.GetPrivateField<string>(loaded, "_userFormula"));
        Assert.True(scope.GetPrivateField<bool>(loaded, "_eventsIsOn"));
        Assert.Equal(555, loaded.CalculationDepth);
        Assert.False(loaded.PercentNormalization);
        Assert.Empty(loaded.Tabs);
    }

    private sealed class BotTabIndexSpreadFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly FieldInfo _startProgramField;
        private readonly FieldInfo _isLoadedField;

        public BotTabIndexSpreadFileScope(string tabName)
        {
            TabName = tabName;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, tabName + "SpreadSet.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _startProgramField = typeof(BotTabIndex).GetField("_startProgram", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _startProgram not found.");
            _isLoadedField = typeof(BotTabIndex).GetField("_isLoaded", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _isLoaded not found.");

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

        public string TabName { get; }

        public string SettingsPath { get; }

        public BotTabIndex CreateWithoutConstructor()
        {
            BotTabIndex instance = (BotTabIndex)RuntimeHelpers.GetUninitializedObject(typeof(BotTabIndex));
            instance.TabName = TabName;
            instance.Tabs = new List<ConnectorCandles>();
            instance.CalculationDepth = 1500;
            instance.PercentNormalization = false;

            _startProgramField.SetValue(instance, StartProgram.IsOsTrader);
            _isLoadedField.SetValue(instance, false);
            SetPrivateField(instance, "_eventsIsOn", true);
            SetPrivateField(instance, "_userFormula", string.Empty);
            return instance;
        }

        public void SetPrivateField(BotTabIndex tab, string fieldName, object? value)
        {
            FieldInfo field = typeof(BotTabIndex).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field not found: " + fieldName);
            field.SetValue(tab, value);
        }

        public T GetPrivateField<T>(BotTabIndex tab, string fieldName)
        {
            FieldInfo field = typeof(BotTabIndex).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field not found: " + fieldName);
            return (T)field.GetValue(tab)!;
        }

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
