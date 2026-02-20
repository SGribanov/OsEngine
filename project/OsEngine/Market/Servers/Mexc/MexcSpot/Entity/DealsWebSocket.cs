#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8765, CS8767

using System.Collections.Generic;

namespace OsEngine.Market.Servers.Mexc.MexcSpot.Entity
{
    public class DealsWebSocket
    {
        public string channel { get; set; }
        public MexcDeals publicAggreDeals { get; set; }
        public string symbol { get; set; }
        public string sendTime { get; set; }
    }

    public class MexcDeals
    {
        public List<MexcDeal> deals { get; set; }
        public string eventType { get; set; }
    }

    public class MexcDeal
    {
        public string price { get; set; }
        public string quantity { get; set; }
        public string tradeType { get; set; }
        public string time { get; set; }
    }
}



