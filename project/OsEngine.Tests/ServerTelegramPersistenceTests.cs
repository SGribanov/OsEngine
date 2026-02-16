using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using OsEngine.Logging;
using Xunit;

namespace OsEngine.Tests;

public class ServerTelegramPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using ServerTelegramFileScope scope = new ServerTelegramFileScope();

        ServerTelegram source = scope.CreateServerWithoutConstructor();
        source.BotToken = "json-bot-token";
        source.ChatId = 123456789L;
        source.ProcessingCommand = true;
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        ServerTelegram loaded = scope.CreateServerWithoutConstructor();
        loaded.Load();

        Assert.Equal("json-bot-token", loaded.BotToken);
        Assert.Equal(123456789L, loaded.ChatId);
        Assert.True(loaded.ProcessingCommand);
        Assert.True(scope.GetIsReady());
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using ServerTelegramFileScope scope = new ServerTelegramFileScope();

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "legacy-token",
            "987654321",
            "false"
        });

        ServerTelegram loaded = scope.CreateServerWithoutConstructor();
        loaded.Load();

        Assert.Equal("legacy-token", loaded.BotToken);
        Assert.Equal(987654321L, loaded.ChatId);
        Assert.False(loaded.ProcessingCommand);
        Assert.True(scope.GetIsReady());
    }

    private sealed class ServerTelegramFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly FieldInfo _serverField;
        private readonly object? _originalServer;
        private readonly FieldInfo _isReadyField;
        private readonly bool _originalIsReady;

        public ServerTelegramFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "telegramSet.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _serverField = typeof(ServerTelegram).GetField("_server", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Field _server not found.");
            _originalServer = _serverField.GetValue(null);

            _isReadyField = typeof(ServerTelegram).GetField("_isReady", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Field _isReady not found.");
            _originalIsReady = (bool)_isReadyField.GetValue(null)!;

            _serverField.SetValue(null, null);
            _isReadyField.SetValue(null, false);

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

        public ServerTelegram CreateServerWithoutConstructor()
        {
            return (ServerTelegram)RuntimeHelpers.GetUninitializedObject(typeof(ServerTelegram));
        }

        public bool GetIsReady()
        {
            return (bool)_isReadyField.GetValue(null)!;
        }

        public void Dispose()
        {
            _serverField.SetValue(null, _originalServer);
            _isReadyField.SetValue(null, _originalIsReady);

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
