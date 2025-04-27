using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Controllers
{
    public class SiteController : Controller
    {
        // GET: Site
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }
        public ActionResult Services()
        {
            return View();
        }
        public ActionResult Faq()
        {
            return View();
        }
        public ActionResult ContactUs()
        {
            return View();
        }

    }
}