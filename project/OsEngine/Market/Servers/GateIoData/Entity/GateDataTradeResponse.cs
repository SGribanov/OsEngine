#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8767


namespace OsEngine.Market.Servers.GateIoData.Entity
{
    public class GateDataTradeResponse
    {
        public string id { get; set; }
        public string create_time { get; set; }
        public string create_time_ms { get; set; }
        public string currency_pair { get; set; }
        public string side { get; set; }
        public string amount { get; set; }
        public string contract { get; set; }
        public string size { get; set; }
        public string price { get; set; }
    }
}


