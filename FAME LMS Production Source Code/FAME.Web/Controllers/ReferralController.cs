using First_Aid_Made_Easy.BLL.Interfaces;
using Serilog;
using System;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    /// <summary>
    /// Public controller for handling referral link redirects and tracking
    /// Handles routes like /ref/ABC123 or /r/ABC123
    /// </summary>
    public class ReferralController : Controller
    {
        private readonly IReferralRepository _referralRepository;
        private readonly IAmbassadorRepository _ambassadorRepository;
        private readonly ILogger _logger;
        private const string REFERRAL_COOKIE_NAME = "fame_ref";
        private const int COOKIE_EXPIRY_DAYS = 30;

        public ReferralController(IReferralRepository referralRepository, IAmbassadorRepository ambassadorRepository)
        {
            _referralRepository = referralRepository;
            _ambassadorRepository = ambassadorRepository;
            _logger = Log.ForContext<ReferralController>();
        }

        /// <summary>
        /// Handle referral link click: /ref/{code} or /r/{code}
        /// Tracks the click, stores referral code in cookie, and redirects to registration
        /// </summary>
        [Route("ref/{code}")]
        [Route("r/{code}")]
        public ActionResult Track(string code, string utm_source = null, string utm_medium = null, string utm_campaign = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    _logger.Warning("Empty referral code received");
                    return RedirectToAction("Register", "Account");
                }

                // Validate referral code exists and ambassador is active
                var ambassador = _ambassadorRepository.GetByReferralCode(code);
                if (ambassador == null)
                {
                    _logger.Warning("Invalid referral code: {Code}", code);
                    return RedirectToAction("Register", "Account");
                }

                if (ambassador.Status != "Active")
                {
                    _logger.Warning("Referral code {Code} belongs to inactive ambassador", code);
                    return RedirectToAction("Register", "Account");
                }

                // Track the click
                var ipAddress = GetClientIpAddress();
                var userAgent = Request.UserAgent;
                var landingPage = Request.Url?.AbsoluteUri;
                var source = utm_source ?? utm_campaign ?? "direct";

                _referralRepository.TrackClick(code, ipAddress, userAgent, landingPage, source);

                // Store referral code in cookie
                SetReferralCookie(code, source);

                _logger.Information("Referral click tracked: Code={Code}, IP={IP}, Source={Source}", code, ipAddress, source);

                // Redirect to registration with referral context
                return RedirectToAction("Register", "Account", new { refCode = code });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing referral code: {Code}", code);
                return RedirectToAction("Register", "Account");
            }
        }

        /// <summary>
        /// Store referral code in a cookie for attribution during registration
        /// </summary>
        private void SetReferralCookie(string code, string source)
        {
            var cookie = new HttpCookie(REFERRAL_COOKIE_NAME)
            {
                Value = $"{code}|{source}|{DateTime.UtcNow:O}",
                Expires = DateTime.UtcNow.AddDays(COOKIE_EXPIRY_DAYS),
                HttpOnly = true,
                Secure = Request.IsSecureConnection,
                Path = "/"
            };

            Response.Cookies.Add(cookie);
        }

        /// <summary>
        /// Get client IP address, handling proxies/load balancers
        /// </summary>
        private string GetClientIpAddress()
        {
            var ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ip))
            {
                // May contain multiple IPs, take the first one
                var ips = ip.Split(',');
                return ips[0].Trim();
            }

            ip = Request.ServerVariables["HTTP_X_REAL_IP"];
            if (!string.IsNullOrEmpty(ip))
                return ip;

            return Request.UserHostAddress;
        }

        /// <summary>
        /// Static helper to retrieve referral info from cookie during registration
        /// Call this from AccountController.Register to attribute new users
        /// </summary>
        public static ReferralCookieInfo GetReferralFromCookie(HttpRequestBase request)
        {
            var cookie = request.Cookies[REFERRAL_COOKIE_NAME];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value))
                return null;

            try
            {
                var parts = cookie.Value.Split('|');
                if (parts.Length >= 1)
                {
                    return new ReferralCookieInfo
                    {
                        ReferralCode = parts[0],
                        Source = parts.Length > 1 ? parts[1] : "direct",
                        ClickedAt = parts.Length > 2 ? DateTime.Parse(parts[2]) : DateTime.UtcNow
                    };
                }
            }
            catch
            {
                // Malformed cookie
            }

            return null;
        }

        /// <summary>
        /// Clear referral cookie after successful attribution
        /// </summary>
        public static void ClearReferralCookie(HttpResponseBase response)
        {
            var cookie = new HttpCookie(REFERRAL_COOKIE_NAME)
            {
                Value = "",
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/"
            };
            response.Cookies.Add(cookie);
        }
    }

    /// <summary>
    /// DTO for referral cookie data
    /// </summary>
    public class ReferralCookieInfo
    {
        public string ReferralCode { get; set; }
        public string Source { get; set; }
        public DateTime ClickedAt { get; set; }
    }
}
