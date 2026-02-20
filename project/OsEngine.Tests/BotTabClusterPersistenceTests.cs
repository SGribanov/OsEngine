#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.OsTrader.Panels.Tab;
using Xunit;

namespace OsEngine.Tests;

public class BotTabClusterPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string tabName = "CodexBotTabClusterJson";
        using BotTabClusterFileScope scope = new BotTabClusterFileScope(tabName);

        BotTabCluster source = scope.CreateWithoutConstructor();
        scope.SetEventsIsOn(source, false);
        scope.InvokePrivateSave(source);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        BotTabCluster loaded = scope.CreateWithoutConstructor();
        scope.SetEventsIsOn(loaded, true);
        scope.InvokePrivateLoad(loaded);
        Assert.False(scope.GetEventsIsOn(loaded));
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string tabName = "CodexBotTabClusterLegacy";
        using BotTabClusterFileScope scope = new BotTabClusterFileScope(tabName);

        File.WriteAllLines(scope.SettingsPath, new[] { "False" });

        BotTabCluster loaded = scope.CreateWithoutConstructor();
        scope.SetEventsIsOn(loaded, true);
        scope.InvokePrivateLoad(loaded);
        Assert.False(scope.GetEventsIsOn(loaded));
    }

    private sealed class BotTabClusterFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly FieldInfo _eventsIsOnField;
        private readonly MethodInfo _saveMethod;
        private readonly MethodInfo _loadMethod;

        public BotTabClusterFileScope(string tabName)
        {
            TabName = tabName;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, tabName + "ClusterOnOffSet.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _eventsIsOnField = typeof(BotTabCluster).GetField("_eventsIsOn", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _eventsIsOn not found.");
            _saveMethod = typeof(BotTabCluster).GetMethod("Save", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Save not found.");
            _loadMethod = typeof(BotTabCluster).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Load not found.");

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

        public BotTabCluster CreateWithoutConstructor()
        {
            BotTabCluster instance =
                (BotTabCluster)RuntimeHelpers.GetUninitializedObject(typeof(BotTabCluster));
            instance.TabName = TabName;
            _eventsIsOnField.SetValue(instance, true);
            return instance;
        }

        public void SetEventsIsOn(BotTabCluster tab, bool value)
        {
            _eventsIsOnField.SetValue(tab, value);
        }

        public bool GetEventsIsOn(BotTabCluster tab)
        {
            return (bool)_eventsIsOnField.GetValue(tab)!;
        }

        public void InvokePrivateSave(BotTabCluster tab)
        {
            _saveMethod.Invoke(tab, null);
        }

        public void InvokePrivateLoad(BotTabCluster tab)
        {
            _loadMethod.Invoke(tab, null);
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
