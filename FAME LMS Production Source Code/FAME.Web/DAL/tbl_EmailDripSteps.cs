namespace First_Aid_Made_Easy.DAL
{
    using System;
    using System.Collections.Generic;

    public partial class tbl_EmailDripSteps
    {
        public int Id { get; set; }
        public int DripCampaignId { get; set; }
        public int StepOrder { get; set; }
        public int DelayDays { get; set; }
        public int DelayHours { get; set; }
        public string Subject { get; set; }
        public Nullable<int> TemplateId { get; set; }
        public string HtmlBody { get; set; }
        public string Condition { get; set; }
        public bool IsActive { get; set; }

        public virtual tbl_EmailDripCampaigns DripCampaign { get; set; }
        public virtual tbl_EmailTemplates Template { get; set; }
    }
}
