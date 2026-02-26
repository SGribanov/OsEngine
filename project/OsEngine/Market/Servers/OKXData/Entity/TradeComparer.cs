#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8767, CS8620

using OsEngine.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace OsEngine.Market.Servers.OKXData.Entity
{
    public class TradeComparer : IComparer<Trade>
    {
        public int Compare(Trade x, Trade y)
        {
            int timeComparison = x.Time.CompareTo(y.Time);
            if (timeComparison != 0)
                return timeComparison;

            if (long.TryParse(x.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out long xId)
                && long.TryParse(y.Id, NumberStyles.Integer, CultureInfo.InvariantCulture, out long yId))
            {
                return xId.CompareTo(yId);
            }

            return string.Compare(x.Id, y.Id, StringComparison.Ordinal);
        }
    }
}



