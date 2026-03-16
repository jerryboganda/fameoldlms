using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.BLL
{
    public class ZoomApiRepository : IZoomApiRepository
    {
        public MeetingVM GetByID(long id)
        {

            using (FAMEEntities db = new FAMEEntities())
            {
                var meet = db.tbl_Meeting.FirstOrDefault(x => x.MeetingNo == id);
                var model = new MeetingVM
                {
                    MeetingNo = id,
                    CourseIDs = meet.CourseIDs,
                    PackageIDs = meet.PackageIDs,
                    Duration = meet.Duration,
                    Password = meet.Password,
                    Topic = meet.Topic,
                    ID = meet.ID,
                    StartTime = meet.StartTime ?? Common.GetCurrentDate(),
                };
                return model;
            }
        }
        public VideoVM GetVideo(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var meet = db.tbl_MeetingVideo.Find(id);
                var model = new VideoVM
                {
                    Video_Id = meet.MeetVideoID,
                    MeetID = meet.MeetID,
                    Video_Name = meet.Title,
                    Video_ShortDescription = meet.Description,
                    Video_Tags = meet.Tags,
                    Video_Path = meet.Path
                };
                return model;
            }
        }
        public VideoVM SaveVideo(VideoVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var meet = new tbl_MeetingVideo();
                if (model.Video_Id > 0)
                    meet = db.tbl_MeetingVideo.Find(model.Video_Id);
                meet.Title = model.Video_Name;
                meet.Description = model.Video_ShortDescription;
                meet.Tags = model.Video_Tags;
                meet.MeetID = model.MeetID;
                if (model.Video_Id == 0)
                    db.tbl_MeetingVideo.Add(meet);
                db.SaveChanges();
                model.Video_Id = meet.MeetVideoID;
                return model;
            }
        }
        public bool SavePic(string FileName, int ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var qp = db.tbl_MeetingVideo.Find(ID);
                qp.Path = FileName;
                db.SaveChanges();
                return true;
            }
        }
        public bool Delete(int ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var meet = db.tbl_MeetingVideo.Find(ID);
                db.tbl_MeetingVideo.Remove(meet);
                db.SaveChanges();
                return true;
            }
        }
        public List<sp_GetMeetingsForStudent_Result> GetMyMeetings(string ID , string Status = "" , string DateFrom = "")
        {


            using (FAMEEntities db = new FAMEEntities())
            {
                var List = db.sp_GetMeetingsForStudent(ID, Status,Common.TryStringToDate(DateFrom)).ToList();
                return List;
            }
        }

        public List<MeetingVM> GetMeetingList(string userId)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var list = db.tbl_Meeting.Where(x => x.CreatedBy == userId).ToList();
                return list.ConvertAll(x => new MeetingVM
                {
                    MeetingNo = x.MeetingNo,
                    Duration = x.Duration,
                    CourseIDs = x.CourseIDs,
                    PackageIDs = x.PackageIDs,
                    JoinUrl = x.JoinUrl,
                    Password = x.Password,
                    StartTime = x.StartTime,
                    Topic = x.Topic,
                    ID = x.ID,
                    Status = x.Status ?? MeetStatus.UpComing.ToString(),
                    VideoID = x.tbl_MeetingVideo.FirstOrDefault()?.MeetVideoID ?? 0,
                });
            }
        }

        public bool DeleteMeeting(long meetingNo)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var meet = db.tbl_Meeting.FirstOrDefault(x => x.MeetingNo == meetingNo);
                    if (meet != null)
                    {
                        db.tbl_Meeting.Remove(meet);
                        db.SaveChanges();
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool UpdateMeetingStatus(long meetingNo, string status)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var data = db.tbl_Meeting.FirstOrDefault(x => x.MeetingNo == meetingNo);
                    if (data != null)
                    {
                        data.Status = status;
                        db.SaveChanges();
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SaveMeetingLocal(string CourseIds, string PackageIds, string StartTime, Meeting meet, string password, bool New, string userId)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var list = db.sp_GetEnrollmentByCourse(CourseIds, PackageIds, false).Select(x => x.Email).ToList();
                bool NotSend = Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["NotSend"]);

                if (!NotSend && New)
                    await Common.SendMail(new MessageVM
                    {
                        Body = "<h1 style=\"text-align: center; \"><b><font style=\"background-color: rgb(255, 255, 255);\" color=\"#9c00ff\">First Aid Made Easy</font></b></h1>" +
                          "<h2 style=\"text-align: center; \"><font color=\"#311873\">A live class has been arranged for you.</font></h2>" +
                          "<h2 style=\"text-align: center; \"><font color=\"#311873\">Class Start Time is :  \" " + StartTime + " \"</font></h2>",
                        Subject = "FAME Live Class",
                        Destinations = list
                    });

                if (meet?.id == 0 && New) return false;
                if (meet?.id > 0 && New)
                {
                    tbl_Meeting m = new tbl_Meeting()
                    {
                        CourseIDs = CourseIds,
                        PackageIDs = PackageIds,
                        MeetingNo = meet.id,
                        Duration = meet.duration,
                        JoinUrl = meet.join_url,
                        Password = password,
                        Topic = meet.topic,
                        StartTime = Common.TryStringToDateTime(StartTime),
                        CreatedBy = userId,
                        CreatedDT = Common.GetCurrentDate(),
                        Status = MeetStatus.UpComing.ToString(),
                    };
                    db.tbl_Meeting.Add(m);
                    await db.SaveChangesAsync();
                }
                else if (meet?.id > 0)
                {
                    var table = db.tbl_Meeting.FirstOrDefault(x => x.MeetingNo == meet.id);
                    if (table != null)
                    {
                        table.CourseIDs = CourseIds;
                        table.PackageIDs = PackageIds;
                        table.Duration = meet.duration;
                        table.Topic = meet.topic;
                        table.Password = password;
                        table.StartTime = Common.TryStringToDateTime(StartTime);
                        db.SaveChanges();
                    }
                }
                return true;
            }
        }

        public int GetMeetingVideoId(int meetingId)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var video = db.tbl_MeetingVideo.FirstOrDefault(x => x.MeetID == meetingId);
                return video?.MeetVideoID ?? 0;
            }
        }



    }
}