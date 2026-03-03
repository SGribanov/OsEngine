#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Logging;
using OsEngine.Market;
using OsEngine.Market.Connectors;
using OsEngine.Market.Servers;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace OsEngine.OsTrader.Grids
{
    public class TradeGridErrorsReaction
    {
        #region Service

        public TradeGridErrorsReaction(TradeGrid grid)
        {
            _myGrid = grid;
        }

        private TradeGrid? _myGrid;

        public void Delete()
        {
            _myGrid = null;
        }

        public bool FailOpenOrdersReactionIsOn = true;

        public int FailOpenOrdersCountToReaction = 10;

        public int FailOpenOrdersCountFact;

        public bool FailCancelOrdersReactionIsOn = true;

        public int FailCancelOrdersCountToReaction = 10;

        public int FailCancelOrdersCountFact;

        public bool WaitOnStartConnectorIsOn = true;

        public int WaitSecondsOnStartConnector = 30;

        public bool ReduceOrdersCountInMarketOnNoFundsError = true;

        public string GetSaveString()
        {
            string result = "";

            result += FailOpenOrdersReactionIsOn + "@";
            result += "@";
            result += FailOpenOrdersCountToReaction.ToString(CultureInfo.InvariantCulture) + "@";

            result += "@";
            result += FailCancelOrdersCountToReaction.ToString(CultureInfo.InvariantCulture) + "@";
            result += FailCancelOrdersReactionIsOn + "@";

            result += WaitOnStartConnectorIsOn + "@";
            result += WaitSecondsOnStartConnector.ToString(CultureInfo.InvariantCulture) + "@";

            result += ReduceOrdersCountInMarketOnNoFundsError + "@";

            result += "@";
            result += "@";
            result += "@";
            result += "@";
            result += "@"; // пять пустых полей в резерв

            return result;
        }

        public void LoadFromString(string? value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                string[] values = value.Split('@');

                if (values.Length > 0 && string.IsNullOrWhiteSpace(values[0]) == false)
                {
                    if (TryParseBoolFlexible(values[0], out bool parsed))
                    {
                        FailOpenOrdersReactionIsOn = parsed;
                    }
                }
                //Enum.TryParse(values[1], out FailOpenOrdersReaction);
                if (values.Length > 2 && string.IsNullOrWhiteSpace(values[2]) == false)
                {
                    if (TryParsePositiveInt(values[2], out int parsed))
                    {
                        FailOpenOrdersCountToReaction = parsed;
                    }
                }
                //Enum.TryParse(values[3], out FailCancelOrdersReaction);
                if (values.Length > 4 && string.IsNullOrWhiteSpace(values[4]) == false)
                {
                    if (TryParsePositiveInt(values[4], out int parsed))
                    {
                        FailCancelOrdersCountToReaction = parsed;
                    }
                }
                if (values.Length > 5 && string.IsNullOrWhiteSpace(values[5]) == false)
                {
                    if (TryParseBoolFlexible(values[5], out bool parsed))
                    {
                        FailCancelOrdersReactionIsOn = parsed;
                    }
                }

                if (values.Length > 6 && string.IsNullOrWhiteSpace(values[6]) == false)
                {
                    if (TryParseBoolFlexible(values[6], out bool parsed))
                    {
                        WaitOnStartConnectorIsOn = parsed;
                    }
                }
                if (values.Length > 7 && string.IsNullOrWhiteSpace(values[7]) == false)
                {
                    if (TryParsePositiveInt(values[7], out int parsed))
                    {
                        WaitSecondsOnStartConnector = parsed;
                    }
                }
                if (values.Length > 8 && string.IsNullOrWhiteSpace(values[8]) == false)
                {
                    if (TryParseBoolFlexible(values[8], out bool parsed))
                    {
                        ReduceOrdersCountInMarketOnNoFundsError = parsed;
                    }
                }
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        private static bool TryParseBoolFlexible(string value, out bool parsed)
        {
            ReadOnlySpan<char> normalized = value.AsSpan().Trim();

            if (bool.TryParse(normalized, out parsed))
            {
                return true;
            }

            if (normalized.SequenceEqual("1".AsSpan())
                || normalized.Equals("yes".AsSpan(), StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("on".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                parsed = true;
                return true;
            }

            if (normalized.SequenceEqual("0".AsSpan())
                || normalized.Equals("no".AsSpan(), StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("off".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                parsed = false;
                return true;
            }

            parsed = false;
            return false;
        }

        private static bool TryParsePositiveInt(string value, out int parsed)
        {
            ReadOnlySpan<char> valueSpan = value.AsSpan().Trim();

            if (int.TryParse(valueSpan, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed) == false
                && int.TryParse(valueSpan, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed) == false)
            {
                parsed = 0;
                return false;
            }

            if (parsed <= 0)
            {
                parsed = 0;
                return false;
            }

            return true;
        }

        #endregion

        #region Errors collect

        public void PositionClosingFailEvent(Position position)
        {
            try
            {
                if (position == null)
                {
                    return;
                }

                if (position.CloseOrders == null
                 || position.CloseOrders.Count == 0)
                {
                    return;
                }

                Order lastOrder = position.CloseOrders[^1];
                if (lastOrder == null)
                {
                    return;
                }

                if (lastOrder.State == OrderStateType.Fail)
                {
                    FailCancelOrdersCountFact++;
                }

                TryFindNoFundsError(position, false);
            }
            catch(Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        public void PositionOpeningFailEvent(Position position)
        {
            try
            {
                if (position == null)
                {
                    return;
                }

                if (position.OpenOrders == null
                || position.OpenOrders.Count == 0)
                {
                    return;
                }

                Order lastOrder = position.OpenOrders[^1];

                if (lastOrder == null)
                {
                    return;
                }

                if (lastOrder.State == OrderStateType.Fail)
                {
                    FailOpenOrdersCountFact++;
                }

                TryFindNoFundsError(position,true);
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        private DateTime _lastResetTime;

        public bool TryResetErrorsAtStartOfDay(DateTime time)
        {
            if(_lastResetTime.Date == time.Date)
            {
                return false;
            }

            _lastResetTime = time;

            if(FailOpenOrdersCountFact != 0 
                || FailCancelOrdersCountFact != 0)
            {
                FailOpenOrdersCountFact = 0;
                FailCancelOrdersCountFact = 0;
                return true;
            }

            return false;
        }

        #endregion

        #region No funds error reaction

        private void TryFindNoFundsError(Position position, bool isOpenOrder)
        {
            try
            {
                TradeGrid? myGrid = _myGrid;
                if (myGrid == null)
                {
                    return;
                }

                BotTabSimple tab = myGrid.Tab;
                if (tab == null || tab.StartProgram != StartProgram.IsOsTrader)
                {
                    return;
                }

                if(ReduceOrdersCountInMarketOnNoFundsError == false)
                {
                    return;
                }

                ConnectorCandles connector = tab.Connector;
                IServer server = connector?.MyServer;
                if (server == null)
                {
                    return;
                }

                if (server.ServerType != ServerType.TInvest)
                {
                    return;
                }

                AServer tInvest = server as AServer;
                if (tInvest == null || tInvest.Log == null)
                {
                    return;
                }

                List<LogMessage> messages = tInvest.Log.LastErrorMessages;
                if (messages == null || messages.Count == 0)
                {
                    return;
                }

                bool haveNoFundsError = false;

                for (int i = 0; i < messages.Count; i++)
                {
                    LogMessage messageObj = messages[i];
                    if (messageObj == null || string.IsNullOrEmpty(messageObj.Message))
                    {
                        continue;
                    }

                    string message = messageObj.Message;

                    if(message.Contains(OsLocalization.Market.Label301))
                    {
                        haveNoFundsError = true;
                        break;
                    }
                }

                if(haveNoFundsError == true)
                {
                    if(isOpenOrder == true 
                        && myGrid.MaxOpenOrdersInMarket > 1)
                    {
                        myGrid.MaxOpenOrdersInMarket--;
                        myGrid.Save();
                        myGrid.RePaintGrid();

                        string message = "Open order rejected: no funds on deposit.\n";
                        message += "Reduce open orders in market. " + "\n";
                        message += "New value open orders in market: " + myGrid.MaxOpenOrdersInMarket;
                        SendNewLogMessage(message, LogMessageType.Signal);
                    }
                    else if( isOpenOrder == false
                        && myGrid.MaxCloseOrdersInMarket > 1)
                    {
                        myGrid.MaxCloseOrdersInMarket--;
                        myGrid.Save();
                        myGrid.RePaintGrid();
                        string message = "Close order rejected: no funds on deposit.\n";
                        message += "Reduce close orders in market. " + "\n";
                        message += "New value close orders in market: " + myGrid.MaxCloseOrdersInMarket;
                        SendNewLogMessage(message, LogMessageType.Signal);
                    }
                }
            }
            catch(Exception error)
            {
                SendNewLogMessage(error.ToString(),LogMessageType.Error);
            }
        }

        #endregion

        #region Logic on Errors reation

        public TradeGridRegime GetReactionOnErrors(TradeGrid grid)
        {
            if(FailOpenOrdersReactionIsOn == false 
                && FailCancelOrdersReactionIsOn == false)
            {
                return TradeGridRegime.On;
            }

            if(FailOpenOrdersReactionIsOn == true)
            {
                if(FailOpenOrdersCountFact >= FailOpenOrdersCountToReaction)
                {
                    string message = "Open orders threshold reached.\n";
                    message += "Errors count: " + FailOpenOrdersCountFact.ToString() + "\n";
                    message += "New regime: Off";
                    SendNewLogMessage(message, LogMessageType.Signal);

                    return TradeGridRegime.Off;
                }
            }

            if(FailCancelOrdersReactionIsOn == true)
            {
                if (FailCancelOrdersCountFact >= FailCancelOrdersCountToReaction)
                {
                    string message = "Cancel orders threshold reached.\n";
                    message += "Errors count: " + FailCancelOrdersCountFact.ToString() + "\n";
                    message += "New regime: Off";
                    SendNewLogMessage(message, LogMessageType.Signal);

                    return TradeGridRegime.Off;
                }
            }

            return TradeGridRegime.On;
        }

        #endregion

        #region Logic awaiting on start connection

        public bool AwaitOnStartConnector(AServer server)
        {
            if (server == null)
            {
                return false;
            }

            if (WaitOnStartConnectorIsOn == false)
            {
                return false;
            }

            if(WaitSecondsOnStartConnector <= 0)
            {
                return false;
            }

            if(server.LastStartServerTime.AddSeconds(WaitSecondsOnStartConnector) > DateTime.Now)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Log

        public void SendNewLogMessage(string message, LogMessageType type)
        {
            LogMessageEvent?.Invoke(message, type);

            if (LogMessageEvent == null && type == LogMessageType.Error)
            {
                ServerMaster.SendNewLogMessage(message, type);
            }
        }

        public event Action<string, LogMessageType>? LogMessageEvent;

        #endregion

    }
}

