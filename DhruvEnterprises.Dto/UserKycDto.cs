using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    
    public partial class UserKycDto
    {
        public int Id { get; set; }
        public string AdhaarFrontEnd { get; set; }
        public string AdhaarBackEnd { get; set; }
        public string PanCard { get; set; }
        public string CancelChequeUrl { get; set; }
        public int UserId { get; set; }
        public int Status { get; set; }
        public Nullable<int> Addedbyid { get; set; }
        public Nullable<System.DateTime> Addeddate { get; set; }
        public Nullable<int> updatebyid { get; set; }
        public Nullable<System.DateTime> Updateddate { get; set; }
        public string Photo { get; set; }
        public string Comment { get; set; }
        
    }
}
