#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.Market;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OsEngine.OsTrader.Grids
{
    public class TradeGridAutoStarter
    {
        #region Service

        public TradeGridAutoStartRegime AutoStartRegime;

        public decimal AutoStartPrice;

        public GridAutoStartShiftFirstPriceRegime RebuildGridRegime;

        public decimal ShiftFirstPrice;

        public bool StartGridByTimeOfDayIsOn = false;

        public int StartGridByTimeOfDayHour = 14;

        public int StartGridByTimeOfDayMinute = 15;

        public int StartGridByTimeOfDaySecond = 0;

        public bool SingleActivationMode = true;

        public string GetSaveString()
        {
            string result = "";

            result += AutoStartRegime + "@";
            result += AutoStartPrice.ToString(CultureInfo.InvariantCulture) + "@";
            result += RebuildGridRegime + "@";
            result += ShiftFirstPrice.ToString(CultureInfo.InvariantCulture) + "@";
            result += StartGridByTimeOfDayIsOn +"@";
            result += StartGridByTimeOfDayHour.ToString(CultureInfo.InvariantCulture) + "@";
            result += StartGridByTimeOfDayMinute.ToString(CultureInfo.InvariantCulture) + "@";
            result += StartGridByTimeOfDaySecond.ToString(CultureInfo.InvariantCulture) + "@";
            result += SingleActivationMode;
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

                // auto start
                if (values.Length > 0 && string.IsNullOrWhiteSpace(values[0]) == false)
                {
                    if (TryParseEnumValue(values[0], out TradeGridAutoStartRegime parsed))
                    {
                        AutoStartRegime = parsed;
                    }
                }
                if (values.Length > 1 && string.IsNullOrWhiteSpace(values[1]) == false)
                {
                    if (TryParseDecimalFlexible(values[1], out decimal parsed))
                    {
                        AutoStartPrice = parsed;
                    }
                }

                if (values.Length > 2 && string.IsNullOrWhiteSpace(values[2]) == false)
                {
                    if (TryParseEnumValue(values[2], out GridAutoStartShiftFirstPriceRegime parsed))
                    {
                        RebuildGridRegime = parsed;
                    }
                }
                if (values.Length > 3 && string.IsNullOrWhiteSpace(values[3]) == false)
                {
                    if (TryParseDecimalFlexible(values[3], out decimal parsed))
                    {
                        ShiftFirstPrice = parsed;
                    }
                }

                if (values.Length > 4 && string.IsNullOrWhiteSpace(values[4]) == false)
                {
                    if (TryParseBoolFlexible(values[4], out bool parsed))
                    {
                        StartGridByTimeOfDayIsOn = parsed;
                    }
                }
                if (values.Length > 5 && string.IsNullOrWhiteSpace(values[5]) == false)
                {
                    if (TryParseRangeInt(values[5], 0, 23, out int parsed))
                    {
                        StartGridByTimeOfDayHour = parsed;
                    }
                }
                if (values.Length > 6 && string.IsNullOrWhiteSpace(values[6]) == false)
                {
                    if (TryParseRangeInt(values[6], 0, 59, out int parsed))
                    {
                        StartGridByTimeOfDayMinute = parsed;
                    }
                }
                if (values.Length > 7 && string.IsNullOrWhiteSpace(values[7]) == false)
                {
                    if (TryParseRangeInt(values[7], 0, 59, out int parsed))
                    {
                        StartGridByTimeOfDaySecond = parsed;
                    }
                }
                if (values.Length > 8 && string.IsNullOrWhiteSpace(values[8]) == false)
                {
                    if (TryParseBoolFlexible(values[8], out bool parsed))
                    {
                        SingleActivationMode = parsed;
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

        private static bool TryParseDecimalFlexible(string value, out decimal parsed)
        {
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
            {
                return true;
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
            {
                return true;
            }

            parsed = 0;
            return false;
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

        public bool HaveEventToStart(TradeGrid grid)
        {
            if (grid == null)
            {
                return false;
            }

            BotTabSimple tab = grid.Tab;

            if (tab == null)
            {
                return false;
            }

            if(AutoStartRegime != TradeGridAutoStartRegime.Off)
            {
                List<Candle> candles = tab.CandlesAll;

                if (candles == null
                    || candles.Count == 0)
                {
                    return false;
                }

                Candle lastCandle = candles[candles.Count - 1];

                if (lastCandle == null)
                {
                    return false;
                }

                decimal price = lastCandle.Close;

                if (price == 0)
                {
                    return false;
                }

                if (AutoStartRegime == TradeGridAutoStartRegime.HigherOrEqual
                    && price >= AutoStartPrice)
                {
                    string message = "Auto-start grid. \n";
                    message += "Auto-starter price regime: " + AutoStartRegime.ToString() + "\n";
                    message += "Auto-starter price: " + AutoStartPrice + "\n";
                    message += "Market price: " + price;

                    SendNewLogMessage(message, LogMessageType.Signal);

                    if(SingleActivationMode == true)
                    {
                        AutoStartRegime = TradeGridAutoStartRegime.Off;
                    }

                    return true;
                }
                else if (AutoStartRegime == TradeGridAutoStartRegime.LowerOrEqual
                    && price <= AutoStartPrice)
                {
                    string message = "Auto-start grid. \n";
                    message += "Auto-starter price regime: " + AutoStartRegime.ToString() + "\n";
                    message += "Auto-starter price: " + AutoStartPrice + "\n";
                    message += "Market price: " + price;
                    SendNewLogMessage(message, LogMessageType.Signal);

                    if (SingleActivationMode == true)
                    {
                        AutoStartRegime = TradeGridAutoStartRegime.Off;
                    }

                    return true;
                }
            }

            if (StartGridByTimeOfDayIsOn)
            {
                DateTime time = tab.TimeServerCurrent;

                if(time != DateTime.MinValue)
                {
                    bool isActivate = false;

                    if (time.Hour == StartGridByTimeOfDayHour
                        && time.Minute == StartGridByTimeOfDayMinute
                        && time.Second >= StartGridByTimeOfDaySecond)
                    {
                        isActivate = true;
                    }
                    else if (time.Hour == StartGridByTimeOfDayHour
                        && time.Minute > StartGridByTimeOfDayMinute)
                    {
                        isActivate = true;
                    }
                    else if (time.Hour > StartGridByTimeOfDayHour)
                    {
                        isActivate = true;
                    }

                    if (isActivate == true)
                    {
                        string message = "Auto-start grid by time of day. \n";
                        message += "Current server time: " + time.ToString();
                        SendNewLogMessage(message, LogMessageType.Signal);

                        if (SingleActivationMode == true)
                        {
                            StartGridByTimeOfDayIsOn = false;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public decimal GetNewGridPriceStart(TradeGrid grid)
        {
            if (grid == null)
            {
                return 0;
            }

            BotTabSimple tab = grid.Tab;
            TradeGridCreator gridCreator = grid.GridCreator;

            if (tab == null || gridCreator == null)
            {
                return 0;
            }

            List<Candle> candles = tab.CandlesAll;

            if(candles == null 
                || candles.Count == 0)
            {
                return 0;
            }

            Candle lastCandle = candles[^1];

            if (lastCandle == null)
            {
                return 0;
            }

            decimal lastPrice = lastCandle.Close;

            if(lastPrice == 0)
            {
                return 0;
            }

            decimal result = lastPrice;

            if(ShiftFirstPrice != 0)
            {
                Security security = tab.Security;
                if (security == null)
                {
                    return 0;
                }

                result = result + result * (ShiftFirstPrice / 100);

                result = tab.RoundPrice(result, security, gridCreator.GridSide);
            }

            return result;
        }

        public void ShiftGridOnNewPrice(decimal newPrice, TradeGrid grid)
        {
            if (grid == null)
            {
                return;
            }

            TradeGridCreator gridCreator = grid.GridCreator;
            List<TradeGridLine> lines = gridCreator?.Lines;

            if (lines == null 
                || lines.Count == 0)
            {
                return;
            }

            decimal maxEntryPriceInGrid = decimal.MinValue;
            decimal minEntryPriceInGrid = decimal.MaxValue;
            Side? gridSide = null;

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];

                if (line == null)
                {
                    continue;
                }

                if (gridSide == null)
                {
                    gridSide = line.Side;
                }

                if(line.PriceEnter > maxEntryPriceInGrid)
                {
                    maxEntryPriceInGrid = line.PriceEnter;
                }

                if(line.PriceEnter <  minEntryPriceInGrid)
                {
                    minEntryPriceInGrid = line.PriceEnter;
                }
            }

            if(maxEntryPriceInGrid == 0 
                || minEntryPriceInGrid == 0
                || maxEntryPriceInGrid == decimal.MinValue
                || minEntryPriceInGrid == decimal.MaxValue
                || gridSide == null)
            {
                return;
            }

            decimal shift = 0;

            if (gridSide == Side.Buy)
            {
                shift = newPrice - maxEntryPriceInGrid;
            }
            else if (gridSide == Side.Sell)
            {
                shift = newPrice - minEntryPriceInGrid;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];

                if (line == null)
                {
                    continue;
                }

                line.CanReplaceExitOrder = true;
                line.PriceEnter += shift;
                line.PriceExit += shift;
            }
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

    public enum TradeGridAutoStartRegime
    {
        Off,
        HigherOrEqual,
        LowerOrEqual
    }

    public enum GridAutoStartShiftFirstPriceRegime
    {
        Off,
        On_FullRebuild,
        On_ShiftOnNewPrice
    }
}

