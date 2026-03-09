/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Attributes;
using OsEngine.OsTrader.Panels.Tab;

/* Description
trading robot for osengine

Manual impulse strategy without indicators.

Buy:
1. Both mid-price momentum values are above 0 and rising.
2. Candle volume is above the average volume by X percent.
3. Candle body is larger than Y * ATR.
4. Close is above the candle midpoint.

Sell:
1. Both mid-price momentum values are below 0 and falling.
2. Candle volume is above the average volume by X percent.
3. Candle body is larger than Y * ATR.
4. Close is below the candle midpoint.

Exit:
Take-profit in price steps or close after N bars.
Open positions are not netted. Every new signal opens another position.
 */

namespace OsEngine.Robots
{
    [Bot("ImpulsV1")]
    public class ImpulsV1 : BotPanel
    {
        private readonly BotTabSimple _tab;
        private readonly List<decimal> _momentumValues = new List<decimal>();
        private readonly List<decimal> _secondMomentumValues = new List<decimal>();
        private readonly List<decimal> _trueRangeValues = new List<decimal>();
        private readonly List<decimal> _atrValues = new List<decimal>();
        private bool _isDeleted;
        private int _cachedMomentumLength;
        private int _cachedSecondMomentumLength;
        private int _cachedAtrLength;

        private readonly StrategyParameterString _regime;
        private readonly StrategyParameterDecimal _slippageSteps;
        private readonly StrategyParameterTimeOfDay _startTradeTime;
        private readonly StrategyParameterTimeOfDay _endTradeTime;

        private readonly StrategyParameterInt _momentumLength;
        private readonly StrategyParameterInt _secondMomentumLength;
        private readonly StrategyParameterBool _checkMomentumOneLongZero;
        private readonly StrategyParameterBool _checkMomentumOneShortZero;
        private readonly StrategyParameterBool _checkMomentumTwoLongZero;
        private readonly StrategyParameterBool _checkMomentumTwoShortZero;
        private readonly StrategyParameterInt _averageVolumeLength;
        private readonly StrategyParameterInt _atrLength;
        private readonly StrategyParameterDecimal _volumeExcessPercent;
        private readonly StrategyParameterDecimal _bodyAtrMultiplier;

        private readonly StrategyParameterInt _exitBars;
        private readonly StrategyParameterDecimal _takeProfitPoints;

        public ImpulsV1(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];

            _regime = CreateParameter("Regime", "Off", new[] { "Off", "On", "OnlyLong", "OnlyShort", "OnlyClosePosition" }, "Base");
            _slippageSteps = CreateParameter("Slippage steps", 0m, 0m, 50m, 1m, "Base");
            _startTradeTime = CreateParameterTimeOfDay("Start Trade Time", 0, 0, 0, 0, "Base");
            _endTradeTime = CreateParameterTimeOfDay("End Trade Time", 24, 0, 0, 0, "Base");

            _momentumLength = CreateParameter("Momentum Length", 24, 2, 500, 1, "Momentum");
            _secondMomentumLength = CreateParameter("Second Momentum Length", 48, 2, 500, 1, "Momentum");
            _checkMomentumOneLongZero = CreateParameter("Check Momentum 1 > 0 for Long", true, "Momentum");
            _checkMomentumOneShortZero = CreateParameter("Check Momentum 1 < 0 for Short", true, "Momentum");
            _checkMomentumTwoLongZero = CreateParameter("Check Momentum 2 > 0 for Long", true, "Momentum");
            _checkMomentumTwoShortZero = CreateParameter("Check Momentum 2 < 0 for Short", true, "Momentum");

            _averageVolumeLength = CreateParameter("Average Volume Length", 200, 5, 5000, 5, "Filters");
            _atrLength = CreateParameter("ATR Length", 5, 1, 200, 1, "Filters");
            _volumeExcessPercent = CreateParameter("Volume Excess Percent", 10.0m, 0.1m, 500m, 0.1m, "Filters");
            _bodyAtrMultiplier = CreateParameter("Body ATR Multiplier", 0.5m, 0.1m, 10m, 0.1m, "Filters");

            _exitBars = CreateParameter("Exit Bars", 20, 1, 500, 1, "Exit");
            _takeProfitPoints = CreateParameter("Take Profit Points", 500.0m, 1m, 100000m, 1m, "Exit");

            _tab.CandleFinishedEvent += OnCandleFinished;
            DeleteEvent += OnDelete;

