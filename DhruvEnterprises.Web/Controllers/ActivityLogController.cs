using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using DhruvEnterprises.Web.Code.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Controllers
{
   
    public class ActivityLogController : BaseController
    {
        #region "Fields"

        private readonly IActivityLogService activityLogService;
        private readonly IRoleService roleService;
        private readonly IUserService userService;


        ActivityLogDto activityLogModel;
        public ActionAllowedDto actionAllowedDto;
        #endregion

        #region "Constructor"
        public ActivityLogController(IRoleService _userroleService, IActivityLogService _activityLogService, IUserService _userService) : base(_activityLogService, _userroleService)
        { 
            this.activityLogService = _activityLogService;
            this.roleService = _userroleService;
            this.userService = _userService;

            this.activityLogModel = new ActivityLogDto();
            this.actionAllowedDto = new ActionAllowedDto();
        }
        #endregion

        // GET: ActivityLog
      
        public ActionResult Index(int? y,  long? u, string a="", string f = "", string e = "", string n = "", string p = "", string r = "")
        {
            UpdateActivity("ActivityLog Report REQUEST", "Get:ActivityLog/Index", string.Empty);

            ActiVityLogFilterDto filter = new ActiVityLogFilterDto();
            filter.isshow = Convert.ToInt32(y.HasValue ? 1 : 0);
            filter.userid = Convert.ToInt32(u.HasValue ? u : 0);
            filter.ipaddress = a;
            filter.actname = n;
            filter.url = p;
            filter.remark = r;
            filter.sdate = f;
            filter.sdateNow = !string.IsNullOrEmpty(filter.sdate) ? filter.sdate : DateTime.Now.ToString("dd/MM/yyy");
            filter.edate = e;
            filter.edateNow = !string.IsNullOrEmpty(filter.edate) ? filter.edate  : DateTime.Now.ToString("dd/MM/yyy");

            ViewBag.FilterData = TempData["ActiVityLogFilterDto"] = filter;

            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? filter.userid : CurrentUser.UserID;
            
            ViewBag.UserList = userService.GetUserList().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.UserProfile?.FullName ?? string.Empty, Selected = (x.Id == u) }).ToList();

            return View();
        }

        [HttpPost]
        public ActionResult GetActivityLogReport(DataTableServerSide model)
        {
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("ActivityLog", CurrentUser.Roles.FirstOrDefault());

            ActiVityLogFilterDto filter = TempData["ActiVityLogFilterDto"] != null ? (ActiVityLogFilterDto)TempData["ActiVityLogFilterDto"] : new ActiVityLogFilterDto();


            int userrole = CurrentUser.Roles.FirstOrDefault();
            bool IsAdminRole = (userrole != 3) ? true : false;
            int uid = IsAdminRole ? filter.userid : CurrentUser.UserID;

           
            filter.userid = IsAdminRole ? filter.userid : uid;
            ViewBag.FilterData = TempData["ActiVityLogFilterDto"] = filter;

            KeyValuePair<int, List<ActivityLog>> requestResponses = activityLogService.GetActivityLogs(model, filter);
            
            return Json(new
            {
                draw = model.draw,
                recordsTotal = requestResponses.Key,
                recordsFiltered = requestResponses.Key,
                data = requestResponses.Value.Select(c => new List<object> {
                    c.Id,
                    c.User?.UserProfile?.FullName??string.Empty,
                    c.ActivityName,
                    (c.ActivityDate).ToString(),
                    c.IPAddress,
                    c.ActivityPage,
                    c.Remark 
                    })
            }, JsonRequestBehavior.AllowGet);

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