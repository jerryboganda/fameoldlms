using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System.Web;
using System.Web.Mvc;
using First_Aid_Made_Easy.BLL.Interfaces;


namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize(Roles = "Admin,Teacher")]
    public class ExamController : Controller
    {
        private readonly IExamRepository _examRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IPackageRepository _packageRepository;

        public ExamController(
            IExamRepository examRepository, 
            ICourseRepository courseRepository, 
            IPackageRepository packageRepository)
        {
            _examRepository = examRepository;
            _courseRepository = courseRepository;
            _packageRepository = packageRepository;
        }


        #region Question Exam CRUD

        public ActionResult Delete(int id)
        {
            bool f = _examRepository.DeleteExam(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Index(int? id = 0)
        {
            ExamVM c = new ExamVM();
            if (id > 0)
                c = _examRepository.GetByID(id);

            ViewBag.Courses = _courseRepository.GetList(User.Identity.GetUserId(), User.IsInRole("Admin"));
            ViewBag.Packages = _packageRepository.GetList();
            return View(c);
        }
        [HttpPost]
        public ActionResult Index(ExamVM vid)
        {
            vid.CreatedBy = User.Identity.GetUserId();
            ExamVM id = _examRepository.Save(vid);
            return Json(id, JsonRequestBehavior.AllowGet);
        }
        public ActionResult List()
        {
            ExamVM model = new ExamVM { List = _examRepository.GetList(0, "", "", "") };
            return View(model);
        }
        public ActionResult Filter(int DifficultyLevel, string Tags)
        {
            ExamVM model = new ExamVM();
            model.List = _examRepository.GetList(DifficultyLevel, Tags, "");
            return PartialView("_List", model);
        }

        #endregion

        public ActionResult UploadImage(int ID)
        {
            HttpFileCollectionBase files = Request.Files;
            HttpPostedFileBase file = files[0];
            string FileName = Common.SavePic(file, "Exam/");
            var f = _examRepository.SavePic(FileName, ID);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
    }
}