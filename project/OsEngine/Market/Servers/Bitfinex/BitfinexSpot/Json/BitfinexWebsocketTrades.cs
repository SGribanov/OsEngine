#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629



namespace OsEngine.Market.Servers.Bitfinex.Json
{
    public class BitfinexSubscriptionResponse
    {
        public string Event { get; set; }
        public string Channel { get; set; }
        public string ChanId { get; set; }
        public string Symbol { get; set; }
        public string Pair { get; set; }
    }

    class BitfinexAuthResponseWebSocket
    {
        public string Event { get; set; }
        public string Status { get; set; }
        public string ChanId { get; set; }
        public string UserId { get; set; }
        public string AuthId { get; set; }
        public string Msg { get; set; }
    }
}
