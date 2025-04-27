using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.LIBS;
using DhruvEnterprises.Web.MobipactRechargeService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SiteKey = DhruvEnterprises.Web.LIBS.SiteKey;

namespace DhruvEnterprises.Web.Controllers
{
    public class DMTReportController : BaseController
    {
        // GET: DMTReport
        #region "Fields"

        private readonly IDMTReportService DMTReportService;
        private readonly IRoleService roleService;
        private readonly IRechargeService rechargeService;
        private readonly IApiWalletService apiWalletService;
        private readonly IWalletService walletService;
        private readonly IUserService userService;
        private readonly IPackageService packageService;
        private readonly IRequestResponseService reqResService;
        private readonly IRechargeReportService rechargeReportService;

        private readonly IApiService apiService;
        private readonly IOperatorSwitchService operatorSwitchService;
        private readonly ITagValueService tagValueService;

        ActivityLogDto aLogdto;
        public ActionAllowedDto action;
        public static readonly string autochecktoken = "AutoStatusCheck-12345-EzytmDotIn";

        #endregion

        #region "Constructor"
        public DMTReportController(IDMTReportService _DMTReportService,
                                        IRoleService _userroleService,
                                        IActivityLogService _activityLogService,
                                        IRechargeService _rechargeService,
                                        IApiWalletService _apiWalletService,
                                        IWalletService _walletService,
                                         IUserService _userService,
                                         IPackageService _packageService,
                                         IRequestResponseService _reqResService,
                                         IApiService _apiService,
                                         IRechargeReportService _rechargeReportService,
                                         IOperatorSwitchService _operatorSwitchService,
                                         ITagValueService _tagValueService) : base(_activityLogService, _userroleService)
        {

            this.DMTReportService = _DMTReportService;
            this.roleService = _userroleService;
            this.rechargeService = _rechargeService;
            this.walletService = _walletService;
            this.apiWalletService = _apiWalletService;
            this.userService = _userService;
            this.packageService = _packageService;
            this.reqResService = _reqResService;
            this.rechargeReportService = _rechargeReportService;

            this.apiService = _apiService;
            this.operatorSwitchService = _operatorSwitchService;
            this.tagValueService = _tagValueService;

            this.aLogdto = new ActivityLogDto();
            this.action = new ActionAllowedDto();

        }
        #endregion
        // GET: Report
        [HttpGet]
        public ActionResult Index(int? u2, int? u, int? v, int? o, int? s, int? c, string f = "", string m = "", string e = "", int? i = 0, string rto = "", string ut = "", string ot = "", string vt = "")
        {
            UpdateActivity("RechargeReport REQUEST", "GET:RechargeReport/Index", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("DMTReport", CurrentUser.RoleId);
            var optlist = operatorSwitchService.circlesList();

            RechargeFilterDto filter = new RechargeFilterDto();

            filter.UpdatedById = Convert.ToInt32(u2.HasValue ? u2 : 0);
            filter.Uid = Convert.ToInt32(u.HasValue ? u : 0);
            filter.Isa = Convert.ToInt32(i.HasValue ? i : 0);
            filter.Apiid = Convert.ToInt32(v.HasValue ? v : 0);
            filter.Opid = Convert.ToInt32(o.HasValue ? o : 0);
            filter.Searchid = rto;
            filter.Sid = Convert.ToInt32(s.HasValue ? s : 0);
            filter.Sdate = f;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.ToString("dd/MM/yyy");
            filter.Edate = e;
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("dd/MM/yyy");
            filter.CustomerNo = m;
            filter.Circleid = Convert.ToInt32(c.HasValue ? c : 0);
            filter.UserReqid = ut;
            filter.OpTxnid = ot;
            filter.ApiTxnid = vt;

            ViewBag.FilterData = TempData["RechargeFilterDto"] = filter;

            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole >= 4 && userrole <= 6) ? true : false;
            int uid = 0;
            if (userrole >= 4 && userrole <= 6)
            {
                uid = 0;
            }
            else
            {
                uid = IsAdminRole ? filter.Uid : CurrentUser.UserID;
            }
            //bool IsAdminRole = (userrole != 3) ? true : false;
            //int uid = IsAdminRole ? filter.Uid : CurrentUser.UserID;
            //ViewBag.CircleList = operatorSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == c) }).ToList();
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == o) }).ToList();
            ViewBag.StatusList = DMTReportService.GetStatusList().Where(r => r.Remark.Contains("Recharge")).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = ((x.Id == s)) }).ToList();
            ViewBag.UserList = userService.GetUserList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = (x.Username ?? string.Empty), Selected = (x.Id == u) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == v) }).ToList();
            ViewBag.UserList2 = userService.GetUserList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = (x.Username ?? string.Empty), Selected = (x.Id == u2) }).ToList();

            return View();
        }


        public ActionResult GetRechargeReport(DataTableServerSide model, FormCollection FC)
        {
            
         ViewBag.actionAllowed = action = ActionAllowed("DMTReport", CurrentUser.Roles.FirstOrDefault());
            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;
            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole == 1 || userrole == 2) ? true : false;
            flt.Uid = IsAdminRole ? flt.Uid : CurrentUser.UserID;

            KeyValuePair<int, List<DMT>> requestResponses = DMTReportService.GetRechargeReport(model, flt.Uid, flt.Apiid, flt.Opid, Convert.ToByte(flt.Sid), flt.Searchid, flt.Sdate, flt.Edate, flt.Circleid, flt.CustomerNo, flt.UserReqid, flt.ApiTxnid, flt.OpTxnid, flt.UpdatedById);

            return Json(new
            {
                draw = model.draw,
                recordsTotal = requestResponses.Key,
                recordsFiltered = requestResponses.Key,
                data = requestResponses.Value.Select(c => new List<object> {
                    c.Id,
                    c.TxnId??c.DTxnId,
                    !IsAdminRole ?c.User?.Username+" "+ c.User?.Username :c.User?.Username+" "+c.User?.Username,
                    c.AccountNo
                    +(!string.IsNullOrEmpty(c.BeneficiaryName)? ",<br />" +c.BeneficiaryName:string.Empty)+
                    (!string.IsNullOrEmpty(c.BeneMobile)? ",<br />" +c.BeneMobile:string.Empty),
                    c.ApiSource?.ApiName,
                    c.Operator?.Name,
                    c.Amount,
                    IsAdminRole && action.AllowEdit? DataTableButton.HyperLink(Url.Action( "changestatus", "DMTReport",new { id = c.Id }),"modal-change-recharge-status", c.StatusType.TypeName,"Change Status",setColor(c.StatusId)): "<b style='color:"+setColor(c.StatusId)+"'>"+c.StatusType.TypeName+"</b>",
                   !IsAdminRole ? c.UserTxnId : c.ApiTxnId,
                 c.OptTxnId,
                    (c.RequestTime).ToString(),
                    (c.ResponseTime).ToString(),
                    //c.UserComm
                    //+(!string.IsNullOrEmpty(c.AmtType?.AmtTypeName)? ",<br />" +c.AmtType?.AmtTypeName:string.Empty),
                    c.MediumType?.TypeName??"",
                    c.OurRefTxnId,
                    c.UserTxnId,
                     (c.UpdatedDate).ToString(),
                     IsAdminRole ? c.User1?.Username??string.Empty : string.Empty,
                     action.AllowCreate && c.StatusId==1? DataTableButton.ComplaintButton(Url.Action( "createcomplaint", "DMTReport",new { id = c.Id }),"modal-generate-complaint"): string.Empty,

                    })
            }, JsonRequestBehavior.AllowGet);

        }
        private string setColor(int? id)
        {
            string color = id == 1 || id == 8 || id == 6 ? "green" : id == 2 || id == 5 ? "blue" : id == 3 ? "red" : id == 4 ? "orange" : "";
            return color;
        }
        public ActionResult ChangeStatus(long? id)
        {
            long actid = UpdateActivity("DMTReport StatusChange REQUEST", "GET:DMTReport/ChangeStatus/", "recid=" + id);
            //action = ActionAllowed("DMTReport", CurrentUser.RoleId, 3);


            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;

            RechargeUpdateDto rcStatusChangeDto = new RechargeUpdateDto();

            if (id.HasValue && id.Value > 0)
            {
                DMT recharge = DMTReportService.GetRecharge(id.Value);
                rcStatusChangeDto.RecId = recharge.Id;
                rcStatusChangeDto.StatusId = recharge.StatusId ?? 0;


                var stlist = DMTReportService.GetStatusList();

                rcStatusChangeDto.StatusList = stlist.Where(s => s.Id <= 3).Select(x => new StatusTypeDto()
                {
                    StatusId = x.Id,
                    StatusName = x.TypeName

                }).ToList();

                UpdateActivity("DMTReport StatusChange REQUEST", "GET:DMTReport/ChangeStatus/", "recid=" + id + ", oldStatus=" + recharge.StatusId, actid);

            }
            return PartialView("_ChangeStatus", rcStatusChangeDto);

        }
        [HttpPost]
        public ActionResult ChangeStatus(RechargeUpdateDto model, FormCollection FC)
        {
                    UpdateActivity("DMTReport StatusChange REQUEST", "POST:DMTReport/ChangeStatus/", "recid=" + model.RecId + ", newStatus=" + model.StatusId);
            //action = ActionAllowed("DMTReport", CurrentUser.RoleId, 3);

            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;

            string message = string.Empty;


            try
            {
                if (ModelState.IsValid)
                {
                    DMT recharge = DMTReportService.GetRecharge(model.RecId);

                    StatusChange(model.StatusId, recharge, model.OpTxnId, model.ApiTxnId);

                    ShowSuccessMessage("Success!", "Status has been changed", false);

                }
            }
            catch (Exception Ex)
            {
                message = "Internal Server Error!";
                ShowErrorMessage("Error!", message, false);

                LogException(Ex);
            }


            #region "set route params"
            IDictionary<string, object> routeValues = new Dictionary<string, object>();

            if (flt.Apiid > 0) routeValues.Add("v", flt.Apiid);
            if (flt.Opid > 0) routeValues.Add("o", flt.Opid);
            if (flt.Isa > 0) routeValues.Add("i", flt.Isa);
            if (flt.Sid > 0) routeValues.Add("s", flt.Sid);
            if (flt.Uid > 0) routeValues.Add("u", flt.Uid);
            if (flt.Circleid > 0) routeValues.Add("c", flt.Circleid);
            if (!string.IsNullOrEmpty(flt.Sdate)) routeValues.Add("f", flt.Sdate);
            if (!string.IsNullOrEmpty(flt.Edate)) routeValues.Add("e", flt.Edate);
            if (!string.IsNullOrEmpty(flt.Searchid)) routeValues.Add("rto", flt.Searchid);
            if (!string.IsNullOrEmpty(flt.CustomerNo)) routeValues.Add("m", flt.CustomerNo);
            if (!string.IsNullOrEmpty(flt.UserReqid)) routeValues.Add("ut", flt.UserReqid);
            if (!string.IsNullOrEmpty(flt.OpTxnid)) routeValues.Add("ot", flt.OpTxnid);
            if (!string.IsNullOrEmpty(flt.ApiTxnid)) routeValues.Add("vt", flt.ApiTxnid);
            #endregion

            return RedirectToAction("Index", new RouteValueDictionary(routeValues));
        }
        private string StatusChange(int statusId, DMT dmt, string optxnid, string apitxnid)
        {

            string log = "status change start, recid=" + dmt.Id;

            UpdateActivity("StatusChange REQUEST", "POST:RechargeReport/StatusChange/", log);

            int oldStatusId = dmt.StatusId ?? 0;
            int newStatusId = statusId;


            //if (oldStatusId == 1 || oldStatusId == 2 || oldStatusId == 4)
            //{
            bool IsDownline = false;
            bool IsRefund = false;

            //UpdateStatusWithCheck(dmt.Id, newStatusId, apitxnid, optxnid, dmt.StatusMsg, "Refund Manual", ref IsDownline, ref IsRefund, ref log, 0, string.Empty, 0);

            UpdateStatusWithDMTCheck(dmt.Id, (int)dmt.UserId, newStatusId, apitxnid, optxnid, dmt.StatusMsg, "Refund Manual", string.Empty, ref IsDownline, ref IsRefund, ref log, 0, string.Empty, Convert.ToInt32(dmt.OpId), dmt.BeneficiaryName);

            #region "Send Callback to User"
            log += "\r\nget user callback url ";
            User user = userService.GetUser(dmt.UserId ?? 0);
            if (user != null && oldStatusId != newStatusId)
            {
                SendCallBack(dmt, user.CallbackURL, dmt.OptTxnId, newStatusId, ref log);
            }
            else
            {
                log += "\r\nsame status or user not found ";
            }
            log += "\r\ncallback sent to user ";
            #endregion
            LogActivity(log);
            //  }

            return "";
        }
        private void SendCallBack(DMT dmt, string CallbackURL, string optxnid, int statusId, ref string log)
        {
            log += "\r\nsend callback start, ";
            if (string.IsNullOrEmpty(CallbackURL))
            {
                log += "\r\ncallback not exists, ";
            }

            else
            {

                CallbackURL = CallbackURL.ReplaceURL(string.Empty, string.Empty, dmt.UserTxnId, dmt.AccountNo, optxnid, "", dmt.Amount.ToString(), dmt.TxnId.ToString(), statusId.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Now.ToString("yyyyMMddHHmmss"), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

                log += "\r\ncallback url, " + CallbackURL + " dmtid=" + dmt.Id + ", ";

                RequestResponseDto userReqRes = new RequestResponseDto();
                userReqRes.Remark = "downline status";
                userReqRes.UserId = dmt.UserId ?? 0;
                userReqRes.RecId = dmt.Id;
                userReqRes.RefId = dmt.OurRefTxnId;
                userReqRes.RequestTxt = CallbackURL;
                //userReqRes = AddUpdateReqRes(userReqRes, ref log);
                AddDMTUpdateReqRes(userReqRes, ref log);

                ApiCall apiCall = new ApiCall(reqResService);
                apiCall.Get(CallbackURL, ref userReqRes);
                AddDMTUpdateReqRes(userReqRes, ref log);

                log += "\r\ncallback end, ";
            }

        }
        private RequestResponseDto AddDMTUpdateReqRes(RequestResponseDto model, ref string log, string filter = "NA")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_UpdateDMTDetailToReqRes", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@UrlId", model.UrlId);
                    cmd.Parameters.AddWithValue("@RecId", model.RecId);
                    cmd.Parameters.AddWithValue("@RefId", model.RefId);
                    cmd.Parameters.AddWithValue("@Remark", model.Remark);
                    cmd.Parameters.AddWithValue("@RequestTxt", model.RequestTxt);
                    cmd.Parameters.AddWithValue("@ResponseText", model.ResponseText);
                    cmd.Parameters.AddWithValue("@CustomerNo", model.CustomerNo);
                    cmd.Parameters.AddWithValue("@OpId", model.OpId);
                    cmd.Parameters.AddWithValue("@UserTxnId", model.UserReqId);
                    cmd.Parameters.AddWithValue("@UserId", model.UserId);
                    cmd.Parameters.AddWithValue("@FilterType", filter);
                    cmd.Parameters.Add("@CurrentId", SqlDbType.BigInt).Direction = ParameterDirection.Output;

                    log += "\r\n ,  before execute = usp_UpdateRecDetailToReqRes";
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    log += "\r\n ,  after execute";
                    string CurrentId = Convert.ToString(cmd.Parameters["@CurrentId"].Value);
                    if (model.Id == 0)
                        model.Id = !string.IsNullOrWhiteSpace(CurrentId) ? Convert.ToInt64(CurrentId) : 0;

                    log += "\r\n ,  reqres Id=" + model.Id;
                }
            }
            catch (Exception ex)
            {
                log += "\r\n , excp=" + ex.Message;
                LIBS.Common.LogException(ex);
            }

            return model;

        }

        [HttpGet]
        public ActionResult PendingRecharge()
        {
            UpdateActivity("PendingRecharge REQUEST", "GET:RechargeReport/PendingRecharge/", "");
            //action = ActionAllowed("PendingRecharge", CurrentUser.RoleId);

            return View();
        }
        [HttpGet]
        public ActionResult OpPendingRecharge(int? id)
        {
            TempData["ApiId"] = id ?? 0;

            UpdateActivity("OpPendingRecharge REQUEST", "GET:RechargeReport/OpPendingRecharge/", "");
            //action = ActionAllowed("PendingRecharge", CurrentUser.RoleId);

            return View();
        }
        [HttpGet]
        public ActionResult ProcessingRecharge()
        {
            UpdateActivity("ProcessingRecharge REQUEST", "GET:RechargeReport/ProcessingRecharge/", "");
            //action = ActionAllowed("ProcessingRecharge", CurrentUser.RoleId);

            return View();
        }

        public ActionResult ChangeStatusBulk()
        {

            UpdateActivity("ChangeStatusBulk REQUEST", "GET:RechargeReport/ChangeStatusBulk/", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("ProcessingRecharge", CurrentUser.RoleId, 3);

            RechargeUpdateDto rcStatusChangeDto = new RechargeUpdateDto();
            try
            {
                var stlist = DMTReportService.GetStatusList();

                rcStatusChangeDto.StatusList = stlist.Where(s => s.Id == 1 || s.Id == 3).Select(x => new StatusTypeDto()
                {
                    StatusId = x.Id,
                    StatusName = x.TypeName

                }).ToList();
            }
            catch (Exception e)
            {
                LogException(e);
            }




            return PartialView("_StatusChangeBulk", rcStatusChangeDto);

        }

        [HttpPost]
        public ActionResult ChangeStatusBulk(RechargeUpdateDto model, FormCollection FC)
        {
            UpdateActivity("ChangeStatusBulk REQUEST", "POST:RechargeReport/ChangeStatusBulk/", "RecIds=" + model.RecIds);
            ViewBag.actionAllowed = action = ActionAllowed("ProcessingRecharge", CurrentUser.RoleId, 3);

            string log = "bulkstatuschange-start recids=" + model.RecIds + ", sid=" + model.StatusId;
            int count = 0;
            int totalcount = 0;
            if (string.IsNullOrEmpty(model.RecIds))
            {
                ShowErrorMessage("Error!", "No recharge selected", false);
            }
            else
            {
                var recIdList = model.RecIds.Replace(" ", "").Split(',').ToList();

                recIdList = recIdList.Where(x => !string.IsNullOrEmpty(x)).ToList();

                if (recIdList.Count > 0)
                {
                    totalcount = recIdList.Count;

                    foreach (var rId in recIdList)
                    {

                        log += "\r\n, recid=" + rId;
                        //get recharge
                        long recid = Convert.ToInt64(rId);

                        bool IsRefund = false;
                        bool IsDownline = false;
                        string spLog = "";
                        string remark = "Refund StatusChangeBulk";
                        DMT recharge = DMTReportService.GetRecharge(recid);
                        // UpdateStatusWithCheck(recharge.Id, model.StatusId, recharge.ApiTxnId, recharge.OptTxnId, recharge.StatusMsg, remark, ref IsDownline, ref IsRefund, ref log);
                        UpdateStatusWithCheck(recharge.Id, model.StatusId, recharge.ApiTxnId, recharge.OptTxnId, recharge.StatusMsg, remark, ref IsDownline, ref IsRefund, ref log, 0, string.Empty, 0);


                        log += "\r\n, spLog=" + spLog;

                        #region "Send Callback to User"
                        log += "\r\n, get user callback url ";
                        User user = userService.GetUser(recharge.UserId ?? 0);
                        if (user != null)
                        {
                            SendCallBack(recharge, user.CallbackURL, recharge.OptTxnId, model.StatusId, ref log);
                        }
                        else
                        {
                            log += "\r\n, usernotfound";
                        }
                        log += "\r\n, callback sent to user ";
                        #endregion

                        count += 1;

                    }
                }

                ShowSuccessMessage("Success!", "Status has been changed. " + count + " out of " + totalcount, false);
            }
            log += "\r\n, end ";
            LogActivity(log);
            return RedirectToAction("ProcessingRecharge");
        }

        [HttpPost]
        public string ApCount()
        {

            return Session["apCount"] != null ? Convert.ToString(Session["apCount"]) : "0";


        }

        [HttpPost]
        public string OpCount()
        {

            return Session["opCount"] != null ? Convert.ToString(Session["opCount"]) : "0";


        }

        private string FilterRespTagValue(int apiid, int UrlId, string resType, string apires, ref string status, ref int statusId, ref string apitxnid, ref string statusmsg, ref string optxnid, ref string log, ref decimal Vendor_CL_Bal, ref decimal Vendor_OP_Bal)
        {
            string reqtxnid = "";
            var tagvalues = tagValueService.GetTagValuesByUrlId(apiid, UrlId);

            //  var tags = tagValueService.GetTagValuesByUrlId(apiid, apiurl.Id);
            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")ResType=" + resType + ",";

            if (resType.ToLower().Contains("split"))
            {

                foreach (var tg in tagvalues)
                {
                    var cmpList = new List<string>();

                    int index = Convert.ToInt16((tg.TagIndex ?? 0)) - 1;

                    if (!string.IsNullOrEmpty(tg.CompareTxt))
                    {
                        cmpList = tg.CompareTxt.Replace(" ", string.Empty).Split(',').Where(x => x != string.Empty).ToList();
                    }

                    if (index >= 0)
                    {
                        log += "\r\nindex=" + index + ",";
                        if (tg.TagId == 1) //status-success
                        {
                            try
                            {
                                status = apires.GetSplitstringByIndex(tg.ResSeparator, index);

                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-1=" + status + ", ";

                                string sval = status.ToLower();

                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                                {
                                    statusId = 1;
                                }

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")sucess status expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == 4)//status-failed
                        {
                            try
                            {
                                status = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-3=" + status + ", ";
                                string sval = status.ToLower();
                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                                {
                                    statusId = 3;
                                }
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")failed status expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }
                        }
                        else if (tg.TagId == 2) //status-processing
                        {
                            try
                            {
                                status = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-2=" + status + ", ";

                                string sval = status.ToLower();
                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                                {
                                    statusId = 2;
                                }
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")processing status expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == 3)//status-pending
                        {
                            try
                            {
                                status = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-5=" + status + ", ";
                                string sval = status.ToLower();
                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                                {
                                    statusId = 5;
                                }
                            }
                            catch (Exception ex)
                            {

                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")pending status expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == 5)//api txn id
                        {
                            try
                            {
                                apitxnid = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")apitxnid=" + apitxnid + ", ";
                            }
                            catch (Exception ex)
                            {

                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")api txnid expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }
                        }
                        else if (tg.TagId == 6) //operator txn id
                        {
                            try
                            {
                                optxnid = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")optxnid=" + optxnid + ", ";
                            }
                            catch (Exception ex)
                            {
                                log += "\r\noptr txnid expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }
                        }
                        else if (tg.TagId == 7) //status message
                        {

                            try
                            {
                                statusmsg = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")statusmsg=" + statusmsg + ", ";
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status msg expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == 8) //request txn id
                        {

                            try
                            {
                                reqtxnid = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid=" + reqtxnid + ", ";
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid msg expc= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == 9) //Vendor_CL_Bal
                        {

                            try
                            {
                                string clbal = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal=" + clbal + ", ";
                                clbal = clbal.Length > 199 ? statusmsg.Substring(0, 198) : clbal;
                                Vendor_CL_Bal = Convert.ToDecimal(clbal);
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal  excp= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == 10) //Vendor_OP_Bal
                        {

                            try
                            {
                                string opbal = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal=" + opbal + ", ";
                                opbal = opbal.Length > 50 ? opbal.Substring(0, 45) : opbal;
                                Vendor_OP_Bal = Convert.ToDecimal(opbal);
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal excp= " + ex.Message;
                                LIBS.Common.LogException(ex);
                            }

                        }
                    }

                }
            }
            else
            {
                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")tagvalue retrival,";
                foreach (var tg in tagvalues)
                {
                    var cmpList = new List<string>();
                    if (!string.IsNullOrEmpty(tg.CompareTxt))
                    {
                        cmpList = tg.CompareTxt.Split(',').Where(x => x != string.Empty).Select(s => s.Trim()).ToList();
                    }
                    if (tg.TagId == 1) //status-success
                    {
                        try
                        {
                            status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-1=" + status + ", ";

                            string sval = status.ToLower();

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                            {
                                statusId = 1;
                            }
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")sucess status expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == 4)//status-failed
                    {
                        try
                        {
                            status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-3=" + status + ", ";
                            string sval = status.ToLower();

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                            {
                                statusId = 3;
                            }
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")failed status expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }
                    }
                    else if (tg.TagId == 2) //status-processing
                    {
                        try
                        {
                            status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-2=" + status + ", ";
                            string sval = status.ToLower();

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                            {
                                statusId = 2;
                            }
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")processing status expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == 3)//status-pending
                    {
                        try
                        {
                            status = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status-5=" + status + ", ";
                            string sval = status.ToLower();

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval))
                            {
                                statusId = 5;
                            }
                        }
                        catch (Exception ex)
                        {

                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")pending status expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == 5)//api txn id
                    {
                        try
                        {

                            apitxnid = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")apitxnid=" + apitxnid + ", ";
                            apitxnid = apitxnid.Length > 50 ? apitxnid.Substring(0, 45) : apitxnid;
                        }
                        catch (Exception ex)
                        {

                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")api txnid expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }
                    }
                    else if (tg.TagId == 6) //operator txn id
                    {
                        try
                        {
                            optxnid = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\noptxnid=" + optxnid + ", ";
                            optxnid = optxnid.Length > 50 ? optxnid.Substring(0, 45) : optxnid;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")optr txnid expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == 7) //status message
                    {

                        try
                        {
                            statusmsg = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")statusmsg=" + statusmsg + ", ";
                            statusmsg = statusmsg.Length > 199 ? statusmsg.Substring(0, 198) : statusmsg;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")status msg expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == 8) //request txn id
                    {

                        try
                        {
                            reqtxnid = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid=" + reqtxnid + ", ";
                            reqtxnid = reqtxnid.Length > 50 ? reqtxnid.Substring(0, 45) : reqtxnid;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid msg expc= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == 9) //Vendor_CL_Bal
                    {

                        try
                        {
                            string clbal = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal=" + clbal + ", ";
                            clbal = clbal.Length > 199 ? statusmsg.Substring(0, 198) : clbal;
                            Vendor_CL_Bal = Convert.ToDecimal(clbal);
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal  excp= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == 10) //Vendor_OP_Bal
                    {

                        try
                        {
                            string opbal = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal=" + opbal + ", ";
                            opbal = opbal.Length > 50 ? opbal.Substring(0, 45) : opbal;
                            Vendor_OP_Bal = Convert.ToDecimal(opbal);
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal excp= " + ex.Message;
                            LIBS.Common.LogException(ex);
                        }

                    }
                }
            }

            return reqtxnid;
        }

        private void CheckStatus(Recharge recharge, string remark, ref bool IsDownline, ref string apitxnid, ref string statusmsg, ref string optxnid, ref int statusId, ref string log)
        {
            string status = "";
            decimal Vendor_CL_Bal = 0;
            decimal Vendor_OP_Bal = 0;

            var stChkUrl = apiService.GetApiurl(recharge.ApiId ?? 0, 3);
            var apisource = apiService.GetApiSource(recharge.ApiId ?? 0);
            var opcode = operatorSwitchService.OpcodeApiList(recharge.OpId ?? 0).Where(x => x.ApiId == recharge.ApiId).FirstOrDefault();
            var circlecode = recharge.Circle != null ? recharge.Circle.CircleCode : "00";


            #region "APi Call"

            RequestResponseDto requestResponse = new RequestResponseDto();
            ApiCall apiCall = new ApiCall(reqResService);

            // log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")apicall, ";

            int unitid = 0;
            int len = 0;
            int reflen = 0;
            string randomkey = "";
            string dtformat = "";
            string refpadding = "";
            string apiamount = "";
            string ourref = "";
            string datetime = "";

            GetApiExtDetails(recharge.ApiId ?? 0, ref unitid, ref len, ref randomkey, ref dtformat, ref refpadding, ref log, ref reflen);
            apiamount = unitid == 2 ? (recharge.Amount ?? 0 * 100).ToString("D" + len) : recharge.Amount.ToString();
            ourref = reflen > 0 ? (reflen < recharge.OurRefTxnId.Length ? recharge.OurRefTxnId.Remove(0, recharge.OurRefTxnId.Length - reflen) : refpadding + recharge.OurRefTxnId) : recharge.OurRefTxnId;

            var url = stChkUrl.URL?.ReplaceURL(apisource.ApiUserId, apisource.ApiPassword, apisource.Remark, recharge.CustomerNo, opcode.OpCode, circlecode, apiamount, ourref, "", recharge.ApiTxnId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            var postdata = !string.IsNullOrEmpty(stChkUrl.PostData) ? stChkUrl.PostData?.ReplaceURL(apisource.ApiUserId, apisource.ApiPassword, apisource.Remark, recharge.CustomerNo, opcode.OpCode, circlecode, apiamount, ourref, "", recharge.ApiTxnId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) : "";

            url = url?.ReplaceURL(apisource.ApiUserId, apisource.ApiPassword, apisource.Remark, recharge.CustomerNo, opcode.OpCode, circlecode, apiamount, ourref, "", recharge.ApiTxnId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
            postdata = !string.IsNullOrEmpty(postdata) ? postdata?.ReplaceURL(apisource.ApiUserId, apisource.ApiPassword, apisource.Remark, recharge.CustomerNo, opcode.OpCode, circlecode, apiamount, ourref, "", recharge.ApiTxnId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) : "";

            string apires = string.Empty;
            if (apisource.Id == 12)
            {
                RechargeAPI mobiPactservice = new RechargeAPI();
                apires = mobiPactservice.Status(apisource.ApiUserId, apisource.ApiPassword, recharge.OurRefTxnId);
                requestResponse.ResponseText = apires;
            }
            else
            {
                apires = stChkUrl.Method == "POST" ? apiCall.Post(url, postdata, stChkUrl.ContentType, stChkUrl.ResType, ref requestResponse, recharge.ApiId ?? 0, apisource.ApiUserId, apisource.ApiPassword)
                                                                         : apiCall.Get(url, ref requestResponse, recharge.ApiId ?? 0);

            }

            requestResponse.RecId = Convert.ToInt64(recharge.Id);
            requestResponse.RefId = recharge.OurRefTxnId;
            requestResponse.Remark = "Status";
            requestResponse = AddUpdateReqRes(requestResponse, ref log);

            #endregion

            string reqtxnid = FilterRespTagValue(recharge.ApiId ?? 0, stChkUrl.Id, stChkUrl.ResType, apires, ref status, ref statusId, ref apitxnid, ref statusmsg, ref optxnid, ref log, ref Vendor_CL_Bal, ref Vendor_OP_Bal);

        }

        public ActionResult ResendRechargeBulk()
        {
            UpdateActivity("ResendRechargeBulk REQUEST", "GET:RechargeReport/ResendRechargeBulk/", "");
            ViewBag.actionAllowed = action = ActionAllowed("ProcessingRecharge", CurrentUser.RoleId, 3);

            RechargeUpdateDto rcStatusChangeDto = new RechargeUpdateDto();
            rcStatusChangeDto.IsActive = true;
            try
            {
                var stlist = apiService.GetApiList();

                rcStatusChangeDto.ApiList = stlist.Select(x => new ApiSourceDTO()
                {
                    Apiid = x.Id,
                    Name = x.ApiName

                }).ToList();
            }
            catch (Exception e)
            {
                LogException(e);
            }

            return PartialView("_ResendRechargeBulk", rcStatusChangeDto);

        }


        private void UpdateStatusWithCheck(long RecId, int statusId, string apitxnid, string optxnid, string statusmsg, string remark, ref bool IsDownline, ref bool IsRefund, ref string log, long lapuid, string lapuno, int opid, string updatetype = "StatusWithCheck")
        {
            using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
            {
                SqlCommand cmd = new SqlCommand("usp_UpdateDMTStatus", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UpdateType", updatetype);
                cmd.Parameters.AddWithValue("@RecId", RecId);
                cmd.Parameters.AddWithValue("@StatusId", statusId);

                cmd.Parameters.AddWithValue("@UserId", CurrentUser != null ? CurrentUser.UserID : 1);

                if (!string.IsNullOrWhiteSpace(apitxnid))
                    cmd.Parameters.AddWithValue("@ApiTxnId", apitxnid);

                if (!string.IsNullOrWhiteSpace(optxnid))
                    cmd.Parameters.AddWithValue("@OptTxnId", optxnid);

                if (!string.IsNullOrWhiteSpace(statusmsg))
                    cmd.Parameters.AddWithValue("@StatusMsg", statusmsg);

                if (!string.IsNullOrWhiteSpace(remark))
                    cmd.Parameters.AddWithValue("@Remark", remark);

                if (lapuid > 0)
                    cmd.Parameters.AddWithValue("@LapuId", lapuid);

                if (!string.IsNullOrWhiteSpace(lapuno))
                    cmd.Parameters.AddWithValue("@LapuNo", lapuno);

                if (opid > 0)
                    cmd.Parameters.AddWithValue("@OpId", opid);

                cmd.Parameters.Add("@Log", SqlDbType.NVarChar, 250).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@IsRefund", SqlDbType.Bit).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@IsDownline", SqlDbType.Bit).Direction = ParameterDirection.Output;

                log += "\r\n ,  before exec = usp_UpdateRechargeStatus";
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                log += "\r\n ,  after exec = usp_UpdateRechargeStatus";
                string Refund = Convert.ToString(cmd.Parameters["@IsRefund"].Value);
                string Downline = Convert.ToString(cmd.Parameters["@IsDownline"].Value);
                string spLog = Convert.ToString(cmd.Parameters["@Log"].Value);


                IsRefund = !string.IsNullOrEmpty(Refund) ? Convert.ToBoolean(Refund) : false;
                IsDownline = !string.IsNullOrEmpty(Downline) ? Convert.ToBoolean(Downline) : false;

                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")spLog=" + spLog;
            }

            SetRechargeUpdatedBy("Update", RecId, CurrentUser != null ? CurrentUser.UserID : 1, string.Empty, ref log);

        }
        private void UpdateStatusWithDMTCheck(long RecId, int UserId, int statusId, string apitxnid, string optxnid, string statusmsg, string remark, string fsssion, ref bool IsDownline, ref bool IsRefund, ref string log, long lapuid, string lapuno, int opid, string BeneName, string updatetype = "StatusWithCheck")
        {
            using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
            {
                SqlCommand cmd = new SqlCommand("usp_UpdateDMTStatus", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UpdateType", updatetype);
                cmd.Parameters.AddWithValue("@RecId", RecId);
                cmd.Parameters.AddWithValue("@StatusId", statusId);
                cmd.Parameters.AddWithValue("@UserId", UserId);
                if (!string.IsNullOrWhiteSpace(apitxnid))
                    cmd.Parameters.AddWithValue("@ApiTxnId", apitxnid);

                if (!string.IsNullOrWhiteSpace(optxnid))
                    cmd.Parameters.AddWithValue("@OptTxnId", optxnid);

                if (!string.IsNullOrWhiteSpace(statusmsg))
                    cmd.Parameters.AddWithValue("@StatusMsg", statusmsg);

                if (!string.IsNullOrWhiteSpace(BeneName))
                    cmd.Parameters.AddWithValue("@BeneName", BeneName);

                if (!string.IsNullOrWhiteSpace(remark))
                    cmd.Parameters.AddWithValue("@Remark", remark);

                if (lapuid > 0)
                    cmd.Parameters.AddWithValue("@LapuId", lapuid);

                if (!string.IsNullOrWhiteSpace(lapuno))
                    cmd.Parameters.AddWithValue("@LapuNo", lapuno);
                if (!string.IsNullOrWhiteSpace(fsssion))
                    cmd.Parameters.AddWithValue("@FessionNo", fsssion);
                if (opid > 0)
                    cmd.Parameters.AddWithValue("@OpId", opid);

                cmd.Parameters.Add("@Log", SqlDbType.NVarChar, 250).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@IsRefund", SqlDbType.Bit).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@IsDownline", SqlDbType.Bit).Direction = ParameterDirection.Output;

                log += "\r\n ,  before exec = usp_UpdateDMTStatus";
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                log += "\r\n ,  after exec = usp_UpdateDMTStatus";
                string Refund = Convert.ToString(cmd.Parameters["@IsRefund"].Value);
                string Downline = Convert.ToString(cmd.Parameters["@IsDownline"].Value);
                string spLog = Convert.ToString(cmd.Parameters["@Log"].Value);


                IsRefund = !string.IsNullOrEmpty(Refund) ? Convert.ToBoolean(Refund) : false;
                IsDownline = !string.IsNullOrEmpty(Downline) ? Convert.ToBoolean(Downline) : false;

                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")spLog=" + spLog;
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

        public ActionResult CreateComplaint(long? id)
        {
            UpdateActivity("CreateComplaint REQUEST", "GET:RechargeReport/CreateComplaint/", "id=" + id);
            var action = ActionAllowed("MyComplaint", CurrentUser.RoleId, 2);

            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;


            ComplaintDto model = new ComplaintDto();



            if (id.HasValue && id.Value > 0)
            {
                model.RecId = id ?? 0;
            }

            return PartialView("_CreateComplaint", model);

        }


        // GET: Report
        [HttpGet]
        public ActionResult Complaint(int? u, int? v, int? o, int? s, int? c, string f = "", string m = "", string e = "", int? i = 0)
        {
            UpdateActivity("Complaint Report Request", "GET:RechargeReport/Complaint", string.Empty);
            action = ActionAllowed("Complaint", CurrentUser.RoleId);
            var optlist = operatorSwitchService.circlesList();

            RechargeFilterDto filter = new RechargeFilterDto();

            filter.Uid = Convert.ToInt32(u.HasValue ? u : 0);
            filter.Isa = Convert.ToInt32(i.HasValue ? i : 0);
            filter.Apiid = Convert.ToInt32(v.HasValue ? v : 0);
            filter.Opid = Convert.ToInt32(o.HasValue ? o : 0);
            filter.Sid = Convert.ToInt32(s.HasValue ? s : 5);
            filter.Sdate = f;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.ToString("dd/MM/yyy");
            filter.Edate = e;
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("dd/MM/yyy");
            filter.CustomerNo = m;
            filter.Circleid = Convert.ToInt32(c.HasValue ? c : 0);


            ViewBag.FilterData = TempData["RechargeFilterDto"] = filter;

            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? 0 : CurrentUser.UserID;


            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == filter.Opid) }).ToList();
            ViewBag.StatusList = DMTReportService.GetStatusList().Where(r => r.Remark.Contains("Complaint")).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = ((x.Id == filter.Sid)) }).ToList();
            ViewBag.CircleList = operatorSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == filter.Circleid) }).ToList();
            ViewBag.UserList = userService.GetUserList().Where(x => uid == 0 ? true : x.Id == uid && x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = (x.Username ?? string.Empty), Selected = (x.Id == filter.Uid) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Where(x => uid == 0 ? true : x.Id == uid).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == filter.Apiid) }).ToList();

            return View();
        }

        [HttpPost]
        public ActionResult GetComplaintReport(DataTableServerSide model, FormCollection FC)
        {

            ViewBag.actionAllowed = action = ActionAllowed("Complaint", CurrentUser.RoleId);

            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;

            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            flt.Uid = IsAdminRole ? flt.Uid : CurrentUser.UserID;

            KeyValuePair<int, List<Complaint>> requestResponses = DMTReportService.GetComplaintReport(model, flt.Uid, flt.Apiid, flt.Opid, Convert.ToByte(flt.Sid), flt.Sdate, flt.Edate, flt.Circleid, flt.CustomerNo);

            return Json(new
            {
                draw = model.draw,
                recordsTotal = requestResponses.Key,
                recordsFiltered = requestResponses.Key,
                data = requestResponses.Value.Select(c => new List<object> {
                    c.Id,
                    IsAdminRole|| c.ComplaintById==c.Recharge.UserId?c.User.Username: c.ComplaintById==c.Recharge.UserId? c.Recharge.User.Username:"Admin",
                    (c.ComplaintDate)?.ToString(),
                    (IsAdminRole && action.AllowEdit? DataTableButton.HyperLink(Url.Action( "resolvecomplaint", "rechargereport",new { id = c.Id }),"modal-resolve-complaint", c.StatusType.TypeName,"Resolve",setColor(c.StatusId)): "<b style='color:"+setColor(c.StatusId)+"'>"+c.StatusType.TypeName+"</b>")
                    +(IsAdminRole && action.AllowEdit? "&nbsp;"+ DataTableButton.RefreshButton(Url.Action("checkcomplaint","rechargereport",new { cid=c.Id})): string.Empty)
                    ,
                    c.RecId,
                     c.Recharge.OurRefTxnId,
                      c.Recharge.TxnId,
                       c.Recharge?.StatusType?.TypeName??string.Empty,
                       (c.Recharge.RequestTime)?.ToString(),


                    c.Recharge.CustomerNo,
                    c.Recharge.Operator.Name,
                    c.Recharge?.Amount??0,
                    c.Recharge?.TxnLedger?.DB_Amt,
                    IsAdminRole?c.Recharge.ApiSource?.ApiName??string.Empty:c.Recharge.OptTxnId,

                    (c.ResolvedDate)?.ToString(),
                    c.IsRefund==true?"Yes":"",
                     c.RefundTxnId,
                     IsAdminRole?c.User1?.Username??string.Empty: c.User1!=null?"Admin": string.Empty

                    })
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ResolveComplaint(long? id)
        {
            UpdateActivity("ResolveComplain REQUEST", "GET:RechargeReport/ResolveComplain/", "cmpid=" + id);
            action = ActionAllowed("Complaint", CurrentUser.RoleId, 3);


            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;


            ComplaintDto model = new ComplaintDto();

            if (id.HasValue && id.Value > 0)
            {
                Complaint complaint = DMTReportService.GetComplaint(id.Value);

                model.ComplaintId = complaint.Id;
                model.RecId = complaint.RecId ?? 0;
                model.StatusId = complaint.StatusId ?? 0;
                var stlist = DMTReportService.GetStatusList().Where(s => s.Remark.Contains("Complaint")).Select(x => new StatusTypeDto()
                {
                    StatusId = x.Id,
                    StatusName = x.TypeName

                }).ToList();

            }

            return PartialView("_ResolveComplaint", model);

        }

        [HttpPost]
        public ActionResult ResolveComplaint(ComplaintDto model, FormCollection FC)
        {
            UpdateActivity("ResolveComplain REQUEST", "POST:RechargeReport/ResolveComplain/", "cmpid=" + model.ComplaintId + ", IsResolved=" + model.IsResolved + ", IsRefund=" + model.IsRefund);
            action = ActionAllowed("Complaint", CurrentUser.RoleId, 3);

            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;

            string message = string.Empty;
            string log = "ResolveComplain-start";
            Complaint complaint = DMTReportService.GetComplaint(model.ComplaintId);
            DMT recharge = DMTReportService.GetRecharge(model.RecId);
            string optxnid = !string.IsNullOrWhiteSpace(model.OptTxnId) ? model.OptTxnId : recharge.OptTxnId;

            if (complaint.StatusId == 5 || complaint.StatusId == 2)
            {
                complaint.UpdatedById = CurrentUser.UserID;
                complaint.StatusId = model.IsResolved ? Convert.ToByte(8) : Convert.ToByte(2); //8-resolved, 2-processing

                log += "\r\n model.IsResolved=" + model.IsResolved;
                if (model.IsRefund)
                {
                    complaint.IsRefund = model.IsRefund;

                    log += "\r\n\r\n model.IsRefund=" + model.IsRefund;

                    bool IsDownline = false;
                    bool IsRefund = false;

                    // UpdateStatusWithCheck(recharge.Id, 3, recharge.ApiTxnId, optxnid, recharge.StatusMsg, "Refund Complaint", ref IsDownline, ref IsRefund, ref log);
                    UpdateStatusWithCheck(recharge.Id, 3, recharge.ApiTxnId, optxnid, recharge.StatusMsg, "Refund Complaint", ref IsDownline, ref IsRefund, ref log, 0, string.Empty, 0);

                }

                DMTReportService.Save(complaint);

                if (model.IsResolved && !string.IsNullOrWhiteSpace(model.OptTxnId))
                    SetRechargeUpdatedBy("Complain", model.RecId, CurrentUser.UserID, optxnid, ref log);

                ShowSuccessMessage("Success!", "Complain Updated", false);
            }
            else
            {
                ShowErrorMessage("Error!", "Complain Cann't be Updated", false);
            }



            #region "set route params"
            IDictionary<string, object> routeValues = new Dictionary<string, object>();

            if (flt.Apiid > 0) routeValues.Add("v", flt.Apiid);
            if (flt.Opid > 0) routeValues.Add("o", flt.Opid);
            if (flt.Isa > 0) routeValues.Add("i", flt.Isa);
            if (flt.Sid > 0) routeValues.Add("s", flt.Sid);
            if (flt.Uid > 0) routeValues.Add("u", flt.Uid);
            if (flt.Circleid > 0) routeValues.Add("c", flt.Circleid);
            if (!string.IsNullOrEmpty(flt.Sdate)) routeValues.Add("f", flt.Sdate);
            if (!string.IsNullOrEmpty(flt.Edate)) routeValues.Add("e", flt.Edate);
            if (!string.IsNullOrEmpty(flt.CustomerNo)) routeValues.Add("m", flt.CustomerNo);

            #endregion

            log += "\r\n\r\n ResolveComplain-end";

            LogActivity(log);

            return RedirectToAction("complaint", new RouteValueDictionary(routeValues));
        }

        public ActionResult CheckComplaint(long? cid)
        {
            string log = string.Empty;
            action = ActionAllowed("Complaint", CurrentUser.RoleId);

            try
            {

                long complaintId = 0;

                complaintId = cid.HasValue ? cid.Value : 0;

                var complaint = rechargeService.GetComplaint(complaintId);
                long recid = complaint.RecId ?? 0;
                Recharge recharge = rechargeService.GetRecharge(recid);
                var apiurl = apiService.GetApiurl(recharge.ApiId ?? 0, 7);

                int urlid = apiurl.Id;

                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")complaintId=" + complaintId + " recid=" + recid + " urlid=" + urlid;



                if (urlid > 0 && recid > 0)
                {


                    ApiUrl apiUrl = apiService.GetApiurl(urlid);
                    ApiSource apiSource = apiService.GetApiSource(apiUrl.ApiId);

                    string postdata = apiUrl.PostData;
                    string complainUrl = apiUrl.URL;
                    var oprator = rechargeService.GetOperator("", recharge.OpId ?? 0);
                    var opcode = rechargeService.GetOperatorByApiId(recharge.OpId ?? 0, recharge.ApiId ?? 0);


                    int unitid = 0;
                    int len = 0;
                    int reflen = 0;
                    string randomkey = "";
                    string dtformat = "";
                    string refpadding = "";
                    string datetime = "";

                    GetApiExtDetails(apiUrl.ApiId ?? 0, ref unitid, ref len, ref randomkey, ref dtformat, ref refpadding, ref log, ref reflen);
                    datetime = DateTime.Now.ToString(dtformat);

                    complainUrl = complainUrl.ReplaceURL(apiSource.ApiUserId, apiSource.ApiPassword, apiSource.Remark, complaint?.Id.ToString(), opcode?.OpCode ?? "", "", recharge.Amount.ToString(), recharge.OurRefTxnId, string.Empty, string.Empty, opcode.ExtraUrl, opcode.ExtraUrlData, recharge.AccountNo, recharge.AccountOther, recharge.Optional1, recharge.Optional2, recharge.Optional3, recharge.Optional4, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                    postdata = !string.IsNullOrEmpty(postdata) ? postdata?.ReplaceURL(apiSource.ApiUserId, apiSource.ApiPassword, apiSource.Remark, complaint?.Id.ToString(), opcode?.OpCode ?? "", "", recharge.Amount.ToString(), recharge.OurRefTxnId, string.Empty, string.Empty, opcode.ExtraUrl, opcode.ExtraUrlData, recharge.AccountNo, recharge.AccountOther, recharge.Optional1, recharge.Optional2, recharge.Optional3, recharge.Optional4, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) : "";

                    complainUrl = complainUrl.ReplaceURL(apiSource.ApiUserId, apiSource.ApiPassword, apiSource.Remark, complaint?.Id.ToString(), opcode?.OpCode ?? "", "", recharge.Amount.ToString(), recharge.OurRefTxnId, string.Empty, string.Empty, opcode.ExtraUrl, opcode.ExtraUrlData, recharge.AccountNo, recharge.AccountOther, recharge.Optional1, recharge.Optional2, recharge.Optional3, recharge.Optional4, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
                    postdata = !string.IsNullOrEmpty(postdata) ? postdata?.ReplaceURL(apiSource.ApiUserId, apiSource.ApiPassword, apiSource.Remark, complaint?.Id.ToString(), opcode?.OpCode ?? "", "", recharge.Amount.ToString(), recharge.OurRefTxnId, string.Empty, string.Empty, opcode.ExtraUrl, opcode.ExtraUrlData, recharge.AccountNo, recharge.AccountOther, recharge.Optional1, recharge.Optional2, recharge.Optional3, recharge.Optional4, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) : "";
                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")after replace" + " complainUrl=" + apiurl.URL + " PostData=" + apiurl.PostData;

                    #region "APi Call"

                    RequestResponseDto requestResponse = new RequestResponseDto();

                    if (Convert.ToInt32(recharge.UserId) > 0)
                    {
                        requestResponse.UserId = Convert.ToInt32(recharge.UserId);
                    }

                    if (complainUrl != null)
                        requestResponse.UrlId = Convert.ToInt32(apiUrl.Id);

                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + "), (set reqres) userid=" + recharge.UserId + " urlid=" + apiUrl.Id;

                    requestResponse.Remark = "Complain_S";
                    ApiCall apiCall = new ApiCall(reqResService);

                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")apicall, ";

                    string apires = apiUrl.Method == "POST" ?
                                    apiCall.Post(complainUrl, postdata, apiUrl.ContentType, apiUrl.ResType, ref requestResponse, apiurl.ApiId ?? 0, apiSource.ApiUserId, apiSource.ApiPassword)
                                    : apiCall.Get(complainUrl, ref requestResponse, apiurl.ApiId ?? 0);

                    requestResponse.RecId = recharge.Id;
                    requestResponse.RefId = recharge.OurRefTxnId;
                    requestResponse = AddUpdateReqRes(requestResponse, ref log);

                    string status = "Hold";
                    string optxnid = "NA";
                    string apitxnid = "";
                    string reqtxnid = "";
                    decimal Vendor_CL_Bal = 0;
                    decimal Vendor_OP_Bal = 0;
                    int statusId = 2;
                    string statusmsg = "";

                    reqtxnid = FilterRespTagValue(recharge.ApiId ?? 0, apiurl.Id, apiUrl.ResType, apires, ref status, ref statusId, ref apitxnid, ref statusmsg, ref optxnid, ref log, ref Vendor_CL_Bal, ref Vendor_OP_Bal);
                    // UpdateComplaint(complaintId, apitxnid, statusmsg, ref log);
                    if (statusId == 1 && (complaint.StatusId == 5 || complaint.StatusId == 2))
                    {
                        complaint.StatusId = 8;
                        complaint.Comment = "Auto Closed";
                        // complaint.ApiComplaintId = apitxnid;
                        // complaint.ApiResponse = statusmsg;
                        rechargeService.Save(complaint);
                    }

                }

                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")resp saved to rec, ";
                #endregion

            }
            catch (Exception ex)
            {
                log += "\r\n , excp complaint=" + ex.Message;
                Core.Common.LogException(ex);
            }

            return RedirectToAction("Complaint", new { });
        }

        private RequestResponseDto AddUpdateReqRes(RequestResponseDto model, ref string log, string filter = "NA")
        {
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_UpdateRecDetailToReqRes", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@UrlId", model.UrlId);
                    cmd.Parameters.AddWithValue("@RecId", model.RecId);
                    cmd.Parameters.AddWithValue("@RefId", model.RefId);
                    cmd.Parameters.AddWithValue("@Remark", model.Remark);
                    cmd.Parameters.AddWithValue("@RequestTxt", model.RequestTxt);
                    cmd.Parameters.AddWithValue("@ResponseText", model.ResponseText);
                    cmd.Parameters.AddWithValue("@FilterType", filter);
                    cmd.Parameters.Add("@CurrentId", SqlDbType.BigInt).Direction = ParameterDirection.Output;

                    log += "\r\n ,  before execute = usp_UpdateRecDetailToReqRes";
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    log += "\r\n ,  after execute";
                    string CurrentId = Convert.ToString(cmd.Parameters["@CurrentId"].Value);
                    if (model.Id == 0)
                        model.Id = !string.IsNullOrWhiteSpace(CurrentId) ? Convert.ToInt64(CurrentId) : 0;

                    log += "\r\n ,  reqres Id=" + model.Id;
                }
            }
            catch (Exception ex)
            {
                log += "\r\n , excp=" + ex.Message;
                LogException(ex);
            }

            return model;

        }

        private void SetRechargeUpdatedBy(string filter, long recid, int userid, string remark, ref string log)
        {
            log += "\r\n , SetRechargeUpdatedBy, recid=" + recid + " userid=" + userid + " filter=" + filter + " remark=" + remark;
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_SetRechargeUpdatedBy", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FilterType", filter);
                    cmd.Parameters.AddWithValue("@RecId", recid);
                    cmd.Parameters.AddWithValue("@UserId", userid);
                    cmd.Parameters.AddWithValue("@Remark", remark);


                    log += "\r\n ,  before execute = usp_SetRechargeUpdatedBy";
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    log += "\r\n ,  after execute";

                }
            }
            catch (Exception ex)
            {
                log += "\r\n , excp=" + ex.Message;
                LogException(ex);
            }

        }

        private void GetApiExtDetails(int apiid, ref int unitid, ref int len, ref string randomkey, ref string dtformat, ref string refpadding, ref string log, ref int reflen)
        {

            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_GetApiValidation", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ApiId", apiid);
                    cmd.Parameters.Add("@AmountUnitId", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@AmountLength", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@RandomKey", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@DateTimeFormat", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@RefPadding", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@RefLength", SqlDbType.Int).Direction = ParameterDirection.Output;

                    // log += "\r\n ,  before execute = usp_UpdateRecDetailToReqRes";
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    // log += "\r\n ,  after execute";
                    string AmountUnitId = Convert.ToString(cmd.Parameters["@AmountUnitId"].Value);
                    string AmountLength = Convert.ToString(cmd.Parameters["@AmountLength"].Value);
                    randomkey = Convert.ToString(cmd.Parameters["@RandomKey"].Value);
                    string DateTimeFormat = Convert.ToString(cmd.Parameters["@DateTimeFormat"].Value);
                    refpadding = Convert.ToString(cmd.Parameters["@RefPadding"].Value);
                    string RefLength = Convert.ToString(cmd.Parameters["@RefLength"].Value);

                    unitid = !string.IsNullOrWhiteSpace(AmountUnitId) ? Convert.ToInt32(AmountUnitId) : 0;
                    len = !string.IsNullOrWhiteSpace(AmountLength) ? Convert.ToInt32(AmountLength) : 0;
                    reflen = !string.IsNullOrWhiteSpace(RefLength) ? Convert.ToInt32(RefLength) : 0;
                    dtformat = !string.IsNullOrWhiteSpace(DateTimeFormat) ? DateTimeFormat : "yyyyMMddHHmmss";


                    // log += "\r\n ,  reqres Id=" + model.Id;
                }
            }
            catch (Exception ex)
            {
                // log += "\r\n , excp=" + ex.Message;
                LogException(ex);
            }

        }


    }
}