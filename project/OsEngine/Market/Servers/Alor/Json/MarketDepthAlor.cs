#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Collections.Generic;

namespace OsEngine.Market.Servers.Alor.Json
{
    public class MarketDepthFullMessage
    {
        public string guid;

        public MarketDepthAlor data;

    }

    public class MarketDepthAlor
    {
        public List<MarketDepthLevelAlor> bids;

        public List<MarketDepthLevelAlor> asks;

        public string ms_timestamp;

        public string existing;

        public bool snapshot;
    }



    public class MarketDepthLevelAlor
    {
        public string price;

        public string volume;
    }
}

