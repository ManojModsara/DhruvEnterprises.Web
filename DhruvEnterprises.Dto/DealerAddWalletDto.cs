using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DhruvEnterprises.Dto
{
    public class DealerAddWalletDto
    {

        public int TxnId { get; set; }
        [DisplayName("Dealer Id")]
        public int DealerId { get; set; }
        [DisplayName("Pullout?")]
        public bool IsPullout { get; set; }
        [DisplayName("Balance")]
        public decimal CurrentBalance { get; set; }
        [DisplayName("Sent(INR)")]
        public decimal SentAmount { get; set; }
        [DisplayName("Margin(INR)")]
        public decimal CommissionAmount { get; set; }
        public string Remark { get; set; }
        [DisplayName("Cheque/Ref No")]
        public string ChequeNo { get; set; }
        [DisplayName("Account")]
        public int AccountId { get; set; }
        [DisplayName("Operator")]
        public int OperatorId { get; set; }
        [DisplayName("Mode")]
        public int TrTypeId { get; set; }
        public string PaymentDate { get; set; }
        public List<TransferTypeDto> TrTypeList { get; set; } = new List<TransferTypeDto>();
        public List<BankAccountDto> BankAccountList { get; set; } = new List<BankAccountDto>();
        public List<OperatorDto> OperatorList { get; set; } = new List<OperatorDto>();

    }

    public class LapuAddWalletDto
    {
        public int TxnId { get; set; }
        [DisplayName("Lapu")]
        public long LapuId { get; set; }
        [DisplayName("Dealer")]
        public int DealerId { get; set; }
        [DisplayName("Pullout?")]
        public bool IsPullout { get; set; }
        [DisplayName("Balance")]
        public decimal CurrentBalance { get; set; }
        [DisplayName("Amount(INR)")]
        public decimal AmountReceived { get; set; }
        [DisplayName("Total Received(INR)")]
        public decimal TotalReceived { get; set; }
        [DisplayName("Commission(INR)")]
        public decimal Commission { get; set; }
        [DisplayName("ROffer(INR)")]
        public decimal ROfferAmount { get; set; }
        [DisplayName("Margin(%)")]
        public decimal CommRate { get; set; }
        public string Remark { get; set; }
         
        public string RecentTxnDate { get; set; }
        public string RecentUpdatedBy { get; set; }
        public string RecentAmount { get; set; }
        public string RecentLapu { get; set; }
        public string PurchaseDate { get; set; } 

    }

    public class LapuAddWalletExcelDto
    {
        [DisplayName("Dealer")]
        public int DealerId { get; set; }

        [DisplayName("Operator")]
        public int OpId { get; set; }

        [DisplayName("Purchase Date")]
        public string PurchaseDate { get; set; }


        [Required(ErrorMessage = "Select File")]
        [FileExt(Allow = ".xls,.xlsx", ErrorMessage = "Only excel file Allowed.")]
        public HttpPostedFileBase UploadedFile { get; set; }

        public string RecentTxnDate { get; set; }
        public string RecentUpdatedBy { get; set; }
        public string RecentAmount { get; set; }
        public string RecentLapu { get; set; }
        public bool IsPullOut { get; set; }

    }
}
