using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.LIBS;
using DhruvEnterprises.Web.MobipactRechargeService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using NPOI.SS.Formula.Functions;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Web.Configuration;
using Org.BouncyCastle.Bcpg;

namespace DhruvEnterprises.Web.Controllers
{
    public class RechargeReportController : BaseController
    {
        #region "Fields"

        private readonly IRechargeReportService rechargeReportService;
        private readonly IRoleService roleService;
        private readonly IRechargeService rechargeService;
        private readonly IApiWalletService apiWalletService;
        private readonly IWalletService walletService;
        private readonly IUserService userService;
        private readonly IPackageService packageService;
        private readonly IRequestResponseService reqResService;
        private readonly IDMTReportService DMTReportService;
        private readonly IApiService apiService;
        private readonly IOperatorSwitchService operatorSwitchService;
        private readonly ITagValueService tagValueService;

        ActivityLogDto aLogdto;
        public ActionAllowedDto action;
        public static readonly string autochecktoken = "AutoStatusCheck-12345-SHakti";
        public static int autocheck = 0;
        public static int checktime = 60;
        #endregion

        #region "Constructor"
        public RechargeReportController(IRechargeReportService _rechargeReportService,
            IDMTReportService _DMTReportService,
                                        IRoleService _userroleService,
                                        IActivityLogService _activityLogService,
                                        IRechargeService _rechargeService,
                                        IApiWalletService _apiWalletService,
                                        IWalletService _walletService,
                                        IUserService _userService,
                                        IPackageService _packageService,
                                        IRequestResponseService _reqResService,
                                        IApiService _apiService,
                                        IOperatorSwitchService _operatorSwitchService,
                                        ITagValueService _tagValueService) : base(_activityLogService, _userroleService)
        {

            this.rechargeReportService = _rechargeReportService;
            this.DMTReportService = _DMTReportService;
            this.roleService = _userroleService;
            this.rechargeService = _rechargeService;
            this.walletService = _walletService;
            this.apiWalletService = _apiWalletService;
            this.userService = _userService;
            this.packageService = _packageService;
            this.reqResService = _reqResService;

            this.apiService = _apiService;
            this.operatorSwitchService = _operatorSwitchService;
            this.tagValueService = _tagValueService;

            this.aLogdto = new ActivityLogDto();
            this.action = new ActionAllowedDto();

        }
        #endregion
        // GET: Report
        [HttpGet]
        public ActionResult Index(int? u2, int? u, int? v, int? o, int? s, int? c, string f = "", string m = "", string e = "", int? i = 0, string rto = "", string ut = "", string ot = "", string vt = "", FilterData fdata = null)
        {
            TempData["RechargeFilterDto"] = null;

            var str = "userrole=" + CurrentUser.RoleId +
                      ", CurrentUser.UserID=" + CurrentUser.UserID +
                      ", u=" + u +
                      ", v=" + v +
                      ", o=" + o +
                      ", s=" + s +
                      ", rto=" + rto +
                      ", f=" + f +
                      ",e=" + e +
                      ", c=" + c +
                      ", m=" + m +
                      ", ut=" + ut +
                      ", vt=" + vt +
                      ", ot=" + ot +
                      ", u2=" + u2;

            LogActivity(str);

            UpdateActivity("RechargeReport REQUEST", "GET:RechargeReport/Index", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("RechargeReport", CurrentUser.RoleId);

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
            filter.Edate = e;
            //filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd hh:mm:ss");
            //filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyy");
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("dd/MM/yyy");
            filter.CustomerNo = m;
            filter.Circleid = Convert.ToInt32(c.HasValue ? c : 0);
            filter.UserReqid = ut;
            filter.OpTxnid = ot;
            filter.ApiTxnid = vt;

            ViewBag.FilterData = TempData["RechargeFilterDto"] = filter;//co

            int uid = CurrentUser.RoleId != 3 ? filter.Uid : CurrentUser.UserID;

            //ViewBag.CircleList = operatorSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == filter.Circleid) }).ToList();
            //ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == filter.Opid) }).ToList();
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == filter.Uid) }).ToList();
            //ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == filter.Apiid) }).ToList();
            ViewBag.UserList2 = userService.GetUserList().Where(x => x.RoleId != 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == filter.UpdatedById) }).ToList();


            var statusList = rechargeReportService.GetStatusList().Where(r => r.Remark.Contains("Recharge") && r.Id != 5).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = ((x.Id == filter.Sid)) }).ToList();

            if (CurrentUser.RoleId != 3)
                statusList.Insert(statusList.Count, new SelectListItem() { Value = "11", Text = "Manual Updated", Selected = (filter.Sid == 11) });

            ViewBag.StatusList = statusList;
            return View();
        }
        [HttpPost]
        public ActionResult GetRechargeReport(DataTableServerSide model)
        {
            string log = string.Empty;
            ViewBag.actionAllowed = action = ActionAllowed("RechargeReport", CurrentUser.RoleId);
            var action2 = ActionAllowed("Complaint", CurrentUser.RoleId);

            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;

            int userrole = CurrentUser.RoleId;
            bool IsAdminRole = (userrole != 3) ? true : false;
            model.filterdata.UserId = IsAdminRole ? model.filterdata.UserId : CurrentUser.UserID;
            KeyValuePair<int, List<Recharge>> recharges = new KeyValuePair<int, List<Recharge>>();
            try
            {
                recharges = rechargeReportService.GetRechargeReport(model, flt.Uid, flt.Apiid, flt.Opid, Convert.ToByte(model.filterdata?.StatusId ?? 0), flt.Searchid, flt.Sdate, flt.Edate, flt.Circleid, flt.CustomerNo, flt.UserReqid, flt.ApiTxnid, flt.OpTxnid, flt.UpdatedById, ref log);


            }
            catch (Exception ex)
            {

                var innerException = ex.InnerException;
                LogException(ex, "recharge report");
                try
                {
                    recharges = rechargeReportService.GetRechargeReport(model, flt.Uid, flt.Apiid, flt.Opid, Convert.ToByte(flt.Sid), flt.Searchid, flt.Sdate, flt.Edate, flt.Circleid, flt.CustomerNo, flt.UserReqid, flt.ApiTxnid, flt.OpTxnid, flt.UpdatedById, ref log);

                }
                catch (Exception ex2)
                {
                    LogException(ex, "ex2-recharge report");
                }
            }
            return Json(new
            {
                draw = model.draw,
                recordsTotal = recharges.Key,
                recordsFiltered = recharges.Key,
                data = recharges.Value.Select(c => new List<object> {
                   ( action2.AllowView && c.Complaints.Any(x=>x.RecId==c.Id) && c.StatusId==1?  DataTableButton.ExtLinkRedButton(Url.Action( "complaint", "rechargeReport",new { rt = c.Id }), "View Complaint")+ "&nbsp;"+
                   DataTableButton.ComplaintReOpenButton(Url.Action( "createcomplaint", "rechargeReport",new { id = c.Id }),"modal-generate-complaint") :
                   action2.AllowCreate && c.StatusId==1?DataTableButton.ComplaintButton(Url.Action( "createcomplaint", "rechargeReport",new { id = c.Id }),"modal-generate-complaint") : string.Empty),
                   c.Id,
                    c.User?.UserProfile?.FullName??string.Empty,
                    c.CustomerNo??string.Empty,
                    c.Operator.Name,
                    c.Amount,
                    c.RequestTime?.ToString()??string.Empty,
                    (IsAdminRole && action.AllowEdit? DataTableButton.HyperLink(Url.Action( "changestatus", "rechargeReport",new { id = c.Id }),"modal-change-recharge-status", c.StatusType.TypeName,"Change Status("+(c.ResendCount??0)+")",c.ResendTime!=null?"":setColor(c.StatusId)): "<b style='color:"+setColor(c.StatusId)+"'>"+c.StatusType.TypeName+"</b>"),
                    c.UserTxnId??string.Empty,
                    c.OurRefTxnId??string.Empty,
                    c.ApiTxnId??string.Empty
                   
                    })
            }, JsonRequestBehavior.AllowGet);


        }


        [HttpGet]
        public ActionResult DmtReport(int? u2, int? u, int? v, int? o, int? s, int? c, string f = "", string m = "", string e = "", int? i = 0, string rto = "", string ut = "", string ot = "", string vt = "", FilterData fdata = null)
        {
            TempData["RechargeFilterDto"] = null;

            var str = "userrole=" + CurrentUser.RoleId +
                      ", CurrentUser.UserID=" + CurrentUser.UserID +
                      ", u=" + u +
                      ", v=" + v +
                      ", o=" + o +
                      ", s=" + s +
                      ", rto=" + rto +
                      ", f=" + f +
                      ",e=" + e +
                      ", c=" + c +
                      ", m=" + m +
                      ", ut=" + ut +
                      ", vt=" + vt +
                      ", ot=" + ot +
                      ", u2=" + u2;

            LogActivity(str);

            UpdateActivity("RechargeReport REQUEST", "GET:RechargeReport/DmtReport", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("DmtReport", CurrentUser.RoleId);

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
            filter.Edate = e;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyy");
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("dd/MM/yyy");
            filter.CustomerNo = m;
            filter.Circleid = Convert.ToInt32(c.HasValue ? c : 0);
            filter.UserReqid = ut;
            filter.OpTxnid = ot;
            filter.ApiTxnid = vt;

            ViewBag.FilterData = TempData["RechargeFilterDto"] = filter;//co

            int uid = CurrentUser.RoleId != 3 ? filter.Uid : CurrentUser.UserID;

            ViewBag.CircleList = operatorSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == filter.Circleid) }).ToList();
            ViewBag.OperatorList = packageService.GetOperatorList().Where(x => x.OpTypeId == 11).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == filter.Opid) }).ToList();
            //ViewBag.UserList = userService.GetUserList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.MerchantName ?? "NA", Selected = (x.Id == filter.Uid) }).ToList();
            //ViewBag.ApiList = apiService.GetApiList().Where(x => x.Id == 110).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == filter.Apiid) }).ToList();
            //ViewBag.UserList2 = userService.GetAEPSUserList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.MerchantName ?? "NA", Selected = (x.Id == filter.UpdatedById) }).ToList();

            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == filter.Uid) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == filter.Apiid) }).ToList();
            ViewBag.UserList2 = userService.GetUserList().Where(x => x.RoleId != 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == filter.UpdatedById) }).ToList();


            var statusList = rechargeReportService.GetStatusList().Where(r => r.Remark.Contains("Recharge") && r.Id != 5).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = ((x.Id == filter.Sid)) }).ToList();

            if (CurrentUser.RoleId != 3)
                statusList.Insert(statusList.Count, new SelectListItem() { Value = "11", Text = "Manual Updated", Selected = (filter.Sid == 11) });

            ViewBag.StatusList = statusList;
            return View();
        }
                 
        [HttpPost]
        public ActionResult GetDmtReport(DataTableServerSide model)
        {
            string log = string.Empty;
            ViewBag.actionAllowed = action = ActionAllowed("DmtReport", CurrentUser.RoleId);
            var action2 = ActionAllowed("Complaint", CurrentUser.RoleId);
            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;
            int userrole = CurrentUser.RoleId;
            bool IsAdminRole = (userrole != 3) ? true : false;
            model.filterdata.UserId = IsAdminRole ? model.filterdata.UserId : CurrentUser.UserID;
            KeyValuePair<int, List<DMT>> recharges = new KeyValuePair<int, List<DMT>>();
            try
            {
                recharges = rechargeReportService.GetDMTReport(model, flt.Uid, flt.Apiid, flt.Opid, Convert.ToByte(model.filterdata?.StatusId ?? 0), flt.Searchid, flt.Sdate, flt.Edate, flt.Circleid, flt.CustomerNo, flt.UserReqid, flt.ApiTxnid, flt.OpTxnid, flt.UpdatedById, ref log);
            }
            catch (Exception ex)
            {
                LogException(ex, "recharge report");
            }
            return Json(new
            {
                draw = model.draw,
                recordsTotal = recharges.Key,
                recordsFiltered = recharges.Key,
                data = recharges.Value.Select(c => new List<object> {
                   //( action2.AllowView && c.Complaints.Any(x=>x.RecId==c.Id) && c.StatusId==1?  DataTableButton.ExtLinkRedButton(Url.Action( "complaint", "rechargeReport",new { rt = c.Id }), "View Complaint") :
                   //action2.AllowCreate && c.StatusId==1?DataTableButton.ComplaintButton(Url.Action( "createcomplaint", "rechargeReport",new { id = c.Id }),"modal-generate-complaint") : string.Empty),
                   c.Id,
                    c.User?.UserProfile?.FullName??string.Empty,
                    c.TxnId,
                    c.ApiSource?.ApiName??string.Empty,
                    c.AccountNo,
                    c.Operator.Name,
                    c.Amount,
                    c.RCType.TypeName,
                    //(IsAdminRole && action.AllowEdit? DataTableButton.HyperLink(Url.Action( "changestatus", "rechargeReport",new { id = c.Id }),"modal-change-recharge-status", c.StatusType.TypeName,"Change Status("+(c.ResendCount??0)+")",c.ResendTime!=null?"":setColor(c.StatusId)): "<b style='color:"+setColor(c.StatusId)+"'>"+c.StatusType.TypeName+"</b>"),
                    c.StatusType.TypeName,
                    (c.RequestTime)?.ToString()??string.Empty,
                    (c.ResponseTime)?.ToString()??string.Empty,
                    c.MediumType.TypeName,
                    (IsAdminRole?c.StatusMsg: c.OptTxnId),
                     c.UserTxnId,
                     c.OurRefTxnId,
                     c.ApiTxnId,
                     c.OptTxnId,
                     c.BeneficiaryName,
                    c.BeneMobile,
                    c.IFSCCode,
                    c.UserComm,
                    c.AmtType.AmtTypeName
                    //(!string.IsNullOrEmpty(c.AccountNo)? ",<br />" +c.AccountNo:string.Empty)+
                    //(!string.IsNullOrEmpty(c.BeneficiaryName)? ",<br />" +c.BeneficiaryName:string.Empty),

                    //(!string.IsNullOrEmpty(c.AccountOther)? ",<br />" +c.AccountOther:string.Empty),
                    
                    //!IsAdminRole ? c.User?.UserProfile?.FullName??string.Empty : c.ApiSource?.ApiName??string.Empty,
                    
                    //c.transferMode,
                    //(c.UpdatedDate)?.ToString()??string.Empty,
                    
                    //c.Optional1+
                    //(!string.IsNullOrEmpty(c.Optional2)? ",<br />" +c.Optional2:string.Empty)+
                    //(!string.IsNullOrEmpty(c.Optional3)? ",<br />" +c.Optional3:string.Empty)+
                    //(!string.IsNullOrEmpty(c.Optional4)? ",<br />" +c.Optional4:string.Empty)

                    })
            }, JsonRequestBehavior.AllowGet);


        }
        public JObject GetAutoRefresh()
        {
            JObject response = new JObject();
            response = JObject.FromObject(new
            {
                AUTOCHECK = autocheck,
                CHECKTIME = checktime,
            });
            return response;
        }

        [HttpPost]
        public JObject SetAutoRefresh(int Setautocheck = 0, int setchecktime = 60)
        {
            JObject response = new JObject();
            autocheck = Setautocheck;
            checktime = setchecktime;
            response = JObject.FromObject(new
            {
                AUTOCHECK = autocheck,
                CHECKTIME = checktime,
            });
            return response;
        }

        private string setColor(int? id)
        {
            string color = id == 1 || id == 8 || id == 6 ? "green" :
                           id == 2 || id == 9 ? "blue" :
                           id == 3 ? "red" :
                           id == 10 ? "#800020" :
                           id == 4 || id == 5 ? "orange" :
                           "";
            return color;
        }
        public ActionResult ChangeStatus(long id = 0, int p = 0) //p==> for processing
        {
            long actid = UpdateActivity("RechargeReport StatusChange REQUEST", "GET:RechargeReport/ChangeStatus/", "recid=" + id);
            action = ActionAllowed("RechargeReport", CurrentUser.RoleId, 3);
            RechargeFilterDto flt = new RechargeFilterDto();
            RechargeUpdateDto model = new RechargeUpdateDto();
            model.IsProcessing = p;
            if (model.IsProcessing == 1)
            {
                flt = TempData["pRechargeFilterDto"] != null ? (RechargeFilterDto)TempData["pRechargeFilterDto"] : new RechargeFilterDto();
                TempData["pRechargeFilterDto"] = flt;
            }
            else
            {
                flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
                TempData["RechargeFilterDto"] = flt;
            }
            if (id > 0)
            {
                Recharge recharge = rechargeReportService.GetRecharge(id);
                model.RecId = recharge.Id;
                model.StatusId = recharge.StatusId ?? 0;
                var stlist = rechargeReportService.GetStatusList();
                model.StatusList = stlist.Where(s => s.Id <= 3).Select(x => new StatusTypeDto()
                {
                    StatusId = x.Id,
                    StatusName = x.TypeName

                }).ToList();
                UpdateActivity("RechargeReport StatusChange REQUEST", "GET:RechargeReport/ChangeStatus/", "recid=" + id + ", oldStatus=" + recharge.StatusId, actid);
            }
            return PartialView("_ChangeStatus", model);
        }
        [HttpPost]
        public ActionResult ChangeStatus(RechargeUpdateDto model, FormCollection FC)
        {
            UpdateActivity("RechargeReport StatusChange REQUEST", "POST:RechargeReport/ChangeStatus/", "recid=" + model.RecId + ", newStatus=" + model.StatusId);
            action = ActionAllowed("RechargeReport", CurrentUser.RoleId, 3);
            RechargeFilterDto flt = new RechargeFilterDto();
            if (model.IsProcessing == 1)
            {
                flt = TempData["pRechargeFilterDto"] != null ? (RechargeFilterDto)TempData["pRechargeFilterDto"] : new RechargeFilterDto();
                TempData["pRechargeFilterDto"] = flt;
            }
            else
            {
                flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
                TempData["RechargeFilterDto"] = flt;
            }
            string message = string.Empty;
            try
            {
                if (ModelState.IsValid)
                {
                    Recharge recharge = rechargeReportService.GetRecharge(model.RecId);
                    StatusChange(model.StatusId, recharge, model.OpTxnId, model.ApiTxnId);
                    //if (model.StatusId == 3)
                    //{
                    //    long complaitId = 0;
                    //    int cmpStatusId = 0;
                    //    string cmpRemark = "";
                    //    string cmpComment = "";
                    //    long RecId = 0;
                    //    string log ="";
                    //    GETandSETcomplaintByRef("0", recharge.OurRefTxnId, recharge.TxnId.ToString(), recharge.ApiTxnId, 8, "RC Status Change ", ref complaitId, ref cmpStatusId, ref cmpRemark, ref cmpComment, ref RecId, ref log);
                    //    LogActivity(log);
                    //}
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
            string viewname = string.Empty;
            if (model.ApiId ==12) // payin api only 
            {
                viewname = model.IsProcessing == 1 ? "ProcessingRecharge" : "PendingRecharge";
            }
            else
            {
            viewname = model.IsProcessing == 1 ? "ProcessingRecharge" : "Index";
            }
            return RedirectToAction(viewname, new RouteValueDictionary(routeValues));
        }
        private string StatusChange(int statusId, Recharge recharge, string optxnid, string apitxnid)
        {

            string log = "status change start, recid=" + recharge.Id;
            UpdateActivity("StatusChange REQUEST", "POST:RechargeReport/StatusChange/", log);
            int oldStatusId = recharge.StatusId ?? 0;
            int newStatusId = statusId;
            //if (oldStatusId == 1 || oldStatusId == 2 || oldStatusId == 4)
            //{
            bool IsDownline = false;
            bool IsRefund = false;
            // UpdateStatusWithCheck(recharge.Id, newStatusId, apitxnid, optxnid, recharge.StatusMsg, "Manual", ref IsDownline, ref IsRefund, ref log);
            //if (recharge.OpId == 97 || recharge.OpId == 98)
            //{
            //    UpdateAEPSStatusWithCheck(recharge.Id, newStatusId , ref log);
            //}
            //else
            //{
            if (recharge.ApiId == 12) // payin api proce to like aeps
            {  
                UpdateStatusWithCheckPayin(recharge.Id, newStatusId, apitxnid, optxnid, recharge.StatusMsg, "Manual", ref IsDownline, ref IsRefund, ref log, 0, string.Empty, 0);
            }
            else { 
            UpdateStatusWithCheck(recharge.Id, newStatusId, apitxnid, optxnid, recharge.StatusMsg, "Manual", ref IsDownline, ref IsRefund, ref log, 0, string.Empty, 0);
            }
            #region "Send Callback to User"
            log += "\r\nget user callback url ";
            User user = userService.GetUser(recharge.UserId ?? 0);
            if (user != null && oldStatusId != newStatusId)
            {
                if (recharge.OpId == 100 || recharge.OpId == 101 || recharge.OpId == 102)
                    user.CallbackURL = string.IsNullOrEmpty(user.DMTCallbackURL) ? user.CallbackURL : user.DMTCallbackURL;
                else
                    user.CallbackURL = user.CallbackURL;

                SendCallBack(recharge, user.CallbackURL, string.IsNullOrEmpty(optxnid) ? recharge.OptTxnId : optxnid, newStatusId, ref log);
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
        private void SendCallBack(Recharge recharge, string CallbackURL, string optxnid, int statusId, ref string log)
        {
            log += "\r\nsend callback start, ";
            if (string.IsNullOrEmpty(CallbackURL))
            {
                log += "\r\ncallback not exists, ";
            }
            else
            {

                CallbackURL = CallbackURL.ReplaceURL(string.Empty, string.Empty, recharge.UserTxnId, recharge.CustomerNo, optxnid, recharge.CircleId.ToString(), recharge.Amount.ToString(), recharge.TxnId.ToString(), statusId.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Now.ToString("yyyyMMddHHmmss"), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, recharge.OpId.ToString());

                log += "\r\ncallback url, " + CallbackURL + " recid=" + recharge.Id + ", ";

                RequestResponseDto userReqRes = new RequestResponseDto();
                userReqRes.Remark = "downline status";
                userReqRes.UserId = recharge.UserId ?? 0;
                userReqRes.RecId = recharge.Id;
                userReqRes.RefId = recharge.OurRefTxnId;
                userReqRes.RequestTxt = CallbackURL;
                userReqRes = AddUpdateReqRes(userReqRes, ref log);

                ApiCall apiCall = new ApiCall(reqResService);
                apiCall.Get(CallbackURL, ref userReqRes);
                userReqRes = AddUpdateReqRes(userReqRes, ref log);

                log += "\r\ncallback end, ";
            }

        }

        [HttpGet]
        public ActionResult PendingRecharge(int? ck, int? u2, int? u, int? v, int? c, string f = "", string m = "", string e = "", int? i = 0, string rto = "", string ut = "", string ot = "", string vt = "")
        
        {
            UpdateActivity("PendingRecharge REQUEST", "GET:RechargeReport/PendingRecharge/", "");
            action = ActionAllowed("PendingRecharge", CurrentUser.RoleId);
            var optlist = operatorSwitchService.circlesList();
            RechargeFilterDto filter = new RechargeFilterDto();
            filter.UpdatedById = Convert.ToInt32(u2.HasValue ? u2 : 0);
            filter.Uid = Convert.ToInt32(u.HasValue ? u : 0);
            filter.Isa = Convert.ToInt32(i.HasValue ? i : 0);
            filter.Apiid = Convert.ToInt32(v.HasValue ? v : 0);
            filter.Searchid = rto;
            filter.Sid = 2;
            filter.Sdate = f;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyy");
            filter.Edate = e;
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("dd/MM/yyy");
            filter.CustomerNo = m;
            filter.Circleid = Convert.ToInt32(c.HasValue ? c : 0);
            filter.UserReqid = ut;
            filter.OpTxnid = ot;
            filter.ApiTxnid = vt;
            filter.IsResentOnly = ck ?? 0;
            ViewBag.FilterData = TempData["pRechargeFilterDto"] = filter;
            int uid = (CurrentUser.RoleId != 3) ? filter.Uid : CurrentUser.UserID;
            ViewBag.CircleList = operatorSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == c) }).ToList();
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == u) }).ToList();
            UpdateActivity("PendingRecharge REQUEST", "GET:RechargeReport/PendingRecharge/", "");
            action = ActionAllowed("PendingRecharge", CurrentUser.RoleId);
            return View();
        } 

        [HttpPost]
        public ActionResult PendingRecharge(DataTableServerSide model)
        {
            action = ActionAllowed("PendingRecharge", CurrentUser.RoleId);
            RechargeFilterDto flt = TempData["pRechargeFilterDto"] != null ? (RechargeFilterDto)TempData["pRechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["pRechargeFilterDto"] = flt;
            flt.Uid = CurrentUser.RoleId != 3 ? flt.Uid : CurrentUser.UserID;
            flt.Sid = 2;
            bool IsAdminRole = CurrentUser.RoleId == 3 ? false : true;
            KeyValuePair<int, List<Recharge>> requestResponses = rechargeReportService.GetPendingrecharge(model, flt);
            return Json(new
            {
                draw = model.draw,
                recordsTotal = requestResponses.Key,
                recordsFiltered = requestResponses.Key,
                data = requestResponses.Value.Select(c => new List<object> {
                    DataTableButton.CheckBox("chkRecId", c.Id.ToString(),"chkCheckBoxId"),
                    c.Id,
                    c.UserId,
                    c?.ApiSource?.ApiName??"",
                    c.CustomerNo,
                    c.Operator.Name,
                    c.Amount,
                    (IsAdminRole && action.AllowEdit? DataTableButton.HyperLink(Url.Action( "changestatus", "rechargeReport",new { id = c.Id }),"modal-change-recharge-status", c.StatusType.TypeName,"Change Status("+(c.ResendCount??0)+")",c.ResendTime!=null?"":setColor(c.StatusId)): "<b style='color:"+setColor(c.StatusId)+"'>"+c.StatusType.TypeName+"</b>"),
                    //(CurrentUser.RoleId!=3 && action.AllowEdit? DataTableButton.HyperLink(Url.Action( "changestatus", "",new { id = c.Id, p=1 }),"modal-change-recharge-status", c.StatusType.TypeName,c.ResponseTime!=null?"":setColor(c.StatusId)): "<b style='color:"+setColor(c.StatusId)+"'>"+c.StatusType.TypeName+"</b>"),
                    (c.RequestTime).ToString(),
                    (c.ResponseTime).ToString() ??string.Empty,
                    c.StatusMsg ,
                    c.UserTxnId ,
                    c?.OurRefTxnId ??"",
                    c?.ApiTxnId??"",
                    c?.OptTxnId??"",
                    c.UpdatedDate.ToString()??string.Empty
                    })
            }, JsonRequestBehavior.AllowGet);

        }
    
        public string GetApiName(int id=0)
        {
            string name = apiService.GetApiSource(id).ApiName;
            return name;
        }

        [HttpGet]
        public ActionResult OpPendingRecharge(int? id)
        {
            TempData["ApiId"] = id ?? 0;

            UpdateActivity("OpPendingRecharge REQUEST", "GET:RechargeReport/OpPendingRecharge/", "");
            action = ActionAllowed("PendingRecharge", CurrentUser.RoleId);

            return View();
        }

        [HttpGet]
        public ActionResult ProcessingRecharge(int? ck, int? u2, int? u, int? v, int? o, int? c, string f = "", string m = "", string e = "", int? i = 0, string rto = "", string ut = "", string ot = "", string vt = "")
        {
            UpdateActivity("ProcessingRecharge REQUEST", "GET:RechargeReport/ProcessingRecharge/", "");
            action = ActionAllowed("ProcessingRecharge", CurrentUser.RoleId);

            var optlist = operatorSwitchService.circlesList();

            RechargeFilterDto filter = new RechargeFilterDto();

            filter.UpdatedById = Convert.ToInt32(u2.HasValue ? u2 : 0);
            filter.Uid = Convert.ToInt32(u.HasValue ? u : 0);
            filter.Isa = Convert.ToInt32(i.HasValue ? i : 0);
            filter.Apiid = Convert.ToInt32(v.HasValue ? v : 0);
            filter.Opid = Convert.ToInt32(o.HasValue ? o : 0);
            filter.Searchid = rto;
            filter.Sid = 2;
            filter.Sdate = f;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyy");
            filter.Edate = e;
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("dd/MM/yyy");
            filter.CustomerNo = m;
            filter.Circleid = Convert.ToInt32(c.HasValue ? c : 0);
            filter.UserReqid = ut;
            filter.OpTxnid = ot;
            filter.ApiTxnid = vt;
            filter.IsResentOnly = ck ?? 0;
            ViewBag.FilterData = TempData["pRechargeFilterDto"] = filter;

            int uid = (CurrentUser.RoleId != 3) ? filter.Uid : CurrentUser.UserID;

            ViewBag.CircleList = operatorSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == c) }).ToList();
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == o) }).ToList();
            //ViewBag.StatusList = rechargeReportService.GetStatusList().Where(r => r.Remark.Contains("Recharge")).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = ((x.Id == 2)) }).ToList();
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == u) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == v) }).ToList();
            ViewBag.UserList2 = userService.GetUserList().Where(x => x.RoleId != 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == u2) }).ToList();

            UpdateActivity("ProcessingRecharge REQUEST", "GET:RechargeReport/ProcessingRecharge/", "");
            action = ActionAllowed("ProcessingRecharge", CurrentUser.RoleId);
            return View();
        }
        [HttpPost]
        public ActionResult ProcessingRecharge(DataTableServerSide model)
        {
            action = ActionAllowed("ProcessingRecharge", CurrentUser.RoleId);
            RechargeFilterDto flt = TempData["pRechargeFilterDto"] != null ? (RechargeFilterDto)TempData["pRechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["pRechargeFilterDto"] = flt;
            flt.Uid = CurrentUser.RoleId != 3 ? flt.Uid : CurrentUser.UserID;
            flt.Sid = 2;
            bool IsAdminRole = CurrentUser.RoleId == 3 ? false : true;

            KeyValuePair<int, List<DMT>> requestResponses = rechargeReportService.GetProcessingDMT(model, flt);

            return Json(new
            {
                draw = model.draw,
                recordsTotal = requestResponses.Key,
                recordsFiltered = requestResponses.Key,
                data = requestResponses.Value.Select(c => new List<object> {
                    DataTableButton.CheckBox("chkRecId", c.Id.ToString(),"chkCheckBoxId"),
                    c.Id,
                    c.User.UserProfile.FullName,
                    c.BeneMobile,
                    c.ApiSource?.ApiName,
                    c.Operator.Name,
                    //IsAdminRole ? c.ApiSource.ApiName : c.ApiId.ToString()??string.Empty,
                    c.AccountNo,
                    c.Amount,
                      (CurrentUser.RoleId!=3 && action.AllowEdit? DataTableButton.HyperLink(Url.Action( "changestatus", "DMTReport",new { id = c.Id, p=1 }),"modal-change-recharge-status", c.StatusType.TypeName,c.ResponseTime!=null?"":setColor(c.StatusId)): "<b style='color:"+setColor(c.StatusId)+"'>"+c.StatusType.TypeName+"</b>"),
                  //  "<b style='color:"+setColor(c.StatusId)+"'>"+c.StatusType.TypeName+"</b>",
                    (c.RequestTime).ToString(),
                    (c.ResponseTime)?.ToString()??string.Empty,
                     c.StatusMsg,
                     //c.Circle?.CircleName,
                     c.OurRefTxnId,
                     c.ApiTxnId,
                     c.OptTxnId,
                     c.UserTxnId,
                     c.TxnId,

                     (c.ResponseTime)?.ToString()??string.Empty,
                     c.User?.UserProfile?.FullName??string.Empty
                    })
            }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult ProcessingMoneyTransfer(int? ck, int? u2, int? u, int? v, int? o, int? c, string f = "", string m = "", string e = "", int? i = 0, string rto = "", string ut = "", string ot = "", string vt = "")
        {
            UpdateActivity("ProcessingRecharge REQUEST", "GET:RechargeReport/ProcessingRecharge/", "");
            action = ActionAllowed("ProcessingRecharge", CurrentUser.RoleId);
            var optlist = operatorSwitchService.circlesList();
            RechargeFilterDto filter = new RechargeFilterDto();
            filter.UpdatedById = Convert.ToInt32(u2.HasValue ? u2 : 0);
            filter.Uid = Convert.ToInt32(u.HasValue ? u : 0);
            filter.Isa = Convert.ToInt32(i.HasValue ? i : 0);
            filter.Apiid = 110;// Convert.ToInt32(v.HasValue ? v : 0);
            filter.Opid = 100;// Convert.ToInt32(o.HasValue ? o : 0);
            filter.Searchid = rto;
            filter.Sid = 2;
            filter.Sdate = f;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyy");
            filter.Edate = e;
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("dd/MM/yyy");
            filter.CustomerNo = m;
            filter.Circleid = Convert.ToInt32(c.HasValue ? c : 0);
            filter.UserReqid = ut;
            filter.OpTxnid = ot;
            filter.ApiTxnid = vt;
            filter.IsResentOnly = ck ?? 0;
            ViewBag.FilterData = TempData["pRechargeFilterDto"] = filter;

            int uid = (CurrentUser.RoleId != 3) ? filter.Uid : CurrentUser.UserID;

            ViewBag.CircleList = operatorSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == c) }).ToList();
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == o) }).ToList();
            //ViewBag.StatusList = rechargeReportService.GetStatusList().Where(r => r.Remark.Contains("Recharge")).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = ((x.Id == 2)) }).ToList();
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == u) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == v) }).ToList();
            ViewBag.UserList2 = userService.GetUserList().Where(x => x.RoleId != 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == u2) }).ToList();

            UpdateActivity("ProcessingRecharge REQUEST", "GET:RechargeReport/ProcessingRecharge/", "");
            action = ActionAllowed("ProcessingRecharge", CurrentUser.RoleId);
            return View();
        }
        [HttpPost]
        public ActionResult ProcessingMoneyTransfer(DataTableServerSide model)
        {
            action = ActionAllowed("ProcessingRecharge", CurrentUser.RoleId);
            RechargeFilterDto flt = TempData["pRechargeFilterDto"] != null ? (RechargeFilterDto)TempData["pRechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["pRechargeFilterDto"] = flt;
            flt.Uid = CurrentUser.RoleId != 3 ? flt.Uid : CurrentUser.UserID;
            flt.Sid = 2;

            bool IsAdminRole = CurrentUser.RoleId == 3 ? false : true;

            KeyValuePair<int, List<Recharge>> requestResponses = rechargeReportService.GetProcessingRecharge(model, flt);

            return Json(new
            {
                draw = model.draw,
                recordsTotal = requestResponses.Key,
                recordsFiltered = requestResponses.Key,
                data = requestResponses.Value.Select(c => new List<object> {
                    DataTableButton.CheckBox("chkRecId", c.Id.ToString(),"chkCheckBoxId"),
                    c.Id,
                    c.User.UserProfile.FullName,
                    c.CustomerNo,
                    c.Operator.Name,
                    IsAdminRole ? c.ApiSource.ApiName : c.ApiId.ToString(),
                    c.Amount,
                    (CurrentUser.RoleId!=3 && action.AllowEdit? DataTableButton.HyperLink(Url.Action( "ChangeStatusMt", "rechargeReport",new { id = c.Id, p=1 }),"modal-change-recharge-status", c.StatusType.TypeName,"Change Status("+(c.ResendCount??0)+")",c.ResendTime!=null?"":setColor(c.StatusId)): "<b style='color:"+setColor(c.StatusId)+"'>"+c.StatusType.TypeName+"</b>"),
                  //  "<b style='color:"+setColor(c.StatusId)+"'>"+c.StatusType.TypeName+"</b>",
                    (c.RequestTime).ToString(),
                    (c.ResponseTime)?.ToString()??string.Empty,
                     c.StatusMsg,
                     c.Circle?.CircleName,
                     c.OurRefTxnId,
                     c.ApiTxnId,
                     c.OptTxnId,
                     c.UserTxnId,
                     c.TxnId,
                     c.ResendCount??0,
                     (c.ResendTime)?.ToString()??string.Empty,
                     c.User2?.UserProfile?.FullName??string.Empty
                    })
            }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult ChangeStatusMt(long id = 0, int p = 0) //p==> for processing
        {
            long actid = UpdateActivity("RechargeReport StatusChange REQUEST", "GET:RechargeReport/ChangeStatus/", "recid=" + id);
            action = ActionAllowed("RechargeReport", CurrentUser.RoleId, 3);
            RechargeFilterDto flt = new RechargeFilterDto();
            RechargeUpdateDto model = new RechargeUpdateDto();

            model.IsProcessing = p;

            if (model.IsProcessing == 1)
            {
                flt = TempData["pRechargeFilterDto"] != null ? (RechargeFilterDto)TempData["pRechargeFilterDto"] : new RechargeFilterDto();
                TempData["pRechargeFilterDto"] = flt;
            }
            else
            {
                flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
                TempData["RechargeFilterDto"] = flt;
            }


            if (id > 0)
            {
                Recharge recharge = rechargeReportService.GetRecharge(id);
                model.RecId = recharge.Id;
                model.StatusId = recharge.StatusId ?? 0;


                var stlist = rechargeReportService.GetStatusList();

                model.StatusList = stlist.Where(s => s.Id <= 3).Select(x => new StatusTypeDto()
                {
                    StatusId = x.Id,
                    StatusName = x.TypeName

                }).ToList();

                UpdateActivity("RechargeReport StatusChange REQUEST", "GET:RechargeReport/ChangeStatusMt/", "recid=" + id + ", oldStatus=" + recharge.StatusId, actid);


            }


            return PartialView("_ChangeStatus", model);

        }
        [HttpPost]
        public ActionResult ChangeStatusMt(RechargeUpdateDto model, FormCollection FC)
        {
            UpdateActivity("RechargeReport StatusChange REQUEST", "POST:RechargeReport/ChangeStatus/", "recid=" + model.RecId + ", newStatus=" + model.StatusId);
            action = ActionAllowed("RechargeReport", CurrentUser.RoleId, 3);
            RechargeFilterDto flt = new RechargeFilterDto();
            if (model.IsProcessing == 1)
            {
                flt = TempData["pRechargeFilterDto"] != null ? (RechargeFilterDto)TempData["pRechargeFilterDto"] : new RechargeFilterDto();
                TempData["pRechargeFilterDto"] = flt;
            }

            else
            {
                flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
                TempData["RechargeFilterDto"] = flt;
            }


            string message = string.Empty;

            try
            {
                if (ModelState.IsValid)
                {
                    Recharge recharge = rechargeReportService.GetRecharge(model.RecId);
                    StatusChange(model.StatusId, recharge, model.OpTxnId, model.ApiTxnId);

                    //if (model.StatusId == 3)
                    //{
                    //    long complaitId = 0;
                    //    int cmpStatusId = 0;
                    //    string cmpRemark = "";
                    //    string cmpComment = "";
                    //    long RecId = 0;
                    //    string log ="";
                    //    GETandSETcomplaintByRef("0", recharge.OurRefTxnId, recharge.TxnId.ToString(), recharge.ApiTxnId, 8, "RC Status Change ", ref complaitId, ref cmpStatusId, ref cmpRemark, ref cmpComment, ref RecId, ref log);
                    //    LogActivity(log);
                    //}

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
            string viewname = model.IsProcessing == 1 ? "ProcessingRecharge" : "Index";
            return RedirectToAction(viewname, new RouteValueDictionary(routeValues));
        }

        [HttpPost]
        public string CheckStatusBulk(string recIds)
        {
            UpdateActivity("CheckStatusBulk REQUEST", "POST:RechargeReport/CheckStatusBulk/", "recIds=" + recIds);
            ViewBag.actionAllowed = action = ActionAllowed("ProcessingRecharge", CurrentUser.RoleId, 3);

            string log = "Checkbulkstatus-start recids=" + recIds;
            int count = 0;
            int totalcount = 0;

            if (string.IsNullOrEmpty(recIds))
            {
                ShowErrorMessage("Error!", "No recharge selected", false);
            }
            else
            {
                var recIdList = recIds.Replace(" ", "").Split(',').ToList();
                int userid = CurrentUser.UserID;
                recIdList = recIdList.Where(x => !string.IsNullOrEmpty(x)).ToList();

                totalcount = recIdList.Count;

                if (recIdList.Count > 0)
                {
                    foreach (var rId in recIdList)
                    {
                        try
                        {
                            // CheckAndUpdateRCStatusAndCalllback(ref log, ref count, rId);
                            Thread thread = new Thread(new ThreadStart(() => CheckStatusThread(rId, userid)));
                            thread.Start();
                        }
                        catch (Exception ex)
                        {

                            LogException(ex, "thread exception");
                        }
                    }
                }
                // ShowSuccessMessage("Success!", "Status has been checked. (" + totalcount + ")", false);
            }
            LogActivity(log);
            return "Status has been checked. (" + totalcount + ")";

        }
        private void CheckAndUpdateRCStatusAndCalllback(ref string log, ref int count, string rId)
        {

            LogActivity("\n tread start rId=" + rId + " and count=" + count + "\n");
            try
            {
                int statusId = 0;
                string optxnid = "";
                string apitxnid = "";
                string statusmsg = "";
                string remark = "AutoStatusCheck";
                bool IsDownline = false;
                bool IsRefund = false;
                var recharge = rechargeService.GetRecharge(Convert.ToInt64(rId));

                CheckStatus(recharge, remark, ref IsDownline, ref apitxnid, ref statusmsg, ref optxnid, ref statusId, ref log);

                // UpdateStatusWithCheck(recharge.Id, statusId, apitxnid, optxnid, statusmsg, remark, ref IsDownline, ref IsRefund, ref log);
                UpdateStatusWithCheck(recharge.Id, statusId, apitxnid, optxnid, recharge.StatusMsg, "Manual", ref IsDownline, ref IsRefund, ref log, 0, string.Empty, 0);

                if (IsDownline)
                {
                    #region "Send Callback to User"
                    log += "\r\n, get user callback url ";
                    User user = userService.GetUser(recharge.UserId ?? 0);
                    if (user != null)
                    {
                        SendCallBack(recharge, user.CallbackURL, optxnid, statusId, ref log);
                    }
                    else
                    {
                        log += "\r\n, usernotfound";
                    }
                    log += "\r\n, callback sent to user ";
                    #endregion
                }

                count += 1;
            }
            catch (Exception ex)
            {

                LogException(ex);
                log += "\r\n, excp(recid=" + rId + "): " + ex.Message;

            }
            LogActivity("\n tread end rId=" + rId + " and count=" + count + "\n");
        }
        public ActionResult ChangeStatusBulk()
        {

            UpdateActivity("ChangeStatusBulk REQUEST", "GET:RechargeReport/ChangeStatusBulk/", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("ProcessingRecharge", CurrentUser.RoleId, 3);

            RechargeUpdateDto rcStatusChangeDto = new RechargeUpdateDto();
            try
            {
                var stlist = rechargeReportService.GetStatusList();

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
                        UpdateRCStatusAndCallback(model, ref log, rId);

                        count += 1;
                    }
                }

                ShowSuccessMessage("Success!", "Status has been changed.(" + totalcount + ")", false);
            }
            log += "\r\n, end ";
            LogActivity(log);
            return RedirectToAction("ProcessingRecharge");

        }
        private void UpdateRCStatusAndCallback(RechargeUpdateDto model, ref string log, string rId)
        {
            log += "\r\n, recid=" + rId;
            //get recharge
            long recid = Convert.ToInt64(rId);

            bool IsRefund = false;
            bool IsDownline = false;
            string spLog = "";
            string remark = "StatusChangeBulk";

            var recharge = rechargeService.GetRecharge(recid);
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
            LogActivity(log);
        }
        [HttpPost]
        public string apCount()
        {

            return Session["apCount"] != null ? Convert.ToString(Session["apCount"]) : "0";


        }

        [HttpPost]
        public string opCount()
        {

            return Session["opCount"] != null ? Convert.ToString(Session["opCount"]) : "0";


        }

        private string FilterRespTagValue(int apiid, int UrlId, string resType, string apires, ref int statusId, ref string log, ref FilterResponseModel filterResponse)
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
                        log += "\r\n index=" + index + ",";
                        if (tg.TagId == TAGName.SUCCESS) //status-success
                        {
                            try
                            {
                                string sval = apires.GetSplitstringByIndex(tg.ResSeparator, index);

                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.SUCCESS=" + sval + ", ";

                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                                {
                                    filterResponse.StatusId = statusId = StatsCode.SUCCESS;
                                }

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.SUCCESS expc= " + ex.Message;
                                //   LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.FAILED)//status-failed
                        {
                            try
                            {
                                string sval = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.FAILED=" + sval + ", ";

                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                                {
                                    filterResponse.StatusId = statusId = StatsCode.FAILED;
                                }
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.FAILED expc= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }
                        }
                        else if (tg.TagId == TAGName.PROCESSING) //status-processing
                        {
                            try
                            {
                                string sval = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PROCESSING=" + sval + ", ";

                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                                {
                                    filterResponse.StatusId = statusId = StatsCode.PROCESSING;
                                }
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PROCESSING expc= " + ex.Message;
                                //LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.PENDING)//status-pending
                        {
                            try
                            {
                                string sval = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PENDING=" + sval + ", ";

                                if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                                {
                                    filterResponse.StatusId = statusId = 5;
                                }
                            }
                            catch (Exception ex)
                            {

                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PENDING expc= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.APITXNID)//api txn id
                        {
                            try
                            {
                                filterResponse.ApiTxnID = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")apitxnid=" + filterResponse.ApiTxnID + ", ";
                            }
                            catch (Exception ex)
                            {

                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")api-txnid expc= " + ex.Message;
                                //  LIBS.Common.LogException(ex);
                            }
                        }
                        else if (tg.TagId == TAGName.OPERATORTXNID) //operator txn id
                        {
                            try
                            {
                                filterResponse.OperatorTxnID = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                filterResponse.OperatorTxnID = HttpUtility.UrlDecode(filterResponse.OperatorTxnID);
                                filterResponse.OperatorTxnID = filterResponse.OperatorTxnID.Trim();
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")optxnid=" + filterResponse.OperatorTxnID + ", ";
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n optr txnid expc= " + ex.Message;
                                //  LIBS.Common.LogException(ex);
                            }
                        }
                        else if (tg.TagId == TAGName.MESSAGE) //status message
                        {

                            try
                            {
                                filterResponse.Message = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")statusmsg=" + filterResponse.Message + ", ";
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")status msg expc= " + ex.Message;
                                //  LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.REQUESTTXNID) //request txn id
                        {

                            try
                            {
                                filterResponse.RequestTxnId = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid=" + filterResponse.RequestTxnId + ", ";
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid msg expc= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.VENDOR_CL_BAL) //Vendor_CL_Bal
                        {

                            try
                            {
                                string clbal = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal=" + clbal + ", ";
                                clbal = clbal.Length > 199 ? clbal.Substring(0, 198) : clbal;
                                filterResponse.Vendor_CL_Bal = Convert.ToDecimal(clbal);
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal  excp= " + ex.Message;
                                //  LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.VENDOR_OP_BAL) //Vendor_OP_Bal
                        {
                            try
                            {
                                string opbal = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal=" + opbal + ", ";
                                opbal = opbal.Length > 50 ? opbal.Substring(0, 45) : opbal;
                                filterResponse.Vendor_OP_Bal = Convert.ToDecimal(opbal);
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal excp= " + ex.Message;
                                //  LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.LAPUNO) //Lapu Number
                        {

                            try
                            {
                                string lapuno = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")lapuno=" + lapuno + ", ";
                                filterResponse.LapuNo = lapuno.Length > 50 ? lapuno.Substring(0, 45) : lapuno;
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")lapuno excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.COMPLAINT_ID)
                        {

                            try
                            {
                                string cid = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")COMPLAINT_ID=" + cid + ", ";
                                filterResponse.Complaint_Id = cid.Length > 50 ? cid.Substring(0, 45) : cid;
                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")COMPLAINT_ID excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                        else if (tg.TagId == TAGName.R_OFFER)
                        {

                            try
                            {
                                string ro = apires.GetSplitstringByIndex(tg.ResSeparator, index);
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")roffer=" + ro + ", ";
                                ro = ro.Length > 50 ? ro.Substring(0, 45) : ro;
                                filterResponse.R_Offer = Convert.ToDecimal(ro);

                            }
                            catch (Exception ex)
                            {
                                log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")roffer excp= " + ex.Message;
                                // LIBS.Common.LogException(ex);
                            }

                        }
                    }

                }
            }
            else
            {
                log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")tagvalue retrival,";
                foreach (var tg in tagvalues)
                {
                    var cmpList = new List<string>();
                    if (!string.IsNullOrEmpty(tg.CompareTxt))
                    {
                        cmpList = tg.CompareTxt.Replace(" ", string.Empty).Split(',').Where(x => x != string.Empty).ToList();
                    }
                    if (tg.TagId == TAGName.SUCCESS) //status-success
                    {
                        try
                        {
                            string sval = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.SUCCESS=" + sval + ", ";

                            // string sval = filterResponse.Status;

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                            {
                                filterResponse.StatusId = statusId = StatsCode.SUCCESS;
                            }
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.SUCCESS expc= " + ex.Message;
                            //LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.FAILED)//status-failed
                    {
                        try
                        {
                            string sval = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.FAILED=" + sval + ", ";
                            //string sval = filterResponse.Status;

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                            {
                                filterResponse.StatusId = statusId = StatsCode.FAILED;
                            }
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.FAILED expc= " + ex.Message;
                            /// LIBS.Common.LogException(ex);
                        }
                    }
                    else if (tg.TagId == TAGName.PROCESSING) //status-processing
                    {
                        try
                        {
                            string sval = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PROCESSING=" + sval + ", ";

                            // string sval = filterResponse.Status;

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                            {
                                filterResponse.StatusId = statusId = StatsCode.PROCESSING;
                            }
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PROCESSING expc= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.PENDING)//status-pending
                    {
                        try
                        {
                            string sval = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PENDING=" + sval + ", ";
                            // string sval = filterResponse.Status;

                            if (cmpList.Any(s => s?.Trim()?.ToLower() == sval.ToLower()))
                            {
                                filterResponse.StatusId = statusId = 5;
                            }
                        }
                        catch (Exception ex)
                        {

                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")TAGName.PENDING expc= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.APITXNID)//api txn id
                    {
                        try
                        {

                            filterResponse.ApiTxnID = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")apitxnid=" + filterResponse.ApiTxnID + ", ";
                            filterResponse.ApiTxnID = filterResponse.ApiTxnID.Length > 50 ? filterResponse.ApiTxnID.Substring(0, 45) : filterResponse.ApiTxnID;
                        }
                        catch (Exception ex)
                        {

                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")api txnid expc= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }
                    }
                    else if (tg.TagId == TAGName.OPERATORTXNID) //operator txn id
                    {
                        try
                        {
                            filterResponse.OperatorTxnID = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);

                            filterResponse.OperatorTxnID = HttpUtility.UrlDecode(filterResponse.OperatorTxnID);
                            filterResponse.OperatorTxnID = filterResponse.OperatorTxnID.Trim();

                            log += "\r\n optxnid=" + filterResponse.OperatorTxnID.Trim() + ", ";
                            filterResponse.OperatorTxnID = filterResponse.OperatorTxnID.Length > 50 ? filterResponse.OperatorTxnID.Substring(0, 45) : filterResponse.OperatorTxnID;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")optr txnid expc= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.MESSAGE) //status message
                    {

                        try
                        {
                            filterResponse.Message = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")statusmsg=" + filterResponse.Message + ", ";
                            filterResponse.Message = filterResponse.Message.Length > 199 ? filterResponse.Message.Substring(0, 198) : filterResponse.Message;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")status msg expc= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.REQUESTTXNID) //request txn id
                    {

                        try
                        {
                            filterResponse.RequestTxnId = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid=" + filterResponse.RequestTxnId + ", ";
                            filterResponse.RequestTxnId = filterResponse.RequestTxnId.Length > 50 ? filterResponse.RequestTxnId.Substring(0, 45) : filterResponse.RequestTxnId;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")reqtxnid msg expc= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.VENDOR_CL_BAL) //Vendor_CL_Bal
                    {

                        try
                        {
                            string clbal = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal=" + clbal + ", ";
                            clbal = clbal.Length > 199 ? clbal.Substring(0, 198) : clbal;
                            filterResponse.Vendor_CL_Bal = Convert.ToDecimal(clbal);
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")clbal  excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.VENDOR_OP_BAL) //Vendor_OP_Bal
                    {

                        try
                        {
                            string opbal = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal=" + opbal + ", ";
                            opbal = opbal.Length > 50 ? opbal.Substring(0, 45) : opbal;
                            filterResponse.Vendor_OP_Bal = Convert.ToDecimal(opbal);
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")opbal excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.LAPUNO) //Lapu Number
                    {
                        try
                        {
                            string lapuno = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")lapuno=" + lapuno + ", ";
                            filterResponse.LapuNo = lapuno.Length > 50 ? lapuno.Substring(0, 45) : lapuno;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")lapuno excp= " + ex.Message;
                            //LIBS.Common.LogException(ex);
                        }

                    }
                    else if (tg.TagId == TAGName.COMPLAINT_ID)
                    {

                        try
                        {
                            var cid = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")COMPLAINT_ID=" + cid + ", ";
                            filterResponse.Complaint_Id = cid.Length > 50 ? cid.Substring(0, 45) : cid;
                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")COMPLAINT_ID excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }



                    }
                    else if (tg.TagId == TAGName.R_OFFER)
                    {

                        try
                        {
                            string ro = apires.GetSubstring(tg.PreTxt, tg.PostText, tg.PreMargin ?? 0, tg.PostMargin ?? 0);
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")roffer=" + ro + ", ";
                            ro = ro.Length > 50 ? ro.Substring(0, 45) : ro;
                            filterResponse.R_Offer = Convert.ToDecimal(ro);

                        }
                        catch (Exception ex)
                        {
                            log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")roffer excp= " + ex.Message;
                            // LIBS.Common.LogException(ex);
                        }

                    }
                }
            }

            filterResponse.Message = "status=" + filterResponse.Status + " and msg=" + filterResponse.Message;

            return filterResponse.RequestTxnId;
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

            var url = stChkUrl.URL?.ReplaceURL(apisource.ApiUserId, apisource.ApiPassword, apisource.Remark, recharge.CustomerNo, opcode.OpCode, circlecode, apiamount, ourref, "", recharge.ApiTxnId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, recharge.UserId.ToString());
            var postdata = !string.IsNullOrEmpty(stChkUrl.PostData) ? stChkUrl.PostData?.ReplaceURL(apisource.ApiUserId, apisource.ApiPassword, apisource.Remark, recharge.CustomerNo, opcode.OpCode, circlecode, apiamount, ourref, "", recharge.ApiTxnId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, recharge.UserId.ToString()) : "";

            url = url?.ReplaceURL(apisource.ApiUserId, apisource.ApiPassword, apisource.Remark, recharge.CustomerNo, opcode.OpCode, circlecode, apiamount, ourref, "", recharge.ApiTxnId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, recharge.UserId.ToString());
            postdata = !string.IsNullOrEmpty(postdata) ? postdata?.ReplaceURL(apisource.ApiUserId, apisource.ApiPassword, apisource.Remark, recharge.CustomerNo, opcode.OpCode, circlecode, apiamount, ourref, "", recharge.ApiTxnId, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, recharge.UserId.ToString()) : "";

            string apires = string.Empty;
            if (apisource.Id == 12)
            {
                RechargeAPI mobiPactservice = new RechargeAPI();
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;
                apires = mobiPactservice.Status(apisource.ApiUserId, apisource.ApiPassword, recharge.OurRefTxnId);
                requestResponse.ResponseText = apires;
            }
            else if (apisource.Id == 159)
            {
                string g = "{\"header\":{\"operatingSystem\":\"web\",\"sessionId\":\"" + stChkUrl.PostData + "\",\"version\":\"1.0.0\"},\"transaction\":{\"requestType\":\"TMH\",\"requestSubType\":\"WTAL\",\"tranCode\":0,\"txnAmt\":0.0,\"id\":\"" + stChkUrl.PostData + "\"},\"payOutBean\":{\"customerId\":\"" + stChkUrl.PostData + "\",\"orderRefNo\":\"" + ourref + "\"}}";
                string ds = SpaceXpay.encrypt(g, stChkUrl.URL);
                RequestResponseDto requestResponseDto = new RequestResponseDto();
                string response12 = apiCall.PostSpaceXPay(ds, stChkUrl.PostData, ref requestResponse);
                dynamic res = JObject.Parse(response12);
                apires = SpaceXpay.decrypt(Convert.ToString(res.payload), stChkUrl.URL);
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

            // string reqtxnid = FilterRespTagValue(recharge.ApiId ?? 0, stChkUrl.Id, stChkUrl.ResType, apires, ref status, ref statusId, ref apitxnid, ref statusmsg, ref optxnid, ref log, ref Vendor_CL_Bal, ref Vendor_OP_Bal);
            FilterResponseModel fResp = new FilterResponseModel();
            FilterRespTagValue(recharge.ApiId ?? 0, stChkUrl.Id, stChkUrl.ResType, apires, ref statusId, ref log, ref fResp);
            optxnid = fResp.OperatorTxnID;
            apitxnid = fResp.ApiTxnID;
            optxnid = fResp.OperatorTxnID;
            statusmsg = fResp.Message;
            statusId = fResp.StatusId;
            status = fResp.Status;
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

        [HttpPost]
        public ActionResult ResendRechargeBulk(RechargeUpdateDto model, FormCollection FC)
        {
            UpdateActivity("ResendRechargeBulk REQUEST", "POST:RechargeReport/ResendRechargeBulk/", "recids=" + model.RecIds);
            ViewBag.actionAllowed = action = ActionAllowed("ProcessingRecharge", CurrentUser.RoleId, 3);

            string log = "resendbulk-start recids=" + model.RecIds + ", apiid=" + model.ApiId;
            int count = 0;
            int totalcount = 0;
            int userid = CurrentUser.UserID;
            int chk = model.IsActive ? 1 : 0;

            if (string.IsNullOrEmpty(model.RecIds))
            {
                ShowErrorMessage("Error!", "No recharge selected", false);
            }
            else
            {
                var recIdList = model.RecIds.Replace(" ", "").Split(',').ToList();

                recIdList = recIdList.Where(x => !string.IsNullOrEmpty(x)).ToList();
                count = recIdList.Count;

                if (recIdList.Count > 0)
                {
                    totalcount = recIdList.Count;

                    foreach (var rId in recIdList)
                    {
                        try
                        {
                            Thread thread = new Thread(new ThreadStart(() => ResendRechargeThread(rId, model.ApiId, chk, userid)));
                            thread.Start();
                        }
                        catch (Exception ex)
                        {
                            LogException(ex, "resend-Exception");
                        }

                    }
                }

                ShowSuccessMessage("Success!", "Recharge Resend Done.(" + totalcount + ")", false);
            }
            log += "\r\n, end ";
            LogActivity(log);
            return RedirectToAction("ProcessingRecharge");

        }

        private Dto.RechargeDetail GetDMTDetail(long recid, string remark)
        {
            DataTable dt = new DataTable();
            Dto.RechargeDetail model = new Dto.RechargeDetail();


            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_GetDMTDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RecId", recid);
                    cmd.Parameters.AddWithValue("@FilterType", remark);
                    cmd.Parameters.Add("@ErrorCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Log", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                    {

                        var row = dt.Rows[0];

                        model.RecId = !string.IsNullOrEmpty(Convert.ToString(row["Id"])) ? Convert.ToInt64(row["Id"]) : 0;
                        model.UserId = !string.IsNullOrEmpty(Convert.ToString(row["UserId"])) ? Convert.ToInt32(row["UserId"]) : 0;
                        model.ApiId = !string.IsNullOrEmpty(Convert.ToString(row["ApiId"])) ? Convert.ToInt32(row["ApiId"]) : 0;
                        model.TxnId = !string.IsNullOrEmpty(Convert.ToString(row["TxnId"])) ? Convert.ToInt64(row["TxnId"]) : 0;
                        model.CustomerNo = Convert.ToString(row["CustomerNo"]);
                        model.OpId = !string.IsNullOrEmpty(Convert.ToString(row["OpId"])) ? Convert.ToInt32(row["OpId"]) : 0;
                        model.Amount = !string.IsNullOrEmpty(Convert.ToString(row["Amount"])) ? Convert.ToDecimal(row["Amount"]) : 0;
                        model.StatusId = !string.IsNullOrEmpty(Convert.ToString(row["StatusId"])) ? Convert.ToInt32(row["StatusId"]) : 0;
                        model.BeneficiaryName = !string.IsNullOrEmpty(Convert.ToString(row["BeneficiaryName"])) ? Convert.ToString(row["BeneficiaryName"]) : "";

                        model.StatusMsg = Convert.ToString(row["StatusMsg"]);
                        model.UserTxnId = Convert.ToString(row["UserTxnId"]);
                        model.OurRefTxnId = Convert.ToString(row["OurRefTxnId"]);
                        model.OptTxnId = Convert.ToString(row["OptTxnId"]);
                        model.AccountNo = Convert.ToString(row["AccountNo"]);
                        model.UrlId = !string.IsNullOrEmpty(Convert.ToString(row["UrlId"])) ? Convert.ToInt32(row["UrlId"]) : 0;
                        model.ApiUrl = Convert.ToString(row["ApiUrl"]);
                        model.Method = Convert.ToString(row["Method"]);
                        model.ContentType = Convert.ToString(row["ContentType"]);
                        model.ResType = Convert.ToString(row["ResType"]);
                        model.PostData = Convert.ToString(row["PostData"]);
                        model.OpCode = Convert.ToString(row["OpCode"]);
                        model.ExtraUrl = Convert.ToString(row["ExtraUrl"]);
                        model.ExtraUrlData = Convert.ToString(row["ExtraUrlData"]);
                        model.ApiUserId = Convert.ToString(row["ApiUserId"]);
                        model.ApiPassword = Convert.ToString(row["ApiPassword"]);
                        model.ApiOptional = Convert.ToString(row["ApiOptional"]);
                        model.ApiTypeId = !string.IsNullOrEmpty(Convert.ToString(row["ApiTypeId"])) ? Convert.ToInt32(row["ApiTypeId"]) : 0;
                        model.CallbackURL = Convert.ToString(row["CallbackURL"]);
                        model.DB_Amt = !string.IsNullOrEmpty(Convert.ToString(row["DB_Amt"])) ? Convert.ToDecimal(row["DB_Amt"]) : 0;
                        model.TxnRemark = Convert.ToString(row["TxnRemark"]);
                        model.ApiName = Convert.ToString(row["ApiName"]);

                        model.RequestTime = Convert.ToString(row["RequestTime"]);
                        model.CallbackId = Convert.ToString(row["CallbackId"]);


                    }

                }
            }
            catch (Exception ex)
            {
                // log += "\r\n , excp=" + ex.Message;
                LogException(ex, "GetRechargeDetail RecId=" + recid);
            }

            return model;

        }


        private void RCresend(RechargeUpdateDto model, string rId, ref string log, ref int count)
        {
            long recid = 0;
            bool IsRefund = false;
            bool IsDownline = false;
            string spLog = "";
            string remark = "resend";
            int statusId = 0;
            string apitxnid = "NA";
            string optxnid = "NA";
            string statusmsg = "resend";

            log += "\r\n, recid=" + rId;
            try
            {

                //get recharge
                recid = Convert.ToInt64(rId);

                var recharge = rechargeService.GetRecharge(recid);

                model.ApiId = model.ApiId > 0 ? model.ApiId : recharge.ApiId ?? 0;



                var apisource = apiService.GetApiSource(model.ApiId);

                if (model.IsActive && !apisource.ApiName.ToLower().Contains("default"))
                {
                    log += "\r\n, checkstatusApi= " + recharge.ApiId;
                    CheckStatus(recharge, remark, ref IsDownline, ref apitxnid, ref statusmsg, ref optxnid, ref statusId, ref log);

                }
                else
                {
                    log += "\r\n, status-not-checked-Api=" + recharge.ApiId + "-" + apisource.ApiName;
                }

                //check status

                if (recharge.ResendTime == null && (statusId == 3 || apisource.ApiName.ToLower().Contains("processing") || !model.IsActive) && recharge.StatusId == 2)
                {
                    string resendurl = string.Empty;

                    TxnLedger ledger = walletService.GetTxnLedger(recharge.TxnId ?? 0);

                    var rk = ledger?.Remark?.ToLower() ?? string.Empty;

                    if (rk.Contains("billpay"))
                    {

                        resendurl = SiteKey.ApiDomainName + "Service/ResendBillPay?AuthToken=ResendRecharge&RecId=" + rId + "&ApiId=" + model.ApiId;

                    }
                    else
                    {
                        resendurl = SiteKey.ApiDomainName + "Service/ResendRecharge?AuthToken=ResendRecharge&RecId=" + rId + "&ApiId=" + model.ApiId;
                    }
                    log += "\r\n, resendurl=" + resendurl;

                    #region "APi Call"


                    ApiCall apiCall = new ApiCall(reqResService);

                    log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")apicall, ";

                    RequestResponseDto userReqRes = new RequestResponseDto();
                    userReqRes.UserId = recharge.UserId ?? 0;
                    userReqRes.RecId = Convert.ToInt64(recharge.Id);
                    userReqRes.RefId = recharge.OurRefTxnId;
                    userReqRes.Remark = "Resend";
                    userReqRes = AddUpdateReqRes(userReqRes, ref log);

                    string apires = apiCall.Get(resendurl, ref userReqRes, model.ApiId);

                    SetRechargeUpdatedBy("Resend", recharge.Id, CurrentUser.UserID, string.Empty, ref log);

                    log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")resp saved, ";
                    #endregion

                    if (string.IsNullOrEmpty(apires))
                    {
                        log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")blank reponse, ";
                        statusId = 2;
                    }
                    else
                    {
                        dynamic jObj = JsonConvert.DeserializeObject(apires);

                        if ((jObj.STATUS == "1" || jObj.STATUS == "3") && jObj.ERRORCODE == "0")
                        {
                            #region "Send Callback to User"
                            try
                            {
                                statusId = Convert.ToInt32(jObj.STATUS);

                                log += "\r\n, get user callback url ";
                                User user = userService.GetUser(recharge.UserId ?? 0);
                                if (user != null)
                                {

                                    SendCallBack(recharge, user.CallbackURL, optxnid, statusId, ref log);
                                }
                                else
                                {
                                    log += "\r\n, usernotfound";
                                }
                                log += "\r\n, callback sent to user ";
                            }
                            catch (Exception)
                            {

                                log += "\r\n , downline_callback_excp jObj.STATUS=" + Convert.ToString(jObj.STATUS) + " ,statusId=" + statusId;
                            }
                            #endregion

                        }
                    }

                }

                count += 1;
            }
            catch (Exception ex)
            {

                LogException(ex);
                log += "\r\n, excp(recid=" + recid + "): " + ex.Message;
            }
            LogActivity(log);
        }

        private void UpdateAEPSStatusWithCheck(long RecId, int statusId, ref string log)
        {
            using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
            {
                SqlCommand cmd = new SqlCommand("[usp_UpdateAepsStatus]", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@RecId", RecId);
                cmd.Parameters.AddWithValue("@StatusId", statusId);
                cmd.Parameters.Add("@ErrorCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("@Log", SqlDbType.NVarChar, 250).Direction = ParameterDirection.Output;
                log += "\r\n ,  before exec = usp_UpdateAepsStatus";
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                log += "\r\n ,  after exec = usp_UpdateAepsStatus";
                string error = Convert.ToString(cmd.Parameters["@ErrorCode"].Value);
                string ErrorDesc = Convert.ToString(cmd.Parameters["@ErrorDesc"].Value);
                string Log = Convert.ToString(cmd.Parameters["@Log"].Value);
                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")spLog=" + Log;
            }



        }
        private void UpdateStatusWithCheck(long RecId, int statusId, string apitxnid, string optxnid, string statusmsg, string remark, ref bool IsDownline, ref bool IsRefund, ref string log, long lapuid, string lapuno, int opid, string updatetype = "StatusWithCheck")
        {
            using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
            {
                SqlCommand cmd = new SqlCommand("usp_UpdateRechargeStatus", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UpdateType", updatetype);
                cmd.Parameters.AddWithValue("@RecId", RecId);
                cmd.Parameters.AddWithValue("@StatusId", statusId);
                if (CurrentUser != null)
                    cmd.Parameters.AddWithValue("@UserId", CurrentUser.UserID);
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
        #region payin 
        private void UpdateStatusWithCheckPayin(long RecId, int statusId, string apitxnid, string optxnid, string statusmsg, string remark, ref bool IsDownline, ref bool IsRefund, ref string log, long lapuid, string lapuno, int opid, string updatetype = "StatusWithCheck")
        {
            using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
            {
                SqlCommand cmd = new SqlCommand("usp_UpdateRechargeStatusPayin", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UpdateType", updatetype);
                cmd.Parameters.AddWithValue("@RecId", RecId);
                cmd.Parameters.AddWithValue("@StatusId", statusId);
                if (CurrentUser != null)
                    cmd.Parameters.AddWithValue("@UserId", CurrentUser.UserID);
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

                log += "\r\n ,  before exec = usp_UpdateRechargeStatusPayin";
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                log += "\r\n ,  after exec = usp_UpdateRechargeStatusPayin";
                string Refund = Convert.ToString(cmd.Parameters["@IsRefund"].Value);
                string Downline = Convert.ToString(cmd.Parameters["@IsDownline"].Value);
                string spLog = Convert.ToString(cmd.Parameters["@Log"].Value);


                IsRefund = !string.IsNullOrEmpty(Refund) ? Convert.ToBoolean(Refund) : false;
                IsDownline = !string.IsNullOrEmpty(Downline) ? Convert.ToBoolean(Downline) : false;

                log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")spLog=" + spLog;
            }

            SetRechargeUpdatedBy("Update", RecId, CurrentUser != null ? CurrentUser.UserID : 1, string.Empty, ref log);

        }

        #endregion
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
            var action = ActionAllowed("Complaint", CurrentUser.RoleId, 2);
            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;
            ComplaintDto model = new ComplaintDto();
            if (id.HasValue && id.Value > 0)
            {
                model.RecId = id ?? 0;
            }

            return PartialView("_CreateComplaint", model);

        }

        [HttpPost]
        public ActionResult CreateComplaint(ComplaintDto model, FormCollection FC)
        {
            long actid = UpdateActivity("CreateComplaint REQUEST", "POST:RechargeReport/CreateComplaint/", "");
            var action = ActionAllowed("Complaint", CurrentUser.RoleId, 2);

            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;


            string message = string.Empty;
            string log = "";
            int count = 0;

            CheckAndUpdateRCStatusAndCalllback(ref log, ref count, model.RecId.ToString());

            Complaint complaint = new Complaint();
            Recharge recharge = rechargeReportService.GetRecharge(model.RecId);

            if (recharge != null)
            {
                var rcModel = GetRechargeDetail(model.RecId, "complaint");

                try
                {
                    if (recharge.OpId == 1)
                    {
                        string planapiurl = "http://planapi.in//api/Mobile/RechargeCheck?ApiUserID=3014&ApiPassword=549079&Mobileno=" + recharge.CustomerNo + "&Amount=" + recharge.Amount.ToString().Replace(".00", "") + "&Operator=2";
                        var presp = GetRequest(planapiurl);
                        dynamic jObj = JObject.Parse(presp);
                        if (jObj.STATUS == "1")
                        {
                            complaint.ExpiryDate = DateTime.ParseExact(jObj.ExpiryDate.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            complaint.LastDate = DateTime.ParseExact(jObj.LastDate.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            rechargeService.Save(complaint);
                        }
                    }

                }
                catch (Exception ex)
                {

                }
                var Complaint = rechargeReportService.GetComplaintByRecId(model.RecId);

                complaint.RecId = model.RecId;
                complaint.ComplaintDate = DateTime.Now;
                complaint.Remark = model.Remark;
                if (Complaint.Count == 0)
                {
                    complaint.StatusId = 5; //pending
                }
                else
                {
                    complaint.StatusId = 9; //ReOpen
                }
                //complaint.StatusId = 5; //pending
                complaint.ComplaintById = CurrentUser.UserID;
                rechargeReportService.Save(complaint);

                rcModel.CompId = complaint.Id;
                rcModel.CompRemark = "Complaint-FWD";

                if (rcModel.StatusId == 1)
                {
                    int statusId = 0;
                    var resp = SendComplaint(rcModel);

                    FilterResponseModel fResp = new FilterResponseModel();
                    FilterRespTagValue(recharge.ApiId ?? 0, rcModel.UrlId, rcModel.ResType, resp, ref statusId, ref log, ref fResp);

                    complaint.ApiComplaintId = fResp.Complaint_Id;
                    complaint.ApiResponse = fResp.Message;
                    rechargeService.Save(complaint);
                }

            }

            UpdateActivity("CreateComplaint REQUEST", "POST:RechargeReport/CreateComplaint/", "comp_Id=" + complaint.Id, actid);
            #region "set route values"

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
        public JsonResult CreateComplaint1(long RecId, string Remark)
        {
            long actid = UpdateActivity("CreateComplaint REQUEST", "POST:RechargeReport/CreateComplaint/", "");
            var action = ActionAllowed("Complaint", CurrentUser.RoleId, 2);

            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;


            string message = string.Empty;
            string log = "";
            int count = 0;

            CheckAndUpdateRCStatusAndCalllback(ref log, ref count, RecId.ToString());

            Complaint complaint = new Complaint();
            Recharge recharge = rechargeReportService.GetRecharge(RecId);

            if (recharge != null)
            {
                var rcModel = GetRechargeDetail(RecId, "complaint");

                try
                {
                    if (recharge.OpId == 1)
                    {
                        string planapiurl = "http://planapi.in//api/Mobile/RechargeCheck?ApiUserID=3014&ApiPassword=549079&Mobileno=" + recharge.CustomerNo + "&Amount=" + recharge.Amount.ToString().Replace(".00", "") + "&Operator=2";
                        var presp = GetRequest(planapiurl);
                        dynamic jObj = JObject.Parse(presp);
                        if (jObj.STATUS == "1")
                        {
                            complaint.ExpiryDate = DateTime.ParseExact(jObj.ExpiryDate.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            complaint.LastDate = DateTime.ParseExact(jObj.LastDate.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            rechargeService.Save(complaint);
                        }
                    }

                }
                catch (Exception ex)
                {

                }
                var Complaint = rechargeReportService.GetComplaintByRecId(RecId);
                complaint.RecId = RecId;
                complaint.ComplaintDate = DateTime.Now;
                complaint.Remark = Remark;
                if (Complaint.Count == 0)
                {
                    complaint.StatusId = 5; //pending
                }
                else
                {
                    complaint.StatusId = 9; //ReOpen
                }
                complaint.ComplaintById = CurrentUser.UserID;
                rechargeReportService.Save(complaint);
                message = "1";
                rcModel.CompId = complaint.Id;
                rcModel.CompRemark = "Complaint-FWD";

                if (rcModel.StatusId == 1)
                {
                    int statusId = 0;
                    var resp = SendComplaint(rcModel);

                    FilterResponseModel fResp = new FilterResponseModel();
                    FilterRespTagValue(recharge.ApiId ?? 0, rcModel.UrlId, rcModel.ResType, resp, ref statusId, ref log, ref fResp);

                    complaint.ApiComplaintId = fResp.Complaint_Id;
                    complaint.ApiResponse = fResp.Message;
                    rechargeService.Save(complaint);
                }

            }

            UpdateActivity("CreateComplaint REQUEST", "POST:RechargeReport/CreateComplaint1/", "comp_Id=" + complaint.Id, actid);

            return Json(message, JsonRequestBehavior.AllowGet);
            //#region "set route values"

            //IDictionary<string, object> routeValues = new Dictionary<string, object>();

            //if (flt.Apiid > 0) routeValues.Add("v", flt.Apiid);
            //if (flt.Opid > 0) routeValues.Add("o", flt.Opid);
            //if (flt.Isa > 0) routeValues.Add("i", flt.Isa);
            //if (flt.Sid > 0) routeValues.Add("s", flt.Sid);
            //if (flt.Uid > 0) routeValues.Add("u", flt.Uid);
            //if (flt.Circleid > 0) routeValues.Add("c", flt.Circleid);
            //if (!string.IsNullOrEmpty(flt.Sdate)) routeValues.Add("f", flt.Sdate);
            //if (!string.IsNullOrEmpty(flt.Edate)) routeValues.Add("e", flt.Edate);
            //if (!string.IsNullOrEmpty(flt.Searchid)) routeValues.Add("rto", flt.Searchid);
            //if (!string.IsNullOrEmpty(flt.CustomerNo)) routeValues.Add("m", flt.CustomerNo);
            //if (!string.IsNullOrEmpty(flt.UserReqid)) routeValues.Add("ut", flt.UserReqid);
            //if (!string.IsNullOrEmpty(flt.OpTxnid)) routeValues.Add("ot", flt.OpTxnid);
            //if (!string.IsNullOrEmpty(flt.ApiTxnid)) routeValues.Add("vt", flt.ApiTxnid);
            //#endregion

            // return RedirectToAction("Index", new RouteValueDictionary(routeValues));
        }
        // GET: Report
        [HttpGet]   
        public ActionResult Complaint(int? u, int? v, int? o, int? s, int? c, long? rt, string f = "", string m = "", string e = "", int i = 0, string ot = "")
        {
            UpdateActivity("Complaint Report Request", "GET:RechargeReport/Complaint", string.Empty);
            action = ActionAllowed("Complaint", CurrentUser.RoleId);

            var optlist = operatorSwitchService.circlesList();

            RechargeFilterDto filter = new RechargeFilterDto();

            filter.Uid = u ?? 0;
            filter.Isa = i;
            filter.Apiid = v ?? 0;
            filter.Opid = o ?? 0;
            filter.Sid = s ?? 25;
            filter.Sdate = f;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyy");
            filter.Edate = e;
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("dd/MM/yyy");
            filter.CustomerNo = m;
            filter.Circleid = Convert.ToInt32(c.HasValue ? c : 0);

            if (rt != null)
                filter.Searchid = rt.ToString();

            filter.OpTxnid = ot;

            ViewBag.FilterData = TempData["ComplaintFilterDto"] = filter;

            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? 0 : CurrentUser.UserID;

            List<SelectListItem> itemlist = new List<SelectListItem>()
                {
                    new SelectListItem() { Text="ALL", Value="11" },
            };
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == filter.Opid) }).ToList();
            ViewBag.StatusList = rechargeReportService.GetStatusList().Where(r => r.Remark.Contains("Complaint")).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = ((x.Id == filter.Sid)) }).Union(itemlist).ToList();
            ViewBag.CircleList = operatorSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == filter.Circleid) }).ToList();
            ViewBag.UserList = userService.GetUserList().Where(x => uid == 0 ? x.RoleId == 3 : x.Id == uid && x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? string.Empty, Selected = (x.Id == filter.Uid) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == filter.Apiid) }).ToList();
            //ViewBag.StatusList += itemlist;
            return View();
        }

        [HttpPost]
        public ActionResult GetComplaintReport(DataTableServerSide model, FormCollection FC)
        {
            string log = "ComplaintFilterDto: ";
            ViewBag.actionAllowed = action = ActionAllowed("Complaint", CurrentUser.RoleId);
            RechargeFilterDto flt = TempData["ComplaintFilterDto"] != null ? (RechargeFilterDto)TempData["ComplaintFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["ComplaintFilterDto"] = flt;
            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            flt.Uid = IsAdminRole ? flt.Uid : CurrentUser.UserID;
            model.filterdata.UserId = IsAdminRole ? model.filterdata.UserId : CurrentUser.UserID;
            KeyValuePair<int, List<Complaint>> complaints = new KeyValuePair<int, List<Complaint>>();
            log += "flt.Uid=" + flt.Uid + ",flt.Apiid=" + flt.Apiid + ",flt.Opid=" + flt.Opid + ",flt.Sid=" + flt.Sid + ", flt.Sdate=" + flt.Sdate + ",flt.Edate=" + flt.Edate + ", flt.Circleid=" + flt.Circleid + ",flt.CustomerNo=" + flt.CustomerNo;
            try
            {
                complaints = rechargeReportService.GetComplaintReport(model, flt.Uid, flt.Apiid, flt.Opid, Convert.ToByte(flt.Sid), flt.Sdate, flt.Edate, flt.Circleid, flt.CustomerNo);
            }
            catch (Exception ex)
            {
                log += " , excp-1=" + ex.Message;
                LogException(ex, "complaint report");

                try
                {
                    complaints = rechargeReportService.GetComplaintReport(model, flt.Uid, flt.Apiid, flt.Opid, Convert.ToByte(flt.Sid), flt.Sdate, flt.Edate, flt.Circleid, flt.CustomerNo);

                }
                catch (Exception ex2)
                {
                    log += " , excp-2=" + ex2.Message;

                    LogException(ex, "ex2-complaint report");
                }

            }

            LogActivity(log);
            return Json(new
            {
                draw = model.draw,
                recordsTotal = complaints.Key,
                recordsFiltered = complaints.Key,
                data = complaints.Value.Select(c => new List<object> {
                    c.Id,
                    IsAdminRole && c.ComplaintById==c.Recharge.UserId ? c.User?.UserProfile?.FullName??string.Empty: c.ComplaintById==c.Recharge.UserId? c.Recharge.User.UserProfile.FullName:"Admin",
                    (c.ComplaintDate)?.ToString(),
                    (IsAdminRole && action.AllowEdit? DataTableButton.HyperLink(Url.Action( "resolvecomplaint", "rechargereport",new { id = c.Id }),"modal-resolve-complaint", c.StatusType.TypeName,"Resolve",setColor(c.StatusId)): "<b style='color:"+setColor(c.StatusId)+"'>"+c.StatusType.TypeName+"</b>")
                    +(IsAdminRole && action.AllowEdit? "&nbsp;"+ DataTableButton.RefreshButton(Url.Action("checkcomplaint","rechargereport",new { cid=c.Id})): string.Empty),
                    c.RecId,
                    (c.Recharge.RequestTime)?.ToString(),
                    c.Recharge?.StatusType?.TypeName??string.Empty,
                    c.Recharge.CustomerNo,
                    IsAdminRole?c.Recharge?.ApiSource?.ApiName??string.Empty:c.Recharge?.ApiId?.ToString(),
                    c.Recharge?.Operator?.Name??string.Empty,
                    c.Recharge?.Amount??0,
                    (c.UpdatedDate)?.ToString(),
                    c.Remark,
                    c.Comment,
                    IsAdminRole?c.User2?.UserProfile?.FullName??string.Empty: c.User2!=null?"Admin": string.Empty,
                    c.Recharge?.OptTxnId??string.Empty,
                    c.IsRefund==true?"Yes":"",
                    c.Recharge.OurRefTxnId,
                    (c.LastDate)?.ToString("dd/MM/yyyy"),
                    (c.ExpiryDate)?.ToString("dd/MM/yyyy")
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
                Complaint complaint = rechargeReportService.GetComplaint(id.Value);

                model.ComplaintId = complaint.Id;
                model.RecId = complaint.RecId ?? 0;
                model.StatusId = complaint.StatusId ?? 0;


                var stlist = rechargeReportService.GetStatusList().Where(s => s.Remark.Contains("Complaint")).Select(x => new StatusTypeDto()
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

            var oldsid = 0;
            var newsid = 0;


            string message = string.Empty;
            string log = "ResolveComplain-start";
            Complaint complaint = rechargeReportService.GetComplaint(model.ComplaintId);

            oldsid = complaint.StatusId ?? 0;

            Recharge recharge = rechargeReportService.GetRecharge(model.RecId);
            string optxnid = !string.IsNullOrWhiteSpace(model.OptTxnId) ? model.OptTxnId : recharge.OptTxnId;

            if (complaint.StatusId == 5 || complaint.StatusId == 2 || complaint.StatusId == 9)
            {
                complaint.UpdatedById = CurrentUser.UserID;
                complaint.StatusId = model.IsResolved ? Convert.ToByte(8) : Convert.ToByte(2); //8-resolved, 2-processing
                complaint.Comment = model.Comment;
                log += "\r\n model.IsResolved=" + model.IsResolved;
                if (model.IsRefund)
                {
                    complaint.IsRefund = model.IsRefund;

                    log += "\r\n\r\n model.IsRefund=" + model.IsRefund;

                    bool IsDownline = false;
                    bool IsRefund = false;

                    // UpdateStatusWithCheck(recharge.Id, 3, recharge.ApiTxnId, optxnid, recharge.StatusMsg, "Complaint", ref IsDownline, ref IsRefund, ref log);
                    UpdateStatusWithCheck(recharge.Id, StatsCode.Refunded, recharge.ApiTxnId, optxnid, recharge.StatusMsg, "Complaint", ref IsDownline, ref IsRefund, ref log, 0, string.Empty, 0);

                    #region "Send Callback to User"
                    log += "\r\nget user callback url ";
                    User user = userService.GetUser(recharge.UserId ?? 0);
                    if (user != null && IsDownline)
                    {
                        SendCallBack(recharge, user.CallbackURL, string.IsNullOrEmpty(optxnid) ? recharge.OptTxnId : optxnid, StatsCode.FAILED, ref log);
                    }
                    else
                    {
                        log += "\r\nsame status or user not found ";
                    }
                    log += "\r\ncallback sent to user ";
                    #endregion
                }
                rechargeReportService.Save(complaint);

                newsid = complaint.StatusId ?? 0;

                if (oldsid != newsid)
                    SendComplaintCallBack(complaint.RecId ?? 0, complaint.Id, complaint.StatusId ?? 0, complaint.Remark, complaint.Comment);

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

                    //  ApiUrl apiUrl = apiService.GetApiurl(urlid);
                    ApiSource apiSource = apiService.GetApiSource(apiurl.ApiId);

                    string postdata = apiurl.PostData;
                    string complainUrl = apiurl.URL;
                    var oprator = rechargeService.GetOperator("", recharge.OpId ?? 0);
                    var opcode = rechargeService.GetOperatorByApiId(recharge.OpId ?? 0, recharge.ApiId ?? 0);

                    try
                    {
                        if (recharge.OpId == 1)
                        {
                            string planapiurl = "http://planapi.in//api/Mobile/RechargeCheck?ApiUserID=3014&ApiPassword=549079&Mobileno=" + recharge.CustomerNo + "&Amount=" + recharge.Amount.ToString().Replace(".00", "") + "&Operator=2";
                            var presp = GetRequest(planapiurl);
                            dynamic jObj = JObject.Parse(presp);
                            if (jObj.STATUS == "1")
                            {
                                complaint.ExpiryDate = DateTime.ParseExact(jObj.ExpiryDate.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                complaint.LastDate = DateTime.ParseExact(jObj.LastDate.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                rechargeService.Save(complaint);
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    int unitid = 0;
                    int len = 0;
                    int reflen = 0;
                    string randomkey = "";
                    string dtformat = "";
                    string refpadding = "";
                    string datetime = "";

                    GetApiExtDetails(apiurl.ApiId ?? 0, ref unitid, ref len, ref randomkey, ref dtformat, ref refpadding, ref log, ref reflen);
                    datetime = DateTime.Now.ToString(dtformat);

                    complainUrl = complainUrl.ReplaceURL(apiSource.ApiUserId, apiSource.ApiPassword, apiSource.Remark, complaint?.Id.ToString(), opcode?.OpCode ?? "", "", recharge.Amount.ToString(), recharge.OurRefTxnId, string.Empty, string.Empty, opcode.ExtraUrl, opcode.ExtraUrlData, recharge.AccountNo, recharge.AccountOther, recharge.Optional1, recharge.Optional2, recharge.Optional3, recharge.Optional4, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, complaint.ApiComplaintId, string.Empty, recharge.UserId.ToString());
                    postdata = !string.IsNullOrEmpty(postdata) ? postdata?.ReplaceURL(apiSource.ApiUserId, apiSource.ApiPassword, apiSource.Remark, complaint?.Id.ToString(), opcode?.OpCode ?? "", "", recharge.Amount.ToString(), recharge.OurRefTxnId, string.Empty, string.Empty, opcode.ExtraUrl, opcode.ExtraUrlData, recharge.AccountNo, recharge.AccountOther, recharge.Optional1, recharge.Optional2, recharge.Optional3, recharge.Optional4, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, complaint.ApiComplaintId, string.Empty, recharge.UserId.ToString()) : "";

                    complainUrl = complainUrl.ReplaceURL(apiSource.ApiUserId, apiSource.ApiPassword, apiSource.Remark, complaint?.Id.ToString(), opcode?.OpCode ?? "", "", recharge.Amount.ToString(), recharge.OurRefTxnId, string.Empty, string.Empty, opcode.ExtraUrl, opcode.ExtraUrlData, recharge.AccountNo, recharge.AccountOther, recharge.Optional1, recharge.Optional2, recharge.Optional3, recharge.Optional4, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, complaint.ApiComplaintId, string.Empty, recharge.UserId.ToString());
                    postdata = !string.IsNullOrEmpty(postdata) ? postdata?.ReplaceURL(apiSource.ApiUserId, apiSource.ApiPassword, apiSource.Remark, complaint?.Id.ToString(), opcode?.OpCode ?? "", "", recharge.Amount.ToString(), recharge.OurRefTxnId, string.Empty, string.Empty, opcode.ExtraUrl, opcode.ExtraUrlData, recharge.AccountNo, recharge.AccountOther, recharge.Optional1, recharge.Optional2, recharge.Optional3, recharge.Optional4, datetime, randomkey, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, complaint.ApiComplaintId, string.Empty, recharge.UserId.ToString()) : "";
                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")after replace" + " complainUrl=" + apiurl.URL + " PostData=" + apiurl.PostData;

                    #region "APi Call"





                    RequestResponseDto requestResponse = new RequestResponseDto();

                    if (Convert.ToInt32(recharge.UserId) > 0)
                    {
                        requestResponse.UserId = Convert.ToInt32(recharge.UserId);
                    }

                    if (complainUrl != null)
                        requestResponse.UrlId = Convert.ToInt32(apiurl.Id);

                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + "), (set reqres) userid=" + recharge.UserId + " urlid=" + apiurl.Id;

                    requestResponse.Remark = "Complain_S";
                    ApiCall apiCall = new ApiCall(reqResService);

                    log += "\r\n (" + DateTime.Now.TimeOfDay.ToString() + ")apicall, ";

                    string apires = apiurl.Method == "POST" ?
                                    apiCall.Post(complainUrl, postdata, apiurl.ContentType, apiurl.ResType, ref requestResponse, apiurl.ApiId ?? 0, apiSource.ApiUserId, apiSource.ApiPassword)
                                    : apiCall.Get(complainUrl, ref requestResponse, apiurl.ApiId ?? 0);


                    requestResponse.RecId = recharge.Id;
                    requestResponse.RefId = recharge.OurRefTxnId;
                    requestResponse = AddUpdateReqRes(requestResponse, ref log);

                    string status = "Hold";
                    int statusId = 2;
                    bool IsDownline = false;
                    bool IsRefund = false;

                    FilterResponseModel fResp = new FilterResponseModel();
                    FilterRespTagValue(recharge.ApiId ?? 0, apiurl.Id, apiurl.ResType, apires, ref statusId, ref log, ref fResp);

                    if (statusId == 1 && (complaint.StatusId == 5 || complaint.StatusId == 2))
                    {
                        complaint.StatusId = 8;
                        complaint.Comment = "Auto Closed CHK";
                        complaint.ApiComplaintId = fResp.Complaint_Id;
                        complaint.ApiResponse = fResp.Message;
                        rechargeService.Save(complaint);

                        SendComplaintCallBack(complaint.RecId ?? 0, complaint.Id, complaint.StatusId ?? 0, complaint.Remark, complaint.Comment);

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

        [HttpPost]
        [AllowAnonymous]
        public string AutoCheckStatus()
        {
            string[] keys = Request.Form.AllKeys;

            string token = Request["token"];
            string recIds = Request["recid"];
            int totalcount = 0;
            var log = "AutoCheckStatus recid=" + recIds + ",token=" + token;
            string stst = "";
            UpdateActivity("AutoCheckStatus REQUEST", "POST:RechargeReport/AutoCheckStatus/", "recid=" + recIds);

            if (string.IsNullOrEmpty(token) || token != autochecktoken)
            {
                log += "\n auth fail";
                return "failed";
            }


            if (string.IsNullOrEmpty(recIds))
            {
                log += "\n no rec";
                stst = "failed";
            }
            else
            {

                var recIdList = recIds.Replace(" ", "").Split(',').ToList();

                recIdList = recIdList.Where(x => !string.IsNullOrEmpty(x)).ToList();
                int count = 0;
                totalcount = recIdList.Count;
                log += "\n no totalcount=" + totalcount;

                if (recIdList.Count > 0)
                {
                    foreach (var rId in recIdList)
                    {
                        //CheckStatusThread
                        CheckAndUpdateRCStatusAndCalllback(ref log, ref count, rId);
                        log += "\n rId start=" + rId;

                        log += "\n rId end(" + rId + ") count=" + count;
                    }
                }

                ShowSuccessMessage("Success!", "Status has been checked. " + count + " out of " + totalcount, false);
            }

            stst = "success";

            LogActivity(log);

            return stst;

        }

        [HttpPost]
        [AllowAnonymous]
        public string AutoCheckStatusTrhead()
        {
            string filepath = System.Web.HttpContext.Current.Server.MapPath("~/Activity_Log/");

            string[] keys = Request.Form.AllKeys;

            string token = Request["token"];
            string recIds = Request["recid"];
            int totalcount = 0;
            var log = "AutoCheckStatus recid=" + recIds + ",token=" + token;
            string stst = "";
            UpdateActivity("AutoCheckStatus REQUEST", "POST:RechargeReport/AutoCheckStatus/", "recid=" + recIds);

            if (string.IsNullOrEmpty(token) || token != autochecktoken)
            {
                log += "\n auth fail";
                return "failed";
            }


            if (string.IsNullOrEmpty(recIds))
            {
                log += "\n no rec";
                stst = "failed";
            }
            else
            {

                var recIdList = recIds.Replace(" ", "").Split(',').ToList();

                recIdList = recIdList.Where(x => !string.IsNullOrEmpty(x)).ToList();
                int count = 0;
                totalcount = recIdList.Count;
                log += "\n no totalcount=" + totalcount;

                if (recIdList.Count > 0)
                {
                    foreach (var rId in recIdList)
                    {
                        try
                        {
                            log += "\n rId start=" + rId;

                            Thread thread = new Thread(new ThreadStart(() => CheckStatusThread(rId)));
                            thread.Start();
                            log += "\n rId end(" + rId + ") count=" + count;
                        }
                        catch (Exception ex)
                        {

                            LogException(ex, "thread exception");
                        }

                    }
                }

                // ShowSuccessMessage("Success!", "Status has been checked. " + count + " out of " + totalcount, false);
            }

            stst = "success";

            LogActivity(log, filepath);

            return stst;

        }

        [HttpPost]
        [AllowAnonymous]
        public string RechargeGapThread()
        {
            string filepath = System.Web.HttpContext.Current.Server.MapPath("~/Activity_Log/");

            string[] keys = Request.Form.AllKeys;

            string token = Request["token"];
            string recIds = Request["recid"];
            int totalcount = 0;
            var log = "AutoCheckStatus recid=" + recIds + ",token=" + token;
            string stst = "";
            UpdateActivity("autoRechargeGapThread", "POST:RechargeReport/RechargeGapThread/", "recid=" + recIds + ",token=" + token);

            if (string.IsNullOrEmpty(token) || token != "RechargeGap")
            {
                log += "\n auth fail";
                return "failed";
            }


            if (string.IsNullOrEmpty(recIds))
            {
                log += "\n no rec";
                stst = "failed";
            }
            else
            {

                var recIdList = recIds.Replace(" ", "").Split(',').ToList();

                recIdList = recIdList.Where(x => !string.IsNullOrEmpty(x)).ToList();
                int count = 0;
                totalcount = recIdList.Count;
                log += "\n no totalcount=" + totalcount;

                LogActivity(log, filepath);

                if (recIdList.Count > 0)
                {
                    foreach (var rId in recIdList)
                    {
                        log += "\n rId start=" + rId;

                        Thread thread = new Thread(new ThreadStart(() => CallRechargeGapAPI(rId)));
                        thread.Start();

                        log += "\n rId end(" + rId + ") count=" + count;
                    }
                }

                // ShowSuccessMessage("Success!", "Status has been checked. " + count + " out of " + totalcount, false);
            }

            stst = "success";

            // LogActivity(log, filepath);

            return stst;

        }

        public void CallRechargeGapAPI(string rcid)
        {

            var url = SiteKey.ApiDomainName + "Service/RechargeGap?AuthToken=RechargeGap&RecId=" + rcid;

            try
            {
                long recid = !string.IsNullOrEmpty(rcid) ? Convert.ToInt64(rcid) : 0;
                var recharge = GetRechargeDetail(recid, string.Empty);

                if (recharge.TxnRemark.Contains("billpay"))
                    url = SiteKey.ApiDomainName + "Service/BillPayGap?AuthToken=RechargeGap&RecId=" + rcid;

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                HttpWebRequest httpreq = (HttpWebRequest)WebRequest.Create(url);
                httpreq.GetResponseAsync();
            }
            catch (Exception ex)
            {

            }
        }

        private Dto.RechargeDetail GetRechargeDetail(long recid, string remark)
        {
            DataTable dt = new DataTable();
            Dto.RechargeDetail model = new Dto.RechargeDetail();


            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_GetRechargeDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@RecId", recid);
                    cmd.Parameters.AddWithValue("@FilterType", remark);
                    cmd.Parameters.Add("@ErrorCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 50).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Log", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dt);
                    }

                    if (dt.Rows.Count > 0)
                    {

                        var row = dt.Rows[0];

                        model.RecId = !string.IsNullOrEmpty(Convert.ToString(row["Id"])) ? Convert.ToInt64(row["Id"]) : 0;
                        model.UserId = !string.IsNullOrEmpty(Convert.ToString(row["UserId"])) ? Convert.ToInt32(row["UserId"]) : 0;
                        model.ApiId = !string.IsNullOrEmpty(Convert.ToString(row["ApiId"])) ? Convert.ToInt32(row["ApiId"]) : 0;
                        model.TxnId = !string.IsNullOrEmpty(Convert.ToString(row["TxnId"])) ? Convert.ToInt64(row["TxnId"]) : 0;
                        model.CustomerNo = Convert.ToString(row["CustomerNo"]);
                        model.OpId = !string.IsNullOrEmpty(Convert.ToString(row["OpId"])) ? Convert.ToInt32(row["OpId"]) : 0;
                        model.Amount = !string.IsNullOrEmpty(Convert.ToString(row["Amount"])) ? Convert.ToDecimal(row["Amount"]) : 0;
                        model.StatusId = !string.IsNullOrEmpty(Convert.ToString(row["StatusId"])) ? Convert.ToInt32(row["StatusId"]) : 0;
                        model.StatusMsg = Convert.ToString(row["StatusMsg"]);
                        model.CircleId = !string.IsNullOrEmpty(Convert.ToString(row["CircleId"])) ? Convert.ToInt32(row["CircleId"]) : 0;
                        model.UserTxnId = Convert.ToString(row["UserTxnId"]);
                        model.OurRefTxnId = Convert.ToString(row["OurRefTxnId"]);
                        model.ApiTxnId = Convert.ToString(row["ApiTxnId"]);
                        model.OptTxnId = Convert.ToString(row["OptTxnId"]);
                        model.AccountNo = Convert.ToString(row["AccountNo"]);
                        model.AccountOther = Convert.ToString(row["AccountOther"]);
                        model.Optional1 = Convert.ToString(row["Optional1"]);
                        model.Optional2 = Convert.ToString(row["Optional2"]);
                        model.Optional3 = Convert.ToString(row["Optional3"]);
                        model.Optional4 = Convert.ToString(row["Optional4"]);
                        model.LapuId = !string.IsNullOrEmpty(Convert.ToString(row["LapuId"])) ? Convert.ToInt64(row["LapuId"]) : 0;
                        model.LapuNo = Convert.ToString(row["LapuNo"]);
                        model.AmountUnitId = !string.IsNullOrEmpty(Convert.ToString(row["AmountUnitId"])) ? Convert.ToInt32(row["AmountUnitId"]) : 0;
                        model.AmountLength = !string.IsNullOrEmpty(Convert.ToString(row["AmountLength"])) ? Convert.ToInt32(row["AmountLength"]) : 0;
                        model.DateTimeFormat = Convert.ToString(row["DateTimeFormat"]);
                        model.RefPadding = Convert.ToString(row["RefPadding"]);
                        model.RandomKey = Convert.ToString(row["RandomKey"]);
                        model.IsNumericOnly = !string.IsNullOrEmpty(Convert.ToString(row["IsNumericOnly"])) ? Convert.ToInt32(row["IsNumericOnly"]) : 0;
                        model.UrlId = !string.IsNullOrEmpty(Convert.ToString(row["UrlId"])) ? Convert.ToInt32(row["UrlId"]) : 0;
                        model.ApiUrl = Convert.ToString(row["ApiUrl"]);
                        model.Method = Convert.ToString(row["Method"]);
                        model.ContentType = Convert.ToString(row["ContentType"]);
                        model.ResType = Convert.ToString(row["ResType"]);
                        model.PostData = Convert.ToString(row["PostData"]);
                        model.OpCode = Convert.ToString(row["OpCode"]);
                        model.ExtraUrl = Convert.ToString(row["ExtraUrl"]);
                        model.ExtraUrlData = Convert.ToString(row["ExtraUrlData"]);
                        model.ApiUserId = Convert.ToString(row["ApiUserId"]);
                        model.ApiPassword = Convert.ToString(row["ApiPassword"]);
                        model.ApiOptional = Convert.ToString(row["ApiOptional"]);
                        model.ApiTypeId = !string.IsNullOrEmpty(Convert.ToString(row["ApiTypeId"])) ? Convert.ToInt32(row["ApiTypeId"]) : 0;
                        model.LapuPass = Convert.ToString(row["LapuPass"]);
                        model.LapuPIN = Convert.ToString(row["LapuPIN"]);
                        model.LapuOP1 = Convert.ToString(row["LapuOP1"]);
                        model.LapuOP2 = Convert.ToString(row["LapuOP2"]);
                        model.CallbackURL = Convert.ToString(row["CallbackURL"]);
                        model.CircleCode = Convert.ToString(row["CircleCode"]);
                        model.DB_Amt = !string.IsNullOrEmpty(Convert.ToString(row["DB_Amt"])) ? Convert.ToDecimal(row["DB_Amt"]) : 0;
                        model.TxnRemark = Convert.ToString(row["TxnRemark"]);
                        model.ApiName = Convert.ToString(row["ApiName"]);

                        model.RequestTime = Convert.ToString(row["RequestTime"]);
                        model.ResendById = !string.IsNullOrEmpty(Convert.ToString(row["ResendById"])) ? Convert.ToInt32(row["ResendById"]) : 0;
                        model.ResendTime = Convert.ToString(row["ResendTime"]);
                        model.CircleCode = string.IsNullOrEmpty(model.CircleCode) ? "10" : model.CircleCode;

                        model.ResendWaitTime = Convert.ToInt32(row["ResendWaitTime"]);
                        model.WaitTime = Convert.ToInt32(row["WaitTime"]);
                        model.StatusCheckTime = Convert.ToInt32(row["StatusCheckTime"]);
                        model.IsAutoStatusCheck = Convert.ToBoolean(row["IsAutoStatusCheck"]);
                        model.CallbackId = Convert.ToString(row["CallbackId"]);
                        model.ApiBal = Convert.ToDecimal(row["ApiBal"]);
                        model.UserBal = Convert.ToDecimal(row["UserBal"]);
                        model.ResendCount = Convert.ToInt32(row["ResendCount"]);
                        model.IsROChecked = Convert.ToBoolean(row["IsROChecked"]);
                        model.IsValidRO = Convert.ToBoolean(row["IsValidRO"]);
                        model.Comment = Convert.ToString(row["Comment"]);
                        model.ApiComm = Convert.ToDecimal(row["ApiComm"]);
                        model.UserComm = Convert.ToDecimal(row["UserComm"]);
                        model.UserName = Convert.ToString(row["UserName"]);


                    }

                }
            }
            catch (Exception ex)
            {
                // log += "\r\n , excp=" + ex.Message;
                LogException(ex, "GetRechargeDetail RecId=" + recid);
            }

            return model;

        }

        private void UpdateRCStatusWithCallbackNEW(string rId, int statusid)
        {
            string log = "";
            RechargeDetail model = new RechargeDetail();
            //get recharge
            long recid = Convert.ToInt64(rId);

            bool IsRefund = false;
            bool IsDownline = false;
            string spLog = "";
            string remark = "StatusChangeBulk";

            model = GetRechargeDetail(recid, string.Empty);

            UpdateStatusWithCheck(recid, Convert.ToInt16(model.StatusId), model.ApiTxnId, model.OptTxnId, model.StatusMsg, remark, ref IsDownline, ref IsRefund, ref log, 0, string.Empty, 0);

            if (!string.IsNullOrEmpty(model.CallbackURL))
            {
                SendCallBackNEW(model, model.OptTxnId, statusid);
            }
            else
            {
                log += "\r\n, usernotfound";
            }
            log += "\r\n, callback sent to user ";

            // LogActivity(log);
        }

        private void SendCallBackNEW(RechargeDetail model, string optxnid, int statusId)
        {
            string log = "\r\nsend callback start, ";
            if (string.IsNullOrEmpty(model.CallbackURL))
            {
                log += "\r\ncallback not exists, ";
            }

            else
            {

                var CallbackURL = model.CallbackURL.ReplaceURL(string.Empty, string.Empty, model.UserTxnId, model.CustomerNo, optxnid, model.CircleId.ToString(), model.Amount.ToString(), model.TxnId.ToString(), statusId.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, DateTime.Now.ToString("yyyyMMddHHmmss"), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, model.UserId.ToString());

                log += "\r\ncallback url, " + CallbackURL + " recid=" + model.RecId + ", ";

                RequestResponseDto userReqRes = new RequestResponseDto();
                userReqRes.Remark = "downline status";
                userReqRes.UserId = Convert.ToInt32(model.UserId);
                userReqRes.RecId = Convert.ToInt64(model.RecId);
                userReqRes.RefId = model.OurRefTxnId;
                userReqRes.RequestTxt = CallbackURL;
                userReqRes = AddUpdateReqRes(userReqRes, ref log);

                ApiCall apiCall = new ApiCall(reqResService);
                apiCall.Get(CallbackURL, ref userReqRes);
                userReqRes = AddUpdateReqRes(userReqRes, ref log);

                log += "\r\ncallback end, ";
            }

        }
        private void CheckStatusThread(string rId, int uid = 0)
        {
            try
            {
                string resendurl = string.Empty;
                long recid = !string.IsNullOrEmpty(rId) ? Convert.ToInt64(rId) : 0;
                var recharge = GetRechargeDetail(recid, string.Empty);
                if (recharge.ApiId == 12)
                {
                     resendurl = SiteKey.ApiDomainName + "Service/VendorStatusCheckRec?token=VendorStatusCheck&recid=" + rId + "&uid=" + uid;
                }
                else
                {
                     resendurl = SiteKey.ApiDomainName + "Service/VendorStatusCheck?token=VendorStatusCheck&recid=" + rId + "&uid=" + uid;
                }
                    ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                HttpWebRequest httpreq = (HttpWebRequest)WebRequest.Create(resendurl);
                httpreq.GetResponseAsync();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        private void ResendRechargeThread(string rId, int apiid, int isCheck, int userid)
        {
            try
            {
                long recid = !string.IsNullOrEmpty(rId) ? Convert.ToInt64(rId) : 0;
                var recharge = GetDMTDetail(recid, string.Empty);
                string resendurl = SiteKey.ApiDomainName + "Service/ResendRecharge?token=ResendRecharge&recid=" + rId + "&apiid=" + apiid + "&chk=" + isCheck + "&uid=" + userid;

                if (recharge.TxnRemark.Contains("billpay"))
                    resendurl = SiteKey.ApiDomainName + "Service/ResendBillPay?token=ResendRecharge&recid=" + rId + "&apiid=" + apiid + "&chk=" + isCheck + "&uid=" + userid;

                if (recharge.TxnRemark.Contains("PAYOUT"))
                    resendurl = SiteKey.ApiDomainName + "Service/ResendDmt?token=ResendRecharge&recid=" + rId + "&apiid=" + apiid + "&chk=" + isCheck + "&uid=" + userid;

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                HttpWebRequest httpreq = (HttpWebRequest)WebRequest.Create(resendurl);

                httpreq.GetResponseAsync();

            }
            catch (Exception ex)
            {

                LogException(ex);

            }

        }
        private string SendComplaint(RechargeDetail model)
        {
            string apires = string.Empty;
            string log = "\r\nsend callback start, ";
            if (string.IsNullOrEmpty(model.ApiUrl))
            {
                log += "\r\ncallback not exists, ";
            }

            else
            {
                int unitid = 0, len = 0, reflen = 0, IsNumeric = 0;
                string randomkey = "", dtformat = "", refpadding = "", apiamount = "", ourref = "", datetime = "";

                if (IsNumeric == 1)
                    ourref = model.TxnId.ToString();
                else
                    ourref = model.OurRefTxnId;


                var apiUrl = model.ApiUrl.ReplaceURL(model.ApiUserId, model.ApiPassword, model.UserTxnId, model.CustomerNo, model.OptTxnId, model.CircleId.ToString(), model.Amount.ToString(), model.TxnId.ToString(), model.StatusId.ToString(), model.ApiTxnId, model.ExtraUrl, model.ExtraUrlData, model.AccountNo, model.AccountOther, model.Optional1, model.Optional2, model.Optional3, model.Optional4, DateTime.Now.ToString("yyyyMMddHHmmss"), model.RandomKey, model.LapuNo, model.ApiPassword, model.LapuPIN, model.LapuOP1, model.LapuOP2, string.Empty, string.Empty, model.UserId.ToString());
                var postData = model.PostData.ReplaceURL(model.ApiUserId, model.ApiPassword, model.UserTxnId, model.CustomerNo, model.OptTxnId, model.CircleId.ToString(), model.Amount.ToString(), model.TxnId.ToString(), model.StatusId.ToString(), model.ApiTxnId, model.ExtraUrl, model.ExtraUrlData, model.AccountNo, model.AccountOther, model.Optional1, model.Optional2, model.Optional3, model.Optional4, DateTime.Now.ToString("yyyyMMddHHmmss"), model.RandomKey, model.LapuNo, model.ApiPassword, model.LapuPIN, model.LapuOP1, model.LapuOP2, string.Empty, string.Empty, model.UserId.ToString());


                apiUrl = model.ApiUrl.ReplaceURL(model.ApiUserId, model.ApiPassword, model.UserTxnId, model.CustomerNo, model.OptTxnId, model.CircleId.ToString(), model.Amount.ToString(), model.OurRefTxnId.ToString(), model.StatusId.ToString(), model.ApiTxnId, model.ExtraUrl, model.ExtraUrlData, model.AccountNo, model.AccountOther, model.Optional1, model.Optional2, model.Optional3, model.Optional4, DateTime.Now.ToString("yyyyMMddHHmmss"), model.RandomKey, model.LapuNo, model.ApiPassword, model.LapuPIN, model.LapuOP1, model.LapuOP2, string.Empty, string.Empty, model.UserId.ToString());
                postData = postData.ReplaceURL(model.ApiUserId, model.ApiPassword, model.UserTxnId, model.CustomerNo, model.OptTxnId, model.CircleId.ToString(), model.Amount.ToString(), model.OurRefTxnId.ToString(), model.StatusId.ToString(), model.ApiTxnId, model.ExtraUrl, model.ExtraUrlData, model.AccountNo, model.AccountOther, model.Optional1, model.Optional2, model.Optional3, model.Optional4, DateTime.Now.ToString("yyyyMMddHHmmss"), model.RandomKey, model.LapuNo, model.ApiPassword, model.LapuPIN, model.LapuOP1, model.LapuOP2, string.Empty, string.Empty, model.UserId.ToString());


                log += "\r\ncallback url, " + apiUrl + " recid=" + model.RecId + ", ";

                RequestResponseDto userReqRes = new RequestResponseDto();
                userReqRes.Remark = "Complaint";
                userReqRes.UserId = Convert.ToInt32(model.UserId);
                userReqRes.RecId = Convert.ToInt64(model.RecId);
                userReqRes.RefId = model.OurRefTxnId;
                userReqRes.RequestTxt = apiUrl;
                userReqRes = AddUpdateReqRes(userReqRes, ref log);

                ApiCall apiCall = new ApiCall(reqResService);


                apires = model.Method == "POST" ? apiCall.Post(apiUrl, postData, model.ContentType, model.ResType, ref userReqRes, model.ApiId, model.ApiUserId, model.ApiPassword)
                                                                   : apiCall.Get(apiUrl, ref userReqRes, model.ApiId, model.ContentType, model.ResType, model.ApiUserId, model.ApiPassword);

                userReqRes = AddUpdateReqRes(userReqRes, ref log);

                log += "\r\ncallback end, ";
            }

            return apires;

        }
        private void SendComplaintCallBack(long recid, long cmpid, int statusid, string remark, string comment)
        {
            string log = "";
            var model = GetRechargeDetail(recid, "complaint");

            RequestResponseDto reqResp = new RequestResponseDto();
            ApiCall apiCall = new ApiCall(reqResService);

            // log += "\r\n(" + DateTime.Now.TimeOfDay.ToString() + ")apicall, ";

            var url = model.CallbackURL?.ReplaceURL(string.Empty, string.Empty, model.UserTxnId, model.CustomerNo, model.OptTxnId, string.Empty, model.Amount.ToString(), model.OurRefTxnId, statusid.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, remark, comment, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, model.StatusId.ToString(), string.Empty, string.Empty, cmpid.ToString(), string.Empty, model.UserId.ToString());
            url = url?.ReplaceURL(string.Empty, string.Empty, model.UserTxnId, model.CustomerNo, model.OptTxnId, string.Empty, model.Amount.ToString(), model.OurRefTxnId, statusid.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, remark, comment, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, model.StatusId.ToString(), string.Empty, string.Empty, cmpid.ToString(), string.Empty, model.UserId.ToString());

            string cbres = string.Empty;

            cbres = apiCall.Get(url, ref reqResp, model.ApiId);

            reqResp.RecId = Convert.ToInt64(model.RecId);
            reqResp.RefId = model.OurRefTxnId;
            reqResp.Remark = "Complain CallBack Downline";
            reqResp = AddUpdateReqRes(reqResp, ref log);

        }

        private JObject GETandSETcomplaintByRef
           (string cmpid,
            string reqid,
            string txnid,
            string vtxid,
            int statusId,
            string remark,
            ref long ccid,
            ref int stausId,
            ref string cmpRemark,
            ref string cmpComment,
            ref long rcid,
            ref string log
           )
        {
            JObject response = new JObject();

            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    //set sp parameters
                    SqlCommand cmd = new SqlCommand("usp_UpdateComplaintByRef", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (!string.IsNullOrEmpty(cmpid))
                        cmd.Parameters.AddWithValue("@cmpid", cmpid);
                    if (!string.IsNullOrEmpty(reqid))
                        cmd.Parameters.AddWithValue("@reqid", reqid);
                    if (!string.IsNullOrEmpty(txnid))
                        cmd.Parameters.AddWithValue("@txnid", txnid);
                    if (!string.IsNullOrEmpty(vtxid))
                        cmd.Parameters.AddWithValue("@vtxid", vtxid);
                    if (statusId > 0)
                        cmd.Parameters.AddWithValue("@statusid", statusId);
                    if (!string.IsNullOrEmpty(remark))
                        cmd.Parameters.AddWithValue("@remark", remark);

                    cmd.Parameters.Add("@complaintId", SqlDbType.BigInt).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@cmpStatusId", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@cmpRemark", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@cmpComment", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@RecId", SqlDbType.BigInt).Direction = ParameterDirection.Output;

                    cmd.Parameters.Add("@ErrorCode", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@splog", SqlDbType.VarChar, 200).Direction = ParameterDirection.Output;

                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ")before exec  usp_UpdateComplaintByRef";
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    log += "\r\n , (" + DateTime.Now.TimeOfDay.ToString() + ") after exec  usp_UpdateComplaintByRef";

                    string error = Convert.ToString(cmd.Parameters["@ErrorCode"].Value);
                    string ErrorDesc = Convert.ToString(cmd.Parameters["@ErrorDesc"].Value);
                    string splog = Convert.ToString(cmd.Parameters["@splog"].Value);

                    log += "\r\n ,  error=" + error + ", ErrorDesc=" + ErrorDesc + ", splog=" + splog;

                    if (error != ErrorCode.NO_ERROR)
                    {
                        response = JObject.FromObject(new
                        {
                            STATUS = "3",
                            MESSAGE = ErrorDesc,
                            ERRORCODE = error
                        });
                    }
                    else
                    {
                        string complaintId = Convert.ToString(cmd.Parameters["@complaintId"].Value);
                        string cmpStatusId = Convert.ToString(cmd.Parameters["@cmpStatusId"].Value);
                        cmpRemark = Convert.ToString(cmd.Parameters["@cmpRemark"].Value);
                        cmpComment = Convert.ToString(cmd.Parameters["@cmpComment"].Value);
                        string RecId = Convert.ToString(cmd.Parameters["@RecId"].Value);

                        log += "ComplaintId=" + complaintId + ", cmpStatusId =" + cmpStatusId + ", RecId=" + RecId;
                        rcid = !string.IsNullOrEmpty(RecId) ? Convert.ToInt64(RecId) : 0;
                        ccid = !string.IsNullOrEmpty(complaintId) ? Convert.ToInt64(complaintId) : 0;

                        if ((cmpStatusId == "2" || cmpStatusId == "5") && statusId == 1)
                        {
                            SendComplaintCallBack(rcid, ccid, statusId, cmpRemark, cmpComment);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LogException(ex, "complaint update");
            }

            return response;
        }

        [HttpGet]
        public ActionResult Recharge2(int? u2, int? u, int? v, int? o, int? s, int? c, string f = "", string m = "", string e = "", int? i = 0, string rto = "", string ut = "", string ot = "", string vt = "", FilterData fdata = null)
        {
            TempData["RechargeFilterDto"] = null;

            var str = "userrole=" + CurrentUser.RoleId +
                      ", CurrentUser.UserID=" + CurrentUser.UserID +
                      ", u=" + u +
                      ", v=" + v +
                      ", o=" + o +
                      ", s=" + s +
                      ", rto=" + rto +
                      ", f=" + f +
                      ",e=" + e +
                      ", c=" + c +
                      ", m=" + m +
                      ", ut=" + ut +
                      ", vt=" + vt +
                      ", ot=" + ot +
                      ", u2=" + u2;

            LogActivity(str);

            UpdateActivity("RechargeReport REQUEST", "GET:RechargeReport/Recharge2", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("RechargeReport2", CurrentUser.RoleId);

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
            filter.Edate = e;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyy");
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("dd/MM/yyy");
            filter.CustomerNo = m;
            filter.Circleid = Convert.ToInt32(c.HasValue ? c : 0);
            filter.UserReqid = ut;
            filter.OpTxnid = ot;
            filter.ApiTxnid = vt;

            ViewBag.FilterData = TempData["RechargeFilterDto"] = filter;

            int uid = CurrentUser.RoleId != 3 ? filter.Uid : CurrentUser.UserID;

            ViewBag.CircleList = operatorSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == filter.Circleid) }).ToList();
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == filter.Opid) }).ToList();
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == filter.Uid) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == filter.Apiid) }).ToList();
            ViewBag.UserList2 = userService.GetUserList().Where(x => x.RoleId != 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == filter.UpdatedById) }).ToList();


            var statusList = rechargeReportService.GetStatusList().Where(r => r.Remark.Contains("Recharge") && r.Id != 5).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = ((x.Id == filter.Sid)) }).ToList();
            if (CurrentUser.RoleId != 3)
                statusList.Insert(statusList.Count, new SelectListItem() { Value = "11", Text = "Manual Updated" });
            ViewBag.StatusList = statusList;
            return View();
        }
        [HttpPost]
        public ActionResult GetRechargeReport2(DataTableServerSide model)
        {
            string log = string.Empty;
            ViewBag.actionAllowed = action = ActionAllowed("RechargeReport2", CurrentUser.RoleId);
            var action2 = ActionAllowed("RechargeReport2", CurrentUser.RoleId);

            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;

            int userrole = CurrentUser.RoleId;
            bool IsAdminRole = (userrole != 3) ? true : false;
            model.filterdata.UserId = IsAdminRole ? model.filterdata.UserId : CurrentUser.UserID;

            KeyValuePair<int, List<Recharge>> recharges = new KeyValuePair<int, List<Recharge>>();
            try
            {
                recharges = rechargeReportService.GetRechargeReport(model, flt.Uid, flt.Apiid, flt.Opid, Convert.ToByte(flt.Sid), flt.Searchid, flt.Sdate, flt.Edate, flt.Circleid, flt.CustomerNo, flt.UserReqid, flt.ApiTxnid, flt.OpTxnid, flt.UpdatedById, ref log);
            }
            catch (Exception ex)
            {

                LogException(ex, "recharge report 2");
                try
                {
                    recharges = rechargeReportService.GetRechargeReport(model, flt.Uid, flt.Apiid, flt.Opid, Convert.ToByte(flt.Sid), flt.Searchid, flt.Sdate, flt.Edate, flt.Circleid, flt.CustomerNo, flt.UserReqid, flt.ApiTxnid, flt.OpTxnid, flt.UpdatedById, ref log);
                }
                catch (Exception ex2)
                {
                    LogException(ex, "ex2-recharge report 2");
                }
            }

            return Json(new
            {
                draw = model.draw,
                recordsTotal = recharges.Key,
                recordsFiltered = recharges.Key,
                data = recharges.Value.Select(c => new List<object> {
                     c.Id,
                     c.User?.UserProfile?.FullName??string.Empty,
                    (c.RequestTime)?.ToString()??string.Empty,
                     c.CustomerNo+
                    (!string.IsNullOrEmpty(c.AccountNo)? ",<br />" +c.AccountNo:string.Empty)+
                    (!string.IsNullOrEmpty(c.AccountOther)? ",<br />" +c.AccountOther:string.Empty),
                     c.Operator.Name,
                     ("<b style='color:"+setColor(c.StatusId)+"'>"+c.StatusType.TypeName+"</b>"),
                     c.Amount,
                     c.UserComm,
                     c.TxnLedger?.OP_Bal??0,
                     c.TxnLedger?.DB_Amt??0,
                     c.TxnLedger?.CL_Bal??0,
                     (!IsAdminRole ? c.ApiId.ToString(): c.ApiSource?.ApiName??string.Empty),
                     (!IsAdminRole ? 0 : c.ApiComm??0),
                    (c.OpId==100 || c.OpId==101 || c.OpId==102)?c.OptTxnId: c.beneficiaryName??c.OptTxnId,//c.beneficiaryName??c.OptTxnId,                    
                     c.UserTxnId
                    })
            }, JsonRequestBehavior.AllowGet);
        }
        public string GetRequest(string url)
        {
            var response = "";
            try
            {
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                HttpWebRequest httpreq = (HttpWebRequest)WebRequest.Create(url);


                httpreq.ContentType = "application/json";
                using (HttpWebResponse httpres = (HttpWebResponse)httpreq.GetResponse())
                {
                    StreamReader sr = new StreamReader(httpres.GetResponseStream());
                    response = sr.ReadToEnd();
                    sr.Close();
                }
            }
            catch (Exception ex)
            {

            }


            return response;

        }





    }
}