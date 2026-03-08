#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Language;
using System;
using System.Globalization;
using System.Windows;
using OsEngine.Entity;


namespace OsEngine.OsTrader.Panels.Tab
{
    /// <summary>
    /// Interaction logic for BotTabPairCommonSettingsUi.xaml
    /// </summary>
    public partial class BotTabPairCommonSettingsUi : Window
    {
        BotTabPair _tabPair;

        public BotTabPairCommonSettingsUi(BotTabPair tabPair)
        {
            InitializeComponent();

            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);

            _tabPair = tabPair;

            // локализация

            Title = OsLocalization.Trader.Label248;

            LabelOrdersPlacement.Content = OsLocalization.Trader.Label244;
            LabelIndicators.Content = OsLocalization.Trader.Label251;

            LabelSecurity1.Content = OsLocalization.Trader.Label102 + " 1";
            LabelSecurity2.Content = OsLocalization.Trader.Label102 + " 2";

            LabelSlippage1.Content = OsLocalization.Trader.Label92;
            LabelSlippage2.Content = OsLocalization.Trader.Label92;

            LabelVolume1.Content = OsLocalization.Trader.Label223;
            LabelVolume2.Content = OsLocalization.Trader.Label223;

            ButtonPositionSupport.Content = OsLocalization.Trader.Label47;

            LabelRegime1.Content = OsLocalization.Trader.Label115;
            LabelRegime2.Content = OsLocalization.Trader.Label115;

            LabelCorrelation.Content = OsLocalization.Trader.Label242;
            LabelCointegration.Content = OsLocalization.Trader.Label238;


            LabelCorrelationLookBack.Content = OsLocalization.Trader.Label240;
            LabelCointegrationLookBack.Content = OsLocalization.Trader.Label240;

            LabelCointegrationDeviation.Content = OsLocalization.Trader.Label239;

            ButtonSave.Content = OsLocalization.Trader.Label246;
            ButtonApply.Content = OsLocalization.Trader.Label247;

            CheckBoxCointegrationAutoIsOn.Content = OsLocalization.Trader.Label309;
            CheckBoxCorrelationAutoIsOn.Content = OsLocalization.Trader.Label309;

            // стартовые значения

            ComboBoxSlippageTypeSec1.Items.Add(PairTraderSlippageType.Absolute.ToString());
            ComboBoxSlippageTypeSec1.Items.Add(PairTraderSlippageType.Percent.ToString());
            ComboBoxSlippageTypeSec1.SelectedItem = _tabPair.Sec1SlippageType.ToString();

            ComboBoxVolumeTypeSec1.Items.Add(PairTraderVolumeType.Contract.ToString());
            ComboBoxVolumeTypeSec1.Items.Add(PairTraderVolumeType.Currency.ToString());
            ComboBoxVolumeTypeSec1.SelectedItem = _tabPair.Sec1VolumeType.ToString();

            ComboBoxSlippageTypeSec2.Items.Add(PairTraderSlippageType.Absolute.ToString());
            ComboBoxSlippageTypeSec2.Items.Add(PairTraderSlippageType.Percent.ToString());
            ComboBoxSlippageTypeSec2.SelectedItem = _tabPair.Sec2SlippageType.ToString();

            ComboBoxVolumeTypeSec2.Items.Add(PairTraderVolumeType.Contract.ToString());
            ComboBoxVolumeTypeSec2.Items.Add(PairTraderVolumeType.Currency.ToString());
            ComboBoxVolumeTypeSec2.SelectedItem = _tabPair.Sec2VolumeType.ToString();

            ComboBoxRegime1.Items.Add(PairTraderSecurityTradeRegime.Off.ToString());
            ComboBoxRegime1.Items.Add(PairTraderSecurityTradeRegime.Limit.ToString());
            ComboBoxRegime1.Items.Add(PairTraderSecurityTradeRegime.Market.ToString());
            ComboBoxRegime1.Items.Add(PairTraderSecurityTradeRegime.Second.ToString());
            ComboBoxRegime1.SelectedItem = _tabPair.Sec1TradeRegime.ToString();

            ComboBoxRegime2.Items.Add(PairTraderSecurityTradeRegime.Off.ToString());
            ComboBoxRegime2.Items.Add(PairTraderSecurityTradeRegime.Limit.ToString());
            ComboBoxRegime2.Items.Add(PairTraderSecurityTradeRegime.Market.ToString());
            ComboBoxRegime2.Items.Add(PairTraderSecurityTradeRegime.Second.ToString());
            ComboBoxRegime2.SelectedItem = _tabPair.Sec2TradeRegime.ToString();

            TextBoxSlippage1.Text = _tabPair.Sec1Slippage.ToString();
            TextBoxSlippage2.Text = _tabPair.Sec2Slippage.ToString();

            TextBoxVolume1.Text = _tabPair.Sec1Volume.ToString();
            TextBoxVolume2.Text = _tabPair.Sec2Volume.ToString();

