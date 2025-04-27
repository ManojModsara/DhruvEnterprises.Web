using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using System;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Controllers
{

    public class SettingController : BaseController
    {
        public ActionAllowedDto actionAllowedDto;
        private IUserService adminUserService;
        ActivityLogDto activityLogModel;
        public SettingController(IUserService _adminUserService, IActivityLogService _activityLogService, IRoleService _roleService) : base(_activityLogService, _roleService)
        {
            this.adminUserService = _adminUserService;
            this.actionAllowedDto = new ActionAllowedDto();
            this.activityLogModel = new ActivityLogDto();
        }
        // GET: Setting
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UserProfile()
        {
            UpdateActivity("Setting UserProfile REQUEST", "GET:Setting/UserProfile/");

            //if (Session["OTP"] == null)
            //{
            //  OtpVerficationDto otpVerficationDto = new OtpVerficationDto();
            //  otpVerficationDto.Url = "Setting/UserProfile";
            //  otpVerficationDto.UserId = CurrentUser.UserID;
            //  return RedirectToAction("OtpVerfication", "Account", otpVerficationDto);
            //}
            UserProfilesDTO adminUserDto = new UserProfilesDTO();
            User adminUser = adminUserService.GetUser(CurrentUser.UserID);
            adminUserDto.Id = adminUser.Id;
            adminUserDto.Name = adminUser?.UserProfile?.FullName;
            adminUserDto.EmailID = adminUser?.UserProfile?.Email;
            adminUserDto.Mobileno = adminUser?.UserProfile?.MobileNumber;
            adminUserDto.ORGName = adminUser?.UserProfile?.ORGName;
            adminUserDto.GSTNo = adminUser?.UserProfile?.GSTNumber;
            adminUserDto.Contactno = adminUser?.UserProfile?.PhoneNumber;
            adminUserDto.GSTADDRESS = adminUser?.UserProfile?.GSTRegAddress;
            adminUserDto.Address = adminUser?.UserProfile?.Address;
            adminUserDto.ApiToken = adminUser?.TokenAPI;
            adminUserDto.HiddenApiToken = adminUser?.TokenAPI;

            adminUserDto.IpAddress = adminUser?.LoginIP;
            adminUserDto.CallBackUrl = adminUser?.CallbackURL;
            return View(adminUserDto);
        }

        [HttpPost]
        public ActionResult UserProfile(UserProfilesDTO model)
        {
            UpdateActivity("Setting UserProfile REQUEST", "POST:Setting/UserProfile/", "id=" + model.Id);

         
            string message = string.Empty;
            //if (Session["OTP"] == null)
            //{
            //    OtpVerficationDto otpVerficationDto = new OtpVerficationDto();
            //    return PartialView("_OtpVerfication");
            //}
            try
            {
                if (CurrentUser.UserID != 0)
                {
                    User user = adminUserService.GetUser(model.Id);
                    user.LoginIP = model.IpAddress;
                    user.CallbackURL = model.CallBackUrl;
                    if (user.UserProfile != null)
                    {


                        user.TokenAPI = model.ChangeToken?Guid.NewGuid().ToString():user.TokenAPI;
                        user.UserProfile.Address = model.Address;
                        user.UserProfile.GSTNumber = model.GSTNo;
                        user.UserProfile.ORGName = model.ORGName;
                        user.UserProfile.PhoneNumber = model.Contactno;
                        user.UserProfile.GSTRegAddress = model.GSTADDRESS;
                        user.UserProfile.Address = model.Address;
                        user.UserProfile.FullName = model.Name;
                       

                        adminUserService.Save(user);
                    }
                    ShowSuccessMessage("Success!", "User has been Update", false);
                    return RedirectToAction("UserProfile");
                }
            }
            catch (Exception Ex)
            {
                var msg = Ex.GetBaseException().ToString();
                message = "An internal error found during to process your requested data!";
                ModelState.AddModelError("error", message);
            }
            return View();
        }

        private void UpdateActivity(string activityName, string ativityPage, string remark = "")
        {
            try
            {
                activityLogModel.ActivityName = activityName;
                activityLogModel.ActivityPage = ativityPage;
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