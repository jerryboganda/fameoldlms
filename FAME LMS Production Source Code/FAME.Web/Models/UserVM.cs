using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.Models
{
    public class UserVM : RequestVM
    {
        public int User_Id { get; set; }
        public int? Type { get; set; }
        [Display(Name = "User Name")]
        public string User_Name { get; set; }
        [Display(Name = "Father Name")]
        public string User_FatherName { get; set; }
        public string FatherEmail { get; set; }
        public string FatherProfession { get; set; }
        public string SponserProfession { get; set; }
        public string User_Pic { get; set; }
        public string CNICFront { get; set; }
        public string CNICBack { get; set; }
        public string StuCardFront { get; set; }
        public string StuCardBack { get; set; }
        public string User_AspUser { get; set; }
        public Nullable<int> Allowed_Devices { get; set; }
        public Nullable<int> OccupationID { get; set; }
        public string JobLocation { get; set; }
        public Nullable<int> CategoryID { get; set; }
        public Nullable<int> InstituteID { get; set; }
        public string Institute { get; set; }
        public string ExamType { get; set; }
        public string City { get; set; }
        public string YearOfMBBS { get; set; }
        [Display(Name = "Mobile No")]
        public string User_Mobile { get; set; }
        public string CNIC { get; set; }
        public Nullable<int> CountryID { get; set; }
        public HttpPostedFileBase User_PicFile { get; set; }
        public HttpPostedFileBase CNICFrontFile { get; set; }
        public HttpPostedFileBase CNICBackFile { get; set; }
        public HttpPostedFileBase StuCardFrontFile { get; set; }
        public HttpPostedFileBase StuCardBackFile { get; set; }
        public bool? NotComplete { get; set; }
        public bool DisableRequest { get; set; }
        public bool? ShouldChangeInfo { get; set; }
        public bool? ShouldChangePassword { get; set; }
        public int? MockTestType { get; set; }
    }
    public class StudentVM
    {
        public int User_Id { get; set; }
        public string User_Name { get; set; }
        public string User_FatherName { get; set; }
        public string FatherEmail { get; set; }
        public string SponserProfession { get; set; }
        public string FatherProfession { get; set; }
        public string CNIC { get; set; }
        public string User_Pic { get; set; }
        public string CNICFront { get; set; }
        public string CNICBack { get; set; }
        public string StuCardFront { get; set; }
        public string StuCardBack { get; set; }
        public string User_AspUser { get; set; }
        public string Email { get; set; }
        public string Occupation { get; set; }
        public string JobLocation { get; set; }
        public string Institute { get; set; }
        public string City { get; set; }
        public string YearOfMBBS { get; set; }
        public string Student_Mobile { get; set; }
        public string Notes { get; set; }
        public List<DAL.sp_lstStudents_Result> List { get; set; }
        public int RequestID { get; set; }
        public string ExamType { get; set; }
        public int Type { get; set; }
        public bool IsAccepted { get; set; }
        public int Allowed_Mob_Dev { get; set; }
        public int Allowed_PC_Dev { get; set; }
    }
    public class EditStudentDetail
    {

        public int ID { get; set; }
        public string Notes { get; set; }
        public string Email { get; set; }
        public string CNIC { get; set; }
        public string Student_Mobile { get; set; }
        public string FatherEmail { get; set; }
        public string FatherName { get; set; }
        public string City { get; set; }
        public string Institute { get; set; }
        public int UniversityID { get; set; }
        public int Allowed_PC_Dev { get; set; }
        public int Allowed_Mob_Dev { get; set; }

        public string User_Pic { get; set; }
        public string CNICFront { get; set; }
        public string CNICBack { get; set; }
    }

    public class UserLockoutVM
    {
        public int ID { get; set; }
        public Nullable<System.DateTime> LockoutDate { get; set; }
        public string LockoutDT { get; set; }
        public string Remarks { get; set; }
        public string AspUserID { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
    }

    public class StudentDetail
    {
        public StudentVM User { get; set; }
        public List<DAL.tbl_UserDevices> DeviceList { get; set; }
        public SubscriptionsVM Subscrption { get; set; }
    }
}