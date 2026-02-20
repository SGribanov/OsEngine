#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System;

namespace OsEngine.Market.Servers.Entity
{
    public class ServerWorkingTimeSettings
    {
        /// <summary>
        /// beginning of the trading session
        /// начало торговой сессии
        /// </summary>
        public TimeSpan StartSessionTime;

        /// <summary>
        /// ending of the trading session
        /// конец торговой сессии
        /// </summary>
        public TimeSpan EndSessionTime;

        /// <summary>
        /// server time zone
        /// временная зона сервера
        /// </summary>
        public string ServerTimeZone;

        /// <summary>
        /// if the exchange is working on weekends, it returns true 
        /// если биржа работает по выходным возвращается true
        /// </summary>
        public bool WorkingAtWeekend;
    }
}

