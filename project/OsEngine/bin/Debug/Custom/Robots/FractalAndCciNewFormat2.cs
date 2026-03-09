/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Logging;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Attributes;
using OsEngine.OsTrader.Panels.Tab;

/* Description
trading robot for osengine

The trend robot on Fractal And CCI without indicators.
Signal calculations are manual (approach 2) to avoid indicator object lifecycle overhead
and reduce extra per-candle allocations in hot path.

Buy:
1. Formed fractal at a local minimum.
2. CCI crosses additional -300 level from below.

Sell:
1. Formed fractal at a local maximum.
2. CCI crosses additional 300 level from above.

Exit from buy: trailing stop in % of the Low of current candle.
Exit from sell: trailing stop in % of the High of current candle.
 */

namespace OsEngine.Robots
{
    [Bot("FractalAndCciNewFormat2")]
    public class FractalAndCciNewFormat2 : BotPanel
    {
        private readonly BotTabSimple _tab;
        private bool _isDeleted;

        // Basic Settings
        private readonly StrategyParameterString _regime;
        private readonly StrategyParameterDecimal _slippage;
        private readonly StrategyParameterTimeOfDay _startTradeTime;
        private readonly StrategyParameterTimeOfDay _endTradeTime;

        // GetVolume Settings
        private readonly StrategyParameterString _volumeType;
        private readonly StrategyParameterDecimal _volume;
        private readonly StrategyParameterString _tradeAssetInPortfolio;

        // Signal Settings
        private readonly StrategyParameterInt _lengthCci;

        // Exit Settings
        private readonly StrategyParameterInt _trailingValueLong;
        private readonly StrategyParameterInt _trailingValueShort;

        public FractalAndCciNewFormat2(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];

            // Basic Settings
            _regime = CreateParameter("Regime", "Off", new[] { "Off", "On", "OnlyLong", "OnlyShort", "OnlyClosePosition" }, "Base");
            _slippage = CreateParameter("Slippage %", 0m, 0, 20, 1, "Base");
            _startTradeTime = CreateParameterTimeOfDay("Start Trade Time", 0, 0, 0, 0, "Base");
            _endTradeTime = CreateParameterTimeOfDay("End Trade Time", 24, 0, 0, 0, "Base");

            // GetVolume Settings
            _volumeType = CreateParameter("Volume type", "Deposit percent", new[] { "Contracts", "Contract currency", "Deposit percent" });
            _volume = CreateParameter("Volume", 20, 1.0m, 50, 4);
            _tradeAssetInPortfolio = CreateParameter("Asset in portfolio", "Prime");

            // Signal Settings
            _lengthCci = CreateParameter("CCI Length", 21, 7, 48, 7, "Indicator");

            // Exit Settings
            _trailingValueLong = CreateParameter("Long Exit", 5, 5, 200, 5, "Exit");
            _trailingValueShort = CreateParameter("Short Exit", 5, 5, 200, 5, "Exit");

            _tab.CandleFinishedEvent += OnCandleFinished;
            DeleteEvent += OnDelete;

            Description = OsLocalization.Description.DescriptionLabel317;
        }

        public override string GetNameStrategyType()
        {
            return "FractalAndCciNewFormat2";
        }

        public override void ShowIndividualSettingsDialog()
        {
        }

        private void OnDelete()
        {
            if (_isDeleted)
            {
                return;
            }

            _isDeleted = true;

            try
            {
                _tab.CandleFinishedEvent -= OnCandleFinished;
            }
            catch
            {
                // Tab may already be disposed in panel delete sequence.
            }

            DeleteEvent -= OnDelete;
        }

