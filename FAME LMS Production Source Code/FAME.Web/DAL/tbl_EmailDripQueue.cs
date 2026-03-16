namespace First_Aid_Made_Easy.DAL
{
    using System;
    using System.Collections.Generic;

    public partial class tbl_EmailDripQueue
    {
        public long Id { get; set; }
        public int DripCampaignId { get; set; }
        public int StepId { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public System.DateTime ScheduledAt { get; set; }
        public Nullable<System.DateTime> SentAt { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }

        public virtual tbl_EmailDripCampaigns DripCampaign { get; set; }
        public virtual tbl_EmailDripSteps Step { get; set; }
    }
}
