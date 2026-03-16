using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using First_Aid_Made_Easy.Models.Certificate;
using Microsoft.AspNet.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Areas.Certificate.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class AdminController : Controller
    {
        private readonly ICertificateTemplateService _templateService;
        private readonly ICertificateRuleService _ruleService;
        private readonly ICertificateIssuanceService _issuanceService;
        private readonly ICertificateRenderService _renderService;
        private readonly ICertificateAuditService _auditService;
        private readonly ICertificateCorrectionService _correctionService;
        private readonly IManualCertificateService _manualCertService;
        private readonly ILogger _logger;

        public AdminController(
            ICertificateTemplateService templateService,
            ICertificateRuleService ruleService,
            ICertificateIssuanceService issuanceService,
            ICertificateRenderService renderService,
            ICertificateAuditService auditService,
            ICertificateCorrectionService correctionService,
            IManualCertificateService manualCertService,
            ILogger logger)
        {
            _templateService = templateService;
            _ruleService = ruleService;
            _issuanceService = issuanceService;
            _renderService = renderService;
            _auditService = auditService;
            _correctionService = correctionService;
            _manualCertService = manualCertService;
            _logger = logger;
        }

        #region Dashboard

        public ActionResult Index()
        {
            var vm = _templateService.GetAdminDashboard();
            return View(vm);
        }

        #endregion

        #region Template Builder

        public ActionResult Builder(int? id)
        {
            CertificateTemplateVM vm;
            if (id.HasValue && id.Value > 0)
            {
                vm = _templateService.GetTemplateForEdit(id.Value);
                if (vm == null) return HttpNotFound();
            }
            else
            {
                vm = new CertificateTemplateVM
                {
                    Orientation = "Landscape",
                    PageSize = "A4",
                    Type = "Completion",
                    LayoutJson = "{}",
                    AvailableAssets = _templateService.GetAssets()
                };
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveTemplate(CertificateTemplateVM model)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var templateId = _templateService.SaveTemplate(model, userId);
                return Json(new { success = true, id = templateId });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "SaveTemplate failed");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult PreviewTemplate(string layoutJson, string orientation, string pageSize)
        {
            try
            {
                var pdfBytes = _renderService.GeneratePreviewPdf(layoutJson, orientation, pageSize);
                if (pdfBytes == null)
                    return new HttpStatusCodeResult(500, "PDF generation failed");
                return File(pdfBytes, "application/pdf", "certificate_preview.pdf");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "PreviewTemplate failed");
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActivateTemplate(int id)
        {
            var userId = User.Identity.GetUserId();
            _templateService.UpdateTemplateStatus(id, "Active", userId);
            _templateService.CreateVersion(id, "Activated", userId);
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ArchiveTemplate(int id)
        {
            _templateService.DeleteTemplate(id, User.Identity.GetUserId());
            return Json(new { success = true });
        }

        public ActionResult Templates()
        {
            var templates = _templateService.GetAllTemplates();
            var vm = new CertificateTemplateListPageVM
            {
                Templates = templates,
                TotalPages = 1,
                CurrentPage = 1
            };
            return View(vm);
        }

        #endregion

        #region Assets

        public ActionResult Assets()
        {
            var assets = _templateService.GetAssets();
            return View(assets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadAsset(AssetUploadVM model)
        {
            if (!ModelState.IsValid || model.File == null)
                return Json(new { success = false, message = "Invalid upload" });

            try
            {
                var userId = User.Identity.GetUserId();
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.File.FileName)}";
                var relDir = "Images/Certificates/Assets";
                var absDir = Server.MapPath($"~/{relDir}");
                Directory.CreateDirectory(absDir);
                var absPath = Path.Combine(absDir, fileName);
                model.File.SaveAs(absPath);

                var relPath = $"{relDir}/{fileName}";
                var id = _templateService.SaveAsset(model.AssetType, model.Name, relPath,
                    model.SignatoryName, model.SignatoryTitle, userId);

                return Json(new { success = true, id, filePath = relPath });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "UploadAsset failed");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteAsset(int id)
        {
            _templateService.DeleteAsset(id, User.Identity.GetUserId());
            return Json(new { success = true });
        }

        [HttpGet]
        public ActionResult GetAssets(string type)
        {
            var assets = _templateService.GetAssets(type);
            return Json(assets, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Rules

        public ActionResult Rules(int? templateId)
        {
            ViewBag.Templates = _templateService.GetAllTemplates("Active");
            ViewBag.Courses = _ruleService.GetCoursesForDropdown();

            if (templateId.HasValue)
            {
                var rules = _ruleService.GetRuleSetsForTemplate(templateId.Value);
                ViewBag.SelectedTemplateId = templateId.Value;
                return View(rules);
            }

            return View(_ruleService.GetActiveRuleSets());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveRuleSet(CertificateRuleSetVM model)
        {
            try
            {
                var userId = User.Identity.GetUserId();
                var id = _ruleService.SaveRuleSet(model, userId);
                return Json(new { success = true, id });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "SaveRuleSet failed");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeactivateRuleSet(int id)
        {
            _ruleService.DeactivateRuleSet(id, User.Identity.GetUserId());
            return Json(new { success = true });
        }

        #endregion

        #region Issued Certificates

        public ActionResult Issues(string status, string search, int page = 1)
        {
            int pageSize = 20;
            var allIssues = _issuanceService.GetAllIssues(status, search, page);
            var total = _issuanceService.GetTotalIssueCount(status, search);
            var vm = new CertificateIssueListPageVM
            {
                Issues = allIssues,
                TotalPages = (int)Math.Ceiling((double)total / pageSize),
                CurrentPage = page,
                Search = search,
                StatusFilter = status
            };
            return View(vm);
        }

        public ActionResult IssueDetail(int id)
        {
            var vm = _issuanceService.GetIssueById(id);
            if (vm == null) return HttpNotFound();

            vm.AuditTrail = _auditService.GetAuditTrail("Issue", id);
            vm.CorrectionRequests = _correctionService.GetRequestsForIssue(id);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveIssue(int id)
        {
            _issuanceService.ApprovePendingIssue(id, User.Identity.GetUserId());
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectIssue(int id, string reason)
        {
            _issuanceService.RejectPendingIssue(id, reason, User.Identity.GetUserId());
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RevokeIssue(RevokeVM model)
        {
            _issuanceService.RevokeCertificate(model.IssueId, model.Reason, User.Identity.GetUserId());
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReIssue(ReIssueVM model)
        {
            try
            {
                var result = _issuanceService.ReIssueCertificate(
                    model.OriginalIssueId, model.CorrectedLearnerName,
                    model.CorrectedCredits, model.Notes, User.Identity.GetUserId());
                return Json(new { success = true, id = result.Id });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ReIssue failed");
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Corrections

        public ActionResult Corrections()
        {
            var requests = _correctionService.GetOpenRequests();
            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResolveCorrection(int id, string notes, bool approved)
        {
            _correctionService.ResolveRequest(id, User.Identity.GetUserId(), notes, approved);
            return Json(new { success = true });
        }

        #endregion

        #region Manual Certificate Upload (User List)

        /// <summary>
        /// User List page — shows enrolled users filtered by course or package.
        /// Admin can upload manual certificates for each user.
        /// </summary>
        public ActionResult UserList(string filterType, int? courseId, int? packageId)
        {
            var vm = new ManualCertificateUserListVM
            {
                Courses = _manualCertService.GetAllCourses(),
                Packages = _manualCertService.GetAllPackages(),
                FilterType = filterType,
                SelectedCourseId = courseId,
                SelectedPackageId = packageId
            };

            if (filterType == "course" && courseId.HasValue)
            {
                vm.Users = _manualCertService.GetUsersByCourse(courseId.Value);
            }
            else if (filterType == "package" && packageId.HasValue)
            {
                vm.Users = _manualCertService.GetUsersByPackage(packageId.Value);
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadManualCertificate(string userId, int? courseId, int? packageId,
            string learnerName, string title, HttpPostedFileBase certificateFile)
        {
            try
            {
                if (certificateFile == null || certificateFile.ContentLength == 0)
                    return Json(new { success = false, message = "Please select a PDF file to upload." });

                var ext = Path.GetExtension(certificateFile.FileName).ToLower();
                if (ext != ".pdf")
                    return Json(new { success = false, message = "Only PDF files are allowed." });

                if (certificateFile.ContentLength > 10 * 1024 * 1024)
                    return Json(new { success = false, message = "File size must be under 10 MB." });

                var adminId = User.Identity.GetUserId();
                var safeFileName = $"{Guid.NewGuid()}{ext}";
                var relDir = "Images/Certificates/Manual";
                var absDir = Server.MapPath($"~/{relDir}");
                Directory.CreateDirectory(absDir);
                var absPath = Path.Combine(absDir, safeFileName);
                certificateFile.SaveAs(absPath);

                var relPath = $"{relDir}/{safeFileName}";
                var id = _manualCertService.UploadManualCertificate(
                    userId, courseId, packageId, learnerName,
                    certificateFile.FileName, relPath, title, adminId);

                // Send email notification to the student
                try
                {
                    var tblUser = Common.GetUserNameByASpUserID(userId);
                    if (tblUser != null)
                    {
                        var studentEmail = tblUser.AspNetUsers?.Email;
                        if (!string.IsNullOrEmpty(studentEmail))
                        {
                            var certTitle = string.IsNullOrEmpty(title) ? "Certificate" : title;
                            var baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                            var certLink = $"{baseUrl}/Certificate/Student";
                            var body = $@"<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>
                                <div style='background:#0F766E;color:white;padding:24px;text-align:center;border-radius:8px 8px 0 0;'>
                                    <h2 style='margin:0;font-size:22px;'>First Aid Made Easy</h2>
                                    <p style='margin:6px 0 0;opacity:0.9;font-size:14px;'>Certificate Notification</p>
                                </div>
                                <div style='padding:28px;background:#ffffff;border:1px solid #e2e8f0;'>
                                    <p style='margin:0 0 16px;font-size:15px;'>Dear <strong>{learnerName}</strong>,</p>
                                    <p style='margin:0 0 16px;font-size:14px;color:#334155;'>
                                        Great news! A new certificate has been uploaded to your account:
                                    </p>
                                    <div style='background:#f0fdfa;border-left:4px solid #0F766E;padding:16px;border-radius:0 6px 6px 0;margin:0 0 20px;'>
                                        <p style='margin:0;font-size:14px;font-weight:600;color:#0F766E;'>{certTitle}</p>
                                    </div>
                                    <p style='margin:0 0 24px;font-size:14px;color:#334155;'>
                                        You can view and download your certificate from your student dashboard.
                                    </p>
                                    <p style='text-align:center;margin:0 0 16px;'>
                                        <a href='{certLink}' style='display:inline-block;background:#0F766E;color:white;padding:12px 32px;text-decoration:none;border-radius:6px;font-weight:600;font-size:14px;'>
                                            View My Certificates
                                        </a>
                                    </p>
                                </div>
                                <div style='text-align:center;padding:16px;background:#f8fafc;border:1px solid #e2e8f0;border-top:none;border-radius:0 0 8px 8px;'>
                                    <p style='margin:0;font-size:12px;color:#94a3b8;'>This is an automated notification from FAME LMS.</p>
                                </div>
                            </div>";

                            var msg = new MessageVM
                            {
                                Destination = studentEmail,
                                Subject = $"New Certificate Available - {certTitle}",
                                Body = body
                            };
                            // Fire-and-forget so admin doesn't wait for email delivery
                            var _ = Common.SendMail(msg);
                        }
                    }
                }
                catch (Exception emailEx)
                {
                    // Log but don't fail the upload if email fails
                    _logger.Warning(emailEx, "Failed to send certificate notification email to user {UserId}", userId);
                }

                return Json(new { success = true, id, message = "Certificate uploaded successfully." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "UploadManualCertificate failed for user {UserId}", userId);
                return Json(new { success = false, message = "Upload failed: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteManualCertificate(int id)
        {
            try
            {
                _manualCertService.DeleteManualCertificate(id, User.Identity.GetUserId());
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DeleteManualCertificate failed for id {Id}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Google Sheet Data

        /// <summary>
        /// Lists all saved Google Sheets the admin can browse.
        /// </summary>
        public ActionResult GoogleSheets()
        {
            var vm = new GoogleSheetListVM();
            using (var certDb = new DAL.CertificateDbContext())
            {
                vm.Sheets = certDb.GoogleSheets
                    .Where(s => s.IsActive)
                    .OrderByDescending(s => s.CreatedAt)
                    .Select(s => new GoogleSheetItemVM
                    {
                        Id = s.Id,
                        SheetName = s.SheetName,
                        SheetId = s.SheetId,
                        GId = s.GId,
                        CreatedAt = s.CreatedAt,
                        IsActive = s.IsActive
                    })
                    .ToList();
            }
            return View(vm);
        }

        /// <summary>
        /// Adds a new Google Sheet reference.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddGoogleSheet(string sheetName, string sheetUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sheetName) || string.IsNullOrWhiteSpace(sheetUrl))
                    return Json(new { success = false, message = "Sheet name and URL are required." });

                // Parse the sheet ID and gid from the URL
                // Expected URL format:
                //   https://docs.google.com/spreadsheets/d/{SHEET_ID}/edit#gid={GID}
                //   https://docs.google.com/spreadsheets/d/{SHEET_ID}/edit?...#gid={GID}
                //   Or just the sheet ID directly
                string parsedSheetId = sheetUrl.Trim();
                string parsedGid = "0";

                if (sheetUrl.Contains("docs.google.com/spreadsheets"))
                {
                    // Extract sheet ID from /d/{ID}/
                    var dIdx = sheetUrl.IndexOf("/d/");
                    if (dIdx >= 0)
                    {
                        var afterD = sheetUrl.Substring(dIdx + 3);
                        var slashIdx = afterD.IndexOf('/');
                        parsedSheetId = slashIdx > 0 ? afterD.Substring(0, slashIdx) : afterD;
                        // Remove any query/hash from sheetId
                        var qIdx = parsedSheetId.IndexOf('?');
                        if (qIdx > 0) parsedSheetId = parsedSheetId.Substring(0, qIdx);
                        var hIdx = parsedSheetId.IndexOf('#');
                        if (hIdx > 0) parsedSheetId = parsedSheetId.Substring(0, hIdx);
                    }

                    // Extract gid from #gid= or &gid= or ?gid=
                    var gidMatch = System.Text.RegularExpressions.Regex.Match(sheetUrl, @"[#&?]gid=(\d+)");
                    if (gidMatch.Success)
                        parsedGid = gidMatch.Groups[1].Value;
                }

                if (string.IsNullOrWhiteSpace(parsedSheetId))
                    return Json(new { success = false, message = "Could not parse a valid Sheet ID from the URL." });

                var adminId = User.Identity.GetUserId();

                using (var certDb = new DAL.CertificateDbContext())
                {
                    var entry = new tbl_GoogleSheet
                    {
                        SheetName = sheetName.Trim(),
                        SheetId = parsedSheetId,
                        GId = parsedGid,
                        AddedBy = adminId,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    certDb.GoogleSheets.Add(entry);
                    certDb.SaveChanges();
                }

                return Json(new { success = true, message = "Sheet added successfully!" });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "AddGoogleSheet failed");
                return Json(new { success = false, message = "Failed to add sheet: " + ex.Message });
            }
        }

        /// <summary>
        /// Soft-deletes a Google Sheet reference.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteGoogleSheet(int id)
        {
            try
            {
                using (var certDb = new DAL.CertificateDbContext())
                {
                    var sheet = certDb.GoogleSheets.Find(id);
                    if (sheet == null)
                        return Json(new { success = false, message = "Sheet not found." });

                    sheet.IsActive = false;
                    certDb.SaveChanges();
                }
                return Json(new { success = true, message = "Sheet removed." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "DeleteGoogleSheet failed for id {Id}", id);
                return Json(new { success = false, message = "Failed to delete: " + ex.Message });
            }
        }

        /// <summary>
        /// Fetches data from a specific Google Sheet by its DB id.
        /// </summary>
        public ActionResult GoogleSheetData(int? id)
        {
            if (!id.HasValue)
                return RedirectToAction("GoogleSheets");

            var vm = new GoogleSheetDataVM();
            try
            {
                string sheetId, gid, sheetName;
                using (var certDb = new DAL.CertificateDbContext())
                {
                    var sheet = certDb.GoogleSheets.Find(id.Value);
                    if (sheet == null || !sheet.IsActive)
                    {
                        vm.ErrorMessage = "Sheet not found or has been removed.";
                        return View(vm);
                    }
                    sheetId = sheet.SheetId;
                    gid = sheet.GId;
                    sheetName = sheet.SheetName;
                    vm.SheetDbId = sheet.Id;
                    vm.SheetName = sheetName;
                }

                var csvUrl = $"https://docs.google.com/spreadsheets/d/{sheetId}/gviz/tq?tqx=out:csv&gid={gid}";

                string csvContent;
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                using (var wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    try
                    {
                        csvContent = wc.DownloadString(csvUrl);
                    }
                    catch
                    {
                        // Fallback to /export format (works for "Published to web" sheets)
                        var fallbackUrl = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={gid}";
                        csvContent = wc.DownloadString(fallbackUrl);
                    }
                }

                var rows = ParseCsv(csvContent);
                if (rows.Count > 0)
                {
                    vm.Headers = rows[0];
                    vm.Rows = rows.Skip(1).ToList();
                }

                vm.FetchedAt = DateTime.Now;
                vm.TotalRecords = vm.Rows.Count;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to fetch Google Sheet data for id {Id}", id);
                vm.ErrorMessage = "Failed to fetch data from Google Sheet: " + ex.Message;
            }

            return View(vm);
        }

        /// <summary>
        /// Simple RFC 4180 CSV parser that handles quoted fields with commas and newlines.
        /// </summary>
        private List<List<string>> ParseCsv(string csvText)
        {
            var results = new List<List<string>>();
            var currentRow = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;
            int i = 0;

            while (i < csvText.Length)
            {
                char c = csvText[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // Check for escaped quote ""
                        if (i + 1 < csvText.Length && csvText[i + 1] == '"')
                        {
                            currentField.Append('"');
                            i += 2;
                        }
                        else
                        {
                            inQuotes = false;
                            i++;
                        }
                    }
                    else
                    {
                        currentField.Append(c);
                        i++;
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                        i++;
                    }
                    else if (c == ',')
                    {
                        currentRow.Add(currentField.ToString());
                        currentField.Clear();
                        i++;
                    }
                    else if (c == '\r' || c == '\n')
                    {
                        currentRow.Add(currentField.ToString());
                        currentField.Clear();
                        if (currentRow.Any(f => !string.IsNullOrEmpty(f)))
                            results.Add(currentRow);
                        currentRow = new List<string>();
                        // Handle \r\n
                        if (c == '\r' && i + 1 < csvText.Length && csvText[i + 1] == '\n')
                            i += 2;
                        else
                            i++;
                    }
                    else
                    {
                        currentField.Append(c);
                        i++;
                    }
                }
            }

            // Last field/row
            currentRow.Add(currentField.ToString());
            if (currentRow.Any(f => !string.IsNullOrEmpty(f)))
                results.Add(currentRow);

            return results;
        }

        /// <summary>
        /// Upload a certificate by looking up the student via email (used from Google Sheet data).
        /// Same flow as UploadManualCertificate but resolves userId from email.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadCertificateByEmail(string email, string learnerName, string title,
            HttpPostedFileBase certificateFile)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return Json(new { success = false, message = "Email address is required." });

                if (certificateFile == null || certificateFile.ContentLength == 0)
                    return Json(new { success = false, message = "Please select a PDF file to upload." });

                var ext = Path.GetExtension(certificateFile.FileName).ToLower();
                if (ext != ".pdf")
                    return Json(new { success = false, message = "Only PDF files are allowed." });

                if (certificateFile.ContentLength > 10 * 1024 * 1024)
                    return Json(new { success = false, message = "File size must be under 10 MB." });

                // Look up user by email in AspNetUsers
                string userId = null;
                using (var fameDb = new DAL.FAMEEntities())
                {
                    var trimmedEmail = email.Trim().ToLower();
                    var user = fameDb.AspNetUsers
                        .FirstOrDefault(u => u.Email.ToLower() == trimmedEmail);
                    if (user != null)
                        userId = user.Id;
                }

                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "No registered student found with email: " + email + ". The student must have an account in the LMS first." });

                var adminId = User.Identity.GetUserId();
                var safeFileName = $"{Guid.NewGuid()}{ext}";
                var relDir = "Images/Certificates/Manual";
                var absDir = Server.MapPath($"~/{relDir}");
                Directory.CreateDirectory(absDir);
                var absPath = Path.Combine(absDir, safeFileName);
                certificateFile.SaveAs(absPath);

                var relPath = $"{relDir}/{safeFileName}";
                var id = _manualCertService.UploadManualCertificate(
                    userId, null, null, learnerName,
                    certificateFile.FileName, relPath, title, adminId);

                // Send email notification to the student
                try
                {
                    var certTitle = string.IsNullOrEmpty(title) ? "Certificate" : title;
                    var baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
                    var certLink = $"{baseUrl}/Certificate/Student";
                    var body = $@"<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;'>
                        <div style='background:#0F766E;color:white;padding:24px;text-align:center;border-radius:8px 8px 0 0;'>
                            <h2 style='margin:0;font-size:22px;'>First Aid Made Easy</h2>
                            <p style='margin:6px 0 0;opacity:0.9;font-size:14px;'>Certificate Notification</p>
                        </div>
                        <div style='padding:28px;background:#ffffff;border:1px solid #e2e8f0;'>
                            <p style='margin:0 0 16px;font-size:15px;'>Dear <strong>{learnerName}</strong>,</p>
                            <p style='margin:0 0 16px;font-size:14px;color:#334155;'>
                                Great news! A new certificate has been uploaded to your account:
                            </p>
                            <div style='background:#f0fdfa;border-left:4px solid #0F766E;padding:16px;border-radius:0 6px 6px 0;margin:0 0 20px;'>
                                <p style='margin:0;font-size:14px;font-weight:600;color:#0F766E;'>{certTitle}</p>
                            </div>
                            <p style='margin:0 0 24px;font-size:14px;color:#334155;'>
                                You can view and download your certificate from your student dashboard.
                            </p>
                            <p style='text-align:center;margin:0 0 16px;'>
                                <a href='{certLink}' style='display:inline-block;background:#0F766E;color:white;padding:12px 32px;text-decoration:none;border-radius:6px;font-weight:600;font-size:14px;'>
                                    View My Certificates
                                </a>
                            </p>
                        </div>
                        <div style='text-align:center;padding:16px;background:#f8fafc;border:1px solid #e2e8f0;border-top:none;border-radius:0 0 8px 8px;'>
                            <p style='margin:0;font-size:12px;color:#94a3b8;'>This is an automated notification from FAME LMS.</p>
                        </div>
                    </div>";

                    var msg = new MessageVM
                    {
                        Destination = email.Trim(),
                        Subject = $"New Certificate Available - {certTitle}",
                        Body = body
                    };
                    var _ = Common.SendMail(msg);
                }
                catch (Exception emailEx)
                {
                    _logger.Warning(emailEx, "Failed to send certificate email to {Email}", email);
                }

                return Json(new { success = true, id, message = "Certificate uploaded successfully! Student will be notified by email." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "UploadCertificateByEmail failed for {Email}", email);
                return Json(new { success = false, message = "Upload failed: " + ex.Message });
            }
        }

        #endregion
    }
}
