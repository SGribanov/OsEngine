#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

namespace OsEngine.Market.Servers.HTX.Spot.Entity
{
    public class WebSocketAuthenticationRequestV2
    {
        public class Params
        {
            public string authType { get { return "api"; } }
            public string accessKey { get; set; }
            public string signatureMethod { get { return "HmacSHA256"; } }
            public string signatureVersion { get { return "2.1"; } }
            public string timestamp { get; set; }
            public string signature { get; set; }
        }

        public string action { get { return "req"; } }
        public string ch { get { return "auth"; } }
        public Params @params;

    }
}

