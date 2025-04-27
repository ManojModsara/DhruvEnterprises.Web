using DhruvEnterprises.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public interface IRechargeService : IDisposable
    {
        Recharge GetRecharge(long recid);
        ICollection<OperatorKeyGenrate> GetOperatorKeyGenrateList(int opid, int userid, int vendorid);
        Operator GetOperator(string code, int opid=0);
        OperatorSerial GetOperatorBySerial(string serial);
        Circle GetCircleByCode(string code, int id = 0);
        CircleRouting GetCircleRouting(int cid, int opid);
        ICollection<CommanRouting> GetCommanRouting(int cid, int opid);
        ICollection<CommanRouting> GetAmountRouts(int cid, int opid, decimal amount);
        Recharge Save(Recharge recharge);
        OperatorCode GetOperatorByApiId(int opid, int apiid);
        bool IsDuplicateReq(long Uid, string usertxnid, string mobileno, decimal amount, int opid);
        Recharge RechargeCheck(string Txnid, string ApiTxnId);
        Recharge GetRecharge(long txnId, string apitxnid, string reqtxnid, int userid = 0);
        Complaint GetComplaint(long cId);
        Complaint Save(Complaint complaint);
        RechargeGiftCard Save(RechargeGiftCard rechargeGiftCard);
        BankAccountData GetBankAccountDataFetch(string AccountNo, string IFSCCode);
        List<RechargeGiftCard> RechargeGiftList(int RechargeId);
        BankAccountData Save(BankAccountData bankAccountData);
        ICollection<StopRouteMessage> GetStopRouteMessages(int apiid = 0, int opid = 0);
        Recharge GetOurClientId(string ClientId);
    }
}
