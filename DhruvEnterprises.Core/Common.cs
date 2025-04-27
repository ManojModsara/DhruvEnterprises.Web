
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Core
{
    public static class Common
    {
       // private static Object thisLock = new Object();

        public static string GetHourMinute(TimeSpan span)
        {
            var hours = (int)span.TotalHours;
            var minutes = span.Minutes.ToString("00");
            //return string.Format("{0} hr {1} min", hours, minutes);
            string hour = hours > 0 ? hours.ToString() + " hr " : string.Empty;
            string minute = minutes != "00" ? minutes + " min" : string.Empty;
            return hour + minute;
        }
        public static DateTime GetStartDateOfWeek(DateTime value)
        {
            // Get rid of the time part first...
            value = value.Date;
            int daysIntoWeek = (int)value.DayOfWeek;
            DateTime weekStartDate = value.AddDays(-daysIntoWeek);
            return weekStartDate;
        }
        public static DateTime GetEndDateOfWeek(DateTime value)
        {
            // Get rid of the time part last...
            value = value.Date;
            int daysIntoWeek = (int)value.DayOfWeek;
            DateTime weekStartDate = value.AddDays(-daysIntoWeek);
            DateTime weekEndDate = value.AddDays(7 - daysIntoWeek - 1);
            return weekEndDate;
        }
        public static string GetStartEndDateOfWeek(DateTime value)
        {
            // Get rid of the time part first and last date string...
            value = value.Date;
            int daysIntoWeek = (int)value.DayOfWeek;
            DateTime weekStartDate = value.AddDays(-daysIntoWeek);
            DateTime weekEndDate = value.AddDays(7 - daysIntoWeek - 1);
            return weekStartDate.ToString("dd/MM/yyyy") + " - " + weekEndDate.ToString("dd/MM/yyyy");
        }
        public static void LogException(Exception ex, string comment = "")
        {
            try
            {
                string filepath = System.Web.HttpContext.Current.Server.MapPath("~/ExceptionLog/");  //Text File Path

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);

                }
                filepath = filepath + DateTime.Today.ToString("dd-MM-yy") + ".txt";   //Text File Name
                if (!System.IO.File.Exists(filepath))
                {


                    System.IO.File.Create(filepath).Dispose();

                }
                //lock (thisLock)
                //{
                    using (StreamWriter sw = System.IO.File.AppendText(filepath))
                    {
                        sw.WriteLine("================= ***EXCEPTION DETAILS" + " " + DateTime.Now.ToString() + "*** =============");

                        sw.WriteLine("COMMENT:");
                        sw.WriteLine(comment);
                        sw.WriteLine();

                        sw.WriteLine("Date Time:");
                        sw.WriteLine(DateTime.Now.ToString());
                        sw.WriteLine();

                        sw.WriteLine("Error Code:");
                        sw.WriteLine(ex.GetHashCode().ToString());
                        sw.WriteLine();

                        sw.WriteLine("Base Exception:");
                        sw.WriteLine(ex.GetBaseException().ToString());
                        sw.WriteLine();

                        sw.WriteLine("Exception Type:");
                        sw.WriteLine(ex.GetType().ToString());
                        sw.WriteLine();

                        sw.WriteLine("Inner Exception:");
                        sw.WriteLine(ex.InnerException.ToString());
                        sw.WriteLine();

                        sw.WriteLine("Exception Message: ");
                        sw.WriteLine(ex.Message);
                        sw.WriteLine();

                        sw.WriteLine("Exception Source:  ");
                        sw.WriteLine(ex.Source);
                        sw.WriteLine();

                        sw.WriteLine("Stack Trace: ");
                        sw.WriteLine(ex.StackTrace.ToString());
                        sw.WriteLine();

                        sw.WriteLine("Generic Info: ");
                        sw.WriteLine(ex.ToString());
                        sw.WriteLine();


                        sw.WriteLine("=================================== ***End*** =============================================");
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();

                    }
               // }
            }
            catch (Exception e)
            {
                //Common.LogException(ex);
            }

        }
        public static void LogActivity(string str)
        {
            //try
            //{
            DateTime dateTime = DateTime.Now;
            string date = dateTime.ToString("dd-MM-yy");

            string filepath = System.Web.HttpContext.Current.Server.MapPath("~/Activity_Log/" + date + "/");  //Text File Path

            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            filepath = filepath + date + ".txt";   //Text File Name


            if (!File.Exists(filepath))
            {
                File.Create(filepath).Dispose();
            }
            //lock (thisLock)
            //{
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
            //}
            //catch (Exception e)
            //{
            //    //Common.LogException(ex);
            //}

        }

        public static string GetUniqueNumber(int length = 11)
        {
            string dt = DateTime.Now.ToString("yyMMddhhmmss");
            var rndDigits = new StringBuilder().Insert(0, "0123456789", length).ToString().ToCharArray();
            return "E" + dt + "T" + string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
        }

        public static string GetUniqueNumberic(int length = 11)
        {
            string dt = DateTime.Now.ToString("yyMMddhhmmss");
            var rndDigits = new StringBuilder().Insert(0, "0123456789", length).ToString().ToCharArray();
            return  string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
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

        public static string GetUniqueAlphaNumeric(int length = 11)
        {
            string dt = DateTime.Now.ToString("yyMMddhhmmss");
            var rndDigits = new StringBuilder().Insert(0, "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", length).ToString().ToCharArray();
            return string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
        }


        public static string GetSubstring(this string originalstr, string prestr = null, string poststr = null, int premargin = 0, int postmargin = 0, StringComparison comparison = StringComparison.InvariantCulture)
        {
            var fromLength = (!string.IsNullOrEmpty(prestr) ? prestr.ToString().ToLower() != "none" ? prestr : string.Empty : string.Empty).Length;
            var startIndex = !string.IsNullOrEmpty(prestr)
                ? prestr.ToLower() != "none" ? originalstr.IndexOf(prestr, comparison) + fromLength : 0
                : 0;

            if (startIndex < fromLength) { throw new ArgumentException("prestr: Failed to find "); }

            var endIndex = !string.IsNullOrEmpty(poststr) ?
                            poststr.ToLower() != "none" ? originalstr.IndexOf(poststr, startIndex, comparison) : originalstr.Length :
                            originalstr.Length;

            if (endIndex < 0) { throw new ArgumentException("poststr:Failed to find"); }

            startIndex += premargin;
            endIndex += postmargin;

            var subString = originalstr.Substring(startIndex, endIndex - startIndex);
            return subString;
        }

        public static string GetSplitstringByIndex(this string originalstr, string separator = null, int index = 0)
        {
            separator = separator?.Replace("\r", "")?.Replace("\t", "")?.Replace("\n", "")?.Trim();
            var subString = originalstr.Split(Convert.ToChar(separator))[index];
            return subString;
        }

    }
}
