#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8767

/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using OsEngine.Entity;
using OsEngine.Logging;

namespace OsEngine.Market.Servers
{

    /// <summary>
    /// server ticks storage
    /// </summary>
    public class ServerTickStorage
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="server"> server for saving trades</param>
        public ServerTickStorage(AServer server)
        {
            _server = server;

            if (!Directory.Exists(@"Data"))
            {
                Directory.CreateDirectory(@"Data");
            }

            _pathName = @"Data" + @"\" + server.ServerNameUnique + @"Trades";

            Thread saver = new Thread(TickSaverSpaceInOneFile);
            saver.CurrentCulture = new CultureInfo("RU-ru");
            saver.IsBackground = false;
            saver.Start();
        }

        /// <summary>
        /// Serviced connection
        /// </summary>
        private AServer? _server;

        /// <summary>
        /// shows whether need to save trades
        /// </summary>
        public bool NeedToSave;

        /// <summary>
        /// how many days upload from history
        /// </summary>
        public int DaysToLoad;

        /// <summary>
        /// directory for saving data
        /// </summary>
        private string _pathName;

        /// <summary>
        /// securities for saving
        /// </summary>
        private readonly List<Security> _securities = new List<Security>();

        /// <summary>
        /// save security data 
        /// </summary>
        public void SetSecurityToSave(Security? security)
        {
            if (security == null)
            {
                return;
            }

            if (_securities.Find(security1 => security1.Name == security.Name) == null)
            {
                _securities.Add(security);
            }
        }

        /// <summary>
        /// upload ticks for some instrument
        /// </summary>
        public event Action<List<Trade>[]>? TickLoadedEvent;

        /// <summary>
        /// service information for saving trades
        /// </summary>
        private readonly List<TradeSaveInfo> _tradeSaveInfo = new List<TradeSaveInfo>();

        // for saving in one file

        /// <summary>
        /// method with tick saving thread
        /// </summary>
        private void TickSaverSpaceInOneFile()
        {
            if (!Directory.Exists(_pathName))
            {
                Directory.CreateDirectory(_pathName);
            }

            while (true)
            {
                try
                {
                    Thread.Sleep(15000);

                    AServer? server = _server;
                    if (server == null)
                    {
                        return;
                    }

                    if(server.IsDeleted == true)
                    {
                        _server = null;
                        return;
                    }

                    if (server.ServerStatus != ServerConnectStatus.Connect)
                    {
                        continue;
                    }

                    if (NeedToSave == false)
                    {
                        continue;
                    }

                    if (_weLoadTrades == false)
                    {
                        continue;
                    }

                    if (MainWindow.ProccesIsWorked == false)
                    {
                        return;
                    }

                    List<Trade>[]? allTrades = server.AllTrades;

                    for (int i1 = 0;
                        allTrades != null && Thread.CurrentThread.Name != "deleteThread" && i1 < allTrades.Length;
                        i1++)
                    {
                        if (allTrades[i1].Count == 0)
                        {
                            continue;
                        }
                        if (MainWindow.ProccesIsWorked == false)
                        {
                            // если приложение закрывается
                            return;
                        }

                        if (_securities.Find(security => security.Name == allTrades[i1][0].SecurityNameCode) == null)
                        {
                            continue;
                        }

                        TradeSaveInfo tradeInfo =
                            _tradeSaveInfo.Find(s => s.NameSecurity == allTrades[i1][0].SecurityNameCode);

                        if (tradeInfo == null)
                        {
                            tradeInfo = new TradeSaveInfo();
                            tradeInfo.NameSecurity = allTrades[i1][0].SecurityNameCode;
                            _tradeSaveInfo.Add(tradeInfo);
                        }

                        if (tradeInfo.LastSaveIndex >= allTrades[i1].Count)
                        {
                            continue;
                        }

                        int lastSecond = allTrades[i1][tradeInfo.LastSaveIndex].Time.Second;
                        int lastMillisecond = allTrades[i1][tradeInfo.LastSaveIndex].MicroSeconds;

                        string securityPath = _pathName + @"\" + allTrades[i1][0].SecurityNameCode + ".txt";
                        List<string> linesToAppend = new List<string>();

                        for (int i = tradeInfo.LastSaveIndex; i < allTrades[i1].Count - 1; i++)
                        {
                            if (allTrades[i1][i].MicroSeconds == 0)
                            { // for some time in microseconds if the connector did not issue them to us / генерим какое-то время микросекунд, если нам коннектор их не выдал
                                if (lastSecond != allTrades[i1][i].Time.Second)
                                {
                                    lastMillisecond = 0;
                                    lastSecond = allTrades[i1][i].Time.Second;
                                }

                                allTrades[i1][i].MicroSeconds = lastMillisecond += 10;
                            }

                            linesToAppend.Add(allTrades[i1][i].GetSaveString());
                        }

                        AppendLinesAtomically(securityPath, linesToAppend);
                        tradeInfo.LastSaveIndex = allTrades[i1].Count - 1;


                    }
                }
                catch (Exception error)
                {
                    SendNewLogMessage(error.ToString(), LogMessageType.Error);
                }
            }
        }

