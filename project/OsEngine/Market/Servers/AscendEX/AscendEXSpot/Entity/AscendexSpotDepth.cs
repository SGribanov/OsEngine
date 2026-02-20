#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System.Collections.Generic;

namespace OsEngine.Market.Servers.AscendexSpot.Entity
{
    class AscendexSpotDepthResponse
    {
        public string m { get; set; } // "depth-snapshot"
        public string symbol { get; set; }
        public AscendexSpotDepthtData data { get; set; }
    }

    class AscendexSpotDepthtData
    {
        public string seqnum { get; set; }
        public string ts { get; set; }
        public List<List<string>> asks { get; set; }
        public List<List<string>> bids { get; set; }
    }
}

