using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class LapuPurchageDto
    {
        public string LapuId { get; set; }
        public string TxnDate { get; set; }
        public string LapuNo { get; set; }
        public string OpName { get; set; }
        public string CircleName { get; set; }
        public string CR_Amt { get; set; }
        public string CM_Amt { get; set; }
        public string Total { get; set; }
        public string DealerName { get; set; }
        public string Margin  { get; set; }
        public string DB_Amt { get; set; }
        public string PurDate { get; set; } 
    }

    public class LapuPurFilterDto
    {   
        public string LapuId { get; set; }
        public string OpId { get; set; }
        public string CircleId { get; set; }
        public string DealerId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
         
    }
}
