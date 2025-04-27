using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;

namespace PayinPayout.API.LIBS
{
    public class Validation
    {
        public string GetUniqueNumber(int length = 6)
        {
            string dt = DateTime.Now.ToString("yyMMddHHmmss");
            string randomstr = GetUniqueDigit();
            string autorefno = "U" + dt + Guid.NewGuid().ToString().ToUpper().Replace("-", "").Substring(10, 6);
            if (!IsOK(autorefno))
            {
                do
                {
                    autorefno = "U" + dt + Guid.NewGuid().ToString().ToUpper().Replace("-", "").Substring(10, 6);
                }
                while (!IsOK(autorefno));
            }
            return autorefno;
        }

        public bool IsOK(string s)
        {
            if (s.Length < 3) return true;

            return !s.Where((c, i) => i >= 2 && s[i - 1] == c && s[i - 2] == c && Char.IsLetter(c)).Any();
        }

        public string Fetch_UserIP()
        {
            string VisitorsIPAddress = string.Empty;
            try
            {
                if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
                {
                    VisitorsIPAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
                }
                else if (HttpContext.Current.Request.UserHostAddress.Length != 0)
                {
                    VisitorsIPAddress = HttpContext.Current.Request.UserHostAddress;
                }
            }
            catch (Exception)
            {

                //Handle Exceptions  
            }
            return VisitorsIPAddress;
        }

        public string GetUniqueDigit(int min = 0, int max = 10)
        {
            return string.Join("", Enumerable.Range(min, max).OrderBy(g => Guid.NewGuid()).Take(10).ToArray());
        }
        public string ComputeSHA512Hash(string inputString)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(inputString);
                byte[] hashBytes = sha512.ComputeHash(inputBytes);
                StringBuilder hashStringBuilder = new StringBuilder();
                foreach (byte hashByte in hashBytes)
                {
                    hashStringBuilder.Append(hashByte.ToString("x2"));
                }
                return hashStringBuilder.ToString();
            }
        }

    }
}