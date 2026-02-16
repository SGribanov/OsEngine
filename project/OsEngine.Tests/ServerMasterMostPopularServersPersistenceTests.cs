using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.Market;
using Xunit;

namespace OsEngine.Tests;

public class ServerMasterMostPopularServersPersistenceTests
{
    [Fact]
    public void SaveMostPopularServers_ShouldPersistJson_AndLoadCounts()
    {
        using MostPopularServersFileScope scope = new MostPopularServersFileScope();
        scope.ClearFile();

        scope.InvokePrivateSaveMostPopularServers(ServerType.QuikLua);
        scope.InvokePrivateSaveMostPopularServers(ServerType.QuikLua);
        scope.InvokePrivateSaveMostPopularServers(ServerType.MetaTrader5);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        List<ServerPop> loaded = ServerMaster.LoadMostPopularServersWithCount();

        Assert.Equal(2, GetCount(loaded, ServerType.QuikLua));
        Assert.Equal(1, GetCount(loaded, ServerType.MetaTrader5));
    }

    [Fact]
    public void LoadMostPopularServersWithCount_ShouldSupportLegacyLineBasedFormat()
    {
        using MostPopularServersFileScope scope = new MostPopularServersFileScope();
        scope.ClearFile();

        string legacyContent = string.Join(
            Environment.NewLine,
            "QuikLua&5",
            "MetaTrader5&2");
        File.WriteAllText(scope.SettingsPath, legacyContent);

        List<ServerPop> loaded = ServerMaster.LoadMostPopularServersWithCount();

        Assert.Equal(5, GetCount(loaded, ServerType.QuikLua));
        Assert.Equal(2, GetCount(loaded, ServerType.MetaTrader5));
    }

    private static int GetCount(List<ServerPop> servers, ServerType type)
    {
        for (int i = 0; i < servers.Count; i++)
        {
            if (servers[i].ServerType == type)
            {
                return servers[i].CountOfCreation;
            }
        }

        return 0;
    }

    private sealed class MostPopularServersFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackupPath;
        private readonly MethodInfo _saveMostPopularServersMethod;

        public MostPopularServersFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "MostPopularServers.txt");
            _settingsBackupPath = SettingsPath + ".codex.bak";

            _saveMostPopularServersMethod = typeof(ServerMaster).GetMethod("SaveMostPopularServers", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Method SaveMostPopularServers not found.");

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

        public void ClearFile()
        {
            if (File.Exists(SettingsPath))
            {
                File.Delete(SettingsPath);
            }
        }

        public void InvokePrivateSaveMostPopularServers(ServerType type)
        {
            _saveMostPopularServersMethod.Invoke(null, new object[] { type });
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
