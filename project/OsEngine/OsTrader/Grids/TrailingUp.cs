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
using System.Globalization;

namespace OsEngine.OsTrader.Grids
{
    public class TrailingUp
    {
        #region Service

        public TrailingUp(TradeGrid grid)
        {
            _grid = grid;
        }

        protected TradeGrid? _grid;

        public void Delete()
        {
            _grid = null;
        }

        public bool TrailingUpIsOn;

        public decimal TrailingUpStep;

        public decimal TrailingUpLimit;

        public bool TrailingUpCanMoveExitOrder;

        public bool TrailingDownIsOn;

        public decimal TrailingDownStep;

        public decimal TrailingDownLimit;

        public bool TrailingDownCanMoveExitOrder;

        public virtual string GetSaveString()
        {
            string result = "";

            result += TrailingUpIsOn + "@";
            result += TrailingUpStep.ToString(CultureInfo.InvariantCulture) + "@";
            result += TrailingUpLimit.ToString(CultureInfo.InvariantCulture) + "@";

            result += TrailingDownIsOn + "@";
            result += TrailingDownStep.ToString(CultureInfo.InvariantCulture) + "@";
            result += TrailingDownLimit.ToString(CultureInfo.InvariantCulture) + "@";
            result += TrailingUpCanMoveExitOrder + "@";
            result += TrailingDownCanMoveExitOrder + "@";
            result += "@";
            result += "@";
            result += "@";
            result += "@";
            result += "@"; // пять пустых полей в резерв

            return result;
        }

