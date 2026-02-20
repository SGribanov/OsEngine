#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8767

using Newtonsoft.Json;
using System.Collections.Generic;

namespace OsEngine.Market.Servers.Deribit.Entity
{
    public class JsonRequest
    {
        public string jsonrpc { get { return "2.0"; } }
        public int id { get; set; } 
        public string method { get; set; }
        public Params @params { get; set; }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class Params
        {
            public string grant_type { get; set; }
            public string client_id { get; set; }
            public long timestamp { get; set; }
            public string signature { get; set; }
            public string nonce { get; set; }
            public string data { get; set; }
            public List<string> channels { get; set; }
            public string access_token { get; set; }
            public string order_id { get; set; }
            public string instrument_name { get; set; }
            public decimal amount { get; set; }
            public string type { get; set; }
            public string label { get; set; }
            public decimal price { get; set; }
            public string post_only { get; set; }
        }        
    }    
}


