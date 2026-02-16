using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.OsTrader.SystemAnalyze;
using Xunit;

namespace OsEngine.Tests;

public class RamMemoryUsageAnalyzePersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using RamMemorySettingsFileScope scope = new RamMemorySettingsFileScope();

        RamMemoryUsageAnalyze source = scope.CreateWithoutConstructor();
        scope.SetPrivateField(source, "_ramCollectDataIsOn", true);
        scope.SetPrivateField(source, "_ramPeriodSavePoint", SavePointPeriod.TenSeconds);
        scope.SetPrivateField(source, "_ramPointsMax", 777);
        scope.InvokeSave(source);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        RamMemoryUsageAnalyze loaded = scope.CreateWithoutConstructor();
        scope.SetPrivateField(loaded, "_ramCollectDataIsOn", false);
        scope.SetPrivateField(loaded, "_ramPeriodSavePoint", SavePointPeriod.OneSecond);
        scope.SetPrivateField(loaded, "_ramPointsMax", 100);
        scope.InvokeLoad(loaded);

        Assert.True(scope.GetPrivateField<bool>(loaded, "_ramCollectDataIsOn"));
        Assert.Equal(SavePointPeriod.TenSeconds, scope.GetPrivateField<SavePointPeriod>(loaded, "_ramPeriodSavePoint"));
        Assert.Equal(777, scope.GetPrivateField<int>(loaded, "_ramPointsMax"));
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using RamMemorySettingsFileScope scope = new RamMemorySettingsFileScope();

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "True",
            "Minute",
            "321"
        });

        RamMemoryUsageAnalyze loaded = scope.CreateWithoutConstructor();
        scope.SetPrivateField(loaded, "_ramCollectDataIsOn", false);
        scope.SetPrivateField(loaded, "_ramPeriodSavePoint", SavePointPeriod.OneSecond);
        scope.SetPrivateField(loaded, "_ramPointsMax", 100);
        scope.InvokeLoad(loaded);

        Assert.True(scope.GetPrivateField<bool>(loaded, "_ramCollectDataIsOn"));
        Assert.Equal(SavePointPeriod.Minute, scope.GetPrivateField<SavePointPeriod>(loaded, "_ramPeriodSavePoint"));
        Assert.Equal(321, scope.GetPrivateField<int>(loaded, "_ramPointsMax"));
    }

    private sealed class RamMemorySettingsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly string _systemStressDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _systemStressDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly MethodInfo _loadMethod;
        private readonly MethodInfo _saveMethod;

        public RamMemorySettingsFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            _systemStressDirPath = Path.Combine(_engineDirPath, "SystemStress");
            SettingsPath = Path.Combine(_systemStressDirPath, "RamMemorySettings.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _loadMethod = typeof(RamMemoryUsageAnalyze).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Load not found.");
            _saveMethod = typeof(RamMemoryUsageAnalyze).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Save not found.");

            _engineDirExisted = Directory.Exists(_engineDirPath);
            if (!_engineDirExisted)
            {
                Directory.CreateDirectory(_engineDirPath);
            }

            _systemStressDirExisted = Directory.Exists(_systemStressDirPath);
            if (!_systemStressDirExisted)
            {
                Directory.CreateDirectory(_systemStressDirPath);
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

        public string SettingsPath { get; }

        public RamMemoryUsageAnalyze CreateWithoutConstructor()
        {
            return (RamMemoryUsageAnalyze)RuntimeHelpers.GetUninitializedObject(typeof(RamMemoryUsageAnalyze));
        }

        public void InvokeLoad(RamMemoryUsageAnalyze analyze)
        {
            _loadMethod.Invoke(analyze, null);
        }

        public void InvokeSave(RamMemoryUsageAnalyze analyze)
        {
            _saveMethod.Invoke(analyze, null);
        }

        public void SetPrivateField(RamMemoryUsageAnalyze analyze, string fieldName, object value)
        {
            FieldInfo field = typeof(RamMemoryUsageAnalyze).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field not found: " + fieldName);
            field.SetValue(analyze, value);
        }

        public T GetPrivateField<T>(RamMemoryUsageAnalyze analyze, string fieldName)
        {
            FieldInfo field = typeof(RamMemoryUsageAnalyze).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field not found: " + fieldName);
            return (T)field.GetValue(analyze)!;
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

            if (!_systemStressDirExisted
                && Directory.Exists(_systemStressDirPath)
                && !Directory.EnumerateFileSystemEntries(_systemStressDirPath).Any())
            {
                Directory.Delete(_systemStressDirPath);
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
