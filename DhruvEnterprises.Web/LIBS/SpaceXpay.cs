using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Security.Cryptography;

namespace DhruvEnterprises.Web.LIBS
{
    public static class SpaceXpay
    {
        private static string initVector = "0123456789abcdef";
        private static AesManaged CreateAes(string key)
        {
            var aes = new AesManaged();
            aes.Key = Convert.FromBase64String(key); //UTF8-Encoding
            aes.IV = System.Text.Encoding.UTF8.GetBytes(initVector);//UT8-Encoding
            return aes;
        }
        public static string encrypt(string text, string key)
        {
            using (
                AesManaged aes = CreateAes(key))
            {
                ICryptoTransform encryptor = aes.CreateEncryptor();
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(text);
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }
        public static string decrypt(string text, string key)
        {
            using (var aes = CreateAes(key))
            {
                ICryptoTransform decryptor = aes.CreateDecryptor();
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(text)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader reader = new StreamReader(cs))
                        {
                            return reader.ReadToEnd();
                        }
                    }
                }

            }
        }
    }
}