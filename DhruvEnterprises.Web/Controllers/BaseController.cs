using DhruvEnterprises.Dto;
using DhruvEnterprises.Web.LIBS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DhruvEnterprises.Web.Code.LIBS;
using DataTables.AspNet.Mvc5;
using DataTables.AspNet.Core;
using static DhruvEnterprises.Core.Enums;
using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Models.Secuirity;
using DhruvEnterprises.Web.Code.Serialization;
using DhruvEnterprises.Service;
using SiteKey = DhruvEnterprises.Web.LIBS.SiteKey;
using System.IO;

namespace DhruvEnterprises.Web.Controllers
{
    public class BaseController : Controller
    {
        private IActivityLogService activityLogService;
        private IRoleService roleService;

      //  public Object thisLock = new Object();

        public BaseController(IActivityLogService _activityLogService, IRoleService _roleService)
        {
            this.activityLogService = _activityLogService;
            this.roleService = _roleService;

        }

        #region "Authentication"
        public CustomPrincipal CurrentUser
        {
            get { return HttpContext.User as CustomPrincipal; }
        }

        public void CreateAuthenticationTicket(dynamic user, bool isPersist)
        {
            if (user != null)
            {
                CustomPrincipal principal = new CustomPrincipal(user, (byte)user.RoleId);
                principal.UserID = user.Id;
                principal.Username = user.Username;
                principal.RoleId = user.RoleId;
                var authTicket = new FormsAuthenticationTicket(1,
                    user.Username,
                    DateTime.Now,
                    DateTime.Now.AddDays(1),
                    isPersist,
                    JsonConvert.SerializeObject(principal));

                string encTicket = FormsAuthentication.Encrypt(authTicket);
                HttpCookie faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
                Response.Cookies.Add(faCookie);
            }
        }

        public void RemoveAuthentication()
        {

            FormsAuthentication.SignOut();
        }

        //aId=1=view, 2=create,3=edit,4=delete
        public ActionAllowedDto ActionAllowed(string menuid, int roleid, int aId = 1)
        {
            var menu = roleService.GetMenusByRoleId(roleid).Where(x => x.MenuId == menuid).FirstOrDefault();
            ActionAllowedDto action = new ActionAllowedDto();
            action.AllowView = menu != null ? true : false;
            int mid = menu != null ? menu.Id : 0;
            var RolePermission = roleService.GetRolePermission(roleid);
            var acts = RolePermission.Where(m => m.MenuId == mid).FirstOrDefault();
            action.RoleId = roleid;
            action.AllowCreate = acts != null ? acts.AllowCreate : false;
            action.AllowEdit = acts != null ? acts.AllowUpdate : false;
            action.AllowDelete = acts != null ? acts.AllowDelete : false;
            if (!action.AllowView || (aId == 2 && !action.AllowCreate) || (aId == 3 && !action.AllowEdit) || (aId == 4 && !action.AllowDelete))
                throw new Exception("Access Denied!");
            return action;
        }

        public GlobalSettingAllowedDto GlobalSettingAllowed(string actionName)
        {

            //var menu = roleService.GetMenusByRoleId(roleid).Where(x => x.MenuId == menuid).FirstOrDefault();
            GlobalSettingAllowedDto action = new GlobalSettingAllowedDto();
            // action.AllowView = menu != null ? true : false;
            // int mid = menu != null ? menu.Id : 0;
            var RolePermission = roleService.GetSMSEmailPermission(actionName);
            var acts = RolePermission.Where(m => m.Actionname.ToLower() == actionName).FirstOrDefault();
           
           // var acts = RolePermission.Where(m => m.MenuId == mid).FirstOrDefault();
            action.AllowEmail = acts != null ? acts.AllowEmail : false;
            action.AllowSms = acts != null ? acts.AllowSMS : false;
            //if (!action.AllowView )
            //    throw new Exception("Access Denied!");


            return action;

        }
        #endregion

        #region "Notificatons"

        private void ShowMessages(string title, string message, MessageType messageType, bool isCurrentView)
        {
            Notification model = new Notification
            {
                Heading = title,
                Message = message,
                Type = messageType
            };
            if (isCurrentView)
                this.ViewData.AddOrReplace("NotificationModel", model);
            else
                this.TempData.AddOrReplace("NotificationModel", model);
        }

        protected void ShowErrorMessage(string title, string message, bool isCurrentView = true)
        {
            ShowMessages(title, message, MessageType.Danger, isCurrentView);
        }

