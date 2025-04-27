using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DhruvEnterprises.API
{
    public class ResponseModel
    {
       
    }

    public class UpiResponse
    {
        public string Error { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string ApiTxnId { get; set; }
        public string ClientId { get; set; }
        public UpiUrlResponse Data { get; set; }

        //[JsonIgnore]
        //public long TxnLedger_Id { get; set; }
    }

    public class UpiStatusResponse
    {
        public string Error { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string ApiTxnId { get; set; }
        public string Amount { get; set; }
        public string Order_Id { get; set; }
        public string UpiTxnId { get; set; }
        public string UtrNo { get; set; }
        public string ClientId { get; set; }
        public string Integration_id { get; set; }
        
    }
    public class UpiUrlResponse
    {
        public string QrIntent { get; set; }
        public string PaymentUrl { get; set; }
    }
   
}
