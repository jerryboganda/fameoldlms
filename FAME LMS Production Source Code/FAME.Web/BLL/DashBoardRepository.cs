using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace First_Aid_Made_Easy.BLL
{
    public class DashBoardRepository : IDashBoardRepository
    {
        public DashBoardVM Get()
        {
            using (DAL.FAMEEntities db = new DAL.FAMEEntities())
            {
                return new DashBoardVM()
                {
                    Counts = GetCounts(),
                    CourseEnrollments = GetCourseEnrl(),
                    Students = GetStudentsByUni(),
                    SectionEnrollments = GetSectionEnrl(),
                    PackageEnrollments = GetPackageEnrl(),
                };
            }
        }

        public List<sp_DashBoardCounts_Result> GetCounts()
        {
            using (DAL.FAMEEntities db = new DAL.FAMEEntities())
            {
                return db.sp_DashBoardCounts().OrderBy(x => x.SortID).ToList();
            }
        }

        public List<CountVM> GetCourseEnrl()
        {
            using (DAL.FAMEEntities db = new DAL.FAMEEntities())
            {
                var list = db.tbl_EnrollmentDetail.Where(x => x.CourseID > 0).ToList().GroupBy(x => x.CourseID);
                var EnList = new List<CountVM>();
                foreach (var i in list)
                {
                    var model = new CountVM()
                    {
                        Name = i.FirstOrDefault().tbl_Courses.Course_Name,
                        Count = i.Count()
                    };
                    EnList.Add(model);
                }
                return EnList;
            }
        }
        public List<CountVM> GetSectionEnrl()
        {
            using (DAL.FAMEEntities db = new DAL.FAMEEntities())
            {
                var list = db.tbl_EnrollmentDetail.Where(x => x.Section_Fid > 0).ToList().GroupBy(x => x.Section_Fid);
                var EnList = new List<CountVM>();
                foreach (var i in list)
                {
                    var model = new CountVM()
                    {
                        Name = i.FirstOrDefault().tbl_Section.Section_Name,
                        Count = i.Count()
                    };
                    EnList.Add(model);
                }
                return EnList;
            }
        }
        public List<CountVM> GetStudentsByUni()
        {
            using (DAL.FAMEEntities db = new DAL.FAMEEntities())
            {
                var list = db.tbl_User.ToList().GroupBy(x => x.Type);
                var EnList = new List<CountVM>();
                foreach (var i in list)
                {
                    var model = new CountVM()
                    {
                        Name = Common.Universities.FirstOrDefault(x => x.Value == i.FirstOrDefault().Type)?.Text,
                        Count = i.Count()
                    };
                    EnList.Add(model);
                }
                return EnList;
            }
        }

        public List<sp_lstStudents_Result> GetStudentList(string text, int Type, string DateFrom, string DateTo)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                DateTime? DF = Common.TryStringToDate(DateFrom);
                DateTime? DT = Common.TryStringToDate(DateTo);
                return db.sp_lstStudents(text, Type, DF, DT).ToList();
            }
        }

        public List<sp_GetUsersListByRole_Result> GetUserList(int role, int CategoryID = 0, string Email = "")
        {
            using (DAL.FAMEEntities db = new DAL.FAMEEntities())
            {
                return db.sp_GetUsersListByRole(role, CategoryID, Email).ToList();
            }
        }

        public bool AgentStatus(string ID, bool Status)
        {
            try
            {
                using (DAL.FAMEEntities db = new DAL.FAMEEntities())
                {
                    var u = db.AspNetUsers.Find(ID);
                    u.IsActive = Status;
                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<sp_lstVideos_Result> StudentProgress(string ID)
        {
            try
            {
                using (DAL.FAMEEntities db = new DAL.FAMEEntities())
                {
                    var Slist = db.sp_MySections(ID).ToList();
                    var list = db.sp_MyCourses(ID).ToList();
                    return db.sp_lstVideos(ID, string.Join(",", list.Select(x => x.Course_Id)), "", string.Join(",", Slist.Select(x => x.Section_ID)), "", "").ToList();
                }
            }
            catch
            {
                return null;
            }
        }

        public async Task<StudentDetail> GetStudentDetailAsync(int iD, string ID = null)
        {
            using (DAL.FAMEEntities db = new DAL.FAMEEntities())
            {
                var rt = RequestType.Register.ToString();

                var user = string.IsNullOrEmpty(ID) ?
                    db.tbl_User.Find(iD) :
                    db.tbl_User.FirstOrDefault(x => x.User_AspUser == ID);


                DateTime date = Common.GetCurrentDate();
                var list = db.tbl_EnrollmentDetail.Where(x => x.Section_Fid != null && x.tbl_EnrollmentMaster.StudentFid == user.User_AspUser);
                var Clist = db.tbl_EnrollmentDetail.Where(x => x.tbl_EnrollmentMaster.PackageDurationFid == null && x.CourseID != null && x.tbl_EnrollmentMaster.StudentFid == user.User_AspUser);
                SubscriptionsVM c = new SubscriptionsVM
                {
                    SecList = await list.Select(x => new SubscriptionsVM
                    {
                        SectionName = x.tbl_Section.Section_Name,
                        DEnrollmentDate = x.tbl_EnrollmentMaster.Enrollment_Date,
                        DExpiryDate = x.tbl_EnrollmentMaster.Enrollment_EndDate,
                        EnrollmentMethod = x.tbl_EnrollmentMaster.CouponID == null ? "Payment" : "Coupon",
                        Status = x.tbl_EnrollmentMaster.Enrollment_EndDate < date ? "Expired" : (x.tbl_EnrollmentMaster.IsExpired == true || x.IsExpired == true) ? "Override" : "Continue",
                        GivenBy = x.tbl_EnrollmentMaster.AspNetUsers.tbl_User.FirstOrDefault().User_Name
                    }).ToListAsync(),
                    CorList = await Clist.Select(x => new SubscriptionsVM
                    {
                        CourseName = x.tbl_Courses.Course_Name,
                        DEnrollmentDate = x.tbl_EnrollmentMaster.Enrollment_Date,
                        DExpiryDate = x.tbl_EnrollmentMaster.Enrollment_EndDate,
                        EnrollmentMethod = x.tbl_EnrollmentMaster.CouponID == null ? "Payment" : "Coupon",
                        Status = x.tbl_EnrollmentMaster.Enrollment_EndDate < date ? "Expired" : (x.tbl_EnrollmentMaster.IsExpired == true || x.IsExpired == true) ? "Override" : "Continue",
                        GivenBy = x.tbl_EnrollmentMaster.AspNetUsers.tbl_User.FirstOrDefault().User_Name
                    }).ToListAsync(),
                    PackList = await GetPackSubscriptions(user.User_AspUser),
                    InstList = db.sp_GetPendingInstallments(null, user.User_AspUser).ToList(),
                };

                var model = new StudentDetail()
                {
                    User = new StudentVM()
                    {
                        User_Name = user.User_Name,
                        User_FatherName = user.User_FatherName,
                        FatherEmail = user.FatherEmail,
                        SponserProfession = user.SponserProfession,
                        FatherProfession = user.FatherProfession,
                        City = user.City,
                        ExamType = user.ExamType,
                        Institute = user.Institute,
                        JobLocation = user.JobLocation,
                        Email = user.AspNetUsers.Email,
                        Student_Mobile = user.User_Mobile,
                        YearOfMBBS = user.YearOfMBBS,
                        Occupation = user.tbl_Master?.Master_Value,
                        User_Pic = user.User_Pic ?? "userLogo.jpg",
                        User_Id = user.User_Id,
                        Notes = user.Notes,
                        CNIC = user.CNIC,
                        CNICBack = user.CNICBack,
                        CNICFront = user.CNICFront,
                        User_AspUser = user.User_AspUser,
                        StuCardBack = user.StuCardBack,
                        StuCardFront = user.StuCardFront,
                        RequestID = user.AspNetUsers.tbl_Request.FirstOrDefault(x => x.RequestFor == rt)?.RequestID ?? 0,
                        IsAccepted = user.AspNetUsers.tbl_Request.FirstOrDefault(x => x.RequestFor == rt)?.IsAccepted ?? false,
                        Allowed_Mob_Dev = user.Allowed_Mob_Dev ?? 1,
                        Allowed_PC_Dev = user.Allowed_PC_Dev ?? 1,
                        Type = user.Type ?? 1
                    },
                    Subscrption = c,
                    DeviceList = db.tbl_UserDevices.Where(x => x.UserID == user.User_AspUser && x.IsActive == true).ToList()
                };
                return model;
            }
        }

        public async Task<List<SubscriptionsVM>> GetPackSubscriptions(string id)
        {
            using (var db = new FAMEEntities())
            {
                DateTime date = Common.GetCurrentDate();
                var Plist = db.tbl_EnrollmentMaster.Where(x => x.PackageDurationFid != null && x.StudentFid == id);

                var a = await Plist.Select(x => new SubscriptionsVM
                {
                    ID = x.tbl_PackageDuration.PackageID,
                    EnrollmentID = x.Enrollment_Id,
                    Amount = x.Enrollment_Price,
                    CourseName = x.tbl_PackageDuration.tbl_Package.PackageName,
                    DEnrollmentDate = x.Enrollment_Date,
                    DExpiryDate = x.Enrollment_EndDate,
                    EnrollmentMethod = x.CouponID == null ? "Payment" : "Coupon",
                    Status = x.Enrollment_EndDate < date ? "Expired" : (x.IsExpired == true) ? "Override" : "Continue",
                    GivenBy = x.AspNetUsers.tbl_User.FirstOrDefault().User_Name
                }).OrderByDescending(x => x.DEnrollmentDate).ToListAsync();
                return a;
            }
        }

        public List<PackageEnrollments> GetPackageEnrl()
        {
            using (DAL.FAMEEntities db = new DAL.FAMEEntities())
            {
                var list = db.tbl_EnrollmentMaster.Where(x => x.PackageDurationFid > 0).GroupBy(x => x.tbl_PackageDuration.tbl_Package);
                var EnList = new List<PackageEnrollments>();
                foreach (var i in list)
                {
                    if (i.FirstOrDefault().tbl_PackageDuration != null)
                    {
                        var model = new PackageEnrollments()
                        {
                            PackageName = i.FirstOrDefault().tbl_PackageDuration.tbl_Package.PackageName,
                            TeacherName = i.FirstOrDefault().tbl_PackageDuration.tbl_Package.PackageName,
                            Enrollments = i.Count()
                        };
                        EnList.Add(model);
                    }
                }
                return EnList;
            }
        }
        public List<sp_ListEnrollments_Result> GetEnrollmentList(
            string datef, string datet, string Sid, int CourseID, int PackageID, bool? IsExpire, string AddedBy, string ApproveBy, string City, string Institute)
        {
            try
            {
                using (DAL.FAMEEntities db = new DAL.FAMEEntities())
                {
                    DateTime? datefrom = Common.TryStringToDate(datef);
                    DateTime? dateto = Common.TryStringToDate(datet);

                    // Map AddedBy to ByRequest, ByManual, ByALL
                    bool byRequest = AddedBy == "Request";
                    bool byManual = AddedBy == "Manual";
                    bool byAll = AddedBy == "All";

                    string sql = "EXEC sp_ListEnrollments @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11";

                    return db.Database.SqlQuery<sp_ListEnrollments_Result>(
                        sql,
                        datefrom,
                        dateto,
                        Sid ?? "",
                        ApproveBy ?? "", // ApproveBy (not used in your UI, so pass null)
                        CourseID,
                        PackageID,
                        byRequest,
                        byManual,
                        byAll,
                        IsExpire,
                        City,
                        Institute
                    ).ToList();
                }
            }
            catch
            {
                return new List<sp_ListEnrollments_Result>();
            }
        }

        public List<AspNetUsers> GetPartiallyRegistered()
        {
            using (DAL.FAMEEntities db = new DAL.FAMEEntities())
            {
                return db.AspNetUsers.Where(x => !x.tbl_User.Any()).ToList();
            }
        }

    }
}