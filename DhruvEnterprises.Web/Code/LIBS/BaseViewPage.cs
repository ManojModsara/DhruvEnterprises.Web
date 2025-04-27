using DhruvEnterprises.Models.Secuirity;
using System.Web;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Code.LIBS
{
    public class BaseViewPage : WebViewPage
    {
        public CustomPrincipal CurrentUser
        {
            get { return (CustomPrincipal)HttpContext.Current.User; }
        }

        public override void Execute()
        {
            throw new System.NotImplementedException();
        }
    }

    public class BaseViewPage<T> : WebViewPage<T>
    {
        public CustomPrincipal CurrentUser
        {
            get { return HttpContext.Current.User as CustomPrincipal; }
        }

        public override void Execute()
        {
            throw new System.NotImplementedException();
        }

        public object getHtmlAttributes(bool readonl, string cssClass)
        {
            if (readonl)
            {
                return new { @class = cssClass, @readonly = true };
            }
            return new { @class = cssClass };
        }
    }
}