using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace RSAS.Utilities
{
    public static class SecurityUtilities
    {
        public static string MD5Hash(string inputString)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = ASCIIEncoding.ASCII.GetBytes(inputString);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            string t = "";

            //convert each byte in the resulting hashed byte array to a string in Hex format ("X") and concatenate
            foreach (byte hash in hashBytes)
                t += hash.ToString("X");

            return t;
        }
    }
}
