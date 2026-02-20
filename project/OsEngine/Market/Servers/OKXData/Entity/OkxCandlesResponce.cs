#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8767, CS8620

using System.Collections.Generic;

namespace OsEngine.Market.Servers.OKXData.Entity
{
    public class OkxCandlesResponce
    {
        public string code;
        public string msg;
        public List<List<string>> data;
    }
}



