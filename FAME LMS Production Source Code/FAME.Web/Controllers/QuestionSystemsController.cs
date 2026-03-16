using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.BLL.JzTimer;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize(Roles = "Admin,Teacher,Assistant")]
    public class QuestionSystemsController : Controller
    {
        private readonly IQuestionSystemsRepository _questionSystemsRepository;
        private readonly ISettingRepository _settingRepository;
        private readonly IQuestionPaperRepository _questionPaperRepository;

        public QuestionSystemsController(
            IQuestionSystemsRepository questionSystemsRepository,
            ISettingRepository settingRepository,
            IQuestionPaperRepository questionPaperRepository)
        {
            _questionSystemsRepository = questionSystemsRepository;
            _settingRepository = settingRepository;
            _questionPaperRepository = questionPaperRepository;
        }

        #region QuestionSystems CRUD

        public ActionResult Delete(int id)
        {
            bool f = _questionSystemsRepository.Delete(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Index(int? id = 0)
        {
            QuestionSystemVM c = new QuestionSystemVM();
            if (id > 0)
                c = _questionSystemsRepository.GetByID(id);
            return View(c);
        }
        [HttpPost]
        public ActionResult Index(QuestionSystemVM vid)
        {
            QuestionSystemVM id = _questionSystemsRepository.Save(vid);
            return Json(id, JsonRequestBehavior.AllowGet);
        }
        public ActionResult List()
        {
            List<QuestionSystemVM> model = _questionSystemsRepository.GetList();
            return View(model);
        }
        public ActionResult Settings()
        {
            var model = _settingRepository.GetQuestionSystemSettings();
            ViewBag.Systems = _questionSystemsRepository.GetList();
            ViewBag.Papers = _questionPaperRepository.GetList();

            return View(model);
        }
        [HttpPost]
        public ActionResult SaveSettings(QuestionSystemSettingsVM t)
        {
            var model = _settingRepository.SaveQuestionSystemSettings(t);
            return Json(true, 0);
        }

        #endregion


    }
}