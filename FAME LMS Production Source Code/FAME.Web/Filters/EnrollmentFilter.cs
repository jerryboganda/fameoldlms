using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace First_Aid_Made_Easy.Filters
{
    public class EnrollmentFilter : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            var user = httpContext.User;

            // Only check authenticated Students
            if (!user.Identity.IsAuthenticated) return;
            if (!user.IsInRole("Student")) return;

            // Skip the Account controller (login/logout/register) to avoid redirect loops
            var controller = (filterContext.RouteData.Values["controller"] ?? "").ToString();
            if (controller.Equals("Account", StringComparison.OrdinalIgnoreCase)) return;

            // Skip Area controllers (e.g., Ambassador, Blogs, Certificate) - enrollment check
            // only applies to main Student area controllers
            var area = (filterContext.RouteData.DataTokens["area"] ?? "").ToString();
            if (!string.IsNullOrEmpty(area)) return;

            // Throttle: only check once per 5 minutes via session flag
            var session = httpContext.Session;
            if (session != null)
            {
                var lastCheck = session["__EnrollmentCheckUTC"] as DateTime?;
                if (lastCheck.HasValue && (DateTime.UtcNow - lastCheck.Value).TotalMinutes < 5)
                    return; // Already checked recently
            }

            // Check enrollment
            var userId = user.Identity.GetUserId();
            using (FAMEEntities db = new FAMEEntities())
            {
                var cDate = Common.GetCurrentDate();
                var hasActive = db.tbl_EnrollmentMaster
                    .Any(x => x.StudentFid == userId
                           && x.Enrollment_EndDate >= cDate
                           && x.IsExpired != true);

                if (!hasActive)
                {
                    // Also check book codes
                    var sixMonthsAgo = cDate.AddMonths(-6);
                    hasActive = db.tbl_BookCode
                        .Any(x => x.UsedBy == userId && x.UsedAt > sixMonthsAgo);
                }

                // Update session timestamp
                if (session != null)
                    session["__EnrollmentCheckUTC"] = DateTime.UtcNow;

                if (!hasActive)
                {
                    // Sign out and redirect to login with expired flag
                    var authManager = httpContext.GetOwinContext().Authentication;
                    authManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new
                        {
                            area = "",
                            controller = "Account",
                            action = "Login",
                            subscriptionExpired = "true"
                        }));
                }
            }
        }
    }
}