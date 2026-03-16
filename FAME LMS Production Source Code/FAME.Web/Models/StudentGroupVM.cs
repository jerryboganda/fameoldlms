using First_Aid_Made_Easy.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.Models
{
    public class StudentGroupDetailVM
    {
        public string StudentID { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public Nullable<bool> IsLeader { get; set; }
    }
    public class StudentGroupVM
    {
        public int Id { get; set; }
        public string GroupTitle { get; set; }
        public string Pic { get; set; }
        public string UniversityName { get; set; }
        public Nullable<int> UniversityID { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public virtual List<StudentGroupDetailVM> Detail { get; set; }
        public virtual List<StudentGroupVM> List { get; set; }
    }
}