        private void OnCandleFinished(List<Candle> candles)
        {
            if (_isDeleted)
            {
                return;
            }

            if (_regime.ValueString == "Off")
            {
                return;
            }

            int minCandles = Math.Max(_lengthCci.ValueInt + 2, 5);
            if (candles == null || candles.Count < minCandles)
            {
                return;
            }

            if (_startTradeTime.Value > _tab.TimeServerCurrent || _endTradeTime.Value < _tab.TimeServerCurrent)
            {
                return;
            }

            List<Position> openPositions = _tab.PositionsOpenAll;

            if (openPositions != null && openPositions.Count > 0)
            {
                LogicClosePosition(candles, openPositions);
            }

            if (_regime.ValueString == "OnlyClosePosition")
            {
                return;
            }

            if (openPositions == null || openPositions.Count == 0)
            {
                LogicOpenPosition(candles);
            }
        }

        private void LogicOpenPosition(List<Candle> candles)
        {
            int cciLastIndex = candles.Count - 2;
            int cciPrevIndex = candles.Count - 3;
            int signalIndex = candles.Count - 3;

            decimal lastCci = CalculateCci(candles, cciLastIndex, _lengthCci.ValueInt);
            decimal prevCci = CalculateCci(candles, cciPrevIndex, _lengthCci.ValueInt);

            decimal lastUpFractal = GetLastUpFractal(candles, signalIndex);
            decimal lastDownFractal = GetLastDownFractal(candles, signalIndex);

            decimal lastPrice = candles[signalIndex].Close;
            decimal slippage = _slippage.ValueDecimal * _tab.Security.PriceStep;
            decimal volume = GetVolume(_tab);
            decimal bestAsk = _tab.PriceBestAsk;
            decimal bestBid = _tab.PriceBestBid;

            if (volume <= 0 || bestAsk <= 0 || bestBid <= 0)
            {
                return;
            }

            if (_regime.ValueString != "OnlyShort"
                && lastDownFractal != 0
                && lastDownFractal < lastPrice
                && prevCci < -300
                && lastCci > -300)
            {
                _tab.BuyAtLimit(volume, _tab.PriceBestAsk + slippage);
            }

            if (_regime.ValueString != "OnlyLong"
                && lastUpFractal != 0
                && lastUpFractal > lastPrice
                && prevCci > 300
                && lastCci < 300)
            {
                _tab.SellAtLimit(volume, _tab.PriceBestBid - slippage);
            }
        }

        private void LogicClosePosition(List<Candle> candles, List<Position> openPositions)
        {
            decimal candleLow = candles[candles.Count - 1].Low;
            decimal candleHigh = candles[candles.Count - 1].High;

            for (int i = 0; i < openPositions.Count; i++)
            {
                Position position = openPositions[i];

                if (position.State != PositionStateType.Open)
                {
                    continue;
                }

                decimal stopPrice;

                if (position.Direction == Side.Buy)
                {
                    stopPrice = candleLow - candleLow * _trailingValueLong.ValueInt / 100m;
                }
                else
                {
                    stopPrice = candleHigh + candleHigh * _trailingValueShort.ValueInt / 100m;
                }

                _tab.CloseAtTrailingStop(position, stopPrice, stopPrice);
            }
        }

        private decimal CalculateCci(List<Candle> candles, int index, int period)
        {
            if (index < period - 1)
            {
                return 0;
            }

            int start = index - period + 1;
            decimal sumTypicalPrice = 0;

            for (int i = start; i <= index; i++)
            {
                sumTypicalPrice += GetTypicalPrice(candles[i]);
            }

            decimal sma = sumTypicalPrice / period;

            decimal meanDeviationSum = 0;
            for (int i = start; i <= index; i++)
            {
                decimal typicalPrice = GetTypicalPrice(candles[i]);
                meanDeviationSum += Math.Abs(typicalPrice - sma);
            }

            decimal meanDeviation = meanDeviationSum / period;

            if (meanDeviation == 0)
            {
                return 0;
            }

            decimal currentTypicalPrice = GetTypicalPrice(candles[index]);
            return (currentTypicalPrice - sma) / (0.015m * meanDeviation);
        }

        private decimal GetTypicalPrice(Candle candle)
        {
            return (candle.High + candle.Low + candle.Close) / 3m;
        }

