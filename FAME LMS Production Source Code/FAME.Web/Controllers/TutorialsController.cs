using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
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
    [Authorize(Roles = "Admin,Teacher")]
    public class TutorialsController : Controller
    {
        private readonly ITutorialRepository _tutorialRepository;
        private readonly IGeneralRepository _generalRepository;
        private readonly IVideoRepository _videoRepository;

        public TutorialsController(
            ITutorialRepository tutorialRepository,
            IGeneralRepository generalRepository,
            IVideoRepository videoRepository)
        {
            _tutorialRepository = tutorialRepository;
            _generalRepository = generalRepository;
            _videoRepository = videoRepository;
        }


        #region Tutorials CRUD

        public ActionResult Delete(int id)
        {
            bool f = _tutorialRepository.DeleteTutorial(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Index(int? id = 0)
        {
            ViewBag.Categories = _generalRepository.GetList((int)MasterGroup.AgentCategory);
            TutorialVM c = new TutorialVM();
            if (id > 0)
                c = _tutorialRepository.GetByID(id);
            return View(c);
        }
        public ActionResult Preview(int id)
        {
            var c = _tutorialRepository.GetByID(id);
            return View(c);
        }
        [HttpPost]
        public ActionResult Index(TutorialVM vid)
        {
            vid.CreatedBy = User.Identity.GetUserId();
            TutorialVM id = _tutorialRepository.SaveTutorial(vid);
            return Json(id, JsonRequestBehavior.AllowGet);
        }
        public ActionResult List()
        {
            TutorialVM model = new TutorialVM { List = _tutorialRepository.GetList()};
            return View(model);
        }
        public ActionResult TrialList()
        {
            List<VideoVM> model = _videoRepository.GetVideosByType(ContentType.Trial);
            return View(model);
        }

        #endregion


    }
}