        protected void ShowSuccessMessage(string title, string message, bool isCurrentView = true)
        {
            ShowMessages(title, message, MessageType.Success, isCurrentView);
        }

        protected void ShowWarningMessage(string title, string message, bool isCurrentView = true)
        {
            ShowMessages(title, message, MessageType.Warning, isCurrentView);
        }

        protected void ShowInfoMessage(string title, string message, bool isCurrentView = true)
        {
            ShowMessages(title, message, MessageType.Info, isCurrentView);
        }


        #endregion

        #region "HTTP Errors"

        protected ActionResult Redirect404()
        {
            return Redirect("~/error/pagenotfound");
        }

        protected ActionResult Redirect500()
        {
            return Redirect("~/error/servererror");
        }

        protected ActionResult Redirect401()
        {
            return Redirect("~/error/AccessDenied401");
        }

        #endregion

        #region "Serialization"

        public ActionResult NewtonSoftJsonResult(object data)
        {
            return new JsonNetResult
            {
                Data = data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        #endregion

        #region "Exception Handling"

        public PartialViewResult CreateModelStateErrors()
        {
            return PartialView("_ValidationSummary", ModelState.Values.SelectMany(x => x.Errors));
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            //filterContext.Result = RedirectToAction("Index", "Account");


            filterContext.Result = new ViewResult
            {

                ViewName = "~/Views/Account/Index.cshtml"
            };

            string message = "Something went wrong!";

            if (filterContext.Exception.Message.Contains("Access Denied"))
                message = "Access Denied!";

          //  ShowErrorMessage("Error!", message, false);

            base.OnException(filterContext);

            LogException(filterContext.Exception, message);

        }

        public void LogException(Exception ex, string comment = "", string path="")
        {
            try
            {
                
                string filepath = !string.IsNullOrEmpty(path)? path: System.Web.HttpContext.Current.Server.MapPath("~/ExceptionLog/");  //Text File Path

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);

                }
                filepath = filepath + DateTime.Today.ToString("dd-MM-yy") + ".txt";   //Text File Name
                if (!System.IO.File.Exists(filepath))
                {


                    System.IO.File.Create(filepath).Dispose();

                }
                //lock (thisLock)
                //{
                    using (StreamWriter sw = System.IO.File.AppendText(filepath))
                    {
                        sw.WriteLine("================= ***EXCEPTION DETAILS" + " " + DateTime.Now.ToString() + "*** =============");
                        sw.WriteLine("COMMENT:");
                        sw.WriteLine(comment);
                        sw.WriteLine();
                        sw.WriteLine("Error Occured:");
                        sw.WriteLine((SiteKey.DomainName.Contains("localhost") || SiteKey.DomainName.Contains("www.dev.") ? "Test" : "Live"));
                        sw.WriteLine();

                        sw.WriteLine("Date Time:");
                        sw.WriteLine(DateTime.Now.ToString());
                        sw.WriteLine();

                        sw.WriteLine("Error Code:");
                        sw.WriteLine(ex.GetHashCode().ToString());
                        sw.WriteLine();

                        sw.WriteLine("Base Exception:");
                        sw.WriteLine(ex.GetBaseException().ToString());
                        sw.WriteLine();

                        sw.WriteLine("Exception Type:");
                        sw.WriteLine(ex.GetType().ToString());
                        sw.WriteLine();

                        sw.WriteLine("Inner Exception:");
                        sw.WriteLine(ex.InnerException.ToString());
                        sw.WriteLine();

                        sw.WriteLine("Exception Message: ");
                        sw.WriteLine(ex.Message);
                        sw.WriteLine();

                        sw.WriteLine("Exception Source:  ");
                        sw.WriteLine(ex.Source);
                        sw.WriteLine();

                        sw.WriteLine("Stack Trace: ");
                        sw.WriteLine(ex.StackTrace.ToString());
                        sw.WriteLine();

                        sw.WriteLine("Generic Info: ");
                        sw.WriteLine(ex.ToString());
                        sw.WriteLine();


                        sw.WriteLine("=================================== ***End*** =============================================");
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();

                    }
                //}
            }
            catch (Exception e)
            {



            }

        }

