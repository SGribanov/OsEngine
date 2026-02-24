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
using OsEngine.OsTrader.Panels;

namespace OsEngine.Robots.CounterTrend
{
    public partial class RsiContrtrendUi
    {
        private RsiContrtrend _strategy;
        public RsiContrtrendUi(RsiContrtrend strategy)
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);
            _strategy = strategy;

            ComboBoxRegime.Items.Add(BotTradeRegime.Off);
            ComboBoxRegime.Items.Add(BotTradeRegime.On);
            ComboBoxRegime.Items.Add(BotTradeRegime.OnlyClosePosition);
            ComboBoxRegime.Items.Add(BotTradeRegime.OnlyLong);
            ComboBoxRegime.Items.Add(BotTradeRegime.OnlyShort);
            ComboBoxRegime.SelectedItem = _strategy._regime;

            ComboBoxVolumeType.Items.Add("Deposit percent");
            ComboBoxVolumeType.Items.Add("Contracts");
            ComboBoxVolumeType.Items.Add("Contract currency");
            ComboBoxVolumeType.SelectedItem = _strategy._volumeType;

            TextBoxAssetInPortfolio.Text = "Prime";
            TextBoxSlippage.Text = _strategy._slippage.ToString(new CultureInfo("ru-RU"));

            TextBoxVolumeOne.Text = _strategy._volume.ToString();
            RsiUp.Text = _strategy._upline.Value.ToString(new CultureInfo("ru-RU"));
            RsiDown.Text = _strategy._downline.Value.ToString(new CultureInfo("ru-RU"));

            LabelRegime.Content = OsLocalization.Trader.Label115;
            LabelVolume.Content = OsLocalization.Trader.Label30;
            LabelSlippage.Content = OsLocalization.Trader.Label92;
            ButtonAccept.Content = OsLocalization.Trader.Label17;
            LabelRsiOverbought.Content = OsLocalization.Trader.Label141;
            LabelRsiOversold.Content = OsLocalization.Trader.Label142;
            LabelVolumeType.Content = OsLocalization.Trader.Label554;
            LabelAssetInPortfolio.Content = OsLocalization.Trader.Label555;

            this.Activate();
            this.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TextBoxVolumeOne.Text.ToDecimal() <= 0 || 
                    Convert.ToInt32(RsiUp.Text, CultureInfo.InvariantCulture) <= 0 || 
                    Convert.ToInt32(RsiDown.Text, CultureInfo.InvariantCulture) <= 0 ||
                    TextBoxSlippage.Text.ToDecimal() < 0 ||
                    Convert.ToInt32(TextBoxVolumeOne.Text, CultureInfo.InvariantCulture) <= 0)
                {
                    throw new Exception("");
                }
            }
            catch (Exception)
            {
                MessageBox.Show(OsLocalization.Trader.Label13);
                return;
            }

            Enum.TryParse(ComboBoxRegime.Text, true, out _strategy._regime);
            _strategy._volumeType =Convert.ToString(ComboBoxVolumeType.Text);
            _strategy._tradeAssetInPortfolio = Convert.ToString(TextBoxAssetInPortfolio.Text);
            _strategy._slippage = TextBoxSlippage.Text.ToDecimal();
            _strategy._volume = TextBoxVolumeOne.Text.ToDecimal();
            _strategy._upline.Value = RsiUp.Text.ToDecimal();
            _strategy._downline.Value = RsiDown.Text.ToDecimal();

            _strategy._upline.Refresh();
            _strategy._downline.Refresh();

            _strategy.Save();
            Close();
        }
    }
}

