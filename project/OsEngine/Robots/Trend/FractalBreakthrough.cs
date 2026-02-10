/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using OsEngine.Entity;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Attributes;
using OsEngine.OsTrader.Panels.Tab;

/* Description
trading robot for osengine

Fractal Breakthrough - breakout strategy based on fractal levels.

Upper Fractal: 5 bars, center High is higher than 2 Highs on each side.
Lower Fractal: 5 bars, center Low is lower than 2 Lows on each side.

Buy:
Lower fractal formed, non-cancelled upper fractal exists, EMA1 and EMA2 rising.
Pending stop-limit order: activation = upper fractal + 1 tick, order price = upper fractal - 1 tick.

Sell:
Upper fractal formed, non-cancelled lower fractal exists, EMA1 and EMA2 falling.
Pending stop-limit order: activation = lower fractal - 1 tick, order price = lower fractal + 1 tick.

Exit:
Stop-loss: for long = last lower fractal - 2 ticks, for short = last upper fractal + 2 ticks.
Take-profit = kTake * stop-loss distance.

Filters:
- Take-profit must be >= Close * kComiss * Comiss
- Stop distance must be >= kATR * ATR and <= k2ATR * ATR
- Fractal is cancelled if price crosses it
*/

namespace OsEngine.Robots.Trend
{
    [Bot("FractalBreakthrough")]
    public class FractalBreakthrough : BotPanel
    {
        private BotTabSimple _tab;

        // Basic Settings
        private StrategyParameterString _regime;

        // Strategy Settings
        private StrategyParameterDecimal _kTake;
        private StrategyParameterDecimal _comiss;
        private StrategyParameterDecimal _kComiss;
        private StrategyParameterInt _atrLength;
        private StrategyParameterDecimal _kATR;
        private StrategyParameterDecimal _k2ATR;
        private StrategyParameterInt _ema1Length;
        private StrategyParameterInt _ema2Length;

        // GetVolume Settings
        private StrategyParameterString _volumeType;
        private StrategyParameterDecimal _volume;
        private StrategyParameterString _tradeAssetInPortfolio;

        // Internal indicator values
        private decimal _currentATR = 0;
        private decimal _currentEMA1 = 0;
        private decimal _currentEMA2 = 0;
        private decimal _previousEMA1 = 0;
        private decimal _previousEMA2 = 0;

        // Fractal state
        private decimal _lastUpperFractal = 0;
        private decimal _lastLowerFractal = 0;

        public FractalBreakthrough(string name, StartProgram startProgram) : base(name, startProgram)
        {
            // 1. Create tab
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];

            // 2. Basic settings
            _regime = CreateParameter("Regime", "Off", new[] { "Off", "On", "OnlyLong", "OnlyShort" }, "Base");

            // 3. Strategy settings
            _kTake = CreateParameter("kTake", 1.5m, 0.5m, 5.0m, 0.1m, "Base");
            _comiss = CreateParameter("Comiss", 0.1m, 0.01m, 1.0m, 0.01m, "Base");
            _kComiss = CreateParameter("kComiss", 3.0m, 1.0m, 10.0m, 0.5m, "Base");
            _atrLength = CreateParameter("ATR Length", 14, 5, 50, 1, "Indicator");
            _kATR = CreateParameter("kATR", 1.0m, 0.1m, 5.0m, 0.1m, "Filter");
            _k2ATR = CreateParameter("k2ATR", 3.0m, 1.0m, 10.0m, 0.5m, "Filter");
            _ema1Length = CreateParameter("Ema1 Length", 20, 5, 200, 1, "Indicator");
            _ema2Length = CreateParameter("Ema2 Length", 50, 5, 200, 1, "Indicator");

            // 4. Volume settings
            _volumeType = CreateParameter("Volume type", "Deposit percent", new[] { "Contracts", "Contract currency", "Deposit percent" });
            _volume = CreateParameter("Volume", 20, 1.0m, 50, 4);
            _tradeAssetInPortfolio = CreateParameter("Asset in portfolio", "Prime");

            // 5. Subscribe to events
            _tab.CandleFinishedEvent += _tab_CandleFinishedEvent;

            // 7. Description
            Description = "Fractal Breakthrough - breakout strategy based on fractal levels with EMA trend filter and ATR volatility filter.";
        }

        public override string GetNameStrategyType()
        {
            return "FractalBreakthrough";
        }

        public override void ShowIndividualSettingsDialog()
        {
        }

