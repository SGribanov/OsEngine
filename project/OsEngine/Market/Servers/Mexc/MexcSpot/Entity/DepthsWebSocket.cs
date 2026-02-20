#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8765, CS8767

using System.Collections.Generic;

namespace OsEngine.Market.Servers.Mexc.MexcSpot.Entity
{
    public class DepthsWebSocket
    {
        public string channel { get; set; }
        public MexcDepth publicLimitDepths { get; set; }
        public string symbol { get; set; }
        public string sendTime { get; set; }
    }

    public class MexcDepthRow
    {
        public string price { get; set; }
        public string quantity { get; set; }
    }

    public class MexcDepth
    {
        public List<MexcDepthRow> asks { get; set; }
        public List<MexcDepthRow> bids { get; set; }
        public string eventType { get; set; }
        public string version { get; set; }
    }
}



