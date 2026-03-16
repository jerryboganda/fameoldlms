using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Models
{
    // =============================================
    // SENDER ACCOUNT VIEW MODELS
    // =============================================

    public class SenderAccountVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Account name is required")]
        [Display(Name = "Account Name")]
        public string AccountName { get; set; }

        // View-friendly aliases
        public string SenderName { get { return AccountName; } set { AccountName = value; } }

        [Required(ErrorMessage = "From email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "From Email")]
        public string FromEmail { get; set; }

        // View-friendly alias
        public string Email { get { return FromEmail; } set { FromEmail = value; } }

        [Required(ErrorMessage = "From name is required")]
        [Display(Name = "From Name")]
        public string FromName { get; set; }

        [Display(Name = "SMTP Host")]
        public string SmtpHost { get; set; }

        [Display(Name = "SMTP Port")]
        public int? SmtpPort { get; set; }

        [Display(Name = "SMTP Username")]
        public string SmtpUsername { get; set; }

        // View-friendly alias
        public string SmtpUser { get { return SmtpUsername; } set { SmtpUsername = value; } }

        [Display(Name = "SMTP Password")]
        public string SmtpPassword { get; set; }

        [Display(Name = "Enable SSL")]
        public bool EnableSSL { get; set; } = true;

        // View-friendly alias
        public bool SmtpUseSsl { get { return EnableSSL; } set { EnableSSL = value; } }

        [Display(Name = "Use API")]
        public bool UseApi { get; set; }

        // View-friendly alias
        public bool UseBrevoApi { get { return UseApi; } set { UseApi = value; } }

        [Display(Name = "API Key")]
        public string ApiKey { get; set; }

        // View-friendly alias
        public string BrevoApiKey { get { return ApiKey; } set { ApiKey = value; } }

        [Display(Name = "API Provider")]
        public string ApiProvider { get; set; } = "SMTP"; // SMTP, Brevo, SendGrid

        [Display(Name = "Daily Limit")]
        public int DailyLimit { get; set; } = 500;

        public int SentToday { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
        public string ReplyToEmail { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SenderAccountListVM
    {
        public List<SenderAccountVM> Accounts { get; set; } = new List<SenderAccountVM>();
        // View-friendly alias
        public List<SenderAccountVM> Senders { get { return Accounts; } }
    }

    // =============================================
    // EMAIL TEMPLATE VIEW MODELS
    // =============================================

    public class EmailTemplateVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Template name is required")]
        [Display(Name = "Template Name")]
        public string TemplateName { get; set; }

        [Display(Name = "Default Subject")]
        public string Subject { get; set; }

        // View-friendly alias for TemplateEditor
        public string SubjectLine { get { return Subject; } set { Subject = value; } }

        [AllowHtml]
        [Display(Name = "HTML Body")]
        public string HtmlBody { get; set; }

        [AllowHtml]
        public string JsonDesign { get; set; }

        public string ThumbnailPath { get; set; }

        [Display(Name = "Category")]
        public string Category { get; set; } = "Marketing";

        public bool IsSystem { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class EmailTemplateListVM
    {
        public List<EmailTemplateVM> Templates { get; set; } = new List<EmailTemplateVM>();
        public List<string> Categories { get; set; } = new List<string>();
    }

    // =============================================
    // AUDIENCE / SEGMENT VIEW MODELS
    // =============================================

    public class AudienceSegmentVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Segment name is required")]
        [Display(Name = "Segment Name")]
        public string SegmentName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        public AudienceFilterVM Filter { get; set; } = new AudienceFilterVM();

        /// <summary>Serialized JSON of the filter (stored in DB)</summary>
        public string FilterJson { get; set; }

        public int RecipientCount { get; set; }
        // View-friendly alias
        public int EstimatedCount { get { return RecipientCount; } set { RecipientCount = value; } }
        public bool IsDefault { get; set; }
        public bool IsSystem { get { return IsDefault; } set { IsDefault = value; } }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class AudienceFilterVM
    {
        public string Role { get; set; } = "Student";
        public List<string> Universities { get; set; } = new List<string>();
        public List<string> ExamTypes { get; set; } = new List<string>();
        public List<string> Packages { get; set; } = new List<string>();
        public List<string> Courses { get; set; } = new List<string>();
        public string EnrollmentStatus { get; set; }   // Active, Trial, Expired, All
        public bool? EmailConfirmed { get; set; }
        public bool? IsActive { get; set; }
        public int? MinInactiveDays { get; set; }
        public int? MaxInactiveDays { get; set; }
        // View-friendly aliases
        public int? InactiveDaysMin { get { return MinInactiveDays; } set { MinInactiveDays = value; } }
        public int? InactiveDaysMax { get { return MaxInactiveDays; } set { MaxInactiveDays = value; } }
        public DateTime? RegisteredAfter { get; set; }
        public DateTime? RegisteredBefore { get; set; }
        public List<string> Countries { get; set; } = new List<string>();
        public List<string> Cities { get; set; } = new List<string>();
        public bool? IsPaid { get; set; }
        public bool ExcludeUnsubscribed { get; set; } = true;
    }

    public class AudiencePreviewVM
    {
        public int TotalCount { get; set; }
        public List<AudienceRecipientVM> SampleRecipients { get; set; } = new List<AudienceRecipientVM>();
    }

    public class AudienceRecipientVM
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string University { get; set; }
        public string ExamType { get; set; }
        public string City { get; set; }
    }

    public class AudienceListVM
    {
        public List<AudienceSegmentVM> Segments { get; set; } = new List<AudienceSegmentVM>();
    }

    // =============================================
    // CAMPAIGN VIEW MODELS
    // =============================================

    public class CampaignVM
    {
        public int Id { get; set; }
        public string CampaignName { get; set; }
        public string Subject { get; set; }
        public string Status { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public int TotalRecipients { get; set; }
        public int TotalSent { get; set; }
        public int TotalFailed { get; set; }
        public int TotalOpens { get; set; }
        public int TotalClicks { get; set; }
        public int UniqueOpens { get; set; }
        public int UniqueClicks { get; set; }
        public int TotalBounces { get; set; }
        public int TotalUnsubscribes { get; set; }
        public decimal OpenRate => TotalSent > 0 ? Math.Round((decimal)UniqueOpens / TotalSent * 100, 1) : 0;
        public decimal ClickRate => TotalSent > 0 ? Math.Round((decimal)UniqueClicks / TotalSent * 100, 1) : 0;
        public decimal BounceRate => TotalSent > 0 ? Math.Round((decimal)TotalBounces / TotalSent * 100, 1) : 0;
        public DateTime? ScheduledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Tags { get; set; }
    }

    public class CampaignDashboardVM
    {
        public List<CampaignVM> Campaigns { get; set; } = new List<CampaignVM>();
        public int TotalCampaigns { get; set; }
        public int TotalEmailsSentThisMonth { get; set; }
        public decimal AverageOpenRate { get; set; }
        public decimal AverageClickRate { get; set; }
        public int ActiveDrips { get; set; }
    }

    public class CampaignCreateVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Campaign name is required")]
        [Display(Name = "Campaign Name")]
        public string CampaignName { get; set; }

        [Required(ErrorMessage = "Subject is required")]
        [Display(Name = "Subject Line")]
        public string Subject { get; set; }

        [Display(Name = "Preheader Text")]
        public string PreheaderText { get; set; }

        [Required(ErrorMessage = "Please select a sender account")]
        [Display(Name = "Send From")]
        public int FromAccountId { get; set; }

        [Display(Name = "Template")]
        public int? TemplateId { get; set; }

        [AllowHtml]
        [Required(ErrorMessage = "Email body is required")]
        [Display(Name = "Email Content")]
        public string HtmlBody { get; set; }

        [Display(Name = "Audience Segment")]
        public int? AudienceId { get; set; }

        /// <summary>Custom filter if not using a saved segment</summary>
        public AudienceFilterVM CustomFilter { get; set; }

        // Targeting mode: "audience", "package", "course", "single"
        [Display(Name = "Targeting Mode")]
        public string TargetingMode { get; set; } = "audience";

        [Display(Name = "Package")]
        public int? PackageId { get; set; }

        [Display(Name = "Course")]
        public int? CourseId { get; set; }

        [Display(Name = "Enrollment Status")]
        public string EnrollmentStatus { get; set; } = "All";

        [Display(Name = "Single Email")]
        public string SingleEmail { get; set; }

        [Display(Name = "Schedule Date/Time")]
        public DateTime? ScheduledAt { get; set; }

        [Display(Name = "Tags")]
        public string Tags { get; set; }

        // A/B Testing
        public bool ABTestEnabled { get; set; }
        public string ABSubjectVariant { get; set; }
        [AllowHtml]
        public string ABBodyVariant { get; set; }
        public int ABTestSplitPct { get; set; } = 20;
        public string ABTestWinnerMetric { get; set; } = "OpenRate";

        // Dropdown data
        public List<SelectListItem> SenderAccounts { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Templates { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Audiences { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PackagesList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> CoursesList { get; set; } = new List<SelectListItem>();

        // Preview data
        public int EstimatedRecipientCount { get; set; }
    }

    public class CampaignStatsVM
    {
        public CampaignVM Campaign { get; set; }
        public List<CampaignLinkStatsVM> TopLinks { get; set; } = new List<CampaignLinkStatsVM>();
        public List<CampaignRecipientVM> Recipients { get; set; } = new List<CampaignRecipientVM>();
        public int TotalRecipientPages { get; set; }
        public int CurrentPage { get; set; }

        // Timeline data for charts (JSON serialized in view)
        public string OpenTimelineJson { get; set; }
        public string ClickTimelineJson { get; set; }
    }

    public class CampaignLinkStatsVM
    {
        public int LinkId { get; set; }
        public string Url { get; set; }
        public int TotalClicks { get; set; }
        public int UniqueClicks { get; set; }
    }

    public class CampaignRecipientVM
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? FirstOpenedAt { get; set; }
        public DateTime? FirstClickedAt { get; set; }
        public int OpenCount { get; set; }
        public int ClickCount { get; set; }
    }

    // =============================================
    // DRIP CAMPAIGN VIEW MODELS
    // =============================================

    public class DripCampaignVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        // View-friendly alias
        public string DripName { get { return Name; } set { Name = value; } }

        public string Description { get; set; }

        [Required(ErrorMessage = "Trigger type is required")]
        public string TriggerType { get; set; }

        public string TriggerConfig { get; set; }

        public int? FromAccountId { get; set; }
        public bool IsActive { get; set; }
        public int TotalEnrolled { get; set; }
        public int TotalCompleted { get; set; }
        public int StepCount { get; set; }
        public int EnrolledCount { get { return TotalEnrolled; } }
        public int TotalSent { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<DripStepVM> Steps { get; set; } = new List<DripStepVM>();
        public List<SelectListItem> SenderAccounts { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Templates { get; set; } = new List<SelectListItem>();
    }

    public class DripStepVM
    {
        public int Id { get; set; }
        public int StepOrder { get; set; }
        public int DelayDays { get; set; }
        public int DelayHours { get; set; }
        public string Subject { get; set; }
        public int? TemplateId { get; set; }
        [AllowHtml]
        public string HtmlBody { get; set; }
        public string Condition { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class DripCampaignListVM
    {
        public List<DripCampaignVM> DripCampaigns { get; set; } = new List<DripCampaignVM>();
        // View-friendly alias
        public List<DripCampaignVM> Drips { get { return DripCampaigns; } }
    }

    // =============================================
    // UNSUBSCRIBE VIEW MODELS
    // =============================================

    public class UnsubscribeVM
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Reason { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsInvalid { get; set; }
    }

    public class UnsubscribeReportVM
    {
        public List<UnsubscribeRecordVM> Records { get; set; } = new List<UnsubscribeRecordVM>();
        public int TotalUnsubscribed { get; set; }
        public int TotalUnsubscribes { get { return TotalUnsubscribed; } }
        public int UnsubscribedThisMonth { get; set; }
        public int ThisMonthCount { get { return UnsubscribedThisMonth; } }
        public decimal UnsubscribeRate { get; set; }
        public string TopReason { get; set; }
        public Dictionary<string, int> ReasonBreakdown { get; set; } = new Dictionary<string, int>();
        public string MonthlyTrendJson { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; } = 1;
    }

    public class UnsubscribeRecordVM
    {
        public string Email { get; set; }
        public string Reason { get; set; }
        public DateTime UnsubscribedAt { get; set; }
        public string CampaignName { get; set; }
    }
}
