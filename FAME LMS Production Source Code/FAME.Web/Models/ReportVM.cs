using System;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.Models
{
    public class EmailReportVM
    {
        public string StudentID { get; set; }
        public string StudentName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Nullable<int> InactiveDays { get; set; }
        public Nullable<int> Type { get; set; }
        public string Data { get; set; }
        public Nullable<System.DateTime> SentAt { get; set; }
    }

    public class TrialStudentsVM
    {
        public string Student { get; set; }
        public int Count { get; set; }
        public DateTime? LastTest { get; set; }
    }

    public class SubscriptionReport
    {
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string Sid { get; set; }
        public int CourseID { get; set; }
        public int PackageID { get; set; }
        public bool? IsExpire { get; set; }
        public bool IsPDF { get; set; }
        public bool IsExcel { get; set; }
        public bool IsSummary { get; set; }
        public string AddedBy { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public string PackageName { get; set; }
        public string City { get; set; }
        public string Institute { get; set; }

    }

    public class sp_ListEnrollments_Result
    {
        public Nullable<int> Enrollment_Id { get; set; }
        public Nullable<System.DateTime> Enrollment_Date { get; set; }
        public string Enrollment_Status { get; set; }
        public Nullable<decimal> Enrollment_Price { get; set; }
        public Nullable<int> Duration { get; set; }
        public Nullable<int> PackageID { get; set; }
        public Nullable<System.DateTime> Enrollment_EndDate { get; set; }
        public string StudentName { get; set; }
        public string Email { get; set; }
        public Nullable<bool> ByRequest { get; set; }
        public Nullable<bool> ByManual { get; set; }
        public string MobileNo { get; set; }
        public string Course_Name { get; set; }
        public Nullable<bool> IsExpired { get; set; }
        public string ApproveBy { get; set; }
        public string PackageName { get; set; }
        public string City { get; set; }
        public string Institute { get; set; }
    }
}