#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System.Collections.Generic;


namespace OsEngine.Market.Servers.BitGet.BitGetFutures.Entity
{
    public class RequestWebsocketAuth
    {
        public string op;
        public List<AuthItem> args;
    }

    public class AuthItem
    {
        public string apiKey;
        public string passphrase;
        public string timestamp;
        public string sign;
    }
}

