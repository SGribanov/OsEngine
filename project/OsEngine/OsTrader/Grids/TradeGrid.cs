#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Logging;
using OsEngine.Market;
using OsEngine.Market.Servers;
using OsEngine.OsTrader.Panels.Tab;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace OsEngine.OsTrader.Grids
{
    public class TradeGrid
    {
        #region Service

        public TradeGrid(StartProgram startProgram, BotTabSimple tab, int number)
        {
            Tab = tab;
            Number = number;

            if (Tab.ManualPositionSupport != null)
            {
                Tab.ManualPositionSupport.DisableManualSupport();
            }

            Tab.NewTickEvent += Tab_NewTickEvent;
            Tab.PositionOpeningSuccesEvent += Tab_PositionOpeningSuccesEvent;
            Tab.PositionClosingSuccesEvent += Tab_PositionClosingSuccesEvent;
            Tab.PositionStopActivateEvent += Tab_PositionStopActivateEvent;
            Tab.Connector.TestStartEvent += Connector_TestStartEvent;

            Tab.PositionOpeningFailEvent += Tab_PositionOpeningFailEvent;
            Tab.PositionClosingFailEvent += Tab_PositionClosingFailEvent;

            StartProgram = startProgram;

            NonTradePeriods = new TradeGridNonTradePeriods(tab.TabName + "Grid" + number);
            NonTradePeriods.LogMessageEvent += SendNewLogMessage;

            StopBy = new TradeGridStopBy();
            StopBy.LogMessageEvent += SendNewLogMessage;

            StopAndProfit = new TradeGridStopAndProfit();
            StopAndProfit.LogMessageEvent += SendNewLogMessage;

            AutoStarter = new TradeGridAutoStarter();
            AutoStarter.LogMessageEvent += SendNewLogMessage;

            GridCreator = new TradeGridCreator();
            GridCreator.LogMessageEvent += SendNewLogMessage;

            ErrorsReaction = new TradeGridErrorsReaction(this);
            ErrorsReaction.LogMessageEvent += SendNewLogMessage;

            TrailingUp = new TrailingUp(this);
            TrailingUp.LogMessageEvent += SendNewLogMessage;

            if (StartProgram == StartProgram.IsOsTrader)
            {
                Thread worker = new Thread(ThreadWorkerPlace);
                worker.Name = "GridThread." + tab.TabName;
                worker.IsBackground = true;
                worker.Start();

                RegimeLogicEntry = TradeGridLogicEntryRegime.OncePerSecond;
                AutoClearJournalIsOn = true;
            }
            else
            {
                RegimeLogicEntry = TradeGridLogicEntryRegime.OnTrade;
                AutoClearJournalIsOn = false;
            }
        }

        public StartProgram StartProgram;

        public int Number;

        public BotTabSimple Tab;

        public TradeGridNonTradePeriods NonTradePeriods;

        public TradeGridStopBy StopBy;

        public TradeGridStopAndProfit StopAndProfit;

        public TradeGridAutoStarter AutoStarter;

        public TradeGridCreator GridCreator;

        public TradeGridErrorsReaction ErrorsReaction;

        public TrailingUp TrailingUp;

        public string GetSaveString()
        {
            TradeGridNonTradePeriods nonTradePeriods = NonTradePeriods;
            TradeGridStopBy stopBy = StopBy;
            TradeGridCreator gridCreator = GridCreator;
            TradeGridStopAndProfit stopAndProfit = StopAndProfit;
            TradeGridAutoStarter autoStarter = AutoStarter;
            TradeGridErrorsReaction errorsReaction = ErrorsReaction;
            TrailingUp trailingUp = TrailingUp;

            string result = "";

            // settings prime

            result += Number.ToString(CultureInfo.InvariantCulture) + "@";
            result += GridType + "@";
            result += Regime + "@";
            result += RegimeLogicEntry + "@";
            result += AutoClearJournalIsOn + "@";
            result += MaxClosePositionsInJournal.ToString(CultureInfo.InvariantCulture) + "@";
            result += MaxOpenOrdersInMarket.ToString(CultureInfo.InvariantCulture) + "@";
            result += MaxCloseOrdersInMarket.ToString(CultureInfo.InvariantCulture) + "@";
            result += _firstTradePrice.ToString(CultureInfo.InvariantCulture) + "@";
            result += _openPositionsBySession.ToString(CultureInfo.InvariantCulture) + "@";
            result += _firstTradeTime.ToString("O", CultureInfo.InvariantCulture) + "@";
            result += DelayInReal.ToString(CultureInfo.InvariantCulture) + "@";
            result += CheckMicroVolumes + "@";
            result += MaxDistanceToOrdersPercent.ToString(CultureInfo.InvariantCulture) + "@";
            result += OpenOrdersMakerOnly + "@";
            result += "@";
            result += "@";

            result += "%";

            // non trade periods
            result += nonTradePeriods?.GetSaveString() ?? string.Empty;
            result += "%";

            // trade days
            result += "";
            result += "%";

            // stop grid by event
            result += stopBy?.GetSaveString() ?? string.Empty;
            result += "%";

            // grid lines creation and storage
            result += gridCreator?.GetSaveString() ?? string.Empty;
            result += "%";

            // stop and profit 
            result += stopAndProfit?.GetSaveString() ?? string.Empty;
            result += "%";

            // auto start
            result += autoStarter?.GetSaveString() ?? string.Empty;
            result += "%";

            // errors reaction
            result += errorsReaction?.GetSaveString() ?? string.Empty;
            result += "%";

            // trailing up / down
            result += trailingUp?.GetSaveString() ?? string.Empty;
            result += "%";

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

                string[] array = value.Split('%');

                if (array.Length == 0 || string.IsNullOrWhiteSpace(array[0]))
                {
                    return;
                }

                string[] values = array[0].Split('@');

                // settings prime

                if (values.Length > 0 && string.IsNullOrWhiteSpace(values[0]) == false)
                {
                    Number = Convert.ToInt32(values[0], CultureInfo.InvariantCulture);
                }
                if (values.Length > 1 && string.IsNullOrWhiteSpace(values[1]) == false)
                {
                    Enum.TryParse(values[1], out GridType);
                }
                if (values.Length > 2 && string.IsNullOrWhiteSpace(values[2]) == false)
                {
                    Enum.TryParse(values[2], out _regime);
                }
                if (values.Length > 3 && string.IsNullOrWhiteSpace(values[3]) == false)
                {
                    Enum.TryParse(values[3], out RegimeLogicEntry);
                }
                if (values.Length > 4 && string.IsNullOrWhiteSpace(values[4]) == false)
                {
                    AutoClearJournalIsOn = Convert.ToBoolean(values[4]);
                }
                if (values.Length > 5 && string.IsNullOrWhiteSpace(values[5]) == false)
                {
                    MaxClosePositionsInJournal = Convert.ToInt32(values[5], CultureInfo.InvariantCulture);
                }
                if (values.Length > 6 && string.IsNullOrWhiteSpace(values[6]) == false)
                {
                    MaxOpenOrdersInMarket = Convert.ToInt32(values[6], CultureInfo.InvariantCulture);
                }
                if (values.Length > 7 && string.IsNullOrWhiteSpace(values[7]) == false)
                {
                    MaxCloseOrdersInMarket = Convert.ToInt32(values[7], CultureInfo.InvariantCulture);
                }
                if (values.Length > 8 && string.IsNullOrWhiteSpace(values[8]) == false)
                {
                    _firstTradePrice = values[8].ToDecimal();
                }
                if (values.Length > 9 && string.IsNullOrWhiteSpace(values[9]) == false)
                {
                    _openPositionsBySession = Convert.ToInt32(values[9], CultureInfo.InvariantCulture);
                }
                if (values.Length > 10 && string.IsNullOrWhiteSpace(values[10]) == false)
                {
                    _firstTradeTime = ParseDateInvariantOrCurrent(values[10]);
                }

                try
                {
                    if (values.Length <= 12
                        || string.IsNullOrWhiteSpace(values[11])
                        || string.IsNullOrWhiteSpace(values[12]))
                    {
                        throw new FormatException("Legacy payload has no delay/micro-volume tail.");
                    }

                    if (values.Length > 11)
                    {
                        DelayInReal = Convert.ToInt32(values[11], CultureInfo.InvariantCulture);
                    }
                    if (values.Length > 12)
                    {
                        CheckMicroVolumes = Convert.ToBoolean(values[12]);
                    }
                }
                catch
                {
                    DelayInReal = 500;
                    CheckMicroVolumes = true;
                }

                try
                {
                    if (values.Length <= 13 || string.IsNullOrWhiteSpace(values[13]))
                    {
                        throw new FormatException("Legacy payload has no max-distance tail.");
                    }

                    MaxDistanceToOrdersPercent = values[13].ToDecimal();
                }
                catch
                {
                    MaxDistanceToOrdersPercent = 1.5m;
                }

                try
                {
                    if (values.Length <= 14 || string.IsNullOrWhiteSpace(values[14]))
                    {
                        throw new FormatException("Legacy payload has no maker-only tail.");
                    }

                    OpenOrdersMakerOnly = Convert.ToBoolean(values[14]);
                }
                catch
                {
                    OpenOrdersMakerOnly = true;
                }

                // non trade periods
                if (array.Length > 1 && NonTradePeriods != null)
                {
                    NonTradePeriods.LoadFromString(array[1]);
                }

                // trade days
                // removed

                // stop grid by event
                if (array.Length > 3 && StopBy != null)
                {
                    StopBy.LoadFromString(array[3]);
                }

                // grid lines creation and storage
                if (array.Length > 4 && GridCreator != null)
                {
                    GridCreator.LoadFromString(array[4]);
                }

                // stop and profit 
                if (array.Length > 5 && StopAndProfit != null)
                {
                    StopAndProfit.LoadFromString(array[5]);
                }

                // auto start
                if (array.Length > 6 && AutoStarter != null)
                {
                    AutoStarter.LoadFromString(array[6]);
                }

                // errors reaction
                if (array.Length > 7 && ErrorsReaction != null)
                {
                    ErrorsReaction.LoadFromString(array[7]);
                }

                // trailing up / down
                if (array.Length > 8 && TrailingUp != null)
                {
                    TrailingUp.LoadFromString(array[8]);
                }
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        private static DateTime ParseDateInvariantOrCurrent(string value)
        {
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsed))
            {
                return parsed;
            }

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                return parsed;
            }

            if (DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out parsed))
            {
                return parsed;
            }

            if (DateTime.TryParse(value, new CultureInfo("ru-RU"), DateTimeStyles.None, out parsed))
            {
                return parsed;
            }

            return DateTime.MinValue;
        }

        public void Delete()
        {
            _isDeleted = true;

            if (Tab != null)
            {
                Tab.NewTickEvent -= Tab_NewTickEvent;
                Tab.PositionOpeningSuccesEvent -= Tab_PositionOpeningSuccesEvent;
                Tab.PositionClosingSuccesEvent -= Tab_PositionClosingSuccesEvent;
                Tab.PositionStopActivateEvent -= Tab_PositionStopActivateEvent;
                Tab.Connector.TestStartEvent -= Connector_TestStartEvent;
                Tab.PositionOpeningFailEvent -= Tab_PositionOpeningFailEvent;
                Tab.PositionClosingFailEvent -= Tab_PositionClosingFailEvent;

                Tab = null;
            }

            if (NonTradePeriods != null)
            {
                NonTradePeriods.LogMessageEvent -= SendNewLogMessage;
                NonTradePeriods.Delete();
                NonTradePeriods = null;
            }

            if (StopBy != null)
            {
                StopBy.LogMessageEvent -= SendNewLogMessage;
                StopBy = null;
            }

            if (StopAndProfit != null)
            {
                StopAndProfit.LogMessageEvent -= SendNewLogMessage;
                StopAndProfit = null;
            }

            if (AutoStarter != null)
            {
                AutoStarter.LogMessageEvent -= SendNewLogMessage;
                AutoStarter = null;
            }

            if (GridCreator != null)
            {
                GridCreator.LogMessageEvent -= SendNewLogMessage;
                GridCreator = null;
            }

            if (ErrorsReaction != null)
            {
                ErrorsReaction.LogMessageEvent -= SendNewLogMessage;
                ErrorsReaction.Delete();
                ErrorsReaction = null;
            }

            if (TrailingUp != null)
            {
                TrailingUp.LogMessageEvent -= SendNewLogMessage;
                TrailingUp.Delete();
                TrailingUp = null;
            }
        }

        public void Save()
        {
            NeedToSaveEvent?.Invoke();
        }

        public void RePaintGrid()
        {
            RePaintSettingsEvent?.Invoke();
        }

        public void FullRePaintGrid()
        {
            FullRePaintGridEvent?.Invoke();
        }

        private void Connector_TestStartEvent()
        {
            try
            {
                TradeGridCreator gridCreator = GridCreator;
                if (gridCreator == null)
                {
                    return;
                }

                List<TradeGridLine> lines = gridCreator.Lines;

                if (lines == null)
                {
                    return;
                }

                for (int i = 0; i < lines.Count; i++)
                {
                    lines[i].Position = null;
                    lines[i].PositionNum = 0;
                }
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        private void Tab_PositionClosingFailEvent(Position position)
        {
            try
            {
                if (Regime != TradeGridRegime.Off)
                {
                    TradeGridCreator gridCreator = GridCreator;
                    if (gridCreator == null || gridCreator.Lines == null)
                    {
                        return;
                    }

                    bool isInArray = false;

                    for (int i = 0; i < gridCreator.Lines.Count; i++)
                    {
                        TradeGridLine line = gridCreator.Lines[i];

                        if (line.Position != null
                            && line.Position.Number == position.Number)
                        {
                            isInArray = true;
                            break;
                        }
                    }

                    if (isInArray)
                    {
                        TradeGridErrorsReaction errorsReaction = ErrorsReaction;
                        if (errorsReaction == null)
                        {
                            return;
                        }

                        errorsReaction.PositionClosingFailEvent(position);
                    }
                }
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        private void Tab_PositionOpeningFailEvent(Position position)
        {
            try
            {
                if (Regime != TradeGridRegime.Off)
                {
                    TradeGridCreator gridCreator = GridCreator;
                    if (gridCreator == null || gridCreator.Lines == null)
                    {
                        return;
                    }

                    bool isInArray = false;

                    for (int i = 0; i < gridCreator.Lines.Count; i++)
                    {
                        TradeGridLine line = gridCreator.Lines[i];

                        if (line.Position != null
                            && line.Position.Number == position.Number)
                        {
                            isInArray = true;
                            break;
                        }
                    }

                    if (isInArray)
                    {
                        TradeGridErrorsReaction errorsReaction = ErrorsReaction;
                        if (errorsReaction == null)
                        {
                            return;
                        }

                        errorsReaction.PositionOpeningFailEvent(position);
                    }
                }
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        public event Action? NeedToSaveEvent;

        public event Action? RePaintSettingsEvent;

        public event Action? FullRePaintGridEvent;

        #endregion

        #region Settings Prime

        public TradeGridPrimeType GridType;

        public TradeGridRegime Regime
        {
            get
            {
                return _regime;
            }
            set
            {
                if (_regime == value)
                {
                    return;
                }

                _regime = value;

                FullRePaintGridEvent?.Invoke();
                RePaintSettingsEvent?.Invoke();
            }
        }
        private TradeGridRegime _regime;

        public TradeGridLogicEntryRegime RegimeLogicEntry;

        public bool AutoClearJournalIsOn;

        public int MaxClosePositionsInJournal = 100;

        public int MaxOpenOrdersInMarket = 5;

        public int MaxCloseOrdersInMarket = 5;

        public int DelayInReal = 500;

        public bool CheckMicroVolumes = true;

        public decimal MaxDistanceToOrdersPercent = 0;

        public bool OpenOrdersMakerOnly = true;

        #endregion

        #region Grid managment

        public void CreateNewGridSafe()
        {
            try
            {
                TradeGridCreator gridCreator = GridCreator;
                BotTabSimple tab = Tab;

                if (gridCreator == null || tab == null)
                {
                    return;
                }

                if (Regime != TradeGridRegime.Off &&
                    gridCreator.Lines != null
                    && gridCreator.Lines.Count > 0)
                {
                    // Сетка включена. Есть линии. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label510);
                    ui.Show();
                    return;
                }
                if (HaveOpenPositionsByGrid == true)
                {
                    // По сетке есть открытые позиции. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label511);
                    ui.Show();
                    return;
                }

                if (tab.IsConnected == false
                    || tab.IsReadyToTrade == false)
                {
                    // По сетке не подключены данные. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label512);
                    ui.Show();
                    return;
                }

                if (gridCreator.LineCountStart <= 0)
                {
                    // Количество линий в сетке не установлено. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label514);
                    ui.Show();
                    return;
                }

                if (gridCreator.LineStep <= 0)
                {
                    // Шаг сетки не указан. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label515);
                    ui.Show();
                    return;
                }

                if (GridType == TradeGridPrimeType.MarketMaking
                    && gridCreator.ProfitStep <= 0)
                {
                    // Шаг сетки для профита не указан. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label516);
                    ui.Show();
                    return;
                }

                if (gridCreator.StartVolume <= 0)
                {
                    // Стартовый объём не указан. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label517);
                    ui.Show();
                    return;
                }

                if (gridCreator.StepMultiplicator <= 0)
                {
                    // Мультипликатор шага ноль. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label518);
                    ui.Show();
                    return;
                }

                if (GridType == TradeGridPrimeType.MarketMaking
                    && gridCreator.ProfitMultiplicator <= 0)
                {
                    // Мультипликатор профита ноль. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label519);
                    ui.Show();
                    return;
                }

                if (gridCreator.MartingaleMultiplicator <= 0)
                {
                    // Мультипликатор объёма ноль. Запрет
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label520);
                    ui.Show();
                    return;
                }

                if (gridCreator.Lines.Count > 0)
                {
                    AcceptDialogUi ui = new AcceptDialogUi(OsLocalization.Trader.Label522);

                    ui.ShowDialog();

                    if (ui.UserAcceptAction == false)
                    {
                        return;
                    }
                }

                gridCreator.CreateNewGrid(tab, GridType);
                Save();
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        public void DeleteGrid()
        {
            try
            {
                if (GridCreator == null)
                {
                    return;
                }

                if (HaveOpenPositionsByGrid == true
                    && StartProgram == StartProgram.IsOsTrader)
                {
                    CustomMessageBoxUi ui = new CustomMessageBoxUi(OsLocalization.Trader.Label524);
                    ui.Show();
                    return;
                }

                GridCreator.DeleteGrid();
                Save();
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        private bool _isDeleted;

        public void CreateNewLine()
        {
            try
            {
                if (GridCreator == null)
                {
                    return;
                }

                GridCreator.CreateNewLine();

                Save();
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        public void RemoveSelected(List<int> numbers)
        {
            try
            {
                TradeGridCreator gridCreator = GridCreator;
                BotTabSimple tab = Tab;
                if (gridCreator == null || numbers == null || numbers.Count == 0)
                {
                    return;
                }

                for (int i = numbers.Count - 1; i > -1; i--)
                {
                    int curNumber = numbers[i];

                    if (curNumber >= gridCreator.Lines.Count)
                    {
                        continue;
                    }

                    TradeGridLine line = gridCreator.Lines[curNumber];

                    if (line.Position != null
                        && line.Position.OpenActive
                        && tab != null)
                    {
                        tab.CloseOrder(line.Position.OpenOrders[^1]);
                    }
                }

                gridCreator.RemoveSelected(numbers);
                Save();
            }
            catch (Exception e)
            {
                SendNewLogMessage(e.ToString(), LogMessageType.Error);
            }
        }

        #endregion

        #region Trade logic. Entry in logic

        private void Tab_NewTickEvent(Trade trade)
        {
            if (_isDeleted == true)
            {
                return;
            }
            if (RegimeLogicEntry == TradeGridLogicEntryRegime.OnTrade)
            {
                Process();
            }
        }

        private void ThreadWorkerPlace()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(1000);

                    if (_isDeleted == true)
                    {
                        return;
                    }

                    if (RegimeLogicEntry == TradeGridLogicEntryRegime.OncePerSecond)
                    {
                        Process();
                    }

                    if (_needToSave)
                    {
                        _needToSave = false;
                        Save();
                    }
                }
                catch (Exception e)
                {
                    Thread.Sleep(1000);

                    try
                    {
                        SendNewLogMessage(e.ToString(), LogMessageType.Error);
                    }
                    catch
                    {
                        ServerMaster.SendNewLogMessage(e.ToString(), LogMessageType.Error);
                    }
                }
            }
        }

        private void Tab_PositionOpeningSuccesEvent(Position position)
        {
            if (Regime != TradeGridRegime.Off)
            {
                TradeGridCreator gridCreator = GridCreator;
                if (gridCreator == null || gridCreator.Lines == null)
                {
                    return;
                }

                bool isInArray = false;

                for (int i = 0; i < gridCreator.Lines.Count; i++)
                {
                    TradeGridLine line = gridCreator.Lines[i];

                    if (line.Position != null
                        && line.Position.Number == position.Number)
                    {
                        isInArray = true;
                        break;
                    }
                }

                if (isInArray)
                {
                    _openPositionsBySession++;
                    _needToSave = true;
                }
            }

            if (Regime == TradeGridRegime.On)
            {
                _firstPositionIsOpen = true;
            }
            else
            {
                _firstPositionIsOpen = false;
            }
        }

        private void Tab_PositionClosingSuccesEvent(Position position)
        {
            if (Regime == TradeGridRegime.On)
            {
                _firstPositionIsOpen = true;
            }
            else
            {
                _firstPositionIsOpen = false;
            }
        }

        private bool _needToSave;

        #endregion

        #region Trade logic. Main logic tree

        private DateTime _vacationTime;

        private void Process()
        {
            BotTabSimple tab = Tab;
            TradeGridCreator gridCreator = GridCreator;
            TradeGridErrorsReaction errorsReaction = ErrorsReaction;
            TradeGridAutoStarter autoStarter = AutoStarter;
            TradeGridNonTradePeriods nonTradePeriods = NonTradePeriods;
            TradeGridStopBy stopBy = StopBy;
            TrailingUp trailingUp = TrailingUp;

            if (tab == null
                || gridCreator == null
                || errorsReaction == null
                || autoStarter == null
                || nonTradePeriods == null
                || stopBy == null
                || trailingUp == null)
            {
                return;
            }

            if (tab.IsConnected == false
                || tab.IsReadyToTrade == false)
            {
                return;
            }

            if (tab.CandlesAll == null
                || tab.CandlesAll.Count == 0)
            {
                return;
            }

            if (gridCreator.Lines == null
                || gridCreator.Lines.Count == 0)
            {
                return;
            }

            if (tab.EventsIsOn == false)
            {
                return;
            }

            if (MainWindow.ProccesIsWorked == false)
            {
                return;
            }

            if (StartProgram == StartProgram.IsOsTrader)
            {
                if (tab.IsNonTradePeriodInConnector == true)
                {
                    return;
                }
            }

            if (StartProgram == StartProgram.IsOsTrader
               && errorsReaction.WaitOnStartConnectorIsOn == true)
            {
                IServer server = tab.Connector.MyServer;

                if (server.GetType().BaseType.Name == "AServer")
                {
                    AServer aServer = (AServer)server;
                    if (errorsReaction.AwaitOnStartConnector(aServer) == true)
                    {
                        return;
                    }
                }
            }

            if (StartProgram == StartProgram.IsOsTrader)
            {// сбрасываем кол-во ошибок по утрам и на старте сессии

                if (errorsReaction.TryResetErrorsAtStartOfDay(tab.TimeServerCurrent) == true)
                {
                    Save();
                }
            }

            TradeGridRegime baseRegime = Regime;

            // 1 Авто-старт сетки, если выключено
            if (baseRegime == TradeGridRegime.Off ||
                baseRegime == TradeGridRegime.OffAndCancelOrders)
            {
                _firstPositionIsOpen = false;

                if (StartProgram == StartProgram.IsOsTrader)
                {
                    if (_vacationTime > DateTime.Now)
                    {
                        return;
                    }
                }

                if (_openPositionsBySession != 0)
                {
                    _openPositionsBySession = 0;
                    _needToSave = true;
                }
                if (_firstTradePrice != 0)
                {
                    _firstTradePrice = 0;
                    _needToSave = true;
                }

                if (_firstTradeTime != DateTime.MinValue)
                {
                    _firstTradeTime = DateTime.MinValue;
                    _needToSave = true;
                }

                _firstStopIsActivate = false;

                if (errorsReaction.FailCancelOrdersCountFact != 0
                    || errorsReaction.FailOpenOrdersCountFact != 0)
                {
                    errorsReaction.FailCancelOrdersCountFact = 0;
                    errorsReaction.FailOpenOrdersCountFact = 0;
                    _needToSave = true;
                }

                if (GridType == TradeGridPrimeType.OpenPosition)
                {
                    TryDeleteDonePositions();
                }

                // отзываем ордера с рынка

                if (HaveOrdersTryToCancelLastSecond())
                {
                    return;
                }

                if (baseRegime == TradeGridRegime.OffAndCancelOrders)
                {
                    int countRejectOrders = TryCancelClosingOrders();

                    if (countRejectOrders > 0)
                    {
                        _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                        return;
                    }

                    countRejectOrders = TryCancelOpeningOrders();

                    if (countRejectOrders > 0)
                    {
                        _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                        return;
                    }
                }

                // проверяем работу авто-стартера, если он включен

                if (autoStarter.AutoStartRegime == TradeGridAutoStartRegime.Off
                    && autoStarter.StartGridByTimeOfDayIsOn == false)
                {
                    return;
                }

                DateTime serverTime = tab.TimeServerCurrent;

                TradeGridRegime nonTradePeriodsRegime = nonTradePeriods.GetNonTradePeriodsRegime(serverTime);

                if (nonTradePeriodsRegime != TradeGridRegime.On)
                { // авто-старт не может быть включен, если сейчас не торговый период
                    return;
                }

                if (autoStarter.HaveEventToStart(this))
                {
                    if (autoStarter.RebuildGridRegime == GridAutoStartShiftFirstPriceRegime.On_FullRebuild)
                    {// пересобираем сетку полностью
                        decimal newPriceStart = autoStarter.GetNewGridPriceStart(this);

                        if (newPriceStart != 0)
                        {
                            gridCreator.FirstPrice = newPriceStart;
                            gridCreator.CreateNewGrid(tab, GridType);
                            Save();
                            FullRePaintGrid();
                        }
                    }
                    else if (autoStarter.RebuildGridRegime == GridAutoStartShiftFirstPriceRegime.On_ShiftOnNewPrice)
                    {// просто сдвигаем сетку на новую цену

                        decimal newPriceStart = autoStarter.GetNewGridPriceStart(this);

                        if (newPriceStart != 0)
                        {
                            autoStarter.ShiftGridOnNewPrice(newPriceStart, this);
                            Save();
                            FullRePaintGrid();
                        }
                    }

                    baseRegime = TradeGridRegime.On;
                    Regime = TradeGridRegime.On;
                    Save();
                    RePaintGrid();
                }
                else
                {
                    return;
                }
            }

            // 2 проверяем ошибки и реагируем на них

            if (StartProgram == StartProgram.IsOsTrader)
            {
                TradeGridRegime reaction = errorsReaction.GetReactionOnErrors(this);

                if (reaction != TradeGridRegime.On)
                {
                    errorsReaction.FailCancelOrdersCountFact = 0;
                    errorsReaction.FailOpenOrdersCountFact = 0;
                    baseRegime = reaction;
                    Regime = reaction;
                    Save();
                    RePaintGrid();
                }
            }

            // 3 проверяем ожидание в бою. Только что были отозваны или выставлены N кол-во ордеров

            if (StartProgram == StartProgram.IsOsTrader)
            {
                if (_vacationTime > DateTime.Now)
                {
                    return;
                }
            }

            // 4 проверяем наличие ордеров без номеров в маркете. Для медленных подключений

            if (StartProgram == StartProgram.IsOsTrader)
            {
                if (HaveOrdersWithNoMarketOrders())
                {
                    return;
                }

                if (HaveOrdersTryToCancelLastSecond())
                {
                    return;
                }
            }

            // 5 попытка смены режима если блокировано по времени или по дням

            if (baseRegime != TradeGridRegime.Off)
            {
                DateTime serverTime = tab.TimeServerCurrent;

                TradeGridRegime nonTradePeriodsRegime = nonTradePeriods.GetNonTradePeriodsRegime(serverTime);

                if (nonTradePeriodsRegime != TradeGridRegime.On)
                {
                    baseRegime = nonTradePeriodsRegime;

                    if (baseRegime == TradeGridRegime.CloseForced)
                    {
                        Regime = baseRegime;
                        Save();
                        RePaintGrid();
                    }
                }
            }

            // 6 попытка смены режима по остановке торгов

            if (baseRegime == TradeGridRegime.On)
            {
                TradeGridRegime stopByRegime = stopBy.GetRegime(this, tab);

                if (stopByRegime != TradeGridRegime.On)
                {
                    baseRegime = stopByRegime;
                    Regime = stopByRegime;
                    Save();
                    RePaintGrid();
                }
            }

            // 7 попытка сместить сетку

            if (baseRegime == TradeGridRegime.On)
            {
                if (trailingUp.TryTrailingGrid())
                {
                    _needToSave = true;
                    RePaintGrid();
                    FullRePaintGrid();
                }
            }

            // 8 вход в различную логику различных сеток

            if (baseRegime == TradeGridRegime.On
                || baseRegime == TradeGridRegime.CloseOnly
                || baseRegime == TradeGridRegime.CloseForced)
            {
                if (GridType == TradeGridPrimeType.MarketMaking)
                {
                    GridTypeMarketMakingLogic(baseRegime);
                }
                else if (GridType == TradeGridPrimeType.OpenPosition)
                {
                    GridTypeOpenPositionLogic(baseRegime);
                }
            }
            else if (baseRegime == TradeGridRegime.OffAndCancelOrders)
            {
                int countRejectOrders = TryCancelClosingOrders();

                if (countRejectOrders > 0)
                {
                    _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                    return;
                }

                countRejectOrders = TryCancelOpeningOrders();

                if (countRejectOrders > 0)
                {
                    _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                    return;
                }
            }
        }

        #endregion

        #region Open Position end logic

        private void GridTypeOpenPositionLogic(TradeGridRegime baseRegime)
        {
            if (_firstStopIsActivate == true)
            {
                if (_firstStopActivateTime.AddSeconds(5) < DateTime.Now)
                {
                    string message = "First stop by grid is activate. \n";
                    message += "Stop trading" + "\n";
                    message += "New regime: CloseForced";

                    SendNewLogMessage(message, LogMessageType.Signal);

                    Regime = TradeGridRegime.CloseForced;
                    Save();
                    RePaintGrid();
                    _firstStopIsActivate = false;
                    _vacationTime = DateTime.Now.AddSeconds(5);
                }
                else
                {
                    return;
                }
            }

            // 1 сверям позиции в журнале и в сетке

            TryFindPositionsInJournalAfterReconnect();
            TryDeleteOpeningFailPositions();

            // 2 удаляем ордера стоящие не на своём месте

            int countRejectOrders = TryRemoveWrongOrders();

            if (countRejectOrders > 0)
            {
                _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                return;
            }

            // 3 торговая логика 

            if (baseRegime == TradeGridRegime.On)
            {
                if (_firstStopIsActivate == false)
                {
                    TradeGridStopAndProfit stopAndProfit = StopAndProfit;

                    // 1 пытаемся почистить журнал от лишних сделок
                    TryFreeJournal();

                    // 2 проверяем выставлены ли ордера на открытие
                    TrySetOpenOrders();

                    // 3 проверяем выставлены ли закрытия
                    TrySetStopAndProfit();

                    // 4 проверяем лимитки за закрытие по профиту
                    if (stopAndProfit != null
                        && stopAndProfit.ProfitRegime == OnOffRegime.On)
                    {
                        TrySetLimitProfit();

                        if (stopAndProfit.StopTradingAfterProfit == true)
                        {
                            CheckStopTradingAfterProfit();
                        }
                        else
                        {
                            TryDeleteDonePositions();
                        }
                    }
                }
            }
            else
            {
                countRejectOrders = TryCancelOpeningOrders();

                if (countRejectOrders > 0)
                {
                    _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                    return;
                }

                if (_openPositionsBySession != 0)
                {
                    _openPositionsBySession = 0;
                    _needToSave = true;
                }
                if (_firstTradePrice != 0)
                {
                    _firstTradePrice = 0;
                    _needToSave = true;
                }
                if (_firstTradeTime != DateTime.MinValue)
                {
                    _firstTradeTime = DateTime.MinValue;
                    _needToSave = true;
                }

                if (baseRegime == TradeGridRegime.CloseOnly)
                {
                    // закрываем позиции штатно
                    TrySetStopAndProfit();
                }
                else if (baseRegime == TradeGridRegime.CloseForced)
                {
                    countRejectOrders = TryCancelClosingOrders();

                    if (countRejectOrders > 0)
                    {
                        _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                        return;
                    }

                    // закрываем позиции насильно
                    TryForcedCloseGrid();
                }
            }
        }

        private void TrySetStopAndProfit()
        {
            TradeGridStopAndProfit stopAndProfit = StopAndProfit;
            if (stopAndProfit == null)
            {
                return;
            }

            if (stopAndProfit.ProfitRegime == OnOffRegime.Off
                && stopAndProfit.StopRegime == OnOffRegime.Off
                && stopAndProfit.TrailStopRegime == OnOffRegime.Off)
            {
                return;
            }

            stopAndProfit.Process(this);
        }

        private void TryDeleteOpeningFailPositions()
        {
            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return;
            }

            List<TradeGridLine> lines = gridCreator.Lines;

            if (lines == null)
            {
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];

                if (line.Position != null)
                {
                    // Открывающий ордер был отозван
                    if (line.Position.State == PositionStateType.OpeningFail
                        && line.Position.OpenActive == false)
                    {
                        line.Position = null;
                        line.PositionNum = -1;
                    }
                }
            }
        }

        private bool _firstStopIsActivate = false;

        private DateTime _firstStopActivateTime;

        private bool _firstPositionIsOpen = false;

        private void Tab_PositionStopActivateEvent(Position obj)
        {
            if (_firstStopIsActivate == false)
            {
                _firstStopIsActivate = true;
                _firstStopActivateTime = DateTime.Now;
            }
        }

        private void TrySetLimitProfit()
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return;
            }

            // 1 проверяем отзыв не правильных лимиток

            int countRejectOrders = TryCancelWrongCloseProfitOrders();

            if (countRejectOrders > 0)
            {
                _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                return;
            }

            // 2 выставляем лимитки 

            TrySetClosingProfitOrders(tab.PriceBestAsk);

        }

        private int TryCancelWrongCloseProfitOrders()
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return 0;
            }

            List<TradeGridLine> lines = GetLinesWithClosingOrdersFact();

            int cancelledOrders = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];

                if (line.Position == null
                    || line.Position.CloseActive == false)
                {
                    continue;
                }

                Order order = lines[i].Position.CloseOrders[^1];

                if (order.NumberMarket != null
                    && order.LastCancelTryLocalTime.AddSeconds(5) < DateTime.Now)
                {
                    if (order.Price != line.Position.ProfitOrderPrice
                        || order.Volume - order.VolumeExecute != line.Position.OpenVolume)
                    {
                        tab.CloseOrder(order);
                        cancelledOrders++;
                    }
                }
            }

            return cancelledOrders;
        }

        private void TrySetClosingProfitOrders(decimal lastPrice)
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return;
            }

            List<TradeGridLine> linesOpenPoses = GetLinesWithOpenPosition();

            for (int i = 0; i < linesOpenPoses.Count; i++)
            {
                Position pos = linesOpenPoses[i].Position;
                TradeGridLine line = linesOpenPoses[i];

                if (pos.CloseActive == true)
                {
                    continue;
                }

                if (pos.ProfitOrderPrice == 0)
                {
                    continue;
                }

                decimal volume = pos.OpenVolume;

                if (CheckMicroVolumes == true
                    && tab.CanTradeThisVolume(volume) == false)
                {
                    continue;
                }

                if (tab.Security.PriceLimitHigh != 0
                 && tab.Security.PriceLimitLow != 0)
                {
                    if (line.PriceExit > tab.Security.PriceLimitHigh
                        || line.PriceExit < tab.Security.PriceLimitLow)
                    {
                        continue;
                    }
                }

                if (tab.StartProgram == StartProgram.IsOsTrader
                    && MaxDistanceToOrdersPercent != 0
                    && lastPrice != 0)
                {
                    decimal maxPriceUp = lastPrice + lastPrice * (MaxDistanceToOrdersPercent / 100);
                    decimal minPriceDown = lastPrice - lastPrice * (MaxDistanceToOrdersPercent / 100);

                    if (line.PriceExit > maxPriceUp
                     || line.PriceExit < minPriceDown)
                    {
                        continue;
                    }
                }

                tab.CloseAtLimitUnsafe(pos, pos.ProfitOrderPrice, volume);
            }
        }

        private void CheckStopTradingAfterProfit()
        {
            List<TradeGridLine> linesOpenPoses = GetLinesWithOpenPosition();

            // И если линий с открытыми позами нет - переключаемся в CloseForced

            if (linesOpenPoses == null
                || linesOpenPoses.Count == 0)
            {
                if (_firstPositionIsOpen == true)
                {
                    Regime = TradeGridRegime.CloseForced;

                    string message = "Grid is stop by Profit. \n";
                    message += "Stop trading" + "\n";
                    message += "New regime: CloseForced";

                    SendNewLogMessage(message, LogMessageType.Signal);
                }
            }
        }

        #endregion

        #region MarketMaking end logic

        private void GridTypeMarketMakingLogic(TradeGridRegime baseRegime)
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return;
            }

            // 1 сверям позиции в журнале и в сетке

            TryFindPositionsInJournalAfterReconnect();
            TryDeleteDonePositions();

            // 2 удаляем ордера стоящие не на своём месте

            int countRejectOrders = TryRemoveWrongOrders();

            if (countRejectOrders > 0)
            {
                _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                return;
            }

            // 8 торговая логика 

            if (baseRegime == TradeGridRegime.On)
            {
                // 1 пытаемся почистить журнал от лишних сделок
                TryFreeJournal();

                // 2 проверяем выставлены ли ордера на открытие
                TrySetOpenOrders();

                // 3 проверяем выставлены ли закрытия
                TrySetClosingOrders(tab.PriceBestAsk);
            }
            else
            {
                countRejectOrders = TryCancelOpeningOrders();

                if (countRejectOrders > 0)
                {
                    _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                    return;
                }

                if (_openPositionsBySession != 0)
                {
                    _openPositionsBySession = 0;
                    _needToSave = true;
                }
                if (_firstTradePrice != 0)
                {
                    _firstTradePrice = 0;
                    _needToSave = true;
                }
                if (_firstTradeTime != DateTime.MinValue)
                {
                    _firstTradeTime = DateTime.MinValue;
                    _needToSave = true;
                }

                if (baseRegime == TradeGridRegime.CloseOnly)
                {
                    // закрываем позиции штатно
                    TrySetClosingOrders(tab.PriceBestAsk);
                }
                else if (baseRegime == TradeGridRegime.CloseForced)
                {
                    countRejectOrders = TryCancelClosingOrders();

                    if (countRejectOrders > 0)
                    {
                        _vacationTime = DateTime.Now.AddMilliseconds(DelayInReal * countRejectOrders);
                        return;
                    }

                    // закрываем позиции насильно 
                    TryForcedCloseGrid();
                }
            }
        }

        private int TryRemoveWrongOrders()
        {
            BotTabSimple tab = Tab;
            TradeGridCreator gridCreator = GridCreator;
            if (tab == null || gridCreator == null)
            {
                return 0;
            }

            List<Candle> candles = tab.CandlesAll;

            if (candles == null || candles.Count == 0)
            {
                return 0;
            }

            decimal lastPrice = candles[candles.Count - 1].Close;

            // 1 убираем ордера на открытие и закрытие с неправильной ценой.

            List<Order> ordersToCancelBadPrice = GetOrdersBadPriceToGrid();

            if (ordersToCancelBadPrice != null
                && ordersToCancelBadPrice.Count > 0)
            {
                for (int i = 0; i < ordersToCancelBadPrice.Count; i++)
                {
                    //Tab.SetNewLogMessage("Отзыв ордера по не правильной цене", LogMessageType.System);
                    tab.CloseOrder(ordersToCancelBadPrice[i]);
                }

                return ordersToCancelBadPrice.Count;
            }

            // 2 убираем ордера лишние на открытие. Когда в сетке больше ордеров чем указал пользователь

            List<Order> ordersToCancelBadLines = GetOrdersBadLinesMaxCount();

            if (ordersToCancelBadLines != null
                && ordersToCancelBadLines.Count > 0)
            {
                for (int i = 0; i < ordersToCancelBadLines.Count; i++)
                {
                    //Tab.SetNewLogMessage("Отзыв ордера по количеству", LogMessageType.System);
                    tab.CloseOrder(ordersToCancelBadLines[i]);
                }

                return ordersToCancelBadLines.Count;
            }

            // 3 убираем ордера на открытие, если имеет место дыра в сетке

            List<Order> ordersToCancelOpenOrders = GetOpenOrdersGridHole();

            if (ordersToCancelOpenOrders != null
                && ordersToCancelOpenOrders.Count > 0)
            {
                for (int i = 0; i < ordersToCancelOpenOrders.Count; i++)
                {
                    //Tab.SetNewLogMessage("Отзыв ордера по дыре в сетке", LogMessageType.System);
                    tab.CloseOrder(ordersToCancelOpenOrders[i]);
                }

                return ordersToCancelOpenOrders.Count;
            }

            // 4 убираем ордера лишние на закрытие.
            // Когда в сетке больше ордеров чем указал пользователь
            // И когда объём на закрытие не совпадает с тем что в ордере закрывающем

            if (GridType == TradeGridPrimeType.MarketMaking)
            {
                List<Order> ordersToCancelCloseOrders = GetCloseOrdersGridHole();

                if (ordersToCancelCloseOrders != null
                    && ordersToCancelCloseOrders.Count > 0)
                {
                    for (int i = 0; i < ordersToCancelCloseOrders.Count; i++)
                    {
                        tab.CloseOrder(ordersToCancelCloseOrders[i]);
                    }

                    return ordersToCancelCloseOrders.Count;
                }
            }

            return 0;
        }

        private List<Order> GetOrdersBadPriceToGrid()
        {
            // 1 смотрим совпадение цен у ордера на открытие с ценой открытия линии 
            // 2 смотрим совпадиние цен у ордера на закрытие с ценой закрытия линии

            List<Order> ordersToCancel = new List<Order>();

            List<TradeGridLine> linesWithOrdersToOpenFact = GetLinesWithOpenOrdersFact();

            for (int i = 0; linesWithOrdersToOpenFact != null && i < linesWithOrdersToOpenFact.Count; i++)
            {
                Position position = linesWithOrdersToOpenFact[i].Position;
                TradeGridLine currentLine = linesWithOrdersToOpenFact[i];

                if (position.OpenActive)
                {
                    Order openOrder = position.OpenOrders[^1];

                    if (openOrder.Price != currentLine.PriceEnter)
                    {
                        ordersToCancel.Add(openOrder);
                    }
                }
            }

            List<TradeGridLine> linesWithOrdersToCloseFact = GetLinesWithClosingOrdersFact();

            for (int i = 0; linesWithOrdersToCloseFact != null && i < linesWithOrdersToCloseFact.Count; i++)
            {
                Position position = linesWithOrdersToCloseFact[i].Position;
                TradeGridLine currentLine = linesWithOrdersToCloseFact[i];

                if (position.CloseActive
                    && currentLine.CanReplaceExitOrder == true)
                {
                    Order closeOrder = position.CloseOrders[^1];

                    if (GridType == TradeGridPrimeType.MarketMaking)
                    {
                        if (closeOrder.Price != currentLine.PriceExit
                         && closeOrder.TypeOrder != OrderPriceType.Market)
                        {
                            ordersToCancel.Add(closeOrder);
                        }
                    }
                    else if (GridType == TradeGridPrimeType.OpenPosition)
                    {
                        if (closeOrder.Price != position.ProfitOrderPrice
                        && closeOrder.TypeOrder != OrderPriceType.Market)
                        {
                            ordersToCancel.Add(closeOrder);
                        }
                    }
                }
            }

            return ordersToCancel;
        }

        private List<Order> GetOrdersBadLinesMaxCount()
        {
            List<TradeGridLine> linesWithOrdersToOpenFact = GetLinesWithOpenOrdersFact();

            List<Order> ordersToCancel = new List<Order>();

            // 1 Открытие. Смотрим чтобы не было ордеров больше чем указал пользователь

            for (int i = MaxOpenOrdersInMarket; i < linesWithOrdersToOpenFact.Count; i++)
            {
                Position curPosition = linesWithOrdersToOpenFact[i].Position;
                ordersToCancel.Add(curPosition.OpenOrders[^1]);
            }

            return ordersToCancel;
        }

        private List<Order> GetOpenOrdersGridHole()
        {
            BotTabSimple tab = Tab;
            TradeGridCreator gridCreator = GridCreator;
            if (tab == null || gridCreator == null)
            {
                return null;
            }

            List<Candle> candles = tab.CandlesAll;

            if (candles == null || candles.Count == 0)
            {
                return null;
            }

            decimal lastPrice = candles[candles.Count - 1].Close;

            // 1 берём текущие линии с позициями

            List<TradeGridLine> linesWithOrdersToOpenNeed = GetLinesWithOpenOrdersNeed(lastPrice);

            List<TradeGridLine> linesWithOrdersToOpenFact = GetLinesWithOpenOrdersFact();

            if (linesWithOrdersToOpenFact == null ||
                linesWithOrdersToOpenFact.Count == 0)
            {
                return null;
            }

            if (linesWithOrdersToOpenNeed == null ||
                linesWithOrdersToOpenNeed.Count == 0)
            {
                return null;
            }

            List<Order> ordersToCancel = new List<Order>();

            // 2 смотрим, Стоит ли первый ордер на своём месте

            TradeGridLine firstLineFirstNeed = linesWithOrdersToOpenNeed[0];
            TradeGridLine firstLineFirstFact = linesWithOrdersToOpenFact[0];

            TradeGridLine firstLineLastNeed = linesWithOrdersToOpenNeed[^1];
            TradeGridLine firstLineLastFact = linesWithOrdersToOpenFact[^1];

            if (firstLineFirstFact.PriceEnter == firstLineFirstNeed.PriceEnter
                && firstLineLastFact.PriceEnter == firstLineLastNeed.PriceEnter)
            {// всё в порядке
                return null;
            }

            if (linesWithOrdersToOpenFact.Count >= linesWithOrdersToOpenNeed.Count)
            {
                ordersToCancel.Add(linesWithOrdersToOpenFact[^1].Position.OpenOrders[^1]);
            }

            return ordersToCancel;
        }

        private List<Order> GetCloseOrdersGridHole()
        {
            List<TradeGridLine> linesOpenPoses = GetLinesWithOpenPosition();

            List<Order> ordersToCancel = new List<Order>();

            // 1 отправляем на отзыв ордера которые за пределами желаемого пользователем кол-ва

            for (int i = 0; i < linesOpenPoses.Count - MaxCloseOrdersInMarket; i++)
            {
                Position pos = linesOpenPoses[i].Position;
                TradeGridLine line = linesOpenPoses[i];

                if (pos.CloseActive == true)
                {
                    ordersToCancel.Add(pos.CloseOrders[^1]);
                }
            }

            // 2 отправляем на отзыв ордера которые с не верным объёмом

            for (int i = 0; i < linesOpenPoses.Count; i++)
            {
                Position pos = linesOpenPoses[i].Position;
                TradeGridLine line = linesOpenPoses[i];

                if (pos.CloseActive == false)
                {
                    continue;
                }

                Order orderToClose = pos.CloseOrders[^1];

                if (orderToClose.Volume != pos.OpenVolume)
                {
                    bool isInArray = false;

                    for (int j = 0; j < ordersToCancel.Count; j++)
                    {
                        if (ordersToCancel[j].NumberUser == orderToClose.NumberUser)
                        {
                            isInArray = true;
                            break;
                        }
                    }
                    if (isInArray == false)
                    {
                        ordersToCancel.Add(orderToClose);
                    }
                }
            }

            return ordersToCancel;
        }

        private int TryCancelOpeningOrders()
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return 0;
            }

            List<TradeGridLine> lines = GetLinesWithOpenOrdersFact();

            int cancelledOrders = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];

                if (line.Position == null
                    || line.Position.OpenActive == false)
                {
                    continue;
                }

                Order order = lines[i].Position.OpenOrders[^1];

                if (order.NumberMarket != null)
                {
                    tab.CloseOrder(order);
                    cancelledOrders++;
                }
            }

            return cancelledOrders;
        }

        private void TrySetClosingOrders(decimal lastPrice)
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return;
            }

            CheckWrongCloseOrders();

            List<TradeGridLine> linesOpenPoses = GetLinesWithOpenPosition();

            int startIndex = linesOpenPoses.Count - MaxCloseOrdersInMarket;

            if (startIndex < 0)
            {
                startIndex = 0;
            }

            for (int i = startIndex; i < linesOpenPoses.Count; i++)
            {
                Position pos = linesOpenPoses[i].Position;
                TradeGridLine line = linesOpenPoses[i];

                if (pos.CloseActive == true)
                {
                    continue;
                }

                decimal volume = pos.OpenVolume;

                if (CheckMicroVolumes == true
                    && tab.CanTradeThisVolume(volume) == false)
                {
                    continue;
                }

                if (tab.Security.PriceLimitHigh != 0
                 && tab.Security.PriceLimitLow != 0)
                {
                    if (line.PriceExit > tab.Security.PriceLimitHigh
                        || line.PriceExit < tab.Security.PriceLimitLow)
                    {
                        continue;
                    }
                }

                if (tab.StartProgram == StartProgram.IsOsTrader
                    && MaxDistanceToOrdersPercent != 0
                    && lastPrice != 0)
                {
                    decimal maxPriceUp = lastPrice + lastPrice * (MaxDistanceToOrdersPercent / 100);
                    decimal minPriceDown = lastPrice - lastPrice * (MaxDistanceToOrdersPercent / 100);

                    if (line.PriceExit > maxPriceUp
                     || line.PriceExit < minPriceDown)
                    {
                        continue;
                    }
                }

                tab.CloseAtLimitUnsafe(pos, line.PriceExit, volume);
            }
        }

        private void CheckWrongCloseOrders()
        {
            BotTabSimple tab = Tab;
            TradeGridCreator gridCreator = GridCreator;
            if (tab == null || gridCreator == null || tab.StartProgram != StartProgram.IsOsTrader)
            {
                return;
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;

            for (int i = 0; i < linesAll.Count; i++)
            {
                TradeGridLine curLine = linesAll[i];
                Position pos = curLine.Position;

                if (pos == null)
                {
                    continue;
                }

                decimal volumePosOpen = pos.OpenVolume;

                if (pos.CloseActive == true)
                {
                    Order orderToClose = pos.CloseOrders[^1];
                    decimal volumeCloseOrder = orderToClose.Volume;
                    decimal volumeExecuteCloseOrder = orderToClose.VolumeExecute;

                    if (volumePosOpen != (volumeCloseOrder - volumeExecuteCloseOrder))
                    {
                        tab.CloseOrder(orderToClose);
                    }
                }
            }
        }

        private int TryCancelClosingOrders()
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return 0;
            }

            List<TradeGridLine> lines = GetLinesWithOpenPosition();

            int cancelledOrders = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];

                if (line.Position == null
                    || line.Position.CloseActive == false)
                {
                    continue;
                }

                Order order = lines[i].Position.CloseOrders[^1];

                if (order.NumberMarket != null
                   && order.TypeOrder != OrderPriceType.Market)
                {
                    tab.CloseOrder(order);
                    cancelledOrders++;
                }
            }

            return cancelledOrders;
        }

        private void TrySetOpenOrders()
        {
            BotTabSimple tab = Tab;
            TradeGridCreator gridCreator = GridCreator;
            if (tab == null || gridCreator == null)
            {
                return;
            }

            List<Candle> candles = tab.CandlesAll;

            if (candles == null || candles.Count == 0)
            {
                return;
            }

            decimal lastPrice = candles[candles.Count - 1].Close;

            if (lastPrice == 0)
            {
                return;
            }

            if (tab.PriceBestAsk == 0
                || tab.PriceBestBid == 0)
            {
                return;
            }

            // 1 берём текущие линии с позициями

            List<TradeGridLine> linesWithOrdersToOpenNeed = GetLinesWithOpenOrdersNeed(lastPrice);

            List<TradeGridLine> linesWithOrdersToOpenFact = GetLinesWithOpenOrdersFact();

            // 2 ничего не делаем если уже кол-во ордеров максимально

            if (linesWithOrdersToOpenFact.Count >= MaxOpenOrdersInMarket)
            {
                return;
            }

            // 3 открываемся по новой схеме

            for (int i = 0; i < linesWithOrdersToOpenNeed.Count; i++)
            {
                TradeGridLine curLineNeed = linesWithOrdersToOpenNeed[i];

                if (curLineNeed.Position != null)
                {
                    continue;
                }

                // открываемся. Позиции по линии нет

                decimal volume = gridCreator.GetVolume(curLineNeed, tab);

                Position newPosition = null;

                if (curLineNeed.Side == Side.Buy)
                {
                    decimal price = curLineNeed.PriceEnter;

                    if (OpenOrdersMakerOnly == false
                        && tab.Security.PriceLimitHigh != 0
                        && price >= tab.Security.PriceLimitHigh)
                    {
                        price = tab.Security.PriceLimitHigh - (tab.Security.PriceStep * 10);
                    }

                    newPosition = tab.BuyAtLimit(volume, price);
                }
                else if (curLineNeed.Side == Side.Sell)
                {
                    decimal price = curLineNeed.PriceEnter;

                    if (OpenOrdersMakerOnly == false
                        && tab.Security.PriceLimitLow != 0
                        && price <= tab.Security.PriceLimitLow)
                    {
                        price = tab.Security.PriceLimitLow + (tab.Security.PriceStep * 10);
                    }

                    newPosition = tab.SellAtLimit(volume, price);
                }

                if (newPosition != null)
                {
                    curLineNeed.Position = newPosition;
                    curLineNeed.PositionNum = newPosition.Number;

                    if (_firstTradePrice == 0)
                    {
                        _firstTradePrice = curLineNeed.PriceEnter;
                    }

                    if (_firstTradeTime == DateTime.MinValue)
                    {
                        _firstTradeTime = tab.TimeServerCurrent;
                    }

                    _needToSave = true;
                }

                linesWithOrdersToOpenFact = GetLinesWithOpenOrdersFact();

                if (linesWithOrdersToOpenFact.Count >= MaxOpenOrdersInMarket)
                {
                    return;
                }
            }
        }

        private DateTime _lastCheckJournalTime = DateTime.MinValue;

        private void TryFreeJournal()
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return;
            }

            if (AutoClearJournalIsOn == false)
            {
                return;
            }

            if (_lastCheckJournalTime.AddSeconds(10) > DateTime.Now)
            {
                return;
            }

            _lastCheckJournalTime = DateTime.Now;

            Position[] positions = tab.PositionsAll.ToArray();

            // 1 удаляем позиции с OpeningFail без всяких условий

            for (int i = 0; i < positions.Length; i++)
            {
                Position pos = positions[i];

                if (pos == null)
                {
                    continue;
                }

                if (pos.State == PositionStateType.OpeningFail
                    && pos.OpenVolume == 0
                    && pos.OpenActive == false
                    && pos.CloseActive == false)
                {
                    TryDeletePositionsFromJournal(pos);
                }
            }

            // 2 удаляем позиции со статусом Done, если пользователь это включил        

            int curDonePosInJournal = 0;

            for (int i = positions.Length - 1; i >= 0; i--)
            {
                Position pos = positions[i];

                if (pos == null)
                {
                    continue;
                }

                if (pos.State != PositionStateType.Done)
                {
                    continue;
                }

                if (pos.OpenVolume != 0)
                {
                    continue;
                }

                if (pos.OpenActive == true
                    || pos.CloseActive == true)
                {
                    continue;
                }

                curDonePosInJournal++;

                if (curDonePosInJournal > MaxClosePositionsInJournal)
                {
                    TryDeletePositionsFromJournal(pos);
                }
            }
        }

        private void TryDeletePositionsFromJournal(Position position)
        {
            TradeGridCreator gridCreator = GridCreator;
            BotTabSimple tab = Tab;
            if (gridCreator == null || tab == null || position == null)
            {
                return;
            }

            List<TradeGridLine> lines = gridCreator.Lines;

            bool isInGridNow = false;

            for (int i = 0; lines != null && i < lines.Count; i++)
            {
                if (lines[i].PositionNum == position.Number)
                {
                    isInGridNow = true;
                    break;
                }
            }

            if (isInGridNow == false)
            {
                tab._journal.DeletePosition(position);
            }
        }

        private void TryDeleteDonePositions()
        {
            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return;
            }

            List<TradeGridLine> lines = gridCreator.Lines;

            if (lines == null)
            {
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];

                if (line.Position != null)
                {
                    // Позиция была закрыта
                    // Открывающий ордер был отозван
                    if (line.Position.State == PositionStateType.Done
                        ||
                        (line.Position.State == PositionStateType.OpeningFail
                        && line.Position.OpenActive == false))
                    {
                        line.Position = null;
                        line.PositionNum = -1;
                    }

                    else if (line.Position.State == PositionStateType.Deleted)
                    {
                        line.Position = null;
                        line.PositionNum = -1;
                    }
                }
            }
        }

        private void TryFindPositionsInJournalAfterReconnect()
        {
            TradeGridCreator gridCreator = GridCreator;
            BotTabSimple tab = Tab;
            if (gridCreator == null || tab == null)
            {
                return;
            }

            List<TradeGridLine> lines = gridCreator.Lines;
            List<Position> positions = tab.PositionsAll;

            if (lines == null || positions == null)
            {
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];

                // проблема 1. Номер позиции есть - самой позиции нет. 
                // произошёл перезапуск терминала. Ищем позу в журнале
                if (line.PositionNum != -1
                    && line.Position == null)
                {
                    bool isInArray = false;

                    for (int j = 0; j < positions.Count; j++)
                    {
                        if (positions[j].Number == line.PositionNum)
                        {
                            isInArray = true;
                            line.Position = positions[j];
                            break;
                        }
                    }

                    if (isInArray == false)
                    {
                        line.Position = null;
                        line.PositionNum = -1;
                    }
                }
            }
        }

        #endregion

        #region Forced Close regime logic

        private void TryForcedCloseGrid()
        {
            BotTabSimple tab = Tab;
            if (tab == null)
            {
                return;
            }

            List<TradeGridLine> lines = GetLinesWithOpenPosition();

            bool havePositions = false;

            for (int i = 0; i < lines.Count; i++)
            {
                TradeGridLine line = lines[i];

                if (line.Position == null
                    || line.Position.CloseActive == true)
                {
                    continue;
                }

                Position pos = line.Position;

                if (pos.State != PositionStateType.Done
                    || pos.OpenVolume >= 0)
                {
                    if (CheckMicroVolumes == true
                    && tab.CanTradeThisVolume(pos.OpenVolume) == false)
                    {
                        string message = "Micro volume detected. Position deleted \n";
                        message += "Position volume: " + pos.OpenVolume + "\n";
                        message += "Security name: " + pos.SecurityName;
                        SendNewLogMessage(message, LogMessageType.Error);

                        line.Position = null;
                        line.PositionNum = -1;
                        continue;
                    }

                    tab.CloseAtMarket(pos, pos.OpenVolume);
                    havePositions = true;
                }
            }

            if (Regime == TradeGridRegime.CloseForced
                && havePositions == false)
            {
                string message = "Close Forced regime ended. No positions \n";
                message += "New regime: Off";
                SendNewLogMessage(message, LogMessageType.Signal);
                Regime = TradeGridRegime.Off;
                RePaintGrid();
                _needToSave = true;
            }
        }

        #endregion

        #region Public interface

        public decimal FirstPriceReal
        {
            get
            {
                return _firstTradePrice;
            }
        }
        private decimal _firstTradePrice;

        public int OpenPositionsCount
        {
            get
            {

                return _openPositionsBySession;
            }
        }
        private int _openPositionsBySession;

        public decimal OpenVolumeByLines
        {
            get
            {
                // 1 берём позиции по сетке

                List<TradeGridLine> linesWithPositions = GetLinesWithOpenPosition();

                if (linesWithPositions == null
                    || linesWithPositions.Count == 0)
                {
                    return 0;
                }

                decimal result = 0;

                for (int i = 0; i < linesWithPositions.Count; i++)
                {
                    if (linesWithPositions[i].Position == null)
                    {
                        continue;
                    }

                    result += linesWithPositions[i].Position.OpenVolume;
                }

                return result;
            }
        }

        public decimal AllVolumeInLines
        {
            get
            {
                TradeGridCreator gridCreator = GridCreator;
                if (gridCreator == null)
                {
                    return 0;
                }

                List<TradeGridLine> lines = gridCreator.Lines;

                if (lines == null
                    || lines.Count == 0)
                {
                    return 0;
                }

                decimal result = 0;

                for (int i = 0; i < lines.Count; i++)
                {
                    result += lines[i].Volume;
                }

                return result;
            }
        }

        public DateTime FirstTradeTime
        {
            get
            {
                return _firstTradeTime;
            }
        }
        private DateTime _firstTradeTime;

        public bool HaveOrdersWithNoMarketOrders()
        {
            // 1 берём все уровни с позициями
            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return false;
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;

            for (int i = 0; linesAll != null && i < linesAll.Count; i++)
            {
                if (linesAll[i].Position != null)
                {
                    Position position = linesAll[i].Position;

                    if (position.OpenActive)
                    {
                        if (string.IsNullOrEmpty(position.OpenOrders[^1].NumberMarket))
                        {
                            if (position.OpenOrders[^1].State == OrderStateType.None
                                && _lastNoneOrderTime == DateTime.MinValue)
                            {
                                _lastNoneOrderTime = DateTime.Now;
                            }
                            else if (position.OpenOrders[^1].State == OrderStateType.None
                                && _lastNoneOrderTime.AddMinutes(5) < DateTime.Now)
                            {// 5ть минут висит ордер со статусом NONE. Утерян
                                position.OpenOrders.RemoveAt(position.OpenOrders.Count - 1);
                                SendNewLogMessage("Remove NONE open order. Five minutes rule", LogMessageType.Error);
                                return true;
                            }

                            return true;
                        }
                    }

                    if (position.CloseActive)
                    {
                        if (string.IsNullOrEmpty(position.CloseOrders[^1].NumberMarket))
                        {
                            if (position.CloseOrders[^1].State == OrderStateType.None
                                && _lastNoneOrderTime == DateTime.MinValue)
                            {
                                _lastNoneOrderTime = DateTime.Now;
                            }
                            else if (position.CloseOrders[^1].State == OrderStateType.None
                                && _lastNoneOrderTime.AddMinutes(5) < DateTime.Now)
                            {// 5ть минут висит ордер со статусом NONE. Утерян
                                position.CloseOrders.RemoveAt(position.CloseOrders.Count - 1);
                                SendNewLogMessage("Remove NONE close order. Five minutes rule", LogMessageType.Error);
                                return true;
                            }

                            return true;
                        }
                    }
                }
            }

            if (_lastNoneOrderTime != DateTime.MinValue)
            {
                _lastNoneOrderTime = DateTime.MinValue;
            }

            return false;
        }

        private DateTime _lastNoneOrderTime;

        public bool HaveOrdersTryToCancelLastSecond()
        {
            // возвращает true - если есть ордер который уже отослан на отзыв но всё ещё в статусе Active. За последние 3 секунды.
            // если true - значит последние операции ещё не завершены по снятию ордеров

            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return false;
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;

            for (int i = 0; linesAll != null && i < linesAll.Count; i++)
            {
                if (linesAll[i].Position != null)
                {
                    Position position = linesAll[i].Position;

                    if (position.OpenActive)
                    {
                        if (position.OpenOrders[^1].State == OrderStateType.Active
                            && position.OpenOrders[^1].IsSendToCancel == true)
                        {
                            if (position.OpenOrders[^1].LastCancelTryLocalTime.AddSeconds(3) > DateTime.Now)
                            {
                                return true;
                            }
                        }
                    }

                    if (position.CloseActive)
                    {
                        if (position.CloseOrders[^1].State == OrderStateType.Active
                            && position.CloseOrders[^1].IsSendToCancel == true)
                        {
                            if (position.CloseOrders[^1].LastCancelTryLocalTime.AddSeconds(3) > DateTime.Now)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool HaveCloseOrders
        {
            get
            {
                // 1 если уже есть позиции с ордерами на закрытие. Ничего не делаем

                List<TradeGridLine> linesWithOpenPositions = GetLinesWithOpenPosition();

                for (int i = 0; i < linesWithOpenPositions.Count; i++)
                {
                    Position pos = linesWithOpenPositions[i].Position;

                    if (pos == null)
                    {
                        continue;
                    }

                    if (pos.CloseActive == true)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool HaveOpenPositionsByGrid
        {
            get
            {
                List<TradeGridLine> linesWithPositions = GetLinesWithOpenPosition();

                if (linesWithPositions != null &&
                    linesWithPositions.Count != 0)
                {
                    return true;
                }

                return false;
            }
        }

        public bool HaveOrdersInMarketInGrid
        {
            get
            {
                List<TradeGridLine> linesWithOpenOrders = GetLinesWithOpenOrdersFact();
                List<TradeGridLine> linesWithCloseOrders = GetLinesWithClosingOrdersFact();

                if (linesWithOpenOrders != null
                    && linesWithOpenOrders.Count > 0)
                {
                    return true;
                }
                if (linesWithCloseOrders != null
                  && linesWithCloseOrders.Count > 0)
                {
                    return true;
                }

                return false;
            }
        }

        public decimal MiddleEntryPrice
        {
            get
            {
                // 1 берём позиции по сетке

                List<Position> positions = GetPositionByGrid();

                if (positions == null
                    || positions.Count == 0)
                {
                    return 0;
                }

                // 2 берём из позиций все MyTrade по открывающим ордерам

                List<MyTrade> tradesOpenPos = new List<MyTrade>();

                for (int i = 0; i < positions.Count; i++)
                {
                    Position currentPos = positions[i];

                    if (currentPos == null)
                    {
                        continue;
                    }

                    List<Order> orders = currentPos.OpenOrders;

                    if (orders == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < orders.Count; j++)
                    {
                        Order currentOrder = orders[j];

                        if (currentOrder == null)
                        {
                            continue;
                        }

                        List<MyTrade> myTrades = currentOrder.MyTrades;

                        if (myTrades == null
                            || myTrades.Count == 0)
                        {
                            continue;
                        }

                        tradesOpenPos.AddRange(myTrades);
                    }
                }

                if (tradesOpenPos.Count == 0)
                {
                    return 0;
                }

                // 3 считаем среднюю цену входа

                decimal summ = 0;
                decimal volume = 0;

                for (int i = 0; i < tradesOpenPos.Count; i++)
                {
                    MyTrade trade = tradesOpenPos[i];

                    if (trade == null)
                    {
                        continue;
                    }

                    volume += trade.Volume;
                    summ += trade.Volume * trade.Price;
                }

                decimal result = summ / volume;

                return result;
            }
        }

        public decimal MaxGridPrice
        {
            get
            {
                try
                {
                    TrailingUp trailingUp = TrailingUp;
                    if (trailingUp == null)
                    {
                        return 0;
                    }

                    return trailingUp.MaxGridPrice;
                }
                catch (Exception e)
                {
                    SendNewLogMessage(e.ToString(), LogMessageType.Error);
                    return 0;
                }
            }
        }

        public decimal MinGridPrice
        {
            get
            {
                try
                {
                    TrailingUp trailingUp = TrailingUp;
                    if (trailingUp == null)
                    {
                        return 0;
                    }

                    return trailingUp.MinGridPrice;
                }
                catch (Exception e)
                {
                    SendNewLogMessage(e.ToString(), LogMessageType.Error);
                    return 0;
                }
            }
        }

        public List<TradeGridLine> GetLinesWithOpenPosition()
        {
            List<TradeGridLine> linesWithPositionFact = new List<TradeGridLine>();
            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return linesWithPositionFact;
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;
            if (linesAll == null || linesAll.Count == 0)
            {
                return linesWithPositionFact;
            }

            for (int i = 0; linesAll != null && i < linesAll.Count; i++)
            {
                if (linesAll[i].Position != null
                    && linesAll[i].Position.OpenVolume != 0)
                {
                    linesWithPositionFact.Add(linesAll[i]);
                }
            }
            return linesWithPositionFact;
        }

        public List<Position> GetPositionByGrid()
        {
            List<Position> positions = new List<Position>();

            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return positions;
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;

            if (linesAll == null ||
                linesAll.Count == 0)
            {
                return positions;
            }

            for (int i = 0; linesAll != null && i < linesAll.Count; i++)
            {
                Position position = linesAll[i].Position;

                if (position != null)
                {
                    positions.Add(position);
                }
            }
            return positions;
        }

        public List<TradeGridLine> GetLinesWithOpenOrdersNeed(decimal lastPrice)
        {
            List<TradeGridLine> linesWithOrdersToOpenNeed = new List<TradeGridLine>();
            TradeGridCreator gridCreator = GridCreator;
            BotTabSimple tab = Tab;

            if (gridCreator == null || tab == null)
            {
                return linesWithOrdersToOpenNeed;
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;
            if (linesAll == null || linesAll.Count == 0)
            {
                return linesWithOrdersToOpenNeed;
            }

            decimal maxPriceUp = 0;
            decimal minPriceDown = 0;

            if (tab.StartProgram == StartProgram.IsOsTrader
                && MaxDistanceToOrdersPercent != 0)
            {
                maxPriceUp = lastPrice + lastPrice * (MaxDistanceToOrdersPercent / 100);
                minPriceDown = lastPrice - lastPrice * (MaxDistanceToOrdersPercent / 100);
            }

            if (gridCreator.GridSide == Side.Buy)
            {
                for (int i = 0; i < linesAll.Count; i++)
                {
                    TradeGridLine curLine = linesAll[i];

                    Position position = curLine.Position;

                    if (position != null
                        && position.OpenVolume > 0
                        && position.OpenActive == false)
                    {
                        continue;
                    }

                    if (tab.Security.PriceLimitHigh != 0
                        && tab.Security.PriceLimitLow != 0)
                    {
                        if (OpenOrdersMakerOnly == true
                            &&
                            (curLine.PriceEnter > tab.Security.PriceLimitHigh
                            || curLine.PriceEnter < tab.Security.PriceLimitLow))
                        {
                            continue;
                        }

                        if (OpenOrdersMakerOnly == false
                            && curLine.Side == Side.Buy
                            && curLine.PriceEnter < tab.Security.PriceLimitLow)
                        {
                            continue;
                        }
                        if (OpenOrdersMakerOnly == false
                            && curLine.Side == Side.Sell
                            && curLine.PriceEnter > tab.Security.PriceLimitHigh)
                        {
                            continue;
                        }
                    }

                    if (maxPriceUp != 0
                        && minPriceDown != 0)
                    {
                        if (curLine.PriceEnter > maxPriceUp
                         || curLine.PriceEnter < minPriceDown)
                        {
                            continue;
                        }
                    }

                    if (OpenOrdersMakerOnly
                        && curLine.PriceEnter > lastPrice)
                    {
                        continue;
                    }

                    linesWithOrdersToOpenNeed.Add(curLine);

                    if (linesWithOrdersToOpenNeed.Count >= MaxOpenOrdersInMarket)
                    {
                        break;
                    }
                }
            }
            else if (gridCreator.GridSide == Side.Sell)
            {
                for (int i = 0; i < linesAll.Count; i++)
                {
                    TradeGridLine curLine = linesAll[i];

                    Position position = curLine.Position;

                    if (position != null
                        && position.OpenVolume > 0
                        && position.OpenActive == false)
                    {
                        continue;
                    }

                    if (tab.Security.PriceLimitHigh != 0
                        && tab.Security.PriceLimitLow != 0)
                    {
                        if (curLine.PriceEnter > tab.Security.PriceLimitHigh
                            || curLine.PriceEnter < tab.Security.PriceLimitLow)
                        {
                            continue;
                        }
                    }

                    if (maxPriceUp != 0
                        && minPriceDown != 0)
                    {
                        if (curLine.PriceEnter > maxPriceUp
                         || curLine.PriceEnter < minPriceDown)
                        {
                            continue;
                        }
                    }

                    if (OpenOrdersMakerOnly
                        && curLine.PriceEnter < lastPrice)
                    {
                        continue;
                    }

                    linesWithOrdersToOpenNeed.Add(curLine);

                    if (linesWithOrdersToOpenNeed.Count >= MaxOpenOrdersInMarket)
                    {
                        break;
                    }
                }
            }
            return linesWithOrdersToOpenNeed;
        }

        public List<TradeGridLine> GetLinesWithOpenOrdersFact()
        {
            List<TradeGridLine> linesWithOpenOrder = new List<TradeGridLine>();
            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return linesWithOpenOrder;
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;
            if (linesAll == null || linesAll.Count == 0)
            {
                return linesWithOpenOrder;
            }

            for (int i = 0; linesAll != null && i < linesAll.Count; i++)
            {
                if (linesAll[i].Position != null
                    && linesAll[i].Position.OpenActive)
                {
                    linesWithOpenOrder.Add(linesAll[i]);
                }
            }
            return linesWithOpenOrder;
        }

        public List<TradeGridLine> GetLinesWithClosingOrdersFact()
        {
            List<TradeGridLine> linesWithCloseOrder = new List<TradeGridLine>();
            TradeGridCreator gridCreator = GridCreator;
            if (gridCreator == null)
            {
                return linesWithCloseOrder;
            }

            List<TradeGridLine> linesAll = gridCreator.Lines;
            if (linesAll == null || linesAll.Count == 0)
            {
                return linesWithCloseOrder;
            }

            for (int i = 0; linesAll != null && i < linesAll.Count; i++)
            {
                if (linesAll[i].Position != null
                    && linesAll[i].Position.CloseActive)
                {
                    linesWithCloseOrder.Add(linesAll[i]);
                }
            }
            return linesWithCloseOrder;
        }

        #endregion

        #region Log

        public void SendNewLogMessage(string message, LogMessageType type)
        {
            if (type == LogMessageType.Error)
            {
                string botName = Tab?.NameStrategy ?? "unknown";
                string securityName = Tab?.Connector?.SecurityName ?? "unknown";

                message = "Grid error. Bot: " + botName + "\n"
                + "Security name: " + securityName + "\n"
                + message;
            }

            LogMessageEvent?.Invoke(message, type);

            if (LogMessageEvent == null && type == LogMessageType.Error)
            {
                ServerMaster.SendNewLogMessage(message, type);
            }
        }

        public event Action<string, LogMessageType>? LogMessageEvent;

        #endregion
    }

    public enum TradeGridPrimeType
    {
        MarketMaking,
        OpenPosition
    }

    public enum TradeGridRegime
    {
        Off,
        OffAndCancelOrders,
        On,
        CloseOnly,
        CloseForced
    }

    public enum TradeGridLogicEntryRegime
    {
        OnTrade,
        OncePerSecond
    }

    public enum OnOffRegime
    {
        On,
        Off
    }

    public enum TradeGridValueType
    {
        Absolute,
        Percent,
    }

    public enum TradeGridVolumeType
    {
        Contracts,
        ContractCurrency,
        DepositPercent
    }
}

