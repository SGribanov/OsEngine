using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Entity;
using OsEngine.OsTrader.Panels.Tab;
using Xunit;

namespace OsEngine.Tests;

public class BotTabPairNamesPersistenceTests
{
    [Fact]
    public void SavePairNames_ShouldPersistJson()
    {
        const string tabName = "CodexPairNamesJson";
        using BotTabPairNamesFileScope scope = new BotTabPairNamesFileScope(tabName);

        BotTabPair source = scope.CreateWithoutConstructor();
        source.Pairs = new List<PairToTrade>
        {
            scope.CreatePairWithoutConstructor("PAIR_A"),
            scope.CreatePairWithoutConstructor("PAIR_B")
        };

        source.SavePairNames();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());
    }

    [Fact]
    public void ParseLegacyPairNamesToLoadSettings_ShouldSupportLineBasedFormat()
    {
        const string tabName = "CodexPairNamesLegacy";
        using BotTabPairNamesFileScope scope = new BotTabPairNamesFileScope(tabName);

        string legacy = string.Join("\n", "PAIR_X", "PAIR_Y", "PAIR_Z") + "\n";

        object settings = scope.ParseLegacy(legacy);
        Assert.NotNull(settings);

        List<string> names = scope.ExtractNames(settings);
        Assert.Equal(3, names.Count);
        Assert.Equal("PAIR_X", names[0]);
        Assert.Equal("PAIR_Y", names[1]);
        Assert.Equal("PAIR_Z", names[2]);
    }

    private sealed class BotTabPairNamesFileScope : IDisposable
    {
        private readonly string _tabName;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _parseLegacyMethod;
        private readonly FieldInfo _pairNameField;

        public BotTabPairNamesFileScope(string tabName)
        {
            _tabName = tabName;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, _tabName + "PairsNamesToLoad.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _parseLegacyMethod = typeof(BotTabPair).GetMethod(
                "ParseLegacyPairNamesToLoadSettings",
                BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method ParseLegacyPairNamesToLoadSettings not found.");
            _pairNameField = typeof(PairToTrade).GetField("Name", BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field PairToTrade.Name not found.");

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

        public BotTabPair CreateWithoutConstructor()
        {
            BotTabPair pair = (BotTabPair)RuntimeHelpers.GetUninitializedObject(typeof(BotTabPair));
            pair.TabName = _tabName;
            pair.StartProgram = StartProgram.IsOsOptimizer;
            return pair;
        }

        public PairToTrade CreatePairWithoutConstructor(string name)
        {
            PairToTrade pair = (PairToTrade)RuntimeHelpers.GetUninitializedObject(typeof(PairToTrade));
            _pairNameField.SetValue(pair, name);
            return pair;
        }

        public object ParseLegacy(string content)
        {
            return _parseLegacyMethod.Invoke(null, new object[] { content })!;
        }

        public List<string> ExtractNames(object settings)
        {
            PropertyInfo pairNamesProperty = settings.GetType().GetProperty("PairNames", BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException("PairNames property not found.");
            IEnumerable<string> values = (IEnumerable<string>)pairNamesProperty.GetValue(settings)!;
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
