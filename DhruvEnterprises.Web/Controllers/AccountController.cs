using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Data;
using DhruvEnterprises.Web.Code.LIBS;
using DhruvEnterprises.Service;
using DhruvEnterprises.Core;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using DhruvEnterprises.Web.SmsApi;
namespace DhruvEnterprises.Web.Controllers
{
    [AllowAnonymous()]
    public class AccountController : BaseController
    {
        #region "Fields"
        private ILoginService loginService;
        private IRoleService roleService;
        private IUserService userService;
        SMSapiCall sMSapiCall;
        ActivityLogDto aLogdto;
        private IRequestResponseService reqResService;
        private IUserKycService repoUserKYC;
        public GlobalSettingAllowedDto globalSettingAllowedDto;
        private IEmailApiService emailApiService;
        #endregion

        #region "Constructor"
        public AccountController(IEmailApiService _emailApiService, IUserKycService _repoUserKYC, IRequestResponseService _reqResService, ILoginService _userloginService, IRoleService _userroleService, IUserService _adminUserService, IActivityLogService _activityLogService) : base(_activityLogService, _userroleService)
        {
            this.repoUserKYC = _repoUserKYC;
            this.loginService = _userloginService;
            this.roleService = _userroleService;
            this.userService = _adminUserService;
            this.aLogdto = new ActivityLogDto();
            this.reqResService = _reqResService;
            this.sMSapiCall = new SMSapiCall();
            this.globalSettingAllowedDto = new GlobalSettingAllowedDto();
            this.emailApiService = _emailApiService;
        }
        #endregion

        #region "Action"
        [HttpGet]
        public ActionResult Index()
        {
            string pass = EncryptDecrypt.Decrypt("gO1lL/H7RBaicQdFUX4WMg==");
            //apiCall.Post("https://mrobotics.in/api/order_id_status?api_token=e9a41bcf-45b9-4769-9e4b-f889068470f3&order_id=D200220144144B345B3", "", "", "", ref userReqRes);
            UpdateActivity("LOGIN REQUEST", "Get:Account/Index");
            return View(new LoginDto());
        }
        [HttpPost]
        public ActionResult Index(LoginDto model)
        {
            string returnurl = Request.UrlReferrer.ToString();
            // returnurl = returnurl.Contains("=")?returnurl.Split('=')[1]:string.Empty;
            UpdateActivity("LOGIN REQUEST", "Post:Account/Index", model.Email);
            if (ModelState.IsValid)
            {
                //Business.UserLogin objLogin = Business.UserLogin.GetUsers(model.Email, model.Password, true);
                User user = loginService.GetUserDeatils(model.Email, EncryptDecrypt.Encrypt(model.Password));
                if (user != null)
                {
                    string ipaddress = GeneralMethods.Fetch_UserIP();
                    string[] Ip = user.LoginIP?.Replace(" ", "")?.Split(',');
                    if (user.RoleId != 3 && (string.IsNullOrEmpty(user.LoginIP) || !Ip.Any(ip => ip == ipaddress || ip == "*")))
                    {
                        ShowErrorMessage("Failed!", "Invalid User Details.");
                        UpdateActivity("LOGIN FAILED", "Post:Account/Index", "Ip not matching, userid=" + user.Id);
                    }
                    else
                    {
                        if (model.RememberMe)
                        {
                            Response.Cookies["Username"].Expires = DateTime.Now.AddDays(30);
                            Response.Cookies["Password"].Expires = DateTime.Now.AddDays(30);
                        }
                        else
                        {
                            Response.Cookies["Username"].Expires = DateTime.Now.AddDays(-1);
                            Response.Cookies["Password"].Expires = DateTime.Now.AddDays(-1);
                        }
                        Response.Cookies["Username"].Value = model.Email.Trim();
                        Response.Cookies["Password"].Value = model.Password.Trim();
                        User userInfo = loginService.GetUserDeatilByEmail(model.Email);
                        //  var json = JsonConvert.SerializeObject(userInfo);
                        CreateAuthenticationTicket(userInfo, model.RememberMe);
                        System.Web.HttpCookie userSessionCookies = new System.Web.HttpCookie("ShaktiUserSessionCookies");
                        //    userSessionCookies.Value = json.ToString();
                        userSessionCookies.Expires = DateTime.Now.AddHours(10);
                        Response.Cookies.Add(userSessionCookies);
                        ShowSuccessMessage("Success!", "Hi" + userInfo.UserProfile?.FullName ?? "" + "User logged in successfully");

                        try
                        {
                            aLogdto.ActivityName = "LOGIN SUCCESS";
                            aLogdto.ActivityPage = "Post:Account/Index";
                            aLogdto.Remark = model.Email;
                            aLogdto.UserId = userInfo.Id;
                            LogActivity(aLogdto);
                        }
                        catch (Exception e)
                        {
                            LogException(e);
                        }
                        //if (!string.IsNullOrEmpty(returnurl))
                        //{
                        //    Redirect(returnurl);
                        //}
                        return RedirectToAction("Index", "Home", new { id = userInfo.Id });
                    }

                }
                else
                {
                    ShowErrorMessage("Failed!", "Invalid Username or Password.");
                }
            }
            else
            {
                UpdateActivity("LOGIN FAILED", "Post:Account/Index", "Invalid Credentials, Email: " + model.Email + ", Password: " + model.Password);
                ShowErrorMessage("Failed!", "Invalid Username or Password.");

            }
            return View();
        }

