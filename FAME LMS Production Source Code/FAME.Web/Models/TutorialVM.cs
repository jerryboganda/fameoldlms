using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

namespace First_Aid_Made_Easy.Models
{
    public class TutorialVM
    {
        public int TutorialID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string VideoLink { get; set; }
        public string Category { get; set; }
        public Nullable<int> CategoryID { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDT { get; set; }
        public List<TutorialVM> List { get; set; }
        public bool IsPopular { get; set; }
    }
    public class MasterVM
    {
        public int Master_ID { get; set; }
        public string Master_Name { get; set; }
        public string Master_Value { get; set; }
        public int? Master_Group { get; set; }
        public MasterGroup Group { get; set; }

        public HttpPostedFileBase File { get; set; }
    }

}