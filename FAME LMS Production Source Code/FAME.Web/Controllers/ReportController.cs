using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using First_Aid_Made_Easy.DAL;
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using Rotativa;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IQuestionPaperRepository _questionPaperRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IQuestionSystemsRepository _questionSystemsRepository;
        private readonly IReportRepository _reportRepository;
        private readonly IUserRepository _userRepository;

        public ReportController(
            IQuestionPaperRepository questionPaperRepository,
            ISectionRepository sectionRepository,
            IQuestionSystemsRepository questionSystemsRepository,
            IReportRepository reportRepository,
            IUserRepository userRepository)
        {
            _questionPaperRepository = questionPaperRepository;
            _sectionRepository = sectionRepository;
            _questionSystemsRepository = questionSystemsRepository;
            _reportRepository = reportRepository;
            _userRepository = userRepository;
        }

        #region Progress
        public ActionResult Progress()
        {
            ViewBag.Papers = _questionPaperRepository.GetList().ConvertAll(x => new SelectListItem
            {
                Text = x.PaperTitle,
                Value = x.PaperID.ToString()
            });
            ViewBag.Sections = _sectionRepository.List(User.Identity.GetUserId(), true);
            return View();
        }
        public ActionResult GetStudent(int T)
        {
            var s = _userRepository.GetStudentsByType(T);
            return Json(s, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetProgress(int UniversityID, int PaperID, string SectionID, string StudentID, string rptType, bool IsPDF = false, bool IsExcel = false)
        {
            ViewBag.Total = _reportRepository.GetUniversityTotalUsers(UniversityID);
            var Model = _reportRepository.GetResultReport(UniversityID, PaperID, SectionID, StudentID);
                if (IsPDF)
                    //return View("ProgressPDF", Model);
                    return new ViewAsPdf("ProgressPDF", Model);
                else if (IsExcel)
                {
                    string htmlContent = Common.ViewToString(this.ControllerContext, "ProgressPDF", Model);
                    Common.ExportExcel(htmlContent, "ProgressReport");
                    return null;
                }
                else
                    return PartialView("_Progress", Model);
        }
        #endregion

        #region Email Report

        public ActionResult EmailReport()
        {
            return View();
        }
        public async Task<ActionResult> GetEmailReport(DateTime DateFrom, DateTime DateTo, bool IsPDF = false, bool IsExcel = false)
        {
            DateFrom = DateFrom.Date;
            DateTo = DateTo.Date.AddDays(1).AddMilliseconds(-1);
            var Model = await _reportRepository.GetEmailReport(DateFrom, DateTo);

            if (IsPDF)
                return new ViewAsPdf("EmailReportPDF", Model);
            else if (IsExcel)
            {
                string htmlContent = Common.ViewToString(this.ControllerContext, "EmailReportPDF", Model);
                Common.ExportExcel(htmlContent, "EmailReportReport");
                return null;
            }
            else
                return PartialView("_EmailReport", Model);
        }


        #endregion

        #region Question Stats

        public ActionResult QuestionStats()
        {
            ViewBag.Papers = _questionPaperRepository.GetList().ConvertAll(x => new SelectListItem
            {
                Text = x.PaperTitle,
                Value = x.PaperID.ToString()
            });
            ViewBag.Systems = _questionSystemsRepository.GetList();
            return View();
        }
        public ActionResult GetQuestionStats(int PaperID, int SystemID, bool IsPDF = false, bool IsExcel = false)
        {
            var Model = _reportRepository.GetQuestionStats(PaperID, SystemID);
            if (IsPDF)
                return new ViewAsPdf("QuestionStatsPDF", Model);
            else if (IsExcel)
            {
                string htmlContent = Common.ViewToString(this.ControllerContext, "QuestionStatsPDF", Model);
                Common.ExportExcel(htmlContent, "QuestionStatsReport");
                return null;
            }
            else
                return PartialView("_QuestionStats", Model);
        }


        #endregion

        #region Trial Students

        public ActionResult TrialStudents()
        {
            var list = _reportRepository.GetTrialStudents();
            return View(list);
        }
        #endregion
    }
}