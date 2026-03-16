using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    public class DailyReportController : Controller
    {
        private readonly IDailyReportRepository _repository;

        public DailyReportController(IDailyReportRepository repository)
        {
            _repository = repository;
        }

        // Custom authorization check
        private bool IsAuthenticated()
        {
            return Session["DR_UserId"] != null;
        }

        private ActionResult CheckAuthentication()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth", new { area = "DailyReport" });
            }
            return null;
        }

        public ActionResult Index(int id = 0)
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            DailyReportVM model = new DailyReportVM();
            if (id > 0)
            {
                model = _repository.GetByID(id);
                // Check if user owns this report or is admin
                int currentUserId = (int)Session["DR_UserId"];
                bool isAdmin = Session["DR_Role"]?.ToString() == "Admin";
                
                if (model != null && model.UserFid != currentUserId && !isAdmin)
                {
                    return RedirectToAction("AccessDenied", "Auth", new { area = "DailyReport" });
                }
            }
            return View(model);
        }


        [HttpPost]
        public ActionResult Index(DailyReportVM model)
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            if (ModelState.IsValid)
            {
                model.UserFid = (int)Session["DR_UserId"]; // Get from session
                model.CreatedDT = DateTime.Now;
                if (model.AttachmentFile != null)
                {
                    model.FilePath = Common.SavePic(model.AttachmentFile, "DailyReports/", null);
                }
                bool success = _repository.Create(model);
                if (success)
                {
                    TempData["Success"] = "Report saved successfully";
                    return RedirectToAction("List");
                }
            }

            return View(model);
        }

        public ActionResult List()
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            int userId = (int)Session["DR_UserId"];
            var model = new DailyReportVM { List = new List<DailyReportVM>() };
            return View(model);
        }

        public ActionResult FilterList(DateTime DateFrom, DateTime DateTo)
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            DailyReportVM model = new DailyReportVM();
            int userId = (int)Session["DR_UserId"];
            model.List = _repository.GetList(userId.ToString(), DateFrom, DateTo);
            return PartialView("_List", model);
        }

        public ActionResult Edit(int id)
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return authCheck;

            var model = _repository.GetByID(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            // Check if user owns this report or is admin
            int currentUserId = (int)Session["DR_UserId"];
            bool isAdmin = Session["DR_Role"]?.ToString() == "Admin";
            
            if (model.UserFid != currentUserId && !isAdmin)
            {
                return RedirectToAction("AccessDenied", "Auth", new { area = "DailyReport" });
            }

            return View(model);
        }


        public ActionResult Delete(int id)
        {
            var authCheck = CheckAuthentication();
            if (authCheck != null) return Json(false, JsonRequestBehavior.AllowGet);

            // Check if user owns this report or is admin
            var report = _repository.GetByID(id);
            if (report == null)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            int currentUserId = (int)Session["DR_UserId"];
            bool isAdmin = Session["DR_Role"]?.ToString() == "Admin";
            
            if (report.UserFid != currentUserId && !isAdmin)
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            var success = _repository.Delete(id);
            return Json(success, JsonRequestBehavior.AllowGet);
        }
    }
}