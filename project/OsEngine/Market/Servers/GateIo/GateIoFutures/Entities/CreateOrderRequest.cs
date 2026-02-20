#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629


namespace OsEngine.Market.Servers.GateIo.Futures.Request
{
    public partial class CreateOrderRequest
    {
        public string contract { get; set; }
        public string size { get; set; }
        public string iceberg { get; set; }
        public string price { get; set; }
        public string tif { get; set; }
        public string text { get; set; }
        public string amend_text { get; set; }

        //public string auto_size { get; set; }
        public string close { get; set; }
        public string reduce_only { get; set; }
    }

    public partial class CreateOrderRequestDoubleModeClose
    {
        public string contract { get; set; }
        public string size { get; set; }
        public string iceberg { get; set; }
        public string price { get; set; }
        public string tif { get; set; }
        public string text { get; set; }
        public string amend_text { get; set; }

        //public string auto_size { get; set; }
        public string close { get; set; }
        public string reduce_only { get; set; }
    }
}

