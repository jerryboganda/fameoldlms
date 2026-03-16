using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IQuestionPaperRepository
    {
        List<sp_lstQuestionPapers_Result> GetList(string ForStudent = "", bool? mockTest = null);
        bool DeleteQuestionPaper(int id);
        QuestionPaperVM Save(QuestionPaperVM model);
        QuestionPaperVM GetByID(int? id);
        SolvePaperDto GetQuestionToSolve(int id);
        QuestionPaperVM GetByResultID(int id);
        bool SavePic(string FileName, int ID);
        ResultVM SaveResult(ResultVM vid);
        CreateTestInput AutoCreateResult(CreateTestInput result);
        int AutoCreateResultFromPaper(int id, string m, string uid);
        List<ResultVM> ResultList(string Uid, bool MockTest = false);
        List<ResultVM> InCompleteResultList();
        ResultDetailVM SaveResult(ResultDetailVM vid);
        ResultVM GetResultByID(int resultID);
        bool RecheckQuestion(int QPID, int QuestionID);
        void Completed(int ID);
        void DeleteResult(int ID);
        void Suspend(int ID);
        bool AllowReopen(int ID);
        List<sp_QuestionCountByStudent_Result> GetQuestionCount(string SID, string SysID = "");
        List<SystemVM> GetSystems(string SID);
        List<SystemVM> GetFreeSystems(string Type);
        List<sp_TestsPerformance_Result> TestsPerformance(string id);
    }
}
