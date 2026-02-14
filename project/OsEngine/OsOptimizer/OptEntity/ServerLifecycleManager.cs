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
using OsEngine.Market.Servers.Optimizer;
using OsEngine.Market.Servers.Tester;
using OsEngine.OsTrader.Panels;
using OsEngine.OsTrader.Panels.Tab;

namespace OsEngine.OsOptimizer.OptEntity
{
    /// <summary>
    /// Server creation, tracking, cleanup, and progress events for optimization.
    /// Создание серверов, отслеживание, очистка и события прогресса для оптимизации.
    /// </summary>
    public class ServerLifecycleManager
    {
        private readonly OptimizerSettings _settings;
        private readonly OptimizerDataStorage _storage;

        public ServerLifecycleManager(OptimizerSettings settings, OptimizerDataStorage storage)
        {
            _settings = settings;
            _storage = storage;
        }

        /// <summary>
        /// Reference bot whose tab configuration is used to set up data on servers.
        /// </summary>
        public BotPanel BotToTest { get; set; }

        public List<OptimizerServer> Servers { get; } = new List<OptimizerServer>();

        public int ServerNum { get; set; } = 1;

        public int CountAllServersMax { get; set; }

        public int CountAllServersEndTest { get; set; }

        public List<BotPanel> BotsInTest { get; } = new List<BotPanel>();

        public List<TimeSpan> TestBotsTime { get; } = new List<TimeSpan>();

        public readonly Lock ServerRemoveLocker = new();

        public OptimizerServer CreateNewServer(OptimizerFazeReport report, bool needToDelete)
        {
            OptimizerServer server = ServerMaster.CreateNextOptimizerServer(_storage, ServerNum,
                _settings.StartDeposit);

            server.OrderExecutionType = _settings.OrderExecutionType;
            server.SlippageToSimpleOrder = _settings.SlippageToSimpleOrder;
            server.SlippageToStopOrder = _settings.SlippageToStopOrder;
            server.ClearingTimes = _settings.ClearingTimes;
            server.NonTradePeriods = _settings.NonTradePeriods;

            lock (ServerRemoveLocker)
            {
                ServerNum++;
                Servers.Add(server);
            }

            if (needToDelete)
            {
                server.TestingEndEvent += OnTestingEnd;
            }

            server.TypeTesterData = _storage.TypeTesterData;
            server.TestingProgressChangeEvent += OnTestingProgressChange;

            List<IIBotTab> sources = BotToTest.GetTabs();

            for (int i = 0; i < sources.Count; i++)
            {
                if (sources[i].TabType == BotTabType.Simple)
                {
                    BotTabSimple simple = (BotTabSimple)sources[i];

                    Security secToStart =
                    _storage.Securities.Find(s => s.Name == simple.Connector.SecurityName);

                    server.GetDataToSecurity(secToStart, simple.Connector.TimeFrame, report.Faze.TimeStart,
                        report.Faze.TimeEnd);
                }
                else if (sources[i].TabType == BotTabType.Index)
                {
                    BotTabIndex index = (BotTabIndex)sources[i];

                    for (int i2 = 0; i2 < index.Tabs.Count; i2++)
                    {
                        Security secToStart =
                          _storage.Securities.Find(s => s.Name == index.Tabs[i2].SecurityName);

                        server.GetDataToSecurity(secToStart, index.Tabs[i2].TimeFrame, report.Faze.TimeStart,
                            report.Faze.TimeEnd);
                    }
                }
                else if (sources[i].TabType == BotTabType.Screener)
                {
                    BotTabScreener screener = (BotTabScreener)sources[i];

                    for (int i2 = 0; i2 < screener.Tabs.Count; i2++)
                    {
                        Security secToStart =
                          _storage.Securities.Find(s => s.Name == screener.Tabs[i2].Connector.SecurityName);

                        server.GetDataToSecurity(secToStart, screener.Tabs[i2].Connector.TimeFrame, report.Faze.TimeStart,
                            report.Faze.TimeEnd);
                    }
                }
            }

            return server;
        }

