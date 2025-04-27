using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DhruvEnterprises.API.Models
{
    public class RequestResponseDto
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public int UrlId { get; set; }
        public long RecId { get; set; }
        public long RechargeId { get; set; }
        public string RefId { get; set; }
        public string Remark { get; set; }
        public string RequestTxt { get; set; }
        public string ResponseText { get; set; }
        public string OpId { get; set; } 
        public string CustomerNo { get; set; }
        public string UserReqId { get; set; } 
    }
}