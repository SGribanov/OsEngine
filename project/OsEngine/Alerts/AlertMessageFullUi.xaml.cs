#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использования кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Windows;
using System.Windows.Forms;
using OsEngine.Language;

namespace OsEngine.Alerts
{
    /// <summary>
    /// Interaction logic for AlertMessageUi.xaml
    /// Логика взаимодействия для AlertMessageUi.xaml
    /// </summary>
    public partial class AlertMessageFullUi
    {
        public AlertMessageFullUi(DataGridView grid)
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            HostAlerts.Child = grid;
            HostAlerts.Child.Show();
            Title = OsLocalization.Alerts.TitleAlertMessageFullUi;

            this.Activate();
            this.Focus();

            this.Closed += AlertMessageFullUi_Closed;
        }

        private void AlertMessageFullUi_Closed(object sender, System.EventArgs e)
        {
            HostAlerts.Child = null;
            HostAlerts = null;



        } 
    }
}

