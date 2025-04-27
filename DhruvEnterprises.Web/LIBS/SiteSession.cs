using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DhruvEnterprises.Dto;

namespace DhruvEnterprises.Web.LIBS
{
    public class SiteSession
    {

        public static UserDto SessionUser
        {
            get { return HttpContext.Current.Session["SessionUser"] == null ? null : (UserDto)HttpContext.Current.Session["SessionUser"]; }
            set { HttpContext.Current.Session["SessionUser"] = value; }
        }

        //public static UserLogin SessionUser
        //{
        //    get { return HttpContext.Current.Session["SessionUser"] == null ? null : (UserLogin)HttpContext.Current.Session["SessionUser"]; }
        //    set { HttpContext.Current.Session["SessionUser"] = value; }
        //}

        //public static IntwUser SessionTestUser
        //{
        //    get { return HttpContext.Current.Session["SessionTestUser"] == null ? null : (IntwUser)HttpContext.Current.Session["SessionTestUser"]; }
        //    set { HttpContext.Current.Session["SessionTestUser"] = value; }
        //}
        public static Dictionary<string, object> SessionOthers
        {
            get
            {
                if (HttpContext.Current.Session["SessionOthers"] == null)
                    HttpContext.Current.Session["SessionOthers"] = new Dictionary<string, object>();

                return (Dictionary<string, object>)HttpContext.Current.Session["SessionOthers"];
            }
        }
        //SiteSession.SessionOthers.GetObjectOrDefault<string>("CaptchaText")
    }
}