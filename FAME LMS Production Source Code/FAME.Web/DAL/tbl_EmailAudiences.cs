namespace First_Aid_Made_Easy.DAL
{
    using System;
    using System.Collections.Generic;

    public partial class tbl_EmailAudiences
    {
        public int Id { get; set; }
        public string SegmentName { get; set; }
        public string Description { get; set; }
        public string FilterJson { get; set; }
        public int RecipientCount { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public Nullable<System.DateTime> UpdatedAt { get; set; }
    }
}
