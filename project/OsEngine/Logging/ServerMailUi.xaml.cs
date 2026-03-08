#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Windows;
using OsEngine.Language;

namespace OsEngine.Logging
{
    /// <summary>
    /// window of mailing server settings
    /// Окно настроек сервера почтовой рассылки
    /// </summary>
    public partial class ServerMailDeliveryUi
    {
        private const string YandexProviderName = "Yandex";
        private const string GoogleProviderName = "Google";
        private const string YandexSmtpHost = "smtp.yandex.ru";
        private const string GoogleSmtpHost = "smtp.gmail.com";

        public ServerMailDeliveryUi() // constructor / конструктор
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);

            ServerMail serverMail = ServerMail.GetServer();

            TextBoxMyAdress.Text = serverMail.MyAdress;
            TextBoxPassword.Text = serverMail.MyPassword;
            TextBoxAdress.Text = BuildAddressText(serverMail.Adress);

            ComboBoxMyMaster.Items.Add(YandexProviderName);
            ComboBoxMyMaster.Items.Add(GoogleProviderName);
            ComboBoxMyMaster.SelectedItem = GetProviderNameForSmtp(serverMail.Smtp);

            Title = OsLocalization.Logging.TitleEmailServer;
            ButtonAccept.Content = OsLocalization.Logging.Button1;
            Label11.Content = OsLocalization.Logging.Label11;
            Label12.Content = OsLocalization.Logging.Label12;
            Label13.Content = OsLocalization.Logging.Label13;
            Label14.Content = OsLocalization.Logging.Label14;

            this.Activate();
            this.Focus();
        }

        private void buttonAccept_Click(object sender, RoutedEventArgs e) // accept / принять
        {
            ServerMail serverMail = ServerMail.GetServer();
            serverMail.MyAdress = TextBoxMyAdress.Text;
            serverMail.MyPassword = TextBoxPassword.Text;
            serverMail.Adress = ParseAddresses(TextBoxAdress.Text);
            serverMail.Smtp = ResolveSmtpHost(ComboBoxMyMaster.SelectedItem);
            serverMail.Save();
            Close();
        }

        private static string BuildAddressText(string[]? addresses)
        {
            if (addresses == null || addresses.Length == 0)
            {
                return string.Empty;
            }

            return string.Join(Environment.NewLine, addresses) + Environment.NewLine;
        }

        private static string[]? ParseAddresses(string? addressText)
        {
            if (string.IsNullOrWhiteSpace(addressText))
            {
                return null;
            }

            string[] lines = addressText.Replace("\r", string.Empty).Split('\n');
            List<string> addresses = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]) == false)
                {
                    addresses.Add(lines[i].Trim());
                }
            }

            return addresses.Count == 0 ? null : addresses.ToArray();
        }

        private static string ResolveSmtpHost(object? selectedProvider)
        {
            return string.Equals(selectedProvider?.ToString(), YandexProviderName, StringComparison.Ordinal)
                ? YandexSmtpHost
                : GoogleSmtpHost;
        }

        private static string GetProviderNameForSmtp(string? smtpHost)
        {
            return string.Equals(smtpHost, YandexSmtpHost, StringComparison.OrdinalIgnoreCase)
                ? YandexProviderName
                : GoogleProviderName;
        }
    }
}

