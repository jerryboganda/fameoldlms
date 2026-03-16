using System;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.Models
{

    public class CreateTestInput
    {
        public int ID { get; set; }
        public string StudentID { get; set; }
        public string TrialStudent { get; set; }
        public string Mode { get; set; }
        public string Systems { get; set; }
        public string QuestionMode { get; set; }
        public Nullable<int> maxQPerBlock { get; set; }
    }

    public class ResultVM
    {
        public int ID { get; set; }
        public Nullable<System.DateTime> Datetime { get; set; }
        public string StudentID { get; set; }
        public string Mode { get; set; }
        public Nullable<int> maxQPerBlock { get; set; }
        public Nullable<int> TimeElapsed { get; set; }
        public Nullable<int> QuestionPaperID { get; set; }
        public Nullable<int> ObtainedMarks { get; set; }
        public Nullable<int> TotalMarks { get; set; }
        public Nullable<int> TimeGiven { get; set; }
        public List<ResultDetailVM> Detail { get; set; }
        public string PaperTitle { get; set; }
        public Nullable<System.DateTime> AnswerAt { get; set; }
        public bool IsComplete { get; set; }
        public bool AllowReopen { get; set; }
        public Nullable<int> PassPer { get; set; }
    }
    public class ResultDetailVM
    {
        public int ID { get; set; }
        public Nullable<int> ResultID { get; set; }
        public Nullable<int> QuestionID { get; set; }
        public string Answer { get; set; }
        public Nullable<int> TimeSpent { get; set; }
        public Nullable<bool> IsTrue { get; set; }
        public Nullable<int> TimeElapsed { get; set; }
    }
}