using System.Web.Mvc;

namespace First_Aid_Made_Easy.Areas.Ambassador.Controllers
{
    /// <summary>
    /// Public Ambassador Home Controller
    /// Handles the public landing page for the Ambassador Program
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Public Ambassador Program Landing Page
        /// GET: /Ambassador/Home/Index or /Ambassador/Home
        /// </summary>
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Redirect to Apply - Smart routing based on authentication status
        /// GET: /Ambassador/Home/Join
        /// </summary>
        [AllowAnonymous]
        public ActionResult Join()
        {
            // Redirect directly to the unified registration/application form
            return RedirectToAction("Apply", "Profile", new { area = "Ambassador" });
        }
    }
}
