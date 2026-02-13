/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
 */

using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Attributes;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.IO;

/* Description
Торговая система Пробой фрактала с фильтром Kalman

Торговля только отложенными лимитными ордерами.
Выход по стоп-лосс или тейк-профит.

Фрактал Нижний – 5 баров, центральный Low ниже 2-х других Low слева и справа.
Фрактал Верхний – 5 баров, центральный High выше High 2-х других слева и справа.

Фильтр Kalman для определения направления тренда вместо EMA.
*/

namespace OsEngine.Robots.Trend
{
    [Bot("wsurfbot")]
    public class WsurfBot : BotPanel
    {
        private BotTabSimple _tab;

        // Параметры из ТЗ
        public BotTradeRegime Regime;
        public decimal KTake = 1.5m;
        public decimal Comiss = 0.001m;
        public decimal KComiss = 3.0m;
        public int AtrLength = 14;
        public decimal Katr = 1.0m;
        public decimal K2Atr = 2.0m;
        public int Kalman1Length = 12;
        public int Kalman2Length = 26;
        
        // Параметры объема
        public decimal Volume = 1;
        public string VolumeType = "Deposit percent";
        public string TradeAssetInPortfolio = "Prime";

        // Внутренние переменные для расчетов
        private readonly List<decimal> _highPrices = new();
        private readonly List<decimal> _lowPrices = new();
        private readonly List<decimal> _closePrices = new();
        private readonly List<decimal> _trueRanges = new();
        
        // Kalman фильтры
        private KalmanFilter _kalman1;
        private KalmanFilter _kalman2;
        
        // Фракталы
        private decimal? _lastUpperFractal;
        private decimal? _lastLowerFractal;
        private int _lastUpperFractalIndex = -1;
        private int _lastLowerFractalIndex = -1;
        
        // Последние значения ATR
        private decimal _lastAtr;

        public WsurfBot(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];

            // Инициализация Kalman фильтров
            _kalman1 = new KalmanFilter(Kalman1Length);
            _kalman2 = new KalmanFilter(Kalman2Length);

            // Подписка на события
            _tab.CandleFinishedEvent += Strateg_CandleFinishedEvent;

            DeleteEvent += Strategy_DeleteEvent;
            Load();

            Description = "WsurfBot - Fractal Breakthrough with Kalman filter";
        }

        public override string GetNameStrategyType()
        {
            return "wsurfbot";
        }

        public override void ShowIndividualSettingsDialog()
        {
            // TODO: Создать UI для настроек
        }

        private void Strateg_CandleFinishedEvent(List<Candle> candles)
        {
            if (Regime == BotTradeRegime.Off)
                return;

            if (candles.Count < Math.Max(AtrLength, Math.Max(Kalman1Length, Kalman2Length)) + 5)
                return;

            UpdateIndicators(candles);
            CheckFractalCancellation(candles);
            
            var openPositions = _tab.PositionsOpenAll;
            
            // Закрытие позиций по стоп/тейк
            if (openPositions.Count > 0)
            {
                foreach (var position in openPositions)
                {
                    CheckStopLossAndTakeProfit(position, candles);
                }
            }
            
            if (Regime == BotTradeRegime.OnlyClosePosition)
                return;

            // Открытие новых позиций
            if (openPositions.Count == 0)
            {
                CheckAndPlacePendingOrders(candles);
            }
        }

