#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8620

using OsEngine.Language;
using System.Windows;


namespace OsEngine.Market.Servers.TelegramNews.TGAuthEntity
{
    /// <summary>
    /// Логика взаимодействия для AuthTGPasswordDialogUi.xaml
    /// </summary>
    public partial class AuthTGPasswordDialogUi : Window
    {
        public string Password { get; private set; }

        public AuthTGPasswordDialogUi()
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);

            Title = OsLocalization.Market.TelegramAuthTitle;
            LabelPassword.Content = OsLocalization.Market.ServerParamPassword + ":";
            ButtonSend.Content = OsLocalization.Market.SendButton;
            ButtonCancel.Content = OsLocalization.Entity.ButtonCancel1;

            this.Activate();
            this.Focus();
        }

        private void ButtonSendPass_Click(object sender, RoutedEventArgs e)
        {
            Password = TextBoxCode.Text;
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}


