namespace First_Aid_Made_Easy.DAL
{
    using System;
    using System.Collections.Generic;

    public partial class tbl_EmailDripCampaigns
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TriggerType { get; set; }
        public string TriggerConfig { get; set; }
        public Nullable<int> FromAccountId { get; set; }
        public bool IsActive { get; set; }
        public int TotalEnrolled { get; set; }
        public int TotalCompleted { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public Nullable<System.DateTime> UpdatedAt { get; set; }

        public virtual tbl_EmailSenderAccounts SenderAccount { get; set; }
    }
}
