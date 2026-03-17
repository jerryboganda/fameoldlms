using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Serilog;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.Threading.Tasks;

namespace First_Aid_Made_Easy.Areas.Ambassador.Controllers
{
    /// <summary>
    /// Ambassador Profile Controller
    /// Handles ambassador application, profile management, and status
    /// </summary>
    public class ProfileController : Controller
    {
        private readonly IAmbassadorRepository _ambassadorRepo;
        private readonly IAmbassadorAuditService _auditService;
        private readonly ILogger _logger;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ProfileController(
            IAmbassadorRepository ambassadorRepo,
            IAmbassadorAuditService auditService,
            ILogger logger)
        {
            _ambassadorRepo = ambassadorRepo;
            _auditService = auditService;
            _logger = logger;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        /// <summary>
        /// Ambassador Profile page
        /// GET: /Ambassador/Profile
        /// </summary>
        [AmbassadorAuthorize(Roles = "Ambassador,Admin")]
        public ActionResult Index()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null)
                {
                    return RedirectToAction("Apply");
                }

                var profileVm = new AmbassadorProfileVM
                {
                    Id = ambassador.Id,
                    FullName = ambassador.FullName,
                    Email = ambassador.Email,
                    PhoneNumber = ambassador.PhoneNumber,
                    Institution = ambassador.University,
                    Country = ambassador.Country,
                    Status = ambassador.Status,
                    Tier = ambassador.Tier,
                    ReferralCode = ambassador.ReferralCode,
                    ReferralLink = GetFullReferralUrl(ambassador.ReferralCode),
                    JoinedAt = ambassador.CreatedAt,
                    TotalReferrals = 0,
                    TotalEarnings = 0,
                    ConversionRate = 0,
                    Currency = AmbassadorSettingsHelper.GetDefaultCurrency(),
                    PromotionMethod = null,
                    ExpectedReach = null,
                    WhyAmbassador = ambassador.ApplicationNotes,
                    SocialLinks = new Dictionary<string, string>(),
                    CommissionRates = new System.Collections.Generic.List<CommissionRateDisplay>()
                };

                // Try to get stats
                try
                {
                    var stats = _ambassadorRepo.GetStats(ambassador.Id);
                    if (stats != null)
                    {
                        profileVm.TotalReferrals = stats.TotalReferrals;
                        profileVm.TotalEarnings = stats.TotalEarnings;
                        profileVm.ConversionRate = stats.TotalReferrals > 0
                            ? (decimal)stats.ConvertedReferrals / stats.TotalReferrals * 100
                            : 0;
                    }
                }
                catch { /* Stats not critical */ }

