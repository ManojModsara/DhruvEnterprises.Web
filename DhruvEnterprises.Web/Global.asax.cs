using DhruvEnterprises.Web.Code.LIBS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using FluentValidation.Mvc;
using DhruvEnterprises.Validation;
using DhruvEnterprises.Models.Secuirity;
using System.Web.Http;

namespace DhruvEnterprises
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ValidationConfiguration();

            //HttpConfiguration config = GlobalConfiguration.Configuration;
            //config.Formatters.JsonFormatter
            //            .SerializerSettings
            //            .ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = this.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                try
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                    var serializeModel = JsonConvert.DeserializeObject<CustomPrincipal>(authTicket.UserData);
                    CustomPrincipal newUser = new CustomPrincipal(authTicket.Name);
                    newUser.UserID = serializeModel.UserID;
                    newUser.Roles = serializeModel.Roles;
                    newUser.RoleId = serializeModel.RoleId ;

                    HttpContext.Current.User = newUser;
                }
                catch (CryptographicException)
                {
                    FormsAuthentication.SignOut();
                }
            }
        }

        private void ValidationConfiguration()
        {
            FluentValidationModelValidatorProvider.Configure(provider =>
            {
                provider.ValidatorFactory = new ValidatorFactory();
            });
        }

        protected void Application_Error()
        {
            //var ex = Server.GetLastError();
            
        }

    }
}
