#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System.Collections.Generic;

namespace OsEngine.Market.Servers.GateIo.GateIoFutures.Entities
{
    public class GfTicker
    {
        public string time { get; set; }
        public string time_ms { get; set; }
        public string channel { get; set; }
        public string @event { get; set; }
        public List<TickerItem> result { get; set; }
    }

    public class TickerItem
    {
        public string contract { get; set; }
        public string last { get; set; }
        public string change_percentage { get; set; }
        public string funding_rate { get; set; }
        public string funding_rate_indicative { get; set; }
        public string mark_price { get; set; }
        public string index_price { get; set; }
        public string total_size { get; set; }
        public string volume_24h { get; set; }
        public string volume_24h_btc { get; set; }
        public string volume_24h_usd { get; set; }
        public string quanto_base_rate { get; set; }
        public string volume_24h_quote { get; set; }
        public string volume_24h_settle { get; set; }
        public string volume_24h_base { get; set; }
        public string low_24h { get; set; }
        public string high_24h { get; set; }
    }

    public class FundingItemHistory
    {
        public string t { get; set; }
        public string r { get; set; }
    }
}

