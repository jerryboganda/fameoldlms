using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize(Roles = "Teacher,Admin")]
    public class CouponController : Controller
    {
        private readonly ICouponRepository _couponRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly ISectionRepository _sectionRepository;

        public CouponController(
            ICouponRepository couponRepository,
            ICourseRepository courseRepository,
            ISectionRepository sectionRepository)
        {
            _couponRepository = couponRepository;
            _courseRepository = courseRepository;
            _sectionRepository = sectionRepository;
        }

        // GET: Coupon
        public ActionResult Index(int id = 0)
        {
            LoadCourses();
            CouponVM model = new CouponVM();
            if (id > 0)
            {
                model = _couponRepository.GetByID(id);
                LoadSections(model.CourseFid ?? 0);
            }
            else
            {
                model.CouponSecret = RandomString(10, true);
                model.ExpiryDate = Common.DateToString(DateTime.Today.AddDays(30));
                model.NoOfUses = 1;
                model.DiscountPer = 100;
                model.IsActive = true;
                LoadSections(0);
            }
            return View(model);
        }

        [AllowAnonymous]
        public JsonResult LoadSections(int id)
        {
            List<SectionVM> list = _sectionRepository.GetSectionsByCourse(id, "");
            var options = list.ConvertAll(x => new
            {
                Value = x.Section_ID,
                Text = x.Section_Name
            });

            options.Add(new
            {
                Value = 0,
                Text = "All Sections"
            });

            ViewBag.Section = options;
            return Json(options.OrderBy(x => x.Value), JsonRequestBehavior.AllowGet);
        }

        private void LoadCourses()
        {
            List<DAL.tbl_Courses> list = _courseRepository.GetList();
            ViewBag.Courses = list.ConvertAll(x => new
            {
                Value = x.Course_Id,
                Text = x.Course_Name
            });
        }

        [HttpPost]
        public ActionResult Index(CouponVM model)
        {

            if (ModelState.IsValid)
            {
                model.CreatedBy = User.Identity.GetUserId();
                bool f = _couponRepository.Create(model);
                return RedirectToAction("List");
            }
            LoadCourses();
            LoadSections(model.CourseFid ?? 0);
            return View(model);
        }
        public ActionResult List()
        {
            CouponVM model = new CouponVM();
            string id = User.Identity.GetUserId();
            model.List = _couponRepository.GetList(id);
            return View(model);
        }
        public string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
    }
}