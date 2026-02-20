#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System;

namespace OsEngine.Market.Servers.MoexFixFastCurrency.Entity
{
    public class OrderChange
    {
        public string UniqueName { get; set; }
        public int RptSeq { get; set; }
        public string MDEntryID { get; set; }
        public string Action { get; set; }
        public string OrderType { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
    }
}


