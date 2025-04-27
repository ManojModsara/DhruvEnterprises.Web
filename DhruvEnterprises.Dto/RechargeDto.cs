using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class RechargeDto
    {
        public string RecId { get; set; }
        public int StatusId { get; set; }
        public int StatusName { get; set; } 
        public string CustomerNo { get; set; }  
       
    }

    public class RechargeUpdateDto 
    {
        public long RecId { get; set; }
        [DisplayName("Status")]
        public int StatusId { get; set; }
        public int StatusName { get; set; }
        [DisplayName("Vendor")]
        public int ApiId { get; set; }
        public int ApiName { get; set; }
        [DisplayName("Operator-TxnId")]
        public string OpTxnId { get; set; }
        [DisplayName("Vendor-TxnId")]
        public string ApiTxnId { get; set; }
        public string RecIds { get; set; }

        [DisplayName("Check Status")]
        public bool IsActive { get; set; }
        public string OurRefId { get; set; }

        public int IsProcessing { get; set; } 

        public List<StatusTypeDto> StatusList = new List<StatusTypeDto>();
        public List<ApiSourceDTO> ApiList = new List<ApiSourceDTO>(); 
    }

    public class StatusTypeDto 
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public bool Selected { get; set; } 
    }

    public class FaultRcdto
    {   
        public string RecId { get; set; }
        public string OurRefTxnId { get; set; }
        public string UserId { get; set; }
        public string ApiId { get; set; }
        public string UserName { get; set; }
        public string ApiName { get; set; }    
        public string TxnId { get; set; }
        public string ApiTxnId { get; set; }
        public string RecDate { get; set; }
        public string CustomerNo { get; set; }
        public string Amount { get; set; }
        public string DB_Amt { get; set; }
        public string StatusId { get; set; }
        public string UpdateDate { get; set; }
        public string TxnDate { get; set; }
    }

}
