#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Language;
using OsEngine.Market;
using System;
using System.Windows;

namespace OsEngine.OsData
{
    public partial class LqdtDataUi : Window
    {
        private OsDataSet _set;

        private OsDataSetPainter _setPainter;

        public LqdtDataUi(OsDataSet set, OsDataSetPainter setPainter)
        {
            InitializeComponent();

            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);

            _set = set;
            _setPainter = setPainter;

            Title = OsLocalization.Data.TitleAddLqdt;
            ExchangeLabel.Content = OsLocalization.Data.Label60;
            CreateButton.Content = OsLocalization.Data.ButtonCreate;

            Activate();
            Focus();

            Closed += LqdtDataUi_Closed;
        }

        private void LqdtDataUi_Closed(object sender, EventArgs e)
        {
            try
            {
                _set = null;
                _setPainter = null;
            }
            catch (Exception ex)
            {
                ServerMaster.Log?.ProcessMessage(ex.ToString(), Logging.LogMessageType.Error);
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ComboBoxExchange.Text == "MOEX")
                {
                    _set.AddLqdtMoex();
                }
                else // NYSE
                {
                    _set.AddLqdtNyse();
                }

                _setPainter.RePaintInterface();

                Close();
            }
            catch (Exception ex)
            {
                ServerMaster.Log?.ProcessMessage(ex.ToString(), Logging.LogMessageType.Error);
            }

        }
    }
}


