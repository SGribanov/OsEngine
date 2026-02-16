using System;
using System.IO;
using System.Linq;
using OsEngine.Entity;
using OsEngine.Logging;
using Xunit;

namespace OsEngine.Tests;

public class MessageSenderPersistenceTests
{
    [Fact]
    public void Save_ShouldPersistJson_AndLoadRoundTrip()
    {
        const string senderName = "CodexMessageSenderJson";
        using MessageSenderFileScope scope = new MessageSenderFileScope(senderName);

        MessageSender source = new MessageSender(senderName, StartProgram.IsOsTrader)
        {
            MailSendOn = true,
            MailSystemSendOn = true,
            MailSignalSendOn = false,
            MailErrorSendOn = true,
            MailConnectSendOn = false,
            MailTradeSendOn = true,
            MailNoNameSendOn = false,
            SmsSendOn = true,
            SmsSystemSendOn = false,
            SmsSignalSendOn = true,
            SmsErrorSendOn = false,
            SmsConnectSendOn = true,
            SmsTradeSendOn = false,
            SmsNoNameSendOn = true,
            WebhookSendOn = true,
            WebhookSystemSendOn = false,
            WebhookSignalSendOn = true,
            WebhookErrorSendOn = false,
            WebhookConnectSendOn = true,
            WebhookTradeSendOn = false,
            WebhookNoNameSendOn = true,
            TelegramSendOn = true,
            TelegramSystemSendOn = false,
            TelegramSignalSendOn = true,
            TelegramErrorSendOn = false,
            TelegramConnectSendOn = true,
            TelegramTradeSendOn = false,
            TelegramNoNameSendOn = true,
            TelegramUserSendOn = true
        };
        source.Save();

        string content = File.ReadAllText(scope.SettingsPath);
        Assert.StartsWith("{", content.TrimStart());

        MessageSender loaded = new MessageSender(senderName, StartProgram.IsOsTrader);
        Assert.True(loaded.MailSendOn);
        Assert.True(loaded.MailSystemSendOn);
        Assert.False(loaded.MailSignalSendOn);
        Assert.True(loaded.MailErrorSendOn);
        Assert.True(loaded.SmsSendOn);
        Assert.True(loaded.SmsSignalSendOn);
        Assert.True(loaded.WebhookSendOn);
        Assert.True(loaded.WebhookSignalSendOn);
        Assert.True(loaded.TelegramSendOn);
        Assert.True(loaded.TelegramSignalSendOn);
        Assert.True(loaded.TelegramUserSendOn);
    }

    [Fact]
    public void Load_ShouldSupportLegacyLineBasedFormat()
    {
        const string senderName = "CodexMessageSenderLegacy";
        using MessageSenderFileScope scope = new MessageSenderFileScope(senderName);

        File.WriteAllLines(scope.SettingsPath, new[]
        {
            "true",  // MailSendOn
            "true",  // MailSystemSendOn
            "false", // MailSignalSendOn
            "true",  // MailErrorSendOn
            "false", // MailConnectSendOn
            "true",  // MailTradeSendOn
            "false", // MailNoNameSendOn
            "true",  // SmsSendOn
            "false", // SmsSystemSendOn
            "true",  // SmsSignalSendOn
            "false", // SmsErrorSendOn
            "true",  // SmsConnectSendOn
            "false", // SmsTradeSendOn
            "true",  // SmsNoNameSendOn
            "true",  // WebhookSendOn
            "false", // WebhookSystemSendOn
            "true",  // WebhookSignalSendOn
            "false", // WebhookErrorSendOn
            "true",  // WebhookConnectSendOn
            "false", // WebhookTradeSendOn
            "true",  // WebhookNoNameSendOn
            "true",  // TelegramSendOn
            "false", // TelegramSystemSendOn
            "true",  // TelegramSignalSendOn
            "false", // TelegramErrorSendOn
            "true",  // TelegramConnectSendOn
            "false", // TelegramTradeSendOn
            "true"   // TelegramNoNameSendOn
            // TelegramUserSendOn intentionally missing (legacy compatibility)
        });

        MessageSender loaded = new MessageSender(senderName, StartProgram.IsOsTrader);
        Assert.True(loaded.MailSendOn);
        Assert.True(loaded.MailSystemSendOn);
        Assert.False(loaded.MailSignalSendOn);
        Assert.True(loaded.SmsSendOn);
        Assert.True(loaded.SmsSignalSendOn);
        Assert.True(loaded.WebhookSendOn);
        Assert.True(loaded.WebhookSignalSendOn);
        Assert.True(loaded.TelegramSendOn);
        Assert.True(loaded.TelegramSignalSendOn);
        Assert.True(loaded.TelegramNoNameSendOn);
        Assert.False(loaded.TelegramUserSendOn);
    }

    private sealed class MessageSenderFileScope : IDisposable
    {
        private readonly string _engineDirPath;
        private readonly bool _engineDirExisted;
        private readonly bool _settingsFileExisted;
        private readonly string _settingsBackup;

        public MessageSenderFileScope(string senderName)
        {
            _engineDirPath = Path.GetFullPath("Engine");
            SettingsPath = Path.Combine(_engineDirPath, senderName + "MessageSender.txt");
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