        private void _tab_CandleFinishedEvent(List<Candle> candles)
        {
            if (_regime.ValueString == "Off")
            {
                return;
            }

            // Need at least 5 candles for fractal + enough for indicators
            int minBars = Math.Max(5, Math.Max(_ema1Length.ValueInt, Math.Max(_ema2Length.ValueInt, _atrLength.ValueInt)));

            if (candles.Count < minBars + 1)
            {
                return;
            }

            // Calculate indicators
            CalculateATR(candles);

            // Save previous EMA values before updating
            _previousEMA1 = _currentEMA1;
            _previousEMA2 = _currentEMA2;

            _currentEMA1 = CalculateEMA(candles, _ema1Length.ValueInt, _currentEMA1);
            _currentEMA2 = CalculateEMA(candles, _ema2Length.ValueInt, _currentEMA2);

            if (_currentATR == 0 || _currentEMA1 == 0 || _currentEMA2 == 0)
            {
                return;
            }

            int last = candles.Count - 1;
            decimal lastClose = candles[last].Close;
            decimal tick = _tab.Securiti.PriceStep;

            // 1. Detect new fractals at index [count - 3]
            int fi = candles.Count - 3;

            // Upper fractal: center High > both neighbors' Highs
            if (candles[fi].High > candles[fi - 2].High &&
                candles[fi].High > candles[fi - 1].High &&
                candles[fi].High > candles[fi + 1].High &&
                candles[fi].High > candles[fi + 2].High)
            {
                _lastUpperFractal = candles[fi].High;
            }

            // Lower fractal: center Low < both neighbors' Lows
            if (candles[fi].Low < candles[fi - 2].Low &&
                candles[fi].Low < candles[fi - 1].Low &&
                candles[fi].Low < candles[fi + 1].Low &&
                candles[fi].Low < candles[fi + 2].Low)
            {
                _lastLowerFractal = candles[fi].Low;
            }

            // 2. Fractal invalidation: cancel if price crossed fractal level
            if (_lastUpperFractal != 0 && lastClose > _lastUpperFractal)
            {
                _lastUpperFractal = 0;
            }

            if (_lastLowerFractal != 0 && lastClose < _lastLowerFractal)
            {
                _lastLowerFractal = 0;
            }

            // 3. Handle open positions - set stop and take if not yet set
            List<Position> openPositions = _tab.PositionsOpenAll;

            if (openPositions != null && openPositions.Count != 0)
            {
                for (int i = 0; i < openPositions.Count; i++)
                {
                    Position pos = openPositions[i];

                    if (pos.State != PositionStateType.Open)
                    {
                        continue;
                    }

                    if (pos.StopOrderIsActive == false)
                    {
                        SetStopAndTake(pos, tick);
                    }
                }

                return;
            }

            // 4. No open positions - check if we can place pending orders
            // Need both fractals to exist
            if (_lastUpperFractal == 0 || _lastLowerFractal == 0)
            {
                return;
            }

            // 5. Get indicator values for filters
            decimal atrValue = _currentATR;
            decimal ema1Last = _currentEMA1;
            decimal ema1Prev = _previousEMA1;
            decimal ema2Last = _currentEMA2;
            decimal ema2Prev = _previousEMA2;

            // 6. Try to place Long pending order
            if (_regime.ValueString != "OnlyShort")
            {
                // EMA filter: both EMAs rising
                if (ema1Last > ema1Prev && ema2Last > ema2Prev)
                {
                    decimal stopPrice = _lastLowerFractal - 2 * tick;
                    decimal entryApprox = _lastUpperFractal;
                    decimal stopDistance = entryApprox - stopPrice;
                    decimal takeDistance = _kTake.ValueDecimal * stopDistance;

                    // Commission filter: take must cover commission
                    if (takeDistance >= lastClose * _kComiss.ValueDecimal * _comiss.ValueDecimal)
                    {
                        // ATR filter: stop distance within acceptable range
                        if (stopDistance >= _kATR.ValueDecimal * atrValue &&
                            stopDistance <= _k2ATR.ValueDecimal * atrValue)
                        {
                            decimal activationPrice = _lastUpperFractal + tick;
                            decimal orderPrice = _lastUpperFractal - tick;

                            _tab.BuyAtStop(
                                GetVolume(_tab),
                                orderPrice,
                                activationPrice,
                                StopActivateType.HigherOrEqual);
                        }
                    }
                }
            }

            // 7. Try to place Short pending order
            if (_regime.ValueString != "OnlyLong")
            {
                // EMA filter: both EMAs falling
                if (ema1Last < ema1Prev && ema2Last < ema2Prev)
                {
                    decimal stopPrice = _lastUpperFractal + 2 * tick;
                    decimal entryApprox = _lastLowerFractal;
                    decimal stopDistance = stopPrice - entryApprox;
                    decimal takeDistance = _kTake.ValueDecimal * stopDistance;

                    // Commission filter: take must cover commission
                    if (takeDistance >= lastClose * _kComiss.ValueDecimal * _comiss.ValueDecimal)
                    {
                        // ATR filter: stop distance within acceptable range
                        if (stopDistance >= _kATR.ValueDecimal * atrValue &&
                            stopDistance <= _k2ATR.ValueDecimal * atrValue)
                        {
                            decimal activationPrice = _lastLowerFractal - tick;
                            decimal orderPrice = _lastLowerFractal + tick;

                            _tab.SellAtStop(
                                GetVolume(_tab),
                                orderPrice,
                                activationPrice,
                                StopActivateType.LowerOrEqual);
                        }
                    }
                }
            }
        }

