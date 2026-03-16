using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using First_Aid_Made_Easy.DAL;
using System.Web.Mvc;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;

namespace First_Aid_Made_Easy.Areas.Landing.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGeneralRepository _generalRepository;
        private readonly IWebsitePackageRepository _websitePackageRepository;

        public HomeController(IGeneralRepository generalRepository, IWebsitePackageRepository websitePackageRepository)
        {
            _generalRepository = generalRepository;
            _websitePackageRepository = websitePackageRepository;
        }

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ContactUs()
        {
            return View();
        }
        public ActionResult AboutUs()
        {
            return View();
        }
        public ActionResult Collaboration()
        {
            return View();
        }
        public ActionResult Team()
        {
            return View();
        }
        public ActionResult Faqs()
        {
            return View();
        }
        public ActionResult Terms()
        {
            return View();
        }
        public ActionResult Guidelines()
        {
            return View();
        }
        public ActionResult DemoVideos()
        {
            return View();
        }
        public ActionResult SuccessStories()
        {
            return View();
        }
        public ActionResult OurPackages()
        {
            var packages = _websitePackageRepository.GetActivePackages();
            return View(packages);
        }
        public ActionResult TermsAndConditions()
        {
            return View();
        }
        public ActionResult PrivacyPolicy()
        {
            return View();
        }
        public ActionResult OurDifferences()
        {
            return View();
        }
        //public ActionResult TrialVideos()
        //{
        //    VideoRepository repo = new VideoRepository();
        //    List<VideoVM> model = repo.GetVideosByType(ContentType.Trial);
        //    return View(model);
        //}
        public ActionResult SubscribeEmail(string Email)
        {
            var result = _generalRepository.SubscribeEmail(Email);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
