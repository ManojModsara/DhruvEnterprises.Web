using DhruvEnterprises.Data;
using DhruvEnterprises.Repo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DhruvEnterprises.Service
{
    public class RechargeService : IRechargeService
    {
        #region "Fields"
        private IRepository<User> repoAdminUser;
        private IRepository<Operator> repoOperator;
        private IRepository<Package> repoPackage;
        private IRepository<PackageComm> repoPackagecomm;
        private IRepository<ApiSource> repoApiSource;
        private IRepository<TxnLedger> repoTxnLedger;
        private IRepository<CircleRouting> repoCircleRouting;
        private IRepository<CommanRouting> repoCommanRouting;
        private IRepository<AmountRouting> repoAmountRouting;
        private IRepository<Circle> repoCircle;
        private IRepository<OperatorSerial> repoOperatorSerial;
        private IRepository<ApiUrl> repoApiUrl;
        private IRepository<RechargeGiftCard> repoRechargeGiftCard;
        private IRepository<ApiUrlType> repoApiUrlType;
        private IRepository<RequestResponse> repoRequestResponse;
        private IRepository<TagValue> repoTagValue;
        private IRepository<BankAccountData> repoBankAccountData;
        private IRepository<Tag> repoTag;
        private IRepository<Recharge> repoRecharge;
        private IRepository<OperatorCode> repoOperatorCode;
        private IRepository<Complaint> repoComplaint;
        private IRepository<StopRouteMessage> repoStopRouteMessage;
        private IRepository<OperatorKeyGenrate> repoOperatorKeyGenrate;
        #endregion

        #region "Cosntructor"
        public RechargeService
            (
             IRepository<User> _repoUserMaster,
             IRepository<Operator> _repoOperator,
             IRepository<Package> _repoPackage,
             IRepository<PackageComm> _repoPackagecomm,
             IRepository<ApiSource> _repoApiSource,
             IRepository<TxnLedger> _repoTxnLedger,
             IRepository<CircleRouting> _repoCircleRouting,
             IRepository<CommanRouting> _repoCommanRouting,
             IRepository<AmountRouting> _repoAmountRouting,
             IRepository<Circle> _repoCircle,
             IRepository<OperatorSerial> _repoOperatorSerial,
             IRepository<ApiUrl> _repoApiUrl,
             IRepository<ApiUrlType> _repoApiUrlType,
             IRepository<RequestResponse> _repoRequestResponse,
             IRepository<TagValue> _repoTagValue,
             IRepository<Tag> _repoTag,
             IRepository<BankAccountData> _repoBankAccountData,
             IRepository<RechargeGiftCard> _repoRechargeGiftCard,
             IRepository<Recharge> _repoRecharge,
             IRepository<OperatorCode> _repoOperatorCode,
             IRepository<Complaint> _repoComplaint,
             IRepository<StopRouteMessage> _repoStopRouteMessage,
              IRepository<OperatorKeyGenrate> _repoOperatorKeyGenrate
            )
        {
            this.repoAdminUser = _repoUserMaster;
            this.repoOperator = _repoOperator;
            this.repoRechargeGiftCard = _repoRechargeGiftCard;
            this.repoPackage = _repoPackage;
            this.repoPackagecomm = _repoPackagecomm;
            this.repoApiSource = _repoApiSource;
            this.repoTxnLedger = _repoTxnLedger;
            this.repoCircleRouting = _repoCircleRouting;
            this.repoCommanRouting = _repoCommanRouting;
            this.repoAmountRouting = _repoAmountRouting;
            this.repoCircle = _repoCircle;
            this.repoOperatorSerial = _repoOperatorSerial;
            this.repoApiUrl = _repoApiUrl;
            this.repoApiUrlType = _repoApiUrlType;
            this.repoOperatorKeyGenrate = _repoOperatorKeyGenrate;
            this.repoRequestResponse = _repoRequestResponse;
            this.repoTagValue = _repoTagValue;
            this.repoTag = _repoTag;
            this.repoRecharge = _repoRecharge;
            this.repoOperatorCode = _repoOperatorCode;
            this.repoComplaint = _repoComplaint;
            this.repoBankAccountData = _repoBankAccountData;
            this.repoStopRouteMessage = _repoStopRouteMessage;
        }
        #endregion

        #region "Method"
        public ICollection<OperatorKeyGenrate> GetOperatorKeyGenrateList(int opid, int userid, int vendorid)
        {
            return repoOperatorKeyGenrate.Query().Filter(x => x.OpId == opid && (x.Userid == userid) && x.VendorId == vendorid).Get().ToList();
        }
        public Recharge GetRecharge(long recid)
        {
            var rc = repoRecharge.FindById(recid);
            return rc;
        }

        public Operator GetOperator(string code, int opid = 0)
        {
            if (opid > 0)
            {
                return repoOperator.FindById(opid);
            }
            else
            {
                return repoOperator.Query().Filter(x => x.Name == code).Get().FirstOrDefault();
            }

        }

        public OperatorCode GetOperatorByApiId(int opid, int apiid)
        {

            return repoOperatorCode.Query().Filter(x => x.OpId == opid && x.ApiId == apiid).Get().FirstOrDefault();

        }

        public OperatorSerial GetOperatorBySerial(string serial)
        {
            return repoOperatorSerial.Query().Filter(x => serial.StartsWith(x.Series)).Get().FirstOrDefault();
        }

        public Circle GetCircleByCode(string code, int id = 0)
        {
            if (id > 0)
            {
                return repoCircle.FindById(id);
            }
            else
            {
                Circle circle = new Circle();
                circle = repoCircle.Query().Filter(x => x.CircleCode == code || x.CircleName == code).Get().FirstOrDefault();
                return repoCircle.Query().Filter(x => x.CircleCode == code || x.CircleName == code).Get().FirstOrDefault();
            }

        }

        public BankAccountData GetBankAccountDataFetch(string AccountNo, string IFSCCode)
        {
            return repoBankAccountData.Query().Filter(x => x.AccountNo == AccountNo && x.IFSCCode == IFSCCode).Get().FirstOrDefault();
        }

        public CircleRouting GetCircleRouting(int cid, int opid)
        {
            return repoCircleRouting.Query().Filter(x => (x.CircleId == cid) && (x.OpId == opid)).Get().FirstOrDefault();
        }

        public ICollection<CommanRouting> GetCommanRouting(int cid, int opid)
        {
            var clist = repoCommanRouting.Query().Filter(x => (x.IsActive ?? false) == true && x.OpId == opid && (x.CircleFilter.Contains("All") || x.CircleFilter.Contains(cid.ToString()))).Get().OrderBy(x => x.Priority).ThenByDescending(f => f.MinRO).ToList();

            List<CommanRouting> routelist = new List<CommanRouting>();
            foreach (var item in clist)
            {
                if (item.CircleFilter.ToLower().Contains("all"))
                {
                    routelist.Add(item);
                }
                else if (item.CircleFilter.Replace(" ", "").Split(',').Any(y => y == cid.ToString()))
                {
                    var cf = item.CircleFilter.Split(',').ToList();
                    if (cf.Any(x => Convert.ToInt32(x.Trim()) == cid))
                    {
                        routelist.Add(item);
                    }
                }
            }

            List<CommanRouting> routelistdistinct = new List<CommanRouting>();
            var plist = routelist.Select(x => x.Priority).Distinct();
            foreach (var p in plist)
            {
                var route = routelist.Where(x => x.Priority == p).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
                if (route != null)
                {
                    routelistdistinct.Add(route);
                }
            }

            return routelistdistinct;

        }

        public ICollection<CommanRouting> GetAmountRouts(int cid, int opid, decimal amount)
        {

            //ftype=4=range, 3=Amounts, 2=non-rofer, 1=roffer

            var aclist = repoCommanRouting.Query().Filter(x => (x.OpId == opid) && (x.IsActive ?? false) == true &&
                                                                   (x.CircleFilter.Contains("All") || x.CircleFilter.Contains(cid.ToString())) &&
                                                                   (x.AmountFilter.Contains(amount.ToString()) || x.FTypeId == 4)).Get().OrderBy(x => x.Priority).ThenBy(x => x.FTypeId).ThenByDescending(f => f.MinRO).ToList();

            var clist = aclist.Where(x => x.AmountFilter.Replace(" ", "").Split(',').Any(y => y == amount.ToString() || x.FTypeId == 4)).ToList();

            List<CommanRouting> routelist = new List<CommanRouting>();


            foreach (var item in clist)
            {

                if (item.CircleFilter.Contains("All") || item.CircleFilter.Replace(" ", "").Split(',').Where(x => x != "" || x != null).Any(y => y == cid.ToString()))
                {
                    if (item.FTypeId == 3 && !item.AmountFilter.Contains("-"))
                    {
                        if (item.AmountFilter.Replace(" ", "").Split(',').Where(x => x != "" || x != null).Any(y => y == amount.ToString()))
                        {
                            routelist.Add(item);
                        }

                    }
                    else if (item.FTypeId == 4 && item.AmountFilter.Contains("-"))
                    {
                        var ranges = item.AmountFilter.Replace(" ", "").Split(',').Where(x => x != "" || x != null).ToList();

                        if (ranges.Any(y => amount >= Convert.ToInt64(y.Split('-')[0]) && amount <= Convert.ToInt64(y.Split('-')[1])))
                        {
                            routelist.Add(item);
                        }
                    }

                }

            }

            List<CommanRouting> routelistdistinct = new List<CommanRouting>();
            var plist = routelist.Select(x => x.Priority).Distinct();
            foreach (var p in plist)
            {
                var route = routelist.Where(x => x.Priority == p && !routelistdistinct.Any(y => y.ApiId == x.ApiId && y.UserFilter == x.UserFilter && y.BlockUser == x.BlockUser && y.AmountFilter == x.AmountFilter)).OrderBy(x => Guid.NewGuid()).FirstOrDefault();

                if (route != null)
                {
                    routelistdistinct.Add(route);
                }
            }

            return routelistdistinct;
        }

        public Recharge Save(Recharge recharge)
        {
            recharge.UpdatedDate = DateTime.Now;
            if (recharge.Id == 0)
            {
                recharge.RequestTime = DateTime.Now;
                repoRecharge.Insert(recharge);
            }
            else
            {
                repoRecharge.Update(recharge);
            }
            return recharge;
        }
        public BankAccountData Save(BankAccountData bankAccountData)
        {
            bankAccountData.UpdatedDate = DateTime.Now;
            if (bankAccountData.Id == 0)
            {
                bankAccountData.AddedDate = DateTime.Now;
                repoBankAccountData.Insert(bankAccountData);
            }
            else
            {
                repoBankAccountData.Update(bankAccountData);
            }
            return bankAccountData;
        }

        public bool IsDuplicateReq(long Uid, string usertxnid, string mobileno, decimal amount, int opid)
        {

            bool res = false;
            if (!string.IsNullOrEmpty(usertxnid))
            {
                var recharge = repoRecharge.Query().Filter(x => x.UserId == Uid && x.UserTxnId == usertxnid).Get().ToList().FirstOrDefault();
                if (recharge == null)
                {
                    res = false;
                }
                else
                {
                    res = true;
                }
            }

            //if (!string.IsNullOrEmpty(mobileno) && amount > 0 && opid > 0)
            //{
            //    var recharge1 = repoRecharge.Query().Filter(x => x.UserId == Uid && x.CustomerNo == mobileno && x.Amount == amount && x.OpId == opid).Get().ToList().FirstOrDefault();
            //    if (recharge1 == null)
            //    {
            //        res = true;
            //    }
            //    else
            //    {
            //        res = false;
            //    }
            //}



            //if (recharge != null)
            //{
            //    var reqtime = recharge.RequestTime; 
            //    if()
            //}

            return res;
        }

        public Recharge RechargeCheck(string ourrefid, string apitxnid)
        {
            if (!string.IsNullOrEmpty(ourrefid))
            {
                return repoRecharge.Query().Filter(x => x.OurRefTxnId == ourrefid).Get().FirstOrDefault();
            }
            else
            {
                return repoRecharge.Query().Filter(x => x.ApiTxnId == apitxnid && x.ApiTxnId != "NA").Get().FirstOrDefault();
            }

        }

        public Recharge GetRecharge(long txnId, string apitxnid, string reqtxnid, int userid = 0)
        {
            if (txnId > 0)
            {
                return repoRecharge.Query().Filter(x => x.TxnId == txnId).Get().FirstOrDefault();
            }
            else if (!string.IsNullOrEmpty(apitxnid))
            {
                return repoRecharge.Query().Filter(x => x.ApiTxnId == apitxnid && x.ApiTxnId != "NA").Get().FirstOrDefault();
            }
            else
            {
                return repoRecharge.Query().Filter(x => x.UserTxnId == reqtxnid && x.UserId == userid).Get().FirstOrDefault();
            }

        }

        public Complaint GetComplaint(long cId)
        {
            return repoComplaint.FindById(cId);

        }
        public RechargeGiftCard Save(RechargeGiftCard rechargeGiftCard)
        {
            if (rechargeGiftCard.Id == 0)
            {
                repoRechargeGiftCard.Insert(rechargeGiftCard);
            }
            else
            {
                repoRechargeGiftCard.Update(rechargeGiftCard);
            }
            return rechargeGiftCard;
        }
        public List<RechargeGiftCard> RechargeGiftList(int RechargeId)
        {
            return repoRechargeGiftCard.Query().Get().Where(x => x.RecId == RechargeId).ToList();
        }
        public Complaint Save(Complaint complaint)
        {
            complaint.UpdatedDate = DateTime.Now;
            if (complaint.Id == 0)
            {
                complaint.UpdatedDate = DateTime.Now;
                repoComplaint.Insert(complaint);
            }
            else
            {
                repoComplaint.Update(complaint);
            }
            return complaint;
        }

        public ICollection<StopRouteMessage> GetStopRouteMessages(int apiid = 0, int opid = 0)
        {
            string op = opid.ToString();
            string api = apiid.ToString();

            var routemsg = repoStopRouteMessage.Query().Filter(x => (x.OpFilter.Contains(op) || x.OpFilter.Contains("All")) && (x.ApiFilter.Contains(op) || x.ApiFilter.Contains("All"))).Get().ToList();

            return routemsg?.Where(x => x.OpFilter.Trim().Split(',').Any(y => y == op || y.Contains("All")) && x.ApiFilter.Trim().Split(',').Any(z => z == api || z.Contains("All")))?.ToList();

        }
        public Recharge GetOurClientId(string ClientId)
        {
            return repoRecharge.Query().Get().Where(x => x.UserTxnId == ClientId).FirstOrDefault();

        }

        #endregion

        #region "Dispose"
        public void Dispose()
        {
            if (repoAdminUser != null)
            {
                repoAdminUser.Dispose();
                repoAdminUser = null;
            }
            if (repoBankAccountData != null)
            {
                repoBankAccountData.Dispose();
                repoBankAccountData = null;
            }
            if (repoOperator != null)
            {
                repoOperator.Dispose();
                repoOperator = null;
            }
            if (repoPackage != null)
            {
                repoPackage.Dispose();
                repoPackage = null;
            }
            if (repoPackagecomm != null)
            {
                repoPackagecomm.Dispose();
                repoPackagecomm = null;
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
            if (repoCircleRouting != null)
            {
                repoCircleRouting.Dispose();
                repoCircleRouting = null;
            }
            if (repoCommanRouting != null)
            {
                repoCommanRouting.Dispose();
                repoCommanRouting = null;
            }
            if (repoAmountRouting != null)
            {
                repoAmountRouting.Dispose();
                repoAmountRouting = null;
            }
            if (repoCircle != null)
            {
                repoCircle.Dispose();
                repoCircle = null;
            }
            if (repoOperatorSerial != null)
            {
                repoOperatorSerial.Dispose();
                repoOperatorSerial = null;
            }
            if (repoApiUrl != null)
            {
                repoApiUrl.Dispose();
                repoApiUrl = null;
            }
            if (repoApiUrlType != null)
            {
                repoApiUrlType.Dispose();
                repoApiUrlType = null;
            }
            if (repoRequestResponse != null)
            {
                repoRequestResponse.Dispose();
                repoRequestResponse = null;
            }
            if (repoTagValue != null)
            {
                repoTagValue.Dispose();
                repoTagValue = null;
            }
            if (repoTag != null)
            {
                repoTag.Dispose();
                repoTag = null;
            }
            if (repoRecharge != null)
            {
                repoRecharge.Dispose();
                repoRecharge = null;
            }
            if (repoOperatorCode != null)
            {
                repoOperatorCode.Dispose();
                repoOperatorCode = null;
            }
            if (repoOperatorKeyGenrate != null)
            {
                repoOperatorKeyGenrate.Dispose();
                repoOperatorKeyGenrate = null;
            }
        }
        #endregion
    }
}
