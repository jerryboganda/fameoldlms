using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.BLL.JzTimer;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Areas.Blogs.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class BlogPostController : Controller
    {
        private readonly IBlogPostRepository _blogPostRepository;

        public BlogPostController(IBlogPostRepository blogPostRepository)
        {
            _blogPostRepository = blogPostRepository;
        }

        #region BlogPost CRUD

        public ActionResult Delete(int id)
        {
            bool f = _blogPostRepository.Delete(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Index(int? id = 0)
        {
            BlogPostVM c = new BlogPostVM() { Author = "Dr. Hafiz Atif" };
            if (id > 0)
                c = _blogPostRepository.GetSingle(id);
            else 
                c.SortID = _blogPostRepository.GetCount() + 1;
            return View(c);
        }
        [HttpPost]
        public ActionResult Index(BlogPostVM vid)
        {
            string userId = User.Identity.GetUserId();
            BlogPostVM id = _blogPostRepository.Save(vid, userId);
            id.FeaturedImageFile = null;
            return Json(id, JsonRequestBehavior.AllowGet);
        }
        public ActionResult List()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> GetBlogPosts()
        {
            var draw = Request.Form["draw"];
            var start = Convert.ToInt32(Request.Form["start"]);
            var length = Convert.ToInt32(Request.Form["length"]);
            var searchValue = Request.Form["search[value]"];
            var sortColumnIndex = Convert.ToInt32(Request.Form["order[0][column]"]);
            var sortColumnName = Request.Form["columns[" + sortColumnIndex + "][data]"];
            var sortDirection = Request.Form["order[0][dir]"]; // asc or desc

            var result = await _blogPostRepository.GetBlogPostsAsync(draw, start, length, searchValue, sortColumnName, sortDirection);

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        #endregion


    }
}