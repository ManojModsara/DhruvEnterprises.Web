using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.IO;
namespace DhruvEnterprises.Web.LIBS
{
    public static class SiteKey
    {
        public static string DomainName
        {
            get { return ConfigurationManager.AppSettings["DomainName"]; }
        }
        public static string ApiDomainName
        {
            get { return ConfigurationManager.AppSettings["ApiDomainName"]; }
        }
        public static string SqlConn
        {
            get { return ConfigurationManager.ConnectionStrings["sqlconn"].ConnectionString; }
        }
        public static string CyberPlatCert
        {
            get { return ConfigurationManager.AppSettings["CyberPlatCert"]; }
        }
        public static string CyberPlatPass
        {
            get { return ConfigurationManager.AppSettings["CyberPlatPass"]; }
        }
        public static string From { get { return ConfigurationManager.AppSettings["From"]; } }
        public static string To { get { return ConfigurationManager.AppSettings["To"]; } }
        public static string CC { get { return ConfigurationManager.AppSettings["CC"]; } }
        public static string BCC { get { return ConfigurationManager.AppSettings["BCC"]; } }
        public static string VACC { get { return ConfigurationManager.AppSettings["VirtualAccount"]; } }

        
        public static string Host
        {
            get { return ConfigurationManager.AppSettings["Host"]; }
        }
        
        public static string Port
        {
            get { return ConfigurationManager.AppSettings["Port"]; }
        }

        public enum MessageType
        {
            Info,
            Warning,
            Error,
            Success
        }

        public static string CompanyFullName
        {
            get { return ConfigurationManager.AppSettings["CompanyFullName"]; }
        }

        public static string CompanyShortName
        {
            get { return ConfigurationManager.AppSettings["CompanyShortName"]; }
        }

        public static string CompanyContact
        {
            get { return ConfigurationManager.AppSettings["CompanyContact"]; }
        }

        public static string CompanyEmail
        {
            get { return ConfigurationManager.AppSettings["CompanyEmail"]; }
        }

        public static string CompanyCity
        {
            get { return ConfigurationManager.AppSettings["CompanyCity"]; }
        }

        public static string CompanyAddress
        {
            get { return ConfigurationManager.AppSettings["CompanyAddress"]; }
        }

        public static string SMSDBWalletUserId
        {
            get { return ConfigurationManager.AppSettings["SMSDBWalletUserId"]; }
        }

        public static string SMSDBConn
        {
            get { return ConfigurationManager.ConnectionStrings["smsdbconn"].ConnectionString; }
        }
        //smsdbconn


        public static string PlanApiID
        {
            get { return ConfigurationManager.AppSettings["PlanapiID"]; }
        }

        public static string PlanApiPassword
        {
            get { return ConfigurationManager.AppSettings["PlanapiPass"]; }
        }

    }
}