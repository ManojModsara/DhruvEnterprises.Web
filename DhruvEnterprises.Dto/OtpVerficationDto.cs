using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
   public class OtpVerficationDto
    {
        public string Url { get; set; }
        public int OTP { get; set; }
        public int UserId { get; set; }
    }

    public class UserProfilesDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EmailID { get; set; }
        public string Mobileno { get; set; }
        public string Phoneno { get; set; }
        public string Contactno { get; set; }
        public string Address { get; set; }
        public string ORGName { get; set; }
        public string GSTNo { get; set; }
        public string GSTADDRESS { get; set; }
        public string ApiToken { get; set; }
        public string HiddenApiToken { get; set; }
        [DisplayName("Get New Token")]
        public bool ChangeToken { get; set; }
        public int OTP { get; set; }
        public string IpAddress { get; set; }
        public string CallBackUrl { get; set; } 
    }
}
