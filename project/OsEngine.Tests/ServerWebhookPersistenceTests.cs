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
    public void Save_ShouldPersistToml_AndLoadRoundTrip()
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

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("SlackBotToken = \"slack-token-json\"", content);

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
    public void Load_ShouldSupportLegacyLineBasedFormat_AndSaveToml()
    {
        using ServerWebhookFileScope scope = new ServerWebhookFileScope();

        File.WriteAllLines(scope.LegacyTxtPath, new[]
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

        loaded.Save();
        Assert.True(File.Exists(scope.CanonicalPath));
        Assert.Contains("Webhooks = [", File.ReadAllText(scope.CanonicalPath));
    }

    private sealed class ServerWebhookFileScope : IDisposable
    {
        private readonly StructuredSettingsFileScope _settingsScope;
        private readonly FieldInfo _serverField;
        private readonly object? _originalServer;
        private readonly bool _originalIsReady;

        public ServerWebhookFileScope()
        {
            _serverField = typeof(ServerWebhook).GetField("_server", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("Field _server not found.");
            _originalServer = _serverField.GetValue(null);
            _originalIsReady = ServerWebhook.IsReady;

            _serverField.SetValue(null, null);
            ServerWebhook.IsReady = false;

            _settingsScope = new StructuredSettingsFileScope(Path.Combine("Engine", "webhookSet.toml"));
        }

        public string CanonicalPath => _settingsScope.CanonicalPath;

        public string LegacyTxtPath => _settingsScope.LegacyTxtPath;

        public void ResetSingleton()
        {
            _serverField.SetValue(null, null);
            ServerWebhook.IsReady = false;
        }

        public void Dispose()
        {
            _serverField.SetValue(null, _originalServer);
            ServerWebhook.IsReady = _originalIsReady;

            _settingsScope.Dispose();
        }
    }
}
