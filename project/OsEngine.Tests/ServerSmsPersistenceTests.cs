using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.Logging;
using Xunit;

namespace OsEngine.Tests;

public class ServerSmsPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using ServerSmsFileScope scope = new ServerSmsFileScope();

        ServerSms source = ServerSms.GetSmsServer();
        source.SmscLogin = "json_login";
        source.SmscPassword = "json_password";
        source.Phones = "+10000000000";
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        scope.ResetSingleton();
        ServerSms loaded = ServerSms.GetSmsServer();

        Assert.Equal("json_login", loaded.SmscLogin);
        Assert.Equal("json_password", loaded.SmscPassword);
        Assert.Equal("+10000000000", loaded.Phones);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using ServerSmsFileScope scope = new ServerSmsFileScope();

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "legacy_login",
            "legacy_password",
            "+79990000000"
        });

        scope.ResetSingleton();
        ServerSms loaded = ServerSms.GetSmsServer();

        Assert.Equal("legacy_login", loaded.SmscLogin);
        Assert.Equal("legacy_password", loaded.SmscPassword);
        Assert.Equal("+79990000000", loaded.Phones);
    }

    private sealed class ServerSmsFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly FieldInfo _serverField;
        private readonly object? _originalServer;

        public ServerSmsFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "smsSet.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _serverField = typeof(ServerSms).GetField("_server", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Field _server not found.");
            _originalServer = _serverField.GetValue(null);
            _serverField.SetValue(null, null);

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

        public void ResetSingleton()
        {
            _serverField.SetValue(null, null);
        }

        public void Dispose()
        {
            _serverField.SetValue(null, _originalServer);

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
