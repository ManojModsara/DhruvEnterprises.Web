using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
  
    public class RoboLapuDto
    {  
        public int Id { get; set; }
        public string LapuNo { get; set; }
        public bool IsActive { get; set; }
        public decimal Balance { get; set; }

        public string LapuNo2 { get; set; }
        public bool IsActive2 { get; set; }
        public decimal Balance2 { get; set; }

        public decimal BalanceDiff { get; set; }
        public int TypeId { get; set; } // 1=Robo, 2=Common, 3=Ezytm

    }
    
    public class RoboLapuReport
    {
        public string LapuNumber { get; set; }
        public string OpCode { get; set; }
        public string Lstatus { get; set; }
        public double LapuBal { get; set; }
    }

    public class RootRoboLapu
    {
        public string ERROR { get; set; }
        public string STATUS { get; set; }
        public string MESSAGE { get; set; }
        public List<RoboLapuReport> LAPUREPORT { get; set; }
    }

    public class RootRoboLapuPurchase 
    { 
        public string ERROR { get; set; }
        public string STATUS { get; set; }
        public string MESSAGE { get; set; }
        public List<LapuPurchaseHistoryDto> LAPUREPORT { get; set; } 
    }

    public class LapuPurchaseHistoryDto
    {
        public string transDate { get; set; }
        public string transId { get; set; }
        public string DisMobileNumber { get; set; }
        public double BalAmount { get; set; }
    }

}
