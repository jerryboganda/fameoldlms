using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.BLL.JzTimer;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [Authorize]
    public class QuestionPaperController : Controller
    {
        private readonly IQuestionPaperRepository _questionPaperRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IExamRepository _examRepository;
        private readonly IQuestionSystemsRepository _questionSystemsRepository;
        private readonly IQuestionPartitionRepository _questionPartitionRepository;
        private readonly ISettingRepository _settingRepository;
        private readonly IGeneralRepository _generalRepository;

        public QuestionPaperController(
            IQuestionPaperRepository questionPaperRepository,
            IQuestionRepository questionRepository,
            ICourseRepository courseRepository,
            ISectionRepository sectionRepository,
            IPackageRepository packageRepository,
            IExamRepository examRepository,
            IQuestionSystemsRepository questionSystemsRepository,
            IQuestionPartitionRepository questionPartitionRepository,
            ISettingRepository settingRepository,
            IGeneralRepository generalRepository)
        {
            _questionPaperRepository = questionPaperRepository;
            _questionRepository = questionRepository;
            _courseRepository = courseRepository;
            _sectionRepository = sectionRepository;
            _packageRepository = packageRepository;
            _examRepository = examRepository;
            _questionSystemsRepository = questionSystemsRepository;
            _questionPartitionRepository = questionPartitionRepository;
            _settingRepository = settingRepository;
            _generalRepository = generalRepository;
        }

        #region Question Paper CRUD

        public ActionResult Delete(int id)
        {
            bool f = _questionPaperRepository.DeleteQuestionPaper(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public ActionResult Index(int? id = 0)
        {
            QuestionPaperVM c = new QuestionPaperVM();
            if (id > 0)
                c = _questionPaperRepository.GetByID(id);

            ViewBag.Courses = _courseRepository.GetList(User.Identity.GetUserId(), User.IsInRole("Admin"));
            ViewBag.Sections = _sectionRepository.List(User.Identity.GetUserId(), User.IsInRole("Admin"));
            ViewBag.Packages = _packageRepository.GetList();
            ViewBag.Exams = _examRepository.GetList(0, "", "", "");
            ViewBag.Systems = _questionSystemsRepository.GetList();
            ViewBag.Partitions = _questionPartitionRepository.GetList();
            return View(c);
        }
        public ActionResult Preview(int? id = 0)
        {
            var c = _questionPaperRepository.GetByID(id);
            return View(c);
        }
        [HttpPost]
        public ActionResult Index(QuestionPaperVM vid)
        {
            vid.CreatedBy = User.Identity.GetUserId();
            QuestionPaperVM id = _questionPaperRepository.Save(vid);
            return Json(id, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public ActionResult List()
        {
            QuestionPaperVM model = new QuestionPaperVM { List = _questionPaperRepository.GetList() };
            return View(model);
        }
        public ActionResult Filter(int DifficultyLevel, string Tags)
        {
            QuestionPaperVM model = new QuestionPaperVM();
            model.List = _questionPaperRepository.GetList();
            return PartialView("_List", model);
        }

        public ActionResult FilterList(int DifficultyLevel, string Tags, string Text)
        {
            QuestionPaperVM model = new QuestionPaperVM();
            model.List = _questionPaperRepository.GetList();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UploadImage(int ID)
        {
            HttpFileCollectionBase files = Request.Files;
            HttpPostedFileBase file = files[0];
            string FileName = Common.SavePic(file, "Paper/");
            var f = _questionPaperRepository.SavePic(FileName, ID);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region For Student
        public ActionResult CreateTest()
        {
            ViewBag.QuestionCount = _questionPaperRepository.GetQuestionCount(User.Identity.GetUserId(), "");
            ViewBag.Systems = _questionPaperRepository.GetSystems(User.Identity.GetUserId());
            return View();
        }
        [HttpPost]
        public ActionResult CreateTest(CreateTestInput result)
        {
            result.StudentID = User.Identity.GetUserId();
            CreateTestInput id = _questionPaperRepository.AutoCreateResult(result);
            return Json(id.ID, JsonRequestBehavior.AllowGet);
        }
        public ActionResult StudentList()
        {
            QuestionPaperVM model = new QuestionPaperVM { List = _questionPaperRepository.GetList(User.Identity.GetUserId(), false).ToList() };
            return View(model);
        }
        public ActionResult MockTests()
        {
            QuestionPaperVM model = new QuestionPaperVM { List = _questionPaperRepository.GetList(User.Identity.GetUserId()).ToList() };
            return View(model);
        }
        public ActionResult PreviousTests()
        {
            ViewBag.ResultList = _questionPaperRepository.ResultList(User.Identity.GetUserId());
            return View();
        }

        public ActionResult Solve(int id, string m = "Exam")
        {
            int ID = _questionPaperRepository.AutoCreateResultFromPaper(id, m, User.Identity.GetUserId());
            ViewBag.Mode = m;
            return RedirectToAction(nameof(Continue), new { ID });
        }

        [AllowAnonymous]
        public ActionResult SolveFreeTrialTest()
        {
            var id = _settingRepository.GetConfigByName(Settings.QSys.TrialTestId);
            if(id == null)
            {
                ViewBag.Title = "Sorry !";
                ViewBag.Remarks = "No Free Trial Test Available";
                return View("Lockout");
            }
            var m = "Exam";
            int resultID = _questionPaperRepository.AutoCreateResultFromPaper(Convert.ToInt32(id), m, User.Identity.GetUserId());
            ViewBag.Mode = m;
            return RedirectToAction(nameof(Continue), new { ID= resultID });
        }
        [AllowAnonymous]
        public ActionResult Continue(int id)
        {
            var Result = _questionPaperRepository.GetResultByID(id);
            QuestionPaperVM c = _questionPaperRepository.GetByResultID(id);
            ViewBag.Result = Result;
            ViewBag.Mode = Result.Mode;
            return View(nameof(Solve), c);
        }
        [AllowAnonymous]
        public ActionResult View(int id)
        {
            var Result = _questionPaperRepository.GetResultByID(id);
            QuestionPaperVM c = _questionPaperRepository.GetByResultID(id);
            ViewBag.Result = Result;
            return View(c);
        }
        public ActionResult SelectMockTests(int id)
        {
            string UID = User.Identity.GetUserId();
            var c = _generalRepository.SelectMockTests(id, UID);
            return Json(c, 0);
        }

        #endregion

        #region Result

        [AllowAnonymous]
        public ActionResult Analysis(int id)
        {
            var Result = _questionPaperRepository.GetResultByID(id);
            QuestionPaperVM c = _questionPaperRepository.GetByResultID(id);
            ViewBag.Result = Result;
            ViewBag.Mode = Result.Mode;
            return View(c);
        }
        public ActionResult SaveResult(ResultVM vid)
        {
            vid.StudentID = User.Identity.GetUserId();
            ResultVM id = _questionPaperRepository.SaveResult(vid);
            return Json(id.ID, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteResult(int ID)
        {
            _questionPaperRepository.DeleteResult(ID);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveResultDetail(ResultDetailVM vid)
        {
            ResultDetailVM id = _questionPaperRepository.SaveResult(vid);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult GetResultByID(int id)
        {
            var result = _questionPaperRepository.GetResultByID(id);
            var paper = _questionPaperRepository.GetByResultID(id);
            if (result != null && paper != null)
            {
                result.TotalMarks = paper.TotalMarks;
                result.PassPer = paper.PassPer;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public void Completed(int ID)
        {
            _questionPaperRepository.Completed(ID);
        }
        public void Suspend(int ID)
        {
            _questionPaperRepository.Suspend(ID);
        }


        public ActionResult RecheckQuestion(int QPID, int QuestionID)
        {
            return Json(_questionPaperRepository.RecheckQuestion(QPID, QuestionID), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Comment / Notes / Tags / Reviews

        public ActionResult SaveComment(string Comment, int id = 0, int VidID = 0)
        {
            var a = new tbl_Comment
            {
                UserFid = User.Identity.GetUserId(),
                Comment_Body = Comment,
                Comment_Date = Common.GetCurrentDate(),
                QuestionID = id,
                Video_Fid = VidID
            };
            var f = _questionRepository.PostComment(a);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveNotesTags(int id, string Notes, string Tags)
        {
            bool f = false;

            var a = new tbl_Notes
            {
                StudentID = User.Identity.GetUserId(),
                FID = id,
                CreatedDT = Common.GetCurrentDate()
            };
            if (!string.IsNullOrEmpty(Notes))
            {
                a.Notes = Notes;
                a.Type = NotesType.Notes.ToString();
                f = _questionRepository.SaveNotesTags(a);
            }
            if (!string.IsNullOrEmpty(Tags))
            {
                a.Notes = Tags;
                a.Type = NotesType.Tags.ToString();
                f = _questionRepository.SaveNotesTags(a);
            }
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetCommentNotesTags(int id)
        {
            var comments = _questionRepository.GetComments(QuestionID: id);
            var NotesTags = _questionRepository.GetNotesTags(id, User.Identity.GetUserId());
            return Json(new { comments, notes = NotesTags.Item1, tags = NotesTags.Item2 }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ApproveComment(int id, bool IsApproved)
        {
            var comments = _questionRepository.ApproveComments(id, User.Identity.GetUserId(), IsApproved);
            return Json(comments, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CommentList()
        {
            var comments = _questionRepository.GetApproveComments("Pending");
            return View(comments);
        }
        public ActionResult FilterApproveComments(string IsApproved)
        {
            var comments = _questionRepository.GetApproveComments(IsApproved);
            return PartialView("_CommentList", comments);
        }
        public ActionResult SaveReview(int id, string Type, int Stars)
        {
            sp_GetReview_Result f = _questionRepository.SaveReview(id, Type, User.Identity.GetUserId(), Stars);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region  Allow Reopen

        public ActionResult AllowReopen()
        {
            var model = _questionPaperRepository.InCompleteResultList();
            return View(model);
        }
        public ActionResult Allow(int ID)
        {
            var f = _questionPaperRepository.AllowReopen(ID);
            return Json(f, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}