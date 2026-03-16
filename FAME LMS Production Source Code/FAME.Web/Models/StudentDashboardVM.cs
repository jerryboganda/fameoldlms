using First_Aid_Made_Easy.DAL;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.Models
{
    public class StudentDashboardVM
    {
        public UserVM User { get; set; }
        public sp_CurrentPackDetail_Result CurrentEnrollment { get; set; }
        public List<sp_GetMeetingsForStudent_Result> UpcomingClasses { get; set; }
        public List<CourseVM> RecentCourses { get; set; }

        
        // UI Helper Properties
        public bool HasActiveSubscription => CurrentEnrollment != null && CurrentEnrollment.Enrollment_EndDate > System.DateTime.Now;
        public int DaysRemaining => (CurrentEnrollment?.Enrollment_EndDate.HasValue == true) 
            ? (CurrentEnrollment.Enrollment_EndDate.Value - System.DateTime.Now).Days 
            : 0;
    }
}
