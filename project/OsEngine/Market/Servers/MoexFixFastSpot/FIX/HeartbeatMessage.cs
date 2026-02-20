#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8767

using System.Text;

namespace OsEngine.Market.Servers.MoexFixFastSpot.FIX
{
    class HeartbeatMessage: AFIXMessageBody
    {
        public string TestReqID { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("112=").Append(TestReqID).Append('\u0001');

            return sb.ToString();
        }        
    }
}


