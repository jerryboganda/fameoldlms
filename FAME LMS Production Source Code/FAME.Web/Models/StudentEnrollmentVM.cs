using System;

namespace First_Aid_Made_Easy.Models
{
    public class StudentEnrollmentVM
    {
        public int EnrollmentId { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public int? CurrentExamAttemptId { get; set; }
        public string CurrentExamAttemptName { get; set; }
        public DateTime? EnrollmentEndDate { get; set; }
        public int? NewExamAttemptId { get; set; }
    }
}
