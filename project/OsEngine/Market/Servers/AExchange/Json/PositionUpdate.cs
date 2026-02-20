#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace OsEngine.Market.Servers.AE.Json
{
    public class WebSocketPositionUpdateMessage : WebSocketMessageBase
    {
        [JsonProperty("account")]
        public string AccountNumber { get; set; }

        [JsonProperty("ticker")]
        public string Ticker { get; set; }

        [JsonProperty("open_date")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime OpenDate { get; set; }

        [JsonProperty("shares")]
        public decimal Shares { get; set; }

        [JsonProperty("open_price")]
        public decimal OpenPrice { get; set; }
    }
}

