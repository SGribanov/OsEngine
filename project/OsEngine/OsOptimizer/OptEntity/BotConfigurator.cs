/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Threading;
using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.Market;
using OsEngine.Market.Connectors;
using OsEngine.Market.Servers.Optimizer;
using OsEngine.Market.Servers.Tester;
using OsEngine.OsOptimizer.OptimizerEntity;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;
using OsEngine.OsTrader.Panels.Tab.Internal;

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Creates and configures bots for optimization runs.
    /// Создаёт и конфигурирует ботов для оптимизационных прогонов.
    /// </summary>
    public class BotConfigurator
    {
        private readonly OptimizerSettings _settings;
        private readonly AsyncBotFactory _asyncBotFactory;
        private readonly BotManualControl _manualControl;

        public BotConfigurator(OptimizerSettings settings, AsyncBotFactory asyncBotFactory, BotManualControl manualControl)
        {
            _settings = settings;
            _asyncBotFactory = asyncBotFactory;
            _manualControl = manualControl;
        }

        /// <summary>
        /// Reference bot whose tab configuration is copied to each new bot.
        /// </summary>
        public BotPanel BotToTest { get; set; }

        public BotPanel CreateAndConfigureBot(
            string botName,
            List<IIStrategyParameter> parameters,
            List<IIStrategyParameter> parametersOptimized,
            OptimizerServer server,
            StartProgram regime,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(botName) || parameters == null)
            {
                return null;
            }

            if (server == null)
            {
                SendLogMessage("CreateAndConfigureBot skipped: server is null.", LogMessageType.Error);
                return null;
            }

            if (BotToTest == null)
            {
                SendLogMessage("CreateAndConfigureBot skipped: BotToTest is null.", LogMessageType.Error);
                return null;
            }

            BotPanel bot = null;

            try
            {
                bot = _asyncBotFactory.GetBot(_settings.StrategyName, botName, cancellationToken);

                if (bot == null)
                {
                    return null;
                }

                if (bot.Parameters.Count != parameters.Count)
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                SendLogMessage(ex.ToString(), LogMessageType.Error);
                return null;
            }

            try
            {
                ApplyParameters(bot, parameters, parametersOptimized);
            }
            catch (Exception ex)
            {
                SendLogMessage(ex.ToString(), LogMessageType.Error);
                return null;
            }

            try
            {
                CopyTabSources(bot, server);
                return bot;
            }
            catch (Exception ex)
            {
                SendLogMessage(ex.ToString(), LogMessageType.Error);
                return null;
            }
        }

        private void ApplyParameters(BotPanel bot, List<IIStrategyParameter> parameters, List<IIStrategyParameter> parametersOptimized)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                IIStrategyParameter par = null;

                if (parametersOptimized != null)
                {
                    par = parametersOptimized.Find(p => p.Name == parameters[i].Name);
                }
                bool isInOptimizeParameters = true;

                if (par == null)
                {
                    isInOptimizeParameters = false;
                    par = parameters[i];
                }

                if (par == null)
                {
                    continue;
                }

                if (par.Type == StrategyParameterType.Bool)
                {
                    ((StrategyParameterBool)bot.Parameters[i]).ValueBool = ((StrategyParameterBool)par).ValueBool;
                }
                else if (par.Type == StrategyParameterType.String)
                {
                    ((StrategyParameterString)bot.Parameters[i]).ValueString = ((StrategyParameterString)par).ValueString;
                }
                else if (par.Type == StrategyParameterType.TimeOfDay)
                {
                    ((StrategyParameterTimeOfDay)bot.Parameters[i]).Value = ((StrategyParameterTimeOfDay)par).Value;
                }
                else if (par.Type == StrategyParameterType.CheckBox)
                {
                    ((StrategyParameterCheckBox)bot.Parameters[i]).CheckState = ((StrategyParameterCheckBox)par).CheckState;
                }

                if (isInOptimizeParameters || parametersOptimized == null)
                {
                    if (par.Type == StrategyParameterType.Int)
                    {
                        ((StrategyParameterInt)bot.Parameters[i]).ValueInt = ((StrategyParameterInt)par).ValueInt;
                    }
                    else if (par.Type == StrategyParameterType.Decimal)
                    {
                        ((StrategyParameterDecimal)bot.Parameters[i]).ValueDecimal = ((StrategyParameterDecimal)par).ValueDecimal;
                    }
                    else if (par.Type == StrategyParameterType.DecimalCheckBox)
                    {
                        ((StrategyParameterDecimalCheckBox)bot.Parameters[i]).ValueDecimal = ((StrategyParameterDecimalCheckBox)par).ValueDecimal;
                        ((StrategyParameterDecimalCheckBox)bot.Parameters[i]).CheckState = ((StrategyParameterDecimalCheckBox)par).CheckState;
                    }
                }
                else
                {
                    if (par.Type == StrategyParameterType.Int)
                    {
                        ((StrategyParameterInt)bot.Parameters[i]).ValueInt = ((StrategyParameterInt)par).ValueIntDefolt;
                    }
                    else if (par.Type == StrategyParameterType.Decimal)
                    {
                        ((StrategyParameterDecimal)bot.Parameters[i]).ValueDecimal = ((StrategyParameterDecimal)par).ValueDecimalDefolt;
                    }
                    else if (par.Type == StrategyParameterType.DecimalCheckBox)
                    {
                        ((StrategyParameterDecimalCheckBox)bot.Parameters[i]).ValueDecimal = ((StrategyParameterDecimalCheckBox)par).ValueDecimalDefolt;
                        ((StrategyParameterDecimalCheckBox)bot.Parameters[i]).CheckState = ((StrategyParameterDecimalCheckBox)par).CheckState;
                    }
                }
            }
        }

        private void CopyTabSources(BotPanel bot, OptimizerServer server)
        {
            List<IIBotTab> sourcesFrom = BotToTest.GetTabs();
            List<IIBotTab> sourcesTo = bot.GetTabs();

            for (int i = 0; i < sourcesFrom.Count; i++)
            {
                if (sourcesFrom[i].TabType == BotTabType.Simple)
                {
                    BotTabSimple simpleFrom = (BotTabSimple)sourcesFrom[i];
                    BotTabSimple simpleTo = (BotTabSimple)sourcesTo[i];
                    CopySettingsInBotTabSimpleSource(simpleFrom, simpleTo, server);
                }
                else if (sourcesFrom[i].TabType == BotTabType.Index)
                {
                    BotTabIndex indexFrom = (BotTabIndex)sourcesFrom[i];
                    BotTabIndex indexTo = (BotTabIndex)sourcesTo[i];

                    for (int i2 = 0; i2 < indexFrom.Tabs.Count; i2++)
                    {
                        indexTo.CreateNewSecurityConnector();

                        ConnectorCandles indexConnectorFrom = indexFrom.Tabs[i2];
                        ConnectorCandles indexConnectorTo = indexTo.Tabs[i2];

                        CopySettingsInConnectorCandlesSource(indexConnectorFrom, indexConnectorTo, server);
                    }

                    indexTo.AutoFormulaBuilder.DayOfWeekToRebuildIndex = indexFrom.AutoFormulaBuilder.DayOfWeekToRebuildIndex;
                    indexTo.AutoFormulaBuilder.DaysLookBackInBuilding = indexFrom.AutoFormulaBuilder.DaysLookBackInBuilding;
                    indexTo.AutoFormulaBuilder.HourInDayToRebuildIndex = indexFrom.AutoFormulaBuilder.HourInDayToRebuildIndex;
                    indexTo.AutoFormulaBuilder.IndexMultType = indexFrom.AutoFormulaBuilder.IndexMultType;
                    indexTo.AutoFormulaBuilder.IndexSecCount = indexFrom.AutoFormulaBuilder.IndexSecCount;
                    indexTo.AutoFormulaBuilder.IndexSortType = indexFrom.AutoFormulaBuilder.IndexSortType;
                    indexTo.AutoFormulaBuilder.Regime = indexFrom.AutoFormulaBuilder.Regime;
                    indexTo.AutoFormulaBuilder.WriteLogMessageOnRebuild = false;
                    indexTo.UserFormula = indexFrom.UserFormula;
                }
                else if (sourcesFrom[i].TabType == BotTabType.Screener)
                {
                    BotTabScreener screenerFrom = (BotTabScreener)sourcesFrom[i];
                    BotTabScreener screenerTo = (BotTabScreener)sourcesTo[i];

                    CopySettingsInScreenerSource(screenerFrom, screenerTo, server);
                    screenerTo.TryLoadTabs();
                    screenerTo.NeedToReloadTabs = true;
                    screenerTo.TryReLoadTabs();
                    screenerTo.ReloadIndicatorsOnTabs();
                }
            }
        }

        private void CopySettingsInScreenerSource(BotTabScreener from, BotTabScreener to, OptimizerServer server)
        {
            to.ServerType = ServerType.Optimizer;
            to.PortfolioName = server.Portfolios[0].Number;
            to.TimeFrame = from.TimeFrame;
            to.ServerUid = server.NumberServer;

            to.SecuritiesClass = from.SecuritiesClass;
            to.CandleCreateMethodType = from.CandleCreateMethodType;
            to.CandleMarketDataType = from.CandleMarketDataType;
            to.CommissionType = _settings.CommissionType;
            to.CommissionValue = _settings.CommissionValue;
            to.SaveTradesInCandles = from.SaveTradesInCandles;
            to.CandleSeriesRealization.SetSaveString(from.CandleSeriesRealization.GetSaveString());

            for (int i = 0; i < from.SecuritiesNames.Count; i++)
            {
                ActivatedSecurity sec = from.SecuritiesNames[i];
                to.SecuritiesNames.Add(sec);
            }
        }

        private void CopySettingsInConnectorCandlesSource(ConnectorCandles from, ConnectorCandles to, OptimizerServer server)
        {
            to.ServerType = ServerType.Optimizer;
            to.PortfolioName = server.Portfolios[0].Number;
            to.SecurityName = from.SecurityName;
            to.SecurityClass = from.SecurityClass;
            to.ServerUid = server.NumberServer;
            to.CandleCreateMethodType = from.CandleCreateMethodType;
            to.TimeFrame = from.TimeFrame;

            to.TimeFrameBuilder.CandleSeriesRealization.SetSaveString(
                 from.TimeFrameBuilder.CandleSeriesRealization.GetSaveString());

            if (server.TypeTesterData == TesterDataType.Candle)
            {
                to.CandleMarketDataType = CandleMarketDataType.Tick;
            }
            else if (server.TypeTesterData == TesterDataType.MarketDepthAllCandleState ||
                     server.TypeTesterData == TesterDataType.MarketDepthOnlyReadyCandle)
            {
                to.CandleMarketDataType = CandleMarketDataType.MarketDepth;
            }
        }

        private void CopySettingsInBotTabSimpleSource(BotTabSimple from, BotTabSimple to, OptimizerServer server)
        {
            to.Connector.ServerType = ServerType.Optimizer;
            to.Connector.PortfolioName = server.Portfolios[0].Number;
            to.Connector.SecurityName = from.Connector.SecurityName;
            to.Connector.TimeFrame = from.Connector.TimeFrame;

            to.Connector.CandleCreateMethodType = from.Connector.CandleCreateMethodType;

            to.Connector.TimeFrameBuilder.CandleSeriesRealization.SetSaveString(
                from.Connector.TimeFrameBuilder.CandleSeriesRealization.GetSaveString());

            to.Connector.ServerUid = server.NumberServer;
            to.CommissionType = _settings.CommissionType;
            to.CommissionValue = _settings.CommissionValue;

            if (server.TypeTesterData == TesterDataType.Candle
                || server.TypeTesterData == TesterDataType.TickAllCandleState
                || server.TypeTesterData == TesterDataType.TickOnlyReadyCandle)
            {
                to.Connector.CandleMarketDataType = CandleMarketDataType.Tick;
            }
            else if (server.TypeTesterData == TesterDataType.MarketDepthAllCandleState ||
                     server.TypeTesterData == TesterDataType.MarketDepthOnlyReadyCandle)
            {
                to.Connector.CandleMarketDataType = CandleMarketDataType.MarketDepth;
            }

            to.ManualPositionSupport.DoubleExitIsOn = _manualControl.DoubleExitIsOn;
            to.ManualPositionSupport.DoubleExitSlippage = _manualControl.DoubleExitSlippage;

            to.ManualPositionSupport.ProfitDistance = _manualControl.ProfitDistance;
            to.ManualPositionSupport.ProfitIsOn = _manualControl.ProfitIsOn;
            to.ManualPositionSupport.ProfitSlippage = _manualControl.ProfitSlippage;

            to.ManualPositionSupport.SecondToCloseIsOn = _manualControl.SecondToCloseIsOn;
            to.ManualPositionSupport.SecondToClose = _manualControl.SecondToClose;

            to.ManualPositionSupport.SecondToOpenIsOn = _manualControl.SecondToOpenIsOn;
            to.ManualPositionSupport.SecondToOpen = _manualControl.SecondToOpen;

            to.ManualPositionSupport.SetbackToCloseIsOn = _manualControl.SetbackToCloseIsOn;
            to.ManualPositionSupport.SetbackToClosePosition = _manualControl.SetbackToClosePosition;

            to.ManualPositionSupport.SetbackToOpenIsOn = _manualControl.SetbackToOpenIsOn;
            to.ManualPositionSupport.SetbackToOpenPosition = _manualControl.SetbackToOpenPosition;

            to.ManualPositionSupport.StopDistance = _manualControl.StopDistance;
            to.ManualPositionSupport.StopIsOn = _manualControl.StopIsOn;
            to.ManualPositionSupport.StopSlippage = _manualControl.StopSlippage;

            to.ManualPositionSupport.ProfitDistance = _manualControl.ProfitDistance;
            to.ManualPositionSupport.ProfitIsOn = _manualControl.ProfitIsOn;
            to.ManualPositionSupport.ProfitSlippage = _manualControl.ProfitSlippage;

            to.ManualPositionSupport.TypeDoubleExitOrder = _manualControl.TypeDoubleExitOrder;
            to.ManualPositionSupport.ValuesType = _manualControl.ValuesType;

            to.ManualPositionSupport.OrderTypeTime = _manualControl.OrderTypeTime;
        }

        public void CopyManualSupportSettings(BotManualControl manualControlTo)
        {
            manualControlTo.DoubleExitIsOn = _manualControl.DoubleExitIsOn;
            manualControlTo.DoubleExitSlippage = _manualControl.DoubleExitSlippage;
            manualControlTo.OrderTypeTime = _manualControl.OrderTypeTime;
            manualControlTo.ProfitDistance = _manualControl.ProfitDistance;
            manualControlTo.ProfitIsOn = _manualControl.ProfitIsOn;
            manualControlTo.ProfitSlippage = _manualControl.ProfitSlippage;
            manualControlTo.SecondToClose = _manualControl.SecondToClose;
            manualControlTo.SecondToCloseIsOn = _manualControl.SecondToCloseIsOn;
            manualControlTo.SecondToOpen = _manualControl.SecondToOpen;
            manualControlTo.SecondToOpenIsOn = _manualControl.SecondToOpenIsOn;
            manualControlTo.SetbackToCloseIsOn = _manualControl.SetbackToCloseIsOn;
            manualControlTo.SetbackToClosePosition = _manualControl.SetbackToClosePosition;
            manualControlTo.SetbackToOpenIsOn = _manualControl.SetbackToOpenIsOn;
            manualControlTo.SetbackToOpenPosition = _manualControl.SetbackToOpenPosition;
            manualControlTo.StopDistance = _manualControl.StopDistance;
            manualControlTo.StopIsOn = _manualControl.StopIsOn;
            manualControlTo.StopSlippage = _manualControl.StopSlippage;
            manualControlTo.TypeDoubleExitOrder = _manualControl.TypeDoubleExitOrder;
            manualControlTo.ValuesType = _manualControl.ValuesType;
        }

        private void SendLogMessage(string message, LogMessageType type)
        {
            LogMessageEvent?.Invoke(message, type);
        }

        public event Action<string, LogMessageType> LogMessageEvent;
    }
}
