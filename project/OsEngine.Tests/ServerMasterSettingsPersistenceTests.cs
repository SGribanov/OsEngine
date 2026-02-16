using System;
using System.IO;
using System.Linq;
using OsEngine.Market;
using Xunit;

namespace OsEngine.Tests;

public class ServerMasterSettingsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using ServerMasterSettingsFileScope scope = new ServerMasterSettingsFileScope();

        ServerMaster.NeedToConnectAuto = true;
        ServerMaster.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        ServerMaster.NeedToConnectAuto = false;
        ServerMaster.Load();

        Assert.True(ServerMaster.NeedToConnectAuto);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using ServerMasterSettingsFileScope scope = new ServerMasterSettingsFileScope();

        File.WriteAllText(scope.SettingsPath, "False");
        ServerMaster.NeedToConnectAuto = true;

        ServerMaster.Load();

        Assert.False(ServerMaster.NeedToConnectAuto);
    }

    private sealed class ServerMasterSettingsFileScope : IDisposable
    {
        private readonly bool _needToConnectAutoBackup;
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;

        public ServerMasterSettingsFileScope()
        {
            _needToConnectAutoBackup = ServerMaster.NeedToConnectAuto;
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "ServerMaster.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

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

        public void Dispose()
        {
            ServerMaster.NeedToConnectAuto = _needToConnectAutoBackup;

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
