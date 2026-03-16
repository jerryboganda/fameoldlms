using System;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IQuestionRepository
    {
        List<QuestionVM> GetQuestionsBySection(int sectionID);
        List<sp_lstQuestions_Result> GetList(QuestionType type, int PartitionID, string tags, string Text, string CreatedBY = "", int SystemID = 0);
        List<sp_lstVideoFaqs_Result> GetList(int VideoID);
        bool DeleteQuestion(int id);
        QuestionVM SaveQuestion(QuestionVM vid);
        bool SaveQuestions(List<QuestionVM> list);
        List<QuestionVM> AllMcqs(int? Type);
        QuestionVM GetByID(int? id);
        int GetCourseFid(int sectionID);
        bool PostComment(tbl_Comment comment);
        sp_GetReview_Result SaveReview(int ID, string Type, string UserID, int Stars);
        sp_GetReview_Result GetReview(int ID, string Type, string UserID);
        bool SaveNotesTags(tbl_Notes comment);
        Tuple<string, string> GetNotesTags(int QuestionID, string Uid);
        List<sp_lstComments_Result> GetComments(int QuestionID = 0, int VideoID = 0, bool? isApproved = null);
        List<sp_lstApproveComments_Result> GetApproveComments(string isApproved = "All");
        bool ApproveComments(int ID, string ApprovedBy, bool IsApproved);
    }
}
