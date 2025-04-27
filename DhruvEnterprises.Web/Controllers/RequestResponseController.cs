using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.Code.Attributes;
using DhruvEnterprises.Web.LIBS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
namespace DhruvEnterprises.Web.Controllers
{
   
    public class RequestResponseController : BaseController
    {
        #region "Fields"

        private readonly IRequestResponseService requestResponseService;
        
        private readonly IRechargeReportService rechargeReportService;
        private readonly IRechargeService rechargeService;
        private readonly IUserService userService;
        private readonly IPackageService packageService;
        private readonly IApiService apiService;
        private readonly IOperatorSwitchService operatorSwitchService;

        ActivityLogDto activityLogModel;
        public ActionAllowedDto action;
        #endregion

        #region "Constructor"
        public RequestResponseController
            (IRequestResponseService _requestResponseService, 
             IRechargeReportService _rechargeReportService,
             IRechargeService _rechargeService,
             IUserService _userService,
             IPackageService _packageService,
             IApiService _apiService,
             IOperatorSwitchService _operatorSwitchService,
              IActivityLogService _activityLogService,
               IRoleService _userroleService
            ) : base(_activityLogService, _userroleService)
        {

            this.requestResponseService = _requestResponseService;
            this.rechargeReportService = _rechargeReportService;
            this.rechargeService = _rechargeService;
            this.userService = _userService;
            this.packageService = _packageService;
            this.apiService = _apiService;
            this.operatorSwitchService = _operatorSwitchService;
            this.activityLogModel = new ActivityLogDto();
            this.action = new ActionAllowedDto();
        }
        #endregion

