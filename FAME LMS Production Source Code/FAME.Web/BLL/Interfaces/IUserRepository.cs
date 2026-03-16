using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IUserRepository
    {
        List<UserVM> GetList();
        UserVM GetProfile(string id);
        UserVM GetByID(string id);
        UserVM GetByID(int id, string UserID);
        bool Save(UserVM user);
        bool Delete(string id);
        List<SelectListItem> GetStudentsByType(int type);
        List<tbl_Master> GetAgentCategories();
        StudentDetail GetStudentDetail(int iD, string ID = null);
        bool SaveRoles(string UserID, int?[] Roles);
        List<string> GetEmailsByRole(string roleName);
        int GetLoginCount(string userId);
        List<string> GetUserPics(System.DateTime since);
    }
}
