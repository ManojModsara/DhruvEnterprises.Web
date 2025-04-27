using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public class ApiService : IApiService
    {
        #region "Fields"
        private IRepository<User> repoAdminUser;
        private IRepository<Operator> repoOperator;
        private IRepository<ApiSource> repoApiSource;
        private IRepository<ApiUrl> repoApiUrl;
        private IRepository<ApiType> repoApiType;
        private IRepository<ApiUrlType> repoApiUrlType;
        private IRepository<TagValue> repoTagValue;
        private IRepository<CircleCode> repoCircleCode;

        #endregion
        #region "Cosntructor"
        public ApiService(IRepository<User> _repoUserMaster,
            IRepository<ApiUrlType> _repoApiUrlType,
            IRepository<Operator> _repoOperator,
            IRepository<ApiSource> _repoApiSource,
            IRepository<ApiType> _repoApiType,
            IRepository<ApiUrl> _repoApiUrl,
            IRepository<TagValue> _repoTagValue,
            IRepository<CircleCode> _repoCircleCode)
        {
            this.repoAdminUser = _repoUserMaster;
            this.repoOperator = _repoOperator;
            this.repoApiSource = _repoApiSource;
            this.repoApiUrlType = _repoApiUrlType;
            this.repoApiType = _repoApiType;
            this.repoApiUrl = _repoApiUrl;
            this.repoTagValue = _repoTagValue;
            this.repoCircleCode = _repoCircleCode;
        }
        #endregion

        #region "Metods"
        public ICollection<ApiSource> GetApiList()
        {
            return repoApiSource.Query().AsTracking().Get().ToList();
        }

        public ApiSource GetCallbackApi(string callbackId)
        {
            return repoApiSource.Query().Filter(x => x.CallbackId == callbackId).AsTracking().Get().ToList().FirstOrDefault();
        }

        public ICollection<ApiType> GetApiType(bool isActive = false)
        {
            return repoApiType.Query().AsTracking()
                //.Filter(c => c.IsActive == isActive || c.IsActive == true)
                .Get().ToList();
        }

        public ICollection<ApiUrlType> GetApiUrlType()
        {
            return repoApiUrlType.Query().AsTracking()
                //.Filter(c => c.IsActive == isActive || c.IsActive == true)
                .Get().ToList();
        }

        public ICollection<ApiUrl> ApiUrlList(int? ID)
        {
            List<ApiUrl> apiUrlslist = new List<ApiUrl>();
            apiUrlslist = repoApiUrl.Query().Filter(x => x.ApiId == ID).Get().ToList();
            //if (Apiid !=null)
            //{
            //    apiUrlslist = repoApiUrl.Query().Filter(x => x.ApiId == Apiid.Id).Get().ToList();
            //}
            return apiUrlslist;
        }

        public ApiSource Save(ApiSource apiSource)
        {

            if (apiSource.Id == 0)
            {
                apiSource.AddedDate = DateTime.Now;
                repoApiSource.Insert(apiSource);
            }
            else
            {
                apiSource.UpdatedDate = DateTime.Now;
                repoApiSource.Update(apiSource);
            }
            return apiSource;
        }

        public KeyValuePair<int, List<ApiSource>> GetApiSourceList(DataTableServerSide searchModel, int userId = 0, bool IsLowOnly = false)
        {
            var predicate = CustomPredicate.BuildPredicate<ApiSource>(searchModel, new Type[] { typeof(ApiSource) });
            if (IsLowOnly)
            {
                predicate = predicate.And(x => x.ApiBal != null && x.BlockAmount != null && x.BlockAmount > 0 && x.ApiBal < x.BlockAmount);

            }

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<ApiSource> results = repoApiSource
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(ApiSource) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<ApiSource>> resultResponse = new KeyValuePair<int, List<ApiSource>>(totalCount, results);

            return resultResponse;
        }

        public ApiSource GetApiSource(int? id)
        {
            return repoApiSource.FindById(id);
        }

        public ApiUrl GetApiurl(int apiId, int urlTypeId)
        {
            return repoApiUrl.Query().Filter(c => c.ApiId == apiId && c.UrlTypeId == urlTypeId)
                                      .Get()
                                      .FirstOrDefault();
        }

        public ApiUrl GetApiurl(int urlid)
        {
            return repoApiUrl.FindById(urlid);
        }

        public bool Save(List<ApiUrl> apiUrls, int Apiid)
        {
            foreach (var data in apiUrls)
            {
                if (data.Id == 0)
                {
                    data.AddedDate = DateTime.Now;
                    repoApiUrl.Insert(data);
                }
                else
                {
                    data.UpdatedDate = DateTime.Now;
                    repoApiUrl.Update(data);
                }
            }
            return true;
        }

        public bool DeleteApiTag(int Apiid)
        {
            var tagValues = repoTagValue.Query().Filter(m => m.ApiId == Apiid).Get().ToList();

            foreach (var tagValue in tagValues)
            {
                repoTagValue.Delete(tagValue);
            }
            return true;

        }

        public bool DeleteApiUrl(int Apiid)
        {

            var apiUrls = repoApiUrl.Query().Filter(m => m.ApiId == Apiid).Get().ToList();

            foreach (var apiUrl in apiUrls)
            {
                repoApiUrl.Delete(apiUrl);
            }
            return true;

        }

        public bool DeleteApiSource(int Apiid)
        {
            var apiSources = repoApiSource.Query().Filter(m => m.Id == Apiid).Get().ToList();
            foreach (var apiSource in apiSources)
            {
                repoApiSource.Delete(apiSource);
            }
            return true;

        }

        public CircleCode GetCircleCode(long? id)
        {
            return repoCircleCode.FindById(id);
        }

        public CircleCode Save(CircleCode apiSource)
        {

            if (apiSource.Id == 0)
            {
                apiSource.AddedDate = DateTime.Now;
                repoCircleCode.Insert(apiSource);
            }
            else
            {
                apiSource.UpdatedDate = DateTime.Now;
                repoCircleCode.Update(apiSource);
            }
            return apiSource;
        }

        public ICollection<CircleCode> GetCircleCodeList(int apiid=0, int circleid=0)
        {
            return repoCircleCode.Query().Filter(x=>(apiid==0?true:x.ApiId==apiid) && (circleid == 0 ? true : x.CircleId == circleid)).Get().ToList();
        }


        #endregion

        #region "Dispose"
        public void Dispose()
        {
            if (repoAdminUser != null)
            {
                repoAdminUser.Dispose();
                repoAdminUser = null;
            }

            if (repoOperator != null)
            {
                repoOperator.Dispose();
                repoOperator = null;
            }

            if (repoApiSource != null)
            {
                repoApiSource.Dispose();
                repoApiSource = null;
            }

            if (repoApiUrl != null)
            {
                repoApiUrl.Dispose();
                repoApiUrl = null;
            }

            if (repoApiType != null)
            {
                repoApiType.Dispose();
                repoApiType = null;
            }

            if (repoApiUrlType != null)
            {
                repoApiUrlType.Dispose();
                repoApiUrlType = null;
            }

            if (repoTagValue != null)
            {
                repoTagValue.Dispose();
                repoTagValue = null;
            }
        }
        #endregion
    }
}
