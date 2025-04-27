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
    public class WalletRequestDto
    {
        public WalletRequestDto()
        {
            this.StatusList = new List<StatusTypeDto>();
            this.TrTypeList = new List<TransferTypeDto>();
            this.BankAccountList = new List<BankAccountDto>();
        }

        public long Id { get; set; }
        public int UserId { get; set; }
        public decimal? Amount { get; set; }
        public byte TxnTypeId { get; set; }
        public byte AmtTypeId { get; set; }
        public int StatusId { get; set; }
        public string TxnId { get; set; }
        public string ImagePath { get; set; }
        [DisplayName("Bank Detail")]
        public string Bankname { get; set; }
        [DisplayName("Cheque/Ref No")]
        public string Chequeno { get; set; }
        [DisplayName("Remark")]
        public string PaymentRemark { get; set; }
        public string Comment { get; set; }
        public DateTime AddedDate { get; set; }
        public int AddedById { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UpdatedById { get; set; }
        [DisplayName("Transfer Type")]
        public int TrTypeId { get; set; }
        [DisplayName("Bank Account")]
        public int BankAccountId { get; set; }
        [DisplayName("Clear Credit?")]
        public bool IsClearCredit { get; set; }
        [DisplayName("Payment Date")]
        public string PaymentDate { get; set; }
        public bool IsClearCheck { get; set; }
        public string ImageUrl { get; set; }
        public HttpPostedFileBase FileAttach { get; set; }


        public List<StatusTypeDto> StatusList { get; set; }
        public List<TransferTypeDto> TrTypeList { get; set; }
        public List<BankAccountDto> BankAccountList { get; set; } 

    }


    public class MoneyTransferDto
    {
        public string OrderID { get; set; }
        public string Account { get; set; }
        public string IFSC { get; set; }
        public string PinCheck { get; set; }
        public string Amount { get; set; }
        public string Mode { get; set; }
        public string Name { get; set; }
        public string ApiToken { get; set; }
        public string Mobileno { get; set; }
        public string Pin { get; set; }
        public string BankName { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string OpId { get; set; }
        public int BankID { get; set; }
        public int settlementId { get; set; }
        public string AgentId     { get; set; }
        public string BeneficiaryId { get; set; }

    }
    public class ExcelDataDto
    {
        public string Mobileno { get; set; }
        public string MODE { get; set; }
        public decimal Amount { get; set; }
        public string Name { get; set; }

        public string Account  { get; set; }
        public string IFSC { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }

    }

        public class PayoutDto
    {
        public string BCID { get; set; }
        public long DMTId { get; set; }
        public string UserID { get; set; }
        public string MODE { get; set; }
        public string Mobileno { get; set; }
        public string IP { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public string Pin { get; set; }
        public string Operator { get; set; }
        public decimal Amount { get; set; }
        public string BankName { get; set; }
        [Required(ErrorMessage = "Account No")]

        public string AccountNo { get; set; }

        public string BeneMobileNo { get; set; }
        public string Name { get; set; }
        [Required(ErrorMessage = "IFSC Code")]
        public string IFSC { get; set; }
        public string AgentID { get; set; }
        public int ApiID1 { get; set; }
        public int BankID { get; set; }
        public decimal CommAmount { get; set; }
    }

    public class PayoutResponsedto
    {
        public int Error { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }

    }

}
