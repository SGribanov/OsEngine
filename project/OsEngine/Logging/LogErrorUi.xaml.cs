#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Windows.Forms;
using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Layout;

namespace OsEngine.Logging
{
    /// <summary>
    /// Interaction logic for LogErrorUi.xaml
    /// Логика взаимодействия для LogErrorUi.xaml
    /// </summary>
    public partial class LogErrorUi
    {
        public LogErrorUi(DataGridView gridErrorLog)
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            GlobalGUILayout.Listen(this, "LogErrorUi");
            HostLog.Child = gridErrorLog;
            Title = OsLocalization.Logging.TitleExtraLog;
            Title = Title + " " + OsEngine.PrimeSettings.PrimeSettingsMaster.LabelInHeaderBotStation;
            
            this.Activate();
            this.Focus();

            ButtonClear.Content = OsLocalization.Logging.ButtonClearExtraLog;
        }

        private void ButtonClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AcceptDialogUi ui = new AcceptDialogUi(OsLocalization.Logging.Label30);
            ui.ShowDialog();

            if (ui.UserAcceptAction == false)
            {
                return;
            }

            Log.ClearErrorLog();
        }
    }
}