        public ActionResult ForgotPassword()
        {
            UpdateActivity("FORGOTPASSWORD", "Get:Account/ForgotPassword");
            return View(new ForgotPasswordDto());
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordDto model)
        {
            UpdateActivity("FORGOTPASSWORD", "Post:Account/ForgotPassword");
            if (model != null && model.Email != null && model.Email != "")
            {
                try
                {
                    var objforgotmail = loginService.GetUserDeatilByEmail(model.Email);
                    if (objforgotmail != null)
                    {
                        objforgotmail.ResetCode = Guid.NewGuid().ToString();
                        objforgotmail = loginService.Update(objforgotmail);

                        #region "Send Reset Link"
                        //Value Array: v1 = Name, v2 = domain , v3 = guid,
                        FlexiMail objMail = new FlexiMail
                        {
                            From = SiteKey.From,
                            To = objforgotmail.Username,
                            CC = "",
                            BCC = "",
                            Subject = "Forgot Passoword",
                            MailBodyManualSupply = true,
                            ValueArray = new string[]  {
                                                    objforgotmail.UserProfile.FullName,
                                                    SiteKey.DomainName,
                                                    objforgotmail.ResetCode

                                                    }
                        };


                        objMail.MailBody = objMail.GetHtml("ForgotPassword.html");
                        objMail.Send();

                        ViewBag.Invalid = "Password has been sent your email. ";
                        return View();
                        #endregion
                    }

                    else
                    {
                        ViewBag.Invalid = "Invalid email";
                        try
                        {
                            aLogdto.ActivityName = "FORGOTPASSWORD FAILED";
                            aLogdto.ActivityPage = "Post:Account/ForgotPassword";
                            aLogdto.Remark = "Email not valid, email: " + model.Email;
                            aLogdto.UserId = CurrentUser?.UserID ?? 0;
                            LogActivity(aLogdto);
                        }
                        catch (Exception e)
                        {
                            LogException(e);
                        }



                        model.Email = "";
                        return View();
                    }

                }
                catch (Exception ex)
                {
                    ShowErrorMessage("Error", "Internal error occured.", true);



                    try
                    {
                        aLogdto.ActivityName = "FORGOTPASSWORD FAILED";
                        aLogdto.ActivityPage = "Post:Account/ForgotPassword";
                        aLogdto.Remark = "Exception: " + ex.Message;
                        aLogdto.UserId = CurrentUser?.UserID ?? 0;
                        LogActivity(aLogdto);
                    }
                    catch (Exception e)
                    {
                        LogException(e);
                    }
                    return View(model);
                }


            }
            //ShowErrorMessage("Error", "Internal error occured.", false);
            return View(model);
        }

