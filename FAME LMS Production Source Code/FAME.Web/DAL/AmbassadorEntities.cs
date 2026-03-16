using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace First_Aid_Made_Easy.DAL
{
    /// <summary>
    /// Ambassador profile entity
    /// </summary>
    [Table("tbl_Ambassador")]
    public partial class tbl_Ambassador
    {
        public tbl_Ambassador()
        {
            tbl_Referral = new HashSet<tbl_Referral>();
            tbl_ReferralClick = new HashSet<tbl_ReferralClick>();
            tbl_AmbassadorEarning = new HashSet<tbl_AmbassadorEarning>();
            tbl_PayoutMethod = new HashSet<tbl_PayoutMethod>();
            tbl_PayoutRequest = new HashSet<tbl_PayoutRequest>();
            tbl_AmbassadorAuditLog = new HashSet<tbl_AmbassadorAuditLog>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string ReferralCode { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        [Required]
        [StringLength(20)]
        public string Tier { get; set; }

        [StringLength(200)]
        public string University { get; set; }

        [StringLength(100)]
        public string Country { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(200)]
        public string FullName { get; set; }

        public string ApplicationNotes { get; set; }

        [StringLength(128)]
        public string ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [StringLength(500)]
        public string SuspendedReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Computed properties (not mapped to DB)
        [NotMapped]
        public bool IsActive { get { return Status == "Active"; } }

        [NotMapped]
        public string Email { get { return AspNetUsers != null ? AspNetUsers.Email : "Unknown"; } }

        [NotMapped]
        public decimal TotalEarnings { get { return 0; } } // Computed from earnings

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual AmbassadorUser AspNetUsers { get; set; }

        [ForeignKey("ApprovedBy")]
        public virtual AmbassadorUser AspNetUsers1 { get; set; }

        public virtual ICollection<tbl_Referral> tbl_Referral { get; set; }
        public virtual ICollection<tbl_ReferralClick> tbl_ReferralClick { get; set; }
        public virtual ICollection<tbl_AmbassadorEarning> tbl_AmbassadorEarning { get; set; }
        public virtual ICollection<tbl_PayoutMethod> tbl_PayoutMethod { get; set; }
        public virtual ICollection<tbl_PayoutRequest> tbl_PayoutRequest { get; set; }
        public virtual ICollection<tbl_AmbassadorAuditLog> tbl_AmbassadorAuditLog { get; set; }
    }

    /// <summary>
    /// Referral click tracking entity
    /// </summary>
    [Table("tbl_ReferralClick")]
    public partial class tbl_ReferralClick
    {
        [Key]
        public long Id { get; set; }

        public int AmbassadorId { get; set; }

        [StringLength(64)]
        public string IPHash { get; set; }

        [StringLength(64)]
        public string UserAgentHash { get; set; }

        [StringLength(500)]
        public string Referer { get; set; }

        [StringLength(500)]
        public string LandingPage { get; set; }

        public DateTime CreatedAt { get; set; }

        // Alias for CreatedAt
        [NotMapped]
        public DateTime ClickedAt { get { return CreatedAt; } }

        // Navigation
        [ForeignKey("AmbassadorId")]
        public virtual tbl_Ambassador tbl_Ambassador { get; set; }
    }

    /// <summary>
    /// Referral entity (student referred by ambassador)
    /// </summary>
    [Table("tbl_Referral")]
    public partial class tbl_Referral
    {
        public tbl_Referral()
        {
            tbl_ReferralConversion = new HashSet<tbl_ReferralConversion>();
        }

        [Key]
        public int Id { get; set; }

        public int AmbassadorId { get; set; }

        [Required]
        [StringLength(128)]
        public string ReferredUserId { get; set; }

        public DateTime RegisteredAt { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [Required]
        [StringLength(50)]
        public string Source { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        [StringLength(64)]
        public string IPHash { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public long? ClickId { get; set; }

        // Computed/Alias properties
        [NotMapped]
        public DateTime ReferredAt { get { return RegisteredAt; } }

        [NotMapped]
        public string ReferredEmail { get { return AspNetUsers != null ? AspNetUsers.Email : null; } }

        [NotMapped]
        public string ReferredName { get { return AspNetUsers != null ? AspNetUsers.UserName : null; } }

        [NotMapped]
        public DateTime? ConvertedAt { get { var first = tbl_ReferralConversion != null ? tbl_ReferralConversion.FirstOrDefault() : null; return first != null ? first.ConvertedAt : (DateTime?)null; } }

        [NotMapped]
        public decimal LifetimeValue { get { return tbl_ReferralConversion != null ? tbl_ReferralConversion.Sum(c => c.Amount) : 0; } }

        // Navigation
        [ForeignKey("AmbassadorId")]
        public virtual tbl_Ambassador tbl_Ambassador { get; set; }

        [ForeignKey("ReferredUserId")]
        public virtual AmbassadorUser AspNetUsers { get; set; }

        public virtual ICollection<tbl_ReferralConversion> tbl_ReferralConversion { get; set; }
    }

    /// <summary>
    /// Referral conversion entity (when referred student subscribes)
    /// </summary>
    [Table("tbl_ReferralConversion")]
    public partial class tbl_ReferralConversion
    {
        public tbl_ReferralConversion()
        {
            tbl_AmbassadorEarning = new HashSet<tbl_AmbassadorEarning>();
        }

        [Key]
        public int Id { get; set; }

        public int ReferralId { get; set; }

        public int EnrollmentId { get; set; }

        public decimal Amount { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public DateTime ConvertedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        [StringLength(500)]
        public string RevokeReason { get; set; }

        [StringLength(50)]
        public string ConversionType { get; set; }

        public int? OrderId { get; set; }

        // Navigation
        [ForeignKey("ReferralId")]
        public virtual tbl_Referral tbl_Referral { get; set; }

        [ForeignKey("EnrollmentId")]
        public virtual tbl_EnrollmentMaster tbl_EnrollmentMaster { get; set; }

        public virtual ICollection<tbl_AmbassadorEarning> tbl_AmbassadorEarning { get; set; }
    }

    /// <summary>
    /// Commission rule entity
    /// </summary>
    [Table("tbl_CommissionRule")]
    public partial class tbl_CommissionRule
    {
        public tbl_CommissionRule()
        {
            tbl_AmbassadorEarning = new HashSet<tbl_AmbassadorEarning>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string RuleName { get; set; }

        public int? PackageId { get; set; }

        public int? DurationMonths { get; set; }

        [Required]
        [StringLength(20)]
        public string CommissionType { get; set; }

        public decimal CommissionValue { get; set; }

        [Required]
        [StringLength(30)]
        public string AppliesTo { get; set; }

        [StringLength(20)]
        public string MinTier { get; set; }

        public decimal? MinOrderAmount { get; set; }

        // Alias for MinTier
        [NotMapped]
        public string Tier { get { return MinTier; } }

        // Alias for flat amount (same as CommissionValue when type is Flat)
        [NotMapped]
        public decimal FlatAmount { get { return CommissionType == "Flat" ? CommissionValue : 0; } }

        public int Priority { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public bool IsActive { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("PackageId")]
        public virtual tbl_Package tbl_Package { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual AmbassadorUser AspNetUsers { get; set; }

        public virtual ICollection<tbl_AmbassadorEarning> tbl_AmbassadorEarning { get; set; }
    }

    /// <summary>
    /// Ambassador earning ledger entity
    /// </summary>
    [Table("tbl_AmbassadorEarning")]
    public partial class tbl_AmbassadorEarning
    {
        [Key]
        public int Id { get; set; }

        public int AmbassadorId { get; set; }

        public int? ConversionId { get; set; }

        public decimal Amount { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public int? RuleId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? AvailableAt { get; set; }

        public DateTime? PaidOutAt { get; set; }

        public int? PayoutRequestId { get; set; }

        // Alias properties
        [NotMapped]
        public int? ReferralId { get { return tbl_ReferralConversion != null ? tbl_ReferralConversion.ReferralId : (int?)null; } }

        [NotMapped]
        public string EarningType { get { return Type; } }

        [NotMapped]
        public DateTime EarnedAt { get { return CreatedAt; } }

        [NotMapped]
        public int? CommissionRuleId { get { return RuleId; } }

        // Navigation
        [ForeignKey("AmbassadorId")]
        public virtual tbl_Ambassador tbl_Ambassador { get; set; }

        [ForeignKey("ConversionId")]
        public virtual tbl_ReferralConversion tbl_ReferralConversion { get; set; }

        [ForeignKey("RuleId")]
        public virtual tbl_CommissionRule tbl_CommissionRule { get; set; }

        [ForeignKey("PayoutRequestId")]
        public virtual tbl_PayoutRequest tbl_PayoutRequest { get; set; }
    }

    /// <summary>
    /// Payout method entity
    /// </summary>
    [Table("tbl_PayoutMethod")]
    public partial class tbl_PayoutMethod
    {
        public tbl_PayoutMethod()
        {
            tbl_PayoutRequest = new HashSet<tbl_PayoutRequest>();
        }

        [Key]
        public int Id { get; set; }

        public int AmbassadorId { get; set; }

        [Required]
        [StringLength(50)]
        public string MethodType { get; set; }

        [Required]
        [StringLength(200)]
        public string AccountHolderName { get; set; }

        [Required]
        [StringLength(200)]
        public string AccountTitle { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; }
        
        [StringLength(50)]
        public string MobileProvider { get; set; }

        [StringLength(50)]
        public string MaskedAccountNumber { get; set; }

        public string EncryptedPayload { get; set; }

        [StringLength(100)]
        public string BankName { get; set; }

        [StringLength(20)]
        public string BankBranch { get; set; }

        [StringLength(20)]
        public string BranchCode { get; set; }

        public bool IsDefault { get; set; }

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation
        [ForeignKey("AmbassadorId")]
        public virtual tbl_Ambassador tbl_Ambassador { get; set; }

        public virtual ICollection<tbl_PayoutRequest> tbl_PayoutRequest { get; set; }
    }

    /// <summary>
    /// Payout request entity
    /// </summary>
    [Table("tbl_PayoutRequest")]
    public partial class tbl_PayoutRequest
    {
        public tbl_PayoutRequest()
        {
            tbl_AmbassadorEarning = new HashSet<tbl_AmbassadorEarning>();
        }

        [Key]
        public int Id { get; set; }

        public int AmbassadorId { get; set; }

        public int PayoutMethodId { get; set; }

        public decimal Amount { get; set; }

        [Required]
        [StringLength(10)]
        public string Currency { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }

        public DateTime RequestedAt { get; set; }

        public DateTime? ProcessedAt { get; set; }

        public DateTime? PaidAt { get; set; }

        [StringLength(128)]
        public string ProcessedBy { get; set; }

        [StringLength(500)]
        public string AdminNotes { get; set; }

        [StringLength(100)]
        public string TransactionRef { get; set; }

        [StringLength(500)]
        public string RejectionReason { get; set; }

        // Navigation
        [ForeignKey("AmbassadorId")]
        public virtual tbl_Ambassador tbl_Ambassador { get; set; }

        [ForeignKey("PayoutMethodId")]
        public virtual tbl_PayoutMethod tbl_PayoutMethod { get; set; }

        [ForeignKey("ProcessedBy")]
        public virtual AmbassadorUser AspNetUsers { get; set; }

        public virtual ICollection<tbl_AmbassadorEarning> tbl_AmbassadorEarning { get; set; }
    }

    /// <summary>
    /// Ambassador audit log entity
    /// </summary>
    [Table("tbl_AmbassadorAuditLog")]
    public partial class tbl_AmbassadorAuditLog
    {
        [Key]
        public long Id { get; set; }

        public int? AmbassadorId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityType { get; set; }

        [StringLength(50)]
        public string EntityId { get; set; }

        public string OldValue { get; set; }

        public string NewValue { get; set; }

        [Required]
        [StringLength(128)]
        public string PerformedBy { get; set; }

        public DateTime PerformedAt { get; set; }

        [StringLength(64)]
        public string IPHash { get; set; }

        [StringLength(500)]
        public string UserAgent { get; set; }

        // Alias properties
        [NotMapped]
        public string Details { get { return NewValue; } set { NewValue = value; } }

        [NotMapped]
        public string IpAddress { get { return IPHash; } set { IPHash = value; } }

        // Navigation
        [ForeignKey("AmbassadorId")]
        public virtual tbl_Ambassador tbl_Ambassador { get; set; }

        [ForeignKey("PerformedBy")]
        public virtual AmbassadorUser AspNetUsers { get; set; }
    }
}
