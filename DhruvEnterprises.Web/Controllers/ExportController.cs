using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Repo;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.LIBS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Controllers
{
    public class ExportController : BaseController
    {
        #region "Fields"

        private readonly IRechargeReportService rechargeReportService;
        //private readonly IDMTService dmtService;

        //private IRepository<GatWayTxn> repoGateWayTxn;
        private IRepository<DMT> repoDMT;

        private readonly IRoleService roleService;
        private readonly IRechargeService rechargeService;
        private readonly IApiWalletService apiWalletService;
        private readonly IWalletService walletService;
        private readonly IUserService userService;
        private readonly IApiService apiService;
        private readonly IRequestResponseService reqResService;
        private readonly IBankAccountService bankAccountService;
        private readonly IActivityLogService activityLogService;

        ActivityLogDto activityLogModel;
        public ActionAllowedDto action;
        #endregion

        #region "Constructor"
        public ExportController(IRechargeReportService _rechargeReportService,
                                        IRoleService _userroleService,
                                        IActivityLogService _activityLogService,
                                        IRechargeService _rechargeService,
                                        IApiWalletService _apiWalletService,
                                        IWalletService _walletService,
                                         IUserService _userService,
                                         IApiService _apiService,
                                         IRequestResponseService _reqResService,
                                         IBankAccountService _bankAccountService,
                                          IRepository<DMT> _repoDMT
                                         //IRepository<GatWayTxn> _repoGateWayTxn
            ) : base(_activityLogService, _userroleService)
            
        {

            this.rechargeReportService = _rechargeReportService;
            this.roleService = _userroleService;
            this.rechargeService = _rechargeService;
            this.walletService = _walletService;
            this.apiWalletService = _apiWalletService;
            this.userService = _userService;
            this.apiService = _apiService;
            this.reqResService = _reqResService;
            this.bankAccountService = _bankAccountService;
            this.activityLogService = _activityLogService;
            this.activityLogModel = new ActivityLogDto();
            //this.repoGateWayTxn = _repoGateWayTxn;
            this.repoDMT = _repoDMT;
            this.action = new ActionAllowedDto();

        }
        #endregion

        // GET: Export
        public ActionResult Index()
        {
            UpdateActivity("Export REQUEST", "GET:Export/Index/");
            ViewBag.actionAllowed = action = ActionAllowed("Export", CurrentUser.RoleId);

            ExportDto exportDto = new ExportDto();

            var rptType = new List<ExportTypeDto>()
            {
                new ExportTypeDto() { TypeId = 1, TypeName = "Recharge Report" }
                ,new ExportTypeDto() { TypeId = 2, TypeName = "User TXN Report" }
                ,new ExportTypeDto() { TypeId = 3, TypeName = "Request Response" }
                ,new ExportTypeDto() { TypeId = 4, TypeName = "Bank Statement" }
                ,new ExportTypeDto() { TypeId = 5, TypeName = "Activity Log" }
                ,new ExportTypeDto() { TypeId = 6, TypeName = "Complaint Report" }
                ,new ExportTypeDto() { TypeId = 7, TypeName = "New Recharge Report" }
                ,new ExportTypeDto() { TypeId = 8, TypeName = "Gatwaytxn Report" }
                ,new ExportTypeDto() { TypeId = 9, TypeName = "DMT Report" }
            };

            if (!CurrentUser.IsAdminRole)
            {
                rptType = rptType.Where(x => x.TypeId == 1 || x.TypeId == 2 || x.TypeId == 6 || x.TypeId == 7 || x.TypeId == 8 || x.TypeId ==9).ToList();
            }
            ViewBag.ExTypeList = rptType;

            ViewBag.UserList = userService.GetUserList().Where(x => !(x.IsAdminRole ?? false)).Select(x => new UserDto()
            {
                Uid = x.Id,
                Name = (x.UserProfile.FullName) + " -" + x.Username
            }).ToList();

            ViewBag.ApiList = apiService.GetApiList().Select(x => new ApiDto()
            {
                Id = x.Id,
                ApiName = x.ApiName
            }).ToList();



            return View(exportDto);
        }

        public ActionResult ExportCSV(int? im, int? u2, int? rt, int? u, int? v, int? o, int? s, int? c, string f = "", string m = "", string e = "", string rto = "", string ut = "", string ot = "", string vt = "", string rm = "", string nm = "")
        {
            //rt=1: recharge report
            //rt=2: txnlegder report
            //rt=3: Requestresponse report
            //rt=4 bank statement
            //rt=5 ActiVityLog report
            //rt=6 complaint report
            //rt=7 new recharge report-2

            int rtype = rt ?? 0;
            try
            {
                MemoryStream output = new MemoryStream();
                StreamWriter writer = new StreamWriter(output, Encoding.UTF8);
                var filename = "";

                if (rtype == 1)
                {
                    ExportDto model = new ExportDto();
                    model.TypeId = rt.HasValue ? Convert.ToInt32(rt) : 1;
                    model.UserId = u.HasValue ? Convert.ToInt32(u) : 0;
                    model.ApiId = v.HasValue ? Convert.ToInt32(v) : 0;
                    model.StartDate = f;
                    model.EndDate = e;
                    model.StatusId = s.HasValue ? Convert.ToInt32(s) : 0;
                    model.SearchId = !string.IsNullOrEmpty(rto) ? rto : "0";
                    model.OpId = o.HasValue ? Convert.ToInt32(o) : 0;
                    model.CircleId = c.HasValue ? Convert.ToInt32(c) : 0;

                    model.CustomerNo = !string.IsNullOrEmpty(m) ? m : "0";
                    model.UserRefId = !string.IsNullOrEmpty(ut) ? ut : "0";
                    model.ApiTxnId = !string.IsNullOrEmpty(vt) ? vt : "0";
                    model.OpTxnId = !string.IsNullOrEmpty(ot) ? ot : "0";

                    model.UserId = CurrentUser.IsAdminRole ? model.UserId : CurrentUser.UserID;
                    model.UpdatedById = u2.HasValue ? Convert.ToInt32(u2) : 0;


                    var data = GetRechargeData(model);
                    writer = GetCsvFromModel(data, writer);
                    filename = "RechargeReport_" + DateTime.Now.ToString("ddMMyyyy_hhmmss_tt") + ".csv";

                }
                else if (rtype == 2)
                {
                    TxnFilterDto filter = new TxnFilterDto();
                    filter.recid = !string.IsNullOrEmpty(rto) ? Convert.ToInt64(rto) : 0;
                    filter.txnid = !string.IsNullOrEmpty(ut) ? Convert.ToInt64(ut) : 0;
                    filter.txntypeid = !string.IsNullOrEmpty(ot) ? Convert.ToInt32(ot) : 0;
                    filter.amttypeid = !string.IsNullOrEmpty(vt) ? Convert.ToInt32(vt) : 0;
                    filter.apiid = v ?? 0;
                    filter.userid = u ?? 0;
                    filter.remark = rm != "" ? rm : "";
                    filter.sdate = f;
                    filter.edate = e;

                    string ignorecols = !CurrentUser.IsAdminRole ? "UserId,VendorId,RefTxnId,ApiTxnId,AddedBy,Remark" : "";

                    var data = GetTxnReportData(filter);
                    writer = GetCsvFromModel(data, writer, ignorecols);
                    filename = "TxnReport_" + DateTime.Now.ToString("ddMMyyyy_hhmmss_tt") + ".csv";

                }
                else if (rtype == 3)
                {

                    if (!CurrentUser.IsAdminRole) { throw new Exception("Error! Access Denied"); }

                    ReqResFilterDto model = new ReqResFilterDto();

                    model.UserId = Convert.ToInt32(u.HasValue ? u : 0);
                    model.ApiId = Convert.ToInt32(v.HasValue ? v : 0);
                    model.OpId = Convert.ToInt32(o.HasValue ? o : 0);
                    model.RecId = Convert.ToInt64(!string.IsNullOrEmpty(rto) ? rto : "0");
                    model.RefId = ot;
                    model.StatusId = Convert.ToInt32(s.HasValue ? s : 0);
                    model.Sdate = f;
                    model.SdateNow = !string.IsNullOrEmpty(model.Sdate) ? model.Sdate : DateTime.Now.ToString("dd/MM/yyy");
                    model.Edate = e;
                    model.EdateNow = !string.IsNullOrEmpty(model.EdateNow) ? model.EdateNow : DateTime.Now.ToString("dd/MM/yyy");
                    model.CustomerNo = m;
                    model.UserTxnId = ut;
                    model.Remark = rm;


                    var data = GetReqResData(model);
                    writer = GetCsvFromModel(data, writer);
                    filename = "RequestResponse_" + DateTime.Now.ToString("ddMMyyyy_hhmmss_tt") + ".csv";


                }
                else if (rtype == 4)
                {
                    if (!CurrentUser.IsAdminRole) { throw new Exception("Error! Access Denied"); }

                    BankStatementFilterDto filter = new BankStatementFilterDto();

                    filter.UserId = Convert.ToInt32(u.HasValue ? u : 0);
                    filter.ApiId = Convert.ToInt32(v.HasValue ? v : 0);
                    filter.AccountId = Convert.ToInt32(o.HasValue ? o : 0);
                    filter.RefAccountId = Convert.ToInt32(s.HasValue ? s : 0);
                    filter.TrTypeId = Convert.ToInt32(c.HasValue ? c : 0);
                    filter.TxnTypeId = Convert.ToInt32(m != "" ? m : "0");
                    filter.AmtTypeId = Convert.ToInt32(rto != "" ? rto : "0");
                    filter.StatementId = Convert.ToInt32(ut != "" ? ut : "0");
                    filter.Sdate = f;
                    filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.ToString("dd/MM/yyy");
                    filter.Edate = e;
                    filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("dd/MM/yyy");
                    filter.PaymentRef = vt;
                    filter.Remark = rm;


                    var data = GetStatementData(filter);

                    writer = GetCsvFromModel(data, writer);
                    filename = "BankStatement_" + DateTime.Now.ToString("ddMMyyyy_hhmmss_tt") + ".csv";
                }
                else if (rtype == 5)
                {
                    if (!CurrentUser.IsAdminRole) { throw new Exception("Error! Access Denied"); }

                    ActiVityLogFilterDto filter = new ActiVityLogFilterDto();

                    filter.userid = Convert.ToInt32(u.HasValue ? u : 0);
                    filter.ipaddress = m;
                    filter.actname = rto;
                    filter.url = ut;
                    filter.sdate = f;
                    filter.sdateNow = !string.IsNullOrEmpty(filter.sdate) ? filter.sdate : DateTime.Now.ToString("dd/MM/yyy");
                    filter.edate = e;
                    filter.edateNow = !string.IsNullOrEmpty(filter.edate) ? filter.edate : DateTime.Now.ToString("dd/MM/yyy");
                    filter.remark = rm;


                    var data = GetActivityLogData(filter);

                    writer = GetCsvFromModel(data, writer);
                    filename = "ActivityLog_" + DateTime.Now.ToString("ddMMyyyy_hhmmss_tt") + ".csv";
                }
                else if (rtype == 6)
                {
                    string ignoreColumns = "";

                    if (!CurrentUser.IsAdminRole)
                        ignoreColumns = "Vendor,Comment,OurRefId,VendorTxnId";

                    ExportDto model = new ExportDto();
                    model.TypeId = rt.HasValue ? Convert.ToInt32(rt) : 1;
                    model.UserId = u.HasValue ? Convert.ToInt32(u) : 0;
                    model.ApiId = v.HasValue ? Convert.ToInt32(v) : 0;
                    model.StartDate = f;
                    model.EndDate = e;
                    model.StatusId = s.HasValue ? Convert.ToInt32(s) : 0;
                    model.SearchId = !string.IsNullOrEmpty(rto) ? rto : "0";
                    model.OpId = o.HasValue ? Convert.ToInt32(o) : 0;
                    model.CircleId = c.HasValue ? Convert.ToInt32(c) : 0;

                    model.CustomerNo = !string.IsNullOrEmpty(m) ? m : "";
                    model.UserRefId = !string.IsNullOrEmpty(ut) ? ut : "";
                    model.ApiTxnId = !string.IsNullOrEmpty(vt) ? vt : "";
                    model.OpTxnId = !string.IsNullOrEmpty(ot) ? ot : "";

                    int userrole = CurrentUser.Roles.FirstOrDefault();
                    model.UserId = userrole == 3 ? CurrentUser.UserID : model.UserId;
                    model.UpdatedById = u2.HasValue ? Convert.ToInt32(u2) : 0;
                    model.IsMail = im ?? 0;



                    FilterData fdata = new FilterData();
                    fdata.FromDate = model.StartDate;
                    fdata.ToDate = model.EndDate;
                    fdata.StatusId = model.StatusId;
                    fdata.UserId = model.UserId;
                    fdata.ApiId = model.ApiId;
                    fdata.RecId = Convert.ToInt64(model.SearchId);
                    fdata.OpId = model.OpId;
                    fdata.CircleId = model.CircleId;
                    fdata.CustomerNo = model.CustomerNo;
                    fdata.OpTxnId = model.OpTxnId;

                    var data = GetComplaintData(fdata);
                    filename = "ComplaintReport_" + DateTime.Now.ToString("ddMMyyyy_hhmmss_tt") + ".csv";
                    writer = GetCsvFromModel(data, writer, ignoreColumns);


                }
                else if (rtype == 7)
                {
                    ExportDto model = new ExportDto();
                    model.TypeId = rt.HasValue ? Convert.ToInt32(rt) : 1;
                    model.UserId = u.HasValue ? Convert.ToInt32(u) : 0;
                    model.ApiId = v.HasValue ? Convert.ToInt32(v) : 0;
                    model.StartDate = f;
                    model.EndDate = e;
                    model.StatusId = s.HasValue ? Convert.ToInt32(s) : 0;
                    model.SearchId = !string.IsNullOrEmpty(rto) ? rto : "0";
                    model.OpId = o.HasValue ? Convert.ToInt32(o) : 0;
                    model.CircleId = c.HasValue ? Convert.ToInt32(c) : 0;
                    model.CustomerNo = !string.IsNullOrEmpty(m) ? m : "0";
                    model.UserRefId = !string.IsNullOrEmpty(ut) ? ut : "0";
                    model.ApiTxnId = !string.IsNullOrEmpty(vt) ? vt : "0";
                    model.OpTxnId = !string.IsNullOrEmpty(ot) ? ot : "0";

                    model.UserId = CurrentUser.IsAdminRole ? model.UserId : CurrentUser.UserID;

                    string ignorecols = !CurrentUser.IsAdminRole ? "User,Vendor,VComm" : string.Empty;

                    var data = GetRecharge2Data(model);
                    writer = GetCsvFromModel(data, writer, ignorecols);
                    filename = "RecReport_" + DateTime.Now.ToString("ddMMyyyy_hhmmss_tt") + ".csv";

                }
                //else if (rtype == 8)
                //{
                //    ExportDto model = new ExportDto();
                //    model.TypeId = rt.HasValue ? Convert.ToInt32(rt) : 1;
                //    model.UserId = u.HasValue ? Convert.ToInt32(u) : 0;
                //    model.TypeName = !string.IsNullOrEmpty(m) ? m : "";
                //    model.ApiTxnId = !string.IsNullOrEmpty(vt) ? vt : "";
                //    model.StartDate = f;
                //    model.EndDate = e;
                //    model.Remark = !string.IsNullOrEmpty(rm) ? rm : "";
                //    model.StatusId = s.HasValue ? Convert.ToInt32(s) : 0;
                //    model.SearchId = !string.IsNullOrEmpty(rto) ? rto : "";
                //    model.OpId = o.HasValue ? Convert.ToInt32(o) : 0;
                //    model.CircleId = c.HasValue ? Convert.ToInt32(c) : 0;
                //    model.CustomerNo = !string.IsNullOrEmpty(nm) ? nm : "";
                //    model.UserRefId = !string.IsNullOrEmpty(ut) ? ut : "";
                //    model.IsMail = im.HasValue ? Convert.ToInt32(im) : 0;
                //    model.OpTxnId = !string.IsNullOrEmpty(ot) ? ot : "";
                //    model.UserId = CurrentUser.IsAdminRole ? model.UserId : CurrentUser.UserID;


                //    var data = GetExportDMTDtos(model);
                //    writer = GetCsvFromModel(data, writer, "");
                //    filename = "GatewayReport" + DateTime.Now.ToString("ddMMyyyy_hhmmss_tt") + ".csv";

                //}
                else if (rtype == 9)
                {
                    ExportDto model = new ExportDto();
                    model.TypeId = rt.HasValue ? Convert.ToInt32(rt) : 1;
                    model.UserId = u.HasValue ? Convert.ToInt32(u) : 0;
                    model.TypeName = !string.IsNullOrEmpty(m) ? m : "";
                    model.ApiTxnId = !string.IsNullOrEmpty(vt) ? vt : "";
                    model.StartDate = f;
                    model.EndDate = e;
                    model.Remark = !string.IsNullOrEmpty(rm) ? rm : "";
                    model.StatusId = s.HasValue ? Convert.ToInt32(s) : 0;
                    model.SearchId = !string.IsNullOrEmpty(rto) ? rto : "";
                    model.OpId = o.HasValue ? Convert.ToInt32(o) : 0;
                    model.CircleId = c.HasValue ? Convert.ToInt32(c) : 0;
                    model.CustomerNo = !string.IsNullOrEmpty(nm) ? nm : "";
                    model.UserRefId = !string.IsNullOrEmpty(ut) ? ut : "";
                    model.IsMail = im.HasValue ? Convert.ToInt32(im) : 0;
                    model.OpTxnId = !string.IsNullOrEmpty(ot) ? ot : "";
                    model.UserId = CurrentUser.RoleId==1 ? model.UserId : CurrentUser.UserID;


                    var data = GetExportDMTDtos(model);
                    writer = GetCsvFromModel(data, writer, "");
                    filename = "DMT Report" + DateTime.Now.ToString("ddMMyyyy_hhmmss_tt") + ".csv";

                }
                //else if (rtype == 10)
                //{
                //    ExportDto model = new ExportDto();
                //    model.TypeId = rt.HasValue ? Convert.ToInt32(rt) : 1;
                //    model.UserId = u.HasValue ? Convert.ToInt32(u) : 0;
                //    model.TypeName = !string.IsNullOrEmpty(m) ? m : "";
                //    model.ApiTxnId = !string.IsNullOrEmpty(vt) ? vt : "";
                //    model.StartDate = f;
                //    model.EndDate = e;
                //    model.Remark = !string.IsNullOrEmpty(rm) ? rm : "";
                //    model.StatusId = s.HasValue ? Convert.ToInt32(s) : 0;
                //    model.SearchId = !string.IsNullOrEmpty(rto) ? rto : "";
                //    model.OpId = o.HasValue ? Convert.ToInt32(o) : 0;
                //    model.CircleId = c.HasValue ? Convert.ToInt32(c) : 0;
                //    model.CustomerNo = !string.IsNullOrEmpty(nm) ? nm : "";
                //    model.UserRefId = !string.IsNullOrEmpty(ut) ? ut : "";
                //    model.IsMail = im.HasValue ? Convert.ToInt32(im) : 0;
                //    model.OpTxnId = !string.IsNullOrEmpty(ot) ? ot : "";
                //    model.UserId = CurrentUser.IsAdminRole ? model.UserId : CurrentUser.UserID;


                //    var data = GetExportDMTDtos(model);
                //    writer = GetCsvFromModel(data, writer, "");
                //    filename = "DMT Report" + DateTime.Now.ToString("ddMMyyyy_hhmmss_tt") + ".csv";

                //}

                writer.Flush();
                output.Position = 0;

                ShowSuccessMessage("Success!", "Data Exported Successfully.", true);

                return File(output, "text/comma-separated-values", filename);
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error!", "Internal Server Error.", true);
                LogException(ex);
            }

            return RedirectToAction("Index");
        }

        public StreamWriter GetCsvFromModel<T>(IEnumerable<T> data, StreamWriter output, string strIgnoreColumns = "")
        {
            var IgnoreColumns = strIgnoreColumns.Split(',').ToList();

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            foreach (PropertyDescriptor prop in props)
            {
                if (!IgnoreColumns.Any(s => s == prop.DisplayName))
                {
                    output.Write(prop.DisplayName); // header
                    output.Write(",");
                }

            }
            output.WriteLine();
            foreach (T item in data)
            {
                foreach (PropertyDescriptor prop in props)
                {
                    if (!IgnoreColumns.Any(s => s == prop.DisplayName))
                    {

                        output.Write(prop.Converter.ConvertToString(prop.GetValue(item)));
                        output.Write(",");
                    }
                }
                output.WriteLine();
            }

            return output;
        }

        private List<ExportRechargeDto> GetRechargeData(ExportDto model)
        {
            model.UserId = CurrentUser.IsAdminRole ? model.UserId : CurrentUser.UserID;


            var rcReport = rechargeReportService.GetRechargeList(model.StartDate, model.EndDate, Convert.ToByte(model.StatusId), model.UserId, model.ApiId, model.SearchId, model.OpId, model.CircleId, model.CustomerNo, model.UserRefId, model.ApiTxnId, model.OpTxnId, model.UpdatedById);

            var data = rcReport.Select(x => new ExportRechargeDto()
            {
                RecId = x.Id.ToString(),
                UserId = model.UserId > 0 ? x.User.Username : x.User.UserProfile.FullName,
                VendorId = !CurrentUser.IsAdminRole ? x.User.UserProfile.FullName : x.ApiSource.ApiName,
                TxnId = x.TxnId?.ToPlainExcelText(),
                CustomerNo = x.CustomerNo?.ToPlainExcelText(),
                Service = x.Operator.Name,
                Amount = x.Amount.ToString(),
                RCTypeId = x.RCType?.TypeName,
                StatusId = x.StatusType?.TypeName,
                RequestTime = x.RequestTime?.ToString("dd/MM/yyyy HH:mm:ss.fff"),
                ResponseTime = x.ResponseTime?.ToString("dd/MM/yyyy HH:mm:ss.fff"),
                MediumId = x.MediumType?.TypeName,
                StatusMsg = x.StatusMsg?.ToPlainExcelText(),
                IPAddress = x.IPAddress,
                //CircleId = x.Circle?.CircleName,
                ROfferAmount = x.ROfferAmount.ToString(),
                UserTxnId = x.UserTxnId?.ToPlainExcelText(),
                OurRefTxnId = x.OurRefTxnId,
                VendorTxnId = (!CurrentUser.IsAdminRole ? x.UserTxnId : x.ApiTxnId)?.ToPlainExcelText(),
                OptTxnId = x.OptTxnId?.ToPlainExcelText(),
                UpdatedDate = x.UpdatedDate?.ToString("dd/MM/yyyy HH:mm:ss.fff"),
                UpdatedBy = CurrentUser.IsAdminRole ? x.User1?.UserProfile?.FullName ?? string.Empty : string.Empty,

            }).OrderByDescending(x => x.RecId).ToList();

            return data;
        }

        private List<ExportReqResDto> GetReqResData(ReqResFilterDto model)
        {
            model.UserId = CurrentUser.IsAdminRole ? model.UserId : CurrentUser.UserID;

            var report = reqResService.GetRequestResponse(model);

            var data = report.Where(u => !CurrentUser.IsAdminRole ? u.Remark == "Request_R" || u.Remark == "Request_F" : true).Select(x => new ExportReqResDto()
            {
                Id = x.Id.ToString(),
                RecId = x.RecId.ToString(),
                OurRefTxnId = x.RefId,
                User = !CurrentUser.IsAdminRole ? x.User?.Username ?? string.Empty : x.User?.UserProfile?.FullName ?? string.Empty,
                Vendor = !CurrentUser.IsAdminRole ? x.User?.UserProfile?.FullName ?? string.Empty : x.ApiUrl?.ApiSource?.ApiName ?? string.Empty,
                CustomerNo = x.CustomerNo?.ToPlainExcelText(),
                UserTxnId = x.UserTxnId?.ToPlainExcelText(),
                RcOperator = x.Recharge?.Operator?.Name ?? string.Empty,
                RcStatus = x.Recharge?.StatusType?.TypeName ?? string.Empty,
                AddedDate = x.AddedDate.ToString("dd/MM/yyyy HH:mm:ss.fff"),
                Remark = x.Remark,
                RequestText = x.RequestTxt?.ToPlainExcelText(),
                ResponseText = x.ResponseText?.ToPlainExcelText(),
                UpdatedDate = x.UpdatedDate?.ToString("dd/MM/yyyy HH:mm:ss.fff"),

            }).OrderByDescending(y => y.AddedDate).ToList();

            return data;
        }

        private List<ExporStatementDto> GetStatementData(BankStatementFilterDto model)
        {
            model.UserId = CurrentUser.IsAdminRole ? model.UserId : CurrentUser.UserID;

            var report = bankAccountService.GetBankStatements(model);

            var data = report.Select(x => new ExporStatementDto()
            {
                // CustomerNo = x.CustomerNo?.ToPlainExcelText(),


                Id = x.Id.ToString(),
                AccountId = x.BankAccount.BankName ?? string.Empty,
                OP_Bal = x.OP_Bal.ToString(),
                CR_Amt = x.CR_Amt.ToString(),
                DB_Amt = x.DB_Amt.ToString(),
                CL_Bal = x.CL_Bal.ToString(),
                TxnTypeId = x.TxnType?.TypeName ?? string.Empty,
                AmtTypeId = x.AmtType?.AmtTypeName ?? string.Empty,
                TrTypeId = x.TransferType?.Name ?? string.Empty,
                TxnId = x.TxnId?.ToPlainExcelText(),
                ApiTxnId = x.ApiTxnId?.ToPlainExcelText(),
                RefAccountId = x.BankAccount1?.BankName ?? string.Empty,
                PaymentRef = x.PaymentRef ?? string.Empty,
                Remark = x.Remark?.ToPlainExcelText() ?? string.Empty,
                Comment = x.Comment?.ToPlainExcelText() ?? string.Empty,
                AddedById = x.User?.UserProfile?.FullName ?? string.Empty,
                AddedDate = x.AddedDate?.ToString("dd/MM/yyyy HH:mm:ss.fff") ?? string.Empty,
                UpdatedById = x.User1?.UserProfile?.FullName ?? string.Empty,
                UpdatedDate = x.UpdatedDate?.ToString("dd/MM/yyyy HH:mm:ss.fff") ?? string.Empty,
                UserId = x.User2?.UserProfile?.FullName ?? string.Empty,
                ApiId = x.ApiSource?.ApiName ?? string.Empty,
                PaymentDate = x.AddedDate?.ToString("dd/MM/yyyy") ?? string.Empty,


            }).OrderByDescending(y => y.AddedDate).ToList();

            return data;
        }

        private List<ExportActivityLogDto> GetActivityLogData(ActiVityLogFilterDto model)
        {
            model.userid = CurrentUser.IsAdminRole ? model.userid : CurrentUser.UserID;

            var report = activityLogService.GetActivityLog(model);

            var data = report.Select(x => new ExportActivityLogDto()
            {
                Id = x.Id.ToString() ?? string.Empty,
                UserId = x.User?.UserProfile?.FullName ?? string.Empty,
                ActivityName = x.ActivityName ?? string.Empty,
                ActivityDate = x.ActivityDate?.ToString("dd/MM/yyyy HH:mm:ss.fff") ?? string.Empty,
                IPAddress = x.IPAddress ?? string.Empty,
                ActivityPage = x.ActivityPage ?? string.Empty,
                Remark = x.Remark?.ToPlainExcelText() ?? string.Empty

            }).OrderByDescending(y => y.Id).ToList();

            return data;
        }

        private List<ExportRecharge2Dto> GetRecharge2Data(ExportDto model)
        {
            model.UserId = CurrentUser.IsAdminRole ? model.UserId : CurrentUser.UserID;


            var rcReport = rechargeReportService.GetRechargeList(model.StartDate, model.EndDate, Convert.ToByte(model.StatusId), model.UserId, model.ApiId, model.SearchId, model.OpId, model.CircleId, model.CustomerNo, model.UserRefId, model.ApiTxnId, model.OpTxnId, model.UpdatedById);

            var data = rcReport.Select(x => new ExportRecharge2Dto()
            {
                Id = x.Id.ToString(),
                User = x.User?.UserProfile?.FullName ?? string.Empty,
                RecDate = x.RequestTime?.ToString("dd/MM/yyyy HH:mm:ss.fff"),
                CustomerNo = x.CustomerNo?.ToPlainExcelText(),
                Service = x.Operator?.Name ?? string.Empty,
                Status = x.StatusType?.TypeName,
                Amount = x.Amount?.ToString(),
                Earn = x.UserComm?.ToString(),
                Cl_Bal = x.TxnLedger?.CL_Bal?.ToString(),
                //Circle = x.Circle?.CircleName ?? string.Empty,
                OptTxnId = x.OptTxnId?.ToPlainExcelText(),
                Vendor = x.ApiSource?.ApiName ?? string.Empty,
                VComm = x.ApiComm?.ToString(),
                UserReqId = x.UserTxnId

            }).OrderByDescending(x => x.Id).ToList();

            return data;
        }

        private void UpdateActivity(string ActivityName, string ActivityPage, string remark = "")
        {
            try
            {
                activityLogModel.ActivityName = ActivityName;
                activityLogModel.ActivityPage = ActivityPage;
                activityLogModel.Remark = remark;
                activityLogModel.UserId = CurrentUser?.UserID ?? 0;
                LogActivity(activityLogModel);
            }
            catch (Exception ex)
            {

                LogException(ex);
            }

        }
        private List<ExportDMTDto> GetExportDMTDtos(ExportDto model)
        {
            var fdata = model;
            #region "Filter"
            fdata.EndDate = fdata.EndDate;
            var predicate = PredicateBuilder.True<DMT>();
            DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.StartDate) ? fdata.StartDate : DateTime.Now.AddDays(-30).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.EndDate) ? fdata.EndDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
            tdate = !string.IsNullOrEmpty(fdata.EndDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

            predicate = fdata.IsMail == 0 ? predicate : predicate.And(x => x.Id == fdata.IsMail);
            predicate = fdata.StatusId == 0 ? predicate : predicate.And(x => x.StatusId == fdata.StatusId);
            predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.UserId == fdata.UserId);
            predicate = model.StartDate == "" ? predicate : predicate.And(x => x.RequestTime >= fdate);
            predicate = model.EndDate == "" ? predicate : predicate.And(x => x.RequestTime <= tdate);
            //predicate = fdata.CircleId == 0 ? predicate : predicate.And(x => x.PaymentMode == fdata.CircleId);

            #endregion

            List<DMT> results = repoDMT
               .Query().Filter(predicate).Get()
               .ToList();
            var resultResponse = results.Select(c => new ExportDMTDto()
            {
                Id = c.UserId,
                UserId = c.User.Username.Replace(",", ""),
                Txn_Id = c.TxnId,
                DMT_Id = c.Id,
                UserReqId = c.UserTxnId,
                CustomerNo = c.BeneMobile,
                Vndr_TxnId = c.ApiTxnId,
                Op_TxnId = c.OptTxnId,
                Bank = c.IFSCCode,
                BalanceAmt = c.Amount,
                Discount = c.UserComm,
                Status = c.StatusType.TypeName,
                ReqTime = c.RequestTime,
                ResTime = c.ResponseTime,
                UpdatedON = c.UpdatedDate,
                UpdatedBY = c.UpdatedById,

            }).OrderByDescending(x => x.ReqTime).ToList();
            return resultResponse;
        }
        public ActionResult Import(int? importid)
        {
            UpdateActivity("Export REQUEST", "GET:Export/Import/");
            ViewBag.actionAllowed = action = ActionAllowed("Import", CurrentUser.RoleId);
            if (!CurrentUser.IsAdminRole) { throw new Exception("Error! Access Denied"); }

            // UpdateActivity("ResolveComplain REQUEST", "GET:RechargeReport/ResolveComplain/", "cmpid=" + id);

            return View("Import");

        }

        [HttpPost]
        public ActionResult Import(FormCollection FC)
        {
            UpdateActivity("Export REQUEST", "POST:Export/Import/");
            ViewBag.actionAllowed = action = ActionAllowed("Import", CurrentUser.RoleId, 3);
            if (!CurrentUser.IsAdminRole) { throw new Exception("Error! Access Denied"); }

            try
            {
                if (Request != null)
                {
                    DataTable dtExcelData = new DataTable();
                    HttpPostedFileBase file = Request.Files["UploadedFile"];
                    if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                    {
                        string uploadfolder = System.Configuration.ConfigurationManager.AppSettings["UploadPath"] + "OPTXNID/";

                        string extname = file.FileName.EndsWith(".xls") ? ".xls" :
                                        file.FileName.EndsWith(".xlsx") ? ".xlsx" :
                                        file.FileName.EndsWith(".ods") ? ".ods" : string.Empty;

                        string fileName = "OPTXNID_U" + CurrentUser.UserID + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + extname;

                        string path = Server.MapPath(uploadfolder + fileName);
                        if (!Directory.Exists(Server.MapPath(uploadfolder)))
                        {
                            Directory.CreateDirectory(Server.MapPath(uploadfolder));
                        }

                        file.SaveAs(path);

                        var excelData = new ExcelData(path);
                        var sData = excelData.getData(1);
                        dtExcelData = sData.CopyToDataTable();

                        UpdateOPTxnIds(dtExcelData);

                        ShowSuccessMessage("Error!", "Data Updated Succesfully!", false);
                    }
                    else
                    {
                        ShowErrorMessage("Error!", "File not found", false);
                    }
                }
                else
                {
                    ShowErrorMessage("Error!", "Invalid Request", false);
                }

            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error!", "Something Went Wrong", false);
                LogException(ex, "import optxnid");
            }

            return View("Import");
        }

        private void UpdateOPTxnIds(DataTable dtExcelData)
        {
            using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
            {
                SqlCommand cmd = new SqlCommand("usp_UpdateOpTxnIdFromExcel", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 300;
                cmd.Parameters.AddWithValue("@UserId", CurrentUser.UserID);
                cmd.Parameters.AddWithValue("@ExcelData", dtExcelData);

                // log += "\r\n ,  before execute = usp_UpdateRecDetailToReqRes";
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

            }

        }

        private List<ExportComplaintDto> GetComplaintData(FilterData fdata)
        {

            var rcReport = rechargeReportService.GetComplaints(fdata);

            var data = rcReport.Select(x => new ExportComplaintDto()
            {
                CmpId = x.Id.ToString(),
                User = x.User.UserProfile.FullName,
                Vendor = x.Recharge?.ApiSource?.ApiName?.ToPlainExcelText() ?? string.Empty,
                AddedOn = x.ComplaintDate?.ToString("dd/MM/yyyy HH:mm:ss.fff"),
                CmpStatus = x.StatusType?.TypeName ?? string.Empty,
                CmpRemark = x.Remark,
                Comment = x.Comment,
                IsRefund = x.IsRefund == true ? "Yes" : "No",
                UpdatedOn = x.UpdatedDate?.ToString("dd/MM/yyyy HH:mm:ss.fff") ?? string.Empty,
                RecId = x.Id.ToString(),
                RecDate = x.Recharge?.RequestTime?.ToString("dd/MM/yyyy HH:mm:ss.fff"),
                CustomerNo = x.Recharge?.CustomerNo?.ToPlainExcelText(),
                Operator = x.Recharge?.Operator?.Name ?? string.Empty,
                OptTxnId = x.Recharge.OptTxnId?.ToPlainExcelText(),
                Amount = x.Recharge?.Amount?.ToString(),
                RcStatus = x.Recharge?.StatusType?.TypeName ?? string.Empty,
                UserReqId = x.Recharge?.UserTxnId?.ToPlainExcelText(),
                OurRefId = x.Recharge?.OurRefTxnId,
                TxnId = x.Recharge?.TxnId?.ToPlainExcelText(),
                VendorTxnId = x.Recharge?.ApiTxnId?.ToPlainExcelText(),
                Circle = x.Recharge?.Circle?.CircleName

            }).OrderByDescending(x => x.CmpId).ToList();

            return data;
        }

        [HttpPost]
        public int EmailComplaint(FilterData fdata)
        {
            if (!CurrentUser.IsAdminRole) { throw new Exception("Error! Access Denied"); }

            string ignoreColumns = "User,Vendor,AddedOn,Comment,IsRefund,UpdatedOn,RecId,UserReqId,TxnId";

            var vendor = apiService.GetApiSource(fdata.ApiId);

            if (fdata.ApiId == 0)
            {
                return 1;
            }
            else if (string.IsNullOrEmpty(vendor.Email))
            {
                return 2;
            }
            else
            {
                try
                {
                    var filename = "ComplaintReport_" + DateTime.Now.ToString("ddMMyyyy_hhmmss_tt") + ".csv";
                    var data = GetComplaintData(fdata);
                    EmailComplaint(data.ToDataTable(ignoreColumns), ignoreColumns, filename, vendor.Email);
                    return 0;
                }
                catch (Exception ex)
                {

                    LogException(ex, "EmailComplaint");

                    return 3;
                }

            }

        }

        private void EmailComplaint(DataTable data, string ignoreColumns, string filename, string mailto)
        {
            string uploadfolder = ConfigurationManager.AppSettings["UploadPath"] + "Temp/";
            string path = Server.MapPath(uploadfolder + filename);

            data.SaveDataTableToExcel(path);
            FlexiMail objSendMail = new FlexiMail();
            objSendMail = new FlexiMail();
            objSendMail.To = mailto;
            objSendMail.Subject = "Complaint List";
            objSendMail.MailBody = "<h3>Hello,</h3> <br /> Please find the list of pending complaints as attached herewith. Kindly, Resolved these complaint as soon as possible. <br /> <br /> <br /> <b>Regards,</b><br /> Support Team";

            objSendMail.From = SiteKey.From;
            objSendMail.FromName = SiteKey.CompanyFullName; ;
            objSendMail.CC = SiteKey.CC;
            objSendMail.BCC = SiteKey.BCC;
            objSendMail.MailBodyManualSupply = true;
            objSendMail.AttachFile = path.Split(';').ToArray();
            objSendMail.Send();

            try
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "Delete Complaint File");
            }



        }

        private List<ExportTxnLedger> GetTxnReportData(TxnFilterDto model)
        {

            int uid = CurrentUser.IsAdminRole ? model.userid : CurrentUser.UserID;

            var data1 = walletService.GetUserTxnReport(model);

            var data = data1.Select(x => new ExportTxnLedger()
            {
                TxnId = x.Id.ToPlainExcelText(),
                TxnDate = x.TxnDate.ToString(),
                RecId = x.RecId.ToString(),
                RcAmount = x.Recharge?.Amount?.ToString() ?? string.Empty,
                CommAmt = x.Recharge?.UserComm?.ToString() ?? string.Empty,
                OP_Bal = x.OP_Bal.ToString(),
                CR_Amt = x.CR_Amt.ToString(),
                DB_Amt = x.DB_Amt.ToString(),
                CL_Bal = x.CL_Bal.ToString(),
                UserId = !CurrentUser.IsAdminRole ? x.User?.Username ?? string.Empty : x.User?.UserProfile?.FullName ?? string.Empty,
                VendorId = !CurrentUser.IsAdminRole ? x.User?.UserProfile?.FullName ?? string.Empty : x.Recharge?.ApiSource?.ApiName ?? string.Empty,
                RefTxnId = x.RefTxnId.ToString(),
                TxnType = x.TxnType.TypeName.ToString(),
                AmtType = x.AmtType.AmtTypeName.ToString(),
                Remark = x.Remark.ToString(),
                ApiTxnId = x.ApiTxnId.ToString(),
                AddedBy = x.TxnTypeId == 4 ? x.User1?.UserProfile?.FullName?.ToString() ?? string.Empty : string.Empty
            }).OrderByDescending(x => x.TxnId).ToList();

            return data;
        }

        public ActionResult ImportCompareRC(int? importid)
        {
            UpdateActivity("ImportCompareRC", "GET:Export/ImportCompareRC/");
            ViewBag.actionAllowed = action = ActionAllowed("ImportCompareRC", CurrentUser.RoleId);
            if (!CurrentUser.IsAdminRole) { throw new Exception("Error! Access Denied"); }

            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName });

            CompareRcDto model = new CompareRcDto();
            model.RecDate = DateTime.Now.ToString("dd/MM/yyyy");

            return View("ImportCompareRC", model);
        }

        [HttpPost]
        public ActionResult ImportCompareRC(CompareRcDto model)
        {
            UpdateActivity("ImportCompareRC", "POST:Export/ImportCompareRC/");
            ViewBag.actionAllowed = action = ActionAllowed("ImportCompareRC", CurrentUser.RoleId);
            if (!CurrentUser.IsAdminRole) { throw new Exception("Error! Access Denied"); }


            try
            {
                string expfilepath = System.Web.HttpContext.Current.Server.MapPath("~/ExceptionLog/");  //Text File Path

                bool IsValidDate = false;

                try
                {
                    DateTime rcdate = DateTime.ParseExact(model.RecDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    IsValidDate = true;

                    if (model.IsClear)
                    {
                        ClearCompareRc(model.ApiId, rcdate.ToString("yyyy-MM-dd"));
                    }
                }
                catch (Exception ex1)
                {
                    LogException(ex1, "Upload Compare RC invalid RecDate=" + model.RecDate);
                }

                if (!IsValidDate)
                {
                    ShowErrorMessage("Error!", "Please, Enter Valid Date.", false);
                }
                else if (model.ApiId <= 0)
                {
                    ShowErrorMessage("Error!", "Please, Select a Vendor.", false);
                }
                else if (string.IsNullOrEmpty(model.RecDate))
                {
                    ShowErrorMessage("Error!", "Please, Enter Valid Date.", false);
                }
                else if (model.UploadedFile == null)
                {
                    if (model.IsClear)
                        ShowSuccessMessage("Success!", "Removed Existing Compared Data Successfully.", false);
                    else
                        ShowErrorMessage("Error!", "Please, Select a File to Upload.", false);
                }
                else if (!model.UploadedFile.FileName.EndsWith(".xls") && !model.UploadedFile.FileName.EndsWith(".xlsx") && !model.UploadedFile.FileName.EndsWith(".ods"))
                {
                    ShowErrorMessage("Error!", "Please, Select a File to Upload.", false);
                }
                else
                {

                    string uploadfolder = System.Configuration.ConfigurationManager.AppSettings["UploadPath"] + "CompareRc/";

                    string extname = model.UploadedFile.FileName.EndsWith(".xls") ? ".xls" :
                                    model.UploadedFile.FileName.EndsWith(".xlsx") ? ".xlsx" :
                                   model.UploadedFile.FileName.EndsWith(".ods") ? ".ods" : string.Empty;

                    if (string.IsNullOrEmpty(extname))
                    {
                        ShowErrorMessage("Error!", "Invalid File Format.(Allowed Only .xls, .xlsx, .ods Format)", false);
                    }
                    else
                    {
                        DateTime rcdate = DateTime.ParseExact(model.RecDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        string fileName = "CompareRC" + "_V" + model.ApiId + "_DT" + rcdate.ToString("ddMMyyyy") + "_U" + CurrentUser.UserID + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + extname;

                        string path = Server.MapPath(uploadfolder + fileName);
                        if (!Directory.Exists(Server.MapPath(uploadfolder)))
                        {
                            Directory.CreateDirectory(Server.MapPath(uploadfolder));
                        }

                        model.UploadedFile.SaveAs(path);

                        //CompareRcApi compareRcApi = apiService.GetCompareRcApi(model.ApiId, rcdate) ?? new CompareRcApi();
                        //compareRcApi.ApiId = model.ApiId;
                        //compareRcApi.UploadCount = (compareRcApi.UploadCount ?? 0) + 1;
                        //compareRcApi.RecDate = rcdate;
                        //compareRcApi.FilesName = fileName;
                        //compareRcApi.FilesPath = SiteKey.DomainName + "Upload/CompareRc/" + fileName;
                        //compareRcApi.AddedDate = compareRcApi.Id == 0 ? DateTime.Now : compareRcApi.AddedDate;
                        //compareRcApi.UpdatedDate = compareRcApi.Id > 0 ? DateTime.Now : compareRcApi.UpdatedDate;
                        //compareRcApi.AddedById = compareRcApi.Id == 0 ? CurrentUser.UserID : compareRcApi.AddedById;
                        //compareRcApi.UpdatedById = compareRcApi.Id > 0 ? CurrentUser.UserID : compareRcApi.UpdatedById;

                        //apiService.Save(compareRcApi);

                        string rcDateStr = rcdate.ToString("yyyy-MM-dd");

                        Thread thread = new Thread(new ThreadStart(() => CompareRcProcess(model.ApiId, fileName, rcDateStr, path, expfilepath)));
                        thread.Start();


                        if (model.IsClear)
                            ShowSuccessMessage("Success!", "Removed Existing Compared Data and Successfully Imported New Data.", false);
                        else
                            ShowSuccessMessage("Success!", "Successfully Imported", false);

                    }

                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage("OOPS!", "Something Went Wrong.", false);
                LogException(ex, "Import RC");
            }

            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName });

            return View(new CompareRcDto());
        }

        private void CompareRcProcess(int ApiId, string FilesName, string RecDate, string path, string excpfilepath = "")
        {
            try
            {

                string excelConStr = @"Provider='Microsoft.ACE.OLEDB.12.0';Data Source='" + path + "';Extended Properties='Excel 12.0 Xml;IMEX=1'";
                OleDbConnection excelCon = new OleDbConnection(excelConStr);

                //Sheet Name
                excelCon.Open();
                var table = excelCon.GetSchema("Tables");
                string tableName = excelCon.GetSchema("Tables").Rows[0]["TABLE_NAME"].ToString();
                excelCon.Close();
                //End
                string sqlquery = "select distinct *," + ApiId + " as ApiId, '" + RecDate + "' as RecDate FROM [" + tableName + "] WHERE OurRefId IS  NOT NULL OR VendorRefId IS NOT NULL";

                OleDbCommand olecmd = new OleDbCommand(sqlquery, excelCon);

                excelCon.Open();

                OleDbDataReader dReader;
                dReader = olecmd.ExecuteReader();

                SqlBulkCopy sqlBulk = new SqlBulkCopy(SiteKey.SqlConn);

                //Give your Destination table name
                sqlBulk.DestinationTableName = "CompareRc";

                //Mappings
                sqlBulk.ColumnMappings.Add("ApiId", "ApiId");
                sqlBulk.ColumnMappings.Add("RecDate", "RecDate");
                sqlBulk.ColumnMappings.Add("OurRefId", "OurRefId");
                sqlBulk.ColumnMappings.Add("VendorRefId", "ApiTxnId");
                sqlBulk.ColumnMappings.Add("CustomerNo", "CustomerNo");
                sqlBulk.ColumnMappings.Add("Amount", "Amount");
                sqlBulk.ColumnMappings.Add("Status", "StatusTxt");

                sqlBulk.WriteToServer(dReader);
                excelCon.Close();

                using (SqlConnection sqlcon = new SqlConnection(SiteKey.SqlConn))
                {

                    SqlCommand sqlcmd = new SqlCommand("usp_CompareRcProcess", sqlcon);
                    sqlcmd.CommandTimeout = 5 * 60 * 1000;
                    sqlcmd.CommandType = CommandType.StoredProcedure;
                    sqlcmd.Parameters.AddWithValue("@ApiId", ApiId);
                    sqlcmd.Parameters.AddWithValue("@FilesName", FilesName);
                    sqlcmd.Parameters.AddWithValue("@RecDate", RecDate);

                    sqlcon.Open();
                    sqlcmd.ExecuteNonQuery();
                    sqlcon.Close();

                }

            }
            catch (Exception ex)
            {
                LogException(ex, "usp_CompareRcProcess");
            }
        }

        private void ClearCompareRc(int ApiId, string RecDate)
        {
            using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
            {

                SqlCommand cmd = new SqlCommand("usp_ClearCompareRc", con);
                cmd.CommandTimeout = 5 * 60 * 1000;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ApiId", ApiId);
                cmd.Parameters.AddWithValue("@RecDate", RecDate);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();

            }


        }

        //[HttpPost]
        //public string CompareRcProcessManual(int? id)
        //{
        //    UpdateActivity("CompareRcVendor process manual", "POST:Export/CompareRcProcessManual/" + id, "Id=" + id);
        //    action = ActionAllowed("CompareRcApi", CurrentUser.RoleId, id.HasValue ? 3 : 2);
        //    if (!CurrentUser.IsAdminRole) { throw new Exception("Error! Access Denied"); }

        //    CompareRcApi compareRcApi = apiService.GetCompareRcApi(id ?? 0);

        //    compareRcApi.UpdatedDate = DateTime.Now;
        //    compareRcApi.UpdatedById = CurrentUser.UserID;
        //    apiService.Save(compareRcApi);

        //    ClearCompareRc(compareRcApi.ApiId ?? 0, compareRcApi.RecDate?.ToString("yyyy-MM-dd"));

        //    string uploadfolder = System.Configuration.ConfigurationManager.AppSettings["UploadPath"] + "CompareRc/";
        //    string fileName = compareRcApi.FilesName;
        //    string path = Server.MapPath(uploadfolder + fileName);

        //    Thread thread = new Thread(new ThreadStart(() => CompareRcProcess(compareRcApi.ApiId ?? 0, fileName, compareRcApi.RecDate?.ToString("yyyy-MM-dd"), path)));
        //    thread.Start();

        //    return "1";

        //}

        //private List<ExportGatwayTxn> GatewayTxnData(ExportDto model)
        //{
        //    var fdata = model;
        //    #region "Filter"
        //    fdata.EndDate = fdata.EndDate;
        //    var predicate = PredicateBuilder.True<GatWayTxn>();
        //    DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.StartDate) ? fdata.StartDate : DateTime.Now.AddDays(-30).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
        //    DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.EndDate) ? fdata.EndDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
        //    tdate = !string.IsNullOrEmpty(fdata.EndDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;
        //    if (model.UserId != 1 && model.UserId != 0)
        //    {
        //        predicate = predicate.And(x => x.userId == model.UserId);
        //    }
        //    predicate = fdata.IsMail == 0 ? predicate : predicate.And(x => x.Id == fdata.IsMail);
        //    predicate = fdata.StatusId == 0 ? predicate : predicate.And(x => x.status == fdata.StatusId);
        //    predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.userId == fdata.UserId);
        //    predicate = fdata.CircleId == 0 ? predicate : predicate.And(x => x.PaymentMode == fdata.CircleId);
        //    predicate = string.IsNullOrEmpty(fdata.CustomerNo) ? predicate : predicate.And(x => x.User.MobileNumber == fdata.CustomerNo);
        //    predicate = string.IsNullOrEmpty(fdata.OpTxnId) ? predicate : predicate.And(x => x.GatewayTxnId == fdata.OpTxnId);
        //    predicate = string.IsNullOrEmpty(fdata.UserRefId) ? predicate : predicate.And(x => x.GateWayname == fdata.UserRefId);
        //    predicate = string.IsNullOrEmpty(fdata.TypeName) ? predicate : predicate.And(x => x.BankName == fdata.TypeName);
        //    predicate = Convert.ToInt32(string.IsNullOrEmpty(fdata.SearchId) ? "0" : fdata.SearchId) == 0 ? predicate : predicate.And(x => x.OrderId == Convert.ToInt32(fdata.SearchId));
        //    predicate = string.IsNullOrEmpty(fdata.StartDate) ? predicate : predicate.And(x => x.UpdatedDate >= fdate);
        //    predicate = string.IsNullOrEmpty(fdata.EndDate) ? predicate : predicate.And(x => x.UpdatedDate <= tdate);
        //    #endregion

        //    List<GatWayTxn> results = repoGateWayTxn
        //        .Query().Filter(predicate).Get()
        //        .ToList();
        //    var resultResponse = results.Select(c => new ExportGatwayTxn()
        //    {
        //        Id = c.Id,
        //        User = c?.User?.FullName + " " + c?.User?.MobileNumber ?? string.Empty,
        //        GatewayTxnId = (c?.GatewayTxnId ?? string.Empty),
        //        OrderId = c?.OrderId?.ToString() ?? string.Empty,
        //        TxnAmount = c?.TxnAmount.ToString(),
        //        Status = c?.StatusType.TypeName,
        //        Remark = c?.Remark ?? string.Empty,
        //        AddedDate = c?.AddedDate?.ToString(),
        //        ResponseDate = c?.GateWayTxnDate?.ToString(),
        //        UpdatedDate = c?.UpdatedDate?.ToString(),
        //        BankTxnId = c?.BankTxnId ?? string.Empty,
        //        RespCode = (CurrentUser.IsAdminRole ? c?.RespCode : String.Empty),
        //        RespMsg = c?.RespMsg,
        //        BankName = c?.BankName ?? string.Empty,
        //        GateWayname = c?.GateWayname ?? string.Empty,
        //        GatewayTransferType = c?.GatewayTransferType?.TypeName ?? string.Empty,
        //        Currency = c?.Currency,
        //        CheckSumHash = c?.CheckSumHash ?? string.Empty
        //    }).OrderByDescending(x => x.Id).ToList();
        //    return resultResponse;
        //}

        //private List<ExportDMTDto> GetExportDMTDtos(ExportDto model)
        //{
        //    var fdata = model;
        //    #region "Filter"
        //    fdata.EndDate = fdata.EndDate;
        //    var predicate = PredicateBuilder.True<DMT>();
        //    DateTime fdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.StartDate) ? fdata.StartDate : DateTime.Now.AddDays(-30).ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
        //    DateTime tdate = DateTime.ParseExact(!string.IsNullOrEmpty(fdata.EndDate) ? fdata.EndDate : DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
        //    tdate = !string.IsNullOrEmpty(fdata.EndDate) ? tdate.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(997) : DateTime.Now;

        //    predicate = fdata.IsMail == 0 ? predicate : predicate.And(x => x.Id == fdata.IsMail);
        //    //predicate = fdata.StatusId == 0 ? predicate : predicate.And(x => x.status == fdata.StatusId);
        //    //predicate = fdata.UserId == 0 ? predicate : predicate.And(x => x.userId == fdata.UserId);
        //    //predicate = fdata.CircleId == 0 ? predicate : predicate.And(x => x.PaymentMode == fdata.CircleId);

        //    #endregion

        //    List<DMT> results = repoDMT
        //       .Query().Filter(predicate).Get()
        //       .ToList();
        //    var resultResponse = results.Select(c => new ExportDMTDto()
        //    {
        //        Id = c.UserId,
        //        UserId=c.User.FullName.Replace(",",""),
        //        Txn_Id = c.DTxnId,
        //        DMT_Id = c.Id,
        //        UserReqId=c.UserTxnId,
        //        CustomerNo = c.BeneMobile,
        //        Vndr_TxnId = c.ApiTxnId,             
        //        Op_TxnId = c.OptTxnId,
        //        Bank = c.IFSCCode,
        //        BalanceAmt = c.Amount,
        //        Discount =c.UserComm,
        //        Status = c.StatusType.TypeName,
        //        //Status = c.StatusMsg,
        //        ReqTime = c.RequestTime,
        //        ResTime = c.ResponseTime,
        //        UpdatedON =c.UpdatedDate,
        //        UpdatedBY =c.UpdatedById,

        //    }).OrderByDescending(x => x.ReqTime).ToList();
        //    return resultResponse;
        //}
    }

}