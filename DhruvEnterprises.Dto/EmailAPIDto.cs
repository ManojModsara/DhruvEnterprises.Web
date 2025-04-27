using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{   
    public class EmailAPIDto
    {
       
        public int Id { get; set; }
        public string ApiName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FromAddress { get; set; }
        public Boolean status { get; set; }
        public int portNumber { get; set; }
    }
}
