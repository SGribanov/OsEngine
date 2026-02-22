#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Charts.CandleChart.Indicators;
using OsEngine.Entity;
using OsEngine.Indicators;
using OsEngine.Language;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.Market.Servers.Tester;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Attributes;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

/* Description
The Countertrend robot
Buy:
1. Price below BollingerDownLine.
Sell:
1. The price is more than BollingerUpLine.
Exit:
1. At the intersection of Sma with the price
*/

namespace OsEngine.Robots.CounterTrend
{
    [Bot("StrategyBollinger")]
    public class StrategyBollinger : BotPanel
    {
        // Tab to trade
        private BotTabSimple _tab;

        // Basic settings
        public decimal _slippage;
        public BotTradeRegime _regime;

        // GetVolume settings
        public decimal _volume;
        public string _volumeType;
        public string _tradeAssetInPortfolio;
        
        // Indicators
        private Aindicator _bollinger;
        private Aindicator _sma;

        public StrategyBollinger(string name, StartProgram startProgram) : base(name, startProgram)
        {
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];

            // Create indicator Bollinger
            _bollinger = IndicatorsFactory.CreateIndicatorByName("Bollinger", name + "bollinger", false);
            _bollinger = (Aindicator)_tab.CreateCandleIndicator(_bollinger, "Prime");
            _bollinger.Save();

            // Create indicator Sma
            _sma = IndicatorsFactory.CreateIndicatorByName("Sma", name + "Sma", false);
            _sma = (Aindicator)_tab.CreateCandleIndicator(_sma, "Prime");
            ((IndicatorParameterInt)_sma.Parameters[0]).ValueInt = 15;
            _sma.Save();

            _tab.CandleFinishedEvent += Bot_CandleFinishedEvent;

            _volumeType = "Deposit percent";
            _slippage = 0;
            _volume = 1;
            _regime = BotTradeRegime.On;

            DeleteEvent += Strategy_DeleteEvent;

            Load();

            Description = OsLocalization.Description.DescriptionLabel24;

        }

        // The name of the robot in OsEngine
        public override string GetNameStrategyType()
        {
            return "StrategyBollinger";
        }

        // show settings window
        public override void ShowIndividualSettingsDialog()
        {
            StrategyBollingerUi ui = new StrategyBollingerUi(this);
            ui.ShowDialog();
        }

        // save settings in .txt file
        public void Save()
        {
            try
            {
                global::OsEngine.Entity.SafeFileWriter.WriteAllLines(GetSettingsPath(), new[]
                {
                    _volumeType,
                    _tradeAssetInPortfolio,
                    _slippage.ToString(CultureInfo.InvariantCulture),
                    _volume.ToString(CultureInfo.InvariantCulture),
                    _regime.ToString()
                });
            }
            catch (Exception)
            {
                // ignore
            }
        }

