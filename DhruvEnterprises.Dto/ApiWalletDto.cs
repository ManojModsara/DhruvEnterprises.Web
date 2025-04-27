using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class ApiWalletDto
    {
        
        public int TxnId { get; set; }
        [DisplayName("VendorId")]
        public int ApiId { get; set; }
        [DisplayName("Current Bal")]
        public decimal CurrentBalance { get; set; }
        [DisplayName("Sent (INR)")]
        public decimal SentAmount { get; set; }
        [DisplayName("Recieved (INR)")]
        public decimal ReceivedAmount { get; set; }
        [DisplayName("Incentive (INR)")]
        public decimal IncentiveAmount { get; set; } 
        public string Remark { get; set; }
        [DisplayName("Cheque/Ref No")]
        public string ChequeNo { get; set; }
        public int  BankAccountId { get; set; }
        public int TrTypeId { get; set; }
        public List<TransferTypeDto> TrTypeList { get; set; } = new List<TransferTypeDto>();
        public List<BankAccountDto> BankAccountList { get; set; } = new List<BankAccountDto>();

        [DisplayName("Is Pull Out?")]
        public bool IsPullOut { get; set; }

        public bool IsDirect { get; set; } 
    }
}
