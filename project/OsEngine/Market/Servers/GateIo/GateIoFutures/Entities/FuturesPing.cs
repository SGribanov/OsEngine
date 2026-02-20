#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629


namespace OsEngine.Market.Servers.GateIo.Futures.Request
{
    public class FuturesPing
    {
        public long time { get; set; }
        public string channel { get; set; }
    }

    public class FuturesPong
    {
        public string time { get; set; }
        public string channel { get; set; }
        public string @event { get; set; }
        public string error { get; set; }
        public string result { get; set; }
    }
}