        public virtual void LoadFromString(string? value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                string[] values = value.Split('@');

                if (values.Length > 0 && string.IsNullOrEmpty(values[0]) == false)
                {
                    if (TryParseBoolFlexible(values[0], out bool parsed))
                    {
                        TrailingUpIsOn = parsed;
                    }
                }
                if (values.Length > 1 && string.IsNullOrEmpty(values[1]) == false)
                {
                    if (TryParseDecimalFlexible(values[1], out decimal parsed))
                    {
                        if (parsed >= 0)
                        {
                            TrailingUpStep = parsed;
                        }
                    }
                }
                if (values.Length > 2 && string.IsNullOrEmpty(values[2]) == false)
                {
                    if (TryParseDecimalFlexible(values[2], out decimal parsed))
                    {
                        if (parsed >= 0)
                        {
                            TrailingUpLimit = parsed;
                        }
                    }
                }

                if (values.Length > 3 && string.IsNullOrEmpty(values[3]) == false)
                {
                    if (TryParseBoolFlexible(values[3], out bool parsed))
                    {
                        TrailingDownIsOn = parsed;
                    }
                }
                if (values.Length > 4 && string.IsNullOrEmpty(values[4]) == false)
                {
                    if (TryParseDecimalFlexible(values[4], out decimal parsed))
                    {
                        if (parsed >= 0)
                        {
                            TrailingDownStep = parsed;
                        }
                    }
                }
                if (values.Length > 5 && string.IsNullOrEmpty(values[5]) == false)
                {
                    if (TryParseDecimalFlexible(values[5], out decimal parsed))
                    {
                        if (parsed >= 0)
                        {
                            TrailingDownLimit = parsed;
                        }
                    }
                }

                if (values.Length > 6 && string.IsNullOrEmpty(values[6]) == false)
                {
                    if (TryParseBoolFlexible(values[6], out bool parsed))
                    {
                        TrailingUpCanMoveExitOrder = parsed;
                    }
                }
                if (values.Length > 7 && string.IsNullOrEmpty(values[7]) == false)
                {
                    if (TryParseBoolFlexible(values[7], out bool parsed))
                    {
                        TrailingDownCanMoveExitOrder = parsed;
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

            if (decimal.TryParse(value, NumberStyles.Any, new CultureInfo("ru-RU"), out parsed))
            {
                return true;
            }

            parsed = 0;
            return false;
        }

        #endregion

        #region Logic

        public virtual bool TryTrailingGrid()
        {
            if (TrailingUpIsOn == false
                && TrailingDownIsOn == false)
            {
                return false;
            }

            TradeGrid? grid = _grid;
            if (grid == null)
            {
                return false;
            }

            BotTabSimple tab = grid.Tab;
            if (tab == null)
            {
                return false;
            }

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

            decimal lastPrice = lastCandle.Close;

            if (lastPrice == 0)
            {
                return false;
            }

            bool trailUpIsDone = false;
            bool trailDownIsDone = false;

            if (TrailingUpIsOn == true
                && TrailingUpStep > 0 
                && TrailingUpLimit > 0)
            {
                trailUpIsDone = TrailingUpMethod(lastPrice);
            }

            if (TrailingDownIsOn == true
                 && TrailingDownStep > 0
                && TrailingDownLimit > 0)
            {
                trailDownIsDone = TrailingDownMethod(lastPrice);
            }

            if(trailUpIsDone == true 
                || trailDownIsDone == true)
            {
                return true;
            }

            return false;
        }

        private bool TrailingUpMethod(decimal lastPrice)
        {
            decimal maxPriceGrid = MaxGridPrice;

            if(maxPriceGrid == 0)
            {
                return false;
            }

            if(lastPrice < maxPriceGrid)
            {
                return false;
            }

            if(maxPriceGrid >= TrailingUpLimit)
            {
                return false;
            }

            decimal different = lastPrice - maxPriceGrid;

            if (different < TrailingUpStep)
            {
                return false;
            }

            if (maxPriceGrid + different >= TrailingUpLimit)
            {
                return false;
            }

            int stepsToUp = Convert.ToInt32(Math.Round(different / TrailingUpStep, 0));

            decimal upValue = stepsToUp * TrailingUpStep;

            ShiftGridUpOnValue(upValue);

            return true;
        }

        private bool TrailingDownMethod(decimal lastPrice)
        {
            decimal minPriceGrid = MinGridPrice;

            if (minPriceGrid == 0)
            {
                return false;
            }

            if (lastPrice > minPriceGrid)
            {
                return false;
            }

            if (minPriceGrid <= TrailingDownLimit)
            {
                return false;
            }

            decimal different = minPriceGrid - lastPrice;

            if (different < TrailingDownStep)
            {
                return false;
            }

            if (minPriceGrid - different <= TrailingDownLimit)
            {
                return false;
            }

            int stepsToDown = Convert.ToInt32(Math.Round(different / TrailingDownStep,0));

            decimal downValue = stepsToDown * TrailingDownStep;

            ShiftGridDownOnValue(downValue);

            return true;
        }

        public decimal MaxGridPrice
        {
            get
            {
                TradeGrid? grid = _grid;
                if (grid == null)
                {
                    return 0;
                }

                TradeGridCreator gridCreator = grid.GridCreator;
                List<TradeGridLine> lines = gridCreator?.Lines;

                if (lines == null || lines.Count == 0)
                {
                    return 0;
                }

                decimal maxPriceGrid = decimal.MinValue;

                for(int i = 0;i < lines.Count;i++)
                {
                    TradeGridLine line = lines[i];
                    if (line == null)
                    {
                        continue;
                    }

                    if (line.PriceEnter >  maxPriceGrid)
                    {
                        maxPriceGrid = line.PriceEnter;
                    }
                }

                if(maxPriceGrid ==  decimal.MinValue)
                {
                    return 0;
                }

                return maxPriceGrid;
            }
        }

        public decimal MinGridPrice
        {
            get
            {
                TradeGrid? grid = _grid;
                if (grid == null)
                {
                    return 0;
                }

                TradeGridCreator gridCreator = grid.GridCreator;
                List<TradeGridLine> lines = gridCreator?.Lines;

                if (lines == null || lines.Count == 0)
                {
                    return 0;
                }

                decimal minPriceGrid = decimal.MaxValue;

                for (int i = 0; i < lines.Count; i++)
                {
                    TradeGridLine line = lines[i];
                    if (line == null)
                    {
                        continue;
                    }

                    if (line.PriceEnter < minPriceGrid)
                    {
                        minPriceGrid = line.PriceEnter;
                    }
                }

                if (minPriceGrid == decimal.MaxValue)
                {
                    return 0;
                }

                return minPriceGrid;
            }
        }

        public void ShiftGridDownOnValue(decimal value)
        {
            TradeGrid? grid = _grid;
            if (grid == null)
            {
                return;
            }

            TradeGridCreator gridCreator = grid.GridCreator;
            List<TradeGridLine> lines = gridCreator?.Lines;

            if (lines == null || lines.Count == 0)
            {
                return;
            }

            for(int i = 0;i < lines.Count;i++)
            {
                TradeGridLine line = lines[i];
                if (line == null)
                {
                    continue;
                }

                line.CanReplaceExitOrder = TrailingDownCanMoveExitOrder;
                line.PriceEnter -= value;
                line.PriceExit -= value;
            }
        }

        public void ShiftGridUpOnValue(decimal value)
        {
            TradeGrid? grid = _grid;
            if (grid == null)
            {
                return;
            }

            TradeGridCreator gridCreator = grid.GridCreator;
            List<TradeGridLine> lines = gridCreator?.Lines;

            if (lines == null || lines.Count == 0)
            {
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];
                if (line == null)
                {
                    continue;
                }

                line.CanReplaceExitOrder = TrailingUpCanMoveExitOrder;
                line.PriceEnter += value;
                line.PriceExit += value;
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
}

