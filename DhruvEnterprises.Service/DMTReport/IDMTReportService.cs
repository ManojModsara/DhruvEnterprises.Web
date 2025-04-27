using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using System;
using System.Collections.Generic;

namespace DhruvEnterprises.Service
{
    public interface IDMTReportService : IDisposable
    {
        KeyValuePair<int, List<DMT>> GetRechargeReport(DataTableServerSide searchModel, int uid, int apiid, int opid, byte statusid, string SearchId, string startDate, string endDate, int cid, string cno, string ut, string vt, string ot, int uid2);
        DMT GetRecharge(long recid);
        ApiUrl GetApiUrlById(int Apiid, byte urltypeId);
        ICollection<StatusType> GetStatusList();
        ICollection<DMT> GetRechargeList(string StartDate, string EndDate, byte StatusId, int UserId = 0, int ApiId = 0, string SearchId = "0", int OpId = 0, int cid = 0, string cno = "0", string ut = "0", string vt = "0", string ot = "0", int u2 = 0);
        ICollection<TxnType> GetTxnTypes(string remark = "");
        ICollection<AmtType> GetAmtTypes(string remark = "");
        Complaint GetComplaint(long compid);
        ICollection<Complaint> GetComplaintByRecId(long recid);
        Complaint Save(Complaint complaint);
        DMT GetDmtStatusCheck(long txnId, string apitxnid, string reqtxnid, int userid = 0);
        KeyValuePair<int, List<Complaint>> GetComplaintReport(DataTableServerSide searchModel, int userid, int apiid, int opid, byte statusid, string sdate, string edate, int circleid, string custno);
    }
}
