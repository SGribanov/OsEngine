#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8767

namespace OsEngine.Market.Servers.Pionex.Entity
{
    public class SendNewOrder
    {
        public string symbol;
        public string side;
        public string type;
        public string clientOrderId;
        public string size;            // Quantity. Required in limit order and market sell order.
        public string price;           // Required in limit order.
        public string amount;          // Buying amount. Required in market buy order.
        public bool IOC;               // Immediate or Cancel (IOC) Order
    }
}


