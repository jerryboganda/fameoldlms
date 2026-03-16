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
    public class CertificateRuleService : ICertificateRuleService
    {
        private readonly ICertificateIssuanceService _issuanceService;
        private readonly ICertificateAuditService _auditService;
        private readonly ILogger _logger;

        public CertificateRuleService(
            ICertificateAuditService auditService,
            ILogger logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        // Lazy-set by AutofacConfig to avoid circular dependency
        private ICertificateIssuanceService _lazyIssuanceService;
        public void SetIssuanceService(ICertificateIssuanceService svc) => _lazyIssuanceService = svc;

        private ICertificateIssuanceService IssuanceService =>
            _lazyIssuanceService ?? throw new InvalidOperationException("IssuanceService not configured.");

        #region CRUD

        public CertificateRuleSetVM GetRuleSetById(int id)
        {
            using (var db = new CertificateDbContext())
            {
                var r = db.CertificateRuleSets
                    .Include(x => x.Template)
                    .FirstOrDefault(x => x.Id == id);
                if (r == null) return null;

                return MapToVM(r, db);
            }
        }

        public List<CertificateRuleSetVM> GetRuleSetsForTemplate(int templateId)
        {
            using (var db = new CertificateDbContext())
            {
                return db.CertificateRuleSets
                    .Include(x => x.Template)
                    .Where(r => r.TemplateId == templateId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList()
                    .Select(r => MapToVM(r, db))
                    .ToList();
            }
        }

        public List<CertificateRuleSetVM> GetActiveRuleSets()
        {
            using (var db = new CertificateDbContext())
            {
                var now = DateTime.Now;
                return db.CertificateRuleSets
                    .Include(x => x.Template)
                    .Where(r => r.IsActive
                        && r.Template.Status == "Active"
                        && (r.ActiveFrom == null || r.ActiveFrom <= now)
                        && (r.ActiveTo == null || r.ActiveTo >= now))
                    .OrderBy(r => r.TemplateId)
                    .ToList()
                    .Select(r => MapToVM(r, db))
                    .ToList();
            }
        }

        public int SaveRuleSet(CertificateRuleSetVM model, string userId)
        {
            using (var db = new CertificateDbContext())
            {
                tbl_CertificateRuleSet entity;

                if (model.Id > 0)
                {
                    entity = db.CertificateRuleSets.Find(model.Id);
                    if (entity == null) throw new InvalidOperationException("RuleSet not found.");

                    entity.TemplateId = model.TemplateId;
                    entity.CourseId = model.CourseId;
                    entity.PackageId = model.PackageId;
                    entity.RuleJson = JsonConvert.SerializeObject(model.Rules);
                    entity.IssuanceMode = model.IssuanceMode;
                    entity.ActiveFrom = model.ActiveFrom;
                    entity.ActiveTo = model.ActiveTo;
                    entity.IsActive = model.IsActive;
                    entity.UpdatedBy = userId;
                    entity.UpdatedAt = DateTime.Now;

                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();

                    _auditService.Log(userId, "RuleUpdated", "RuleSet", entity.Id,
                        details: $"RuleSet updated for template {entity.TemplateId}");
                }
                else
                {
                    entity = new tbl_CertificateRuleSet
                    {
                        TemplateId = model.TemplateId,
                        CourseId = model.CourseId,
                        PackageId = model.PackageId,
                        RuleJson = JsonConvert.SerializeObject(model.Rules),
                        IssuanceMode = model.IssuanceMode ?? "Auto",
                        ActiveFrom = model.ActiveFrom,
                        ActiveTo = model.ActiveTo,
                        IsActive = true,
                        CreatedBy = userId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    db.CertificateRuleSets.Add(entity);
                    db.SaveChanges();

                    _auditService.Log(userId, "RuleCreated", "RuleSet", entity.Id,
                        details: $"RuleSet created for template {entity.TemplateId}, course {entity.CourseId}");
                }

                return entity.Id;
            }
        }

        public void DeactivateRuleSet(int id, string userId)
        {
            using (var db = new CertificateDbContext())
            {
                var entity = db.CertificateRuleSets.Find(id);
                if (entity == null) return;
                entity.IsActive = false;
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.Now;
                db.Entry(entity).State = EntityState.Modified;
                db.SaveChanges();

                _auditService.Log(userId, "RuleDeactivated", "RuleSet", id);
            }
        }

        #endregion

        #region Eligibility Evaluation

        public EligibilityResult EvaluateEligibility(string userId, int courseId, int ruleSetId)
        {
            using (var db = new CertificateDbContext())
            {
                var ruleSet = db.CertificateRuleSets.Find(ruleSetId);
                if (ruleSet == null) return new EligibilityResult { IsEligible = false };
                return EvaluateEligibility(userId, courseId, ruleSet);
            }
        }

        public EligibilityResult EvaluateEligibility(string userId, int courseId, tbl_CertificateRuleSet ruleSet)
        {
            var result = new EligibilityResult();
            RuleDefinition ruleDef;

            try
            {
                ruleDef = JsonConvert.DeserializeObject<RuleDefinition>(ruleSet.RuleJson);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to parse RuleJson for RuleSet {RuleSetId}", ruleSet.Id);
                return result;
            }

            if (ruleDef?.Conditions == null || !ruleDef.Conditions.Any())
            {
                // No conditions = always eligible
                result.IsEligible = true;
                return result;
            }

            foreach (var condition in ruleDef.Conditions)
            {
                var condResult = EvaluateCondition(userId, courseId, condition);
                if (condResult.IsMet)
                    result.MetConditions.Add(condResult);
                else
                    result.UnmetConditions.Add(condResult);
            }

            if (ruleDef.Operator?.ToUpper() == "OR")
                result.IsEligible = result.MetConditions.Any();
            else // AND
                result.IsEligible = !result.UnmetConditions.Any();

            return result;
        }

        private ConditionResult EvaluateCondition(string userId, int courseId, RuleCondition condition)
        {
            switch (condition.Type)
            {
                case "VideoProgress":
                    return EvaluateVideoProgress(userId, courseId, condition.Threshold ?? 100);
                case "ExamPass":
                    return EvaluateExamPass(userId, courseId, condition.Threshold ?? 50, condition.PaperId);
                default:
                    return new ConditionResult
                    {
                        Type = condition.Type,
                        IsMet = false,
                        Description = $"Unknown condition type: {condition.Type}"
                    };
            }
        }

        private ConditionResult EvaluateVideoProgress(string userId, int courseId, int threshold)
        {
            using (var db = new FAMEEntities())
            {
                // Count total videos in the course
                var totalVideos = db.tbl_Video
                    .Where(v => v.Course_Fid == courseId)
                    .Count();

                if (totalVideos == 0)
                {
                    return new ConditionResult
                    {
                        Type = "VideoProgress",
                        Required = threshold,
                        Actual = 0,
                        IsMet = true, // No videos = condition satisfied
                        Description = "No videos in course"
                    };
                }

                // Count watched/completed videos for this student
                var watchedVideos = db.tbl_Progress
                    .Where(p => p.Student_Fid == userId
                        && p.isCompleted == true
                        && p.tbl_Video.Course_Fid == courseId)
                    .Count();

                var progressPct = (int)Math.Round((decimal)watchedVideos / totalVideos * 100);

                return new ConditionResult
                {
                    Type = "VideoProgress",
                    Required = threshold,
                    Actual = progressPct,
                    IsMet = progressPct >= threshold,
                    Description = $"Video progress: {progressPct}% (required: {threshold}%)"
                };
            }
        }

        private ConditionResult EvaluateExamPass(string userId, int courseId, int minScore, int? paperId)
        {
            using (var db = new FAMEEntities())
            {
                var query = db.tbl_ResultMaster
                    .Where(r => r.StudentID == userId);

                if (paperId.HasValue)
                {
                    query = query.Where(r => r.QuestionPaperID == paperId.Value);
                }

                // Get the best result
                var bestResult = query
                    .OrderByDescending(r => r.ObtainedMarks)
                    .FirstOrDefault();

                if (bestResult == null)
                {
                    return new ConditionResult
                    {
                        Type = "ExamPass",
                        Required = minScore,
                        Actual = 0,
                        IsMet = false,
                        Description = "No exam results found"
                    };
                }

                var totalMarks = bestResult.TotalMarks ?? 100;
                var obtained = bestResult.ObtainedMarks ?? 0;
                var scorePct = totalMarks > 0 ? (int)Math.Round((decimal)obtained / totalMarks * 100) : 0;

                return new ConditionResult
                {
                    Type = "ExamPass",
                    Required = minScore,
                    Actual = scorePct,
                    IsMet = scorePct >= minScore,
                    Description = $"Exam score: {scorePct}% (required: {minScore}%)"
                };
            }
        }

        #endregion

        #region Auto-Issuance Hook

        public void CheckAndIssueIfEligible(string userId, int courseId)
        {
            try
            {
                using (var db = new CertificateDbContext())
                {
                    var now = DateTime.Now;
                    var activeRules = db.CertificateRuleSets
                        .Include(r => r.Template)
                        .Where(r => r.IsActive
                            && r.CourseId == courseId
                            && r.IssuanceMode == "Auto"
                            && r.Template.Status == "Active"
                            && (r.ActiveFrom == null || r.ActiveFrom <= now)
                            && (r.ActiveTo == null || r.ActiveTo >= now))
                        .ToList();

                    foreach (var ruleSet in activeRules)
                    {
                        // Check if student already has an active cert for this
                        var alreadyIssued = db.CertificateIssues.Any(i =>
                            i.UserId == userId
                            && i.TemplateId == ruleSet.TemplateId
                            && i.CourseId == courseId
                            && (i.Status == "Issued" || i.Status == "Pending" || i.Status == "PendingApproval"));

                        if (alreadyIssued) continue;

                        var eligibility = EvaluateEligibility(userId, courseId, ruleSet);
                        if (eligibility.IsEligible)
                        {
                            IssuanceService.IssueCertificate(userId, ruleSet.Id);
                            _logger.Information("Auto-issued certificate for user {UserId}, course {CourseId}, ruleSet {RuleSetId}",
                                userId, courseId, ruleSet.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in CheckAndIssueIfEligible for user {UserId}, course {CourseId}", userId, courseId);
            }
        }

        #endregion

        #region Helpers

        public List<CourseSelectItem> GetCoursesForDropdown()
        {
            using (var db = new FAMEEntities())
            {
                return db.tbl_Courses
                    .OrderBy(c => c.Course_Name)
                    .Select(c => new CourseSelectItem
                    {
                        CourseId = c.Course_Id,
                        CourseName = c.Course_Name
                    }).ToList();
            }
        }

        private CertificateRuleSetVM MapToVM(tbl_CertificateRuleSet r, CertificateDbContext db)
        {
            string courseName = null;
            if (r.CourseId.HasValue)
            {
                using (var fameDb = new FAMEEntities())
                {
                    courseName = fameDb.tbl_Courses
                        .Where(c => c.Course_Id == r.CourseId.Value)
                        .Select(c => c.Course_Name)
                        .FirstOrDefault();
                }
            }

            RuleDefinition rules;
            try
            {
                rules = JsonConvert.DeserializeObject<RuleDefinition>(r.RuleJson);
            }
            catch
            {
                rules = new RuleDefinition();
            }

            return new CertificateRuleSetVM
            {
                Id = r.Id,
                TemplateId = r.TemplateId,
                TemplateName = r.Template?.Name,
                CourseId = r.CourseId,
                CourseName = courseName,
                PackageId = r.PackageId,
                IssuanceMode = r.IssuanceMode,
                ActiveFrom = r.ActiveFrom,
                ActiveTo = r.ActiveTo,
                IsActive = r.IsActive,
                Rules = rules,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            };
        }

        #endregion
    }
}
