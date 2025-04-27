using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.Code.LIBS;
using DhruvEnterprises.Web.Models.Others;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static DhruvEnterprises.Core.Enums;

namespace DhruvEnterprises.Web.Controllers
{
    public class DailyReportController : BaseController
    {
        #region "FIELDS"
        ActivityLogDto activityLogModel;
        public ActionAllowedDto action;
        private readonly IRoleService roleService;
        private readonly IUserService userService;
        private readonly IApiService apiService;
        private readonly IPackageService packageService;
        private readonly IOperatorSwitchService opSwitchService;
        //private readonly ILapuDealerService dealerService;
        private readonly IRechargeReportService rechargeReportService;
        private readonly ICommonSwitchService userFilterRuleReportService;
        

        #endregion

        #region "CONSTRUCTOR"
        public DailyReportController(/*ILapuDealerService _dealerService*/IOperatorSwitchService _opSwitchService, IApiService _apiService, IRoleService _userroleService, IUserService _userService, IActivityLogService _activityLogService, IPackageService _packageService, IRechargeReportService _rechargeReportService, ICommonSwitchService _userFilterRuleReportService) : base(_activityLogService, _userroleService)
        {
            this.roleService = _userroleService;
            this.activityLogModel = new ActivityLogDto();
            this.action = new ActionAllowedDto();
            this.userService = _userService;
            this.apiService = _apiService;
            this.packageService = _packageService;
            this.opSwitchService = _opSwitchService;
            //this.dealerService = _dealerService;
            this.rechargeReportService = _rechargeReportService;
            this.userFilterRuleReportService = _userFilterRuleReportService;
        }
        #endregion

