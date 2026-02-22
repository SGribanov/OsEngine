#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;
using System.Windows;
using OsEngine.Entity;
using OsEngine.Language;

namespace OsEngine.OsTrader.RiskManager
{
    /// <summary>
    /// Risk Manager window
    /// Окно Риск Менеджера
    /// </summary>
    public partial class RiskManagerUi
    {
        /// <summary>
        /// risk manager
        /// риск менеджер
        /// </summary>
        private RiskManager _riskManager;
        public RiskManagerUi(RiskManager riskManager)
        {
            try
            {
                _riskManager = riskManager;
                InitializeComponent();
                OsEngine.Layout.StickyBorders.Listen(this);
                OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);
                LoadDateOnForm();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }

            Title = OsLocalization.Trader.Label12;
            LabelMaxRisk.Content = OsLocalization.Trader.Label14;
            LabelMaxLossReactioin.Content = OsLocalization.Trader.Label15;
            CheckBoxIsOn.Content = OsLocalization.Trader.Label16;
            ButtonAccept.Content = OsLocalization.Trader.Label17;

            this.Activate();
            this.Focus();
        }

        /// <summary>
        /// upload data to the form
        /// загрузить данные на форму
        /// </summary>
        private void LoadDateOnForm()
        {
            CheckBoxIsOn.IsChecked = _riskManager.IsActiv;
            TextBoxOpenMaxDd.Text = _riskManager.MaxDrowDownToDayPersent.ToString(new CultureInfo("ru-RU"));

            ComboBoxReaction.Items.Add(RiskManagerReactionType.CloseAndOff);
            ComboBoxReaction.Items.Add(RiskManagerReactionType.ShowDialog);
            ComboBoxReaction.Items.Add(RiskManagerReactionType.None);

            ComboBoxReaction.Text = _riskManager.ReactionType.ToString();
            
        }

        /// <summary>
        /// clicked accept
        /// нажали кнопку принять
        /// </summary>
        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBoxOpenMaxDd.Text.ToDecimal();
            }
            catch (Exception)
            {
                MessageBox.Show(OsLocalization.Trader.Label13);
                return;
            }


           _riskManager.IsActiv =  CheckBoxIsOn.IsChecked.Value;
           _riskManager.MaxDrowDownToDayPersent = TextBoxOpenMaxDd.Text.ToDecimal();

           Enum.TryParse(ComboBoxReaction.Text,false,out _riskManager.ReactionType);
           _riskManager.Save();
            Close();
        }
    }
}

