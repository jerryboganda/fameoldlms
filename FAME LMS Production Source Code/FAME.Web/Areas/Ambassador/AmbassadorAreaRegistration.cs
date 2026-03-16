using System.Web.Mvc;

namespace First_Aid_Made_Easy.Areas.Ambassador
{
    public class AmbassadorAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Ambassador";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            // Public Landing Page (no auth required)
            context.MapRoute(
                "Ambassador_Landing",
                "Ambassador",
                new { controller = "Home", action = "Index" },
                namespaces: new[] { "First_Aid_Made_Easy.Areas.Ambassador.Controllers" }
            );

            // Ambassador Dashboard (for authenticated users)
            context.MapRoute(
                "Ambassador_Dashboard",
                "Ambassador/Dashboard",
                new { controller = "Dashboard", action = "Index" },
                namespaces: new[] { "First_Aid_Made_Easy.Areas.Ambassador.Controllers" }
            );

            // Ambassador Area routes
            context.MapRoute(
                "Ambassador_default",
                "Ambassador/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "First_Aid_Made_Easy.Areas.Ambassador.Controllers" }
            );
        }
    }
}
