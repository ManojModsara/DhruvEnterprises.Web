using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public interface ITagValueService
    {
        ICollection<TagValue> GetApiList(int ApiID);
        ICollection<Tag> GetTagList();
        ICollection<TagValue> GetTagValuesByUrlId(int ApiID, int UrlId);
        ICollection<TagValue> GetApiWithUrlList(int? UrlId);
        bool Save(List<TagValue> tagValueslist, int Urlid, int Apiid);
    }
}