        public static string CreateRandomOtp(int OtpLength)
        {
            string _allowedChars = "0123456789";
            Random randNum = new Random();
            char[] chars = new char[OtpLength];
            int allowedCharCount = _allowedChars.Length;
            for (int i = 0; i < OtpLength; i++)
            {
                chars[i] = _allowedChars[(int)((_allowedChars.Length) * randNum.NextDouble())];
            }
            return new string(chars);
        }
        private void SendEmail(string MessageBody = "", string mailto = "", string Subject = "")
        {
            try
            {
                #region "Activation Code"
                //Value Array: v1 = Name, v2 = domain , v3 = guid,
                //var emails = emailApiService.EmailApiList();
                //FlexiMail objMail = new FlexiMail
                //{
                //    From = emails.FromAddress,
                //    To = mailto,
                //    CC = "",
                //    BCC = "",
                //    Subject = MessageBody,
                //    MailBodyManualSupply = true,
                //    ValueArray = new string[]  {
                //                                    mailto,
                //                                    SiteKey.DomainName,
                //                                    Subject
                //                                    }
                //};
                //objMail.MailBody = objMail.GetHtml("Activation.html");
                //objMail.Send(emails.UserName, emails.Password);
                #endregion
            }
            catch (Exception ex)
            {
            }

        }


        public ActionResult RecoverPassword(Guid id)
        {
            UpdateActivity("FORGOTPASSWORD", "Get:Account/RecoverPassword", id != Guid.Empty ? id.ToString() : "RECOVERPASSWORD FAILED");


            if (id != Guid.Empty)
            {
                ResetPasswordDto model = new ResetPasswordDto();

                User user = loginService.GetUserDeatilByGuid(id);
                if (user != null)
                {
                    model.ResetToken = id;
                    model.UserId = user.Id;

                }
                return View(model);
            }
            return View(new ResetPasswordDto());
        }

        [HttpPost]
        public ActionResult RecoverPassword(ResetPasswordDto model)
        {
            UpdateActivity("RecoverPassword", "Post:Account/RecoverPassword");

            if (ModelState.IsValid)
            {
                User user = loginService.GetUserDeatilByGuid(model.ResetToken);
                if (user != null)
                {
                    user.ResetCode = null;
                    user.Password = model.Password;
                    loginService.Update(user);
                    ShowSuccessMessage("Success", "Password have been saved successfullly", false);
                    return RedirectToAction("index", "account");
                }
                else
                {
                    ShowErrorMessage("Error", "We are sorry, but the link you are trying to access has expired. You can either login or go to home page.", true);
                    UpdateActivity("RecoverPassword", "Post:Account/RecoverPassword", "Invalid Reset Link: " + model.ResetToken);

                }

            }
            return View();
        }

        public ActionResult ChangePassword()
        {
            UpdateActivity("CHANGEPASSWORD", "Get:Account/ChangePassword");

            if (CurrentUser != null && CurrentUser.UserID > 0)
            {
                ChangePasswordDto model = new ChangePasswordDto();

                User user = loginService.GetUserDeatilById(CurrentUser.UserID);
                if (user != null)
                {
                    // model.ResetToken = userId;
                    model.UserId = user.Id;
                    model.CurrentPassword = user.Password;

                }
                return View(model);
            }
            return View(new ChangePasswordDto());
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordDto model)
        {
            UpdateActivity("CHANGEPASSWORD", "Post:Account/ChangePassword", "PWDold:" + model.CurrentPassword + " PWDnew" + model.NewPassword);

            if (ModelState.IsValid)
            {
                User user = loginService.GetUserDeatilById(CurrentUser.UserID);
                if (user != null)
                {
                    if (user.Password == model.CurrentPassword)
                    {
                        user.ResetCode = null;
                        user.Password = model.NewPassword;//AESSecurity.EncryptStringToBytes(model.Password);
                        loginService.Update(user);
                        ShowSuccessMessage("Success", "Password has been changed successfully.", true);
                    }
                    else
                    {
                        ShowErrorMessage("Error", "Current password doesn't match.", true);

                    }

                }
                else
                {
                    ShowErrorMessage("Error", "We are sorry, but the link you are trying to access has expired. You can either login or go to home page.", false);
                    UpdateActivity("CHANGEPASSWORD", "Post:Account/ChangePassword", "PWDold:" + model.CurrentPassword + " PWDnew" + model.NewPassword);

                }

            }
            return View();
        }

