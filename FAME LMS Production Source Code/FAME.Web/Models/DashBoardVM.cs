using First_Aid_Made_Easy.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.Models
{
    public class DashBoardVM
    {
        public List<sp_DashBoardCounts_Result> Counts { get; set; }
        public List<CountVM> SectionEnrollments { get; set; }
        public List<CountVM> CourseEnrollments { get; set; }
        public List<CountVM> Students { get; set; }
        public List<PackageEnrollments> PackageEnrollments { get; set; }
    }
    public class DashBoardCountVM
    {
        public int Students { get; set; }
        public int Enrollments { get; set; }
        public int InActiveStudents { get; set; }
        public int Requests { get; set; }
        public int ActiveStudents { get; set; }
        public int EnrollmentsThisWeek { get; set; }
    }

    public class CountVM
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }
    public class PackageEnrollments
    {
        public string PackageName { get; set; }
        public string TeacherName { get; set; }
        public int Enrollments { get; set; }
    }
}