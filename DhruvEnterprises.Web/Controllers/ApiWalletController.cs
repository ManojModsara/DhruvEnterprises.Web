using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.Code.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Controllers
{


    public class ApiWalletController : BaseController
    {
        #region "Properties"
        public ActionAllowedDto action;
        private readonly IUserService userService;
        private readonly IApiWalletService apiWalletService;
        private readonly IApiService apiService;
        private readonly IBankAccountService bankAccountService;
        private readonly IRechargeReportService rechargeReportService;
        ActivityLogDto activityLogModel;

        // private IApiService apiService;
        #endregion

        #region "Constructor" 
        public ApiWalletController
            (
             IUserService _userService,
             IApiWalletService _apiWalletService,
             IApiService _apiService,
             IActivityLogService _activityLogService,
             IRoleService _roleService,
             IBankAccountService _bankAccountService,
            IRechargeReportService _rechargeReportService
            ) : base(_activityLogService, _roleService)
        {
            this.userService = _userService;
            this.apiWalletService = _apiWalletService;
            this.apiService = _apiService;
            this.bankAccountService = _bankAccountService;
            this.rechargeReportService = _rechargeReportService;
            this.action = new ActionAllowedDto();
            this.activityLogModel = new ActivityLogDto();
        }
        #endregion

        #region "Methods"
        public ActionResult Index(int? y, long? ri, long? ti, int? tt, int? at, int? vi, int? ui, string fd = "", string ed = "", string rm = "")
        {
            UpdateActivity("ApiWalletTxn ", "Get:ApiWallet/ApiWalletTxn");
            ViewBag.actionAllowed = action = ActionAllowed("ApiWallet", CurrentUser.RoleId);


            #region "filter"
            TxnFilterDto filter = new TxnFilterDto();
            filter.isshow = Convert.ToInt32(y.HasValue ? 1 : 0);
            filter.recid = Convert.ToInt64(ri.HasValue ? ri : 0);
            filter.txnid = Convert.ToInt64(ti.HasValue ? ti : 0);
            filter.txntypeid = Convert.ToInt32(tt.HasValue ? tt : 0);
            filter.amttypeid = Convert.ToInt32(at.HasValue ? at : 0);
            filter.apiid = Convert.ToInt32(vi.HasValue ? vi : 0);
            filter.userid = Convert.ToInt32(ui.HasValue ? ui : 0);
            filter.remark = rm != "" ? rm : "";

            if (fd != "") { TempData["sDate"] = filter.sdate = fd; } else { filter.sdate = DateTime.Now.AddDays(-3).ToString("dd/MM/yyy"); }
            if (ed != "") { TempData["eDate"] = filter.edate = ed; } else { filter.edate = DateTime.Now.ToString("dd/MM/yyy"); }

            ViewBag.FilterData = TempData["TxnFilterDto"] = filter;

            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? filter.userid : CurrentUser.UserID;


            ViewBag.UserList = userService.GetUserList().Where(x => uid == 0 ? true : x.Id == uid && x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? string.Empty, Selected = (x.Id == ui) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Where(x => uid == 0 ? true : x.Id == uid).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == vi) }).ToList();
            ViewBag.TxnTypeList = rechargeReportService.GetTxnTypes().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = (x.Id == tt) }).ToList();
            ViewBag.AmtTypeList = rechargeReportService.GetAmtTypes("Api").Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.AmtTypeName, Selected = (x.Id == at) }).ToList();
            #endregion
            return View();
        }

        [HttpPost]
        public ActionResult GetApiWalletTxns(DataTableServerSide model)
        {

            UpdateActivity("ApiWalletTxn ", "Post:ApiWallet/ApiWalletTxn");
            ViewBag.actionAllowed = action = ActionAllowed("ApiWallet", CurrentUser.RoleId);


            #region "filter"

            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? 0 : CurrentUser.UserID;

            TxnFilterDto filter = TempData["TxnFilterDto"] != null ? (TxnFilterDto)TempData["TxnFilterDto"] : new TxnFilterDto();

            filter.userid = IsAdminRole ? filter.userid : uid;
            ViewBag.FilterData = TempData["TxnFilterDto"] = filter;

            filter.sdate = TempData["sDate"] != null ? filter.sdate : "";
            filter.edate = TempData["eDate"] != null ? filter.edate : "";

            #endregion

            KeyValuePair<int, List<ApiWalletTxn>> users = apiWalletService.GetApiWalletTxns(model, filter.recid, filter.txnid, filter.txntypeid, filter.amttypeid, filter.apiid, filter.userid, filter.sdate, filter.edate, filter.remark);

            return Json(new
            {
                draw = model.draw,
                recordsTotal = users.Key,
                recordsFiltered = users.Key,
                data = users.Value.Select(c => new List<object> {
                    c.Id,
                    DataTableButton.HyperLink(Url.Action( "apidetail", "requestresponse",new { id = c.ApiId}),"modal-view-api-detail",c?.ApiSource?.ApiName?.ToString()??"","View Api Detail"),
                    (c.TxnDate).ToString(),
                     c.RecId,
                     c?.RefTxnId??0,
                    c.OP_Bal,
                    c.CR_Amt,
                     c.Ins_Amt,
                    c.DB_Amt,
                    c.CL_Bal,
                    c?.TxnType?.TypeName??"",
                    c?.AmtType?.AmtTypeName??"",
                    c.Remark,
                     c.ApiClBal,
                    c.ClBalDiff,
                     DataTableButton.HyperLink(Url.Action( "userdetail", "requestresponse",new { id = c.AddedById}),"modal-view-user-detail",c.User?.UserProfile?.FullName?.ToString(),"View User Detail")
                })
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult AddEdit(int? id)
        {
            int apiid = id ?? 0;

            UpdateActivity("Add/Edit ApiWallet ", "Get:ApiWallet/AddEdit");
            ViewBag.actionAllowed = action = ActionAllowed("ApiWallet", CurrentUser.RoleId, 2);

            ApiWalletDto model = new ApiWalletDto();
            model.BankAccountList = bankAccountService.GetBankAccounts(true).Where(b => b.Id != 2).Select(x => new BankAccountDto() { Id = x.Id, HolderName = x.HolderName }).ToList();
            model.TrTypeList = bankAccountService.GetTransferTypeList().Where(t => !t.Name.Contains("CREDIT")).Select(x => new TransferTypeDto() { Id = x.Id, Name = x.Name }).ToList();
            ViewBag.APIList = apiService.GetApiList();
            model.ApiId = apiid;
            model.CurrentBalance = apiService.GetApiSource(apiid)?.ApiBal ?? 0;

            model.IsDirect = apiid > 0 ? true : false;
            model.BankAccountId = 1;
            model.TrTypeId = 3;

            string dtime = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            model.ChequeNo = string.IsNullOrWhiteSpace(model.ChequeNo) ? dtime : model.ChequeNo;
            model.Remark = string.IsNullOrWhiteSpace(model.Remark) ? dtime : model.Remark;


            return PartialView("_addedit", model);

        }

        [HttpPost]
        public ActionResult AddEdit(ApiWalletDto model, FormCollection FC)
        {
            UpdateActivity("Add/Edit ApiWallet ", "Get:ApiWallet/AddEdit");
            ViewBag.actionAllowed = action = ActionAllowed("ApiWallet", CurrentUser.RoleId, 2);

            model.Remark = !string.IsNullOrEmpty(model.Remark) ? model.Remark : "NA";
            string error = "0";
            try
            {
                string dtime = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                model.ChequeNo = string.IsNullOrWhiteSpace(model.ChequeNo) ? dtime : model.ChequeNo;
                model.Remark = string.IsNullOrWhiteSpace(model.Remark) ? dtime : model.Remark;

                var totalAmt = (model.SentAmount + model.ReceivedAmount + model.IncentiveAmount);

                if (model.ApiId <= 0)
                {
                    ShowErrorMessage("Error!", "Invalid Vendor", false);
                    model.IsDirect = false;
                }
                else if (model.TrTypeId <= 0)
                {
                    model.IsDirect = false;
                    ShowErrorMessage("Error!", "Invalid Transfer Type", false);
                }
                else if (model.BankAccountId <= 0)
                {
                    model.IsDirect = false;
                    ShowErrorMessage("Error!", "Invalid Bank Account", false);
                }
                else if (totalAmt <= 0 || model.ReceivedAmount > 10000000 || model.SentAmount > 10000000 || model.IncentiveAmount > 10000000)
                {
                    model.IsDirect = false;
                    ShowErrorMessage("Error!", "Invalid Amout", false);
                }
                else if (string.IsNullOrWhiteSpace(model.ChequeNo))
                {
                    model.IsDirect = false;
                    ShowErrorMessage("Error!", "Invalid ChequeNo/RefNo", false);
                }
                else
                {
                    using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
                    {

                        SqlCommand cmd = new SqlCommand("sp_AddApiWallet", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ApiId", model.ApiId);
                        cmd.Parameters.AddWithValue("@Ins_Amt", model.IncentiveAmount);
                        cmd.Parameters.AddWithValue("@AmountSent", model.SentAmount);
                        cmd.Parameters.AddWithValue("@AmountReceived", model.ReceivedAmount);
                        cmd.Parameters.AddWithValue("@Remark", model.Remark);
                        cmd.Parameters.AddWithValue("@AddedById", CurrentUser.UserID);
                        cmd.Parameters.AddWithValue("@BankAccountId", model.BankAccountId);
                        cmd.Parameters.AddWithValue("@TrTypeId", model.TrTypeId);
                        cmd.Parameters.AddWithValue("@ChequeNo", model.ChequeNo);
                        cmd.Parameters.AddWithValue("@IsPullOut", model.IsPullOut ? "Yes" : "No");
                        cmd.Parameters.Add("@error", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();

                        error = Convert.ToString(cmd.Parameters["@error"].Value);

                    }
                    if (error == "0")
                    {
                        model.IsDirect = model.IsDirect ? true : false;
                        ShowSuccessMessage("Success!", "Wallet has been updated", false);
                    }

                    else if (error == "2")
                    {
                        model.IsDirect = false;
                        ShowErrorMessage("Error!", "Duplicate Cheque/Ref number!", false);
                    }

                    else
                    {
                        model.IsDirect = false;
                        ShowErrorMessage("OOPS!", "Something Went Wrong!", false);
                    }

                }



            }
            catch (Exception ex)
            {
                model.IsDirect = false;
                string message = "An internal error found during to process your requested data!";
                ShowErrorMessage("Error -Vendor Wallet!", message, false);
                LogException(ex);
            }

            var controllername = model.IsDirect ? "ApiSource" : "ApiWallet";


            return RedirectToAction("Index", controllername);
        }

       

        private long UpdateActivity(string title, string action, string remark = "", long id = 0)
        {
            try
            {
                activityLogModel.Id = id;
                activityLogModel.ActivityName = title;
                activityLogModel.ActivityPage = action;
                activityLogModel.Remark = remark;
                activityLogModel.UserId = CurrentUser?.UserID ?? 0;
                activityLogModel = LogActivity(activityLogModel);
            }
            catch (Exception ex)
            {

                LogException(ex);
            }
            return activityLogModel.Id;
        }
        #endregion
    }
}