        // GET: RequestResponse
        [HttpGet]
        public ActionResult Index(int? u, int? v, int? o, int? s, int? r, string m = "", string f = "", string e = "", int? i = 0, string rf = "", string rm = "",string ut = "")
        {
            TempData["ReqResFilterDto"] = null;
            UpdateActivity("RequestReponse  REQUEST", "GET:RequestReponse/Index/", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("RequestResponse", CurrentUser.RoleId);
            try
            {
                                
                ReqResFilterDto ft = new ReqResFilterDto();
                ft.UserId = Convert.ToInt32(u.HasValue ? u : 0);
                ft.Isa = Convert.ToInt32(i.HasValue ? i : 0);
                ft.ApiId = Convert.ToInt32(v.HasValue ? v : 0);
                ft.OpId = Convert.ToInt32(o.HasValue ? o : 0);
                ft.RecId = Convert.ToInt64(r.HasValue ? r : 0);
                ft.RefId = rf;
                ft.StatusId = Convert.ToInt32(s.HasValue ? s : 0);
                ft.Sdate = f;
                ft.Edate = e;
                ft.SdateNow = !string.IsNullOrEmpty(ft.Sdate) ? ft.Sdate : DateTime.Now.ToString("dd/MM/yyy");
                //ft.EdateNow = !string.IsNullOrEmpty(ft.EdateNow) ? ft.EdateNow : DateTime.Now.ToString("dd/MM/yyy");
                ft.EdateNow = !string.IsNullOrEmpty(ft.Edate) ? ft.Edate : DateTime.Now.ToString("dd/MM/yyy");
                ft.CustomerNo = m;
                ft.UserTxnId = ut;
                ft.Remark = rm;
                ViewBag.FilterData = TempData["ReqResFilterDto"] = ft;
                int userrole = CurrentUser.RoleId;
                bool IsAdminRole = (userrole != 3) ? true : false;
                int uid = IsAdminRole ? 0 : CurrentUser.UserID;
                ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == ft.OpId) }).ToList();
                ViewBag.StatusList = rechargeReportService.GetStatusList().Where(st => st.Remark.Contains("Recharge")).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = ((x.Id == ft.StatusId)) }).ToList();
                ViewBag.UserList = userService.GetUserList().Where(x => uid == 0 ? x.RoleId == 3 : x.Id == uid && x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? string.Empty, Selected = (x.Id == ft.UserId) }).ToList();
                ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == ft.ApiId) }).ToList();
            }
            catch (Exception ex)
            {
                LogException(ex, "RequestResponse Index");
            }
          
            return View();
        }

        [HttpPost]
        public ActionResult GetRequestResponses(DataTableServerSide model)
        {

            ViewBag.actionAllowed = action = ActionAllowed("RequestResponse", CurrentUser.RoleId);

            ReqResFilterDto ft = TempData["ReqResFilterDto"] != null ? (ReqResFilterDto)TempData["ReqResFilterDto"] : new ReqResFilterDto();
            ViewBag.FilterData = TempData["ReqResFilterDto"] = ft;

            int userrole = CurrentUser.RoleId;
            bool IsAdminRole = (userrole != 3) ? true : false;
            model.filterdata.UserId = IsAdminRole ? model.filterdata.UserId : CurrentUser.UserID;
            KeyValuePair<int, List<RequestResponse>> requestResponses = requestResponseService.GetRequestResponses(model, ft);
            if (IsAdminRole)
            {
                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = requestResponses.Key,
                    recordsFiltered = requestResponses.Key,
                    data = requestResponses.Value?.Select(c => new List<object> {
                    c.Id,
                    //DataTableButton.HyperLink(Url.Action( "userdetail", "requestresponse",new { id = c.UserId }),"modal-view-user-detail", c.User.UserProfile?.FullName?.ToString(),"View User Detail"),
                    //DataTableButton.HyperLink(Url.Action( "urldetail", "requestresponse",new { id = c.UrlId }),"modal-view-url-detail", c.ApiUrl?.ApiSource?.ApiName?.ToString(),"View Url Detail"),
                    //DataTableButton.HyperLink(Url.Action( "rechargedetail", "requestresponse",new { id = c.RecId }),"modal-view-rec-detail", c.RecId?.ToString(),"View Recharge Detail"),                   
                    c.User?.UserProfile?.FullName??string.Empty,
                    c.ApiUrl?.ApiSource?.ApiName??string.Empty,
                    c.Remark,
                    (c.AddedDate).ToString(),
                    c.RequestTxt,
                    "<xmp class='txt-wrap-hide'>"+c.ResponseText+"</xmp>",
                    (c.UpdatedDate)?.ToString()??string.Empty,
                    //c.RecId,
                    c.RefId,
                    c.CustomerNo,
                    c.UserTxnId
                    //c.Recharge?.Operator?.Name??string.Empty,
                    //c.Recharge?.StatusType?.TypeName??string.Empty
                    })
                }, JsonRequestBehavior.AllowGet);
            }

            else
            {
                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = requestResponses.Key,
                    recordsFiltered = requestResponses.Key,
                    data = requestResponses.Value.Where(x => IsAdminRole ? true : x.Remark == "Request_R" || x.Remark == "Request_F").Select(c => new List<object> {
                    c.Id,
                    //DataTableButton.HyperLink(Url.Action( "userdetail", "requestresponse",new { id = c.UserId }),"modal-view-user-detail", c.User.UserProfile?.FullName?.ToString(),"View User Detail"),
                    //DataTableButton.HyperLink(Url.Action( "urldetail", "requestresponse",new { id = c.UrlId }),"modal-view-url-detail", c.ApiUrl?.ApiSource?.ApiName?.ToString(),"View Url Detail"),
                    //DataTableButton.HyperLink(Url.Action( "rechargedetail", "requestresponse",new { id = c.RecId }),"modal-view-rec-detail", c.RecId?.ToString(),"View Recharge Detail"),                   
                    c.User?.UserProfile?.FullName??string.Empty,
                    c.ApiUrl?.ApiSource?.ApiName??string.Empty,
                    c.Remark,
                    (c.AddedDate).ToString(),
                    c.RequestTxt,
                    "<xmp class='txt-wrap-hide'>"+c.ResponseText+"</xmp>",
                    (c.UpdatedDate)?.ToString()??string.Empty,
                    //c.RecId,
                    c.RefId,
                    c.CustomerNo,
                    c.UserTxnId
                    //c.Recharge?.Operator?.Name??string.Empty,
                    //c.Recharge?.StatusType?.TypeName??string.Empty
                    })
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public JObject GetRequestResponse1(long recid)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("proc_RequestResponse", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@recID", recid);
                    con.Open();
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dataTable = new DataTable();
                    sqlDataAdapter.Fill(dataTable);
                    con.Close();
                    

                    string JSONresult;
                    JSONresult = JsonConvert.SerializeObject(dataTable);
                    
                    JArray jsonArray = JArray.Parse(JSONresult);
                    dynamic data = JObject.Parse(jsonArray[0].ToString());
                    //                 // dynamic _responseDetail = JObject.Parse(JSONresult);
                    return data;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                return null;
            }
        }
        public JObject GetRequestResponse(long recid)
        {
            try
            {
             
                var apires = requestResponseService.GetRequestResponse(recid);
                var json = JsonConvert.SerializeObject(apires);
                dynamic _responseDetail = JObject.Parse(json);
                return _responseDetail;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public ActionResult DisplayReqRes( int? id , string rf = "")
        {
            TempData["ReqResFilterDto"] = null;
            UpdateActivity("RequestReponse  REQUEST", "GET:RequestReponse/Index/", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("RequestResponse", CurrentUser.RoleId);
            try
            {       

                ReqResFilterDto ft = new ReqResFilterDto();
                ft.UserId =  0;
                ft.Isa = 0;
                ft.ApiId = 0;
                ft.OpId = 0;
                ft.RecId = Convert.ToInt64(id.HasValue ? id : 0);
                ft.RefId = rf;
                ft.StatusId = 0;
                ViewBag.FilterData = TempData["ReqResFilterDto"] = ft;
            }
            catch (Exception ex)
            {
                LogException(ex, "RequestResponse DisplayReqRes");
            }
            return View();
        }


        [HttpPost]
        public ActionResult GetRequestResponses1(DataTableServerSide model)
        {

            ViewBag.actionAllowed = action = ActionAllowed("RequestResponse", CurrentUser.RoleId);            
            ReqResFilterDto ft = TempData["ReqResFilterDto"] != null ? (ReqResFilterDto)TempData["ReqResFilterDto"] : new ReqResFilterDto();
            ViewBag.FilterData = TempData["ReqResFilterDto"] = ft;
            int userrole = CurrentUser.RoleId;
            bool IsAdminRole = (userrole !=3) ? true : false;
           // model.filterdata.UserId = IsAdminRole ? model.filterdata.UserId : CurrentUser.UserID;  
            KeyValuePair<int, List<RequestResponse>> requestResponses = requestResponseService.GetRequestResponses1(model, ft);
            if (IsAdminRole)
            {
                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = requestResponses.Key,
                    recordsFiltered = requestResponses.Key,
                    data = requestResponses.Value?.Select(c => new List<object> {
                    c.Id,
                    //DataTableButton.HyperLink(Url.Action( "userdetail", "requestresponse",new { id = c.UserId }),"modal-view-user-detail", c.User.UserProfile?.FullName?.ToString(),"View User Detail"),
                    //DataTableButton.HyperLink(Url.Action( "urldetail", "requestresponse",new { id = c.UrlId }),"modal-view-url-detail", c.ApiUrl?.ApiSource?.ApiName?.ToString(),"View Url Detail"),
                    //DataTableButton.HyperLink(Url.Action( "rechargedetail", "requestresponse",new { id = c.RecId }),"modal-view-rec-detail", c.RecId?.ToString(),"View Recharge Detail"),                   
                    c.User?.UserProfile?.FullName??string.Empty,
                    c.ApiUrl?.ApiSource?.ApiName??string.Empty,
                    c.Remark,
                    (c.AddedDate).ToString(),
                    c.RequestTxt,
                    "<xmp class='txt-wrap-hide'>"+c.ResponseText+"</xmp>",
                    (c.UpdatedDate)?.ToString()??string.Empty,
                    c.RecId,
                    c.RefId,
                    c.CustomerNo,
                    c.UserTxnId,
                    c.Recharge?.Operator?.Name??string.Empty,
                    c.Recharge?.StatusType?.TypeName??string.Empty
                    })
                }, JsonRequestBehavior.AllowGet);
            }

            else
            {
                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = requestResponses.Key,
                    recordsFiltered = requestResponses.Key,
                    data = requestResponses.Value.Where(x => IsAdminRole ? true : x.Remark == "Request_R" || x.Remark == "Request_F").Select(c => new List<object> {
                    c.Id,
                    //DataTableButton.HyperLink(Url.Action( "userdetail", "requestresponse",new { id = c.UserId }),"modal-view-user-detail", c.User.UserProfile?.FullName?.ToString(),"View User Detail"),
                    //DataTableButton.HyperLink(Url.Action( "urldetail", "requestresponse",new { id = c.UrlId }),"modal-view-url-detail", c.ApiUrl?.ApiSource?.ApiName?.ToString(),"View Url Detail"),
                    //DataTableButton.HyperLink(Url.Action( "rechargedetail", "requestresponse",new { id = c.RecId }),"modal-view-rec-detail", c.RecId?.ToString(),"View Recharge Detail"),                   
                    c.User?.UserProfile?.FullName??string.Empty,
                    c.ApiUrl?.ApiSource?.ApiName??string.Empty,
                    c.Remark,
                    (c.AddedDate).ToString(),
                    c.RequestTxt,
                    "<xmp class='txt-wrap-hide'>"+c.ResponseText+"</xmp>",
                    (c.UpdatedDate)?.ToString()??string.Empty,
                    c.RecId,
                    c.RefId,
                    c.CustomerNo,
                    c.UserTxnId,
                    c.Recharge?.Operator?.Name??string.Empty,
                    c.Recharge?.StatusType?.TypeName??string.Empty
                    })
                }, JsonRequestBehavior.AllowGet);
            }
           

        }

        public ActionResult UserDetail(int? id)
        {
            //RoleDto roleDto = new RoleDto();
            //if (id.HasValue && id.Value > 0)
            //{
            //    Role role = roleService.GetAdminRole(id.Value);
            //    roleDto.Id = role.Id;
            //    roleDto.RoleName = role.RoleName;
            //}

            //return PartialView("_ViewUserDetail", roleDto);
            return View();

        }

        public ActionResult UrlDetail(int? id)
        {
            //RoleDto roleDto = new RoleDto();
            //if (id.HasValue && id.Value > 0)
            //{
            //    Role role = roleService.GetAdminRole(id.Value);
            //    roleDto.Id = role.Id;
            //    roleDto.RoleName = role.RoleName;
            //}

            //return PartialView("_ViewApiUrlDetail", roleDto);
            return View();
        }

        public ActionResult RechargeDetail(int? id)
        {
            //RoleDto roleDto = new RoleDto();
            //if (id.HasValue && id.Value > 0)
            //{
            //    Role role = roleService.GetAdminRole(id.Value);
            //    roleDto.Id = role.Id;
            //    roleDto.RoleName = role.RoleName;
            //}

            //  return PartialView("_ViewRechargeDetail", roleDto);

            return View();
        }

        private void UpdateActivity(string activityName, string ativityPage, string remark = "")
        {
            try
            {
                activityLogModel.ActivityName = activityName;
                activityLogModel.ActivityPage = ativityPage;
                activityLogModel.Remark = remark;
                activityLogModel.UserId = CurrentUser?.UserID ?? 0;
                LogActivity(activityLogModel);
            }
            catch (Exception ex)
            {

                LogException(ex);
            }

        }
    }
}