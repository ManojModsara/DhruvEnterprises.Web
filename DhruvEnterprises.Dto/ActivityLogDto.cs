using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class ActivityLogDto
    {

        public long Id { get; set; }
        public Nullable<int> UserId { get; set; }
        public string ActivityName { get; set; }
        public Nullable<System.DateTime> ActivityDate { get; set; }
        public string IPAddress { get; set; }
        public string ActivityPage { get; set; }
        public string Remark { get; set; }
    }
}
