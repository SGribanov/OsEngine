#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8767

using System.Text;

namespace OsEngine.Market.Servers.MoexFixFastSpot.FIX
{
    class LogoutMessage: AFIXMessageBody
    {        
        public string Text = " ";

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("58=").Append(Text).Append('\u0001');

            return sb.ToString();
        }
    }
}


