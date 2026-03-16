using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using System;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    /// <summary>
    /// Public (unauthenticated) endpoints for email open tracking, click tracking, and unsubscribe.
    /// </summary>
    [AllowAnonymous]
    public class EmailTrackingController : Controller
    {
        private readonly IEmailMarketingRepository _emailRepo;

        // 1x1 transparent GIF bytes
        private static readonly byte[] TransparentGif = Convert.FromBase64String(
            "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");

        public EmailTrackingController(IEmailMarketingRepository emailRepo)
        {
            _emailRepo = emailRepo;
        }

        /// <summary>
        /// Track email open via 1x1 transparent pixel.
        /// URL: /EmailTracking/Open/{recipientId}
        /// </summary>
        [HttpGet]
        public ActionResult Open(long id)
        {
            try
            {
                _emailRepo.TrackOpen(id);
            }
            catch { /* Fail silently - tracking should never break */ }

            // Return 1x1 transparent GIF
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            return File(TransparentGif, "image/gif");
        }

        /// <summary>
        /// Track link click and redirect to original URL.
        /// URL: /EmailTracking/Click/{linkId}/{recipientId}
        /// </summary>
        [HttpGet]
        public ActionResult Click(int id, long recipientId)
        {
            string originalUrl = "https://firstaidmadeeasy.com.pk";

            try
            {
                var userAgent = Request.UserAgent;
                var ipAddress = Request.UserHostAddress;
                originalUrl = _emailRepo.TrackClick(id, recipientId, userAgent, ipAddress) ?? originalUrl;
            }
            catch { /* Fail silently */ }

            return Redirect(originalUrl);
        }

        /// <summary>
        /// Unsubscribe page.
        /// URL: /EmailTracking/Unsubscribe/{token}
        /// </summary>
        [HttpGet]
        public ActionResult Unsubscribe(string id)
        {
            var email = _emailRepo.DecodeUnsubscribeToken(id ?? "");
            if (string.IsNullOrEmpty(email))
            {
                return View(new UnsubscribeVM { Token = id, IsInvalid = true });
            }
            var model = new UnsubscribeVM
            {
                Token = id,
                Email = email
            };
            return View(model);
        }

        /// <summary>
        /// Process unsubscribe.
        /// </summary>
        [HttpPost]
        public ActionResult Unsubscribe(UnsubscribeVM model)
        {
            try
            {
                var email = _emailRepo.DecodeUnsubscribeToken(model.Token ?? "");
                if (string.IsNullOrEmpty(email))
                {
                    model.IsInvalid = true;
                    model.Success = false;
                    model.Message = "Invalid unsubscribe link.";
                    return View(model);
                }

                _emailRepo.ProcessUnsubscribe(email, model.Reason, null);
                model.Email = email;
                model.Success = true;
                model.IsProcessed = true;
                model.Message = "You have been successfully unsubscribed from marketing emails.";
            }
            catch (Exception)
            {
                model.Success = false;
                model.Message = "An error occurred. Please try again later.";
            }

            return View(model);
        }
    }
}