        private void UpdateIndicators(List<Candle> candles)
        {
            var currentCandle = candles[^1];
            
            // Обновление ценовых данных
            _highPrices.Add(currentCandle.High);
            _lowPrices.Add(currentCandle.Low);
            _closePrices.Add(currentCandle.Close);
            
            // Расчет True Range для ATR
            if (_closePrices.Count > 1)
            {
                var tr = Math.Max(
                    currentCandle.High - currentCandle.Low,
                    Math.Max(
                        Math.Abs(currentCandle.High - _closePrices[^2]),
                        Math.Abs(currentCandle.Low - _closePrices[^2])
                    )
                );
                _trueRanges.Add(tr);
            }
            
            // Ограничиваем размер массивов
            const int maxHistory = 300;
            if (_highPrices.Count > maxHistory)
            {
                _highPrices.RemoveAt(0);
                _lowPrices.RemoveAt(0);
                _closePrices.RemoveAt(0);
                _trueRanges.RemoveAt(0);
            }
            
            // Обновление Kalman фильтров
            _kalman1.Update(currentCandle.Close);
            _kalman2.Update(currentCandle.Close);
            
            // Расчет ATR
            if (_trueRanges.Count >= AtrLength)
            {
                var sum = 0m;
                for (int i = _trueRanges.Count - AtrLength; i < _trueRanges.Count; i++)
                {
                    sum += _trueRanges[i];
                }
                _lastAtr = sum / AtrLength;
            }
            
            // Проверка фракталов
            CheckFractals(candles);
        }

        private void CheckFractals(List<Candle> candles)
        {
            if (candles.Count < 5)
                return;

            var currentIndex = candles.Count - 3; // Центральный бар фрактала (третий с конца)
            
            if (currentIndex < 2)
                return;

            // Проверка верхнего фрактала
            var isUpperFractal = true;
            var centerHigh = candles[currentIndex].High;
            
            for (int i = currentIndex - 2; i <= currentIndex + 2; i++)
            {
                if (i == currentIndex) continue;
                if (candles[i].High >= centerHigh)
                {
                    isUpperFractal = false;
                    break;
                }
            }
            
            if (isUpperFractal)
            {
                _lastUpperFractal = centerHigh;
                _lastUpperFractalIndex = currentIndex;
            }
            
            // Проверка нижнего фрактала
            var isLowerFractal = true;
            var centerLow = candles[currentIndex].Low;
            
            for (int i = currentIndex - 2; i <= currentIndex + 2; i++)
            {
                if (i == currentIndex) continue;
                if (candles[i].Low <= centerLow)
                {
                    isLowerFractal = false;
                    break;
                }
            }
            
            if (isLowerFractal)
            {
                _lastLowerFractal = centerLow;
                _lastLowerFractalIndex = currentIndex;
            }
        }

        private void CheckFractalCancellation(List<Candle> candles)
        {
            var currentCandle = candles[^1];
            
            // Отмена верхнего фрактала
            if (_lastUpperFractal.HasValue && currentCandle.High > _lastUpperFractal.Value)
            {
                _lastUpperFractal = null;
                _lastUpperFractalIndex = -1;
            }
            
            // Отмена нижнего фрактала
            if (_lastLowerFractal.HasValue && currentCandle.Low < _lastLowerFractal.Value)
            {
                _lastLowerFractal = null;
                _lastLowerFractalIndex = -1;
            }
        }

        private void CheckAndPlacePendingOrders(List<Candle> candles)
        {
            if (!_lastUpperFractal.HasValue || !_lastLowerFractal.HasValue)
                return;

            var currentCandle = candles[^1];
            
            // Проверка фильтров
            if (!PassFilters(currentCandle))
                return;

            // Проверка направления тренда через Kalman
            var kalman1Trend = _kalman1.GetCurrentValue() > _kalman1.GetPreviousValue();
            var kalman2Trend = _kalman2.GetCurrentValue() > _kalman2.GetPreviousValue();

            // Long: если оба Kalman растут и цена пробивает верхний фрактал
            if (kalman1Trend && kalman2Trend && Regime != BotTradeRegime.OnlyShort)
            {
                if (currentCandle.Close > _lastUpperFractal.Value)
                {
                    _tab.BuyAtLimit(GetVolume(_tab), currentCandle.Close + _tab.Security.PriceStep);
                }
            }

            // Short: если оба Kalman падают и цена пробивает нижний фрактал
            if (!kalman1Trend && !kalman2Trend && Regime != BotTradeRegime.OnlyLong)
            {
                if (currentCandle.Close < _lastLowerFractal.Value)
                {
                    _tab.SellAtLimit(GetVolume(_tab), currentCandle.Close - _tab.Security.PriceStep);
                }
            }
        }

