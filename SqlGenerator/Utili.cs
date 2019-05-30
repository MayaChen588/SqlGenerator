using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlGenerator
{
    public class Utili
    {
        public static string ToHex(int num)
        {
            string[] hex = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F" };
            string hexval = String.Empty;
            int balnum = num;
            int balval = 0;

            while (true)
            {
                balval = balnum % 16;
                balnum = balnum / 16;

                hexval = hex[balval] + hexval;

                if (balnum == 0)
                {
                    break;
                }
            }

            return hexval.Length % 2 == 0 ? hexval : "0" + hexval;
        }


        public static byte[] HexStringToBytes(string hex)
        {
            if (String.IsNullOrEmpty(hex))
            {
                return new byte[0];
            }

            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
