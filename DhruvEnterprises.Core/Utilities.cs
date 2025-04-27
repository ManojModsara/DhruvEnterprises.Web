using System;
using System.Web.Mvc;

namespace DhruvEnterprises.Core
{
    public static class Utilities
    {
        public static string IsLinkSelected(this HtmlHelper html, string controller = null, string action = null)
        {
            string cssClass = "active";
            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            string currentController = (string)html.ViewContext.RouteData.Values["controller"];

            string[] strcontroller = String.IsNullOrEmpty(controller) ? null : controller.Split(',');
            if (strcontroller.Length > 1)
            {
                bool isActive = false;
                foreach (var item in strcontroller)
                {
                    if (item.ToString().ToLower() == currentController.ToLower())
                        isActive = true;
                }
                return isActive ? cssClass : String.Empty;
            }

            if (String.IsNullOrEmpty(controller))
                controller = currentController;

            if (String.IsNullOrEmpty(action))
                action = currentAction;

            return controller == currentController && action == currentAction ?
                cssClass : String.Empty;
        }


    }
}