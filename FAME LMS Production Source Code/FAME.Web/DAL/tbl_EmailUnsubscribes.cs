namespace First_Aid_Made_Easy.DAL
{
    using System;
    using System.Collections.Generic;

    public partial class tbl_EmailUnsubscribes
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        public Nullable<int> CampaignId { get; set; }
        public string Reason { get; set; }
        public System.DateTime UnsubscribedAt { get; set; }
        public bool IsResubscribed { get; set; }
        public Nullable<System.DateTime> ResubscribedAt { get; set; }
    }
}
