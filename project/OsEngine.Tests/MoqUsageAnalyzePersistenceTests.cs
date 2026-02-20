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

public class MoqUsageAnalyzePersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using MoqMemorySettingsFileScope scope = new MoqMemorySettingsFileScope();

        MoqUsageAnalyze source = scope.CreateWithoutConstructor();
        scope.SetPrivateField(source, "_moqCollectDataIsOn", true);
        scope.SetPrivateField(source, "_moqPeriodSavePoint", SavePointPeriod.TenSeconds);
        scope.SetPrivateField(source, "_moqPointsMax", 432);
        scope.InvokeSave(source);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        MoqUsageAnalyze loaded = scope.CreateWithoutConstructor();
        scope.SetPrivateField(loaded, "_moqCollectDataIsOn", false);
        scope.SetPrivateField(loaded, "_moqPeriodSavePoint", SavePointPeriod.OneSecond);
        scope.SetPrivateField(loaded, "_moqPointsMax", 100);
        scope.InvokeLoad(loaded);

        Assert.True(scope.GetPrivateField<bool>(loaded, "_moqCollectDataIsOn"));
        Assert.Equal(SavePointPeriod.TenSeconds, scope.GetPrivateField<SavePointPeriod>(loaded, "_moqPeriodSavePoint"));
        Assert.Equal(432, scope.GetPrivateField<int>(loaded, "_moqPointsMax"));
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using MoqMemorySettingsFileScope scope = new MoqMemorySettingsFileScope();

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "True",
            "Minute",
            "852"
        });

        MoqUsageAnalyze loaded = scope.CreateWithoutConstructor();
        scope.SetPrivateField(loaded, "_moqCollectDataIsOn", false);
        scope.SetPrivateField(loaded, "_moqPeriodSavePoint", SavePointPeriod.OneSecond);
        scope.SetPrivateField(loaded, "_moqPointsMax", 100);
        scope.InvokeLoad(loaded);

        Assert.True(scope.GetPrivateField<bool>(loaded, "_moqCollectDataIsOn"));
        Assert.Equal(SavePointPeriod.Minute, scope.GetPrivateField<SavePointPeriod>(loaded, "_moqPeriodSavePoint"));
        Assert.Equal(852, scope.GetPrivateField<int>(loaded, "_moqPointsMax"));
    }

    private sealed class MoqMemorySettingsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly string _systemStressDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _systemStressDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly MethodInfo _loadMethod;
        private readonly MethodInfo _saveMethod;

        public MoqMemorySettingsFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            _systemStressDirPath = Path.Combine(_engineDirPath, "SystemStress");
            SettingsPath = Path.Combine(_systemStressDirPath, "MoqMemorySettings.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _loadMethod = typeof(MoqUsageAnalyze).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Load not found.");
            _saveMethod = typeof(MoqUsageAnalyze).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance)
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

        public MoqUsageAnalyze CreateWithoutConstructor()
        {
            return (MoqUsageAnalyze)RuntimeHelpers.GetUninitializedObject(typeof(MoqUsageAnalyze));
        }

        public void InvokeLoad(MoqUsageAnalyze analyze)
        {
            _loadMethod.Invoke(analyze, null);
        }

        public void InvokeSave(MoqUsageAnalyze analyze)
        {
            _saveMethod.Invoke(analyze, null);
        }

        public void SetPrivateField(MoqUsageAnalyze analyze, string fieldName, object value)
        {
            FieldInfo field = typeof(MoqUsageAnalyze).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field not found: " + fieldName);
            field.SetValue(analyze, value);
        }

        public T GetPrivateField<T>(MoqUsageAnalyze analyze, string fieldName)
        {
            FieldInfo field = typeof(MoqUsageAnalyze).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
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
