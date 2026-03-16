using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.Models
{
    public class EnrollmentMasterVM
    {
        public int Enrollment_Id { get; set; }
        public System.DateTime Enrollment_Date { get; set; }
        public string Enrollment_Status { get; set; }
        public decimal Enrollment_Price { get; set; }
        public int Enrollment_No { get; set; }
        public int? CouponFid { get; set; }
        public Nullable<System.DateTime> Enrollment_EndDate { get; set; }
        public string EndDate { get; set; }
        public string TeacherFid { get; set; }
        public string StudentFid { get; set; }
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public string ApprovedBy { get; set; }
        public Nullable<bool> IsExpired { get; set; }
        public Nullable<bool> ByRequest { get; set; }
        public Nullable<bool> ByManual { get; set; }
        public Nullable<bool> WithPackage { get; set; }
        public Nullable<int> PackageDurationFid { get; set; }
        public Nullable<int> PackageId { get; set; }
        public List<EnrollmentDetailVM> Details { get; set; }
        public List<EnrollmentMasterVM> List { get; set; }
        public bool? ByExtension { get; set; }
        public int? ExamAttemptID { get; set; }
    }

    public class EnrollmentDetailVM
    {
        public int ID { get; set; }
        public Nullable<int> EnrollmentID { get; set; }
        public Nullable<int> Section_Fid { get; set; }
        public Nullable<int> CourseID { get; set; }
        public Nullable<bool> IsExpired { get; set; }
    }
    public class AddSubscriptioVM
    {
        public string UserID { get; set; }
        public string StudentFid { get; set; }
        public int?[] CourseFid { get; set; }
        public int?[] SectionFid { get; set; }
        public string EndDate { get; set; }
        public bool Expire { get; set; }
        public string Payment { get; set; }
        public string EnrollmentFor { get; set; }
        public int? PackageID { get; set; }
        public int? Duration { get; set; }
        //public int? DurationID { get; set; }
        public int? UniversityID { get; set; }
        public decimal? Price { get; set; }
        public int? ExamAttemptID { get; set; }
    }

    public class UniversityEnrollVM
    {

    }
}