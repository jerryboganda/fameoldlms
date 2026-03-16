using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using Newtonsoft.Json;

namespace First_Aid_Made_Easy.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class CommunicationController : Controller
    {
        private readonly IEmailMarketingRepository _emailRepo;
        private readonly IUserRepository _userRepository;

        public CommunicationController(IEmailMarketingRepository emailRepo, IUserRepository userRepository)
        {
            _emailRepo = emailRepo;
            _userRepository = userRepository;
        }

        // ── Campaign Dashboard ───────────────────────────────────
        public ActionResult Index()
        {
            var model = _emailRepo.GetDashboard();
            return View(model);
        }

        // ── Create Campaign (Wizard) ─────────────────────────────
        public ActionResult Create(int? templateId)
        {
            var model = new CampaignCreateVM();
            PopulateDropdowns(model);

            if (templateId.HasValue)
            {
                var template = _emailRepo.GetTemplate(templateId.Value);
                if (template != null)
                {
                    model.TemplateId = template.Id;
                    model.HtmlBody = template.HtmlBody;
                    if (!string.IsNullOrEmpty(template.Subject))
                        model.Subject = template.Subject;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CampaignCreateVM model)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var id = _emailRepo.SaveCampaign(model, userId);
                if (id > 0)
                    return Json(new { success = true, id = id });
                return Json(new { success = false, message = "Failed to save campaign" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error while saving campaign: " + ex.Message });
            }
        }

        // ── Edit Campaign ────────────────────────────────────────
        public ActionResult Edit(int id)
        {
            var model = _emailRepo.GetCampaignForEdit(id);
            if (model == null) return HttpNotFound();
            return View("Create", model);
        }

        // ── Send Campaign Now ────────────────────────────────────
        [HttpPost]
        public async Task<ActionResult> SendNow(int id)
        {
            try
            {
                var result = await _emailRepo.SendCampaignAsync(id);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error while starting campaign send: " + ex.Message });
            }
        }

        // ── Schedule Campaign ────────────────────────────────────
        [HttpPost]
        public ActionResult Schedule(int id, DateTime scheduledAt)
        {
            using (var db = new DAL.EmailMarketingDbContext())
            {
                var campaign = db.tbl_EmailCampaigns.Find(id);
                if (campaign == null) return Json(new { success = false });
                campaign.ScheduledAt = scheduledAt;
                campaign.Status = "Scheduled";
                campaign.UpdatedAt = DateTime.Now;
                db.SaveChanges();
            }
            return Json(new { success = true });
        }

        // ── Pause / Resume ───────────────────────────────────────
        [HttpPost]
        public ActionResult Pause(int id)
        {
            return Json(new { success = _emailRepo.PauseCampaign(id) });
        }

        [HttpPost]
        public ActionResult Resume(int id)
        {
            return Json(new { success = _emailRepo.ResumeCampaign(id) });
        }

        // ── Delete Campaign ──────────────────────────────────────
        [HttpPost]
        public ActionResult Delete(int id)
        {
            return Json(new { success = _emailRepo.DeleteCampaign(id) });
        }

        // ── Reset stuck campaign back to Draft ───────────────────
        [HttpPost]
        public ActionResult ResetToDraft(int id)
        {
            using (var db = new DAL.EmailMarketingDbContext())
            {
                var campaign = db.tbl_EmailCampaigns.Find(id);
                if (campaign == null) return Json(new { success = false, message = "Campaign not found" });
                if (campaign.Status != "Sending" && campaign.Status != "Failed")
                    return Json(new { success = false, message = "Only stuck/failed campaigns can be reset" });

                // Clean up any existing recipient records so send can start fresh
                var recipients = db.tbl_EmailCampaignRecipients.Where(r => r.CampaignId == id).ToList();
                db.tbl_EmailCampaignRecipients.RemoveRange(recipients);

                var links = db.tbl_EmailCampaignLinks.Where(l => l.CampaignId == id).ToList();
                db.tbl_EmailCampaignLinks.RemoveRange(links);

                campaign.Status = "Draft";
                campaign.SendStartedAt = null;
                campaign.CompletedAt = null;
                campaign.TotalRecipients = 0;
                campaign.TotalSent = 0;
                campaign.TotalFailed = 0;
                campaign.TotalOpens = 0;
                campaign.TotalClicks = 0;
                campaign.UniqueOpens = 0;
                campaign.UniqueClicks = 0;
                campaign.TotalBounces = 0;
                campaign.TotalUnsubscribes = 0;
                campaign.UpdatedAt = DateTime.Now;
                db.SaveChanges();
            }
            return Json(new { success = true });
        }

        // ── Duplicate Campaign ───────────────────────────────────
        [HttpPost]
        public ActionResult Duplicate(int id)
        {
            var userId = User.Identity.GetUserId();
            var newId = _emailRepo.DuplicateCampaign(id, userId);
            return Json(new { success = newId > 0, id = newId });
        }

        // ── Campaign Stats ───────────────────────────────────────
        public ActionResult Stats(int id, int page = 1)
        {
            var model = _emailRepo.GetCampaignStats(id, page);
            if (model == null) return HttpNotFound();
            return View(model);
        }

        // ── Campaign Recipients (AJAX) ───────────────────────────
        [HttpGet]
        public ActionResult Recipients(int id, string status, int page = 1)
        {
            var recipients = _emailRepo.GetCampaignRecipients(id, status, page);
            return Json(recipients, JsonRequestBehavior.AllowGet);
        }

        // ── Send Test Email ──────────────────────────────────────
        [HttpPost]
        public async Task<ActionResult> SendTest(int id, string email)
        {
            try
            {
                var result = await _emailRepo.SendTestEmailAsync(id, email);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── Quick Test Email (no campaign needed) ───────────────
        [HttpPost]
        public async Task<ActionResult> QuickTestEmail(int senderId, string email, string subject, string htmlBody)
        {
            try
            {
                var result = await _emailRepo.SendQuickTestEmail(senderId, email, subject, htmlBody);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── Resend to Non-Openers ────────────────────────────────
        [HttpPost]
        public async Task<ActionResult> ResendNonOpeners(int id, string newSubject)
        {
            var result = await _emailRepo.ResendToNonOpenersAsync(id, newSubject);
            return Json(new { success = result });
        }

        // ── Audience Preview (AJAX) ──────────────────────────────
        [HttpPost]
        public ActionResult PreviewAudience(AudienceFilterVM filter)
        {
            var preview = _emailRepo.PreviewAudience(filter);
            return Json(preview);
        }

        [HttpGet]
        public ActionResult PreviewAudienceById(int id)
        {
            var audience = _emailRepo.GetAudience(id);
            if (audience == null) return Json(new { TotalCount = 0 }, JsonRequestBehavior.AllowGet);
            var preview = _emailRepo.PreviewAudience(audience.Filter);
            return Json(preview, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult PreviewByPackage(int? packageId, string enrollmentStatus = "All")
        {
            try
            {
                var recipients = _emailRepo.GetRecipientsByPackage(packageId, enrollmentStatus);
                return Json(new
                {
                    TotalCount = recipients.Count,
                    SampleRecipients = recipients.Take(10).ToList()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    TotalCount = 0,
                    SampleRecipients = new List<AudienceRecipientVM>(),
                    message = "Preview failed: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult PreviewByCourse(int? courseId, string enrollmentStatus = "All")
        {
            try
            {
                var recipients = _emailRepo.GetRecipientsByCourse(courseId, enrollmentStatus);
                return Json(new
                {
                    TotalCount = recipients.Count,
                    SampleRecipients = recipients.Take(10).ToList()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    TotalCount = 0,
                    SampleRecipients = new List<AudienceRecipientVM>(),
                    message = "Preview failed: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // ── Templates Page ───────────────────────────────────────
        public ActionResult Templates(string category)
        {
            var templates = _emailRepo.GetTemplates(category);
            var categories = templates.Select(t => t.Category).Distinct().ToList();
            var model = new EmailTemplateListVM { Templates = templates, Categories = categories };
            return View(model);
        }

        public ActionResult TemplateEditor(int? id)
        {
            var model = id.HasValue ? _emailRepo.GetTemplate(id.Value) : new EmailTemplateVM();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveTemplate(EmailTemplateVM model)
        {
            var userId = User.Identity.GetUserId();
            var id = _emailRepo.SaveTemplate(model, userId);
            return Json(new { success = id > 0, id = id });
        }

        [HttpPost]
        public ActionResult DeleteTemplate(int id)
        {
            return Json(new { success = _emailRepo.DeleteTemplate(id) });
        }

        [HttpGet]
        public ActionResult GetTemplateHtml(int id)
        {
            var template = _emailRepo.GetTemplate(id);
            if (template == null) return Json(new { html = "" }, JsonRequestBehavior.AllowGet);
            return Json(new { html = template.HtmlBody, subject = template.Subject }, JsonRequestBehavior.AllowGet);
        }

        // ── Audiences Page ───────────────────────────────────────
        public ActionResult Audiences()
        {
            var model = new AudienceListVM { Segments = _emailRepo.GetAudiences() };
            return View(model);
        }

        [HttpGet]
        public ActionResult GetAudience(int id)
        {
            var audience = _emailRepo.GetAudience(id);
            if (audience == null) return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            return Json(audience, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveAudience(AudienceSegmentVM model)
        {
            var userId = User.Identity.GetUserId();
            var id = _emailRepo.SaveAudience(model, userId);
            return Json(new { success = id > 0, id = id });
        }

        [HttpPost]
        public ActionResult DeleteAudience(int id)
        {
            return Json(new { success = _emailRepo.DeleteAudience(id) });
        }

        // ── Sender Accounts (Admin only) ─────────────────────────
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Senders()
        {
            var userId = User.Identity.GetUserId();

            // Auto-import existing SMTP/Brevo config from tbl_Settings on first visit
            _emailRepo.ImportExistingSmtpSettings(userId);

            // Auto-sync from Brevo if we have an API key stored
            var senders = _emailRepo.GetSenderAccounts();
            var brevoSender = senders.FirstOrDefault(s => s.UseApi && !string.IsNullOrWhiteSpace(s.ApiKey));
            if (brevoSender != null)
            {
                try
                {
                    await _emailRepo.SyncFromBrevoApi(brevoSender.ApiKey, userId);
                    senders = _emailRepo.GetSenderAccounts(); // Refresh after sync
                }
                catch { /* Silently continue if Brevo API is unreachable */ }
            }

            var model = new SenderAccountListVM { Accounts = senders };
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> SyncFromBrevo(string apiKey)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var results = await _emailRepo.SyncFromBrevoApi(apiKey, userId);
                return Json(new { success = true, count = results.Count, message = results.Count + " sender(s) synced from Brevo." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Sync failed: " + ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSender(SenderAccountVM model)
        {
            var userId = User.Identity.GetUserId();
            var id = _emailRepo.SaveSenderAccount(model, userId);
            return Json(new { success = id > 0, id = id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult DeleteSender(int id)
        {
            return Json(new { success = _emailRepo.DeleteSenderAccount(id) });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult SetDefaultSender(int id)
        {
            return Json(new { success = _emailRepo.SetDefaultSenderAccount(id) });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult> TestSender(int id, string email)
        {
            try
            {
                var result = await _emailRepo.TestSenderAccount(id, email);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult GetSender(int id)
        {
            var account = _emailRepo.GetSenderAccount(id);
            if (account == null) return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            return Json(new { success = true, data = account }, JsonRequestBehavior.AllowGet);
        }

        // ── Drip Campaigns ───────────────────────────────────────
        public ActionResult Drips()
        {
            var model = new DripCampaignListVM { DripCampaigns = _emailRepo.GetDripCampaigns() };
            return View(model);
        }

        public ActionResult DripEditor(int? id)
        {
            var model = id.HasValue ? _emailRepo.GetDripCampaign(id.Value) : new DripCampaignVM();
            if (model == null) model = new DripCampaignVM();

            // Populate dropdowns
            model.SenderAccounts = _emailRepo.GetSenderAccounts()
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.AccountName + " (" + a.FromEmail + ")" }).ToList();
            model.Templates = _emailRepo.GetTemplates()
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.TemplateName }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveDrip(DripCampaignVM model)
        {
            var userId = User.Identity.GetUserId();
            var id = _emailRepo.SaveDripCampaign(model, userId);
            return Json(new { success = id > 0, id = id });
        }

        [HttpPost]
        public ActionResult ToggleDrip(int id)
        {
            return Json(new { success = _emailRepo.ToggleDripCampaign(id) });
        }

        [HttpPost]
        public ActionResult DeleteDrip(int id)
        {
            return Json(new { success = _emailRepo.DeleteDripCampaign(id) });
        }

        // ── Unsubscribe Report ───────────────────────────────────
        public ActionResult Unsubscribes(int page = 1)
        {
            var model = _emailRepo.GetUnsubscribeReport(page);
            return View(model);
        }

        // ── Legacy Bulk Send (backward compatible) ───────────────
        [HttpPost]
        public async Task<ActionResult> SendEmail(MessageVM model)
        {
            var list = _userRepository.GetEmailsByRole("Student");
            await Common.SendMail(new MessageVM
            {
                Body = model.Body,
                Subject = model.Subject,
                Destinations = list
            });
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        // ── Private Helpers ──────────────────────────────────────
        private void PopulateDropdowns(CampaignCreateVM model)
        {
            var accounts = _emailRepo.GetSenderAccounts();
            model.SenderAccounts = accounts.Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.AccountName + " (" + a.FromEmail + ")",
                Selected = a.IsDefault
            }).ToList();

            if (accounts.Any(a => a.IsDefault))
                model.FromAccountId = accounts.First(a => a.IsDefault).Id;

            model.Templates = _emailRepo.GetTemplates().Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.TemplateName + " [" + t.Category + "]"
            }).ToList();
            model.Templates.Insert(0, new SelectListItem { Value = "", Text = "-- No Template (Blank) --" });

            model.Audiences = _emailRepo.GetAudiences().Select(a => new SelectListItem
            {
                Value = a.Id.ToString(),
                Text = a.SegmentName + " (" + a.RecipientCount + " recipients)"
            }).ToList();
            model.Audiences.Insert(0, new SelectListItem { Value = "", Text = "-- Select Audience --" });

            // Package & Course dropdowns
            model.PackagesList = _emailRepo.GetPackagesDropdown();
            model.CoursesList = _emailRepo.GetCoursesDropdown();
        }
    }
}
