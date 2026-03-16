using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IExamRepository
    {
        List<sp_lstExams_Result> GetList(int difficultyLevel, string tags, string Text = "", string CreatedBY = "");
        bool DeleteExam(int id);
        ExamVM Save(ExamVM model);
        bool SavePic(string FileName, int ID);
        ExamVM GetByID(int? id);
    }
}
