#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629


namespace OsEngine.Market.Servers.GateIo.GateIoFutures.Entities.Response
{
    public class GfAccount
    {
        public string order_margin { get; set; }
        public string point { get; set; }
        public CancelOrderResponseHistory history { get; set; }
        public string unrealised_pnl { get; set; }
        public string total { get; set; }
        public string available { get; set; }
        public string currency { get; set; }
        public string position_margin { get; set; }
        public string user { get; set; }
    }

    public class CancelOrderResponseHistory
    {
        public decimal dnw { get; set; }
        public string pnl { get; set; }
        public string point_refr { get; set; }
        public string refr { get; set; }
        public string point_fee { get; set; }
        public string fund { get; set; }
        public string fee { get; set; }
        public string point_dnw { get; set; }
    }
}

