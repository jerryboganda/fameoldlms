using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

namespace First_Aid_Made_Easy.Models
{
    public class ExamVM
    {

        public int ExamID { get; set; }
        public int? Price { get; set; }
        public string ExamTitle { get; set; }
        public Nullable<System.TimeSpan> Time { get; set; }
        public string Thumbnail { get; set; }
        public string TopicsCovered { get; set; }
        public string CourseIDs { get; set; }
        public string PackageIDs { get; set; }
        public string Tags { get; set; }
        public Nullable<int> DifficultyLevel { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public List<int?> Papers { get; set; }
        public List<QuestionPaperVM> PaperList { get; set; }
        public List<sp_lstExams_Result> List { get; set; }
    }

}