        public ActionResult AdminMenu()
        {
            int cmpCount = 0;
            int prcCount = 0;
            int wrCount = 0;
            string noteMsg = string.Empty;

            GetCounts(ref cmpCount, ref prcCount, ref wrCount, ref noteMsg);
            var user = userService.GetUser(CurrentUser.UserID);
            ViewBag.WalletBal = user?.UserBal ?? Convert.ToDecimal(0);
            ViewBag.CreditBal = user?.CreditBal ?? Convert.ToDecimal("0.00");

            ViewBag.cmpCount = cmpCount;
            ViewBag.prcCount = prcCount;
            ViewBag.wrCount = wrCount;
            TempData["noteMsg"] = noteMsg;

            int roleid = CurrentUser.RoleId == 3 ? CurrentUser.RoleId : 0;
            var notebar = userService.GetNotificationBarList((byte)roleid).OrderByDescending(x => x.IsActive).OrderByDescending(x => x.UpdatedDate).FirstOrDefault();

            if (notebar != null)
                TempData["notebar"] = notebar.NotificationMsg;


            ViewBag.UserFullName = user?.UserProfile?.FullName ?? CurrentUser.Username;
            int roleID = CurrentUser.Roles.FirstOrDefault();
            var menus = roleService.GetMenusByRoleId(roleID).OrderBy(x => x.ParentId).ThenBy(x => x.Priority).ToList();
            return PartialView("_MenuBar", menus);
        }

        public ActionResult AdminMenu1()
        {
            int cmpCount = 0;
            int prcCount = 0;
            int wrCount = 0;
            string noteMsg = string.Empty;
            GetCounts(ref cmpCount, ref prcCount, ref wrCount, ref noteMsg);
            var user = userService.GetUser(CurrentUser.UserID);
            ViewBag.WalletBal = user?.UserBal ?? Convert.ToDecimal(0);
            ViewBag.CreditBal = user?.CreditBal ?? Convert.ToDecimal("0.00");
            ViewBag.cmpCount = cmpCount;
            ViewBag.prcCount = prcCount;
            ViewBag.wrCount = wrCount;
            TempData["noteMsg"] = noteMsg;
            int roleid = CurrentUser.RoleId == 3 ? CurrentUser.RoleId : 0;

            var notebar = userService.GetNotificationBarList((byte)roleid).OrderByDescending(x => x.IsActive).OrderByDescending(x => x.UpdatedDate).FirstOrDefault();
            if (notebar != null)
                TempData["notebar"] = notebar.NotificationMsg;
            ViewBag.UserFullName = user?.UserProfile?.FullName ?? CurrentUser.Username;
            int roleID = CurrentUser.Roles.FirstOrDefault();
            var menus = roleService.GetMenusByRoleId(roleID).OrderBy(x => x.ParentId).ThenBy(x => x.Priority).ToList();
            return PartialView("_MenuBar1", menus);
        }



        [HttpGet]
        public ActionResult Logout()
        {
            var c = CurrentUser.UserID;

            Cache.RemoveAll();

            RemoveAuthentication();
            return RedirectToAction("index", "account");
        }

        public ActionResult Success()
        {
            RemoveAuthentication();
            return RedirectToAction("success");
        }

        public ActionResult OtpVerfication(OtpVerficationDto OTPVD)
        {
            return View("OtpVerfication", OTPVD);
        }

