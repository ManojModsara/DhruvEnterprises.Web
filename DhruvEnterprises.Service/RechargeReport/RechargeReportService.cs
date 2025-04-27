using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Repo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public class RechargeReportService : IRechargeReportService
    {
        #region "Fields"
        private IRepository<User> repoUser;
        private IRepository<ApiUrl> repoApiUrl;
        private IRepository<Recharge> repoRecharge;
        private IRepository<DMT> repoDmt;
        private IRepository<RequestResponse> repoRequestResponse;
        private IRepository<StatusType> repoStatusType;
        private IRepository<TxnType> repoTxnType;
        private IRepository<AmtType> repoAmtType;
        private IRepository<Complaint> repoComplaint;
        private IRepository<OperatorKeyGenrate> repoOperatorKeyGenrate;
        #endregion

        #region "Cosntructor"
        public RechargeReportService(IRepository<User> _repoUser,
                                     IRepository<ApiUrl> _repoApiUrl,
                                     IRepository<Recharge> _repoRecharge,
                                     IRepository<RequestResponse> _repoRequestResponse,
                                     IRepository<StatusType> _repoStatusType,
                                     IRepository<TxnType> _repoTxnType,
                                     IRepository<AmtType> _repoAmtType,
                                     IRepository<Complaint> _repoComplaint,
                                     IRepository<DMT> _repository,
              IRepository<OperatorKeyGenrate> _repoOperatorKeyGenrate)
        {
            this.repoUser = _repoUser;
            this.repoApiUrl = _repoApiUrl;
            this.repoRecharge = _repoRecharge;
            this.repoRequestResponse = _repoRequestResponse;
            this.repoStatusType = _repoStatusType;
            this.repoTxnType = _repoTxnType;
            this.repoAmtType = _repoAmtType;
            this.repoComplaint = _repoComplaint;
            this.repoOperatorKeyGenrate = _repoOperatorKeyGenrate;
            this.repoDmt = _repository;
        }
        #endregion

        #region Method
        public ICollection<OperatorKeyGenrate> GetOperatorKeyGenrateList(int opid, int userid, int vendorid)
        {
            return repoOperatorKeyGenrate.Query().Filter(x => x.OpId == opid && x.Userid == userid && x.VendorId == vendorid).Get().ToList();
        }
        public KeyValuePair<int, List<Recharge>> GetRechargeReport(DataTableServerSide searchModel, int uid, int apiid, int opid, byte statusid, string SearchId, string sdate, string edate, int cid, string cno, string ut, string vt, string ot, int uid2, ref string log)
        {
            var fdata = searchModel.filterdata;
            fdata.ToDate = fdata.EndDate;
            var predicate = CustomPredicate.BuildPredicate<Recharge>(searchModel, new Type[] { typeof(Recharge) });

            if (statusid == 11)
            {

                DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(sdate) ? sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(edate) ? edate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                tdate = !string.IsNullOrEmpty(edate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

                SearchId = !string.IsNullOrEmpty(SearchId) ? SearchId : "";
                long searchid = 0;
                try { searchid = !string.IsNullOrEmpty(SearchId) ? Convert.ToInt64(SearchId) : 0; } catch { }

                predicate = apiid == 0 ? predicate : predicate.And(x => x.ApiId == apiid);
                predicate = opid == 0 ? predicate : predicate.And(x => x.OpId == opid);
                predicate = predicate.And(x => (x.StatusId == 2 || x.StatusId == 4 || x.StatusId == 5));
                predicate = uid == 0 ? predicate : predicate.And(x => x.UserId == uid);
                //predicate = statusid == 0 ? predicate : predicate.And(x => x.StatusId == statusid);
                predicate = cid == 0 ? predicate : predicate.And(x => x.CircleId == cid);
                predicate = string.IsNullOrEmpty(cno) ? predicate : predicate.And(x => x.CustomerNo == cno);
                predicate = string.IsNullOrEmpty(ot) ? predicate : predicate.And(x => x.OptTxnId != null && x.OptTxnId == ot);
                predicate = string.IsNullOrEmpty(ut) ? predicate : predicate.And(x => x.UserTxnId == ut);
                predicate = string.IsNullOrEmpty(vt) ? predicate : predicate.And(x => x.ApiTxnId != null && x.ApiTxnId == vt);
                predicate = string.IsNullOrEmpty(SearchId) || searchid > 0 ? predicate : predicate.And(x => x.OurRefTxnId == SearchId);
                predicate = searchid == 0 ? predicate : predicate.And(x => (x.Id == searchid || x.TxnId == searchid));

                if (uid2 > 0)
                {
                    predicate = uid2 == 0 ? predicate : predicate.And(x => x.UpdatedById == uid2);
                    predicate = string.IsNullOrEmpty(sdate) ? predicate : predicate.And(x => x.UpdatedDate != null && x.UpdatedDate >= fdate);
                    predicate = string.IsNullOrEmpty(edate) ? predicate : predicate.And(x => x.UpdatedDate != null && x.UpdatedDate <= tdate);
                }
                else
                {
                    predicate = string.IsNullOrEmpty(sdate) ? predicate : predicate.And(x => x.RequestTime >= fdate);
                    predicate = string.IsNullOrEmpty(edate) ? predicate : predicate.And(x => x.RequestTime <= tdate);
                }

            }
            else
            {
                fdata.FromDate = Convert.ToDateTime(fdata.FromDate).ToString("dd/MM/yyyy");
                fdata.ToDate = Convert.ToDateTime(fdata.ToDate).ToString("dd/MM/yyyy");
                DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.AddDays(-30).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                tdate = !string.IsNullOrEmpty(fdata.ToDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

                SearchId = !string.IsNullOrEmpty(fdata.RefId) ? fdata.RefId : "";
                long searchid = 0;
                try { searchid = !string.IsNullOrEmpty(SearchId) ? Convert.ToInt64(SearchId) : 0; } catch { }

                predicate = fdata.ApiId == 0 ? predicate : predicate.And(x => x.ApiId == fdata.ApiId);
                predicate = fdata.OpId == 0 ? predicate : predicate.And(x => x.OpId == fdata.OpId);

                predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.UserId == fdata.UserId);

                if (fdata.StatusId == 11)
                    predicate = predicate.And(x => (x.UserId != x.UpdatedById) && (x.UpdatedById != null || x.ResendById != null));
                else
                    predicate = fdata.StatusId == 0 ? predicate : predicate.And(x => x.StatusId == fdata.StatusId);

                predicate = fdata.CircleId == 0 ? predicate : predicate.And(x => x.CircleId == fdata.CircleId);
                predicate = string.IsNullOrEmpty(fdata.CustomerNo) ? predicate : predicate.And(x => x.CustomerNo == fdata.CustomerNo);
                predicate = string.IsNullOrEmpty(fdata.OpTxnId) ? predicate : predicate.And(x => x.OptTxnId != null && x.OptTxnId == fdata.OpTxnId);
                predicate = string.IsNullOrEmpty(fdata.UserTxnId) ? predicate : predicate.And(x => x.UserTxnId == fdata.UserTxnId);
                predicate = string.IsNullOrEmpty(fdata.ApiTxnId) ? predicate : predicate.And(x => x.ApiTxnId != null && x.ApiTxnId == fdata.ApiTxnId);

                predicate = string.IsNullOrEmpty(SearchId) || searchid > 0 ? predicate : predicate.And(x => x.OurRefTxnId == SearchId);

                predicate = searchid == 0 ? predicate : predicate.And(x => (x.Id == searchid || x.TxnId == searchid));

                if (fdata.UserId2 > 0)
                {
                    predicate = fdata.UserId2 == 0 ? predicate : predicate.And(x => x.UpdatedById == fdata.UserId2);
                    predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.UpdatedDate >= fdate);
                    predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.UpdatedDate <= tdate);
                }
                else
                {
                    predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.RequestTime >= fdate);
                    predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.RequestTime <= tdate);
                }
            }

            int totalCount;

            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<Recharge> results = repoRecharge
                .Query()
                 .Filter(predicate)
                .CustomOrderBy(u => u.OrderByDescending(x => x.Id))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<Recharge>> resultResponse = new KeyValuePair<int, List<Recharge>>(totalCount, results);

            return resultResponse;
        }

        public KeyValuePair<int, List<OperatorKeyGenrate>> GetOperatorKeyGenrateReport(DataTableServerSide searchModel, int uid, int apiid, int opid)
        {
            var fdata = searchModel.filterdata;
            fdata.ToDate = fdata.EndDate;
            var predicate = CustomPredicate.BuildPredicate<OperatorKeyGenrate>(searchModel, new Type[] { typeof(OperatorKeyGenrate) });

            predicate = fdata.ApiId == 0 ? predicate : predicate.And(x => x.VendorId == fdata.ApiId);
            predicate = fdata.OpId == 0 ? predicate : predicate.And(x => x.OpId == fdata.OpId);

            predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.Userid == fdata.UserId);

            int totalCount;

            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<OperatorKeyGenrate> results = repoOperatorKeyGenrate
                .Query()
                 .Filter(predicate)
                .CustomOrderBy(u => u.OrderByDescending(x => x.Id))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<OperatorKeyGenrate>> resultResponse = new KeyValuePair<int, List<OperatorKeyGenrate>>(totalCount, results);

            return resultResponse;
        }

        public KeyValuePair<int, List<Recharge>> GetProcessingRecharge(DataTableServerSide searchModel, RechargeFilterDto ft)
        {
            var fdata = searchModel.filterdata;
            var predicate = CustomPredicate.BuildPredicate<Recharge>(searchModel, new Type[] { typeof(Recharge) });
            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(fdata.ToDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;
            fdata.RefId = !string.IsNullOrEmpty(fdata.RefId) ? fdata.RefId : string.Empty;
            long searchid = 0;
            try { searchid = !string.IsNullOrEmpty(fdata.RefId) ? Convert.ToInt64(fdata.RefId) : 0; } catch { }
            predicate = fdata.ApiId == 0 ? predicate : predicate.And(x => x.ApiId == fdata.ApiId);
            predicate = fdata.OpId == 0 ? predicate : predicate.And(x => x.OpId == fdata.OpId);
            predicate = predicate.And(x => (x.StatusId == 2));
            predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.UserId == fdata.UserId);
            predicate = fdata.CircleId == 0 ? predicate : predicate.And(x => x.CircleId == fdata.CircleId);
            predicate = string.IsNullOrEmpty(fdata.CustomerNo) ? predicate : predicate.And(x => x.CustomerNo == fdata.CustomerNo);
            predicate = string.IsNullOrEmpty(fdata.OpTxnId) ? predicate : predicate.And(x => x.OptTxnId == fdata.OpTxnId);
            predicate = string.IsNullOrEmpty(fdata.UserTxnId) ? predicate : predicate.And(x => x.UserTxnId == fdata.UserTxnId);
            predicate = string.IsNullOrEmpty(fdata.ApiTxnId) ? predicate : predicate.And(x => x.ApiTxnId == fdata.ApiTxnId);
            predicate = string.IsNullOrEmpty(fdata.RefId) || searchid > 0 ? predicate : predicate.And(x => x.OurRefTxnId == fdata.RefId);
            predicate = searchid == 0 ? predicate : predicate.And(x => (x.Id == searchid || x.TxnId == searchid));
            predicate = fdata.IsResentOnly == 0 ? predicate : predicate.And(x => x.ResendTime != null);
            if (fdata.UserId2 > 0)
            {
                predicate = fdata.UserId2 == 0 ? predicate : predicate.And(x => x.UpdatedById == fdata.UserId2);
                predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.UpdatedDate >= fdate);
                predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.UpdatedDate <= tdate);
            }
            else
            {
                predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.RequestTime >= fdate);
                predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.RequestTime <= tdate);
            }
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);
            List<Recharge> results = repoRecharge
    .Query()
     .Filter(predicate)
    .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(Recharge) }))
    .GetPage(page, searchModel.length, out totalCount)
    .ToList();
            KeyValuePair<int, List<Recharge>> resultResponse = new KeyValuePair<int, List<Recharge>>(totalCount, results);
            return resultResponse;
        }

        #region dmt 
        public KeyValuePair<int, List<DMT>> GetProcessingDMT(DataTableServerSide searchModel, RechargeFilterDto ft)
        {
            var fdata = searchModel.filterdata;
            var predicate = CustomPredicate.BuildPredicate<DMT>(searchModel, new Type[] { typeof(DMT) });
            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(fdata.ToDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;
            fdata.RefId = !string.IsNullOrEmpty(fdata.RefId) ? fdata.RefId : string.Empty;
            long searchid = 0;
            try { searchid = !string.IsNullOrEmpty(fdata.RefId) ? Convert.ToInt64(fdata.RefId) : 0; } catch { }
            predicate = fdata.ApiId == 0 ? predicate : predicate.And(x => x.ApiId == fdata.ApiId);
            predicate = fdata.OpId == 0 ? predicate : predicate.And(x => x.OpId == fdata.OpId);
            predicate = predicate.And(x => (x.StatusId == 2));
            predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.UserId == fdata.UserId);
            predicate = string.IsNullOrEmpty(fdata.CustomerNo) ? predicate : predicate.And(x => x.BeneMobile == fdata.CustomerNo);
            predicate = string.IsNullOrEmpty(fdata.OpTxnId) ? predicate : predicate.And(x => x.OptTxnId == fdata.OpTxnId);
            predicate = string.IsNullOrEmpty(fdata.UserTxnId) ? predicate : predicate.And(x => x.UserTxnId == fdata.UserTxnId);
            predicate = string.IsNullOrEmpty(fdata.ApiTxnId) ? predicate : predicate.And(x => x.ApiTxnId == fdata.ApiTxnId);
            predicate = string.IsNullOrEmpty(fdata.RefId) || searchid > 0 ? predicate : predicate.And(x => x.OurRefTxnId == fdata.RefId);
            predicate = searchid == 0 ? predicate : predicate.And(x => (x.Id == searchid || x.TxnId == searchid));
            predicate = fdata.IsResentOnly == 0 ? predicate : predicate.And(x => x.ResponseTime != null);
            if (fdata.UserId2 > 0)
            {
                predicate = fdata.UserId2 == 0 ? predicate : predicate.And(x => x.UpdatedById == fdata.UserId2);
                predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.UpdatedDate >= fdate);
                predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.UpdatedDate <= tdate);
            }
            else
            {
                predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.RequestTime >= fdate);
                predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.RequestTime <= tdate);
            }
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);
            List<DMT> results = repoDmt
    .Query()
     .Filter(predicate)
    .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(DMT) }))
    .GetPage(page, searchModel.length, out totalCount)
    .ToList();
            KeyValuePair<int, List<DMT>> resultResponse = new KeyValuePair<int, List<DMT>>(totalCount, results);
            return resultResponse;
        }

        #region Recharge 
        public KeyValuePair<int, List<Recharge>> GetPendingrecharge(DataTableServerSide searchModel, RechargeFilterDto ft)
        {
            var fdata = searchModel.filterdata;
            var predicate = CustomPredicate.BuildPredicate<Recharge>(searchModel, new Type[] { typeof(Recharge) });
            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(fdata.ToDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;
            fdata.RefId = !string.IsNullOrEmpty(fdata.RefId) ? fdata.RefId : string.Empty;
            long searchid = 0;
            try { searchid = !string.IsNullOrEmpty(fdata.RefId) ? Convert.ToInt64(fdata.RefId) : 0; } catch { }
            //predicate = fdata.ApiId == 0 ? predicate : predicate.And(x => x.ApiId == fdata.ApiId);
           //predicate = fdata.OpId == 0 ? predicate : predicate.And(x => x.OpId == fdata.OpId);
            predicate = predicate.And(x => (x.StatusId == 2));
            predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.UserId == fdata.UserId);
            predicate = string.IsNullOrEmpty(fdata.CustomerNo) ? predicate : predicate.And(x => x.CustomerNo == fdata.CustomerNo);
            predicate = string.IsNullOrEmpty(fdata.OpTxnId) ? predicate : predicate.And(x => x.OptTxnId == fdata.OpTxnId);
            predicate = string.IsNullOrEmpty(fdata.UserTxnId) ? predicate : predicate.And(x => x.UserTxnId == fdata.UserTxnId);
            predicate = string.IsNullOrEmpty(fdata.ApiTxnId) ? predicate : predicate.And(x => x.ApiTxnId == fdata.ApiTxnId);
            predicate = string.IsNullOrEmpty(fdata.RefId) || searchid > 0 ? predicate : predicate.And(x => x.OurRefTxnId == fdata.RefId);
            predicate = searchid == 0 ? predicate : predicate.And(x => (x.Id == searchid || x.TxnId == searchid));
            predicate = fdata.IsResentOnly == 0 ? predicate : predicate.And(x => x.ResponseTime != null);
            if (fdata.UserId2 > 0)
            {
                predicate = fdata.UserId2 == 0 ? predicate : predicate.And(x => x.UpdatedById == fdata.UserId2);
                predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.UpdatedDate >= fdate);
                predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.UpdatedDate <= tdate);
            }
            else
            {
                predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.RequestTime >= fdate);
                predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.RequestTime <= tdate);
            }
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);
            List<Recharge> results = repoRecharge
              .Query()
               .Filter(predicate)
              .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(Recharge) }))
              .GetPage(page, searchModel.length, out totalCount)
              .ToList();
                      KeyValuePair<int, List<Recharge>> resultResponse = new KeyValuePair<int, List<Recharge>>(totalCount, results);
                      return resultResponse;
            }
        #endregion
        public Recharge GetRecharge(long recid)
        {
            return repoRecharge.FindById(recid);
        }
        
        public Recharge Save(Recharge adminUser)
        {
            adminUser.UpdatedDate = DateTime.Now;
            if (adminUser.Id == 0)
            {
                repoRecharge.Insert(adminUser);
            }
            else
            {
                repoRecharge.Update(adminUser);
            }
            return adminUser;
        }

        public ICollection<StatusType> GetStatusList()
        {
            return repoStatusType.Query().Get().OrderBy(x => x.TypeName).ToList();
        }

        public ICollection<Recharge> GetRechargeList(string sdate, string edate, byte StatusId, int UserId = 0, int ApiId = 0, string rto = "0", int OpId = 0, int cid = 0, string cno = "0", string ut = "0", string vt = "0", string ot = "0", int u2 = 0)
        {
            UserId = UserId == 1 ? 0 : UserId;
            DateTime fdate = !string.IsNullOrEmpty(sdate) ? DateTime.ParseExact(sdate, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now.AddDays(-30);
            DateTime tdate = !string.IsNullOrEmpty(edate) ? DateTime.ParseExact(edate, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
            tdate = !string.IsNullOrEmpty(edate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

            rto = !string.IsNullOrEmpty(rto) ? rto : "0";
            rto = !string.IsNullOrEmpty(rto) ? rto : "0";
            long searchid = 0;
            try { searchid = !string.IsNullOrEmpty(rto) ? Convert.ToInt64(rto) : 0; } catch { }

            var predicate = PredicateBuilder.True<Recharge>();

            predicate = UserId == 0 ? predicate : predicate.And(x => x.UserId == UserId);
            predicate = ApiId == 0 ? predicate : predicate.And(x => x.ApiId == ApiId);
            //predicate = StatusId == 0 ? predicate.And(x => x.StatusId != 4) : predicate.And(x => x.StatusId == StatusId);
            predicate = OpId == 0 ? predicate : predicate.And(x => x.OpId == OpId);
            predicate = cid == 0 ? predicate : predicate.And(x => x.CircleId == cid);
            predicate = cno == "0" || cno == "" ? predicate : predicate.And(x => x.CustomerNo == cno);
            predicate = ut == "0" || ut == "" ? predicate : predicate.And(x => x.UserTxnId == ut);
            predicate = vt == "0" || vt == "" ? predicate : predicate.And(x => x.ApiTxnId == vt);
            predicate = ot == "0" || ot == "" ? predicate : predicate.And(x => x.OptTxnId == ot);
            predicate = rto == "0" || rto == "" ? predicate : predicate.And(x => (x.Id == searchid || x.TxnId == searchid || x.OurRefTxnId == rto));
            if (u2 > 0)
            {
                predicate = u2 == 0 ? predicate : predicate.And(x => x.UpdatedById == u2);
                predicate = sdate == "" ? predicate : predicate.And(x => x.UpdatedDate >= fdate);
                predicate = edate == "" ? predicate : predicate.And(x => x.UpdatedDate <= tdate);
            }
            else
            {
                predicate = sdate == "" ? predicate : predicate.And(x => x.RequestTime >= fdate);
                predicate = edate == "" ? predicate : predicate.And(x => x.RequestTime <= tdate);
            }

            return repoRecharge.Query().Filter(predicate).Get().ToList();

        }

        #endregion

        public KeyValuePair<int, List<DMT>> GetDMTReport(DataTableServerSide searchModel, int uid, int apiid, int opid, byte statusid, string SearchId, string sdate, string edate, int cid, string cno, string ut, string vt, string ot, int uid2, ref string log)
        {

            var fdata = searchModel.filterdata;
            fdata.ToDate = fdata.EndDate;
            var predicate = CustomPredicate.BuildPredicate<DMT>(searchModel, new Type[] { typeof(DMT) });

            if (statusid == 11)
            {

                DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(sdate) ? sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(edate) ? edate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                tdate = !string.IsNullOrEmpty(edate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

                SearchId = !string.IsNullOrEmpty(SearchId) ? SearchId : "";
                long searchid = 0;
                try { searchid = !string.IsNullOrEmpty(SearchId) ? Convert.ToInt64(SearchId) : 0; } catch { }

                predicate = apiid == 0 ? predicate : predicate.And(x => x.ApiId == apiid);
                predicate = predicate.And(x => x.OpId == 3 && x.OpId == 4);
                predicate = predicate.And(x => (x.StatusId == 2 || x.StatusId == 4 || x.StatusId == 5));
                predicate = uid == 0 ? predicate : predicate.And(x => x.UserId == uid);
                //predicate = statusid == 0 ? predicate : predicate.And(x => x.StatusId == statusid);
                //predicate = cid == 0 ? predicate : predicate.And(x => x.CircleId == cid);
                predicate = string.IsNullOrEmpty(cno) ? predicate : predicate.And(x => x.BeneMobile == cno);
                predicate = string.IsNullOrEmpty(ot) ? predicate : predicate.And(x => x.OptTxnId != null && x.OptTxnId == ot);
                predicate = string.IsNullOrEmpty(ut) ? predicate : predicate.And(x => x.UserTxnId == ut);
                predicate = string.IsNullOrEmpty(vt) ? predicate : predicate.And(x => x.ApiTxnId != null && x.ApiTxnId == vt);
                predicate = string.IsNullOrEmpty(SearchId) || searchid > 0 ? predicate : predicate.And(x => x.OurRefTxnId == SearchId);
                predicate = searchid == 0 ? predicate : predicate.And(x => (x.Id == searchid || x.TxnId == searchid));

                if (uid2 > 0)
                {
                    predicate = uid2 == 0 ? predicate : predicate.And(x => x.UpdatedById == uid2);
                    predicate = string.IsNullOrEmpty(sdate) ? predicate : predicate.And(x => x.UpdatedDate != null && x.UpdatedDate >= fdate);
                    predicate = string.IsNullOrEmpty(edate) ? predicate : predicate.And(x => x.UpdatedDate != null && x.UpdatedDate <= tdate);
                }
                else
                {
                    predicate = string.IsNullOrEmpty(sdate) ? predicate : predicate.And(x => x.RequestTime >= fdate);
                    predicate = string.IsNullOrEmpty(edate) ? predicate : predicate.And(x => x.RequestTime <= tdate);
                }

            }
            else
            {
                DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.AddDays(-30).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                tdate = !string.IsNullOrEmpty(fdata.ToDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

                SearchId = !string.IsNullOrEmpty(fdata.RefId) ? fdata.RefId : "";
                long searchid = 0;
                try { searchid = !string.IsNullOrEmpty(SearchId) ? Convert.ToInt64(SearchId) : 0; } catch { }

                predicate = fdata.ApiId == 0 ? predicate : predicate.And(x => x.ApiId == fdata.ApiId);
                predicate = predicate.And(x => x.OpId == 3 || x.OpId == 4);

                predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.UserId == fdata.UserId);

                if (fdata.StatusId == 11)
                    predicate = predicate.And(x => (x.UserId != x.UpdatedById) && (x.UpdatedById != null /*|| x.ResendById != null*/));
                else
                    predicate = fdata.StatusId == 0 ? predicate : predicate.And(x => x.StatusId == fdata.StatusId);

                //predicate = fdata.CircleId == 0 ? predicate : predicate.And(x => x.CircleId == fdata.CircleId);
                predicate = string.IsNullOrEmpty(fdata.CustomerNo) ? predicate : predicate.And(x => x.BeneMobile == fdata.CustomerNo);
                predicate = string.IsNullOrEmpty(fdata.OpTxnId) ? predicate : predicate.And(x => x.OptTxnId != null && x.OptTxnId == fdata.OpTxnId);
                predicate = string.IsNullOrEmpty(fdata.UserTxnId) ? predicate : predicate.And(x => x.UserTxnId == fdata.UserTxnId);
                predicate = string.IsNullOrEmpty(fdata.ApiTxnId) ? predicate : predicate.And(x => x.ApiTxnId != null && x.ApiTxnId == fdata.ApiTxnId);

                predicate = string.IsNullOrEmpty(SearchId) || searchid > 0 ? predicate : predicate.And(x => x.OurRefTxnId == SearchId);

                predicate = searchid == 0 ? predicate : predicate.And(x => (x.Id == searchid || x.TxnId == searchid));

                if (fdata.UserId2 > 0)
                {
                    predicate = fdata.UserId2 == 0 ? predicate : predicate.And(x => x.UpdatedById == fdata.UserId2);
                    predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.UpdatedDate >= fdate);
                    predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.UpdatedDate <= tdate);
                }
                else
                {
                    predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.RequestTime >= fdate);
                    predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.RequestTime <= tdate);
                }
            }

            int totalCount;

            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<DMT> results = repoDmt
                .Query()
                 .Filter(predicate)
                .CustomOrderBy(u => u.OrderByDescending(x => x.Id))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<DMT>> resultResponse = new KeyValuePair<int, List<DMT>>(totalCount, results);

            return resultResponse;
        }
        public ICollection<TxnType> GetTxnTypes(string remark = "")
        {
            return repoTxnType.Query().Filter(x => x.Remark.Contains(remark) || remark == "").Get().ToList();

        }

        public ICollection<AmtType> GetAmtTypes(string remark = "")
        {
            return repoAmtType.Query().Filter(x => remark == "" ? true : x.Remark.Contains(remark)).Get().ToList();

        }

        public Complaint GetComplaint(long compid)
        {
            return repoComplaint.FindById(compid);
        }

        public ICollection<Complaint> GetComplaintByRecId(long recid)
        {
            return repoComplaint.Query().Filter(x => x.RecId == recid).Get().ToList();
        }

        public Complaint Save(Complaint complaint)
        {
            if (complaint.Id == 0)
            {
                var existingcomplain = repoComplaint.Query().Filter(x => x.RecId == complaint.RecId && x.StatusId == 5).Get().FirstOrDefault();
                if (existingcomplain == null)
                {
                    complaint.ComplaintDate = DateTime.Now;
                    repoComplaint.Insert(complaint);
                }
            }
            else
            {
                complaint.UpdatedDate = DateTime.Now;
                repoComplaint.Update(complaint);
            }
            return complaint;
        }

        public OperatorKeyGenrate OperatorKeyGenrateGetData(int opid, int userid, int vendorid,int KeyTypeId)
        {
            return repoOperatorKeyGenrate.Query().Filter(x => x.OpId == opid && x.Userid == userid && x.VendorId == vendorid && x.KeyTypeId== KeyTypeId).Get().FirstOrDefault();
        }
        public OperatorKeyGenrate Save(OperatorKeyGenrate operatorKeyGenrate)
        {
            if (operatorKeyGenrate.Id == 0)
            {
                operatorKeyGenrate.AddedByDate = DateTime.Now;
                repoOperatorKeyGenrate.Insert(operatorKeyGenrate);
            }
            else
            {
                operatorKeyGenrate.UpdatedByDate = DateTime.Now;
                repoOperatorKeyGenrate.Update(operatorKeyGenrate);
            }
            return operatorKeyGenrate;
        }

        public KeyValuePair<int, List<Complaint>> GetComplaintReport(DataTableServerSide searchModel, int userid, int apiid, int opid, byte statusid, string sdate, string edate, int circleid, string custno)
        {
            var fdata = searchModel.filterdata;

            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(fdata.ToDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

            var predicate = CustomPredicate.BuildPredicate<Complaint>(searchModel, new Type[] { typeof(Complaint) });

            predicate = (fdata.ApiId > 0 || fdata.OpId > 0 || fdata.UserId > 0 || fdata.StatusId > 0 || fdata.CircleId > 0 || !string.IsNullOrEmpty(fdata.CustomerNo)) ? predicate.And(x => x.Recharge != null) : predicate;


            predicate = fdata.ApiId == 0 ? predicate : predicate.And(x => x.Recharge.ApiId == fdata.ApiId);
            predicate = fdata.OpId == 0 ? predicate : predicate.And(x => x.Recharge.OpId == fdata.OpId);
            predicate = fdata.UserId == 0 ? predicate : predicate.And(x => (x.Recharge.UserId == fdata.UserId));
            if (statusid == 11)
            {
                // predicate = fdata.StatusId == 0 ? predicate : (fdata.StatusId == 11) ? predicate.And(x => x.StatusId == 2 || x.StatusId == 5) : predicate.And(x => x.StatusId == fdata.StatusId);
            }
            else
            {
                predicate = fdata.StatusId == 0 ? predicate : (fdata.StatusId == 25) ? predicate.And(x => x.StatusId == 2 || x.StatusId == 5 || x.StatusId == 9) : predicate.And(x => x.StatusId == fdata.StatusId);
            }
            predicate = fdata.CircleId == 0 ? predicate : predicate.And(x => x.Recharge.CircleId == fdata.CircleId);
            predicate = string.IsNullOrEmpty(fdata.CustomerNo) ? predicate : predicate.And(x => x.Recharge.CustomerNo == fdata.CustomerNo);

            if (fdata.StatusId != 0 && fdata.StatusId != 25)
            {
                predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.ComplaintDate >= fdate);
                predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.ComplaintDate <= tdate);
            }

            predicate = fdata.RecId == 0 ? predicate : predicate.And(x => (x.RecId == fdata.RecId));
            predicate = string.IsNullOrEmpty(fdata.OpTxnId) ? predicate : predicate.And(x => (x.Recharge.OptTxnId == fdata.OpTxnId));


            int totalCount;

            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<Complaint> results = repoComplaint
                .Query()
                 .Filter(predicate)
                .CustomOrderBy(u => u.OrderByDescending(x => x.Id))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<Complaint>> resultResponse = new KeyValuePair<int, List<Complaint>>(totalCount, results);

            return resultResponse;
        }

        public ICollection<Complaint> GetComplaints(Core.FilterData fdata)
        {

            var predicate = PredicateBuilder.True<Complaint>();


            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.FromDate) ? fdata.FromDate : DateTime.Now.AddDays(-30).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.ToDate) ? fdata.ToDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(fdata.ToDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

            predicate = (fdata.ApiId > 0 || fdata.OpId > 0 || fdata.UserId > 0 || fdata.StatusId > 0 || fdata.CircleId > 0 || !string.IsNullOrEmpty(fdata.CustomerNo)) ? (predicate.And(x => x.Recharge != null)) : predicate;

            predicate = fdata.ApiId == 0 ? predicate : predicate.And(x => x.Recharge.ApiId == fdata.ApiId);
            predicate = fdata.OpId == 0 ? predicate : predicate.And(x => x.Recharge.OpId == fdata.OpId);
            predicate = fdata.UserId == 0 ? predicate : predicate.And(x => (x.Recharge.UserId == fdata.UserId));

            predicate = fdata.StatusId == 0 ? predicate : (fdata.StatusId == 25) ? predicate.And(x => x.StatusId == 2 || x.StatusId == 5) : predicate.And(x => x.StatusId == fdata.StatusId);
            predicate = fdata.CircleId == 0 ? predicate : predicate.And(x => x.Recharge.CircleId == fdata.CircleId);
            predicate = string.IsNullOrEmpty(fdata.CustomerNo) ? predicate : predicate.And(x => x.Recharge.CustomerNo == fdata.CustomerNo);

            predicate = string.IsNullOrEmpty(fdata.FromDate) ? predicate : predicate.And(x => x.ComplaintDate >= fdate);
            predicate = string.IsNullOrEmpty(fdata.ToDate) ? predicate : predicate.And(x => x.ComplaintDate <= tdate);

            predicate = fdata.RecId == 0 ? predicate : predicate.And(x => (x.RecId == fdata.RecId));
            predicate = string.IsNullOrEmpty(fdata.OpTxnId) ? predicate : predicate.And(x => (x.Recharge.OptTxnId == fdata.OpTxnId));

            return repoComplaint.Query().Filter(predicate).Get().ToList();
        }

        
        #region "Dispose"
        public void Dispose()
        {
            if (repoUser != null)
            {
                repoUser.Dispose();
                repoUser = null;
            }
            if (repoApiUrl != null)
            {
                repoApiUrl.Dispose();
                repoApiUrl = null;
            }
        }
        #endregion
    }
}
#endregion