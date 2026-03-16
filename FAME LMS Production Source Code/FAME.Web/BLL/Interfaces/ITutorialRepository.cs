using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface ITutorialRepository
    {
        List<TutorialVM> GetList();
        bool DeleteTutorial(int id);
        TutorialVM SaveTutorial(TutorialVM Tutorial);
        TutorialVM GetByID(int? id);
    }
}
