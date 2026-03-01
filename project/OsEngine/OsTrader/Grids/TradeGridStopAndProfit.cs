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
    public class TradeGridStopAndProfit
    {
        #region Service

        public OnOffRegime ProfitRegime = OnOffRegime.Off;
        public TradeGridValueType ProfitValueType = TradeGridValueType.Percent;
        public decimal ProfitValue = 1.5m;
        public bool StopTradingAfterProfit = true;

        public OnOffRegime StopRegime = OnOffRegime.Off;
        public TradeGridValueType StopValueType = TradeGridValueType.Percent;
        public decimal StopValue = 0.8m;

        public OnOffRegime TrailStopRegime = OnOffRegime.Off;
        public TradeGridValueType TrailStopValueType = TradeGridValueType.Percent;
        public decimal TrailStopValue = 0.8m;

        public string GetSaveString()
        {
            string result = "";

            result += ProfitRegime + "@";
            result += ProfitValueType + "@";
            result += ProfitValue.ToString(CultureInfo.InvariantCulture) + "@";
            result += StopRegime + "@";
            result += StopValueType + "@";
            result += StopValue.ToString(CultureInfo.InvariantCulture) + "@";
            result += TrailStopRegime + "@";
            result += TrailStopValueType + "@";
            result += TrailStopValue.ToString(CultureInfo.InvariantCulture) + "@";
            result += StopTradingAfterProfit + "@";
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
                    if (TryParseEnumValue(values[0], out OnOffRegime parsed))
                    {
                        ProfitRegime = parsed;
                    }
                }
                if (values.Length > 1 && string.IsNullOrWhiteSpace(values[1]) == false)
                {
                    if (TryParseEnumValue(values[1], out TradeGridValueType parsed))
                    {
                        ProfitValueType = parsed;
                    }
                }
                if (values.Length > 2 && string.IsNullOrWhiteSpace(values[2]) == false)
                {
                    if (TryParsePositiveDecimal(values[2], out decimal parsed))
                    {
                        ProfitValue = parsed;
                    }
                }

                if (values.Length > 3 && string.IsNullOrWhiteSpace(values[3]) == false)
                {
                    if (TryParseEnumValue(values[3], out OnOffRegime parsed))
                    {
                        StopRegime = parsed;
                    }
                }
                if (values.Length > 4 && string.IsNullOrWhiteSpace(values[4]) == false)
                {
                    if (TryParseEnumValue(values[4], out TradeGridValueType parsed))
                    {
                        StopValueType = parsed;
                    }
                }
                if (values.Length > 5 && string.IsNullOrWhiteSpace(values[5]) == false)
                {
                    if (TryParsePositiveDecimal(values[5], out decimal parsed))
                    {
                        StopValue = parsed;
                    }
                }

                if (values.Length > 6 && string.IsNullOrWhiteSpace(values[6]) == false)
                {
                    if (TryParseEnumValue(values[6], out OnOffRegime parsed))
                    {
                        TrailStopRegime = parsed;
                    }
                }
                if (values.Length > 7 && string.IsNullOrWhiteSpace(values[7]) == false)
                {
                    if (TryParseEnumValue(values[7], out TradeGridValueType parsed))
                    {
                        TrailStopValueType = parsed;
                    }
                }
                if (values.Length > 8 && string.IsNullOrWhiteSpace(values[8]) == false)
                {
                    if (TryParsePositiveDecimal(values[8], out decimal parsed))
                    {
                        TrailStopValue = parsed;
                    }
                }

                if (values.Length > 9 && string.IsNullOrWhiteSpace(values[9]) == false)
                {
                    if (TryParseBoolFlexible(values[9], out bool parsed))
                    {
                        StopTradingAfterProfit = parsed;
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

        public void Process(TradeGrid grid)
        {
            if (grid == null)
            {
                return;
            }

            TradeGridCreator gridCreator = grid.GridCreator;
            BotTabSimple tab = grid.Tab;
            Security security = tab?.Security;

            if (gridCreator == null || tab == null || security == null)
            {
                return;
            }

            List<TradeGridLine> lines = grid.GetLinesWithOpenPosition();

            if (lines == null
                || lines.Count == 0)
            {
                return;
            }

            List<Position> positions = new List<Position>();

            for(int i = 0;i < lines.Count;i++)
            {
                Position pos = lines[i].Position;

                if(pos == null)
                {
                    continue;
                }

                positions.Add(pos);
            }

            if (TrailStopRegime == OnOffRegime.On
                && TrailStopValue > 0)
            {
                SetTrailStop(grid, positions);
            }

            if (ProfitRegime != OnOffRegime.Off
             || StopRegime != OnOffRegime.Off)
            {
                decimal middleEntryPrice = grid.MiddleEntryPrice;

                if (middleEntryPrice == 0)
                {
                    return;
                }

                if (ProfitRegime == OnOffRegime.On
                     && ProfitValue > 0)
                {
                    SetProfit(grid, middleEntryPrice, positions);
                }

                if (StopRegime == OnOffRegime.On
                     && StopValue > 0)
                {
                    SetStop(grid, middleEntryPrice, positions);
                }
            }
        }

        private void SetProfit(TradeGrid grid, decimal middleEntryPrice, List<Position> positions)
        {
            if (grid == null || positions == null || positions.Count == 0)
            {
                return;
            }

            TradeGridCreator gridCreator = grid.GridCreator;
            BotTabSimple tab = grid.Tab;
            Security security = tab?.Security;
            if (gridCreator == null || tab == null || security == null)
            {
                return;
            }

            decimal profitPrice = 0;

            if (gridCreator.GridSide == Side.Buy)
            {
                if (ProfitValueType == TradeGridValueType.Absolute)
                {
                    profitPrice = middleEntryPrice + ProfitValue;
                }
                else if(ProfitValueType == TradeGridValueType.Percent)
                {
                    profitPrice = middleEntryPrice + middleEntryPrice * (ProfitValue/100);
                }

                profitPrice = tab.RoundPrice(profitPrice, security, Side.Buy);
            }
            else if (gridCreator.GridSide == Side.Sell)
            {
                if (ProfitValueType == TradeGridValueType.Absolute)
                {
                    profitPrice = middleEntryPrice - ProfitValue;
                }
                else if (ProfitValueType == TradeGridValueType.Percent)
                {
                    profitPrice = middleEntryPrice - middleEntryPrice * (ProfitValue / 100);
                }

                profitPrice = tab.RoundPrice(profitPrice, security, Side.Sell);
            }

            if(profitPrice == 0)
            {
                return;
            }

            for(int i = 0;i < positions.Count;i++)
            {
                Position pos = positions[i];

                if (pos == null
                    || pos.OpenVolume == 0
                    || pos.State == PositionStateType.Done)
                {
                    continue;
                }

                if(pos.ProfitOrderPrice == profitPrice)
                {
                    continue;
                }

                pos.ProfitOrderPrice = profitPrice;
            }
        }

        private void SetStop(TradeGrid grid, decimal middleEntryPrice, List<Position> positions)
        {
            if (grid == null || positions == null || positions.Count == 0)
            {
                return;
            }

            TradeGridCreator gridCreator = grid.GridCreator;
            BotTabSimple tab = grid.Tab;
            Security security = tab?.Security;
            if (gridCreator == null || tab == null || security == null)
            {
                return;
            }

            decimal stopPrice = 0;

            if (gridCreator.GridSide == Side.Buy)
            {
                if (StopValueType == TradeGridValueType.Absolute)
                {
                    stopPrice = middleEntryPrice - StopValue;
                }
                else if (StopValueType == TradeGridValueType.Percent)
                {
                    stopPrice = middleEntryPrice - middleEntryPrice * (StopValue / 100);
                }

                stopPrice = tab.RoundPrice(stopPrice, security, Side.Buy);
            }
            else if (gridCreator.GridSide == Side.Sell)
            {
                if (StopValueType == TradeGridValueType.Absolute)
                {
                    stopPrice = middleEntryPrice + StopValue;
                }
                else if (StopValueType == TradeGridValueType.Percent)
                {
                    stopPrice = middleEntryPrice + middleEntryPrice * (StopValue / 100);
                }

                stopPrice = tab.RoundPrice(stopPrice, security, Side.Sell);
            }

            if (stopPrice == 0)
            {
                return;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];

                if (pos == null
                    || pos.OpenVolume == 0
                    || pos.State == PositionStateType.Done)
                {
                    continue;
                }

                if (pos.StopOrderRedLine == stopPrice)
                {
                    continue;
                }

                tab.CloseAtStopMarket(pos, stopPrice);
            }
        }

        private void SetTrailStop(TradeGrid grid, List<Position> positions)
        {
            if (grid == null || positions == null || positions.Count == 0)
            {
                return;
            }

            TradeGridCreator gridCreator = grid.GridCreator;
            BotTabSimple tab = grid.Tab;
            Security security = tab?.Security;
            if (gridCreator == null || tab == null || security == null)
            {
                return;
            }

            List<Candle> candles = tab.CandlesAll;

            if (candles == null || candles.Count == 0)
            {
                return;
            }

            Candle lastCandle = candles[candles.Count - 1];
            if (lastCandle == null)
            {
                return;
            }

            decimal lastPrice = lastCandle.Close;

            decimal stopPrice = 0;

            if (gridCreator.GridSide == Side.Buy)
            {
                if (TrailStopValueType == TradeGridValueType.Absolute)
                {
                    stopPrice = lastPrice - TrailStopValue;
                }
                else if (TrailStopValueType == TradeGridValueType.Percent)
                {
                    stopPrice = lastPrice - lastPrice * (TrailStopValue / 100);
                }

                stopPrice = tab.RoundPrice(stopPrice, security, Side.Buy);
            }
            else if (gridCreator.GridSide == Side.Sell)
            {
                if (TrailStopValueType == TradeGridValueType.Absolute)
                {
                    stopPrice = lastPrice + TrailStopValue;
                }
                else if (TrailStopValueType == TradeGridValueType.Percent)
                {
                    stopPrice = lastPrice + lastPrice * (TrailStopValue / 100);
                }

                stopPrice = tab.RoundPrice(stopPrice, security, Side.Sell);
            }

            if (stopPrice == 0)
            {
                return;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];

                if (pos == null
                    || pos.OpenVolume == 0
                    || pos.State == PositionStateType.Done
                    || pos.CloseActive == true)
                {
                    continue;
                }

                if (pos.StopOrderRedLine == stopPrice)
                {
                    continue;
                }

                tab.CloseAtTrailingStopMarket(pos, stopPrice);
            }

            decimal maxPrice = decimal.MinValue;
            decimal minPrice = decimal.MaxValue;

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];

                if (pos == null
                    || pos.OpenVolume == 0
                    || pos.State == PositionStateType.Done
                    || pos.CloseActive == true)
                {
                    continue;
                }

                if (pos.StopOrderRedLine == 0)
                {
                    continue;
                }

                if(pos.StopOrderRedLine > maxPrice)
                {
                    maxPrice = pos.StopOrderRedLine;
                }
                if(pos.StopOrderRedLine < minPrice)
                {
                    minPrice = pos.StopOrderRedLine;
                }
            }

            if(maxPrice == decimal.MinValue
                || minPrice == decimal.MaxValue)
            {
                return;
            }
            
            if(maxPrice != minPrice)
            {

                for (int i = 0; i < positions.Count; i++)
                {
                    Position pos = positions[i];

                    if (pos == null
                        || pos.OpenVolume == 0
                        || pos.State == PositionStateType.Done
                        || pos.CloseActive == true)
                    {
                        continue;
                    }

                    if (pos.Direction == Side.Buy)
                    {
                        tab.CloseAtTrailingStopMarket(pos, maxPrice);
                    }
                    else if (pos.Direction == Side.Sell)
                    {
                        tab.CloseAtTrailingStopMarket(pos, minPrice);
                    }
                }
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

