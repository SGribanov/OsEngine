#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

namespace OsEngine.Market.Servers.GateIo.GateIoFutures.Entities
{
    public partial class GateFuturesWsRequest
    {
        public long time { get; set; }
        public long id { get; set; }
        public string channel { get; set; }
        public string @event { get; set; }
        public string[] payload { get; set; }
        public Auth auth { get; set; }
    }

    public partial class Auth
    {
        public string method { get; set; }
        public string KEY { get; set; }
        public string SIGN { get; set; }
    }
}

