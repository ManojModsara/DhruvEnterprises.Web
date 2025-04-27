using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DhruvEnterprises.Web.LIBS
{
    public class FilterResponseModel
    {
        public int StatusId { get; set; }
        public string Status { get; set; }
        public string ApiTxnID { get; set; }
        public string OperatorTxnID { get; set; }
        public string Message { get; set; }
        public string RequestTxnId { get; set; }
        public decimal Vendor_CL_Bal { get; set; }
        public decimal Vendor_OP_Bal { get; set; }
        public string LapuNo { get; set; }
        public string Complaint_Id { get; set; }
        public decimal R_Offer { get; set; }
        public string CustomerNo { get; set; }
        public string CustomerName { get; set; }
        public string BillNo { get; set; }
        public string BillPeriod { get; set; }
        public string BillDate { get; set; }
        public string BillDueDate { get; set; }
        public decimal BillPrice { get; set; }
    }
}