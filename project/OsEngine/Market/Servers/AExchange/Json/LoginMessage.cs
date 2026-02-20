#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using Newtonsoft.Json;
using OsEngine.Market.Servers.AE.Json;

namespace OsEngine.Market.Servers.AE.Json
{
    public class WebSocketLoginMessage : WebSocketMessageBase
    {
        [JsonProperty("login")]
        public string Login { get; set; }

        public WebSocketLoginMessage()
        {
            Type = "Login";
        }
    }
}

