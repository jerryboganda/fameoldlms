using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Serilog;

namespace First_Aid_Made_Easy.Areas.Ambassador.Controllers
{
    /// <summary>
    /// Ambassador Payouts Controller
    /// Handles payout methods and payout requests
    /// </summary>
        [AmbassadorAuthorize(Roles = "Ambassador,Admin")]
    public class PayoutsController : Controller
    {
        private readonly IAmbassadorRepository _ambassadorRepo;
        private readonly IPayoutService _payoutService;
        private readonly ICommissionService _commissionService;
        private readonly ILogger _logger;

        public PayoutsController(
            IAmbassadorRepository ambassadorRepo,
            IPayoutService payoutService,
            ICommissionService commissionService,
            ILogger logger)
        {
            _ambassadorRepo = ambassadorRepo;
            _payoutService = payoutService;
            _commissionService = commissionService;
            _logger = logger;
        }

        /// <summary>
        /// Payouts page - List payout requests
        /// GET: /Ambassador/Payouts
        /// </summary>
        public ActionResult Index(int page = 1)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null && User.IsInRole("Admin"))
                {
                    return RedirectToAction("Payouts", "Admin");
                }

                if (ambassador == null || !ambassador.IsActive)
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                var requests = _payoutService.GetPayoutRequests(ambassador.Id, null, page, 25);
                var balance = _commissionService.GetBalance(ambassador.Id);

                var model = new PayoutsPageVM
                {
                    Balance = balance,
                    Requests = requests,
                    TotalCount = requests.Count,
                    Page = page,
                    PageSize = 25
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading payouts");
                TempData["Error"] = "An error occurred while loading payouts.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// Payout methods management page
        /// GET: /Ambassador/Payouts/Methods
        /// </summary>
        public ActionResult Methods()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null && User.IsInRole("Admin"))
                {
                    return RedirectToAction("Payouts", "Admin");
                }

                if (ambassador == null || !ambassador.IsActive)
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                var methods = _payoutService.GetPayoutMethods(ambassador.Id);

                var model = new PayoutMethodsPageVM
                {
                    Methods = methods,
                    NewMethod = new PayoutMethodVM()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading payout methods");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Add new payout method
        /// POST: /Ambassador/Payouts/AddMethod
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMethod(PayoutMethodVM model)
        {
            try
            {
                // Remove validation for fields not needed for all method types
                if (model.MethodType != "BankTransfer")
                {
                    ModelState.Remove("BankName");
                    ModelState.Remove("BranchCode");
                }

                if (model.MethodType == "MobileMoney")
                {
                    model.AccountNumber = string.IsNullOrWhiteSpace(model.AccountNumber) ? model.PhoneNumber : model.AccountNumber;
                    model.AccountHolderName = string.IsNullOrWhiteSpace(model.AccountHolderName) ? model.MobileAccountName : model.AccountHolderName;
                }

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please correct the errors below.";
                    return RedirectToAction("Methods");
                }

                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null || !ambassador.IsActive)
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                // Check max methods limit
                var existingMethods = _payoutService.GetPayoutMethods(ambassador.Id);
                if (existingMethods.Count >= 5) // Max 5 methods
                {
                    TempData["Error"] = "You have reached the maximum number of payout methods.";
                    return RedirectToAction("Methods");
                }

                var methodId = _payoutService.AddPayoutMethod(ambassador.Id, model);
                if (methodId <= 0)
                {
                    TempData["Error"] = "We could not save this payout method. Please review the details and try again.";
                    return RedirectToAction("Methods");
                }

                TempData["Success"] = "Payout method added successfully.";

                return RedirectToAction("Methods");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding payout method");
                TempData["Error"] = "An error occurred while adding the payout method.";
                return RedirectToAction("Methods");
            }
        }

        /// <summary>
        /// Delete payout method
        /// POST: /Ambassador/Payouts/DeleteMethod
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteMethod(int methodId)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null || !ambassador.IsActive)
                {
                    return Json(new { success = false, message = "Not authorized." });
                }

                // Verify ownership
                if (!_payoutService.ValidateMethodOwnership(methodId, ambassador.Id))
                {
                    return Json(new { success = false, message = "Method not found." });
                }

                var result = _payoutService.DeletePayoutMethod(methodId);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error deleting payout method");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        /// <summary>
        /// Set default payout method
        /// POST: /Ambassador/Payouts/SetDefault
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SetDefault(int methodId)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null || !ambassador.IsActive)
                {
                    return Json(new { success = false, message = "Not authorized." });
                }

                var result = _payoutService.SetDefaultMethod(ambassador.Id, methodId);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error setting default method");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        /// <summary>
        /// Request payout page
        /// GET: /Ambassador/Payouts/Request
        /// </summary>
        public ActionResult Request()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null || !ambassador.IsActive)
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                var balance = _commissionService.GetBalance(ambassador.Id);
                var methods = _payoutService.GetPayoutMethods(ambassador.Id);

                if (!methods.Any())
                {
                    TempData["Warning"] = "Please add a payout method before requesting a payout.";
                    return RedirectToAction("Methods");
                }

                var model = new RequestPayoutVM
                {
                    AvailableBalance = balance.AvailableBalance,
                    MinAmount = _payoutService.GetMinPayoutAmount(),
                    PayoutMethodOptions = methods.Select(m => new SelectListItem
                    {
                        Value = m.Id.ToString(),
                        Text = $"{m.MethodTypeDisplay} - {m.MaskedAccountNumber}" + (m.IsDefault ? " (Default)" : ""),
                        Selected = m.IsDefault
                    }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading request payout page");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Submit payout request
        /// POST: /Ambassador/Payouts/Request
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Request(RequestPayoutVM model)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null || !ambassador.IsActive)
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                // Re-populate for validation errors
                var methods = _payoutService.GetMethods(ambassador.Id);
                model.PayoutMethodOptions = methods.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = $"{m.MethodTypeDisplay} - {m.MaskedAccountNumber}",
                    Selected = m.Id == model.PayoutMethodId
                }).ToList();

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Validate can request
                var (canRequest, message) = _payoutService.CanRequestPayout(ambassador.Id, model.Amount);
                if (!canRequest)
                {
                    ModelState.AddModelError("", message);
                    return View(model);
                }

                // Validate method ownership
                if (!_payoutService.ValidateMethodOwnership(model.PayoutMethodId, ambassador.Id))
                {
                    ModelState.AddModelError("PayoutMethodId", "Invalid payout method.");
                    return View(model);
                }

                var requestId = _payoutService.RequestPayout(ambassador.Id, model.PayoutMethodId, model.Amount);

                TempData["Success"] = "Payout request submitted successfully. We will process it within 3-5 business days.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error submitting payout request");
                TempData["Error"] = "An error occurred while submitting your request.";
                return RedirectToAction("Request");
            }
        }

        /// <summary>
        /// Cancel payout request
        /// POST: /Ambassador/Payouts/Cancel
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int requestId)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null || !ambassador.IsActive)
                {
                    return Json(new { success = false, message = "Not authorized." });
                }

                var result = _payoutService.CancelRequest(requestId, ambassador.Id);

                if (result)
                {
                    return Json(new { success = true, message = "Payout request cancelled." });
                }
                else
                {
                    return Json(new { success = false, message = "Cannot cancel this request." });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error cancelling payout request");
                return Json(new { success = false, message = "An error occurred." });
            }
        }
    }
}
