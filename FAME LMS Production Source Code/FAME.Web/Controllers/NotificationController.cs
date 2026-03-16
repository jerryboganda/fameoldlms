using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize(Roles = "Admin")]
    public class NotificationController : Controller
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IPackageRepository _packageRepository;

        public NotificationController(
            INotificationRepository notificationRepository,
            IPackageRepository packageRepository)
        {
            _notificationRepository = notificationRepository;
            _packageRepository = packageRepository;
        }

        // GET: Notification
        public ActionResult Index(int id = 0)
        {
            StudentNotifVM model = new StudentNotifVM();
            ViewBag.Packages = _packageRepository.GetList();
            if (id > 0)
            {
                model = _notificationRepository.GetByID(id);
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Index(StudentNotifVM model)
        {
            if (ModelState.IsValid)
            {
                model.PicturePath = Common.SavePic(model.ImageFile, "Notif/", null);
                model.CreatedBy = User.Identity.GetUserId();
                bool f = _notificationRepository.Create(model);
                return RedirectToAction("List");
            }
            return View(model);
        }
        public ActionResult List()
        {
            StudentNotifVM model = new StudentNotifVM();
            string id = User.Identity.GetUserId();
            model.List = _notificationRepository.GetList();
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var f = _notificationRepository.Delete(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }

        #region Welcome Message

        // GET: Notification
        public ActionResult MessageIndex()
        {
            StudentNotifVM model = _notificationRepository.GetMessage();
            return View(model);
        }
        [HttpPost]
        public ActionResult MessageIndex(StudentNotifVM model)
        {
            if (ModelState.IsValid)
            {
                model.PicturePath = Common.SavePic(model.ImageFile, "Notif/");
                model.Banner = Common.SavePic(model.BannerFile, "Notif/");
                model.CreatedBy = "Welcome";
                bool f = _notificationRepository.Create(model);
                if (f) TempData["Success"] = "Data Stored";
                return RedirectToAction("MessageIndex");
            }
            return View(model);
        }
        #endregion
    }
}