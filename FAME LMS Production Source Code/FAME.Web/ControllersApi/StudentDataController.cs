using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace First_Aid_Made_Easy.Controllers
{
    [JzJwtAuthorize]
    public class StudentDataV2Controller : ApiBaseController
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IDashBoardRepository _dashBoardRepository;
        private readonly ITutorialRepository _tutorialRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IReportRepository _reportRepository;
        private readonly IGeneralRepository _generalRepository;

        public StudentDataV2Controller(
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            ISectionRepository sectionRepository,
            IVideoRepository videoRepository,
            IQuestionRepository questionRepository,
            IDashBoardRepository dashBoardRepository,
            ITutorialRepository tutorialRepository,
            ITicketRepository ticketRepository,
            INotificationRepository notificationRepository,
            IReportRepository reportRepository,
            IGeneralRepository generalRepository)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _sectionRepository = sectionRepository;
            _videoRepository = videoRepository;
            _questionRepository = questionRepository;
            _dashBoardRepository = dashBoardRepository;
            _tutorialRepository = tutorialRepository;
            _ticketRepository = ticketRepository;
            _notificationRepository = notificationRepository;
            _reportRepository = reportRepository;
            _generalRepository = generalRepository;
        }

        #region Subscription Data
        [HttpGet]
        public IHttpActionResult IsRequestAccepted()
        {
            var isAccepted = Common.IsRequestAccepted(JzUserId);
            return Ok(new { success = true, Data = isAccepted, message = isAccepted ? "Accepted" : "Not Accepted" });
        }
        [HttpGet]
        public IHttpActionResult MyCourses()
        {
            MyCoursesVM MyCourses = _courseRepository.GetListByStudent(JzUserId);
            return Ok(new
            {
                success = true,
                data = new
                {
                    Enrollment = _enrollmentRepository.GetCurrentEnroll(JzUserId),
                    Courses = MyCourses.CorList,
                    MyCourses.PackageList,
                    MyCourses.SectionList,
                }
            });

        }

        [HttpGet]
        public IHttpActionResult GetSections(int CourseID)
        {
            CourseVM course = _courseRepository.GetByID(CourseID, JzUserId);
            return Ok(new
            {
                success = true,
                data = course
            });
        }

        [HttpGet]
        public IHttpActionResult GetVideos(int SectionID)
        {
            var user = Common.GetUserNameByASpUserID(JzUserId);
            if (user != null)
            {

                SectionVM s = _sectionRepository.GetByIDForTakeLec(SectionID, JzUserId);
                s.VideoList = _videoRepository.GetVideosBySection(SectionID, JzUserId);
                s.QuestionList = _questionRepository.GetQuestionsBySection(SectionID);
                return Ok(new
                {
                    success = true,
                    data = s
                });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }

        [HttpGet]
        public IHttpActionResult GetCourse(string id, int CourseID)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                CourseVM course = _courseRepository.GetByID(CourseID, id);
                return Ok(new
                {
                    success = true,
                    data = course
                });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }

        [HttpGet]
        public async Task<IHttpActionResult> MySubscriptions(string id)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {

                var sbs = await _dashBoardRepository.GetPackSubscriptions(id);
                return Ok(new
                {
                    success = true,
                    data = sbs.ConvertAll(x => new
                    {
                        x.ID,
                        Name = x.CourseName,
                        x.Status,
                        x.EnrollmentMethod,
                        x.DEnrollmentDate,
                        x.DExpiryDate,
                        x.RemainingDays,
                    })
                });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }

        [HttpGet]
        public IHttpActionResult isWatched(string id, int VideoID, bool? IsCompleted)
        {
            bool f = _videoRepository.isWatched(VideoID, id, IsCompleted);
            return Ok(new { success = true, data = f });
        }
        #endregion

        #region Ticket
        [HttpGet]
        public IHttpActionResult GetCategories()
        {
            var data = _generalRepository.GetList((int)MasterGroup.AgentCategory);
            return Ok(new
            {
                success = true,
                data = data.Select(x => new
                {
                    x.Master_ID,
                    x.Master_Name,
                    x.Master_Value,
                    x.Master_Group,
                })
            });
        }
        public IHttpActionResult GetTutorials()
        {
            var data = _tutorialRepository.GetList();
            return Ok(new
            {
                success = true,
                data
            });
        }

        [HttpGet]
        public IHttpActionResult GetFaqs()
        {

            var data = _questionRepository.GetList((QuestionType.FAQ), 0, "", "");
            return Ok(new
            {
                success = true,
                data = data.ConvertAll(x => new
                {
                    x.IsPopular,
                    x.Question_Id,
                    x.Category,
                    x.CategoryID,
                    x.Question,
                    x.Answer,
                })
            });
        }

        [HttpGet]
        public IHttpActionResult TicketsList(string id)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                var Tickets = _ticketRepository.GetList("", "", "", id);
                return Ok(new
                {
                    success = true,
                    data = Tickets.ConvertAll(x => new
                    {
                        x.Ticket_ID,
                        x.CreatedDT,
                        x.Subject,
                        x.Body,
                        x.Status,
                    })
                });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }
        [HttpGet]
        public IHttpActionResult GetTicketDetails(string id, int Ticket_ID)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                var Tickets = _ticketRepository.GetByID(Ticket_ID, id);
                return Ok(new
                {
                    success = true,
                    data = Tickets
                });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }
        [HttpPost]
        public IHttpActionResult OpenNewTicket(TicketVM model)
        {
            var user = Common.GetUserNameByASpUserID(model.CreatedBy);
            if (user != null)
            {
                var id = _ticketRepository.Save(model);

                if (id == 0)
                {
                    return Ok(new { success = false, message = "No Agent Available for this category" });
                }
                return Ok(new
                {
                    success = true,
                    data = id
                });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }
        [HttpPost]
        public IHttpActionResult ReplyToTicket(TicketReplyVM model)
        {
            var user = Common.GetUserNameByASpUserID(model.SendBy);
            if (user != null)
            {
                var Tickets = _ticketRepository.SaveReply(model);
                return Ok(new
                {
                    success = true,
                    data = Tickets
                });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }
        [HttpPost]
        public IHttpActionResult CloseTicket(int Ticket_ID)
        {
            _ticketRepository.ChangeStatus(Ticket_ID, Status.Close.ToString());
            return Ok(new
            {
                success = true,
            });
        }

        #endregion

        #region Notifications 
        [HttpGet]
        public IHttpActionResult CheckForNotification(string id)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                var data = _notificationRepository.CheckForNotification(id);
                return Ok(new { success = true, data });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }
        [HttpGet]
        public IHttpActionResult HaveSeen(string id, string NotifIDs)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                var f = _notificationRepository.HaveSeen(id, NotifIDs);
                return Ok(new { success = true, message = "Successfully Seen" });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }
        [HttpGet]
        public IHttpActionResult Notifications(string id)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                var data = _notificationRepository.GetNotifications(id);
                return Ok(new { success = true, data });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }
        [HttpGet]
        public IHttpActionResult WelcomeMessage(string id)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                var data = _notificationRepository.GetMessage();
                return Ok(new { success = true, data });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }

        [HttpPost]
        public IHttpActionResult SaveDevice(tbl_UserDevices model)
        {
            var user = Common.GetUserNameByASpUserID(model.UserID);
            if (user != null)
            {
                var success = _generalRepository.SaveUserDevice(model);
                return Ok(new { success = true, message = "Device processed", Data = true });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }
        #endregion

        #region Student Data

        [HttpGet]
        public IHttpActionResult GetProgress(string id)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                var data = _reportRepository.GetProgress(id);
                return Ok(new { success = true, data });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }

        [HttpGet]
        public IHttpActionResult DeleteAccount(string id)
        {
            var user = Common.GetUserNameByASpUserID(id);
            if (user != null)
            {
                var data = _enrollmentRepository.DeleteRequest(id);
                return Ok(new { success = data, message = "Error deleting account." });
            }
            return Ok(new { success = false, message = "Invalid ID" });
        }

        [HttpGet]
        public IHttpActionResult GetStudentID(string email)
        {
            var ID = _enrollmentRepository.GetStudentID(email);
            return Ok(new { success = true, message = "Invalid ID", Data = ID });
        }

        #endregion

        #region Mistakes


        #endregion

    }
}
