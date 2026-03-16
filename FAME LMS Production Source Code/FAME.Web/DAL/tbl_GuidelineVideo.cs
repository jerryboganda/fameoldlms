using System;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.DAL
{
    public partial class tbl_GuidelineVideo
    {
        public int GuidelineVideoID { get; set; }
        public int PackageID { get; set; }
        public string VideoTitle { get; set; }
        public string VideoDescription { get; set; }
        public string VideoPath { get; set; }
        public Nullable<int> SortOrder { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<System.DateTime> CreatedDT { get; set; }
    }
}
