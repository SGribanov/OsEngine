#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

#pragma warning disable SYSLIB0014 // WebRequest/WebClient are obsolete
using Newtonsoft.Json;
using OsEngine.Entity;
using OsEngine.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace OsEngine.Market.Proxy
{
    public class ProxyMaster
    {
        private static readonly string ProxyMasterSettingsPath = @"Engine\ProxyMaster.txt";
        private static readonly string ProxyHubPath = @"Engine\ProxyHub.txt";

        public void Activate()
        {
            LoadSettings();
            LoadProxy();

            SendLogMessage("Proxy master activated. Proxy count: " 
                + Proxies.Count, LogMessageType.System);

            Task.Run(AutoPingThreadArea);
        }

        private void LoadSettings()
        {
            if (!File.Exists(ProxyMasterSettingsPath))
            {
                return;
            }
            try
            {
                ProxyMasterSettings settings = SettingsManager.Load(
                    ProxyMasterSettingsPath,
                    defaultValue: null,
                    legacyLoader: ParseLegacySettings);

                if (settings == null)
                {
                    return;
                }

                AutoPingIsOn = settings.AutoPingIsOn;
                AutoPingLastTime = settings.AutoPingLastTime;
                AutoPingMinutes = settings.AutoPingMinutes;
            }
            catch
            {
                // ignore
            }
        }

        public void SaveSettings()
        {
            try
            {
                SettingsManager.Save(
                    ProxyMasterSettingsPath,
                    new ProxyMasterSettings
                    {
                        AutoPingIsOn = AutoPingIsOn,
                        AutoPingLastTime = AutoPingLastTime,
                        AutoPingMinutes = AutoPingMinutes
                    });
            }
            catch (Exception error)
            {
                SendLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        private static ProxyMasterSettings ParseLegacySettings(string content)
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

            return new ProxyMasterSettings
            {
                AutoPingIsOn = Convert.ToBoolean(lines[0]),
                AutoPingLastTime = ParseDateInvariantOrCurrent(lines[1]),
                AutoPingMinutes = Convert.ToInt32(lines[2], CultureInfo.InvariantCulture)
            };
        }

        private static DateTime ParseDateInvariantOrCurrent(string value)
        {
            DateTime parsed;

            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsed))
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

        public void ShowDialog()
        {
            if(_ui == null)
            {
                _ui = new ProxyMasterUi(this);
                _ui.Show();
                _ui.Closed += _ui_Closed;
            }
            else
            {
                if(_ui.WindowState == System.Windows.WindowState.Minimized)
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

        private ProxyMasterUi _ui;

        public bool AutoPingIsOn = true;

        public DateTime AutoPingLastTime;

        public int AutoPingMinutes = 10;

        private sealed class ProxyMasterSettings
        {
            public bool AutoPingIsOn { get; set; }

            public DateTime AutoPingLastTime { get; set; }

            public int AutoPingMinutes { get; set; }
        }

        private sealed class ProxyHubSettings
        {
            public List<string> ProxyLines { get; set; }
        }

        #region Proxy hub

        public List<ProxyOsa> Proxies = new List<ProxyOsa>();

        private readonly Lock _getProxyLocker = new();

        public WebProxy GetProxyAutoRegime(ServerType serverType, string serverName)
        {
            try
            {
                lock (_getProxyLocker)
                {
                    if (Proxies == null
                                    || Proxies.Count == 0)
                    {
                        return null;
                    }

                    List<ProxyOsa> connectedProxy = new List<ProxyOsa>();

                    for (int i = 0; i < Proxies.Count; i++)
                    {
                        if (Proxies[i].AutoPingLastStatus != "Connect")
                        {
                            PingProxy(Proxies[i]);

                            if (Proxies[i].AutoPingLastStatus != "Connect")
                            {
                                continue;
                            }

                            continue;
                        }

                        if (Proxies[i].IsOn == false)
                        {
                            continue;
                        }

                        connectedProxy.Add(Proxies[i]);
                    }

                    if (connectedProxy.Count == 0)
                    {
                        return null;
                    }

                    if (connectedProxy.Count > 1)
                    {
                        connectedProxy = connectedProxy.OrderBy(x => x.UseConnectionCount).ToList();
                    }
                    connectedProxy[0].UseConnectionCount++;
                    return connectedProxy[0].GetWebProxy();
                }
            }
            catch
            {
                return null;
            }
        }

        public WebProxy GetProxyManualRegime(string userValue)
        {
            try
            {
                lock (_getProxyLocker)
                {
                    if (Proxies == null
                 || Proxies.Count == 0)
                    {
                        return null;
                    }

                    for (int i = 0; i < Proxies.Count; i++)
                    {
                        if (Proxies[i].AutoPingLastStatus != "Connect")
                        {
                            PingProxy(Proxies[i]);

                            if (Proxies[i].AutoPingLastStatus != "Connect")
                            {
                                continue;
                            }

                            continue;
                        }

                        if (Proxies[i].IsOn == false)
                        {
                            continue;
                        }

                        if (Proxies[i].Number.ToString() == userValue)
                        {
                            Proxies[i].UseConnectionCount++;
                            return Proxies[i].GetWebProxy();
                        }

                        if (Proxies[i].Ip == userValue)
                        {
                            Proxies[i].UseConnectionCount++;
                            return Proxies[i].GetWebProxy();
                        }

                        if (Proxies[i].Ip + ":" + Proxies[i].Port == userValue)
                        {
                            Proxies[i].UseConnectionCount++;
                            return Proxies[i].GetWebProxy();
                        }
                    }
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private void LoadProxy()
        {
            if (!File.Exists(ProxyHubPath))
            {
                return;
            }
            try
            {
                ProxyHubSettings settings = SettingsManager.Load(
                    ProxyHubPath,
                    defaultValue: null,
                    legacyLoader: ParseLegacyProxyHubSettings);

                if (settings?.ProxyLines == null)
                {
                    return;
                }

                for (int i = 0; i < settings.ProxyLines.Count; i++)
                {
                    string line = settings.ProxyLines[i];

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    ProxyOsa newProxy = new ProxyOsa();
                    newProxy.LoadFromString(line);
                    Proxies.Add(newProxy);
                }
            }
            catch
            {
                // игнор
            }
        }

        public void SaveProxy()
        {
            try
            {
                List<string> lines = new List<string>();

                for (int i = 0; i < Proxies.Count; i++)
                {
                    lines.Add(Proxies[i].GetStringToSave());
                }

                SettingsManager.Save(
                    ProxyHubPath,
                    new ProxyHubSettings
                    {
                        ProxyLines = lines
                    });
            }
            catch (Exception error)
            {
                SendLogMessage(error.ToString(), LogMessageType.Error);
            }
        }

        private static ProxyHubSettings ParseLegacyProxyHubSettings(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            return new ProxyHubSettings
            {
                ProxyLines = new List<string>(lines)
            };
        }

        public ProxyOsa CreateNewProxy()
        {
            ProxyOsa newProxy = new ProxyOsa();

            int actualNumber = 0;

            for(int i = 0;i < Proxies.Count;i++)
            {
                if (Proxies[i].Number >= actualNumber)
                {
                    actualNumber = Proxies[i].Number + 1;
                }
            }

            newProxy.Number = actualNumber;

            Proxies.Add(newProxy);
            SaveProxy();

            return newProxy;
        }

        public void RemoveProxy(int number)
        {
            for(int i = 0;i < Proxies.Count;i++)
            {
                if (Proxies[i].Number == number)
                {
                    Proxies.RemoveAt(i);
                    SaveProxy();
                    return;
                }
            }

        }

        #endregion

        #region Proxy ping

        private void AutoPingThreadArea()
        {
            while (true)
            {
                try
                {
                   Thread.Sleep(10000);

                    if(MainWindow.ProccesIsWorked == false)
                    {
                        return;
                    }

                    if(AutoPingIsOn == false)
                    {
                        continue;
                    }

                    if (Proxies == null
                        || Proxies.Count == 0)
                    {
                        continue;
                    }

                    if(AutoPingLastTime.AddMinutes(AutoPingMinutes) > DateTime.Now)
                    {
                        continue;
                    }

                    AutoPingLastTime = DateTime.Now;

                    CheckPing();

                }
                catch (Exception ex)
                {
                    SendLogMessage(ex.ToString(), LogMessageType.Error);
                }
            }

        }

        public void CheckPing()
        {
            try
            {
                if (_pingThread != null)
                {
                    SendLogMessage("Ping in process", LogMessageType.Error);
                    return;
                }

                _pingThread = new Thread(CheckPingThreadArea);
                _pingThread.IsBackground = true;
                _pingThread.Start();
            }
            catch (Exception ex)
            {
                SendLogMessage(ex.ToString(), LogMessageType.Error);
            }
        }

        private Thread _pingThread;

        private void CheckPingThreadArea()
        {
            try
            {
                // 1 сначала просто проверяем интернет

                WebRequest request = null;
                request = (WebRequest)WebRequest.Create("https://www.moex.com");

                bool haveError = false;

                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response == null || response.StatusCode != HttpStatusCode.OK)
                    {
                        haveError = true;
                    }
                }
                catch
                {
                    haveError = true;
                }

                if (haveError)
                {
                    SendLogMessage("Error. No internet. Can`t do proxy ping", LogMessageType.Error);
                    _pingThread = null;
                    return;
                }

                // 2 теперь проверяем отдельно прокси

                for (int i = 0;i < Proxies.Count;i++)
                {
                    PingProxy(Proxies[i]);
                }
            }
            catch(Exception ex)
            {
                SendLogMessage(ex.ToString(), LogMessageType.Error);
            }

            AutoPingLastTime = DateTime.Now;

            SaveProxy();

            if (ProxyPingEndEvent != null)
            {
                ProxyPingEndEvent();
            }

            _pingThread = null;
        }

        public void PingProxy(ProxyOsa proxy)
        {
            if (string.IsNullOrEmpty(proxy.Ip) == true)
            {
                proxy.AutoPingLastStatus = "Error. no IP";
                return;
            }
            if (string.IsNullOrEmpty(proxy.Login) == true)
            {
                proxy.AutoPingLastStatus = "Error. no Login";
                return;
            }
            if (string.IsNullOrEmpty(proxy.UserPassword) == true)
            {
                proxy.AutoPingLastStatus = "Error. no Password";
                return;
            }
            if (proxy.Port == 0)
            {
                proxy.AutoPingLastStatus = "Error. no Port";
                return;
            }
            if (string.IsNullOrEmpty(proxy.PingWebAddress) == true)
            {
                proxy.AutoPingLastStatus = "Error. no ping address";
                return;
            }

            string address = proxy.PingWebAddress;

            WebRequest request = null;
            request = (WebRequest)WebRequest.Create(address);

            WebProxy myProxy = proxy.GetWebProxy();

            if (myProxy != null)
            {
                request.Proxy = myProxy;
            }

            bool haveError = false;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response == null || response.StatusCode != HttpStatusCode.OK)
                {
                    haveError = true;
                }
            }
            catch
            {
                haveError = true;
            }

            if (haveError)
            {
                proxy.AutoPingLastStatus = "Error. no ping address";
            }
            else
            {
                proxy.AutoPingLastStatus = "Connect";
            }
        }

        public event Action ProxyPingEndEvent;

        #endregion

        #region Proxy location

        public void CheckLocation()
        {
            if (_locationThread != null)
            {
                SendLogMessage("Location in process", LogMessageType.Error);
                return;
            }

            _locationThread = new Thread(CheckLocationThreadArea);
            _locationThread.IsBackground = true;
            _locationThread.Start();
        }

        private Thread _locationThread;

        private void CheckLocationThreadArea()
        {
            try
            {
                // 1 сначала просто проверяем интернет

                WebRequest request = null;
                request = (WebRequest)WebRequest.Create("https://www.moex.com");

                bool haveError = false;

                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response == null || response.StatusCode != HttpStatusCode.OK)
                    {
                        haveError = true;
                    }
                }
                catch
                {
                    haveError = true;
                }

                if (haveError)
                {
                    SendLogMessage("Error. No internet. Can`t find proxy location", LogMessageType.Error);
                    _locationThread = null;
                    return;
                }

                // 2 теперь проверяем отдельно прокси

                for (int i = 0; i < Proxies.Count; i++)
                {
                    CheckLocationProxy(Proxies[i]);
                }
            }
            catch (Exception ex)
            {
                SendLogMessage(ex.ToString(), LogMessageType.Error);
            }

            SaveProxy();

            if (ProxyCheckLocationEndEvent != null)
            {
                ProxyCheckLocationEndEvent();
            }

            _locationThread = null;


        }

        private void CheckLocationProxy(ProxyOsa proxy)
        {
            if (string.IsNullOrEmpty(proxy.Ip) == true)
            {
                proxy.Location = "Error. no IP";
                return;
            }

            IpInfo ipInfo = new IpInfo();
            try
            {
                string info = new WebClient().DownloadString("http://ipinfo.io/" + proxy.Ip);
                ipInfo = JsonConvert.DeserializeObject<IpInfo>(info);
                RegionInfo myRI1 = new RegionInfo(ipInfo.Country);
                ipInfo.Country = myRI1.EnglishName;
            }
            catch
            {
                ipInfo.Country = null;
            }

            if(string.IsNullOrEmpty(ipInfo.Country) == false)
            {
                if(string.IsNullOrEmpty(ipInfo.City) == false
                   && ipInfo.Country != ipInfo.City)
                {
                    proxy.Location = ipInfo.Country + "_" + ipInfo.City;
                }
                else
                {
                    proxy.Location = ipInfo.Country;
                }
                
            }
        }

        public event Action ProxyCheckLocationEndEvent;

        #endregion

        #region Log

        public event Action<string, LogMessageType> LogMessageEvent;

        public void SendLogMessage(string message, LogMessageType messageType)
        {
            message = "Proxy master.  " + message;
            LogMessageEvent?.Invoke(message, messageType);
        }

        #endregion

    }

    public class IpInfo
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("loc")]
        public string Loc { get; set; }

        [JsonProperty("org")]
        public string Org { get; set; }

        [JsonProperty("postal")]
        public string Postal { get; set; }
    }
}