            Description = "Manual impulse strategy using two mid-price momentum confirmations with configurable zero-threshold checks, volume expansion, ATR body filter and midpoint close confirmation.";
        }

        public override string GetNameStrategyType()
        {
            return "ImpulsV1";
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
                // Tab can already be disposed during bot removal.
            }

            DeleteEvent -= OnDelete;
            ClearCalculatedValues();
        }

        private void OnCandleFinished(List<Candle> candles)
        {
            if (_isDeleted
                || candles == null
                || _regime.ValueString == "Off")
            {
                return;
            }

            if (_startTradeTime.Value > _tab.TimeServerCurrent
                || _endTradeTime.Value < _tab.TimeServerCurrent)
            {
                return;
            }

            if (candles.Count < GetMinCandlesCount())
            {
                return;
            }

            EnsureCalculatedValues(candles);

            List<Position> openPositions = _tab.PositionsOpenAll;

            if (openPositions != null && openPositions.Count > 0)
            {
                LogicClosePosition(candles, openPositions);
            }

            if (_regime.ValueString == "OnlyClosePosition")
            {
                return;
            }

            LogicOpenPosition(candles);
        }

        private int GetMinCandlesCount()
        {
            int minCandles = Math.Max(_momentumLength.ValueInt, _secondMomentumLength.ValueInt) + 2;
            int volumeCandles = _averageVolumeLength.ValueInt + 1;
            int atrCandles = _atrLength.ValueInt;

            if (volumeCandles > minCandles)
            {
                minCandles = volumeCandles;
            }

            if (atrCandles > minCandles)
            {
                minCandles = atrCandles;
            }

            return minCandles;
        }

        private void LogicOpenPosition(List<Candle> candles)
        {
            int lastIndex = candles.Count - 1;
            Candle lastCandle = candles[lastIndex];

            decimal momentum = _momentumValues[lastIndex];
            decimal previousMomentum = _momentumValues[lastIndex - 1];
            decimal secondMomentum = _secondMomentumValues[lastIndex];
            decimal previousSecondMomentum = _secondMomentumValues[lastIndex - 1];
            decimal averageVolume = CalculateAverageVolume(candles, lastIndex, _averageVolumeLength.ValueInt);
            decimal atr = _atrValues[lastIndex];

            if (averageVolume <= 0m || atr <= 0m)
            {
                return;
            }

            decimal bodySize = Math.Abs(lastCandle.Close - lastCandle.Open);
            decimal requiredVolume = averageVolume * (1m + _volumeExcessPercent.ValueDecimal / 100m);
            bool isVolumeConditionPassed = lastCandle.Volume > requiredVolume;
            bool isBodyConditionPassed = bodySize > _bodyAtrMultiplier.ValueDecimal * atr;

            if (!isVolumeConditionPassed || !isBodyConditionPassed)
            {
                return;
            }

            decimal volume = GetEntryVolume();

            if (volume <= 0m)
            {
                return;
            }

            decimal slippage = GetSlippagePrice();
            bool isFirstMomentumLongConfirmed = IsLongMomentumConfirmed(momentum, previousMomentum, _checkMomentumOneLongZero.ValueBool);
            bool isSecondMomentumLongConfirmed = IsLongMomentumConfirmed(secondMomentum, previousSecondMomentum, _checkMomentumTwoLongZero.ValueBool);
            bool isLongMomentumConfirmed = isFirstMomentumLongConfirmed
                && isSecondMomentumLongConfirmed;

            bool isFirstMomentumShortConfirmed = IsShortMomentumConfirmed(momentum, previousMomentum, _checkMomentumOneShortZero.ValueBool);
            bool isSecondMomentumShortConfirmed = IsShortMomentumConfirmed(secondMomentum, previousSecondMomentum, _checkMomentumTwoShortZero.ValueBool);
            bool isShortMomentumConfirmed = isFirstMomentumShortConfirmed
                && isSecondMomentumShortConfirmed;

            if (_regime.ValueString != "OnlyShort"
                && isLongMomentumConfirmed
                && lastCandle.Close > lastCandle.Low + 0.5m * (lastCandle.High - lastCandle.Low))
            {
                _tab.BuyAtLimit(volume, GetBuyOrderPrice(lastCandle.Close, slippage), "ImpulseLong");
            }

            if (_regime.ValueString != "OnlyLong"
                && isShortMomentumConfirmed
                && lastCandle.Close < lastCandle.High - 0.5m * (lastCandle.High - lastCandle.Low))
            {
                _tab.SellAtLimit(volume, GetSellOrderPrice(lastCandle.Close, slippage), "ImpulseShort");
            }
        }

        private void LogicClosePosition(List<Candle> candles, List<Position> openPositions)
        {
            int lastIndex = candles.Count - 1;
            decimal slippage = GetSlippagePrice();
            decimal takeProfitDistance = _takeProfitPoints.ValueDecimal * GetPriceStep();
            decimal closePrice = candles[lastIndex].Close;

            for (int i = 0; i < openPositions.Count; i++)
            {
                Position position = openPositions[i];

                if (position == null
                    || position.State != PositionStateType.Open
                    || position.OpenVolume <= 0)
                {
                    continue;
                }

                if (NeedCloseByBarCount(position, candles))
                {
                    if (position.CloseActive)
                    {
                        continue;
                    }

                    if (position.Direction == Side.Buy)
                    {
                        _tab.CloseAtLimit(position, GetSellOrderPrice(closePrice, slippage), position.OpenVolume);
                    }
                    else
                    {
                        _tab.CloseAtLimit(position, GetBuyOrderPrice(closePrice, slippage), position.OpenVolume);
                    }

                    continue;
                }

                if (takeProfitDistance <= 0m || position.CloseActive)
                {
                    continue;
                }

                if (position.Direction == Side.Buy)
                {
                    decimal activationPrice = position.EntryPrice + takeProfitDistance;
                    _tab.CloseAtProfit(position, activationPrice, activationPrice + slippage);
                }
                else
                {
                    decimal activationPrice = position.EntryPrice - takeProfitDistance;
                    _tab.CloseAtProfit(position, activationPrice, activationPrice - slippage);
                }
            }
        }

        private bool NeedCloseByBarCount(Position position, List<Candle> candles)
        {
            DateTime openTime = position.TimeOpen;
            int barsFromOpen = 0;

            for (int i = candles.Count - 1; i >= 0; i--)
            {
                barsFromOpen++;

                if (candles[i].TimeStart <= openTime)
                {
                    return barsFromOpen >= _exitBars.ValueInt + 1;
                }
            }

            return false;
        }

        private void EnsureCalculatedValues(List<Candle> candles)
        {
            if (_cachedMomentumLength != _momentumLength.ValueInt
                || _cachedSecondMomentumLength != _secondMomentumLength.ValueInt
                || _cachedAtrLength != _atrLength.ValueInt
                || _momentumValues.Count > candles.Count
                || _secondMomentumValues.Count > candles.Count
                || _trueRangeValues.Count > candles.Count
                || _atrValues.Count > candles.Count)
            {
                RebuildCalculatedValues(candles);
                return;
            }

            if (_momentumValues.Count == candles.Count
                && _secondMomentumValues.Count == candles.Count
                && _trueRangeValues.Count == candles.Count
                && _atrValues.Count == candles.Count)
            {
                UpdateCalculatedValue(candles, candles.Count - 1);
                return;
            }

            if (_momentumValues.Count + 1 == candles.Count
                && _secondMomentumValues.Count + 1 == candles.Count
                && _trueRangeValues.Count + 1 == candles.Count
                && _atrValues.Count + 1 == candles.Count)
            {
                AppendCalculatedValue(candles, candles.Count - 1);
                return;
            }

            RebuildCalculatedValues(candles);
        }

        private void RebuildCalculatedValues(List<Candle> candles)
        {
            ClearCalculatedValues();
            _cachedMomentumLength = _momentumLength.ValueInt;
            _cachedSecondMomentumLength = _secondMomentumLength.ValueInt;
            _cachedAtrLength = _atrLength.ValueInt;

            for (int i = 0; i < candles.Count; i++)
            {
                AppendCalculatedValue(candles, i);
            }
        }

        private void ClearCalculatedValues()
        {
            _momentumValues.Clear();
            _secondMomentumValues.Clear();
            _trueRangeValues.Clear();
            _atrValues.Clear();
            _cachedMomentumLength = 0;
            _cachedSecondMomentumLength = 0;
            _cachedAtrLength = 0;
        }

        private void AppendCalculatedValue(List<Candle> candles, int index)
        {
            _momentumValues.Add(CalculateMomentum(candles, index, _momentumLength.ValueInt));
            _secondMomentumValues.Add(CalculateMomentum(candles, index, _secondMomentumLength.ValueInt));
            _trueRangeValues.Add(CalculateTrueRange(candles, index));
            _atrValues.Add(CalculateAtr(index));
        }

        private void UpdateCalculatedValue(List<Candle> candles, int index)
        {
            _momentumValues[index] = CalculateMomentum(candles, index, _momentumLength.ValueInt);
            _secondMomentumValues[index] = CalculateMomentum(candles, index, _secondMomentumLength.ValueInt);
            _trueRangeValues[index] = CalculateTrueRange(candles, index);
            _atrValues[index] = CalculateAtr(index);
        }

        private decimal CalculateMomentum(List<Candle> candles, int index, int length)
        {
            if (length <= 0 || index < length)
            {
                return 0m;
            }

            decimal baseMidPrice = GetMidPrice(candles[index - length]);

            if (baseMidPrice <= 0m)
            {
                return 0m;
            }

            decimal currentMidPrice = GetMidPrice(candles[index]);
            return Math.Round((currentMidPrice / baseMidPrice - 1m) * 100m, 2);
        }

        private bool IsLongMomentumConfirmed(decimal currentMomentum, decimal previousMomentum, bool checkZeroThreshold)
        {
            if (currentMomentum <= previousMomentum)
            {
                return false;
            }

            return !checkZeroThreshold || currentMomentum > 0m;
        }

        private bool IsShortMomentumConfirmed(decimal currentMomentum, decimal previousMomentum, bool checkZeroThreshold)
        {
            if (currentMomentum >= previousMomentum)
            {
                return false;
            }

            return !checkZeroThreshold || currentMomentum < 0m;
        }

        private decimal CalculateTrueRange(List<Candle> candles, int index)
        {
            if (index == 0)
            {
                return 0m;
            }

            decimal highToLow = Math.Abs(candles[index].High - candles[index].Low);
            decimal previousCloseToHigh = Math.Abs(candles[index - 1].Close - candles[index].High);
            decimal previousCloseToLow = Math.Abs(candles[index - 1].Close - candles[index].Low);

            decimal trueRange = Math.Max(highToLow, previousCloseToHigh);

            if (previousCloseToLow > trueRange)
            {
                trueRange = previousCloseToLow;
            }

            return trueRange;
        }

        private decimal CalculateAtr(int index)
        {
            int length = _atrLength.ValueInt;

            if (length <= 0 || index < length - 1)
            {
                return 0m;
            }

            if (index == length - 1)
            {
                decimal sum = 0m;

                for (int i = 0; i < length; i++)
                {
                    sum += _trueRangeValues[i];
                }

                return Math.Round(sum / length, 7);
            }

            decimal previousAtr = _atrValues[index - 1];
            decimal currentTrueRange = _trueRangeValues[index];

            return Math.Round((previousAtr * (length - 1) + currentTrueRange) / length, 7);
        }

        private decimal CalculateAverageVolume(List<Candle> candles, int endIndexExclusive, int length)
        {
            if (length <= 0 || endIndexExclusive < length)
            {
                return 0m;
            }

            decimal sum = 0m;
            int startIndex = endIndexExclusive - length;

            for (int i = startIndex; i < endIndexExclusive; i++)
            {
                sum += candles[i].Volume;
            }

            return sum / length;
        }

        private decimal GetMidPrice(Candle candle)
        {
            return (candle.High + candle.Low) / 2m;
        }

        private decimal GetEntryVolume()
        {
            if (_tab.Security == null)
            {
                return 1m;
            }

            return Math.Round(1m, _tab.Security.DecimalsVolume);
        }

        private decimal GetPriceStep()
        {
            if (_tab.Security == null || _tab.Security.PriceStep <= 0m)
            {
                return 1m;
            }

            return _tab.Security.PriceStep;
        }

        private decimal GetSlippagePrice()
        {
            return _slippageSteps.ValueDecimal * GetPriceStep();
        }

        private decimal GetBuyOrderPrice(decimal fallbackPrice, decimal slippage)
        {
            decimal price = _tab.PriceBestAsk > 0m ? _tab.PriceBestAsk : fallbackPrice;
            return price + slippage;
        }

        private decimal GetSellOrderPrice(decimal fallbackPrice, decimal slippage)
        {
            decimal price = _tab.PriceBestBid > 0m ? _tab.PriceBestBid : fallbackPrice;
            return price - slippage;
        }
    }
}
