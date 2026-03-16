using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Areas.Ambassador.Controllers
{
    /// <summary>
    /// Dedicated Ambassador Account Controller
    /// Handles login/logout for the Ambassador Program
    /// </summary>
    public class AmbassadorAccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly IAmbassadorRepository _ambassadorRepo;

        public AmbassadorAccountController() { }

        public AmbassadorAccountController(
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            IAmbassadorRepository ambassadorRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _ambassadorRepo = ambassadorRepo;
        }

        public ApplicationSignInManager SignInManager
        {
            get { return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }
            private set { _signInManager = value; }
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        /// <summary>
        /// Ambassador Login page
        /// GET: /Ambassador/AmbassadorAccount/Login
        /// </summary>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // If already logged in, check if ambassador and redirect
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                if (UserManager.IsInRole(userId, "Admin"))
                {
                    return RedirectToAction("Index", "Admin", new { area = "Ambassador" });
                }

                var ambassador = _ambassadorRepo.GetByUserId(userId);
                if (ambassador != null)
                {
                    return ambassador.IsActive
                        ? RedirectToAction("Index", "Dashboard", new { area = "Ambassador" })
                        : RedirectToAction("Status", "Profile", new { area = "Ambassador" });
                }

                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                TempData["Info"] = "You were signed in with a non-ambassador account. Please log in with your ambassador account to continue.";
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        /// <summary>
        /// Ambassador Login POST
        /// POST: /Ambassador/AmbassadorAccount/Login
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await SignInManager.PasswordSignInAsync(
                model.Email.ToLower(), model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    var user = await UserManager.FindByEmailAsync(model.Email.ToLower());

                    if (UserManager.IsInRole(user.Id, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin", new { area = "Ambassador" });
                    }

                    // Check if email is confirmed
                    if (!await UserManager.IsEmailConfirmedAsync(user.Id))
                    {
                        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        TempData["Error"] = "Please verify your email before logging in.";
                        return View(model);
                    }

                    var ambassador = _ambassadorRepo.GetByUserId(user.Id);
                    if (ambassador != null)
                    {
                        return ambassador.IsActive
                            ? RedirectToAction("Index", "Dashboard", new { area = "Ambassador" })
                            : RedirectToAction("Status", "Profile", new { area = "Ambassador" });
                    }

                    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    TempData["Error"] = "This account is not part of the Ambassador program yet. Please apply first if you want ambassador access.";
                    return View(model);

                case SignInStatus.LockedOut:
                    TempData["Error"] = "Your account has been locked out. Please try again later.";
                    return View(model);

                case SignInStatus.Failure:
                default:
                    TempData["Error"] = "Invalid email or password. Please try again.";
                    return View(model);
            }
        }

        /// <summary>
        /// Ambassador Logout
        /// POST: /Ambassador/AmbassadorAccount/Logout
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null) { _userManager.Dispose(); _userManager = null; }
                if (_signInManager != null) { _signInManager.Dispose(); _signInManager = null; }
            }
            base.Dispose(disposing);
        }
    }
}
