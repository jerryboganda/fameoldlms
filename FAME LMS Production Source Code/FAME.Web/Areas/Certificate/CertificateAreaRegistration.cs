using System.Web.Mvc;

namespace First_Aid_Made_Easy.Areas.Certificate
{
    public class CertificateAreaRegistration : AreaRegistration
    {
        public override string AreaName => "Certificate";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Certificate_default",
                "Certificate/{controller}/{action}/{id}",
                new { controller = "Admin", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "First_Aid_Made_Easy.Areas.Certificate.Controllers" }
            );
        }
    }
}
