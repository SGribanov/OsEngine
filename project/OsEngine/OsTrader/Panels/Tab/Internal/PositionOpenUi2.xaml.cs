#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Layout;
using OsEngine.Market;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

namespace OsEngine.OsTrader.Panels.Tab.Internal
{
    /// <summary>
    /// Interaction logic for PositionOpenUi2.xaml
    /// </summary>
    public partial class PositionOpenUi2 : Window
    {
        public PositionOpenUi2(BotTabSimple tab)
        {
            InitializeComponent();
            OsEngine.Layout.StickyBorders.Listen(this);

            Title = OsLocalization.Trader.Label196;

            Tab = tab;
            ActivateMarketDepth();
            Tab.MarketDepthUpdateEvent += Tab_MarketDepthUpdateEvent;
            Tab.BestBidAskChangeEvent += Tab_BestBidAskChangeEvent;
            Tab.Connector.ConnectorStartedReconnectEvent += Connector_ConnectorStartedReconnectEvent;
            Closed += PositionOpenUi2_Closed;

            LabelServerTypeValue.Content = Tab.Connector.ServerType;
            LabelSecurityValue.Content = Tab.Connector.SecurityName;
            LabelTabNameValue.Content = Tab.TabName;

            LabelServerType.Content = OsLocalization.Trader.Label178 + ":";
            LabelSecurity.Content = OsLocalization.Trader.Label102 + ":";
            LabelTabName.Content = OsLocalization.Trader.Label194 + ":";

            LabelLimitPrice.Content = OsLocalization.Trader.Label205;
            LabelStopPrice.Content = OsLocalization.Trader.Label205;
            LabelFakePrice.Content = OsLocalization.Trader.Label205;

            LabelStopActivationPrice.Content = OsLocalization.Trader.Label206;
            LabelStopActivationType.Content = OsLocalization.Trader.Label207;

            ComboBoxStopLimitType.Items.Add(StopActivateType.LowerOrEqual.ToString());
            ComboBoxStopLimitType.Items.Add(StopActivateType.HigherOrEqual.ToString());
            ComboBoxStopLimitType.SelectedItem = StopActivateType.HigherOrEqual.ToString();

            LabelStopLifeTime.Content = OsLocalization.Trader.Label208;
            LabelStopLifeTimeType.Content = OsLocalization.Trader.Label212;

            ComboBoxStopLifetimeType.Items.Add(PositionOpenerToStopLifeTimeType.CandlesCount.ToString());
            ComboBoxStopLifetimeType.Items.Add(PositionOpenerToStopLifeTimeType.NoLifeTime.ToString());
            ComboBoxStopLifetimeType.SelectedItem = PositionOpenerToStopLifeTimeType.CandlesCount.ToString();

            LabelLimitVolume.Content = OsLocalization.Trader.Label30;
            LabelMarketVolume.Content = OsLocalization.Trader.Label30;
            LabelStopVolume.Content = OsLocalization.Trader.Label30;
            LabelFakeVolume.Content = OsLocalization.Trader.Label30;

            CheckBoxIsEmulator.Content = OsLocalization.Trader.Label204;
            ButtonBuy.Content = OsLocalization.Trader.Label198;
            ButtonSell.Content = OsLocalization.Trader.Label199;

            TabItemLimit.Header = OsLocalization.Trader.Label200;
            TabItemMarket.Header = OsLocalization.Trader.Label201;
            TabItemStopLimit.Header = OsLocalization.Trader.Label202;
            TabItemFake.Header = OsLocalization.Trader.Label203;

            LabelFakeOpenDate.Content = OsLocalization.Trader.Label209;
            LabelFakeOpenTime.Content = OsLocalization.Trader.Label210;
            ButtonFakeTimeOpenNow.Content = OsLocalization.Trader.Label211;

            TextBoxLimitVolume.Text = "1";
            TextBoxMarketVolume.Text = "1";
            TextBoxStopVolume.Text = "1";
            TextBoxStopLifeTime.Text = "1";
            TextBoxFakeVolume.Text = "1";

            if (Tab.StartProgram == StartProgram.IsTester)
            {
                CheckBoxIsEmulator.IsEnabled = false;
            }
            else
            {
                CheckBoxIsEmulator.IsChecked = Tab.EmulatorIsOn;
                CheckBoxIsEmulator.Click += CheckBoxIsEmulator_Click;

                Tab.EmulatorIsOnChangeStateEvent += Tab_EmulatorIsOnChangeStateEvent;
            }

            GlobalGUILayout.Listen(this, "mD_" + Tab.TabName);

            SetNowTimeInControlsFakeOpenPos();
        }

