using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class CashDepositwithOTPDto
    {

        public CashDepositwithOTPDto()
        {
            
        }
        
        public int superMerchantId { get; set; }
        public string merchantUserName { get; set; }
        public string merchantPin { get; set; }
        public string subMerchantId { get; set; }
        public string mobileNumber { get; set; }
        public string iin { get; set; }
        public string transactionType { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string requestRemarks { get; set; }
        public string merchantTranId { get; set; }
        public string accountNumber { get; set; }
        public double amount { get; set; }
        public string fingpayTransactionId { get; set; }
        public string otp { get; set; }
        public int cdPkId { get; set; }
        public string paymentType { get; set; }
    }
    public class AepsRequestResponseDto
    {
       
        public string RequestTxt { get; set; }
        public string ResponseText { get; set; }
        public string aadharnumber { get; set; }
        public bool status { get; set; }
        public string message { get; set; }
        public string terminalId { get; set; }
        public string requestTransactionTime { get; set; }
        public string transactionAmount { get; set; }
        public string transactionStatus { get; set; }
        public string balanceAmount { get; set; }
        public string strMiniStatementBalance { get; set; }
        public string bankRRN { get; set; }
        public string transactionType { get; set; }
        public string fpTransactionId { get; set; }
        public string merchantTxnId { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public string merchantTransactionId { get; set; }
        public string bankAccountNumber { get; set; }
        public string ifscCode { get; set; }
        public string bcName { get; set; }
        public string transactionTime { get; set; }
        public string agentId { get; set; }

        public string issuerBank { get; set; }
        public string customerAadhaarNumber { get; set; }
        public string customerName { get; set; }
        public string stan { get; set; }
        public string rrn { get; set; }
        public string uidaiAuthCode { get; set; }
        public string bcLocation { get; set; }
        public string demandSheetId { get; set; }
        public string mobileNumber { get; set; }
        public string urnId { get; set; }
        public string miniStatementStructureModel { get; set; }
        public string miniOffusStatementStructureModel { get; set; }
        public string transactionRemark { get; set; }
        public string bankName { get; set; }
        public string subVillageName { get; set; }
        public string responseCode { get; set; }
        public string transactionStatusMessage { get; set; }
        public string apiStatusCode { get; set; }


    }
}
