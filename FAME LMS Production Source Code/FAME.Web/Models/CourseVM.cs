using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Models
{
    public class MyCoursesVM
    {
        public List<CourseVM> CorList { get; set; }
        public List<SectionVM> SectionList { get; set; }
        public List<PackageVM> PackageList { get; set; } = new List<PackageVM>();
    }
    public class CourseVM : IsEnrolled
    {
        public int Course_Id { get; set; }
        [Required]
        public string Course_Name { get; set; }
        public string Course_Description { get; set; }
        public string Course_Pic { get; set; }
        public decimal Course_Price { get; set; }
        public string Teacher_Name { get; set; }
        public string Teacher_Pic { get; set; }
        public string Teacher_Fid { get; set; }
        public int SortID { get; set; }
        public List<CourseDetailVM> Details { get; set; }
        public List<CourseVM> CorList { get; set; }
        public List<SectionVM> SectionList { get; set; }
        public List<ChapterVM> ChapterList { get; set; }
    }
    public class SortingVM
    {
        public int?[] Ids { get; set; }
        public string Type { get; set; }
    }
    public class ChapterVM
    {

        public int ChapterID { get; set; }
        public int? Course_Id { get; set; }
        public int? SortID { get; set; }
        public string Title { get; set; }
        [AllowHtml]
        public string Content { get; set; }
    }
}