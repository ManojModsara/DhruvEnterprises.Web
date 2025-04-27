using System.Web;
using System.Web.Optimization;

namespace DhruvEnterprises
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                       "~/Scripts/jquery-{version}.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));


            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/css/bootstrap.css",
                      "~/Content/css/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                     "~/Scripts/jquery.validate.js"));


            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                 "~/scripts/plugins/datatables/jquery.dataTables.js",
                 "~/scripts/plugins/datatables/dataTables.bootstrap.js",
                 "~/Scripts/Common/bootstrap-switch.js"
             ));

            bundles.Add(new ScriptBundle("~/bundles/datepicker").Include(
            "~/Scripts/plugins/datepicker/moment-with-locales.js",
          "~/Scripts/Common/bootstrap-datepicker.js"));

            bundles.Add(new ScriptBundle("~/bundles/select2").Include(
                  "~/Scripts/select2.js"
              ));

            bundles.Add(new StyleBundle("~/bundles/css/select2").Include(
               "~/Content/select2/select2-bootstrap.css", "~/Content/select2/select2.css"
           ));



            bundles.Add(new StyleBundle("~/datatables/css").Include(
                "~/Content/plugins/datatables/dataTables.bootstrap.css",
                "~/Content/css/bootstrap-switch.css"
            ));
        }
    }
}
