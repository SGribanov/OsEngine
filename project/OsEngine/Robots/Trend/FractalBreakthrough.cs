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
        #region Constants

        // Fractal detection constants
        private const int FractalOffset = 3;           // Fractal detection at candles.Count - 3
        private const int FractalLookback = 2;         // Look 2 candles on each side
        private const int MinFractalBars = 5;          // Minimum bars needed for fractal detection

        // Stop/Take offset constants
        private const int StopTickOffset = 2;          // Stop offset from fractal in ticks
        private const int StopOrderTickOffset = 3;     // Stop order additional offset
        private const int EntryTickOffset = 1;         // Entry offset from fractal in ticks

        // Regime constants
        private const string RegimeOff = "Off";
        private const string RegimeOn = "On";
        private const string RegimeOnlyLong = "OnlyLong";
        private const string RegimeOnlyShort = "OnlyShort";

        // Volume type constants
        private const string VolumeTypeContracts = "Contracts";
        private const string VolumeTypeContractCurrency = "Contract currency";
        private const string VolumeTypeDepositPercent = "Deposit percent";

        // Asset constants
        private const string AssetPrime = "Prime";

        // Rounding constants
        private const int DefaultVolumeDecimals = 6;
        private const int TesterVolumeDecimals = 7;

        #endregion

        #region Fields

        private readonly BotTabSimple _tab;

        // Basic Settings
        private readonly StrategyParameterString _regime;

        // Strategy Settings
        private readonly StrategyParameterDecimal _kTake;
        private readonly StrategyParameterDecimal _comiss;
        private readonly StrategyParameterDecimal _kComiss;
        private readonly StrategyParameterInt _atrLength;
        private readonly StrategyParameterDecimal _kATR;
        private readonly StrategyParameterDecimal _k2ATR;
        private readonly StrategyParameterInt _ema1Length;
        private readonly StrategyParameterInt _ema2Length;

        // GetVolume Settings
        private readonly StrategyParameterString _volumeType;
        private readonly StrategyParameterDecimal _volume;
        private readonly StrategyParameterString _tradeAssetInPortfolio;

        // Internal indicator values
        private decimal _currentATR;
        private decimal _currentEMA1;
        private decimal _currentEMA2;
        private decimal _previousEMA1;
        private decimal _previousEMA2;

        // Fractal state
        private decimal _lastUpperFractal;
        private decimal _lastLowerFractal;

        #endregion

        public FractalBreakthrough(string name, StartProgram startProgram) : base(name, startProgram)
        {
            // 1. Create tab
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];

            // 2. Basic settings
            _regime = CreateParameter("Regime", RegimeOff, [RegimeOff, RegimeOn, RegimeOnlyLong, RegimeOnlyShort], "Base");

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
            _volumeType = CreateParameter("Volume type", VolumeTypeDepositPercent,
                [VolumeTypeContracts, VolumeTypeContractCurrency, VolumeTypeDepositPercent]);
            _volume = CreateParameter("Volume", 20, 1.0m, 50, 4);
            _tradeAssetInPortfolio = CreateParameter("Asset in portfolio", AssetPrime);

            // 5. Subscribe to events
            _tab.CandleFinishedEvent += _tab_CandleFinishedEvent;

            // 6. Description
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
            if (_regime.ValueString == RegimeOff)
            {
                return;
            }

            // Need at least MinFractalBars candles for fractal + enough for indicators
            int minBars = Math.Max(MinFractalBars, Math.Max(_ema1Length.ValueInt, Math.Max(_ema2Length.ValueInt, _atrLength.ValueInt)));

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
            decimal tick = _tab.Security.PriceStep;

            // 1. Detect new fractals
            DetectFractals(candles);

            // 2. Fractal invalidation: cancel if price crossed fractal level
            InvalidateFractals(lastClose);

            // 3. Handle open positions - set stop and take if not yet set
            if (HandleOpenPositions(tick))
            {
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
            if (_regime.ValueString != RegimeOnlyShort)
            {
                TryPlaceLongOrder(lastClose, tick, atrValue, ema1Last, ema1Prev, ema2Last, ema2Prev);
            }

            // 7. Try to place Short pending order
            if (_regime.ValueString != RegimeOnlyLong)
            {
                TryPlaceShortOrder(lastClose, tick, atrValue, ema1Last, ema1Prev, ema2Last, ema2Prev);
            }
        }

        #region Helper Methods

        /// <summary>
        /// Detect fractals at the specified index
        /// </summary>
        private void DetectFractals(List<Candle> candles)
        {
            int fi = candles.Count - FractalOffset;

            // Upper fractal: center High > both neighbors' Highs
            if (IsUpperFractal(candles, fi))
            {
                _lastUpperFractal = candles[fi].High;
            }

            // Lower fractal: center Low < both neighbors' Lows
            if (IsLowerFractal(candles, fi))
            {
                _lastLowerFractal = candles[fi].Low;
            }
        }

        /// <summary>
        /// Check if the candle at index forms an upper fractal
        /// </summary>
        private static bool IsUpperFractal(List<Candle> candles, int index)
        {
            decimal centerHigh = candles[index].High;
            return centerHigh > candles[index - FractalLookback].High &&
                   centerHigh > candles[index - 1].High &&
                   centerHigh > candles[index + 1].High &&
                   centerHigh > candles[index + FractalLookback].High;
        }

        /// <summary>
        /// Check if the candle at index forms a lower fractal
        /// </summary>
        private static bool IsLowerFractal(List<Candle> candles, int index)
        {
            decimal centerLow = candles[index].Low;
            return centerLow < candles[index - FractalLookback].Low &&
                   centerLow < candles[index - 1].Low &&
                   centerLow < candles[index + 1].Low &&
                   centerLow < candles[index + FractalLookback].Low;
        }

        /// <summary>
        /// Invalidate fractals if price crosses them
        /// </summary>
        private void InvalidateFractals(decimal lastClose)
        {
            if (_lastUpperFractal != 0 && lastClose > _lastUpperFractal)
            {
                _lastUpperFractal = 0;
            }

            if (_lastLowerFractal != 0 && lastClose < _lastLowerFractal)
            {
                _lastLowerFractal = 0;
            }
        }

        /// <summary>
        /// Handle open positions - set stop and take if not yet set
        /// Returns true if there are open positions (strategy should return)
        /// </summary>
        private bool HandleOpenPositions(decimal tick)
        {
            List<Position> openPositions = _tab.PositionsOpenAll;

            if (openPositions is null || openPositions.Count == 0)
            {
                return false;
            }

            foreach (Position pos in openPositions)
            {
                if (pos.State == PositionStateType.Open && !pos.StopOrderIsActive)
                {
                    SetStopAndTake(pos, tick);
                }
            }

            return true;
        }

        /// <summary>
        /// Try to place a long pending order if conditions are met
        /// </summary>
        private void TryPlaceLongOrder(decimal lastClose, decimal tick, decimal atrValue,
            decimal ema1Last, decimal ema1Prev, decimal ema2Last, decimal ema2Prev)
        {
            // EMA filter: both EMAs rising
            if (ema1Last <= ema1Prev || ema2Last <= ema2Prev)
            {
                return;
            }

            decimal stopPrice = _lastLowerFractal - StopTickOffset * tick;
            decimal entryApprox = _lastUpperFractal;
            decimal stopDistance = entryApprox - stopPrice;
            decimal takeDistance = _kTake.ValueDecimal * stopDistance;

            // Commission filter: take must cover commission
            if (takeDistance < lastClose * _kComiss.ValueDecimal * _comiss.ValueDecimal)
            {
                return;
            }

            // ATR filter: stop distance within acceptable range
            if (stopDistance < _kATR.ValueDecimal * atrValue ||
                stopDistance > _k2ATR.ValueDecimal * atrValue)
            {
                return;
            }

            decimal activationPrice = _lastUpperFractal + EntryTickOffset * tick;
            decimal orderPrice = _lastUpperFractal - EntryTickOffset * tick;

            _tab.BuyAtStop(GetVolume(_tab), orderPrice, activationPrice, StopActivateType.HigherOrEqual);
        }

        /// <summary>
        /// Try to place a short pending order if conditions are met
        /// </summary>
        private void TryPlaceShortOrder(decimal lastClose, decimal tick, decimal atrValue,
            decimal ema1Last, decimal ema1Prev, decimal ema2Last, decimal ema2Prev)
        {
            // EMA filter: both EMAs falling
            if (ema1Last >= ema1Prev || ema2Last >= ema2Prev)
            {
                return;
            }

            decimal stopPrice = _lastUpperFractal + StopTickOffset * tick;
            decimal entryApprox = _lastLowerFractal;
            decimal stopDistance = stopPrice - entryApprox;
            decimal takeDistance = _kTake.ValueDecimal * stopDistance;

            // Commission filter: take must cover commission
            if (takeDistance < lastClose * _kComiss.ValueDecimal * _comiss.ValueDecimal)
            {
                return;
            }

            // ATR filter: stop distance within acceptable range
            if (stopDistance < _kATR.ValueDecimal * atrValue ||
                stopDistance > _k2ATR.ValueDecimal * atrValue)
            {
                return;
            }

            decimal activationPrice = _lastLowerFractal - EntryTickOffset * tick;
            decimal orderPrice = _lastLowerFractal + EntryTickOffset * tick;

            _tab.SellAtStop(GetVolume(_tab), orderPrice, activationPrice, StopActivateType.LowerOrEqual);
        }

        #endregion

        /// <summary>
        /// Set stop-loss and take-profit for an open position
        /// </summary>
        private void SetStopAndTake(Position position, decimal tick)
        {
            decimal entryPrice = position.EntryPrice;

            if (position.Direction == Side.Buy)
            {
                if (_lastLowerFractal == 0)
                {
                    return;
                }

                decimal stopActivation = _lastLowerFractal - StopTickOffset * tick;
                decimal stopOrder = _lastLowerFractal - StopOrderTickOffset * tick;
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

                decimal stopActivation = _lastUpperFractal + StopTickOffset * tick;
                decimal stopOrder = _lastUpperFractal + StopOrderTickOffset * tick;
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

        /// <summary>
        /// Calculate volume for order based on selected volume type
        /// </summary>
        private decimal GetVolume(BotTabSimple tab)
        {
            return _volumeType.ValueString switch
            {
                VolumeTypeContracts => _volume.ValueDecimal,
                VolumeTypeContractCurrency => CalculateVolumeByContractCurrency(tab),
                VolumeTypeDepositPercent => CalculateVolumeByDepositPercent(tab),
                _ => 0
            };
        }

        /// <summary>
        /// Calculate volume based on contract currency
        /// </summary>
        private decimal CalculateVolumeByContractCurrency(BotTabSimple tab)
        {
            decimal contractPrice = tab.PriceBestAsk;
            decimal volume = _volume.ValueDecimal / contractPrice;

            if (StartProgram == StartProgram.IsOsTrader)
            {
                IServerPermission serverPermission = ServerMaster.GetServerPermission(tab.Connector.ServerType);

                if (serverPermission is not null &&
                    serverPermission.IsUseLotToCalculateProfit &&
                    tab.Security.Lot > 1)
                {
                    volume = _volume.ValueDecimal / (contractPrice * tab.Security.Lot);
                }

                return Math.Round(volume, tab.Security.DecimalsVolume);
            }

            return Math.Round(volume, DefaultVolumeDecimals);
        }

        /// <summary>
        /// Calculate volume based on deposit percent
        /// </summary>
        private decimal CalculateVolumeByDepositPercent(BotTabSimple tab)
        {
            Portfolio myPortfolio = tab.Portfolio;

            if (myPortfolio is null)
            {
                return 0;
            }

            decimal portfolioPrimeAsset = GetPortfolioPrimeAsset(myPortfolio);

            if (portfolioPrimeAsset == 0)
            {
                SendNewLogMessage($"Can't found portfolio {_tradeAssetInPortfolio.ValueString}", Logging.LogMessageType.Error);
                return 0;
            }

            decimal moneyOnPosition = portfolioPrimeAsset * (_volume.ValueDecimal / 100);
            decimal qty = moneyOnPosition / tab.PriceBestAsk / tab.Security.Lot;

            if (tab.StartProgram == StartProgram.IsOsTrader)
            {
                if (tab.Security.UsePriceStepCostToCalculateVolume &&
                    tab.Security.PriceStep != tab.Security.PriceStepCost &&
                    tab.PriceBestAsk != 0 &&
                    tab.Security.PriceStep != 0 &&
                    tab.Security.PriceStepCost != 0)
                {
                    qty = moneyOnPosition / (tab.PriceBestAsk / tab.Security.PriceStep * tab.Security.PriceStepCost);
                }
                return Math.Round(qty, tab.Security.DecimalsVolume);
            }

            return Math.Round(qty, TesterVolumeDecimals);
        }

        /// <summary>
        /// Get portfolio prime asset value
        /// </summary>
        private decimal GetPortfolioPrimeAsset(Portfolio portfolio)
        {
            if (_tradeAssetInPortfolio.ValueString == AssetPrime)
            {
                return portfolio.ValueCurrent;
            }

            List<PositionOnBoard> positionOnBoard = portfolio.GetPositionOnBoard();

            if (positionOnBoard is null)
            {
                return 0;
            }

            foreach (PositionOnBoard position in positionOnBoard)
            {
                if (position.SecurityNameCode == _tradeAssetInPortfolio.ValueString)
                {
                    return position.ValueCurrent;
                }
            }

            return 0;
        }
    }
}
