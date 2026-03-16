using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
namespace First_Aid_Made_Easy.Models
{
    public class VideoVM
    {
        public int Video_Id { get; set; }
        public string Video_Name { get; set; }
        public string Video_ShortDescription { get; set; }
        public string Video_Tags { get; set; }
        public decimal? Video_Length { get; set; }
        public string Video_Path { get; set; }
        public string Difficulty { get; set; }
        public bool IsWatched { get; set; }
        public int ScreenTime { get; set; }
        public int? Section_Fid { get; set; }
        public int? Course_Fid { get; set; }

        // Only Used In Case of file 
        public HttpPostedFileBase File { get; set; }

        [Required]
        public Nullable<int> Type { get; set; }
        public string TypeS { get { switch (Type) { case (int)ContentType.File: return "File"; case (int)ContentType.Video: return "Video"; case (int)ContentType.Audio: return "Audio"; default: return ""; }; } set { } }
        public int SortID { get; set; }
        public int Views { get; set; }
        public int? MeetID { get; set; }
        public int? SectionSub_Fid { get; set; }
    }
}