#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.OsTrader.Panels.Tab;
using Xunit;

namespace OsEngine.Tests;

public class BotTabScreenerIndicatorsPersistenceTests
{
    [Fact]
    public void SaveIndicators_ShouldPersistJson_AndLoadRoundTrip()
    {
        using BotTabScreenerIndicatorsFileScope scope = new BotTabScreenerIndicatorsFileScope("CodexScreenerIndicators");

        BotTabScreener source = scope.CreateWithoutConstructor();
        scope.Setup(source);
        source._indicators.Add(new IndicatorOnTabs
        {
            Type = "Sma",
            NameArea = "Prime",
            Num = 1,
            CanDelete = false,
            Parameters = new List<string> { "10", "20" }
        });

        scope.InvokePrivateSaveIndicators(source);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        BotTabScreener target = scope.CreateWithoutConstructor();
        scope.Setup(target);
        scope.InvokePrivateLoadIndicators(target);

        Assert.Single(target._indicators);
        IndicatorOnTabs loaded = target._indicators[0];
        Assert.Equal("Sma", loaded.Type);
        Assert.Equal("Prime", loaded.NameArea);
        Assert.Equal(1, loaded.Num);
        Assert.False(loaded.CanDelete);
        Assert.Equal(new List<string> { "10", "20" }, loaded.Parameters);
    }

    [Fact]
    public void LoadIndicators_ShouldSupportLegacyLineBasedFormat()
    {
        using BotTabScreenerIndicatorsFileScope scope = new BotTabScreenerIndicatorsFileScope("CodexScreenerLegacy");

        IndicatorOnTabs legacy = new IndicatorOnTabs
        {
            Type = "Rsi",
            NameArea = "Prime",
            Num = 2,
            CanDelete = true,
            Parameters = new List<string> { "14" }
        };
        File.WriteAllText(scope.SettingsPath, legacy.GetSaveStr());

        BotTabScreener target = scope.CreateWithoutConstructor();
        scope.Setup(target);
        scope.InvokePrivateLoadIndicators(target);

        Assert.Single(target._indicators);
        IndicatorOnTabs loaded = target._indicators[0];
        Assert.Equal("Rsi", loaded.Type);
        Assert.Equal(2, loaded.Num);
        Assert.True(loaded.CanDelete);
        Assert.Equal(new List<string> { "14" }, loaded.Parameters);
    }

    private sealed class BotTabScreenerIndicatorsFileScope : IDisposable
    {
        private readonly string _tabName;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _saveIndicatorsMethod;
        private readonly MethodInfo _loadIndicatorsMethod;

        public BotTabScreenerIndicatorsFileScope(string tabName)
        {
            _tabName = tabName;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, $"{_tabName}ScreenerIndicators.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _saveIndicatorsMethod = typeof(BotTabScreener).GetMethod("SaveIndicators", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method SaveIndicators not found.");
            _loadIndicatorsMethod = typeof(BotTabScreener).GetMethod("LoadIndicators", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method LoadIndicators not found.");

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

        public BotTabScreener CreateWithoutConstructor()
        {
            return (BotTabScreener)RuntimeHelpers.GetUninitializedObject(typeof(BotTabScreener));
        }

        public void Setup(BotTabScreener tab)
        {
            tab.TabName = _tabName;
            tab._indicators = new List<IndicatorOnTabs>();
        }

        public void InvokePrivateSaveIndicators(BotTabScreener tab)
        {
            _saveIndicatorsMethod.Invoke(tab, null);
        }

        public void InvokePrivateLoadIndicators(BotTabScreener tab)
        {
            _loadIndicatorsMethod.Invoke(tab, null);
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
