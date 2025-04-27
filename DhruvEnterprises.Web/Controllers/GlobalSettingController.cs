using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.Code.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Controllers
{
    [CustomAuthorization()]
    public class GlobalSettingController : BaseController
    {
        public ActionAllowedDto actionAllowedDto;
        private IUserService adminUserService;
        private ISmsApiService smsApiService;
        private IRoleService roleService;
        ActivityLogDto activityLogModel;
        public GlobalSettingController(IUserService _adminUserService, IActivityLogService _activityLogService, IRoleService _roleService, ISmsApiService _smsApiService) : base(_activityLogService, _roleService)
        {
            this.roleService = _roleService;
            this.adminUserService = _adminUserService;
            this.smsApiService = _smsApiService;
            this.actionAllowedDto = new ActionAllowedDto();
            this.activityLogModel = new ActivityLogDto();
        }
        // GET: GlobalSetting
        public ActionResult Index()
        {
            List<GlobalSetting> ActionList;
            ActionList = roleService.GetActionNames().OrderBy(x => x.Actionname).ToList();
            ViewData["MyData"] = ActionList;
            return View();
        }
        [HttpPost]
        public bool GlobalSetting(List<GlobalSetting> data)
        {

            // UpdateActivity("Role Permission REQUEST", "POST:Role/Permission/", "roleid=" + data.FirstOrDefault()?.RoleId);
            //action = ActionAllowed("Role", CurrentUser.RoleId, 3);
            string message = string.Empty;
            
            try
            {
                //List<int> editableMenus = data.Select(x => x.id).ToList();
                //Roleid = data.FirstOrDefault().RoleId;
                //roleService.DeleteGlobalPermission(editableMenus);
                List<GlobalSetting> actionlist = new List<GlobalSetting>();
                foreach (var menuallwed in data)
                {
                    GlobalSetting menu = new GlobalSetting();                   
                    menu.id = menuallwed.id;
                    menu.Actionname = menuallwed.Actionname;
                    menu.AllowEmail = menuallwed.AllowEmail;
                    menu.AllowSMS = menuallwed.AllowSMS;
                    menu.Displayname = menuallwed.Displayname;
                    actionlist.Add(menu);
                }
                roleService.AddGlobalPermission(actionlist, CurrentUser.UserID);
                ShowSuccessMessage("Success!", "Menu Permission has been saved", true);
                Cache.RemoveAll();
                return true; // RedirectToAction("Permission", new { id = Roleid });
            }
            catch (Exception ex)
            {
                LogException(ex);
                message = "An internal error found during to process your requested data!";
                ShowErrorMessage("Error!", message, true);
                return false;
            }
            //return View();
        }
    }
}