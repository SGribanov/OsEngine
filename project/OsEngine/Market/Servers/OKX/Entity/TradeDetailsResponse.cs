#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System.Collections.Generic;


namespace OsEngine.Market.Servers.OKX.Entity
{
    public class TradeDetailsResponse
    {
        public string code;
        public string msg;
        public List<TradeDetailsObject> data;
    }


    public class TradeDetailsObject
    {
        public string instType;
        public string instId;
        public string tradeId;
        public string ordId;
        public string clOrdId;
        public string billId;
        public string tag;
        public string fillPx;
        public string fillSz;
        public string side;
        public string posSide;
        public string execType;
        public string feeCcy;
        public string fee;
        public string ts;

    }
}
