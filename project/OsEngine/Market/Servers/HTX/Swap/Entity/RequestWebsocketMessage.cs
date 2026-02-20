#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

namespace OsEngine.Market.Servers.HTX.Swap.Entity
{
    public class WebSocketAuthenticationRequestFutures
    {
        public string op { get { return "auth"; } }
        public string type { get { return "api"; } }

        public string AccessKeyId;
        public string SignatureMethod { get { return "HmacSHA256"; } }
        public string SignatureVersion { get { return "2"; } }
        public string Timestamp;
        public string Signature;
    }
}

