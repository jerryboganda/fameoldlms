using First_Aid_Made_Easy.BLL;
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
    /// Ambassador Admin Controller
    /// Handles admin management of ambassadors, applications, rules, and payouts
    /// </summary>
        [AmbassadorAuthorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAmbassadorRepository _ambassadorRepo;
        private readonly IReferralRepository _referralRepo;
        private readonly ICommissionService _commissionService;
        private readonly IPayoutService _payoutService;
        private readonly IAmbassadorAuditService _auditService;
        private readonly ILogger _logger;

        public AdminController(
            IAmbassadorRepository ambassadorRepo,
            IReferralRepository referralRepo,
            ICommissionService commissionService,
            IPayoutService payoutService,
            IAmbassadorAuditService auditService,
            ILogger logger)
        {
            _ambassadorRepo = ambassadorRepo;
            _referralRepo = referralRepo;
            _commissionService = commissionService;
            _payoutService = payoutService;
            _auditService = auditService;
            _logger = logger;
        }

        #region Dashboard

        /// <summary>
        /// Admin Ambassador Dashboard
        /// GET: /Ambassador/Admin
        /// </summary>
        public ActionResult Index()
        {
            try
            {
                var dashboard = _ambassadorRepo.GetAdminDashboard();
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading admin dashboard");
                TempData["Error"] = "An error occurred while loading the dashboard.";
                return RedirectToAction("Index", "AdminHome", new { area = "" });
            }
        }

        #endregion

        #region Ambassador Management

        /// <summary>
        /// List all ambassadors
        /// GET: /Ambassador/Admin/Ambassadors
        /// </summary>
        public ActionResult Ambassadors(string status = null, string tier = null, string search = null, string sortBy = null, int page = 1)
        {
            try
            {
                var filter = new AmbassadorFilterVM
                {
                    Status = status,
                    Tier = tier,
                    SearchTerm = search,
                    SortBy = sortBy ?? "CreatedAt",
                    Page = page,
                    PageSize = 25
                };

                var result = _ambassadorRepo.GetList(filter);

                var model = new AdminAmbassadorListVM
                {
                    Ambassadors = result.Items,
                    TotalCount = result.TotalCount,
                    Page = result.Page,
                    PageSize = result.PageSize,
                    SearchTerm = search,
                    StatusFilter = status,
                    TierFilter = tier,
                    SortBy = sortBy
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading ambassadors list");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// View ambassador details
        /// GET: /Ambassador/Admin/ViewAmbassador/5
        /// </summary>
        public ActionResult ViewAmbassador(int id)
        {
            try
            {
                var ambassador = _ambassadorRepo.GetById(id);
                if (ambassador == null)
                {
                    TempData["Error"] = "Ambassador not found.";
                    return RedirectToAction("Ambassadors");
                }

                var dashboard = _ambassadorRepo.GetDashboard(id);
                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading ambassador details");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction("Ambassadors");
            }
        }

        /// <summary>
        /// Pending applications
        /// GET: /Ambassador/Admin/Applications
        /// </summary>
        public ActionResult Applications()
        {
            try
            {
                var applications = _ambassadorRepo.GetPendingApplications();
                var model = new AdminApplicationsVM
                {
                    Applications = applications
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading applications");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Approve ambassador application (AJAX)
        /// POST: /Ambassador/Admin/Approve
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var result = _ambassadorRepo.Approve(id, userId);

                if (result)
                {
                    return Json(new { success = true, message = "Ambassador approved successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to approve ambassador." });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error approving ambassador {Id}", id);
                return Json(new { success = false, message = "An error occurred while approving." });
            }
        }

        /// <summary>
        /// Reject ambassador application (AJAX)
        /// POST: /Ambassador/Admin/Reject
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(int id, string reason)
        {
            try
            {
                if (string.IsNullOrEmpty(reason))
                {
                    return Json(new { success = false, message = "Please provide a reason for rejection." });
                }

                var userId = User.Identity.GetUserId();
                var result = _ambassadorRepo.Reject(id, reason, userId);

                if (result)
                {
                    return Json(new { success = true, message = "Application rejected successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to reject application. It may have already been processed." });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error rejecting application {Id}", id);
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        /// <summary>
        /// Suspend ambassador
        /// POST: /Ambassador/Admin/Suspend
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Suspend(int id, string reason)
        {
            try
            {
                if (string.IsNullOrEmpty(reason))
                {
                    return Json(new { success = false, message = "Please provide a reason." });
                }

                var userId = User.Identity.GetUserId();
                var result = _ambassadorRepo.Suspend(id, reason, userId);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error suspending ambassador");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        /// <summary>
        /// Reactivate ambassador
        /// POST: /Ambassador/Admin/Reactivate
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reactivate(int id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var result = _ambassadorRepo.Reactivate(id, userId);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error reactivating ambassador");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        /// <summary>
        /// Update ambassador tier
        /// POST: /Ambassador/Admin/UpdateTier
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateTier(int id, string tier)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var result = _ambassadorRepo.UpdateTier(id, tier, userId);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating tier");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        #endregion

        #region Commission Rules

        /// <summary>
        /// Commission rules management
        /// GET: /Ambassador/Admin/Rules
        /// </summary>
        public ActionResult Rules()
        {
            try
            {
                var rules = _commissionService.GetRules(activeOnly: false);
                var model = new AdminCommissionRulesVM
                {
                    Rules = rules,
                    Currency = AmbassadorSettingsHelper.GetDefaultCurrency(),
                    MinPayoutAmount = _commissionService.GetMinPayoutAmount(),
                    HoldPeriodDays = _commissionService.GetHoldPeriodDays()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading rules");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Create commission rule
        /// POST: /Ambassador/Admin/CreateRule
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRule(CommissionRuleVM model)
        {
            try
            {
                NormalizeCommissionRuleModel(model);
                model.CreatedByName = User.Identity.GetUserId();
                if (string.IsNullOrWhiteSpace(model.RuleName) || model.CommissionValue <= 0)
                {
                    TempData["Error"] = "Please correct the errors.";
                    return RedirectToAction("Rules");
                }

                var ruleId = _commissionService.CreateRule(model);

                if (ruleId <= 0)
                {
                    TempData["Error"] = "Unable to create the commission rule.";
                    return RedirectToAction("Rules");
                }

                TempData["Success"] = "Commission rule created successfully.";
                return RedirectToAction("Rules");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating rule");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction("Rules");
            }
        }

        /// <summary>
        /// Get a single commission rule (AJAX)
        /// GET: /Ambassador/Admin/GetRule?id=5
        /// </summary>
        public ActionResult GetRule(int id)
        {
            try
            {
                var rules = _commissionService.GetRules(activeOnly: false);
                var rule = rules?.FirstOrDefault(r => r.Id == id);

                if (rule == null)
                {
                    return Json(new { success = false, message = "Rule not found." }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    rule.Id,
                    rule.Name,
                    rule.Description,
                    rule.CommissionType,
                    rule.Rate,
                    rule.FlatAmount,
                    rule.AppliesTo,
                    rule.Tier,
                    rule.IsActive
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting rule");
                return Json(new { success = false, message = "An error occurred." }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Update payout settings (min amount, hold period)
        /// POST: /Ambassador/Admin/UpdatePayoutSettings
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdatePayoutSettings(decimal MinPayoutAmount, int HoldPeriodDays)
        {
            try
            {
                var payoutSaved = AmbassadorSettingsHelper.SetDecimal(AmbassadorSettingsHelper.MinPayoutAmountSetting, MinPayoutAmount);
                var holdSaved = AmbassadorSettingsHelper.SetInt(AmbassadorSettingsHelper.CommissionHoldDaysSetting, HoldPeriodDays);

                return Json(new { success = payoutSaved && holdSaved, message = payoutSaved && holdSaved ? "Payout settings updated." : "Unable to save payout settings." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating payout settings");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        /// <summary>
        /// Update commission rule
        /// POST: /Ambassador/Admin/UpdateRule
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateRule(CommissionRuleVM model)
        {
            try
            {
                NormalizeCommissionRuleModel(model);
                if (string.IsNullOrWhiteSpace(model.RuleName) || model.CommissionValue <= 0)
                {
                    TempData["Error"] = "Please correct the errors.";
                    return RedirectToAction("Rules");
                }

                var result = _commissionService.UpdateRule(model);
                if (!result)
                {
                    TempData["Error"] = "Unable to update the commission rule.";
                    return RedirectToAction("Rules");
                }

                TempData["Success"] = "Commission rule updated successfully.";
                return RedirectToAction("Rules");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating rule");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction("Rules");
            }
        }

        /// <summary>
        /// Toggle rule active status
        /// POST: /Ambassador/Admin/ToggleRule
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleRule(int id, bool activate)
        {
            try
            {
                bool result = _commissionService.ToggleRule(id, activate);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error toggling rule");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        #endregion

        #region Conversions

        /// <summary>
        /// View all conversions
        /// GET: /Ambassador/Admin/Conversions
        /// </summary>
        public ActionResult Conversions(int? ambassadorId = null, string status = null, int page = 1)
        {
            try
            {
                var filter = new ConversionFilterVM
                {
                    AmbassadorId = ambassadorId,
                    Status = status,
                    Page = page,
                    PageSize = 25
                };

                var conversions = _referralRepo.GetAllConversions(filter);

                ViewBag.Filter = filter;
                return View(conversions);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading conversions");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Approve conversion
        /// POST: /Ambassador/Admin/ApproveConversion
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveConversion(int id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var result = _referralRepo.ApproveConversion(id, userId);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error approving conversion");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        /// <summary>
        /// Revoke conversion
        /// POST: /Ambassador/Admin/RevokeConversion
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RevokeConversion(int id, string reason)
        {
            try
            {
                if (string.IsNullOrEmpty(reason))
                {
                    return Json(new { success = false, message = "Please provide a reason." });
                }

                var userId = User.Identity.GetUserId();
                var result = _referralRepo.RevokeConversion(id, reason, userId);

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error revoking conversion");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        #endregion

        #region Payouts

        /// <summary>
        /// Payout requests management
        /// GET: /Ambassador/Admin/Payouts
        /// </summary>
        public ActionResult Payouts(string status = null, string search = null, int page = 1)
        {
            try
            {
                var filter = new PayoutFilterVM
                {
                    Status = status,
                    Page = page,
                    PageSize = 25
                };

                var requests = _payoutService.GetAllRequests(filter);
                var summary = _payoutService.GetPayoutSummary();

                var model = new AdminPayoutsVM
                {
                    Requests = requests.Items,
                    TotalCount = requests.TotalCount,
                    Page = page,
                    PageSize = 25,
                    StatusFilter = status,
                    SearchTerm = search,
                    Currency = AmbassadorSettingsHelper.GetDefaultCurrency(),
                    PendingCount = summary.PendingCount,
                    PendingAmount = summary.PendingAmount,
                    ProcessingCount = summary.ProcessingCount,
                    ProcessingAmount = summary.ProcessingAmount,
                    PaidCount = summary.PaidCount,
                    PaidAmount = summary.PaidAmount,
                    RejectedCount = summary.RejectedCount,
                    RejectedAmount = summary.RejectedAmount
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading payouts");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Process payout (approve, reject, mark paid)
        /// POST: /Ambassador/Admin/ProcessPayout
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessPayout(ProcessPayoutVM model)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                bool result = false;

                switch (model.Action?.ToLower())
                {
                    case "approve":
                        result = _payoutService.ApproveRequest(model.RequestId, userId, model.AdminNotes);
                        break;

                    case "reject":
                        if (string.IsNullOrEmpty(model.RejectionReason))
                        {
                            return Json(new { success = false, message = "Rejection reason is required." });
                        }
                        result = _payoutService.RejectRequest(model.RequestId, userId, model.RejectionReason, model.AdminNotes);
                        break;

                    case "markpaid":
                        if (string.IsNullOrEmpty(model.TransactionRef))
                        {
                            return Json(new { success = false, message = "Transaction reference is required." });
                        }
                        result = _payoutService.MarkAsPaid(model.RequestId, model.TransactionRef, userId, model.AdminNotes);
                        break;

                    default:
                        return Json(new { success = false, message = "Invalid action." });
                }

                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing payout");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        #endregion

        private void NormalizeCommissionRuleModel(CommissionRuleVM model)
        {
            model.RuleName = string.IsNullOrWhiteSpace(Request["Name"]) ? model.RuleName : Request["Name"];
            model.Description = string.IsNullOrWhiteSpace(Request["Description"]) ? model.Description : Request["Description"];

            var rawType = string.IsNullOrWhiteSpace(Request["CommissionType"]) ? model.CommissionType : Request["CommissionType"];
            if (string.Equals(rawType, "FlatAmount", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(rawType, "Fixed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(rawType, "Flat", StringComparison.OrdinalIgnoreCase))
            {
                model.CommissionType = "Fixed";
                if (decimal.TryParse(Request["FlatAmount"], out var flatAmount))
                {
                    model.CommissionValue = flatAmount;
                }
            }
            else
            {
                model.CommissionType = "Percentage";
                if (decimal.TryParse(Request["Rate"], out var rate))
                {
                    model.CommissionValue = rate;
                }
            }

            var rawAppliesTo = string.IsNullOrWhiteSpace(Request["AppliesTo"]) ? model.AppliesTo : Request["AppliesTo"];
            if (string.Equals(rawAppliesTo, "FirstPurchase", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(rawAppliesTo, "FirstPaymentOnly", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(rawAppliesTo, "Subscription", StringComparison.OrdinalIgnoreCase))
            {
                model.AppliesTo = "FirstPaymentOnly";
            }
            else if (string.Equals(rawAppliesTo, "AllPurchases", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(rawAppliesTo, "AllPayments", StringComparison.OrdinalIgnoreCase))
            {
                model.AppliesTo = "AllPayments";
            }
            else
            {
                model.AppliesTo = "Registration";
            }

            model.MinTier = string.IsNullOrWhiteSpace(Request["Tier"]) ? model.MinTier : Request["Tier"];
            model.IsActive = !string.IsNullOrWhiteSpace(Request["IsActive"]);
            model.EffectiveFrom = model.EffectiveFrom == default(DateTime) ? DateTime.UtcNow.Date : model.EffectiveFrom;
            model.Priority = model.Priority <= 0 ? 10 : model.Priority;
            model.Currency = AmbassadorSettingsHelper.GetDefaultCurrency();
        }

        #region Adjustments

        /// <summary>
        /// Add manual adjustment to ambassador earnings
        /// POST: /Ambassador/Admin/AddAdjustment
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAdjustment(int ambassadorId, decimal amount, string type, string description)
        {
            try
            {
                if (string.IsNullOrEmpty(description))
                {
                    return Json(new { success = false, message = "Description is required." });
                }

                var validTypes = new[] { "Bonus", "Adjustment", "Reversal" };
                if (Array.IndexOf(validTypes, type) < 0)
                {
                    return Json(new { success = false, message = "Invalid adjustment type." });
                }

                var userId = User.Identity.GetUserId();
                var earningId = _commissionService.AddAdjustment(ambassadorId, amount, type, description, userId);

                return Json(new { success = earningId > 0, earningId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding adjustment");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        #endregion
    }
}
