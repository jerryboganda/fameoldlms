using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.Models
{
    public class SectionSubVM
    {
        public int ID { get; set; }
        public string SectionSub_Name { get; set; }
        public int Course_Fid { get; set; }
        public int Section_Fid { get; set; }
        public Nullable<int> SortID { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public List<VideoVM> VideoList { get; set; }
        public decimal DurationInMin { get; set; }
        public int Lessons { get; set; }
    }
    public class SectionVM : IsEnrolled
    {
        public int Section_ID { get; set; }
        public string Section_Name { get; set; }
        public string Course_Name { get; set; }
        public string Course_Pic { get; set; }
        public string TeacherFid { get; set; }
        public int Course_Fid { get; set; }
        public int SortID { get; set; }
        public decimal Section_Price { get; set; }
        public List<VideoVM> VideoList { get; set; }
        public List<SectionSubVM> SectionSubList { get; set; }
        public List<QuestionVM> QuestionList { get; set; }
    }
    public class IsEnrolled
    {
        public DateTime? EndDate { get; set; }
        public DateTime? NextInstallment { get; set; }
        public decimal? InstallmentPrice { get; set; }
        public int PendingInstallment { get; set; }
        public bool isEnrolled { get { return EndDate > Common.GetCurrentDate(); } }
        public bool isExpired { get; set; }
        public double RemainingDays { get; set; }
        public int Lessons { get; set; }
        //public int Audios { get; set; }
        //public int Mcqs { get; set; }
        //public int Exams { get; set; }
        public decimal DurationInMin { get; set; }
        //public decimal AudiosInMin { get; set; }
        //public decimal ExamsInMin { get; set; }
        public decimal WatchedVideos { get; set; }
        public int Views { get; set; }
        //public decimal WatchedDur { get; set; }
        public int Progress { get { if (Lessons == 0) return 0; else return Convert.ToInt32((WatchedVideos / Lessons) * 100); } set { } }
    }
}