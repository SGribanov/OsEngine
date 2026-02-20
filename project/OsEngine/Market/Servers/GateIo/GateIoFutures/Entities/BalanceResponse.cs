#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629


namespace OsEngine.Market.Servers.GateIo.GateIoFutures.Entities
{
    public class BalanceResponse
    {
        public string balance { get; set; }
        public string change { get; set; }
        public string text { get; set; }
        public string time { get; set; }
        public string time_ms { get; set; }
        public string type { get; set; }
        public string user { get; set; }
        public string currency { get; set; }
    }
}

