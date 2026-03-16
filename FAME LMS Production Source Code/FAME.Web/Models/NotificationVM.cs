using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Models
{
    public class StudentNotifVM
    {
        public int ID { get; set; }
        [Required]
        public string MessageTitle { get; set; }
        [Required]
        public string Description { get; set; }
        [AllowHtml]
        public string MaessageBody { get; set; }
        public string CreatedBy { get; set; }
        public string PicturePath { get; set; }
        public bool IsActive { get; set; }
        public bool IsPush { get; set; }
        public bool IsPopup { get; set; }
        public string Banner { get; set; }


        public string UniversityIds { get; set; }
        public string PackageIds { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> SendAt { get; set; }
        public Nullable<System.DateTime> CreatedDT { get; set; }

        public HttpPostedFileBase ImageFile { get; set; }
        public HttpPostedFileBase BannerFile { get; set; }
        public List<StudentNotifVM> List { get; set; }
    }
    public class MeetingVM
    {
        public int ID { get; set; }
        public int Role { get; set; }
        public int VideoID { get; set; }
        public Nullable<long> MeetingNo { get; set; }
        public Nullable<int> Duration { get; set; }
        public string CourseIDs { get; set; }
        public string PackageIDs { get; set; }
        public string Course { get; set; }
        public string Topic { get; set; }
        public string Status { get; set; }
        public string JoinUrl { get; set; }
        public string Password { get; set; }
        public Nullable<System.DateTime> StartTime { get; set; }
    }
    public class Data
    {
        public int page_size { get; set; }
        public int total_records { get; set; }
        public string next_page_token { get; set; }
        public List<Meeting> meetings { get; set; }
    }
    public class Meeting
    {
        public int duration { get; set; }
        public int type { get; set; }
        public long id { get; set; }
        public string uuid { get; set; }
        public int code { get; set; }
        public string host_id { get; set; }
        public string topic { get; set; }
        public string start_time { get; set; }
        public string timezone { get; set; }
        public string agenda { get; set; }
        public string created_at { get; set; }
        public string join_url { get; set; }

    }
}