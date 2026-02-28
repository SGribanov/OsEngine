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
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace OsEngine.OsTrader.Grids
{
    public class TradeGridStopBy
    {
        #region Service

        public bool StopGridByMoveUpIsOn = false;
        public decimal StopGridByMoveUpValuePercent = 2.5m;
        public TradeGridRegime StopGridByMoveUpReaction = TradeGridRegime.CloseForced;

        public bool StopGridByMoveDownIsOn = false;
        public decimal StopGridByMoveDownValuePercent = 2.5m;
        public TradeGridRegime StopGridByMoveDownReaction = TradeGridRegime.CloseForced;

        public bool StopGridByPositionsCountIsOn = false;
        public int StopGridByPositionsCountValue = 200;
        public TradeGridRegime StopGridByPositionsCountReaction = TradeGridRegime.CloseForced;

        public bool StopGridByLifeTimeIsOn = false;
        public int StopGridByLifeTimeSecondsToLife = 600;
        public TradeGridRegime StopGridByLifeTimeReaction = TradeGridRegime.CloseForced;

        public bool StopGridByTimeOfDayIsOn = false;
        public int StopGridByTimeOfDayHour = 14;
        public int StopGridByTimeOfDayMinute = 15;
        public int StopGridByTimeOfDaySecond = 0;
        public TradeGridRegime StopGridByTimeOfDayReaction = TradeGridRegime.CloseForced;

        public string GetSaveString()
        {
            string result = "";

            result += StopGridByMoveUpIsOn + "@";
            result += StopGridByMoveUpValuePercent.ToString(CultureInfo.InvariantCulture) + "@";
            result += StopGridByMoveUpReaction + "@";

            result += StopGridByMoveDownIsOn + "@";
            result += StopGridByMoveDownValuePercent.ToString(CultureInfo.InvariantCulture) + "@";
            result += StopGridByMoveDownReaction + "@";

            result += StopGridByPositionsCountIsOn + "@";
            result += StopGridByPositionsCountValue.ToString(CultureInfo.InvariantCulture) + "@";
            result += StopGridByPositionsCountReaction + "@";

            result += StopGridByLifeTimeIsOn + "@";
            result += StopGridByLifeTimeSecondsToLife.ToString(CultureInfo.InvariantCulture) + "@";
            result += StopGridByLifeTimeReaction + "@";

            result += StopGridByTimeOfDayIsOn + "@";
            result += StopGridByTimeOfDayHour.ToString(CultureInfo.InvariantCulture) + "@";
            result += StopGridByTimeOfDayMinute.ToString(CultureInfo.InvariantCulture) + "@";
            result += StopGridByTimeOfDaySecond.ToString(CultureInfo.InvariantCulture) + "@";
            result += StopGridByTimeOfDayReaction + "@";

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

                // stop grid by event

                if (values.Length > 0 && string.IsNullOrWhiteSpace(values[0]) == false)
                {
                    StopGridByMoveUpIsOn = Convert.ToBoolean(values[0]);
                }
                if (values.Length > 1 && string.IsNullOrWhiteSpace(values[1]) == false)
                {
                    StopGridByMoveUpValuePercent = values[1].ToDecimal();
                }
                if (values.Length > 2 && string.IsNullOrWhiteSpace(values[2]) == false)
                {
                    Enum.TryParse(values[2], out StopGridByMoveUpReaction);
                }

                if (values.Length > 3 && string.IsNullOrWhiteSpace(values[3]) == false)
                {
                    StopGridByMoveDownIsOn = Convert.ToBoolean(values[3]);
                }
                if (values.Length > 4 && string.IsNullOrWhiteSpace(values[4]) == false)
                {
                    StopGridByMoveDownValuePercent = values[4].ToDecimal();
                }
                if (values.Length > 5 && string.IsNullOrWhiteSpace(values[5]) == false)
                {
                    Enum.TryParse(values[5], out StopGridByMoveDownReaction);
                }

                if (values.Length > 6 && string.IsNullOrWhiteSpace(values[6]) == false)
                {
                    StopGridByPositionsCountIsOn = Convert.ToBoolean(values[6]);
                }
                if (values.Length > 7 && string.IsNullOrWhiteSpace(values[7]) == false)
                {
                    StopGridByPositionsCountValue = Convert.ToInt32(values[7], CultureInfo.InvariantCulture);
                }
                if (values.Length > 8 && string.IsNullOrWhiteSpace(values[8]) == false)
                {
                    Enum.TryParse(values[8], out StopGridByPositionsCountReaction);
                }

                if (values.Length > 9 && string.IsNullOrWhiteSpace(values[9]) == false)
                {
                    StopGridByLifeTimeIsOn = Convert.ToBoolean(values[9]);
                }
                if (values.Length > 10 && string.IsNullOrWhiteSpace(values[10]) == false)
                {
                    StopGridByLifeTimeSecondsToLife = Convert.ToInt32(values[10], CultureInfo.InvariantCulture);
                }
                if (values.Length > 11 && string.IsNullOrWhiteSpace(values[11]) == false)
                {
                    Enum.TryParse(values[11], out StopGridByLifeTimeReaction);
                }

                if (values.Length > 12 && string.IsNullOrWhiteSpace(values[12]) == false)
                {
                    StopGridByTimeOfDayIsOn = Convert.ToBoolean(values[12]);
                }
                if (values.Length > 13 && string.IsNullOrWhiteSpace(values[13]) == false)
                {
                    StopGridByTimeOfDayHour = Convert.ToInt32(values[13], CultureInfo.InvariantCulture);
                }
                if (values.Length > 14 && string.IsNullOrWhiteSpace(values[14]) == false)
                {
                    StopGridByTimeOfDayMinute = Convert.ToInt32(values[14], CultureInfo.InvariantCulture);
                }
                if (values.Length > 15 && string.IsNullOrWhiteSpace(values[15]) == false)
                {
                    StopGridByTimeOfDaySecond = Convert.ToInt32(values[15], CultureInfo.InvariantCulture);
                }
                if (values.Length > 16 && string.IsNullOrWhiteSpace(values[16]) == false)
                {
                    Enum.TryParse(values[16], out StopGridByTimeOfDayReaction);
                }
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(),LogMessageType.Error);
            }
        }

        #endregion

        #region Logic

        public TradeGridRegime GetRegime(TradeGrid grid, BotTabSimple tab)
        {
            if (grid == null || tab == null)
            {
                return TradeGridRegime.On;
            }

            if(StopGridByMoveUpIsOn == false
                &&  StopGridByMoveDownIsOn == false
                && StopGridByPositionsCountIsOn == false
                && StopGridByLifeTimeIsOn == false
                && StopGridByTimeOfDayIsOn == false)
            {
                return TradeGridRegime.On;
            }

            // 1 смена режима по кол-ву закрытых позиций
            if (StopGridByPositionsCountIsOn == true)
            {
                int openPositionsCount = grid.OpenPositionsCount;

                if(openPositionsCount >= StopGridByPositionsCountValue)
                {
                    string message = "Auto-stop grid by positions count. \n";
                    message += "Open positions in grid: " + openPositionsCount + "\n";
                    message += "Max open positions: " + StopGridByPositionsCountValue + "\n";
                    message += "New regime: " + StopGridByPositionsCountReaction;
                    SendNewLogMessage(message, LogMessageType.Signal);

                    return StopGridByPositionsCountReaction;
                }
            }

            // 2 смена режима по движению от первой цены сетки
            if (StopGridByMoveUpIsOn == true 
                || StopGridByMoveDownIsOn == true)
            {
                List<Candle> candles = tab.CandlesAll;

                if(candles == null || candles.Count == 0)
                {
                    return TradeGridRegime.On;
                }

                Candle lastCandle = candles[candles.Count - 1];
                if (lastCandle == null)
                {
                    return TradeGridRegime.On;
                }

                decimal lastSecurityPrice = lastCandle.Close;

                decimal firstGridPrice = grid.FirstPriceReal;

                if(lastSecurityPrice != 0 
                    && firstGridPrice != 0)
                {
                    if (StopGridByMoveUpIsOn)
                    {
                        decimal upLimit = firstGridPrice + firstGridPrice * (StopGridByMoveUpValuePercent / 100);

                        if(lastSecurityPrice >= upLimit)
                        {
                            string message = "Auto-stop grid by move Up. \n";
                            message += "First real price in grid: " + firstGridPrice + "\n";
                            message += "Up limit in %: " + StopGridByMoveUpValuePercent + "\n";
                            message += "Price limit: " + upLimit + "\n";
                            message += "New regime: " + StopGridByMoveUpReaction;
                            SendNewLogMessage(message, LogMessageType.Signal);

                            return StopGridByMoveUpReaction;
                        }
                    }

                    if (StopGridByMoveDownIsOn)
                    {
                        decimal downLimit = firstGridPrice - firstGridPrice * (StopGridByMoveDownValuePercent / 100);

                        if (lastSecurityPrice <= downLimit)
                        {
                            string message = "Auto-stop grid by move Down. \n";
                            message += "First real price in grid: " + firstGridPrice + "\n";
                            message += "Down limit in %: " + StopGridByMoveDownValuePercent + "\n";
                            message += "Price limit: " + downLimit + "\n";
                            message += "New regime: " + StopGridByMoveDownReaction;
                            SendNewLogMessage(message, LogMessageType.Signal);

                            return StopGridByMoveDownReaction;
                        }
                    }
                }
            }

            // 3 смена режима по времени жизни
            if (StopGridByLifeTimeIsOn
                && grid.FirstTradeTime != DateTime.MinValue)
            {
                DateTime time = tab.TimeServerCurrent;

                if (grid.FirstTradeTime.AddSeconds(StopGridByLifeTimeSecondsToLife) < time)
                {
                    string message = "Auto-stop grid by life time. \n";
                    message += "First order time in grid: " + grid.FirstTradeTime.ToString() + "\n";
                    message += "Seconds to life: " + StopGridByLifeTimeSecondsToLife + "\n";
                    message += "Time now: " + time.ToString(OsLocalization.CurCulture) + "\n";
                    message += "New regime: " + StopGridByLifeTimeReaction;
                    SendNewLogMessage(message, LogMessageType.Signal);

                    return StopGridByLifeTimeReaction;
                }

            }

            // 4 смена режима по времени внутри дня
            if (StopGridByTimeOfDayIsOn)
            {
                DateTime time = tab.TimeServerCurrent;

                bool isActivate = false;

                if (time.Hour == StopGridByTimeOfDayHour
                    && time.Minute == StopGridByTimeOfDayMinute
                    && time.Second >= StopGridByTimeOfDaySecond)
                {
                    isActivate = true;
                }
                else if (time.Hour == StopGridByTimeOfDayHour
                    && time.Minute > StopGridByTimeOfDayMinute)
                {
                    isActivate = true;
                }
                else if (time.Hour > StopGridByTimeOfDayHour)
                {
                    isActivate = true;
                }

                if(isActivate == true)
                {
                    string message = "Auto-stop grid by time of day. \n";
                    message += "Current server time: " + time.ToString() + "\n";
                    message += "New regime: " + StopGridByLifeTimeReaction;
                    SendNewLogMessage(message, LogMessageType.Signal);

                    return StopGridByTimeOfDayReaction;
                }
            }

            return TradeGridRegime.On;
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

