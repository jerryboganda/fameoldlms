using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Models
{
    public class OptionVM
    {
        public string OptionText { get; set; }
        public string Details { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsChecked { get; set; }


        //==========  For Statistics ===========
        public int? CheckedByStudents { get; set; }
        public decimal? CheckedByStudentsPerc { get; set; }
    }

    public class QuestionVM
    {
        public int Question_Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public string Tags { get; set; }
        [AllowHtml]
        public string AnsExplain { get; set; }
        public Nullable<DateTime> Time { get; set; }
        public Nullable<int> DifficultyLevel { get; set; }
        public Nullable<int> Section_Fid { get; set; }
        public Nullable<int> Course_Fid { get; set; }
        public Nullable<int> VideoID { get; set; }
        public int Type { get; set; }
        public string TypeS { get { switch (Type) { case (int)QuestionType.Mcq: return "MCQs"; case (int)QuestionType.Short: return "Short Question"; case (int)QuestionType.Long: return "Long Question"; default: return "FAQ"; }; } set { } }

        public bool IsPopular { get; set; }
        public bool IsSolved { get; set; }
        public bool IsMultiAns { get; set; }
        public Nullable<int> CategoryID { get; set; }
        public Nullable<int> SystemID { get; set; }
        public Nullable<int> PartitionID { get; set; }
        public Nullable<int> PaperID { get; set; }
        public List<OptionVM> Options { get; set; }
        public List<sp_lstQuestions_Result> List { get; set; }
        public int? Marks { get; set; }
        public string CreatedBy { get; set; }

        //==========  For Statistics ===========
        public int? Attempts { get; set; }

    }

}