using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DhruvEnterprises.Dto
{
  public  class OperatorSerialDto
    {
        public int Id { get; set; }
        public string Series { get; set; }
        public int OpId { get; set; }
        public int CircleId { get; set; }
    }
    public class OperatorSerialExcelDto
    {
        public int Id { get; set; }
        public string Series { get; set; }
        public int OpId { get; set; }
        public int CircleId { get; set; }
        [Required(ErrorMessage = "Select File")]
        [FileExt(Allow = ".xls,.xlsx", ErrorMessage = "Only excel file Allowed.")]
        public HttpPostedFileBase UploadedFile { get; set; }
    }
}
