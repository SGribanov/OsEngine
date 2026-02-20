#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8767

using OsEngine.Entity;
using System;
using System.Collections.Generic;

namespace OsEngine.Market.Servers.OKXData.Entity
{
    public class TradeComparer : IComparer<Trade>
    {
        public int Compare(Trade x, Trade y)
        {
            int timeComparison = x.Time.CompareTo(y.Time);
            if (timeComparison != 0)
                return timeComparison;

            if (long.TryParse(x.Id, out long xId) && long.TryParse(y.Id, out long yId))
            {
                return xId.CompareTo(yId);
            }

            return string.Compare(x.Id, y.Id, StringComparison.Ordinal);
        }
    }
}


