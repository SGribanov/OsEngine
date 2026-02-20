#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

namespace OsEngine.Market.Servers.AscendexSpot.Entity
{
    public class AscendexSpotApiKeyInfoResponse
    {
        public string code { get; set; }
        public AscendexSpotApiKeyInfo data { get; set; }
    }

    public class AscendexSpotApiKeyInfo
    {
        public string accountGroup { get; set; }
        public string email { get; set; }
        public string expireTime { get; set; } // UTC timestamp millisec
        public string[] allowedIps { get; set; }
        public string[] cashAccount { get; set; }
        public string[] marginAccount { get; set; }
        public string userUID { get; set; }
        public string tradePermission { get; set; }
        public string transferPermission { get; set; }
        public string viewPermission { get; set; }
    }
}

