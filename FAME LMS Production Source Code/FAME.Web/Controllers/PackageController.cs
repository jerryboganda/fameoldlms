using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using First_Aid_Made_Easy.BLL.Interfaces;


namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize(Roles = "Teacher,Admin")]
    public class PackageController : Controller
    {
        private readonly IPackageRepository _packageRepository;
        private readonly ICourseRepository _courseRepository;

        public PackageController(IPackageRepository packageRepository, ICourseRepository courseRepository)
        {
            _packageRepository = packageRepository;
            _courseRepository = courseRepository;
        }

        // GET: Package
        public ActionResult Index(int id = 0)
        {
            LoadViewBags();
            PackageVM model = new PackageVM();
            if (id > 0)
            {
                model = _packageRepository.GetByID(id);
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult Index(PackageVM model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedBy = User.Identity.GetUserId();
                bool f = _packageRepository.Create(model);
                return Json(f, JsonRequestBehavior.AllowGet);
            }
            LoadViewBags();
            return View(model);
        }
        // GET: Package
        public ActionResult DurationIndex(int? PackID = 0, int? id = 0)
        {
            var Model = new PackageDurationVM();
            if (id > 0)
            {
                Model = _packageRepository.GetDuration(id);
            }
            else
            {
                Model.PackageID = PackID;
            }
            return View(Model);
        }
        [HttpPost]
        public ActionResult DurationIndex(PackageDurationVM data)
        {
            var f = _packageRepository.SaveDuration(data);
            return RedirectToAction("Index", new { id = data.PackageID });
        }
        public ActionResult AddInstallments(int id)
        {
            InstallmentVM model = _packageRepository.GetInstallments(id);
            return View(model);
        }
        [HttpPost]
        public ActionResult AddInstallments(InstallmentVM model)
        {
            bool f = _packageRepository.SaveInstallments(model);
            return Json(f, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult CheckInstallments(int id)
        {
            InstallmentVM model = _packageRepository.CheckInstallments(id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public void LoadViewBags()
        {
            string uid = User.Identity.GetUserId();
            List<DAL.tbl_Courses> list = _courseRepository.GetList();
            ViewBag.Courses = list.ConvertAll(x => new SelectListItem
            {
                Value = x.Course_Id.ToString(),
                Text = x.Course_Name,
            });
        }

        public ActionResult List()
        {
            PackageVM model = new PackageVM();
            string id = User.Identity.GetUserId();
            model.List = _packageRepository.GetList();
            return View(model);
        }

        public ActionResult Deactivate(int id)
        {
            var f = _packageRepository.Deactivate(id);
            return Json(id, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult LoadDuration(int id)
        {
            List<PackageDurationVM> list = _packageRepository.getDurations(id);
            var options = list.ConvertAll(x => new
            {
                ID = x.ID,
                Value = x.Duration,
                Text = _packageRepository.getText(x.Duration) + string.Format("{0:n0}", x.Price),
            });
            return Json(options.OrderBy(x => x.Value), JsonRequestBehavior.AllowGet);
        }

    }
}