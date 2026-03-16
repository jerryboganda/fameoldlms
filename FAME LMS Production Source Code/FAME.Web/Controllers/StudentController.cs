using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using First_Aid_Made_Easy.BLL.Interfaces;


namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize]
    public class StudentController : Controller
    {
        private readonly IVideoRepository _videoRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IGeneralRepository _generalRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IZoomApiRepository _zoomApiRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IDashBoardRepository _dashBoardRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IUserRepository _userRepository;

        public StudentController(
            IVideoRepository videoRepository,
            ICourseRepository courseRepository,
            IGeneralRepository generalRepository,
            IEnrollmentRepository enrollmentRepository,
            IZoomApiRepository zoomApiRepository,
            IPackageRepository packageRepository,
            IDashBoardRepository dashBoardRepository,
            INotificationRepository notificationRepository,
            ITicketRepository ticketRepository,
            IQuestionRepository questionRepository,
            IUserRepository userRepository)
        {
            _videoRepository = videoRepository;
            _courseRepository = courseRepository;
            _generalRepository = generalRepository;
            _enrollmentRepository = enrollmentRepository;
            _zoomApiRepository = zoomApiRepository;
            _packageRepository = packageRepository;
            _dashBoardRepository = dashBoardRepository;
            _notificationRepository = notificationRepository;
            _ticketRepository = ticketRepository;
            _questionRepository = questionRepository;
            _userRepository = userRepository;
        }

        #region Courses
        [HttpGet]
        [AllowAnonymous]
        public ActionResult GetVideoPath(int VideoID, int SecID)
        {
            var Video = _videoRepository.GetVideo(VideoID, SecID, User.Identity.GetUserId());
            //QuestionRepository repo = new QuestionRepository();
            //var Faqs = repo.GetList(Video.Video_Id);
            //var Comments = repo.GetComments(VideoID: Video.Video_Id, isApproved: true);
            //sp_GetReview_Result Reviews = repo.GetReview(Video.Video_Id, ContentType.Video.ToString(), User.Identity.GetUserId());
            return Json(new { Video/*, Faqs, Comments, Reviews*/ }, JsonRequestBehavior.AllowGet);
        }
        //======Get Section For Take LEcture ======= 
        public ActionResult TakeCourse(int id)
        {
            string Uid = User.Identity.GetUserId();
            var user = Common.GetUserNameByASpUserID(Uid);
            ViewBag.PhoneNo = user.User_Mobile;
            ViewBag.Name = user.User_Name;
            CourseVM course = _courseRepository.GetByID(id, Uid);
            return View(course);
        }
        public ActionResult DisplaySections(int id)
        {
            string Uid = User.Identity.GetUserId();
            CourseVM course = _courseRepository.GetByID(id, Uid);
            return View(course);
        }
        public ActionResult isWatched(int VideoID, bool? IsCompleted)
        {
            bool f = _videoRepository.isWatched(VideoID, User.Identity.GetUserId(), IsCompleted);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Index()
        {
            string ID = User.Identity.GetUserId();
            
            // Build the StudentDashboardVM for the new 2026 dashboard
            var model = new StudentDashboardVM();
            
            // Get user info
            model.User = _userRepository.GetByID(ID);
            
            // Get current enrollment
            model.CurrentEnrollment = _enrollmentRepository.GetCurrentEnroll(ID);
            
            // Get upcoming live classes
            model.UpcomingClasses = _zoomApiRepository.GetMyMeetings(ID, "Active,UpComing");
            
            // Get recent courses
            var courses = _courseRepository.GetListByStudent(ID);
            model.RecentCourses = courses?.CorList?.Take(5).ToList();
            
            // Legacy ViewBag data for backward compatibility
            ViewBag.Courses = courses?.CorList;
            ViewBag.AdPic = _generalRepository.GetMasterByGroup(MasterGroup.DashBoardAd)?.Master_Value;
            ViewBag.Enrollment = model.CurrentEnrollment;
            ViewBag.LiveClasses = model.UpcomingClasses;
            ViewBag.TaskList = _generalRepository.GetReminderList(ID, DateTime.Now);

            var Packages = _packageRepository.GetList();
            if (model.CurrentEnrollment != null)
            {
                Packages.Remove(Packages.FirstOrDefault(x => x.PackageID == model.CurrentEnrollment.PackageID));
            }
            ViewBag.PackList = Packages;
            ViewBag.Enrolls = (await _dashBoardRepository.GetStudentDetailAsync(0, ID)).Subscrption?.PackList;
            
            return View(model);
        }

        public ActionResult MyCourses()
        {
            MyCoursesVM course = _courseRepository.GetListByStudent(User.Identity.GetUserId());
            return View(course);
        }
        public async Task<ActionResult> MySubscriptions()
        {
            SubscriptionsVM sbs = new SubscriptionsVM();
            sbs = (await _dashBoardRepository.GetStudentDetailAsync(0, User.Identity.GetUserId())).Subscrption;
            return View(sbs);
        }
        public ActionResult MyProgress()
        {
            return View(_dashBoardRepository.StudentProgress(User.Identity.GetUserId()));
        }
        public ActionResult GuidelineVideos()
        {
            var repository = new GuidelineVideoRepository();
            var model = new GuidelineVideoVM
            {
                List = repository.GetStudentVideos(User.Identity.GetUserId())
            };
            return View(model);
        }
        public ActionResult StudySchedule()
        {
            try
            {
                var repository = new ExamAttemptRepository();
                var schedules = repository.GetStudentSchedules(User.Identity.GetUserId());
                ViewBag.GroupedSchedules = schedules.GroupBy(s => new { s.PackageName, s.ExamAttemptID, s.AttemptName });
                return View(new StudyScheduleVM { List = schedules });
            }
            catch (System.Exception ex)
            {
                // Log error and show empty schedule page with error message
                System.Diagnostics.Debug.WriteLine("StudySchedule Error: " + ex.Message);
                ViewBag.GroupedSchedules = new List<IGrouping<object, StudyScheduleVM>>();
                ViewBag.ErrorMessage = "Unable to load study schedules. Please contact support if this issue persists.";
                return View(new StudyScheduleVM { List = new List<StudyScheduleVM>() });
            }
        }

        public ActionResult ChangeExamAttempt(int enrollmentId)
        {
            using (var db = new DAL.FAMEEntities())
            {
                var enrollment = db.tbl_EnrollmentMaster.Find(enrollmentId);
                if (enrollment == null || enrollment.StudentFid != User.Identity.GetUserId())
                    return HttpNotFound();

                var attempts = new ExamAttemptRepository().GetActiveAttemptsByPackage(enrollment.PackageId ?? 0);
                ViewBag.Attempts = attempts;
                ViewBag.EnrollmentId = enrollmentId;
                ViewBag.CurrentAttemptId = enrollment.ExamAttemptID;
                ViewBag.PackageName = db.tbl_Package.Find(enrollment.PackageId)?.PackageName;
                return View();
            }
        }
        [HttpPost]
        public ActionResult ChangeExamAttempt(int enrollmentId, int examAttemptId)
        {
            using (var db = new DAL.FAMEEntities())
            {
                var enrollment = db.tbl_EnrollmentMaster.Find(enrollmentId);
                if (enrollment == null || enrollment.StudentFid != User.Identity.GetUserId())
                    return HttpNotFound();

                enrollment.ExamAttemptID = examAttemptId;
                db.SaveChanges();
                TempData["Success"] = "Exam attempt updated successfully.";
                return RedirectToAction("StudySchedule");
            }
        }
        public ActionResult LoadWaterMark()
        {
            string Uid = User.Identity.GetUserId();
            var user = Common.GetUserNameByASpUserID(Uid);
            return Json(new
            {
                Email = User.Identity.GetUserName(),
                PhoneNo = user.User_Mobile,
                Name = user.User_Name,
                CNIC = user.CNIC,
            },
                JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult LoadWaterMarkSecure()
        {
            string Uid = User.Identity.GetUserId();
            var user = Common.GetUserNameByASpUserID(Uid);
            return Json(new
            {
                Email = User.Identity.GetUserName(),
                PhoneNo = user.User_Mobile,
                Name = user.User_Name,
                CNIC = user.CNIC,
            },
                JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Testing
        //public ActionResult TakeCourseNew(int id)
        //{
        //    string Uid = User.Identity.GetUserId();
        //    var user = Common.GetUserNameByASpUserID(Uid);
        //    ViewBag.PhoneNo = user.User_Mobile;
        //    ViewBag.Name = user.User_Name;
        //    SectionVM s = new SectionRepository().GetByIDForTakeLec(id, Uid);
        //    s.VideoList = new VideoRepository().GetVideosBySection(id, Uid);
        //    return View(s);
        //}

        #endregion

        #region Notifications 
        public ActionResult CheckForNotification()
        {
            string id = User.Identity.GetUserId();
            var a = _notificationRepository.CheckForNotification(id);
            return Json(a, JsonRequestBehavior.AllowGet);
        }
        public ActionResult HaveSeen(int id)
        {
            string Sid = User.Identity.GetUserId();
            var f = _notificationRepository.HaveSeen(Sid, id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult Notifications(string side)
        {
            ViewBag.Side = side;
            string id = User.Identity.GetUserId();
            var model = _notificationRepository.GetNotifications(id);
            return PartialView("_Notifications", model);
        }
        public PartialViewResult Messages()
        {
            string id = User.Identity.GetUserId();
            var model = _notificationRepository.GetNotifications(id);
            return PartialView("_Messages", model);
        }
        #endregion

        #region Tickets

        public ActionResult TicketList()
        {
            ViewBag.Categories = _userRepository.GetAgentCategories();
            TicketVM model = new TicketVM { List = _ticketRepository.GetList("", "", "", User.Identity.GetUserId()) };
            return View(model);
        }
        public ActionResult Ticket()
        {
            ViewBag.Categories = _userRepository.GetAgentCategories();
            return View();
        }

        public ActionResult TicketReply(int id)
        {
            return View(_ticketRepository.GetByID(id, User.Identity.GetUserId()));
        }

        [HttpPost]
        public ActionResult SaveReply(TicketReplyVM model)
        {
            model.SendBy = User.Identity.GetUserId();
            _ticketRepository.SaveReply(model);
            return RedirectToAction("TicketReply", new { id = model.Ticket_ID });
        }
        [HttpPost]
        public ActionResult Save(TicketVM model)
        {
            model.CreatedBy = User.Identity.GetUserId();
            var id = _ticketRepository.Save(model);
            return Json(id, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region FAQ

        public ActionResult Faqs()
        {
            return View(_questionRepository.GetList((QuestionType.FAQ), 0, "", ""));
        }
        #endregion

        #region Exam Engine
        public ActionResult MockTests()
        {
            return View();
        }

        public ActionResult StartExam(int id)
        {
            ViewBag.ExamID = id;
            return View();
        }

        public ActionResult TakeExam(int id)
        {
            ViewBag.ExamID = id;
            return View();
        }

        public ActionResult ExamResult(int id)
        {
            ViewBag.ExamID = id;
            return View();
        }
        #endregion

        #region Meetings

        public ActionResult MyMeetings()
        {
            var List = _zoomApiRepository.GetMyMeetings(User.Identity.GetUserId());
            return View(List);

        }
        public ActionResult WatchVideo(int ID)
        {
            var Id = _zoomApiRepository.GetMeetingVideoId(ID);
            var Vid = _zoomApiRepository.GetVideo(Id);
            return View(Vid);
        }
        #endregion
    }
}