                return View(profileVm);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading ambassador profile");
                TempData["Error"] = "An error occurred while loading your profile.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// Update ambassador profile
        /// POST: /Ambassador/Profile/Update
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AmbassadorAuthorize(Roles = "Ambassador,Admin")]
        public ActionResult Update(AmbassadorApplicationVM model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Please correct the errors below.";
                    return RedirectToAction("Index");
                }

                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null)
                {
                    return RedirectToAction("Apply");
                }

                var result = _ambassadorRepo.UpdateProfile(ambassador.Id, model);

                if (result)
                {
                    TempData["Success"] = "Profile updated successfully.";
                }
                else
                {
                    TempData["Error"] = "Failed to update profile.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating ambassador profile");
                TempData["Error"] = "An error occurred while updating your profile.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Ambassador application page
        /// GET: /Ambassador/Profile/Apply
        /// </summary>
        [AllowAnonymous]
        public ActionResult Apply()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userId = User.Identity.GetUserId();

                    // Check if already an ambassador
                    var existingAmbassador = _ambassadorRepo.GetByUserId(userId);
                    if (existingAmbassador != null)
                    {
                        return RedirectToAction("Status");
                    }

                    if (!UserManager.IsEmailConfirmed(userId))
                    {
                        TempData["Info"] = "Please verify your email before submitting an ambassador application.";
                        return RedirectToAction("VerifyEmail", "Account", new { area = "", id = userId });
                    }

                    // Check if can apply
                    if (!_ambassadorRepo.CanApply(userId))
                    {
                        TempData["Error"] = "You have already applied or are not eligible.";
                        return RedirectToAction("Index", "Home", new { area = "" });
                    }
                }

                return View(new AmbassadorRegistrationVM());
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading apply page");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        /// <summary>
        /// Submit ambassador application
        /// POST: /Ambassador/Profile/Apply
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Apply(AmbassadorRegistrationVM model)
        {
            try
            {
                // For authenticated users, registration fields aren't in the form
                // so clear their required-field validation errors
                if (User.Identity.IsAuthenticated)
                {
                    ModelState.Remove("FullName");
                    ModelState.Remove("Email");
                    ModelState.Remove("Password");
                    ModelState.Remove("ConfirmPassword");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (!model.AcceptTerms)
                {
                    ModelState.AddModelError("AcceptTerms", "You must accept the terms and conditions.");
                    return View(model);
                }

                string userId = User.Identity.GetUserId();

                // If not authenticated, create account first
                if (string.IsNullOrEmpty(userId))
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.Email.ToLower(),
                        Email = model.Email.ToLower(),
                        EmailConfirmed = false,
                        RegisteredFrom = "Web",
                    };

                    var createResult = await UserManager.CreateAsync(user, model.Password);
                    if (!createResult.Succeeded)
                    {
                        foreach (var error in createResult.Errors)
                        {
                            ModelState.AddModelError("", error);
                        }
                        return View(model);
                    }

                    TempData["Info"] = "Your account was created. Please verify your email first, then return to complete your ambassador application.";
                    return RedirectToAction("VerifyEmail", "Account", new { area = "", id = user.Id });
                }

                if (!UserManager.IsEmailConfirmed(userId))
                {
                    TempData["Info"] = "Please verify your email before submitting an ambassador application.";
                    return RedirectToAction("VerifyEmail", "Account", new { area = "", id = userId });
                }

                // Check if can apply
                if (!_ambassadorRepo.CanApply(userId))
                {
                    TempData["Error"] = "Account created, but you have already applied or are not eligible for the ambassador program.";
                    return RedirectToAction("Status");
                }

                var ambassadorId = _ambassadorRepo.Apply(userId, new AmbassadorApplicationVM
                {
                    PhoneNumber = model.PhoneNumber,
                    University = model.University,
                    Country = model.Country,
                    ApplicationNotes = model.ApplicationNotes,
                    AcceptTerms = model.AcceptTerms
                });

                TempData["Success"] = "Account created and application submitted successfully! Our team will review your application soon.";
                return RedirectToAction("Status");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error submitting ambassador application");
                TempData["Error"] = "An error occurred while submitting your application.";
                return View(model);
            }
        }

        /// <summary>
        /// Ambassador status page (for pending/suspended/rejected)
        /// GET: /Ambassador/Profile/Status
        /// </summary>
        public ActionResult Status()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var ambassador = _ambassadorRepo.GetByUserId(userId);

                if (ambassador == null)
                {
                    return RedirectToAction("Apply");
                }

                // If active, redirect to dashboard
                if (ambassador.IsActive)
                {
                    return RedirectToAction("Index", "Dashboard");
                }

                var statusVm = new AmbassadorStatusVM
                {
                    Id = ambassador.Id,
                    Status = ambassador.Status,
                    AppliedAt = ambassador.CreatedAt,
                    RejectionReason = null,
                    SuspensionReason = ambassador.SuspendedReason,
                    TotalReferrals = 0,
                    Currency = AmbassadorSettingsHelper.GetDefaultCurrency(),
                    TotalEarned = 0,
                    PendingBalance = 0
                };

                return View(statusVm);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error loading status page");
                TempData["Error"] = "An error occurred.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        /// <summary>
        /// Ambassador Terms & Conditions
        /// GET: /Ambassador/Profile/Terms
        /// </summary>
        [AllowAnonymous]
        public ActionResult Terms()
        {
            return View();
        }

        private string GetFullReferralUrl(string referralCode)
        {
            var request = HttpContext?.Request;
            var authority = request?.Url?.Authority ?? "localhost:8080";
            var scheme = request?.Url?.Scheme ?? "http";
            return $"{scheme}://{authority}/ref/{referralCode}";
        }
    }
}
