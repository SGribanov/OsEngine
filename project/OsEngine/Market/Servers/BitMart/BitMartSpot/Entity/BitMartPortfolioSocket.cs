#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

/*
 *Your rights to use the code are governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 *Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System.Collections.Generic;

namespace OsEngine.Market.Servers.BitMart.Json
{
    public class BitMartBalanceDetail
    {
        public string ccy;
        public string av_bal;
        public string fz_bal;
    }


    public class BitMartPortfolioItem
    {
        public string event_type;
        public string event_time;
        public List<BitMartBalanceDetail> balance_details;
    }

    public class BitMartPortfolioSocket : List<BitMartPortfolioItem>
    {

    }
}

