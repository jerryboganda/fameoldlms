using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Web.Routing;

namespace First_Aid_Made_Easy
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Enable attribute routing (for [Route("ref/{code}")] etc.)
            routes.MapMvcAttributeRoutes();

            // Referral link routes: /ref/{code} and /r/{code}
            routes.MapRoute(
                name: "ReferralLink",
                url: "ref/{code}",
                defaults: new { controller = "Referral", action = "Track" },
                new[] { "First_Aid_Made_Easy.Controllers" }
            );

            routes.MapRoute(
                name: "ReferralLinkShort",
                url: "r/{code}",
                defaults: new { controller = "Referral", action = "Track" },
                new[] { "First_Aid_Made_Easy.Controllers" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "First_Aid_Made_Easy.Controllers" }
            );
        }
    }
}
