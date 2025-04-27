using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
   public class LapuForHistoryDto
    {
        public int Id { get; set; }
        public int ApiId { get; set; }
        public int OpId { get; set; }
        public string ApiUserId { get; set; }
        public string ApiPassword { get; set; }
        public string OpCode { get; set; }
        public string Remark { get; set; } 
    }
}
