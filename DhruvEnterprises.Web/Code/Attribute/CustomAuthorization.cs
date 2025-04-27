using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DhruvEnterprises.Web.Code.LIBS;
using DhruvEnterprises.Service;
using DhruvEnterprises.Models.Secuirity;
using DhruvEnterprises.Web.LIBS;

namespace DhruvEnterprises.Web.Code.Attributes
{
    public class CustomAuthorization : AuthorizeAttribute
    {
        private byte[] roleTypes;

        #region "Fields"

        #endregion
        public CustomAuthorization()
        {
        }

        public CustomAuthorization(params byte[] roleTypes)
        {
            this.roleTypes = roleTypes;
        }

        protected virtual CustomPrincipal CurrentUser
        {
            get { return HttpContext.Current.User as CustomPrincipal; }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (CurrentUser == null || !CurrentUser.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/account/index/?returnurl=" + filterContext.RequestContext.HttpContext.Request.Url.AbsoluteUri);
            }
            else
            {
                base.OnAuthorization(filterContext);
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.RouteData.DataTokens.ContainsKey("area"))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            {
                controller = "account",
                action = "index",
                AreaRegistration = ""
            }));
            }
            else
                base.HandleUnauthorizedRequest(filterContext);
        }
    }

    public class CustomActionAuthorization : AuthorizeAttribute
    {
        private byte[] roleTypes;
        #region "Fields"

        #endregion
        public CustomActionAuthorization()
        {
        }

        public CustomActionAuthorization(params byte[] roleTypes)
        {
            this.roleTypes = roleTypes;
        }

        protected virtual CustomPrincipal CurrentUser
        {
            get { return HttpContext.Current.User as CustomPrincipal; }
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (CurrentUser != null && CurrentUser.Identity.IsAuthenticated && (CurrentUser.RoleId > 0))
            {
                string url = filterContext.HttpContext.Request.Url.ToString();
                string controllerName = filterContext.RouteData.Values["controller"].ToString();
                string actionName = filterContext.RouteData.Values["action"].ToString();
                int index = url.IndexOf(controllerName);
                string path = (index < 0) ? url : url.Remove(index, url.Length - index);
                url = path + controllerName + "/" + actionName;

                if (!url.ToLower().Contains("accessdenied") && !url.ToLower().Contains("signout") && !url.ToLower().Contains("reportbug.aspx") && !CurrentUser.IsSuperAdmin && !filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    var roleService = DependencyResolver.Current.GetService<IRoleService>();
                    bool isValid = roleService.CheckCurrentMenu(url, CurrentUser.RoleId);
                    if (!isValid)
                    {

                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                        {
                            controller = "error",
                            action = "accessdenied"
                        }));

                    }
                }

            }
            else
            {
                filterContext.Result = new RedirectResult("~/account/index/?returnurl=" + filterContext.RequestContext.HttpContext.Request.Url.AbsoluteUri);
            }
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            {
                controller = "acoount",
                action = "index",
                AreaRegistration = ""
            }));
        }
    }
}