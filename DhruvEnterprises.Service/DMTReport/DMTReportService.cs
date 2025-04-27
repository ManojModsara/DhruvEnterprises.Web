using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Repo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DhruvEnterprises.Service
{
    public class DMTReportService : IDMTReportService
    {
        #region "Fields"
        private IRepository<User> repoUser;
        private IRepository<ApiUrl> repoApiUrl;
        private IRepository<DMT> repoDMT;
        private IRepository<RequestResponse> repoRequestResponse;
        private IRepository<StatusType> repoStatusType;
        private IRepository<TxnType> repoTxnType;
        private IRepository<AmtType> repoAmtType;
        private IRepository<Complaint> repoComplaint;

        #endregion

        #region "Cosntructor"
        public DMTReportService(IRepository<User> _repoUser,
                                     IRepository<ApiUrl> _repoApiUrl,
                                     IRepository<DMT> _repoDMT,
                                     IRepository<RequestResponse> _repoRequestResponse,
                                     IRepository<StatusType> _repoStatusType,
                                     IRepository<TxnType> _repoTxnType,
                                     IRepository<AmtType> _repoAmtType,
                                     IRepository<Complaint> _repoComplaint
            )
        {
            this.repoUser = _repoUser;
            this.repoApiUrl = _repoApiUrl;
            this.repoDMT = _repoDMT;
            this.repoRequestResponse = _repoRequestResponse;
            this.repoStatusType = _repoStatusType;
            this.repoTxnType = _repoTxnType;
            this.repoAmtType = _repoAmtType;
            this.repoComplaint = _repoComplaint;
        }
        #endregion

        #region Method
        public KeyValuePair<int, List<DMT>> GetRechargeReport(DataTableServerSide searchModel, int uid, int apiid, int opid, byte statusid, string SearchId, string sdate, string edate, int cid, string cno, string ut, string vt, string ot, int uid2)
        {

            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(sdate) ? sdate : DateTime.Now.AddDays(-30).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(edate) ? edate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(edate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

            SearchId = !string.IsNullOrEmpty(SearchId) ? SearchId : "";
            long searchid = 0;
            try { searchid = !string.IsNullOrEmpty(SearchId) ? Convert.ToInt64(SearchId) : 0; } catch { }

            var predicate = CustomPredicate.BuildPredicate<DMT>(searchModel, new Type[] { typeof(DMT), typeof(StatusType), typeof(ApiSource), typeof(Operator) });


            predicate = predicate.And(x => opid == 0 ? true : x.OpId == opid);
            if (statusid == 10)
            {
                predicate = predicate.And(x => (x.StatusId == 2 || x.StatusId == 4 || x.StatusId == 5));
            }
            else
            {
                predicate = predicate.And(x => (uid == 0 ? true : (x.UserId == uid )));
                predicate = predicate.And(x => statusid == 0 ? true : x.StatusId == statusid);
                predicate = predicate.And(x => cno == "" || cno == null ? true : x.AccountNo == cno || x.BeneMobile == cno);
                predicate = predicate.And(x => ot == "" || ot == null ? true : x.OptTxnId == ot);
                predicate = predicate.And(x => ut == "" || ut == null ? true : x.UserTxnId == ut);
                predicate = predicate.And(x => vt == "" || vt == null ? true : x.ApiTxnId == vt);
                predicate = predicate.And(x => apiid == 0 ? true : x.ApiId == apiid);
                predicate = predicate.And(x => SearchId == "" ? true : (x.Id == searchid || x.TxnId == searchid || x.OurRefTxnId == SearchId));
                if (uid2 > 0)
                {
                    predicate = predicate.And(x => (uid2 == 0 ? true : x.UpdatedById == uid2));
                    predicate = predicate.And(x => sdate == "" ? true : x.UpdatedDate >= fdate);
                    predicate = predicate.And(x => edate == "" ? true : x.UpdatedDate <= tdate);
                }
                else
                {
                    predicate = predicate.And(x => sdate == "" ? true : x.RequestTime >= fdate);
                    predicate = predicate.And(x => edate == "" ? true : x.RequestTime <= tdate);
                }
            }


            int totalCount;

            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<DMT> results = repoDMT
                .Query()
                 .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(DMT), typeof(StatusType), typeof(ApiSource), typeof(Operator) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<DMT>> resultResponse = new KeyValuePair<int, List<DMT>>(totalCount, results);

            return resultResponse;
        }

        public ApiUrl GetApiUrlById(int Apiid ,byte urltypeId)
        {
            return repoApiUrl.Query().Filter(x => x.ApiId == Apiid && x.UrlTypeId == urltypeId).Get().FirstOrDefault();
        }
        public DMT GetRecharge(long recid)
        {
            return repoDMT.FindById(recid);
        }

        public ICollection<StatusType> GetStatusList()
        {
            return repoStatusType.Query().Get().OrderBy(x => x.TypeName).ToList();
        }

        public ICollection<DMT> GetRechargeList(string sdate, string edate, byte StatusId, int UserId = 0, int ApiId = 0, string rto = "0", int OpId = 0, int cid = 0, string cno = "0", string ut = "0", string vt = "0", string ot = "0", int u2 = 0)
        {

            DateTime fdate = !string.IsNullOrEmpty(sdate) ? DateTime.ParseExact(sdate, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now.AddDays(-30);
            DateTime tdate = !string.IsNullOrEmpty(edate) ? DateTime.ParseExact(edate, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
            tdate = !string.IsNullOrEmpty(edate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

            rto = !string.IsNullOrEmpty(rto) ? rto : "0";
            rto = !string.IsNullOrEmpty(rto) ? rto : "0";
            long searchid = 0;
            try { searchid = !string.IsNullOrEmpty(rto) ? Convert.ToInt64(rto) : 0; } catch { }

            if (u2 > 0)
            {

                return repoDMT.Query().Filter(x => (UserId == 0 ? true : x.UserId == UserId) &&
                                                                       (StatusId == 0 ? true : x.StatusId == StatusId) &&
                                                                       (OpId == 0 ? true : x.OpId == OpId) &&
                                                                       (cno == "0" || cno == "" ? true : x.AccountNo == cno) &&
                                                                        (ut == "0" || ut == "" ? true : x.UserTxnId == ut) &&
                                                                         (vt == "0" || vt == "" ? true : x.ApiTxnId == vt) &&
                                                                       (rto == "0" || rto == "" ? true : (x.Id == searchid || x.TxnId == searchid || x.OurRefTxnId == rto)) &&
                                                                       (u2 == 0 ? true : x.UpdatedById == u2) &&
                                                                      (sdate == "" ? true : x.UpdatedDate >= fdate) &&
                                                                      (edate == "" ? true : x.UpdatedDate <= tdate)
                                                                        ).Get().ToList();
            }
            else
            {
                return repoDMT.Query().Filter(x => (UserId == 0 ? true : x.UserId == UserId) &&
                                                        (StatusId == 0 ? true : x.StatusId == StatusId) &&
                                                        (OpId == 0 ? true : x.OpId == OpId) &&
                                                        (cno == "0" || cno == "" ? true : x.AccountNo == cno) &&
                                                         (ut == "0" || ut == "" ? true : x.UserTxnId == ut) &&
                                                          (vt == "0" || vt == "" ? true : x.ApiTxnId == vt) &&
                                                        (rto == "0" || rto == "" ? true : (x.Id == searchid || x.TxnId == searchid || x.OurRefTxnId == rto)) &&
                                                         (sdate == "" ? true : x.RequestTime >= fdate) &&
                                                         (edate == "" ? true : x.RequestTime <= tdate)
                                                         ).Get().ToList();
            }
        }
        #endregion

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
                complaint.ComplaintDate = DateTime.Now;
                repoComplaint.Insert(complaint);
            }
            else
            {
                complaint.UpdatedDate = DateTime.Now;
                repoComplaint.Update(complaint);
            }
            return complaint;
        }

        public KeyValuePair<int, List<Complaint>> GetComplaintReport(DataTableServerSide searchModel, int userid, int apiid, int opid, byte statusid, string sdate, string edate, int circleid, string custno)
        {

            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(sdate) ? sdate : DateTime.Now.AddDays(-30).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(edate) ? edate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(edate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

            var predicate = CustomPredicate.BuildPredicate<Complaint>(searchModel, new Type[] { typeof(Complaint), typeof(Recharge) });

            predicate = predicate.And(x => apiid == 0 ? true : x.Recharge.ApiId == apiid);
            predicate = predicate.And(x => opid == 0 ? true : x.Recharge.OpId == opid);
            predicate = predicate.And(x => (userid == 0 ? true : x.Recharge.UserId == userid));
            predicate = predicate.And(x => statusid == 0 ? true : x.StatusId == statusid);
            predicate = predicate.And(x => circleid == 0 ? true : x.Recharge.CircleId == circleid);
            predicate = predicate.And(x => custno == "" ? true : x.Recharge.CustomerNo == custno);
            predicate = predicate.And(x => sdate == "" ? true : x.ComplaintDate >= fdate);
            predicate = predicate.And(x => edate == "" ? true : x.ComplaintDate <= tdate);

            int totalCount;

            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<Complaint> results = repoComplaint
                .Query()
                 .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(Complaint), typeof(Recharge) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<Complaint>> resultResponse = new KeyValuePair<int, List<Complaint>>(totalCount, results);

            return resultResponse;
        }

        public DMT GetDmtStatusCheck(long txnId, string apitxnid, string reqtxnid, int userid = 0)
        {
            if (txnId > 0)
            {
                return repoDMT.Query().Filter(x => x.TxnId == txnId).Get().FirstOrDefault();
            }
            else if (!string.IsNullOrEmpty(apitxnid))
            {
                return repoDMT.Query().Filter(x => x.ApiTxnId == apitxnid && x.ApiTxnId != "NA").Get().FirstOrDefault();
            }
            else
            {
                return repoDMT.Query().Filter(x => x.UserTxnId == reqtxnid && x.UserId == userid).Get().FirstOrDefault();
            }

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
