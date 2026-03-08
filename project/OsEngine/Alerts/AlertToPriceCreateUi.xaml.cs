#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 *Ваши права на использования кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;
using System.Windows;
using OsEngine.Entity;
using OsEngine.Language;

namespace OsEngine.Alerts
{
    /// <summary>
    /// Interaction logic for PriceAlertCreateUi.xaml
    /// Логика взаимодействия для PriceAlertCreateUi.xaml
    /// </summary>
    public partial class AlertToPriceCreateUi
    {
        public AlertToPriceCreateUi(AlertToPrice alert)
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);
            MyAlert = alert;

            CheckBoxOnOff.IsChecked = MyAlert.IsOn;

            ComboBoxActivationType.Items.Add(PriceAlertTypeActivation.PriceLowerOrEqual);
            ComboBoxActivationType.Items.Add(PriceAlertTypeActivation.PriceHigherOrEqual);
            ComboBoxActivationType.SelectedItem = MyAlert.TypeActivation;

            TextBoxPriceActivation.Text = MyAlert.PriceActivation.ToString(new CultureInfo("RU-ru"));

            ComboBoxSignalType.Items.Add(SignalType.None);
            ComboBoxSignalType.Items.Add(SignalType.Buy);
            ComboBoxSignalType.Items.Add(SignalType.Sell);
            ComboBoxSignalType.Items.Add(SignalType.CloseAll);
            ComboBoxSignalType.Items.Add(SignalType.CloseOne);
            ComboBoxSignalType.Items.Add(SignalType.OpenNew);
            ComboBoxSignalType.Items.Add(SignalType.ReloadProfit);
            ComboBoxSignalType.Items.Add(SignalType.ReloadStop);
            ComboBoxSignalType.SelectedItem = MyAlert.SignalType;

            ComboBoxOrderType.Items.Add(OrderPriceType.Limit);
            ComboBoxOrderType.Items.Add(OrderPriceType.Market);
            ComboBoxOrderType.SelectedItem = MyAlert.OrderPriceType;

            ComboBoxSlippageType.Items.Add(AlertSlippageType.Persent.ToString());
            ComboBoxSlippageType.Items.Add(AlertSlippageType.PriceStep.ToString());
            ComboBoxSlippageType.Items.Add(AlertSlippageType.Absolute.ToString());
            ComboBoxSlippageType.SelectedItem = MyAlert.SlippageType.ToString();

            TextBoxVolumeReaction.Text = MyAlert.VolumeReaction.ToString();
            TextBoxSlippage.Text = MyAlert.Slippage.ToString(new CultureInfo("RU-ru"));
            TextBoxClosePosition.Text = MyAlert.NumberClosePosition.ToString();

            CheckBoxWindow.IsChecked = MyAlert.MessageIsOn;
            TextBoxAlertMessage.Text = MyAlert.Message;

            ComboBoxMusic.Items.Add(AlertMusic.Bird);
            ComboBoxMusic.Items.Add(AlertMusic.Duck);
            ComboBoxMusic.Items.Add(AlertMusic.Wolf);
            ComboBoxMusic.SelectedItem = MyAlert.MusicType;

            LabelOsa.MouseDown += LabelOsa_MouseDown;
            ChangeText();
            OsLocalization.LocalizationTypeChangeEvent += ChangeText;

            LabelOsa.MouseDown += LabelOsa_MouseDown;

            this.Activate();
            this.Focus();
        }

        private void ChangeText()
        {
            Title = OsLocalization.Alerts.TitleAlertToChartCreateUi;
            CheckBoxOnOff.Content = OsLocalization.Alerts.Label1;
            LabelActivation.Content = OsLocalization.Alerts.Label18;

            LabelTrade.Content = OsLocalization.Alerts.Label3;
            LabelReactionType.Content = OsLocalization.Alerts.Label4;
            LabelOrderType.Content = OsLocalization.Alerts.Label5;
            LabelVolume.Content = OsLocalization.Alerts.Label6;
            LabelSlippage.Content = OsLocalization.Alerts.Label7;
            LabelNumClosedPos.Content = OsLocalization.Alerts.Label8;
            LabelFireworks.Content = OsLocalization.Alerts.Label9;
            CheckBoxMusicAlert.Content = OsLocalization.Alerts.Label10;
            CheckBoxWindow.Content = OsLocalization.Alerts.Label16;
            ButtonSave.Content = OsLocalization.Alerts.Label17;
            LabelSlippageType.Content = OsLocalization.Alerts.Label19;
            LabelActivationPrice.Content = OsLocalization.Alerts.Label20;
        }

        void LabelOsa_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://o-s-a.net");
        }

        public AlertToPrice MyAlert;

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

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {

            if (CheckBoxOnOff.IsChecked.HasValue)
            {
                MyAlert.IsOn = CheckBoxOnOff.IsChecked.Value;
            }
 
            if (TryGetSelectedEnum(ComboBoxActivationType.SelectedItem, out PriceAlertTypeActivation activationType))
            {
                MyAlert.TypeActivation = activationType;
            }

            if (TryReadDecimal(TextBoxPriceActivation.Text, out decimal priceActivation))
            {
                MyAlert.PriceActivation = priceActivation;
            }

            if (TryGetSelectedEnum(ComboBoxSignalType.SelectedItem, out SignalType signalType))
            {
                MyAlert.SignalType = signalType;
            }

            if (TryGetSelectedEnum(ComboBoxOrderType.SelectedItem, out OrderPriceType orderPriceType))
            {
                MyAlert.OrderPriceType = orderPriceType;
            }

            if (TryReadDecimal(TextBoxVolumeReaction.Text, out decimal volumeReaction))
            {
                MyAlert.VolumeReaction = volumeReaction;
            }

            if (TryReadDecimal(TextBoxSlippage.Text, out decimal slippage))
            {
                MyAlert.Slippage = slippage;
            }

            if (TryReadInt(TextBoxClosePosition.Text, out int numberClosePosition))
            {
                MyAlert.NumberClosePosition = numberClosePosition;
            }

            if (TryGetSelectedEnum(ComboBoxSlippageType.SelectedItem, out AlertSlippageType slippageType))
            {
                MyAlert.SlippageType = slippageType;
            }

            if (CheckBoxWindow.IsChecked.HasValue)
            {
                MyAlert.MessageIsOn = CheckBoxWindow.IsChecked.Value;
            }

            MyAlert.Message = TextBoxAlertMessage.Text;
            if (TryGetSelectedEnum(ComboBoxMusic.SelectedItem, out AlertMusic musicType))
            {
                MyAlert.MusicType = musicType;
            }

            Close();
        }
    }
}

