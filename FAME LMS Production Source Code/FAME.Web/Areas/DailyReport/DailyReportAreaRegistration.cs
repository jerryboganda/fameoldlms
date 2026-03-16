using System.Web.Mvc;

namespace First_Aid_Made_Easy.Areas.DailyReport
{
    public class DailyReportAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DailyReport";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            // Default route for authentication
            context.MapRoute(
                "DailyReport_Auth",
                "DailyReport/Auth/{action}",
                new { controller = "Auth", action = "Login" },
                new[] { "First_Aid_Made_Easy.Areas.DailyReport.Controllers" }
            );

            // Default route for dashboard
            context.MapRoute(
                "DailyReport_Dashboard",
                "DailyReport/Dashboard/{action}",
                new { controller = "Dashboard", action = "Index" },
                new[] { "First_Aid_Made_Easy.Areas.DailyReport.Controllers" }
            );

            // Default route for reports
            context.MapRoute(
                "DailyReport_Reports",
                "DailyReport/{action}/{id}",
                new { controller = "DailyReport", action = "Index", id = UrlParameter.Optional },
                new[] { "First_Aid_Made_Easy.Controllers" }
            );

            // Catch-all route for the area
            context.MapRoute(
                "DailyReport_default",
                "DailyReport/{controller}/{action}/{id}",
                new { controller = "Dashboard", action = "Index", id = UrlParameter.Optional },
                new[] { "First_Aid_Made_Easy.Areas.DailyReport.Controllers", "First_Aid_Made_Easy.Controllers" }
            );
        }
    }
}