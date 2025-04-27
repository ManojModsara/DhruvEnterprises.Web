using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
   public class ApiDto
    {
        public ApiDto()
        {
            this.apiURlDtos = new List<ApiURlDto>();
        }

        public List<ApiURlDto> apiURlDtos { get; set; }
        [DisplayName("Vendor Id")]
        public int Id { get; set; }
        [DisplayName("Vendor Name")]
        public string ApiName { get; set; }
        [DisplayName("Vendor Type")]
        public int ApiTypeId { get; set; }
        [DisplayName("Vendor LoginId")]
        public string ApiUserId { get; set; }
        [DisplayName("Vendor Password")]
        public string ApiPassword { get; set; }
        [DisplayName("Optional")]
        public string Remark { get; set; }
        public int AddedById { get; set; }

        //ApiURL
        [DisplayName("UrlId")]
        public int ApiUrlid { get; set; }
        [DisplayName("Vendor Id")]
        public int Apiid { get; set; }
        [DisplayName("Vendor URL")]
        public string ApiUrl { get; set; }
        public int UrlTypeId { get; set; }
        public int Method { get; set; }
        public string ResType { get; set; }
        //ApiType
        public string TypeName { get; set; }
        public bool IsAutoStatusCheck { get; set; }
        [DisplayName("Check")]
        public int CheckTime { get; set; }
        [DisplayName("ReqGap")]
        public int RequestGap { get; set; }

        [DisplayName("Block Amount")]
        public decimal BlockAmount { get; set; }

        [DisplayName("Callback")]
        public int CallbackWaitTime { get; set; }

        [DisplayName("Contact Person")]
        public string ContactPerson { get; set; }
        [DisplayName("Contact No.")]
        public string ContactNo { get; set; }
        [DisplayName("Email-Id")]
        public string EmailId { get; set; }
        [DisplayName("Full Address")]
        public string FullAddress { get; set; }
        public string RequestDate { get; set; }



        [DisplayName("Resend")]
        public int ResendWaitTime { get; set; }

    }


    public class ApiURlDto
    {
        [DisplayName("Vendor Id")]
        public int Id { get; set; }
        [DisplayName("Vendor Name")]
        public string ApiName { get; set; }
        [DisplayName("Vendor Type")]
        public int ApiTypeId { get; set; }
        [DisplayName("LoginId")]
        public string ApiUserId { get; set; }
        [DisplayName("Password")]
        public string ApiPassword { get; set; }
        [DisplayName("Optional")]
        public string Remark { get; set; }
        public int AddedById { get; set; }

        //ApiURL
        [DisplayName("UrlId")]
        public int apiurlid { get; set; }
        [DisplayName("Vendor Id")]
        public int Apiid { get; set; }
        [DisplayName("Vendor Url")]
        public string ApiUrl { get; set; }
        public int UrlTypeId { get; set; }
        public string  Method { get; set; }
        public string ResType { get; set; }
        public string PostData { get; set; }

        //ApiType
        public string TypeName { get; set; }
        public string RequestDate { get; set; }

        public bool IsAutoStatusCheck { get; set; }
        [DisplayName("Chk")]
        public int CheckTime { get; set; }
        [DisplayName("Gap")]
        public int RequestGap { get; set; }

        [DisplayName("Block Amount")]
        public decimal BlockAmount { get; set; }

        [DisplayName("CBk")]
        public int CallbackWaitTime { get; set; }

        [DisplayName("Contact Person")]
        public string ContactPerson { get; set; }
        [DisplayName("Contact No.")]
        public string ContactNo { get; set; }
        [DisplayName("Email-Id")]
        public string EmailId { get; set; }
        [DisplayName("Full Address")]
        public string FullAddress { get; set; }


        [DisplayName("Rsnd")]
        public int ResendWaitTime { get; set; }

    }
}
