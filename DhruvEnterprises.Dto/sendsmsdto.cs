using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
   public class sendsmsdto
    {
        public int Type { get; set; }
        public string UserID { get; set; }
        public string Message { get; set; }
        public bool IsSms { get; set; }
        public bool IsPush { get; set; }
        public int RoleId { get; set; }
    }
}
