#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

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
    public void Save_ShouldPersistToml_AndLoadRoundTrip()
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

        string content = File.ReadAllText(scope.CanonicalPath);
        Assert.Contains("MailSendOn = true", content);

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
    public void Load_ShouldSupportLegacyLineBasedFormat_AndSaveToml()
    {
        const string senderName = "CodexMessageSenderLegacy";
        using MessageSenderFileScope scope = new MessageSenderFileScope(senderName);

        File.WriteAllLines(scope.LegacyTxtPath, new[]
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

        loaded.Save();
        Assert.True(File.Exists(scope.CanonicalPath));
        Assert.Contains("TelegramUserSendOn = false", File.ReadAllText(scope.CanonicalPath));
    }

    private sealed class MessageSenderFileScope : IDisposable
    {
        private readonly StructuredSettingsFileScope _settingsScope;

        public MessageSenderFileScope(string senderName)
        {
            _settingsScope = new StructuredSettingsFileScope(Path.Combine("Engine", senderName + "MessageSender.toml"));
        }

        public string CanonicalPath => _settingsScope.CanonicalPath;

        public string LegacyTxtPath => _settingsScope.LegacyTxtPath;

        public void Dispose()
        {
            _settingsScope.Dispose();
        }
    }
}