        private bool PassFilters(Candle currentCandle)
        {
            if (_lastAtr == 0)
                return false;

            // Расчет стоп-лосса
            var stopLoss = 0m;
            if (_lastLowerFractal.HasValue)
                stopLoss = currentCandle.Close - _lastLowerFractal.Value;
            if (_lastUpperFractal.HasValue)
                stopLoss = Math.Max(stopLoss, _lastUpperFractal.Value - currentCandle.Close);

            // Фильтр по ATR
            if (stopLoss < Katr * _lastAtr || stopLoss > K2Atr * _lastAtr)
                return false;

            // Фильтр по комиссии
            var takeProfit = KTake * stopLoss;
            var minTakeProfit = currentCandle.Close * KComiss * Comiss;
            
            return takeProfit >= minTakeProfit;
        }

        private void CheckStopLossAndTakeProfit(Position position, List<Candle> candles)
        {
            if (position.State != PositionStateType.Open)
                return;

            var currentCandle = candles[^1];
            var stopLoss = 0m;
            
            // Расчет стоп-лосса на основе последних фракталов
            if (position.Direction == Side.Buy && _lastLowerFractal.HasValue)
            {
                stopLoss = _lastLowerFractal.Value - 2 * _tab.Security.PriceStep;
                var takeProfit = position.EntryPrice + KTake * (position.EntryPrice - stopLoss);
                
                if (currentCandle.Low <= stopLoss)
                {
                    _tab.CloseAtLimit(position, stopLoss, position.OpenVolume);
                }
                else if (currentCandle.High >= takeProfit)
                {
                    _tab.CloseAtLimit(position, takeProfit, position.OpenVolume);
                }
            }
            else if (position.Direction == Side.Sell && _lastUpperFractal.HasValue)
            {
                stopLoss = _lastUpperFractal.Value + 2 * _tab.Security.PriceStep;
                var takeProfit = position.EntryPrice - KTake * (stopLoss - position.EntryPrice);
                
                if (currentCandle.High >= stopLoss)
                {
                    _tab.CloseAtLimit(position, stopLoss, position.OpenVolume);
                }
                else if (currentCandle.Low <= takeProfit)
                {
                    _tab.CloseAtLimit(position, takeProfit, position.OpenVolume);
                }
            }
        }

