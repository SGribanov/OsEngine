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
                    Enum.TryParse(values[0], out ProfitRegime);
                }
                if (values.Length > 1 && string.IsNullOrWhiteSpace(values[1]) == false)
                {
                    Enum.TryParse(values[1], out ProfitValueType);
                }
                if (values.Length > 2 && string.IsNullOrWhiteSpace(values[2]) == false)
                {
                    ProfitValue = values[2].ToDecimal();
                }

                if (values.Length > 3 && string.IsNullOrWhiteSpace(values[3]) == false)
                {
                    Enum.TryParse(values[3], out StopRegime);
                }
                if (values.Length > 4 && string.IsNullOrWhiteSpace(values[4]) == false)
                {
                    Enum.TryParse(values[4], out StopValueType);
                }
                if (values.Length > 5 && string.IsNullOrWhiteSpace(values[5]) == false)
                {
                    StopValue = values[5].ToDecimal();
                }

                if (values.Length > 6 && string.IsNullOrWhiteSpace(values[6]) == false)
                {
                    Enum.TryParse(values[6], out TrailStopRegime);
                }
                if (values.Length > 7 && string.IsNullOrWhiteSpace(values[7]) == false)
                {
                    Enum.TryParse(values[7], out TrailStopValueType);
                }
                if (values.Length > 8 && string.IsNullOrWhiteSpace(values[8]) == false)
                {
                    TrailStopValue = values[8].ToDecimal();
                }

                if (values.Length > 9 && string.IsNullOrWhiteSpace(values[9]) == false)
                {
                    StopTradingAfterProfit = Convert.ToBoolean(values[9]);
                }
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(),LogMessageType.Error);
            }
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
            decimal profitPrice = 0;

            if (grid.GridCreator.GridSide == Side.Buy)
            {
                if (ProfitValueType == TradeGridValueType.Absolute)
                {
                    profitPrice = middleEntryPrice + ProfitValue;
                }
                else if(ProfitValueType == TradeGridValueType.Percent)
                {
                    profitPrice = middleEntryPrice + middleEntryPrice * (ProfitValue/100);
                }

                profitPrice = grid.Tab.RoundPrice(profitPrice, grid.Tab.Security, Side.Buy);
            }
            else if (grid.GridCreator.GridSide == Side.Sell)
            {
                if (ProfitValueType == TradeGridValueType.Absolute)
                {
                    profitPrice = middleEntryPrice - ProfitValue;
                }
                else if (ProfitValueType == TradeGridValueType.Percent)
                {
                    profitPrice = middleEntryPrice - middleEntryPrice * (ProfitValue / 100);
                }

                profitPrice = grid.Tab.RoundPrice(profitPrice, grid.Tab.Security, Side.Sell);
            }

            if(profitPrice == 0)
            {
                return;
            }

            for(int i = 0;i < positions.Count;i++)
            {
                Position pos = positions[i];

                if(pos.OpenVolume == 0
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
            decimal stopPrice = 0;

            if (grid.GridCreator.GridSide == Side.Buy)
            {
                if (StopValueType == TradeGridValueType.Absolute)
                {
                    stopPrice = middleEntryPrice - StopValue;
                }
                else if (StopValueType == TradeGridValueType.Percent)
                {
                    stopPrice = middleEntryPrice - middleEntryPrice * (StopValue / 100);
                }

                stopPrice = grid.Tab.RoundPrice(stopPrice, grid.Tab.Security, Side.Buy);
            }
            else if (grid.GridCreator.GridSide == Side.Sell)
            {
                if (StopValueType == TradeGridValueType.Absolute)
                {
                    stopPrice = middleEntryPrice + StopValue;
                }
                else if (StopValueType == TradeGridValueType.Percent)
                {
                    stopPrice = middleEntryPrice + middleEntryPrice * (StopValue / 100);
                }

                stopPrice = grid.Tab.RoundPrice(stopPrice, grid.Tab.Security, Side.Sell);
            }

            if (stopPrice == 0)
            {
                return;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];

                if (pos.OpenVolume == 0
                    || pos.State == PositionStateType.Done)
                {
                    continue;
                }

                if (pos.StopOrderRedLine == stopPrice)
                {
                    continue;
                }

                grid.Tab.CloseAtStopMarket(pos, stopPrice);
            }
        }

        private void SetTrailStop(TradeGrid grid, List<Position> positions)
        {
            List<Candle> candles = grid.Tab.CandlesAll;

            if (candles == null || candles.Count == 0)
            {
                return;
            }

            decimal lastPrice = candles[candles.Count - 1].Close;

            decimal stopPrice = 0;

            if (grid.GridCreator.GridSide == Side.Buy)
            {
                if (TrailStopValueType == TradeGridValueType.Absolute)
                {
                    stopPrice = lastPrice - TrailStopValue;
                }
                else if (TrailStopValueType == TradeGridValueType.Percent)
                {
                    stopPrice = lastPrice - lastPrice * (TrailStopValue / 100);
                }

                stopPrice = grid.Tab.RoundPrice(stopPrice, grid.Tab.Security, Side.Buy);
            }
            else if (grid.GridCreator.GridSide == Side.Sell)
            {
                if (TrailStopValueType == TradeGridValueType.Absolute)
                {
                    stopPrice = lastPrice + TrailStopValue;
                }
                else if (TrailStopValueType == TradeGridValueType.Percent)
                {
                    stopPrice = lastPrice + lastPrice * (TrailStopValue / 100);
                }

                stopPrice = grid.Tab.RoundPrice(stopPrice, grid.Tab.Security, Side.Sell);
            }

            if (stopPrice == 0)
            {
                return;
            }

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];

                if (pos.OpenVolume == 0
                    || pos.State == PositionStateType.Done
                    || pos.CloseActive == true)
                {
                    continue;
                }

                if (pos.StopOrderRedLine == stopPrice)
                {
                    continue;
                }

                grid.Tab.CloseAtTrailingStopMarket(pos, stopPrice);
            }

            decimal maxPrice = decimal.MinValue;
            decimal minPrice = decimal.MaxValue;

            for (int i = 0; i < positions.Count; i++)
            {
                Position pos = positions[i];

                if (pos.OpenVolume == 0
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

                    if (pos.OpenVolume == 0
                        || pos.State == PositionStateType.Done
                        || pos.CloseActive == true)
                    {
                        continue;
                    }

                    if (pos.Direction == Side.Buy)
                    {
                        grid.Tab.CloseAtTrailingStopMarket(pos, maxPrice);
                    }
                    else if (pos.Direction == Side.Sell)
                    {
                        grid.Tab.CloseAtTrailingStopMarket(pos, minPrice);
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

