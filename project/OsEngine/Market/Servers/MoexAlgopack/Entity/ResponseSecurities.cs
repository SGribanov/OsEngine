#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System.Collections.Generic;

namespace OsEngine.Market.Servers.MoexAlgopack.Entity
{
    public class ResponseSecurities
    {
        public Securities securities { get; set; }
    }

    public class Securities
    {
        public List<string> columns { get; set; }
        public List<List<string>> data { get; set; }
    }
}

