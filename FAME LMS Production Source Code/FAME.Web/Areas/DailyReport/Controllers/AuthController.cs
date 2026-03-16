using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Areas.DailyReport.Controllers
{
    public class AuthController : Controller
    {
        private readonly IDailyReportAuthRepository _authRepository;

        public AuthController(IDailyReportAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        // GET: Auth/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl = "")
        {
            // Check if user is already logged in
            if (Session["DR_UserId"] != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(DailyReportLoginVM model, string returnUrl = "")
        {
            if (ModelState.IsValid)
            {
                var user = _authRepository.ValidateUser(model.UserName, model.Password);
                if (user != null)
                {
                    // Set session variables
                    Session["DR_UserId"] = user.ID;
                    Session["DR_UserName"] = user.UserName;
                    Session["DR_FullName"] = user.FullName;
                    Session["DR_Role"] = user.Role;

                    // Handle remember me functionality
                    if (model.RememberMe)
                    {
                        Session.Timeout = 43200; // 30 days in minutes
                    }

                    // Redirect to return URL or dashboard
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Dashboard");
                }
                ModelState.AddModelError("", "Invalid username or password");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        // GET: Auth/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            // Check if user is already logged in
            if (Session["DR_UserId"] != null)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(DailyReportRegisterVM model)
        {
            if (ModelState.IsValid)
            {
                if (_authRepository.IsUsernameTaken(model.UserName))
                {
                    ModelState.AddModelError("UserName", "Username is already taken");
                    return View(model);
                }

                if (_authRepository.RegisterUser(model))
                {
                    TempData["Success"] = "Registration successful! Please login with your credentials.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "Registration failed. Please try again.");
                }
            }

            return View(model);
        }

        // GET: Auth/Logout
        public ActionResult Logout()
        {
            // Clear all Daily Report related session variables
            Session.Remove("DR_UserId");
            Session.Remove("DR_UserName");
            Session.Remove("DR_FullName");
            Session.Remove("DR_Role");

            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        // GET: Auth/AccessDenied
        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            return View();
        }

        // Helper method to check if first admin user exists
        [AllowAnonymous]
        public ActionResult CreateFirstAdmin()
        {
            // Only allow this if no users exist
            if (_authRepository.HasAnyUsers())
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: Auth/CreateFirstAdmin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CreateFirstAdmin(DailyReportUserVM model)
        {
            // Only allow this if no users exist
            if (_authRepository.HasAnyUsers())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                model.Role = "Admin";
                if (_authRepository.CreateAdminUser(model))
                {
                    TempData["Success"] = "First admin user created successfully! Please login.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to create admin user. Please try again.");
                }
            }

            return View(model);
        }
    }
}