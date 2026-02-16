using System;
using System.IO;
using System.Linq;
using OsEngine.Market.Servers.YahooFinance;
using Xunit;

namespace OsEngine.Tests;

public class AServerSettingsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using AServerSettingsFileScope scope = new AServerSettingsFileScope("YahooFinance");

        YahooServer source = new YahooServer();
        source.ServerPrefix = "prefix_json";

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        YahooServer loaded = new YahooServer();
        Assert.Equal("prefix_json", loaded.ServerPrefix);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using AServerSettingsFileScope scope = new AServerSettingsFileScope("YahooFinance");
        File.WriteAllLines(scope.SettingsPath, new[] { "legacy_prefix" });

        YahooServer loaded = new YahooServer();
        Assert.Equal("legacy_prefix", loaded.ServerPrefix);
    }

    private sealed class AServerSettingsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;

        public AServerSettingsFileScope(string serverNameUnique)
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, serverNameUnique + "ServerSettings.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

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

        public string SettingsPath { get; }

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
