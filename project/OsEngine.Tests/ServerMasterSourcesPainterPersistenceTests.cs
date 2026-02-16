using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Market;
using Xunit;

namespace OsEngine.Tests;

public class ServerMasterSourcesPainterPersistenceTests
{
    [Fact]
    public void SaveAttachedServers_ShouldPersistJson_AndLoadRoundTrip()
    {
        using AttachedServersFileScope scope = new AttachedServersFileScope();

        ServerMasterSourcesPainter source = scope.CreateWithoutConstructor();
        scope.SetAttachedServers(source, new List<ServerType> { ServerType.Binance, ServerType.Bybit });
        scope.InvokeSaveAttachedServers(source);

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        ServerMasterSourcesPainter loaded = scope.CreateWithoutConstructor();
        scope.SetAttachedServers(loaded, new List<ServerType>());
        scope.InvokeLoadAttachedServers(loaded);

        List<ServerType> loadedServers = scope.GetAttachedServers(loaded);
        Assert.Equal(new[] { ServerType.Binance, ServerType.Bybit }, loadedServers);
    }

    [Fact]
    public void LoadAttachedServers_ShouldSupportLegacyLineBasedFormat()
    {
        using AttachedServersFileScope scope = new AttachedServersFileScope();

        File.WriteAllLines(scope.SettingsPath, new[] { "Binance", "Bybit" });

        ServerMasterSourcesPainter loaded = scope.CreateWithoutConstructor();
        scope.SetAttachedServers(loaded, new List<ServerType>());
        scope.InvokeLoadAttachedServers(loaded);

        List<ServerType> loadedServers = scope.GetAttachedServers(loaded);
        Assert.Equal(new[] { ServerType.Binance, ServerType.Bybit }, loadedServers);
    }

    private sealed class AttachedServersFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly FieldInfo _attachedServersField;
        private readonly MethodInfo _saveAttachedServersMethod;
        private readonly MethodInfo _loadAttachedServersMethod;

        public AttachedServersFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "AttachedServers.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _attachedServersField = typeof(ServerMasterSourcesPainter).GetField("_attachedServers", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Field _attachedServers not found.");
            _saveAttachedServersMethod = typeof(ServerMasterSourcesPainter).GetMethod("SaveAttachedServers", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method SaveAttachedServers not found.");
            _loadAttachedServersMethod = typeof(ServerMasterSourcesPainter).GetMethod("LoadAttachedServers", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method LoadAttachedServers not found.");

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

        public ServerMasterSourcesPainter CreateWithoutConstructor()
        {
            return (ServerMasterSourcesPainter)RuntimeHelpers.GetUninitializedObject(typeof(ServerMasterSourcesPainter));
        }

        public void SetAttachedServers(ServerMasterSourcesPainter painter, List<ServerType> servers)
        {
            _attachedServersField.SetValue(painter, servers);
        }

        public List<ServerType> GetAttachedServers(ServerMasterSourcesPainter painter)
        {
            return (List<ServerType>)_attachedServersField.GetValue(painter)!;
        }

        public void InvokeSaveAttachedServers(ServerMasterSourcesPainter painter)
        {
            _saveAttachedServersMethod.Invoke(painter, null);
        }

        public void InvokeLoadAttachedServers(ServerMasterSourcesPainter painter)
        {
            _loadAttachedServersMethod.Invoke(painter, null);
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
