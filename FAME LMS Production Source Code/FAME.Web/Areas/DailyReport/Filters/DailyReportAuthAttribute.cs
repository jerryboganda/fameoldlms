using System.Web.Mvc;
using System.Web.Routing;

namespace First_Aid_Made_Easy.Areas.DailyReport.Filters
{
    public class DailyReportAuthAttribute : ActionFilterAttribute
    {
        public bool RequireAdmin { get; set; } = false;

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            
            // Check if user is authenticated
            if (session["DR_UserId"] == null)
            {
                // Redirect to login
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    area = "DailyReport",
                    controller = "Auth",
                    action = "Login",
                    returnUrl = filterContext.HttpContext.Request.Url?.PathAndQuery
                }));
                return;
            }

            // Check admin requirement
            if (RequireAdmin)
            {
                var userRole = session["DR_Role"]?.ToString();
                if (userRole != "Admin")
                {
                    // Redirect to access denied
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                    {
                        area = "DailyReport",
                        controller = "Auth",
                        action = "AccessDenied"
                    }));
                    return;
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}