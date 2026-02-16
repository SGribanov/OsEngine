using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Charts.ClusterChart;
using OsEngine.OsTrader.Panels.Tab;
using Xunit;

namespace OsEngine.Tests;

public class ChartClusterMasterPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string name = "CodexClusterMasterJson";
        using ChartClusterMasterFileScope scope = new ChartClusterMasterFileScope(name);

        ChartClusterMaster source = scope.CreateWithoutConstructor();
        scope.SetChartType(source, ClusterType.DeltaVolume);
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        ChartClusterMaster loaded = scope.CreateWithoutConstructor();
        loaded.Load();
        Assert.Equal(ClusterType.DeltaVolume, scope.GetChartType(loaded));
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string name = "CodexClusterMasterLegacy";
        using ChartClusterMasterFileScope scope = new ChartClusterMasterFileScope(name);

        File.WriteAllLines(scope.SettingsPath, new[] { "SellVolume" });

        ChartClusterMaster loaded = scope.CreateWithoutConstructor();
        loaded.Load();
        Assert.Equal(ClusterType.SellVolume, scope.GetChartType(loaded));
    }

    private sealed class ChartClusterMasterFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly FieldInfo _nameField;
        private readonly FieldInfo _chartTypeField;

        public ChartClusterMasterFileScope(string name)
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, name + "ClusterChartMasterSet.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _nameField = typeof(ChartClusterMaster).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _name not found.");
            _chartTypeField = typeof(ChartClusterMaster).GetField("_chartType", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _chartType not found.");

            Name = name;

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

        public string Name { get; }

        public string SettingsPath { get; }

        public ChartClusterMaster CreateWithoutConstructor()
        {
            ChartClusterMaster instance =
                (ChartClusterMaster)RuntimeHelpers.GetUninitializedObject(typeof(ChartClusterMaster));
            _nameField.SetValue(instance, Name);
            _chartTypeField.SetValue(instance, ClusterType.SummVolume);
            return instance;
        }

        public void SetChartType(ChartClusterMaster master, ClusterType value)
        {
            _chartTypeField.SetValue(master, value);
        }

        public ClusterType GetChartType(ChartClusterMaster master)
        {
            return (ClusterType)_chartTypeField.GetValue(master)!;
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
