#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Globalization;
using System.Net;

namespace OsEngine.Market.Proxy
{
    public class ProxyOsa
    {
        public WebProxy GetWebProxy()
        {
            string address = Ip + ":" + Port.ToString();

            WebProxy newProxy = new WebProxy(address);
           
            newProxy.Credentials = new NetworkCredential(Login, UserPassword);

            return newProxy;
        }

        public int Number;

        public bool IsOn = false;

        public string Ip;

        public int Port;

        public string Login;

        public string UserPassword;

        public string Location = "Unknown";

        public string AutoPingLastStatus = "Unknown";

        public string PingWebAddress = "http://ipinfo.io/";

        public int UseConnectionCount;

        public string GetStringToSave()
        {
            string result = IsOn + "%";
            result += Number + "%";
            result += Location + "%";
            result += Ip + "%";
            result += Port + "%";
            result += Login + "%";
            result += UserPassword + "%";
            result += AutoPingLastStatus + "%";
            result += PingWebAddress + "%";

            return result;
        }

        public void LoadFromString(string saveStr)
        {
            IsOn = Convert.ToBoolean(saveStr.Split('%')[0]);
            Number = Convert.ToInt32(saveStr.Split('%')[1], CultureInfo.InvariantCulture);
            Location = saveStr.Split('%')[2];
            Ip = saveStr.Split('%')[3];
            Port = Convert.ToInt32(saveStr.Split('%')[4], CultureInfo.InvariantCulture);
            Login = saveStr.Split('%')[5];
            UserPassword = saveStr.Split('%')[6];
            AutoPingLastStatus = saveStr.Split('%')[7];
            PingWebAddress = saveStr.Split('%')[8];
        }
    }
}
