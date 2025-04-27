using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class LapuDto
    {
        public LapuDto()
        {
            this.ApiIds = new List<int>();
            this.ApiList = new List<ApiDto>();
        }
        public long Id { get; set; }
        public string Number { get; set; }
       
        public string PassWd { get; set; }
        public string PIN { get; set; }

        public int  DealerId { get; set; }
        public int  OpId { get; set; }
        public int  CircleId { get; set; }

        public string  Dealer { get; set; }
        public string  Operator { get; set; }
        public string  Circle { get; set; }
        public decimal? Margin { get; set; } 
        public decimal?  BlockAmount { get; set; }
        public decimal?  HomeLimit { get; set; }
        public decimal?  RoamLimit { get; set; }
        public decimal?  UsedHomeLimit { get; set; }
        public decimal?  UsedRoamLimit { get; set; }
        public string Remark { get; set; }

        public string AddedDate { get; set; }
        public int? AddedById { get; set; }
        public int? AddedBy { get; set; }

        public string UpdatedDate { get; set; }
        public int? UpdatedById { get; set; }
        public string UpdatedBy { get; set; }

        public bool IsActive { get; set; }

        public string Optional1 { get; set; }
        public string Optional2 { get; set; }
        public List<int> ApiIds { get; set; }

        public bool IsEzytmLapu { get; set; }
        public string LapuOTP { get; set; }
        public string LapuIds { get; set; } 
        public List<ApiDto> ApiList { get; set; }
    }
    
    public class LapuFilterDto
    {
        public long LapuId { get; set; }
        public int OpId { get; set; }
        public int CircleId { get; set; }
        public int DealerId { get; set; }
        public int ApiId { get; set; }
        public int StatusId { get; set; }
        public string Remark { get; set; }

    }
    
}
