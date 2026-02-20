#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

namespace OsEngine.Market.Servers.MoexFixFastTwimeFutures.Entity
{
    public class OrderChange
    {
        public string NameID { get; set; }
        public int RptSeq { get; set; }
        public string MDEntryID { get; set; }
        public OrderAction Action { get; set; }
        public OrderType OrderType { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }
    }

    public enum OrderAction
    {
        Add,
        Change,
        Delete,
        None
    }

    public enum OrderType
    {
        Bid,
        Ask,
        None
    }
}


