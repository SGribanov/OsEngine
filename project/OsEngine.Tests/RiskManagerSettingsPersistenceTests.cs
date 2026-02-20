#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.OsTrader.RiskManager;
using Xunit;

namespace OsEngine.Tests;

public class RiskManagerSettingsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using RiskManagerFileScope scope = new RiskManagerFileScope("CodexRiskManagerSettings");

        RiskManager source = scope.CreateWithoutConstructor();
        scope.SetName(source);
        source.MaxDrowDownToDayPersent = 12m;
        source.IsActiv = true;
        source.ReactionType = RiskManagerReactionType.CloseAndOff;
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        RiskManager target = scope.CreateWithoutConstructor();
        scope.SetName(target);
        scope.InvokePrivateLoad(target);

        Assert.Equal(12m, target.MaxDrowDownToDayPersent);
        Assert.True(target.IsActiv);
        Assert.Equal(RiskManagerReactionType.CloseAndOff, target.ReactionType);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using RiskManagerFileScope scope = new RiskManagerFileScope("CodexRiskManagerLegacy");

        string legacyContent = string.Join(
            Environment.NewLine,
            "7",
            "False",
            "ShowDialog");
        File.WriteAllText(scope.SettingsPath, legacyContent);

        RiskManager target = scope.CreateWithoutConstructor();
        scope.SetName(target);
        scope.InvokePrivateLoad(target);

        Assert.Equal(7m, target.MaxDrowDownToDayPersent);
        Assert.False(target.IsActiv);
        Assert.Equal(RiskManagerReactionType.ShowDialog, target.ReactionType);
    }

    private sealed class RiskManagerFileScope : IDisposable
    {
        private readonly string _name;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _loadMethod;
        private readonly FieldInfo _nameField;

        public RiskManagerFileScope(string name)
        {
            _name = name;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, $"{_name}.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _loadMethod = typeof(RiskManager).GetMethod("Load", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method Load not found.");
            _nameField = typeof(RiskManager).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _name not found.");

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

        public RiskManager CreateWithoutConstructor()
        {
            return (RiskManager)RuntimeHelpers.GetUninitializedObject(typeof(RiskManager));
        }

        public void SetName(RiskManager manager)
        {
            _nameField.SetValue(manager, _name);
        }

        public void InvokePrivateLoad(RiskManager manager)
        {
            _loadMethod.Invoke(manager, null);
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
