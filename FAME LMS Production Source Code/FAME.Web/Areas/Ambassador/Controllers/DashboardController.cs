using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using System;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Serilog;

namespace First_Aid_Made_Easy.Areas.Ambassador.Controllers
{
    /// <summary>
    /// Ambassador Dashboard Controller
    /// Handles the main ambassador dashboard and statistics
    /// </summary>
        [AmbassadorAuthorize(Roles = "Ambassador,Admin")]
    public class DashboardController : Controller
    {
        private readonly IAmbassadorRepository _ambassadorRepo;
        private readonly ILogger _logger;

        public DashboardController(
            IAmbassadorRepository ambassadorRepo,
            ILogger logger)
        {
            _ambassadorRepo = ambassadorRepo;
            _logger = logger;
        }

        /// <summary>
        /// Ambassador Dashboard - Main view
        /// GET: /Ambassador
        /// </summary>
        public ActionResult Index()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null)
                {
                    if (User.IsInRole("Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }

                    // User is not an ambassador, redirect to application
                    return RedirectToAction("Apply", "Profile");
                }

                if (!ambassador.IsActive)
                {
                    // Ambassador is not active, show status page
                    TempData["AmbassadorStatus"] = ambassador.Status;
                    return RedirectToAction("Status", "Profile");
                }

                var dashboard = _ambassadorRepo.GetDashboard(ambassador.Id);

                // Set full referral URL with domain
                dashboard.Ambassador.FullReferralUrl = GetFullReferralUrl(ambassador.ReferralCode);

                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading ambassador dashboard for user {UserId}", User.Identity.GetUserId());
                TempData["Error"] = "An error occurred while loading your dashboard.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        /// <summary>
        /// Get referral link for copying
        /// POST: /Ambassador/Dashboard/GetReferralLink
        /// </summary>
        [HttpPost]
        public JsonResult GetReferralLink()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null || !ambassador.IsActive)
                {
                    return Json(new { success = false, message = "Ambassador not found or not active." });
                }

                var fullUrl = GetFullReferralUrl(ambassador.ReferralCode);

                return Json(new { success = true, link = fullUrl, code = ambassador.ReferralCode });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting referral link");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        /// <summary>
        /// Refresh dashboard statistics
        /// GET: /Ambassador/Dashboard/RefreshStats
        /// </summary>
        [HttpGet]
        public ActionResult RefreshStats()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null || !ambassador.IsActive)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }

                var stats = _ambassadorRepo.GetStats(ambassador.Id);
                return Json(new { success = true, stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error refreshing stats");
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get chart data for dashboard
        /// GET: /Ambassador/Dashboard/GetChartData
        /// </summary>
        [HttpGet]
        public ActionResult GetChartData(string type, int days = 30)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null || !ambassador.IsActive)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }

                var chartData = _ambassadorRepo.GetChartData(ambassador.Id, type, days);
                return Json(new { success = true, data = chartData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting chart data");
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Private Helpers

        private string GetFullReferralUrl(string referralCode)
        {
            var request = HttpContext.Request;
            var baseUrl = $"{request.Url.Scheme}://{request.Url.Authority}";
            return $"{baseUrl}/ref/{referralCode}";
        }

        #endregion
    }
}
