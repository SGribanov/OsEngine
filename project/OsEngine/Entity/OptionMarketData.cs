using System;

#nullable enable

namespace OsEngine.Entity
{
    public class OptionMarketData
    {
        public double Delta;

        public double Vega;

        public double Gamma;

        public double Theta;

        public double Rho;

        public double MarkIV;

        public double MarkPrice;

        public string SecurityName = string.Empty;

        public DateTime TimeCreate;

        public double OpenInterest;

        public double BidIV;

        public double AskIV;

        public double UnderlyingPrice;

        public string UnderlyingAsset = string.Empty;
    }

    public class OptionMarketDataForConnector
    {
        public string Delta = string.Empty;

        public string Vega = string.Empty;

        public string Gamma = string.Empty;

        public string Theta = string.Empty;

        public string Rho = string.Empty;

        public string MarkIV = string.Empty;

        public string MarkPrice = string.Empty;

        public string SecurityName = string.Empty;

        public string TimeCreate = string.Empty;

        public string OpenInterest = string.Empty;

        public string BidIV = string.Empty;

        public string AskIV = string.Empty;

        public string UnderlyingPrice = string.Empty;

        public string UnderlyingAsset = string.Empty;
    }
}
