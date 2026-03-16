using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IVideoRepository
    {
        List<VideoVM> GetVideosByType(ContentType Type);
        List<VideoVM> GetVideosBySection(int sectionID, string StudentFid);
        VideoVM GetVideo(int videoID, int SecID, string UserID);
        bool isWatched(int videoID, string Student_Fid, bool? IsCompleted);
        bool DeleteVideo(int id);
        VideoVM SaveVideo(VideoVM vid);
        VideoVM GetByID(int? id);
        int GetCourseFid(int? sectionID);
    }
}
