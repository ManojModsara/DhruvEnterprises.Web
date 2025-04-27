using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web;
using DhruvEnterprises.Web.Code.Attributes;
using DhruvEnterprises.Web.Controllers;
using DhruvEnterprises.Web.Models.Others;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static DhruvEnterprises.Core.Enums;
using DhruvEnterprises.Web.SmsApi;
using DhruvEnterprises.Web.Code.LIBS;
using System.Data;
using System.Data.SqlClient;
using RestSharp;
using Newtonsoft.Json;

namespace DhruvEnterprises.Controllers
{
    [CustomAuthorization()]
    public class UserController : BaseController
    {
        public ActionAllowedDto action;
        private IUserService userService;
        private IPackageService packageService;
        public GlobalSettingAllowedDto globalSettingAllowedDto;
        ActivityLogDto activityLogModel;
        SMSapiCall sMSapiCall;
        private IEmailApiService emailApiService;
        private readonly IApiService apiService;
        public UserController(IApiService _apiService, IEmailApiService _emailApiService, IUserService _adminUserService, IActivityLogService _activityLogService, IRoleService _roleService, IPackageService _packageService) : base(_activityLogService, _roleService)
        {
            this.userService = _adminUserService;
            this.packageService = _packageService;
            this.action = new ActionAllowedDto();
            this.activityLogModel = new ActivityLogDto();
            this.globalSettingAllowedDto = new GlobalSettingAllowedDto();
            this.emailApiService = _emailApiService;
            this.sMSapiCall = new SMSapiCall();
            this.apiService = _apiService;
        }

