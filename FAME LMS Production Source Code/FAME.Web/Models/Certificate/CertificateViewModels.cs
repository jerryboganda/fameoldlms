using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace First_Aid_Made_Easy.Models.Certificate
{
    // ─────────────────────────────────────────────────────────────────
    // Certificate Module – View Models
    // ─────────────────────────────────────────────────────────────────

    #region Rule Engine Models

    /// <summary>
    /// Deserialized form of CertificateRuleSet.RuleJson
    /// </summary>
    public class RuleDefinition
    {
        public string Operator { get; set; } = "AND"; // AND, OR
        public List<RuleCondition> Conditions { get; set; } = new List<RuleCondition>();
    }

    public class RuleCondition
    {
        public string Type { get; set; }       // VideoProgress, ExamPass, Attendance, EvaluationForm, Attestation
        public int? Threshold { get; set; }    // Percentage or score
        public int? PaperId { get; set; }      // Specific exam paper (null = any)
        public int? MinMinutes { get; set; }   // For attendance rules
    }

    public class EligibilityResult
    {
        public bool IsEligible { get; set; }
        public List<ConditionResult> MetConditions { get; set; } = new List<ConditionResult>();
        public List<ConditionResult> UnmetConditions { get; set; } = new List<ConditionResult>();
    }

    public class ConditionResult
    {
        public string Type { get; set; }
        public int? Required { get; set; }
        public int? Actual { get; set; }
        public bool IsMet { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Snapshot of rules + actuals at the moment of issuance, stored as JSON.
    /// </summary>
    public class CriteriaSnapshot
    {
        public int RuleSetId { get; set; }
        public int TemplateVersion { get; set; }
        public string RuleOperator { get; set; }
        public List<ConditionResult> Conditions { get; set; } = new List<ConditionResult>();
        public DateTime? EvaluatedAt { get; set; }

        // View aliases for IssueDetail.cshtml legacy fields
        public string RuleName { get; set; }
        public decimal? VideoCompletionPct { get; set; }
        public decimal? ExamScorePct { get; set; }
        public bool? CourseCompleted { get; set; }
    }

    #endregion

    #region Template ViewModels

    public class CertificateTemplateVM
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        public string Status { get; set; }

        public int Version { get; set; }

        public string LayoutJson { get; set; }

        public string ThumbnailPath { get; set; }

        [Required]
        public string Orientation { get; set; } = "Landscape";

        [Required]
        public string PageSize { get; set; } = "A4";

        public string Description { get; set; }

        public string CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // For builder page
        public List<CertificateAssetVM> AvailableAssets { get; set; } = new List<CertificateAssetVM>();

        // View alias
        public string FabricJson { get => LayoutJson; set => LayoutJson = value; }
    }

    public class CertificateTemplateListVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public int Version { get; set; }
        public string ThumbnailPath { get; set; }
        public string Orientation { get; set; }
        public int IssueCount { get; set; }
        public int RuleSetCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // View alias
        public string ThumbnailUrl => ThumbnailPath;
    }

    /// <summary>Paginated wrapper for Templates list view</summary>
    public class CertificateTemplateListPageVM
    {
        public List<CertificateTemplateListVM> Templates { get; set; } = new List<CertificateTemplateListVM>();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string Search { get; set; }
        public string StatusFilter { get; set; }
    }

    #endregion

    #region Asset ViewModels

    public class CertificateAssetVM
    {
        public int Id { get; set; }
        public string AssetType { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string SignatoryName { get; set; }
        public string SignatoryTitle { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // View aliases
        public string Category => AssetType;
        public string Url => FilePath;
    }

    public class AssetUploadVM
    {
        [Required]
        public string AssetType { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public HttpPostedFileBase File { get; set; }

        public string SignatoryName { get; set; }
        public string SignatoryTitle { get; set; }
    }

    #endregion

    #region RuleSet ViewModels

    public class CertificateRuleSetVM
    {
        public int Id { get; set; }

        [Required]
        public int TemplateId { get; set; }

        public string TemplateName { get; set; }

        public int? CourseId { get; set; }
        public string CourseName { get; set; }

        public int? PackageId { get; set; }
        public string PackageName { get; set; }

        [Required]
        public string IssuanceMode { get; set; } = "Auto";

        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }
        public bool IsActive { get; set; } = true;

        // Deserialized rule definition
        public RuleDefinition Rules { get; set; } = new RuleDefinition();

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    #endregion

    #region Issue ViewModels

    public class CertificateIssueVM
    {
        public int Id { get; set; }
        public string PublicId { get; set; }
        public string UserId { get; set; }
        public string LearnerName { get; set; }
        public string LearnerEmail { get; set; }
        public string CourseName { get; set; }
        public string TemplateName { get; set; }
        public string TemplateType { get; set; }
        public int TemplateVersion { get; set; }
        public decimal? CreditsAwarded { get; set; }
        public string Status { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string RevocationReason { get; set; }
        public string PdfPath { get; set; }
        public string ThumbnailPath { get; set; }
        public string QrCodePath { get; set; }
        public string PrivacyLevel { get; set; }
        public string IssuedByName { get; set; }
        public DateTime CreatedAt { get; set; }

        // For detail view
        public CriteriaSnapshot CriteriaSnapshot { get; set; }
        public string CriteriaSnapshotJson { get; set; }
        public List<CertificateAuditLogVM> AuditTrail { get; set; }
        public List<CertificateCorrectionVM> CorrectionRequests { get; set; }

        // View aliases
        public string VerificationUrl { get; set; }
        public decimal Credits => CreditsAwarded ?? 0;
        public string RevokeReason => RevocationReason;
        public string PdfUrl => PdfPath;
        public string ThumbnailUrl => ThumbnailPath;
        public string QrCodeBase64 => QrCodePath;
    }

    public class CertificateIssueListVM
    {
        public int Id { get; set; }
        public string PublicId { get; set; }
        public string LearnerName { get; set; }
        public string CourseName { get; set; }
        public string TemplateName { get; set; }
        public string Status { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string ThumbnailPath { get; set; }
    }

    /// <summary>Paginated wrapper for Issues list view</summary>
    public class CertificateIssueListPageVM
    {
        public List<CertificateIssueListVM> Issues { get; set; } = new List<CertificateIssueListVM>();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public string Search { get; set; }
        public string StatusFilter { get; set; }
    }

    #endregion

    #region Verification ViewModel

    public class CertificateVerifyVM
    {
        public bool Found { get; set; }
        public string PublicId { get; set; }
        public string DisplayName { get; set; }       // Based on privacy level
        public string CourseName { get; set; }
        public string TemplateType { get; set; }
        public string Status { get; set; }             // Issued, Revoked, Expired
        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public decimal? CreditsAwarded { get; set; }
        public string IssuingOrganization { get; set; } = "First Aid Made Easy";

        // Computed properties for views
        public string LearnerName => DisplayName;
        public decimal Credits => CreditsAwarded ?? 0;
        public bool IsValid => Found && Status == "Issued" && !IsExpired;
        public bool IsExpired => Status == "Expired" || (ExpiresAt.HasValue && ExpiresAt.Value < DateTime.Now);
        public bool IsRevoked => Status == "Revoked" || RevokedAt.HasValue;
    }

    #endregion

    #region Audit ViewModel

    public class CertificateAuditLogVM
    {
        public long Id { get; set; }
        public string ActorName { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string Details { get; set; }
        public DateTime CreatedAt { get; set; }

        // View aliases
        public DateTime Timestamp => CreatedAt;
        public string Detail => Details;
        public string PerformedBy => ActorName;
    }

    #endregion

    #region Correction ViewModel

    public class CertificateCorrectionVM
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public string LearnerName { get; set; }
        public string CourseName { get; set; }
        public string PublicId { get; set; }

        [Required]
        public string RequestType { get; set; }

        [Required]
        public string Details { get; set; }

        public string Status { get; set; }
        public string ResolvedByName { get; set; }
        public string ResolutionNotes { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // View aliases
        public string CertificatePublicId => PublicId;
        public string Description => Details;
        public DateTime SubmittedAt => CreatedAt;
        public int CertificateIssueId => IssueId;
    }

    #endregion

    #region Admin Dashboard ViewModel

    public class CertificateAdminDashboardVM
    {
        public int TotalTemplates { get; set; }
        public int ActiveTemplates { get; set; }
        public int TotalIssued { get; set; }
        public int PendingApprovals { get; set; }
        public int RevokedCount { get; set; }
        public int RenderFailedCount { get; set; }
        public int OpenCorrectionRequests { get; set; }
        public int TotalVerifications { get; set; }

        public List<CertificateTemplateListVM> RecentTemplates { get; set; } = new List<CertificateTemplateListVM>();
        public List<CertificateIssueListVM> RecentIssues { get; set; } = new List<CertificateIssueListVM>();
        public List<CertificateCorrectionVM> PendingCorrections { get; set; } = new List<CertificateCorrectionVM>();
    }

    #endregion

    #region Student Certificate Center ViewModel

    public class StudentCertificateCenterVM
    {
        public List<StudentCertificateCardVM> Certificates { get; set; } = new List<StudentCertificateCardVM>();
        public int TotalCount { get; set; }
        public int ReadyCount { get; set; }
        public int PendingCount { get; set; }
        public decimal TotalCredits { get; set; }

        // View aliases
        public int TotalEarned => TotalCount;
        public int ActiveCount => ReadyCount;

        // Filters
        public string Search { get; set; }
        public string TypeFilter { get; set; }
        public string StatusFilter { get; set; }
    }

    public class StudentCertificateCardVM
    {
        public int Id { get; set; }
        public string PublicId { get; set; }
        public string CourseName { get; set; }
        public string TemplateType { get; set; }
        public string Status { get; set; }
        public string ThumbnailPath { get; set; }
        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string VerificationUrl { get; set; }
        public decimal Credits { get; set; }

        // View alias
        public string ThumbnailUrl => ThumbnailPath;
    }

    #endregion

    #region Render Context (for PDF generation)

    /// <summary>
    /// All token values needed to render a certificate PDF.
    /// </summary>
    public class CertificateRenderContext
    {
        public string LearnerName { get; set; }
        public string CourseTitle { get; set; }
        public string CertificateId { get; set; }     // PublicId
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal? Credits { get; set; }
        public string VerificationUrl { get; set; }
        public string QrCodeBase64 { get; set; }

        // Template data
        public string LayoutJson { get; set; }
        public string Orientation { get; set; }
        public string PageSize { get; set; }
    }

    #endregion

    #region Bulk / Admin Action ViewModels

    public class RevokeVM
    {
        [Required]
        public int IssueId { get; set; }

        [Required, MaxLength(500)]
        public string Reason { get; set; }
    }

    public class ReIssueVM
    {
        [Required]
        public int OriginalIssueId { get; set; }

        [MaxLength(200)]
        public string CorrectedLearnerName { get; set; }

        public decimal? CorrectedCredits { get; set; }

        public string Notes { get; set; }
    }

    #endregion

    #region Manual Certificate Upload ViewModels

    /// <summary>Page model for the admin "User List" page — filter by course/package, then upload certificates.</summary>
    public class ManualCertificateUserListVM
    {
        public List<EnrolledUserRow> Users { get; set; } = new List<EnrolledUserRow>();
        public List<CourseDropdownItem> Courses { get; set; } = new List<CourseDropdownItem>();
        public List<PackageDropdownItem> Packages { get; set; } = new List<PackageDropdownItem>();
        public int? SelectedCourseId { get; set; }
        public int? SelectedPackageId { get; set; }
        public string FilterType { get; set; } // "course" or "package"
    }

    public class EnrolledUserRow
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string CourseName { get; set; }
        public string PackageName { get; set; }
        public int? CourseId { get; set; }
        public int? PackageId { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public int ManualCertCount { get; set; }
    }

    public class CourseDropdownItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PackageDropdownItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ManualCertificateVM
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string LearnerName { get; set; }
        public int? CourseId { get; set; }
        public int? PackageId { get; set; }
        public string CourseName { get; set; }
        public string PackageName { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Title { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    #endregion

    #region Google Sheet Data

    public class GoogleSheetDataVM
    {
        public int SheetDbId { get; set; }
        public string SheetName { get; set; }
        public List<string> Headers { get; set; } = new List<string>();
        public List<List<string>> Rows { get; set; } = new List<List<string>>();
        public int TotalRecords { get; set; }
        public DateTime? FetchedAt { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class GoogleSheetListVM
    {
        public List<GoogleSheetItemVM> Sheets { get; set; } = new List<GoogleSheetItemVM>();
    }

    public class GoogleSheetItemVM
    {
        public int Id { get; set; }
        public string SheetName { get; set; }
        public string SheetId { get; set; }
        public string GId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    #endregion
}
