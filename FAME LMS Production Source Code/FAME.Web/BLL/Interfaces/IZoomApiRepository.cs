using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IZoomApiRepository
    {
        MeetingVM GetByID(long id);
        VideoVM GetVideo(int id);
        VideoVM SaveVideo(VideoVM model);
        bool SavePic(string FileName, int ID);
        bool Delete(int ID);
        List<sp_GetMeetingsForStudent_Result> GetMyMeetings(string ID, string Status = "", string DateFrom = "");
        List<MeetingVM> GetMeetingList(string userId);
        bool DeleteMeeting(long meetingNo);
        bool UpdateMeetingStatus(long meetingNo, string status);
        Task<bool> SaveMeetingLocal(string CourseIds, string PackageIds, string StartTime, Meeting meet, string password, bool New, string userId);
        int GetMeetingVideoId(int meetingId);
    }
}
