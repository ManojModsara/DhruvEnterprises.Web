using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Data;
using System.Reflection;
using System.IO;
using System.Text;
using System.Configuration;

namespace DhruvEnterprises.Web.LIBS
{
    public static class Extension
    {
        public static string Time(this double hours)
        {
            if (hours <= 0)
                return "-";

            var ts = TimeSpan.FromHours(hours);
            return String.Format("{0}:{1}", ts.Hours.ToString("00"), ts.Minutes.ToString("00"));
        }

        public static string TotalLeaveDays(this int LeaveDays)
        {
            if (LeaveDays == 0)
                return "-";
            else if (LeaveDays < 0)
                return "+ (" + Math.Abs(LeaveDays) + ")";
            return LeaveDays.ToString();
        }

        public static string TotalLeaveDays(this decimal LeaveDays)
        {
            if (LeaveDays == 0.0M)
                return "-";
            else if (LeaveDays < 0)
                return "+ (" + Math.Abs(LeaveDays) + ")";
            return LeaveDays.ToString();
        }

        public static string TrimLength(this string str, int length)
        {
            if (!String.IsNullOrEmpty(str))
                return str.Length <= length ? str : str.Substring(0, length) + "...";

            return String.Empty;
        }

        public static bool CompareDate(this DateTime? dateTime, DateTime? otherDate)
        {
            if (dateTime.HasValue && otherDate.HasValue)
            {
                return dateTime.Value.Day == otherDate.Value.Day
                       && dateTime.Value.Month == otherDate.Value.Month
                       && dateTime.Value.Year == otherDate.Value.Year;
            }

            return false;
        }

        public static string[] SelectedValues(this CheckBoxList CHK)
        {
            List<string> selectedValues = new List<string>();

            foreach (ListItem item in CHK.Items)
                if (item.Selected)
                    selectedValues.Add(item.Value);

            return selectedValues.ToArray();
        }

        public static void SelectedValues(this CheckBoxList CHK, string[] values)
        {
            foreach (ListItem item in CHK.Items)
                if (values.Contains(item.Value))
                    item.Selected = true;
        }

        public static bool ContainsAny(this string input, params string[] values)
        {
            return String.IsNullOrEmpty(input) ? false : values.Any(S => input.Contains(S));
        }

        public static void AddOrReplace(this Dictionary<string, object> DICT, string key, object value)
        {
            if (DICT.ContainsKey(key))
                DICT[key] = value;
            else
                DICT.Add(key, value);
        }

        public static dynamic GetObjectOrDefault(this Dictionary<string, object> DICT, string key)
        {
            if (DICT.ContainsKey(key))
                return DICT[key];
            else
                return null;
        }

        public static T GetObjectOrDefault<T>(this Dictionary<string, object> DICT, string key)
        {
            if (DICT.ContainsKey(key))
                return (T)Convert.ChangeType(DICT[key], typeof(T));
            else
                return default(T);
        }

        public static string ToTitle(this string input)
        {
            return String.IsNullOrEmpty(input) ? String.Empty : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLower());
        }

        public static void ExportToExcel(this System.Data.DataTable Tbl, string ExcelFilePath = null)
        {
            try
            {
                if (Tbl == null || Tbl.Columns.Count == 0)
                    throw new Exception("ExportToExcel: Null or empty input table!\n");

                // load excel, and create a new workbook
                Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
                excelApp.Workbooks.Add();

                // single worksheet
                Microsoft.Office.Interop.Excel._Worksheet workSheet = excelApp.ActiveSheet;

                // column headings
                for (int i = 0; i < Tbl.Columns.Count; i++)
                {
                    workSheet.Cells[1, (i + 1)] = Tbl.Columns[i].ColumnName;
                }

                // rows
                for (int i = 0; i < Tbl.Rows.Count; i++)
                {
                    // to do: format datetime values before printing
                    for (int j = 0; j < Tbl.Columns.Count; j++)
                    {
                        workSheet.Cells[(i + 2), (j + 1)] = Tbl.Rows[i][j];
                    }
                }

                // check fielpath
                if (ExcelFilePath != null && ExcelFilePath != "")
                {
                    try
                    {
                        workSheet.SaveAs(ExcelFilePath);
                        excelApp.Quit();

                    }
                    catch (Exception ex)
                    {
                        throw new Exception("ExportToExcel: Excel file could not be saved! Check filepath.\n"
                            + ex.Message);
                    }
                }
                else    // no filepath is given
                {
                    excelApp.Visible = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ExportToExcel: \n" + ex.Message);
            }
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

        public static string ToPlainExcelText(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            else
            {
                str = str.Replace("<", " ")
                         .Replace(">", " ")
                         .Replace(",", " ")
                         .Replace("\n", " ")
                         .Replace("\r", " ")
                         .Replace("\t", " ");

                str = "'" + str + "'";
            }

            return str;
        }

        public static string ToPlainExcelText(this int? num)
        {
            string str = num.HasValue ? Convert.ToString(num) : string.Empty;

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            else
            {
                str = "'" + str + "'";
            }

            return str;
        }

        public static string ToPlainExcelText(this long? num)
        {
            string str = num.HasValue ? Convert.ToString(num) : string.Empty;

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            else
            {
                str = "'" + str + "'";
            }

            return str;
        }

        public static string ToPlainExcelText(this decimal? num)
        {
            string str = num.HasValue ? Convert.ToString(num) : string.Empty;

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            else
            {
                str = "'" + str + "'";
            }

            return str;
        }

        public static string ToPlainExcelText(this int num)
        {
            string str = Convert.ToString(num);

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            else
            {
                str = "'" + str + "'";
            }

            return str;
        }

        public static string ToPlainExcelText(this long num)
        {
            string str = Convert.ToString(num);

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            else
            {
                str = "'" + str + "'";
            }

            return str;
        }

        public static string ToPlainExcelText(this decimal num)
        {
            string str = Convert.ToString(num);

            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            else
            {
                str = "'" + str + "'";
            }

            return str;
        }

        public static DataTable ToDataTable<T>(this List<T> items, string ignoreCols = "Comma Separated Column Names")
        {

            var ignoreList = ignoreCols.Split(',').ToList();

            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            Props = Props.Where(x => !ignoreList.Any(i => i.ToLower() == x.Name.ToLower())).ToArray();

            foreach (PropertyInfo prop in Props)
            {  //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);

            }
            foreach (T item in items)
            {

                var values = new object[Props.Length];

                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);

                }

                dataTable.Rows.Add(values);

            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public static bool SaveDataTableToExcel(this DataTable table, string savePath)
        {
            //open file
            StreamWriter wr = new StreamWriter(savePath, false, Encoding.Unicode);

            try
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    wr.Write(table.Columns[i].ToString().ToUpper() + "\t");
                }

                wr.WriteLine();

                //write rows to excel file
                for (int i = 0; i < (table.Rows.Count); i++)
                {
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (table.Rows[i][j] != null)
                        {
                            wr.Write(Convert.ToString(table.Rows[i][j]) + "\t");
                        }
                        else
                        {
                            wr.Write("\t");
                        }
                    }
                    //go to next line
                    wr.WriteLine();
                }
                //close file
                wr.Close();
            }
            catch (Exception ex)
            {
                Common.LogException(ex);

                return false;
            }

            return true;
        }

       
    }
}