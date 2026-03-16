using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.Models
{
    public class PackageVM
    {
        public int PackageID { get; set; }
        public int Duration { get; set; }
        public string PackageName { get; set; }
        public string PackageDescription { get; set; }
        public string CreatedBy { get; set; }
        public string EndDate { get; set; }
        public string[] CourseIds { get; set; }
        public bool HasType { get; set; }
        public DateTime CreatedDT { get; set; }
        public Nullable<decimal> PackagePrice { get; set; }
        public List<PackageVM> List { get; set; }
        public List<PackageDetailVM> Detail { get; set; }

        public object Group
        {
            get
            {
                return Detail?.GroupBy(x => x.Type).ToList().ConvertAll(t => new
                {
                    Type = t.Key,
                    List = t.GroupBy(p => p.Paper).ToList().ConvertAll(p => new
                    {
                        Paper = p.Key,
                        CourseIds = p.Select(c => c.CourseID).ToArray()
                    })
                });

            }
        }

        public List<PackageDurationVM> Durations { get; set; }
        public int? Courses { get; set; }
        public int? SortID { get; set; }
    }
    public class PackageDetailVM
    {
        public int DetailID { get; set; }
        public Nullable<int> PackageID { get; set; }
        public string CourseName { get; set; }
        public string CoursePic { get; set; }
        public string Type { get; set; }
        public string Paper { get; set; }
        public Nullable<decimal> DurationInMin { get; set; }
        public Nullable<int> CourseID { get; set; }
    }
    public class PackageDurationVM
    {
        public int ID { get; set; }
        public Nullable<int> PackageID { get; set; }
        public decimal? Price { get; set; }
        public string DurationS { get; set; }
        public Nullable<int> Duration { get; set; }
        public bool InstallmentsExist { get; set; }
        public List<InstallmentVM> Installments { get; set; }
        public string DurationChar { get; set; }
    }
    public class InstallmentVM
    {
        public int? InstallmentID { get; set; }
        public Nullable<decimal> InstallmentPrice { get; set; }
        public Nullable<int> Duration { get; set; }
        public string DurationS { get; set; }
        public Nullable<int> PackageDurationID { get; set; }
        public Nullable<int> PackageID { get; set; }
        public List<InstallmentVM> List { get; set; }
        public PackageDurationVM PackageDuration { get; set; }
    }
    public class StudentInstallmentVM
    {
        public int ID { get; set; }
        public Nullable<int> EnrollmentID { get; set; }
        public Nullable<bool> IsPaid { get; set; }
        public string InstallmentDate { get; set; }
        public Nullable<int> InstallmentNo { get; set; }
        public string StudentID { get; set; }
        public Nullable<decimal> InstallmentPrice { get; set; }
        public string Student { get; set; }
        public string PackageName { get; set; }
        public string PackageDur { get; set; }
        public List<StudentInstallmentVM> List { get; set; }

    }
}