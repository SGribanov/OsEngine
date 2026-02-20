#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using Newtonsoft.Json;

namespace OsEngine.Market.Servers.AE.Json
{
    public class WebSocketPlaceOrderMessage : WebSocketMessageBase
    {
        [JsonProperty("account")]
        public string Account { get; set; }

        [JsonProperty("ticker")]
        public string Ticker { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("shares")]
        public decimal Shares { get; set; }

        [JsonProperty("ext_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ExternalId { get; set; }

        [JsonProperty("comment", NullValueHandling = NullValueHandling.Ignore)]
        public string Comment { get; set; }

        public WebSocketPlaceOrderMessage()
        {
            Type = "PlaceOrder";
        }
    }
}

