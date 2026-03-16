namespace First_Aid_Made_Easy.DAL
{
    using System;
    using System.Collections.Generic;

    public partial class tbl_EmailCampaigns
    {
        public int Id { get; set; }
        public string CampaignName { get; set; }
        public string Subject { get; set; }
        public string PreheaderText { get; set; }
        public int FromAccountId { get; set; }
        public Nullable<int> TemplateId { get; set; }
        public string HtmlBody { get; set; }
        public string PlainTextBody { get; set; }
        public Nullable<int> AudienceId { get; set; }
        public string AudienceFilterJson { get; set; }
        public string TargetingMode { get; set; }       // audience, package, course, single
        public Nullable<int> PackageId { get; set; }
        public Nullable<int> CourseId { get; set; }
        public string SingleEmail { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> ScheduledAt { get; set; }
        public Nullable<System.DateTime> SendStartedAt { get; set; }
        public Nullable<System.DateTime> CompletedAt { get; set; }
        public int TotalRecipients { get; set; }
        public int TotalSent { get; set; }
        public int TotalFailed { get; set; }
        public int TotalOpens { get; set; }
        public int TotalClicks { get; set; }
        public int UniqueOpens { get; set; }
        public int UniqueClicks { get; set; }
        public int TotalBounces { get; set; }
        public int TotalUnsubscribes { get; set; }
        public bool ABTestEnabled { get; set; }
        public Nullable<int> ABTestVariantOf { get; set; }
        public Nullable<int> ABTestSplitPct { get; set; }
        public string ABTestWinnerMetric { get; set; }
        public string Tags { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public Nullable<System.DateTime> UpdatedAt { get; set; }

        // Navigation properties
        public virtual tbl_EmailSenderAccounts SenderAccount { get; set; }
        public virtual tbl_EmailTemplates Template { get; set; }
    }
}
