using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IDashBoardRepository
    {
        DashBoardVM Get();
        List<sp_DashBoardCounts_Result> GetCounts();
        List<CountVM> GetCourseEnrl();
        List<CountVM> GetSectionEnrl();
        List<CountVM> GetStudentsByUni();
        List<sp_lstStudents_Result> GetStudentList(string text, int Type, string DateFrom, string DateTo);
        List<sp_GetUsersListByRole_Result> GetUserList(int role, int CategoryID = 0, string Email = "");
        bool AgentStatus(string ID, bool Status);
        List<sp_lstVideos_Result> StudentProgress(string ID);
        Task<StudentDetail> GetStudentDetailAsync(int iD, string ID = null);
        Task<List<SubscriptionsVM>> GetPackSubscriptions(string id);
        List<PackageEnrollments> GetPackageEnrl();
        List<sp_ListEnrollments_Result> GetEnrollmentList(string datef, string datet, string Sid, int CourseID, int PackageID, bool? IsExpire, string AddedBy, string ApproveBy, string City, string Institute);
        List<AspNetUsers> GetPartiallyRegistered();
    }
}
