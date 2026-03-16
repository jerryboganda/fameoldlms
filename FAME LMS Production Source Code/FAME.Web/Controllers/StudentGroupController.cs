using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    public class StudentGroupController : Controller
    {
        private readonly IStudentGroupRepository _studentGroupRepository;
        private readonly IDashBoardRepository _dashBoardRepository;

        public StudentGroupController(
            IStudentGroupRepository studentGroupRepository,
            IDashBoardRepository dashBoardRepository)
        {
            _studentGroupRepository = studentGroupRepository;
            _dashBoardRepository = dashBoardRepository;
        }



        public ActionResult Delete(int id)
        {
            bool f = _studentGroupRepository.DeleteStudentGroup(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public ActionResult Index(int? id = 0)
        {
            StudentGroupVM c = new StudentGroupVM();
            if (id > 0)
                c = _studentGroupRepository.GetByID(id);
            return View(c);
        }
        [HttpPost]
        public ActionResult Index(StudentGroupVM vid)
        {
            vid.CreatedBy = User.Identity.GetUserId();
            StudentGroupVM id = _studentGroupRepository.Save(vid);
            return Json(id, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public ActionResult List()
        {
            StudentGroupVM model = new StudentGroupVM { List = _studentGroupRepository.GetList(0) };
            return View(model);
        }
        public ActionResult Filter(int? UniID)
        {
            StudentGroupVM model = new StudentGroupVM();
            model.List = _studentGroupRepository.GetList(UniID ?? 0);
            return PartialView("_List", model);
        }
        public ActionResult FilterStudentList(int Type)
        {
            var model = _dashBoardRepository.GetStudentList("", Type, "", "");
            return Json(model, 0);
        }


        public ActionResult UploadImage(int ID)
        {
            HttpFileCollectionBase files = Request.Files;
            HttpPostedFileBase file = files[0];
            string FileName = Common.SavePic(file, "StudentGroup/");
            var f = _studentGroupRepository.SavePic(FileName, ID);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
    }
}