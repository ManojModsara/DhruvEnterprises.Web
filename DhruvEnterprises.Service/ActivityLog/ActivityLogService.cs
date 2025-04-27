using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Repo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DhruvEnterprises.Service
{
    public class ActivityLogService : IActivityLogService
    {

        #region "Fields"
        private IRepository<ActivityLog> repoActivityLog;
       
        #endregion

        #region "Cosntructor"
        public ActivityLogService(IRepository<ActivityLog> _repoActivityLog)
        {
            this.repoActivityLog = _repoActivityLog; 

        }
        #endregion

        #region "Actions"
        /// <summary>
        /// get list of activity log
        /// </summary>
        /// <returns></returns>
        public ICollection<ActivityLog> GetActivityLog(ActiVityLogFilterDto ftr)
        {
            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(ftr.sdate) ? ftr.sdate : DateTime.Now.AddDays(-30).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(ftr.edate) ? ftr.edate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(ftr.edate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

            return repoActivityLog.Query().AsTracking()
                                  .Filter(x => (ftr.userid == 0 ? true : x.UserId == ftr.userid) &&
                                               (ftr.ipaddress == "" ? true : x.IPAddress == ftr.ipaddress) &&
                                               (ftr.actname == "" ? true : x.ActivityName.Contains(ftr.actname)) &&
                                               (ftr.url == "" ? true : x.ActivityPage.Contains(ftr.url)) &&
                                               (ftr.remark == "" ? true : x.Remark.Contains(ftr.remark)) &&
                                               (ftr.sdate == "" ? true : x.ActivityDate >= fdate) &&
                                               (ftr.edate == "" ? true : x.ActivityDate <= tdate) 
                                ).Get().ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActivityLog GetActivityLog(long id)
        {
            return repoActivityLog.FindById(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public KeyValuePair<int, List<ActivityLog>> GetActivityLogs(DataTableServerSide searchModel, ActiVityLogFilterDto ftr)
        {


            var predicate = CustomPredicate.BuildPredicate<ActivityLog>(searchModel, new Type[] { typeof(User) });


            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(ftr.sdate) ? ftr.sdate : DateTime.Now.AddDays(-1).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(ftr.edate) ? ftr.edate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(ftr.edate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;
            
            predicate = predicate.And(x => ftr.userid == 0 ? true : x.UserId == ftr.userid);
            predicate = predicate.And(x => ftr.ipaddress == "" ? true : (x.IPAddress == ftr.ipaddress));
            predicate = predicate.And(x => ftr.actname == "" ? true : (x.ActivityName.Contains(ftr.actname)));
            predicate = predicate.And(x => ftr.url == "" ? true : (x.ActivityPage.Contains(ftr.url)));
            predicate = predicate.And(x => ftr.remark == "" ? true : (x.Remark.Contains(ftr.remark)));
            predicate = predicate.And(x => ftr.sdate == "" ? true : x.ActivityDate >= fdate);
            predicate = predicate.And(x => ftr.edate == "" ? true : x.ActivityDate <= tdate);
            
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<ActivityLog> results = repoActivityLog
                .Query()
                .Filter(predicate) //a.IsDeleted == false &&
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(User) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<ActivityLog>> resultResponse = new KeyValuePair<int, List<ActivityLog>>(totalCount, results);

            return resultResponse;
        }

        /// <summary>
        /// Add or update activity log
        /// </summary>
        /// <param name="activityLog"></param>
        /// <returns></returns>
        public ActivityLog Save(ActivityLog activityLog)
        {
            activityLog.ActivityDate = DateTime.Now;
            if (activityLog.Id == 0)
            {
                repoActivityLog.Insert(activityLog);
            }
            else
            {
                repoActivityLog.Update(activityLog);
            }
            return activityLog;
        }
        #endregion

        #region "Dispose"
        public void Dispose()
        {
            if (repoActivityLog != null)
            {
                repoActivityLog.Dispose();
                repoActivityLog = null;
            }
        }
        #endregion
    }
}
