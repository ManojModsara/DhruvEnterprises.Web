using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Repo;
using DhruvEnterprises.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public class WalletService : IWalletService
    {
        #region "Fields"
        private IRepository<User> repoAdminUser;
        private IRepository<Role> repoRole;
        private IRepository<Menu> repoMenu;
        private IRepository<TxnLedger> repoTxnLedger;
        private IRepository<WalletRequest> repoWalletRequest;
        #endregion

        #region "Cosntructor"
        public WalletService(IRepository<User> _repoUserMaster, IRepository<TxnLedger> _repoTxnLedger, IRepository<Role> _repoAdminRole, IRepository<Menu> _repoMenu, IRepository<WalletRequest> _repoWalletRequest)
        {
            this.repoAdminUser = _repoUserMaster;
            this.repoRole = _repoAdminRole;
            this.repoMenu = _repoMenu;
            this.repoTxnLedger = _repoTxnLedger;
            this.repoWalletRequest = _repoWalletRequest;
        }
        #endregion

        public TxnLedger GetTxnLedger(long TxnId)
        {
            return repoTxnLedger.FindById(TxnId);
        }

        public List<User> GetWalletUserList(int RoleID)
        {
            return repoAdminUser.Query().Filter(x => x.RoleId == RoleID && x.IsActive).Get().ToList();
        }

        public KeyValuePair<int, List<TxnLedger>> GetTxnReport(DataTableServerSide searchModel, long recid, long txnid, int txntypeid, int amttypeid, int apiid, int userid, string sdate, string edate, string remark)
        {
            #region "set filter predicate"
            var predicate = CustomPredicate.BuildPredicate<TxnLedger>(searchModel, new Type[] { typeof(TxnLedger) });

            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(sdate) ? sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(edate) ? edate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(edate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;
            
            predicate = recid == 0 ? predicate : predicate.And(x => x.RecId == recid);
            predicate = txnid == 0 ? predicate : predicate.And(x => x.Id == txnid);
            predicate = txntypeid == 0 ? predicate : predicate.And(x => x.TxnTypeId == txntypeid);
            predicate = amttypeid == 0 ? predicate : predicate.And(x => x.AmtTypeId == amttypeid);
            predicate = apiid == 0 ? predicate : predicate.And(x => x.Recharge.ApiId == apiid);
            predicate = userid == 0 ? predicate : predicate.And(x => x.UserId == userid);
            predicate = string.IsNullOrEmpty(remark) ? predicate : predicate.And(x => x.Remark.Contains(remark));
            predicate = string.IsNullOrEmpty(sdate) ? predicate : predicate.And(x => x.TxnDate >= fdate);
            predicate = string.IsNullOrEmpty(edate) ? predicate : predicate.And(x => x.TxnDate <= tdate);

            #endregion
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<TxnLedger> results = repoTxnLedger
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(TxnLedger) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<TxnLedger>> resultResponse = new KeyValuePair<int, List<TxnLedger>>(totalCount, results);

            return resultResponse;
        }
        public KeyValuePair<int, List<TxnLedger>> GetRefundReport(DataTableServerSide searchModel, long recid, long txnid, int txntypeid, int amttypeid, int apiid, int userid, string sdate, string edate, string remark)
        {
            #region "set filter predicate"
            var predicate = CustomPredicate.BuildPredicate<TxnLedger>(searchModel, new Type[] { typeof(TxnLedger) });

            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(sdate) ? sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(edate) ? edate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(edate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

            predicate = recid == 0 ? predicate : predicate.And(x => x.RecId == recid);
            predicate = txnid == 0 ? predicate : predicate.And(x => x.Id == txnid);
            predicate = txntypeid == 0 ? predicate : predicate.And(x => x.TxnTypeId == txntypeid);
            predicate = amttypeid == 0 ? predicate : predicate.And(x => x.AmtTypeId == amttypeid);
            predicate = apiid == 0 ? predicate : predicate.And(x => x.Recharge.ApiId == apiid);
            predicate =  predicate.And(x => x.Recharge.StatusId == 10);
            predicate = userid == 0 ? predicate : predicate.And(x => x.UserId == userid);
            predicate = string.IsNullOrEmpty(remark) ? predicate : predicate.And(x => x.Remark.Contains(remark));
            predicate = string.IsNullOrEmpty(sdate) ? predicate : predicate.And(x => x.TxnDate >= fdate);
            predicate = string.IsNullOrEmpty(edate) ? predicate : predicate.And(x => x.TxnDate <= tdate);

            #endregion

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<TxnLedger> results = repoTxnLedger
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(TxnLedger) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<TxnLedger>> resultResponse = new KeyValuePair<int, List<TxnLedger>>(totalCount, results);

            return resultResponse;
        }

        public TxnLedger Save(TxnLedger txnLedger)
        {
            txnLedger.TxnDate = DateTime.Now;
            if (txnLedger.Id == 0)
            {
                repoTxnLedger.Insert(txnLedger);
            }
            else
            {
                repoTxnLedger.Update(txnLedger);
            }
            return txnLedger;
        }

        public WalletRequest Save(WalletRequest walletRequest)
        {
           
            if (walletRequest.Id == 0)
            {
                walletRequest.AddedDate = DateTime.Now;
                repoWalletRequest.Insert(walletRequest);
            }
            else
            {
                walletRequest.UpdatedDate = DateTime.Now;
                repoWalletRequest.Update(walletRequest);
            }
            return walletRequest;
        }
        
        public bool AddWalletTopUp(int TxnId,int WalletTxnid)
        {
           TxnLedger txnLedger= repoTxnLedger.FindById(TxnId);
            if(txnLedger!=null)
            {
                txnLedger.RefTxnId = "WR" + WalletTxnid;
                repoTxnLedger.Update(txnLedger);
                return true;
            }
            return false;
        }

        public bool CheckAlreadyRefund(long recid) 
        {
            return repoTxnLedger.Query().Get().Any(x=>x.RecId==recid && x.TxnTypeId==1 && x.AmtTypeId==1);
        }

        public KeyValuePair<int, List<WalletRequest>> GetWalletRequests(DataTableServerSide searchModel, int uid = 0)
        {
            var predicate = CustomPredicate.BuildPredicate<WalletRequest>(searchModel, new Type[] { typeof(WalletRequest)});
            var fdata = searchModel.filterdata;

            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(fdata.ToDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

            predicate = uid == 0 ? predicate : predicate.And(x => x.UserId == uid);
            predicate = fdata.UserId2 == 0 ? predicate : predicate.And(x => x.UpdatedById == fdata.UserId2);
            predicate = fdata.StatusId == 0 ? predicate : predicate.And(x => x.StatusId == fdata.StatusId );
            //predicate = fdata.StatusId == 0 ? predicate.And(x => x.StatusId != 5) : predicate.And(x => x.StatusId == fdata.StatusId);

            predicate = fdata.AccountId == 0 ? predicate : predicate.And(x => x.BankAccountId == fdata.AccountId);
            predicate = fdata.TrTypeId == 0 ? predicate : predicate.And(x => x.TrTypeId == fdata.TrTypeId);
            
            predicate = string.IsNullOrEmpty(fdata.RefId) ? predicate : predicate.And(x => x.Chequeno == fdata.RefId);
            predicate = string.IsNullOrEmpty(fdata.Remark) ? predicate : predicate.And(x => x.PaymentRemark == fdata.Remark);
            predicate = string.IsNullOrEmpty(fdata.Comment) ? predicate : predicate.And(x => x.Comment == fdata.Comment);

            predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.AddedDate >= fdate);
            predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.AddedDate <= tdate);

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<WalletRequest> results = repoWalletRequest
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(WalletRequest)}))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<WalletRequest>> resultResponse = new KeyValuePair<int, List<WalletRequest>>(totalCount, results);

            return resultResponse;
        }

        public WalletRequest GetWalletRequest(long wrId)
        {
            return repoWalletRequest.FindById(wrId);
        }

        public bool IsChequeNoExists(string chequeno)
        { 
            return repoWalletRequest.Query().Filter(x => x.Chequeno.Equals(chequeno.Trim() , StringComparison.InvariantCultureIgnoreCase) && (x.StatusId == 6||x.StatusId==5)).Get().Any();
        }
        
        public ICollection<TxnLedger> GetUserTxnReport(TxnFilterDto ft)
        {
            var predicate = PredicateBuilder.True<TxnLedger>();


            DateTime fdate = !string.IsNullOrEmpty(ft.sdate) ? DateTime.ParseExact(ft.sdate, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now.AddDays(-30);
            DateTime tdate = !string.IsNullOrEmpty(ft.edate) ? DateTime.ParseExact(ft.edate, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
            tdate = !string.IsNullOrEmpty(ft.edate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;           
            predicate = ft.recid == 0 ? predicate : predicate.And(x => x.RecId == ft.recid);
            predicate = ft.txnid == 0 ? predicate : predicate.And(x => x.Id == ft.txnid);
            predicate = ft.txntypeid == 0 ? predicate : predicate.And(x => x.TxnTypeId == ft.txntypeid);
            predicate = ft.amttypeid == 0 ? predicate : predicate.And(x => x.AmtTypeId == ft.amttypeid);
            predicate = ft.apiid == 0 ? predicate : predicate.And(x => x.Recharge!=null && x.Recharge.ApiId == ft.apiid);
            predicate = ft.userid == 0 ? predicate : predicate.And(x => x.UserId == ft.userid);
            predicate = string.IsNullOrEmpty(ft.remark) ? predicate : predicate.And(x => x.Remark == ft.remark);
            predicate = string.IsNullOrEmpty(ft.sdate) ? predicate : predicate.And(x => x.TxnDate >= fdate);
            predicate = string.IsNullOrEmpty(ft.edate) ? predicate : predicate.And(x => x.TxnDate <= tdate);

            return repoTxnLedger.Query().Filter(predicate).Get().ToList();
        }

        public ICollection<WalletRequest> GetWalletRequestList(int statusid=0, int userid=0)
        {
            var predicate = PredicateBuilder.True<WalletRequest>();
            predicate = statusid == 0 ? predicate : predicate.And(x => x.StatusId == statusid);
            predicate = userid == 0 ? predicate : predicate.And(x => x.UserId == userid);
            return repoWalletRequest.Query().Filter(predicate).Get().ToList();
        }
        public WalletRequest GetPendingWalletRequestList(string ChequeNo,decimal Amount,int statusid = 0, int userid = 0)
        {
            var predicate = PredicateBuilder.True<WalletRequest>();
            predicate = statusid == 0 ? predicate : predicate.And(x => x.StatusId == statusid);
            predicate = userid == 0 ? predicate : predicate.And(x => x.UserId == userid);
            predicate = string.IsNullOrEmpty(ChequeNo) ? predicate : predicate.And(x => x.Chequeno == ChequeNo);
            predicate = Amount == 0 ? predicate : predicate.And(x => x.Amount == Amount);
            //predicate = predicate.And(x => x.BankAccountId != 2);
            return repoWalletRequest.Query().Filter(predicate).Get().FirstOrDefault();
        }

        public KeyValuePair<int, List<WalletRequest>> GetWalletExchangeReport(DataTableServerSide searchModel, int uid = 0)
        {
            var predicate = CustomPredicate.BuildPredicate<WalletRequest>(searchModel, new Type[] { typeof(WalletRequest) });
            var fdata = searchModel.filterdata;

            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(fdata.ToDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

            predicate = uid == 0 ? predicate : predicate.And(x => x.UserId == uid);
            //predicate = fdata.UserId2 == 0 ? predicate : predicate.And(x => x.UpdatedById == fdata.UserId2);
            //predicate = fdata.StatusId == 0 ? predicate : predicate.And(x => x.StatusId == fdata.StatusId);
            //predicate = fdata.AccountId == 0 ? predicate : predicate.And(x => x.BankAccountId == fdata.AccountId);
            predicate = fdata.TrTypeId == 0 ? predicate : predicate.And(x => x.TrTypeId == fdata.TrTypeId);

            //predicate = string.IsNullOrEmpty(fdata.RefId) ? predicate : predicate.And(x => x.Chequeno == fdata.RefId);
            //predicate = string.IsNullOrEmpty(fdata.Remark) ? predicate : predicate.And(x => x.PaymentRemark == fdata.Remark);
            //predicate = string.IsNullOrEmpty(fdata.Comment) ? predicate : predicate.And(x => x.Comment == fdata.Comment);

            predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.AddedDate >= fdate);
            predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.AddedDate <= tdate);

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<WalletRequest> results = repoWalletRequest
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(WalletRequest) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<WalletRequest>> resultResponse = new KeyValuePair<int, List<WalletRequest>>(totalCount, results);

            return resultResponse;
        }

















        #region "Dispose"
        public void Dispose()
        {
            if (repoAdminUser != null)
            {
                repoAdminUser.Dispose();
                repoAdminUser = null;
            }
        }
        #endregion
    }
}
