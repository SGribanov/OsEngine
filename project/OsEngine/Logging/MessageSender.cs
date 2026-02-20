#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Diagnostics;
using System.IO;
using OsEngine.Entity;

namespace OsEngine.Logging
{
    /// <summary>
    /// distribution manager
    /// менеджер рассылки
    /// </summary>
    public class MessageSender
    {
 // distribution settings
 // настройки рассылки

        public bool WebhookSendOn;
        
        public bool WebhookSystemSendOn;
        public bool WebhookSignalSendOn;
        public bool WebhookErrorSendOn;
        public bool WebhookConnectSendOn;
        public bool WebhookTradeSendOn;
        public bool WebhookNoNameSendOn;

        public bool TelegramSendOn;

        public bool TelegramSystemSendOn;
        public bool TelegramSignalSendOn;
        public bool TelegramErrorSendOn;
        public bool TelegramConnectSendOn;
        public bool TelegramTradeSendOn;
        public bool TelegramNoNameSendOn;
        public bool TelegramUserSendOn;

        public bool MailSendOn;

        public bool MailSystemSendOn;
        public bool MailSignalSendOn;
        public bool MailErrorSendOn;
        public bool MailConnectSendOn;
        public bool MailTradeSendOn;
        public bool MailNoNameSendOn;

        public bool SmsSendOn;

        public bool SmsSystemSendOn;
        public bool SmsSignalSendOn;
        public bool SmsErrorSendOn;
        public bool SmsConnectSendOn;
        public bool SmsTradeSendOn;
        public bool SmsNoNameSendOn;

        private string _name; // name / имя

        /// <summary>
        /// program that created the object
        /// программа создавшая объект
        /// </summary>
        private StartProgram _startProgram;

        public MessageSender(string name, StartProgram startProgram)
        {
            _startProgram = startProgram;
            _name = name;
            Load();
        }

        /// <summary>
        /// show settings window
        /// показать окно настроек
        /// </summary>
        public void ShowDialog()
        {
            MessageSenderUi ui = new MessageSenderUi(this);
            ui.ShowDialog();
        }

        /// <summary>
        /// download
        /// загрузить
        /// </summary>
        private void Load() 
        {
            MessageSenderSettingsDto settings = SettingsManager.Load(
                GetSettingsPath(),
                defaultValue: null,
                legacyLoader: ParseLegacySettings);

            if (settings == null)
            {
                return;
            }

            ApplySettings(settings);
        }

        private void ApplySettings(MessageSenderSettingsDto settings)
        {
            try
            {
                MailSendOn = settings.MailSendOn;

                MailSystemSendOn = settings.MailSystemSendOn;
                MailSignalSendOn = settings.MailSignalSendOn;
                MailErrorSendOn = settings.MailErrorSendOn;
                MailConnectSendOn = settings.MailConnectSendOn;
                MailTradeSendOn = settings.MailTradeSendOn;
                MailNoNameSendOn = settings.MailNoNameSendOn;

                SmsSendOn = settings.SmsSendOn;

                SmsSystemSendOn = settings.SmsSystemSendOn;
                SmsSignalSendOn = settings.SmsSignalSendOn;
                SmsErrorSendOn = settings.SmsErrorSendOn;
                SmsConnectSendOn = settings.SmsConnectSendOn;
                SmsTradeSendOn = settings.SmsTradeSendOn;
                SmsNoNameSendOn = settings.SmsNoNameSendOn;

                WebhookSendOn = settings.WebhookSendOn;

                WebhookSystemSendOn = settings.WebhookSystemSendOn;
                WebhookSignalSendOn = settings.WebhookSignalSendOn;
                WebhookErrorSendOn = settings.WebhookErrorSendOn;
                WebhookConnectSendOn = settings.WebhookConnectSendOn;
                WebhookTradeSendOn = settings.WebhookTradeSendOn;
                WebhookNoNameSendOn = settings.WebhookNoNameSendOn;

                TelegramSendOn = settings.TelegramSendOn;

                TelegramSystemSendOn = settings.TelegramSystemSendOn;
                TelegramSignalSendOn = settings.TelegramSignalSendOn;
                TelegramErrorSendOn = settings.TelegramErrorSendOn;
                TelegramConnectSendOn = settings.TelegramConnectSendOn;
                TelegramTradeSendOn = settings.TelegramTradeSendOn;
                TelegramNoNameSendOn = settings.TelegramNoNameSendOn;
                TelegramUserSendOn = settings.TelegramUserSendOn;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }

        }

        /// <summary>
        /// save
        /// сохранить
        /// </summary>
        public void Save() 
        {
            try
            {
                SettingsManager.Save(GetSettingsPath(), BuildSettings());
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }
        }

