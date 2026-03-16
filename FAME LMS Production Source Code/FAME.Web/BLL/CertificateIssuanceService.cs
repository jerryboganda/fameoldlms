using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models.Certificate;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    public class CertificateIssuanceService : ICertificateIssuanceService
    {
        private readonly ICertificateRenderService _renderService;
        private readonly ICertificateAuditService _auditService;
        private readonly ILogger _logger;

        public CertificateIssuanceService(
            ICertificateRenderService renderService,
            ICertificateAuditService auditService,
            ILogger logger)
        {
            _renderService = renderService;
            _auditService = auditService;
            _logger = logger;
        }

        #region Issue

        public CertificateIssueVM IssueCertificate(string userId, int ruleSetId, string issuedBy = null)
        {
            using (var db = new CertificateDbContext())
            {
                var ruleSet = db.CertificateRuleSets
                    .Include(r => r.Template)
                    .FirstOrDefault(r => r.Id == ruleSetId);
                if (ruleSet == null) throw new InvalidOperationException("RuleSet not found.");

                var template = ruleSet.Template;
                if (template == null) throw new InvalidOperationException("Template not found.");

                // Get the latest version
                var version = db.CertificateTemplateVersions
                    .Where(v => v.TemplateId == template.Id)
                    .OrderByDescending(v => v.Version)
                    .FirstOrDefault();
                if (version == null) throw new InvalidOperationException("No template version found.");

                // Get student info
                var user = db.AspNetUsers.Find(userId);
                if (user == null) throw new InvalidOperationException("User not found.");

                // Get course name from FAME entities
                string courseName = null;
                if (ruleSet.CourseId.HasValue)
                {
                    using (var fameDb = new FAMEEntities())
                    {
                        courseName = fameDb.tbl_Courses
                            .Where(c => c.Course_Id == ruleSet.CourseId.Value)
                            .Select(c => c.Course_Name)
                            .FirstOrDefault();
                    }
                }

                var publicId = Guid.NewGuid().ToString("D");
                var learnerName = user.UserName ?? "Student";

                // Evaluate eligibility for snapshot
                CriteriaSnapshot snapshot = null;
                try
                {
                    var ruleService = new CertificateRuleService(
                        _auditService, _logger);
                    var eligibility = ruleService.EvaluateEligibility(userId, ruleSet.CourseId ?? 0, ruleSet);
                    snapshot = new CriteriaSnapshot
                    {
                        RuleSetId = ruleSet.Id,
                        TemplateVersion = version.Version,
                        RuleOperator = JsonConvert.DeserializeObject<RuleDefinition>(ruleSet.RuleJson)?.Operator ?? "AND",
                        Conditions = eligibility.MetConditions.Concat(eligibility.UnmetConditions).ToList(),
                        EvaluatedAt = DateTime.Now
                    };
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Failed to create criteria snapshot for user {UserId}", userId);
                }

                var issuanceMode = ruleSet.IssuanceMode;
                var status = issuanceMode == "Manual" ? "PendingApproval" : "Pending";

                // Create issue record
                var issue = new tbl_CertificateIssue
                {
                    PublicId = publicId,
                    UserId = userId,
                    TemplateId = template.Id,
                    TemplateVersionId = version.Id,
                    RuleSetId = ruleSet.Id,
                    CourseId = ruleSet.CourseId,
                    CourseName = courseName,
                    LearnerName = learnerName,
                    Status = status,
                    CriteriaSnapshotJson = snapshot != null ? JsonConvert.SerializeObject(snapshot) : null,
                    PrivacyLevel = "FullName",
                    IssuedBy = issuedBy,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                db.CertificateIssues.Add(issue);
                db.SaveChanges();

                // If auto-issue, generate PDF immediately
                if (status == "Pending")
                {
                    try
                    {
                        GenerateAndSavePdf(issue, version, db);
                        issue.Status = "Issued";
                        issue.IssuedAt = DateTime.Now;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "PDF generation failed for issue {IssueId}", issue.Id);
                        issue.Status = "RenderFailed";
                    }
                    db.Entry(issue).State = EntityState.Modified;
                    db.SaveChanges();
                }

                _auditService.Log(issuedBy ?? "SYSTEM", "Issued", "Issue", issue.Id,
                    details: $"Certificate issued for {learnerName}, course: {courseName}, status: {issue.Status}");

                return MapToVM(issue, db);
            }
        }

        public CertificateIssueVM IssueCertificateManual(string userId, int templateId, int courseId, string issuedBy)
        {
            using (var db = new CertificateDbContext())
            {
                var template = db.CertificateTemplates.Find(templateId);
                if (template == null) throw new InvalidOperationException("Template not found.");

                var version = db.CertificateTemplateVersions
                    .Where(v => v.TemplateId == templateId)
                    .OrderByDescending(v => v.Version)
                    .FirstOrDefault();
                if (version == null) throw new InvalidOperationException("No template version found.");

                var user = db.AspNetUsers.Find(userId);
                if (user == null) throw new InvalidOperationException("User not found.");

                string courseName = null;
                using (var fameDb = new FAMEEntities())
                {
                    courseName = fameDb.tbl_Courses
                        .Where(c => c.Course_Id == courseId)
                        .Select(c => c.Course_Name)
                        .FirstOrDefault();
                }

                var publicId = Guid.NewGuid().ToString("D");
                var learnerName = user.UserName ?? "Student";

                var issue = new tbl_CertificateIssue
                {
                    PublicId = publicId,
                    UserId = userId,
                    TemplateId = templateId,
                    TemplateVersionId = version.Id,
                    CourseId = courseId,
                    CourseName = courseName,
                    LearnerName = learnerName,
                    Status = "Pending",
                    PrivacyLevel = "FullName",
                    IssuedBy = issuedBy,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                db.CertificateIssues.Add(issue);
                db.SaveChanges();

                try
                {
                    GenerateAndSavePdf(issue, version, db);
                    issue.Status = "Issued";
                    issue.IssuedAt = DateTime.Now;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "PDF generation failed for manual issue {IssueId}", issue.Id);
                    issue.Status = "RenderFailed";
                }
                db.Entry(issue).State = EntityState.Modified;
                db.SaveChanges();

                _auditService.Log(issuedBy, "Issued", "Issue", issue.Id,
                    details: $"Manual certificate issued for {learnerName}");

                return MapToVM(issue, db);
            }
        }

        #endregion

        #region Query

        public CertificateIssueVM GetIssueById(int id)
        {
            using (var db = new CertificateDbContext())
            {
                var issue = db.CertificateIssues
                    .Include(i => i.Template)
                    .Include(i => i.TemplateVersion)
                    .FirstOrDefault(i => i.Id == id);
                if (issue == null) return null;
                return MapToVM(issue, db);
            }
        }

        public CertificateIssueVM GetIssueByPublicId(string publicId)
        {
            using (var db = new CertificateDbContext())
            {
                var issue = db.CertificateIssues
                    .Include(i => i.Template)
                    .Include(i => i.TemplateVersion)
                    .FirstOrDefault(i => i.PublicId == publicId);
                if (issue == null) return null;
                return MapToVM(issue, db);
            }
        }

        public List<CertificateIssueListVM> GetIssuesForStudent(string userId, string statusFilter = null, string typeFilter = null, string search = null)
        {
            using (var db = new CertificateDbContext())
            {
                var query = db.CertificateIssues
                    .Include(i => i.Template)
                    .Where(i => i.UserId == userId);

                if (!string.IsNullOrEmpty(statusFilter))
                    query = query.Where(i => i.Status == statusFilter);
                if (!string.IsNullOrEmpty(typeFilter))
                    query = query.Where(i => i.Template.Type == typeFilter);
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(i => i.CourseName.Contains(search) || i.LearnerName.Contains(search));

                return query.OrderByDescending(i => i.CreatedAt)
                    .Select(i => new CertificateIssueListVM
                    {
                        Id = i.Id,
                        PublicId = i.PublicId,
                        LearnerName = i.LearnerName,
                        CourseName = i.CourseName,
                        TemplateName = i.Template.Name,
                        Status = i.Status,
                        IssuedAt = i.IssuedAt,
                        ThumbnailPath = i.ThumbnailPath
                    }).ToList();
            }
        }

        public List<CertificateIssueListVM> GetAllIssues(string statusFilter = null, string search = null, int page = 1, int pageSize = 20)
        {
            using (var db = new CertificateDbContext())
            {
                var query = db.CertificateIssues
                    .Include(i => i.Template)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(statusFilter))
                    query = query.Where(i => i.Status == statusFilter);
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(i => i.CourseName.Contains(search) || i.LearnerName.Contains(search) || i.PublicId.Contains(search));

                return query.OrderByDescending(i => i.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(i => new CertificateIssueListVM
                    {
                        Id = i.Id,
                        PublicId = i.PublicId,
                        LearnerName = i.LearnerName,
                        CourseName = i.CourseName,
                        TemplateName = i.Template.Name,
                        Status = i.Status,
                        IssuedAt = i.IssuedAt,
                        ThumbnailPath = i.ThumbnailPath
                    }).ToList();
            }
        }

        public int GetTotalIssueCount(string statusFilter = null, string search = null)
        {
            using (var db = new CertificateDbContext())
            {
                var query = db.CertificateIssues.AsQueryable();
                if (!string.IsNullOrEmpty(statusFilter))
                    query = query.Where(i => i.Status == statusFilter);
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(i => i.CourseName.Contains(search) || i.LearnerName.Contains(search));
                return query.Count();
            }
        }

        #endregion

        #region Actions

        public void ApprovePendingIssue(int issueId, string approvedBy)
        {
            using (var db = new CertificateDbContext())
            {
                var issue = db.CertificateIssues.Find(issueId);
                if (issue == null || issue.Status != "PendingApproval") return;

                var version = db.CertificateTemplateVersions.Find(issue.TemplateVersionId);
                if (version == null) return;

                try
                {
                    GenerateAndSavePdf(issue, version, db);
                    issue.Status = "Issued";
                    issue.IssuedAt = DateTime.Now;
                    issue.IssuedBy = approvedBy;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "PDF generation failed during approval for issue {IssueId}", issueId);
                    issue.Status = "RenderFailed";
                }

                issue.UpdatedAt = DateTime.Now;
                db.Entry(issue).State = EntityState.Modified;
                db.SaveChanges();

                _auditService.Log(approvedBy, "Approved", "Issue", issueId,
                    details: $"Certificate approved for {issue.LearnerName}");
            }
        }

        public void RejectPendingIssue(int issueId, string reason, string rejectedBy)
        {
            using (var db = new CertificateDbContext())
            {
                var issue = db.CertificateIssues.Find(issueId);
                if (issue == null || issue.Status != "PendingApproval") return;

                issue.Status = "Rejected";
                issue.RevocationReason = reason;
                issue.UpdatedAt = DateTime.Now;
                db.Entry(issue).State = EntityState.Modified;
                db.SaveChanges();

                _auditService.Log(rejectedBy, "Rejected", "Issue", issueId,
                    details: $"Certificate rejected for {issue.LearnerName}. Reason: {reason}");
            }
        }

        public void RevokeCertificate(int issueId, string reason, string revokedBy)
        {
            using (var db = new CertificateDbContext())
            {
                var issue = db.CertificateIssues.Find(issueId);
                if (issue == null) return;

                issue.Status = "Revoked";
                issue.RevokedAt = DateTime.Now;
                issue.RevocationReason = reason;
                issue.UpdatedAt = DateTime.Now;
                db.Entry(issue).State = EntityState.Modified;
                db.SaveChanges();

                _auditService.Log(revokedBy, "Revoked", "Issue", issueId,
                    details: $"Certificate revoked for {issue.LearnerName}. Reason: {reason}");
            }
        }

        public CertificateIssueVM ReIssueCertificate(int originalIssueId, string correctedName, decimal? correctedCredits, string notes, string reIssuedBy)
        {
            using (var db = new CertificateDbContext())
            {
                var original = db.CertificateIssues
                    .Include(i => i.Template)
                    .FirstOrDefault(i => i.Id == originalIssueId);
                if (original == null) throw new InvalidOperationException("Original issue not found.");

                // Mark old as superseded
                original.Status = "Superseded";
                original.UpdatedAt = DateTime.Now;
                db.Entry(original).State = EntityState.Modified;

                // Get latest version
                var version = db.CertificateTemplateVersions
                    .Where(v => v.TemplateId == original.TemplateId)
                    .OrderByDescending(v => v.Version)
                    .FirstOrDefault();

                var publicId = Guid.NewGuid().ToString("D");

                var newIssue = new tbl_CertificateIssue
                {
                    PublicId = publicId,
                    UserId = original.UserId,
                    TemplateId = original.TemplateId,
                    TemplateVersionId = version?.Id ?? original.TemplateVersionId,
                    RuleSetId = original.RuleSetId,
                    CourseId = original.CourseId,
                    CourseName = original.CourseName,
                    LearnerName = !string.IsNullOrEmpty(correctedName) ? correctedName : original.LearnerName,
                    CreditsAwarded = correctedCredits ?? original.CreditsAwarded,
                    Status = "Pending",
                    CriteriaSnapshotJson = original.CriteriaSnapshotJson,
                    PrivacyLevel = original.PrivacyLevel,
                    IssuedBy = reIssuedBy,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                db.CertificateIssues.Add(newIssue);
                db.SaveChanges();

                try
                {
                    GenerateAndSavePdf(newIssue, version ?? db.CertificateTemplateVersions.Find(original.TemplateVersionId), db);
                    newIssue.Status = "Issued";
                    newIssue.IssuedAt = DateTime.Now;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "PDF generation failed for re-issue {IssueId}", newIssue.Id);
                    newIssue.Status = "RenderFailed";
                }

                db.Entry(newIssue).State = EntityState.Modified;
                db.SaveChanges();

                _auditService.Log(reIssuedBy, "ReIssued", "Issue", newIssue.Id,
                    details: $"Re-issued from #{originalIssueId}. Notes: {notes}");

                return MapToVM(newIssue, db);
            }
        }

        #endregion

        #region Student Certificate Center

        public StudentCertificateCenterVM GetStudentCertificateCenter(string userId, string search = null, string typeFilter = null, string statusFilter = null)
        {
            using (var db = new CertificateDbContext())
            {
                var query = db.CertificateIssues
                    .Include(i => i.Template)
                    .Where(i => i.UserId == userId && i.Status != "Superseded" && i.Status != "RenderFailed");

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(i => i.CourseName.Contains(search) || i.LearnerName.Contains(search));
                if (!string.IsNullOrEmpty(typeFilter))
                    query = query.Where(i => i.Template.Type == typeFilter);
                if (!string.IsNullOrEmpty(statusFilter))
                    query = query.Where(i => i.Status == statusFilter);

                var issues = query.OrderByDescending(i => i.CreatedAt).ToList();

                return new StudentCertificateCenterVM
                {
                    TotalCount = issues.Count,
                    ReadyCount = issues.Count(i => i.Status == "Issued"),
                    PendingCount = issues.Count(i => i.Status == "Pending" || i.Status == "PendingApproval"),
                    Search = search,
                    TypeFilter = typeFilter,
                    StatusFilter = statusFilter,
                    Certificates = issues.Select(i => new StudentCertificateCardVM
                    {
                        Id = i.Id,
                        PublicId = i.PublicId,
                        CourseName = i.CourseName,
                        TemplateType = i.Template?.Type,
                        Status = i.Status,
                        ThumbnailPath = i.ThumbnailPath,
                        IssuedAt = i.IssuedAt,
                        ExpiresAt = i.ExpiresAt
                    }).ToList()
                };
            }
        }

        public bool HasActiveCertificate(string userId, int courseId, int templateId)
        {
            using (var db = new CertificateDbContext())
            {
                return db.CertificateIssues.Any(i =>
                    i.UserId == userId
                    && i.TemplateId == templateId
                    && i.CourseId == courseId
                    && (i.Status == "Issued" || i.Status == "Pending" || i.Status == "PendingApproval"));
            }
        }

        #endregion

        #region Helpers

        private void GenerateAndSavePdf(tbl_CertificateIssue issue, tbl_CertificateTemplateVersion version, CertificateDbContext db)
        {
            var verificationUrl = $"/Certificate/Verify/{issue.PublicId}";
            var qrBase64 = _renderService.GenerateQrCodeBase64(verificationUrl);

            var template = db.CertificateTemplates.Find(issue.TemplateId);

            var context = new CertificateRenderContext
            {
                LearnerName = issue.LearnerName,
                CourseTitle = issue.CourseName,
                CertificateId = issue.PublicId,
                IssueDate = DateTime.Now,
                ExpiryDate = issue.ExpiresAt,
                Credits = issue.CreditsAwarded,
                VerificationUrl = verificationUrl,
                QrCodeBase64 = qrBase64,
                LayoutJson = version.LayoutJson,
                Orientation = template?.Orientation ?? "Landscape",
                PageSize = template?.PageSize ?? "A4"
            };

            var pdfPath = _renderService.GeneratePdf(context);
            var thumbPath = _renderService.GenerateThumbnail(context);

            issue.PdfPath = pdfPath;
            issue.ThumbnailPath = thumbPath;
            issue.QrCodePath = qrBase64 != null ? $"data:image/png;base64,{qrBase64}" : null;
        }

        private CertificateIssueVM MapToVM(tbl_CertificateIssue issue, CertificateDbContext db)
        {
            var user = db.AspNetUsers.Find(issue.UserId);

            CriteriaSnapshot snapshot = null;
            if (!string.IsNullOrEmpty(issue.CriteriaSnapshotJson))
            {
                try { snapshot = JsonConvert.DeserializeObject<CriteriaSnapshot>(issue.CriteriaSnapshotJson); }
                catch { /* ignore parse errors */ }
            }

            return new CertificateIssueVM
            {
                Id = issue.Id,
                PublicId = issue.PublicId,
                UserId = issue.UserId,
                LearnerName = issue.LearnerName,
                LearnerEmail = user?.Email,
                CourseName = issue.CourseName,
                TemplateName = issue.Template?.Name,
                TemplateType = issue.Template?.Type,
                TemplateVersion = issue.TemplateVersion?.Version ?? 0,
                CreditsAwarded = issue.CreditsAwarded,
                Status = issue.Status,
                IssuedAt = issue.IssuedAt,
                ExpiresAt = issue.ExpiresAt,
                RevokedAt = issue.RevokedAt,
                RevocationReason = issue.RevocationReason,
                PdfPath = issue.PdfPath,
                ThumbnailPath = issue.ThumbnailPath,
                QrCodePath = issue.QrCodePath,
                PrivacyLevel = issue.PrivacyLevel,
                CriteriaSnapshot = snapshot,
                VerificationUrl = $"/Certificate/Verify/{issue.PublicId}",
                CreatedAt = issue.CreatedAt
            };
        }

        #endregion
    }
}
