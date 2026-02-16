/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Entity;
using OsEngine.Language;
using OsEngine.Logging;
using OsEngine.Market;
using OsEngine.Market.Servers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OsEngine.OsTrader.Panels.Tab.Internal
{

    /// <summary>
    /// Manual position support settings
    /// </summary>
    public class BotManualControl
    {
        // thread work part

        /// <summary>
        /// Revocation thread
        /// </summary>
        public static Task Watcher;

        /// <summary>
        /// Tabs that need to be checked
        /// </summary>
        public static List<BotManualControl> TabsToCheck = new List<BotManualControl>();

        private static readonly Lock _tabsAddLocker = new();

        private static readonly Lock _activatorLocker = new();

        /// <summary>
        /// Activate stream to view deals
        /// </summary>
        public static void Activate()
        {
            lock (_activatorLocker)
            {
                if (Watcher != null)
                {
                    return;
                }

                Watcher = new Task(WatcherHome);
                Watcher.Start();
            }
        }

        /// <summary>
        /// Place of work thread that monitors the execution of transactions
        /// </summary>
        public static async void WatcherHome()
        {
            while (true)
            {
                await Task.Delay(1000);

                for (int i = 0; i < TabsToCheck.Count; i++)
                {
                    if (TabsToCheck[i] == null)
                    {
                        continue;
                    }
                    TabsToCheck[i].CheckPositions();
                }

                if (!MainWindow.ProccesIsWorked)
                {
                    return;
                }
            }
        }

        private string _name;

        /// <summary>
        /// Constructor
        /// </summary>
        public BotManualControl(string name, BotTabSimple botTab,StartProgram startProgram)
        {
            _name = name;
            _startProgram = startProgram;

            StopIsOn = false;
            ProfitIsOn = false;
            DoubleExitIsOn = true;
            SecondToOpenIsOn = true;
            SecondToCloseIsOn = true;
            SetbackToOpenIsOn = false;
            SetbackToCloseIsOn = false;

            StopDistance = 30;
            StopSlippage = 5;
           
            ProfitDistance = 30;
            ProfitSlippage = 5;
           
            DoubleExitSlippage = 10;
            SecondToOpen = new TimeSpan(0, 0, 0, 50);
            SecondToClose = new TimeSpan(0, 0, 0, 50);
            SetbackToOpenPosition = 10;

            SetbackToClosePosition = 10;

            if (Load() == false)
            {
                Save();
            }

            _botTab = botTab;

            if (_startProgram == StartProgram.IsOsTrader)
            {
                if (Watcher == null)
                {
                    Activate();
                }

                lock(_tabsAddLocker)
                {
                    TabsToCheck.Add(this);
                }
                
            }
        }

        /// <summary>
        /// Load
        /// </summary>
        private bool Load()
        {
            string path = GetSettingsPath();

            if (!File.Exists(path))
            {
                return false;
            }
            try
            {
                BotManualControlSettingsDto settings = SettingsManager.Load(
                    path,
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return true;
                }

                StopIsOn = settings.StopIsOn;
                StopDistance = settings.StopDistance;
                StopSlippage = settings.StopSlippage;
                ProfitIsOn = settings.ProfitIsOn;
                ProfitDistance = settings.ProfitDistance;
                ProfitSlippage = settings.ProfitSlippage;
                _secondToOpen = settings.SecondToOpen;
                _secondToClose = settings.SecondToClose;
                DoubleExitIsOn = settings.DoubleExitIsOn;
                SecondToOpenIsOn = settings.SecondToOpenIsOn;
                SecondToCloseIsOn = settings.SecondToCloseIsOn;
                SetbackToOpenIsOn = settings.SetbackToOpenIsOn;
                SetbackToOpenPosition = settings.SetbackToOpenPosition;
                SetbackToCloseIsOn = settings.SetbackToCloseIsOn;
                SetbackToClosePosition = settings.SetbackToClosePosition;
                DoubleExitSlippage = settings.DoubleExitSlippage;
                TypeDoubleExitOrder = settings.TypeDoubleExitOrder;
                ValuesType = settings.ValuesType;
                OrderTypeTime = settings.OrderTypeTime;
                LimitsMakerOnly = settings.LimitsMakerOnly;
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
            return true;
        }

        /// <summary>
        /// Save
        /// </summary>
        public void Save()
        {
            if(_startProgram == StartProgram.IsOsOptimizer)
            {
                return;
            }

            try
            {
                string path = GetSettingsPath();

                string dir = Path.GetDirectoryName(path);

                if(Directory.Exists(dir) == false)
                {
                    Directory.CreateDirectory(dir);
                }

                SettingsManager.Save(
                    path,
                    new BotManualControlSettingsDto
                    {
                        StopIsOn = StopIsOn,
                        StopDistance = StopDistance,
                        StopSlippage = StopSlippage,
                        ProfitIsOn = ProfitIsOn,
                        ProfitDistance = ProfitDistance,
                        ProfitSlippage = ProfitSlippage,
                        SecondToOpen = _secondToOpen,
                        SecondToClose = _secondToClose,
                        DoubleExitIsOn = DoubleExitIsOn,
                        SecondToOpenIsOn = SecondToOpenIsOn,
                        SecondToCloseIsOn = SecondToCloseIsOn,
                        SetbackToOpenIsOn = SetbackToOpenIsOn,
                        SetbackToOpenPosition = SetbackToOpenPosition,
                        SetbackToCloseIsOn = SetbackToCloseIsOn,
                        SetbackToClosePosition = SetbackToClosePosition,
                        DoubleExitSlippage = DoubleExitSlippage,
                        TypeDoubleExitOrder = TypeDoubleExitOrder,
                        ValuesType = ValuesType,
                        OrderTypeTime = OrderTypeTime,
                        LimitsMakerOnly = LimitsMakerOnly
                    });
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        private string GetSettingsPath()
        {
            return @"Engine\" + _name + @"StrategSettings.txt";
        }

        private static BotManualControlSettingsDto ParseLegacySettings(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string normalized = content.Replace("\r", string.Empty);
            string[] lines = normalized.Split('\n');

            if (lines.Length > 0 && lines[lines.Length - 1] == string.Empty)
            {
                Array.Resize(ref lines, lines.Length - 1);
            }

            TimeSpan secondToOpen = TimeSpan.Zero;
            TimeSpan.TryParse(GetLineAt(lines, 6), out secondToOpen);

            TimeSpan secondToClose = TimeSpan.Zero;
            TimeSpan.TryParse(GetLineAt(lines, 7), out secondToClose);

            OrderPriceType typeDoubleExitOrder = OrderPriceType.Limit;
            Enum.TryParse(GetLineAt(lines, 16), out typeDoubleExitOrder);

            ManualControlValuesType valuesType = ManualControlValuesType.MinPriceStep;
            Enum.TryParse(GetLineAt(lines, 17), out valuesType);

            OrderTypeTime orderTypeTime = OrderTypeTime.Specified;
            Enum.TryParse(GetLineAt(lines, 18), out orderTypeTime);

            bool limitsMakerOnly = false;
            string makerOnly = GetLineAt(lines, 19);
            if (!string.IsNullOrEmpty(makerOnly))
            {
                bool.TryParse(makerOnly, out limitsMakerOnly);
            }

            return new BotManualControlSettingsDto
            {
                StopIsOn = TryParseBool(GetLineAt(lines, 0)),
                StopDistance = TryParseDecimal(GetLineAt(lines, 1)),
                StopSlippage = TryParseDecimal(GetLineAt(lines, 2)),
                ProfitIsOn = TryParseBool(GetLineAt(lines, 3)),
                ProfitDistance = TryParseDecimal(GetLineAt(lines, 4)),
                ProfitSlippage = TryParseDecimal(GetLineAt(lines, 5)),
                SecondToOpen = secondToOpen,
                SecondToClose = secondToClose,
                DoubleExitIsOn = TryParseBool(GetLineAt(lines, 8)),
                SecondToOpenIsOn = TryParseBool(GetLineAt(lines, 9)),
                SecondToCloseIsOn = TryParseBool(GetLineAt(lines, 10)),
                SetbackToOpenIsOn = TryParseBool(GetLineAt(lines, 11)),
                SetbackToOpenPosition = TryParseDecimal(GetLineAt(lines, 12)),
                SetbackToCloseIsOn = TryParseBool(GetLineAt(lines, 13)),
                SetbackToClosePosition = TryParseDecimal(GetLineAt(lines, 14)),
                DoubleExitSlippage = TryParseDecimal(GetLineAt(lines, 15)),
                TypeDoubleExitOrder = typeDoubleExitOrder,
                ValuesType = valuesType,
                OrderTypeTime = orderTypeTime,
                LimitsMakerOnly = limitsMakerOnly
            };
        }

        private static string GetLineAt(string[] lines, int index)
        {
            if (lines == null || index >= lines.Length)
            {
                return string.Empty;
            }

            return lines[index];
        }

        private static bool TryParseBool(string value)
        {
            return !string.IsNullOrEmpty(value)
                && value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private static decimal TryParseDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            return value.ToDecimal();
        }

        private sealed class BotManualControlSettingsDto
        {
            public bool StopIsOn { get; set; }
            public decimal StopDistance { get; set; }
            public decimal StopSlippage { get; set; }
            public bool ProfitIsOn { get; set; }
            public decimal ProfitDistance { get; set; }
            public decimal ProfitSlippage { get; set; }
            public TimeSpan SecondToOpen { get; set; }
            public TimeSpan SecondToClose { get; set; }
            public bool DoubleExitIsOn { get; set; }
            public bool SecondToOpenIsOn { get; set; }
            public bool SecondToCloseIsOn { get; set; }
            public bool SetbackToOpenIsOn { get; set; }
            public decimal SetbackToOpenPosition { get; set; }
            public bool SetbackToCloseIsOn { get; set; }
            public decimal SetbackToClosePosition { get; set; }
            public decimal DoubleExitSlippage { get; set; }
            public OrderPriceType TypeDoubleExitOrder { get; set; }
            public ManualControlValuesType ValuesType { get; set; }
            public OrderTypeTime OrderTypeTime { get; set; }
            public bool LimitsMakerOnly { get; set; }
        }

        /// <summary>
        /// Delete
        /// </summary>
        public void Delete()
        {
            try
            {
                string path = GetSettingsPath();

                if (File.Exists(path))
                {
                    FileInfo file = new FileInfo(path);
                    if(file.IsReadOnly)
                    {
                        file.IsReadOnly = false;
                    }

                    File.Delete(path);
                }

                if(TabsToCheck != null)
                {
                    lock (_tabsAddLocker)
                    {
                        for (int i = 0; i < TabsToCheck.Count; i++)
                        {
                            if (TabsToCheck[i]._name == this._name)
                            {
                                TabsToCheck.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }

                if(_botTab != null)
                {
                    _botTab = null;
                }
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        /// <summary>
        /// Show settings
        /// </summary>
        public void ShowDialog(StartProgram startProgram)
        {
            IServerPermission serverPermission = null;

            if (_botTab != null)
            {
                IServer server = _botTab.Connector.MyServer;

                if (server != null)
                {
                    serverPermission = ServerMaster.GetServerPermission(server.ServerType);
                }
            }

            BotManualControlUi ui = new BotManualControlUi(this, startProgram, serverPermission);
            ui.ShowDialog();
        }

        /// <summary>
        /// Program that created the robot
        /// </summary>
        public StartProgram _startProgram;

        /// <summary>
        /// Disable all support functions
        /// </summary>
        public void DisableManualSupport()
        {
            bool valueIsChanged = false;

            if(DoubleExitIsOn == true)
            {
                DoubleExitIsOn = false;
                valueIsChanged = true;
            }
          
            if(ProfitIsOn == true)
            {
                ProfitIsOn = false;
                valueIsChanged = true;
            }
            
            if(SecondToCloseIsOn == true)
            {
                SecondToCloseIsOn = false;
                valueIsChanged = true;
            }
            
            if (SecondToOpenIsOn == true)
            {
                SecondToOpenIsOn = false;
                valueIsChanged = true;
            }
            
            if(SetbackToCloseIsOn == true)
            {
                SetbackToCloseIsOn = false;
                valueIsChanged = true;
            }
            
            if(SetbackToOpenIsOn == true)
            {
                SetbackToOpenIsOn = false;
                valueIsChanged = true;
            }
            
            if(StopIsOn == true)
            {
                StopIsOn = false;
                valueIsChanged = true;
            }
            
            if(valueIsChanged == true)
            {
                Save();
            }
        }

        /// <summary>
        /// Stop is enabled
        /// </summary>
        public bool StopIsOn;

        /// <summary>
        /// Distance from entry to stop 
        /// </summary>
        public decimal StopDistance;

        /// <summary>
        /// Slippage for stop
        /// </summary>
        public decimal StopSlippage;

        /// <summary>
        /// Profit is enabled
        /// </summary>
        public bool ProfitIsOn;

        /// <summary>
        /// Distance from trade entry to order profit
        /// </summary>
        public decimal ProfitDistance;

        /// <summary>
        /// Slippage
        /// </summary>
        public decimal ProfitSlippage;

        /// <summary>
        /// Open orders life time is enabled
        /// </summary>
        public bool SecondToOpenIsOn;

        /// <summary>
        /// Time to open a position in seconds, after which the order will be recalled
        /// </summary>
        public TimeSpan SecondToOpen
        {
            get
            {
                if (SecondToOpenIsOn)
                {
                    return _secondToOpen;
                }
                else
                {
                    return new TimeSpan(1, 0, 0, 0);
                }
            }
            set
            {
                _secondToOpen = value;
            }
        }
        private TimeSpan _secondToOpen;

        /// <summary>
        /// Closed orders life time is enabled
        /// </summary>
        public bool SecondToCloseIsOn;

        /// <summary>
        /// Time to close a position in seconds, after which the order will be recalled
        /// </summary>
        public TimeSpan SecondToClose
        {
            get
            {
                if (SecondToCloseIsOn)
                {
                    return _secondToClose;
                }
                else
                {
                    return new TimeSpan(1, 0, 0, 0);
                }
            }
            set
            {
                _secondToClose = value;
            }
        }
        private TimeSpan _secondToClose;

        /// <summary>
        /// Whether re-issuance of the request for closure is included if the first has been withdrawn
        /// </summary>
        public bool DoubleExitIsOn;

        /// <summary>
        /// Type of re-request for closure
        /// </summary>
        public OrderPriceType TypeDoubleExitOrder;

        /// <summary>
        /// Slip to re-close
        /// </summary>
        public decimal DoubleExitSlippage;

        /// <summary>
        /// Is revocation of orders for opening on price rollback included
        /// </summary>
        public bool SetbackToOpenIsOn;

        /// <summary>
        /// Maximum rollback from order price when opening a position
        /// </summary>
        public decimal SetbackToOpenPosition;

        /// <summary>
        /// Whether revocation of orders for closing on price rollback is included
        /// </summary>
        public bool SetbackToCloseIsOn;

        /// <summary>
        /// Maximum rollback from order price when opening a position
        /// </summary>
        public decimal SetbackToClosePosition;

        public ManualControlValuesType ValuesType;

        /// <summary>
        /// Order lifetime type
        /// </summary>
        public OrderTypeTime OrderTypeTime = OrderTypeTime.Specified;

        public bool LimitsMakerOnly = false;

        /// <summary>
        /// Journal
        /// </summary>
        private BotTabSimple _botTab;

        public DateTime ServerTime = DateTime.MinValue;

        /// <summary>
        /// The method in which the thread monitors the execution of orders
        /// </summary>
        private void CheckPositions()
        {
            if (MainWindow.ProccesIsWorked == false)
            {
                return;
            }

            if (ServerTime == DateTime.MinValue)
            {
                return;
            }

            if (_startProgram != StartProgram.IsOsTrader)
            {
                return;
            }

            try
            {
                if(_botTab == null)
                {
                    return;
                }

                List<Position> openDeals = _botTab.PositionsOpenAll;

                if (openDeals == null)
                {
                    return;
                }

                for (int i = 0; i < openDeals.Count; i++)
                {
                    Position position = null;
                    try
                    {
                        position = openDeals[i];
                    }
                    catch
                    {
                        continue;
                        // ignore
                    }

                    if(position == null)
                    {
                        continue;
                    }    
                    

                    for (int i2 = 0; position.OpenOrders != null && i2 < position.OpenOrders.Count; i2++)
                    {
                        // open orders
                        Order openOrder = position.OpenOrders[i2];

                        if(openOrder == null)
                        {
                            continue;
                        }

                        if (openOrder.State != OrderStateType.Active &&
                            openOrder.State != OrderStateType.Partial &&
                            openOrder.State != OrderStateType.Pending)
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(openOrder.NumberMarket))
                        {
                            continue;
                        }

                        if (IsInArray(openOrder))
                        {
                            continue;
                        }

                        if(openOrder.OrderTypeTime == OrderTypeTime.Specified)
                        {
                            if (SecondToOpenIsOn &&
                                openOrder.TimeCreate.Add(openOrder.LifeTime) < ServerTime)
                            {
                                SendNewLogMessage(OsLocalization.Trader.Label70 + openOrder.NumberMarket,
                                    LogMessageType.Trade);
                                SendOrderToClose(openOrder, position);
                            }
                        }

                        if (SetbackToOpenIsOn &&
                            openOrder.Side == Side.Buy)
                        {
                            decimal maxSpread = GetMaxSpread(openOrder);

                            if (Math.Abs(_botTab.PriceBestBid - openOrder.Price) > maxSpread)
                            {
                                SendNewLogMessage(OsLocalization.Trader.Label157 + openOrder.NumberMarket,
                                    LogMessageType.Trade);
                                SendOrderToClose(openOrder, position);
                            }
                        }

                        if (SetbackToOpenIsOn &&
                            openOrder.Side == Side.Sell)
                        {
                            decimal maxSpread = GetMaxSpread(openOrder);

                            if (Math.Abs(openOrder.Price - _botTab.PriceBestAsk) > maxSpread)
                            {
                                SendNewLogMessage(OsLocalization.Trader.Label157 + openOrder.NumberMarket,
                                    LogMessageType.Trade);
                                SendOrderToClose(openOrder, position);
                            }
                        }
                    }

                    for (int i2 = 0; position.CloseOrders != null && i2 < position.CloseOrders.Count; i2++)
                    {
                        // close orders
                        Order closeOrder = position.CloseOrders[i2];

                        if(closeOrder == null)
                        {
                            continue;
                        }

                        if (closeOrder.State != OrderStateType.Active 
                            && closeOrder.State != OrderStateType.Partial
                            && closeOrder.State != OrderStateType.Pending)
                        {
                            continue;
                        }

                        if(string.IsNullOrEmpty(closeOrder.NumberMarket))
                        {
                            continue;
                        }

                        if (IsInArray(closeOrder))
                        {
                            continue;
                        }

                        if (closeOrder.OrderTypeTime == OrderTypeTime.Specified)
                        {
                            if (SecondToCloseIsOn &&
                            closeOrder.TimeCreate.Add(closeOrder.LifeTime) < ServerTime)
                            {
                                SendNewLogMessage(OsLocalization.Trader.Label70 + closeOrder.NumberMarket,
                                    LogMessageType.Trade);
                                SendOrderToClose(closeOrder, position);
                            }
                        }

                        if (SetbackToCloseIsOn &&
                            closeOrder.Side == Side.Buy)
                        {
                            decimal priceRedLine = closeOrder.Price -
                                                   _botTab.Security.PriceStep * SetbackToClosePosition;

                            if (_botTab.PriceBestBid <= priceRedLine)
                            {
                                SendNewLogMessage(OsLocalization.Trader.Label157 + closeOrder.NumberMarket,
                                    LogMessageType.Trade);
                                SendOrderToClose(closeOrder, position);
                            }
                        }

                        if (SetbackToCloseIsOn &&
                            closeOrder.Side == Side.Sell)
                        {
                            decimal priceRedLine = closeOrder.Price +
                                                   _botTab.Security.PriceStep * SetbackToClosePosition;

                            if (_botTab.PriceBestAsk >= priceRedLine)
                            {
                                SendNewLogMessage(OsLocalization.Trader.Label157 + closeOrder.NumberMarket,
                                    LogMessageType.Trade);
                                SendOrderToClose(closeOrder, position);
                            }
                        }
                    }
                }
            }
            catch
                (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        /// <summary>
        /// Try to set a stop and profit
        /// </summary>
        public void TryReloadStopAndProfit(BotTabSimple bot, Position position)
        {
            if (StopIsOn)
            {
                if (position.Direction == Side.Buy)
                {
                    decimal priceRedLine = position.EntryPrice - GetStopDistance(position,bot.Security);
                    decimal priceOrder = priceRedLine - GetStopSlippageDistance(position, bot.Security);

                    bot.CloseAtStop(position, priceRedLine, priceOrder);
                }

                if (position.Direction == Side.Sell)
                {
                    decimal priceRedLine = position.EntryPrice + GetStopDistance(position, bot.Security);
                    decimal priceOrder = priceRedLine + GetStopSlippageDistance(position, bot.Security);

                    bot.CloseAtStop(position, priceRedLine, priceOrder);
                }
            }

            if (ProfitIsOn)
            {
                if (position.Direction == Side.Buy)
                {
                    decimal priceRedLine = position.EntryPrice + GetProfitDistance(position, bot.Security);
                    decimal priceOrder = priceRedLine - GetProfitDistanceSlippage(position, bot.Security);

                    bot.CloseAtProfit(position, priceRedLine, priceOrder);
                }

                if (position.Direction == Side.Sell)
                {
                    decimal priceRedLine = position.EntryPrice - GetProfitDistance(position, bot.Security);
                    decimal priceOrder = priceRedLine + GetProfitDistanceSlippage(position, bot.Security);

                    bot.CloseAtProfit(position, priceRedLine, priceOrder);
                }
            }
        }

        /// <summary>
        /// Attempt to close the position
        /// </summary>
        public void TryEmergencyClosePosition(BotTabSimple bot, Position position)
        {
            if (TypeDoubleExitOrder == OrderPriceType.Market)
            {
                bot.CloseAtMarket(position, position.OpenVolume, OsLocalization.Trader.Label410);
            }
            else if (TypeDoubleExitOrder == OrderPriceType.Limit)
            {
                decimal price;
                if (position.Direction == Side.Buy)
                {
                    price = bot.PriceBestBid - GetEmergencyExitDistance(bot, position);
                }
                else
                {
                    price = bot.PriceBestAsk + GetEmergencyExitDistance(bot, position);
                }

                bot.CloseAtLimit(position, price, position.OpenVolume, OsLocalization.Trader.Label410);
            }
        }

        /// <summary>
        /// Get slippage
        /// </summary>
        private decimal GetEmergencyExitDistance(BotTabSimple bot, Position position)
        {
            decimal result = 0;

            if (ValuesType == ManualControlValuesType.MinPriceStep)
            {
                result = bot.Security.PriceStep * DoubleExitSlippage;
            }
            else if (ValuesType == ManualControlValuesType.Absolute)
            {
                result = DoubleExitSlippage;
            }
            else if (ValuesType == ManualControlValuesType.Percent)
            {
                if (position.Direction == Side.Buy)
                {
                    result = bot.PriceBestBid * DoubleExitSlippage / 100;
                }
                else
                {
                    result = bot.PriceBestAsk * DoubleExitSlippage / 100;
                }
            }

            return result;
        }

        /// <summary>
        /// Get slippage for profit
        /// </summary>
        public decimal GetProfitDistanceSlippage(Position position,Security security)
        {
            decimal result = 0;

            if (ValuesType == ManualControlValuesType.MinPriceStep)
            {
                result = security.PriceStep * ProfitSlippage;
            }
            else if (ValuesType == ManualControlValuesType.Absolute)
            {
                result = ProfitSlippage;
            }
            else if (ValuesType == ManualControlValuesType.Percent)
            {
                result = position.EntryPrice * ProfitSlippage / 100;
            }

            return result;
        }

        /// <summary>
        /// Get distance for profit
        /// </summary>
        public decimal GetProfitDistance(Position position, Security security)
        {
            decimal result = 0;

            if (ValuesType == ManualControlValuesType.MinPriceStep)
            {
                result = security.PriceStep * ProfitDistance;
            }
            else if (ValuesType == ManualControlValuesType.Absolute)
            {
                result = ProfitDistance;
            }
            else if (ValuesType == ManualControlValuesType.Percent)
            {
                result = position.EntryPrice * ProfitDistance / 100;
            }

            return result;
        }

        /// <summary>
        /// Get stop distance
        /// </summary>
        private decimal GetStopDistance(Position position, Security security)
        {
            decimal result = 0;

            if (ValuesType == ManualControlValuesType.MinPriceStep)
            {
                result = security.PriceStep * StopDistance;
            }
            else if (ValuesType == ManualControlValuesType.Absolute)
            {
                result = StopDistance;
            }
            else if (ValuesType == ManualControlValuesType.Percent)
            {
                result = position.EntryPrice * StopDistance / 100;
            }

            return result;
        }

        /// <summary>
        /// Get slippage for a stop
        /// </summary>
        private decimal GetStopSlippageDistance(Position position, Security security)
        {
            decimal result = 0;

            if (ValuesType == ManualControlValuesType.MinPriceStep)
            {
                result = security.PriceStep * StopSlippage;
            }
            else if (ValuesType == ManualControlValuesType.Absolute)
            {
                result = StopSlippage;
            }
            else if (ValuesType == ManualControlValuesType.Percent)
            {
                result = position.EntryPrice * StopSlippage / 100;
            }

            return result;
        }

        /// <summary>
        /// Get the maximum spread
        /// </summary>
        private decimal GetMaxSpread(Order order)
        {
            if (_botTab == null)
            {
                return 0;
            }
            decimal maxSpread = 0;

            if (ValuesType == ManualControlValuesType.MinPriceStep)
            {
                maxSpread = _botTab.Security.PriceStep * SetbackToOpenPosition;
            }
            else if (ValuesType == ManualControlValuesType.Absolute)
            {
                maxSpread = SetbackToOpenPosition;
            }
            else if (ValuesType == ManualControlValuesType.Percent)
            {
                maxSpread = order.Price * SetbackToOpenPosition / 100;
            }

            return maxSpread;
        }

        /// <summary>
        /// Orders already sent for closure
        /// </summary>
        private List<Order> _ordersToClose = new List<Order>();

        /// <summary>
        /// Send a review order
        /// </summary>
        /// <param name="order">order</param>
        /// <param name="deal">position of which order belongs</param>
        private void SendOrderToClose(Order order, Position deal)
        {
            if (IsInArray(order))
            {
                return;
            }

            _ordersToClose.Add(order);

            if (DontOpenOrderDetectedEvent != null)
            {
                DontOpenOrderDetectedEvent(order, deal);
            }
        }

        /// <summary>
        /// Is this order already sent for review?
        /// </summary>
        private bool IsInArray(Order order)
        {
            for (int i = 0; i < _ordersToClose.Count; i++)
            {
                if (_ordersToClose[i].NumberUser == order.NumberUser)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// New order for withdrawal event
        /// </summary>
        public event Action<Order, Position> DontOpenOrderDetectedEvent;

        // log

        /// <summary>
        /// Send a new log message
        /// </summary>
        private void SendNewLogMessage(string message, LogMessageType type)
        {
            if (LogMessageEvent != null)
            {
                LogMessageEvent(message, type);
            }
        }

        /// <summary>
        /// Outgoing message for log
        /// </summary>
        public event Action<string, LogMessageType> LogMessageEvent;
    }

    /// <summary>
    /// Type of variables for distance calculation
    /// </summary>
    public enum ManualControlValuesType
    {
        /// <summary>
        /// Minimum instrument price step
        /// </summary>
        MinPriceStep,

        /// <summary>
        /// Absolute values
        /// </summary>
        Absolute,

        /// <summary>
        /// %
        /// </summary>
        Percent
    }
}
