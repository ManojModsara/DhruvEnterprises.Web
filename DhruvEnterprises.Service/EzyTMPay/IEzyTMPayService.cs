using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
   public  interface IEzyTMPayService
    {
        EzyTMPayRequest Save(EzyTMPayRequest ezyTMPayRequest);
        EzyTMPayRequest GetTxnRecordById(int Id);
        KeyValuePair<int, List<EzyTMPayRequest>> GetGatewaytxnReport(DataTableServerSide searchModel,int userId=0);
        ICollection<StatusType> GetStatusType();
        ICollection<GatewayTransferType> GetTransfer();


    }
}
