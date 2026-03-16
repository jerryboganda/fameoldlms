using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace First_Aid_Made_Easy.Models.Certificate
{
    // ─────────────────────────────────────────────────────────────────
    // Certificate Module – POCO Entity Classes
    // Maps to tables created by Database/create_certificate_tables.sql
    // ─────────────────────────────────────────────────────────────────

    [Table("tbl_CertificateAsset")]
    public class tbl_CertificateAsset
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string AssetType { get; set; }   // Logo, Signature, Seal, Background, Watermark

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [Required, MaxLength(500)]
        public string FilePath { get; set; }

        [MaxLength(200)]
        public string SignatoryName { get; set; }

        [MaxLength(200)]
        public string SignatoryTitle { get; set; }

        [Required, MaxLength(128)]
        public string UploadedBy { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("tbl_CertificateTemplate")]
    public class tbl_CertificateTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [Required, MaxLength(50)]
        public string Type { get; set; } = "Completion";   // Completion, Attendance, CME, Program

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Draft";      // Draft, Active, Archived

        public int Version { get; set; } = 1;

        [Required]
        public string LayoutJson { get; set; }

        [MaxLength(500)]
        public string ThumbnailPath { get; set; }

        [Required, MaxLength(10)]
        public string Orientation { get; set; } = "Landscape";

        [Required, MaxLength(10)]
        public string PageSize { get; set; } = "A4";

        [MaxLength(500)]
        public string Description { get; set; }

        [Required, MaxLength(128)]
        public string CreatedBy { get; set; }

        [MaxLength(128)]
        public string UpdatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    [Table("tbl_CertificateTemplateVersion")]
    public class tbl_CertificateTemplateVersion
    {
        [Key]
        public int Id { get; set; }

        public int TemplateId { get; set; }

        public int Version { get; set; }

        [Required]
        public string LayoutJson { get; set; }

        [MaxLength(500)]
        public string ChangeNotes { get; set; }

        [Required, MaxLength(128)]
        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("TemplateId")]
        public virtual tbl_CertificateTemplate Template { get; set; }
    }

    [Table("tbl_CertificateRuleSet")]
    public class tbl_CertificateRuleSet
    {
        [Key]
        public int Id { get; set; }

        public int TemplateId { get; set; }

        public int? CourseId { get; set; }

        public int? PackageId { get; set; }

        [Required]
        public string RuleJson { get; set; }

        [Required, MaxLength(20)]
        public string IssuanceMode { get; set; } = "Auto";  // Auto, Manual, Scheduled

        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTo { get; set; }

        public bool IsActive { get; set; } = true;

        [Required, MaxLength(128)]
        public string CreatedBy { get; set; }

        [MaxLength(128)]
        public string UpdatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("TemplateId")]
        public virtual tbl_CertificateTemplate Template { get; set; }
    }

    [Table("tbl_CertificateIssue")]
    public class tbl_CertificateIssue
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(36)]
        public string PublicId { get; set; }

        [Required, MaxLength(128)]
        public string UserId { get; set; }

        public int TemplateId { get; set; }

        public int TemplateVersionId { get; set; }

        public int? RuleSetId { get; set; }

        public int? CourseId { get; set; }

        [MaxLength(200)]
        public string CourseName { get; set; }

        [Required, MaxLength(200)]
        public string LearnerName { get; set; }

        public decimal? CreditsAwarded { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime? IssuedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }

        [MaxLength(500)]
        public string RevocationReason { get; set; }

        [MaxLength(500)]
        public string PdfPath { get; set; }

        [MaxLength(500)]
        public string ThumbnailPath { get; set; }

        [MaxLength(500)]
        public string QrCodePath { get; set; }

        public string CriteriaSnapshotJson { get; set; }

        [Required, MaxLength(20)]
        public string PrivacyLevel { get; set; } = "FullName";

        [MaxLength(128)]
        public string IssuedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("TemplateId")]
        public virtual tbl_CertificateTemplate Template { get; set; }

        [ForeignKey("TemplateVersionId")]
        public virtual tbl_CertificateTemplateVersion TemplateVersion { get; set; }

        [ForeignKey("RuleSetId")]
        public virtual tbl_CertificateRuleSet RuleSet { get; set; }
    }

    [Table("tbl_CertificateAuditLog")]
    public class tbl_CertificateAuditLog
    {
        [Key]
        public long Id { get; set; }

        [MaxLength(128)]
        public string ActorId { get; set; }

        [Required, MaxLength(50)]
        public string Action { get; set; }

        [Required, MaxLength(50)]
        public string EntityType { get; set; }

        public int EntityId { get; set; }

        [MaxLength(64)]
        public string BeforeHash { get; set; }

        [MaxLength(64)]
        public string AfterHash { get; set; }

        public string Details { get; set; }

        [MaxLength(45)]
        public string IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    [Table("tbl_CertificateCorrectionRequest")]
    public class tbl_CertificateCorrectionRequest
    {
        [Key]
        public int Id { get; set; }

        public int IssueId { get; set; }

        [Required, MaxLength(128)]
        public string UserId { get; set; }

        [Required, MaxLength(50)]
        public string RequestType { get; set; }

        [Required]
        public string Details { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Open";

        [MaxLength(128)]
        public string ResolvedBy { get; set; }

        [MaxLength(500)]
        public string ResolutionNotes { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("IssueId")]
        public virtual tbl_CertificateIssue Issue { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────
    // Manual Certificate Upload – Admin uploads PDF for a student
    // ─────────────────────────────────────────────────────────────────

    // ─────────────────────────────────────────────────────────────────
    // Google Sheet – stores references to admin-added Google Sheets
    // ─────────────────────────────────────────────────────────────────

    [Table("tbl_GoogleSheet")]
    public class tbl_GoogleSheet
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string SheetName { get; set; }

        [Required, MaxLength(200)]
        public string SheetId { get; set; }

        [Required, MaxLength(50)]
        public string GId { get; set; } = "0";

        [Required, MaxLength(128)]
        public string AddedBy { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // ─────────────────────────────────────────────────────────────────
    // Manual Certificate Upload – Admin uploads PDF for a student
    // ─────────────────────────────────────────────────────────────────

    [Table("tbl_ManualCertificate")]
    public class tbl_ManualCertificate
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(128)]
        public string UserId { get; set; }

        public int? CourseId { get; set; }

        public int? PackageId { get; set; }

        [MaxLength(200)]
        public string CourseName { get; set; }

        [MaxLength(200)]
        public string PackageName { get; set; }

        [Required, MaxLength(200)]
        public string LearnerName { get; set; }

        [Required, MaxLength(200)]
        public string FileName { get; set; }

        [Required, MaxLength(500)]
        public string FilePath { get; set; }

        [MaxLength(300)]
        public string Title { get; set; }

        [Required, MaxLength(128)]
        public string UploadedBy { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
