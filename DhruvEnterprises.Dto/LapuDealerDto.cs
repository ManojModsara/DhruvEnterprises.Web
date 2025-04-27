using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public  class LapuDealerDto
    {
        public int    Id { get; set; }
        public string Name { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public string GSTNumber { get; set; }
        public string FullAddress { get; set; }
        public string Remark { get; set; }
        public string AddedDate { get; set; }
        public string AddedBy { get; set; }
        public string UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public string HistoryDate { get; set; } 
    }
}
