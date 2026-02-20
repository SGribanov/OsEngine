#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Collections.Generic;
using System.ComponentModel;
using OsEngine.Language;
using OsEngine.Market.Servers;
using OsEngine.Market.SupportTable;

namespace OsEngine.Market
{
    /// <summary>
    /// interaction logic for ServerPrimeUi.xaml
    /// Логика взаимодействия для ServerPrimeUi.xaml
    /// </summary>
    public partial class ServerMasterUi
    {
        /// <summary>
        /// constructor
        /// конструктор
        /// </summary>
        /// <param name="isTester">shows whether the method is called from the tester / вызывается ли метод из тестера </param>
        public ServerMasterUi(bool isTester)
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);

            List<IServer> servers = ServerMaster.GetServers();

            if (isTester)
            {
                servers = ServerMaster.GetServers();

                if (servers == null ||
                    servers.Find(s => s.ServerType == ServerType.Tester) == null)
                {
                    ServerMaster.CreateServer(ServerType.Tester, false);
                }

                Close();

                servers = ServerMaster.GetServers();
                servers.Find(s => s.ServerType == ServerType.Tester).ShowDialog();
                return;
            }

            Title = OsLocalization.Market.TitleServerMasterUi;
            TabItem1.Header = OsLocalization.Market.TabItem1;
            TabItem2.Header = OsLocalization.Market.TabItem2;
            CheckBoxServerAutoOpen.Content = OsLocalization.Market.Label20;
            ButtonSupportTable.Content = OsLocalization.Market.Label81;

            ServerMasterSourcesPainter painter = new ServerMasterSourcesPainter(
                HostSource,
                HostLog,
                CheckBoxServerAutoOpen,
                TextBoxSearchSource,
                ButtonRightInSearchResults,
                ButtonLeftInSearchResults,
                LabelCurrentResultShow,
                LabelCommasResultShow,
                LabelCountResultsShow);

            Closing += delegate (object sender, CancelEventArgs args)
            {
                painter.Dispose();
                painter = null;
            };

            this.Activate();
            this.Focus();
        }

        private void ButtonSupportTable_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SupportTableUi supportTableUi = new SupportTableUi();
            supportTableUi.ShowDialog();
        }
    }
}

