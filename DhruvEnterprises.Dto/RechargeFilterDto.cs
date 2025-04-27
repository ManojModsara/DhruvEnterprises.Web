using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Dto
{
    public class RechargeFilterDto
    {

        public string CustomerNo { get; set; }
        public string Searchid { get; set; }

        public string OpTxnid { get; set; }
        public string ApiTxnid { get; set; }
        public string UserReqid { get; set; }
        public int Circleid { get; set; }


        public int Uid { get; set; }
        public int Apiid { get; set; }
        public int Opid { get; set; }

        public int Sid { get; set; }
        public string Sdate { get; set; }
        public string Edate { get; set; }

        public int Isa { get; set; }

        public string SdateNow { get; set; }
        public string EdateNow { get; set; }

        public int UpdatedById { get; set; }

        public int IsResentOnly { get; set; }
    }

    public class TxnFilterDto
    {
        public int userid { get; set; }
        public int apiid { get; set; }
        public long txnid { get; set; }
        public long recid { get; set; }
        public string remark { get; set; }
        public int amttypeid { get; set; }
        public int txntypeid { get; set; }
        public string sdate { get; set; }
        public string edate { get; set; }
        public int isshow { get; set; }

    }

    public class ReqResFilterDto
    {
        //RecId,CustomerNo,RefId,RefId,UserTxnId,Remark,Uid,Apiid,Opid,Sid,Sdate,Edate,Isa,SdateNow,EdateNow
        public long RecId { get; set; }
        public string CustomerNo { get; set; }
        public string RefId { get; set; }
        public string UserTxnId { get; set; }
        public string Remark { get; set; }
        public int UserId { get; set; }
        public int ApiId { get; set; }
        public int OpId { get; set; }

        public int StatusId { get; set; }
        public string Sdate { get; set; }
        public string Edate { get; set; }

        public int Isa { get; set; }

        public string SdateNow { get; set; }
        public string EdateNow { get; set; }
    }

    public class BankAccountFilterDto
    {
        public string BankName { get; set; }
        public string AccountNo { get; set; }
        public string HolderName { get; set; }
        public string UpiAddress { get; set; }
        public int UserId { get; set; }
        public int ApiId { get; set; }
        public string Remark { get; set; }
        public int IsActive { get; set; }
    }

    public class BankStatementFilterDto
    {
        public long StatementId { get; set; }
        public int AccountId { get; set; }
        public int TxnTypeId { get; set; }
        public int AmtTypeId { get; set; }
        public int TrTypeId { get; set; }
        public string Remark { get; set; }
        public string Comment { get; set; }
        public string PaymentRef { get; set; }
        public int RefAccountId { get; set; }
        public int UserId { get; set; }
        public int ApiId { get; set; }
        public long TxnId { get; set; }
        public long VendorTxnId { get; set; }
        public int IsActive { get; set; }
        public string Sdate { get; set; }
        public string Edate { get; set; }
        public string SdateNow { get; set; }
        public string EdateNow { get; set; }

    }

    public class ActiVityLogFilterDto
    {
        public int userid { get; set; }
        public string actname { get; set; }
        public string ipaddress { get; set; }
        public string url { get; set; }
        public string remark { get; set; }
        public string sdate { get; set; }
        public string edate { get; set; }

        public string sdateNow { get; set; }
        public string edateNow { get; set; }

        public int isshow { get; set; }
    }

    public class LapuTxnFilterDto
    {
        public long lapuid { get; set; }
        public int dealerid { get; set; }
        public int userid { get; set; }
        public int apiid { get; set; }
        public long recid { get; set; }
        public long txnid { get; set; }
        public long accountid { get; set; }
        public string remark { get; set; }
        public string refnumber { get; set; }
        public int amttypeid { get; set; }
        public int txntypeid { get; set; }
        public string sdate { get; set; }
        public string edate { get; set; }
        public int isshow { get; set; }
        public int opid { get; set; }
    }

    public class UserFilterDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public int PackId { get; set; }
        public int StatusId { get; set; }

        public string EmailId { get; set; }
        public string ContactNo { get; set; }
        public string ApiKey { get; set; }
        public string IPAddress { get; set; }

        public int Isa { get; set; }
    }

    public class InvoiceFilterDto
    {
        public int UserId { get; set; }
        public int Isa { get; set; }
    }

    public class UserRulesFilterDto
    {
        public int Uid { get; set; }
        public int Apiid { get; set; }
        public int Opid { get; set; }
        public int Circleid { get; set; }
        public decimal? Amount { get; set; }
        public string Sdate { get; set; }
        public string Edate { get; set; }
        public int Isa { get; set; }
        public string SdateNow { get; set; }
        public string EdateNow { get; set; }

    }


    public class AEPSUserFilterDto
    {


        public string EmailId { get; set; }
        public string ContactNo { get; set; }

        public string IPAddress { get; set; }

        public int Isa { get; set; }
    }
    public class OrderDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string RequestTime { get; set; }
        public long TxnId { get; set; }
        public string FullName { get; set; }
    }
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

}