            TextBoxCorrelationLookBack.Text = _tabPair.CorrelationLookBack.ToString();
            TextBoxCointegrationLookBack.Text = _tabPair.CointegrationLookBack.ToString();
            TextBoxCointegrationDeviation.Text = _tabPair.CointegrationDeviation.ToString();

            CheckBoxCorrelationAutoIsOn.IsChecked = _tabPair.AutoRebuildCorrelation;
            CheckBoxCointegrationAutoIsOn.IsChecked = _tabPair.AutoRebuildCointegration;

            ButtonSave.Click += ButtonSave_Click;
            ButtonApply.Click += ButtonApply_Click;
            ButtonPositionSupport.Click += ButtonPositionSupport_Click;
            Closed += BotTabPairCommonSettingsUi_Closed;
        }

        private void BotTabPairCommonSettingsUi_Closed(object sender, EventArgs e)
        {
            try
            {
                ButtonSave.Click -= ButtonSave_Click;
                ButtonApply.Click -= ButtonApply_Click;
                Closed -= BotTabPairCommonSettingsUi_Closed;
                ButtonPositionSupport.Click -= ButtonPositionSupport_Click;

                _tabPair = null;
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSettingsFromUi();
                _tabPair.ApplySettingsFromStandardToAll();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveSettingsFromUi();
                Close();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private void ButtonPositionSupport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _tabPair.StandardManualControl.ShowDialog(_tabPair.StartProgram);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private void SaveSettingsFromUi()
        {
            try
            {
                if (TryGetSelectedEnum(ComboBoxSlippageTypeSec1.SelectedItem, out PairTraderSlippageType sec1SlippageType))
                {
                    _tabPair.Sec1SlippageType = sec1SlippageType;
                }

                if (TryGetSelectedEnum(ComboBoxVolumeTypeSec1.SelectedItem, out PairTraderVolumeType sec1VolumeType))
                {
                    _tabPair.Sec1VolumeType = sec1VolumeType;
                }

                if (TryGetSelectedEnum(ComboBoxSlippageTypeSec2.SelectedItem, out PairTraderSlippageType sec2SlippageType))
                {
                    _tabPair.Sec2SlippageType = sec2SlippageType;
                }

                if (TryGetSelectedEnum(ComboBoxVolumeTypeSec2.SelectedItem, out PairTraderVolumeType sec2VolumeType))
                {
                    _tabPair.Sec2VolumeType = sec2VolumeType;
                }

                if (TryGetSelectedEnum(ComboBoxRegime1.SelectedItem, out PairTraderSecurityTradeRegime sec1TradeRegime))
                {
                    _tabPair.Sec1TradeRegime = sec1TradeRegime;
                }

                if (TryGetSelectedEnum(ComboBoxRegime2.SelectedItem, out PairTraderSecurityTradeRegime sec2TradeRegime))
                {
                    _tabPair.Sec2TradeRegime = sec2TradeRegime;
                }

                if (TryReadDecimal(TextBoxSlippage1.Text, out decimal sec1Slippage))
                {
                    _tabPair.Sec1Slippage = sec1Slippage;
                }

                if (TryReadDecimal(TextBoxSlippage2.Text, out decimal sec2Slippage))
                {
                    _tabPair.Sec2Slippage = sec2Slippage;
                }

                if (TryReadDecimal(TextBoxVolume1.Text, out decimal sec1Volume))
                {
                    _tabPair.Sec1Volume = sec1Volume;
                }

                if (TryReadDecimal(TextBoxVolume2.Text, out decimal sec2Volume))
                {
                    _tabPair.Sec2Volume = sec2Volume;
                }

                if (TryReadInt(TextBoxCorrelationLookBack.Text, out int correlationLookBack))
                {
                    _tabPair.CorrelationLookBack = correlationLookBack;
                }

                if (TryReadInt(TextBoxCointegrationLookBack.Text, out int cointegrationLookBack))
                {
                    _tabPair.CointegrationLookBack = cointegrationLookBack;
                }

                if (TryReadDecimal(TextBoxCointegrationDeviation.Text, out decimal cointegrationDeviation))
                {
                    _tabPair.CointegrationDeviation = cointegrationDeviation;
                }

                _tabPair.AutoRebuildCointegration = CheckBoxCointegrationAutoIsOn.IsChecked == true;
                _tabPair.AutoRebuildCorrelation = CheckBoxCorrelationAutoIsOn.IsChecked == true;

                _tabPair.SaveStandartSettings();
            }
            catch(Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private static bool TryGetSelectedEnum<TEnum>(object selectedItem, out TEnum value)
            where TEnum : struct
        {
            value = default;

            if (selectedItem is TEnum typedValue)
            {
                value = typedValue;
                return true;
            }

            return selectedItem != null
                   && Enum.TryParse(selectedItem.ToString(), true, out value);
        }

        private static bool TryReadDecimal(string text, out decimal value)
        {
            return decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value)
                   || decimal.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out value)
                   || decimal.TryParse(text, NumberStyles.Any, CultureInfo.GetCultureInfo("ru-RU"), out value);
        }

        private static bool TryReadInt(string text, out int value)
        {
            return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
                   || int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out value);
        }
    }
}
