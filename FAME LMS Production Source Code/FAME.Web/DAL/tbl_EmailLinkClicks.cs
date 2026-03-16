namespace First_Aid_Made_Easy.DAL
{
    using System;
    using System.Collections.Generic;

    public partial class tbl_EmailLinkClicks
    {
        public long Id { get; set; }
        public int LinkId { get; set; }
        public long RecipientId { get; set; }
        public System.DateTime ClickedAt { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }

        public virtual tbl_EmailCampaignLinks Link { get; set; }
    }
}
