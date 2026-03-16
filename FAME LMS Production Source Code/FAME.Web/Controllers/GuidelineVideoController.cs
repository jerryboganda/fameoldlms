using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.Models;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GuidelineVideoController : Controller
    {
        private readonly  GuidelineVideoRepository _repository = new GuidelineVideoRepository();
        private readonly PackageRepository _packageRepository = new PackageRepository();

        public ActionResult Index(int? packageId)
        {
            ViewBag.Packages = _packageRepository.GetList();
            var model = new GuidelineVideoVM
            {
                List = _repository.GetList(packageId)
            };
            ViewBag.SelectedPackageId = packageId;
            return View(model);
        }

        public ActionResult Save(int? id, int? packageId)
        {
            ViewBag.Packages = _packageRepository.GetList();
            GuidelineVideoVM model;

            if (id.HasValue && id.Value > 0)
            {
                model = _repository.GetByID(id.Value);
            }
            else
            {
                model = new GuidelineVideoVM();
                if (packageId.HasValue)
                {
                    model.PackageID = packageId.Value;
                }
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Save(GuidelineVideoVM model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Packages = _packageRepository.GetList();
                return View(model);
            }

            var id = _repository.Save(model);
            return RedirectToAction("Index", new { packageId = model.PackageID });
        }

        public ActionResult Delete(int id)
        {
            var video = _repository.GetByID(id);
            if (video != null)
            {
                _repository.Delete(id);
                return RedirectToAction("Index", new { packageId = video.PackageID });
            }
            return RedirectToAction("Index");
        }
    }
}
