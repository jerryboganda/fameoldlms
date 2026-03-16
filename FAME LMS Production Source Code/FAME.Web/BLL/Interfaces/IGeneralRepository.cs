using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IGeneralRepository
    {
        bool CreateAgentCategory(tbl_Master model);
        bool DeleteAgentCategory(int id);
        bool Create(tbl_BookCode model);
        bool UseBookCode(string code, string id);
        List<tbl_Master> GetList(int group);
        List<tbl_BookCode> GetList();
        List<ReminderVM> GetReminderList(string id, DateTime? DateFrom = null);
        MasterVM GetMasterByGroup(MasterGroup mg);
        bool CreateMaster(MasterVM model);
        Task<Tuple<bool, string, string>> SaveDevice(string ID, string DevieID, bool IsAdmin);
        bool SubscribeEmail(string email);
        bool RemoveDevice(string id, string UserID = "", int ReqID = 0, bool IsAccepted = true);
        List<sp_lstDeleteDeviceReq_Result> GetDeleteReqList(string UserID = "", bool? IsAccepted = null);
        List<tbl_UserDevices> GetActiveUserDevices(string userID);
        bool ResetUserDevices(string userID);
        bool AddDeviceDeleteRequest(string deviceID, string userID, string notes);
        bool? IsDeviceValid(string deviceID, string userID);
        bool SaveNotes(EditStudentDetail m);
        bool UpdateAspNetUser(string id, string email);
        bool UpdateAspNetUser(AspNetUsers u);
        bool SaveUserDevice(tbl_UserDevices model);
        bool SelectMockTests(int id, string UID);
        List<sp_SearchContent_Result> SearchContent(string query);
        tbl_Reminder SaveReminder(tbl_Reminder rem);
        bool DeleteReminder(int id);
    }
}
