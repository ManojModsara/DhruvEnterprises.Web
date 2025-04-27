using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace DhruvEnterprises.API.LIBS
{

    public static class EncryptDecrypt
    {

        public static String Encrypt(string source)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] Key = { 12, 13, 14, 15, 16, 17, 18, 19 };
            byte[] IV = { 12, 13, 14, 15, 16, 17, 18, 19 };

            ICryptoTransform encryptor = des.CreateEncryptor(Key, IV);
            try
            {
                byte[] IDToBytes = ASCIIEncoding.ASCII.GetBytes(source);
                byte[] encryptedID = encryptor.TransformFinalBlock(IDToBytes, 0, IDToBytes.Length);
                return Convert.ToBase64String(encryptedID);
            }
            catch (FormatException)
            {
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static string Decrypt(string encrypted)
        {
            byte[] Key = { 12, 13, 14, 15, 16, 17, 18, 19 };
            byte[] IV = { 12, 13, 14, 15, 16, 17, 18, 19 };

            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            ICryptoTransform decryptor = des.CreateDecryptor(Key, IV);

            try
            {
                byte[] encryptedIDToBytes = Convert.FromBase64String(encrypted.Replace(" ", "+"));
                byte[] IDToBytes = decryptor.TransformFinalBlock(encryptedIDToBytes, 0, encryptedIDToBytes.Length);
                return ASCIIEncoding.ASCII.GetString(IDToBytes);
            }
            catch (FormatException)
            {
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}