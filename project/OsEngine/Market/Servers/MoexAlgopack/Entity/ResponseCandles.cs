#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System.Collections.Generic;
using Newtonsoft.Json;

namespace OsEngine.Market.Servers.MoexAlgopack.Entity
{
    public class ResponseCandles
    {
        public Candles candles { get; set; }
    }

    public class Candles
    {
        [JsonIgnore]
        public List<string> columns { get; set; }
        public List<List<string>> data {get; set;}
    }
}

