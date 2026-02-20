#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using OsEngine.Language;
using OsEngine.Market;
using System.Windows;


namespace OsEngine.OsTrader.Gui.BlockInterface
{
    /// <summary>
    /// Interaction logic for RobotsUiLightUnblock.xaml
    /// </summary>
    public partial class RobotsUiLightUnblock : Window
    {
        public RobotsUiLightUnblock()
        {
            InitializeComponent();

            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);

            LabelPassword.Content = OsLocalization.Trader.Label423;
            ButtonAccept.Content = OsLocalization.Trader.Label429;
            Title = OsLocalization.Trader.Label430;
        }

        public bool IsUnBlocked;

        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            string password = TextBoxPassword.Text;

            string passwordInReal = BlockMaster.Password;

            if(passwordInReal == password)
            {
                IsUnBlocked = true;
                BlockMaster.IsBlocked = false;
                Close();
            }
            else
            {
                ServerMaster.SendNewLogMessage("Error password. ",Logging.LogMessageType.Error);
                Close();
            }
        }
    }
}
