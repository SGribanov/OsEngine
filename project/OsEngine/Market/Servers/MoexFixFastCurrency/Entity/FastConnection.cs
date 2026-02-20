#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System;


namespace OsEngine.Market.Servers.MoexFixFastCurrency.Entity
{
    public class FastConnection
    {
        public string FeedType { get; set; }
        public string FeedID { get; set; }
        public string MulticastIP { get; set; }
        public string SrsIP { get; set; }
        public int Port { get; set; }
    }
}


