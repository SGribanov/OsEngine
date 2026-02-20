#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System;
using OsEngine.Entity;

namespace OsEngine.Market.Servers.MoexFixFastCurrency.Entity
{
    public class WaitingTrade
    {
        public string UniqueName { get; set; }
        public int RptSeq { get; set; }
        public Trade Trade;
    }
}

