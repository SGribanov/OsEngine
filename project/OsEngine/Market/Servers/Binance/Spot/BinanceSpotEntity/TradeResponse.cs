#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

namespace OsEngine.Market.Servers.Binance.Spot.BinanceSpotEntity
{
    public class Data
    {
        public string e { get; set; }
        public long E { get; set; }
        public string s { get; set; }
        public long t { get; set; }
        public string p { get; set; }
        public string q { get; set; }
        public long b { get; set; }
        public long a { get; set; }
        public long T { get; set; }
        public bool m { get; set; }
        public bool M { get; set; }
        public object X { get; set; }
        public object x { get; set; }
    }

    public class TradeResponse
    {
        public string stream { get; set; }
        public Data data { get; set; }
    }
}