        private void SetStopAndTake(Position position, decimal tick)
        {
            decimal entryPrice = position.EntryPrice;

            if (position.Direction == Side.Buy)
            {
                if (_lastLowerFractal == 0)
                {
                    return;
                }

                decimal stopActivation = _lastLowerFractal - 2 * tick;
                decimal stopOrder = _lastLowerFractal - 3 * tick;
                decimal stopDistance = entryPrice - stopActivation;
                decimal takeDistance = _kTake.ValueDecimal * stopDistance;
                decimal takeActivation = entryPrice + takeDistance;
                decimal takeOrder = entryPrice + takeDistance - tick;

                _tab.CloseAtStop(position, stopActivation, stopOrder);
                _tab.CloseAtProfit(position, takeActivation, takeOrder);
            }
            else
            {
                if (_lastUpperFractal == 0)
                {
                    return;
                }

                decimal stopActivation = _lastUpperFractal + 2 * tick;
                decimal stopOrder = _lastUpperFractal + 3 * tick;
                decimal stopDistance = stopActivation - entryPrice;
                decimal takeDistance = _kTake.ValueDecimal * stopDistance;
                decimal takeActivation = entryPrice - takeDistance;
                decimal takeOrder = entryPrice - takeDistance + tick;

                _tab.CloseAtStop(position, stopActivation, stopOrder);
                _tab.CloseAtProfit(position, takeActivation, takeOrder);
            }
        }

        /// <summary>
        /// Calculate ATR (Average True Range)
        /// ATR = SMA(TR) for first period, then smoothed: ATR = (ATR_prev * (n-1) + TR) / n
        /// </summary>
        private void CalculateATR(List<Candle> candles)
        {
            int period = _atrLength.ValueInt;
            int last = candles.Count - 1;

            if (candles.Count < period + 1)
            {
                _currentATR = 0;
                return;
            }

            // True Range = Max(High-Low, |High-PrevClose|, |Low-PrevClose|)
            decimal high = candles[last].High;
            decimal low = candles[last].Low;
            decimal prevClose = candles[last - 1].Close;

            decimal tr = Math.Max(high - low, Math.Max(Math.Abs(high - prevClose), Math.Abs(low - prevClose)));

            if (_currentATR == 0)
            {
                // Initialize ATR as SMA of TR for the first period
                decimal sumTR = 0;
                for (int i = last - period + 1; i <= last; i++)
                {
                    decimal h = candles[i].High;
                    decimal l = candles[i].Low;
                    decimal pc = candles[i - 1].Close;
                    decimal trValue = Math.Max(h - l, Math.Max(Math.Abs(h - pc), Math.Abs(l - pc)));
                    sumTR += trValue;
                }
                _currentATR = sumTR / period;
            }
            else
            {
                // Smooth ATR: ATR = (ATR_prev * (n-1) + TR) / n
                _currentATR = (_currentATR * (period - 1) + tr) / period;
            }
        }

        /// <summary>
        /// Calculate EMA (Exponential Moving Average)
        /// EMA = (Close * multiplier) + (EMA_prev * (1 - multiplier))
        /// where multiplier = 2 / (period + 1)
        /// </summary>
        private decimal CalculateEMA(List<Candle> candles, int period, decimal currentEMA)
        {
            int last = candles.Count - 1;

            if (candles.Count < period)
            {
                return 0;
            }

            decimal close = candles[last].Close;
            decimal multiplier = 2m / (period + 1);

            if (currentEMA == 0)
            {
                // Initialize EMA as SMA for the first period
                decimal sum = 0;
                for (int i = last - period + 1; i <= last; i++)
                {
                    sum += candles[i].Close;
                }
                return sum / period;
            }
            else
            {
                // Calculate EMA
                return (close * multiplier) + (currentEMA * (1 - multiplier));
            }
        }

        private decimal GetVolume(BotTabSimple tab)
        {
            decimal volume = 0;

            if (_volumeType.ValueString == "Contracts")
            {
                volume = _volume.ValueDecimal;
            }
            else if (_volumeType.ValueString == "Contract currency")
            {
                decimal contractPrice = tab.PriceBestAsk;
                volume = _volume.ValueDecimal / contractPrice;

                if (StartProgram == StartProgram.IsOsTrader)
                {
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
                else
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

                decimal qty = moneyOnPosition / tab.PriceBestAsk / tab.Security.Lot;

                if (tab.StartProgram == StartProgram.IsOsTrader)
                {
                    if (tab.Security.UsePriceStepCostToCalculateVolume == true
                       && tab.Security.PriceStep != tab.Security.PriceStepCost
                       && tab.PriceBestAsk != 0
                       && tab.Security.PriceStep != 0
                       && tab.Security.PriceStepCost != 0)
                    {
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
