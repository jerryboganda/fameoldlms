namespace First_Aid_Made_Easy.DAL
{
    using System;
    using System.Collections.Generic;

    public partial class tbl_EmailCampaignRecipients
    {
        public long Id { get; set; }
        public int CampaignId { get; set; }
        public string RecipientEmail { get; set; }
        public string RecipientName { get; set; }
        public string UserId { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> SentAt { get; set; }
        public Nullable<System.DateTime> DeliveredAt { get; set; }
        public Nullable<System.DateTime> FirstOpenedAt { get; set; }
        public Nullable<System.DateTime> LastOpenedAt { get; set; }
        public Nullable<System.DateTime> FirstClickedAt { get; set; }
        public int OpenCount { get; set; }
        public int ClickCount { get; set; }
        public Nullable<System.DateTime> BouncedAt { get; set; }
        public string BounceType { get; set; }
        public string ErrorMessage { get; set; }
        public string MergeData { get; set; }

        // Navigation
        public virtual tbl_EmailCampaigns Campaign { get; set; }
    }
}
