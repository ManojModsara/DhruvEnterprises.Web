using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using System;
using DhruvEnterprises.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public interface IRechargeReportService : IDisposable
    {
        ICollection<OperatorKeyGenrate> GetOperatorKeyGenrateList(int opid, int userid, int vendorid);
        KeyValuePair<int, List<Recharge>> GetRechargeReport(DataTableServerSide searchModel,int uid, int apiid, int opid, byte statusid, string SearchId, string startDate, string endDate, int cid, string cno, string ut, string vt, string ot, int uid2, ref string log);
        KeyValuePair<int, List<Recharge>> GetProcessingRecharge(DataTableServerSide searchModel, RechargeFilterDto ft);
        Recharge GetRecharge(long recid);
        Recharge Save(Recharge ff);
        KeyValuePair<int, List<DMT>> GetProcessingDMT(DataTableServerSide searchModel, RechargeFilterDto ft);
        KeyValuePair<int, List<Recharge>> GetPendingrecharge(DataTableServerSide searchModel, RechargeFilterDto ft);
        ICollection<StatusType> GetStatusList();
        ICollection<Recharge> GetRechargeList(string StartDate, string EndDate, byte StatusId, int UserId = 0, int ApiId = 0, string SearchId = "0", int OpId = 0, int cid = 0, string cno = "0", string ut = "0", string vt = "0", string ot = "0", int u2=0);
        KeyValuePair<int, List<DMT>> GetDMTReport(DataTableServerSide searchModel, int uid, int apiid, int opid, byte statusid, string SearchId, string startDate, string endDate, int cid, string cno, string ut, string vt, string ot, int uid2, ref string log);
        ICollection<TxnType> GetTxnTypes(string remark = "");
        ICollection<AmtType> GetAmtTypes(string remark = "");

        Complaint GetComplaint(long compid);
        ICollection<Complaint> GetComplaintByRecId(long recid);
        Complaint Save(Complaint complaint);
        KeyValuePair<int, List<Complaint>> GetComplaintReport(DataTableServerSide searchModel, int userid, int apiid, int opid, byte statusid, string sdate, string edate, int circleid, string custno);
        ICollection<Complaint> GetComplaints(Core.FilterData fdata);
        OperatorKeyGenrate Save(OperatorKeyGenrate operatorKeyGenrate);
        OperatorKeyGenrate OperatorKeyGenrateGetData(int opid, int userid, int vendorid, int KeyTypeId);
        KeyValuePair<int, List<OperatorKeyGenrate>> GetOperatorKeyGenrateReport(DataTableServerSide searchModel, int uid, int apiid, int opid);
    }
}
