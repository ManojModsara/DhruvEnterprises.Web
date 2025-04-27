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
    public class ExportDto
    {

        [DisplayName("Report Name")]
        public int TypeId { get; set; }
        public string TypeName { get; set; }
        [DisplayName("User Name")]
        public int UserId { get; set; }
        [DisplayName("Vendor Name")]
        public int ApiId { get; set; }
        [DisplayName("From Date")]
        public string StartDate { get; set; }
        [DisplayName("To Date")]
        public string EndDate { get; set; }

        public string SearchId { get; set; }
        public int StatusId { get; set; }
        public int OpId { get; set; }

        public DateTime sDate { get; set; }
        public DateTime eDate { get; set; }

        public int CircleId { get; set; }
        public string CustomerNo { get; set; }
        public string UserRefId { get; set; }
        public string ApiTxnId { get; set; }
        public string OpTxnId { get; set; }

        public string Remark { get; set; }
        public int UpdatedById { get; set; }

        public int IsMail { get; set; }

    }
    public class ExportDMTDto
    {
        public int? Id { get; set; }
        public string UserId { get; set; }
        public string CustomerNo { get; set; }

        public long? DMT_Id { get; set; }
        public long? Txn_Id { get; set; }
        public string Op_TxnId { get; set; }
        public string Vndr_TxnId { get; set; }
        public string Bank { get; set; }
        public string UserReqId { get; set; }
        public string Status { get; set; }
        public decimal? BalanceAmt { get; set; }
        public decimal? Discount { get; set; }
        [DisplayName("Requested Date")]
        public DateTime? ReqTime { get; set; }
        [DisplayName("Response Date")]
        public DateTime? ResTime { get; set; }
        public DateTime? UpdatedON { get; set; }
        public int? UpdatedBY { get; set; }

    }
    public class ExportTypeDto
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; }
    }

    public class ExportRechargeDto
    {
        public string RecId { get; set; }
        public string UserId { get; set; }
        public string VendorId { get; set; }
        public string TxnId { get; set; }
        public string RequestTime { get; set; }
        public string OurRefTxnId { get; set; }
        public string CustomerNo { get; set; }
        public string Service { get; set; }
        //public string CircleId { get; set; }
        public string Amount { get; set; }
        public string StatusId { get; set; }
        public string ResponseTime { get; set; }
        public string UserTxnId { get; set; }
        public string RCTypeId { get; set; }
        public string MediumId { get; set; }
        public string IPAddress { get; set; }
        public string ROfferAmount { get; set; }
        public string VendorTxnId { get; set; }
        public string OptTxnId { get; set; }
        public string StatusMsg { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }


    }

    public class ExportReqResDto
    {
        public string Id { get; set; }
        public string RecId { get; set; }
        public string User { get; set; }
        public string Vendor { get; set; }
        public string OurRefTxnId { get; set; }
        public string CustomerNo { get; set; }
        public string UserTxnId { get; set; }
        public string AddedDate { get; set; }
        public string Remark { get; set; }
        public string RcStatus { get; set; }
        public string RcOperator { get; set; }
        public string RequestText { get; set; }
        public string ResponseText { get; set; }
        public string UpdatedDate { get; set; }
    }

    public class ExporStatementDto
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string PaymentDate { get; set; }
        public string AddedDate { get; set; }
        public string OP_Bal { get; set; }
        public string CR_Amt { get; set; }
        public string DB_Amt { get; set; }
        public string CL_Bal { get; set; }
        public string TxnTypeId { get; set; }
        public string AmtTypeId { get; set; }
        public string TrTypeId { get; set; }
        public string PaymentRef { get; set; }
        public string Remark { get; set; }
        public string Comment { get; set; }
        public string TxnId { get; set; }
        public string ApiTxnId { get; set; }
        public string RefAccountId { get; set; }
        public string AddedById { get; set; }
        public string UpdatedById { get; set; }
        public string UpdatedDate { get; set; }
        public string UserId { get; set; }
        public string ApiId { get; set; }


    }

    public class ExportActivityLogDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string ActivityName { get; set; }
        public string ActivityDate { get; set; }
        public string IPAddress { get; set; }
        public string ActivityPage { get; set; }
        public string Remark { get; set; }
    }

    public class ExportComplaintDto
    {
        public string CmpId { get; set; }
        public string User { get; set; }
        public string Vendor { get; set; }
        public string AddedOn { get; set; }
        public string CmpStatus { get; set; }
        public string CmpRemark { get; set; }
        public string Comment { get; set; }
        public string IsRefund { get; set; }
        public string UpdatedOn { get; set; }

        public string RecId { get; set; }
        public string RecDate { get; set; }
        public string CustomerNo { get; set; }
        public string Operator { get; set; }
        public string OptTxnId { get; set; }
        public string Amount { get; set; }
        public string RcStatus { get; set; }
        public string UserReqId { get; set; }
        public string OurRefId { get; set; }
        public string TxnId { get; set; }
        public string VendorTxnId { get; set; }
        public string Circle { get; set; }

    }

    public class ExportTxnLedger
    { 
        public string TxnId { get; set; }
        public string TxnDate { get; set; }
        public string RecId { get; set; }
        public string RcAmount { get; set; }
        public string CommAmt { get; set; } 
        public string OP_Bal { get; set; }
        public string CR_Amt { get; set; }
        public string DB_Amt { get; set; }
        public string CL_Bal { get; set; }
        public string UserId { get; set; }
        public string VendorId { get; set; }
        public string RefTxnId { get; set; }
        public string TxnType { get; set; }
        public string AmtType { get; set; }
        public string Remark { get; set; }
        public string ApiTxnId { get; set; }
        public string AddedBy { get; set; }
    }

    public class ExportRecharge2Dto
    {
        public string Id { get; set; }
        public string User { get; set; }
        public string RecDate { get; set; }
        public string CustomerNo { get; set; }
        public string Service { get; set; }
        public string Status { get; set; }
        public string Amount { get; set; }
        public string Earn { get; set; }
        public string Db_Amt { get; set; }
        public string Cl_Bal { get; set; }
        public string OptTxnId { get; set; }
        //public string Circle { get; set; }
        public string Vendor { get; set; }
        public string VComm { get; set; }
        public string UserReqId { get; set; }


    }

    public class CompareRcDto
    {
        [Required(ErrorMessage = "Select File")]
        [FileExt(Allow = ".xls,.xlsx", ErrorMessage = "Only excel file Allowed.")]
        public HttpPostedFileBase UploadedFile { get; set; }

        [DisplayName("Vendor")]
        public int ApiId { get; set; }

        [DisplayName("Recharge Date(dd/MM/yyyy)")]
        public string RecDate { get; set; }

        [DisplayName("Clean Old Data?")]
        public bool IsClear { get; set; }  
    }
    public class ExportOperatorSeries
    {
        public string Id { get; set; }
        public string OpId { get; set; }
        public string CircleId { get; set; }
        public string Series { get; set; }
        public int cid { get; set; }

    }

    public class FileExt : ValidationAttribute
    {
        public string Allow;
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                string extension = ((System.Web.HttpPostedFileBase)value).FileName.Split('.')[1];
                if (Allow.Contains(extension))
                    return ValidationResult.Success;
                else
                    return new ValidationResult(ErrorMessage);
            }
            else
                return ValidationResult.Success;
        }
    }
}
