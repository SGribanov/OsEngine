#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System.Collections.Generic;

namespace OsEngine.Market.Servers.Binance.Spot.BinanceSpotEntity
{
    public class AccountBalance
    {
        public string a { get; set; }
        public string f { get; set; }
        public string l { get; set; }
    }

    public class OutboundAccountInfo
    {
        public string e { get; set; }
        public long E { get; set; }
        public int m { get; set; }
        public int t { get; set; }
        public int b { get; set; }
        public int s { get; set; }
        public bool T { get; set; }
        public bool W { get; set; }
        public bool D { get; set; }
        public long u { get; set; }
        public List<AccountBalance> B { get; set; }
    }
}
