#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Entity;
using OsEngine.Language;
using System;
using System.Globalization;
using System.Diagnostics;
using System.Windows;

namespace OsEngine.OsTrader.Panels.Tab
{
    /// <summary>
    /// Interaction logic for BotTabPolygonCommonSettingsUi.xaml
    /// </summary>
    public partial class BotTabPolygonCommonSettingsUi : Window
    {
        BotTabPolygon _polygon;

        public BotTabPolygonCommonSettingsUi(BotTabPolygon polygon)
        {
            InitializeComponent();
            _polygon = polygon;

            OsEngine.Layout.StickyBorders.Listen(this);
            OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);

            TextBoxSeparatorToSecurities.Text = polygon.SeparatorToSecurities;

            ComboBoxCommissionType.Items.Add(CommissionPolygonType.None.ToString());
            ComboBoxCommissionType.Items.Add(CommissionPolygonType.Percent.ToString());
            ComboBoxCommissionType.SelectedItem = _polygon.CommissionType.ToString();

            TextBoxCommissionValue.Text = _polygon.CommissionValue.ToString();
            CheckBoxCommisionIsSubstract.IsChecked = _polygon.CommissionIsSubstract;

            ComboBoxDelayType.Items.Add(DelayPolygonType.ByExecution.ToString());
            ComboBoxDelayType.Items.Add(DelayPolygonType.InMLS.ToString());
            ComboBoxDelayType.Items.Add(DelayPolygonType.Instantly.ToString());
            ComboBoxDelayType.SelectedItem = _polygon.DelayType.ToString();

            TextBoxDelayMls.Text = _polygon.DelayMls.ToString();
            TextBoxLimitQtyStart.Text = _polygon.QtyStart.ToString();
            TextBoxLimitSlippage.Text = _polygon.SlippagePercent.ToString();

            TextBoxProfitToSignal.Text = _polygon.ProfitToSignal.ToString();

            ComboBoxActionOnSignalType.Items.Add(PolygonActionOnSignalType.Bot_Event.ToString());
            ComboBoxActionOnSignalType.Items.Add(PolygonActionOnSignalType.All.ToString());
            ComboBoxActionOnSignalType.Items.Add(PolygonActionOnSignalType.Alert.ToString());
            ComboBoxActionOnSignalType.Items.Add(PolygonActionOnSignalType.None.ToString());
            ComboBoxActionOnSignalType.SelectedItem = _polygon.ActionOnSignalType.ToString();

            ComboBoxOrderPriceType.Items.Add(OrderPriceType.Limit.ToString());
            ComboBoxOrderPriceType.Items.Add(OrderPriceType.Market.ToString());
            ComboBoxOrderPriceType.SelectedItem = _polygon.OrderPriceType.ToString();

            // Localization

            LabelProfitToSignal.Content = OsLocalization.Trader.Label335;
            LabelActionOnSignalType.Content = OsLocalization.Trader.Label336;

            LabelStartSecutiySettings.Content = OsLocalization.Trader.Label315;
            LabelCommissionSettings.Content = OsLocalization.Trader.Label316;
            LabelSeparator.Content = OsLocalization.Trader.Label319;
            LabelCommissionType.Content = OsLocalization.Trader.Label320;
            LabelCommissionValue.Content = OsLocalization.Trader.Label321;
            CheckBoxCommisionIsSubstract.Content = OsLocalization.Trader.Label322;

            LabelQtyStartLimit.Content = OsLocalization.Trader.Label325;
            LabelSlippageLimit.Content = OsLocalization.Trader.Label326;

            LabelExecutionSettings.Content = OsLocalization.Trader.Label329;
            LabelDelay.Content = OsLocalization.Trader.Label330;
            LabelInterval.Content = OsLocalization.Trader.Label331;

            ButtonSave.Content = OsLocalization.Trader.Label246;
            ButtonApply.Content = OsLocalization.Trader.Label247;

            LabelExecution.Content = OsLocalization.Trader.Label337;
            LabelOrderPriceType.Content = OsLocalization.Trader.Label338;

            Title = OsLocalization.Trader.Label232;

            ButtonSave.Click += ButtonSave_Click;
            ButtonApply.Click += ButtonApply_Click;

            this.Closed += BotTabPolygonCommonSettingsUi_Closed;
        }

        private void BotTabPolygonCommonSettingsUi_Closed(object sender, EventArgs e)
        {
            ButtonSave.Click -= ButtonSave_Click;
            ButtonApply.Click -= ButtonApply_Click;
            this.Closed -= BotTabPolygonCommonSettingsUi_Closed;
            _polygon = null;
        }

        private void ButtonApply_Click(object sender, RoutedEventArgs e)
        {
            SaveSettingsFromUiToBot();
            _polygon.ApplyStandardSettingsToAllSequence();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveSettingsFromUiToBot();
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

        private void SaveSettingsFromUiToBot()
        {
            try
            {
                if (TryGetSelectedEnum(ComboBoxOrderPriceType.SelectedItem, out OrderPriceType orderPriceType))
                {
                    _polygon.OrderPriceType = orderPriceType;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }

            try
            {
                if (TryGetSelectedEnum(ComboBoxActionOnSignalType.SelectedItem, out PolygonActionOnSignalType actionOnSignalType))
                {
                    _polygon.ActionOnSignalType = actionOnSignalType;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }

            try
            {
                if (TryReadDecimal(TextBoxProfitToSignal.Text, out decimal profitToSignal))
                {
                    _polygon.ProfitToSignal = profitToSignal;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }

            try
            {
                if (TryReadDecimal(TextBoxLimitSlippage.Text, out decimal slippagePercent))
                {
                    _polygon.SlippagePercent = slippagePercent;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }

            try
            {
                if (TryReadDecimal(TextBoxLimitQtyStart.Text, out decimal qtyStart))
                {
                    _polygon.QtyStart = qtyStart;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }

            try
            {
                if (TryReadInt(TextBoxDelayMls.Text, out int delayMls))
                {
                    _polygon.DelayMls = delayMls;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }

            try
            {
                if (TryGetSelectedEnum(ComboBoxDelayType.SelectedItem, out DelayPolygonType delayType))
                {
                    _polygon.DelayType = delayType;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }

            try
            {
                _polygon.CommissionIsSubstract = CheckBoxCommisionIsSubstract.IsChecked == true;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }

            try
            {
                if (TryReadDecimal(TextBoxCommissionValue.Text, out decimal commissionValue))
                {
                    _polygon.CommissionValue = commissionValue;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }

            try
            {
                if (TryGetSelectedEnum(ComboBoxCommissionType.SelectedItem, out CommissionPolygonType commissionType))
                {
                    _polygon.CommissionType = commissionType;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }

            try
            {
                _polygon.SeparatorToSecurities = TextBoxSeparatorToSecurities.Text;
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }

            _polygon.SaveStandartSettings();
        }
    }
}

