using System;
using System.Collections.Generic;
using System.Linq;
using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Repo;
namespace DhruvEnterprises.Service
{
    public class SmsApiService : ISmsApiService
    {
        #region "Fields"
        private IRepository<SmsAPI> repoSmsAPI;
        private IRepository<SMSData> repoSmsData;
        #endregion

        #region "Cosntructor"
        public SmsApiService(IRepository<SmsAPI> _repoSmsAPI, IRepository<SMSData> _repoSmsData)
        {
            this.repoSmsAPI = _repoSmsAPI;
            this.repoSmsData = _repoSmsData;
        }
        #endregion

        #region "Methods"

        public SmsAPI GetSMSById(int id = 0)
        {
            return repoSmsAPI.FindById(id);
        }

        public KeyValuePair<int, List<SmsAPI>> GetSmsApi(DataTableServerSide searchModel)
        {
            var predicate = CustomPredicate.BuildPredicate<SmsAPI>(searchModel, new Type[] { typeof(SmsAPI) });

            int totalCount;
            totalCount = 0;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<SmsAPI> results = repoSmsAPI
                .Query()
                .Filter(predicate)
                //.OrderBy(x=>x.)
                //.Filter(predicate.And(a => a.IsActive && a.RoleName != Enum.GetName(typeof(RoleType),1)))
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(SmsAPI) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<SmsAPI>> resultResponse = new KeyValuePair<int, List<SmsAPI>>(totalCount, results);

            return resultResponse;
        }

        public KeyValuePair<int, List<SMSData>> GetSmsData(Core.DataTableServerSide searchModel)
        {
            var predicate = Core.CustomPredicate.BuildPredicate<SMSData>(searchModel, new Type[] { typeof(SmsAPI) });

            int totalCount;
            totalCount = 0;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<SMSData> results = repoSmsData
                .Query()
               .Filter(predicate)
                //.Filter(predicate.And(a => a.IsActive && a.RoleName != Enum.GetName(typeof(RoleType),1)))
                //.CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(SmsAPI) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<SMSData>> resultResponse = new KeyValuePair<int, List<SMSData>>(totalCount, results);

            return resultResponse;
        }

        public SmsAPI SmsApiList()
        {
            return repoSmsAPI.Query().Filter(x => x.Status == true).Get().ToList().FirstOrDefault();
        }

        public SMSData Save(SMSData sMSData)
        {
            if (sMSData.Id == 0)
            {
                repoSmsData.Insert(sMSData);
            }
            else
            {
                repoSmsData.Update(sMSData);
            }
            return sMSData;
        }

        public SmsAPI Save(SmsAPI smsAPI)
        {
            if (smsAPI.Id == 0)
            {
                smsAPI.Addeddate = DateTime.Now;
                repoSmsAPI.Insert(smsAPI);
            }
            else
            {

                smsAPI.updatedate = DateTime.Now;
                repoSmsAPI.Update(smsAPI);
            }
            return smsAPI;
        }
        #endregion

        #region "Dispose"
        public void Dispose()
        {
            if (repoSmsAPI != null)
            {
                repoSmsAPI.Dispose();
                repoSmsAPI = null;
            }
        }
        #endregion
    }
}
