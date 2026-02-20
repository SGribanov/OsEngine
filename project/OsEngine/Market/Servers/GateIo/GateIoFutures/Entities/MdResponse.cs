#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System.Collections.Generic;

namespace OsEngine.Market.Servers.GateIo.GateIoFutures.Entities
{
    public class MdResponse
    {
        public string t { get; set; }
        public string contract { get; set; }
        public string id { get; set; }
        public List<Ask> asks { get; set; }
        public List<Bid> bids { get; set; }
    }

    public class Ask
    {
        public string p { get; set; }
        public string s { get; set; }
    }

    public class Bid
    {
        public string p { get; set; }
        public string s { get; set; }
    }
}

