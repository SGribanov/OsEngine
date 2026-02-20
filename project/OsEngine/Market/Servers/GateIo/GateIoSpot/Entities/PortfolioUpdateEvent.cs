#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

namespace OsEngine.Market.Servers.GateIo.GateIoSpot.Entities
{
    public class CurrencyBalance
    {
        public string timestamp { get; set; }
        public string timestamp_ms { get; set; }
        public string user { get; set; }
        public string currency { get; set; }
        public string change { get; set; }
        public string total { get; set; }
        public string available { get; set; }
        public string freeze { get; set; }
        public string freeze_change { get; set; }
        public string change_type { get; set; }
    }
}

