using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.Code.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using DhruvEnterprises.Web.Code.LIBS;
using DhruvEnterprises.Web.SmsApi;
using RestSharp;
using Newtonsoft.Json;

namespace DhruvEnterprises.Web.Controllers
{
    [CustomAuthorization()]
    public class SMSAPIController : BaseController
    {
        public ActionAllowedDto actionAllowedDto;
        private IUserService adminUserService;
        private ISmsApiService smsApiService;
        private IRoleService roleService;
        ActivityLogDto activityLogModel;
        SMSapiCall sMSapiCall;

        public SMSAPIController(IUserService _adminUserService, IActivityLogService _activityLogService, IRoleService _roleService, ISmsApiService _smsApiService) : base(_activityLogService, _roleService)
        {
            this.roleService = _roleService;
            this.adminUserService = _adminUserService;
            this.smsApiService = _smsApiService;
            this.actionAllowedDto = new ActionAllowedDto();
            this.activityLogModel = new ActivityLogDto();
            this.sMSapiCall = new SMSapiCall();
        }

        // GET: SMSAPI
        public ActionResult Index()
        {
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("SMSAPI", CurrentUser.Roles.FirstOrDefault());
            try
            {
                activityLogModel.ActivityName = "SMSAPI Index REQUEST";
                activityLogModel.ActivityPage = "GET:SMSAPI/Index/";
                activityLogModel.Remark = "";
                activityLogModel.UserId = CurrentUser?.UserID ?? 0;
                LogActivity(activityLogModel);
            }
            catch (Exception e)
            {
                LogException(e);
            }
            return View();
        }