        private void Tab_EmulatorIsOnChangeStateEvent(bool value)
        {
            RepaintMainLabels();
        }

        private void Connector_ConnectorStartedReconnectEvent(string arg1, TimeFrame arg2, TimeSpan arg3, string arg4, string arg5)
        {
            RepaintMainLabels();
        }

        private void RepaintMainLabels()
        {
            if (TextBoxLimitPrice.Dispatcher.CheckAccess() == false)
            {
                TextBoxLimitPrice.Dispatcher.InvokeAsync(new Action(RepaintMainLabels));
                return;
            }

            LabelServerTypeValue.Content = Tab.Connector.ServerType;
            LabelSecurityValue.Content = Tab.Connector.SecurityName;
            LabelTabNameValue.Content = Tab.TabName;
            CheckBoxIsEmulator.IsChecked = Tab.EmulatorIsOn;
        }

        private void CheckBoxIsEmulator_Click(object sender, RoutedEventArgs e)
        {
            Tab.EmulatorIsOn = CheckBoxIsEmulator.IsChecked == true;
        }

        MarketDepthPainter _marketDepthPainter;

        public BotTabSimple Tab;

        private void PositionOpenUi2_Closed(object sender, EventArgs e)
        {
            try
            {
                CheckBoxIsEmulator.Click -= CheckBoxIsEmulator_Click;
                Closed -= PositionOpenUi2_Closed;

                if(Tab != null)
                {
                    Tab.MarketDepthUpdateEvent -= Tab_MarketDepthUpdateEvent;
                    Tab.BestBidAskChangeEvent -= Tab_BestBidAskChangeEvent;
                    Tab.EmulatorIsOnChangeStateEvent -= Tab_EmulatorIsOnChangeStateEvent;
                    Tab.Connector.ConnectorStartedReconnectEvent -= Connector_ConnectorStartedReconnectEvent;
                    Tab = null;
                }

                if(_marketDepthPainter != null)
                {
                    _marketDepthPainter.UserClickOnMDAndSelectPriceEvent -= _marketDepthPainter_UserClickOnMDAndSelectPriceEvent;
                    _marketDepthPainter.StopPaint();
                    _marketDepthPainter.Delete();
                    _marketDepthPainter = null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
            }
        }

        private void ActivateMarketDepth()
        {
            _marketDepthPainter = new MarketDepthPainter(Tab.TabName + "OpenPosGui", Tab.Connector);
            _marketDepthPainter.ProcessMarketDepth(Tab.MarketDepth);
            _marketDepthPainter.StartPaint(WinFormsHostMarketDepth, null, null );
            _marketDepthPainter.UserClickOnMDAndSelectPriceEvent += _marketDepthPainter_UserClickOnMDAndSelectPriceEvent;
        }

        private void _marketDepthPainter_UserClickOnMDAndSelectPriceEvent(decimal priceSelectedUser)
        {
            if (TextBoxLimitPrice.Dispatcher.CheckAccess() == false)
            {
                TextBoxLimitPrice.Dispatcher.InvokeAsync(new Action(() => _marketDepthPainter_UserClickOnMDAndSelectPriceEvent(priceSelectedUser)));
                return;
            }

            TextBoxLimitPrice.Text = priceSelectedUser.ToStringWithNoEndZero();
            TextBoxStopActivationPrice.Text = priceSelectedUser.ToStringWithNoEndZero();
            TextBoxStopPrice.Text = priceSelectedUser.ToStringWithNoEndZero();
            TextBoxFakePrice.Text = priceSelectedUser.ToStringWithNoEndZero();
        }

        private void Tab_BestBidAskChangeEvent(decimal bid, decimal ask)
        {
            _marketDepthPainter.ProcessBidAsk(bid, ask);
        }

        private void Tab_MarketDepthUpdateEvent(MarketDepth md)
        {
            _marketDepthPainter.ProcessMarketDepth(md);
        }

        private void ButtonFakeTimeOpenNow_Click(object sender, RoutedEventArgs e)
        {
            SetNowTimeInControlsFakeOpenPos();
        }

        private void ButtonBuy_Click(object sender, RoutedEventArgs e)
        {
            if (TabControlTypePosition.SelectedIndex == 0)
            {
                BuyAtLimit();
            }
            else if (TabControlTypePosition.SelectedIndex == 1)
            {
                BuyAtMarket();
            }
            else if (TabControlTypePosition.SelectedIndex == 2)
            {
                BuyAtStop();
            }
            else if (TabControlTypePosition.SelectedIndex == 3)
            {
                BuyAtFake();
            }
        }

        private void ButtonSell_Click(object sender, RoutedEventArgs e)
        {
            if (TabControlTypePosition.SelectedIndex == 0)
            {
                SellAtLimit();
            }
            else if (TabControlTypePosition.SelectedIndex == 1)
            {
                SellAtMarket();
            }
            else if (TabControlTypePosition.SelectedIndex == 2)
            {
                SellAtStop();
            }
            else if (TabControlTypePosition.SelectedIndex == 3)
            {
                SellAtFake();
            }
        }

        // Limit

        private void BuyAtLimit()
        {
            decimal price = 0;
            decimal volume = 0;

            try
            {
                price = TextBoxLimitPrice.Text.ToDecimal();
                volume = TextBoxLimitVolume.Text.ToDecimal();
            }
            catch (Exception ex)
            {
                Tab.SetNewLogMessage(ex.Message.ToString(), Logging.LogMessageType.Error);
                return;
            }

            if (price == 0 ||
                volume == 0)
            {
                return;
            }

            Tab.BuyAtLimit(volume, price);
        }

        private void SellAtLimit()
        {
            decimal price = 0;
            decimal volume = 0;

            try
            {
                price = TextBoxLimitPrice.Text.ToDecimal();
                volume = TextBoxLimitVolume.Text.ToDecimal();
            }
            catch (Exception ex)
            {
                Tab.SetNewLogMessage(ex.Message.ToString(), Logging.LogMessageType.Error);
                return;
            }

            if (price == 0 ||
                volume == 0)
            {
                return;
            }

            Tab.SellAtLimit(volume, price);
        }

        // Market

        private void BuyAtMarket()
        {
            decimal volume = 0;

            try
            {
                volume = TextBoxMarketVolume.Text.ToDecimal();
            }
            catch (Exception ex)
            {
                Tab.SetNewLogMessage(ex.Message.ToString(), Logging.LogMessageType.Error);
                return;
            }

            if (volume == 0)
            {
                return;
            }

            Tab.BuyAtMarket(volume);
        }

        private void SellAtMarket()
        {
            decimal volume = 0;

            try
            {
                volume = TextBoxMarketVolume.Text.ToDecimal();
            }
            catch (Exception ex)
            {
                Tab.SetNewLogMessage(ex.Message.ToString(), Logging.LogMessageType.Error);
                return;
            }

            if (volume == 0)
            {
                return;
            }

            Tab.SellAtMarket(volume);
        }

        // Stop-Limit

        private void BuyAtStop()
        {
            decimal volume = 0;
            decimal priceOrder = 0;
            decimal priceActivation = 0;
            StopActivateType stopActivateType = StopActivateType.HigherOrEqual;
            PositionOpenerToStopLifeTimeType lifeTimeType = PositionOpenerToStopLifeTimeType.CandlesCount;
            int lifeTime = 0;

            try
            {
                if (TryReadDecimal(TextBoxStopVolume.Text, out decimal parsedVolume) == false
                    || TryReadDecimal(TextBoxStopActivationPrice.Text, out decimal parsedActivationPrice) == false
                    || TryReadDecimal(TextBoxStopPrice.Text, out decimal parsedOrderPrice) == false
                    || TryReadInt(TextBoxStopLifeTime.Text, out int parsedLifeTime) == false
                    || TryGetSelectedEnum(ComboBoxStopLimitType.SelectedItem, out stopActivateType) == false
                    || TryGetSelectedEnum(ComboBoxStopLifetimeType.SelectedItem, out lifeTimeType) == false)
                {
                    throw new InvalidOperationException("Invalid stop order parameters.");
                }

                volume = parsedVolume;
                priceActivation = parsedActivationPrice;
                priceOrder = parsedOrderPrice;
                lifeTime = parsedLifeTime;
            }
            catch (Exception ex)
            {
                Tab.SetNewLogMessage(ex.Message.ToString(), Logging.LogMessageType.Error);
                return;
            }

            if (volume == 0)
            {
                return;
            }

            Tab.BuyAtStop(volume, priceOrder, priceActivation, stopActivateType, lifeTime, "userSendBuyAtStopFromUi", lifeTimeType);
        }

        private void SellAtStop()
        {
            decimal volume = 0;
            decimal priceOrder = 0;
            decimal priceActivation = 0;
            StopActivateType stopActivateType = StopActivateType.HigherOrEqual;
            PositionOpenerToStopLifeTimeType lifeTimeType = PositionOpenerToStopLifeTimeType.CandlesCount;
            int lifeTime = 0;

            try
            {
                if (TryReadDecimal(TextBoxStopVolume.Text, out decimal parsedVolume) == false
                    || TryReadDecimal(TextBoxStopActivationPrice.Text, out decimal parsedActivationPrice) == false
                    || TryReadDecimal(TextBoxStopPrice.Text, out decimal parsedOrderPrice) == false
                    || TryReadInt(TextBoxStopLifeTime.Text, out int parsedLifeTime) == false
                    || TryGetSelectedEnum(ComboBoxStopLimitType.SelectedItem, out stopActivateType) == false
                    || TryGetSelectedEnum(ComboBoxStopLifetimeType.SelectedItem, out lifeTimeType) == false)
                {
                    throw new InvalidOperationException("Invalid stop order parameters.");
                }

                volume = parsedVolume;
                priceActivation = parsedActivationPrice;
                priceOrder = parsedOrderPrice;
                lifeTime = parsedLifeTime;
            }
            catch (Exception ex)
            {
                Tab.SetNewLogMessage(ex.Message.ToString(), Logging.LogMessageType.Error);
                return;
            }

            if (volume == 0)
            {
                return;
            }

            Tab.SellAtStop(volume, priceOrder, priceActivation, stopActivateType, lifeTime, "userSendSellAtStopFromUi", lifeTimeType);
        }

        // Fake

        private void SetNowTimeInControlsFakeOpenPos()
        {
            if (TextBoxFakeOpenTime.Dispatcher.CheckAccess() == false)
            {
                TextBoxFakeOpenTime.Dispatcher.InvokeAsync(SetNowTimeInControlsFakeOpenPos);
                return;
            }

            DateTime time = Tab.TimeServerCurrent;

            if (time == DateTime.MinValue)
            {
                time = DateTime.Now;
            }

            DatePickerFakeOpenDate.SelectedDate = time.AddHours(-time.Hour).AddMinutes(-time.Minute).AddSeconds(-time.Second);
            string timeStr = time.Hour.ToString() + ":" + time.Minute.ToString();
            TextBoxFakeOpenTime.Text = timeStr;
        }

        private void BuyAtFake()
        {
            decimal price = 0;
            decimal volume = 0;
            DateTime timeOpen = DateTime.MinValue;

            try
            {
                if (TryReadDecimal(TextBoxFakePrice.Text, out decimal parsedPrice) == false
                    || TryReadDecimal(TextBoxFakeVolume.Text, out decimal parsedVolume) == false
                    || TryReadDateTime(DatePickerFakeOpenDate.SelectedDate, TextBoxFakeOpenTime.Text, out DateTime parsedTimeOpen) == false)
                {
                    throw new InvalidOperationException("Invalid fake order parameters.");
                }

                price = parsedPrice;
                volume = parsedVolume;
                timeOpen = parsedTimeOpen;

            }
            catch (Exception ex)
            {
                Tab.SetNewLogMessage(ex.Message.ToString(), Logging.LogMessageType.Error);
                return;
            }

            if (price == 0 ||
                volume == 0 ||
                timeOpen == DateTime.MinValue)
            {
                return;
            }

            Tab.BuyAtFake(volume, price, timeOpen);
        }

        private void SellAtFake()
        {
            decimal price = 0;
            decimal volume = 0;
            DateTime timeOpen = DateTime.MinValue;

            try
            {
                if (TryReadDecimal(TextBoxFakePrice.Text, out decimal parsedPrice) == false
                    || TryReadDecimal(TextBoxFakeVolume.Text, out decimal parsedVolume) == false
                    || TryReadDateTime(DatePickerFakeOpenDate.SelectedDate, TextBoxFakeOpenTime.Text, out DateTime parsedTimeOpen) == false)
                {
                    throw new InvalidOperationException("Invalid fake order parameters.");
                }

                price = parsedPrice;
                volume = parsedVolume;
                timeOpen = parsedTimeOpen;

            }
            catch (Exception ex)
            {
                Tab.SetNewLogMessage(ex.Message.ToString(), Logging.LogMessageType.Error);
                return;
            }

            if (price == 0 ||
                volume == 0 ||
                timeOpen == DateTime.MinValue)
            {
                return;
            }

            Tab.SellAtFake(volume, price, timeOpen);
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

        private static bool TryReadDateTime(DateTime? selectedDate, string timeText, out DateTime value)
        {
            value = DateTime.MinValue;

            if (selectedDate.HasValue == false)
            {
                return false;
            }

            string[] openTimeStr = (timeText ?? string.Empty).Split(':');

            if (openTimeStr.Length != 2
                || TryReadInt(openTimeStr[0], out int hours) == false
                || TryReadInt(openTimeStr[1], out int minutes) == false)
            {
                return false;
            }

            DateTime date = selectedDate.Value.Date;

            if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59)
            {
                return false;
            }

            value = date.AddHours(hours).AddMinutes(minutes);
            return true;
        }
    }
}

