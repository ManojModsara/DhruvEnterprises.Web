using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Web;
namespace DhruvEnterprises.Dto
{
    public class BankStatementDto
    {
        public long Id { get; set; }

        [DisplayName("IsWithdraw?")]
        public bool IsWithdraw { get; set; }

        [DisplayName("Bank Account")]
        public int AccountId { get; set; }

        [DisplayName("Transfer Type")]
        public int TrTypeId { get; set; }

        public decimal Amount { get; set; }

        [DisplayName("Cheque/Ref Number")]
        public string PaymentRef { get; set; }

        [DisplayName("Payment Remark")]
        public string Remark { get; set; }

        [DisplayName("Current Balance")]
        public decimal CurrentBalance { get; set; }
        [DisplayName("Bank Account Ref")]
        public int? RefAccountId { get; set; }
        [DisplayName("User")]
        public int? UserId { get; set; }
        [DisplayName("Vendor")]
        public int? ApiId { get; set; }
        [DisplayName("Payment Date")]
        public string PaymentDate { get; set; }


        //accountlist
        //transfertypelist
        //userlist
        //apilist

    }
    public class BankStatementExcelDto
    {
        public long Id { get; set; }

        [DisplayName("IsWithdraw?")]
        public bool IsWithdraw { get; set; }

        [DisplayName("Bank Account")]
        public int AccountId { get; set; }


        [DisplayName("Transfer Type")]
        public int TrTypeId { get; set; }

        [DisplayName("Bank Account")]
        public string  AccountNo { get; set; }

        [DisplayName("Transfer Type")]
        public string  TrType { get; set; }

        public decimal Amount { get; set; }

        [DisplayName("Cheque/Ref Number")]
        public string PaymentRef { get; set; }

        [DisplayName("Payment Remark")]
        public string Remark { get; set; }

        [DisplayName("Current Balance")]
        public decimal CurrentBalance { get; set; }
        [DisplayName("Bank Account Ref")]
        public int? RefAccountId { get; set; }
        [DisplayName("User")]
        public int? UserId { get; set; }
        [DisplayName("Vendor")]
        public int? ApiId { get; set; }
        [DisplayName("Payment Date")]
        public string PaymentDate { get; set; }

        [Required(ErrorMessage = "Select File")]
        [FileExt(Allow = ".xls,.xlsx", ErrorMessage = "Only excel file Allowed.")]
        public HttpPostedFileBase UploadedFile { get; set; }



    }
}
