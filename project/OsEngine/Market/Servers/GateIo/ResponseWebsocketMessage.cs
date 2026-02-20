#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

namespace OsEngine.Market.Servers.GateIo
{
    public class ResponseWebsocketMessage<T>
    {
        public string time;
        public string time_ms;
        public string channel;
        public string Event;
        public string error;
        public T result;
    }
}

