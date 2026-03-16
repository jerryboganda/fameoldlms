using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

namespace First_Aid_Made_Easy.Models
{
    public class QuestionSystemVM
    {

        public int ID { get; set; }
        public string Name { get; set; }
        public string Section { get; set; }
        public bool IsActive { get; set; }
        public bool IsForStudent { get; set; }
    }
    public class SystemVM
    {

        public int? ID { get; set; }
        public string Name { get; set; }
        public string Section { get; set; }
        public bool IsAvailable { get; set; }
        public List<sp_QuestionCountByStudent_Result> QCount { get; set; }
    }

    public class sp_QuestionSystemCountByStudent_Result
    {
        public string QType { get; set; }
        public int NoOfQ { get; set; }
        public int? SystemID { get; set; }
        public string SystemName { get; set; }
        public string Section { get; set; }
    }

    public class QuestionsVM
    {
        public int? ID { get; set; }
        public int? Type { get; set; }
    }
    public class QuestionPaperVM
    {
        public int PaperID { get; set; }
        public string PaperTitle { get; set; }
        public int? Time { get; set; }
        public Nullable<int> PassPer { get; set; }
        public string ExpiryTime { get; set; }
        public string StartTime { get; set; }
        public string AnswerTime { get; set; }
        public DateTime? ExpiryDT { get; set; }
        public DateTime? StartDT { get; set; }
        public DateTime? AnswerDT { get; set; }
        public string Thumbnail { get; set; }
        public string Notes { get; set; }
        public string TopicsCovered { get; set; }
        public string ExamIDs { get; set; }
        public string CourseIDs { get; set; }
        public string PackageIDs { get; set; }
        public string SectionIDs { get; set; }
        public string Tags { get; set; }
        public Nullable<int> DifficultyLevel { get; set; }
        public Nullable<int> MockTestType { get; set; }
        public string UniversityID { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public int? Marks { get; set; }
        public int? TotalMarks { get; set; }
        public List<QuestionsVM> Questions { get; set; }
        public List<QuestionVM> QuestionList { get; set; }
        public List<sp_lstQuestionPapers_Result> List { get; set; }
    }

}