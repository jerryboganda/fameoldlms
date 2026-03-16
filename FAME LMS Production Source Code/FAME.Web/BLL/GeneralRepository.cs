using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using LinqKit;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.UI.WebControls;

namespace First_Aid_Made_Easy.BLL
{
    public class GeneralRepository : IGeneralRepository
    {
        public bool CreateAgentCategory(tbl_Master model)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    tbl_Master table = new tbl_Master();
                    if (model.Master_ID > 0)
                    {
                        table = db.tbl_Master.Find(model.Master_ID);
                    }
                    table.Master_Value = model.Master_Value;
                    table.Master_Group = (int)MasterGroup.AgentCategory;
                    table.Master_Name = model.Master_Name;
                    if (model.Master_ID == 0)
                    {
                        db.tbl_Master.Add(table);
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public List<tbl_Master> GetList(int group)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_Master.AsNoTracking().Where(x => x.Master_Group == group).ToList();
            }
        }
        public bool DeleteAgentCategory(int ID)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    bool Exist = db.tbl_User.Any(x => x.CategoryID == ID);
                    if (Exist) return false;
                    var table = db.tbl_Master.Find(ID);
                    db.tbl_Master.Remove(table);
                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        #region Book Code

        public bool Create(tbl_BookCode model)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    tbl_BookCode table = new tbl_BookCode();
                    if (model.ID > 0)
                    {
                        table = db.tbl_BookCode.Find(model.BookCode);
                    }
                    table.BookCode = model.BookCode;
                    table.Notes = model.Notes;
                    if (model.ID == 0)
                    {
                        table.CreatedDT = Common.GetCurrentDate();
                        table.CreatedBy = model.CreatedBy;
                        table.IsUsed = false;
                        db.tbl_BookCode.Add(table);
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public bool UseBookCode(string BookCode, string UsedBy)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    tbl_BookCode table = db.tbl_BookCode.FirstOrDefault(x => x.BookCode == BookCode && x.IsUsed != true);
                    if (table == null) return false;
                    table.IsUsed = true;
                    table.UsedBy = UsedBy == "" ? null : UsedBy;
                    table.UsedAt = Common.GetCurrentDate();

                    var packID = Convert.ToInt32(WebConfigurationManager.AppSettings["packIDbook"]);
                    var duration = Convert.ToInt32(WebConfigurationManager.AppSettings["durationbook"]);
                    new EnrollmentRepository().Enroll(new AddSubscriptioVM
                    {
                        Duration = duration,
                        PackageID = packID,
                        StudentFid = UsedBy,
                        EnrollmentFor = "Package",
                        Expire = false,
                        Payment = "OneTime",
                        EndDate = string.Format("{0:dd/MM/yyyy}", Common.GetEndDate(duration)),
                    }, false, false, 0);

                    var req = db.tbl_Request.FirstOrDefault(x => x.StudentID == UsedBy);
                    if (req.IsAccepted != true)
                    {
                        req.IsAccepted = true;
                        req.PackageID = packID;
                        req.Duration = duration;
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public List<tbl_BookCode> GetList()
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    return db.tbl_BookCode.AsNoTracking().Include(x => x.AspNetUsers).Include(x => x.AspNetUsers.tbl_User).OrderByDescending(x => x.UsedAt).ToList();
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Reminder

        public List<ReminderVM> GetReminderList(string id, DateTime? DateFrom = null)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var rem = db.tbl_Reminder.AsNoTracking().Where(x => x.UserID == id && (x.DateFrom >= DateFrom || DateFrom == null)).ToList().ConvertAll(x => new ReminderVM
                {
                    groupId = x.RemID,
                    start = (x.IsFullDay == true) ? String.Format("{0:yyyy-MM-dd}", x.DateFrom) : String.Format("{0:yyyy-MM-dd HH:mm}", x.DateFrom),
                    title = x.Reminder,
                    end = string.Format("{0:yyyy-MM-dd}", x.DateTo),
                    allDay = x.IsFullDay ?? false
                });
                return rem;
            }
        }
        #endregion

        #region Msater Files | advertisment pic |

