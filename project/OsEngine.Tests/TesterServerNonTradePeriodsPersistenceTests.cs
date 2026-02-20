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

public class TesterServerNonTradePeriodsPersistenceTests
{
    [Fact]
    public void SaveNonTradePeriods_ShouldPersistJson_AndLoadRoundTrip()
    {
        using TesterServerNonTradePeriodsFileScope scope = new TesterServerNonTradePeriodsFileScope();

        TesterServer source = scope.CreateWithoutConstructor();
        source.NonTradePeriods = new List<NonTradePeriod>
        {
            new NonTradePeriod
            {
                DateStart = new DateTime(2025, 1, 1, 10, 0, 0),
                DateEnd = new DateTime(2025, 1, 1, 11, 0, 0),
                IsOn = true
            },
            new NonTradePeriod
            {
                DateStart = new DateTime(2025, 1, 2, 12, 0, 0),
                DateEnd = new DateTime(2025, 1, 2, 13, 0, 0),
                IsOn = false
            }
        };
        source.SaveNonTradePeriods();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        TesterServer loaded = scope.CreateWithoutConstructor();
        loaded.NonTradePeriods = new List<NonTradePeriod>();
        scope.InvokePrivateLoadNonTradePeriods(loaded);

        Assert.Equal(2, loaded.NonTradePeriods.Count);
        Assert.Equal(new DateTime(2025, 1, 1, 10, 0, 0), loaded.NonTradePeriods[0].DateStart);
        Assert.Equal(new DateTime(2025, 1, 1, 11, 0, 0), loaded.NonTradePeriods[0].DateEnd);
        Assert.True(loaded.NonTradePeriods[0].IsOn);
        Assert.Equal(new DateTime(2025, 1, 2, 12, 0, 0), loaded.NonTradePeriods[1].DateStart);
        Assert.Equal(new DateTime(2025, 1, 2, 13, 0, 0), loaded.NonTradePeriods[1].DateEnd);
        Assert.False(loaded.NonTradePeriods[1].IsOn);
    }

    [Fact]
    public void LoadNonTradePeriods_ShouldSupportLegacyLineBasedFormat()
    {
        using TesterServerNonTradePeriodsFileScope scope = new TesterServerNonTradePeriodsFileScope();

        NonTradePeriod first = new NonTradePeriod
        {
            DateStart = new DateTime(2024, 5, 1, 14, 0, 0),
            DateEnd = new DateTime(2024, 5, 1, 15, 0, 0),
            IsOn = true
        };

        NonTradePeriod second = new NonTradePeriod
        {
            DateStart = new DateTime(2024, 5, 2, 16, 0, 0),
            DateEnd = new DateTime(2024, 5, 2, 17, 0, 0),
            IsOn = false
        };

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            first.GetSaveString(),
            second.GetSaveString()
        });

        TesterServer loaded = scope.CreateWithoutConstructor();
        loaded.NonTradePeriods = new List<NonTradePeriod>();
        scope.InvokePrivateLoadNonTradePeriods(loaded);

        Assert.Equal(2, loaded.NonTradePeriods.Count);
        Assert.Equal(first.DateStart, loaded.NonTradePeriods[0].DateStart);
        Assert.Equal(first.DateEnd, loaded.NonTradePeriods[0].DateEnd);
        Assert.Equal(first.IsOn, loaded.NonTradePeriods[0].IsOn);
        Assert.Equal(second.DateStart, loaded.NonTradePeriods[1].DateStart);
        Assert.Equal(second.DateEnd, loaded.NonTradePeriods[1].DateEnd);
        Assert.Equal(second.IsOn, loaded.NonTradePeriods[1].IsOn);
    }

    private sealed class TesterServerNonTradePeriodsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _loadNonTradePeriodsMethod;

        public TesterServerNonTradePeriodsFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "TestServerNonTradePeriods.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _loadNonTradePeriodsMethod = typeof(TesterServer).GetMethod("LoadNonTradePeriods", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method LoadNonTradePeriods not found.");

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

        public void InvokePrivateLoadNonTradePeriods(TesterServer server)
        {
            _loadNonTradePeriodsMethod.Invoke(server, null);
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
