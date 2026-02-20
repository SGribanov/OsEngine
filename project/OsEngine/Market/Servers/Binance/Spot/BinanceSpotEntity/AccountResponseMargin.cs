#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629

using System.Collections.Generic;

namespace OsEngine.Market.Servers.Binance.Spot.BinanceSpotEntity
{
    public class AccountResponseMargin
    {
        public string borrowEnabled;

        public string marginLevel;

        public string totalAssetOfBtc;

        public string totalLiabilityOfBtc;

        public string totalNetAssetOfBtc;

        public string tradeEnabled;

        public string transferEnabled;

        public List<UserAssets> userAssets;
    }


    public class UserAssets
    {
        public string asset;

        public string borrowed;

        public string free;

        public string interest;

        public string locked;

        public string netAsset;

    }

}
