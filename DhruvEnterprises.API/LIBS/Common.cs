using DhruvEnterprises.Data;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;


namespace DhruvEnterprises.API.LIBS
{
    public static class Common
    {

        // public static Object thisLock = new Object();

        public static void LogException(Exception ex, string comment = "", string path = "")
        {
            try
            {
                DateTime dateTime = DateTime.Now;
                string date = dateTime.ToString("ddMMyy");
                string hour = dateTime.Hour.ToString();

                string filepath = !string.IsNullOrEmpty(path) ? path.Replace("ApiActivityLog", "ApiExceptionLog") : HttpContext.Current.Server.MapPath("~/ApiExceptionLog/" + date + "/");  //Text File Path

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }
                filepath = filepath + date + "-" + hour + ".txt";   //Text File Name

                if (!File.Exists(filepath))
                {
                    File.Create(filepath).Dispose();
                }

                //lock (thisLock)
                //{

                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine("================= ***EXCEPTION DETAILS" + " " + dateTime + "*** =============");
                    sw.WriteLine("COMMENT:");
                    sw.WriteLine(comment);
                    sw.WriteLine();
                    sw.WriteLine("Error Occured: Service");
                    sw.WriteLine();



                    sw.WriteLine("Error Code:");
                    sw.WriteLine(ex.GetHashCode().ToString());
                    sw.WriteLine();

                    sw.WriteLine("Base Exception:");
                    sw.WriteLine(ex.GetBaseException()?.ToString());
                    sw.WriteLine();

                    sw.WriteLine("Exception Type:");
                    sw.WriteLine(ex.GetType()?.ToString());
                    sw.WriteLine();

                    sw.WriteLine("Inner Exception:");
                    sw.WriteLine(ex.InnerException?.ToString());
                    sw.WriteLine();

                    sw.WriteLine("Exception Message: ");
                    sw.WriteLine(ex.Message);
                    sw.WriteLine();

                    sw.WriteLine("Exception Source:  ");
                    sw.WriteLine(ex.Source);
                    sw.WriteLine();

                    sw.WriteLine("Stack Trace: ");
                    sw.WriteLine(ex.StackTrace?.ToString());
                    sw.WriteLine();

                    sw.WriteLine("Generic Info: ");
                    sw.WriteLine(ex.ToString());
                    sw.WriteLine();


                    sw.WriteLine("=================================== ***End*** =============================================");
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();

                }
                //}
            }
            catch (Exception e)
            {
                //Common.LogException(ex);
            }

        }

        public static void LogActivity(string str, string path = "")
        {
            try
            {
                DateTime dateTime = DateTime.Now;
                string date = dateTime.ToString("ddMMyy");
                string hour = dateTime.Hour.ToString();

                string filepath = !string.IsNullOrEmpty(path) ? path : HttpContext.Current.Server.MapPath("~/ApiActivityLog/" + date + "/");  //Text File Path

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }

                filepath = filepath + date + "-" + hour + ".txt";   //Text File Name


                if (!File.Exists(filepath))
                {
                    File.Create(filepath).Dispose();
                }

                // File.SetAccessControl(filepath,FileShare.ReadWrite;

                //  lock (thisLock)
                //  {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine("----------MESSAGE AT: " + dateTime + "----------------");
                    sw.WriteLine();
                    sw.WriteLine(str);

                    sw.WriteLine("-----------------------------------------------------");
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();

                }
                // }


            }
            catch (Exception e)
            {
                Common.LogException(e, "Save Activity Log= " + str, path);
            }

        }

        public static void UpdateRequestResponse()
        {

            RequestResponse requestResponse = new RequestResponse();

        }

        public static string Fetch_UserIP()
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

        public static string GetUniqueNumber(int length = 4)
        {
            string dt = DateTime.Now.ToString("yyMMddhhmmss");
            string randomstr = GetUniqueDigit();

            var rndDigits = new System.Text.StringBuilder().Insert(0, randomstr, length).ToString().ToCharArray();
            return "E" + dt + "T" + string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
        }

        public static string GetUniqueDigit(int length = 10)
        {
            var rndDigits = new System.Text.StringBuilder().Insert(0, "5432106789", length).ToString().ToCharArray();
            return string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
        }

        public static string GetUniqueAlphaNumeric(int length = 11)
        {
            string dt = DateTime.Now.ToString("yyMMddhhmmss");
            var rndDigits = new System.Text.StringBuilder().Insert(0, "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", length).ToString().ToCharArray();
            return string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
        }

        public static string HtmlDecode(this string str)
        {
            //    ' is replaced with &apos;
            //    " is replaced with &quot;
            //    & is replaced with &amp;
            //    < is replaced with &lt;
            //    > is replaced with &gt;
            str = str.Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&amp;", "&")
                .Replace("&apos;", "'")
                .Replace("&quot", "\"");

            return str;
        }

        public static string HtmlEncode(this string str)
        {
            str = str.Replace("<", "&lt;")
                     .Replace(">", "&gt;")
                     .Replace("&", "&amp;")
                     .Replace("'", "&apos;")
                     .Replace("\"", "&quot");

            return str;
        }

        public static string GetUniqueNumberOp(int length = 11)
        {
            string dt = DateTime.Now.ToString("yyMMddhhmmss");
            var rndDigits = new StringBuilder().Insert(0, "0123456789", length).ToString().ToCharArray();
            return "E" + dt + "T" + string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
        }

        public static string GetUniqueNumbericOP(int length = 11)
        {
            string dt = DateTime.Now.ToString("yyMMddhhmmss");
            var rndDigits = new StringBuilder().Insert(0, "0123456789", length).ToString().ToCharArray();
            return string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
        }

        public static string GetUniqueAlphaticLW(int length = 11)
        {
            string dt = DateTime.Now.ToString("yyMMddhhmmss");
            var rndDigits = new StringBuilder().Insert(0, "abcdefghijklmnopqrstuvwxyz", length).ToString().ToCharArray();
            return string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
        }

        public static string GetUniqueAlphaticUP(int length = 11)
        {
            string dt = DateTime.Now.ToString("yyMMddhhmmss");
            var rndDigits = new StringBuilder().Insert(0, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", length).ToString().ToCharArray();
            return string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
        }

        public static string GetUniqueAlphaticMix(int length = 11)
        {
            string dt = DateTime.Now.ToString("yyMMddhhmmss");
            var rndDigits = new StringBuilder().Insert(0, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", length).ToString().ToCharArray();
            return string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
        }

        public static string GetUniqueAlphaNumericLW(int length = 11)
        {
            string dt = DateTime.Now.ToString("yyMMddhhmmss");
            var rndDigits = new StringBuilder().Insert(0, "0123456789abcdefghijklmnopqrstuvwxyz", length).ToString().ToCharArray();
            return string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
        }

        public static string GetUniqueAlphaNumericUP(int length = 11)
        {
            string dt = DateTime.Now.ToString("yyMMddhhmmss");
            var rndDigits = new StringBuilder().Insert(0, "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", length).ToString().ToCharArray();
            return string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
        }

        public static string GetUniqueAlphaNumericMIX(int length = 11)
        {
            string dt = DateTime.Now.ToString("yyMMddhhmmss");
            var rndDigits = new StringBuilder().Insert(0, "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", length).ToString().ToCharArray();
            return string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
        }
    }
}