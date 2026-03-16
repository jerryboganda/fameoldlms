using System.Web.Mvc;

namespace First_Aid_Made_Easy.Areas.Landing
{
    public class LandingAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Landing";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Landing_default",
                "Landing/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "First_Aid_Made_Easy.Areas.Landing.Controllers" }
            );
        }
    }
}
