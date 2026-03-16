using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Google.Rpc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNetCore.Cors;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using First_Aid_Made_Easy.BLL.Interfaces;


namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        private readonly IPackageRepository _packageRepository;
        private readonly IGeneralRepository _generalRepository;
        private readonly IInstallmentRepository _installmentRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IUserRepository _userRepository;

        public AccountController()
        {
        }

        public AccountController(
            ApplicationUserManager userManager, 
            ApplicationSignInManager signInManager,
            IPackageRepository packageRepository,
            IGeneralRepository generalRepository,
            IInstallmentRepository installmentRepository,
            IEnrollmentRepository enrollmentRepository,
            IUserRepository userRepository)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            _packageRepository = packageRepository;
            _generalRepository = generalRepository;
            _installmentRepository = installmentRepository;
            _enrollmentRepository = enrollmentRepository;
            _userRepository = userRepository;
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

        private void LoadViewBags()
        {
            int id = Convert.ToInt32(MasterGroup.Occupation);
            ViewBag.Occupation = _generalRepository.GetList(id).OrderByDescending(x => x.Master_ID).ToList();
            var listP = _packageRepository.GetList();
            ViewBag.Packages = listP.ConvertAll(x => new
            {
                Value = x.PackageID,
                Text = x.PackageName
            });
            ViewBag.Durations = new List<SelectListItem>
                { new SelectListItem() };
        }
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid) { ViewBag.ReturnUrl = returnUrl; return View(); }
            var result = await SignInManager.PasswordSignInAsync(model.Email.ToLower(), model.Password, true, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        var user = await UserManager.FindByEmailAsync(model.Email.ToLower());

                        bool isAdmin = UserManager.IsInRole(user.Id, "Admin");


                        if (!isAdmin)
                        {
                            #region Is App User

                            //if (user.RegisteredFrom != "Web")
                            //{
                            //    ViewBag.Remarks = "You cannot use web please use app";
                            //    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            //    return View("Lockout");
                            //}
                            #endregion

                            #region Is Valid Device
                            var data = await _generalRepository.SaveDevice(user.Id, model.StoredID, isAdmin);
                            TempData["UID"] = data.Item2;
                            #endregion

                            #region Pending Installment

                            if (_installmentRepository.IsPendingInstallment(user.Id))
                            {
                                ViewBag.Remarks = "Your Account Is Blocked Due To Pending Installment, please Contact To Instructor";
                                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                                return View("Lockout");
                            }
                            #endregion

                            #region tbl_UserLockout
                            var Exp = _enrollmentRepository.IsExpired(user.Id);
                            if (Exp != null)
                            {
                                ViewBag.Remarks = Exp.Remarks;
                                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                                return View("Lockout");
                            }
                            #endregion

                            #region Subscription Expired Check
                            if (UserManager.IsInRole(user.Id, "Student"))
                            {
                                if (!_enrollmentRepository.HasActiveSubscription(user.Id))
                                {
                                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                                    return RedirectToAction("Login", new { subscriptionExpired = "true" });
                                }
                            }
                            #endregion

                            #region is Email Confirmed
                            if (!await UserManager.IsEmailConfirmedAsync(user.Id))
                            {
                                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                                TempData["Mail"] = model.Email;
                                TempData["ID"] = user.Id;
                                return View();
                                //TempData["NotConfirmed"] = user.Id;
                            }
                            if (!await UserManager.IsPhoneNumberConfirmedAsync(user.Id))
                            {
                                //TempData["NotConfirmed"] = user.Id;
                            }
                            #endregion

                            #region Should Change Password

                            if (user.ShouldChangePassword)
                            {
                                var code_Time = _enrollmentRepository.GenerateVerificationCode(user.Id, CodeType.Email, true);
                                ViewBag.Reset = true;
                                return View("ForgotPassword", new ResetPasswordViewModel { Code = code_Time.Item1, Email = user.Email });
                            }

                            #endregion
                        }


                        returnUrl = returnUrl == null ? "" : returnUrl == "/" ? "" : returnUrl.Contains("LogOff") ? "" : returnUrl;
                        return RedirectToLocal(returnUrl, user.Id);
                    }
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    ViewBag.Remarks = "Requires Verification";
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Your Email Or Password Is Wrong");
                    return View(model);
            }
        }


        #region AMI Login
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult LoginUni(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> LoginUni(LoginViewModel model, string returnUrl)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email.ToLower());
            var result = await SignInManager.PasswordSignInAsync(model.Email.ToLower(), model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        {
                            var Exp = _enrollmentRepository.IsExpired(user.Id);
                            if (Exp != null)
                            {
                                ViewBag.Remarks = Exp.Remarks;
                                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                                return View("Lockout");
                            }

                            var AmiID = System.Configuration.ConfigurationManager.AppSettings["AmiID"];
                            if (AmiID != model.UniqueID && AmiID != user.Id)
                            {
                                ViewBag.Remarks = "This Device Is Not Allowed";
                                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                                return View("Lockout");
                            }
                            TempData["AmiID"] = AmiID;

                        }
                        return RedirectToLocal(returnUrl, user.Id);
                    }
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    ViewBag.Remarks = "";
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Your Email Or Password Is Wrong");
                    return View(model);
            }
        }

        #endregion

        private bool IsProfileCompleted(string id)
        {
            return _userRepository.GetByID(id) != null;
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register(string refCode = null)
        {
            LoadViewBags();

            var currentReferralCode = ReferralTrackingHelper.GetCurrentReferralCode(Request);
            if (!string.IsNullOrWhiteSpace(refCode) && string.IsNullOrWhiteSpace(currentReferralCode))
            {
                ReferralTrackingHelper.TryStoreReferralCode(refCode, "Code", Request, Response);
                currentReferralCode = refCode;
            }

            return View(new RegisterViewModel
            {
                ReferralCode = currentReferralCode ?? refCode
            });
        }
        [AllowAnonymous]
        public ActionResult RegisterManual()
        {
            LoadViewBags();
            return View(new RegisterViewModel() { ShouldChangeInfo = true, ShouldChangePassword = true });
        }
        //[AllowAnonymous]
        //public ActionResult FreeSubscription() => RedirectToAction(nameof(AmbSubscription), new { t = (int)UserType.AMIFree });
        //[AllowAnonymous]
        //public ActionResult RegisterUni5() => RedirectToAction(nameof(AmbSubscription), new { t = (int)UserType.AMISec });
        //[AllowAnonymous]
        //public ActionResult Jalalabad() => RedirectToAction(nameof(AmbSubscription), new { t = (int)UserType.JLLBD });
        [AllowAnonymous]
        public ActionResult MOCK_TEST_FOR_FCPS_NRE() => RedirectToAction(nameof(AmbSubscription), new { t = (int)UserType.NRETest });
        [AllowAnonymous]
        public ActionResult Free_Nre_2() => RedirectToAction(nameof(AmbSubscription), new { t = (int)UserType.FreeNre2 });
        [AllowAnonymous]
        public ActionResult Free_Nre_1() => RedirectToAction(nameof(AmbSubscription), new { t = (int)UserType.FreeNre1 });
        [AllowAnonymous]
        public ActionResult Mobile_App_Inauguration() => RedirectToAction(nameof(AmbSubscription), new { t = (int)UserType.AppInaug });
        [AllowAnonymous]
        public ActionResult Free_Plab() => RedirectToAction(nameof(AmbSubscription), new { t = (int)UserType.FreePlab });
        [AllowAnonymous]
        public ActionResult FREE_NRE_MADE_EASY_6th() => RedirectToAction(nameof(AmbSubscription), new { t = (int)UserType.FreeCNS });

        [AllowAnonymous]
        public ActionResult AmbSubscription(int t = 0)
        {
            if (t == 0) t = (int)UserType.AMIAmb;

            var packID = WebConfigurationManager.AppSettings["packIDAmb"];
            string duration, durationText;

            switch (t)
            {
                case (int)UserType.AMIAmb:
                    packID = WebConfigurationManager.AppSettings["packID4"];
                    duration = WebConfigurationManager.AppSettings["duration4"];
                    durationText = WebConfigurationManager.AppSettings["durationText4"];
                    break;
                //case (int)UserType.AMISec:
                //    packID = WebConfigurationManager.AppSettings["packID5"];
                //    duration = WebConfigurationManager.AppSettings["duration5"];
                //    durationText = WebConfigurationManager.AppSettings["durationText5"];
                //    break;
                //case (int)UserType.JLLBD:
                //    packID = WebConfigurationManager.AppSettings["packID6"];
                //    duration = WebConfigurationManager.AppSettings["duration6"];
                //    durationText = WebConfigurationManager.AppSettings["durationText6"];
                //    break;
                //case (int)UserType.AMIFree:
                //    packID = WebConfigurationManager.AppSettings["packID"];
                //    duration = WebConfigurationManager.AppSettings["duration"];
                //    durationText = WebConfigurationManager.AppSettings["durationText"];
                //    break;
                case (int)UserType.NRETest:
                    packID = WebConfigurationManager.AppSettings["packID7"];
                    duration = WebConfigurationManager.AppSettings["duration7"];
                    durationText = WebConfigurationManager.AppSettings["durationText7"];
                    break;
                case (int)UserType.AppInaug:
                    packID = WebConfigurationManager.AppSettings["packID8"];
                    duration = WebConfigurationManager.AppSettings["duration8"];
                    durationText = WebConfigurationManager.AppSettings["durationText8"];
                    break;
                case (int)UserType.FreePlab:
                    packID = WebConfigurationManager.AppSettings["packID9"];
                    duration = WebConfigurationManager.AppSettings["duration9"];
                    durationText = WebConfigurationManager.AppSettings["durationText9"];
                    break;
                case (int)UserType.FreeCNS:
                    packID = WebConfigurationManager.AppSettings["packID10"];
                    duration = WebConfigurationManager.AppSettings["duration10"];
                    durationText = WebConfigurationManager.AppSettings["durationText10"];
                    break;
                case (int)UserType.FreeNre2:
                    packID = WebConfigurationManager.AppSettings["packID11"];
                    duration = WebConfigurationManager.AppSettings["duration11"];
                    durationText = WebConfigurationManager.AppSettings["durationText11"];
                    break;
                case (int)UserType.FreeNre1:
                    packID = WebConfigurationManager.AppSettings["packID12"];
                    duration = WebConfigurationManager.AppSettings["duration12"];
                    durationText = WebConfigurationManager.AppSettings["durationText12"];
                    break;
                default:
                    duration = durationText = "";
                    break;
            }

            var a = _packageRepository.GetByID(Convert.ToInt32(packID));
            ViewBag.Packages = new List<SelectListItem> { new SelectListItem() { Value = packID, Text = a?.PackageName ?? "" } };
            ViewBag.Durations = new List<SelectListItem> { new SelectListItem { Text = durationText, Value = duration } };
            ViewBag.UserType = t;

            bool isOtherUserType = t == (int)UserType.NRETest ||
                       t == (int)UserType.FreePlab ||
                       t == (int)UserType.AppInaug ||
                       t == (int)UserType.FreeNre2 ||
                       t == (int)UserType.FreeNre1 ||
                       t == (int)UserType.FreeCNS;

            return View(isOtherUserType ? "RegisterOther" : nameof(RegisterUni), new RegisterViewModel { Duration = Convert.ToInt32(duration) });

        }
        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult RegisterUni()
        {
            ViewBag.UserType = (int)UserType.AMI;
            ViewBag.Packages = new List<SelectListItem> { new SelectListItem() { Value = "27", Text = "ASMI E-Library Subscription Package" } };
            ViewBag.Durations = new List<SelectListItem>();
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            var d = (VerifyCodeVM)Session["VerifyCode"] ?? new VerifyCodeVM();
            var emailVerified = d.Email != null && model.Email != null && d.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase) && d.IsVerified;
            var externalLoginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            model.EmailConfirmed = emailVerified || externalLoginInfo != null;

            if (!model.EmailConfirmed)
            {
                ModelState.AddModelError("Email", "Please verify your email address before completing registration.");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    ShouldChangePassword = model.ShouldChangePassword ?? false,
                    ShouldChangeInfo = model.ShouldChangeInfo ?? false,
                    UserName = model.Email.ToLower(),
                    Email = model.Email.ToLower(),
                    EmailConfirmed = model.EmailConfirmed,
                    RegisteredFrom = "Web",
                };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                ViewBag.Name = model.User_Name;
                ViewBag.Email = model.Email;
                ViewBag.Mobile = model.User_Mobile;
                ViewBag.College = model.Institute;
                ViewBag.City = model.City;
                ViewBag.ID = user.Id;
                model.User_AspUser = user.Id;
                model.StudentID = user.Id;
                model.StudentEmail = model.Email;


                if (result.Succeeded)
                {
                    _ = _userRepository.Save(model);
                    try
                    {
                        model.IsAccepted = Common.Universities.FirstOrDefault(x => x.Value == model.Type)?.AutoAccept ?? false;
                        var resultenr = _enrollmentRepository.AddRequest(model);
                    }
                    catch (Exception ex) { ViewBag.Exception = ex; }
                    var resultr = await UserManager.AddToRoleAsync(user.Id, "Student");
                    var Package = _packageRepository.GetByID(model.PackageID);
                    ViewBag.Package = Package.PackageName;
                    ViewBag.IsConfirmed = user.EmailConfirmed;
                    ViewBag.UserType = model.Type;
                    var Dur = Package.Durations.FirstOrDefault(x => x.Duration == model.Duration);
                    ViewBag.Duration = Dur?.DurationChar + Dur?.Price;
                    if (!user.EmailConfirmed) await SendEmailCode(user.Id);
                    else
                    {
                        try
                        {
                            if (externalLoginInfo != null)
                            {
                                result = await UserManager.AddLoginAsync(user.Id, externalLoginInfo.Login);
                            }
                        }
                        catch { }
                    }

                    // SAFE: Ambassador referral attribution (won't affect registration if fails)
                    try { ReferralTrackingHelper.TryAttributeReferral(user.Id, model.Email, model.User_Name, Request, Response, model.EmailConfirmed); }
                    catch { /* Safely ignored - referral tracking is optional */ }

                    Session.Remove("VerifyCode");

                    return View(nameof(DisplayEmail));
                }
                AddErrors(result);
            }
            LoadViewBags();
            var a = ModelState.Values.Where(x => x.Errors.Count > 0).ToList();
            if (model.Type == 2 || model.Type == 4) return RedirectToAction(nameof(RegisterUni));
            else if (model.Type == 1) return View(model);
            else return RedirectToAction(nameof(AmbSubscription), new { t = model.Type });
        }

        [AllowAnonymous]
        public ActionResult VerifyBookCode(string ID, string BookCode)
        {

            return Json(_generalRepository.UseBookCode(BookCode, ID), JsonRequestBehavior.AllowGet);
        }

        #region Register Teacher

        [Authorize(Roles = "Admin")]
        [AllowAnonymous]
        public ActionResult RegisterTeacher()
        {
            ViewBag.Categories = _userRepository.GetAgentCategories();
            return View(new RegisterViewModel());
        }
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterTeacher(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email.ToLower(), Email = model.Email.ToLower() };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    model.User_AspUser = user.Id;
                    _ = _userRepository.Save(model);
                    var resultr = await UserManager.AddToRoleAsync(user.Id, "Teacher");
                    return RedirectToAction("InstructorsList", "Admin");
                }
                AddErrors(result);
            }
            ViewBag.Categories = _userRepository.GetAgentCategories();
            return View(model);
        }


        #endregion

        #region Register SuppAgent

        [Authorize(Roles = "Admin")]
        public ActionResult RegisterSuppAgent(string id)
        {
            var model = new RegisterViewModel();
            ViewBag.Categories = _userRepository.GetAgentCategories();
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> RegisterSuppAgent(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email.ToLower(), Email = model.Email.ToLower() };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    model.User_AspUser = user.Id;
                    _ = _userRepository.Save(model);
                    var resultr = await UserManager.AddToRoleAsync(user.Id, "SuppAgent");
                    return RedirectToAction("SuppAgentList", "Admin");
                }
                AddErrors(result);
            }
            ViewBag.Categories = _userRepository.GetAgentCategories();
            return View(model);
        }


        #endregion

        [AllowAnonymous]
        [EnableCors]
        public async Task<ActionResult> SendEmail(MessageVM model)
        {
            IdentityMessage message = new IdentityMessage()
            {
                Subject = model.Subject,
                Body = model.Body,
                Destination = model.Destination
            };
            await UserManager.EmailService.SendAsync(message);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public async Task<bool> SendEmailCode(string Id)
        {
            var code_Time = _enrollmentRepository.GenerateVerificationCode(Id, CodeType.Email);
            string code = code_Time.Item1;
            if (code != "")
            {
                var mail = UserManager.FindById(Id).Email;
                IdentityMessage message = new IdentityMessage()
                {
                    Subject = "Verification Code",
                    Body = Common.GetMessage(code),
                    Destination = mail
                };
                await UserManager.EmailService.SendAsync(message);
                return true;
            }
            TempData["RemMinutes"] = code_Time.Item2;
            return false;
        }

        [AllowAnonymous]
        public ActionResult DisplayEmail()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> VerifyEmail(string id)
        {
            var mail = UserManager.FindById(id)?.Email;
            if (!string.IsNullOrEmpty(mail))
            {
                var isSent = await SendEmailCode(id);
                ViewBag.Email = mail;
                ViewBag.isSent = isSent;
            }
            return View();
        }


        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            if (result.Succeeded)
            {
                try { ReferralTrackingHelper.TryMarkReferralVerified(userId); } catch { }
            }
            return result.Succeeded ? RedirectToLocal("/Student/Index") : View("Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View("ForgotPassword");
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            ViewBag.Reset = true;
            if (!ModelState.IsValid)
            {
                return View("ForgotPassword", model);
            }

            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Email Does Not Exist.");
                return View("ForgotPassword", model);
            }
            bool isCodeValid = _enrollmentRepository.ConfirmEmail(model.Email, model.Code);
            if (!isCodeValid)
            {
                ModelState.AddModelError("Email", "Email Does Not Exist.");
                return View("ForgotPassword", model);
            }

            var result = await UserManager.ResetPasswordAsync(user.Id, await UserManager.GeneratePasswordResetTokenAsync(user.Id), model.Password);
            if (result.Succeeded)
            {
                _enrollmentRepository.PasswordChanged(user.Id);
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View("ForgotPassword", model);
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }
        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    {

                        var user = await UserManager.FindByEmailAsync(loginInfo.Email.ToLower());

                        #region Is Valid Device
                        bool isAdmin = UserManager.IsInRole(user.Id, "Admin");
                        var data = await _generalRepository.SaveDevice(user.Id, Request.Cookies["Device"].Values["UID"], isAdmin);
                        //bool IsValidDevice = data.Item1 || isAdmin;

                        //if (!IsValidDevice)
                        //{
                        //    ViewBag.Remarks = data.Item3;
                        //    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        //    return View("Lockout");
                        //}
                        //if (!isAdmin)
                        //    TempData["UID"] = data.Item2;
                        #endregion

                        #region Pending Installment
                        if (_installmentRepository.IsPendingInstallment(user.Id))
                        {
                            ViewBag.Remarks = "Your Account Is Blocked Due To Pending Installment, please Contact To Instructor";
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            return View("Lockout");
                        }
                        #endregion

                        #region tbl_UserLockout
                        var Exp = _enrollmentRepository.IsExpired(user.Id);
                        if (Exp != null)
                        {
                            ViewBag.Remarks = Exp.Remarks;
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            return View("Lockout");
                        }
                        #endregion

                        #region Check Ami And redirect to login Uni
                        var Type = _enrollmentRepository.GetType(user.Id);
                        if (Type == (int)UserType.AMI)
                        {
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            return RedirectToAction("LoginUni");
                        }
                        #endregion

                        #region is Email Confirmed
                        if (!await UserManager.IsEmailConfirmedAsync(user.Id))
                        {
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            TempData["Mail"] = loginInfo.Email;
                            TempData["ID"] = user.Id;
                            return View();
                        }
                        #endregion

                        returnUrl = returnUrl == "/" ? "" : returnUrl;
                        return RedirectToLocal(returnUrl, user.Id);
                    }
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    TempData["LoginInfo"] = loginInfo;
                    return RedirectToAction("Register");
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl, user.Id);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl, string Uid = "")
        {
            if (UserManager.IsInRole(Uid, "Admin") || UserManager.IsInRole(Uid, "Teacher"))
            {
                if (Url.IsLocalUrl(returnUrl) && returnUrl.StartsWith("/Ambassador", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Admin");
            }

            using (var ambassadorDb = new AmbassadorDbContext())
            {
                var ambassador = ambassadorDb.tbl_Ambassador.FirstOrDefault(a => a.UserId == Uid);
                if (ambassador != null)
                {
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.StartsWith("/Ambassador", StringComparison.OrdinalIgnoreCase))
                    {
                        return Redirect(returnUrl);
                    }

                    return ambassador.Status == "Active"
                        ? RedirectToAction("Index", "Dashboard", new { area = "Ambassador" })
                        : RedirectToAction("Status", "Profile", new { area = "Ambassador" });
                }
            }

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (UserManager.IsInRole(Uid, "UniTeacher")) return RedirectToAction("AssistantIndex", "Teacher");
            if (UserManager.IsInRole(Uid, "Assistant")) return RedirectToAction("AssistantIndex", "Teacher");
            if (UserManager.IsInRole(Uid, "SuppAgent")) return RedirectToAction("Index", "SuppAgent");
            return RedirectToAction("Welcome", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
    #region Send Grid

    #endregion
}
