using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IStudentGroupRepository
    {
        List<StudentGroupVM> GetList(int UniID = 0);
        bool DeleteStudentGroup(int id);
        StudentGroupVM Save(StudentGroupVM model);
        StudentGroupVM GetByID(int? id);
        bool SavePic(string FileName, int ID);
    }
}
