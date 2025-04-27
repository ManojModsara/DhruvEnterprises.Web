using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class VirtualBalDto
    {
        public int ID { get; set; }
        public decimal Amount { get; set; }
        public int AddedByID { get; set; }
        public string Comment { get; set; }
    }
}
