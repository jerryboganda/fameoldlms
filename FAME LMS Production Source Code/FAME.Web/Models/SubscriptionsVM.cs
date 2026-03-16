using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;

namespace First_Aid_Made_Easy.Models
{
    public class SubscriptionsVM
    {
        public string SectionName { get; set; }
        public string CourseName { get; set; }
        public string GivenBy { get; set; }
        public string Status { get; set; }

        private DateTime? dEnrollmentDate;
        public DateTime? DEnrollmentDate
        {
            get { return dEnrollmentDate; }
            set
            {
                dEnrollmentDate = value;
                EnrollmentDate = Common.DateToString(value);
            }
        }

        private DateTime? dExpiryDate;
        public DateTime? DExpiryDate
        {
            get { return dExpiryDate; }
            set
            {
                dExpiryDate = value;
                ExpiryDate = Common.DateToString(value);
            }
        }

        public string EnrollmentDate { get; set; }
        public string ExpiryDate { get; set; }
        public int? ID { get; set; }
        public string EnrollmentMethod { get; set; }
        public int EnrollmentID { get; set; }
        public decimal? Amount { get; set; }
        public List<SubscriptionsVM> CorList { get; set; }
        public List<SubscriptionsVM> SecList { get; set; }
        public List<SubscriptionsVM> PackList { get; set; }
        public SubscriptionsVM CurrentEnroll { get { return PackList.OrderBy(x => x.RemainingDays).LastOrDefault(x => x.Status == EnrollStatus.Continue.ToString()); } set { } }
        public List<DAL.sp_GetPendingInstallments_Result> InstList { get; set; }
        public double RemainingDays { get { return Math.Round(DExpiryDate.Value.Subtract(Common.GetCurrentDate()).TotalDays); } set { } }
        public bool IsExpired { get; set; }
    }
}