using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.Code.LIBS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web;
using System.IO;
using System.Configuration;
using System.Net;
using LinqToExcel;
using OfficeOpenXml;
using Microsoft.Office.Interop.Excel;
using DataTable = System.Data.DataTable;
using LinqToExcel.Logging;
using System.Web.DynamicData;
using System.Web.UI.WebControls;
using Excel;
using ExcelDataReader;
using ExcelReaderFactory = ExcelDataReader.ExcelReaderFactory;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
using System.IO.Packaging;
using WebGrease.Activities;
using System.Text;
using Irony.Parsing;


namespace DhruvEnterprises.Web.Controllers
{

    public class WalletController : BaseController
    {
        public ActionAllowedDto action;
        private readonly IUserService userService;
        private readonly IWalletService walletService;
        private readonly IRechargeReportService rechargeReportService;
        private readonly IBankAccountService bankAccountService;
        private readonly IApiService apiService;
        private readonly IRequestResponseService reqResService;
        private SqlConnection con;

        ActivityLogDto activityLogModel;

        public WalletController(IUserService _adminUserService,
            IActivityLogService _activityLogService,
            IRoleService _roleService,
            IWalletService _walletService,
            IRechargeReportService _rechargeReportService,
            IBankAccountService _bankAccountService,
                 IRequestResponseService _reqResService,
            IApiService _apiService) : base(_activityLogService, _roleService)
        {
            this.userService = _adminUserService;
            this.walletService = _walletService;
            this.reqResService = _reqResService;
            this.rechargeReportService = _rechargeReportService;
            this.bankAccountService = _bankAccountService;
            this.apiService = _apiService;

            this.action = new ActionAllowedDto();
            this.activityLogModel = new ActivityLogDto();

        }

        // GET: Wallet
        public ActionResult Index(int? id)
        {
            int userid = id ?? 0;
            UpdateActivity("User Wallet Add REQUEST", "Get:Wallet/Index userid=" + userid);
            ViewBag.actionAllowed = action = ActionAllowed("Wallet", CurrentUser.RoleId, 2);
            ViewBag.User = walletService.GetWalletUserList(3).Select(x => new UserDto()
            {
                Id = x.Id,
                Username = (x.UserProfile?.FullName ?? string.Empty) + "- " + x.Username
            }).ToList();
            WalletDto model = new WalletDto();
            model.userid = userid;
            model.CurrentBalance = userService.GetUser(userid)?.UserBal ?? 0;
            model.PaymentDate = DateTime.Now.ToString("dd/MM/yyyy");
            model.TrTypeList = bankAccountService.GetTransferTypeList().Select(x => new TransferTypeDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
            model.BankAccountList = bankAccountService.GetBankAccounts(true).Select(x => new BankAccountDto() { Id = x.Id, HolderName = x.HolderName }).ToList();
            model.BankAccountId = 1;
            model.TrTypeId = 3;
            string dtime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            model.ChequeNo = string.IsNullOrWhiteSpace(model.ChequeNo) ? dtime : model.ChequeNo;
            model.Remark = string.IsNullOrWhiteSpace(model.Remark) ? dtime : model.Remark;
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(WalletDto model)
        {
            string log = " Wallet Add/pull start";
            UpdateActivity("User Wallet Add", "Post:Wallet/Index");
            ViewBag.actionAllowed = action = ActionAllowed("Wallet", CurrentUser.RoleId, 2);
            string dtime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            DateTime paydate = DateTime.ParseExact(!string.IsNullOrEmpty(model.PaymentDate) ? model.PaymentDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            model.ChequeNo = string.IsNullOrWhiteSpace(model.ChequeNo) ? dtime : model.ChequeNo;
            model.Remark = string.IsNullOrWhiteSpace(model.Remark) ? dtime : model.Remark;
            try
            {
                string error = "0";
                if (model.IsClearCredit && (model.BankAccountId == 2 || model.TrTypeId == 2))
                {
                    ShowErrorMessage("Error!", "Invalid BankAccount/TransferType for credit clear request.", false);
                }
                else if (model.IsClearCredit && model.IsPullOut)
                {
                    ShowErrorMessage("Error!", "Select only one from PullOut and CreditClear.", false);
                }
                else if (!string.IsNullOrEmpty(model.ChequeNo) && model.AddAmount > 0 && model.AddAmount <= 10000000 && model.BankAccountId > 0 && model.TrTypeId > 0)
                {
                    using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
                    {
                        SqlCommand cmd = new SqlCommand("sp_AddUserWallet", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IsPullOut", model.IsPullOut ? "Yes" : "No");
                        cmd.Parameters.AddWithValue("@UserId", model.userid);
                        cmd.Parameters.AddWithValue("@Amount", model.AddAmount);
                        cmd.Parameters.AddWithValue("@Remark", model.Remark);
                        cmd.Parameters.AddWithValue("@AddedById", CurrentUser.UserID);
                        cmd.Parameters.AddWithValue("@IsCreditClear", model.IsClearCredit ? "Yes" : "No");
                        cmd.Parameters.AddWithValue("@TrTypeId", model.TrTypeId);
                        cmd.Parameters.AddWithValue("@BankAccountId", model.BankAccountId);
                        cmd.Parameters.AddWithValue("@ChequeNo", model.ChequeNo);
                        cmd.Parameters.AddWithValue("@PaymentDate", paydate.ToString("yyyy-MM-dd hh:mm:ss.fff"));
                        cmd.Parameters.AddWithValue("@WRID", "0");
                        cmd.Parameters.Add("@error", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@Log", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        error = Convert.ToString(cmd.Parameters["@error"].Value);
                        log += "\r\n error=" + error;
                        log += "\r\n splog=" + Convert.ToString(cmd.Parameters["@Log"].Value);
                    }

                    if (error == "0")
                    {
                        if (model.TrTypeId == 7)
                        {
                            User user = userService.GetUser(model.userid);
                            if (user.VendorId != null)
                            {
                                using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
                                {
                                    SqlCommand cmd = new SqlCommand("sp_AddApiWallet", con);
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@ApiId", user.VendorId);
                                    cmd.Parameters.AddWithValue("@Ins_Amt", 0);
                                    cmd.Parameters.AddWithValue("@AmountSent", model.AddAmount);
                                    cmd.Parameters.AddWithValue("@AmountReceived", model.AddAmount);
                                    cmd.Parameters.AddWithValue("@Remark", model.Remark);
                                    cmd.Parameters.AddWithValue("@AddedById", CurrentUser.UserID);
                                    cmd.Parameters.AddWithValue("@BankAccountId", model.BankAccountId);
                                    cmd.Parameters.AddWithValue("@TrTypeId", model.TrTypeId);
                                    cmd.Parameters.AddWithValue("@ChequeNo", model.ChequeNo + "_AE");
                                    cmd.Parameters.AddWithValue("@IsPullOut", model.IsPullOut ? "Yes" : "No");
                                    cmd.Parameters.Add("@error", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                                    con.Open();
                                    cmd.ExecuteNonQuery();
                                    con.Close();

                                    error = Convert.ToString(cmd.Parameters["@error"].Value);

                                }
                                if (error == "0")
                                {
                                    //model.IsDirect = model.IsDirect ? true : false;
                                    ShowSuccessMessage("Success!", "Wallet has been updated", false);
                                }

                                else if (error == "2")
                                {
                                    //model.IsDirect = false;
                                    ShowErrorMessage("Error!", "Duplicate Cheque/Ref number!", false);
                                }

                                else
                                {
                                    //model.IsDirect = false;
                                    ShowErrorMessage("OOPS!", "Something Went Wrong!", false);
                                }
                            }
                        }
                        ShowSuccessMessage("Success!", "Wallet has been updated", false);
                    }

                    else if (error == "2")
                        ShowErrorMessage("Error!", "Duplicate Cheque/Ref number!", false);
                    else if (error == "3")
                        ShowErrorMessage("Error!", "Already Approved!", false);
                    else if (error == "4")
                        ShowErrorMessage("Error!", "Duplicate Txn of same wallet request!", false);
                    else if (error == "5")
                        ShowErrorMessage("Error!", "User Credit Limit Low Please Contact to Admin !", false);
                    else
                        ShowErrorMessage("OOPS!", "Something Went Wrong!", false);
                }
                else
                {
                    ShowErrorMessage("Error!", "Please Enter All Required Fields.", false);
                }

            }
            catch (Exception e)
            {
                ShowErrorMessage("Error!", "Internal Server Error!", false);
                LogException(e);
            }
            log += "-end=";
            LogActivity(log);
            return NewtonSoftJsonResult(new RequestOutcome<string> { IsSuccess = true, RedirectUrl = Url.Action("Index") });

        }
        public ActionResult ExcelPayoutPayment()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ExcelPayoutPayment(FormCollection formCollection)
        {

            if (Request != null)
            {
                HttpPostedFileBase file = Request.Files["FileUpload"];
                if (file != null)
                {
                    string fileName = file.FileName;
                    string fileContentType = file.ContentType;
                    byte[] fileBytes = new byte[file.ContentLength];
                    var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                }
            }
            PayoutDto model = new PayoutDto();
            List<PayoutDto> billVerifydtos = new List<PayoutDto>();
            JObject res = new JObject();

            int t = Request.Files.Count;
            HttpPostedFileBase FileUpload = Request.Files[0];
            string filepath = System.Web.HttpContext.Current.Server.MapPath("~/Doc/");
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            if (FileUpload.ContentType == "application/vnd.ms-excel" || FileUpload.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                string filename = DateTime.Today.ToString("yyyyMMddhhmmssfffffff") + "_" + FileUpload.FileName;
                if (filename.EndsWith(".xls") || filename.EndsWith(".xlsx"))
                {
                    DataTable dtExcelData = new DataTable();
                    string targetpath = Server.MapPath("~/Doc/");
                    FileUpload.SaveAs(targetpath + filename);
                    string pathToExcelFile = targetpath + filename;
                    string uk = DateTime.Now.ToString("yyyyMMddhhmmssfffffff");
                    string Unkey = CurrentUser.UserID + uk;
                    string sheetName = "Sheet1";
                    var excelFile = new ExcelQueryFactory(pathToExcelFile);
                    var artistAlbums = (from a in excelFile.Worksheet<ExcelDataDto>(sheetName) select a).Skip(1);
                    try
                    {
                        List<PayoutDto> rechargeCalldto2s = new List<PayoutDto>();
                        if (artistAlbums.Count() > 0)
                        {
                            if (artistAlbums.Count() < 50)
                            {
                                foreach (var item in artistAlbums)
                                {
                                    string s = item?.IFSC != null ? "0" : item?.Mobileno?.IndexOf('1', 0).ToString();
                                    if (item?.Mobileno != null && item?.Amount != null && item?.Account != null && item?.IFSC != null && item?.MODE != null && item?.Firstname != null && item?.Lastname != null)
                                    {
                                        InsertIntoExcelVerify(CurrentUser?.UserID ?? 0, item.Mobileno, item.Account, item.IFSC, item.Name, item.Amount, item.MODE, "Waiting", Unkey, item.Firstname, item.Lastname);
                                    }

                                    else
                                    {
                                        ShowErrorMessage("Error!", "Please check your excel data all fields mandatory", false);
                                        return Json(3, JsonRequestBehavior.AllowGet);
                                    }
                                }
                                try
                                {
                                    DataTable dataTable = GetExcelVerify(Unkey);
                                    foreach (DataRow dd in dataTable.Rows)
                                    {
                                        PayoutDto billVerifydto = new PayoutDto
                                        {
                                            Name = Convert.ToString(dd["Name"] ?? ""),
                                            Amount = Convert.ToDecimal(dd["Amount"] ?? ""),
                                            Mobileno = Convert.ToString(dd["MobileNo"] ?? ""),
                                            IFSC = Convert.ToString(dd["IFSC"] ?? ""),
                                            AccountNo = Convert.ToString(dd["AccountNo"] ?? ""),
                                            MODE = Convert.ToString(dd["Type"] ?? ""),
                                            Firstname = Convert.ToString(dd["Firstname"] ?? ""),
                                            Lastname = Convert.ToString(dd["Lastname"] ?? "")
                                        };
                                        billVerifydtos.Add(billVerifydto);

                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogException(ex);
                                }
                            }
                            else
                            {
                                ShowErrorMessage("Error!", "You can do only 50 data at a Time", false);
                                return Json(3, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            ShowErrorMessage("Error!", "No valid Data found in Excel-File !!", false);
                            return Json(3, JsonRequestBehavior.AllowGet);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }
                }
            }
            var userid = CurrentUser.UserID;
            if (userid > 0)
            {
                model.UserID = Convert.ToString(userid);
                model.Operator = "4";
                var apiurl = "https://easepays.in/api/" + "Service/PayoutTransfer";
                //var uniquerefId = DateTime.Now.ToString("yyyyMMddhhmmssfffffff");
                User user = userService.GetUser(CurrentUser.UserID);
                user.HPass = Guid.NewGuid();
                userService.Save(user);
                foreach (var item in billVerifydtos)
                {
                    //var uniquerefId = DateTime.Now.ToString("yyyyMMddhhmmssfffffff");
                    var uniquerefId = GenerateUniqueId().ToUpper(); // Alphanumeric 50 characters Unique Id


                    MoneyTransferDto moneyTransfer = new MoneyTransferDto
                    {
                        Account = item.AccountNo,
                        Amount = Convert.ToString(item.Amount),
                        ApiToken = user.TokenAPI,
                        IFSC = item.IFSC,
                        Mobileno = item.Mobileno,
                        Name = item.Name,
                        BankName = model.BankName,
                        Mode = item.MODE,
                        OpId = model.Operator,
                        OrderID = uniquerefId,
                        BankID = model.BankID,
                        Pin = model.Pin,
                        Firstname = item.Firstname,
                        Lastname = item.Lastname
                    };
                    LIBS.ApiCall apiCall = new LIBS.ApiCall(reqResService);
                    var jsonresult = JsonConvert.SerializeObject(moneyTransfer);

                    PayoutResponsedto payoutResponsedto = new PayoutResponsedto();
                    RequestResponseDto requestReponsedto = new RequestResponseDto();
                    var apires = apiCall.Post(apiurl, jsonresult, "application/json", "application/json", ref requestReponsedto);
                };
            }

            ShowSuccessMessage("Success!", "Request submitted successfully !!", false);
            return Json(1, JsonRequestBehavior.AllowGet);

        }
        private string GenerateUniqueId()
        {
            // Use a combination of timestamp and random string to create a unique ID
            string randomString = GenerateRandomString(50); // Adjust the length as needed


            string uniqueId = randomString;
            var datefromet = System.DateTime.Now.ToString("dddHHmm"); // ddd  day of the current year hh will be in 24 hour fromet and mm  in minutes

            return uniqueId + datefromet;
        }
        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();

            // Generate a random string of the specified length
            string randomString = new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
            return randomString;
        }
        private void CheckStatusThread(string apiurl, string Data = "")
        {

            try
            {
                //string resendurl = SiteKey.ApiDomainName + "Service/VendorStatusCheck?token=VendorStatusCheck&recid=" + rId + "&uid=" + uid;
                string resendurl = apiurl + Data;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                HttpWebRequest httpreq = (HttpWebRequest)WebRequest.Create(resendurl);
                httpreq.GetResponseAsync();
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }
        [HttpPost]
        public ActionResult GetUserWalletBalance(string userid)
        {
            var user = userService.GetUser(Convert.ToInt32(userid));
            return Json(new { bal = user?.UserBal ?? 0, cbal = user?.CreditBal ?? 0 });
        }

        [HttpGet]
        public ActionResult TxnReport(int? y, long? ri, long? ti, int? tt, int? at, int? vi, int? ui, string fd = "", string ed = "", string rm = "")
        {
            UpdateActivity("User TxnReport REQUEST", "Get:Wallet/TxnReport");
            ViewBag.actionAllowed = action = ActionAllowed("TxnReport", CurrentUser.RoleId);

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

            int userrole = CurrentUser.RoleId;
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? filter.userid : CurrentUser.UserID;


            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName, Selected = (x.Id == ui) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == vi) }).ToList();
            ViewBag.TxnTypeList = rechargeReportService.GetTxnTypes().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = (x.Id == tt) }).ToList();
            ViewBag.AmtTypeList = rechargeReportService.GetAmtTypes("Wallet").Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.AmtTypeName, Selected = (x.Id == at) }).ToList();
            #endregion

            return View();
        }

        [HttpPost]
        public ActionResult GetTxnReport(DataTableServerSide model)
        {
            ViewBag.actionAllowed = action = ActionAllowed("TxnReport", CurrentUser.RoleId);

            #region "filter"

            int userrole = CurrentUser.RoleId;
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? 0 : CurrentUser.UserID;

            TxnFilterDto filter = TempData["TxnFilterDto"] != null ? (TxnFilterDto)TempData["TxnFilterDto"] : new TxnFilterDto();

            filter.userid = IsAdminRole ? filter.userid : uid;
            ViewBag.FilterData = TempData["TxnFilterDto"] = filter;

            filter.sdate = TempData["sDate"] != null ? filter.sdate : "";
            filter.edate = TempData["eDate"] != null ? filter.edate : "";

            #endregion


            KeyValuePair<int, List<TxnLedger>> txnledger = walletService.GetTxnReport(model, filter.recid, filter.txnid, filter.txntypeid, filter.amttypeid, filter.apiid, filter.userid, filter.sdate, filter.edate, filter.remark);


            return Json(new
            {
                draw = model.draw,
                recordsTotal = txnledger.Key,
                recordsFiltered = txnledger.Key,
                data = txnledger.Value.Select(c => new List<object> {
                    c.Id,
                    c.RecId??c.DMTid,
                    c.User1?.UserProfile?.FullName??string.Empty,
                    c.OP_Bal,
                    c.CR_Amt,
                    c.DB_Amt,
                    c.CL_Bal,
                    c.AmtType?.AmtTypeName,
                    c.TxnType?.TypeName,
                    (c.TxnDate).ToString(),
                    c.Remark

                    })
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult RefundReport(int? y, long? ri, long? ti, int? tt, int? at, int? vi, int? ui, string fd = "", string ed = "", string rm = "")
        {
            UpdateActivity("User RefundReport REQUEST", "Get:Wallet/RefundReport");
            ViewBag.actionAllowed = action = ActionAllowed("RefundReport", CurrentUser.RoleId);

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

            int userrole = CurrentUser.RoleId;
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? filter.userid : CurrentUser.UserID;


            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName, Selected = (x.Id == ui) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == vi) }).ToList();
            ViewBag.TxnTypeList = rechargeReportService.GetTxnTypes().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = (x.Id == tt) }).ToList();
            ViewBag.AmtTypeList = rechargeReportService.GetAmtTypes("Wallet").Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.AmtTypeName, Selected = (x.Id == at) }).ToList();
            #endregion

