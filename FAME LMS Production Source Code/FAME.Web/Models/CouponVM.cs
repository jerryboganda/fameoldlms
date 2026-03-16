using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.Models
{
    public class CouponVM
    {
        public int Id { get; set; }
        public string ExpiryDate { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string CouponSecret { get; set; }
        public Nullable<int> SectionID { get; set; }
        public bool IsActive { get; set; }
        public string Type { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<decimal> DiscountPer { get; set; }
        public Nullable<int> NoOfUses { get; set; }
        public Nullable<int> RUses { get; set; }
        public Nullable<int> CourseFid { get; set; }
        public string ForName { get; set; }
        public string CouponName { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<bool> ForCourse { get; set; }

        public List<CouponVM> List { get; set; }
    }

    public class ReminderVM
    {
        public int groupId { get; set; }
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public bool allDay { get; set; }

    }
}