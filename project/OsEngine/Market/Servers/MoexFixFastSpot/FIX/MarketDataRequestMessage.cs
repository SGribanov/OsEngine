#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8767

using System.Text;

namespace OsEngine.Market.Servers.MoexFixFastSpot.FIX
{    
    class MarketDataRequestMessage: AFIXMessageBody
    {
        public string ApplID = "OLR"; //"TLR";
        public string ApplBegSeqNum;
        public string ApplEndSeqNum;
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("1180=").Append(ApplID).Append('\u0001');
            sb.Append("1182=").Append(ApplBegSeqNum).Append('\u0001');
            sb.Append("1183=").Append(ApplEndSeqNum).Append('\u0001');

            return sb.ToString();
        }
    }
}