        private static void AppendLinesAtomically(string path, List<string>? linesToAppend)
        {
            if (linesToAppend == null || linesToAppend.Count == 0)
            {
                return;
            }

            List<string> allLines = new List<string>();

            if (File.Exists(path))
            {
                allLines.AddRange(File.ReadAllLines(path));
            }

            allLines.AddRange(linesToAppend);
            SafeFileWriter.WriteAllLines(path, allLines);
        }

        private bool _weLoadTrades;

        /// <summary>
        /// upload ticks
        /// </summary>
        /// <param name="dayCount"> number of days for uploading </param>
        public void LoadTick()
        {
            try
            {
                if (!Directory.Exists(_pathName))
                {
                    _weLoadTrades = true;
                    return;
                }

                AServer? server = _server;
                if (server == null)
                {
                    return;
                }

                List<Trade>[]? allTrades = server.AllTrades;

                string[] saves = Directory.GetFiles(_pathName);

                for (int i = 0; i < saves.Length; i++)
                {
                    // upload / загружаем
                    using StreamReader reader = new StreamReader(saves[i]);

                    List<Trade> newList = new List<Trade>();

                    string nameSecurity;

                    try
                    {
                        string[] array = saves[i].Split('\\');

                        string nameStr = array[2];

                        nameSecurity = nameStr.Replace(".txt","");
                    }
                    catch (Exception ex)
                    {
                        SendNewLogMessage($"ServerTickStorage: path parse error for '{saves[i]}'. {ex.Message}", LogMessageType.System);
                        continue;
                    }

                    try
                    {
                        DateTime timeStart = DateTime.Now.AddDays(-DaysToLoad - 1);

                        if (timeStart.Month == 1 && timeStart.Day < 10)
                        {
                            timeStart = timeStart.AddDays(-10);
                        }

                        List<string> tradesInStr = new List<string>();

                        while (!reader.EndOfStream)
                        {
                            string? line = reader.ReadLine();
                            if (string.IsNullOrWhiteSpace(line))
                            {
                                continue;
                            }

                            tradesInStr.Add(line);
                        }

                        for (int i2 = 0; i2 < tradesInStr.Count; i2++)
                        {
                            Trade newTrade = new Trade();

                            string curTrade = tradesInStr[i2];

                            try
                            {
                                newTrade.SetTradeFromString(curTrade);
                            }
                            catch (Exception ex)
                            {
                                SendNewLogMessage($"ServerTickStorage: trade parse error for '{saves[i]}'. {ex.Message}", LogMessageType.System);
                                continue;
                            }

                            newTrade.SecurityNameCode = nameSecurity;

                            if (newTrade.Time.Date < timeStart.Date)
                            {
                                i2 += 100;
                                continue;
                            }

                            newList.Add(newTrade);
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    // save / сохраняем

                    if (newList.Count == 0)
                    {
                        continue;
                    }

                    if (_tradeSaveInfo.Find(s => s.NameSecurity == newList[0].SecurityNameCode) == null)
                    {
                        TradeSaveInfo tradeInfo = new TradeSaveInfo();
                        tradeInfo.NameSecurity = newList[0].SecurityNameCode;
                        tradeInfo.LastSaveIndex = newList.Count;
                        _tradeSaveInfo.Add(tradeInfo);
                    }

                    if (allTrades == null)
                    {
                        allTrades = new[] { newList };
                    }
                    else
                    {
                        List<Trade>[] newListsArray = new List<Trade>[allTrades.Length + 1];
                        for (int ii = 0; ii < allTrades.Length; ii++)
                        {
                            newListsArray[ii] = allTrades[ii];
                        }
                        newListsArray[newListsArray.Length - 1] = newList;
                        allTrades = newListsArray;
                    }

                }

                if (allTrades != null)
                {
                    TickLoadedEvent?.Invoke(allTrades);
                }
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
            _weLoadTrades = true;
        }

        // log messages

        /// <summary>
        /// send a new message to up
        /// </summary>
        private void SendNewLogMessage(string message, LogMessageType type)
        {
            LogMessageEvent?.Invoke(message, type);

            if (LogMessageEvent == null && type == LogMessageType.Error)
            { // if nobody is subscribed to us and there is a log error / если на нас никто не подписан и в логе ошибка
                System.Windows.MessageBox.Show(message);
            }
        }

        /// <summary>
        /// outgoing log message
        /// </summary>
        public event Action<string, LogMessageType>? LogMessageEvent;

    }

    /// <summary>
    /// information to save trades
    /// </summary>
    public class TradeSaveInfo
    {
        /// <summary>
        /// Security name
        /// </summary>
        public string NameSecurity = string.Empty;

        /// <summary>
        /// last save time
        /// </summary>
        public DateTime LastSaveObjectTime;

        /// <summary>
        /// the last trade Id we saved
        /// </summary>
        public string LastTradeId = string.Empty;

        /// <summary>
        /// last stored index
        /// </summary>
        public int LastSaveIndex;
    }
}

