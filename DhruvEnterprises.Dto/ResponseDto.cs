using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class ResponseDto<T>
    {
        public ResponseDto()
        {
            Code = HttpStatusCode.OK;
        }
        public bool Status { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public T Data { get; set; }
        public HttpStatusCode Code { get; set; }
        public string[] Errors { get; set; }
    }
}
