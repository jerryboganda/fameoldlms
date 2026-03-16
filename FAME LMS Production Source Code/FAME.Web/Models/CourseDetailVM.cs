using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.Models
{
    public class CourseDetailVM
    {
        public int CourseDetail_ID { get; set; }
        public string CourseDetail_Body { get; set; }
        public Nullable<int> Course_Fid { get; set; }
    }
}