using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DhruvEnterprises.Dto
{
  public  class InvoiceDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string InvoiceMonth { get; set; }
        public string InvoiceYear { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceImage { get; set; }

        [Required(ErrorMessage = "Select File")]
        [FileExt(Allow = ".pdf", ErrorMessage = "Only PDF file Allowed.")]
        public HttpPostedFileBase UploadedFile { get; set; }
        public System.DateTime AddedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }

      
      
    }
}
