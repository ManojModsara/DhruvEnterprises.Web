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
    public class TagValueService : ITagValueService
    {
        #region "Fields"
        private IRepository<User> repoAdminUser;
        private IRepository<ApiUrl> repoApiUrl;
        private IRepository<ApiType> repoApiType;
        private IRepository<ApiUrlType> repoApiUrlType;
        private IRepository<TagValue> repoTagValue;
        private IRepository<Tag> repoTag;

        #endregion
        #region "Cosntructor"
        public TagValueService(IRepository<User> _repoUserMaster,
            IRepository<ApiUrlType> _repoApiUrlType,
            IRepository<Operator> _repoOperator,
            IRepository<ApiSource> _repoApiSource,
            IRepository<ApiType> _repoApiType,
            IRepository<ApiUrl> _repoApiUrl,
            IRepository<TagValue> _repoTagValue, IRepository<Tag> _repoTag)
        {
            this.repoAdminUser = _repoUserMaster;
            this.repoApiUrlType = _repoApiUrlType;
            this.repoApiType = _repoApiType;
            this.repoApiUrl = _repoApiUrl;
            this.repoTagValue = _repoTagValue;
            this.repoTag = _repoTag;
        }
        #endregion
        
        public ICollection<TagValue> GetApiList(int ApiID)
        {
            List<TagValue> apitagValues = new List<TagValue>();
            apitagValues = repoTagValue.Query().Filter(x => x.ApiId == ApiID).Get().ToList();
            return apitagValues;
        }

        public ICollection<ApiUrl> GetApiUrlList(int ApiID)
        {
            List<ApiUrl> apiurls = new List<ApiUrl>();
            apiurls = repoApiUrl.Query().Filter(x => x.ApiId == ApiID).Get().ToList();
            return apiurls;
        }

        public ICollection<Tag> GetTagList()
        {
            List<Tag> apitag = new List<Tag>();
            apitag = repoTag.Query().Get().ToList();
            return apitag;
        }

        public ICollection<TagValue> GetTagValuesByUrlId(int ApiID,int urlid)
        {
            List<TagValue> apitagValues = new List<TagValue>();
            apitagValues = repoTagValue.Query().Filter(x => x.ApiId == ApiID && x.UrlId== urlid).Get().OrderBy(x=>x.TagId).ToList();
            return apitagValues;
        }

        public ICollection<TagValue> GetApiWithUrlList(int? UrlId)
        {
            List<TagValue> apitagValues = new List<TagValue>();
            apitagValues = repoTagValue.Query().Filter(x => x.UrlId == UrlId).Get().ToList();
            return apitagValues;
        }

        public bool Save(List<TagValue> tagValueslist, int Urlid,int Apiid)
        {
            var tagValues = repoTagValue.Query().Filter(m => m.UrlId == Urlid && m.ApiId== Apiid).Get().ToList();
            
           
                foreach (var data in tagValueslist)
                {
                    data.AddedDate = DateTime.Now;
                    repoTagValue.Insert(data);
                }

                foreach (var tagValue in tagValues)
                {
                    repoTagValue.Delete(tagValue);
                }
           
           
            return true;
        }
        #region "Dispose"
        public void Dispose()
        {
            if (repoAdminUser != null)
            {
                repoAdminUser.Dispose();
                repoAdminUser = null;
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

            if (repoTag != null)
            {
                repoTag.Dispose();
                repoTag = null;
            }
        }
        #endregion
    }
}
