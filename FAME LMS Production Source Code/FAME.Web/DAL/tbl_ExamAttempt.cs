using System;

namespace First_Aid_Made_Easy.DAL
{
    public class tbl_ExamAttempt
    {
        public int ExamAttemptID { get; set; }
        public int PackageID { get; set; }
        public string AttemptName { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDT { get; set; }
    }
}
