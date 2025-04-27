using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class BankAccountDto
    {
        public BankAccountDto()
        {
            this.accountTypeList = new List<AccountTypeDto>();
            this.transferTypeList = new List<TransferTypeDto>();
           
        }
        public int Id { get; set; }
        public string BankName { get; set; }
        public string AccountNo { get; set; }
        public string HolderName { get; set; }
        public string IFSCCode { get; set; }
        public string UpiAdress { get; set; }
        public string BranchName { get; set; }
        public string BranchAddress { get; set; }
        public Nullable<decimal> BlockAmount { get; set; }
        public int AccountTypeId { get; set; }
        public string TrTypeId { get; set; } 
        public Nullable<int> UserId { get; set; }
        public Nullable<int> ApiId { get; set; }
        public string Remark { get; set; }
        public List<AccountTypeDto> accountTypeList { get; set; }
        public List<TransferTypeDto> transferTypeList { get; set; } 
    }

    public class AccountTypeDto
    { 
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TransferTypeDto 
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
