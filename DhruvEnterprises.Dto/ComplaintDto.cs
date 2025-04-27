using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
   public class ComplaintDto
    {
        public long ComplaintId { get; set; }
        public long RecId { get; set; }
        public string Remark { get; set; }
        public string ComplaintDate { get; set; }
        public int ComplaintById { get; set; }
        [DisplayName("Resolved?")]
        public bool IsResolved { get; set; }
        public string ResolvedDate { get; set; }
        public int ResolvedById { get; set; }
        public string Comment { get; set; }
        [DisplayName("Refund?")]
        public bool IsRefund { get; set; }
        public string RefundDate { get; set; }
        public int RefundById { get; set; }
        public long RefundTxnId { get; set; }
        public int Priorty { get; set; }
        public bool IsRead { get; set; }
        public int StatusId { get; set; }
        [DisplayName("Operator TxnId")]
        public string  OptTxnId { get; set; } 

    }
}
