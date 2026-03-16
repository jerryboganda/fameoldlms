using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using First_Aid_Made_Easy.DAL;
using System.Web.Mvc;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;

namespace First_Aid_Made_Easy.Areas.Blogs.Controllers
{
    public class HomeController : Controller
    {
        private readonly IBlogPostRepository _blogPostRepository;

        public HomeController(IBlogPostRepository blogPostRepository)
        {
            _blogPostRepository = blogPostRepository;
        }

        public ActionResult Index(string Category = null)
        {
            ViewBag.Category = Category;
            return View();
        }

        public ActionResult GetBlogs(int pageNo, int pagelength, string Category)
        {
            var model = _blogPostRepository.GetPublicList(pageNo, pagelength, Category);
            return PartialView("_BlogsPage", model);
        }


        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction(nameof(Index));
            var model = _blogPostRepository.GetSingle(slug: id);
            return View(model);
        }

        public ActionResult GetBlogsMetaData(string id)
        {
            var model = _blogPostRepository.GetBlogsMetaData(slug: id);
            return Json(model, 0);
        }
    }
}
