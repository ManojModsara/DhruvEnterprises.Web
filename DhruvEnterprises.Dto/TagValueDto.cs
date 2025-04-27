using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MYRC.Dto
{
   public class TagValueDto
    {
        public  TagValueDto()
        {
            this.tagNames = new List<TagName>();
            this.apiUrlTypes = new List<ApiUrlType>();
        }
        public int ApiId { get; set; }
        public string ApiName { get; set; }
        public int UrlId { get; set; }
        public string TypeName { get; set; }
        public int TagId { get; set; }
        public int PreTxt { get; set; }
        public string Name { get; set; }
        public int PostText { get; set; }
        public int PreMargin { get; set; }
        public int PostMargin { get; set; }
        public List<int> apiurlList { get; set; }
        public List<TagName> tagNames { get; set; }
        public List<ApiUrlType> apiUrlTypes { get; set; }
    }

    public class TagName
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ApiUrlType
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
    }
}
