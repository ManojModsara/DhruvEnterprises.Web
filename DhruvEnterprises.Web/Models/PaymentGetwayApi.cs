using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DhruvEnterprises.Web.Models
{
    public static class PaymentGetwayApi
    {
        public static string MerchantId = "WcDvxs30012729366643";
        public static string MerchantKey = "dekoS4PlO62UJxmu";
        public static string Website = "WEBSTAGING";
        public static string IndustryType = "Retail";
        public static string ChannelId_WEB = "WEB";
        public static string ChannelId_App = "WAP";
        public static string NameTypedId = "1";
        public static string ApiUrl = "https://securegw-stage.paytm.in/order/process";
        public static string CallBackUrl = "https://DhruvEnterprises.in/ezytmpay/PaymentResponse";
        public static string Call_BackUrl = "https://DhruvEnterprises.in/ezytmpay/PaymentResponse";
    }
}