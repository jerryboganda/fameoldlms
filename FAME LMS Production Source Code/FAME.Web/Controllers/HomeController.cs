using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IGeneralRepository _generalRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IZoomApiRepository _zoomApiRepository;
        private readonly IExamRepository _examRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly ITutorialRepository _tutorialRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IPackageRepository _packageRepository;

        public HomeController(
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            IGeneralRepository generalRepository,
            IEnrollmentRepository enrollmentRepository,
            IZoomApiRepository zoomApiRepository,
            IExamRepository examRepository,
            IQuestionRepository questionRepository,
            ITicketRepository ticketRepository,
            ITutorialRepository tutorialRepository,
            ISectionRepository sectionRepository,
            IVideoRepository videoRepository,
            IPackageRepository packageRepository)
        {
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _generalRepository = generalRepository;
            _enrollmentRepository = enrollmentRepository;
            _zoomApiRepository = zoomApiRepository;
            _examRepository = examRepository;
            _questionRepository = questionRepository;
            _ticketRepository = ticketRepository;
            _tutorialRepository = tutorialRepository;
            _sectionRepository = sectionRepository;
            _videoRepository = videoRepository;
            _packageRepository = packageRepository;
        }

        public ActionResult Welcome()
        {
            string ID = User.Identity.GetUserId();
            ViewBag.LoginCount = _userRepository.GetLoginCount(ID);
            return View();
        }
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View("~/Areas/Landing/Views/Home/Index.cshtml");
            IndexModel model = new IndexModel();
            string ID = User.Identity.GetUserId();
            model.OurCourses = _courseRepository.GetOurCourses();
            ViewBag.Courses = _courseRepository.GetListByStudent(ID).CorList;
            ViewBag.AdPic = _generalRepository.GetMasterByGroup(MasterGroup.DashBoardAd).Master_Value;
            ViewBag.Enrollment = _enrollmentRepository.GetCurrentEnroll(ID);
            ViewBag.LiveClasses = _zoomApiRepository.GetMyMeetings(ID, "Active,UpComing");
            ViewBag.TaskList = _generalRepository.GetReminderList(ID, DateTime.Now);
            ViewBag.Enrolls = _enrollmentRepository.GetInvoiceList("", "", ID, "");
            return View(model);
        }
        public ActionResult Courses()
        {
            CourseVM course = new CourseVM();
            string UserID = User.Identity.GetUserId();
            course.CorList = _courseRepository.GetList(UserID);
            return View(course);
        }
        public ActionResult Exams()
        {
            var model = _examRepository.GetList(0, "");
            return View(model);
        }

        public ActionResult Pricing()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult ContactUs()
        {
            return View();
        }
        public ActionResult Documents()
        {
            return View();
        }
        public ActionResult SupportOverView()
        {
            ViewBag.FaqList = _questionRepository.GetList((QuestionType.FAQ), 0, "", "").Where(x => x.IsPopular == true).ToList();
            ViewBag.TicketList = _ticketRepository.GetList("", "", "", "").Where(x => x.IsPopular == true).ToList();
            ViewBag.TutorialList = _tutorialRepository.GetList().Where(x => x.IsPopular).ToList();
            return View();
        }
        public ActionResult Tutorials()
        {
            ViewBag.TutorialList = _tutorialRepository.GetList();
            return View();
        }
        public ActionResult TutorialPreview(int id)
        {
            var Model = _tutorialRepository.GetByID(id);
            return View(Model);
        }
        public ActionResult AboutUs()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Instructor(string id)
        {
            var user = _userRepository.GetByID(id);
            return View(user);
        }
        public ActionResult Course_Preview(int id)
        {
            CourseVM course = _courseRepository.GetByID(id);
            course.SectionList = _sectionRepository.GetSectionsByCourse(course.Course_Id, null);
            foreach (var vl in course.SectionList)
            {
                vl.VideoList = _videoRepository.GetVideosBySection(vl.Section_ID, null);
            }
            return View(course);
        }
        public ActionResult Package_Preview(int id)
        {
            PackageVM model = _packageRepository.GetByID(id);
            return View(model);
        }
        public ActionResult Packages()
        {
            var model = new PackageVM();
            model.List = _packageRepository.GetList();
            return View(model);
        }
    }
}