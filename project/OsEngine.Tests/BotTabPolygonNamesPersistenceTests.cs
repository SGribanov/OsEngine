using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.OsTrader.Panels.Tab;
using Xunit;

namespace OsEngine.Tests;

public class BotTabPolygonNamesPersistenceTests
{
    [Fact]
    public void SaveSequencesNames_ShouldPersistJson()
    {
        const string tabName = "CodexPolygonNamesJson";
        using BotTabPolygonNamesFileScope scope = new BotTabPolygonNamesFileScope(tabName);

        BotTabPolygon source = scope.CreateWithoutConstructor();
        source.Sequences = new List<PolygonToTrade>
        {
            scope.CreateSequenceWithoutConstructor("POLYGON_A"),
            scope.CreateSequenceWithoutConstructor("POLYGON_B")
        };

        source.SaveSequencesNames();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());
    }

    [Fact]
    public void ParseLegacyPolygonNamesToLoadSettings_ShouldSupportLineBasedFormat()
    {
        const string tabName = "CodexPolygonNamesLegacy";
        using BotTabPolygonNamesFileScope scope = new BotTabPolygonNamesFileScope(tabName);

        string legacy = string.Join("\n", "POLYGON_X", "POLYGON_Y", "POLYGON_Z") + "\n";

        object settings = scope.ParseLegacy(legacy);
        Assert.NotNull(settings);

        List<string> names = scope.ExtractNames(settings);
        Assert.Equal(3, names.Count);
        Assert.Equal("POLYGON_X", names[0]);
        Assert.Equal("POLYGON_Y", names[1]);
        Assert.Equal("POLYGON_Z", names[2]);
    }

    private sealed class BotTabPolygonNamesFileScope : IDisposable
    {
        private readonly string _tabName;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _parseLegacyMethod;

        public BotTabPolygonNamesFileScope(string tabName)
        {
            _tabName = tabName;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, _tabName + "PolygonsNamesToLoad.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _parseLegacyMethod = typeof(BotTabPolygon).GetMethod(
                "ParseLegacyPolygonNamesToLoadSettings",
                BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method ParseLegacyPolygonNamesToLoadSettings not found.");

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

        public BotTabPolygon CreateWithoutConstructor()
        {
            BotTabPolygon tab = (BotTabPolygon)RuntimeHelpers.GetUninitializedObject(typeof(BotTabPolygon));
            tab.TabName = _tabName;
            tab.Sequences = new List<PolygonToTrade>();
            return tab;
        }

        public PolygonToTrade CreateSequenceWithoutConstructor(string name)
        {
            PolygonToTrade sequence = (PolygonToTrade)RuntimeHelpers.GetUninitializedObject(typeof(PolygonToTrade));
            sequence.Name = name;
            return sequence;
        }

        public object ParseLegacy(string content)
        {
            return _parseLegacyMethod.Invoke(null, new object[] { content })!;
        }

        public List<string> ExtractNames(object settings)
        {
            PropertyInfo namesProperty = settings.GetType().GetProperty("SequenceNames", BindingFlags.Public | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Property SequenceNames not found.");
            IEnumerable<string> values = (IEnumerable<string>)namesProperty.GetValue(settings)!;
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
