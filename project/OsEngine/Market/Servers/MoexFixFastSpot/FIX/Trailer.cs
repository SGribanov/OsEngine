#nullable enable
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8605, CS8618, CS8619, CS8622, CS8625, CS8629, CS8767

using System;
using System.Text;

namespace OsEngine.Market.Servers.MoexFixFastSpot.FIX
{
    class Trailer
    {
        private string _Message;

        public Trailer(string message)
        {
            _Message = message;
        }

        public override string ToString()
        {
            int sumChar = 0;

            for (int i = 0; i < _Message.Length; i++)
            {
                sumChar += (int)_Message[i];
            }

            string checksum = Convert.ToString(sumChar % 256).PadLeft(3, '0');

            StringBuilder sb = new StringBuilder();

            sb.Append("10=").Append(checksum).Append('\u0001');

            return sb.ToString();
        }
    }
}


