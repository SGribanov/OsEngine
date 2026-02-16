using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.Market.Proxy;
using Xunit;

namespace OsEngine.Tests;

public class ProxyMasterSettingsPersistenceTests
{
    [Fact]
    public void SaveSettings_ShouldPersistJson_AndLoadRoundTrip()
    {
        using ProxyMasterSettingsFileScope scope = new ProxyMasterSettingsFileScope();

        ProxyMaster source = new ProxyMaster
        {
            AutoPingIsOn = false,
            AutoPingLastTime = new DateTime(2026, 1, 1, 12, 30, 0),
            AutoPingMinutes = 17
        };

        source.SaveSettings();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        ProxyMaster target = new ProxyMaster
        {
            AutoPingIsOn = true,
            AutoPingLastTime = DateTime.MinValue,
            AutoPingMinutes = 1
        };

        scope.InvokePrivateLoadSettings(target);

        Assert.False(target.AutoPingIsOn);
        Assert.Equal(new DateTime(2026, 1, 1, 12, 30, 0), target.AutoPingLastTime);
        Assert.Equal(17, target.AutoPingMinutes);
    }

    [Fact]
    public void LoadSettings_ShouldSupportLegacyLineBasedFormat()
    {
        using ProxyMasterSettingsFileScope scope = new ProxyMasterSettingsFileScope();

        DateTime expectedTime = new DateTime(2025, 5, 10, 9, 8, 7);
        string legacyContent = string.Join(
            Environment.NewLine,
            "False",
            expectedTime.ToString("O", CultureInfo.InvariantCulture),
            "25");
        File.WriteAllText(scope.SettingsPath, legacyContent);

        ProxyMaster target = new ProxyMaster();
        scope.InvokePrivateLoadSettings(target);

        Assert.False(target.AutoPingIsOn);
        Assert.Equal(expectedTime, target.AutoPingLastTime);
        Assert.Equal(25, target.AutoPingMinutes);
    }

    private sealed class ProxyMasterSettingsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _loadSettingsMethod;

        public ProxyMasterSettingsFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "ProxyMaster.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _loadSettingsMethod = typeof(ProxyMaster).GetMethod("LoadSettings", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method LoadSettings not found.");

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

        public void InvokePrivateLoadSettings(ProxyMaster target)
        {
            _loadSettingsMethod.Invoke(target, null);
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
