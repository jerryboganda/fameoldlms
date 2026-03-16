using First_Aid_Made_Easy.BLL.Interfaces;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Areas.Certificate.Controllers
{
    /// <summary>
    /// Public certificate verification page.
    /// No authentication required — anyone with the link or QR can verify.
    /// </summary>
    [AllowAnonymous]
    public class VerifyController : Controller
    {
        private readonly ICertificateVerificationService _verificationService;

        public VerifyController(ICertificateVerificationService verificationService)
        {
            _verificationService = verificationService;
        }

        /// <summary>
        /// GET /Certificate/Verify/{id}
        /// Public verification page.
        /// </summary>
        public ActionResult Index(string id)
        {
            var ipAddress = Request.UserHostAddress;

            // Rate limiting
            if (!_verificationService.CheckRateLimit(ipAddress))
            {
                return new HttpStatusCodeResult(429, "Too many requests. Please try again later.");
            }

            var vm = _verificationService.Verify(id, ipAddress);
            return View(vm);
        }
    }
}
