#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System;
using System.Collections.Generic;

namespace OsEngine.Market.Servers.MoexFixFastTwimeFutures.Entity
{
    public class MarketDataGroup
    {
        public string FeedType { get; set; }
        public string MarketID { get; set; }
        public string Label { get; set; }
        public List<FastConnection> FastConnections { get; set; }
    }

    public class FastConnection
    {
        public string Type { get; set; }
        public string Feed { get; set; }
        public string MulticastIP { get; set; }
        public string SrsIP { get; set; }
        public int Port { get; set; }
    }
}

