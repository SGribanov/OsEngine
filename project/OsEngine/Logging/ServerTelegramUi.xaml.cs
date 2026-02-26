#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Globalization;
using System.Windows;
using OsEngine.Language;

namespace OsEngine.Logging
{
    /// <summary>
    /// settings window of telegram server
    /// Окно настроек сервера телеграм
    /// </summary>
    public partial class ServerTelegramDeliveryUi
    {
        public ServerTelegramDeliveryUi() // constructor / конструктор
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);
            
            ServerTelegram serverTelegram = ServerTelegram.GetServer();

            TextBoxMyBotToken.Text = serverTelegram.BotToken;
            TextBoxChatId.Text = serverTelegram.ChatId.ToString();
            CheckBoxTelegramProcessingCommand.IsChecked = serverTelegram.ProcessingCommand;

            Title = OsLocalization.Logging.Label22;
            ButtonAccept.Content = OsLocalization.Logging.Button1;
            Label23.Content = OsLocalization.Logging.Label23;
            Label24.Content = OsLocalization.Logging.Label24;
            Label25.Content = OsLocalization.Logging.Label25;
            Label25.Visibility = Visibility.Collapsed;
            this.Activate();
            this.Focus();
        }

        private void buttonAccept_Click(object sender, RoutedEventArgs e) // accept / принять
        {
            ServerTelegram serverTelegram = ServerTelegram.GetServer();
            serverTelegram.BotToken = TextBoxMyBotToken.Text;

            if (long.TryParse(TextBoxChatId.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out serverTelegram.ChatId))
            {
                if(CheckBoxTelegramProcessingCommand.IsChecked == null || !CheckBoxTelegramProcessingCommand.IsChecked.Value)
                {
                    CheckBoxTelegramProcessingCommand.IsChecked = false;
                }
                else
                {
                    serverTelegram.ProcessingCommand = true;
                }
                
                serverTelegram.Save();
                Close();
            }
            else
            {
                Label25.Visibility = Visibility.Visible;
            }
        }
    }
}

