using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DhruvEnterprises.API.LIBS
{
    public static class SiteKey
    {
        public static string DomainName
        {
            get { return ConfigurationManager.AppSettings["DomainName"]; }
        }
        public static string SqlConn
        {
            get { return ConfigurationManager.ConnectionStrings["sqlconn"].ConnectionString; }
        }
        public static string UpiClientKey
        {
            get { return ConfigurationManager.AppSettings["UpiClientkey"]; }
        } 
        public static string AgentId
        {
            get { return ConfigurationManager.AppSettings["AgentId"]; }
        }

        public static string UpiClientsecret
        {
            get { return ConfigurationManager.AppSettings["UpiClientsecret"]; }
        }
        public static string Payoutkey
        {
            get { return ConfigurationManager.AppSettings["Payoutkey"]; }
        }
        public static string payoutSalt
        {
            get { return ConfigurationManager.AppSettings["payoutSalt"]; }
        }
        public static string DomainIPAddress
        {
            get { return ConfigurationManager.AppSettings["DomainIPAddress"]; }
        }
        public static string SMSDBConn
        {
            get { return ConfigurationManager.ConnectionStrings["smsdbconn"].ConnectionString; }
        }
        public static string SMSDBWalletUserId
        {
            get { return ConfigurationManager.AppSettings["SMSDBWalletUserId"]; }
        }
        public static string AgentAuthId
        {
            get { return ConfigurationManager.AppSettings["AgentAuthId"]; }
        }
        public static string AgentAuthPassword
        {
            get { return ConfigurationManager.AppSettings["AgentAuthPassword"]; }
        }

    }
}