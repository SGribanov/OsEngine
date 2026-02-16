/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using OsEngine.Entity;
using OsEngine.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace OsEngine.Market.AutoFollow
{
    public class CopyMaster
    {
        private static readonly string CopyTradersDirPath = @"Engine\CopyTrader\";
        private static readonly string CopyTradersHubPath = @"Engine\CopyTrader\CopyTradersHub.txt";

        public void Activate()
        {
            LoadCopyTraders();

            LogCopyMaster = new Log("CopyMaster", StartProgram.IsOsTrader);
            LogCopyMaster.Listen(this);

            SendLogMessage("Copy master activated. Copy traders: " + CopyTraders.Count, LogMessageType.System);
      
            
        }

        public void ShowDialog()
        {
            if (_ui == null)
            {
                _ui = new CopyMasterUi(this);
                _ui.Show();
                _ui.Closed += _ui_Closed;
            }
            else
            {
                if (_ui.WindowState == System.Windows.WindowState.Minimized)
                {
                    _ui.WindowState = System.Windows.WindowState.Normal;
                }

                _ui.Activate();
            }
        }

        private void _ui_Closed(object sender, EventArgs e)
        {
            _ui = null;
        }

        private CopyMasterUi _ui;

        #region CopyTrader hub

        public List<CopyTrader> CopyTraders = new List<CopyTrader>();

        private void LoadCopyTraders()
        {
            if (Directory.Exists(CopyTradersDirPath) == false)
            {
                Directory.CreateDirectory(CopyTradersDirPath);

            }

            if (!File.Exists(CopyTradersHubPath))
            {
                return;
            }
            try
            {
                CopyTradersHubSettings settings = SettingsManager.Load(
                    CopyTradersHubPath,
                    defaultValue: null,
                    legacyLoader: ParseLegacyCopyTradersHubSettings);

                if (settings?.Traders == null)
                {
                    return;
                }

                for (int i = 0; i < settings.Traders.Count; i++)
                {
                    string line = settings.Traders[i];

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    CopyTrader newCopyTrader = new CopyTrader(line);
                    newCopyTrader.NeedToSaveEvent += NewCopyTrader_NeedToSaveEvent;
                    CopyTraders.Add(newCopyTrader);
                }
            }
            catch
            {
                // игнор
            }
        }

        private void NewCopyTrader_NeedToSaveEvent()
        {
            SaveCopyTraders();
        }

        public void SaveCopyTraders()
        {
            try
            {
                List<string> traders = new List<string>();

                for (int i = 0; i < CopyTraders.Count; i++)
                {
                    traders.Add(CopyTraders[i].GetStringToSave());
                }

                SettingsManager.Save(
                    CopyTradersHubPath,
                    new CopyTradersHubSettings
                    {
                        Traders = traders
                    });
            }
            catch (Exception error)
            {
                SendLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        private static CopyTradersHubSettings ParseLegacyCopyTradersHubSettings(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return new CopyTradersHubSettings
            {
                Traders = new List<string>(lines)
            };
        }

        public CopyTrader CreateNewCopyTrader()
        {
            int actualNumber = 0;

            for (int i = 0; i < CopyTraders.Count; i++)
            {
                if (CopyTraders[i].Number >= actualNumber)
                {
                    actualNumber = CopyTraders[i].Number + 1;
                }
            }

            CopyTrader newCopyTrader = new CopyTrader(actualNumber);
            newCopyTrader.NeedToSaveEvent += NewCopyTrader_NeedToSaveEvent;
            CopyTraders.Add(newCopyTrader);
            SaveCopyTraders();

            return newCopyTrader;
        }

        public void RemoveCopyTraderAt(int number)
        {
            for (int i = 0; i < CopyTraders.Count; i++)
            {
                if (CopyTraders[i].Number == number)
                {
                    CopyTraders[i].ClearDelete();
                    CopyTraders[i].NeedToSaveEvent -= NewCopyTrader_NeedToSaveEvent;
                    CopyTraders.RemoveAt(i);
                    SaveCopyTraders();
                    return;
                }
            }
        }

        #endregion

        #region Log

        public Log LogCopyMaster;

        public event Action<string, LogMessageType> LogMessageEvent;

        public void SendLogMessage(string message, LogMessageType messageType)
        {
            message = "Copy master.  " + message;
            LogMessageEvent?.Invoke(message, messageType);
        }

        private sealed class CopyTradersHubSettings
        {
            public List<string> Traders { get; set; }
        }

        #endregion
    }
}
