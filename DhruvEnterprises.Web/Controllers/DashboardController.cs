using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.Code.Attributes;
using DhruvEnterprises.Web.LIBS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Controllers
{
   
    public class DashboardController : BaseController
    {
        public ActionAllowedDto actionAllowedDto;

        public DashboardController(IActivityLogService _activityLogService, IRoleService _roleService) : base(_activityLogService, _roleService)
        {
            this.actionAllowedDto = new ActionAllowedDto();
        }
        // GET: Dashboard
        public ActionResult Index()
        {
            HomeDto homedto = new HomeDto();
            string PC = "0";
            var datas = Homes(ref PC);
            homedto.SUCCESS = datas.Item1.Replace(".0000", ".00");
            homedto.Proceess = datas.Item2.Replace(".0000", ".00");
            homedto.Failed = datas.Item3.Replace(".0000", ".00");
            homedto.Eran = datas.Item4;
            homedto.News = datas.Item5;
            homedto.SCount = datas.Item6;
            homedto.PCount = PC;
            homedto.FCount = datas.Item7;
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("HomePage3", con);


                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Mrbalance", SqlDbType.Int);
                    cmd.Parameters.Add("@Mmsbalance", SqlDbType.Int);
                    cmd.Parameters.Add("@Mdbalance", SqlDbType.Int);


                    cmd.Parameters["@Mrbalance"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@Mmsbalance"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@Mdbalance"].Direction = ParameterDirection.Output;


                    cmd.Parameters.Add("@Drbalance", SqlDbType.Int);
                    cmd.Parameters.Add("@Dmsbalance", SqlDbType.Int);
                    cmd.Parameters.Add("@Ddbalance", SqlDbType.Int);


                    cmd.Parameters["@Drbalance"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@Dmsbalance"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@Ddbalance"].Direction = ParameterDirection.Output;


                    con.Open();
                    cmd.ExecuteNonQuery();
                    ViewBag.Mrb = Convert.ToString(cmd.Parameters["@Mrbalance"].Value);
                    ViewBag.Mmsb = Convert.ToString(cmd.Parameters["@Mmsbalance"].Value);
                    ViewBag.Mdb = Convert.ToString(cmd.Parameters["@Mdbalance"].Value);


                    ViewBag.Drb = Convert.ToString(cmd.Parameters["@Drbalance"].Value);
                    ViewBag.Dmsb = Convert.ToString(cmd.Parameters["@Dmsbalance"].Value);
                    ViewBag.Ddb = Convert.ToString(cmd.Parameters["@Ddbalance"].Value);


                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                DataTable dt = new DataTable();
                LIBS.Common.LogException(ex);
            }
            return View(homedto);
        }

        public Tuple<string, string, string, string, string, string, string> Homes(ref string Processingc)
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
                DataTable dt1 = OperatorActive(ref News);
                return Tuple.Create(Success, Processing, Failed, Eran, News, Successc, Failedc);
            }
            catch (Exception)
            {
                return Tuple.Create("0", "0", "0", "0", "0", "0", "0");
            }

        }

        public DataTable OperatorActive(ref string News)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("MessageWithOpDown", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Message", SqlDbType.NVarChar, 5000);
                    cmd.Parameters["@Message"].Direction = ParameterDirection.Output;
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                    sqlDataAdapter.Fill(dt);
                    News = Convert.ToString(cmd.Parameters["@Message"].Value);
                    con.Close();
                    return dt;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }






        [CustomAuthorization()]
        [HttpGet]
        public ActionResult Signout()
        {
            RemoveAuthentication();
            SiteSession.SessionUser = null; // for webforms
            Response.Cookies["ShaktiUserSessionCookies"].Expires = System.DateTime.Now.AddSeconds(1); // Clear cookies of SiteSession.SessionUser

            return RedirectToAction("Index", "Login");
        }


    }
}