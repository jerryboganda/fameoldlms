using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Serilog;

namespace First_Aid_Made_Easy.Areas.Ambassador.Controllers
{
    /// <summary>
    /// Ambassador Earnings Controller
    /// Handles earnings display and ledger
    /// </summary>
        [AmbassadorAuthorize(Roles = "Ambassador,Admin")]
    public class EarningsController : Controller
    {
        private readonly IAmbassadorRepository _ambassadorRepo;
        private readonly ICommissionService _commissionService;
        private readonly ILogger _logger;

        public EarningsController(
            IAmbassadorRepository ambassadorRepo,
            ICommissionService commissionService,
            ILogger logger)
        {
            _ambassadorRepo = ambassadorRepo;
            _commissionService = commissionService;
            _logger = logger;
        }

        /// <summary>
        /// Earnings ledger page
        /// GET: /Ambassador/Earnings
        /// </summary>
        public ActionResult Index(int page = 1)
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

                var earnings = _commissionService.GetEarnings(ambassador.Id, null, null, null, page, 50);
                var balance = _commissionService.GetBalance(ambassador.Id);

                var model = new EarningsPageVM
                {
                    Balance = balance,
                    Earnings = earnings,
                    TotalCount = earnings.Count,
                    Page = page,
                    PageSize = 50
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading earnings");
                TempData["Error"] = "An error occurred while loading earnings.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// Get balance summary
        /// GET: /Ambassador/Earnings/Balance
        /// </summary>
        [HttpGet]
        public ActionResult Balance()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null || !ambassador.IsActive)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }

                var balance = _commissionService.GetBalance(ambassador.Id);

                return Json(new { success = true, balance }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting balance");
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get earnings data for AJAX
        /// GET: /Ambassador/Earnings/GetData
        /// </summary>
        [HttpGet]
        public ActionResult GetData(int page = 1, int pageSize = 50)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null || !ambassador.IsActive)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }

                var earnings = _commissionService.GetEarnings(ambassador.Id, null, null, null, page, pageSize);

                return Json(new
                {
                    success = true,
                    data = earnings,
                    total = earnings.Count,
                    page = page,
                    totalPages = (earnings.Count + pageSize - 1) / pageSize
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting earnings data");
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
