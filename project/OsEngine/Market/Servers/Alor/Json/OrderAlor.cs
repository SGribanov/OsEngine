#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

namespace OsEngine.Market.Servers.Alor.Json
{
    public class OrderAlor
    {
        public string id;
        public string symbol;
        public string brokerSymbol;
        public string exchange;
        public string comment;
        public string type;
        public string side;
        public string status;
        public string transTime;
        public string endTime;
        public string qtyUnits;
        public string qtyBatch;
        public string qty;
        public string filledQtyUnits;
        public string filledQtyBatch;
        public string filled;
        public string price;
        public string existing;
    }
}

