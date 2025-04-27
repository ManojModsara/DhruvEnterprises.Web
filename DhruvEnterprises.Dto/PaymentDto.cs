using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class PaymentDto
    {
        public PaymentDto()
        {
            this.StatusList = new List<StatusTypeDto>();
        }
        [Required(ErrorMessage = "OrderId is required")]
        public long Id { get; set; }
        [Required(ErrorMessage = "User id is Required")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "Amount is required")]
        public decimal? Amount { get; set; }
        [Required(ErrorMessage = "Status is Required")]
        public byte StatusId { get; set; }
        public string TxnId { get; set; }
        [DisplayName("Remark")]
        public string PaymentRemark { get; set; }
        public DateTime AddedDate { get; set; }
        public int AddedById { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UpdatedById { get; set; }
        [DisplayName("Payment Date")]
        public string PaymentDate { get; set; }
        [Required(ErrorMessage = "Currency Code is required")]
        public string CurrencyCode { get; set; }
        public string ChequeNo { get; set; }
        public string FName { get; set; }
        public string Email { get; set; }
        public string MobileNo { get; set; }
        public string CallBack { get; set; }
        public string WebSite { get; set; }
        public string OrderId { get; set; }


        public string TokenId { get; set; }
        public List<StatusTypeDto> StatusList { get; set; }

    }

    public class EzytmPayment
    {
        public System.Collections.Specialized.NameValueCollection Headers { get; }
        [Required(ErrorMessage = "Required")]
        public string TokenId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Amount { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Required")]
        public string MobileNo { get; set; }
        [Required(ErrorMessage = "Required")]
        public string Remark { get; set; }
        [Required(ErrorMessage = "Required")]
        public string EmailId { get; set; }
        [Required(ErrorMessage = "Required")]
        public string WebSite { get; set; }
        [Required(ErrorMessage = "Required")]
        public string CallBack { get; set; }
        public string CurrencyCode { get; set; }
        public string UserId { get; set; }
        public string OrderId { get; set; }

        public int EzPay { get; set; }

    }
    public class PaymentResponseDto
    {
        public string MID { get; set; }
        public string TXNID { get; set; }
        public string ORDERID { get; set; }
        public string BANKTXNID { get; set; }
        public string TXNAMOUNT { get; set; }
        public string CURRENCY { get; set; }
        public string STATUS { get; set; }
        public string RESPCODE { get; set; }
        public string RESPMSG { get; set; }
        public DateTime TXNDATE { get; set; }
        public string GATEWAYNAME { get; set; }
        public string BANKNAME { get; set; }
        public string PAYMENTMODE { get; set; }
        public string CHECKSUMHASH { get; set; }

    }

    public class EzyTMResponseDto
    {
        public string TXNID { get; set; }
        public string ORDERID { get; set; }
        public string BANKTXNID { get; set; }
        public decimal TXNAMOUNT { get; set; }
        public int STATUS { get; set; }
        public string RESPMSG { get; set; }
        public DateTime TXNDATE { get; set; }

    }
    public class PaymentGatewayDto
    {
        public int Id { get; set; }
        public string MerchantId { get; set; }
        public string MerchantKey { get; set; }
        public string Website { get; set; }
        public string IndustryType { get; set; }
        public string ChannelId_WEB { get; set; }
        public string ChannelId_App { get; set; }
        public int NameTypedId { get; set; }
        public int TypeId { get; set; }
        public DateTime AddedDate { get; set; }
        public int AddedbyId { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public string ApiUrl { get; set; }
        public string Call_BackUrl { get; set; }
        public string App_CallBackUrl { get; set; }
    }
    public class PaymenttxnDto
    {
        public string CustomerNo { get; set; }
        public string gTxnId { get; set; }
        public int userId { get; set; }
        public int orId { get; set; }
        public string geteName { get; set; }
        public string bankTxnId { get; set; }
        public Nullable<int> txnid { get; set; }
        public string remark { get; set; }
        public Nullable<int> modId { get; set; }
        public int statusId { get; set; }
        public string Sdate { get; set; }
        public string Edate { get; set; }

        public int Isa { get; set; }

        public string SdateNow { get; set; }
        public string EdateNow { get; set; }

        public int UpdatedById { get; set; }

        public int IsResentOnly { get; set; }

        public string MobileNumber { get; set; }
        public string bankName { get; set; }
    }
    public class UpdateGatewayStatusDto
    {
        public int Id { get; set; }
        [DisplayName("Status")]
        public int Status { get; set; }
        [DisplayName("Check Status")]
        public bool IsActive { get; set; }
        public string Remark { get; set; }
        public int IsProcessing { get; set; }
        [DisplayName("Bank Transaction Id")]
        public string BankTxnId { get; set; }
        public List<StatusTypeDto> StatusList = new List<StatusTypeDto>();
    }
    public class AEPSChangeStatusDto
    {
        public int UserId { get; set; }
        public int KYCStatus { get; set; }
        public List<StatusTypeDto> StatusList = new List<StatusTypeDto>();
    }
}