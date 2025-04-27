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
    public interface IBankAccountService : IDisposable
    {
        BankAccount GetBankAccountById(int accountId = 0);
        ICollection<BankAccount> GetBankAccounts(bool IsAdmin = false);
        ICollection<BankAccount> GetBankAccountByUserOrApiId(int userId = 0, int apiId = 0);
        KeyValuePair<int, List<BankAccount>> GetBankAccounts(DataTableServerSide searchModel, BankAccountFilterDto filter);
        KeyValuePair<int, List<BankStatement>> GetBankStatements(DataTableServerSide searchModel);
        ICollection<BankStatement> GetBankStatements(BankStatementFilterDto ft);
        ICollection<AccountType> GetAccountTypeList();
        ICollection<TransferType> GetTransferTypeList();
        KeyValuePair<int, List<BankAccount>> GetBankDetailsAccounts(DataTableServerSide searchModel);
        BankAccount Save(BankAccount bankAccount);
        BankAccount CheckBankAccount(string bankname, string accno, string holdername, string upi);
    }
}