        [HttpPost]
        public ActionResult GetAdminSMSAPIs(DataTableServerSide model)
        {
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("SMSAPI", CurrentUser.Roles.FirstOrDefault());

            int roleid = CurrentUser.Roles.FirstOrDefault();
            int userid = CurrentUser?.UserID ?? 0;
            KeyValuePair<int, List<SmsAPI>> Banks = smsApiService.GetSmsApi(model);
            var userbals = adminUserService.GetUserListWithBalace();
            return Json(new
            {
                model.draw,
                recordsTotal = Banks.Key,
                recordsFiltered = Banks.Key,
                data = Banks.Value.Select(c => new List<object> {
                    c.Id,
                    c.ApiName,
                    c.Url,
                    c.Status,
                   (actionAllowedDto.AllowCreate?  DataTableButton.EditButton(Url.Action( "createedit", "SMSAPI",new { id = c.Id })):string.Empty )
                    +"&nbsp;"+
                   (actionAllowedDto.AllowDelete?  DataTableButton.DeleteButton(Url.Action( "delete","SMSAPI", new { id = c.Id }),"modal-delete-SMSAPI"):string.Empty)
                   , actionAllowedDto.AllowEdit?true:false
                })
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CreateEdit(int? id)
        {
            UpdateActivity("SMSAPI CreateEdit REQUEST", "GET:Bank/CreateEdit/", "ID=" + id);
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("SMSAPI", CurrentUser.RoleId, id.HasValue ? 3 : 2);
            SMSAPIDto sMSAPIdto = new SMSAPIDto();
            ViewBag.Range10 = Enumerable.Range(1, 10).Select(x => new { Id = x, Name = x }).ToList();
            if (id.HasValue && id.Value > 0)
            {
                SmsAPI smsAPI = smsApiService.GetSMSById(id.Value);
                sMSAPIdto.SMSID = smsAPI.Id;
                sMSAPIdto.Method = smsAPI.Method;
                sMSAPIdto.SmsURL = smsAPI.Url;
                //sMSAPIdto.Parameter = smsAPI.Parameter;
                sMSAPIdto.ApiName = smsAPI.ApiName;
                //sMSAPIdto.SenderID = smsAPI.SENDER_ID;
                sMSAPIdto.status = (bool)smsAPI.Status;
                //sMSAPIdto.userid = smsAPI.UserId;
                // sMSAPIdto.password = smsAPI.Parameter;
            }
            return View("createedit", sMSAPIdto);
        }

        [HttpPost]
        public ActionResult CreateEdit(SMSAPIDto model, FormCollection FC)
        {
            UpdateActivity("SMSAPI CreateEdit REQUEST", "POST:SMSAPI/CreateEdit/", "ID=" + model.SMSID);
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("SMSAPI", CurrentUser.RoleId, model.SMSID > 0 ? 3 : 2);
            string message = string.Empty;
            try
            {
                if (ModelState.IsValid)
                {
                    int roleid = CurrentUser.Roles.FirstOrDefault();
                    SmsAPI smsAPI = smsApiService.GetSMSById(model.SMSID) ?? new SmsAPI();
                    smsAPI.Id = model.SMSID;
                    smsAPI.ApiName = model.ApiName;
                    //smsAPI.SENDER_ID = model.SenderID;
                    smsAPI.Url = model.SmsURL;
                    //smsAPI.UserId = model.userid;
                    //smsAPI.Password = model.password;
                    //smsAPI.Method = model.Method;
                    smsAPI.Status = model.status;
                    smsApiService.Save(smsAPI);
                    ShowSuccessMessage("Success!", "SMS Api has been saved", false);
                    return RedirectToAction("Index");
                    //return NewtonSoftJsonResult(new RequestOutcome<string> { IsSuccess = true, RedirectUrl = Url.Action("Index") });
                }
            }
            catch (Exception Ex)
            {
                var msg = Ex.GetBaseException().ToString();
                if (msg.Contains("UNIQUE KEY"))
                {
                    message = "AccountNo already exist.";
                    ModelState.AddModelError("error", message);
                }
                else
                {
                    message = "An internal error found during to process your requested data!";
                    ModelState.AddModelError("error", message);
                }
            }
            // return CreateModelStateErrors();
            return View();
        }

        //public bool Active(int id)
        //{
        //    UpdateActivity("Active/Inactive SMSAPI", "GET:SMSAPI/Active/", "SMSID=" + id);
        //    actionAllowedDto = ActionAllowed("SMSAPI", CurrentUser.Roles.FirstOrDefault(), 3);
        //    if (!actionAllowedDto.AllowEdit)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        string message = string.Empty;
        //        try
        //        {
        //            var adminUser = smsApiService.GetSMSById(id);
        //            adminUser.Status = !adminUser.Status;                   
        //            return smsApiService.Save(adminUser).Status == true ? true : false;

        //        }
        //        catch (Exception)
        //        {
        //            return false;
        //        }
        //    }

        //}
        public bool Active(int id)
        {
            UpdateActivity("Active/Inactive SMSAPI", "GET:SMSAPI/Active/", "SMSID=" + id);
            actionAllowedDto = ActionAllowed("SMSAPI", CurrentUser.Roles.FirstOrDefault(), 3);
            if (!actionAllowedDto.AllowEdit)
            {
                return false;
            }
            else
            {

                try
                {
                    using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                    {
                        SqlCommand cmd = new SqlCommand("sp_ActivateSmsApi", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id", id);
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        return true;
                    }

                }
                catch (Exception)
                {
                    return false;
                }
            }

        }
        private void GetCounts(ref int cmpCount, ref int prcCount, ref int wrCount, ref string noteMsg)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    SqlCommand cmd = new SqlCommand("sp_GetCounts", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", CurrentUser.RoleId == 3 ? CurrentUser.UserID : 0);
                    cmd.Parameters.Add("@ComplaintCount", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ProcessingCount", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@WRCount", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@FailedMsg", SqlDbType.VarChar, 4000).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@ProcMsg", SqlDbType.VarChar, 4000).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@HoldMsg", SqlDbType.VarChar, 4000).Direction = ParameterDirection.Output;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                    string cCount = Convert.ToString(cmd.Parameters["@ComplaintCount"].Value);
                    string pCount = Convert.ToString(cmd.Parameters["@ProcessingCount"].Value);
                    string wCount = Convert.ToString(cmd.Parameters["@WRCount"].Value);


                    string failMSG = Convert.ToString(cmd.Parameters["@FailedMsg"].Value);
                    string procMSG = Convert.ToString(cmd.Parameters["@ProcMsg"].Value);
                    string holdMSG = Convert.ToString(cmd.Parameters["@HoldMsg"].Value);

                    if (!string.IsNullOrEmpty(failMSG))
                        noteMsg = " FAILED VENDORS: " + failMSG;

                    if (!string.IsNullOrEmpty(procMSG))
                        noteMsg += " PROCESSING VENDORS:  " + procMSG;

                    if (!string.IsNullOrEmpty(holdMSG))
                        noteMsg += " HOLD VENDORS: " + holdMSG;

                    cmpCount = !string.IsNullOrEmpty(cCount) ? Convert.ToInt32(cCount) : 0;
                    prcCount = !string.IsNullOrEmpty(pCount) ? Convert.ToInt32(pCount) : 0;
                    wrCount = !string.IsNullOrEmpty(wCount) ? Convert.ToInt32(wCount) : 0;

                }
            }
            catch (Exception ex)
            {
                LogException(ex, "sp_GetCounts");
            }


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

        public ActionResult SendSMS()
        {
            sendsmsdto model = new sendsmsdto();
            ViewBag.UserList = adminUserService.GetUserList(3).Take(3).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Username }).ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult SendSMS(sendsmsdto sendsmsdto)
        {
            if (sendsmsdto.IsPush == true)
            {
                var datalist = adminUserService.GetFireBaseToken().OrderByDescending(x=>x.Id);
                string d = string.Join(",", datalist.Select(x => x.TokenId));
                string[] tokens = d.Split(',').ToArray();
                        var data = new
                        {
                            title = "News Notification",
                            body = sendsmsdto.Message,
                            icon = SiteKey.DomainName + SiteKey.CompanyLogo,
                            image = SiteKey.DomainName+ SiteKey.CompanyLogo,
                            click = SiteKey.DomainName,
                        };
                        var body = "Welcome to " ?? "EzyTM";
                        var messageInformation = new
                        {
                            data = data,
                            sound = "default",
                            registration_ids = tokens
                        };
                        string jsonMessage = JsonConvert.SerializeObject(messageInformation);

                        string ServerKey = "AAAAU3lcMOA:APA91bHeGo4fMIQfquhMTSSKxrNn34Z4QFY2jb3k6vnMWLvKfqtxjK0WOpa2Kw4z3TEX64Fyb5vMZ0xVvsl-0pszg0-X3P6iv-VBmM2Msk2gBhbAunIII2-2VELDwS53QXH7LH-HPvSY";
                        var client = new RestClient("https://fcm.googleapis.com/fcm/send");
                        client.Timeout = -1;
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("Authorization", "key=" + ServerKey);
                        request.AddHeader("Content-Type", "application/json");
                        request.AddParameter("application/json", jsonMessage, ParameterType.RequestBody);

                        IRestResponse response = client.Execute(request);
                        //var pushSent = PushNotificationLogic.SendPushNotification(tokens, "EzyTM Technologies", body, data);
            }
            if (sendsmsdto.IsSms == true)
            {
                if (!string.IsNullOrWhiteSpace(sendsmsdto.UserID) && Convert.ToString(sendsmsdto.UserID) != "0")
                {
                }
                else
                {
                    var datalist = adminUserService.GetUserList(3);
                    foreach (var user in datalist)
                    {
                        sMSapiCall.SendSms(user.UserProfile?.MobileNumber ?? "", sendsmsdto.Message);
                    }
                }
            }
            ShowSuccessMessage("Success!", "Message Sent Successfully !!", false);
            return RedirectToAction("sendsms");
        }

    }
}