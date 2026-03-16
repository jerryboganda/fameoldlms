using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.BLL
{
    public class VideoRepository : IVideoRepository
    {
        public List<VideoVM> GetVideosByType(ContentType Type)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    int t = (int)Type;
                    List<tbl_Video> list = db.tbl_Video.Where(x => x.Type == t).OrderBy(x => x.SortID).ToList();
                    List<VideoVM> slist = list.ConvertAll(x => new VideoVM
                    {
                        Video_Id = x.Video_Id,
                        Video_Name = x.Video_Name,
                        Video_Tags = x.Video_Tags,
                        Video_ShortDescription = x.Video_ShortDescription,
                        Video_Length = x.Video_Length,
                        Video_Path = x.Video_Path,
                        Type = x.Type ?? (int)ContentType.Video,
                        Difficulty = Common.DifficultyS(x.Difficulty),
                    });
                    return slist;
                }
                catch
                {
                    throw;
                }
            }
        }

        public List<VideoVM> GetVideosBySection(int SectionID, string Student_Fid)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var list = db.sp_lstVideos(Student_Fid, "", "", SectionID.ToString(), "", "").ToList();
                    List<VideoVM> slist = list.ConvertAll(x => new VideoVM
                    {
                        Video_Id = x.Video_Id,
                        Video_Name = x.Video_Name,
                        Video_Tags = x.Video_Tags,
                        Video_ShortDescription = x.Video_ShortDescription,
                        Video_Length = x.Video_Length,
                        Video_Path = x.Video_Path,
                        Type = x.Type ?? (int)ContentType.Video,
                        Difficulty = Common.DifficultyS(x.Difficulty),
                        IsWatched = x.isCompleted ?? false,
                        ScreenTime = x.ScreenTime ?? 0,
                        SortID = x.SortID ?? 0,

                    });
                    return slist;
                }
                catch
                {
                    throw;
                }
            }
        }

        public VideoVM GetVideo(int videoID, int SecID, string UserID)
        {
            // For getting link of video in the view
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    if (videoID == 0)
                    {
                        videoID = db.sp_VideoForFirst(0, SecID, UserID).FirstOrDefault() ?? 0;
                    }
                    tbl_Video v = db.tbl_Video.Find(videoID);
                    VideoVM video = new VideoVM()
                    {
                        Video_Path = v.Video_Path,
                        Video_Name = v.Video_Name,
                        Video_Length = v.Video_Length,
                        Video_Tags = v.Video_Tags,
                        Section_Fid = v.Section_Fid,
                        Course_Fid = v.Course_Fid,
                        Type = v.Type,
                        Video_ShortDescription = v.Video_ShortDescription,
                        Video_Id = v.Video_Id,
                        Difficulty = v.Difficulty,
                    };
                    return video;
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool isWatched(int videoID, string Student_Fid, bool? IsCompleted)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    tbl_Progress p = db.tbl_Progress.Find(videoID, Student_Fid);
                    if (p != null)
                    {
                        if (IsCompleted != null) p.isCompleted = IsCompleted;
                        else
                        {
                            p.ScreenTime++;
                            p.LastOpenDT = Common.GetCurrentDate();
                        }
                    }
                    else
                        db.tbl_Progress.Add(new tbl_Progress
                        {
                            Video_Fid = videoID,
                            Student_Fid = Student_Fid,
                            ScreenTime = 1,
                            isCompleted = IsCompleted,
                            LastOpenDT = Common.GetCurrentDate(),
                        });
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public bool DeleteVideo(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var v = db.tbl_Video.Find(id);
                    db.tbl_Video.Remove(v);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public VideoVM SaveVideo(VideoVM video)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {


                    tbl_Video model = new tbl_Video();
                    if (video.Video_Id > 0)
                    {
                        model = db.tbl_Video.Find(video.Video_Id);
                    }

                    model.Video_Id = video.Video_Id;
                    model.Video_Name = video.Video_Name;
                    model.Video_Path = video.Video_Path;
                    model.Video_ShortDescription = video.Video_ShortDescription;
                    model.Video_Tags = video.Video_Tags;
                    model.Video_Length = video.Video_Length;
                    model.Section_Fid = video.Section_Fid == 0 ? null : video.Section_Fid;
                    model.SectionSub_Fid = video.SectionSub_Fid == 0 ? null : video.SectionSub_Fid;
                    model.Course_Fid = video.Course_Fid == 0 ? null : video.Course_Fid;
                    model.Type = video.Type;
                    model.Difficulty = video.Difficulty;


                    if (video.File != null)
                        model.Video_Path = "~/Images/" + Common.SavePic(video.File, "file/");
                    else
                        model.Video_Path = video.Video_Path;

                    if (!(video.Video_Id > 0))
                    {
                        db.tbl_Video.Add(model);
                    }
                    db.SaveChanges();
                    video.Video_Id = model.Video_Id;
                    return video;
                }
                catch
                {
                    return new VideoVM
                    {
                        Section_Fid = video.Section_Fid,
                        Course_Fid = video.Course_Fid
                    };
                }
            }
        }

        public VideoVM GetByID(int? id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    tbl_Video model = db.tbl_Video.Find(id);

                    VideoVM s = new VideoVM()
                    {
                        Video_Id = model.Video_Id,
                        Video_ShortDescription = model.Video_ShortDescription,
                        Video_Name = model.Video_Name,
                        Video_Tags = model.Video_Tags,
                        Video_Path = model.Video_Path,
                        Video_Length = model.Video_Length,
                        Type = model.Type,
                        SectionSub_Fid = model.SectionSub_Fid,
                        Difficulty = model.Difficulty,
                        Section_Fid = model.Section_Fid,
                        Course_Fid = model.Course_Fid,
                        SortID = model.SortID ?? 0,
                    };


                    return s;
                }
                catch
                {
                    return null;
                }
            }
        }
        public int GetCourseFid(int? id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    tbl_Section model = db.tbl_Section.Find(id);
                    return model.Course_Fid;
                }
                catch
                {
                    return 0;
                }
            }
        }
    }
}