        public ActivityLogDto LogActivity(ActivityLogDto model)
        {
            try
            {
                ActivityLog entity = model.Id > 0 ? activityLogService.GetActivityLog(model.Id) : new ActivityLog();
                entity.ActivityName = model.ActivityPage.ToLower().StartsWith("post") ? (model.ActivityName + " SUBMIT") : model.ActivityName;
                entity.ActivityPage = model.ActivityPage;
                entity.IPAddress = GeneralMethods.Fetch_UserIP();
                entity.Remark = model.Remark;

                if (model.UserId != null && model.UserId > 0)
                    entity.UserId = model.UserId;
                entity.Id = model.Id;
                activityLogService.Save(entity);

                model.Id = entity.Id;
            }
            catch (Exception ex)
            {
                LogException(ex);

            }

            return model;

        }

        public void LogActivity(string msgText)
        {
            try
            {


                string filepath = System.Web.HttpContext.Current.Server.MapPath("~/Activity_Log/");  //Text File Path

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);

                }
                filepath = filepath + DateTime.Today.ToString("dd-MM-yy") + ".txt";   //Text File Name
                if (!System.IO.File.Exists(filepath))
                {


                    System.IO.File.Create(filepath).Dispose();

                }
                //lock (thisLock)
                //{
                    using (StreamWriter sw = System.IO.File.AppendText(filepath))
                    {
                        sw.WriteLine("---------LOG DETAILS" + " " + DateTime.Now.ToString() + "--------------");
                        sw.WriteLine("Error Occured:");
                        sw.WriteLine((SiteKey.DomainName.Contains("localhost") || SiteKey.DomainName.Contains("www.dev.") ? "Test" : "Live"));
                        sw.WriteLine();

                        sw.WriteLine(msgText);
                        sw.WriteLine();

                        sw.WriteLine("----------------------------------------------------------------");
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();

                    }
                //}

            }
            catch (Exception e)
            {


            }

        }

        public void LogActivity(string msgText, string path)
        {
            try
            {


                string filepath = path;// System.Web.HttpContext.Current.Server.MapPath("~/Activity_Log/");  //Text File Path

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);

                }
                filepath = filepath + DateTime.Today.ToString("dd-MM-yy") + ".txt";   //Text File Name
                if (!System.IO.File.Exists(filepath))
                {


                    System.IO.File.Create(filepath).Dispose();

                }
                //lock (thisLock)
                //{
                    using (StreamWriter sw = System.IO.File.AppendText(filepath))
                    {
                        sw.WriteLine("---------LOG DETAILS" + " " + DateTime.Now.ToString() + "--------------");
                        sw.WriteLine("Error Occured:");
                        sw.WriteLine((SiteKey.DomainName.Contains("localhost") || SiteKey.DomainName.Contains("www.dev.") ? "Test" : "Live"));
                        sw.WriteLine();

                        sw.WriteLine(msgText);
                        sw.WriteLine();

                        sw.WriteLine("----------------------------------------------------------------");
                        sw.WriteLine();
                        sw.Flush();
                        sw.Close();

                    }
                //}
            }
            catch (Exception e)
            {


            }

        }

        public static void WriteToFile(string str, string path = "", string filename = "")
        {
            try
            {


                string filepath = path;

                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }
                filepath = filepath + filename + ".txt";

                if (!System.IO.File.Exists(filepath))
                {
                    System.IO.File.Create(filepath).Dispose();
                }

                // File.SetAccessControl(filepath,FileShare.ReadWrite;

                //  lock (thisLock)
                //  {
                using (StreamWriter sw = System.IO.File.AppendText(filepath))
                {
                    sw.WriteLine("----------MESSAGE AT: " + DateTime.Now.ToString() + "----------------");
                    sw.WriteLine();
                    sw.WriteLine(str);

                    sw.WriteLine("-----------------------------------------------------");
                    sw.WriteLine();
                    sw.Flush();
                    sw.Close();

                }
                // }


            }
            catch (Exception e)
            {

            }

        }


        #endregion

        #region "DataTables Response"

        public DataTablesJsonResult DataTablesJsonResult(int total, IDataTablesRequest request, IEnumerable<object> data)
        {
            var response = DataTablesResponse.Create(request, total, total, data);
            return new DataTablesJsonResult(response, JsonRequestBehavior.AllowGet);
        }

        public DataTablesJsonResult DataTablesJsonResult(int total, IDataTablesRequest request, IEnumerable<object> data, IDictionary<string, object> additionalParameter = null)
        {
            var response = DataTablesResponse.Create(request, total, total, data, additionalParameter);
            return new DataTablesJsonResult(response, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region "Dispose"

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion

    }
}