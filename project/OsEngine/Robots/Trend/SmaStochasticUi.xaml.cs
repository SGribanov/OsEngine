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

namespace OsEngine.Robots.Trend
{
    public partial class SmaStochasticUi
    {
        private SmaStochastic _strategy;
        public SmaStochasticUi(SmaStochastic strategy)
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);
            _strategy = strategy;

            TextBoxVolumeOne.Text = _strategy.Volume.ToString();
            TextBoxAssetInPortfolio.Text = "Prime";

            TextBoxSlippage.Text = _strategy.Slippage.ToString(new CultureInfo("ru-RU"));
            ComboBoxRegime.Items.Add(BotTradeRegime.Off);
            ComboBoxRegime.Items.Add(BotTradeRegime.On);
            ComboBoxRegime.Items.Add(BotTradeRegime.OnlyClosePosition);
            ComboBoxRegime.Items.Add(BotTradeRegime.OnlyLong);
            ComboBoxRegime.Items.Add(BotTradeRegime.OnlyShort);
            ComboBoxRegime.SelectedItem = _strategy.Regime;

            ComboBoxVolumeType.Items.Add("Deposit percent");
            ComboBoxVolumeType.Items.Add("Contracts");
            ComboBoxVolumeType.Items.Add("Contract currency");
            ComboBoxVolumeType.SelectedItem = _strategy.VolumeType;

            StochUp.Text = _strategy.Upline.ToString(new CultureInfo("ru-RU"));
            StochDown.Text = _strategy.Downline.ToString(new CultureInfo("ru-RU"));
            Step.Text = _strategy.Step.ToString(new CultureInfo("ru-RU"));

            LabelRegime.Content = OsLocalization.Trader.Label115;
            LabelVolume.Content = OsLocalization.Trader.Label30;
            LabelSlippage.Content = OsLocalization.Trader.Label92;
            ButtonAccept.Content = OsLocalization.Trader.Label17;
            LabelStohasticUp.Content = OsLocalization.Trader.Label149;
            LabelStochasticLow.Content = OsLocalization.Trader.Label150;
            LabelStep.Content = OsLocalization.Trader.Label151;
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
                    Convert.ToInt32(StochUp.Text) <= 0 ||
                    Convert.ToInt32(StochDown.Text) <= 0 ||
                    Convert.ToInt32(Step.Text) <= 0 ||
                    TextBoxSlippage.Text.ToDecimal() < 0)
                {
                    throw new Exception("");
                }
            }
            catch (Exception)
            {
                MessageBox.Show(OsLocalization.Trader.Label13);
                return;
            }

            _strategy.VolumeType = Convert.ToString(ComboBoxVolumeType.Text);
            _strategy.TradeAssetInPortfolio = Convert.ToString(TextBoxAssetInPortfolio.Text);
            _strategy.Slippage = TextBoxSlippage.Text.ToDecimal();
            _strategy.Volume = TextBoxVolumeOne.Text.ToDecimal();
            _strategy.Upline = StochUp.Text.ToDecimal();
            _strategy.Downline = StochDown.Text.ToDecimal();
            _strategy.Step = Step.Text.ToDecimal();

            Enum.TryParse(ComboBoxRegime.Text, true, out _strategy.Regime);

            _strategy.Save();
            Close();
        }
    }
}