        private decimal GetVolume(BotTabSimple tab)
        {
            decimal volume = 0;

            if (VolumeType == "Contracts")
            {
                volume = Volume;
            }
            else if (VolumeType == "Contract currency")
            {
                decimal contractPrice = tab.PriceBestAsk;
                volume = Volume / contractPrice;

                if (StartProgram == StartProgram.IsOsTrader)
                {
                    IServerPermission serverPermission = ServerMaster.GetServerPermission(tab.Connector.ServerType);

                    if (serverPermission != null &&
                        serverPermission.IsUseLotToCalculateProfit &&
                        tab.Security.Lot != 0 &&
                        tab.Security.Lot > 1)
                    {
                        volume = Volume / (contractPrice * tab.Security.Lot);
                    }

                    volume = Math.Round(volume, tab.Security.DecimalsVolume);
                }
                else // Tester or Optimizer
                {
                    volume = Math.Round(volume, 6);
                }
            }
            else if (VolumeType == "Deposit percent")
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
                        if (positionOnBoard[i].SecurityNameCode == TradeAssetInPortfolio)
                        {
                            portfolioPrimeAsset = positionOnBoard[i].ValueCurrent;
                            break;
                        }
                    }
                }

                if (portfolioPrimeAsset == 0)
                {
                    SendNewLogMessage("Can`t found portfolio " + TradeAssetInPortfolio, Logging.LogMessageType.Error);
                    return 0;
                }

                decimal moneyOnPosition = portfolioPrimeAsset * (Volume / 100);

                decimal qty = moneyOnPosition / tab.PriceBestAsk / tab.Security.Lot;

                if (tab.StartProgram == StartProgram.IsOsTrader)
                {
                    if (tab.Security.UsePriceStepCostToCalculateVolume == true
                        && tab.Security.PriceStep != tab.Security.PriceStepCost
                        && tab.PriceBestAsk != 0
                        && tab.Security.PriceStep != 0
                        && tab.Security.PriceStepCost != 0)
                    {// расчёт количества контрактов для фьючерсов и опционов на Мосбирже
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

        private void Save()
        {
            try
            {
                using var writer = new StreamWriter(@"Engine\" + NameStrategyUniq + @"SettingsBot.txt", false);
                writer.WriteLine(Regime);
                writer.WriteLine(KTake);
                writer.WriteLine(Comiss);
                writer.WriteLine(KComiss);
                writer.WriteLine(AtrLength);
                writer.WriteLine(Katr);
                writer.WriteLine(K2Atr);
                writer.WriteLine(Kalman1Length);
                writer.WriteLine(Kalman2Length);
                writer.WriteLine(VolumeType);
                writer.WriteLine(TradeAssetInPortfolio);
                writer.WriteLine(Volume);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void Load()
        {
            if (!File.Exists(@"Engine\" + NameStrategyUniq + @"SettingsBot.txt"))
                return;

            try
            {
                using var reader = new StreamReader(@"Engine\" + NameStrategyUniq + @"SettingsBot.txt");
                Enum.TryParse(reader.ReadLine(), true, out Regime);
                KTake = Convert.ToDecimal(reader.ReadLine());
                Comiss = Convert.ToDecimal(reader.ReadLine());
                KComiss = Convert.ToDecimal(reader.ReadLine());
                AtrLength = Convert.ToInt32(reader.ReadLine());
                Katr = Convert.ToDecimal(reader.ReadLine());
                K2Atr = Convert.ToDecimal(reader.ReadLine());
                Kalman1Length = Convert.ToInt32(reader.ReadLine());
                Kalman2Length = Convert.ToInt32(reader.ReadLine());
                VolumeType = Convert.ToString(reader.ReadLine());
                TradeAssetInPortfolio = Convert.ToString(reader.ReadLine());
                Volume = Convert.ToDecimal(reader.ReadLine());
            }
            catch (Exception)
            {
                // ignore
            }
        }

        private void Strategy_DeleteEvent()
        {
            if (File.Exists(@"Engine\" + NameStrategyUniq + @"SettingsBot.txt"))
            {
                File.Delete(@"Engine\" + NameStrategyUniq + @"SettingsBot.txt");
            }
        }
    }

    // Kalman Filter implementation
    public class KalmanFilter
    {
        private readonly int _length;
        private decimal _kalman;
        private decimal _kalmanPrevious;
        private decimal _velocity;
        private readonly decimal _processNoise;
        private readonly decimal _measurementNoise;
        private decimal _covariance;
        private readonly decimal _initialCovariance;

        public KalmanFilter(int length)
        {
            _length = length;
            _processNoise = 0.01m / length;
            _measurementNoise = 0.1m;
            _initialCovariance = 1.0m;
            _covariance = _initialCovariance;
            _kalman = 0;
            _kalmanPrevious = 0;
            _velocity = 0;
        }

        public void Update(decimal measurement)
        {
            // Prediction step
            _kalmanPrevious = _kalman;
            _kalman += _velocity;
            _covariance += _processNoise;

            // Update step
            var kalmanGain = _covariance / (_covariance + _measurementNoise);
            _kalman += kalmanGain * (measurement - _kalman);
            _velocity += kalmanGain * (measurement - _kalmanPrevious - _velocity);
            _covariance = (1 - kalmanGain) * _covariance;
        }

        public decimal GetCurrentValue() => _kalman;
        public decimal GetPreviousValue() => _kalmanPrevious;
    }
}