        private MessageSenderSettingsDto BuildSettings()
        {
            return new MessageSenderSettingsDto
            {
                MailSendOn = MailSendOn,
                MailSystemSendOn = MailSystemSendOn,
                MailSignalSendOn = MailSignalSendOn,
                MailErrorSendOn = MailErrorSendOn,
                MailConnectSendOn = MailConnectSendOn,
                MailTradeSendOn = MailTradeSendOn,
                MailNoNameSendOn = MailNoNameSendOn,
                SmsSendOn = SmsSendOn,
                SmsSystemSendOn = SmsSystemSendOn,
                SmsSignalSendOn = SmsSignalSendOn,
                SmsErrorSendOn = SmsErrorSendOn,
                SmsConnectSendOn = SmsConnectSendOn,
                SmsTradeSendOn = SmsTradeSendOn,
                SmsNoNameSendOn = SmsNoNameSendOn,
                WebhookSendOn = WebhookSendOn,
                WebhookSystemSendOn = WebhookSystemSendOn,
                WebhookSignalSendOn = WebhookSignalSendOn,
                WebhookErrorSendOn = WebhookErrorSendOn,
                WebhookConnectSendOn = WebhookConnectSendOn,
                WebhookTradeSendOn = WebhookTradeSendOn,
                WebhookNoNameSendOn = WebhookNoNameSendOn,
                TelegramSendOn = TelegramSendOn,
                TelegramSystemSendOn = TelegramSystemSendOn,
                TelegramSignalSendOn = TelegramSignalSendOn,
                TelegramErrorSendOn = TelegramErrorSendOn,
                TelegramConnectSendOn = TelegramConnectSendOn,
                TelegramTradeSendOn = TelegramTradeSendOn,
                TelegramNoNameSendOn = TelegramNoNameSendOn,
                TelegramUserSendOn = TelegramUserSendOn
            };
        }

        private string GetSettingsPath()
        {
            return @"Engine\" + _name + @"MessageSender.txt";
        }

        private static MessageSenderSettingsDto ParseLegacySettings(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string normalized = content.Replace("\r", string.Empty);
            string[] lines = normalized.Split('\n');

            if (lines.Length > 0 && lines[lines.Length - 1] == string.Empty)
            {
                Array.Resize(ref lines, lines.Length - 1);
            }

            int index = 0;
            return new MessageSenderSettingsDto
            {
                MailSendOn = TryReadBool(lines, ref index),
                MailSystemSendOn = TryReadBool(lines, ref index),
                MailSignalSendOn = TryReadBool(lines, ref index),
                MailErrorSendOn = TryReadBool(lines, ref index),
                MailConnectSendOn = TryReadBool(lines, ref index),
                MailTradeSendOn = TryReadBool(lines, ref index),
                MailNoNameSendOn = TryReadBool(lines, ref index),
                SmsSendOn = TryReadBool(lines, ref index),
                SmsSystemSendOn = TryReadBool(lines, ref index),
                SmsSignalSendOn = TryReadBool(lines, ref index),
                SmsErrorSendOn = TryReadBool(lines, ref index),
                SmsConnectSendOn = TryReadBool(lines, ref index),
                SmsTradeSendOn = TryReadBool(lines, ref index),
                SmsNoNameSendOn = TryReadBool(lines, ref index),
                WebhookSendOn = TryReadBool(lines, ref index),
                WebhookSystemSendOn = TryReadBool(lines, ref index),
                WebhookSignalSendOn = TryReadBool(lines, ref index),
                WebhookErrorSendOn = TryReadBool(lines, ref index),
                WebhookConnectSendOn = TryReadBool(lines, ref index),
                WebhookTradeSendOn = TryReadBool(lines, ref index),
                WebhookNoNameSendOn = TryReadBool(lines, ref index),
                TelegramSendOn = TryReadBool(lines, ref index),
                TelegramSystemSendOn = TryReadBool(lines, ref index),
                TelegramSignalSendOn = TryReadBool(lines, ref index),
                TelegramErrorSendOn = TryReadBool(lines, ref index),
                TelegramConnectSendOn = TryReadBool(lines, ref index),
                TelegramTradeSendOn = TryReadBool(lines, ref index),
                TelegramNoNameSendOn = TryReadBool(lines, ref index),
                TelegramUserSendOn = TryReadBool(lines, ref index)
            };
        }

