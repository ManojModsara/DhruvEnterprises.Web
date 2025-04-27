using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DhruvEnterprises.API
{
    public class AppStatusMsg
    {
        public static class StatusMsg
        {
            public static string UnExpected_MSG = "Unexpected error";
            public static string AUTH_MSG = "Authentication error!";
            public static string User_Register = "Registration Successfull";
            public static string Duplicate = "Already Mobile No or Email ID Exists !!";
            public static string UpdateApp = "Update App !!!";
            public static string Add_Register = "Successfull";
            public static string OTPWrong = "OTP Wrong";

        }

        public static class ErrorCode
        {
            public static string AUTH_SUCCESS = "0";
            public static string AUTH_Valid = "1";
            public static string AUTH_FAIL = "5";
            public static string AUTH_NotValid = "9";
            public static string AUTH_Parameter = "11";
        }
        public static class AppStatusCode
        {
            public static string SUCCESS = "0";
            public static string Valid = "1";
            public static string FAILED = "3";
        }
    }
}