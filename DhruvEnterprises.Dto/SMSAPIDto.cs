using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class SMSAPIDto
    {
        public int SMSID { get; set; }
        public string ApiName { get; set; }
        public string SenderID { get; set; }
        public string userid { get; set; }
        public string password { get; set; }
        public string Method { get; set; }
        public Boolean status { get; set; }
        public string SmsURL { get; set; }
        public string Parameter { get; set; }
    }
}
