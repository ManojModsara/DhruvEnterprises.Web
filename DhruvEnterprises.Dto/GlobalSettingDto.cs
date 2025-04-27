using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
  public  class GlobalSettingDto
    {
        public int id { get; set; }
        public string Actionname { get; set; }
        public bool AllowSMS { get; set; }
        public bool AllowEmail { get; set; }
    }
}
