using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IQuestionPartitionRepository
    {
        List<QuestionSystemVM> GetList();
        bool Delete(int id);
        QuestionSystemVM Save(QuestionSystemVM d);
        QuestionSystemVM GetByID(int? id);
    }
}
