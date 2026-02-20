#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.OsTrader.SystemAnalyze;
using Xunit;

namespace OsEngine.Tests;

public class CpuUsageAnalyzePersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using CpuMemorySettingsFileScope scope = new CpuMemorySettingsFileScope();

        CpuUsageAnalyze source = scope.CreateWithoutConstructor();
        scope.SetPrivateField(source, "_cpuCollectDataIsOn", true);
        scope.SetPrivateField(source, "_cpuPeriodSavePoint", SavePointPeriod.TenSeconds);
        scope.SetPrivateField(source, "_cpuPointsMax", 888);
        scope.InvokeSave(source);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        CpuUsageAnalyze loaded = scope.CreateWithoutConstructor();
        scope.SetPrivateField(loaded, "_cpuCollectDataIsOn", false);
        scope.SetPrivateField(loaded, "_cpuPeriodSavePoint", SavePointPeriod.OneSecond);
        scope.SetPrivateField(loaded, "_cpuPointsMax", 100);
        scope.InvokeLoad(loaded);

        Assert.True(scope.GetPrivateField<bool>(loaded, "_cpuCollectDataIsOn"));
        Assert.Equal(SavePointPeriod.TenSeconds, scope.GetPrivateField<SavePointPeriod>(loaded, "_cpuPeriodSavePoint"));
        Assert.Equal(888, scope.GetPrivateField<int>(loaded, "_cpuPointsMax"));
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using CpuMemorySettingsFileScope scope = new CpuMemorySettingsFileScope();

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "True",
            "Minute",
            "654"
        });

        CpuUsageAnalyze loaded = scope.CreateWithoutConstructor();
        scope.SetPrivateField(loaded, "_cpuCollectDataIsOn", false);
        scope.SetPrivateField(loaded, "_cpuPeriodSavePoint", SavePointPeriod.OneSecond);
        scope.SetPrivateField(loaded, "_cpuPointsMax", 100);
        scope.InvokeLoad(loaded);

        Assert.True(scope.GetPrivateField<bool>(loaded, "_cpuCollectDataIsOn"));
        Assert.Equal(SavePointPeriod.Minute, scope.GetPrivateField<SavePointPeriod>(loaded, "_cpuPeriodSavePoint"));
        Assert.Equal(654, scope.GetPrivateField<int>(loaded, "_cpuPointsMax"));
    }

    private sealed class CpuMemorySettingsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly string _systemStressDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _systemStressDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly MethodInfo _loadMethod;
        private readonly MethodInfo _saveMethod;

        public CpuMemorySettingsFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            _systemStressDirPath = Path.Combine(_engineDirPath, "SystemStress");
            SettingsPath = Path.Combine(_systemStressDirPath, "CpuMemorySettings.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _loadMethod = typeof(CpuUsageAnalyze).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Load not found.");
            _saveMethod = typeof(CpuUsageAnalyze).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance)
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

        public CpuUsageAnalyze CreateWithoutConstructor()
        {
            return (CpuUsageAnalyze)RuntimeHelpers.GetUninitializedObject(typeof(CpuUsageAnalyze));
        }

        public void InvokeLoad(CpuUsageAnalyze analyze)
        {
            _loadMethod.Invoke(analyze, null);
        }

        public void InvokeSave(CpuUsageAnalyze analyze)
        {
            _saveMethod.Invoke(analyze, null);
        }

        public void SetPrivateField(CpuUsageAnalyze analyze, string fieldName, object value)
        {
            FieldInfo field = typeof(CpuUsageAnalyze).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field not found: " + fieldName);
            field.SetValue(analyze, value);
        }

        public T GetPrivateField<T>(CpuUsageAnalyze analyze, string fieldName)
        {
            FieldInfo field = typeof(CpuUsageAnalyze).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
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
