using System;

#nullable enable

namespace OsEngine.Entity
{
    public class SecurityVolumes
    {
        public string SecurityNameCode = string.Empty;

        /// <summary>
        /// volume in currency
        /// </summary>
        public decimal Volume24h;

        /// <summary>
        /// volume in USDT
        /// </summary>
        public decimal Volume24hUSDT;

        public DateTime TimeUpdate;
    }
}

