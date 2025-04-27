using DhruvEnterprises.Service;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Web.LIBS;
using DhruvEnterprises.Web.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Collections;
using DhruvEnterprises.Data;

namespace DhruvEnterprises.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IPackageService packageService;
        private readonly IUserService userService;

        public HomeController(IUserService _userService,IActivityLogService _activityLogService, IRoleService roleService,IPackageService _packageService) : base(_activityLogService, roleService)
        {
            this.packageService = _packageService;
            this.userService = _userService;

        }

        public ActionResult Index()
        {
            string PC = "0";
            HomeDto homedto = new HomeDto();
            if (CurrentUser.RoleId == 1)
            {
                 homedto = Homes(ref PC);
            }
            User user = userService.GetUser(CurrentUser?.UserID ?? 0);
            if (double.TryParse(homedto.Eran, out double numericValue))
            {
                homedto.Eran = (numericValue / 100).ToString();
            }
            homedto.AccountNo = "ZEZYTM" + user.UserProfile?.MobileNumber + "IN";
            return View(homedto);
        }

        public HomeDto Homes(ref string Processingc)
        {
            DataTable dt = new DataTable();
            try
            {
                string sdate = "", edate = "";
                int roleid = CurrentUser.Roles.FirstOrDefault();
                sdate = DateTime.Now.ToString("MM/dd/yyyy");
                edate = DateTime.Now.ToString("MM/dd/yyyy");
                bool isAdmin = roleid == 1 || roleid == 2 ? true : false;
                int userid = isAdmin ? 0 : CurrentUser.UserID;
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("HomePage1", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", sdate);
                    cmd.Parameters.AddWithValue("@edate", edate);
                    cmd.Parameters.AddWithValue("@UserId", userid);
                    cmd.Parameters.AddWithValue("@roleid", roleid);
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    sqlDataAdapter.Fill(dt);
                    con.Close();
                }
                string Success = "0";
                string Processing = "0";
                string Failed = "0";
                string Successc = "0";
                string Failedc = "0";
                string Eran = "0";
                string Complaints = "0";
                string TFComplaints = "0";
                string TProfit = "0";
                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    Success = Convert.ToString(dt.Rows[i]["SuccessAmount"]);
                    Processing = Convert.ToString(dt.Rows[i]["ProcessingAmount"]);
                    Failed = Convert.ToString(dt.Rows[i]["FailedAmount"]);
                    Successc = Convert.ToString(dt.Rows[i]["Successcount"]);
                    Processingc = Convert.ToString(dt.Rows[i]["Pcount"]);
                    Failedc = Convert.ToString(dt.Rows[i]["Fcount"]);
                    Eran = Convert.ToString(dt.Rows[i]["CM_Amt"]);
                }
                string News = "";
                //DataTable dt1 = OperatorActive(ref News);

                return new HomeDto
                {
                    SUCCESS = Success.Replace(".0000", ".00"),
                    Proceess = Processing.Replace(".0000", ".00"),
                    Failed = Failed.Replace(".0000", ".00"),
                    Eran = Eran.Replace(".0000", ".00"),
                    News = News,
                    SCount = Successc,
                    PCount = Processingc,
                    FCount = Failedc,
                    TFComplaints = TFComplaints,
                    TProfit = TProfit.Replace(".0000", ".00"),
                    Complaints = Complaints,

                }; /*Tuple.Create(Success, Processing, Failed, Eran, News, Successc, Failedc, Complaints, TFComplaints, TProfit);*/
            }
            catch (Exception ex)
            {
                return new HomeDto
                {
                    SUCCESS = "00",
                    Proceess = ("00"),
                    Failed = ("00"),
                    Eran = ("00"),
                    News = "",
                    SCount = "0",
                    PCount = "0",
                    FCount = "0",
                    TFComplaints = "0",
                    TProfit = ("00"),
                    Complaints = "0"

                };
            }

        }

        public JsonResult ChartData()
        {
            JObject response = new JObject();
            try
            {
                string sdate = "", edate = "";
                int roleid = CurrentUser.Roles.FirstOrDefault();
                sdate = DateTime.Now.ToString("MM/dd/yyyy");
                edate = DateTime.Now.ToString("MM/dd/yyyy");
                bool isAdmin = roleid == 1 || roleid == 2 ? true : false;
                int userid = isAdmin ? 0 : CurrentUser.UserID;
                DataSet ds = new DataSet();
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("[DashboardChartData]", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", sdate);
                    cmd.Parameters.AddWithValue("@edate", edate);
                    cmd.Parameters.AddWithValue("@UserId", userid);
                    cmd.Parameters.AddWithValue("@roleid", roleid);
                    SqlDataAdapter ad1 = new SqlDataAdapter();
                    ad1.SelectCommand = cmd;
                    ad1.Fill(ds);
                    // int count = ds.Tables.Count;
                    var json1 = DataTableToJSONWithJavaScriptSerializer(ds.Tables[0]);//Todays Sale Data
                    var json2 = DataTableToJSONWithJavaScriptSerializer(ds.Tables[1]);// Last Seven days sale Data
                    var json3 = DataTableToJSONWithJavaScriptSerializer(ds.Tables[2]);//User's Balance
                    var json4 = DataTableToJSONWithJavaScriptSerializer(ds.Tables[3]);//Top 5 Transactions
                                                                                      // var json5 = DataTableToJSONWithJavaScriptSerializer(ds.Tables[4]);//Top 5 Earnings
                    return Json(new
                    {
                        tdaySale = json1,
                        sevendaysSale = json2,
                        userbal = json3,
                        TfiveTranbyUser = json4

                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
                return null;
            }


            //return response;
        }

        public string DataTableToJSONWithJavaScriptSerializer(DataTable table)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    childRow.Add(col.ColumnName, row[col]);
                }
                parentRow.Add(childRow);
            }
            return jsSerializer.Serialize(parentRow);
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Chart()
        {
            TempData["RechargeFilterDto"] = null;
            RechargeFilterDto filter = new RechargeFilterDto();
            filter.CustomerNo = "30";
            filter.Opid = filter.Opid == 0 ? 1 : filter.Opid;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.ToString("yyy-MM-dd 00:mm");
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("yyy-MM-dd HH:mm");
            ViewBag.OperatorList = packageService.GetOperatorList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name, Selected = (x.Id == filter.Opid) }).ToList();
            ViewBag.FilterData = TempData["RechargeFilterDto"] = filter;
            return View();
        }
        public JsonResult GetChartOperatorWise(int? n, int? o, string f = "", string e = "")
        {
            string sdate = ""; string edate = ""; int opid = 0;
            RechargeFilterDto filter = new RechargeFilterDto();
            filter.Sdate = f;
            filter.Edate = e;
            opid = o??0;
            int interval = n ?? 30;
            filter.SdateNow = !string.IsNullOrEmpty(filter.Sdate) ? filter.Sdate : DateTime.Now.ToString("yyy-MM-dd 00:mm");
            filter.EdateNow = !string.IsNullOrEmpty(filter.Edate) ? filter.Edate : DateTime.Now.ToString("yyy-MM-dd HH:mm");

            dynamic dynamic = MultiChartData(filter.SdateNow, filter.EdateNow, opid, 1, interval); //SUCCESS
            dynamic dynamic2 = MultiChartData(filter.SdateNow, filter.EdateNow, opid, 3, interval); //FAILED
            dynamic data2 = dynamic2.Data.json1;//FAILED

            dynamic data = dynamic.Data.json1;//SUCCESS
            return Json(new { data, data2 }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Chart2()
        {
            string sdate = ""; string edate = "";int opid = 0;
            ViewBag.HighesthourGraphData = JsonConvert.SerializeObject(MultiChartData(sdate, edate, opid));

            return View();
        }

        public JsonResult MultiChartData(string sdate = "", string edate = "",int opid=0,int status=1,int interval=30)
        {
            JObject response = new JObject();
            try
            {
                int roleid = CurrentUser.Roles.FirstOrDefault();
                sdate = sdate?? DateTime.Now.ToString("yyy-MM-dd 00:mm");
                edate = edate??DateTime.Now.ToString("yyy-MM-dd HH:mm");
                bool isAdmin = roleid == 1 || roleid == 2 ? true : false;
                int userid = isAdmin ? 0 : CurrentUser.UserID;
                DataSet ds = new DataSet();
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("[Usp_OperatorGraph]", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@start", sdate);
                    cmd.Parameters.AddWithValue("@finish", edate);
                    cmd.Parameters.AddWithValue("@interval", interval);
                    cmd.Parameters.AddWithValue("@OpId", opid);
                    cmd.Parameters.AddWithValue("@status", status);

                    SqlDataAdapter ad1 = new SqlDataAdapter();
                    ad1.SelectCommand = cmd;
                    ad1.Fill(ds);
                    // int count = ds.Tables.Count;
                    var json1 = DataTableToJSONWithJavaScriptSerializer(ds.Tables[0]);//Todays Sale Data
                    return Json(new
                    {
                         json1
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Common.LogException(ex);
                return null;
            }


            //return response;
        }
    }
}