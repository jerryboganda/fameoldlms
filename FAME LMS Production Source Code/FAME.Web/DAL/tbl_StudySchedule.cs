using System;

namespace First_Aid_Made_Easy.DAL
{
    public class tbl_StudySchedule
    {
        public int StudyScheduleID { get; set; }
        public int ExamAttemptID { get; set; }
        public int ScheduleType { get; set; } // 1=Structured, 2=Text/HTML, 3=File, 4=Calendar
        public int? WeekNumber { get; set; }
        public string TopicTitle { get; set; }
        public string TopicDescription { get; set; }
        public string FilePath { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDT { get; set; }
    }
}
