using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web;
using DhruvEnterprises.Web.Code.Attributes;
using DhruvEnterprises.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Controllers
{
    public class BankAccountController : BaseController
    {
        public ActionAllowedDto action;
        private readonly IBankAccountService bankAccountService;
        private readonly IRechargeReportService rechargeReportService;
        private readonly IUserService userService;
        private readonly IApiService apiService;
        private
        ActivityLogDto activityLogModel;
        public BankAccountController
            (IUserService _userService, 
                                     IApiService _apiService, 
                                     IActivityLogService _activityLogService, 
                                     IRoleService _roleService, 
                                     IBankAccountService _bankAccountService, 
                                     IRechargeReportService _rechargeReportService
            ) : base(_activityLogService, _roleService)
        {
            this.bankAccountService = _bankAccountService;
            this.rechargeReportService = _rechargeReportService;
            this.userService = _userService;
            this.apiService = _apiService;
            this.action = new ActionAllowedDto();
            this.activityLogModel = new ActivityLogDto();
        }

        // GET: BankAccount
        public ActionResult Index(int? i, int? v, int? u, string a = "", string h = "", string m = "", string r = "", string b = "")
        {
            UpdateActivity("BankAccounts REQUEST", "GET:BankAccount/Index", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("BankAccount", CurrentUser.RoleId);

            BankAccountFilterDto filter = new BankAccountFilterDto();

            filter.UserId = Convert.ToInt32(u.HasValue ? u : 0);
            filter.IsActive = Convert.ToInt32(i.HasValue ? i : 0);
            filter.ApiId = Convert.ToInt32(v.HasValue ? v : 0);
            filter.AccountNo = a;
            filter.HolderName = h;
            filter.UpiAddress = m;
            filter.Remark = r;
            filter.BankName = b;

            ViewBag.FilterData = TempData["BankAccountFilterDto"] = filter;

            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? 0 : CurrentUser.UserID;
            ViewBag.UserList = userService.GetUserList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Username, Selected = x.Id == u }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = x.Id == v }).ToList();
            ViewBag.AccountTypeList = bankAccountService.GetAccountTypeList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id == u }).ToList();

            return View();
        }

        [HttpPost]
        public ActionResult GetBankAccounts(DataTableServerSide model)
        {

            ViewBag.actionAllowed = action = ActionAllowed("BankAccount", CurrentUser.RoleId);


            BankAccountFilterDto filter = TempData["BankAccountFilterDto"] != null ? (BankAccountFilterDto)TempData["BankAccountFilterDto"] : new BankAccountFilterDto();
            ViewBag.FilterData = TempData["BankAccountFilterDto"] = filter;

            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            filter.UserId = IsAdminRole ? filter.UserId : CurrentUser.UserID;

            KeyValuePair<int, List<BankAccount>> bankAccount = bankAccountService.GetBankAccounts(model, filter);

            return Json(new
            {
                draw = model.draw,
                recordsTotal = bankAccount.Key,
                recordsFiltered = bankAccount.Key,
                data = bankAccount.Value.Select(c => new List<object> {
                    c.Id,
                    c.BankName,
                    c.BranchName,
                    c.BranchAddress,
                    c.AccountNo,
                    c.IFSCCode,
                    c.HolderName,
                    c.UpiAdress,
                    c.User2?.UserProfile?.FullName??string.Empty,
                    c.ApiSource?.ApiName??string.Empty,
                    c.Remark,
                   c.BankStatements?.LastOrDefault()?.CL_Bal??0, //getbank balc
                   0, //min limit 
                   (action.AllowEdit && c.Id!=2?  DataTableButton.EditButton(Url.Action( "addeditbank", "BankAccount",new { id = c.Id })):string.Empty )
                    +"&nbsp;"+
                   (action.AllowDelete?  DataTableButton.ListButton(Url.Action( "BankStatement","BankAccount", new { a = c.Id }),"Bank Statement"):string.Empty)
                   , action.AllowEdit?true:false
                })
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: BankAccount
        public ActionResult BankStatement(int? i, int? v, int? u, int? a, long? s, int? ra, int? tr, int? tx, int? am, string p = "" , string r = "", string f = "", string e = "", string cq = "")
        {
            UpdateActivity("BankStatement", "GET:BankAccount/BankStatement", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("BankStatement", CurrentUser.RoleId);

            BankStatementFilterDto filter = new BankStatementFilterDto();

            filter.UserId = Convert.ToInt32(u.HasValue ? u : 0);
            filter.IsActive = Convert.ToInt32(i.HasValue ? i : 0);
            filter.ApiId = Convert.ToInt32(v.HasValue ? v : 0);
            filter.AccountId = Convert.ToInt32(a.HasValue ? a : 0);
            filter.RefAccountId = Convert.ToInt32(ra.HasValue ? ra : 0);
            filter.TrTypeId = Convert.ToInt32(tr.HasValue ? tr : 0);
            filter.TxnTypeId = Convert.ToInt32(tx.HasValue ? tx : 0);
            filter.AmtTypeId = Convert.ToInt32(am.HasValue ? am : 0);
            filter.StatementId = Convert.ToInt32(s.HasValue ? s : 0);

            filter.Sdate = f;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.ToString("dd/MM/yyy");
            filter.Edate = e;
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate  : DateTime.Now.ToString("dd/MM/yyy");
            filter.PaymentRef = p;
            filter.Remark = r;
           

            ViewBag.FilterData = TempData["BankStatementFilterDto"] = filter;

            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? 0 : CurrentUser.UserID;
            
            ViewBag.UserList = userService.GetUserList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Username, Selected = x.Id == u }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = x.Id == v }).ToList();
            ViewBag.AccountTypeList = bankAccountService.GetAccountTypeList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = x.Id == u }).ToList();
            ViewBag.TrTypeList = bankAccountService.GetTransferTypeList().Select(x => new SelectListItem()  { Value = x.Id.ToString(),  Text = x.Name  }).ToList();
            ViewBag.AdminAccountList = bankAccountService.GetBankAccounts(true).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.BankName + "-" + x.HolderName, Selected=x.Id==a }).ToList();
            ViewBag.BankAccountList = bankAccountService.GetBankAccounts().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.BankName + "-" + x.HolderName, Selected  =x.Id==ra }).ToList();
            ViewBag.TxnTypeList = rechargeReportService.GetTxnTypes().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = (x.Id == tx) }).ToList();
            ViewBag.AmtTypeList = rechargeReportService.GetAmtTypes().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.AmtTypeName, Selected = (x.Id == am) }).ToList();
            
            return View();
            
        }

        [HttpPost]
        public ActionResult GetBankStatement(DataTableServerSide model)
        {
            
            ViewBag.actionAllowed = action = ActionAllowed("BankStatement", CurrentUser.RoleId);

           
            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            model.filterdata.UserId = IsAdminRole ? (model?.filterdata?.UserId??0) : CurrentUser.UserID;


            KeyValuePair<int, List<BankStatement>> bankAccount = bankAccountService.GetBankStatements(model);

            return Json(new
            {
                draw = model.draw,
                recordsTotal = bankAccount.Key,
                recordsFiltered = bankAccount.Key,
                data = bankAccount.Value.Select(c => new List<object> {
                    c.Id,
                    c.BankAccount?.BankName??string.Empty,
                    (c.AddedDate)?.ToString(),
                    (c.AddedDate)?.ToString(),// c.PaymentDate,
                    c.OP_Bal,
                    c.CR_Amt,
                    c.DB_Amt,
                    c.CL_Bal,
                     c.TransferType?.Name??string.Empty,
                    c.TxnType?.TypeName??string.Empty,
                    c.AmtType?.AmtTypeName??string.Empty,
                    c.PaymentRef,
                    c.Remark,
                    c.Comment,
                    c.BankAccount1?.BankName??string.Empty,
                    c.User2?.UserProfile?.FullName??string.Empty,
                    c.TxnId,
                    c.ApiSource?.ApiName??string.Empty,
                    c.ApiTxnId,
                    c.User?.UserProfile?.FullName??string.Empty
                })
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult AddEditBank(int? id)
        {
            UpdateActivity("AddEditBank Request", "GET:BankAccount/AddEditBank", "accountid="+id);
            ViewBag.actionAllowed = action = ActionAllowed("BankAccount", CurrentUser.RoleId,id.HasValue?3:2);

            BankAccountDto model = new BankAccountDto();

            if (id.HasValue)
            {
                var bankaccount = bankAccountService.GetBankAccountById(id.Value);
                BindBankAccountToModel(model, bankaccount);
            }

            ViewBag.UserList = userService.GetUserList();
            ViewBag.ApiList = apiService.GetApiList();
            ViewBag.AccountTypeList = bankAccountService.GetAccountTypeList();
            return View(model);
        }

        [HttpPost]
        public ActionResult AddEditBank(BankAccountDto model)
        {
            UpdateActivity("AddEditBank", "POST:BankAccount/AddEditBank", "accountid=" + model.Id);
            ViewBag.actionAllowed = action = ActionAllowed("BankAccount", CurrentUser.RoleId, model.Id>0 ? 3 : 2);

            try
            {

                //if (string.IsNullOrEmpty(model.BankName))
                //{
                //    ShowErrorMessage("Error!", "BankName Required", false);
                //}
                //else if (string.IsNullOrEmpty(model.AccountNo))
                //{
                //    ShowErrorMessage("Error!", "AccountNo Required", false);
                //}
                //else if (string.IsNullOrEmpty(model.HolderName))
                //{
                //    ShowErrorMessage("Error!", "HolderName Required", false);
                //}
                //else if (string.IsNullOrEmpty(model.IFSCCode))
                //{
                //    ShowErrorMessage("Error!","IFSCCode Required", false);
                //}
                //else if (string.IsNullOrEmpty(model.BranchName))
                //{
                //    ShowErrorMessage("Error!", "BranchName Required", false);
                //}
                //else if (string.IsNullOrEmpty(model.BranchAddress))
                //{
                //    ShowErrorMessage("Error!","BranchAddress Required", false);
                //}
                if (!ModelState.IsValid)
                {
                    ShowErrorMessage("Error!", "Check Required Fields", false);
                }
                else if(model.Id==0 && bankAccountService.CheckBankAccount(model.BankName, model.AccountNo, model.HolderName, model.UpiAdress)!=null)
                {
                    ShowErrorMessage("Error!", "Duplicate Information", false);
                }
                else if (model.Id == 2)
                {
                    ShowErrorMessage("Error!", "Cann't update default credit bank", false);
                }
                else
                {
                    var bankaccount = model.Id > 0 ? bankAccountService.GetBankAccountById(model.Id) : new BankAccount();
                    BindModelToBankAccount(bankaccount, model);
                    bankAccountService.Save(bankaccount);

                    ShowSuccessMessage("Success!", "Account has been saved", false);
                }
                
            }
            catch (Exception ex)
            {
                LogException(ex);
            }

            return NewtonSoftJsonResult(new RequestOutcome<string> { IsSuccess = true, RedirectUrl = Url.Action("Index") });

        }

        public ActionResult AddPullAccount()
        {
            UpdateActivity("AddPullAccount Request", "GET:BankAccount/AddPullAccount", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("AddPullAccount", CurrentUser.RoleId, 2);


            BankStatementDto model = new BankStatementDto();

            ViewBag.UserList = userService.GetUserList();
            ViewBag.ApiList = apiService.GetApiList();
            ViewBag.AccountTypeList = bankAccountService.GetAccountTypeList();
            ViewBag.TrTypeList = bankAccountService.GetTransferTypeList().Select(x => new TransferTypeDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
            ViewBag.AdminAccountList = bankAccountService.GetBankAccounts(true).Select(x => new BankAccountDto() { Id = x.Id, HolderName = x.BankName + "-" + x.HolderName }).ToList();
            ViewBag.BankAccountList = bankAccountService.GetBankAccounts().Select(x => new BankAccountDto() { Id = x.Id, HolderName = x.BankName + "-" + x.HolderName }).ToList();

            return View(model);
        }

        [HttpPost]
        public ActionResult AddPullAccount(BankStatementDto model)
        {
            UpdateActivity("AddPullAccount Request", "POST:BankAccount/AddPullAccount", string.Empty);
            ViewBag.actionAllowed = action = ActionAllowed("AddPullAccount", CurrentUser.RoleId, 2);
            string error = "";
            string errordesc = "";
            string message = "";
            string log = "AddPullAccount start";
            try
            {
                DateTime paydate = DateTime.ParseExact(!string.IsNullOrEmpty(model.PaymentDate) ? model.PaymentDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("sp_AddPullAccount", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IsWithdraw", model.IsWithdraw ? "1" : "0");
                    cmd.Parameters.AddWithValue("@AccountId", model.AccountId);
                    cmd.Parameters.AddWithValue("@Amount", model.Amount);
                    cmd.Parameters.AddWithValue("@TrTypeId", model.TrTypeId);
                    cmd.Parameters.AddWithValue("@RefAccountId", model.RefAccountId);
                    cmd.Parameters.AddWithValue("@PaymentRef", model.PaymentRef);
                    cmd.Parameters.AddWithValue("@Remark", model.Remark);
                    cmd.Parameters.AddWithValue("@AddedById", CurrentUser.UserID);
                    cmd.Parameters.AddWithValue("@UserId", model.UserId);
                    cmd.Parameters.AddWithValue("@ApiId", model.ApiId);
                    cmd.Parameters.Add("@Error", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ErrorDesc", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Log", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    error = Convert.ToString(cmd.Parameters["@Error"].Value);
                    errordesc = Convert.ToString(cmd.Parameters["@ErrorDesc"].Value);
                    var splog = cmd.Parameters["@Log"].Value;
                    log += "\r\n, splog=" + splog;
                }
                if (error == "0")
                    ShowSuccessMessage("Success!", "Amount has been saved", false);
                else
                    ShowErrorMessage("Error!", errordesc, false);
            }
            catch (Exception Ex)
            {
                message = "Internal Server Error!";
                ShowErrorMessage("Error!", message, false);
                log += "\r\n, excp=" + Ex.Message;
                LogException(Ex);
            }

            LogActivity(log);
            return NewtonSoftJsonResult(new RequestOutcome<string> { IsSuccess = true, RedirectUrl = Url.Action("BankStatement") });

        }

       

        [HttpPost]
        
        private BankAccount BindModelToBankAccount(BankAccount bankaccount, BankAccountDto model)
        {
            bankaccount.Id = model.Id;
            bankaccount.BankName = model.BankName;
            bankaccount.AccountNo = model.AccountNo;
            bankaccount.HolderName = model.HolderName;
            bankaccount.IFSCCode = model.IFSCCode;
            bankaccount.UpiAdress = model.UpiAdress;
            bankaccount.BranchName = model.BranchName;
            bankaccount.BranchAddress = model.BranchAddress;
            bankaccount.BlockAmount = model.BlockAmount;
            bankaccount.AccountTypeId = model.AccountTypeId;
            bankaccount.UserId = model.UserId;
            bankaccount.ApiId = model.ApiId;
            bankaccount.Remark = model.Remark;
            if (model.Id ==0)
                bankaccount.AddedById = CurrentUser.UserID;
            else
                bankaccount.UpdatedById = CurrentUser.UserID;

            return bankaccount;
        }

        private BankAccountDto BindBankAccountToModel(BankAccountDto model, BankAccount bankaccount)
        {
            model.Id = bankaccount.Id;
            model.BankName = bankaccount.BankName;
            model.AccountNo = bankaccount.AccountNo;
            model.HolderName = bankaccount.HolderName;
            model.IFSCCode = bankaccount.IFSCCode;
            model.UpiAdress = bankaccount.UpiAdress;
            model.BranchName = bankaccount.BranchName;
            model.BranchAddress = bankaccount.BranchAddress;
            model.BlockAmount = bankaccount.BlockAmount;
            model.AccountTypeId = bankaccount.AccountTypeId ?? 0;
            model.UserId = bankaccount.UserId;
            model.ApiId = bankaccount.ApiId;
            model.Remark = bankaccount.Remark;

            return model;
        }

        private void UpdateActivity(string name, string page, string remark = "")
        {
            try
            {
                activityLogModel.ActivityName = name;
                activityLogModel.ActivityPage = page;
                activityLogModel.Remark = remark;
                activityLogModel.UserId = CurrentUser?.UserID ?? 0;
                LogActivity(activityLogModel);
            }
            catch (Exception ex)
            {

                LogException(ex);
            }

        }

        // GET: BankAccount
        

        public ActionResult BankDetails()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetBankDetailAccounts(DataTableServerSide model)
        {
            KeyValuePair<int, List<BankAccount>> bankAccount = bankAccountService.GetBankDetailsAccounts(model);

            return Json(new
            {
                draw = model.draw,
                recordsTotal = bankAccount.Key,
                recordsFiltered = bankAccount.Key,
                data = bankAccount.Value.Select(c => new List<object> {
                    c.BankName,
                    c.BranchName,
                    c.BranchAddress,
                    c.AccountNo,
                    c.IFSCCode,
                    c.HolderName
                })
            }, JsonRequestBehavior.AllowGet);
        }


    }
}