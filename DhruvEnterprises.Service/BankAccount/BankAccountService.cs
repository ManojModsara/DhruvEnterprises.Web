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


    public class BankAccountService : IBankAccountService
    {
        private IRepository<BankAccount> repoBankAccount;
        private IRepository<BankStatement> repoBankStatement;
        private IRepository<AccountType> repoAccountType;
        private IRepository<TransferType> repoTransferType;
        public BankAccountService
        (
          IRepository<BankAccount> _repoBankAccount,
          IRepository<BankStatement> _repoBankStatement,
          IRepository<AccountType> _repoAccountType,
          IRepository<TransferType> _repoTransferType
        )
        {
            this.repoBankAccount = _repoBankAccount;
            this.repoBankStatement = _repoBankStatement;
            this.repoAccountType = _repoAccountType;
            this.repoTransferType = _repoTransferType;

        }

        public BankAccount GetBankAccountById(int accountId = 0)
        {
            return repoBankAccount.FindById(accountId);
        }

        public ICollection<BankAccount> GetBankAccounts(bool IsAdmin = false)
        {
            if (IsAdmin)
                return repoBankAccount.Query().Filter(x => x.User2.RoleId == 1 || x.User2.RoleId == 2).AsTracking().Get().ToList();
            else
                return repoBankAccount.Query().Filter(x => x.User2.RoleId != 1 && x.User2.RoleId != 2).AsTracking().Get().ToList();
        }

        public ICollection<BankAccount> GetBankAccountByUserOrApiId(int userId = 0, int apiId = 0)
        {
            return repoBankAccount.Query().Filter(x => (userId == 0 ? true : x.UserId != userId) && (apiId == 0 ? true : x.ApiId != apiId)).AsTracking().Get().ToList();
        }

        public KeyValuePair<int, List<BankAccount>> GetBankAccounts(DataTableServerSide searchModel, BankAccountFilterDto ft)
        {

            var predicate = CustomPredicate.BuildPredicate<BankAccount>(searchModel, new Type[] { typeof(BankAccount) });
            predicate = predicate.And(a => ft.UserId == 0 ? true : a.UserId == ft.UserId);
            predicate = predicate.And(a => ft.ApiId == 0 ? true : a.ApiId == ft.ApiId);
            predicate = predicate.And(a => ft.AccountNo == "" ? true : a.AccountNo == ft.AccountNo);
            predicate = predicate.And(a => ft.Remark == "" ? true : a.Remark == ft.Remark);
            predicate = predicate.And(a => ft.HolderName == "" ? true : a.HolderName == ft.HolderName);
            predicate = predicate.And(a => ft.UpiAddress == "" ? true : a.UpiAdress == ft.UpiAddress);
            predicate = predicate.And(a => ft.BankName == "" ? true : a.BankName == ft.BankName);

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<BankAccount> results = repoBankAccount
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(BankAccount) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<BankAccount>> resultResponse = new KeyValuePair<int, List<BankAccount>>(totalCount, results);

            return resultResponse;
        }
        public KeyValuePair<int, List<BankAccount>> GetBankDetailsAccounts(DataTableServerSide searchModel)
        {

            var predicate = CustomPredicate.BuildPredicate<BankAccount>(searchModel, new Type[] { typeof(BankAccount) });
            predicate = predicate.And(a => a.Id!=2 && a.Id != 4);
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<BankAccount> results = repoBankAccount
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(BankAccount) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<BankAccount>> resultResponse = new KeyValuePair<int, List<BankAccount>>(totalCount, results);

            return resultResponse;
        }
        public KeyValuePair<int, List<BankStatement>> GetBankStatements(DataTableServerSide searchModel)
        {
           var ft= searchModel.filterdata;
            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(ft.FromDate) ? ft.FromDate : DateTime.Now.AddDays(-30).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(ft.ToDate) ? ft.ToDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(ft.ToDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

            var predicate = CustomPredicate.BuildPredicate<BankStatement>(searchModel, new Type[] { typeof(BankStatement) });

            predicate = ft.UserId == 0 ? predicate : predicate.And(a =>  a.UserId == ft.UserId);
            predicate = ft.ApiId == 0 ? predicate : predicate.And(a => (a.ApiId == ft.ApiId));
            predicate = ft.AccountId == 0 ? predicate : predicate.And(a =>  a.AccountId == ft.AccountId);
           // predicate = predicate.And(a => ft.RefAccountId == 0 ? true : a.RefAccountId == ft.RefAccountId);
            predicate = ft.TrTypeId == 0 ? predicate : predicate.And(a =>  a.TrTypeId == ft.TrTypeId);
            predicate = ft.TxnTypeId == 0 ? predicate : predicate.And(a => a.TxnTypeId == ft.TxnTypeId);
            predicate = ft.AmtTypeId == 0 ? predicate : predicate.And(a => a.AmtTypeId == ft.AmtTypeId);
            predicate = ft.TxnId == 0 ? predicate : predicate.And(a =>  a.Id == ft.TxnId);
            predicate = string.IsNullOrEmpty(ft.RefId) ? predicate : predicate.And(a => a.PaymentRef == ft.RefId);
            predicate = string.IsNullOrEmpty(ft.Remark) ? predicate : predicate.And(a =>  a.Remark == ft.Remark);
            predicate = string.IsNullOrEmpty(ft.FromDate) ? predicate : predicate.And(x =>  x.AddedDate >= fdate);
            predicate = string.IsNullOrEmpty(ft.ToDate) ? predicate : predicate.And(x =>  x.AddedDate <= tdate);

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<BankStatement> results = repoBankStatement
                .Query()
                .Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(BankStatement) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<BankStatement>> resultResponse = new KeyValuePair<int, List<BankStatement>>(totalCount, results);

            return resultResponse;
        }

        public ICollection<BankStatement> GetBankStatements(BankStatementFilterDto ft)
        {
            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(ft.Sdate) ? ft.Sdate : ft.SdateNow, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(ft.Edate) ? ft.Edate : ft.EdateNow, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            tdate = !string.IsNullOrEmpty(ft.Edate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;



            return repoBankStatement.Query()
                .Filter
                (x => (ft.UserId == 0 ? true : x.UserId == ft.UserId) &&
            (ft.ApiId == 0 ? true : (x.BankAccount.ApiId == ft.ApiId || x.BankAccount1.ApiId == ft.ApiId)) &&
            (ft.AccountId == 0 ? true : x.AccountId == ft.AccountId) &&
            (ft.RefAccountId == 0 ? true : x.RefAccountId == ft.RefAccountId) &&
            (ft.TrTypeId == 0 ? true : x.TrTypeId == ft.TrTypeId) &&
            (ft.TxnTypeId == 0 ? true : x.TxnTypeId == ft.TxnTypeId) &&
            (ft.AmtTypeId == 0 ? true : x.AmtTypeId == ft.AmtTypeId) &&
            (ft.StatementId == 0 ? true : x.Id == ft.AmtTypeId) &&
            (ft.PaymentRef == "" ? true : x.PaymentRef == ft.PaymentRef) &&
            (ft.Remark == "" ? true : x.Remark == ft.Remark) &&
            (ft.Edate == "" ? true : x.AddedDate >= fdate) &&
            (ft.Edate == "" ? true : x.AddedDate <= tdate)


                ).AsTracking().Get().ToList();
        }


        public ICollection<AccountType> GetAccountTypeList()
        {
            return repoAccountType.Query().AsTracking().Get().ToList();
        }

        public ICollection<TransferType> GetTransferTypeList()
        {
            return repoTransferType.Query().AsTracking().Get().ToList();
        }

        public BankAccount Save(BankAccount bankAccount)
        {
            bankAccount.UpdatedDate = DateTime.Now;
            if (bankAccount.Id == 0)
            {
                bankAccount.AddedDate = DateTime.Now;
                repoBankAccount.Insert(bankAccount);
            }
            else
            {
                bankAccount.UpdatedDate = DateTime.Now;
                repoBankAccount.Update(bankAccount);
            }
            return bankAccount;
        }

        public BankAccount CheckBankAccount(string bankname, string accno, string holdername, string upi)
        { 
            BankAccount bankAccount = repoBankAccount .Query()
                                                      .Filter(x=>x.BankName== bankname ||
                                                                 x.AccountNo == accno ||
                                                                 x.HolderName == holdername ||
                                                                 x.UpiAdress == upi 
                                                      )
                                                      .Get()
                                                      .FirstOrDefault();

            return bankAccount;
        }


        #region "Dispose"
        public void Dispose()
        {
            if (repoBankAccount != null)
            {
                repoBankAccount.Dispose();
                repoBankAccount = null;
            }

            if (repoBankStatement != null)
            {
                repoBankStatement.Dispose();
                repoBankStatement = null;
            }

            if (repoAccountType != null)
            {
                repoAccountType.Dispose();
                repoAccountType = null;
            }


        }
        #endregion
    }
}
