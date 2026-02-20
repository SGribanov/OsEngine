#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using Newtonsoft.Json;

namespace OsEngine.Market.Servers.TraderNet.Entity
{
    public class RequestCandle
    {
        public Q q { get; set; }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class Q
        {
            public string cmd { get; set; }
            public Params @params { get; set; }

            public string SID;
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class Params
        {
            public string id;
            public int userId;
            public int timeframe;
            public int count;
            public string date_from;
            public string date_to;
            public string intervalMode = "ClosedRay";
            public string apiKey;

        }
    }
}

