#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Market.Servers.Tester;
using Xunit;

namespace OsEngine.Tests;

public class TesterServerClearingPersistenceTests
{
    [Fact]
    public void SaveClearingInfo_ShouldPersistJson_AndLoadRoundTrip()
    {
        using TesterServerClearingFileScope scope = new TesterServerClearingFileScope();

        TesterServer source = scope.CreateWithoutConstructor();
        source.ClearingTimes = new List<OrderClearing>
        {
            new OrderClearing
            {
                Time = new DateTime(2000, 1, 1, 10, 0, 0),
                IsOn = true
            },
            new OrderClearing
            {
                Time = new DateTime(2000, 1, 1, 19, 30, 0),
                IsOn = false
            }
        };
        source.SaveClearingInfo();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        TesterServer loaded = scope.CreateWithoutConstructor();
        loaded.ClearingTimes = new List<OrderClearing>();
        scope.InvokePrivateLoadClearingInfo(loaded);

        Assert.Equal(2, loaded.ClearingTimes.Count);
        Assert.Equal(new DateTime(2000, 1, 1, 10, 0, 0), loaded.ClearingTimes[0].Time);
        Assert.True(loaded.ClearingTimes[0].IsOn);
        Assert.Equal(new DateTime(2000, 1, 1, 19, 30, 0), loaded.ClearingTimes[1].Time);
        Assert.False(loaded.ClearingTimes[1].IsOn);
    }

    [Fact]
    public void LoadClearingInfo_ShouldSupportLegacyLineBasedFormat()
    {
        using TesterServerClearingFileScope scope = new TesterServerClearingFileScope();

        OrderClearing first = new OrderClearing
        {
            Time = new DateTime(2000, 1, 1, 11, 0, 0),
            IsOn = true
        };

        OrderClearing second = new OrderClearing
        {
            Time = new DateTime(2000, 1, 1, 20, 0, 0),
            IsOn = false
        };

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            first.GetSaveString(),
            second.GetSaveString()
        });

        TesterServer loaded = scope.CreateWithoutConstructor();
        loaded.ClearingTimes = new List<OrderClearing>();
        scope.InvokePrivateLoadClearingInfo(loaded);

        Assert.Equal(2, loaded.ClearingTimes.Count);
        Assert.Equal(first.Time, loaded.ClearingTimes[0].Time);
        Assert.Equal(first.IsOn, loaded.ClearingTimes[0].IsOn);
        Assert.Equal(second.Time, loaded.ClearingTimes[1].Time);
        Assert.Equal(second.IsOn, loaded.ClearingTimes[1].IsOn);
    }

    private sealed class TesterServerClearingFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _loadClearingInfoMethod;

        public TesterServerClearingFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "TestServerClearings.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _loadClearingInfoMethod = typeof(TesterServer).GetMethod("LoadClearingInfo", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method LoadClearingInfo not found.");

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

        public TesterServer CreateWithoutConstructor()
        {
            return (TesterServer)RuntimeHelpers.GetUninitializedObject(typeof(TesterServer));
        }

        public void InvokePrivateLoadClearingInfo(TesterServer server)
        {
            _loadClearingInfoMethod.Invoke(server, null);
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
