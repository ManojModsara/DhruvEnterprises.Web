using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DhruvEnterprises.API
{
    public class ApiModel
    {

    }
    public class Basically
    {
        [Required(ErrorMessage = "UserName is Required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "ApiToken is Required")]
        public string ApiToken { get; set; }
    }

    public class UpiRequestModel : Basically
    {
        [Required(ErrorMessage = "CustomerName is Required")]
        public string CustomerName { get; set; }
        [Required(ErrorMessage = "CustomerEamil is Required")]
        public string CustomerEamil { get; set; }
        [Required(ErrorMessage = "CustomerMobile is Required")]
        public string CustomerMobile { get; set; }
        public string Remark { get; set; }
        [Required(ErrorMessage = "Amount is Required")]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "ClientId is Required")]
        public string ClientId { get; set; }
        [Required(ErrorMessage = "RedirectUrl is Required")]
        public string RedirectUrl { get; set; }
        public string IpAddress { get; set; }
        public string OurTxnId { get; set; }
        public string UserId { get; set; }
    }
    public class UpiStatusCheckModel : Basically
    {
        [Required(ErrorMessage = "ClientId is Required")]
        public string ClientId { get; set; }
    }


    public class CheckRegistration : Basically
    {
        [Required(ErrorMessage = "Mobileno is Required")]
        public string Mobileno { get; set; }
    }


    public class Payout : Basically
    {
        public string sendermobile { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string benename { get; set; }
        public string benemobileno { get; set; }
        public string accountno { get; set; }
        public string ifsccode { get; set; }
        public string remark { get; set; }
        public string txntype { get; set; }
        public string transamount { get; set; }
        public string agentmerchantid { get; set; }
    }

    public class RegistrationKYC : Basically
    {
        [Required(ErrorMessage = "Mobileno is Required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Mobileno is Required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Mobileno is Required")]
        public string MobileNo { get; set; }
    }
    public class MoneyTransferDto
    {
        public string requestId { get; set; }
        public string accountNumber { get; set; }
        public string IFSC { get; set; }
        public string amount { get; set; }
        public string transferMode { get; set; }
        public string accountHolderName { get; set; }   
        public string mobileNumber { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public string bankID { get; set; }
        public string bankName { get; set; }
    }
    public class MoneyTransferModel
    {
        public MoneyTransferModel()
        {
            this.MediumId = 2;
        }
        public string token { get; set; }

        public string OpCode { get; set; }
        public string userid { get; set; }
        public string mobileno { get; set; }
        public string amount { get; set; }
        public string firstname { get; set; }
        public string Name { get; set; }
        public string lastname { get; set; }
        public string opid { get; set; }
        public int bankid { get; set; }
        public string BankName { get; set; }
        public string customerId { get; set; }

        public string accountno { get; set; }
        public string ifsc { get; set; }
        public int MediumId { get; set; }
        public string devimei { get; set; }
        public string reftxnid { get; set; }
        public string ourtxnid { get; set; }
        public string ipaddress { get; set; }
        public string address { get; set; }
        public string Pin { get; set; }
        public string transfermode { get; set; }
        public string beneficiaryName { get; set; }
        public string BCID { get; set; }
        public int AmtTypeId { get; set; }

        public string AgentId { get; set; }
    }
    public class RechargeHelperDto
    {
        public RechargeHelperDto()
        {
            this.ApiRouteList = new List<ApiPriorityDto>();
        }

        public int SwitchId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CommAmount { get; set; }
        public int CurrentPriorityId { get; set; }
        public int CurrentApiId { get; set; }
        public long RecId { get; set; }
        public long RechargeId { get; set; }
        public long TxnId { get; set; }
        public long ApiTxnId { get; set; }
        public List<ApiPriorityDto> ApiRouteList { get; set; }

        public string LapuFilter { get; set; }
        public long LapuId { get; set; }
        public string LapuNo { get; set; }

        public string LapuPass { get; set; }
        public string LapuPIN { get; set; }
        public string LapuOP1 { get; set; }
        public string LapuOP2 { get; set; }
        public string CircleFilter { get; set; }
        public string BlockUser { get; set; }
        public int ApiTypeId { get; set; }
        public int AmtTypeId { get; set; }

        public string UserFilter { get; set; }
        public decimal MinRO { get; set; }
        public long RouteId { get; set; }
        public int FTypeId { get; set; }
        public string RouteOP1 { get; set; }
        public string RouteOP2 { get; set; }
        public int RoutePriorityId { get; set; }
    }
    public class ApiPriorityDto
    {
        public int ApiId { get; set; }
        public int PriorityId { get; set; }
        public string LapuFilter { get; set; }
        public string CircleFilter { get; set; }
        public string BlockUser { get; set; }

        public string UserFilter { get; set; }
        public decimal MinRO { get; set; }
        public long RouteId { get; set; }
        public int FTypeId { get; set; }

        public string RouteOP1 { get; set; }
        public string RouteOP2 { get; set; }

        public int RoutePriorityId { get; set; }

    }
    public class FilterResponseModel
    {
        public int StatusId { get; set; }
        public string Status { get; set; }
        public string ApiTxnID { get; set; }
        public string OperatorTxnID { get; set; }
        public string CustomerMessage { get; set; }

        public string Message { get; set; }
        public string RequestTxnId { get; set; }
        public decimal Vendor_CL_Bal { get; set; }
        public decimal Vendor_OP_Bal { get; set; }
        public string LapuNo { get; set; }
        public string Complaint_Id { get; set; }
        public decimal R_Offer { get; set; }
        public string CustomerName { get; set; }
        public string BillDate { get; set; }
        public string BillPeriod { get; set; }
        public string BillAmount { get; set; }
        public string DueAmount { get; set; }
        public string DueDate { get; set; }
        public string BillNumber { get; set; }
    }


}
