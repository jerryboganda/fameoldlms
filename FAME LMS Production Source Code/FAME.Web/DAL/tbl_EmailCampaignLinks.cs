namespace First_Aid_Made_Easy.DAL
{
    using System;
    using System.Collections.Generic;

    public partial class tbl_EmailCampaignLinks
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string OriginalUrl { get; set; }
        public string TrackingCode { get; set; }
        public int TotalClicks { get; set; }
        public int UniqueClicks { get; set; }

        public virtual tbl_EmailCampaigns Campaign { get; set; }
    }
}
