#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
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

    [Fact]
    public void RequestBuilders_ShouldProduceEscapedCultureSafeTelegramUris()
    {
        using ServerTelegramFileScope scope = new ServerTelegramFileScope();

        ServerTelegram server = scope.CreateServerWithoutConstructor();
        server.BotToken = "  token-value  ";
        server.ChatId = 1234567890123456789L;
        server.ProcessingCommand = true;
        scope.SetIsReady(true);

        CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

        try
        {
            MethodInfo buildSendMessage = typeof(ServerTelegram).GetMethod("BuildSendMessageRequestUri", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method BuildSendMessageRequestUri not found.");
            MethodInfo buildGetUpdates = typeof(ServerTelegram).GetMethod("BuildGetUpdatesRequestUri", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("Method BuildGetUpdatesRequestUri not found.");

            Uri sendUri = (Uri)buildSendMessage.Invoke(server, new object?[] { "Risk _value_ [check]!" })!;
            Uri updatesUri = (Uri)buildGetUpdates.Invoke(server, new object?[] { 1234567890123456790L })!;

            Assert.Equal(
                "https://api.telegram.org/bottoken-value/sendMessage?chat_id=1234567890123456789&text=Risk%20%5C_value%5C_%20%5C%5Bcheck%5C%5D%5C%21&parse_mode=MarkdownV2&reply_markup=%7B%22keyboard%22%3A%5B%5B%7B%22text%22%3A%22StopAllBots%22%7D%2C%7B%22text%22%3A%22StartAllBots%22%7D%5D%2C%5B%7B%22text%22%3A%22CancelAllActiveOrders%22%7D%2C%7B%22text%22%3A%22GetStatus%22%7D%5D%5D%2C%22resize_keyboard%22%3Atrue%7D",
                sendUri.AbsoluteUri);
            Assert.Equal(
                "https://api.telegram.org/bottoken-value/getUpdates?offset=1234567890123456790&timeout=2&allowed_updates=%5B%22message%22%5D",
                updatesUri.AbsoluteUri);
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
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

        public void SetIsReady(bool value)
        {
            _isReadyField.SetValue(null, value);
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
