using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
//using DhruvEnterprises.Web.UserControl;
using DhruvEnterprises.Dto;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using System.Web.Mvc;



namespace DhruvEnterprises.Web.LIBS
{
    public partial class BasePage : System.Web.UI.Page
    {
        protected void Page_PreInit(object sender, EventArgs e)
        {
            if (SiteSession.SessionUser == null)
            {
                HttpCookie cookie = Request.Cookies["ShaktiUserSessionCookies"];
                if (cookie == null)
                {
                    //Response.Redirect(ResolveUrl("~/default.aspx?returnurl=" + Request.RawUrl), true);
                    Response.Redirect(SiteKey.DomainName);
                }
                else
                {
                    var userInfo = cookie.Value;
                    JavaScriptSerializer json_serializer = new JavaScriptSerializer();
                    UserDto myDeserializedObj = (UserDto)JsonConvert.DeserializeObject(userInfo, typeof(UserDto));
                    SiteSession.SessionUser = myDeserializedObj;
                }
            }
            else
            {
                //string str = HttpContext.Current.Request.Url.AbsolutePath.ToString().Split('/').ToList().FirstOrDefault(S => S.ToString().ToLower().Contains(".aspx")).ToLower().ToString();
                string[] CurrentURL = HttpContext.Current.Request.Url.AbsolutePath.ToLower().Trim('/').Split('/');
                Uri uri = new Uri(Request.Url.ToString());
                string path = String.Format("{0}{1}{2}{3}", uri.Scheme, Uri.SchemeDelimiter, uri.Authority, uri.AbsolutePath);

                //var permissions = SiteSession.SessionUser.Role.MenuAccesses.Where(x => x.FrontMenu.PageName.ToLower() == CurrentURL[2].ToLower()).Select(P => P.FrontMenu.PageName.ToLower()).FirstOrDefault();
                //if (permissions == null)
                //{

                //}

                //var a = SiteSession.SessionUser.AllPagesAccess.Any(x => CurrentURL.Contains(x));

                //var childPages = SiteSession.SessionUser.Role.MenuAccesses.Where(x => x.FrontMenu.PageName.ToLower() == a.ToString().ToLower()).
                //    Select(P => P.FrontMenu.ChildPages.ToLower()).FirstOrDefault();

                //if (childPages != null)
                //{ 
                //if (!SiteSession.SessionUser.AllPagesAccess.Any(P => CurrentURL.Contains(P)) && CurrentURL.LastOrDefault().ToLower() == "default.aspx" || CurrentURL.LastOrDefault().ToLower() == "accessdenied.aspx")
                //{
                //    Response.Redirect("~/user/accessdenied.aspx");
                //}

                //}


                //// if (!SiteSession.SessionUser.IsSuperAdmin && !(CurrentURL.LastOrDefault() == "accessdenied.aspx" || CurrentURL.LastOrDefault().ToLower() == "default.aspx" || CurrentURL.LastOrDefault().ToLower() == "reportbug.aspx"))

                if (!SiteSession.SessionUser.IsSuperAdmin && 
                    !(uri.AbsolutePath.ToLower().Contains("user/accessdenied.aspx") ||
                         uri.AbsolutePath.ToLower().Contains("user/default.aspx") ||
                         uri.AbsolutePath.ToLower().Contains("user/reportbug.aspx")))
                {

                    //var menuService = DependencyResolver.Current.GetService<IMenuService>();
                    //bool isValid = menuService.CheckCurrentMenu(path, SiteSession.SessionUser.RoleId);
                    //if (!isValid)
                    //{
                    //    Response.Redirect("~/user/accessdenied.aspx");
                    //}

                }
                //if (!SiteSession.SessionUser.IsSuperAdmin )
                //{
                //    List<string> pageList = new List<string>();
                //    var pages = FrontMenu.GetMenusByRoleId(SiteSession.SessionUser.RoleId).Select(P => P.FrontMenu.PageName.ToLower() + (!String.IsNullOrEmpty(P.FrontMenu.ChildPages) ? ("," + P.FrontMenu.ChildPages) : "")).ToList();
                //    if (pages.Count > 0)
                //    {
                //        foreach (var page in pages)
                //        {
                //            string[] str = page.Trim().Split(',');
                //            if (str.Length > 1)
                //            {
                //                foreach (string value in str)
                //                {
                //                    pageList.Add(value);
                //                }
                //            }
                //            else
                //            {
                //                pageList.Add(page);
                //            }
                //        }
                //    }
                //    if (!pageList.Any(P => path.ToLower().EndsWith(P))
                //        && !(CurrentURL.LastOrDefault() == "accessdenied.aspx" || CurrentURL.LastOrDefault().ToLower() == "default.aspx" || CurrentURL.LastOrDefault().ToLower() == "reportbug.aspx"))
                //    {
                //        Response.Redirect("~/user/accessdenied.aspx");
                //    }
                //}
            }
        }
    }
}