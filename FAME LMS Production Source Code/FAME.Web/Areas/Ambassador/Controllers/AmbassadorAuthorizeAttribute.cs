using First_Aid_Made_Easy.DAL;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace First_Aid_Made_Easy.Areas.Ambassador.Controllers
{
    public class AmbassadorAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var context = filterContext.HttpContext;

            if (context?.User?.Identity == null || !context.User.Identity.IsAuthenticated)
            {
                var returnUrl = context?.Request?.RawUrl ?? "/Ambassador";
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    area = "Ambassador",
                    controller = "AmbassadorAccount",
                    action = "Login",
                    returnUrl
                }));
                return;
            }

            var userId = context.User.Identity.GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                using (var db = new AmbassadorDbContext())
                {
                    var ambassador = db.tbl_Ambassador.FirstOrDefault(a => a.UserId == userId);
                    if (ambassador != null)
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                        {
                            area = "Ambassador",
                            controller = ambassador.Status == "Active" ? "Dashboard" : "Profile",
                            action = ambassador.Status == "Active" ? "Index" : "Status"
                        }));
                        return;
                    }
                }
            }

            if (context.User.IsInRole("Admin"))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    area = "Ambassador",
                    controller = "Admin",
                    action = "Index"
                }));
                return;
            }

            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            {
                area = "Ambassador",
                controller = "Profile",
                action = "Apply"
            }));
        }
    }
}
