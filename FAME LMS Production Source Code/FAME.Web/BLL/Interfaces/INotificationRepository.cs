using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface INotificationRepository
    {
        bool Create(StudentNotifVM model);
        object Delete(int id);
        List<sp_GetNotifications_Result> GetNotifications(string id);
        List<sp_GetNotifications_Result> CheckForNotification(string id);
        bool HaveSeen(string Sid, int NId);
        bool HaveSeen(string Sid, string NIds);
        StudentNotifVM GetMessage();
        StudentNotifVM GetByID(int id);
        List<StudentNotifVM> GetList();
        tbl_StudentNotif IsNotificationTrue(int id, string secret, bool isCourse);
    }
}
