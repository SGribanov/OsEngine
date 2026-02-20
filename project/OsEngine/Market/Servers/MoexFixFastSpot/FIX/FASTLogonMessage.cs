#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8767

using System.Text;

namespace OsEngine.Market.Servers.MoexFixFastSpot.FIX
{
    class FASTLogonMessage: AFIXMessageBody
    {
        public string Username = "user0"; // user1, user2
        public string Password = "pass0"; // pass1, pass2
        public string DefaultApplVerID = "9";

        public override string ToString()
        {            
            StringBuilder sb = new StringBuilder();

            sb.Append("553=").Append(Username).Append('\u0001');
            sb.Append("554=").Append(Password).Append('\u0001');
            sb.Append("1137=").Append(DefaultApplVerID).Append('\u0001');

            return sb.ToString();
        }        
    }
}