        public MasterVM GetMasterByGroup(MasterGroup mg)
        {

            using (FAMEEntities db = new FAMEEntities())
            {
                int grp = (int)mg;
                var data = db.tbl_Master.AsNoTracking().FirstOrDefault(x => x.Master_Group == grp);
                if (data == null) data = new tbl_Master();
                return new MasterVM
                {
                    Master_Value = "~/Images/Theme/" + data.Master_Value,
                    Master_Group = grp,
                    Master_Name = data.Master_Name,
                };
            }
        }

        public bool CreateMaster(MasterVM model)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var table = db.tbl_Master.FirstOrDefault(x => x.Master_Group == model.Master_Group);
                    if (table == null) table = new tbl_Master();
                    if (model.File != null)
                        table.Master_Value = Common.SavePicSameName(model.File, "Theme/");
                    table.Master_Group = model.Master_Group;
                    table.Master_Name = model.Master_Name;
                    if (table.Master_ID == 0)
                    {
                        db.tbl_Master.Add(table);
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region  Device Managment
        public async Task<Tuple<bool, string, string>> SaveDevice(string ID, string DevieID, bool IsAdmin)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var rq = HttpContext.Current.Request;
                var rs = HttpContext.Current.Response;

                var user = await db.tbl_User.Where(y => y.User_AspUser == ID).FirstOrDefaultAsync();
                //int MobCount = user.Allowed_Mob_Dev ?? 1, PCCount = user.Allowed_PC_Dev ?? 1;
                //var Reg_Dev_IDs = db.tbl_UserDevices.Where(x => x.UserID == ID && x.IsActive == true).Select(x => x.DeviceID);
                //var Reg_Devices = db.tbl_UserDevices.Where(x => x.UserID == ID && x.IsActive == true).Select(x => new Devices { DeviceID = x.DeviceID, IsMobile = x.IsMobile ?? false });

                //HttpCookie myCookie = rq.Cookies["Device"];
                //bool IsCookie = myCookie != null && Reg_Dev_IDs.Contains(myCookie.Values["UID"]);
                //bool IsLocalStorage = Reg_Dev_IDs.Contains(DevieID);

                //bool IsDeviceOld = IsCookie && IsLocalStorage;

                #region Save Login Record

                //bool canReg_NewMob = Reg_Devices.Where(x => x.IsMobile).Count() < MobCount;
                //bool canReg_NewPC = Reg_Devices.Where(x => !x.IsMobile).Count() < PCCount;
                //bool canRegisterNow = (canReg_NewMob && rq.Browser.IsMobileDevice) || (canReg_NewPC && !rq.Browser.IsMobileDevice);
                //bool IsValid = IsDeviceOld || canRegisterNow;
                bool IsValid = true;

                if (string.IsNullOrEmpty(DevieID)) DevieID = Guid.NewGuid().ToString();
                if (!IsAdmin)
                {
                    // ========== Create New Device ==============
                    db.tbl_UserDevices.Add(new tbl_UserDevices()
                    {
                        UserAgent = rq.UserAgent,
                        CreatedDT = Common.GetCurrentDate(),
                        UserID = ID,
                        IsMobile = rq.Browser.IsMobileDevice,
                        IpAddress = "",
                        DeviceID = DevieID,
                        DeviceName = /*computer_name[0].ToString() + " | " +*/ rq.Browser.Platform,
                        IsActive = true,
                    });
                }

                db.tbl_UserLogins.Add(new tbl_UserLogins()
                {
                    DateTime = Common.GetCurrentDate(),
                    UserID = ID,
                    IpAddress = rq.UserHostAddress,
                    DeviceID = DevieID,
                    //IsCookie = IsCookie,
                    //IsLocalStorage = IsLocalStorage,
                    //Status = IsValid ? "Success" : "Blocked"
                });

                db.SaveChanges();
                #endregion

                string Message = "";
                //if (IsValid)
                //{
                //    if (!IsAdmin)
                //    {
                //        myCookie = new HttpCookie("Device");
                //        myCookie.Values.Add("UID", DevieID);
                //        myCookie.Expires = DateTime.Now.AddYears(12);
                //        rs.Cookies.Add(myCookie);
                //    }
                //}
                //else
                //{
                //    Message = "You Cannot login on this device" +
                //            "</br> Because you are already logged in ";
                //    if (canReg_NewMob)
                //        Message += "a Desktop (PC or Laptop).</br> Please use a Mobile (Android or IOS)";
                //    else if (canReg_NewPC)
                //        Message += "a Mobile (Android or IOS).</br> Please use a Desktop (PC or Laptop)";
                //    else
                //        Message += "Mobile and Desktop Devices.</br>You can only use " + (MobCount + PCCount) + " Devices";

                //    Message += "</br></br><span class=\"text-danger\"> Account Sharing is against our rules. </br>Strict legal action will be taken against account sharing.</span>";
                //}
                return new Tuple<bool, string, string>(IsValid, DevieID, Message);
            }
        }

        public bool RemoveDevice(string ID, string UserID, int ReqID, bool IsAccepted)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    if (IsAccepted)
                    {
                        var dev = db.tbl_UserDevices.FirstOrDefault(x => x.DeviceID == ID && UserID == x.UserID && x.IsActive == true);
                        dev.IsActive = false;
                    }
                    if (ReqID > 0)
                    {
                        var req = db.tbl_UserDevRemReq.Find(ReqID);
                        req.IsAccepted = IsAccepted;
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public List<sp_lstDeleteDeviceReq_Result> GetDeleteReqList(string UserID = "", bool? IsAccepted = null)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.sp_lstDeleteDeviceReq(UserID, IsAccepted).ToList();
            }
        }
        public List<tbl_UserDevices> GetActiveUserDevices(string userID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_UserDevices.Where(x => x.UserID == userID && x.IsActive == true).ToList();
            }
        }

        public bool ResetUserDevices(string userID)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    db.tbl_UserDevices.RemoveRange(db.tbl_UserDevices.Where(x => userID == x.UserID));
                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool AddDeviceDeleteRequest(string deviceID, string userID, string notes)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    if (!db.tbl_UserDevRemReq.Any(x => x.DeviceID == deviceID && userID == x.UserID && x.IsAccepted == null))
                    {
                        db.tbl_UserDevRemReq.Add(new tbl_UserDevRemReq
                        {
                            DeviceID = deviceID,
                            Notes = notes,
                            UserID = userID,
                            DateTime = Common.GetCurrentDate(),
                        });
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool? IsDeviceValid(string deviceID, string userID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.sp_IsThisDeviceValidNow(deviceID, userID).FirstOrDefault();
            }
        }

        public static string getOperatinSystemDetails(string browserDetails)
        {
            try
            {
                switch (browserDetails.Substring(browserDetails.LastIndexOf("Windows NT") + 11, 3).Trim())
                {
                    case "6.2":
                        return "Windows 8";
                    case "6.1":
                        return "Windows 7";
                    case "6.0":
                        return "Windows Vista";
                    case "5.2":
                        return "Windows XP 64-Bit Edition";
                    case "5.1":
                        return "Windows XP";
                    case "5.0":
                        return "Windows 2000";
                    default:
                        return browserDetails.Substring(browserDetails.LastIndexOf("Windows NT"), 14);
                }
            }
            catch
            {
                if (browserDetails.Length > 149)
                    return browserDetails.Substring(0, 149);
                else
                    return browserDetails;
            }
        }
        #endregion

        public bool SaveNotes(EditStudentDetail m)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var user = db.tbl_User.Find(m.ID);
                    var aspUser = db.AspNetUsers.FirstOrDefault(x => x.Id == user.User_AspUser);

                    #region Change University
                    if (user.Type != m.UniversityID)
                    {
                        if (m.UniversityID > 1)
                        {
                            var En = db.tbl_EnrollmentMaster
                                 .Include("tbl_EnrollmentDetail")
                                 .Where(x =>
                                     x.AspNetUsers1.tbl_User.Any(u => u.Type == m.UniversityID) &&
                                     x.IsExpired != true
                                 )
                                 .OrderByDescending(X => X.Enrollment_Date)
                                 .FirstOrDefault();
                            if (En != null)
                            {
                                var max = db.tbl_EnrollmentMaster.Max(x => x.Enrollment_No) ?? 0;
                                var newEn = new tbl_EnrollmentMaster()
                                {
                                    StudentFid = user.User_AspUser,
                                    Enrollment_Id = En.Enrollment_Id,
                                    IsExpired = false,
                                    TeacherFid = En.TeacherFid,
                                    Enrollment_Date = En.Enrollment_Date,
                                    Enrollment_EndDate = En.Enrollment_EndDate,
                                    ApprovedBy = En.ApprovedBy,
                                    Enrollment_Price = En.Enrollment_Price,
                                    Enrollment_Status = En.Enrollment_Status,
                                    ByManual = En.ByManual,
                                    ByRequest = En.ByRequest,
                                    PackageDurationFid = En.PackageDurationFid,
                                    ByExtension = En.ByExtension,
                                    CouponID = En.CouponID,
                                    Enrollment_No = max + 1,
                                    tbl_EnrollmentDetail = En.tbl_EnrollmentDetail?.ToList().ConvertAll(x => new tbl_EnrollmentDetail
                                    {
                                        EnrollmentID = En.Enrollment_Id,
                                        IsExpired = false,
                                        CourseID = x.CourseID,
                                        Section_Fid = x.Section_Fid,
                                    }),
                                };
                                db.tbl_EnrollmentMaster.Add(newEn);
                            }
                            //db.tbl_EnrollmentMaster.Where(x => x.StudentFid == user.User_AspUser).ToList().ForEach(x => x.IsExpired = true);
                        }
                    }
                    #endregion

                    user.Type = m.UniversityID;
                    user.Notes = m.Notes;
                    user.Allowed_Mob_Dev = m.Allowed_Mob_Dev;
                    user.Allowed_PC_Dev = m.Allowed_PC_Dev;

                    user.User_Mobile = m.Student_Mobile;
                    user.CNIC = m.CNIC;
                    user.User_FatherName = m.FatherName;
                    user.FatherEmail = m.FatherEmail;
                    user.City = m.City;
                    user.Institute = m.Institute;

                    if (m.CNICFront != null) user.CNICFront = m.CNICFront;
                    if (m.CNICBack != null) user.CNICBack = m.CNICBack;
                    if (m.User_Pic != null) user.User_Pic = m.User_Pic;

                    aspUser.Email = m.Email.ToLower();
                    aspUser.UserName = m.Email.ToLower();

                    db.SaveChanges();

                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public bool UpdateAspNetUser(AspNetUsers u)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var aspUser = db.AspNetUsers.FirstOrDefault(x => x.Id == u.Id);
                aspUser.RegisteredFrom = u.RegisteredFrom;
                aspUser.PhoneNumber = u.PhoneNumber;
                aspUser.tbl_User.FirstOrDefault().CreateDT = Common.GetCurrentDate();
                db.SaveChanges();
                return true;
            }
        }
        public bool UpdateAspNetUser(string id, string email)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var user = db.AspNetUsers.Find(id);
                if (user != null)
                {
                    user.Email = email;
                    user.UserName = email;
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }
        public bool SelectMockTests(int id, string UID)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var user = db.tbl_User.FirstOrDefault(x => x.User_AspUser == UID);
                    user.MockTestType = id;
                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public bool SaveUserDevice(tbl_UserDevices model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                if (db.tbl_UserDevices.Any(x => x.DeviceID == model.DeviceID && x.UserID == model.UserID))
                {
                    return true;
                }

                db.tbl_UserDevices.Add(new tbl_UserDevices
                {
                    CreatedDT = Common.GetCurrentDate(),
                    IsActive = true,

                    UserID = model.UserID,
                    DeviceID = model.DeviceID,
                    DeviceName = model.DeviceName,
                    UserAgent = model.UserAgent,
                    IpAddress = model.IpAddress,
                    IsMobile = model.IsMobile,
                    IsApp = model.IsApp,
                });

                db.SaveChanges();
                return true;
            }
        }
        public bool SubscribeEmail(string email)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var Exist = db.tbl_Master.Any(x => x.Master_Value == email.ToLower());
                if (!Exist)
                {
                    db.tbl_Master.Add(new tbl_Master { Master_Value = email.ToLower(), Master_Group = 5, Master_Name = "Email" });
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }
        public List<sp_SearchContent_Result> SearchContent(string query)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.sp_SearchContent(query).ToList();
            }
        }
        public tbl_Reminder SaveReminder(tbl_Reminder rem)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                db.tbl_Reminder.Add(rem);
                db.SaveChanges();
                return rem;
            }
        }
        public bool DeleteReminder(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var reminder = db.tbl_Reminder.Find(id);
                if (reminder != null)
                {
                    db.tbl_Reminder.Remove(reminder);
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }
    }
}
public class Devices
{
    public string DeviceID { get; set; }
    public bool IsMobile { get; set; }
}