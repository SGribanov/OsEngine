#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System.Collections.Generic;

namespace OsEngine.Market.Servers.OKX.Entity
{

    public class RequestSubscribe<T>
    {
        public string op = "subscribe";
        public List<T> args;
    }

    public class SubscribeArgs
    {
        public string channel;
        public string instId;
    }

    public class SubscribeArgsAccount
    {
        public string channel;
        public string instType;
    }

    public class SubscribeArgsOption
    {
        public string channel;
        public string instFamily;
    }

}