        private void UpdateActivity(string activityName, string ativityPage, string remark = "")
        {
            try
            {
                aLogdto.ActivityName = activityName;
                aLogdto.ActivityPage = ativityPage;
                aLogdto.Remark = remark;
                aLogdto.UserId = CurrentUser?.UserID ?? 0;
                LogActivity(aLogdto);
            }
            catch (Exception ex)
            {

                LogException(ex);
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

        public ActionResult UserSignUp()
        {
            UpdateActivity("User CreateEdit REQUEST", "GET:User/CreateEdit/");
            ViewBag.AStates = userService.GetStates();
            return View("UserSignUp");
        }
        [HttpPost]
        public ActionResult UserSignUp(UserDto model, FormCollection FC)
        {
            UpdateActivity("User UserSignUp SUBMIT", "POST:Account/UserSignUp/");
            string message = string.Empty;
            try
            {
                if (ModelState.IsValid)
                {
                    bool CheckUserExists = userService.SignUpUserExists(model.EmailId.ToLower(), model.ContactNo, model.Username);// check records in user table 
                    if (CheckUserExists == true)
                    {
                        ShowErrorMessage("Error!", "Either Username , Mobile or Email already Taken!", false);
                        return RedirectToAction("UserSignUp");
                        //return View("UserSignUp");
                    }
                    else
                    {
                        User user = new User();
                        user.RoleId = 3;
                        user.Username = model.Username;
                        user.Password = EncryptDecrypt.Encrypt(model.Password);
                        user.IsActive = true;
                        user.IsLocked = false;
                        user.PackageId = 2;
                        user.StateID = model.StateID;
                        user.AddedDate = DateTime.Now;
                        user.TokenAPI = Guid.NewGuid().ToString();
                        user.HKey = Guid.NewGuid();
                        user.HPass = Guid.NewGuid();
                        //userService.Save(user);
                        //if (user.UserProfile != null)
                        //{
                        //    user.UserProfile = new UserProfile();
                        //    user.UserProfile.FullName = user.UserProfile.FullName;
                        //    user.UserProfile.ORGName = user.UserProfile.ORGName ?? null;
                        //    user.UserProfile.GSTNumber = user.UserProfile.GSTNumber ?? null;
                        //    user.UserProfile.GSTRegAddress = user.UserProfile.GSTRegAddress ?? null;
                        //    user.UserProfile.Email = user.UserProfile.Email ?? null;
                        //    user.UserProfile.PhoneNumber = user.UserProfile.PhoneNumber ?? null;
                        //    user.UserProfile.MobileNumber = user.UserProfile.MobileNumber ?? null;
                        //}
                        //userService.Save(user);
                        //int id = user.Id;
                        //string message1 = "Your Registeration is done succesfully.\n Username=" + user.Username + " and Password=" + EncryptDecrypt.Decrypt(user.Password);
                        //if (id >= 1)
                        //{
                        //    ShowSuccessMessage("Succes", message1);
                        //    return RedirectToAction("Index");
                        //}
                        string _name = FC["Name"];
                        string _cname = FC["Company"];
                        string _gstNo = FC["gstNo"];
                        string _gstaddress = FC["gstRaddress"];
                        string _phone = FC["phone"];
                        user.UserProfile = new UserProfile();
                        user.UserProfile.FullName = _name;
                        user.UserProfile.ORGName = _cname;
                        user.UserProfile.GSTNumber = _gstNo;
                        user.UserProfile.GSTRegAddress = _gstaddress;
                        user.UserProfile.Email = model.EmailId;
                        user.UserProfile.MobileNumber = model.ContactNo;
                        user.UserProfile.PhoneNumber = _phone;
                        string otp = CreateRandomOtp(5);
                        user.OTP = otp;
                        TempData["UserSignupData"] = user;
                        string message1 = "Your one time password is " + otp + "\n Thank you,\n SHAKTI";
                        var action = RouteData.Values["action"].ToString();
                        globalSettingAllowedDto = GlobalSettingAllowed(action);
                        if (globalSettingAllowedDto.AllowSms == true)
                        {
                            sMSapiCall.SendSms(model.ContactNo.Trim(), message1);
                        }
                        if (globalSettingAllowedDto.AllowEmail == true)
                        {
                            SendEmail("OTP", model.EmailId, otp);
                        }
                        return RedirectToAction("SignUpOtp");
                    }
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


        public ActionResult SignUpOtp()
        {
            UpdateActivity("SignUpOtp", "Get:Account/SignUpOtp");
            if (TempData["UserSignupData"] == null)
            {
                return RedirectToAction("UserSignUp", "Account");
            }
            return View();
        }

        [HttpPost]
        public ActionResult SignUpOtp(FormCollection FC)
        {
            UpdateActivity("SignUpOtp", "Post:Account/SignUpOtp");
            if (ModelState.IsValid)
            {
                User user1 = TempData["UserSignupData"] != null ? (User)TempData["UserSignupData"] : new User();
                if (user1 != null)
                {
                    if (user1.OTP == FC["otp"])
                    {
                        User user = new User();
                        user.RoleId = 3;
                        user.Username = user1.Username;
                        user.Password = user1.Password;
                        user.IsActive = true;
                        user.IsLocked = false;
                        user.PackageId = 2;
                        user.StateID = user1.StateID;
                        user.AddedDate = DateTime.Now;
                        user.TokenAPI = Guid.NewGuid().ToString();
                        user.HKey = Guid.NewGuid();
                        user.HPass = Guid.NewGuid();
                        userService.Save(user);
                        if (user1.UserProfile != null)
                        {
                            user.UserProfile = new UserProfile();
                            user.UserProfile.FullName = user1.UserProfile.FullName;
                            user.UserProfile.ORGName = user1.UserProfile.ORGName;
                            user.UserProfile.GSTNumber = user1.UserProfile.GSTNumber;
                            user.UserProfile.GSTRegAddress = user1.UserProfile.GSTRegAddress;
                            user.UserProfile.Email = user1.UserProfile.Email;
                            user.UserProfile.PhoneNumber = user1.UserProfile.PhoneNumber;
                            user.UserProfile.MobileNumber = user1.UserProfile.MobileNumber;
                        }
                        else
                        {
                            //user.UserProfile = new UserProfile();
                            //user.UserProfile.FullName = model.Name;
                            //user.UserProfile.Email = model.EmailId;
                            //user.UserProfile.MobileNumber = model.ContactNo;
                        }
                        userService.Save(user);
                        int id = user.Id;
                        TempData["userid"] = id;
                        if (id >= 1)
                        {
                            string message1 = "Your Registeration is done succesfully.\n Username=" + user.Username + " and Password=" + EncryptDecrypt.Decrypt(user.Password) + " Keep it save for future access.\n Thank you,\n SHAKTI";
                            var action = RouteData.Values["action"].ToString();
                            globalSettingAllowedDto = GlobalSettingAllowed(action);
                            if (globalSettingAllowedDto.AllowSms == true)
                            {
                                sMSapiCall.SendSms(user.UserProfile.MobileNumber.Trim(), message1);
                            }
                            if (globalSettingAllowedDto.AllowEmail == true)
                            {
                                SendEmail("Registration Succesfully", user.UserProfile.Email, message1);
                            }
                        }

                        return RedirectToAction("UserKyc", new { @gst = user1.UserProfile.GSTNumber });
                        //return RedirectToAction("UserKyc");
                    }
                    else
                    {
                        ShowErrorMessage("Error", "OTP doesn't match.", true);

                    }

                }
                else
                {
                    ShowErrorMessage("Error", "We are sorry, but the link you are trying to access has expired. You can either login or go to home page.", false);


                }

            }
            return View();
        }

        public ActionResult UserKyc(KYCDataDtO models, string gst)
        {

            //models.Id =(int) TempData["userid"];
            int userID = Convert.ToInt32(TempData["userid"]);
            if (userID == 0)
            {
                return RedirectToAction("index");
            }
            UpdateActivity("UserKyc", "Get:User/UserKyc");
            // ViewBag.actionAllowed = action = ActionAllowed("UpLoadKYCDoc", CurrentUser.RoleId);
            if (!string.IsNullOrEmpty(gst))
            {
                if (!models.DocId.Contains(4))
                {
                    models.DocId.Add(4);
                }
                ViewBag.gstn = gst;

            }
            ViewBag.DocumentTypedList = userService.GetKYCDocList().ToList();
            return View("UserKyc", models);

        }

        [HttpPost]
        public ActionResult UserKyc(KYCDataDtO model, FormCollection FC)
        {
            int userID = Convert.ToInt32(TempData["userid"]);
            if (userID == 0)
            {
                return RedirectToAction("index");
            }
            var dictionary = model.DocId.Zip(model.DocNumber, (k, v) => new { Key = k, Value = v })
                    .ToDictionary(x => x.Key, x => x.Value);

            List<ImageInfo> images = new List<ImageInfo>();
            ImageInfo img = new ImageInfo();
            if (dictionary.ContainsKey(4))
            {
                var gst = dictionary.SingleOrDefault(x => x.Key == 4);
                img.DocId = gst.Key;
                img.DocumentNumber = gst.Value;
                img.Userid = userID;
                images.Add(img);
                img = GetImgObj(img, userID, 0, "", true);
                dictionary.Remove(4);
            }
            UpdateActivity("User UserKyc SUBMIT", "POST:Account/UserKyc/", "userid=" + userID);
            string message = string.Empty;
            if (Request.Files.Count > 0)
            {
                if (model.DocNumber.Count() > 0)
                {
                    foreach (var data in model.DocNumber)
                    {
                        if (data == "")
                        {
                            ModelState.AddModelError("DocNumber", "Document Number is mandatory");
                        }
                    }
                    if (!ModelState.IsValid)
                    {
                        ShowErrorMessage("Error!", "Check All Required Values.", false);
                        return RedirectToAction("UserKyc", "Account");
                    }
                }
                else
                {
                    ModelState.AddModelError("DocNumber", "Document Number is mandatory");
                    ShowErrorMessage("Error!", "Check All Required Values.", false);
                    return RedirectToAction("UserKyc", "Account");
                }

                try
                {

                    int k = 1, j = 0;
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  
                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }
                        if (k % 2 == 0)
                        {

                            img = GetImgObj(img);
                            img.BakImg = fname = img.DocumentNumber + fname;
                            images.Add(img);
                            k++;
                        }
                        else
                        {

                            int id = dictionary.ElementAt(j).Key;
                            string no = dictionary.ElementAt(j).Value;
                            if (id == 5)
                            {
                                img = GetImgObj(img, userID, id, no, true);
                                img.FrontImg = fname = img.DocumentNumber + fname;
                                images.Add(img);
                                k = 1;
                            }
                            else
                            {
                                img = GetImgObj(img, userID, id, no, true);
                                img.FrontImg = fname = img.DocumentNumber + fname;
                                k++;
                            }
                            j++;

                        }
                        string filepath = HttpContext.Server.MapPath("~/KYCData/");
                        if (!Directory.Exists(filepath))
                        {
                            Directory.CreateDirectory(filepath);
                        }
                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/KYCData/"), fname);
                        file.SaveAs(fname);
                        if (k > 2)
                            k = 1;
                    }
                    foreach (var userdata in images)
                    {
                        UserKYC userk = new UserKYC
                        {
                            UserId = userID,
                            DocumentTypedId = userdata.DocId,
                            DocumentNumber = userdata.DocumentNumber,
                            AddedBy = userID,
                            AddedDate = DateTime.Now,
                            Front_Image = userdata.FrontImg,
                            Back_Image = userdata.BakImg
                        };
                        try
                        {
                            repoUserKYC.Save(userk);
                        }
                        catch
                        {

                        }
                    }
                    ShowSuccessMessage("Success!", "Your Registration has been done succesfully..", false);
                    return RedirectToAction("Index", "Account");
                }
                catch (Exception Ex)
                {
                    var msg = Ex.GetBaseException().ToString();
                    message = "An internal error found during to process your requested data!";
                    ShowErrorMessage("Error!", message, true);
                    return RedirectToAction("UserKyc", "Account");
                }
            }
            else
            {
                ShowErrorMessage("Error!", "No files selected.", false);
                return RedirectToAction("UserKyc", "Account");

            }
        }


        private ImageInfo GetImgObj(ImageInfo img, int UserId = 0, int DocId = 0, string DocNo = null, bool flag = false)
        {
            if (flag)
            {
                //return new ImageInfo { Userid = CurrentUser.UserID, DocumentNumber = DocNo, DocId = DocId };
                return new ImageInfo { Userid = UserId, DocumentNumber = DocNo, DocId = DocId };
            }
            else
            {
                return img;
            }
        }


        public ActionResult SkipKyc()
        {
            ShowSuccessMessage("Success!", "Your Registration has been done succesfully without Kyc..", false);
            return RedirectToAction("Index", "Account");
        }

        #endregion

        #region FireBase TokenInsert
        public class Tokens
        {
            public string token { get; set; }
        }

        [HttpPost]
        public bool Registration(string token)
        {
            #region Validation Check
            if (string.IsNullOrWhiteSpace(token))
            {

            }
            #endregion
            else
            {
                try
                {
                    bool vaild = register(token);
                    if (vaild == true)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return false;
        }

        public bool register(string Mobileno)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(SiteKey.SqlConn))
                {
                    using (SqlCommand cmd = new SqlCommand("API_Fire", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@webtoken", Mobileno);
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion
    }
}