        // GET: Admin/AdminUser
        public ActionResult Index(int? a, int? u, int? r, int? p, int? s, string n = "", string e = "", string k = "", string i = "")
        {
            UpdateActivity("Users REQUEST", "GET:User/Index/");
            ViewBag.actionAllowed = action = ActionAllowed("User", CurrentUser.RoleId);

            UserFilterDto ft = new UserFilterDto()
            {
                UserId = u ?? 0,
                RoleId = r ?? 0,
                PackId = p ?? 0,
                StatusId = s ?? 0,
                ContactNo = n,
                EmailId = e,
                ApiKey = k,
                IPAddress = i,
                Isa = a ?? 0
            };
            ViewBag.FilterData = TempData["UserFilterDto"] = ft;
            ViewBag.PackList = packageService.GetPackageList().Where(x => x.PTypeId == 1).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.PackageName, Selected = (x.Id == ft.PackId) }).ToList();
            ViewBag.UserList = userService.GetUserList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? string.Empty, Selected = (x.Id == ft.UserId) }).ToList();
            ViewBag.RoleList = userService.GetAdminRole().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.RoleName, Selected = ((x.Id == ft.RoleId)) }).ToList();
            List<SelectListItem> statusList = new List<SelectListItem>();
            statusList.Add(new SelectListItem() { Value = "1", Text = "Active", Selected = (1 == ft.StatusId) });
            statusList.Add(new SelectListItem() { Value = "2", Text = "Not-Active", Selected = (2 == ft.StatusId) });
            statusList.Add(new SelectListItem() { Value = "3", Text = "Locked", Selected = (3 == ft.StatusId) });
            ViewBag.StatusList = statusList;
            return View();
        }

        [HttpPost]
        public ActionResult GetAdminUsers(DataTableServerSide model)
        {
            UpdateActivity("Users REQUEST", "POST:User/Index/");
            ViewBag.actionAllowed = action = ActionAllowed("User", CurrentUser.RoleId);
            var wallet = ActionAllowed("Wallet", CurrentUser.RoleId);
            UserFilterDto ft = TempData["UserFilterDto"] != null ? (UserFilterDto)TempData["UserFilterDto"] : new UserFilterDto();
            KeyValuePair<int, List<User>> users = userService.GetAdminUsers(model, CurrentUser.UserID);
            //  var userbals = adminUserService.GetUserListWithBalace();
            return Json(new
            {
                draw = model.draw,
                recordsTotal = users.Key,
                recordsFiltered = users.Key,
                data = users.Value.Where(x => (CurrentUser.RoleId == 1 ? true : (x.RoleId != CurrentUser.RoleId && x.RoleId != 1))).Select(c => new List<object> {
                    c.Id,
                    (c.UserProfile?.FullName).ToTitle(),
                    c.Role.RoleName,
                    c.UserProfile?.Email,
                    c.UserProfile?.MobileNumber,
                    c.UserBal??0,
                    c.CreditBal??0,
                     c.BlockAmt??0,
                    c.IsActive,
                   (action.AllowCreate?  DataTableButton.EditButton(Url.Action( "createedit", "user",new { id = c.Id })):string.Empty )
                   +"&nbsp;"+
                   (wallet.AllowCreate?  DataTableButton.PlusButton(Url.Action( "Index","Wallet", new { id = c.Id }),"addUserWallet","Add Wallet"):string.Empty)
                   +"&nbsp;"+
                   (action.AllowDelete?  DataTableButton.DeleteButton(Url.Action( "delete","user", new { id = c.Id }),"modal-delete-adminuser"):string.Empty)
                  , action.AllowEdit?true:false
                })
            }, JsonRequestBehavior.AllowGet);

        }        

        public ActionResult CreateEdit(int? id)
        {

            UpdateActivity("User CreateEdit REQUEST", "GET:User/CreateEdit/", "userid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("User", CurrentUser.RoleId, id.HasValue ? 3 : 2);
            UserDto userdto = new UserDto();
            ViewBag.Range10 = Enumerable.Range(1, 10).Select(x => new { Id = x, Name = x }).ToList();
            if (id.HasValue && id.Value > 0)
            {
                User user = userService.GetUser(id.Value);
                userdto.Uid = user.Id;
                userdto.Username = user.Username;
                userdto.PackageId = user.PackageId ?? 0;
                userdto.Password = EncryptDecrypt.Decrypt(user.Password);
                userdto.RoleId = user.RoleId ?? 0;
                userdto.Name = user.UserProfile?.FullName ?? string.Empty;
                userdto.EmailId = user.UserProfile?.Email ?? string.Empty;
                userdto.ContactNo = user.UserProfile?.MobileNumber ?? string.Empty;
                userdto.IsActive = user.IsActive;
                userdto.AddedDate = user.AddedDate;
                userdto.IP = user.LoginIP;
                userdto.UpdatedDate = user.UpdatedDate;
                userdto.CallBackUrl = user.CallbackURL;
                userdto.ApiKey = !string.IsNullOrEmpty(user.TokenAPI) ? new Guid(user.TokenAPI) : Guid.Empty;
                userdto.HKey = user.HKey ?? Guid.Empty;
                userdto.HPass = user.HPass ?? Guid.Empty;
                userdto.ComplaintCallBackUrl = user.ComplainCallbackURL;
                userdto.BlockAmt = user.BlockAmt ?? 0;
                userdto.VendorId = user.VendorId ?? 0;
                userdto.DmtCallBackUrl = user.DMTCallbackURL;
                userdto.CreditLimit = user.CreditLimit;
            }
            int uid = 0;// CurrentUser.UserID;
            ViewBag.Packages = packageService.PackageUserList();
            ViewBag.Roles = userService.GetAdminRole(true); //Where(a => a.Id != (int)RoleType.Admin);
            ViewBag.ApiList = apiService.GetApiList().Where(x => uid == 0 ? true : x.Id == uid).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.ApiName }).ToList();

            return View("createedit", userdto);

        }

        [HttpPost]
        public ActionResult CreateEdit(UserDto model, FormCollection FC)
        {
            UpdateActivity("User CreateEdit SUBMIT", "POST:User/CreateEdit/", "userid=" + model.Id);
            ViewBag.actionAllowed = action = ActionAllowed("User", CurrentUser.RoleId, model.Id > 0 ? 3 : 2);

            string message = string.Empty;
            try
            {
                if (ModelState.IsValid)
                {

                    User user = userService.GetUser(model.Uid) ?? new User();
                    user.Id = model.Uid;
                    user.RoleId = (byte)model.RoleId;
                    user.Username = model.Username;
                    user.Password = EncryptDecrypt.Encrypt(model.Password);
                    user.IsActive = model.IsActive;
                    user.IsLocked = model.IsLocked;
                    int oldPackageId = user.PackageId ?? 0;
                    if (model.PackageId > 0)
                    {
                        user.PackageId = model.PackageId;
                    }
                    //if (model.VendorId > 0)
                    //{                        
                    //    user.VendorId = model.VendorId;
                    //}
                    //else
                    //{
                    //    user.VendorId = null;
                    //}
                    user.VendorId = model.VendorId ?? null;
                    user.LoginIP = model.IP;
                    user.CallbackURL = model.CallBackUrl;
                    user.BlockAmt = model.BlockAmt;
                    if (model.Uid == 0)
                    {
                        user.AddedDate = DateTime.Now;
                        user.TokenAPI = Guid.NewGuid().ToString();
                        user.HKey = Guid.NewGuid();
                        user.HPass = Guid.NewGuid();
                    }
                    else
                    {
                        user.TokenAPI = user.TokenAPI == null ? Guid.NewGuid().ToString() : user.TokenAPI;
                        user.HKey = user.HKey == null ? Guid.NewGuid() : user.HKey;
                        user.HPass = user.HPass == null ? Guid.NewGuid() : user.HPass;
                        user.UpdatedDate = DateTime.Now;
                    }
                    user.ComplainCallbackURL = model.ComplaintCallBackUrl;
                    user.DMTCallbackURL = model.DmtCallBackUrl;
                    user.CreditLimit = model.CreditLimit;
                    // adminUser.ResetCode = model.ResetCode;
                    userService.Save(user);
                    if (user.UserProfile != null)
                    {
                        user.UserProfile.FullName = model.Name;
                        user.UserProfile.Email = model.EmailId;
                        user.UserProfile.MobileNumber = model.ContactNo;
                    }
                    else
                    {
                        user.UserProfile = new UserProfile();
                        user.UserProfile.FullName = model.Name;
                        user.UserProfile.Email = model.EmailId;
                        user.UserProfile.MobileNumber = model.ContactNo;
                    }
                    userService.Save(user);
                    if (oldPackageId != user.PackageId)
                    {
                        string message1 = "Your Package Plan has been changed. Please Login to see more details.\n Thank you,\n Ezytm";
                        var action = "packagechange";
                        globalSettingAllowedDto = GlobalSettingAllowed(action);
                        if (globalSettingAllowedDto.AllowSms == true)
                        {
                            sMSapiCall.SendSms(model.ContactNo.Trim(), message1);
                        }
                        //if (globalSettingAllowedDto.AllowEmail == true)
                        //{
                        //    SendEmail("Package Change", model.EmailId, message1);
                        //}
                    }
                    ShowSuccessMessage("Success!", "User has been saved", false);
                    return NewtonSoftJsonResult(new RequestOutcome<string> { IsSuccess = true, RedirectUrl = Url.Action("Index") });

                }
                else
                {

                    ShowErrorMessage("Error!", "Check All Required Values.", true);

                }


            }
            catch (Exception Ex)
            {
                var msg = Ex.GetBaseException().ToString();
                if (msg.Contains("UNIQUE KEY"))
                {
                    ShowErrorMessage("Error!", "Email/MobileNo already exist.", true);
                }
                else
                {
                    message = "An internal error found during to process your requested data!";
                    ShowErrorMessage("Error!", message, true);
                }
            }
            return CreateModelStateErrors();


        }
        private void SendEmail(string MessageBody = "", string mailto = "", string Subject = "")
        {
            try
            {
                #region "Activation Code"
                //Value Array: v1 = Name, v2 = domain , v3 = guid,
                var emails = emailApiService.EmailApiList();
                FlexiMail objMail = new FlexiMail
                {
                    From = emails.FromAddress,
                    To = mailto,
                    CC = "",
                    BCC = "",
                    Subject = MessageBody,
                    MailBodyManualSupply = true,
                    ValueArray = new string[]  {
                                                    mailto,
                                                    SiteKey.DomainName,
                                                    Subject
                                                    }
                };
                objMail.MailBody = objMail.GetHtml("Activation.html");
                objMail.Send(emails.UserName, emails.Password);
                #endregion
            }
            catch (Exception ex)
            {
            }

        }
        private UserDto BindModelFromEntity(UserDto userModel, User userEntity)
        {
            return new UserDto();
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            UpdateActivity("Delete User", "GET:User/Delete/", "userid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("User", CurrentUser.RoleId, 4);

            //return PartialView("_ModalDelete", new Modal
            //{
            //    Message = AdminRoleRes.DeleteMessage,
            //    Size = ModalSize.Small,
            //    Header = new ModalHeader { Heading = AdminUserRes.DeleteHeader },
            //    Footer = new ModalFooter { SubmitButtonText = "Yes", CancelButtonText = "No" }
            //});
            return View();
        }

        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteAdminUser(int id)
        {
            UpdateActivity("Delete User", "POST:User/Delete/", "userid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("User", CurrentUser.RoleId, 4);
            try
            {
                //var adminUser = adminUserService.GetAdminUser(id);
                //adminUser.IsActive = false;
                //adminUserService.Save(adminUser);
                //ShowSuccessMessage("Success", AdminUserRes.DeleteSuccessMessage, false);
                //ShowSuccessMessage("Success", "hola", false);
            }
            catch (Exception)
            {
                //ShowErrorMessage("Error Occurred", "", false);
            }
            return RedirectToAction("index");
        }

        public bool Active(int id)
        {

            UpdateActivity("Active/Inactive User", "GET:User/Active/", "userid=" + id);
            ViewBag.actionAllowed = action = ActionAllowed("User", CurrentUser.RoleId, 3);

            string message = string.Empty;
            try
            {
                var adminUser = userService.GetUser(id);
                adminUser.IsActive = !adminUser.IsActive;
                return userService.Save(adminUser).IsActive;

            }
            catch (Exception)
            {
                return false;
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
        private string setColor(bool? flag)
        {
            string color = "red";
            if (Convert.ToBoolean(flag))
            {
                color = "green";
            }
            return color;
        }
        public ActionResult ShowKYCUsers(int? a, int? u, int? r, int? p, int? s, string n = "", string e = "", string k = "", string i = "")
        {
            //UpdateActivity("Users REQUEST", "GET:User/ShowPendingKYC/");
            // ViewBag.actionAllowed = action = ActionAllowed("ShowPendingKYC", CurrentUser.RoleId);
            UserFilterDto ft = new UserFilterDto()
            {
                UserId = CurrentUser.UserID,
                RoleId = r ?? 0,
                PackId = p ?? 0,
                StatusId = s ?? 0,
                ContactNo = n,
                EmailId = e,
                ApiKey = k,
                IPAddress = i,
                Isa = a ?? 0
            };
            ViewBag.FilterData = TempData["UserFilterDto"] = ft;
            List<SelectListItem> statusList = new List<SelectListItem>();
            statusList.Add(new SelectListItem() { Value = "1", Text = "Active", Selected = (1 == ft.StatusId) });
            statusList.Add(new SelectListItem() { Value = "2", Text = "Not-Active", Selected = (2 == ft.StatusId) });
            statusList.Add(new SelectListItem() { Value = "3", Text = "Locked", Selected = (3 == ft.StatusId) });
            ViewBag.StatusList = statusList;
            return View();
        }
        [HttpPost]
        public ActionResult GetKYCUsers(DataTableServerSide model)
        {
            List<UserKYC> KYCs = new List<UserKYC>();
            UpdateActivity("GetKYCUsers", "POST:User/GetKYCUsers/");
            //ViewBag.actionAllowed = action = ActionAllowed("ShowPendingKYC", CurrentUser.RoleId);
            UserFilterDto ft = TempData["UserFilterDto"] != null ? (UserFilterDto)TempData["UserFilterDto"] : new UserFilterDto();
            KeyValuePair<int, List<UserKYC>> users = userService.GetKYCsUser(model, CurrentUser.UserID);
            int total = users.Key;
            users.Value.ForEach(delegate (UserKYC KYC)
            {
                if (!KYCs.Any(x => x.UserId == KYC.UserId))
                {
                    KYCs.Add(KYC);
                }
            });
            users = new KeyValuePair<int, List<UserKYC>>(total, KYCs);
            return Json(new
            {
                draw = model.draw,
                recordsTotal = users.Key,
                recordsFiltered = users.Key,
                data = users.Value.Select(c => new List<object> {

                   // action.AllowEdit,
                   //(action.AllowCreate?  DataTableButton.EditButton(Url.Action( "createedit", "user",new { id = c.Id })):string.Empty )
                   "&nbsp;"+
                   //(wallet.AllowCreate?  DataTableButton.PlusButton(Url.Action( "Index","Wallet", new { id = c.Id }),"addUserWallet","Add Wallet"):string.Empty)
                   "&nbsp;"+
                   (!action.AllowDelete?string.Empty:DataTableButton.DeleteButton(Url.Action( "delete","user", new { id = c.Id }),"modal-delete-adminuser")),
                    c.User.Id,
                    c.User.UserProfile.FullName,
                    (c.User.Role?.RoleName),
                    c.User.UserProfile.Email,
                    c.User.UserProfile.MobileNumber,
                    (CurrentUser.IsSuperAdmin ?DataTableButton.Label(setColor(c.User.IsKYC),(Convert.ToBoolean(c.User.IsKYC )?"Approved":"Pending"),"ShowUserByKYCs?Id="+c.User.Id):""),
                    c.User.UserProfile?.FullName??string.Empty,
                    c.AddedDate.ToString(),
                    c.ApprovedDate.ToString()


                })
            }, JsonRequestBehavior.AllowGet);
            //return Json(new
            //{
            //    draw = model.draw,
            //    recordsTotal = users.Key,
            //    recordsFiltered = users.Key,
            //    data = users.Value.Select(c => new List<object> {                      
            //          "&nbsp;"+

            //          "&nbsp;"+
            //          (!action.AllowDelete?string.Empty:DataTableButton.DeleteButton(Url.Action( "delete","user", new { id = c.Id }),"modal-delete-adminuser")),
            //           c.Id,
            //           c.UserId,

            //       })
            //}, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ShowAEPSUsers(int? a, string n = "", string e = "", string i = "")
        {
            UpdateActivity("Users REQUEST", "GET:User/ShowAEPSUsers/");
            ViewBag.actionAllowed = action = ActionAllowed("AepsUsers", CurrentUser.RoleId);
            AEPSUserFilterDto ft = new AEPSUserFilterDto()
            {
                ContactNo = n,
                EmailId = e,
                IPAddress = i,
                Isa = a ?? 0
            };
            ViewBag.FilterData = TempData["AEPSUserFilterDto"] = ft;
            return View();
        }

        #region FireBase Hit

        public string Push(string Message)
        {

            var data = new
            {
                title = "News Notification",
                body = Message,
                icon = SiteKey.DomainName + SiteKey.CompanyLogo,
                image = SiteKey.DomainName + SiteKey.CompanyLogo,
                click = SiteKey.DomainName,
            };
            var body = "Welcome to " ?? "EzyTM";
            var datalist = userService.GetFireBaseToken().OrderByDescending(x => x.Id);
            string d = string.Join(",", datalist.Select(x => x.TokenId));
            string[] tokens = d.Split(',').ToArray();
            //DataTable dt = Tokenlist();
            //var tokens = new string[dt.Rows.Count];
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    tokens[i] = dt.Rows[i]["TokenId"].ToString();
            //}
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
            Console.WriteLine(response.Content);
            return "";
            //return Convert.ToString(data);
        }

        public DataTable Tokenlist()
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    using (SqlCommand cmd = new SqlCommand("select * From FireBaseToken", con))
                    {
                        cmd.CommandType = CommandType.Text;
                        con.Open();
                        SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd);
                        sqlDataAdapter.Fill(dataTable);
                        con.Close();
                        return dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

    }
}