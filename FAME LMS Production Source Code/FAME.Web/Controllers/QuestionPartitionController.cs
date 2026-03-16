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
    public class QuestionPartitionController : Controller
    {
        private readonly IQuestionPartitionRepository _questionPartitionRepository;

        public QuestionPartitionController(IQuestionPartitionRepository questionPartitionRepository)
        {
            _questionPartitionRepository = questionPartitionRepository;
        }

        #region QuestionPartition CRUD

        public ActionResult Delete(int id)
        {
            bool f = _questionPartitionRepository.Delete(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Index(int? id = 0)
        {
            QuestionSystemVM c = new QuestionSystemVM();
            if (id > 0)
                c = _questionPartitionRepository.GetByID(id);
            return View(c);
        }
        [HttpPost]
        public ActionResult Index(QuestionSystemVM vid)
        {
            QuestionSystemVM id = _questionPartitionRepository.Save(vid);
            return Json(id, JsonRequestBehavior.AllowGet);
        }
        public ActionResult List()
        {
            List<QuestionSystemVM> model = _questionPartitionRepository.GetList();
            return View(model);
        }

        #endregion


    }
}