

using System.ComponentModel;

namespace DhruvEnterprises.Dto
{
    public class UserDayBookDto
    {
        [DisplayName("UserId")]
        public int UserId { get; set; }

        [DisplayName("User Name")]
        public string UserName { get; set; }

        [DisplayName("Date")]
        public string TxnDate { get; set; }

        [DisplayName("RC_Amt")]
        public string RC_Amt { get; set; }

        [DisplayName("OP_Bal")]
        public string OP_Bal { get; set; }

        [DisplayName("WR_Amt")]
        public string WR_Amt { get; set; }

        [DisplayName("CL_Bal")]
        public string CL_Bal { get; set; }

        [DisplayName("DB_Amt")]
        public string DB_Amt { get; set; }

        [DisplayName("CM_Amt")]
        public string CM_Amt { get; set; }

        [DisplayName("Calc_Bal")]
        public string Calc_Bal { get; set; }

        [DisplayName("CL_Diff [Calc-CL]")]
        public string CL_Diff { get; set; }

        [DisplayName("Old_DB")]
        public string Old_DB_Amt { get; set; }

        [DisplayName("Old_CR")]
        public string Old_CR_Amt { get; set; }

        [DisplayName("Old_sAmt")]
        public string Old_sRc_Amt { get; set; }

        [DisplayName("Old_fAmt")]
        public string Old_fRc_Amt { get; set; }
        public string Total_Surcharge { get; set; }
        public string Total_Discount { get; set; }

    }

    public class ApiDayBookDto
    {
        [DisplayName("VendorId")]
        public int ApiId { get; set; }

        [DisplayName("Vendor Name")]
        public string ApiName { get; set; }

        [DisplayName("Date")]
        public string TxnDate { get; set; }

        [DisplayName("RC_Amt")]
        public string RC_Amt { get; set; }

        [DisplayName("OP_Bal")]
        public string OP_Bal { get; set; }

        [DisplayName("WR_Amt")]
        public string WR_Amt { get; set; }
          
        [DisplayName("CL_Bal")]
        public string CL_Bal { get; set; }

        [DisplayName("DB_Amt")]
        public string DB_Amt { get; set; }

        [DisplayName("CM_Amt")]
        public string CM_Amt { get; set; }

        [DisplayName("Ins_Amt")]
        public string Ins_Amt { get; set; } 

        [DisplayName("Calc_Bal")]
        public string Calc_Bal { get; set; }

        [DisplayName("CL_Diff [Calc-CL]")]
        public string CL_Diff { get; set; }


    }

    public class LapuDayBookDto
    {
        [DisplayName("LapuId")]
        public int LapuId { get; set; }
         
        [DisplayName("Lapu Number")]
        public string LapuNumber { get; set; }

        [DisplayName("Date")]
        public string TxnDate { get; set; }

        [DisplayName("RC_Amt")]
        public string RC_Amt { get; set; }

        [DisplayName("OP_Bal")]
        public string OP_Bal { get; set; }

        [DisplayName("WR_Amt")]
        public string WR_Amt { get; set; }

        [DisplayName("CL_Bal")]
        public string CL_Bal { get; set; }

        [DisplayName("DB_Amt")]
        public string DB_Amt { get; set; }

        [DisplayName("CM_Amt")]
        public string CM_Amt { get; set; }

        [DisplayName("Calc_Bal")]
        public string Calc_Bal { get; set; }
        
        [DisplayName("Ins_Amt")]
        public string Ins_Amt { get; set; }

        [DisplayName("RO_Amt")]
        public string RO_Amt { get; set; }
         
        [DisplayName("CL_Diff [Calc-CL]")]
        public string CL_Diff { get; set; }


    }
 
    //foroperator wise daily report
    public class dailydto
    { 
        public int ID { get; set; }
        public string CircleName { get; set; }

        public string Name { get; set; }
        [DisplayName("Date")]
        public string DateTime { get; set; }
        [DisplayName("Success")]
        public string SuccessAmount { get; set; }
        [DisplayName("sCount")]
        public string SuccessCount { get; set; }
        [DisplayName("Processing")]
        public string ProcessingAmount { get; set; }
        [DisplayName("pCount")]
        public string ProcessingCount { get; set; }
        [DisplayName("Failed")]
        public string FailedAmount { get; set; }
        [DisplayName("fCount")]
        public string FailedCount { get; set; }
        [DisplayName("Hold")]
        public string HoldAmount { get; set; }
        [DisplayName("hCount")]
        public string HoldCount { get; set; }
        [DisplayName("Roffer")]
        public string RofferAmount { get; set; }
        [DisplayName("RCount")]
        public string RofferCount { get; set; }
        [DisplayName("Op_Bal")]
        public string OpeningBal { get; set; }
        [DisplayName("CL_Bal")]
        public string Closingbal { get; set; }
        [DisplayName("Wallet CR_Amt")]
        public string WalletBalance { get; set; }
        [DisplayName("Calc CL_Amt")]
        public string TotalCheckBalance { get; set; }
        [DisplayName("Diff CL_Amt")]
        public string DiffBalance { get; set; }
        [DisplayName("DR_Amt")]
        public string Debitamt { get; set; }
        public string Roffer { get; set; }

    }

    public class ApiWiseProfitDto
    {
        public string ApiId { get; set; }
        public string ApiName { get; set; }
        public string RC_Amt { get; set; }
        public string User_DB_Amt { get; set; }
        public string Api_DB_Amt { get; set; }
        public string Profit { get; set; }
    }

    public class OpWiseProfitDto
    {
        public string OpId { get; set; }
        public string OpName { get; set; }
        public string RC_Amt { get; set; }
        public string User_DB_Amt { get; set; }
        public string Api_DB_Amt { get; set; }
        public string Profit { get; set; }
        public string ROffer { get; set; } 
    }

    public class ProfitAllDto
    { 
        public string TypeName { get; set; }
        public string RC_Amt { get; set; }
        public string DB_Amt { get; set; }
        public string CM_Amt { get; set; }
        public string Ins_Amt { get; set; }
        public string RO_Amt { get; set; }
        public string ProfitTotal { get; set; } 
    }

    public class BalanceAllDto
    { 
        public string Banks_Bal { get; set; } 
        public string Apis_Bal { get; set; } 
        public string Dealers_Bal { get; set; } 
        public string Lapus_Bal { get; set; }
        public string Users_Bal { get; set; }
        public string Apis_Dues { get; set; }
        public string Banks_Credits { get; set; }
        public string Total { get; set; } 
    }

    public class DealerDayBookDto
    {
        [DisplayName("DealerId")]
        public int UserId { get; set; }

        [DisplayName("Dealer Name")]
        public string DealerName { get; set; }

        [DisplayName("Date")]
        public string TxnDate { get; set; }

        [DisplayName("Opening")]
        public string OP_Bal { get; set; }

        [DisplayName("Closing")]
        public string CL_Bal { get; set; }

        [DisplayName("SentAmount")]
        public string CR_Amt { get; set; }

        [DisplayName("Commission")]
        public string CM_Amt { get; set; }

        [DisplayName("Stock Received")]
        public string DB_Amt { get; set; }
        
        [DisplayName("Commission Received")]
        public string CM_DB_Amt { get; set; }

        [DisplayName("Amt_Diff")]
        public string Amt_Diff { get; set; }

        [DisplayName("R-Offer")]
        public string RO_Amt { get; set; }

        [DisplayName("Calc_CL")]
        public string Calc_CL { get; set; } 
    }
}
