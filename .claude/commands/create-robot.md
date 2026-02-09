---
description: Create a new OsEngine trading robot following all project conventions
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
user-intent: User wants to create a new trading robot (bot) for the OsEngine platform
---

# Create OsEngine Trading Robot

You are creating a new trading robot for the OsEngine algorithmic trading platform. Follow ALL conventions below strictly. The robots in this project follow a very consistent pattern - do not deviate from it.

## Step 1: Clarify Requirements

Before writing code, confirm with the user:
1. **Strategy name** - PascalCase, descriptive (e.g., `BollingerRsiCountertrend`)
2. **Strategy type** - Trend, CounterTrend, Screener, PairArbitrage, Grid, etc.
3. **Which indicators** to use
4. **Entry conditions** for Buy and Sell
5. **Exit conditions**
6. **Tab type** - Simple (single security), Screener (multi-security), Pair, etc.
7. **Deployment** - Compiled (in `project/OsEngine/Robots/{Category}/`) or Script (in `bin/Debug/Custom/Robots/`)

## Step 2: File Structure

### For compiled robots:
- File: `project/OsEngine/Robots/{Category}/{RobotName}.cs`
- Namespace: `OsEngine.Robots.{Category}`
- Must have `[Bot("RobotName")]` attribute

### For script robots:
- File: `project/OsEngine/bin/Debug/Custom/Robots/{RobotName}.cs`
- Namespace: `OsEngine.Robots`
- `[Bot("RobotName")]` attribute optional but recommended

## Step 3: Generate Code

Use this exact template structure:

```csharp
/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Entity;
using OsEngine.Indicators;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Attributes;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;

/* Description
trading robot for osengine

{STRATEGY_DESCRIPTION}

Buy:
{BUY_CONDITIONS}

Sell:
{SELL_CONDITIONS}

Exit:
{EXIT_CONDITIONS}
*/

namespace OsEngine.Robots.{CATEGORY}
{
    [Bot("{ROBOT_NAME}")]
    public class {ROBOT_NAME} : BotPanel
    {
        private BotTabSimple _tab;

        // Basic settings
        private StrategyParameterString _regime;
        private StrategyParameterDecimal _slippage;
        private StrategyParameterTimeOfDay _startTradeTime;
        private StrategyParameterTimeOfDay _endTradeTime;

        // GetVolume settings
        private StrategyParameterString _volumeType;
        private StrategyParameterDecimal _volume;
        private StrategyParameterString _tradeAssetInPortfolio;

        // Indicator settings
        // {INDICATOR_PARAMETER_FIELDS}

        // Indicators
        // {INDICATOR_FIELDS}

        public {ROBOT_NAME}(string name, StartProgram startProgram) : base(name, startProgram)
        {
            // 1. Create tab
            TabCreate(BotTabType.Simple);
            _tab = TabsSimple[0];

            // 2. Basic settings
            _regime = CreateParameter("Regime", "Off", new[] { "Off", "On", "OnlyLong", "OnlyShort", "OnlyClosePosition" }, "Base");
            _slippage = CreateParameter("Slippage %", 0m, 0, 20, 1, "Base");
            _startTradeTime = CreateParameterTimeOfDay("Start Trade Time", 0, 0, 0, 0, "Base");
            _endTradeTime = CreateParameterTimeOfDay("End Trade Time", 24, 0, 0, 0, "Base");

            // 3. Volume settings
            _volumeType = CreateParameter("Volume type", "Deposit percent", new[] { "Contracts", "Contract currency", "Deposit percent" });
            _volume = CreateParameter("Volume", 20, 1.0m, 50, 4);
            _tradeAssetInPortfolio = CreateParameter("Asset in portfolio", "Prime");

            // 4. Indicator settings
            // {CREATE_INDICATOR_PARAMETERS}

            // 5. Create indicators
            // {CREATE_INDICATORS}
            // Pattern:
            // _indicator = IndicatorsFactory.CreateIndicatorByName("Sma", name + "Sma", false);
            // _indicator = (Aindicator)_tab.CreateCandleIndicator(_indicator, "Prime");
            // ((IndicatorParameterInt)_indicator.Parameters[0]).ValueInt = _length.ValueInt;
            // _indicator.Save();

            // 6. Subscribe to events
            _tab.CandleFinishedEvent += _tab_CandleFinishedEvent;
            ParametrsChangeByUser += OnParametersChanged;

            // 7. Description
            Description = "{DESCRIPTION_TEXT}";
        }

        private void OnParametersChanged()
        {
            // Sync indicator parameters with strategy parameters
            // ((IndicatorParameterInt)_indicator.Parameters[0]).ValueInt = _length.ValueInt;
            // _indicator.Save();
            // _indicator.Reload();
        }

        public override string GetNameStrategyType()
        {
            return "{ROBOT_NAME}";
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

            // Check sufficient data for indicators
            // if (candles.Count < _length.ValueInt) return;
            // if (_indicator.DataSeries[0].Values == null) return;

            // Check trading time window
            if (_startTradeTime.Value > _tab.TimeServerCurrent ||
                _endTradeTime.Value < _tab.TimeServerCurrent)
            {
                return;
            }

            List<Position> openPositions = _tab.PositionsOpenAll;

            if (openPositions != null && openPositions.Count != 0)
            {
                LogicClosePosition(candles, openPositions[0]);
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
            decimal lastPrice = candles[candles.Count - 1].Close;
            decimal slippage = _slippage.ValueDecimal * _tab.Securiti.PriceStep;

            // {OPEN_LOGIC}

            // Long example:
            // if (_regime.ValueString != "OnlyShort")
            // {
            //     if ({BUY_CONDITION})
            //     {
            //         _tab.BuyAtLimit(GetVolume(_tab), lastPrice + slippage);
            //     }
            // }

            // Short example:
            // if (_regime.ValueString != "OnlyLong")
            // {
            //     if ({SELL_CONDITION})
            //     {
            //         _tab.SellAtLimit(GetVolume(_tab), lastPrice - slippage);
            //     }
            // }
        }

        private void LogicClosePosition(List<Candle> candles, Position position)
        {
            decimal lastPrice = candles[candles.Count - 1].Close;
            decimal slippage = _slippage.ValueDecimal * _tab.Securiti.PriceStep;

            if (position.State != PositionStateType.Open)
            {
                return;
            }

            // {CLOSE_LOGIC}
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
```

