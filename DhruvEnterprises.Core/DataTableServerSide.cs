using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Core
{

    public class DataTableServerSide
    {
        public List<DataTableColumns> columns { get; set; }
        public int draw { get; set; }
        public int length { get; set; }
        public List<DataTableOrder> order { get; set; }
        public DataTableSearch search { get; set; }
        public List<DataTableMultiSearch> multisearch { get; set; }
        public DataTableCustomFilter filter { get; set; }
        public int start { get; set; }

        public FilterData filterdata { get; set; } 
         

    }

    public class DataTableColumns
    {
        public int data { get; set; }
        public string name { get; set; }
        public int orderable { get; set; }
        public DataTableSearch search { get; set; }
        public int searchable { get; set; }
    }

    public class DataTableSearch
    {
        public bool regex { get; set; }
        public string value { get; set; }
    }

    public class DataTableMultiSearch
    {
        public string column { get; set; }
        public DataTableFilterType filter { get; set; }
        public string value { get; set; }
        public bool withOr { get; set; }
    }

    public class DataTableCustomFilter
    {
        public string text { get; set; }
        public string value { get; set; }
    }

    public class DataTableOrder
    {
        public int column { get; set; }
        public string dir { get; set; }
    }

    public class CustomOrderBy
    {
        public string name { get; set; }
        public string dir { get; set; }
    }

    public enum DataTableFilterType
    {
        Contains = 0,
        Equals = 1,
        StartsWith = 2,
        LessThanOrEqual = 3,
        GreaterThanOrEqual = 4
    }

    public class FilterData
    {
        public int ApiId { get; set; }
        public int UserId { get; set; }
        public int UserId2 { get; set; } 
        public int StatusId { get; set; }
        public int CircleId { get; set; }
        public int OpId { get; set; }
        public long RecId { get; set; }
        public long TxnId { get; set; }
        public string CustomerNo { get; set; } 
        public string RefId { get; set; }
        public string OpTxnId { get; set; }
        public string ApiTxnId { get; set; }
        public string UserTxnId { get; set; }
        public string Remark { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }

        public string EndDate { get; set; }
         
        public string FromDateNow { get; set; }
        public string ToDateNow { get; set; }

        public int PackId { get; set; }
        public int RoleId { get; set; }
        public string Email { get; set; }
        public string ApiKey { get; set; }
        public string IPAddress { get; set; }
        public int IsResentOnly { get; set; }

        public long LapuId { get; set; }
        public int DealerId { get; set; }

        public int TxnTypeId { get; set; }

        public int AmtTypeId { get; set; }

        public int TrTypeId { get; set; }
        public string Comment { get; set; }
        public int AccountId { get; set; }
        public int RefAccountId { get; set; }

        public int Amount { get; set; }
        //PaymentGatewayFilterParams
        public int tid { get; set; }
        public string GtxnId { get; set; }
        public string GatewayNm { get; set; }
        public string BankName { get; set; }
        public int Paymentmode { get; set; }
        public string OrderId { get; set; }
        public int Isa { get; set; }
        public string Usernumber { get; set; }

    }

}
