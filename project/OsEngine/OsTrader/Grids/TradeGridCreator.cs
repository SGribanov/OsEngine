#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;
using System.Collections.Generic;
using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.Market;
using OsEngine.OsTrader.Panels.Tab;

namespace OsEngine.OsTrader.Grids
{
    public class TradeGridCreator
    {
        private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

        #region Service

        public Side GridSide = Side.Buy;

        public decimal FirstPrice;

        public int LineCountStart;

        public TradeGridValueType TypeStep;

        public decimal LineStep;

        public decimal StepMultiplicator = 1;

        public TradeGridValueType TypeProfit;

        public decimal ProfitStep;

        public decimal ProfitMultiplicator = 1;

        public TradeGridVolumeType TypeVolume;

        public decimal StartVolume = 1;

        public string TradeAssetInPortfolio = "Prime";

        public decimal MartingaleMultiplicator = 1;

        public string GetSaveString()
        {
            string result = "";

            result += GridSide + "@";
            result += FirstPrice.ToString(CultureInfo.InvariantCulture) + "@";
            result += LineCountStart.ToString(CultureInfo.InvariantCulture) + "@";
            result += TypeStep + "@";
            result += LineStep.ToString(CultureInfo.InvariantCulture) + "@";
            result += StepMultiplicator.ToString(CultureInfo.InvariantCulture) + "@";
            result += TypeProfit + "@";
            result += ProfitStep.ToString(CultureInfo.InvariantCulture) + "@";
            result += ProfitMultiplicator.ToString(CultureInfo.InvariantCulture) + "@";
            result += TypeVolume + "@";
            result += StartVolume.ToString(CultureInfo.InvariantCulture) + "@";
            result += MartingaleMultiplicator.ToString(CultureInfo.InvariantCulture) + "@";
            result += TradeAssetInPortfolio + "@";
            result += GetSaveLinesString() + "@";
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
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] != null)
                    {
                        values[i] = values[i].Trim();
                    }
                }

                if (values.Length > 0 && string.IsNullOrWhiteSpace(values[0]) == false)
                {
                    if (TryParseEnumFlexible(values[0], out Side parsedValue))
                    {
                        if (parsedValue == Side.Buy
                            || parsedValue == Side.Sell)
                        {
                            GridSide = parsedValue;
                        }
                    }
                }
                if (values.Length > 1 && string.IsNullOrWhiteSpace(values[1]) == false)
                {
                    if (TryParseDecimal(values[1], out decimal parsedValue))
                    {
                        if (parsedValue >= 0)
                        {
                            FirstPrice = parsedValue;
                        }
                    }
                }
                if (values.Length > 2 && string.IsNullOrWhiteSpace(values[2]) == false)
                {
                    if (int.TryParse(values[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedValue))
                    {
                        if (parsedValue > 0)
                        {
                            LineCountStart = parsedValue;
                        }
                    }
                }
                if (values.Length > 3 && string.IsNullOrWhiteSpace(values[3]) == false)
                {
                    if (TryParseEnumFlexible(values[3], out TradeGridValueType parsedValue))
                    {
                        if (parsedValue == TradeGridValueType.Absolute
                            || parsedValue == TradeGridValueType.Percent)
                        {
                            TypeStep = parsedValue;
                        }
                    }
                }
                if (values.Length > 4 && string.IsNullOrWhiteSpace(values[4]) == false)
                {
                    if (TryParseDecimal(values[4], out decimal parsedValue))
                    {
                        if (parsedValue > 0)
                        {
                            LineStep = parsedValue;
                        }
                    }
                }
                if (values.Length > 5 && string.IsNullOrWhiteSpace(values[5]) == false)
                {
                    if (TryParseDecimal(values[5], out decimal parsedValue))
                    {
                        if (parsedValue > 0)
                        {
                            StepMultiplicator = parsedValue;
                        }
                    }
                }
                if (values.Length > 6 && string.IsNullOrWhiteSpace(values[6]) == false)
                {
                    if (TryParseEnumFlexible(values[6], out TradeGridValueType parsedValue))
                    {
                        if (parsedValue == TradeGridValueType.Absolute
                            || parsedValue == TradeGridValueType.Percent)
                        {
                            TypeProfit = parsedValue;
                        }
                    }
                }
                if (values.Length > 7 && string.IsNullOrWhiteSpace(values[7]) == false)
                {
                    if (TryParseDecimal(values[7], out decimal parsedValue))
                    {
                        if (parsedValue >= 0)
                        {
                            ProfitStep = parsedValue;
                        }
                    }
                }
                if (values.Length > 8 && string.IsNullOrWhiteSpace(values[8]) == false)
                {
                    if (TryParseDecimal(values[8], out decimal parsedValue))
                    {
                        if (parsedValue > 0)
                        {
                            ProfitMultiplicator = parsedValue;
                        }
                    }
                }
                if (values.Length > 9 && string.IsNullOrWhiteSpace(values[9]) == false)
                {
                    if (TryParseEnumFlexible(values[9], out TradeGridVolumeType parsedValue))
                    {
                        if (parsedValue == TradeGridVolumeType.Contracts
                            || parsedValue == TradeGridVolumeType.ContractCurrency
                            || parsedValue == TradeGridVolumeType.DepositPercent)
                        {
                            TypeVolume = parsedValue;
                        }
                    }
                }
                if (values.Length > 10 && string.IsNullOrWhiteSpace(values[10]) == false)
                {
                    if (TryParseDecimal(values[10], out decimal parsedValue))
                    {
                        if (parsedValue >= 0)
                        {
                            StartVolume = parsedValue;
                        }
                    }
                }
                if (values.Length > 11 && string.IsNullOrWhiteSpace(values[11]) == false)
                {
                    if (TryParseDecimal(values[11], out decimal parsedValue))
                    {
                        if (parsedValue > 0)
                        {
                            MartingaleMultiplicator = parsedValue;
                        }
                    }
                }
                if (values.Length > 12 && string.IsNullOrWhiteSpace(values[12]) == false)
                {
                    string tradeAsset = values[12].Trim();
                    if (string.IsNullOrWhiteSpace(tradeAsset) == false)
                    {
                        TradeAssetInPortfolio = tradeAsset;
                    }
                }
                if (values.Length > 13 && string.IsNullOrWhiteSpace(values[13]) == false)
                {
                    LoadLines(values[13]);
                }

            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(),LogMessageType.Error);
            }
        }

        #endregion

        #region Grid lines creation and storage

        public List<TradeGridLine> Lines = new List<TradeGridLine>();

        public void CreateNewGrid(BotTabSimple tab, TradeGridPrimeType gridType)
        {
            if (tab == null)
            {
                return;
            }

            CreateMarketMakingGrid(tab);
        }

        public void DeleteGrid()
        {
            if (Lines == null)
            {
                return;
            }

            if(Lines.Count > 0)
            {
                Lines.Clear();
            }
        }

        public void CreateNewLine()
        {
            if (Lines == null)
            {
                Lines = new List<TradeGridLine>();
            }

            TradeGridLine newLine = new TradeGridLine();
            newLine.PriceEnter = 0;
            newLine.Side = GridSide;
            newLine.Volume = 0;
            Lines.Add(newLine);

        }

        public void RemoveSelected(List<int> numbers)
        {
            if (numbers == null || numbers.Count == 0)
            {
                return;
            }

            if (Lines == null || Lines.Count == 0)
            {
                return;
            }

            for(int i = numbers.Count-1; i > -1; i--)
            {
                int curNumber = numbers[i];

                if (curNumber < 0 || curNumber >= Lines.Count)
                {
                    continue;
                }

                TradeGridLine line = Lines[curNumber];

                if(line != null && line.Position != null)
                {
                    SendNewLogMessage("User remove line with Position!!! \n !!!!! \n !!!!!! \n Grid is broken!!!", LogMessageType.Error);
                }

                Lines.RemoveAt(curNumber);
            }
        }

        private void CreateMarketMakingGrid(BotTabSimple tab)
        {
            if (tab == null)
            {
                return;
            }

            if (Lines == null)
            {
                Lines = new List<TradeGridLine>();
            }

            Lines.Clear();

            decimal priceCurrent = FirstPrice;

            decimal volumeCurrent = StartVolume;

            decimal curStep = LineStep;

            decimal profitStep = ProfitStep;

            if (TypeStep == TradeGridValueType.Percent)
            {
                curStep = priceCurrent * (curStep / 100);

                if (tab.Security != null)
                {
                    curStep = Math.Round(curStep, tab.Security.Decimals);
                }
            }
            else if (TypeStep == TradeGridValueType.Absolute)
            {
                curStep = LineStep;

                if (tab.Security != null)
                {
                    curStep = Math.Round(curStep, tab.Security.Decimals);
                }
            }

            if (curStep <= 0)
            {
                return;
            }

            for (int i = 0; i < LineCountStart; i++)
            {
                /*if (FirstPrice > 0 
                    && curStep > FirstPrice*10)
                {
                    break;
                }*/

                if (priceCurrent <= 0)
                {
                    break;
                }

                /*if (priceCurrent / FirstPrice > 3)
                {
                    break;
                }*/

                TradeGridLine newLine = new TradeGridLine();
                newLine.PriceEnter = priceCurrent;

                if (tab.Security != null)
                {
                    newLine.PriceEnter = tab.RoundPrice(newLine.PriceEnter, tab.Security, GridSide);
                }

                newLine.Side = GridSide;
                newLine.Volume = volumeCurrent;

                if (tab.StartProgram == StartProgram.IsOsTrader
                   && tab.Security != null
                   && tab.Security.DecimalsVolume >= 0
                   && TypeVolume == TradeGridVolumeType.Contracts)
                {
                    newLine.Volume = Math.Round(volumeCurrent, tab.Security.DecimalsVolume);
                }
                else
                {
                    newLine.Volume = Math.Round(volumeCurrent, 5);
                }

                if (newLine.Volume <= 0)
                {
                    break;
                }

                Lines.Add(newLine);

                if (GridSide == Side.Buy)
                {
                    if (TypeProfit == TradeGridValueType.Percent)
                    {
                        newLine.PriceExit = newLine.PriceEnter + Math.Abs(newLine.PriceEnter * profitStep / 100);
                    }
                    else if (TypeProfit == TradeGridValueType.Absolute)
                    {
                        newLine.PriceExit = newLine.PriceEnter + profitStep;
                    }

                    if (tab.Security != null)
                    {
                        newLine.PriceExit = tab.RoundPrice(newLine.PriceExit, tab.Security, Side.Sell);
                    }

                    priceCurrent -= curStep;

                }
                else if (GridSide == Side.Sell)
                {
                    if (TypeProfit == TradeGridValueType.Percent)
                    {
                        newLine.PriceExit = newLine.PriceEnter - Math.Abs(newLine.PriceEnter * profitStep / 100);
                    }
                    else if (TypeProfit == TradeGridValueType.Absolute)
                    {
                        newLine.PriceExit = newLine.PriceEnter - profitStep;
                    }

                    if (tab.Security != null)
                    {
                        newLine.PriceExit = tab.RoundPrice(newLine.PriceExit, tab.Security, Side.Buy);
                    }

                    priceCurrent += curStep;
                }

                if (StepMultiplicator != 1
                    && StepMultiplicator != 0)
                {
                    curStep = curStep * StepMultiplicator;

                    if (curStep <= 0)
                    {
                        break;
                    }
                }

                if (ProfitMultiplicator != 1
                    && ProfitMultiplicator != 0)
                {
                    profitStep = profitStep * ProfitMultiplicator;

                    if (profitStep <= 0)
                    {
                        break;
                    }
                }

                if (MartingaleMultiplicator != 0
                    && MartingaleMultiplicator != 1)
                {
                    volumeCurrent = volumeCurrent * MartingaleMultiplicator;

                    if (volumeCurrent <= 0)
                    {
                        break;
                    }
                }
            }
        }

        public string GetSaveLinesString()
        {
            try
            {
                if (Lines == null || Lines.Count == 0)
                {
                    return "";
                }

                string lines = "";

                for (int i = 0; i < Lines.Count; i++)
                {
                    TradeGridLine line = Lines[i];
                    if (line == null)
                    {
                        continue;
                    }

                    lines += line.GetSaveStr() + "^";
                }
                 
                return lines;
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
                return "";
            }
        }

        public void LoadLines(string str)
        {
            if(string.IsNullOrEmpty(str))
            {
                return;
            }

            try
            {
                if (Lines == null)
                {
                    Lines = new List<TradeGridLine>();
                }

                string[] linesInStr = str.Split('^');

                for(int i = 0;i < linesInStr.Length;i++)
                {
                    string line = linesInStr[i];
                    if (line != null)
                    {
                        line = line.Trim();
                    }

                    if(string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    try
                    {
                        TradeGridLine newLine = new TradeGridLine();
                        if (newLine.SetFromStr(line))
                        {
                            Lines.Add(newLine);
                        }
                    }
                    catch (Exception e)
                    {
                        SendNewLogMessage(e.ToString(), LogMessageType.Error);
                    }
                }
            }
            catch (Exception e)
            {
               SendNewLogMessage(e.ToString(),LogMessageType.Error);
            }
        }

        public decimal GetVolume(TradeGridLine line, BotTabSimple tab)
        {
            if (line == null)
            {
                return 0;
            }

            if (TypeVolume == TradeGridVolumeType.Contracts) // кол-во контрактов
            {
                return line.Volume;
            }

            if (tab == null)
            {
                return 0;
            }

            Security security = tab.Security;
            if (security == null)
            {
                return 0;
            }

            decimal volume = 0;
            decimal volumeFromLine = line.Volume;
            decimal priceEnterForLine = line.PriceEnter;

            if (TypeVolume == TradeGridVolumeType.ContractCurrency) // "Валюта контракта"
            {
                decimal contractPrice = priceEnterForLine;
                if (contractPrice == 0)
                {
                    return 0;
                }

                if(tab.StartProgram == StartProgram.IsOsTrader)
                {
                    int decimalsVolume = GetSafeDecimalsVolume(security);
                    if(security.Lot != 0)
                    {
                        volume = Math.Round(volumeFromLine / contractPrice / security.Lot, decimalsVolume);
                    }
                    else
                    {
                        volume = Math.Round(volumeFromLine / contractPrice, decimalsVolume);
                    }
                }
                else
                {
                    if (security.Lot != 0)
                    {
                        volume = Math.Round(volumeFromLine / contractPrice / security.Lot, 7);
                    }
                    else
                    {
                        volume = Math.Round(volumeFromLine / contractPrice, 7);
                    }
                }

                return volume;
            }
            else // if (TypeVolume == Type_Volume.DepoPercent) // процент депозита
            {
                Portfolio myPortfolio = tab.Portfolio;

                if (myPortfolio == null)
                {
                    return 0;
                }

                decimal portfolioPrimeAsset = 0;

                if (TradeAssetInPortfolio == "Prime")
                {
                    portfolioPrimeAsset = myPortfolio.ValueCurrent;
                }
                else
                {
                    List<PositionOnBoard> positionOnBoard = myPortfolio.GetPositionOnBoard();

                    if (positionOnBoard == null)
                    {
                        return 0;
                    }

                    for (int i = 0; i < positionOnBoard.Count; i++)
                    {
                        PositionOnBoard currentPosition = positionOnBoard[i];
                        if (currentPosition == null)
                        {
                            continue;
                        }

                        if (currentPosition.SecurityNameCode == TradeAssetInPortfolio)
                        {
                            portfolioPrimeAsset = currentPosition.ValueCurrent;
                            break;
                        }
                    }
                }

                if (portfolioPrimeAsset == 0
                    || portfolioPrimeAsset == 1)
                {
                    SendNewLogMessage("Can`t found portfolio in Deposit Percent volume mode " + TradeAssetInPortfolio, OsEngine.Logging.LogMessageType.System);
                    return 0;
                }
                decimal moneyOnPosition = portfolioPrimeAsset * (volumeFromLine / 100);
                if (tab.PriceBestAsk == 0)
                {
                    return 0;
                }

                decimal qty = 0;
                if (security.Lot != 0)
                {
                    qty = moneyOnPosition / tab.PriceBestAsk / security.Lot;
                }
                else
                {
                    qty = moneyOnPosition / tab.PriceBestAsk;
                }

                if (tab.StartProgram == StartProgram.IsOsTrader)
                {
                    int decimalsVolume = GetSafeDecimalsVolume(security);
                    if (security.UsePriceStepCostToCalculateVolume == true
                      && security.PriceStep != security.PriceStepCost
                      && tab.PriceBestAsk != 0
                      && security.PriceStep != 0
                      && security.PriceStepCost != 0)
                    {// расчёт количества контрактов для фьючерсов и опционов на Мосбирже
                        qty = moneyOnPosition / (tab.PriceBestAsk / security.PriceStep * security.PriceStepCost);
                    }
                    qty = Math.Round(qty, decimalsVolume);
                }
                else
                {
                    qty = Math.Round(qty, 7);
                }

                return qty;
            }
        }

        private int GetSafeDecimalsVolume(Security security)
        {
            if (security == null)
            {
                return 7;
            }

            if (security.DecimalsVolume < 0 || security.DecimalsVolume > 28)
            {
                return 7;
            }

            return security.DecimalsVolume;
        }

        private bool TryParseDecimal(string value, out decimal parsedValue)
        {
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedValue))
            {
                return true;
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out parsedValue))
            {
                return true;
            }

            if (decimal.TryParse(value, NumberStyles.Any, RuCulture, out parsedValue))
            {
                return true;
            }

            parsedValue = 0;
            return false;
        }

        private bool TryParseEnumFlexible<TEnum>(string value, out TEnum parsedValue)
            where TEnum : struct
        {
            return Enum.TryParse(value, true, out parsedValue);
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

    public class TradeGridLine
    {
        private static readonly CultureInfo RuCulture = CultureInfo.GetCultureInfo("ru-RU");

        public decimal PriceEnter;

        public decimal PriceExit;

        public bool CanReplaceExitOrder;

        public decimal Volume;

        public Side Side;

        public int PositionNum = -1;

        public Position Position;

        public string GetSaveStr()
        {
            string result = "";

            result += PriceEnter.ToString(CultureInfo.InvariantCulture) + "|";
            result += Volume.ToString(CultureInfo.InvariantCulture) + "|";
            result += Side + "|";
            result += PriceExit.ToString(CultureInfo.InvariantCulture) + "|";
            result += PositionNum.ToString(CultureInfo.InvariantCulture) + "|";

            return result;
        }

        public bool SetFromStr(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return false;
            }

            string[] saveArray = str.Split('|');

            if (saveArray.Length < 4)
            {
                return false;
            }

            string priceEnterRaw = saveArray[0]?.Trim();
            string volumeRaw = saveArray[1]?.Trim();
            string sideRaw = saveArray[2]?.Trim();
            string priceExitRaw = saveArray[3]?.Trim();

            if (TryParseDecimal(priceEnterRaw, out decimal priceEnter) == false)
            {
                return false;
            }
            if (TryParseDecimal(volumeRaw, out decimal volume) == false)
            {
                return false;
            }
            if (Enum.TryParse(sideRaw, true, out Side side) == false)
            {
                return false;
            }
            if (side != Side.Buy && side != Side.Sell)
            {
                return false;
            }
            if (TryParseDecimal(priceExitRaw, out decimal priceExit) == false)
            {
                return false;
            }

            if (priceEnter < 0 || volume < 0 || priceExit < 0)
            {
                return false;
            }

            PriceEnter = priceEnter;
            Volume = volume;
            Side = side;
            PriceExit = priceExit;

            if (saveArray.Length > 4
                && string.IsNullOrWhiteSpace(saveArray[4]) == false
                && TryParseInt(saveArray[4].Trim(), out int positionNum))
            {
                if (positionNum >= -1)
                {
                    PositionNum = positionNum;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryParseDecimal(string value, out decimal parsed)
        {
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed))
            {
                return true;
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out parsed))
            {
                return true;
            }

            if (decimal.TryParse(value, NumberStyles.Any, RuCulture, out parsed))
            {
                return true;
            }

            parsed = 0;
            return false;
        }

        private static bool TryParseInt(string value, out int parsed)
        {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            {
                return true;
            }

            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out parsed))
            {
                return true;
            }

            if (int.TryParse(value, NumberStyles.Integer, RuCulture, out parsed))
            {
                return true;
            }

            parsed = 0;
            return false;
        }

    }
}

