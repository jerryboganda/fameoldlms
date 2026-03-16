using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using First_Aid_Made_Easy.DAL;

namespace First_Aid_Made_Easy.Models
{
    public class RequestVM
    {
        public int RequestID { get; set; }
        public string StudentID { get; set; }
        public int? CourseID { get; set; }
        [Required]
        public int PackageID { get; set; }
        public int? ExamAttemptID { get; set; }
        public Nullable<int> SectionID { get; set; }
        public System.DateTime RequestDate { get; set; }
        [Required]
        public int Duration { get; set; }
        public List<sp_lstRequests_Result> List { get; set; }
        public  tbl_User Student{ get; set; }
        public string PackageName { get; set; }
        public string CourseName { get; set; }
        public string StudentName { get;set; }
        public string StudentEmail { get; set; }
        public string SectionName { get; set; }
        public string Payment { get; set; }
        public string DurationS { get; set; }
        public string RequestFor { get; set; }
        public decimal? Price { get; set; }
        public bool? IsAccepted { get; set; }
        public bool HasInstall { get; set; }
    }
}