#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using OsEngine.Logging;
using Xunit;

namespace OsEngine.Tests;

public class ServerWebhookPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        using ServerWebhookFileScope scope = new ServerWebhookFileScope();

        ServerWebhook source = ServerWebhook.GetServer();
        source.SlackBotToken = "slack-token-json";
        source.Webhooks = new[]
        {
            "https://hooks.slack.com/services/T1/B1/W1",
            "https://example.org/my/webhook"
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        scope.ResetSingleton();
        ServerWebhook loaded = ServerWebhook.GetServer();

        Assert.Equal("slack-token-json", loaded.SlackBotToken);
        Assert.NotNull(loaded.Webhooks);
        Assert.Equal(2, loaded.Webhooks.Length);
        Assert.Equal("https://hooks.slack.com/services/T1/B1/W1", loaded.Webhooks[0]);
        Assert.Equal("https://example.org/my/webhook", loaded.Webhooks[1]);
        Assert.True(ServerWebhook.IsReady);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        using ServerWebhookFileScope scope = new ServerWebhookFileScope();

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "legacy-slack-token",
            "https://legacy.example/webhook/1",
            "https://legacy.example/webhook/2"
        });

        scope.ResetSingleton();
        ServerWebhook loaded = ServerWebhook.GetServer();

        Assert.Equal("legacy-slack-token", loaded.SlackBotToken);
        Assert.NotNull(loaded.Webhooks);
        Assert.Equal(2, loaded.Webhooks.Length);
        Assert.Equal("https://legacy.example/webhook/1", loaded.Webhooks[0]);
        Assert.Equal("https://legacy.example/webhook/2", loaded.Webhooks[1]);
        Assert.True(ServerWebhook.IsReady);
    }

    private sealed class ServerWebhookFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;
        private readonly FieldInfo _serverField;
        private readonly object? _originalServer;
        private readonly bool _originalIsReady;

        public ServerWebhookFileScope()
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, "webhookSet.txt");
            _settingsBackup = SettingsPath + ".codex.bak";

            _serverField = typeof(ServerWebhook).GetField("_server", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Field _server not found.");
            _originalServer = _serverField.GetValue(null);
            _originalIsReady = ServerWebhook.IsReady;

            _serverField.SetValue(null, null);
            ServerWebhook.IsReady = false;

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
            ServerWebhook.IsReady = false;
        }

        public void Dispose()
        {
            _serverField.SetValue(null, _originalServer);
            ServerWebhook.IsReady = _originalIsReady;

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