        public void Reset()
        {
            Servers.Clear();
            BotsInTest.Clear();
            TestBotsTime.Clear();
            CountAllServersMax = 0;
            CountAllServersEndTest = 0;
            ServerNum = 1;
        }

        private void OnTestingEnd(int serverNum, TimeSpan testTime)
        {
            TestingProgressChangeEvent?.Invoke(100, 100, serverNum);
            CountAllServersEndTest++;
            PrimeProgressChangeEvent?.Invoke(CountAllServersEndTest, CountAllServersMax);

            BotPanel bot = null;
            OptimizerServer server = null;

            lock (ServerRemoveLocker)
            {
                for (int i = 0; i < BotsInTest.Count; i++)
                {
                    BotPanel curBot = BotsInTest[i];

                    if (curBot != null
                        && curBot.TabsSimple != null
                        && curBot.TabsSimple.Count > 0
                        && curBot.TabsSimple[0].Connector != null
                        && curBot.TabsSimple[0].Connector.ServerUid == serverNum)
                    {
                        bot = curBot;
                        BotsInTest.RemoveAt(i);
                        break;
                    }
                    else if (curBot != null
                        && curBot.TabsScreener != null
                        && curBot.TabsScreener.Count > 0
                        && curBot.TabsScreener[0].ServerUid == serverNum)
                    {
                        bot = curBot;
                        BotsInTest.RemoveAt(i);
                        break;
                    }
                }

                if (bot != null)
                {
                    BotTestCompleted?.Invoke(bot);
                }

                for (int i = 0; i < Servers.Count; i++)
                {
                    if (Servers[i].NumberServer == serverNum)
                    {
                        Servers[i].TestingEndEvent -= OnTestingEnd;
                        Servers[i].TestingProgressChangeEvent -= OnTestingProgressChange;
                        server = Servers[i];
                        Servers.RemoveAt(i);
                        break;
                    }
                }

                TestBotsTime.Add(testTime);

                if (TestBotsTime.Count >= _settings.ThreadsCount)
                {
                    TimeSpan allTime = TimeSpan.Zero;

                    for (int i = 0; i < TestBotsTime.Count; i++)
                    {
                        allTime = TimeSpan.FromMilliseconds(allTime.TotalMilliseconds + TestBotsTime[i].TotalMilliseconds + 1000);
                    }

                    decimal secondsOnOneTest = Convert.ToDecimal(allTime.TotalSeconds / TestBotsTime.Count);

                    int testsToEndCount = CountAllServersMax - CountAllServersEndTest;

                    if (testsToEndCount < 0)
                    {
                        testsToEndCount = 0;
                    }

                    decimal secondsToEndAllTests = testsToEndCount * secondsOnOneTest;

                    decimal secondsToEndDivideThreads = secondsToEndAllTests / _settings.ThreadsCount;

                    TimeSpan timeToEnd = TimeSpan.FromSeconds(Convert.ToInt32(secondsToEndDivideThreads));

                    if (timeToEnd.TotalSeconds != 0)
                    {
                        TimeToEndChangeEvent?.Invoke(timeToEnd);
                    }
                }
            }

            if (bot != null)
            {
                bot.Clear();
                bot.Delete();
            }

            if (server != null)
            {
                ServerMaster.RemoveOptimizerServer(server);
            }
        }

        private void OnTestingProgressChange(int curVal, int maxVal, int numServer)
        {
            TestingProgressChangeEvent?.Invoke(curVal, maxVal, numServer);
        }

        /// <summary>
        /// Fired when a bot's test is completed and the bot has been extracted from BotsInTest.
        /// The subscriber should call ReportsToFazes[last].Load(bot).
        /// </summary>
        public event Action<BotPanel> BotTestCompleted;

        public event Action<int, int> PrimeProgressChangeEvent;

        public event Action<TimeSpan> TimeToEndChangeEvent;

        public event Action<int, int, int> TestingProgressChangeEvent;

        private void SendLogMessage(string message, LogMessageType type)
        {
            LogMessageEvent?.Invoke(message, type);
        }

        public event Action<string, LogMessageType> LogMessageEvent;
    }
}