        private decimal GetLastUpFractal(List<Candle> candles, int fromIndex)
        {
            for (int i = fromIndex; i >= 2; i--)
            {
                if (i + 2 >= candles.Count)
                {
                    continue;
                }

                decimal high = candles[i].High;

                if (high > candles[i - 1].High
                    && high > candles[i - 2].High
                    && high > candles[i + 1].High
                    && high > candles[i + 2].High)
                {
                    return high;
                }
            }

            return 0;
        }

        private decimal GetLastDownFractal(List<Candle> candles, int fromIndex)
        {
            for (int i = fromIndex; i >= 2; i--)
            {
                if (i + 2 >= candles.Count)
                {
                    continue;
                }

                decimal low = candles[i].Low;

                if (low < candles[i - 1].Low
                    && low < candles[i - 2].Low
                    && low < candles[i + 1].Low
                    && low < candles[i + 2].Low)
                {
                    return low;
                }
            }

            return 0;
        }

        // Method for calculating the volume of entry into a position
        private decimal GetVolume(BotTabSimple tab)
        {
            if (tab == null || tab.Security == null)
            {
                return 0;
            }

            decimal volume = 0;

            if (_volumeType.ValueString == "Contracts")
            {
                volume = _volume.ValueDecimal;
            }
            else if (_volumeType.ValueString == "Contract currency")
            {
                decimal contractPrice = tab.PriceBestAsk;

                if (contractPrice <= 0)
                {
                    return 0;
                }

                volume = _volume.ValueDecimal / contractPrice;

                if (StartProgram == StartProgram.IsOsTrader)
                {
                    if (tab.Connector == null)
                    {
                        return 0;
                    }

                    IServerPermission serverPermission = ServerMaster.GetServerPermission(tab.Connector.ServerType);

                    if (serverPermission != null &&
                        serverPermission.IsUseLotToCalculateProfit &&
                        tab.Security.Lot != 0 &&
                        tab.Security.Lot > 1)
                    {
                        volume = _volume.ValueDecimal / (contractPrice * tab.Security.Lot);
                    }

                    volume = Math.Round(volume, tab.Security.DecimalsVolume);
                }
                else // Tester or Optimizer
                {
                    volume = Math.Round(volume, 6);
                }
            }
            else if (_volumeType.ValueString == "Deposit percent")
            {
                Portfolio myPortfolio = tab.Portfolio;

                if (myPortfolio == null)
                {
                    return 0;
                }

                decimal portfolioPrimeAsset = 0;

                if (_tradeAssetInPortfolio.ValueString == "Prime")
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
                        if (positionOnBoard[i].SecurityNameCode == _tradeAssetInPortfolio.ValueString)
                        {
                            portfolioPrimeAsset = positionOnBoard[i].ValueCurrent;
                            break;
                        }
                    }
                }

                if (portfolioPrimeAsset == 0)
                {
                    SendNewLogMessage("Can`t found portfolio " + _tradeAssetInPortfolio.ValueString, Logging.LogMessageType.Error);
                    return 0;
                }

                decimal moneyOnPosition = portfolioPrimeAsset * (_volume.ValueDecimal / 100);

                if (tab.PriceBestAsk <= 0 || tab.Security.Lot <= 0)
                {
                    return 0;
                }

                decimal qty = moneyOnPosition / tab.PriceBestAsk / tab.Security.Lot;

                if (tab.StartProgram == StartProgram.IsOsTrader)
                {
                    if (tab.Security.UsePriceStepCostToCalculateVolume
                        && tab.Security.PriceStep != tab.Security.PriceStepCost
                        && tab.PriceBestAsk != 0
                        && tab.Security.PriceStep != 0
                        && tab.Security.PriceStepCost != 0)
                    {
                        // Contract quantity calculation for MOEX futures/options.
                        qty = moneyOnPosition / (tab.PriceBestAsk / tab.Security.PriceStep * tab.Security.PriceStepCost);
                    }

                    qty = Math.Round(qty, tab.Security.DecimalsVolume);
                }
                else
                {
                    qty = Math.Round(qty, 7);
                }

                return qty;
            }

            return volume;
        }
    }
}
