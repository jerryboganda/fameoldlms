using System.Web.Mvc;

namespace First_Aid_Made_Easy.Areas.Landing
{
    public class BlogsAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Blogs";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Blogs_default",
                "Blogs/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "First_Aid_Made_Easy.Areas.Blogs.Controllers" }
            );
        }
    }
}
