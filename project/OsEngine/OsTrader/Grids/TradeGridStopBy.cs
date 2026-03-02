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
                    if (TryParseBoolFlexible(values[0], out bool parsed))
                    {
                        StopGridByMoveUpIsOn = parsed;
                    }
                }
                if (values.Length > 1 && string.IsNullOrWhiteSpace(values[1]) == false)
                {
                    if (TryParsePositiveDecimal(values[1], out decimal parsed))
                    {
                        StopGridByMoveUpValuePercent = parsed;
                    }
                }
                if (values.Length > 2 && string.IsNullOrWhiteSpace(values[2]) == false)
                {
                    if (TryParseEnumValue(values[2], out TradeGridRegime parsed))
                    {
                        StopGridByMoveUpReaction = parsed;
                    }
                }

                if (values.Length > 3 && string.IsNullOrWhiteSpace(values[3]) == false)
                {
                    if (TryParseBoolFlexible(values[3], out bool parsed))
                    {
                        StopGridByMoveDownIsOn = parsed;
                    }
                }
                if (values.Length > 4 && string.IsNullOrWhiteSpace(values[4]) == false)
                {
                    if (TryParsePositiveDecimal(values[4], out decimal parsed))
                    {
                        StopGridByMoveDownValuePercent = parsed;
                    }
                }
                if (values.Length > 5 && string.IsNullOrWhiteSpace(values[5]) == false)
                {
                    if (TryParseEnumValue(values[5], out TradeGridRegime parsed))
                    {
                        StopGridByMoveDownReaction = parsed;
                    }
                }

                if (values.Length > 6 && string.IsNullOrWhiteSpace(values[6]) == false)
                {
                    if (TryParseBoolFlexible(values[6], out bool parsed))
                    {
                        StopGridByPositionsCountIsOn = parsed;
                    }
                }
                if (values.Length > 7 && string.IsNullOrWhiteSpace(values[7]) == false)
                {
                    if (TryParsePositiveInt(values[7], out int parsed))
                    {
                        StopGridByPositionsCountValue = parsed;
                    }
                }
                if (values.Length > 8 && string.IsNullOrWhiteSpace(values[8]) == false)
                {
                    if (TryParseEnumValue(values[8], out TradeGridRegime parsed))
                    {
                        StopGridByPositionsCountReaction = parsed;
                    }
                }

                if (values.Length > 9 && string.IsNullOrWhiteSpace(values[9]) == false)
                {
                    if (TryParseBoolFlexible(values[9], out bool parsed))
                    {
                        StopGridByLifeTimeIsOn = parsed;
                    }
                }
                if (values.Length > 10 && string.IsNullOrWhiteSpace(values[10]) == false)
                {
                    if (TryParsePositiveInt(values[10], out int parsed))
                    {
                        StopGridByLifeTimeSecondsToLife = parsed;
                    }
                }
                if (values.Length > 11 && string.IsNullOrWhiteSpace(values[11]) == false)
                {
                    if (TryParseEnumValue(values[11], out TradeGridRegime parsed))
                    {
                        StopGridByLifeTimeReaction = parsed;
                    }
                }

                if (values.Length > 12 && string.IsNullOrWhiteSpace(values[12]) == false)
                {
                    if (TryParseBoolFlexible(values[12], out bool parsed))
                    {
                        StopGridByTimeOfDayIsOn = parsed;
                    }
                }
                if (values.Length > 13 && string.IsNullOrWhiteSpace(values[13]) == false)
                {
                    if (TryParseRangeInt(values[13], 0, 23, out int parsed))
                    {
                        StopGridByTimeOfDayHour = parsed;
                    }
                }
                if (values.Length > 14 && string.IsNullOrWhiteSpace(values[14]) == false)
                {
                    if (TryParseRangeInt(values[14], 0, 59, out int parsed))
                    {
                        StopGridByTimeOfDayMinute = parsed;
                    }
                }
                if (values.Length > 15 && string.IsNullOrWhiteSpace(values[15]) == false)
                {
                    if (TryParseRangeInt(values[15], 0, 59, out int parsed))
                    {
                        StopGridByTimeOfDaySecond = parsed;
                    }
                }
                if (values.Length > 16 && string.IsNullOrWhiteSpace(values[16]) == false)
                {
                    if (TryParseEnumValue(values[16], out TradeGridRegime parsed))
                    {
                        StopGridByTimeOfDayReaction = parsed;
                    }
                }
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(),LogMessageType.Error);
            }
        }

        private static bool TryParseBoolFlexible(string value, out bool parsed)
        {
            if (bool.TryParse(value, out parsed))
            {
                return true;
            }

            string normalized = value.Trim();

            if (string.Equals(normalized, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "on", StringComparison.OrdinalIgnoreCase))
            {
                parsed = true;
                return true;
            }

            if (string.Equals(normalized, "0", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "no", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "off", StringComparison.OrdinalIgnoreCase))
            {
                parsed = false;
                return true;
            }

            parsed = false;
            return false;
        }

        private static bool TryParsePositiveDecimal(string value, out decimal parsed)
        {
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed) == false
                && decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed) == false)
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

        private static bool TryParsePositiveInt(string value, out int parsed)
        {
            if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed) == false
                && int.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed) == false)
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

        private static bool TryParseRangeInt(string value, int min, int max, out int parsed)
        {
            if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed) == false
                && int.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed) == false)
            {
                parsed = 0;
                return false;
            }

            if (parsed < min || parsed > max)
            {
                parsed = 0;
                return false;
            }

            return true;
        }

        private static bool TryParseEnumValue<TEnum>(string value, out TEnum parsed)
            where TEnum : struct
        {
            if (Enum.TryParse(value, true, out parsed) == false)
            {
                return false;
            }

            if (Enum.IsDefined(typeof(TEnum), parsed) == false)
            {
                return false;
            }

            return true;
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

