using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize(Roles = "Admin")]
    public class WebsitePackagesController : Controller
    {
        private readonly IWebsitePackageRepository _repository;

        public WebsitePackagesController(IWebsitePackageRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Admin: List all website packages
        /// </summary>
        public ActionResult List()
        {
            var model = new WebsitePackageVM();
            model.List = _repository.GetAllPackages();
            return View(model);
        }

        /// <summary>
        /// Admin: Create or edit a package
        /// </summary>
        public ActionResult Edit(int id = 0)
        {
            WebsitePackageVM model;
            if (id > 0)
            {
                model = _repository.GetById(id);
                if (model == null)
                {
                    TempData["Error"] = "Package not found.";
                    return RedirectToAction("List");
                }
            }
            else
            {
                model = new WebsitePackageVM();
            }
            return View(model);
        }

        /// <summary>
        /// Admin: Save package (AJAX)
        /// </summary>
        [HttpPost]
        public ActionResult Save(WebsitePackageVM model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.PlanName))
                {
                    return Json(new { success = false, message = "Plan name is required." }, JsonRequestBehavior.AllowGet);
                }

                int id = _repository.Save(model);
                return Json(new { success = true, id = id }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving package: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Admin: Delete a package (AJAX)
        /// </summary>
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                bool result = _repository.Delete(id);
                return Json(new { success = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Admin: Toggle package active/inactive (AJAX)
        /// </summary>
        [HttpPost]
        public ActionResult ToggleActive(int id)
        {
            try
            {
                bool result = _repository.ToggleActive(id);
                return Json(new { success = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// PUBLIC: JSON API to get active packages for landing page
        /// This endpoint is used by the OurPackages landing page to load dynamic content
        /// </summary>
        [AllowAnonymous]
        [OutputCache(Duration = 60, VaryByParam = "none")]
        public JsonResult GetActivePackages()
        {
            try
            {
                var packages = _repository.GetActivePackages();
                return Json(packages, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