        // load settins from .txt file
        private void Load()
        {
            if (!File.Exists(GetSettingsPath()))
            {
                return;
            }
            try
            {
                using (StreamReader reader = new StreamReader(GetSettingsPath()))
                {
                    _volumeType = Convert.ToString(reader.ReadLine());
                    _tradeAssetInPortfolio = Convert.ToString(reader.ReadLine());
                    _slippage = reader.ReadLine().ToDecimal();
                    _volume = reader.ReadLine().ToDecimal();
                    Enum.TryParse(reader.ReadLine(), true, out _regime);
                    reader.Close();
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        // delete file with save
        void Strategy_DeleteEvent()
        {
            if (File.Exists(GetSettingsPath()))
            {
                File.Delete(GetSettingsPath());
            }
        }

        private string GetSettingsPath()
        {
            return @"Engine\" + NameStrategyUniq + @"SettingsBot.txt";
        }

        // logic
        private void Bot_CandleFinishedEvent(List<Candle> candles)
        {

            if (_regime == BotTradeRegime.Off)
            {
                return;
            }

            if (_bollinger.DataSeries[0].Values == null ||
                _bollinger.DataSeries[0].Values.Count == 0 ||
                _bollinger.DataSeries[0].Values.Count < candles.Count ||
                _sma.DataSeries[0].Values.Count < candles.Count)
            {
                return;
            }

            List<Position> openPosition = _tab.PositionsOpenAll;

            if (openPosition != null && openPosition.Count != 0
                && candles[candles.Count - 1].TimeStart.Hour <= 18)
            {
                for (int i = 0; i < openPosition.Count; i++)
                {
                    LogicClosePosition(openPosition[i], candles);
                }
            }

            if (_regime == BotTradeRegime.OnlyClosePosition)
            {
                return;
            }

            if (openPosition == null || openPosition.Count == 0)
            {
                LogicOpenPosition(candles);
            }
        }

        // position opening logic
        private void LogicOpenPosition(List<Candle> candles)
        {
            decimal _bollingerUpLast = _bollinger.DataSeries[0].Last;

            decimal _bollingerDownLast = _bollinger.DataSeries[1].Last;

            if (_bollingerUpLast == 0 ||
                _bollingerDownLast == 0)
            {
                return;
            }

            decimal close = candles[candles.Count - 1].Close;

            if (close > _bollingerUpLast
                && _regime != BotTradeRegime.OnlyLong)
            {
                _tab.SellAtLimit(GetVolume(_tab), close - _slippage);
            }

            if (close < _bollingerDownLast
                && _regime != BotTradeRegime.OnlyShort)
            {
                _tab.BuyAtLimit(GetVolume(_tab), close + _slippage);
            }
        }

        // position closing logic
        private void LogicClosePosition(Position position, List<Candle> candles)
        {
            if (position.State == PositionStateType.Closing)
            {
                return;
            }
            decimal _lastSma = _sma.DataSeries[0].Last;

            decimal _lastClose = candles[candles.Count - 1].Close;

            if (position.Direction == Side.Buy)
            {
                if (_lastClose > _lastSma)
                {
                    _tab.CloseAtLimit(position, _lastClose - _slippage, position.OpenVolume);
                }
            }

            if (position.Direction == Side.Sell)
            {
                if (_lastClose < _lastSma)
                {
                    _tab.CloseAtLimit(position, _lastClose + _slippage, position.OpenVolume);
                }
            }
        }

        // Method for calculating the volume of entry into a position
        private decimal GetVolume(BotTabSimple tab)
        {
            decimal volume = 0;

            if (_volumeType == "Contracts")
            {
                volume = _volume;
            }
            else if (_volumeType == "Contract currency")
            {
                decimal contractPrice = tab.PriceBestAsk;
                volume = _volume / contractPrice;

                if (StartProgram == StartProgram.IsOsTrader)
                {
                    IServerPermission serverPermission = ServerMaster.GetServerPermission(tab.Connector.ServerType);

                    if (serverPermission != null &&
                        serverPermission.IsUseLotToCalculateProfit &&
                    tab.Security.Lot != 0 &&
                        tab.Security.Lot > 1)
                    {
                        volume = _volume / (contractPrice * tab.Security.Lot);
                    }

                    volume = Math.Round(volume, tab.Security.DecimalsVolume);
                }
                else // Tester or Optimizer
                {
                    volume = Math.Round(volume, 6);
                }
            }
            else if (_volumeType == "Deposit percent")
            {
                Portfolio myPortfolio = tab.Portfolio;

                if (myPortfolio == null)
                {
                    return 0;
                }

                decimal portfolioPrimeAsset = 0;

                if (_tradeAssetInPortfolio == "Prime")
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
                        if (positionOnBoard[i].SecurityNameCode == _tradeAssetInPortfolio)
                        {
                            portfolioPrimeAsset = positionOnBoard[i].ValueCurrent;
                            break;
                        }
                    }
                }

                if (portfolioPrimeAsset == 0)
                {
                    SendNewLogMessage("Can`t found portfolio " + _tradeAssetInPortfolio, Logging.LogMessageType.Error);
                    return 0;
                }

                decimal moneyOnPosition = portfolioPrimeAsset * (_volume / 100);

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
    }
}

