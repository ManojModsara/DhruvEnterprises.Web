using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Controllers
{
    public class EmailAPIController : BaseController
    {
        // GET: EmailAPI
        public ActionAllowedDto actionAllowedDto;
        private IUserService adminUserService;
        private IEmailApiService emailApiService;

        ActivityLogDto activityLogModel;
        public EmailAPIController(IUserService _adminUserService, IActivityLogService _activityLogService, IRoleService _roleService, IEmailApiService _emailApiService) : base(_activityLogService, _roleService)
        {
            this.adminUserService = _adminUserService;
            this.emailApiService = _emailApiService;
            this.actionAllowedDto = new ActionAllowedDto();
            this.activityLogModel = new ActivityLogDto();
        }

        // GET: SMSAPI
        public ActionResult Index()
        {
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("SMSAPI", CurrentUser.Roles.FirstOrDefault());
            try
            {
                activityLogModel.ActivityName = "EmailAPI Index REQUEST";
                activityLogModel.ActivityPage = "GET:EmailAPI/Index/";
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
        public ActionResult GetAdminEmailAPIs(DataTableServerSide model)
        {
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("SMSAPI", CurrentUser.Roles.FirstOrDefault());

            int roleid = CurrentUser.Roles.FirstOrDefault();
            int userid = CurrentUser?.UserID ?? 0;
            KeyValuePair<int, List<EmailAPI>> emails = emailApiService.GetEmailApi(model);
            //var userbals = adminUserService.GetUserListWithBalace();
            return Json(new
            {
                model.draw,
                recordsTotal = emails.Key,
                recordsFiltered = emails.Key,
                data = emails.Value.Select(c => new List<object> {
                    c.Id,
                    c.ApiName,
                    c.UserName,
                    c.FromAddress,
                    c.Status,
                     c.portNumber??0,
                   (actionAllowedDto.AllowCreate?  DataTableButton.EditButton(Url.Action( "createedit", "EmailAPI",new { id = c.Id })):string.Empty )
                    +"&nbsp;"+
                   (actionAllowedDto.AllowDelete?  DataTableButton.DeleteButton(Url.Action( "delete","EmailAPI", new { id = c.Id }),"modal-delete-EmailAPI"):string.Empty)
                   , actionAllowedDto.AllowEdit?true:false
                })
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CreateEdit(int? id)
        {
            UpdateActivity("EmailAPI CreateEdit REQUEST", "GET:EmailAPI/CreateEdit/", "ID=" + id);
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("SMSAPI", CurrentUser.RoleId, id.HasValue ? 3 : 2);
            EmailAPIDto emailAPIdto = new EmailAPIDto();
            ViewBag.Range10 = Enumerable.Range(1, 10).Select(x => new { Id = x, Name = x }).ToList();
            if (id.HasValue && id.Value > 0)
            {
                EmailAPI emailAPI = emailApiService.GetEmailById(id.Value);
                emailAPIdto.Id = emailAPI.Id;
                emailAPIdto.ApiName = emailAPI.ApiName;
                emailAPIdto.FromAddress = emailAPI.FromAddress;
                emailAPIdto.UserName = emailAPI.UserName;
                emailAPIdto.Password = emailAPI.Password;
                emailAPIdto.portNumber = emailAPI.portNumber??0;
            }
            return View("createedit", emailAPIdto);
        }

        [HttpPost]
        public ActionResult CreateEdit(EmailAPIDto model, FormCollection FC)
        {
            UpdateActivity("EmailAPI CreateEdit REQUEST", "POST:EmailAPI/CreateEdit/", "ID=" + model.Id);
            ViewBag.actionAllowed = actionAllowedDto = ActionAllowed("SMSAPI", CurrentUser.RoleId, model.Id > 0 ? 3 : 2);

            string message = string.Empty;
            try
            {
                if (ModelState.IsValid)
                {
                    int roleid = CurrentUser.Roles.FirstOrDefault();
                    EmailAPI emailAPI = emailApiService.GetEmailById(model.Id) ?? new EmailAPI();
                    emailAPI.Id = model.Id;
                    emailAPI.ApiName = model.ApiName;
                    emailAPI.FromAddress = model.FromAddress;
                    emailAPI.UserName = model.UserName;
                    emailAPI.Password = model.Password;
                    emailAPI.portNumber = model.portNumber;
                    emailAPI.Status = model.status;
                    emailApiService.Save(emailAPI);
                    ShowSuccessMessage("Success!", "Email Api has been saved", false);
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
                string message = string.Empty;
                try
                {
                    var adminUser = emailApiService.GetEmailById(id);
                    adminUser.Status = !adminUser.Status;
                    //adminUser.Status = (adminUser.Status = true ? "1" : "0");
                    return emailApiService.Save(adminUser).Status == true ? true : false;

                }
                catch (Exception)
                {
                    return false;
                }
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
    }
}