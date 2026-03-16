using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IDailyReportAuthRepository
    {
        DailyReportUserVM ValidateUser(string username, string password);
        bool IsUsernameTaken(string username);
        bool RegisterUser(DailyReportRegisterVM model);
        List<DailyReportUserVM> GetAllUsers();
        DailyReportUserVM GetUserById(int id);
        bool UpdateUser(DailyReportUserVM model);
        bool DeleteUser(int id);
        bool CreateAdminUser(DailyReportUserVM model);
        bool HasAnyUsers();
        bool IsAdmin(int userId);
    }
}
