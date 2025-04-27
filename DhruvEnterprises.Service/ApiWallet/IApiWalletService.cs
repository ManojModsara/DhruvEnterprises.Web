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


    public  interface IApiWalletService: IDisposable
    {
        ApiWalletTxn GetApiWalletTxn(long id);
        ApiWalletTxn  GetApiWalletByRef(long id);
        KeyValuePair<int, List<ApiWalletTxn>> GetApiWalletTxns(DataTableServerSide searchModel, long recid, long txnid, int txntypeid, int amttypeid, int apiid, int userid, string sdate, string edate, string remark);
        ApiWalletTxn Save(ApiWalletTxn apiWalletTxn);
        decimal GetApiWalletBalance(int apiId);
        bool CheckApiWalletTxn(long recid, int apid);

    }
}
