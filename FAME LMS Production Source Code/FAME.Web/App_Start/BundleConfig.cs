using System.Web;
using System.Web.Optimization;

namespace First_Aid_Made_Easy
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // Explicitly disable optimizations to bypass minifier parsing errors with modern JS
            BundleTable.EnableOptimizations = false;

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new StyleBundle("~/bundles/metronic-css").Include(
                      "~/Content/assetsn/plugins/global/plugins.bundle.css",
                      "~/Content/assetsn/css/style.bundle.css",
                      "~/Content/assetsn/css/Custom.css"));

            bundles.Add(new ScriptBundle("~/bundles/metronic-js").Include(
                        "~/Content/assetsn/plugins/global/plugins.bundle.js",
                        "~/Content/assetsn/js/scripts.bundle.js",
                        "~/Content/assetsn/js/custom/widgets.js",
                        "~/Content/assetsn/js/custom/apps/chat/chat.js"));

            bundles.Add(new ScriptBundle("~/bundles/datatables").Include(
                        "~/Content/assetsn/plugins/custom/datatables/datatables.bundle.js"));

            bundles.Add(new StyleBundle("~/bundles/datatables-css").Include(
                        "~/Content/assetsn/plugins/custom/datatables/datatables.bundle.css"));

            bundles.Add(new ScriptBundle("~/bundles/vis-timeline").Include(
                        "~/Content/assetsn/plugins/custom/vis-timeline/vis-timeline.bundle.js"));

            bundles.Add(new StyleBundle("~/bundles/vis-timeline-css").Include(
                        "~/Content/assetsn/plugins/custom/vis-timeline/vis-timeline.bundle.css"));

            // FAME 2026 Student Panel Design System
            bundles.Add(new StyleBundle("~/bundles/student-2026-css").Include(
                      "~/Content/fame-student-2026.css"));




        }
    }
}