            return View();
        }

        [HttpPost]
        public ActionResult GetRefundReport(DataTableServerSide model)
        {
            ViewBag.actionAllowed = action = ActionAllowed("RefundReport", CurrentUser.RoleId);

            #region "filter"

            int userrole = CurrentUser.RoleId;
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? 0 : CurrentUser.UserID;

            TxnFilterDto filter = TempData["TxnFilterDto"] != null ? (TxnFilterDto)TempData["TxnFilterDto"] : new TxnFilterDto();

            filter.userid = IsAdminRole ? filter.userid : uid;
            ViewBag.FilterData = TempData["TxnFilterDto"] = filter;

            filter.sdate = TempData["sDate"] != null ? filter.sdate : "";
            filter.edate = TempData["eDate"] != null ? filter.edate : "";

            #endregion


            KeyValuePair<int, List<TxnLedger>> txnledger = walletService.GetRefundReport(model, filter.recid, filter.txnid, filter.txntypeid, filter.amttypeid, filter.apiid, filter.userid, filter.sdate, filter.edate, filter.remark);


            return Json(new
            {
                draw = model.draw,
                recordsTotal = txnledger.Key,
                recordsFiltered = txnledger.Key,
                data = txnledger.Value.Select(c => new List<object> {
                    c.Id,
                    c.RecId,
                    c.User1?.UserProfile?.FullName??string.Empty,
                    c.Recharge.CustomerNo,
                    c.Recharge.Amount,
                    c.OP_Bal,
                    c.CR_Amt,
                    c.DB_Amt,
                    c.CL_Bal,
                    c.AmtType?.AmtTypeName,
                    c.TxnType?.TypeName,
                    (c.TxnDate).ToString(),
                    c.Remark

                    })
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult WalletRequest(int? u, int? u2, int? t, int? a, int? s, string f = "", string e = "", string cq = "", string r = "", string c = "")
        {

            UpdateActivity("User WalletRequest ", "Get:Wallet/WalletRequest");
            ViewBag.actionAllowed = action = ActionAllowed("WalletRequest", CurrentUser.RoleId);

            var filter = new WalletRequestFilterDto
            {
                UserId = u ?? 0,
                UpdatedById = u2 ?? 0,
                TrTypeId = t ?? 0,
                AccountId = a ?? 0,
                Sdate = f,
                SdateNow = !string.IsNullOrEmpty(f) ? f : DateTime.Now.ToString("dd/MM/yyy"),
                Edate = e,
                EdateNow = !string.IsNullOrEmpty(e) ? e : DateTime.Now.ToString("dd/MM/yyy"),
                ChequeNo = cq,
                Remark = r,
                Comment = c,
                StatusId = s ?? 0
            };

            ViewBag.FilterData = filter;
            ViewBag.UserList = walletService.GetWalletUserList(3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = (x.UserProfile?.FullName ?? string.Empty) + "- " + x.Username, Selected = (x.Id == filter.UserId) }).ToList();
            ViewBag.UserList2 = userService.GetUserList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = (x.UserProfile?.FullName ?? string.Empty) + "- " + x.Username, Selected = (x.Id == filter.UpdatedById) }).ToList();
            ViewBag.TransferTypes = bankAccountService.GetTransferTypeList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == filter.TrTypeId) }).ToList();
            ViewBag.BankAccounts = bankAccountService.GetBankAccounts(true).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.HolderName, Selected = (x.Id == filter.AccountId) }).ToList();
            ViewBag.StatusList = rechargeReportService.GetStatusList().Where(x => x.Remark.Contains("WalletRequest")).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = ((x.Id == filter.StatusId)) }).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult GetWalletRequests(DataTableServerSide model)
        {
            ViewBag.actionAllowed = this.action = ActionAllowed("WalletRequest", CurrentUser.RoleId);

            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? (model.filterdata?.UserId ?? 0) : CurrentUser.UserID;

            string action = IsAdminRole ? "approverequest" : "CreateEditRequest";
            string modelid = IsAdminRole ? "modal-approve-wallet-request" : "modal-createedit-wallet-request";

            KeyValuePair<int, List<WalletRequest>> walletRequest = walletService.GetWalletRequests(model, uid);

            TempData["totelamt"] = walletRequest.Value.Sum(c => c.Amount);
            return Json(new
            {
                draw = model.draw,
                recordsTotal = walletRequest.Key,
                recordsFiltered = walletRequest.Key,
                data = walletRequest.Value.Select(c => new List<object> {
                     (this.action.AllowEdit && c.StatusId==5 && IsAdminRole?(
                      DataTableButton.EditButton(Url.Action( "CreateEditRequest" , "wallet",new { id = c.Id }),"modal-createedit-wallet-request")
                        +"&nbsp;"+DataTableButton.ValidateButton(Url.Action( "approverequest" , "wallet", new { id = c.Id }), "modal-approve-wallet-request","Approve")
                     +"&nbsp;"+DataTableButton.DeleteButton(Url.Action( "CancelRequest" , "wallet", new { id = c.Id }),"modal-cancel-wallet-request","Cancel")
                     +"&nbsp;"+DataTableButton.View(Url.Action( "ViewImage" , "wallet", new { id = c.Id }),"modal-viewImage-wallet-request","View Image")
                     ):"<b style='color:green;'>Processed!</b>" ),
                    c.Id,
                    (c.AddedDate).ToString(),
                     (c.PaymentDate)?.ToString("MM/dd/yyyy"),
                    c.User2?.UserProfile?.FullName,
                    c.Amount,
                     c.TransferType?.Name??string.Empty,
                    c.TxnType?.TypeName,
                     c.AmtType?.AmtTypeName,
                     getColorLabel(c.StatusId, c.StatusType.TypeName),
                    //c.ImageUrl??"",
                    c.BankAccount?.HolderName??(!string.IsNullOrEmpty(c.Bankname)?c.Bankname:string.Empty),
                    c.Chequeno,
                    c.PaymentRemark,
                    (c.UpdatedDate).ToString(),
                    c.TxnId,
                    c.Comment,
                    c.User1?.UserProfile?.FullName??string.Empty,
                    (c.IsCreditClear??false)?"Yes":"No"

                    })
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ExcelUpload()
        {
            ExcelUpload excelUpload = new ExcelUpload();
            ViewBag.BankAccounts = bankAccountService.GetBankAccounts(true).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.HolderName }).ToList();
            return View();
        }


        [HttpPost]
        public ActionResult Import(ExcelUpload model1)
        {
            if (Request != null)
            {
                HttpPostedFileBase file = model1.File;
                if (file != null)
                {
                    try
                    {
                        string targetPath = Server.MapPath("~/Doc/");
                        string fileName = DateTime.Now.ToString("yyyyMMddhhmmssfffffff") + "_" + Path.GetFileName(model1.File.FileName);
                        string filePath = Path.Combine(targetPath, fileName);
                        model1.File.SaveAs(filePath);

                        if (fileName.EndsWith(".xls") || fileName.EndsWith(".xlsx"))
                        {
                            DataSet dataSet = new DataSet();
                            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                            {
                                if (model1.BankAccountId == 4)
                                {
                                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                                    {
                                        dataSet = reader.AsDataSet();
                                        DataTable dataTable = dataSet.Tables[0];

                                        var dataRows = dataTable.AsEnumerable().Skip(11);

                                        var headerRow = dataTable.Rows[10];
                                        foreach (DataColumn column in dataTable.Columns)
                                        {
                                            column.ColumnName = headerRow[column.Ordinal].ToString();
                                        }
                                        if (dataRows.Any())
                                        {
                                            dataRows = dataRows.Take(dataRows.Count() - 1);
                                        }

                                        List<AuBankDto> retrievedData = new List<AuBankDto>();

                                        foreach (DataRow r in dataRows)
                                        {
                                            AuBankDto model = new AuBankDto();
                                            model.Date = r["Trans Date"].ToString();
                                            model.ChequeNo = r["Description/Narration"].ToString();
                                            model.Credit = r["Credit(Cr.) INR"].ToString();
                                            //string excelChequeno = GetChequenoFromExcelString(model.ChequeNo);
                                            DataTable matchData = CheckRecordWithDatabaseAu(model, model.ChequeNo, model1);
                                            if (matchData.Rows.Count > 0)
                                            {
                                                foreach (DataRow matchRow in matchData.Rows)
                                                {
                                                    AuBankDto matchModel = new AuBankDto();
                                                    matchModel.Date = Convert.ToDateTime(matchRow["PaymentDate"]).ToString("yyyy-MM-dd");
                                                    matchModel.ChequeNo = matchRow["Chequeno"].ToString();
                                                    matchModel.Credit = matchRow["Amount"].ToString();
                                                    matchModel.Status = matchRow["StatusType"].ToString();
                                                    matchModel.Username = matchRow["Fullname"].ToString();
                                                    retrievedData.Add(matchModel);
                                                }
                                            }
                                            else
                                            {
                                                AuBankDto unmatchedModel = new AuBankDto();
                                                unmatchedModel.Date = model.Date;
                                                //unmatchedModel.ChequeNo = model.ChequeNo;
                                                unmatchedModel.ChequeNo = model.ChequeNo;
                                                unmatchedModel.Credit = model.Credit;
                                                unmatchedModel.Username = model.Username;
                                                unmatchedModel.Status = "Data not found";
                                                retrievedData.Add(unmatchedModel);
                                            }
                                        }
                                        DataTable dt = new DataTable();
                                        dt.Columns.Add("Transaction Date", typeof(string));
                                        dt.Columns.Add("Cheque No", typeof(string));
                                        dt.Columns.Add("Amount", typeof(string));
                                        dt.Columns.Add("StatusType", typeof(string));
                                        dt.Columns.Add("Fullname", typeof(string));
                                        foreach (var item in retrievedData)
                                        {
                                            dt.Rows.Add(item.Date, item.ChequeNo, item.Credit, item.Status,item.Username);
                                        }

                                        string outputPath = Server.MapPath("~/Doc/");
                                        if (!Directory.Exists(outputPath))
                                        {
                                            Directory.CreateDirectory(outputPath);
                                        }
                                        Random rand = new Random();
                                        int randomNumber = rand.Next(1000, 10000);
                                        string processedFileName = $"{Path.GetFileNameWithoutExtension(model1.File.FileName)}_{randomNumber}_Processed.xlsx";

                                        using (MemoryStream output = new MemoryStream())
                                        {
                                            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                                            using (ExcelPackage excelPackage = new ExcelPackage(output))
                                            {
                                                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");
                                                worksheet.Cells["A1"].LoadFromDataTable(dt, true);
                                                excelPackage.SaveAs(new FileInfo(Path.Combine(outputPath, processedFileName)));
                                            }
                                            byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(outputPath, processedFileName));

                                            return Json(new { success = true, fileBytes = fileBytes, fileName = processedFileName });
                                        }
                                    }
                                }
                                else if (model1.BankAccountId == 1)
                                {
                                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                                    {
                                        dataSet = reader.AsDataSet();
                                        DataTable dataTable = dataSet.Tables[0];

                                        var dataRows = dataTable.AsEnumerable().Skip(9);

                                        var headerRow = dataTable.Rows[8];
                                        foreach (DataColumn column in dataTable.Columns)
                                        {
                                            column.ColumnName = headerRow[column.Ordinal].ToString();
                                        }
                                        if (dataRows.Any())
                                        {
                                            dataRows = dataRows.Take(dataRows.Count() - 5);
                                        }
                                        List<CosmosBankDto> retrievedData = new List<CosmosBankDto>();
                                        foreach (DataRow r in dataRows)
                                        {
                                            if (r["Date"] == null && r["Transaction Particulars"] == null)
                                            {
                                                continue;
                                            }
                                            CosmosBankDto model = new CosmosBankDto();
                                            model.Date = r["Date"].ToString() ?? "";
                                            model.TransactionParticulars = r["Transaction Particulars"].ToString() ?? "";
                                            model.ChequeNo = r["Cheque No"].ToString() ?? "";
                                            model.Withdrawal = r["Withdrawal"].ToString() ?? "";
                                            model.Deposit = r["Deposit"].ToString() ?? "";
                                            model.AvailableBalance = r["Available Balance"].ToString() ?? "";
                                            //string excelChequeno = GetChequenoFromExcelString(model.TransactionParticulars);
                                            if (model1.BankAccountId == 1)
                                            {
                                                if (DateTime.TryParseExact(model.Date, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                                                {
                                                    model.Date = parsedDate.ToString("yyyy-MM-dd");
                                                }
                                                else
                                                {
                                                    model.Date = DateTime.Now.ToString();
                                                }
                                            }
                                            DataTable matchData = CheckRecordWithDatabaseCosmos(model, model.TransactionParticulars, model1);
                                            if (matchData.Rows.Count > 0)
                                            {
                                                foreach (DataRow matchRow in matchData.Rows)
                                                {
                                                    CosmosBankDto matchModel = new CosmosBankDto();
                                                    matchModel.Date = Convert.ToDateTime(matchRow["PaymentDate"]).ToString("yyyy-MM-dd") ?? "";
                                                    matchModel.ChequeNo = matchRow["Chequeno"].ToString() ?? "";
                                                    matchModel.Deposit = matchRow["Amount"].ToString() ?? "";
                                                    matchModel.Status = matchRow["StatusType"].ToString() ?? "";
                                                    matchModel.Username = matchRow["Fullname"].ToString();
                                                    retrievedData.Add(matchModel);
                                                }
                                            }
                                            else
                                            {
                                                CosmosBankDto unmatchedModel = new CosmosBankDto();
                                                unmatchedModel.Date = model.Date;
                                                unmatchedModel.ChequeNo = model.TransactionParticulars ?? "";
                                                unmatchedModel.Deposit = model.Deposit ?? "";
                                                unmatchedModel.Username = model.Username;
                                                unmatchedModel.Status = "Data not found";
                                                retrievedData.Add(unmatchedModel);
                                            }
                                        }
                                        DataTable dt = new DataTable();
                                        dt.Columns.Add("Date", typeof(string));
                                        dt.Columns.Add("Transaction Particulars", typeof(string));
                                        dt.Columns.Add("Cheque No", typeof(string));
                                        dt.Columns.Add("Deposit", typeof(string));
                                        dt.Columns.Add("StatusType", typeof(string));
                                        dt.Columns.Add("Fullname", typeof(string));
                                        foreach (var item in retrievedData)
                                        {
                                            dt.Rows.Add(item.Date, item.TransactionParticulars, item.ChequeNo, item.Deposit, item.Status,item.Username);
                                        }

                                        string outputPath = Server.MapPath("~/Doc/");
                                        if (!Directory.Exists(outputPath))
                                        {
                                            Directory.CreateDirectory(outputPath);
                                        }
                                        Random rand = new Random();
                                        int randomNumber = rand.Next(1000, 10000);
                                        string processedFileName = $"{Path.GetFileNameWithoutExtension(model1.File.FileName)}_{randomNumber}_CosmosProcessed.xlsx";
                                        using (MemoryStream output = new MemoryStream())
                                        {
                                            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                                            using (ExcelPackage excelPackage = new ExcelPackage(output))
                                            {
                                                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet2");
                                                worksheet.Cells["A1"].LoadFromDataTable(dt, true);
                                                excelPackage.SaveAs(new FileInfo(Path.Combine(outputPath, processedFileName)));
                                            }
                                            byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(outputPath, processedFileName));

                                            // Send the file to the client's browser for download
                                            return Json(new { success = true, fileBytes = fileBytes, fileName = processedFileName });

                                          
                                        }
                                    }
                                }
                                else if (model1.BankAccountId == 3)
                                {
                                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                                    {
                                        dataSet = reader.AsDataSet();
                                        DataTable dataTable = dataSet.Tables[0];

                                        var dataRows = dataTable.AsEnumerable().Skip(22);

                                        var headerRow = dataTable.Rows[20];
                                        foreach (DataColumn column in dataTable.Columns)
                                        {
                                            column.ColumnName = headerRow[column.Ordinal].ToString();
                                        }
                                        if (dataRows.Any())
                                        {
                                            dataRows = dataRows.Take(dataRows.Count() - 18);
                                        }

                                        List<HdfcBankDto> retrievedDataHDfc = new List<HdfcBankDto>();

                                        foreach (DataRow r in dataRows)
                                        {
                                            HdfcBankDto model = new HdfcBankDto();
                                            model.Date = r["Date"].ToString();
                                            model.Narration = r["Narration"].ToString();
                                            model.ChqRefNo = r["Chq./Ref.No."].ToString();
                                            model.ValueDt = r["Value Dt"].ToString();
                                            model.WithdrawalAmt = r["Withdrawal Amt."].ToString();
                                            model.DepositAmt = r["Deposit Amt."].ToString();
                                            model.ClosingBalance = r["Closing Balance"].ToString();

                                            //string excelChequeno = GetChequenoFromExcelString(model.ChequeNo);
                                            DataTable matchDataHD = CheckRecordWithDatabaseHdfc(model, model.Narration, model1);
                                            if (matchDataHD.Rows.Count > 0)
                                            {
                                                foreach (DataRow matchRow in matchDataHD.Rows)

                                                {
                                                    HdfcBankDto matchM = new HdfcBankDto();
                                                    //matchM.Date = Convert.ToDateTime(matchRow["Date"]).ToString("yyyy-MM-dd");
                                                    matchM.Date = matchRow["PaymentDate"].ToString();
                                                    matchM.Narration = matchRow["TxnId"].ToString();
                                                    matchM.ChqRefNo = matchRow["Chequeno"].ToString();
                                                    matchM.DepositAmt = matchRow["Amount"].ToString();
                                                    matchM.Status = matchRow["StatusType"].ToString();
                                                    matchM.Username = matchRow["Fullname"].ToString();
                                                    retrievedDataHDfc.Add(matchM);
                                                }
                                            }
                                            else
                                            {
                                                HdfcBankDto unmatchedModelHdfc = new HdfcBankDto();
                                                unmatchedModelHdfc.Date = model.Date;
                                                unmatchedModelHdfc.Narration = model.Narration;
                                                unmatchedModelHdfc.ChqRefNo = model.ChqRefNo;
                                                unmatchedModelHdfc.ValueDt = model.ValueDt;
                                                unmatchedModelHdfc.WithdrawalAmt = model.WithdrawalAmt;
                                                unmatchedModelHdfc.DepositAmt = model.DepositAmt;
                                                unmatchedModelHdfc.ClosingBalance = model.ClosingBalance;
                                                unmatchedModelHdfc.Username = model.Username;
                                                unmatchedModelHdfc.Status = "Data not found";
                                                retrievedDataHDfc.Add(unmatchedModelHdfc);
                                            }
                                        }
                                        DataTable dt = new DataTable();
                                        dt.Columns.Add("Date", typeof(string));
                                        dt.Columns.Add("Narration", typeof(string));
                                        dt.Columns.Add("Chq/RefNo", typeof(string));
                                        dt.Columns.Add("DepositAmt", typeof(string));
                                        dt.Columns.Add("StatusType", typeof(string));
                                        dt.Columns.Add("Fullname", typeof(string));
                                        foreach (var item in retrievedDataHDfc)
                                        {
                                            dt.Rows.Add(item.Date, item.Narration, item.ChqRefNo, item.DepositAmt, item.Status, item.Username);
                                        }

                                        string outputPath = Server.MapPath("~/Doc/");
                                        if (!Directory.Exists(outputPath))
                                        {
                                            Directory.CreateDirectory(outputPath);
                                        }
                                        Random rand = new Random();
                                        int randomNumber = rand.Next(1000, 10000);
                                        string processedFileName = $"{Path.GetFileNameWithoutExtension(model1.File.FileName)}_{randomNumber}HdfcProcessed.xlsx";

                                        using (MemoryStream output = new MemoryStream())
                                        {
                                            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                                            using (ExcelPackage excelPackage = new ExcelPackage(output))
                                            {
                                                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet5");
                                                worksheet.Cells["A1"].LoadFromDataTable(dt, true);
                                                excelPackage.SaveAs(new FileInfo(Path.Combine(outputPath, processedFileName)));
                                            }
                                            byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(outputPath, processedFileName));
                                            return Json(new { success = true, fileBytes = fileBytes, fileName = processedFileName });
                                        }
                                    }
                                }
                                else
                                {
                                    ShowErrorMessage("Error!", "Invalid file format. Please upload an Excel file.", false);
                                    return Json(new { success = false, message = "Invalid file format. Please upload an Excel file." });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        ShowErrorMessage("Error!Invalid data in row ", " An error occurred while processing the file.", false);
                        return Json(new { success = false, message = "Invalid data in row or An error occurred while processing the file." });
                    }
                }
            }
            return Json(new { success = false, message = "No file uploaded." });

        }
        public ActionResult Download(string fileName)
        {
            string outputPath = Server.MapPath("~/Doc/");
            try
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(outputPath, fileName));
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch(Exception ex)
            {
                byte[] fileBytes = System.IO.File.ReadAllBytes(Path.Combine(outputPath, fileName));
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        public DataTable CheckRecordWithDatabaseCosmos(CosmosBankDto model, string chequeNumber, ExcelUpload model1)
        {
            using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
            {
                DataTable dt = new DataTable();
                SqlCommand command = new SqlCommand("Sp_CheckExceldata", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@BankAccountId", model1.BankAccountId);
                command.Parameters.AddWithValue("@ChequeNo", string.IsNullOrEmpty(chequeNumber) ? (object)DBNull.Value : chequeNumber);
                command.Parameters.AddWithValue("@TransactionDate", model.Date ?? (object)DBNull.Value);

                con.Open();

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dt);
                }

                return dt;
            }
        }
        public DataTable CheckRecordWithDatabaseHdfc(HdfcBankDto model, string chequeNumber, ExcelUpload model1)
        {
            using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
            {
                DataTable dt = new DataTable();
                SqlCommand command = new SqlCommand("Sp_CheckExceldata", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@BankAccountId", model1.BankAccountId);
                command.Parameters.AddWithValue("@ChequeNo", string.IsNullOrEmpty(chequeNumber) ? (object)DBNull.Value : chequeNumber);
                if (!string.IsNullOrEmpty(model.Date))
                {
                    DateTime transactionDate;
                    if (DateTime.TryParse(model.Date, out transactionDate))
                    {
                        command.Parameters.AddWithValue("@TransactionDate", transactionDate);
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@TransactionDate", DBNull.Value);
                    }
                }
                else
                {
                    command.Parameters.AddWithValue("@TransactionDate", DBNull.Value);
                }

                con.Open();

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dt);
                }

                return dt;
            }
        }
        public DataTable CheckRecordWithDatabaseAu(AuBankDto model, string chequeNumber, ExcelUpload model1)
        {
            using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
            {
                DataTable dt = new DataTable();
                SqlCommand command = new SqlCommand("Sp_CheckExceldata", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@BankAccountId", model1.BankAccountId);
                command.Parameters.AddWithValue("@ChequeNo", string.IsNullOrEmpty(chequeNumber) ? (object)DBNull.Value : chequeNumber);
                command.Parameters.AddWithValue("@TransactionDate", model.Date ?? (object)DBNull.Value);

                con.Open();

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dt);
                }

                return dt;
            }
        }



        public string GetChequenoFromExcelString(string excelString)
        {
            //Hdfcbank
            string pattern1 = @"IMPS-(\d+)";//also this use for au bank
            string pattern7 = @"(?<=IMPS P2P\s)(\d+)";
            string pattern8 = @"-(\d+)-";
            //hdfcend

            // this for au

            string pattern2 = @"RTGS CR-([A-Z\d]+)";




            //cosmosbank
            string pattern3 = @"UPI/CR/(\d+)/";//also use for au bank
            string pattern4 = @"IMPS/P2A/\(\)/";
            //cosmosbankend


            Match match1 = Regex.Match(excelString, pattern1);
            Match match2 = Regex.Match(excelString, pattern2);
            Match match3 = Regex.Match(excelString, pattern3);
            Match match4 = Regex.Match(excelString, pattern4);
            Match match7 = Regex.Match(excelString, pattern7);
            Match match8 = Regex.Match(excelString, pattern8);

            if (match1.Success)
            {
                string rr = match1.Value;
                return rr.Substring(5);
            }
            else if (match2.Success)
            {
                string rrr = match2.Value;


                return rrr.Substring(8); ;
            }
            else if (match3.Success)
            {
                return match3.Value;
            }
            else if (match4.Success)
            {
                return match4.Value;
            }


            else if (match7.Success)
            {
                return match7.Value;
            }
            else if (match8.Success)
            {
                return match8.Value;
            }
            else
            {
                return "0";
            }
        }





        [HttpGet]
        public ActionResult PendingWalletRequest(int? u, int? u2, int? t, int? a, int? s, string f = "", string e = "", string cq = "", string r = "", string c = "")
        {

            UpdateActivity("User WalletRequest ", "Get:Wallet/WalletRequest");
            ViewBag.actionAllowed = action = ActionAllowed("WalletRequest", CurrentUser.RoleId);

            var filter = new WalletRequestFilterDto
            {
                UserId = u ?? 0,
                UpdatedById = u2 ?? 0,
                TrTypeId = t ?? 0,
                AccountId = a ?? 0,
                Sdate = f,
                SdateNow = !string.IsNullOrEmpty(f) ? f : DateTime.Now.ToString("dd/MM/yyy"),
                Edate = e,
                EdateNow = !string.IsNullOrEmpty(e) ? e : DateTime.Now.ToString("dd/MM/yyy"),
                ChequeNo = cq,
                Remark = r,
                Comment = c,
                StatusId = s ?? 0
            };

            ViewBag.FilterData = filter;
            ViewBag.UserList = walletService.GetWalletUserList(3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = (x.UserProfile?.FullName ?? string.Empty) + "- " + x.Username, Selected = (x.Id == filter.UserId) }).ToList();
            ViewBag.UserList2 = userService.GetUserList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = (x.UserProfile?.FullName ?? string.Empty) + "- " + x.Username, Selected = (x.Id == filter.UpdatedById) }).ToList();
            ViewBag.TransferTypes = bankAccountService.GetTransferTypeList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == filter.TrTypeId) }).ToList();
            ViewBag.BankAccounts = bankAccountService.GetBankAccounts(true).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.HolderName, Selected = (x.Id == filter.AccountId) }).ToList();
            ViewBag.StatusList = rechargeReportService.GetStatusList().Where(x => x.Remark.Contains("WalletRequest")).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = ((x.Id == filter.StatusId)) }).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult GetPenWalletRequests(DataTableServerSide model)
        {
            ViewBag.actionAllowed = this.action = ActionAllowed("WalletRequest", CurrentUser.RoleId);

            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? (model.filterdata?.UserId ?? 0) : CurrentUser.UserID;
            model.filterdata.StatusId = 5;

            string action = IsAdminRole ? "approverequest" : "CreateEditRequest";
            string modelid = IsAdminRole ? "modal-approve-wallet-request" : "modal-createedit-wallet-request";

            KeyValuePair<int, List<WalletRequest>> walletRequest = walletService.GetWalletRequests(model, uid);

            TempData["totelamt"] = walletRequest.Value.Sum(c => c.Amount);
            return Json(new
            {
                draw = model.draw,
                recordsTotal = walletRequest.Key,
                recordsFiltered = walletRequest.Key,
                data = walletRequest.Value.Select(c => new List<object> {
                     (this.action.AllowEdit && c.StatusId==5 && IsAdminRole?(
                      DataTableButton.EditButton(Url.Action( "CreateEditRequest" , "wallet",new { id = c.Id }),"modal-createedit-wallet-request")
                        +"&nbsp;"+DataTableButton.ValidateButton(Url.Action( "approverequest" , "wallet", new { id = c.Id }), "modal-approve-wallet-request","Approve")
                     +"&nbsp;"+DataTableButton.DeleteButton(Url.Action( "CancelRequest" , "wallet", new { id = c.Id }),"modal-cancel-wallet-request","Cancel")
                     +"&nbsp;"+DataTableButton.View(Url.Action( "ViewImage" , "wallet", new { id = c.Id }),"modal-viewImage-wallet-request","View Image")
                     ):"<b style='color:green;'>Processed!</b>" ),
                    c.Id,
                    (c.AddedDate).ToString(),
                     (c.PaymentDate)?.ToString("MM/dd/yyyy"),
                    c.User2?.UserProfile?.FullName,
                    c.Amount,
                     c.TransferType?.Name??string.Empty,
                    c.TxnType?.TypeName,
                     c.AmtType?.AmtTypeName,
                     getColorLabel(c.StatusId, c.StatusType.TypeName),
                    //c.ImageUrl??"",
                    c.BankAccount?.HolderName??(!string.IsNullOrEmpty(c.Bankname)?c.Bankname:string.Empty),
                    c.Chequeno,
                    c.PaymentRemark,
                    (c.UpdatedDate).ToString(),
                    c.TxnId,
                    c.Comment,
                    c.User1?.UserProfile?.FullName??string.Empty,
                    (c.IsCreditClear??false)?"Yes":"No"

                    })
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JObject VirtualBankReportSuccessSum()
        {
            JObject jObject = new JObject();
            jObject = JObject.FromObject(new
            {
                SuccessSum = TempData["totelamt"]
            });
            TempData["SuccessSum"] = null;
            return jObject;
        }

        [HttpGet]
        public ActionResult NewWalletRequest(int? u, int? u2, int? t, int? a, int? s, string f = "", string e = "", string cq = "", string r = "", string c = "")
        {

            UpdateActivity("User WalletRequest ", "Get:Wallet/WalletRequest");
            ViewBag.actionAllowed = action = ActionAllowed("WalletRequest", CurrentUser.RoleId);

            var filter = new WalletRequestFilterDto
            {
                UserId = u ?? 0,
                UpdatedById = u2 ?? 0,
                TrTypeId = t ?? 0,
                AccountId = a ?? 0,
                Sdate = f,
                SdateNow = !string.IsNullOrEmpty(f) ? f : DateTime.Now.ToString("dd/MM/yyy"),
                Edate = e,
                EdateNow = !string.IsNullOrEmpty(e) ? e : DateTime.Now.ToString("dd/MM/yyy"),
                ChequeNo = cq,
                Remark = r,
                Comment = c,
                StatusId = s ?? 0
            };

            ViewBag.FilterData = filter;
            ViewBag.UserList = walletService.GetWalletUserList(3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = (x.UserProfile?.FullName ?? string.Empty) + "- " + x.Username, Selected = (x.Id == filter.UserId) }).ToList();
            ViewBag.UserList2 = userService.GetUserList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = (x.UserProfile?.FullName ?? string.Empty) + "- " + x.Username, Selected = (x.Id == filter.UpdatedById) }).ToList();
            ViewBag.TransferTypes = bankAccountService.GetTransferTypeList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == filter.TrTypeId) }).ToList();
            ViewBag.BankAccounts = bankAccountService.GetBankAccounts(true).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.HolderName, Selected = (x.Id == filter.AccountId) }).ToList();
            ViewBag.StatusList = rechargeReportService.GetStatusList().Where(x => x.Remark.Contains("WalletRequest")).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.TypeName, Selected = ((x.Id == filter.StatusId)) }).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult GetNewWalletRequests(DataTableServerSide model)
        {
            ViewBag.actionAllowed = this.action = ActionAllowed("WalletRequest", CurrentUser.RoleId);

            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? (model.filterdata?.UserId ?? 0) : CurrentUser.UserID;


            string action = IsAdminRole ? "approverequest" : "CreateEditRequest";
            string modelid = IsAdminRole ? "modal-approve-wallet-request" : "modal-createedit-wallet-request";

            KeyValuePair<int, List<WalletRequest>> walletRequest = walletService.GetWalletRequests(model, uid);

            return Json(new
            {
                draw = model.draw,
                recordsTotal = walletRequest.Key,
                recordsFiltered = walletRequest.Key,
                data = walletRequest.Value.Select(c => new List<object> {
                     (this.action.AllowEdit && c.StatusId==5 && IsAdminRole?(
                      DataTableButton.EditButton(Url.Action( "CreateEditRequest" , "wallet",new { id = c.Id }),"modal-createedit-wallet-request")
                        +"&nbsp;"+DataTableButton.ValidateButton(Url.Action( "approverequest" , "wallet", new { id = c.Id }), "modal-approve-wallet-request","Approve")
                     +"&nbsp;"+DataTableButton.DeleteButton(Url.Action( "CancelRequest" , "wallet", new { id = c.Id }),"modal-cancel-wallet-request","Cancel")
                     ):"<b style='color:green;'>Processed!</b>" ),
                    c.Id,
                    (c.AddedDate).ToString(),
                     (c.PaymentDate)?.ToString("MM/dd/yyyy"),
                    c.User2?.UserProfile?.FullName,
                    c.Amount,
                     c.TransferType?.Name??string.Empty,
                    c.TxnType?.TypeName,
                     c.AmtType?.AmtTypeName,
                     getColorLabel(c.StatusId, c.StatusType.TypeName),
                    // c.ImagePath,
                    c.BankAccount?.HolderName??(!string.IsNullOrEmpty(c.Bankname)?c.Bankname:string.Empty),
                    c.Chequeno,
                    c.PaymentRemark,
                    (c.UpdatedDate).ToString(),
                    c.TxnId,
                    c.Comment,
                    c.User1?.UserProfile?.FullName??string.Empty,
                    (c.IsCreditClear??false)?"Yes":"No"

                    })
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateEditRequest(long? id)
        {
            UpdateActivity("User WalletRequest ", "Get:Wallet/CreateEditRequest");
            ViewBag.actionAllowed = action = ActionAllowed("WalletRequest", CurrentUser.RoleId, id.HasValue ? 3 : 2);

            WalletRequestDto model = new WalletRequestDto();

            model.TrTypeList = bankAccountService.GetTransferTypeList().Select(x => new TransferTypeDto()
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            model.BankAccountList = bankAccountService.GetBankAccounts(true).Select(x => new BankAccountDto() { Id = x.Id, HolderName = x.HolderName }).ToList();

            try
            {
                DateTime paydate = DateTime.ParseExact(!string.IsNullOrEmpty(model.PaymentDate) ? model.PaymentDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                WalletRequest walletRequest = id.HasValue ? walletService.GetWalletRequest(id ?? 0) : new WalletRequest();

                if (id.HasValue && id.Value > 0)
                {

                    if (walletRequest.StatusId == 5)
                    {
                        model.UserId = walletRequest.UserId ?? 0;
                        model.Amount = walletRequest.Amount ?? 0;
                        model.StatusId = walletRequest.StatusId ?? 0;
                        model.Chequeno = walletRequest.Chequeno;
                        model.PaymentRemark = walletRequest.PaymentRemark;
                        model.BankAccountId = walletRequest.BankAccountId ?? 0;
                        model.TrTypeId = walletRequest.TrTypeId ?? 0;
                        model.PaymentDate = walletRequest.PaymentDate?.ToString("dd/MM/yyyy");
                        model.Bankname = walletRequest.BankAccount?.HolderName ?? string.Empty;
                        model.IsClearCheck = true;

                    }
                    else
                    {
                        ShowErrorMessage("Error!", "Wallet Request Cann't be updated in this stage!", false);
                        return RedirectToAction("WalletRequest");
                    }


                }
                else
                {
                    User user = userService.GetUser(CurrentUser.UserID);
                    if (user.CreditBal != 0 && user.CreditBal != null)
                    {
                        model.IsClearCheck = true;
                    }
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }


            return PartialView("_CreateEditRequest", model);

        }

        [HttpPost]
        public ActionResult CreateEditRequest(WalletRequestDto model, FormCollection FC)
        {
            UpdateActivity("User WalletRequest ", "Post:Wallet/CreateEditRequest");
            ViewBag.actionAllowed = action = ActionAllowed("WalletRequest", CurrentUser.RoleId, model.Id > 0 ? 3 : 2);

            string message = string.Empty;

            try
            {
                WalletRequest walletRequest = model.Id > 0 ? walletService.GetWalletRequest(model.Id) : new WalletRequest();
                model.StatusId = model.Id == 0 ? Convert.ToByte(5) : walletRequest.StatusId ?? 5;
                model.Amount = model.Amount ?? 0;

                if (model.Id == 0 && walletService.IsChequeNoExists(model.Chequeno.Trim()))
                {
                    ShowErrorMessage("Error!", "Duplicate cheque/ref number!", false);

                }
                else if (model.Amount <= 0 || model.Amount > 10000000)
                {
                    ShowErrorMessage("Error!", "Invalid Amount. Allowed Range is 1 to 10000000.", false);
                }
                else if (model.TrTypeId <= 0 || string.IsNullOrEmpty(model.Chequeno) || model.BankAccountId <= 0)
                {
                    ShowErrorMessage("Error!", "All Fields are required. Please check (Transfer Type, Bank, Cheque/RefNo) and try again.", false);
                }
                else if (model.IsClearCredit && (model.BankAccountId == 2 || model.TrTypeId == 2))
                {
                    ShowErrorMessage("Error!", "Invalid BankAccount/TransferType for credit clear request.", false);
                }

                else if (model.StatusId == 5)
                {

                    DateTime paydate = DateTime.ParseExact(!string.IsNullOrEmpty(model.PaymentDate) ? model.PaymentDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    string filename = null;
                    string response = "";
                    walletRequest.UserId = model.Id > 0 ? walletRequest.UserId : CurrentUser.UserID;
                    walletRequest.Amount = model.Amount ?? 0;
                    walletRequest.StatusId = model.StatusId;
                    try
                    {
                        walletRequest.Chequeno = model.Chequeno.Trim();
                    }
                    catch (Exception)
                    {
                        walletRequest.Chequeno = model.Chequeno;
                    }
                    walletRequest.PaymentRemark = model.PaymentRemark;
                    walletRequest.TxnTypeId = 4;
                    walletRequest.AmtTypeId = 10;
                    walletRequest.BankAccountId = model.BankAccountId;
                    walletRequest.TrTypeId = model.TrTypeId;
                    walletRequest.PaymentDate = paydate;
                    walletRequest.IsCreditClear = model.IsClearCredit;
                    if (model.FileAttach != null)
                    {
                        FileUpdoad(model.FileAttach, ref filename, ref response);
                        walletRequest.ImageUrl = filename ?? walletRequest.ImageUrl;
                    }
                    if (model.Id == 0)
                        walletRequest.AddedById = CurrentUser.UserID;
                    else
                        walletRequest.UpdatedById = CurrentUser.UserID;

                    walletService.Save(walletRequest);

                    try
                    {
                        if (model.Id == 0)
                        {
                            var ret = WalletRefrenceNO(walletRequest.Amount.ToString(), walletRequest.Chequeno, SiteKey.SMSDBWalletUserId, walletRequest.UserId ?? 0, (byte)walletRequest.StatusId);
                            //var ret = WalletRefrenceNO(walletRequest.Amount.ToString(), walletRequest.Chequeno, SiteKey.SMSDBWalletUserId);
                            if (ret == 0)
                            {
                                WalletRequestDto wrmodel = new WalletRequestDto();
                                wrmodel.Id = walletRequest.Id;
                                wrmodel.Comment = "Auto Approved SMS";
                                wrmodel.AddedById = 1;
                                AutoApproveWalletRequestSMS(wrmodel);
                                ApproveAllPendingRequests(1);
                            }
                            else
                            {
                                //  ApproveAllPendingRequests(1);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogException(ex, "auto update wallet smsdb wrmodel.Id=" + walletRequest.Id + ",cheqref=" + walletRequest.Chequeno);
                    }

                    ShowSuccessMessage("Success!", "Wallet Request Updated!", false);
                }
                else
                {
                    ShowErrorMessage("Error!", "Wallet Request Cann't be updated in this stage!", false);
                    return RedirectToAction("WalletRequest");
                }
            }
            catch (Exception Ex)
            {
                message = "Internal Server Error!";
                ShowErrorMessage("Error!", message, false);
                LogException(Ex);
            }
            return RedirectToAction("WalletRequest");
        }
        public void FileUpdoad(HttpPostedFileBase file, ref string _FileName, ref string Response)
        {
            string ext = System.IO.Path.GetExtension(file.FileName);
            int maxlength = 1024 * 512 * 1; // 1 MB;
            if (ext.ToUpper().Trim() == ".jpg" || ext.ToUpper().Trim() == ".png" || ext.ToUpper().Trim() == ".jpeg")
            {
                Response = "Please choose only .jpg, .png ,.jpeg image types!";
            }
            else
            {
                if (file != null && file.ContentLength > 0)
                {
                    byte[] upload = new byte[file.ContentLength];
                    file.InputStream.Read(upload, 0, file.ContentLength);
                    string Name = DateTime.Now.ToString("yyyyMMddHHmmss");
                    _FileName = Name + Path.GetFileName(file.FileName);
                    var _path = Path.Combine(Server.MapPath("~/img/product/"), _FileName);
                    Stream strm = file.InputStream;
                    //Compressimage(strm, _path, _FileName);

                    file.SaveAs(_path);

                    _FileName = "/img/product/" + _FileName;
                }
                Response = "FileUpload Successfull";
            }
        }

        public ActionResult ApproveRequest(long? id)
        {
            UpdateActivity("ApproveRequest ", "Get:Wallet/ApproveRequest");
            ViewBag.actionAllowed = action = ActionAllowed("WalletRequest", CurrentUser.RoleId, id.HasValue ? 3 : 2);

            WalletRequestDto model = new WalletRequestDto();


            try
            {
                WalletRequest walletRequest = id.HasValue ? walletService.GetWalletRequest(id ?? 0) : new WalletRequest();

                if (id.HasValue && id.Value > 0)
                {

                    if (walletRequest.StatusId == 5 || walletRequest.StatusId == 7)
                    {
                        model.Id = walletRequest.Id;
                        model.IsClearCredit = walletRequest.IsCreditClear ?? false;

                    }
                    else
                    {
                        ShowErrorMessage("Error!", "Wallet Request Cann't be updated in this stage!", false);
                        return RedirectToAction("WalletRequest");
                    }

                }

            }
            catch (Exception e)
            {
                LogException(e);
            }


            return PartialView("_ApproveRequest", model);

        }

        [HttpPost]
        public ActionResult ApproveRequest(WalletRequestDto model, FormCollection FC)
        {
            string log = "ApproveRequest start";
            UpdateActivity("ApproveRequest ", "Post:Wallet/ApproveRequest");
            ViewBag.actionAllowed = action = ActionAllowed("WalletRequest", CurrentUser.RoleId, model.Id > 0 ? 3 : 2);
            string message = string.Empty;
            string error = string.Empty;
            try
            {
                WalletRequest walletRequest = model.Id > 0 ? walletService.GetWalletRequest(model.Id) : new WalletRequest();
                model.StatusId = model.Id == 0 ? Convert.ToByte(5) : model.StatusId;
                walletRequest.IsCreditClear = walletRequest.IsCreditClear ?? false;
                if (walletRequest.StatusId == 5)
                {
                    using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
                    {
                        SqlCommand cmd = new SqlCommand("sp_AddUserWallet", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IsPullOut", "Approve");
                        cmd.Parameters.AddWithValue("@UserId", walletRequest.UserId);
                        cmd.Parameters.AddWithValue("@Amount", walletRequest.Amount);
                        cmd.Parameters.AddWithValue("@Remark", model.Comment);
                        cmd.Parameters.AddWithValue("@AddedById", CurrentUser.UserID);
                        cmd.Parameters.AddWithValue("@IsCreditClear", walletRequest.IsCreditClear == true ? "Yes" : "No");
                        cmd.Parameters.AddWithValue("@TrTypeId", walletRequest.TrTypeId);
                        cmd.Parameters.AddWithValue("@BankAccountId", walletRequest.BankAccountId);
                        cmd.Parameters.AddWithValue("@ChequeNo", walletRequest.Chequeno);
                        cmd.Parameters.AddWithValue("@WRID", walletRequest.Id);
                        cmd.Parameters.Add("@error", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@Log", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        error = Convert.ToString(cmd.Parameters["@error"].Value);
                        var splog = cmd.Parameters["@Log"].Value;
                        log += "\r\n, splog=" + splog;
                    }

                    if (error == "0")
                    {
                        string VendorWalletRelation = string.Empty;
                        if (walletRequest.TrTypeId == 7)
                        {
                            int UserID = walletRequest.UserId ?? 0;
                            if (UserID > 0)
                            {
                                User user = userService.GetUser(UserID);
                                if (user.VendorId != null)
                                {
                                    using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
                                    {
                                        //SqlCommand cmd = new SqlCommand("sp_AddApiWallet", con);
                                        SqlCommand cmd = new SqlCommand("sp_AddUserApiWalletExchange", con);
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.AddWithValue("@ApiId", user.VendorId);
                                        cmd.Parameters.AddWithValue("@Ins_Amt", 0);
                                        cmd.Parameters.AddWithValue("@AmountSent", walletRequest.Amount);
                                        cmd.Parameters.AddWithValue("@AmountReceived", walletRequest.Amount);
                                        cmd.Parameters.AddWithValue("@Remark", model.Comment);
                                        cmd.Parameters.AddWithValue("@AddedById", CurrentUser.UserID);
                                        cmd.Parameters.AddWithValue("@BankAccountId", walletRequest.BankAccountId);
                                        cmd.Parameters.AddWithValue("@TrTypeId", walletRequest.TrTypeId);
                                        cmd.Parameters.AddWithValue("@ChequeNo", walletRequest.Chequeno + "_AE");
                                        // cmd.Parameters.AddWithValue("@IsPullOut", model.IsPullOut ? "Yes" : "No");
                                        cmd.Parameters.AddWithValue("@IsPullOut", "No");
                                        cmd.Parameters.Add("@error", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                                        con.Open();
                                        cmd.ExecuteNonQuery();
                                        con.Close();
                                        error = Convert.ToString(cmd.Parameters["@error"].Value);
                                    }
                                    if (error == "0")
                                    {  //model.IsDirect = model.IsDirect ? true : false;
                                        ShowSuccessMessage("Success!", "Wallet has been updated", false);
                                    }
                                    else if (error == "2")
                                    {
                                        //model.IsDirect = false;
                                        ShowErrorMessage("Error!", "Duplicate Cheque/Ref number!", false);
                                    }
                                    else
                                    {
                                        //model.IsDirect = false;
                                        ShowErrorMessage("OOPS!", "Something Went Wrong!", false);
                                    }
                                }
                            }
                            else
                            {
                                VendorWalletRelation = "No Vendor Relation with User";
                            }
                        }
                        ShowSuccessMessage("Success!", "Wallet has been updated (" + VendorWalletRelation + ")", false);
                    }
                    else if (error == "2")
                        ShowErrorMessage("Error!", "Duplicate Cheque/Ref number!", false);
                    else if (error == "3")
                        ShowErrorMessage("Error!", "Already Approved!", false);
                    else if (error == "4")
                        ShowErrorMessage("Error!", "Duplicate Txn of same wallet request!", false);
                    else if (error == "5")
                        ShowErrorMessage("Error!", "User Credit Limit Low Please Contact to Admin !", false);
                    else
                        ShowErrorMessage("OOPS!", "Something Went Wrong!", false);
                }
                else
                {
                    log += "\r\n, already status=" + walletRequest.StatusId;
                    ShowErrorMessage("Error!", "Wallet Request Cann't be updated in this stage!", false);
                    return RedirectToAction("WalletRequest");
                }

                ApproveAllPendingRequests(CurrentUser.UserID);
                if (error == "5")
                {
                    ShowErrorMessage("Error!", "User Credit Limit Low Please Contact to Admin !", false);
                }
                else
                {
                    ShowSuccessMessage("Success!", "Wallet Request Updated!", false);
                }

            }
            catch (Exception Ex)
            {
                message = "Internal Server Error!";
                ShowErrorMessage("Error!", message, false);
                log += "\r\n, excp=" + Ex.Message;
                LogException(Ex);
            }

            log += "End";
            LogActivity(log);
            return RedirectToAction("WalletRequest");
        }


        public ActionResult ViewImage(long? id)
        {
            WalletRequestDto model = new WalletRequestDto();

            try
            {
                WalletRequest walletRequest = id.HasValue ? walletService.GetWalletRequest(id ?? 0) : new WalletRequest();

                if (id.HasValue && id.Value > 0)
                {

                    if (walletRequest.StatusId == 5)
                    {
                        model.ImageUrl = walletRequest.ImageUrl ?? "/Images/NoImage.png";

                    }
                    else
                    {
                        ShowErrorMessage("Error!", "Wallet Request Cann't be updated in this stage!", false);
                        return RedirectToAction("WalletRequest");
                    }

                }

            }
            catch (Exception e)
            {
                LogException(e);
            }


            return PartialView("_ViewImageRequest", model);

        }

        public ActionResult CancelRequest(long? id)
        {
            UpdateActivity("CancelRequest ", "get:Wallet/CancelRequest");
            ViewBag.actionAllowed = action = ActionAllowed("WalletRequest", CurrentUser.RoleId, 4);

            WalletRequestDto model = new WalletRequestDto();

            try
            {
                WalletRequest walletRequest = id.HasValue ? walletService.GetWalletRequest(id ?? 0) : new WalletRequest();

                if (id.HasValue && id.Value > 0)
                {

                    if (walletRequest.StatusId == 5)
                    {
                        model.Id = walletRequest.Id;

                    }
                    else
                    {
                        ShowErrorMessage("Error!", "Wallet Request Cann't be updated in this stage!", false);
                        return RedirectToAction("WalletRequest");
                    }

                }

            }
            catch (Exception e)
            {
                LogException(e);
            }


            return PartialView("_CancelRequest", model);

        }

        [HttpPost]
        public ActionResult CancelRequest(WalletRequestDto model, FormCollection FC)
        {
            UpdateActivity("CancelRequest ", "Post:Wallet/CancelRequest");
            ViewBag.actionAllowed = action = ActionAllowed("WalletRequest", CurrentUser.RoleId, 4);

            string message = string.Empty;

            try
            {
                WalletRequest walletRequest = model.Id > 0 ? walletService.GetWalletRequest(model.Id) : new WalletRequest();
                model.StatusId = model.Id == 0 ? Convert.ToByte(5) : model.StatusId;

                if (walletRequest.StatusId == 5)
                {
                    walletRequest.StatusId = 7;
                    walletRequest.UpdatedById = CurrentUser.UserID;
                    walletRequest.UpdatedDate = DateTime.Now;
                    walletService.Save(walletRequest);
                }
                else
                {
                    ShowErrorMessage("Error!", "Wallet Request Cann't be updated in this stage!", false);
                    return RedirectToAction("WalletRequest");
                }

                ShowSuccessMessage("Success!", "Wallet Request Updated!", false);

            }
            catch (Exception Ex)
            {
                message = "Internal Server Error!";
                ShowErrorMessage("Error!", message, false);

                LogException(Ex);
            }

            return RedirectToAction("WalletRequest");
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

        private string getColorLabel(int? id, string displayText)
        {
            string color = id == 6 ? "green" : id == 5 ? "blue" : id == 7 ? "red" : "";


            return "<b style='color:" + color + "'>" + displayText + "</b>";
        }

        private int WalletRefrenceNO(string amount, string txnrefno, string WalletUserID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SMSDBConn))
                {
                    using (SqlCommand cmd = new SqlCommand("AutoWalletMessageread", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@amount", amount);
                        cmd.Parameters.AddWithValue("@refid", txnrefno.Trim());
                        cmd.Parameters.AddWithValue("@WalletUserID", WalletUserID);
                        cmd.Parameters.Add("@result", SqlDbType.Int);
                        cmd.Parameters["@result"].Direction = ParameterDirection.Output;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        int error = Convert.ToInt32(cmd.Parameters["@result"].Value);
                        return error;
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "Auto read walllet SMS txnrefno=" + txnrefno + ", WalletUserID" + WalletUserID + ", amount=" + amount);
                return 5;
            }
        }
        private int WalletRefrenceNO(string amount, string txnrefno, string WalletUserID, int CurrentUser, int Status)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(WalletUserID))
                {

                    RequestResponseDto requestResponseDto = new RequestResponseDto();
                    requestResponseDto.Remark = "Wallet Request Check";
                    Web.LIBS.ApiCall api = new LIBS.ApiCall(reqResService);
                    int UserId = 0;
                    try
                    {
                        string log = " (" + DateTime.Now.TimeOfDay.ToString() + ")Wallet start-,WalletBankRefno=" + txnrefno + " Amount=" + amount;

                        string Url = "http://local.ezytm.com/api/mobile/WalletFetch?UserId=" + WalletUserID + "&BankRefNo=" + txnrefno + "&Amount=" + amount;
                        requestResponseDto.RequestTxt = Url;

                        string response = api.Get(Url, ref requestResponseDto);
                        requestResponseDto.ResponseText = "Old Value : " + Status + "New Value : " + response;// "recharge-statusId=" + rcstatusid + ", callback statusId=" + statusId;
                        if (CurrentUser > 0)
                        {
                            requestResponseDto.UserId = CurrentUser;
                        }
                        //AddUpdateReqRes(requestResponseDto, ref log);
                        UserId = Convert.ToInt32(response);
                    }
                    catch (Exception ex)
                    {
                        LogException(ex, "Auto read response walllet SMS txnrefno=" + txnrefno + ", WalletUserID" + WalletUserID + ", amount=" + amount);
                        UserId = 5;
                    }
                    return UserId;

                }
                else
                {
                    return 3;
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "Auto read walllet SMS txnrefno=" + txnrefno + ", WalletUserID" + WalletUserID + ", amount=" + amount);
                return 5;
            }
        }

        private void AutoApproveWalletRequestSMS(WalletRequestDto model)
        {
            string log = "AutoApproveRequest start";
            string message = string.Empty;
            string error = string.Empty;
            try
            {
                WalletRequest walletRequest = model.Id > 0 ? walletService.GetWalletRequest(model.Id) : new WalletRequest();
                model.StatusId = model.Id == 0 ? Convert.ToByte(5) : model.StatusId;
                if (walletRequest.StatusId == 5)
                {
                    using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
                    {
                        SqlCommand cmd = new SqlCommand("sp_AddUserWallet", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IsPullOut", "Approve");
                        cmd.Parameters.AddWithValue("@UserId", walletRequest.UserId);
                        cmd.Parameters.AddWithValue("@Amount", walletRequest.Amount);
                        cmd.Parameters.AddWithValue("@Remark", model.Comment);
                        cmd.Parameters.AddWithValue("@AddedById", model.AddedById);
                        cmd.Parameters.AddWithValue("@IsCreditClear", model.IsClearCredit ? "Yes" : "No");
                        cmd.Parameters.AddWithValue("@TrTypeId", walletRequest.TrTypeId);
                        cmd.Parameters.AddWithValue("@BankAccountId", walletRequest.BankAccountId);
                        cmd.Parameters.AddWithValue("@ChequeNo", walletRequest.Chequeno);
                        cmd.Parameters.AddWithValue("@WRID", walletRequest.Id);
                        cmd.Parameters.Add("@error", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("@Log", SqlDbType.VarChar, 250).Direction = ParameterDirection.Output;
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        error = Convert.ToString(cmd.Parameters["@error"].Value);
                        var splog = cmd.Parameters["@Log"].Value;
                        log += "\r\n, splog=" + splog;
                    }
                }
                else
                {
                    log += "\r\n, already status=" + walletRequest.StatusId;
                }
            }
            catch (Exception Ex)
            {
                message = "Internal Server Error!";
                ShowErrorMessage("Error!", message, false);
                log += "\r\n, excp=" + Ex.Message;
                LogException(Ex);
            }
            log += "AutowalletUpdate-End";
            LogActivity(log);
        }

        private void ApproveAllPendingRequests(int userid)
        {
            try
            {
                var reqList = walletService.GetWalletRequestList(5);

                foreach (var walletRequest in reqList)
                {
                    var ret = WalletRefrenceNO(walletRequest.Amount.ToString(), walletRequest.Chequeno, SiteKey.SMSDBWalletUserId, walletRequest.UserId ?? 0, (byte)walletRequest.StatusId);

                    //var ret = WalletRefrenceNO(walletRequest.Amount.ToString(), walletRequest.Chequeno, SiteKey.SMSDBWalletUserId);
                    if (ret == 0)
                    {
                        WalletRequestDto wrmodel = new WalletRequestDto();
                        wrmodel.Id = walletRequest.Id;
                        wrmodel.Comment = "Auto Approved SMS";
                        wrmodel.AddedById = userid;
                        AutoApproveWalletRequestSMS(wrmodel);
                    }
                }

            }
            catch (Exception ex)
            {
                LogException(ex, "ApproveAllPendingRequests");
            }

        }

        [HttpPost]
        public string NumberToWords(decimal? numbers)
        {
            string words = LIBS.Common.NumberToWords(numbers);
            if (words.StartsWith("and"))
            {
                words = words.Replace("and ", "");
            }
            return words;
        }


        public JsonResult WExchange(string f = "", string e = "")
        {
            UpdateActivity("User WalletRequest ", "Get:Wallet/WalletExchange");
            ViewBag.actionAllowed = action = ActionAllowed("WalletExchange", CurrentUser.RoleId);
            JObject response = new JObject();
            try
            {
                int roleid = CurrentUser.Roles.FirstOrDefault();
                //string sdate = "", edate = "";
                //sdate = f != "" ? f : DateTime.Now.ToString("MM/dd/yyyy");
                //edate = e != "" ? e : DateTime.Now.ToString("MM/dd/yyyy");
                DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(f) ? f : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(e) ? e : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);

                ViewBag.StartDate = fdate;
                ViewBag.EndDate = tdate;
                bool isAdmin = roleid == 1 || roleid == 2 ? true : false;
                int userid = isAdmin ? 0 : CurrentUser.UserID;
                DataSet ds = new DataSet();
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("[usp_WalletExchageReport]", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", fdate);
                    cmd.Parameters.AddWithValue("@edate", tdate);
                    SqlDataAdapter ad1 = new SqlDataAdapter();
                    ad1.SelectCommand = cmd;
                    ad1.Fill(ds);
                    var json1 = DataTableToJSONWithJavaScriptSerializer(ds.Tables[0]);//User Exchange Report
                    var json2 = DataTableToJSONWithJavaScriptSerializer(ds.Tables[1]);//Vendor Exchange Report
                    return Json(new
                    {
                        UserExchangeReport = json1,
                        VendorExchangeReport = json2
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
                return null;
            }

        }

        public string DataTableToJSONWithJavaScriptSerializer(DataTable table)
        {
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.DateFormatString = "MM/dd/yyy hh:mm:ss";

            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    if (col.ColumnName == "AddedDate")
                    {
                        childRow.Add(col.ColumnName, JsonConvert.SerializeObject(row[col], jsonSettings));
                    }
                    else
                    {
                        childRow.Add(col.ColumnName, row[col]);
                    }

                }
                parentRow.Add(childRow);
            }
            return jsSerializer.Serialize(parentRow);
        }
        private void Connection()
        {
            string constr = ConfigurationManager.ConnectionStrings["sqlconn"].ConnectionString;
            con = new SqlConnection(constr);
        }

        public DataTable InsertIntoExcelVerify(int UserId, string CustomerNo = "", string AccountNo = "", string IFSC = "", string Name = "", decimal Amount = 0, string Type = "", string RtStatus = "", string Unkey = "", string Firstname = "", string Lastname = "")
        {
            try
            {
                Connection();
                using (SqlCommand cmd = new SqlCommand("Sp_InserdataExcelbillPayVerify", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", UserId);
                    cmd.Parameters.AddWithValue("@CustomerNo", CustomerNo);
                    cmd.Parameters.AddWithValue("@AccountNo", AccountNo);
                    cmd.Parameters.AddWithValue("@IFSC", IFSC);
                    cmd.Parameters.AddWithValue("@Name", Name);
                    cmd.Parameters.AddWithValue("@Amount", Amount);
                    cmd.Parameters.AddWithValue("@Type", Type);
                    cmd.Parameters.AddWithValue("@RtStatus", RtStatus);
                    cmd.Parameters.AddWithValue("@UniqueIdkey", Unkey);
                    cmd.Parameters.AddWithValue("@Firstname", Firstname);
                    cmd.Parameters.AddWithValue("@Lastname", Lastname);
                    con.Open();
                    //SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable ds = new DataTable();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    return ds;
                }
            }

            catch (Exception ex)
            {
                return null;
            }
        }

        public DataTable GetExcelVerify(string Unkey = "")
        {
            try
            {
                Connection();
                using (SqlCommand cmd = new SqlCommand("Sp_GetPayoutPayVerify", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UniqueIdkey", Unkey);
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable ds = new DataTable();
                    da.Fill(ds);
                    con.Close();
                    return ds;
                }
            }

            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet]
        public ActionResult WalletExchange(int? i, int? t, string f = "", string e = "")
        {
            UpdateActivity("User WalletRequest ", "Get:Wallet/WalletExchange");
            ViewBag.actionAllowed = action = ActionAllowed("WalletExchange", CurrentUser.RoleId);
            var filter = new WalletRequestFilterDto
            {
                IsActive = Convert.ToInt32(i.HasValue ? i : 0),
                TrTypeId = t ?? 7,
                Sdate = f,
                SdateNow = !string.IsNullOrEmpty(f) ? f : DateTime.Now.ToString("dd/MM/yyy"),
                Edate = e,
                EdateNow = !string.IsNullOrEmpty(e) ? e : DateTime.Now.ToString("dd/MM/yyy"),
            };
            ViewBag.FilterData = filter;
            return View();
        }

        //[HttpPost]
        //public ActionResult GetWalletExchange(DataTableServerSide model)
        //{
        //    //ViewBag.actionAllowed = this.action = ActionAllowed("WalletRequest", CurrentUser.RoleId);

        //    int userrole = CurrentUser.Roles.FirstOrDefault();
        //    bool IsAdminRole = (userrole != 3) ? true : false;
        //    int uid = IsAdminRole ? (model.filterdata?.UserId ?? 0) : CurrentUser.UserID;
        //    //string action = IsAdminRole ? "approverequest" : "CreateEditRequest";
        //    //string modelid = IsAdminRole ? "modal-approve-wallet-request" : "modal-createedit-wallet-request";
        //    KeyValuePair<int, List<WalletRequest>> walletRequest = walletService.GetWalletExchangeReport(model, uid);
        //    return Json(new
        //    {
        //        draw = model.draw,
        //        recordsTotal = walletRequest.Key,
        //        recordsFiltered = walletRequest.Key,
        //        data = walletRequest.Value.Select(c => new List<object> {
        //            c.User2?.UserProfile?.FullName,
        //            c.Amount,
        //             (c.UpdatedDate).ToString()??null,
        //              c.PaymentRemark,
        //            c.User1?.UserProfile?.FullName??string.Empty,
        //             c.TransferType?.Name??string.Empty,
        //            })
        //    }, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public JsonResult BankListData(int typeid)
        {
            if (typeid == 2)
            {
                List<BankAccount> user = bankAccountService.GetBankAccounts(true).Where(x => x.HolderName.Contains("Credit")).ToList();
                var subCategoryToReturn = user.Select(x => new UserDto()
                {
                    Id = x.Id,
                    Username = (x.HolderName ?? string.Empty)
                }).ToList();
                //var json = new JavaScriptSerializer().Serialize(user);
                return Json(subCategoryToReturn, JsonRequestBehavior.AllowGet);
            }
            else
            {
                List<BankAccount> user = bankAccountService.GetBankAccounts(true).Where(x => !x.HolderName.Contains("Credit")).ToList();
                var subCategoryToReturn = user.Select(x => new UserDto()
                {
                    Id = x.Id,
                    Username = (x.HolderName ?? string.Empty)
                }).ToList();
                //var json = new JavaScriptSerializer().Serialize(user);
                return Json(subCategoryToReturn, JsonRequestBehavior.AllowGet);
            }

        }

    }
}