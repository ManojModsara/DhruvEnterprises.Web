using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class RandomKeyGenDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int OpId { get; set; }
        public int Apiid { get; set; }
        public int KeyTypeId { get; set; }
        public string LengthOrText { get; set; }
        public string RandomKey { get; set; }
        public int NoOfKeys { get; set; }
    }
}
