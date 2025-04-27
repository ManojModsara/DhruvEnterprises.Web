using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class RechargeDetail
    {
        public long RecId { get; set; }
        public string CustomerNo { get; set; }
        public decimal Amount { get; set; }
        public int StatusId { get; set; }
        public string StatusMsg { get; set; }
        public string UserTxnId { get; set; }
        public string OurRefTxnId { get; set; }
        public string ApiTxnId { get; set; }
        public string OptTxnId { get; set; }
        public string AccountNo { get; set; }
        public string IFSCCode { get; set; }
        public string BeneficiaryName { get; set; }
        public string Mode { get; set; }

        public string AccountOther { get; set; }
        public string Optional1 { get; set; }
        public string Optional2 { get; set; }
        public string Optional3 { get; set; }
        public string Optional4 { get; set; }

        public int AmountUnitId { get; set; }
        public int AmountLength { get; set; }
        public string DateTimeFormat { get; set; }
        public string RefPadding { get; set; }
        public string RandomKey { get; set; }
        public int IsNumericOnly { get; set; }
        public int RefLength { get; set; }
        public int AmtTypeId { get; set; }
         
        public int UrlId { get; set; }
        public string ApiUrl { get; set; }
        public string Method { get; set; }
        public string ContentType { get; set; }
        public string ResType { get; set; }
        public string PostData { get; set; }

        public int OpId { get; set; }
        public string OpCode { get; set; }
        public string ExtraUrl { get; set; }
        public string ExtraUrlData { get; set; }

        public int ApiId { get; set; }
        public string ApiName { get; set; }
        public string ApiUserId { get; set; }
        public string ApiPassword { get; set; }
        public string ApiOptional { get; set; }
        public int ApiTypeId { get; set; }

        public long LapuId { get; set; }
        public string LapuNo { get; set; }
        public string LapuPass { get; set; }
        public string LapuPIN { get; set; }
        public string LapuOP1 { get; set; }
        public string LapuOP2 { get; set; }
        
        public int CircleId { get; set; }
        public string CircleCode { get; set; }

        public long TxnId { get; set; }
        public decimal DB_Amt { get; set; }
        public string TxnRemark { get; set; }

        public int UserId { get; set; }
        public string CallbackURL { get; set; }
        public string RequestTime { get; set; }
        public string ARequestTime { get; set; }

        public int ResendById { get; set; }
        public string ResendTime { get; set; }

        public long CompId { get; set; }
        public string CompRemark { get; set; }

        public string UserName { get; set; }
        public string ResendByName { get; set; }
        public string UpdatedByName { get; set; }
        public string CircleName { get; set; }
        public string OperatorName { get; set; }
        public decimal ROffer { get; set; }
        public string StatusName { get; set; }
        public string RecDate { get; set; }
        public string IsResend { get; set; }
        public string UpdatedDate { get; set; }


        public int ResendWaitTime { get; set; }
        public int WaitTime { get; set; }
        public int StatusCheckTime { get; set; }
        public bool IsAutoStatusCheck { get; set; }
        public string CallbackId { get; set; }
        public decimal ApiBal { get; set; }
        public decimal UserBal { get; set; }
        public decimal ApiComm { get; set; }
        public decimal UserComm { get; set; } 
        public int ResendCount { get; set; }
        public bool IsROChecked { get; set; }
        public bool IsValidRO { get; set; }
        public string Comment { get; set; }
        public long ComplaintId { get; set; }
    }

    public class dmtDetail
    {
        public long Id { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<long> TxnId { get; set; }
        public Nullable<int> BId { get; set; }
        public Nullable<int> ApiId { get; set; }
        public string AccountNo { get; set; }
        public Nullable<int> OpId { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<byte> RCTypeId { get; set; }
        public Nullable<int> StatusId { get; set; }
        public Nullable<System.DateTime> RequestTime { get; set; }
        public Nullable<System.DateTime> ResponseTime { get; set; }
        public Nullable<byte> MediumId { get; set; }
        public string StatusMsg { get; set; }
        public string IPAddress { get; set; }
        public string UserTxnId { get; set; }
        public string OurRefTxnId { get; set; }
        public string ApiTxnId { get; set; }
        public string OptTxnId { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<int> UpdatedById { get; set; }
        public string ExternalRefno { get; set; }
        public int WTYPEID { get; set; }
        public string BeneficiaryName { get; set; }
        public string BeneMobile { get; set; }
        public string IFSCCode { get; set; }
        public Nullable<decimal> UserComm { get; set; }
        public Nullable<byte> AmtTypeId { get; set; }
        public Nullable<long> DTxnId { get; set; }

    }

}
