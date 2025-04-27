using DhruvEnterprises.Core;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.Code.LIBS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.Objects;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Controllers
{
    public class OrderController : BaseController
    {
        #region "Constructor"
        private IRoleService roleService;
        private IActivityLogService activityLogService;
        ActivityLogDto aLogdto;
        public ActionAllowedDto action;
        public OrderController(IRoleService _userroleService, IActivityLogService _activityLogService) : base(_activityLogService, _userroleService)
        {
            this.activityLogService = _activityLogService;
            this.roleService = _userroleService;
            this.aLogdto = new ActivityLogDto();
            this.action = new ActionAllowedDto();
        }
        #endregion
        public ActionResult Index(string f = "",  string e = "", int? i = 0, string rto = "")
        {

            TempData["RechargeFilterDto"] = null;

            UpdateActivity("Order REQUEST", "GET:Order/Index", string.Empty);
            //ViewBag.actionAllowed = action = ActionAllowed("MyOrder", CurrentUser.RoleId);


            RechargeFilterDto filter = new RechargeFilterDto();            
            filter.Isa = Convert.ToInt32(i.HasValue ? i : 0);
            filter.Searchid = rto;
            filter.Sdate = f;
            filter.Edate = e;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyy");
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("dd/MM/yyy");            
            ViewBag.FilterData = TempData["RechargeFilterDto"] = filter;

            return View();
        }
        [HttpPost]
        public ActionResult MyOrder(DataTableServerSide model)
        {
            //ViewBag.actionAllowed = action = ActionAllowed("MyOrder", CurrentUser.RoleId);

            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;
            int Userid = CurrentUser.RoleId != 3 ? 0 : CurrentUser.UserID;

            int page = model.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(model.start) / model.length)) + 1);

            var pro = Sp_MyOrder(page, model.draw == 1 ? 25 : model.length, Userid, flt.SdateNow, flt.EdateNow, flt.Searchid);
            ObjectParameter objParam = new ObjectParameter("TotalCount", typeof(int));
            objParam.Value = pro.Item1;

            List<OrderDto> orderDto = new List<OrderDto>();
            for (int i = 0; i < pro.Item2.Rows.Count; i++)
            {
                orderDto.Add(new OrderDto
                {
                    Id = Convert.ToInt32(pro.Item2.Rows[i]["Id"]),
                    FullName = pro.Item2.Rows[i]["FullName"] != DBNull.Value ? Convert.ToString(pro.Item2.Rows[i]["FullName"]) : null,
                    Amount = pro.Item2.Rows[i]["Amount"] != DBNull.Value ? Convert.ToDecimal(pro.Item2.Rows[i]["Amount"]) : 0,
                    TxnId = pro.Item2.Rows[i]["TxnId"] != DBNull.Value ? Convert.ToInt64(pro.Item2.Rows[i]["TxnId"]) : 0,
                    RequestTime = pro.Item2.Rows[i]["RequestTime"] != DBNull.Value ? Convert.ToString(pro.Item2.Rows[i]["RequestTime"]) : null
                });
            }

            return Json(new
            {
                draw = model.draw,
                recordsTotal = pro.Item1,
                recordsFiltered = pro.Item1,
                data = orderDto.Select(c => new List<object> {
                    DataTableButton.View(Url.Action("OrderDetail", "Order",new { id = c.Id }),"modal-Order-Detail", "View"),
                    c?.Id,
                    c?.FullName,
                    c?.TxnId,
                    c?.Amount,
                    c?.RequestTime
                })
            }, JsonRequestBehavior.AllowGet);
        }

        public Tuple<int, DataTable> Sp_MyOrder(int? PageNumber, int? PageSize, int? Userid, string SdateNow, string EdateNow, string Searchid)
        {            
            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(SdateNow) ? SdateNow : DateTime.Now.AddDays(-3).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(EdateNow) ? EdateNow : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(EdateNow) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;
            try
            {
                using (SqlConnection conn = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("Sp_MyOrder", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sDate", fdate);
                    cmd.Parameters.AddWithValue("@PageNumber", PageNumber);
                    cmd.Parameters.AddWithValue("@PageSize", PageSize);
                    cmd.Parameters.AddWithValue("@eDate", tdate);
                    cmd.Parameters.AddWithValue("@Uid", Userid);
                    cmd.Parameters.AddWithValue("@Txnid", Searchid);
                    cmd.Parameters.Add("@TotalCount", SqlDbType.Int).Direction = ParameterDirection.Output;
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable ds = new DataTable();
                    conn.Open();
                    sqlDataAdapter.Fill(ds);
                    conn.Close();
                    int COUNT = Convert.ToInt32(cmd.Parameters["@TotalCount"].Value);
                    return Tuple.Create<int, DataTable>(COUNT, ds);
                }
            }
            catch (Exception ex)
            {
                DataTable ds = new DataTable();
                return Tuple.Create<int, DataTable>(0, ds);
            }
        }
        private long UpdateActivity(string title, string action, string remark = "", long Id = 0)
        {
            try
            {
                aLogdto.Id = Id;
                aLogdto.ActivityName = title;
                aLogdto.ActivityPage = action;
                aLogdto.Remark = remark;
                aLogdto.UserId = CurrentUser?.UserID ?? 0;
                aLogdto = LogActivity(aLogdto);
            }
            catch (Exception ex)
            {

                LogException(ex);
            }

            return aLogdto.Id;
        }
        
        [HttpGet]
        public ActionResult OrderDetail(int id)
        {
            DataTable ds = new DataTable();
            using (SqlConnection conn = new SqlConnection(SiteKey.SqlConn))
            {
                SqlCommand cmd = new SqlCommand("Sp_MyOrderDetails", conn);
                cmd.CommandType = CommandType.StoredProcedure;                
                cmd.Parameters.AddWithValue("@Orderid", id);                
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);                
                conn.Open();
                sqlDataAdapter.Fill(ds);
                conn.Close();                
            }

            List<OrderDetailDto> orderDto = new List<OrderDetailDto>();
            for (int i = 0; i < ds.Rows.Count; i++)
            {
                orderDto.Add(new OrderDetailDto
                {
                    Id= Convert.ToInt32(ds.Rows[i]["Id"]),
                    OrderId = Convert.ToInt32(ds.Rows[i]["OrderId"]),
                    Name = ds.Rows[i]["Name"] != DBNull.Value ? Convert.ToString(ds.Rows[i]["Name"]) : null,
                    ImageUrl = ds.Rows[i]["ImageUrl"] != DBNull.Value ? Convert.ToString(ds.Rows[i]["ImageUrl"]) : null,
                    Quantity = Convert.ToInt32(ds.Rows[i]["Quantity"]),
                    UnitPrice = ds.Rows[i]["UnitPrice"] != DBNull.Value ? Convert.ToDecimal(ds.Rows[i]["UnitPrice"]) : 0
                });
            }

            return PartialView("_OrderDetail", orderDto);
        }
        
    }
}