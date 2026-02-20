#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System.Collections.Generic;


namespace OsEngine.Market.Servers.OKX.Entity
{
    public class SecurityResponse
    {
        public string code;
        public List<SecurityResponseItem> data;
    }


    // https://www.okx.com/docs-v5/en/#public-data-rest-api-get-instruments
    public class SecurityResponseItem
    {
        public string alias;
        public string baseCcy;
        public string category;
        public string ctMult;
        public string ctType;
        public string ctVal;
        public string ctValCcy;
        public string expTime;
        public string instId;
        public string instType;
        public string lever;
        public string listTime;
        public string lotSz;
        public string maxIcebergSz;
        public string maxLmtSz;
        public string maxMktSz;
        public string maxStopSz;
        public string maxTriggerSz;
        public string maxTwapSz;
        public string minSz;
        public string optType;
        public string quoteCcy;
        public string settleCcy;
        public string state;
        public string stk;
        public string tickSz;
        public string uly; // underlying security
    }

    public class SecurityUnderlyingResponse
    {
        public string code;
        public List<List<string>> data;
    }

}