## Key Rules

1. **GetVolume method is boilerplate** - copy it exactly as shown, it handles Contracts / Contract currency / Deposit percent modes plus MOEX futures sizing
2. **Always check `_regime` first** in the candle event handler
3. **Always validate indicator data** before using (null check, count check)
4. **Use `decimal` for all prices and volumes**, never `double`
5. **Indicator areas**: `"Prime"` for main chart, `"NewArea0"`, `"NewArea1"` etc. for sub-areas, or custom names like `"RsiArea"`
6. **Slippage**: calculate as `_slippage.ValueDecimal * _tab.Securiti.PriceStep` (note: `Securiti` not `Security` - this is a property on BotTabSimple)
7. **Order types**: Use `BuyAtLimit`/`SellAtLimit` for limit orders, `BuyAtMarket`/`SellAtMarket` for market orders
8. **Close types**: `CloseAtLimit`, `CloseAtMarket`, `CloseAtTrailingStop`, `CloseAtTrailingStopMarket`, `CloseAtStop`, `CloseAtProfit`
9. **Position direction check**: `position.Direction == Side.Buy` or `Side.Sell`
10. **Position state check**: `position.State == PositionStateType.Open` before closing
11. **Respect regime restrictions**: check `OnlyLong`/`OnlyShort` before opening
12. **Include the license header** at the top of every file

## Available Indicators

Common built-in indicator names for `IndicatorsFactory.CreateIndicatorByName()`:
Sma, Ema, Ssma, Vwma, RSI, MACD, Bollinger, Stochastic, ATR, CCI, PriceChannel, ADX, Alligator, ParabolicSAR, Momentum, OBV, Volume, ZigZag, Envelops, LinearRegressionChannel, and many more.

## For Screener Robots

Use `BotTabType.Screener` with `TabsScreener[0]`. The CandleFinishedEvent provides both `List<Candle>` and `BotTabSimple tab`. Create indicators via `_screenerTab.CreateCandleIndicator(index, "Sma", params, "Prime")`. Access them via `tab.Indicators[index]`.

## For Pair Trading Robots

Use `BotTabType.Pair` with `TabsPair[0]`. Subscribe to `CorrelationChangeEvent`. Use `pair.BuySec1SellSec2()` / `pair.SellSec1BuySec2()` / `pair.ClosePositions()`.
