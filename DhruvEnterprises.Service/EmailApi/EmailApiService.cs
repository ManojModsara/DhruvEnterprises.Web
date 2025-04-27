using System;
using System.Collections.Generic;
using System.Linq;
using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Repo;

namespace DhruvEnterprises.Service
{
   public class EmailApiService: IEmailApiService
    {
        #region "Fields"
        private IRepository<EmailAPI> repoEmailAPI;
        
        #endregion

        #region "Cosntructor"
        public EmailApiService(IRepository<EmailAPI> _repoEmailAPI)
        {
            this.repoEmailAPI = _repoEmailAPI;
                   }
        #endregion

        #region "Methods"

        public EmailAPI GetEmailById(int id = 0)
        {
            return repoEmailAPI.FindById(id);
        }

        public KeyValuePair<int, List<EmailAPI>> GetEmailApi(DataTableServerSide searchModel)
        {
            var predicate = CustomPredicate.BuildPredicate<EmailAPI>(searchModel, new Type[] { typeof(EmailAPI) });

            int totalCount;
            totalCount = 0;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<EmailAPI> results = repoEmailAPI
                .Query()
                .Filter(predicate)
                //.OrderBy(x=>x.)
                //.Filter(predicate.And(a => a.IsActive && a.RoleName != Enum.GetName(typeof(RoleType),1)))
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(EmailAPI) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<EmailAPI>> resultResponse = new KeyValuePair<int, List<EmailAPI>>(totalCount, results);

            return resultResponse;
        }

       
        public EmailAPI EmailApiList()
        {
            return repoEmailAPI.Query().Filter(x => x.Status == true).Get().ToList().FirstOrDefault();
        }

        

        public EmailAPI Save(EmailAPI emailAPI)
        {
            if (emailAPI.Id == 0)
            {
                emailAPI.Addeddate = DateTime.Now;
                repoEmailAPI.Insert(emailAPI);
            }
            else
            {
                emailAPI.updatedate = DateTime.Now;
                repoEmailAPI.Update(emailAPI);
            }
            return emailAPI;
        }
        #endregion

        #region "Dispose"
        public void Dispose()
        {
            if (repoEmailAPI != null)
            {
                repoEmailAPI.Dispose();
                repoEmailAPI = null;
            }
        }
        #endregion
    }
}
