using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Serilog;

namespace First_Aid_Made_Easy.Areas.Ambassador.Controllers
{
    /// <summary>
    /// Ambassador Referrals Controller
    /// Handles referral tracking and management
    /// </summary>
        [AmbassadorAuthorize(Roles = "Ambassador,Admin")]
    public class ReferralsController : Controller
    {
        private readonly IAmbassadorRepository _ambassadorRepo;
        private readonly IReferralRepository _referralRepo;
        private readonly ILogger _logger;

        public ReferralsController(
            IAmbassadorRepository ambassadorRepo,
            IReferralRepository referralRepo,
            ILogger logger)
        {
            _ambassadorRepo = ambassadorRepo;
            _referralRepo = referralRepo;
            _logger = logger;
        }

        /// <summary>
        /// Referrals list page
        /// GET: /Ambassador/Referrals
        /// </summary>
        public ActionResult Index(string status = null, string source = null, string search = null, int page = 1)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null && User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }

                if (ambassador == null || !ambassador.IsActive)
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                var filter = new ReferralFilterVM
                {
                    Status = status,
                    Source = source,
                    SearchTerm = search,
                    Page = page,
                    PageSize = 25
                };

                var referrals = _referralRepo.GetByAmbassador(ambassador.Id, filter);

                var model = new ReferralListPageVM
                {
                    Referrals = referrals.Items,
                    Filter = filter,
                    TotalCount = referrals.TotalCount
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading referrals");
                TempData["Error"] = "An error occurred while loading referrals.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// Referral details
        /// GET: /Ambassador/Referrals/Details/5
        /// </summary>
        public ActionResult Details(int id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null || !ambassador.IsActive)
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                var referral = _referralRepo.GetById(id);

                if (referral == null || referral.AmbassadorId != ambassador.Id)
                {
                    TempData["Error"] = "Referral not found.";
                    return RedirectToAction("Index");
                }

                return View(referral);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading referral details");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Get referrals data for AJAX requests
        /// GET: /Ambassador/Referrals/GetData
        /// </summary>
        [HttpGet]
        public ActionResult GetData(string status = null, string source = null, string search = null, int page = 1, int pageSize = 25)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null && User.IsInRole("Admin"))
                {
                    return Json(new { success = false, redirectUrl = Url.Action("Index", "Admin") }, JsonRequestBehavior.AllowGet);
                }

                if (ambassador == null || !ambassador.IsActive)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }

                var filter = new ReferralFilterVM
                {
                    Status = status,
                    Source = source,
                    SearchTerm = search,
                    Page = page,
                    PageSize = pageSize
                };

                var referrals = _referralRepo.GetByAmbassador(ambassador.Id, filter);

                return Json(new
                {
                    success = true,
                    data = referrals.Items,
                    total = referrals.TotalCount,
                    page = referrals.Page,
                    totalPages = referrals.TotalPages
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting referrals data");
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get referral statistics
        /// GET: /Ambassador/Referrals/Stats
        /// </summary>
        [HttpGet]
        public ActionResult Stats()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null || !ambassador.IsActive)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }

                var stats = _referralRepo.GetAllStatusCounts(ambassador.Id);

                return Json(new { success = true, stats }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting referral stats");
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
