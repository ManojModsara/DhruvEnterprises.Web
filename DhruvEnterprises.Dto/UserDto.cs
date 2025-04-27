using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class UserDto
    { 
        //USER
        public int Uid { get; set; }
        public int Id { get; set; }
        [Required] 
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public Boolean IsSuperAdmin { get; set; }
        public Boolean IsActive { get; set; }
        public Boolean IsLocked { get; set; }
        [Required]
        public int RoleId { get; set; }
        public int? PackageId { get; set; }

        public DateTime? AddedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int LockCount { get; set; }
        public string AppId { get; set; }
        public string ResetCode { get; set; }

        [DisplayName("Recharge CallBack Url")]
        public string CallBackUrl { get; set; }
     
        public string IP { get; set; }
        //PROFILE
        [Required]
        public string Name { get; set; }
        [Required]
        public string EmailId { get; set; }
     
        public string ContactNo { get; set; }

        //Extras
        public string RoleName { get; set; }
        public string PackageName { get; set; }

        public decimal CreditLimit { get; set; }
        public Guid ApiKey { get; set; }
        public Guid HKey { get; set; }
        public Guid HPass { get; set; }

        [DisplayName("Complaint CallBack Url")]
        public string ComplaintCallBackUrl { get; set; }
        public int? StateID { get; set; }

        public decimal BlockAmt { get; set; }
        public int? VendorId { get; set; }
        [DisplayName("Payout CallBack Url")]
        public string DmtCallBackUrl { get; set; }
    }
    public class KYCDataDtO
    {
        public KYCDataDtO()
        {
            this.DocId = new List<int>();

        }
        [DisplayName("UserID")]
        [Required]
        public int Id { get; set; }
        [Required]
        public List<string> DocNumber { get; set; }
        [DisplayName("IsKYC")]
        public bool IsKYC { get; set; }
        [Required]
        public List<int> DocId { get; set; }
        [DisplayName("Document Type")]
        public string Doctype { get; set; }
    }
    public class ImageInfo
    {
        public int Userid { get; set; }
        public string DocumentNumber { get; set; }
        public string FrontImg { get; set; }
        public string BakImg { get; set; }
        public int DocId { get; set; }
    }
    public class KYCDataDto
    {
        [DisplayName("Retailer Id")]
        public int Id { get; set; }
        [DisplayName("Adhar No.")]
        public string AdharNo { get; set; }
        [DisplayName("Front IMG")]
        public string FrtIMG { get; set; }
        [DisplayName("Back IMG")]
        public string BckIMG { get; set; }
        [DisplayName("IsKYC")]
        public bool IsKYC { get; set; }

    }

    public class AepsUserDto
    {
        //USER
        
        public int Id { get; set; }
        
        public Boolean IsAdminRoute { get; set; }
        
    }


}