        #region METHOD
        // GET: DailyReport
        public ActionResult Index(int? u, int? v, string f = "", string e = "")
        {
            int uid = u ?? 0;
            int apiid = v ?? 0;

            uid = CurrentUser.RoleId != 3 ? uid : CurrentUser.UserID;
            apiid = CurrentUser.RoleId != 3 ? apiid : 0;

            UpdateActivity("Operator DayBook", "GET:DailyReport/Index/");
            ViewBag.actionAllowed = action = ActionAllowed("DailyReport", CurrentUser.RoleId);

            #region Search
            RechargeFilterDto filter = new RechargeFilterDto();

            if (f != "") { TempData["sDate"] = filter.Sdate = f; } else { filter.Sdate = DateTime.Now.ToString("MM/dd/yyyy"); }
            if (e != "") { TempData["eDate"] = filter.Edate = e; } else { filter.Edate = DateTime.Now.ToString("MM/dd/yyyy"); }

            ViewBag.FilterData = filter;
            #endregion
            List<dailydto> dailydto = new List<dailydto>();
            dailydto = DailyOperator(filter.Sdate, filter.Edate, uid, apiid);   //operator day book sp
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == uid) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == apiid) }).ToList();

            return View(dailydto);
        }

        public ActionResult UserDayBook(string f = "", string e = "")
        {
            UpdateActivity("User DayBook", "GET:DailyReport/UserDayBook/");
            ViewBag.actionAllowed = action = ActionAllowed("UserDayBook", CurrentUser.RoleId);

            #region Search

            string date1 = "";
            string date2 = "";
            int uid = CurrentUser.RoleId == 3 ? CurrentUser.UserID : 0;

            if (f != "") { TempData["sDate"] = date1 = f; } else { date1 = DateTime.Now.ToString("MM/dd/yyyy"); }
            if (e != "") { TempData["eDate"] = date2 = e; } else { date2 = DateTime.Now.ToString("MM/dd/yyyy"); }
            ViewBag.FilterData = date1;
            ViewBag.FilterData2 = date2;
            #endregion

            List<UserDayBookDto> model = new List<UserDayBookDto>();
            model = GetUserDayBook(uid, date1, date2);
            return View(model);
        }

        public ActionResult VendorDayBook(string f = "", string e = "")
        {
            UpdateActivity("Vendor DayBook", "GET:DailyReport/VendorDayBook/");
            ViewBag.actionAllowed = action = ActionAllowed("VendorDayBook", CurrentUser.RoleId);

            #region Search


            int uid = CurrentUser.RoleId == 3 ? CurrentUser.UserID : 0;

            string sdate = "", edate = "";
            sdate = f != "" ? f : DateTime.Now.ToString("MM/dd/yyyy");
            edate = e != "" ? e : DateTime.Now.ToString("MM/dd/yyyy");

            ViewBag.StartDate = sdate;
            ViewBag.EndDate = edate;
            #endregion

            var model = GetApiDayBook(uid, sdate, edate);

            return View(model);
        }

        public ActionResult LapuDayBook(string f = "", string e = "")
        {
            UpdateActivity("Lapu DayBook", "GET:DailyReport/LapuDayBook/");
            ViewBag.actionAllowed = action = ActionAllowed("LapuDayBook", CurrentUser.RoleId);

            #region Search


            int uid = CurrentUser.RoleId == 3 ? CurrentUser.UserID : 0;

            string sdate = "", edate = "";
            sdate = f != "" ? f : DateTime.Now.ToString("MM/dd/yyyy");
            edate = e != "" ? e : DateTime.Now.ToString("MM/dd/yyyy");

            ViewBag.StartDate = sdate;
            ViewBag.EndDate = edate;

            #endregion

            var model = GetLapuDayBook(uid, sdate, edate);

            return View(model);
        }

        public ActionResult UserMargin(string f = "", string e = "")
        {
            UpdateActivity("UserMargin", "GET:DailyReport/UserMargin/");
            ViewBag.actionAllowed = action = ActionAllowed("UserMargin", CurrentUser.RoleId);

            #region Search

            string sdate = "", edate = "";
            sdate = f != "" ? f : DateTime.Now.ToString("MM/dd/yyyy");
            edate = e != "" ? e : DateTime.Now.ToString("MM/dd/yyyy");

            ViewBag.StartDate = sdate;
            ViewBag.EndDate = edate;

            int uid = CurrentUser.RoleId == 3 ? CurrentUser.UserID : 0;

            #endregion


            List<UserDayBookDto> model = new List<UserDayBookDto>();
            model = GetUserDayBook(uid, sdate, edate);
            return View(model);
        }

        public ActionResult VendorMargin(string f = "", string e = "")
        {
            UpdateActivity("VendorMargin", "GET:DailyReport/VendorMargin/");
            ViewBag.actionAllowed = action = ActionAllowed("VendorMargin", CurrentUser.RoleId);
            #region Search

            string sdate = "", edate = "";
            sdate = f != "" ? f : DateTime.Now.ToString("MM/dd/yyyy");
            edate = e != "" ? e : DateTime.Now.ToString("MM/dd/yyyy");

            ViewBag.StartDate = sdate;
            ViewBag.EndDate = edate;

            int uid = CurrentUser.RoleId == 3 ? CurrentUser.UserID : 0;

            #endregion

            var model = GetApiDayBook(uid, sdate, edate);

            return View(model);
        }

        public ActionResult LapuMargin(string f = "", string e = "")
        {
            //LapuMargin
            UpdateActivity("LapuMargin", "GET:DailyReport/LapuMargin/");
            ViewBag.actionAllowed = action = ActionAllowed("LapuMargin", CurrentUser.RoleId);
            #region Search

            string sdate = "", edate = "";
            sdate = f != "" ? f : DateTime.Now.ToString("MM/dd/yyyy");
            edate = e != "" ? e : DateTime.Now.ToString("MM/dd/yyyy");

            ViewBag.StartDate = sdate;
            ViewBag.EndDate = edate;

            int uid = CurrentUser.RoleId == 3 ? CurrentUser.UserID : 0;

            #endregion


            var model = GetLapuDayBook(uid, sdate, edate);
            return View(model);
        }

        public ActionResult SearchNumber(string n = "")
        {
            //LapuMargin
            UpdateActivity("LapuMargin", "GET:DailyReport/SearchNumber/");
            ViewBag.actionAllowed = action = ActionAllowed("SearchNumber", CurrentUser.RoleId);
            #region Search
            string number = ViewBag.Number = n;
            int uid = CurrentUser.RoleId == 3 ? CurrentUser.UserID : 0;
            #endregion
            var model = GetRechargeByCustomerNo(uid, number);
            return View(model);
        }


        public ActionResult RecentRechargeCrWise(int? o)
        {
            //LapuMargin
            UpdateActivity("LapuMargin", "GET:DailyReport/RecentRechargeCrWise/");
            ViewBag.actionAllowed = action = ActionAllowed("RecentRechargeCrWise", CurrentUser.RoleId);
            #region Search

            int opid = ViewBag.Number = o ?? 0;
            int uid = CurrentUser.RoleId == 3 ? CurrentUser.UserID : 0;

            #endregion

            var model = GetRecentRcCrWise(opid, uid);

            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == opid) }).ToList();


            return View(model);
        }

        public ActionResult ServiceCheck(int? o, int? c)
        {
            //LapuMargin
            UpdateActivity("ServiceCheck", "GET:DailyReport/ServiceCheck/");
            ViewBag.actionAllowed = action = ActionAllowed("ServiceCheck", CurrentUser.RoleId);
            #region Search

            int opid = ViewBag.Number = o ?? 1;
            int uid = CurrentUser.RoleId == 3 ? CurrentUser.UserID : 0;

            #endregion

            var model = GetServiceCheck(opid, c ?? 0);

            ViewBag.OperatorList = packageService.GetOperatorList().Where(x => x.OpTypeId == 1 || x.OpTypeId == 2).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == opid) }).ToList();

            ViewBag.CircleList = opSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == c) }).ToList();
            return View(model);
        }

        public ActionResult RecentRechargeOpWise()
        {
            //LapuMargin
            UpdateActivity("LapuMargin", "GET:DailyReport/RecentRechargeOpWise/");
            ViewBag.actionAllowed = action = ActionAllowed("RecentRechargeOpWise", CurrentUser.RoleId);
            #region Search


            int uid = CurrentUser.RoleId == 3 ? CurrentUser.UserID : 0;

            #endregion

            var model = GetRecentRcOpWise(uid);

            //ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == opid) }).ToList();


            return View(model);
        }

        public ActionResult RandomKeyGen()
        {

            UpdateActivity("LapuMargin", "GET:DailyReport/RandomKeyGen/");
            ViewBag.actionAllowed = action = ActionAllowed("RandomKeyGen", CurrentUser.RoleId);

            int uid = CurrentUser.RoleId == 3 ? CurrentUser.UserID : 0;



            return View();
        }

        [HttpPost]
        public string RandomKeyGen(List<RandomKeyGenDto> data)
        {
            List<RandomKeyGenDto> keyList = new List<RandomKeyGenDto>();


            string keylist2 = "<div id='tblResult'><table class='table table-hover table-responsive table-bordered'><tr><th>SRN</th><th>Random Key</th>";

            int id = 1;
            for (int i = 0; i < data.FirstOrDefault().NoOfKeys; i++)
            {
                string rkey = "";

                foreach (var item in data)
                {

                    if (item.KeyTypeId > 0)
                    {
                        int kTypeId = Convert.ToInt32(item.KeyTypeId);
                        int length = kTypeId == 1 ? item.LengthOrText.Length : Convert.ToInt32(item.LengthOrText);
                        string fixtext = kTypeId == 1 ? item.LengthOrText : string.Empty;

                        var partialkey = string.Empty;

                        if (kTypeId == 1)//fix
                        {
                            partialkey = fixtext;
                        }
                        else if (kTypeId == 2)//numeric
                        {
                            partialkey = Common.GetUniqueNumberic(length);
                        }
                        else if (kTypeId == 3)//alpfa-low
                        {
                            partialkey = Common.GetUniqueAlphaticLW(length);
                        }
                        else if (kTypeId == 4)//alpfa-up
                        {
                            partialkey = Common.GetUniqueAlphaticUP(length);

                        }
                        else if (kTypeId == 5)//alpfa-mix
                        {
                            partialkey = Common.GetUniqueAlphaticMix(length);

                        }
                        else if (kTypeId == 6)
                        {
                            partialkey = Common.GetUniqueAlphaNumericLW(length);

                        }
                        else if (kTypeId == 7)
                        {
                            partialkey = Common.GetUniqueAlphaNumericUP(length);

                        }
                        else if (kTypeId == 8)
                        {
                            partialkey = Common.GetUniqueAlphaNumericMIX(length);

                        }

                        rkey = rkey + partialkey;
                    }

                }


                //  keyList.Add(new RandomKeyGenDto() { Id= i+1, RandomKey = rkey });

                keylist2 += "<tr><td>" + id + "</td><td>" + rkey + "</td></tr></div>";
                id++;
            }


            keylist2 += "</table>";


            // var jsonresult = JsonConvert.SerializeObject(keyList); ;

            return keylist2; // jsonresult;
        }

        public ActionResult RcCommOPWise(int? u, int? v, string f = "", string e = "")
        {
            int uid = u ?? 0;
            int apiid = v ?? 0;

            uid = CurrentUser.RoleId != 3 ? uid : CurrentUser.UserID;
            apiid = CurrentUser.RoleId != 3 ? apiid : 0;

            UpdateActivity("Operator DayBook", "GET:DailyReport/RcCommOpWise/");
            ViewBag.actionAllowed = action = ActionAllowed("RcCommOpWise", CurrentUser.RoleId);

            #region Search
            RechargeFilterDto filter = new RechargeFilterDto();

            if (f != "") { TempData["sDate"] = filter.Sdate = f; } else { filter.Sdate = DateTime.Now.ToString("MM/dd/yyyy"); }
            if (e != "") { TempData["eDate"] = filter.Edate = e; } else { filter.Edate = DateTime.Now.ToString("MM/dd/yyyy"); }

            ViewBag.FilterData = filter;
            #endregion
            List<OpWiseProfitDto> dailydto = new List<OpWiseProfitDto>();
            dailydto = GetRcCommOpWise(filter.Sdate, filter.Edate, uid, apiid);   //operator day book sp
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == uid) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == apiid) }).ToList();

            return View(dailydto);
        }

        public ActionResult CircleDayBook(int? o, int? u, int? v, string f = "", string e = "")
        {
            int uid = u ?? 0;
            int apiid = v ?? 0;
            int opid = o ?? 0;

            uid = CurrentUser.RoleId != 3 ? uid : CurrentUser.UserID;
            apiid = CurrentUser.RoleId != 3 ? apiid : 0;

            UpdateActivity("Circle DayBook", "GET:DailyReport/CircleDayBook/");
            ViewBag.actionAllowed = action = ActionAllowed("CircleDayBook", CurrentUser.RoleId);

            #region Search
            RechargeFilterDto filter = new RechargeFilterDto();

            if (f != "") { TempData["sDate"] = filter.Sdate = f; } else { filter.Sdate = DateTime.Now.ToString("MM/dd/yyyy"); }
            if (e != "") { TempData["eDate"] = filter.Edate = e; } else { filter.Edate = DateTime.Now.ToString("MM/dd/yyyy"); }

            ViewBag.FilterData = filter;
            #endregion
            List<dailydto> dailydto = new List<dailydto>();
            dailydto = GetCircleDayBook(filter.Sdate, filter.Edate, uid, apiid, opid);   //operator day book sp
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == uid) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == apiid) }).ToList();
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == opid) }).ToList();

            return View(dailydto);
        }

        public ActionResult DayBookV2(int? u, int? v, int? o, int? c, string f = "", string e = "")
        {
            int uid = u ?? 0;
            int apiid = v ?? 0;
            int opid = o ?? 0;
            int circleid = c ?? 0;
            uid = CurrentUser.RoleId != 3 ? uid : CurrentUser.UserID;
            apiid = CurrentUser.RoleId != 3 ? apiid : 0;

            UpdateActivity("DayBookV2", "GET:DailyReport/DayBookV2/");
            ViewBag.actionAllowed = action = ActionAllowed("DayBookV2", CurrentUser.RoleId);

            #region Search
            RechargeFilterDto filter = new RechargeFilterDto();

            if (f != "") { TempData["sDate"] = filter.Sdate = f; } else { filter.Sdate = DateTime.Now.ToString("MM/dd/yyyy"); }
            if (e != "") { TempData["eDate"] = filter.Edate = e; } else { filter.Edate = DateTime.Now.ToString("MM/dd/yyyy"); }

            ViewBag.FilterData = filter;
            #endregion
            List<dailydto> dailydto = new List<dailydto>();
            dailydto = GetDayBookV2VendorWise(filter.Sdate, filter.Edate, uid, apiid, opid, circleid);   //operator day book sp
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == uid) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == apiid) }).ToList();
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == opid) }).ToList();
            ViewBag.CircleList = opSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == circleid) }).ToList();

            return View(dailydto);
        }

        public ActionResult AmountTraffic(int? u, int? v, int? o, int? c, string f = "", string e = "")
        {
            int uid = u ?? 0;
            int apiid = v ?? 0;
            int opid = o ?? 1;
            int cid = c ?? 0;
            uid = CurrentUser.RoleId != 3 ? uid : CurrentUser.UserID;
            apiid = CurrentUser.RoleId != 3 ? apiid : 0;

            UpdateActivity("Amount Traffic", "GET:DailyReport/AmountTraffic/");
            ViewBag.actionAllowed = action = ActionAllowed("AmountTraffic", CurrentUser.RoleId);

            #region Search
            RechargeFilterDto filter = new RechargeFilterDto();

            if (f != "") { TempData["sDate"] = filter.Sdate = f; } else { filter.Sdate = DateTime.Now.ToString("MM/dd/yyyy"); }
            if (e != "") { TempData["eDate"] = filter.Edate = e; } else { filter.Edate = DateTime.Now.ToString("MM/dd/yyyy"); }

            ViewBag.FilterData = filter;
            #endregion
            List<dailydto> dailydto = new List<dailydto>();
            dailydto = GetTrafficAmountWise(filter.Sdate, filter.Edate, uid, apiid, opid, cid);
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == uid) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == apiid) }).ToList();
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == opid) }).ToList();
            ViewBag.CircleList = opSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == cid) }).ToList();

            return View(dailydto);
        }

        public ActionResult AmountCricleTraffic(int? u, int? v, int? o, int? c, string f = "", string e = "")
        {
            int uid = u ?? 0;
            int apiid = v ?? 0;
            int opid = o ?? 1;
            int cid = c ?? 0;
            uid = CurrentUser.RoleId != 3 ? uid : CurrentUser.UserID;
            apiid = CurrentUser.RoleId != 3 ? apiid : 0;

            UpdateActivity("Amount Cricle Traffic", "GET:DailyReport/AmountCricleTraffic/");
            ViewBag.actionAllowed = action = ActionAllowed("AmountCricleTraffic", CurrentUser.RoleId);

            #region Search
            RechargeFilterDto filter = new RechargeFilterDto();

            if (f != "") { TempData["sDate"] = filter.Sdate = f; } else { filter.Sdate = DateTime.Now.ToString("MM/dd/yyyy"); }
            if (e != "") { TempData["eDate"] = filter.Edate = e; } else { filter.Edate = DateTime.Now.ToString("MM/dd/yyyy"); }

            ViewBag.FilterData = filter;
            #endregion
            List<dailydto> dailydto = new List<dailydto>();
            dailydto = GetTrafficCricleAmountWise(filter.Sdate, filter.Edate, uid, apiid, opid, cid);
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == uid) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == apiid) }).ToList();
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == opid) }).ToList();
            ViewBag.CircleList = opSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == cid) }).ToList();

            return View(dailydto);
        }

        public ActionResult UserDayBookV2(int? u, int? v, int? o, int? c, string f = "", string e = "")
        {
            int uid = u ?? 0;
            int apiid = v ?? 0;
            int opid = o ?? 0;
            int cid = c ?? 0;

            uid = CurrentUser.RoleId != 3 ? uid : CurrentUser.UserID;
            apiid = CurrentUser.RoleId != 3 ? apiid : 0;

            UpdateActivity("UserDayBookV2", "GET:DailyReport/UserDayBookV2/");
            ViewBag.actionAllowed = action = ActionAllowed("UserDayBookV2", CurrentUser.RoleId);

            #region Search
            RechargeFilterDto filter = new RechargeFilterDto();

            if (f != "") { TempData["sDate"] = filter.Sdate = f; } else { filter.Sdate = DateTime.Now.ToString("MM/dd/yyyy"); }
            if (e != "") { TempData["eDate"] = filter.Edate = e; } else { filter.Edate = DateTime.Now.ToString("MM/dd/yyyy"); }

            ViewBag.FilterData = filter;
            #endregion
            List<dailydto> dailydto = new List<dailydto>();
            dailydto = GetDayBookV2UserWise(filter.Sdate, filter.Edate, uid, apiid, opid, cid);   //operator day book sp
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == uid) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == apiid) }).ToList();
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == opid) }).ToList();
            ViewBag.CircleList = opSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == cid) }).ToList();

            return View(dailydto);
        }

        public ActionResult LapuPurchase(long? u, int? v, int? o, int? c, string f = "", string e = "")
        {
            long lapuid = u ?? 0;
            int dealerid = v ?? 0;
            int opid = o ?? 0;
            int cid = c ?? 0;
            lapuid = CurrentUser.RoleId != 3 ? lapuid : CurrentUser.UserID;
            dealerid = CurrentUser.RoleId != 3 ? dealerid : 0;

            UpdateActivity("LapuPurchaseReport", "GET:DailyReport/LapuPurchase/");
            ViewBag.actionAllowed = action = ActionAllowed("LapuPurchase", CurrentUser.RoleId);

            #region Search
            LapuPurFilterDto filter = new LapuPurFilterDto();

            if (f != "") { TempData["sDate"] = filter.StartDate = f; } else { filter.StartDate = DateTime.Now.ToString("MM/dd/yyyy"); }
            if (e != "") { TempData["eDate"] = filter.EndDate = e; } else { filter.EndDate = DateTime.Now.ToString("MM/dd/yyyy"); }

            ViewBag.FilterData = filter;
            #endregion

            ViewBag.DB_Amt = Convert.ToDecimal(0);
            ViewBag.Total = Convert.ToDecimal(0);

            List<LapuPurchageDto> dailydto = new List<LapuPurchageDto>();
            dailydto = GetLapuPurchase(filter.StartDate, filter.EndDate, lapuid, dealerid, opid, cid);   //operator day book sp
            //ViewBag.UserList = dealerService.GetLapuListByDealer(null).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Number + " -" + x.LapuDealer?.Name ?? string.Empty + " -" + x.Operator?.Name ?? string.Empty, Selected = (x.Id == lapuid) }).ToList();
            //ViewBag.ApiList = dealerService.GetDealerList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == dealerid) }).ToList();
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == opid) }).ToList();
            ViewBag.CircleList = opSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == cid) }).ToList();

            return View(dailydto);
        }

        public ActionResult UserDayBookDateWise(int? u, string f = "", string e = "")
        {
            UpdateActivity("User DayBookDateWise", "GET:DailyReport/UserDayBookWise/");
            ViewBag.actionAllowed = action = ActionAllowed("UserDayBookDateWise", CurrentUser.RoleId);

            #region Search

            string date1 = "";
            string date2 = "";
            int uid = CurrentUser.RoleId == 3 ? CurrentUser.UserID : u ?? 0;

            if (f != "") { TempData["sDate"] = date1 = f; } else { date1 = DateTime.Now.ToString("MM/dd/yyyy"); }
            if (e != "") { TempData["eDate"] = date2 = e; } else { date2 = DateTime.Now.ToString("MM/dd/yyyy"); }
            ViewBag.FilterData = date1;
            ViewBag.FilterData2 = date2;
            #endregion

            List<UserDayBookDto> model = new List<UserDayBookDto>();
            model = GetUserDayBookDateWise(uid, date1, date2);
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == uid) }).ToList();

            return View(model);
        }

        // GET: Report
        [HttpGet]
        public ActionResult CompareRc(int? i, int? v, int? s, string rf = "", string vt = "", string m = "", string f = "", string e = "")
        {
            UpdateActivity("CompareRc", "GET:DailyReportReport/CompareRc", string.Empty);
            action = ActionAllowed("CompareRc", CurrentUser.RoleId);

            RechargeFilterDto filter = new RechargeFilterDto();

            filter.Isa = i ?? 0;
            filter.Apiid = v ?? 0;
            filter.Sid = s ?? 0;
            filter.Sdate = f;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyy");
            filter.Edate = e;
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("dd/MM/yyy");
            filter.CustomerNo = m;
            filter.Searchid = rf;
            filter.ApiTxnid = vt;

            ViewBag.FilterData = filter;

            //ViewBag.StatusList = rechargeReportService.GetCompareRcStatusList().Where(x => x.Id > 2).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = ((x.Id == filter.Sid)) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == filter.Apiid) }).ToList();

            return View();
        }

        [HttpPost]
        //public ActionResult GetCompareRcReport(DataTableServerSide model, FormCollection FC)
        //{
        //    string log = "CompareRcFilterDto: ";

        //    UpdateActivity("CompareRc", "POST:DailyReportReport/CompareRc", string.Empty);
        //    action = ActionAllowed("CompareRc", CurrentUser.RoleId);

        //    int userrole = CurrentUser.Roles.FirstOrDefault();
        //    bool IsAdminRole = (userrole != 3) ? true : false;
        //    model.filterdata.UserId = IsAdminRole ? model.filterdata.UserId : CurrentUser.UserID;

        //    KeyValuePair<int, List<CompareRc>> CompareRcs = new KeyValuePair<int, List<CompareRc>>();

        //    CompareRcs = rechargeReportService.GetCompareRcReport(model);

        //    return Json(new
        //    {
        //        draw = model.draw,
        //        recordsTotal = CompareRcs.Key,
        //        recordsFiltered = CompareRcs.Key,
        //        data = CompareRcs.Value.Select(c => new List<object> {
        //            c.Id,
        //            c.RecDate?.ToString("dd/MM/yyyy"),
        //            c.ApiSource?.ApiName??string.Empty,
        //            c.CustomerNo,
        //            c.Amount,
        //            c.StatusTxt,
        //            c.OurRefId,
        //            c.ApiTxnId,
        //            c.CompareRcStatu?.Name??string.Empty,
        //            c.AddedDate?.ToString("dd/MM/yyyy hh:mm:ss tt")??string.Empty
        //            })
        //    }, JsonRequestBehavior.AllowGet);

        //}

        // GET: Report
        [HttpGet]
        public ActionResult CompareRcVendor(int? i, int? v, string f = "", string e = "")
        {
            UpdateActivity("CompareRcApi", "GET:DailyReport/CompareRcApi", string.Empty);
            action = ActionAllowed("CompareRcApi", CurrentUser.RoleId);

            RechargeFilterDto filter = new RechargeFilterDto();

            filter.Isa = i ?? 0;
            filter.Apiid = v ?? 0;
            filter.Sdate = f;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyy");
            filter.Edate = e;
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("dd/MM/yyy");


            ViewBag.FilterData = filter;

            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == filter.Apiid) }).ToList();

            return View();
        }

        //[HttpPost]
        //public ActionResult GetCompareRcVendorReport(DataTableServerSide model, FormCollection FC)
        //{
        //    string log = "CompareRcApiFilterDto: ";

        //    UpdateActivity("CompareRcApi", "POST:DailyReport/CompareRcApi", string.Empty);
        //    action = ActionAllowed("CompareRcApi", CurrentUser.RoleId);

        //    int userrole = CurrentUser.Roles.FirstOrDefault();
        //    bool IsAdminRole = (userrole != 3) ? true : false;
        //    model.filterdata.UserId = IsAdminRole ? model.filterdata.UserId : CurrentUser.UserID;

        //    KeyValuePair<int, List<CompareRcApi>> CompareRcApis = new KeyValuePair<int, List<CompareRcApi>>();

        //    CompareRcApis = rechargeReportService.GetCompareRcApiReport(model);

        //    LogActivity(log);
        //    return Json(new
        //    {
        //        draw = model.draw,
        //        recordsTotal = CompareRcApis.Key,
        //        recordsFiltered = CompareRcApis.Key,
        //        data = CompareRcApis.Value.Select(c => new List<object> {
        //            c.Id,
        //            c.RecDate?.ToString("dd/MM/yyyy"),
        //            c.ApiSource.ApiName,
        //            DataTableButton.ButtonDownload(c.FilesPath, c.FilesName, "Download "+c.FilesName) +"&nbsp;&nbsp"+
        //            DataTableButton.RefreshVbalButton(Url.Action("CompareRcVendor", "dailyreport"),"Compare Again"),
        //            c.MissMatchCount,
        //            c.MatchCount,
        //            c.TotalCount,
        //            c.RcCount,
        //            c.UploadCount,
        //            (c.UpdatedDate?.ToString("dd/MM/yyyy hh:mm:ss tt"))??(c.AddedDate?.ToString("dd/MM/yyyy hh:mm:ss  tt")??string.Empty),
        //            (c.User1?.UserProfile?.FullName)?? (c.User?.UserProfile?.FullName??string.Empty),
        //            })
        //    }, JsonRequestBehavior.AllowGet);

        //}

        //public ActionResult DealerDayBook(int? d, string f = "", string e = "")
        //{
        //    UpdateActivity("DailyReport DealerDayBook", "GET:DailyReport/DealerDayBook/");
        //    ViewBag.actionAllowed = action = ActionAllowed("DealerDayBook", CurrentUser.RoleId);

        //    #region Search

        //    string date1 = "";
        //    string date2 = "";
        //    int uid = d ?? 0;

        //    if (f != "") { TempData["sDate"] = date1 = f; } else { date1 = DateTime.Now.ToString("MM/dd/yyyy"); }
        //    if (e != "") { TempData["eDate"] = date2 = e; } else { date2 = DateTime.Now.ToString("MM/dd/yyyy"); }
        //    ViewBag.StartDate = date1;
        //    ViewBag.EndDate = date2;
        //    #endregion

        //    List<DealerDayBookDto> model = new List<DealerDayBookDto>();
        //    model = GetDealerDayBook(uid, date1, date2);
        //    //ViewBag.DealerList = dealerService.GetDealerList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name ?? "NA", Selected = (x.Id == uid) }).ToList();

        //    return View(model);
        //}

        //public ActionResult DealerDayBookDateWise(int? d, string f = "", string e = "")
        //{
        //    UpdateActivity("DailyReport DealerDayBookDateWise", "GET:DailyReport/DealerDayBookDateWise/");
        //    ViewBag.actionAllowed = action = ActionAllowed("DealerDayBookDateWise", CurrentUser.RoleId);

        //    #region Search

        //    string date1 = "";
        //    string date2 = "";
        //    int uid = d ?? 0;

        //    if (f != "") { TempData["sDate"] = date1 = f; } else { date1 = DateTime.Now.ToString("MM/dd/yyyy"); }
        //    if (e != "") { TempData["eDate"] = date2 = e; } else { date2 = DateTime.Now.ToString("MM/dd/yyyy"); }
        //    ViewBag.StartDate = date1;
        //    ViewBag.EndDate = date2;
        //    #endregion

        //    List<DealerDayBookDto> model = new List<DealerDayBookDto>();
        //    model = GetDealerDayBook(uid, date1, date2, "datewise");
        //    //ViewBag.DealerList = dealerService.GetDealerList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name ?? "NA", Selected = (x.Id == uid) }).ToList();

        //    return View(model);
        //}     


       
        [HttpGet]
        public ActionResult UserFilterRuleReport(int? i, int? u, int? v, int? o, int? c, decimal? a, string f = "", string e = "")
        {
            UpdateActivity("UserFilterRuleReport", "GET:DailyReport/UserFilterRuleReport", string.Empty);
            action = ActionAllowed("UserFilterRuleReport", CurrentUser.RoleId);

            UserRulesFilterDto filter = new UserRulesFilterDto();

            filter.Isa = i ?? 0;
            filter.Uid = u ?? 0;
            filter.Apiid = v ?? 0;
            filter.Opid = o ?? 0;
            filter.Circleid = c ?? 0;
            filter.Amount = a ?? 0;
            filter.Sdate = f;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.AddDays(-3).ToString("dd/MM/yyy");
            filter.Edate = e;
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("dd/MM/yyy");

            ViewBag.FilterData = filter;

            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == u) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == v) }).ToList();
            ViewBag.CircleList = opSwitchService.circlesList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.CircleName, Selected = (x.Id == c) }).ToList();
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == o) }).ToList();

            return View();
        }

        [HttpPost]
        public ActionResult GetUserFilterRuleReport(DataTableServerSide model, FormCollection FC)
        {

            UpdateActivity("UserFilterRule", "POST:DailyReportReport/UserFilterRule", string.Empty);
            action = ActionAllowed("UserFilterRule", CurrentUser.RoleId);

            KeyValuePair<int, List<UserFilterRuleReport>> UserFilterRules = new KeyValuePair<int, List<UserFilterRuleReport>>();

            UserFilterRules = userFilterRuleReportService.GetUserFilterRuleReport(model);

            return Json(new
            {
                draw = model.draw,
                recordsTotal = UserFilterRules.Key,
                recordsFiltered = UserFilterRules.Key,
                data = UserFilterRules.Value.Select(c => new List<object> {
                    c.Id,
                    c.AddedDate?.ToString("dd/MM/yyyy hh:mm:ss"),
                    c.User2?.UserProfile?.FullName?? string.Empty,
                    c.Circle?.CircleName??string.Empty,
                    c.Operator?.Name??string.Empty,
                    c.AmtPercent??0,
                    c.Amount??0,
                    c.Roffer??0,
                    c.ApiSource?.ApiName??string.Empty,
                    c.TotalCount??0,
                    c.TotalAmount??0,
                    c.RofferCount??0,
                    c.RofferRcAmount??0
                    })
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult OperatorGenrateUserWithVendor(int? opid, int? UserId, int? VendorId)
        {
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == opid) }).ToList();
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == UserId) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == VendorId) }).ToList();
            RechargeFilterDto ft = new RechargeFilterDto()
            {
                Uid = UserId ?? 0,
                Apiid = VendorId ?? 0,
                Opid = opid ?? 0
            };
            ViewBag.FilterData = TempData["RechargeFilterDto"] = ft;
            return View();
        }

        [HttpPost]
        public ActionResult GetOperatorGenrateReport(DataTableServerSide model)
        {
            string log = string.Empty;
            RechargeFilterDto flt = TempData["RechargeFilterDto"] != null ? (RechargeFilterDto)TempData["RechargeFilterDto"] : new RechargeFilterDto();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = flt;

            int userrole = CurrentUser.RoleId;
            bool IsAdminRole = (userrole != 3) ? true : false;
            model.filterdata.UserId = IsAdminRole ? model.filterdata.UserId : CurrentUser.UserID;

            KeyValuePair<int, List<OperatorKeyGenrate>> recharges = new KeyValuePair<int, List<OperatorKeyGenrate>>();
            try
            {
                recharges = rechargeReportService.GetOperatorKeyGenrateReport(model, flt.Uid, flt.Apiid, flt.Opid);

            }
            catch (Exception ex)
            {

                LogException(ex, "recharge report");
                try
                {
                    recharges = rechargeReportService.GetOperatorKeyGenrateReport(model, flt.Uid, flt.Apiid, flt.Opid);
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
                   c.Id,
                    c.User?.UserProfile?.FullName??string.Empty,
                    c.Operator?.Name??"",
                    c.ApiSource?.ApiName??"",
                    c.NoLength,
                    c.KeyTypeId,
                    c.TextLength,
                    c.IsActive,
                    DataTableButton.EditButton(Url.Action( "OpGenrate", "dailyReport",new { opid = c.OpId,UserId=c.Userid,VendorId=c.VendorId }))+"&nbsp;"+
                    DataTableButton.DeleteButton(Url.Action( "delete","dailyReport", new { ID = c.Userid,VendorId=c.VendorId,OpId=c.OpId }),"modal-delete-adminrole")
                    })
            }, JsonRequestBehavior.AllowGet);


        }
        public ActionResult OpGenrate(int? opid, int? UserId, int? VendorId)
        {
            UpdateActivity("LapuMargin", "GET:DailyReport/RandomKeyGen/");
            ViewBag.actionAllowed = action = ActionAllowed("RandomKeyGen", CurrentUser.RoleId);
            int uid = CurrentUser.RoleId == 3 ? CurrentUser.UserID : 0;
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == opid) }).ToList();
            ViewBag.UserList = userService.GetUserList().Where(x => x.RoleId == 3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? "NA", Selected = (x.Id == UserId) }).ToList();
            ViewBag.ApiList = apiService.GetApiList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName, Selected = (x.Id == VendorId) }).ToList();
            OperatorGenrateListdto operatorGenrateListdto = new OperatorGenrateListdto();
            if (opid != 0)
            {
                List<OperatorGenatedto> Operatorlist = new List<OperatorGenatedto>();
                var data = rechargeReportService.GetOperatorKeyGenrateList(opid ?? 0, UserId ?? 0, VendorId ?? 0);
                foreach (var d in data)
                {
                    OperatorGenatedto operatorGenatedto = new OperatorGenatedto
                    {
                        Id = d.Id,
                        KeyTypeId = d.KeyTypeId ?? 0,
                        NoLength = d.NoLength ?? 0,
                        OpId = d.OpId ?? 0,
                        TextLength = d.TextLength
                    };
                    Operatorlist.Add(operatorGenatedto);
                }
                operatorGenrateListdto.OperatorGenateList = Operatorlist;
            }
            return View(operatorGenrateListdto);
        }

        [HttpPost]
        public int OpGenrateList(List<RandomKeyGenDto> data)
        {
            List<RandomKeyGenDto> keyList = new List<RandomKeyGenDto>();
            List<OperatorKeyGenrate> operatorKeyGenrates = new List<OperatorKeyGenrate>();
            foreach (var item in data)
            {
                if (item.KeyTypeId > 0)
                {
                    OperatorKeyGenrate operatorKeyGenrate = rechargeReportService.OperatorKeyGenrateGetData(item.OpId, item.UserId, item.Apiid,item.KeyTypeId) ?? new OperatorKeyGenrate();
                    operatorKeyGenrate.OpId = item.OpId;
                    operatorKeyGenrate.NoLength = item.NoOfKeys;
                    operatorKeyGenrate.KeyTypeId = item.KeyTypeId;
                    operatorKeyGenrate.TextLength = item.LengthOrText;
                    operatorKeyGenrate.Userid = item.UserId;
                    operatorKeyGenrate.VendorId = item.Apiid;
                    operatorKeyGenrate.IsActive = true;
                    operatorKeyGenrates.Add(operatorKeyGenrate);
                    rechargeReportService.Save(operatorKeyGenrate);
                }
            }

            return 1; // jsonresult;
        }

        [HttpGet]
        public ActionResult Delete(int ID, int VendorID,int OpId)
        {
            return PartialView("_OperatorUserDelete", new Modal
            {
                Message = "Are you sure you want to delete this Permission All Record UserId and Api?",
                Size = ModalSize.Small,
                Header = new ModalHeader { Heading = "Delete Permission" },
                Footer = new ModalFooter { SubmitButtonText = "Yes", CancelButtonText = "No" }
            });
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteAdminUser(int ID, int VendorID,int OpId)
        {
            try
            {
                //var adminUser = adminUserService.GetAdminUser(id);
                //adminUser.IsActive = false;
                //adminUserService.Save(adminUser);
                OpertaorDeleteActive(ID, VendorID, OpId, 1);
                ShowSuccessMessage("Success", "Permission have be deleted.", false);
                //ShowSuccessMessage("Success", "hola", false);
            }
            catch (Exception)
            {
                //ShowErrorMessage("Error Occurred", "", false);
            }
            return RedirectToAction("OperatorGenrateUserWithVendor", "DailyReport");
        }
        public bool Active(int id)
        {
            string message = string.Empty;
            try
            {
                OpertaorDeleteActive(id, 0, 0, 2);
                return true;

            }
            catch (Exception)
            {
                return false;
            }


        }

        #endregion

        #region SP CALL

        public List<dailydto> DailyOperator(string date, string Edate, int userid = 0, int apiid = 0)
        {
            List<dailydto> dailydtos = new List<dailydto>();
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("use_GetOperatorDaybook", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", date);
                    cmd.Parameters.AddWithValue("@edate", Edate);
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@apiid", apiid);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dailydto operatordayreport = new dailydto();
                        operatordayreport.Name = dt.Rows[i]["name"].ToString();
                        operatordayreport.SuccessAmount = dt.Rows[i]["SuccessAmount"].ToString();
                        operatordayreport.SuccessCount = dt.Rows[i]["SuccessCount"].ToString();
                        operatordayreport.ProcessingAmount = dt.Rows[i]["ProcessingAmount"].ToString();
                        operatordayreport.ProcessingCount = dt.Rows[i]["ProcessingCount"].ToString();
                        operatordayreport.FailedAmount = dt.Rows[i]["FailedAmount"].ToString();
                        operatordayreport.FailedCount = dt.Rows[i]["FailedCount"].ToString();
                        operatordayreport.HoldAmount = dt.Rows[i]["HoldAmount"].ToString();
                        operatordayreport.HoldCount = dt.Rows[i]["HoldCount"].ToString();

                        dailydtos.Add(operatordayreport);
                    }
                    ViewBag.SuccessAmount = dt.Compute("Sum(SuccessAmount)", string.Empty).ToString();
                    ViewBag.SuccessCount = dt.Compute("Sum(SuccessCount)", string.Empty).ToString();
                    ViewBag.ProcessingAmount = dt.Compute("Sum(ProcessingAmount)", string.Empty).ToString();
                    ViewBag.ProcessingCount = dt.Compute("Sum(ProcessingCount)", string.Empty).ToString();
                    ViewBag.FailedAmount = dt.Compute("Sum(FailedAmount)", string.Empty).ToString();
                    ViewBag.FailedCount = dt.Compute("Sum(FailedCount)", string.Empty).ToString();
                    ViewBag.HoldAmount = dt.Compute("Sum(HoldAmount)", string.Empty).ToString();
                    ViewBag.HoldCount = dt.Compute("Sum(HoldCount)", string.Empty).ToString();

                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return dailydtos;
        }

        public List<dailydto> UserDailyOperator(int Userid, string Edate)
        {
            List<dailydto> dailydtos = new List<dailydto>();
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("UserDayBook", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@userid", Userid);
                    cmd.Parameters.AddWithValue("@date", Edate);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dailydto operatordayreport = new dailydto();
                        operatordayreport.DateTime = String.Format("{0:dd/MM/yyyy}", dt.Rows[i]["Rcdate"]);

                        operatordayreport.SuccessAmount = dt.Rows[i]["SuccessAmount"].ToString();
                        operatordayreport.SuccessCount = dt.Rows[i]["SuccessCount"].ToString();
                        operatordayreport.ProcessingAmount = dt.Rows[i]["ProcessingAmount"].ToString();
                        operatordayreport.ProcessingCount = dt.Rows[i]["ProcessingCount"].ToString();
                        operatordayreport.FailedAmount = dt.Rows[i]["FailedAmount"].ToString();
                        operatordayreport.FailedCount = dt.Rows[i]["FailedCount"].ToString();
                        operatordayreport.HoldAmount = dt.Rows[i]["HoldAmount"].ToString();
                        operatordayreport.HoldCount = dt.Rows[i]["HoldCount"].ToString();
                        operatordayreport.OpeningBal = dt.Rows[i]["Openingbal"].ToString();
                        operatordayreport.Closingbal = dt.Rows[i]["Closingbal"].ToString();
                        operatordayreport.WalletBalance = dt.Rows[i]["WalletAmount"].ToString();
                        operatordayreport.TotalCheckBalance = dt.Rows[i]["Totalcheck"].ToString();
                        operatordayreport.DiffBalance = dt.Rows[i]["diff"].ToString();
                        operatordayreport.Debitamt = dt.Rows[i]["Debitamt"].ToString();
                        dailydtos.Add(operatordayreport);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return dailydtos;
        }

        public List<UserDayBookDto> GetUserDayBook(int Userid, string Sdate, string Edate)
        {
            List<UserDayBookDto> mList = new List<UserDayBookDto>();
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("usp_GetUserDayBook", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", Userid);
                    cmd.Parameters.AddWithValue("@sDate", Sdate);
                    cmd.Parameters.AddWithValue("@eDate", Edate);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    sqlDataAdapter.SelectCommand.CommandTimeout = 1800;
                    //cmd.CommandTimeout = 30;
                    DataTable dt = new DataTable();
                    con.Open();
                    sqlDataAdapter.Fill(dt);
                    con.Close();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        UserDayBookDto model = new UserDayBookDto();
                        //    model.TxnDate = String.Format("{0:MM/dd/yyyy}", dt.Rows[i]["TxnDate"]);

                        model.UserId = Convert.ToInt32(dt.Rows[i]["UserId"]);
                        model.UserName = model.UserId + "- " + dt.Rows[i]["UserName"]?.ToString() ?? string.Empty;
                        model.RC_Amt = dt.Rows[i]["RC_Amt"]?.ToString() ?? "0.00";
                        model.OP_Bal = dt.Rows[i]["OP_Bal"]?.ToString() ?? "0.00";
                        model.WR_Amt = dt.Rows[i]["WR_Amt"]?.ToString() ?? "0.00";
                        model.DB_Amt = dt.Rows[i]["DB_Amt"]?.ToString() ?? "0.00";
                        model.CM_Amt = dt.Rows[i]["CM_Amt"]?.ToString() ?? "0.00";
                        model.CL_Bal = dt.Rows[i]["CL_Bal"]?.ToString() ?? "0.00";
                        model.Calc_Bal = dt.Rows[i]["Calc_Bal"]?.ToString() ?? "0.00";
                        model.CL_Diff = dt.Rows[i]["CL_Diff"]?.ToString() ?? "0.00";

                        model.Old_fRc_Amt = dt.Rows[i]["fRcAmount"]?.ToString() ?? "0.00";
                        model.Old_sRc_Amt = dt.Rows[i]["sRcAmount"]?.ToString() ?? "0.00";
                        model.Old_CR_Amt = dt.Rows[i]["fCR_Amt"]?.ToString() ?? "0.00";
                        model.Old_DB_Amt = dt.Rows[i]["sDB_Amt"]?.ToString() ?? "0.00";
                        model.Total_Surcharge = dt.Rows[i]["TolalSurcharge"]?.ToString() ?? "0.00";
                        model.Total_Discount = dt.Rows[i]["TolalDiscount"]?.ToString() ?? "0.00";
                        mList.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return mList;
        }

        public List<ApiDayBookDto> GetApiDayBook(int Userid, string Sdate, string Edate)
        {
            List<ApiDayBookDto> mList = new List<ApiDayBookDto>();
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_GetApiDayBook", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", Userid);
                    cmd.Parameters.AddWithValue("@sDate", Sdate);
                    cmd.Parameters.AddWithValue("@eDate", Edate);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ApiDayBookDto model = new ApiDayBookDto();
                        //    model.TxnDate = String.Format("{0:dd/MM/yyyy}", dt.Rows[i]["TxnDate"]);

                        model.ApiId = Convert.ToInt32(dt.Rows[i]["ApiId"]);
                        model.ApiName = model.ApiId + "- " + dt.Rows[i]["ApiName"]?.ToString() ?? string.Empty;
                        model.RC_Amt = dt.Rows[i]["RC_Amt"]?.ToString() ?? "0.00";
                        model.OP_Bal = dt.Rows[i]["OP_Bal"]?.ToString() ?? "0.00";
                        model.WR_Amt = dt.Rows[i]["WR_Amt"]?.ToString() ?? "0.00";
                        model.DB_Amt = dt.Rows[i]["DB_Amt"]?.ToString() ?? "0.00";
                        model.CM_Amt = dt.Rows[i]["CM_Amt"]?.ToString() ?? "0.00";
                        model.Ins_Amt = dt.Rows[i]["Ins_Amt"]?.ToString() ?? "0.00";
                        model.CL_Bal = dt.Rows[i]["CL_Bal"]?.ToString() ?? "0.00";
                        model.Calc_Bal = dt.Rows[i]["Calc_Bal"]?.ToString() ?? "0.00";
                        model.CL_Diff = dt.Rows[i]["CL_Diff"]?.ToString() ?? "0.00";

                        mList.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return mList;
        }

        public List<LapuDayBookDto> GetLapuDayBook(int Userid, string Sdate, string Edate)
        {
            List<LapuDayBookDto> mList = new List<LapuDayBookDto>();
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_GetLapuDayBook", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", Userid);
                    cmd.Parameters.AddWithValue("@sDate", Sdate);
                    cmd.Parameters.AddWithValue("@eDate", Edate);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        LapuDayBookDto model = new LapuDayBookDto();
                        // model.TxnDate = String.Format("{0:dd/MM/yyyy}", dt.Rows[i]["TxnDate"]);

                        model.LapuId = Convert.ToInt32(dt.Rows[i]["LapuId"]);
                        model.LapuNumber = dt.Rows[i]["Number"]?.ToString() ?? string.Empty;
                        model.RC_Amt = dt.Rows[i]["RC_Amt"]?.ToString() ?? "0.00";
                        model.OP_Bal = dt.Rows[i]["OP_Bal"]?.ToString() ?? "0.00";
                        model.WR_Amt = dt.Rows[i]["WR_Amt"]?.ToString() ?? "0.00";
                        model.DB_Amt = dt.Rows[i]["DB_Amt"]?.ToString() ?? "0.00";
                        model.CM_Amt = dt.Rows[i]["CM_Amt"]?.ToString() ?? "0.00";
                        model.Ins_Amt = dt.Rows[i]["Ins_Amt"]?.ToString() ?? "0.00";
                        model.RO_Amt = dt.Rows[i]["RO_Amt"]?.ToString() ?? "0.00";
                        model.CL_Bal = dt.Rows[i]["CL_Bal"]?.ToString() ?? "0.00";
                        model.Calc_Bal = dt.Rows[i]["Calc_Bal"]?.ToString() ?? "0.00";
                        model.CL_Diff = dt.Rows[i]["CL_Diff"]?.ToString() ?? "0.00";

                        mList.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return mList;
        }

        public List<RechargeDetail> GetRechargeByCustomerNo(int userid, string number)
        {
            List<RechargeDetail> mList = new List<RechargeDetail>();

            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    // SqlCommand cmd = new SqlCommand("usp_GetRecListByNumber", con);
                    SqlCommand cmd = new SqlCommand("usp_GetRecListByNumber1", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userid);
                    cmd.Parameters.AddWithValue("@number", number);
                    cmd.Parameters.AddWithValue("@Remark", "All");
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        RechargeDetail model = new RechargeDetail();
                        //    model.TxnDate = String.Format("{0:MM/dd/yyyy}", dt.Rows[i]["TxnDate"]);
                        model.RecId = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["RecId"])) ? Convert.ToInt64(dt.Rows[i]["RecId"]) : 0;
                        model.CustomerNo = dt.Rows[i]["Number"]?.ToString() ?? string.Empty;
                        model.OperatorName = dt.Rows[i]["Operator"]?.ToString() ?? string.Empty;
                        model.Amount = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Amount"])) ? Convert.ToDecimal(dt.Rows[i]["Amount"]) : 0;
                        model.StatusName = dt.Rows[i]["Status"]?.ToString() ?? string.Empty;
                        model.RecDate = dt.Rows[i]["RecDate"]?.ToString() ?? string.Empty;
                        model.TxnId = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["TxnId"])) ? Convert.ToInt64(dt.Rows[i]["TxnId"]) : 0;
                        model.UserTxnId = dt.Rows[i]["UserTxnId"]?.ToString() ?? string.Empty;
                        model.OurRefTxnId = dt.Rows[i]["OurRefTxnId"]?.ToString() ?? string.Empty;
                        model.ApiTxnId = dt.Rows[i]["ApiTxnId"]?.ToString() ?? string.Empty;
                        model.OptTxnId = dt.Rows[i]["OptTxnId"]?.ToString() ?? string.Empty;
                        model.StatusMsg = dt.Rows[i]["StatusMsg"]?.ToString() ?? string.Empty;
                        model.UserName = dt.Rows[i]["UserName"]?.ToString() ?? string.Empty;
                        model.ApiName = dt.Rows[i]["VendorName"]?.ToString() ?? string.Empty;
                        model.CircleName = dt.Rows[i]["CircleName"]?.ToString() ?? string.Empty;
                        model.ROffer = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["ROfferAmount"])) ? Convert.ToDecimal(dt.Rows[i]["ROfferAmount"]) : 0;
                        model.AccountOther = dt.Rows[i]["AccountOther"]?.ToString() ?? string.Empty;
                        model.Optional1 = dt.Rows[i]["Optionals"]?.ToString() ?? string.Empty;
                        model.IsResend = dt.Rows[i]["IsResend"]?.ToString() ?? string.Empty;
                        model.ResendTime = dt.Rows[i]["ResendTime"]?.ToString() ?? string.Empty;
                        model.ResendByName = dt.Rows[i]["ResendBy"]?.ToString() ?? string.Empty;
                        model.UpdatedByName = dt.Rows[i]["UpdatedBy"]?.ToString() ?? string.Empty;
                        model.LapuNo = dt.Rows[i]["LapuNo"]?.ToString() ?? string.Empty;
                        model.DB_Amt = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["DB_Amt"])) ? Convert.ToDecimal(dt.Rows[i]["DB_Amt"]) : 0;
                        model.UpdatedDate = dt.Rows[i]["UpdatedDate"]?.ToString() ?? string.Empty;
                        model.ComplaintId = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["complaintId"])) ? Convert.ToInt64(dt.Rows[i]["complaintId"]) : 0;
                        mList.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return mList;
        }

        public List<RechargeDetail> GetRecentRcCrWise(int opid, int userid)
        {
            List<RechargeDetail> mList = new List<RechargeDetail>();

            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_RecentRechargeCrWise", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OpId", opid);
                    cmd.Parameters.AddWithValue("@UserId", userid);


                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        RechargeDetail model = new RechargeDetail();
                        //    model.TxnDate = String.Format("{0:MM/dd/yyyy}", dt.Rows[i]["TxnDate"]);

                        model.RecId = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["RecId"])) ? Convert.ToInt64(dt.Rows[i]["RecId"]) : 0;
                        model.CustomerNo = dt.Rows[i]["Number"]?.ToString() ?? string.Empty;
                        model.OperatorName = dt.Rows[i]["Operator"]?.ToString() ?? string.Empty;
                        model.Amount = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Amount"])) ? Convert.ToDecimal(dt.Rows[i]["Amount"]) : 0;
                        model.StatusName = dt.Rows[i]["Status"]?.ToString() ?? string.Empty;
                        model.RecDate = dt.Rows[i]["RecDate"]?.ToString() ?? string.Empty;
                        model.TxnId = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["TxnId"])) ? Convert.ToInt64(dt.Rows[i]["TxnId"]) : 0;
                        model.UserTxnId = dt.Rows[i]["UserTxnId"]?.ToString() ?? string.Empty;
                        model.OurRefTxnId = dt.Rows[i]["OurRefTxnId"]?.ToString() ?? string.Empty;
                        model.ApiTxnId = dt.Rows[i]["ApiTxnId"]?.ToString() ?? string.Empty;
                        model.OptTxnId = dt.Rows[i]["OptTxnId"]?.ToString() ?? string.Empty;
                        model.StatusMsg = dt.Rows[i]["StatusMsg"]?.ToString() ?? string.Empty;
                        model.UserName = dt.Rows[i]["UserName"]?.ToString() ?? string.Empty;
                        model.ApiName = dt.Rows[i]["VendorName"]?.ToString() ?? string.Empty;
                        model.CircleName = dt.Rows[i]["CircleName"]?.ToString() ?? string.Empty;
                        model.ROffer = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["ROfferAmount"])) ? Convert.ToDecimal(dt.Rows[i]["ROfferAmount"]) : 0;
                        model.AccountOther = dt.Rows[i]["AccountOther"]?.ToString() ?? string.Empty;
                        model.Optional1 = dt.Rows[i]["Optionals"]?.ToString() ?? string.Empty;
                        model.IsResend = dt.Rows[i]["IsResend"]?.ToString() ?? string.Empty;
                        model.ResendTime = dt.Rows[i]["ResendTime"]?.ToString() ?? string.Empty;
                        model.ResendByName = dt.Rows[i]["ResendBy"]?.ToString() ?? string.Empty;
                        model.UpdatedByName = dt.Rows[i]["UpdatedBy"]?.ToString() ?? string.Empty;
                        model.LapuNo = dt.Rows[i]["LapuNo"]?.ToString() ?? string.Empty;
                        model.DB_Amt = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["DB_Amt"])) ? Convert.ToDecimal(dt.Rows[i]["DB_Amt"]) : 0;
                        model.UpdatedDate = dt.Rows[i]["UpdatedDate"]?.ToString() ?? string.Empty;
                        mList.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return mList;
        }

        public List<RechargeDetail> GetServiceCheck(int opid, int CircleId)
        {
            List<RechargeDetail> mList = new List<RechargeDetail>();

            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_ServiceCrWise", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@OpId", opid);
                    cmd.Parameters.AddWithValue("@Circle", CircleId);


                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        RechargeDetail model = new RechargeDetail();
                        //    model.TxnDate = String.Format("{0:MM/dd/yyyy}", dt.Rows[i]["TxnDate"]);
                        model.CircleName = dt.Rows[i]["CircleName"]?.ToString() ?? string.Empty;

                        model.OperatorName = dt.Rows[i]["Operator"]?.ToString() ?? string.Empty;
                        model.StatusName = dt.Rows[i]["Status"]?.ToString() ?? string.Empty;
                        mList.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return mList;
        }

        public List<RechargeDetail> GetRecentRcOpWise(int userid)
        {
            List<RechargeDetail> mList = new List<RechargeDetail>();

            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_RecentRechargeOpWise", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userid);
                    cmd.Parameters.AddWithValue("@Remark", "All");


                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        RechargeDetail model = new RechargeDetail();
                        //    model.TxnDate = String.Format("{0:MM/dd/yyyy}", dt.Rows[i]["TxnDate"]);

                        model.RecId = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["RecId"])) ? Convert.ToInt64(dt.Rows[i]["RecId"]) : 0;
                        model.CustomerNo = dt.Rows[i]["Number"]?.ToString() ?? string.Empty;
                        model.OperatorName = dt.Rows[i]["Operator"]?.ToString() ?? string.Empty;
                        model.Amount = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["Amount"])) ? Convert.ToDecimal(dt.Rows[i]["Amount"]) : 0;
                        model.StatusName = dt.Rows[i]["Status"]?.ToString() ?? string.Empty;
                        model.RecDate = dt.Rows[i]["RecDate"]?.ToString() ?? string.Empty;
                        model.TxnId = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["TxnId"])) ? Convert.ToInt64(dt.Rows[i]["TxnId"]) : 0;
                        model.UserTxnId = dt.Rows[i]["UserTxnId"]?.ToString() ?? string.Empty;
                        model.OurRefTxnId = dt.Rows[i]["OurRefTxnId"]?.ToString() ?? string.Empty;
                        model.ApiTxnId = dt.Rows[i]["ApiTxnId"]?.ToString() ?? string.Empty;
                        model.OptTxnId = dt.Rows[i]["OptTxnId"]?.ToString() ?? string.Empty;
                        model.StatusMsg = dt.Rows[i]["StatusMsg"]?.ToString() ?? string.Empty;
                        model.UserName = dt.Rows[i]["UserName"]?.ToString() ?? string.Empty;
                        model.ApiName = dt.Rows[i]["VendorName"]?.ToString() ?? string.Empty;
                        model.CircleName = dt.Rows[i]["CircleName"]?.ToString() ?? string.Empty;
                        model.ROffer = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["ROfferAmount"])) ? Convert.ToDecimal(dt.Rows[i]["ROfferAmount"]) : 0;
                        model.AccountOther = dt.Rows[i]["AccountOther"]?.ToString() ?? string.Empty;
                        model.Optional1 = dt.Rows[i]["Optionals"]?.ToString() ?? string.Empty;
                        model.IsResend = dt.Rows[i]["IsResend"]?.ToString() ?? string.Empty;
                        model.ResendTime = dt.Rows[i]["ResendTime"]?.ToString() ?? string.Empty;
                        model.ResendByName = dt.Rows[i]["ResendBy"]?.ToString() ?? string.Empty;
                        model.UpdatedByName = dt.Rows[i]["UpdatedBy"]?.ToString() ?? string.Empty;
                        model.LapuNo = dt.Rows[i]["LapuNo"]?.ToString() ?? string.Empty;
                        model.DB_Amt = !string.IsNullOrEmpty(Convert.ToString(dt.Rows[i]["DB_Amt"])) ? Convert.ToDecimal(dt.Rows[i]["DB_Amt"]) : 0;
                        model.UpdatedDate = dt.Rows[i]["UpdatedDate"]?.ToString() ?? string.Empty;
                        mList.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return mList;
        }

        public List<OpWiseProfitDto> GetRcCommOpWise(string date, string Edate, int userid = 0, int apiid = 0)
        {
            List<OpWiseProfitDto> dailydtos = new List<OpWiseProfitDto>();
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_GetMarginRCOperatorWise", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", date);
                    cmd.Parameters.AddWithValue("@edate", Edate);
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@apiid", apiid);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        OpWiseProfitDto operatordayreport = new OpWiseProfitDto();
                        operatordayreport.OpId = dt.Rows[i]["cnt"].ToString();
                        operatordayreport.OpName = dt.Rows[i]["name"].ToString();
                        operatordayreport.RC_Amt = dt.Rows[i]["RcAmount"].ToString();
                        operatordayreport.User_DB_Amt = dt.Rows[i]["UserComm"].ToString();
                        operatordayreport.Api_DB_Amt = dt.Rows[i]["ApiComm"].ToString();
                        operatordayreport.Profit = dt.Rows[i]["CommDiff"].ToString();
                        operatordayreport.ROffer = dt.Rows[i]["ROffer"].ToString();

                        dailydtos.Add(operatordayreport);
                    }

                    ViewBag.RcAmount = dt.Compute("Sum(RcAmount)", string.Empty).ToString();
                    ViewBag.RcCount = dt.Compute("Sum(cnt)", string.Empty).ToString();
                    ViewBag.UserComm = dt.Compute("Sum(UserComm)", string.Empty).ToString();
                    ViewBag.ApiComm = dt.Compute("Sum(ApiComm)", string.Empty).ToString();
                    ViewBag.CommDiff = dt.Compute("Sum(CommDiff)", string.Empty).ToString();
                    ViewBag.ROffer = dt.Compute("Sum(ROffer)", string.Empty).ToString();

                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return dailydtos;
        }

        public List<dailydto> GetCircleDayBook(string date, string Edate, int userid = 0, int apiid = 0, int opid = 0)
        {
            List<dailydto> dailydtos = new List<dailydto>();
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_GetCircleDayBook", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", date);
                    cmd.Parameters.AddWithValue("@edate", Edate);
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@apiid", apiid);
                    cmd.Parameters.AddWithValue("@opid", opid);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dailydto operatordayreport = new dailydto();
                        operatordayreport.Name = dt.Rows[i]["CircleName"].ToString();
                        operatordayreport.SuccessAmount = dt.Rows[i]["SuccessAmount"].ToString();
                        operatordayreport.SuccessCount = dt.Rows[i]["SuccessCount"].ToString();
                        operatordayreport.ProcessingAmount = dt.Rows[i]["ProcessingAmount"].ToString();
                        operatordayreport.ProcessingCount = dt.Rows[i]["ProcessingCount"].ToString();
                        operatordayreport.FailedAmount = dt.Rows[i]["FailedAmount"].ToString();
                        operatordayreport.FailedCount = dt.Rows[i]["FailedCount"].ToString();
                        operatordayreport.HoldAmount = dt.Rows[i]["HoldAmount"].ToString();
                        operatordayreport.HoldCount = dt.Rows[i]["HoldCount"].ToString();

                        dailydtos.Add(operatordayreport);
                    }
                    ViewBag.SuccessAmount = dt.Compute("Sum(SuccessAmount)", string.Empty).ToString();
                    ViewBag.SuccessCount = dt.Compute("Sum(SuccessCount)", string.Empty).ToString();
                    ViewBag.ProcessingAmount = dt.Compute("Sum(ProcessingAmount)", string.Empty).ToString();
                    ViewBag.ProcessingCount = dt.Compute("Sum(ProcessingCount)", string.Empty).ToString();
                    ViewBag.FailedAmount = dt.Compute("Sum(FailedAmount)", string.Empty).ToString();
                    ViewBag.FailedCount = dt.Compute("Sum(FailedCount)", string.Empty).ToString();
                    ViewBag.HoldAmount = dt.Compute("Sum(HoldAmount)", string.Empty).ToString();
                    ViewBag.HoldCount = dt.Compute("Sum(HoldCount)", string.Empty).ToString();

                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return dailydtos;
        }

        public List<dailydto> GetDayBookV2VendorWise(string date, string Edate, int userid = 0, int apiid = 0, int opid = 0, int circleid = 0)
        {
            List<dailydto> dailydtos = new List<dailydto>();
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_GetDayBookV2VendorWise", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", date);
                    cmd.Parameters.AddWithValue("@edate", Edate);
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@apiid", apiid);
                    cmd.Parameters.AddWithValue("@opid", opid);
                    cmd.Parameters.AddWithValue("@circleid", circleid);


                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dailydto operatordayreport = new dailydto();
                        operatordayreport.Name = dt.Rows[i]["ApiName"].ToString() + "- " + dt.Rows[i]["ApiId"].ToString();
                        operatordayreport.SuccessAmount = dt.Rows[i]["SuccessAmount"].ToString();
                        operatordayreport.SuccessCount = dt.Rows[i]["SuccessCount"].ToString();
                        operatordayreport.ProcessingAmount = dt.Rows[i]["ProcessingAmount"].ToString();
                        operatordayreport.ProcessingCount = dt.Rows[i]["ProcessingCount"].ToString();
                        operatordayreport.FailedAmount = dt.Rows[i]["FailedAmount"].ToString();
                        operatordayreport.FailedCount = dt.Rows[i]["FailedCount"].ToString();
                        operatordayreport.HoldAmount = dt.Rows[i]["HoldAmount"].ToString();
                        operatordayreport.HoldCount = dt.Rows[i]["HoldCount"].ToString();
                        //operatordayreport.RofferAmount = dt.Rows[i]["ROfferAmount"].ToString();
                        //operatordayreport.RofferCount = dt.Rows[i]["SuccessCount"].ToString();
                        //try
                        //{
                        //    var RofferAmount = Convert.ToDecimal(dt.Rows[i]["ROfferAmount"]);
                        //    var amount = Convert.ToDecimal(dt.Rows[i]["SuccessAmount"].ToString());

                        //    if (amount != 0)
                        //    {


                        //        decimal Rpercent = (RofferAmount / amount) * 100;
                        //        Rpercent = Math.Round(Rpercent, 2);
                        //        operatordayreport.Roffer = Rpercent.ToString();
                        //    }
                        //    else
                        //    {
                        //        operatordayreport.Roffer = "0";
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    operatordayreport.Roffer = "0";
                        //}

                        dailydtos.Add(operatordayreport);
                    }
                    ViewBag.SuccessAmount = dt.Compute("Sum(SuccessAmount)", string.Empty).ToString();
                    ViewBag.SuccessCount = dt.Compute("Sum(SuccessCount)", string.Empty).ToString();
                    ViewBag.ProcessingAmount = dt.Compute("Sum(ProcessingAmount)", string.Empty).ToString();
                    ViewBag.ProcessingCount = dt.Compute("Sum(ProcessingCount)", string.Empty).ToString();
                    ViewBag.FailedAmount = dt.Compute("Sum(FailedAmount)", string.Empty).ToString();
                    ViewBag.FailedCount = dt.Compute("Sum(FailedCount)", string.Empty).ToString();
                    ViewBag.HoldAmount = dt.Compute("Sum(HoldAmount)", string.Empty).ToString();
                    ViewBag.HoldCount = dt.Compute("Sum(HoldCount)", string.Empty).ToString();
                    //ViewBag.RofferAmount = dt.Compute("Sum(RofferAmount)", string.Empty).ToString();
                    //decimal ROpercent = (Convert.ToDecimal(ViewBag.RofferAmount) / Convert.ToDecimal(ViewBag.SuccessAmount)) * 100;
                    //ROpercent = Math.Round(ROpercent, 2);
                    //ViewBag.Roffer = ROpercent;
                    ViewBag.RofferCount = ViewBag.SuccessCount;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return dailydtos;
        }

        public List<dailydto> GetTrafficAmountWise(string date, string Edate, int userid = 0, int apiid = 0, int opid = 0, int cid = 0)
        {
            List<dailydto> dailydtos = new List<dailydto>();
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_GetTrafficAmountWise", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", date);
                    cmd.Parameters.AddWithValue("@edate", Edate);
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@apiid", apiid);
                    cmd.Parameters.AddWithValue("@opid", opid);
                    cmd.Parameters.AddWithValue("@circleid", cid);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();

                    var amountSum = Convert.ToDecimal(dt.Compute("Sum(TotalAmount)", string.Empty));

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        var amount = Convert.ToDecimal(dt.Rows[i]["TotalAmount"]);
                        decimal percent = (amount / amountSum) * 100;
                        percent = Math.Round(percent, 2);
                        dailydto operatordayreport = new dailydto();
                        try
                        {
                            var RofferAmount = Convert.ToDecimal(dt.Rows[i]["RofferAmount"]);
                            if (amount != 0)
                            {


                                decimal Rpercent = (RofferAmount / amount) * 100;
                                Rpercent = Math.Round(Rpercent, 2);
                                operatordayreport.FailedAmount = Rpercent.ToString();
                            }
                            else
                            {
                                operatordayreport.FailedAmount = "0";
                            }
                        }
                        catch (Exception ex)
                        {
                            operatordayreport.FailedAmount = "0";
                        }

                        operatordayreport.Name = dt.Rows[i]["Amount"].ToString();
                        operatordayreport.HoldAmount = dt.Rows[i]["TotalCount"].ToString();
                        operatordayreport.SuccessAmount = amount.ToString();
                        operatordayreport.ProcessingAmount = percent.ToString();
                        operatordayreport.SuccessCount = dt.Rows[i]["SuccessCount"].ToString();
                        operatordayreport.ProcessingCount = dt.Rows[i]["ProcessingCount"].ToString();
                        operatordayreport.FailedCount = dt.Rows[i]["FailedCount"].ToString();
                        operatordayreport.HoldCount = dt.Rows[i]["HoldCount"].ToString();
                        operatordayreport.RofferAmount = dt.Rows[i]["RofferAmount"].ToString();
                        dailydtos.Add(operatordayreport);
                    }
                    ViewBag.SuccessCount = dt.Compute("Sum(SuccessCount)", string.Empty).ToString();
                    ViewBag.ProcessingCount = dt.Compute("Sum(ProcessingCount)", string.Empty).ToString();
                    ViewBag.FailedCount = dt.Compute("Sum(FailedCount)", string.Empty).ToString();
                    ViewBag.HoldCount = dt.Compute("Sum(HoldCount)", string.Empty).ToString();
                    ViewBag.TotalCount = dt.Compute("Sum(TotalCount)", string.Empty).ToString();
                    ViewBag.Roffer = dt.Compute("Sum(RofferAmount)", string.Empty).ToString();
                    decimal ROpercent = (Convert.ToDecimal(ViewBag.Roffer) / amountSum) * 100;
                    ROpercent = Math.Round(ROpercent, 2);
                    ViewBag.RPercantage = ROpercent;
                    ViewBag.TotalAmount = amountSum.ToString();

                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return dailydtos;
        }

        public int OpertaorDeleteActive(int userid = 0, int apiid = 0, int opid = 0, int type = 0)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("Usp_OperatorGenrateService", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userid);
                    cmd.Parameters.AddWithValue("@VendorId", apiid);
                    cmd.Parameters.AddWithValue("@Opid", opid);
                    cmd.Parameters.AddWithValue("@TypeId", type);
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return 0;
        }

        public List<dailydto> GetTrafficCricleAmountWise(string date, string Edate, int userid = 0, int apiid = 0, int opid = 0, int cid = 0)
        {
            List<dailydto> dailydtos = new List<dailydto>();
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_GetTrafficCricleAmountWise", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", date);
                    cmd.Parameters.AddWithValue("@edate", Edate);
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@apiid", apiid);
                    cmd.Parameters.AddWithValue("@opid", opid);
                    cmd.Parameters.AddWithValue("@circleid", cid);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();

                    var amountSum = Convert.ToDecimal(dt.Compute("Sum(TotalAmount)", string.Empty));

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {

                        var amount = Convert.ToDecimal(dt.Rows[i]["TotalAmount"]);
                        decimal percent = (amount / amountSum) * 100;
                        percent = Math.Round(percent, 2);
                        dailydto operatordayreport = new dailydto();
                        try
                        {
                            var RofferAmount = Convert.ToDecimal(dt.Rows[i]["RofferAmount"]);
                            if (amount != 0)
                            {


                                decimal Rpercent = (RofferAmount / amount) * 100;
                                Rpercent = Math.Round(Rpercent, 2);
                                operatordayreport.FailedAmount = Rpercent.ToString();
                            }
                            else
                            {
                                operatordayreport.FailedAmount = "0";
                            }
                        }
                        catch (Exception ex)
                        {
                            operatordayreport.FailedAmount = "0";
                        }
                        operatordayreport.CircleName = dt.Rows[i]["CircleName"].ToString();
                        operatordayreport.Name = dt.Rows[i]["Amount"].ToString();
                        operatordayreport.HoldAmount = dt.Rows[i]["TotalCount"].ToString();
                        operatordayreport.SuccessAmount = amount.ToString();
                        operatordayreport.ProcessingAmount = percent.ToString();
                        operatordayreport.SuccessCount = dt.Rows[i]["SuccessCount"].ToString();
                        operatordayreport.ProcessingCount = dt.Rows[i]["ProcessingCount"].ToString();
                        operatordayreport.FailedCount = dt.Rows[i]["FailedCount"].ToString();
                        operatordayreport.HoldCount = dt.Rows[i]["HoldCount"].ToString();
                        operatordayreport.RofferAmount = dt.Rows[i]["RofferAmount"].ToString();
                        dailydtos.Add(operatordayreport);
                    }
                    ViewBag.SuccessCount = dt.Compute("Sum(SuccessCount)", string.Empty).ToString();
                    ViewBag.ProcessingCount = dt.Compute("Sum(ProcessingCount)", string.Empty).ToString();
                    ViewBag.FailedCount = dt.Compute("Sum(FailedCount)", string.Empty).ToString();
                    ViewBag.HoldCount = dt.Compute("Sum(HoldCount)", string.Empty).ToString();
                    ViewBag.TotalCount = dt.Compute("Sum(TotalCount)", string.Empty).ToString();
                    ViewBag.Roffer = dt.Compute("Sum(RofferAmount)", string.Empty).ToString();
                    decimal ROpercent = (Convert.ToDecimal(ViewBag.Roffer) / amountSum) * 100;
                    ROpercent = Math.Round(ROpercent, 2);
                    ViewBag.RPercantage = ROpercent;
                    ViewBag.TotalAmount = amountSum.ToString();

                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return dailydtos;
        }

        public List<dailydto> GetDayBookV2UserWise(string date, string Edate, int userid = 0, int apiid = 0, int opid = 0, int circleid = 0)
        {
            List<dailydto> dailydtos = new List<dailydto>();
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_GetUserDayBookV2", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", date);
                    cmd.Parameters.AddWithValue("@edate", Edate);
                    cmd.Parameters.AddWithValue("@userid", userid);
                    cmd.Parameters.AddWithValue("@apiid", apiid);
                    cmd.Parameters.AddWithValue("@opid", opid);
                    cmd.Parameters.AddWithValue("@circleid", circleid);


                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dailydto operatordayreport = new dailydto();
                        operatordayreport.Name = dt.Rows[i]["FullName"].ToString() + "- " + dt.Rows[i]["UserId"].ToString();
                        operatordayreport.SuccessAmount = dt.Rows[i]["SuccessAmount"].ToString();
                        operatordayreport.SuccessCount = dt.Rows[i]["SuccessCount"].ToString();
                        operatordayreport.ProcessingAmount = dt.Rows[i]["ProcessingAmount"].ToString();
                        operatordayreport.ProcessingCount = dt.Rows[i]["ProcessingCount"].ToString();
                        operatordayreport.FailedAmount = dt.Rows[i]["FailedAmount"].ToString();
                        operatordayreport.FailedCount = dt.Rows[i]["FailedCount"].ToString();
                        operatordayreport.HoldAmount = dt.Rows[i]["HoldAmount"].ToString();
                        operatordayreport.HoldCount = dt.Rows[i]["HoldCount"].ToString();
                        //operatordayreport.RofferAmount = dt.Rows[i]["ROfferAmount"].ToString();
                        //operatordayreport.RofferCount = dt.Rows[i]["SuccessCount"].ToString();
                        //try
                        //{
                        //    var RofferAmount = Convert.ToDecimal(dt.Rows[i]["ROfferAmount"]);
                        //    var amount = Convert.ToDecimal(dt.Rows[i]["SuccessAmount"].ToString());

                        //    if (amount != 0)
                        //    {


                        //        decimal Rpercent = (RofferAmount / amount) * 100;
                        //        Rpercent = Math.Round(Rpercent, 2);
                        //        operatordayreport.Roffer = Rpercent.ToString();
                        //    }
                        //    else
                        //    {
                        //        operatordayreport.Roffer = "0";
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    operatordayreport.Roffer = "0";
                        //}
                        dailydtos.Add(operatordayreport);
                    }
                    ViewBag.SuccessAmount = dt.Compute("Sum(SuccessAmount)", string.Empty).ToString();
                    ViewBag.SuccessCount = dt.Compute("Sum(SuccessCount)", string.Empty).ToString();
                    ViewBag.ProcessingAmount = dt.Compute("Sum(ProcessingAmount)", string.Empty).ToString();
                    ViewBag.ProcessingCount = dt.Compute("Sum(ProcessingCount)", string.Empty).ToString();
                    ViewBag.FailedAmount = dt.Compute("Sum(FailedAmount)", string.Empty).ToString();
                    ViewBag.FailedCount = dt.Compute("Sum(FailedCount)", string.Empty).ToString();
                    ViewBag.HoldAmount = dt.Compute("Sum(HoldAmount)", string.Empty).ToString();
                    ViewBag.HoldCount = dt.Compute("Sum(HoldCount)", string.Empty).ToString();
                    //ViewBag.RofferAmount = dt.Compute("Sum(RofferAmount)", string.Empty).ToString();
                    //decimal ROpercent = (Convert.ToDecimal(ViewBag.RofferAmount) / Convert.ToDecimal(ViewBag.SuccessAmount)) * 100;
                    //ROpercent = Math.Round(ROpercent, 2);
                    //ViewBag.Roffer = ROpercent;
                    //ViewBag.RofferCount = ViewBag.SuccessCount;

                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return dailydtos;
        }

        public List<LapuPurchageDto> GetLapuPurchase(string date, string Edate, long lapuid = 0, int dealerid = 0, int opid = 0, int cid = 0)
        {
            List<LapuPurchageDto> dailydtos = new List<LapuPurchageDto>();
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_GetLapuPurchage", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromeDate", date);
                    cmd.Parameters.AddWithValue("@ToDate", Edate);
                    cmd.Parameters.AddWithValue("@LapuId", lapuid);
                    cmd.Parameters.AddWithValue("@DealerId", dealerid);
                    cmd.Parameters.AddWithValue("@OpId", opid);
                    cmd.Parameters.AddWithValue("@CircleId", cid);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        LapuPurchageDto operatordayreport = new LapuPurchageDto();
                        operatordayreport.TxnDate = dt.Rows[i]["TxnDate"].ToString();
                        operatordayreport.LapuNo = dt.Rows[i]["LapuNo"].ToString();
                        operatordayreport.OpName = dt.Rows[i]["OpName"].ToString();
                        operatordayreport.CircleName = dt.Rows[i]["CircleName"].ToString();
                        operatordayreport.CR_Amt = dt.Rows[i]["CR_Amt"].ToString();
                        operatordayreport.CM_Amt = dt.Rows[i]["CM_Amt"].ToString();
                        operatordayreport.Total = dt.Rows[i]["Total"].ToString();
                        operatordayreport.Margin = dt.Rows[i]["Margin"].ToString();
                        operatordayreport.DB_Amt = dt.Rows[i]["DB_Amt"].ToString();
                        operatordayreport.DealerName = dt.Rows[i]["DealerName"].ToString();
                        operatordayreport.PurDate = dt.Rows[i]["PurDate"].ToString();

                        dailydtos.Add(operatordayreport);
                    }
                    ViewBag.CR_Amt = dt.Compute("Sum(CR_Amt)", string.Empty).ToString();
                    ViewBag.CM_Amt = dt.Compute("Sum(CM_Amt)", string.Empty).ToString();
                    ViewBag.DB_Amt = dt.Compute("Sum(DB_Amt)", string.Empty).ToString();
                    ViewBag.Total = dt.Compute("Sum(Total)", string.Empty).ToString();

                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return dailydtos;
        }

        public List<UserDayBookDto> GetUserDayBookDateWise(int Userid, string Sdate, string Edate)
        {
            List<UserDayBookDto> mList = new List<UserDayBookDto>();

            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_GetUserDayBookDateWise", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", Userid);
                    cmd.Parameters.AddWithValue("@sDate", Sdate);
                    cmd.Parameters.AddWithValue("@eDate", Edate);

                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        UserDayBookDto model = new UserDayBookDto();

                        model.TxnDate = String.Format("{0:yyyy-MM-dd}", dt.Rows[i]["TxnDate"]);

                        // model.UserId =Userid;
                        // model.UserName = model.UserId + "- " + dt.Rows[i]["UserName"]?.ToString() ?? string.Empty;
                        model.RC_Amt = dt.Rows[i]["RC_Amt"]?.ToString() ?? "0.00";
                        model.OP_Bal = dt.Rows[i]["OP_Bal"]?.ToString() ?? "0.00";
                        model.WR_Amt = dt.Rows[i]["WR_Amt"]?.ToString() ?? "0.00";
                        model.DB_Amt = dt.Rows[i]["DB_Amt"]?.ToString() ?? "0.00";
                        model.CM_Amt = dt.Rows[i]["CM_Amt"]?.ToString() ?? "0.00";
                        model.CL_Bal = dt.Rows[i]["CL_Bal"]?.ToString() ?? "0.00";
                        model.Calc_Bal = dt.Rows[i]["Calc_Bal"]?.ToString() ?? "0.00";
                        model.CL_Diff = dt.Rows[i]["CL_Diff"]?.ToString() ?? "0.00";

                        model.Old_fRc_Amt = dt.Rows[i]["fRcAmount"]?.ToString() ?? "0.00";
                        model.Old_sRc_Amt = dt.Rows[i]["sRcAmount"]?.ToString() ?? "0.00";
                        model.Old_CR_Amt = dt.Rows[i]["fCR_Amt"]?.ToString() ?? "0.00";
                        model.Old_DB_Amt = dt.Rows[i]["sDB_Amt"]?.ToString() ?? "0.00";

                        mList.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return mList;
        }

        public List<DealerDayBookDto> GetDealerDayBook(int DealerId, string Sdate, string Edate, string remark = "")
        {
            List<DealerDayBookDto> mList = new List<DealerDayBookDto>();

            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_GetDealerDayBook", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DealerId", DealerId);
                    cmd.Parameters.AddWithValue("@sDate", Sdate);
                    cmd.Parameters.AddWithValue("@eDate", Edate);
                    cmd.Parameters.AddWithValue("@FilterType", remark);
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DealerDayBookDto model = new DealerDayBookDto();
                        if (remark == "datewise")
                        {
                            model.DealerName = String.Format("{0:yyyy-MM-dd}", dt.Rows[i]["TxnDate"]);
                        }
                        else
                        {
                            model.DealerName = dt.Rows[i]["Name"]?.ToString() ?? string.Empty;
                        }

                        model.OP_Bal = dt.Rows[i]["OP_Bal"]?.ToString() ?? "0.00";
                        model.CR_Amt = dt.Rows[i]["CR_Amt"]?.ToString() ?? "0.00";
                        model.CM_Amt = dt.Rows[i]["CM_Amt"]?.ToString() ?? "0.00";
                        model.DB_Amt = dt.Rows[i]["DB_Amt"]?.ToString() ?? "0.00";
                        model.CM_DB_Amt = dt.Rows[i]["CM_DB_Amt"]?.ToString() ?? "0.00";
                        model.CL_Bal = dt.Rows[i]["CL_Bal"]?.ToString() ?? "0.00";
                        model.Amt_Diff = dt.Rows[i]["Amt_Diff"]?.ToString() ?? "0.00";

                        mList.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return mList;
        }

        public List<DealerDayBookDto> GetDealerLapuDayBook(int DealerId, string Sdate, string Edate)
        {
            List<DealerDayBookDto> mList = new List<DealerDayBookDto>();

            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_GetDealerLapuDayBook", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DealerId", DealerId);
                    cmd.Parameters.AddWithValue("@sDate", Sdate);
                    cmd.Parameters.AddWithValue("@eDate", Edate);
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DealerDayBookDto model = new DealerDayBookDto();

                        model.DealerName = dt.Rows[i]["Name"]?.ToString() ?? string.Empty;
                        model.OP_Bal = dt.Rows[i]["OP_Bal"]?.ToString() ?? "0.00";
                        model.CR_Amt = dt.Rows[i]["CR_Amt"]?.ToString() ?? "0.00";
                        model.CM_Amt = dt.Rows[i]["CM_Amt"]?.ToString() ?? "0.00";
                        model.DB_Amt = dt.Rows[i]["DB_Amt"]?.ToString() ?? "0.00";
                        // model.CM_DB_Amt = dt.Rows[i]["CM_DB_Amt"]?.ToString() ?? "0.00";
                        model.CL_Bal = dt.Rows[i]["CL_Bal"]?.ToString() ?? "0.00";
                        model.RO_Amt = dt.Rows[i]["RO_Amt"]?.ToString() ?? "0.00";
                        model.Amt_Diff = dt.Rows[i]["Amt_Diff"]?.ToString() ?? "0.00";

                        mList.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return mList;
        }

        public List<DealerDayBookDto> GetDealerLapuDayBookDateWise(int DealerId, string Sdate, string Edate)
        {
            List<DealerDayBookDto> mList = new List<DealerDayBookDto>();

            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("usp_GetDealerLapuDayBookDateWise", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DealerId", DealerId);
                    cmd.Parameters.AddWithValue("@sDate", Sdate);
                    cmd.Parameters.AddWithValue("@eDate", Edate);
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    sqlDataAdapter.Fill(dt);
                    con.Close();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DealerDayBookDto model = new DealerDayBookDto();

                        model.DealerName = String.Format("{0:yyyy-MM-dd}", dt.Rows[i]["TxnDate"]);
                        model.OP_Bal = dt.Rows[i]["OP_Bal"]?.ToString() ?? "0.00";
                        model.CR_Amt = dt.Rows[i]["CR_Amt"]?.ToString() ?? "0.00";
                        model.CM_Amt = dt.Rows[i]["CM_Amt"]?.ToString() ?? "0.00";
                        model.DB_Amt = dt.Rows[i]["DB_Amt"]?.ToString() ?? "0.00";
                        // model.CM_DB_Amt = dt.Rows[i]["CM_DB_Amt"]?.ToString() ?? "0.00";
                        model.CL_Bal = dt.Rows[i]["CL_Bal"]?.ToString() ?? "0.00";
                        model.RO_Amt = dt.Rows[i]["RO_Amt"]?.ToString() ?? "0.00";
                        model.Amt_Diff = dt.Rows[i]["Amt_Diff"]?.ToString() ?? "0.00";

                        mList.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
            return mList;
        }

        #endregion


        private void UpdateActivity(string title, string action, string remark = "")
        {
            try
            {
                activityLogModel.ActivityName = title;
                activityLogModel.ActivityPage = action;
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