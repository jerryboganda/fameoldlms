using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using LinqKit;
using Microsoft.AspNet.Identity;
using First_Aid_Made_Easy.BLL.Interfaces;
using System;

using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize(Roles = "Admin,Teacher")]
    public class CourseController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly ISectionSubRepository _sectionSubRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IDashBoardRepository _dashBoardRepository;

        public CourseController(
            ICourseRepository courseRepository,
            ISectionRepository sectionRepository,
            ISectionSubRepository sectionSubRepository,
            IVideoRepository videoRepository,
            IQuestionRepository questionRepository,
            IDashBoardRepository dashBoardRepository)
        {
            _courseRepository = courseRepository;
            _sectionRepository = sectionRepository;
            _sectionSubRepository = sectionSubRepository;
            _videoRepository = videoRepository;
            _questionRepository = questionRepository;
            _dashBoardRepository = dashBoardRepository;
        }

        #region Course Crud
        public ActionResult CourseIndex(int id)
        {
            LoadCourses();
            var course = _courseRepository.GetDetailsByID(id);
            return View(course);
        }
        public ActionResult Courses()
        {
            CourseVM course = new CourseVM();
            course.CorList = _courseRepository.GetListVM(User.Identity.GetUserId(), User.IsInRole("Admin"));
            return View(course);
        }
        public ActionResult CourseSave(int? id = 0)
        {
            CourseVM c = new CourseVM();
            ViewBag.Teachers = _dashBoardRepository.GetUserList((int)Roles.Teacher);
            if (id > 0)
            {
                c = _courseRepository.GetByID(id ?? 0);
            }
            else
            {
                c.Course_Pic = "Course_200x168.png";
            }
            return View(c);
        }
        [HttpPost]
        public ActionResult SaveCourse(CourseVM course)
        {
            course.Teacher_Fid = course.Teacher_Fid ?? User.Identity.GetUserId();
            int id = _courseRepository.SaveCourse(course);
            TempData["Success"] = "Success";
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region SECTION CRUD

        public ActionResult SectionIndex(int CourseFid, int? id = 0)
        {
            SectionVM c = new SectionVM();
            if (id > 0)
            {
                c = _sectionRepository.GetByID(id);
            }
            c.Course_Fid = CourseFid;
            return View(c);
        }
        [HttpPost]
        public ActionResult SectionIndex(SectionVM section)
        {
            int id = _sectionRepository.SaveSection(section);
            return RedirectToAction("CourseIndex", new { id = section.Course_Fid });
        }

        public ActionResult DeleteSection(int id)
        {
            bool f = _sectionRepository.DeleteSection(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region SUB SECTION CRUD

        public ActionResult SectionSubIndex(int SectionFid, int? id = 0)
        {
            SectionSubVM c = new SectionSubVM();
            if (id > 0)
            {
                c = _sectionSubRepository.GetByID(id);
            }
            else
            {
                c.Section_Fid = SectionFid;
                c.Course_Fid = _videoRepository.GetCourseFid(SectionFid);
            }

            return View(c);
        }
        [HttpPost]
        public ActionResult SectionSubIndex(SectionSubVM section)
        {
            int id = _sectionSubRepository.SaveSection(section);
            return RedirectToAction("CourseIndex", new { id = section.Course_Fid });
        }

        public ActionResult DeleteSectionSub(int id)
        {
            bool f = _sectionSubRepository.DeleteSection(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Video CRUD

        public ActionResult DeleteVideo(int id)
        {
            bool f = _videoRepository.DeleteVideo(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        public ActionResult VideoIndex(int ContentType,int? SectionFid = null, int? SectionSubId = null, int id = 0)
        {
            VideoVM c;
            if (id > 0)
            {
                c = _videoRepository.GetByID(id);
                ViewBag.Faq = _questionRepository.GetList(id);
            }
            else
            {
                c = new VideoVM
                {
                    //ContentType = ContentType,
                    Section_Fid = SectionFid,
                    SectionSub_Fid = SectionSubId,
                    Course_Fid = _videoRepository.GetCourseFid(SectionFid)
                };
            }
            return View(c);
        }
        [HttpPost]
        public ActionResult VideoIndex(VideoVM vid)
        {
            if (vid.Type == (int)ContentType.File && vid.Video_Id == 0 && vid.File == null)
                ModelState.AddModelError("File", "File Is Required");
            if (ModelState.IsValid)
            {
                VideoVM id = _videoRepository.SaveVideo(vid);
                if (id.Video_Id > 0)
                {
                    TempData["Success"] = "Data Saved Successfully";
                    return RedirectToAction("CourseIndex", "Course", new { id = id.Course_Fid, sectionID = vid.Section_Fid });
                }
            }
            return View(vid);
        }
        #endregion

        #region Question CRUD

        public ActionResult DeleteQuestion(int id)
        {
            bool f = _questionRepository.DeleteQuestion(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        public ActionResult QuestionIndex(int SectionFid, int? id = 0)
        {
            QuestionVM c = new QuestionVM();
            if (id > 0)
            {
                c = _questionRepository.GetByID(id);
            }
            c.Section_Fid = SectionFid;
            c.Course_Fid = _questionRepository.GetCourseFid(SectionFid);
            return View(c);
        }
        [HttpPost]
        public ActionResult QuestionIndex(QuestionVM vid)
        {
            vid.CreatedBy = User.Identity.GetUserId();
            QuestionVM id = _questionRepository.SaveQuestion(vid);
            return Json(id, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Chapter Blog

        public ActionResult ChapterBlog(int id, int ChapterID = 0)
        {
            if (ChapterID > 0) return View(_courseRepository.GetChapterByID(ChapterID));
            return View(new ChapterVM { Course_Id = id });
        }
        public ActionResult SaveChapter(ChapterVM model)
        {
            _courseRepository.SaveChapter(model);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteChapter(int id)
        {
            bool f = _courseRepository.DeleteChapter(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Save Sortings
        [HttpPost]
        public ActionResult SaveSorting(SortingVM d)
        {
            var f = _courseRepository.SaveSorting(d);
            return Json(f, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Copy Videos

        private void LoadCourses()
        {
            string id = User.Identity.GetUserId();
            List<DAL.tbl_Courses> list = _courseRepository.GetList(id, User.IsInRole("Admin"));
            ViewBag.Courses = list.ConvertAll(x => new
            {
                Value = x.Course_Id,
                Text = x.Course_Name
            });
            ViewBag.Section = _sectionRepository.List(User.Identity.GetUserId(), User.IsInRole("Admin"));
        }
        public ActionResult GetFromCourse(int id)
        {
            var course = _courseRepository.GetDetailsByID(id);
            return PartialView("_CopyVideos", course);
        }
        public ActionResult CopyVideos(int SectionId, string VideoIds, int? SectionSubID = null)
        {
            var f = _courseRepository.CopyVideos(SectionId, SectionSubID, VideoIds);
            return Json(f, 0);
        }

        #endregion
        public ActionResult UploadImage()
        {
            try
            {
                HttpFileCollectionBase files = Request.Files;
                HttpPostedFileBase file = files[0];
                string fname;
                // Checking for Internet Explorer      
                if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                {
                    string[] testfiles = file.FileName.Split(new char[] { '\\' });
                    fname = testfiles[testfiles.Length - 1];
                }
                else
                {
                    fname = file.FileName;
                }
                fname = "Course" + fname;
                System.IO.File.Delete(Server.MapPath("~/Images/" + fname));
                fname = Path.Combine(Server.MapPath("~/Images/"), fname);
                file.SaveAs(fname);
                return Json("Course" + file.FileName, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return null;
            }
        }
    }
}