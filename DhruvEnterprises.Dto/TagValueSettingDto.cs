using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
   public class TagValueSettingDto
    {
        public TagValueSettingDto()
        {
            this.tagValueDtos = new List<TagValueDto>();
        }
        [DisplayName("Vendor Id")]
        public int ApiId { get; set; }
        [DisplayName("Vendor Name")]
        public string ApiName { get; set; }
        public int UrlId { get; set; }
        public string TypeName { get; set; }
        public List<TagValueDto> tagValueDtos { get; set; }

    }

    public class TagValueDto
    {
        public int UrlId { get; set; }
        [DisplayName("Vendor Id")]
        public int ApiId { get; set; }
        public int TagId { get; set; }
        public string PreTxt { get; set; }
        public string Name { get; set; }
        public string PostText { get; set; }
        public string TagMsg { get; set; }
        public string CompareTxt { get; set; }
        public Nullable<int> PreMargin { get; set; }
        public Nullable<int> PostMargin { get; set; }
        public int TagIndex { get; set; }
        public string ResSeparator { get; set; }
        public string ResValue { get; set; } 

    }

}
