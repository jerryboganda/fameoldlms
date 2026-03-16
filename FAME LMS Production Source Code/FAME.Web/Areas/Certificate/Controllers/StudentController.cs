using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models.Certificate;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.IO;
using System.Web.Hosting;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Areas.Certificate.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly ICertificateIssuanceService _issuanceService;
        private readonly ICertificateCorrectionService _correctionService;
        private readonly ICertificateAuditService _auditService;
        private readonly IManualCertificateService _manualCertService;
        private readonly ILogger _logger;

        public StudentController(
            ICertificateIssuanceService issuanceService,
            ICertificateCorrectionService correctionService,
            ICertificateAuditService auditService,
            IManualCertificateService manualCertService,
            ILogger logger)
        {
            _issuanceService = issuanceService;
            _correctionService = correctionService;
            _auditService = auditService;
            _manualCertService = manualCertService;
            _logger = logger;
        }

        /// <summary>
        /// Certificate Center — list all certificates for the logged-in student.
        /// </summary>
        public ActionResult Index(string search, string type, string status)
        {
            var userId = User.Identity.GetUserId();
            var vm = _issuanceService.GetStudentCertificateCenter(userId, search, type, status);

            // Also fetch manual certificates uploaded by admin
            var manualCerts = _manualCertService.GetManualCertificatesForUser(userId);
            ViewBag.ManualCertificates = manualCerts;

            return View(vm);
        }

        /// <summary>
        /// Single certificate detail with preview, download options, and share link.
        /// </summary>
        public ActionResult Detail(int id)
        {
            var userId = User.Identity.GetUserId();
            var vm = _issuanceService.GetIssueById(id);

            if (vm == null || vm.UserId != userId)
                return HttpNotFound();

            // Build absolute verification URL  
            var baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            vm.VerificationUrl = $"{baseUrl}/Certificate/Verify/{vm.PublicId}";

            // Get correction requests for this certificate
            vm.CorrectionRequests = _correctionService.GetRequestsForUser(userId);

            return View(vm);
        }

        /// <summary>
        /// Download the certificate PDF.
        /// </summary>
        public ActionResult Download(int id)
        {
            var userId = User.Identity.GetUserId();
            var issue = _issuanceService.GetIssueById(id);

            if (issue == null || issue.UserId != userId)
                return HttpNotFound();

            if (string.IsNullOrEmpty(issue.PdfPath) || issue.Status != "Issued")
                return new HttpStatusCodeResult(400, "Certificate PDF not available");

            var absPath = HostingEnvironment.MapPath($"~/{issue.PdfPath}");
            if (!System.IO.File.Exists(absPath))
                return HttpNotFound("PDF file not found");

            _auditService.Log(userId, "Downloaded", "Issue", id,
                ipAddress: Request.UserHostAddress);

            var fileName = $"Certificate_{issue.CourseName?.Replace(" ", "_")}_{issue.PublicId.Substring(0, 8)}.pdf";
            return File(absPath, "application/pdf", fileName);
        }

        /// <summary>
        /// Download a PNG image of the certificate.
        /// </summary>
        public ActionResult DownloadImage(int id)
        {
            var userId = User.Identity.GetUserId();
            var issue = _issuanceService.GetIssueById(id);

            if (issue == null || issue.UserId != userId)
                return HttpNotFound();

            if (string.IsNullOrEmpty(issue.ThumbnailPath))
                return new HttpStatusCodeResult(400, "Thumbnail not available");

            var absPath = HostingEnvironment.MapPath($"~/{issue.ThumbnailPath}");
            if (!System.IO.File.Exists(absPath))
                return HttpNotFound("Thumbnail file not found");

            var fileName = $"Certificate_{issue.PublicId.Substring(0, 8)}.png";
            return File(absPath, "image/png", fileName);
        }

        /// <summary>
        /// Email a copy of the certificate to the student.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EmailCopy(int id)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var issue = _issuanceService.GetIssueById(id);
                if (issue == null || issue.UserId != userId)
                    return Json(new { success = false, message = "Certificate not found." });

                if (string.IsNullOrEmpty(issue.LearnerEmail))
                    return Json(new { success = false, message = "No email address found." });

                var baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                var downloadLink = $"{baseUrl}/Certificate/Student/Download/{id}";
                var verifyLink = $"{baseUrl}/Certificate/Verify/{issue.PublicId}";

                var body = $@"<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>
                    <div style='background:#0F766E;color:white;padding:20px;text-align:center;'>
                        <h2>First Aid Made Easy</h2>
                    </div>
                    <div style='padding:24px;background:#f8f9fa;'>
                        <p>Dear {issue.LearnerName},</p>
                        <p>Here is your certificate for <strong>{issue.CourseName}</strong>.</p>
                        <p>Certificate ID: <code>{issue.PublicId}</code></p>
                        <p>Issued on: {issue.IssuedAt?.ToString("MMMM dd, yyyy")}</p>
                        <p style='margin-top:20px;'>
                            <a href='{downloadLink}' style='background:#0F766E;color:white;padding:10px 24px;text-decoration:none;border-radius:6px;'>Download Certificate</a>
                        </p>
                        <p style='margin-top:16px;font-size:13px;color:#666;'>
                            Verification link: <a href='{verifyLink}'>{verifyLink}</a>
                        </p>
                    </div>
                </div>";

                var msg = new First_Aid_Made_Easy.Models.MessageVM
                {
                    Destination = issue.LearnerEmail,
                    Subject = $"Your Certificate - {issue.CourseName}",
                    Body = body
                };
                Common.SendMail(msg);

                _auditService.Log(userId, "EmailCopy", "Issue", id);

                return Json(new { success = true, message = "Email sent successfully!" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "EmailCopy failed for issue {Id}", id);
                return Json(new { success = false, message = "Failed to send email." });
            }
        }

        /// <summary>
        /// Report an issue / request a correction.
        /// </summary>
        public ActionResult ReportIssue(int? issueId)
        {
            ViewBag.IssueId = issueId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitCorrection(int issueId, string requestType, string details)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                // Verify the student owns this certificate
                var issue = _issuanceService.GetIssueById(issueId);
                if (issue == null || issue.UserId != userId)
                    return Json(new { success = false, message = "Certificate not found." });

                var id = _correctionService.SubmitRequest(issueId, userId, requestType, details);
                return Json(new { success = true, id, message = "Correction request submitted. We'll review it shortly." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "SubmitCorrection failed");
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Download a manual certificate uploaded by admin.
        /// </summary>
        public ActionResult DownloadManual(int id)
        {
            var userId = User.Identity.GetUserId();
            var certs = _manualCertService.GetManualCertificatesForUser(userId);
            var cert = certs.Find(c => c.Id == id);
            if (cert == null) return HttpNotFound();

            var absPath = HostingEnvironment.MapPath("~/" + cert.FilePath);
            if (string.IsNullOrEmpty(absPath) || !System.IO.File.Exists(absPath))
                return HttpNotFound("Certificate file not found.");

            return File(absPath, "application/pdf", cert.FileName);
        }
    }
}
