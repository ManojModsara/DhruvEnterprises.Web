using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;

namespace DhruvEnterprises.API.LIBS
{
    public static class Enums
    {
        public enum RcStatus
        {

            SUCCESS = 1,
            PROCESSING = 2,
            FAILED = 3,
            HOLD = 4,
            PENDING = 5
        }

        public enum RCType
        {
            Normal = 1,
            Special = 2,
            Roffer = 3,
            Other = 4
        }

        public enum UserRole
        {
            [Description("Super Admin")]
            SuperAdmin = 1,
            [Description("Admin")]
            Admin = 2,
            [Description("Api User")]
            ApiUser = 3,
            [Description("Lapu User")]
            LapuUser = 4,
            [Description("Accounts User")]
            AccountUser = 5
        }

        public enum CommType
        {
            Flat = 1,
            Percent = 2,
            Range = 3
        }

        public enum AmtType
        {
            Credit = 1,
            Debit = 2,
            Refund = 3,
            Surcharge = 4,
            Discount = 5,
            Received = 6,
            Sent = 7,
            In = 8,
            Out = 9
        }

        public enum TxnType
        {
            Recharge = 1,
            BillPayment = 2,
            MoneyTransfer = 3,
            Wallet = 4,
            Cash = 5,
            IMPS = 6
        }

        public enum ApiType
        {
            LapuApi = 1,
            WebApi = 2,
            Other = 3
        }

        public enum MediumType
        {
            App = 1,
            Web = 2,
            Other = 3
        }

        public enum ApiUrlType
        {
            RechargeRequest = 1,
            BalanceCheck = 2,
            StatusCheck = 3,
            CallBack = 4
        }

        public enum Tag
        {
            StatusSUCCESS=1,
            StatusPROCESSING=2,
            StatusPENDING=3,
            StatusFAILED=4,
            ApiTxnID=5,
            OperatorTxnID=6,
            Message=7


        }

        public enum FilterType
        {
            ROffer = 1,
            NonROffer = 2,
            ROfferAndNonROffer = 3,
            Range = 4 
        }
    }
}