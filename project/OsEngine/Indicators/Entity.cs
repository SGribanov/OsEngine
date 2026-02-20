#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8620, CS8622, CS8625, CS8629, CS8765, CS8767

using System.Collections.Generic;


namespace OsEngine.Indicators
{
    public class Entity
    {

        public static readonly List<string> CandlePointsArray = new List<string>
            {"Open","High","Low","Close","Median","Typical"};

        /// <summary>
        /// what price of candle taken when building
        /// какая цена свечи берётся при построении
        /// </summary>
        public enum CandlePointType
        {
            /// <summary>
            /// Open
            /// </summary>
            Open,

            /// <summary>
            /// High
            /// </summary>
            High,

            /// <summary>
            /// Low
            /// </summary>
            Low,

            /// <summary>
            /// Close
            /// </summary>
            Close,

            /// <summary>
            /// Median. (High + Low) / 2
            /// </summary>
            Median,

            /// <summary>
            /// Typical price (High + Low + Close) / 3
            /// </summary>
            Typical
        }

    }
}
