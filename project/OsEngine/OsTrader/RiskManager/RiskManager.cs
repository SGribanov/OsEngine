#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OsEngine.Entity;
using OsEngine.Logging;
using OsEngine.Market;

namespace OsEngine.OsTrader.RiskManager
{
    /// <summary>
    /// Risk Manager
    /// </summary>
    public class RiskManager
    {
        // static part with work flow 

        public static Task Watcher;

        /// <summary>
        /// Risk managers who need to be serviced
        /// </summary>
        public static List<RiskManager> RiskManagersToCheck = new List<RiskManager>();

        private static readonly Lock _activatorLocker = new();

        /// <summary>
        /// Activate flow
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
        /// Place of work thread
        /// </summary>
        public static async void WatcherHome()
        {
            while (true)
            {
                await Task.Delay(2000);

                try
                {
                    for (int i = 0; i < RiskManagersToCheck.Count; i++)
                    {
                        if (RiskManagersToCheck[i] == null)
                        {
                            RiskManagersToCheck.RemoveAt(i);
                            i--;
                            continue;
                        }
                    }

                    for (int i = 0; i < RiskManagersToCheck.Count; i++)
                    {
                        RiskManagersToCheck[i].CheckJournals();
                    }

                    if (!MainWindow.ProccesIsWorked)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning(ex.ToString());
                }
            }
        }

        // service

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nameBot">uid</param>
        /// <param name="startProgram">the program that launched the class</param>
        public RiskManager(string nameBot, StartProgram startProgram)
        {
            _startProgram = startProgram;
            _name = nameBot + "RiskManager";
            MaxDrowDownToDayPersent = 1;
            
            if(_startProgram != StartProgram.IsOsOptimizer)
            {
                Load();

                if (Watcher == null)
                {
                    Activate();
                }
                RiskManagersToCheck.Add(this);
            }
        }

        /// <summary>
        /// The program that launched the class
        /// </summary>
        private StartProgram _startProgram;

        /// <summary>
        /// Name
        /// </summary>
        private string _name;

        /// <summary>
        /// Maximum drawdown per day
        /// </summary>
        public decimal MaxDrowDownToDayPersent;

        /// <summary>
        /// Type of reaction
        /// </summary>
        public RiskManagerReactionType ReactionType;

        /// <summary>
        /// Is the manager included
        /// </summary>
        public bool IsActiv;

        /// <summary>
        /// Clear previously loaded jorunals
        /// </summary>
        public void ClearJournals()
        {
            _journals = null;
        }

        /// <summary>
        /// Load in Risk Manager
        /// </summary>
        public void SetNewJournal(Journal.Journal newJournal)
        {
            try
            {
                if (_journals == null)
                {
                    _journals = new List<Journal.Journal>();
                }
                _journals.Add(newJournal);
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        /// <summary>
        /// Risk manager journals
        /// </summary>
        private List<Journal.Journal> _journals;

        /// <summary>
        /// Save
        /// </summary>
        private void Load()
        {
            if (!File.Exists(GetSettingsPath()))
            {
                return;
            }
            try
            {
                RiskManagerSettingsDto settings = SettingsManager.Load(
                    GetSettingsPath(),
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                MaxDrowDownToDayPersent = settings.MaxDrowDownToDayPersent;
                IsActiv = settings.IsActiv;
                ReactionType = settings.ReactionType;
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        /// <summary>
        /// Load
        /// </summary>
        public void Save()
        {
            try
            {
                SettingsManager.Save(
                    GetSettingsPath(),
                    new RiskManagerSettingsDto
                    {
                        MaxDrowDownToDayPersent = MaxDrowDownToDayPersent,
                        IsActiv = IsActiv,
                        ReactionType = ReactionType
                    });
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        /// <summary>
        /// Delete
        /// </summary>
        public void Delete()
        {
            try
            {
                if (_startProgram == StartProgram.IsOsOptimizer)
                {
                    return;
                }

                if (File.Exists(GetSettingsPath()))
                {
                    File.Delete(GetSettingsPath());
                }

                RiskManagersToCheck.Remove(this);

                ClearJournals();
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        private string GetSettingsPath()
        {
            return @"Engine\" + _name + @".txt";
        }

        private static RiskManagerSettingsDto ParseLegacySettings(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 3)
            {
                return null;
            }

            Enum.TryParse(lines[2], false, out RiskManagerReactionType reactionType);

            return new RiskManagerSettingsDto
            {
                MaxDrowDownToDayPersent = Convert.ToDecimal(lines[0]),
                IsActiv = Convert.ToBoolean(lines[1]),
                ReactionType = reactionType
            };
        }

        private sealed class RiskManagerSettingsDto
        {
            public decimal MaxDrowDownToDayPersent { get; set; }

            public bool IsActiv { get; set; }

            public RiskManagerReactionType ReactionType { get; set; }
        }

        /// <summary>
        /// Show
        /// </summary>
        public void ShowDialog()
        {
            try
            {
                RiskManagerUi ui = new RiskManagerUi(this);
                ui.ShowDialog();
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        /// <summary>
        /// Method in which we review logs for excess of allowable losses
        /// </summary>
        private void CheckJournals()
        {
            try
            {
                if (!IsActiv)
                {
                    return;
                }

                if (_startProgram == StartProgram.IsOsOptimizer ||
                    _startProgram == StartProgram.IsOsConverter ||
                    _startProgram == StartProgram.IsOsData)
                {
                    return;
                }

                if (_journals == null)
                {
                    return;
                }

                decimal profit = 0;

                for (int i = 0; i < _journals.Count; i++)
                {
                    profit += _journals[i].GetProfitFromThatDayInPercent();
                }

                if (profit < -Math.Abs(MaxDrowDownToDayPersent))
                {
                    IsActiv = false;

                    if (RiskManagerAlarmEvent != null)
                    {
                        RiskManagerAlarmEvent(ReactionType);
                    }
                }
            }
            catch (Exception error)
            {
                SendNewLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        // logging

        /// <summary>
        /// Send new message
        /// </summary>
        private void SendNewLogMessage(string message, LogMessageType type)
        {
            if (LogMessageEvent != null)
            {
                LogMessageEvent(message, type);
            }
            else if (type == LogMessageType.Error)
            {
                System.Windows.MessageBox.Show(message);
            }
        }

        /// <summary>
        /// Outgoing message for log
        /// </summary>
        public event Action<string, LogMessageType> LogMessageEvent;

        /// <summary>
        /// Send excess loss alert
        /// </summary>
        public event Action<RiskManagerReactionType> RiskManagerAlarmEvent;

    }

    /// <summary>
    /// Manager's risk response reaction to too much loss
    /// </summary>
    public enum RiskManagerReactionType
    {
        /// <summary>
        /// Pop up a window
        /// </summary>
        ShowDialog,

        /// <summary>
        /// Close all positions and disable the robot
        /// </summary>
        CloseAndOff,

        /// <summary>
        /// None
        /// </summary>
        None
    }
}

