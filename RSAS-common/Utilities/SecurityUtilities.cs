using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace RSAS.Utilities
{
    public static class SecurityUtilities
    {
        const char SaltHashSeparator = '$';
        const int RBKDF2SaltSize = 128;
        const int RBKDF2HashSize = 256;
        const int RBKDF2Iterations = 5000;

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

        public static string SHA256Hash(string inputString)
        {
            SHA256 sha = SHA256.Create();

            byte[] hashBytes = sha.ComputeHash(ASCIIEncoding.ASCII.GetBytes(inputString));

            string t = "";

            //convert each byte in the resulting hashed byte array to a string in Hex format ("X") and concatenate
            foreach (byte hash in hashBytes)
                t += hash.ToString("X");

            return t;
        }

        public static string PBKDF2(string input, string salt)
        {
            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(input, Convert.FromBase64String(salt), RBKDF2Iterations);
            return salt + SaltHashSeparator + Convert.ToBase64String(rfc.GetBytes(RBKDF2HashSize));
        }

        public static string PBKDF2(string input)
        {

            byte[] salt = new byte[RBKDF2SaltSize];

            RNGCryptoServiceProvider csp = new RNGCryptoServiceProvider();
            csp.GetBytes(salt);

            return PBKDF2(input, Convert.ToBase64String(salt));
        }

        public static string SaltFromKey(string key)
        {
            if (key.Contains(SaltHashSeparator))
                return key.Split(SaltHashSeparator)[0];
            else
                throw new ArgumentException("Key does not contain valid salt/hash separator (" + SaltHashSeparator + ")");
        }
    }
}
