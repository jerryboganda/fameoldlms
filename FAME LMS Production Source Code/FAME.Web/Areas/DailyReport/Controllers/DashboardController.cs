using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Areas.DailyReport.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDailyReportRepository _reportRepository;
        private readonly IDailyReportAuthRepository _authRepository;

        public DashboardController(IDailyReportRepository reportRepository, IDailyReportAuthRepository authRepository)
        {
            _reportRepository = reportRepository;
            _authRepository = authRepository;
        }

        // Custom authorization check
        private bool IsAuthenticated()
        {
            return Session["DR_UserId"] != null;
        }

        private bool IsAdmin()
        {
            return Session["DR_Role"] != null && Session["DR_Role"].ToString() == "Admin";
        }

        private ActionResult CheckAuthentication()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth", new { area = "DailyReport" });
            }
            return null;
        }

        // GET: Dashboard/Index
        public ActionResult Index()
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            int userId = (int)Session["DR_UserId"];
            bool isAdmin = IsAdmin();

            // Get dashboard data
            var model = new DashboardVM
            {
                TotalReports = isAdmin ? _reportRepository.GetTotalReportsCount() : _reportRepository.GetUserReportsCount(userId),
                RecentReports = isAdmin ? _reportRepository.GetRecentReports(5) : _reportRepository.GetUserRecentReports(userId, 5),
                TotalUsers = isAdmin ? _authRepository.GetAllUsers().Count : 0,
                IsAdmin = isAdmin
            };

            return View(model);
        }

        // GET: Dashboard/AllReports (Admin only)
        public ActionResult AllReports()
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth", new { area = "DailyReport" });
            }

            var reports = _reportRepository.GetAllReports();
            return View(reports);
        }

        // GET: Dashboard/Users (Admin only)
        public ActionResult Users()
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth", new { area = "DailyReport" });
            }

            var users = _authRepository.GetAllUsers();
            return View(users);
        }

        // GET: Dashboard/CreateUser (Admin only)
        public ActionResult CreateUser()
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth", new { area = "DailyReport" });
            }

            var model = new DailyReportUserVM();
            ViewBag.Roles = new SelectList(new[] { "User", "Admin" });
            return View(model);
        }

        // POST: Dashboard/CreateUser (Admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateUser(DailyReportUserVM model)
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth", new { area = "DailyReport" });
            }

            if (ModelState.IsValid)
            {
                if (_authRepository.IsUsernameTaken(model.UserName))
                {
                    ModelState.AddModelError("UserName", "Username is already taken");
                }
                else
                {
                    bool success = model.Role == "Admin" ? 
                        _authRepository.CreateAdminUser(model) : 
                        _authRepository.RegisterUser(new DailyReportRegisterVM 
                        { 
                            FirstName = model.FirstName, 
                            LastName = model.LastName, 
                            UserName = model.UserName, 
                            Password = model.Password 
                        });

                    if (success)
                    {
                        TempData["Success"] = "User created successfully!";
                        return RedirectToAction("Users");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create user. Please try again.");
                    }
                }
            }

            ViewBag.Roles = new SelectList(new[] { "User", "Admin" });
            return View(model);
        }

        // GET: Dashboard/EditUser (Admin only)
        public ActionResult EditUser(int id)
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth", new { area = "DailyReport" });
            }

            var user = _authRepository.GetUserById(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            ViewBag.Roles = new SelectList(new[] { "User", "Admin" }, user.Role);
            return View(user);
        }

        // POST: Dashboard/EditUser (Admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(DailyReportUserVM model)
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            if (!IsAdmin())
            {
                return RedirectToAction("AccessDenied", "Auth", new { area = "DailyReport" });
            }

            if (ModelState.IsValid)
            {
                if (_authRepository.UpdateUser(model))
                {
                    TempData["Success"] = "User updated successfully!";
                    return RedirectToAction("Users");
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update user. Username might be taken.");
                }
            }

            ViewBag.Roles = new SelectList(new[] { "User", "Admin" }, model.Role);
            return View(model);
        }

        // POST: Dashboard/DeleteUser (Admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteUser(int id)
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Access denied" });
            }

            // Don't allow deleting yourself
            if (id == (int)Session["DR_UserId"])
            {
                return Json(new { success = false, message = "You cannot delete your own account" });
            }

            bool success = _authRepository.DeleteUser(id);
            if (success)
            {
                return Json(new { success = true, message = "User deleted successfully" });
            }
            else
            {
                return Json(new { success = false, message = "Cannot delete user. User may have existing reports." });
            }
        }
    }

    // Dashboard View Model
    public class DashboardVM
    {
        public int TotalReports { get; set; }
        public List<DailyReportVM> RecentReports { get; set; }
        public int TotalUsers { get; set; }
        public bool IsAdmin { get; set; }

        public DashboardVM()
        {
            RecentReports = new List<DailyReportVM>();
        }
    }
}