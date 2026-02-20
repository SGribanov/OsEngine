#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System.Collections.Generic;

namespace OsEngine.Market.Servers.MoexFixFastTwimeFutures.Entity
{
    public class ControlFastDepth
    {
        public ControlFastDepth()
        {
            AsksF = new List<ControlDepthLevel>();
            BidsF = new List<ControlDepthLevel>();
        }

        public List<ControlDepthLevel> AsksF;

        public List<ControlDepthLevel> BidsF;
    }

    public class ControlDepthLevel
    {
        public int ImmutabilityCount { get; set; }

        public double Ask;

        public double Bid;

        public double Price;
    }
}

