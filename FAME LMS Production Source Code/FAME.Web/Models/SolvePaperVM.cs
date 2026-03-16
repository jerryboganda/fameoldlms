using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.Models
{
    public class SolvePaperDto
    {
        public int ID { get; set; }
        public int? QuestionPaperID { get; set; }
        public string PaperTitle { get; set; }
        public int? ObtainedMarks { get; set; }
        public int? TotalMarks { get; set; }
        public string Mode { get; set; }
        public int? QuestionCount { get; set; }
        public int? PassPer { get; set; }
        public int? TimeElapsed { get; set; }
        public int? TimeGiven { get; set; }
        public bool AllowReopen { get; set; }
        public bool IsComplete { get; set; }
        public DateTime? AnswerAt { get; set; }

        public DateTime? Datetime { get; set; }
        public string StudentID { get; set; }

        public List<PaperQuestionDetail> Details { get; set; }
    }

    public class PaperQuestionDetail
    {
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string AnsExplain { get; set; }
        public Nullable<int> TimeSpent { get; set; }
        public Nullable<int> Marks { get; set; }
        public Nullable<bool> IsTrue { get; set; }
        public Nullable<bool> IsPopular { get; set; }
        public List<OptionVM> QuestOptions { get; set; }
        public bool IsSolved { get { return IsTrue != null; } }
    }

}