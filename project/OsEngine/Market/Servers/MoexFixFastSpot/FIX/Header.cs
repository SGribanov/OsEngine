#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8767

using System.Globalization;
using System.Text;

namespace OsEngine.Market.Servers.MoexFixFastSpot.FIX
{
    class Header : AFIXHeader
    {        
        public Header()
        {
            BeginString = "FIX.4.4"; //Версия FIX "FIX.4.4",
        }
      
        public override string GetHalfMessage()
        {
            string sendingTime = SendingTime.ToString("yyyyMMdd-HH:mm:ss.fff", CultureInfo.InvariantCulture);

            StringBuilder sb = new StringBuilder();

            sb.Append("35=").Append(MsgType).Append('\u0001');
            sb.Append("34=").Append(MsgSeqNum).Append('\u0001');
            sb.Append("49=").Append(SenderCompID).Append('\u0001');
            sb.Append("52=").Append(sendingTime).Append('\u0001');
            sb.Append("56=").Append(TargetCompID).Append('\u0001');

            return sb.ToString();
        }        
    }
}


