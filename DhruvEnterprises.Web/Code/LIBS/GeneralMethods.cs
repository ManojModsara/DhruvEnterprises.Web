using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace DhruvEnterprises.Web.Code.LIBS
{
    public static class GeneralMethods
    {
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
            catch (Exception ex)
            {

                //Handle Exceptions  
            }
            return VisitorsIPAddress;
        }

        public static string GetUniqueNumber(int length=20)
        { 
            var rndDigits = new System.Text.StringBuilder().Insert(0, "0123456789", length).ToString().ToCharArray();
            return string.Join("", rndDigits.OrderBy(o => Guid.NewGuid()).Take(length));
        }
        
    }
    

    public class ExportExcelColumn
    {
        public string ColumnName { get; set; }
        public string PropertyName { get; set; }
        public int ColumnWidth { get; set; }

    }
}