        private static bool TryReadBool(string[] lines, ref int index)
        {
            if (lines == null || index >= lines.Length)
            {
                return false;
            }

            string value = lines[index];
            index++;
            return value != null && value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private sealed class MessageSenderSettingsDto
        {
            public bool WebhookSendOn { get; set; }
            public bool WebhookSystemSendOn { get; set; }
            public bool WebhookSignalSendOn { get; set; }
            public bool WebhookErrorSendOn { get; set; }
            public bool WebhookConnectSendOn { get; set; }
            public bool WebhookTradeSendOn { get; set; }
            public bool WebhookNoNameSendOn { get; set; }
            public bool TelegramSendOn { get; set; }
            public bool TelegramSystemSendOn { get; set; }
            public bool TelegramSignalSendOn { get; set; }
            public bool TelegramErrorSendOn { get; set; }
            public bool TelegramConnectSendOn { get; set; }
            public bool TelegramTradeSendOn { get; set; }
            public bool TelegramNoNameSendOn { get; set; }
            public bool TelegramUserSendOn { get; set; }
            public bool MailSendOn { get; set; }
            public bool MailSystemSendOn { get; set; }
            public bool MailSignalSendOn { get; set; }
            public bool MailErrorSendOn { get; set; }
            public bool MailConnectSendOn { get; set; }
            public bool MailTradeSendOn { get; set; }
            public bool MailNoNameSendOn { get; set; }
            public bool SmsSendOn { get; set; }
            public bool SmsSystemSendOn { get; set; }
            public bool SmsSignalSendOn { get; set; }
            public bool SmsErrorSendOn { get; set; }
            public bool SmsConnectSendOn { get; set; }
            public bool SmsTradeSendOn { get; set; }
            public bool SmsNoNameSendOn { get; set; }
        }

        /// <summary>
        /// delete
        /// удалить
        /// </summary>
        public void Delete() 
        {
            if (File.Exists(GetSettingsPath()))
            {
                File.Delete(GetSettingsPath());
            }
        }

        /// <summary>
        /// Send message. If this message type is subscribed and distribution servers are configured, the message will be sent
        /// If test server is enabled, the message will not be sent
        /// Отправить сообщение. Если такой тип сообщений подписан на рассылку и сервера рассылки настроены, сообщение будет отправлено
        /// Если включен тестовый сервер - сообщение не будет отправленно
        /// </summary>
        public void AddNewMessage(LogMessage message)
        {
            if (_startProgram != StartProgram.IsOsTrader)
            {
                return;
            }

            if (TelegramSendOn)
            {
                if (message.Type == LogMessageType.Connect &&
                    TelegramConnectSendOn)
                {
                    ServerTelegram.GetServer().Send(message, _name);
                }
                if (message.Type == LogMessageType.Error &&
                    TelegramErrorSendOn)
                {
                    ServerTelegram.GetServer().Send(message, _name);
                }
                if (message.Type == LogMessageType.Signal &&
                    TelegramSignalSendOn)
                {
                    ServerTelegram.GetServer().Send(message, _name);
                }
                if (message.Type == LogMessageType.System &&
                    TelegramSystemSendOn)
                {
                    ServerTelegram.GetServer().Send(message, _name);
                }
                if (message.Type == LogMessageType.Trade &&
                    TelegramTradeSendOn)
                {
                    ServerTelegram.GetServer().Send(message, _name);
                }
                if (message.Type == LogMessageType.User &&
                    TelegramUserSendOn)
                {
                    ServerTelegram.GetServer().Send(message, _name);
                }
            }
            
            if (WebhookSendOn)
            {
                if (message.Type == LogMessageType.Connect &&
                    WebhookConnectSendOn)
                {
                    ServerWebhook.GetServer().Send(message, _name);
                }
                if (message.Type == LogMessageType.Error &&
                    WebhookErrorSendOn)
                {
                    ServerWebhook.GetServer().Send(message, _name);
                }
                if (message.Type == LogMessageType.Signal &&
                    WebhookSignalSendOn)
                {
                    ServerWebhook.GetServer().Send(message, _name);
                }
                if (message.Type == LogMessageType.System &&
                    WebhookSystemSendOn)
                {
                    ServerWebhook.GetServer().Send(message, _name);
                }
                if (message.Type == LogMessageType.Trade &&
                    WebhookTradeSendOn)
                {
                    ServerWebhook.GetServer().Send(message, _name);
                }
            }

            if (MailSendOn)
            {
                if (message.Type == LogMessageType.Connect &&
                    MailConnectSendOn)
                {
                    ServerMail.GetServer().Send(message, _name);
                }
                if (message.Type == LogMessageType.Error &&
                MailErrorSendOn)
                {
                    ServerMail.GetServer().Send(message, _name);
                }
                if (message.Type == LogMessageType.Signal &&
                    MailSignalSendOn)
                {
                    ServerMail.GetServer().Send(message, _name);
                }
                if (message.Type == LogMessageType.System &&
                    MailSystemSendOn)
                {
                    ServerMail.GetServer().Send(message, _name);
                }
                if (message.Type == LogMessageType.Trade &&
                    MailTradeSendOn)
                {
                    ServerMail.GetServer().Send(message, _name);
                }
            }
            if (SmsSendOn)
            {
                if (message.Type == LogMessageType.Connect &&
                    SmsConnectSendOn)
                {
                    ServerSms.GetSmsServer().Send(message.GetString());
                }
                if (message.Type == LogMessageType.Error &&
                SmsErrorSendOn)
                {
                    ServerSms.GetSmsServer().Send(message.GetString());
                }
                if (message.Type == LogMessageType.Signal &&
                    SmsSignalSendOn)
                {
                    ServerSms.GetSmsServer().Send(message.GetString());
                }
                if (message.Type == LogMessageType.System &&
                    SmsSystemSendOn)
                {
                    ServerSms.GetSmsServer().Send(message.GetString());
                }
                if (message.Type == LogMessageType.Trade &&
                    SmsTradeSendOn)
                {
                    ServerSms.GetSmsServer().Send(message.GetString());
                }
            }
        }
    }
}

