using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Repo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public class ApiWalletService: IApiWalletService
    {

        #region "Fields"
        private IRepository<ApiWalletTxn> repoApiWalletTxn;
        private IRepository<User> repoUser; 
        private IRepository<ApiSource> repoApiSource;
        private IRepository<TxnLedger> repoTxnLedger;

        #endregion

        #region "Cosntructor"
        public ApiWalletService(
            
            IRepository<ApiWalletTxn> _repoApiWalletTxn,
             IRepository<User>      _repoUser,
             IRepository<ApiSource> _repoApiSource,
             IRepository<TxnLedger> _repoTxnLedger
            )
        {
            this.repoApiWalletTxn = _repoApiWalletTxn;
            this.repoUser= _repoUser;
            this.repoApiSource= _repoApiSource;
            this.repoTxnLedger= _repoTxnLedger;
        }
        #endregion

        #region "Methods"

        public ApiWalletTxn GetApiWalletTxn(long id)
        {
            return repoApiWalletTxn.FindById(id);
        }

        public ApiWalletTxn GetApiWalletByRef(long reftxnid)
        {
            return repoApiWalletTxn.Query().Filter(x=>x.RefTxnId== reftxnid).Get().ToList().FirstOrDefault();
        }


        /// <summary>
        /// Get all AdminUsers by DataTable SearchModel
        /// </summary>
        /// <param name="searchModel" type="DataTableServerSide"></param>
        /// <returns></returns>
        public KeyValuePair<int, List<ApiWalletTxn>> GetApiWalletTxns(DataTableServerSide searchModel, long recid, long txnid, int txntypeid, int amttypeid, int apiid, int userid, string sdate, string edate, string remark)
        {

            #region "set filter predicate"
            var predicate = CustomPredicate.BuildPredicate<ApiWalletTxn>(searchModel, new Type[] { typeof(ApiWalletTxn), typeof(TxnType), typeof(AmtType), typeof(ApiSource), typeof(TxnLedger), typeof(User), typeof(Recharge) });
            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(sdate) ? sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(edate) ? edate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(edate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

            predicate = predicate.And(x => (recid == 0 ? true : x.RecId == recid));
            predicate = predicate.And(x => txnid == 0 ? true : x.Id == txnid);
            predicate = predicate.And(x => txntypeid == 0 ? true : x.TxnTypeId == txntypeid);
            predicate = predicate.And(x => amttypeid == 0 ? true : x.AmtTypeId == amttypeid);
            predicate = predicate.And(x => apiid == 0 ? true : x.ApiId == apiid);
            predicate = predicate.And(x => userid == 0 ? true : x.Recharge.UserId == userid || x.User.Id== userid);
            predicate = predicate.And(x => remark == "" ? true : (x.Remark.Contains(remark)));
            predicate = predicate.And(x => sdate == "" ? true : x.TxnDate >= fdate);
            predicate = predicate.And(x => edate == "" ? true : x.TxnDate <= tdate);
            #endregion

            int totalCount; 
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<ApiWalletTxn> results = repoApiWalletTxn
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(ApiWalletTxn), typeof(TxnType), typeof(AmtType), typeof(ApiSource), typeof(TxnLedger),  typeof(User), typeof(Recharge) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<ApiWalletTxn>> resultResponse = new KeyValuePair<int, List<ApiWalletTxn>>(totalCount, results);

            return resultResponse;
        }

        /// <summary>
        /// Save/update AdminUser 
        /// </summary>
        /// <param name="AdminUser" type="AdminUser"></param>
        /// <returns></returns>

        public ApiWalletTxn Save(ApiWalletTxn apiWalletTxn)
        {
            apiWalletTxn.TxnDate = DateTime.Now;
            repoApiWalletTxn.Insert(apiWalletTxn);
            return apiWalletTxn;
        }



        public decimal GetApiWalletBalance(int apiId)
        {
            if (apiId > 0)
            {
                ApiSource apiSource = repoApiSource.FindById(apiId);

                var ApiWalletTxn = apiSource.ApiWalletTxns.OrderByDescending(t => t.Id).FirstOrDefault();
                return ApiWalletTxn != null ? ApiWalletTxn.CL_Bal ?? 0 : 0;
            }
            else
            {
                return 0;
            }
           
        }

        

        public bool CheckApiWalletTxn(long recid, int apid)
        {
            return repoApiWalletTxn.Query().Get().Any(x => x.ApiId == apid && x.RecId == recid);
        }
        
       
        #endregion

        /// <summary>
        /// Get all AdminUsers by DataTable SearchModel
        /// </summary>
        /// <param name="searchModel" type="DataTableServerSide"></param>
        /// <returns></returns>

        #region "Dispose"
        public void Dispose()
        {
            if (repoApiWalletTxn != null)
            {
                repoApiWalletTxn.Dispose();
                repoApiWalletTxn = null;
            }

            if (repoUser != null)
            {
                repoUser.Dispose();
                repoUser = null;
            }

            if (repoApiSource != null)
            {
                repoApiSource.Dispose();
                repoApiSource = null;
            }

            if (repoTxnLedger != null)
            {
                repoTxnLedger.Dispose();
                repoTxnLedger = null;
            }

        }
        #endregion
    }
}
