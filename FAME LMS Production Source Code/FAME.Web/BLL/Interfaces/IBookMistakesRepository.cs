using First_Aid_Made_Easy.DAL;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IBookMistakesRepository
    {
        bool Create(tbl_BookMistakes table);
        tbl_BookMistakes GetByID(int id);
        List<tbl_BookMistakes> GetList();
        bool Delete(int iD);
    }
}
