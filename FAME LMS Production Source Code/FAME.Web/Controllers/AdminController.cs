using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using First_Aid_Made_Easy.BLL.Interfaces;


namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize]
    public class AdminController : Controller
    {
        private readonly IDashBoardRepository _dashBoardRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IGeneralRepository _generalRepository;
        private readonly IUserRepository _userRepository;

        public AdminController(
            IDashBoardRepository dashBoardRepository,
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            IPackageRepository packageRepository,
            IGeneralRepository generalRepository,
            IUserRepository userRepository)
        {
            _dashBoardRepository = dashBoardRepository;
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _packageRepository = packageRepository;
            _generalRepository = generalRepository;
            _userRepository = userRepository;
        }

        // GET: Teacher
        public ActionResult Index()
        {
            return View(_dashBoardRepository.Get());
        }

        private void LoadCourses()
        {
            List<DAL.tbl_Courses> list = _courseRepository.GetList();
            ViewBag.Courses = list.ConvertAll(x => new
            {
                Value = x.Course_Id,
                Text = x.Course_Name
            });
            ViewBag.Students = _enrollmentRepository.GetStudentsList();
            ViewBag.Packages = _packageRepository.GetList().ConvertAll(x => new
            {
                Value = x.PackageID,
                Text = x.PackageName
            }).OrderBy(x => x.Text);
        }
        public ActionResult EnrollmentList()
        {
            LoadCourses();
            ViewBag.Teachers = _dashBoardRepository.GetUserList((int)Roles.Teacher);
            ViewBag.Package = true;
            return View();
        }
        public ActionResult FilterEnrollmentList(SubscriptionReport m)
        {
            ViewBag.Param = m;
            var model = _dashBoardRepository.GetEnrollmentList(m.DateFrom, m.DateTo, m.Sid, m.CourseID, m.PackageID, m.IsExpire, m.AddedBy, m.ApprovedBy, m.City, m.Institute);
            if (m.IsPDF)
                //return View("EnrollmentListPDF", model);
                return new ViewAsPdf("EnrollmentListPDF", model);
            else if (m.IsExcel)
            {
                string htmlContent = Common.ViewToString(this.ControllerContext, "EnrollmentListPDF", model);
                Common.ExportExcel(htmlContent, "EnrollmentListReport");
                return null;
            }
            else
                return PartialView("_EnrollmentList", model);
        }
        public ActionResult StudentList(int Type = 1)
        {
            StudentVM model = new StudentVM();
            ViewBag.Type = Type;
            model.List = new List<DAL.sp_lstStudents_Result>();
            return View(model);
        }

        public ActionResult PartiallyRegistered()
        {
            ViewBag.List = _dashBoardRepository.GetPartiallyRegistered();
            return View();
        }
        public async Task<ActionResult> StudentDetail(int ID)
        {
            var Model = await _dashBoardRepository.GetStudentDetailAsync(ID);
            return View(Model);
        }
        public ActionResult FilterStudentList(string DateFrom, string DateTo, string text, int Type = 1)
        {
            StudentVM model = new StudentVM();
            model.List = _dashBoardRepository.GetStudentList(text, Type, DateFrom, DateTo);
            return PartialView("_StudentList", model);
        }
        public ActionResult AllStudentList()
        {
            StudentVM model = new StudentVM();
            model.List = new List<DAL.sp_lstStudents_Result>();
            return View(model);
        }

        public ActionResult FilterAllStudentList(string DateFrom, string DateTo, string text)
        {
            StudentVM model = new StudentVM();
            model.List = _dashBoardRepository.GetStudentList(text, 0, DateFrom, DateTo);
            return PartialView("_AllStudentList", model);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult InstructorsList()
        {
            var model = _dashBoardRepository.GetUserList((int)Roles.Teacher);
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult SuppAgentList()
        {
            ViewBag.Categories = _userRepository.GetAgentCategories();
            var model = _dashBoardRepository.GetUserList((int)Roles.SuppAgent);
            return View(model);
        }

        public ActionResult FilterUsers(int RoleID, int CategoryID, string Email)
        {
            var model = _dashBoardRepository.GetUserList(RoleID, CategoryID, Email);
            return PartialView("_SuppAgentList", model);
        }
        public ActionResult SaveNotes(EditStudentDetail m)
        {
            var model = _generalRepository.SaveNotes(m);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AgentStatus(string ID, bool Status)
        {
            var model = _dashBoardRepository.AgentStatus(ID, Status);
            return Json(model, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult SaveRoles(string UserID, int?[] RoleIds)
        {
            var a = _userRepository.SaveRoles(UserID, RoleIds);
            TempData["Message"] = a ? "Roles Saved successfully" : "Something Went wrong";
            return RedirectToAction(nameof(ManageRoles));
        }

        public ActionResult ManageRoles()
        {
            ViewBag.UserList = _dashBoardRepository.GetUserList(0);
            return View();
        }
    }
}