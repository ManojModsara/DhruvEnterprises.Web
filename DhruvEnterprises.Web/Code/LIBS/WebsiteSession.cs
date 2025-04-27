using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DhruvEnterprises.Web.Code.LIBS
{
    public class WebsiteSession
    {
        public static Dictionary<string, object> SessionProjectStatus
        {
            get
            {
                if (HttpContext.Current.Session["SessionProjectStatus"] == null)
                    HttpContext.Current.Session["SessionProjectStatus"] = new Dictionary<string, object>();

                return (Dictionary<string, object>)HttpContext.Current.Session["SessionProjectStatus"];
            }
        }

        public void GetSessionValue()
        {


        }

    }
}