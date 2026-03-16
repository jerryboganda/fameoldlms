using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using First_Aid_Made_Easy.BLL.Interfaces;


namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    public class TeacherController : Controller
    {
        private readonly ISectionRepository _sectionRepository;
        private readonly IVideoRepository _videoRepository;

        public TeacherController(ISectionRepository sectionRepository, IVideoRepository videoRepository)
        {
            _sectionRepository = sectionRepository;
            _videoRepository = videoRepository;
        }

        // GET: Teacher
        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Teacher,Admin,Assistant,UniTeacher")]
        public ActionResult AssistantIndex()
        {
            return View();
        }
        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult CoursePreview(int id)
        {
            string Uid = User.Identity.GetUserId();
            var user = Common.GetUserNameByASpUserID(Uid);
            ViewBag.PhoneNo = user.User_Mobile;
            ViewBag.Name = user.User_Name;
            SectionVM s = _sectionRepository.GetByIDForTakeLec(id, Uid);
            s.VideoList = _videoRepository.GetVideosBySection(id, Uid);
            return View(s);
        }
    }
}