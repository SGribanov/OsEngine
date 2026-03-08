#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

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
         public ServerMailDeliveryUi() // constructor / конструктор
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);

            ServerMail serverMail = ServerMail.GetServer();

            TextBoxMyAdress.Text = serverMail.MyAdress;
            TextBoxPassword.Text = serverMail.MyPassword;
            TextBoxAdress.Text = BuildAddressText(serverMail.Adress);

            ComboBoxMyMaster.Items.Add("Yandex");
            ComboBoxMyMaster.Items.Add("Google");

            if (serverMail.Smtp == "smtp.yandex.ru")
            {
                ComboBoxMyMaster.SelectedItem = "Yandex";
            }
            else
            {
                ComboBoxMyMaster.SelectedItem = "Google";
            }

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

        private static string BuildAddressText(string[] addresses)
        {
            if (addresses == null || addresses.Length == 0)
            {
                return string.Empty;
            }

            return string.Join("\r\n", addresses) + "\r\n";
        }

        private static string[] ParseAddresses(string addressText)
        {
            string[] lines = addressText.Replace("\r", string.Empty).Split('\n');
            List<string> addresses = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] != string.Empty)
                {
                    addresses.Add(lines[i]);
                }
            }

            return addresses.Count == 0 ? null : addresses.ToArray();
        }

        private static string ResolveSmtpHost(object selectedProvider)
        {
            return selectedProvider as string == "Yandex"
                ? "smtp.yandex.ru"
                : "smtp.gmail.com";
        }
    }
}

