#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8767


using System.Text;

namespace OsEngine.Market.Servers.MoexFixFastSpot.FIX
{
    class LogonMessage: AFIXMessageBody
    {
        public int EncryptMethod;
        public int HeartBtInt;
        public bool ResetSeqNumFlag;
        public string Password;
        public string NewPassword = "";
        public string LanguageID = "E"; // English by default. R - for Russian, E - for English

        public override string ToString()
        {
            string reset = ResetSeqNumFlag ? "Y" : "N";

            StringBuilder sb = new StringBuilder();

            sb.Append("98=").Append(EncryptMethod).Append('\u0001');
            sb.Append("108=").Append(HeartBtInt).Append('\u0001');
            sb.Append("141=").Append(reset).Append('\u0001');
            sb.Append("554=").Append(Password).Append('\u0001');
            if (NewPassword != "")
            {
                sb.Append("925=").Append(NewPassword).Append('\u0001');
            }
            sb.Append("6936=").Append(LanguageID).Append('\u0001');

            return sb.ToString();
        }        
    }
}


