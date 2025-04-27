using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DhruvEnterprises.API.LIBS
{
    public static class ErrorCode 
    {
        //Status messages
        public static string VALID_REQUEST = "0";
        public static string NO_ERROR = "0";
        public static string AUTH_FAIL = "1";
        public static string INVALID_REQUEST = "2";
        public static string INVALID_MOBILE = "3";
        public static string INVALID_AMOUNT = "4";
        public static string INVALID_OPERATOR = "5";
        public static string UNKNOWN_ERROR = "6";
        public static string USER_NOT_EXIST = "7";
        public static string INVALID_ROLE = "8";
        public static string LOW_USER_BALANCE = "9";
        public static string LOW_API_BALANCE = "10";
        public static string OPERATOR_NOT_EXIST = "11";
        public static string OPERATOR_MISMATCH = "12";
        public static string API_NULLRESP = "13";
        public static string APIURL_NOT_SET = "14";
        public static string APIREQURL_NOT_SET = "15";
        public static string DUPLICATE_REQUEST = "16";
        public static string USER_INACTIVE = "17";
        public static string INVALID_IPADDRESS = "18";
        public static string INVALID_REQTXNID = "19";
        public static string INVALID_DATA = "20";
        public static string FREQUEST_REQUEST = "21";
        public static string INVALID_REQUEST_ID = "22";
        public static string LAPU_NOT_SET = "23";
        public static string LAPU_LOW_BA = "24";
        public static string ROUTE_NOT_FOUND = "25";
        public static string NO_RECORD_FOUND = "26";
        public static string DUPLICATE_RESEND_REQUEST = "27";
        public static string OPERATOR_LIMIT_EXCEEDED = "28";
        public static string CIRCLE_LIMIT_EXCEEDED = "29";
        public static string REQUEST_GAP = "30";
        public static string ROFFER_NOT_AVAILABLE = "31";
        public static string INVALID_COMMISSION_AMT = "32";//when user comm>api comm
        public static string PROCESSING_QUEUE_SIZE_FULL = "33";
        public static string INVALID_ACCOUNTNO = "34";
        public static string INVALID_IFSCCODE = "35";
        public static string INVALID_TRANSFERMODE = "36";
        public static string INVALID_BENEFICIERNAME = "37";
        public static string DEFAULT_OFFLINE = "100";
        public static string DEFAULT_FAILED = "101";
        public static string DEFAULT_PROCESSING = "102";
        public static string ROUTE_BLOCKED = "103";
        public static string CALLBACK_RECHARGE_THREAD = "104";
        public static string BILL_FETCH_ALREADY_AVAILABLE = "105";
        public static string INVALID_ADHAAR = "106";
        public static string INVALID_RETID = "107";
        public static string INVALID_PAN = "108";
        public static string INVALID_LATITUDE = "109";
        public static string INVALID_LONGITUDE = "110";
        public static string INVALID_MERCHANTLOGIN = "111";
        public static string BANK_NOT_SET = "112";
        public static string INVALID_firstname = "113";
        public static string INVALID_lastname = "114";
        public static string INVALID_TXNTYPE = "115";
        public static string VENDOR_NOT_ACTIVE = "116";
        public static string INVALID_TOKEN = "117";
        public static string BCID_Id = "118";
        public static string INVALID_BANKCODE = "120";
      


    }

    public static class StatusMsg 
    {
        //Status messages

        public static string AUTH_FAIL = "Authentication Failed!";
        public static string INVALID_REQUEST = "Invalid Request!";
        public static string VALID_REQUEST = "Request is processed!";
        public static string INVALID_MOBILE = "Invalid Mobile Number!";
        public static string INVALID_AMOUNT = "Invalid Amount! Amount must be between 10 and 25000";
        public static string INVALID_NAME = "The name must be between 3 and 120 characters.";
        public static string INVALID_OPERATOR = "Invalid Operator!";
        public static string UNKNOWN_ERROR = "Internal Server Error!";
        public static string LOW_USER_BALANCE = "Insufficient Balance";
        public static string INVALID_APIURL = "Invalid Api Url Settings!";
        public static string DUPLICATE_REQUEST = "Duplicate Request!";
        public static string USER_INACTIVE = "User Not Active!";
        public static string INVALID_IPADDRESS = "Invalid Request From IP";

        public static string RECHARGE_FAILED = "Request is failed!";
        public static string RECHARGE_SUCCESS = "Request is successfull!";
        public static string RECHARGE_PROCESSING = "Request is processing!";
        public static string INVALID_REQTXNID = "Invalid Request Id";
        public static string INVALID_DATA = "Invalid Data";
        public static string FREQUEST_REQUEST = "Frequest Request. Retry After Some time.";
        public static string DUPLICATE_RESEND_REQUEST = "Duplicate Resend Request.";
        public static string INVALID_REQUEST_ID = "Invalid Request-Id(max 65 digits allowed)";
        //for code=10,14,23,24,25,27,28 
        public static string ERROR_CONTACT_ADMIN = "Error, Contact to Administrator";
        public static string NO_RECORD_FOUND = "No record found";
        public static string REQUEST_GAP = "Request is processed!";
        public static string ROFFER_NOT_AVAILABLE = "Error, Contact to Administrator";
        public static string INVALID_COMMISSION_AMT = "Error, Contact to Administrator";//when user comm>api comm
        public static string PROCESSING_QUEUE_SIZE_FULL = "Error, Contact to Administrator";//when no of hold+processing is greater than maxQsize
        public static string CALLBACK_RECHARGE_THREAD = "Request Accepted";
        public static string DEFAULT_OFFLINE = "Operator Service Down.";
        public static string DEFAULT_FAILED = "Request failed";
        public static string DEFAULT_PROCESSING = "Request Processing";
        public static string ROUTE_BLOCKED = "Service Down.";
        public static string BILL_FETCH_ALREADY_AVAILABLE = "Request is successfull!";

        public static string INVALID_ADHAARNUMBER = "Adhaar number is not valid!";
        public static string INVALID_BANK = "Invalid BANK!";
        public static string INVALID_lastname = "Invalid lastname!";
        public static string INVALID_firstname = "Invalid firstname!";
        public static string INVALID_TXNTYPE= "Invalid TXN MODE!";
        public static string INVALID_TOKEN = "Invalid Token Id.";
        public static string BCID_Id = "Invalid recipient Id.";
        public static string INVALID_BANKCODE = "Bank code Not Found For This IFSC";


    }

    public static class StatsCode
    {
        public static int SUCCESS = 1;
        public static int PROCESSING = 2;
        public static int FAILED = 3;
        public static int HOLD = 4;
        public static int QUEUED = 2;
    }

    public static class StatusName
    {
        public static string SUCCESS = "SUCCESS";
        public static string PROCESSING = "processing";
        public static string FAILED = "FAILED";
        public static string HOLD = "HOLD";
        public static string QUEUED = "QUEUED";
        public static string PROCESSED = "processed";
        public static string SUCCEED = "succeed";
        public static string FAILURE = "failure";
    }

    public static class TAGName
    {
        public static int SUCCESS = 1;
        public static int PROCESSING = 2;
        public static int PENDING = 3;
        public static int FAILED = 4;
        public static int APITXNID = 5;
        public static int OPERATORTXNID = 6;
        public static int MESSAGE = 7;
        public static int REQUESTTXNID = 8;
        public static int VENDOR_CL_BAL = 9;
        public static int VENDOR_OP_BAL = 10;
        public static int LAPUNO = 11;
        public static int COMPLAINT_ID = 12;
        public static int R_OFFER = 13;
        public static int CUSTOMER_NUMBER = 14;
        public static int CUSTOMER_NAME = 15;
        public static int BILL_PERIOD = 16;
        public static int BILL_DATE = 17;
        public static int BILL_DUE_DATE = 18;
        public static int BILL_PRICE = 19;
        public static int BILL_NUMBER = 20;

    }

    public static class RouteType
    {
        public static int COMMON_ROUTE = 1;
        public static int OPERATOR_WISE = 2;
        public static int CIRCLE_WISE = 3;
        public static int AMOUNT_WISE = 4;

    }
    public static class AEPSOperators
    {
        public static string BalanceEnquiry = "96";
        public static string CashWithdrawal = "97";
        public static string AdhaarPay = "98";
        public static string CashDeposit = "99";
        public static string MoneyTransfer = "100";
        public static string VerifyMoneyTransferAccount = "101";
        public static string MoneySettlement = "102";
        public static string MerchantOnboarding = "103";
        public static string EKYCAEPS = "105";
        public static string AEPSMiniStatement = "106";
        public static string MATCashDeposit = "110";
        //public static string StatusCheck = "102";
        //public static string Dmt = "103";

    }

    public static class AEPSAuthentication
    {
        public static string merchantUserName = "EZYTMm";
        public static string merchantPin = "1234m";
        public static string merchantDistUserName = "EZYTMd";
        public static string merchantDistPin = "1234d";
        public static string subMerchantId = "EZYTMm";
        public static string superMerchantId = "982";
        public const string secKey = "c7b4528781829f9e013be6273f9ee564ee2916b04bbfcd9e90ada1aaa62f6321";
        public const string languageCode = "en";
        public const string paymentType = "B";
        public static string superMerchantLoginId = "EZYTMm";

    }

   
    public struct response
    {
        public string Txnrply;
        public string Statusrply;
        public string OrignlTxnidrply;
        public string Error1;
        public string orderId;
    }
    public class callbackstatus
    {
        public string status { get; set; }
        public string statusCode { get; set; }
        public string statusMessage { get; set; }
        public callbackdata result { get; set; }
    }
    public class callbackdata
    {
        public string mid { get; set; }
        public string orderId { get; set; }
        public string paytmOrderId { get; set; }
        public string amount { get; set; }
        public string commissionAmount { get; set; }
        public string taxAmount { get; set; }
        public string rrn { get; set; }

    }
    public static class AEPSTransactionType
    {
        public static string CashWithdrawal = "CW";
        public static string BalanceEnquiry = "BE";
        public static string Adhaarpay = "M";
        public static string CashDeposit = "CD";
        public static string CashDepositOTP = "CDO";
        public static string EKYC = "EKY";
        public static string MiniStatement = "MS";
    }
    public static class IndicatorforUID
    {
        public static int adhaarPayment = 0;
        public static int virtualid = 2;
    }
}