using LinqToExcel.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;



namespace DhruvEnterprises.Dto
{
    public class WalletDto
    {
        public WalletDto() 
        {
            this.StatusList = new List<StatusTypeDto>();
            this.TrTypeList = new List<TransferTypeDto>();
            this.BankAccountList = new List<BankAccountDto>();
        }
        [DisplayName("User")]
        public int userid { get; set; }
        public List<WalletUser> UserList;
        [DisplayName("Current Balance")]
        public decimal CurrentBalance { get; set; }
        [DisplayName("Amount")]
        public decimal AddAmount { get; set; } 
        public string Datetime { get; set; }
        public string Remark { get; set; }
        public bool IsPullOut { get; set; }

        [DisplayName("Transfer Type")]
        public int TrTypeId { get; set; }
        [DisplayName("Bank Account")]
        public int BankAccountId { get; set; }
        [DisplayName("Cheque/Ref Number")]
        public string ChequeNo { get; set; }
        public string PaymentDate { get; set; }
        [DisplayName("Clear Credit?")]
        public bool IsClearCredit { get; set; }

        public List<StatusTypeDto> StatusList { get; set; }
        public List<TransferTypeDto> TrTypeList { get; set; }
        public List<BankAccountDto> BankAccountList { get; set; }
    }

    public class WalletUser
    {
        public int Userid { get; set; }
        public string UserName { get; set; }
        public string MobileNo { get; set; }
    }
    
    public class ExcelUpload
    {
        public string Date { get; set; }
        public int BankAccountId { get; set; }
        public HttpPostedFileBase File { get; set; }
    }
    public class CosmosBankDto 
    {
        [ExcelColumn("Date")]
        public string Date { get; set; }

        [ExcelColumn("Transaction Particulars")] 
        public string TransactionParticulars { get; set; }

        [ExcelColumn("Cheque No")]
        public string ChequeNo { get; set; }
        [ExcelColumn("Withdrawal")]
        public string Withdrawal { get; set; }

        [ExcelColumn("Deposit")]
        public string Deposit { get; set; }

        [ExcelColumn("Available Balance")]
        public string AvailableBalance { get; set; }
        public string Status { get; set; }
        public string Username { get; set; }


    }
    public class AuBankDto 
    {

        [ExcelColumn("Trans Date")]
        public string Date { get; set; }

        [ExcelColumn("Value Date")]
        public string ValueDate { get; set; }

        [ExcelColumn("Description/Narration")]
        public string Description { get; set; }

        [ExcelColumn("Chq./Ref.No.")]
        public string ChequeNo { get; set; }

        [ExcelColumn("Debit(Dr.) INR")]
        public string Debit { get; set; }

        [ExcelColumn("Credit(Cr.) INR")]
        public string Credit { get; set; }

        [ExcelColumn("Balance INR")]
        public string Balance { get; set; }
        [ExcelColumn("Status")]
        public string Status { get; set; }
        public string Username { get; set; }

    }


    public class HdfcBankDto
    {
        [ExcelColumn("Date")]
        public string Date { get; set; }

        [ExcelColumn("Narration")]
        public string Narration { get; set; }

        [ExcelColumn("Chq./Ref.No.")]
        public string ChqRefNo { get; set; }

        [ExcelColumn("Value Dt")]
        public string ValueDt { get; set; }

        [ExcelColumn("Withdrawal Amt.")]
        public string WithdrawalAmt { get; set; }

        [ExcelColumn("Deposit Amt.")]
        public string DepositAmt { get; set; }

        [ExcelColumn("Closing Balance")]
        public string ClosingBalance { get; set; }
        [ExcelColumn("Status")]
        public string Status { get; set; }
        public string Username { get; set; }
    }

    public class WalletRequestFilterDto
    {
        public int Isa { get; set; }
        [DisplayName("Cheque/Ref No.")]
        public string ChequeNo { get; set; }

        public string Sdate { get; set; }
        public string Edate { get; set; }

        public string SdateNow { get; set; }
        public string EdateNow { get; set; }

        public int UserId { get; set; }
        public int UpdatedById { get; set; }
        public int AccountId { get; set; }

        public int TrTypeId { get; set; }

        public string Remark { get; set; }
        public string Comment { get; set; }

        public int StatusId { get; set; }
        public int IsActive { get; set; }

    }
}
