using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
   public interface IApiService : IDisposable
    {
        ICollection<ApiSource> GetApiList();
        KeyValuePair<int, List<ApiSource>> GetApiSourceList(DataTableServerSide searchModel, int userId = 0, bool IsLowOnly = false);
        ICollection<ApiType> GetApiType(bool isActive = false);
        ApiSource Save(ApiSource apiSource);
        ApiSource GetApiSource(int? id);
        ApiSource GetCallbackApi(string callbackId);
        ICollection<ApiUrlType> GetApiUrlType();
        ICollection<ApiUrl> ApiUrlList(int? ID);
        bool Save(List<ApiUrl> apiUrls, int Apiid);
        ApiUrl GetApiurl(int apiId, int urlTypeId);
        bool DeleteApiTag(int Apiid);
        bool DeleteApiUrl(int Apiid);
        bool DeleteApiSource(int Apiid);
        ApiUrl GetApiurl(int urlid);
        CircleCode GetCircleCode(long? id);
        CircleCode Save(CircleCode apiSource);
        ICollection<CircleCode> GetCircleCodeList(int apiid = 0, int circleid = 0);
    }
}
