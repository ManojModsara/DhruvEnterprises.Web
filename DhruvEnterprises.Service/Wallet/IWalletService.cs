using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public interface IWalletService : IDisposable
    {
        TxnLedger GetTxnLedger(long TxnId);
        List<User> GetWalletUserList(int RoleID);
        
        KeyValuePair<int, List<TxnLedger>> GetRefundReport(DataTableServerSide searchModel, long recid, long txnid, int txntypeid, int amttypeid, int apiid, int userid, string sdate, string edate, string remark);

        KeyValuePair<int, List<TxnLedger>> GetTxnReport(DataTableServerSide searchModel, long recid, long txnid, int txntypeid, int amttypeid, int apiid, int userid, string sdate, string edate, string remark);
        TxnLedger Save(TxnLedger txnLedger);
        WalletRequest Save(WalletRequest txnLedger);
        bool CheckAlreadyRefund(long recid);
        KeyValuePair<int, List<WalletRequest>> GetWalletRequests(DataTableServerSide searchModel, int uid = 0);
        WalletRequest GetWalletRequest(long wrId);
        bool IsChequeNoExists(string chequeno);
        ICollection<TxnLedger> GetUserTxnReport(TxnFilterDto ft);
        ICollection<WalletRequest> GetWalletRequestList(int statusid = 0, int userid = 0);
        WalletRequest GetPendingWalletRequestList(string ChequeNo, decimal Amount, int statusid = 0, int userid = 0);
        KeyValuePair<int, List<WalletRequest>> GetWalletExchangeReport(DataTableServerSide searchModel, int uid = 0);
    }
}
