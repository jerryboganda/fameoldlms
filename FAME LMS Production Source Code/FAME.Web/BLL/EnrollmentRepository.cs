using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Razor.Generator;

namespace First_Aid_Made_Easy.BLL
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        #region Enrollment
        public bool Save(EnrollmentMasterVM model, bool Installments = false, bool Expire = false)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var max = db.tbl_EnrollmentMaster.Max(x => x.Enrollment_No) ?? 0;
                    tbl_EnrollmentMaster tb = new tbl_EnrollmentMaster()
                    {
                        StudentFid = model.StudentFid,
                        TeacherFid = model.TeacherFid,
                        Enrollment_Date = Common.GetCurrentDate(),
                        Enrollment_No = max + 1,
                        Enrollment_EndDate = model.Enrollment_EndDate,
                        Enrollment_Price = model.Enrollment_Price,
                        Enrollment_Id = model.Enrollment_Id,
                        Enrollment_Status = model.Enrollment_Status ?? EnrollStatus.Pending.ToString(),
                        CouponID = model.CouponFid,
                        PackageDurationFid = model.PackageDurationFid,
                        PackageId = model.PackageId,
                        IsExpired = false,
                        ByRequest = model.ByRequest,
                        ByManual = model.ByManual,
                        ApprovedBy = model.ApprovedBy,
                        ByExtension = model.ByExtension,
                        ExamAttemptID = model.ExamAttemptID,
                    };

                    tb.tbl_EnrollmentDetail = model.Details?.ConvertAll(x => new tbl_EnrollmentDetail
                    {
                        CourseID = x.CourseID,
                        Section_Fid = x.Section_Fid,
                        IsExpired = false,
                        EnrollmentID = tb.Enrollment_Id,
                    });
                    if (tb.CouponID > 0)
                    {
                        var coupon = db.tbl_Coupon.Find(tb.CouponID);
                        coupon.RemUses = coupon.RemUses - 1;
                        db.Entry(coupon).State = EntityState.Modified;
                    }
                    //===========Expire Previouse Enrollments=================

                    if (Expire)
                    {
                        // ======= Expire All Enrollments ============
                        db.tbl_EnrollmentMaster.Where(x => x.StudentFid == tb.StudentFid).ToList().ForEach(x => x.IsExpired = true);
                    }


                    db.tbl_EnrollmentMaster.Add(tb);
                    db.SaveChanges();
                    //=========== Add Installments For Enrollment =================
                    if (Installments)
                    {
                        var list = db.tbl_Installments.Where(x => x.PackageDurationID == tb.PackageDurationFid).ToList();
                        int? PreDuration = 0, Number = 0;
                        var listSave = new List<tbl_StudentInstallments>();
                        foreach (var x in list)
                        {
                            Number++;
                            int CurrDuration = (int)((x.Duration) / 30);
                            var ins = new tbl_StudentInstallments
                            {
                                EnrollmentID = tb.Enrollment_Id,
                                InstallmentDate = Common.GetCurrentDate().AddMonths((CurrDuration + PreDuration) ?? 0),
                                InstallmentNo = Number,
                                InstallmentPrice = x.InstallmentPrice,
                                PackageID = x.tbl_PackageDuration.PackageID,
                                StudentID = model.StudentFid,
                                PkgDuration = x.tbl_PackageDuration.Duration,
                                IsPaid = x.Duration == 0 //=====First Installment Is Paid By Default===
                            };
                            listSave.Add(ins);
                            PreDuration += CurrDuration;
                        };
                        db.tbl_StudentInstallments.AddRange(listSave);
                        db.SaveChanges();
                    }

                    return true;
                }
            }
            catch
            {
                throw;
                return false;
            }
        }

        public bool Enroll(AddSubscriptioVM en, bool ByManual, bool ByRequest, decimal? DiscountedPrice)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {

                    List<string> Students = new List<string>() { en.StudentFid };
                    if (en.UniversityID > 0) Students = db.tbl_User.Where(x => x.Type == en.UniversityID).Select(z => z.User_AspUser).ToList();

                    if (en.EnrollmentFor == "Course")
                    {
                        foreach (var StudentFid in Students)
                        {
                            decimal? price = 0;
                            if (en.CourseFid != null) price += db.tbl_Courses.Where(x => en.CourseFid.Contains(x.Course_Id)).Sum(x => x.Course_Price);
                            if (en.SectionFid != null) price += db.tbl_Section.Where(x => en.SectionFid.Contains(x.Section_ID)).Sum(x => x.Section_Price);
                            var Teacher_Fid = en.CourseFid != null ? db.tbl_Courses.Where(x => en.CourseFid.Contains(x.Course_Id)).FirstOrDefault().TeacherFid : "";
                            EnrollmentMasterVM enrlm = new EnrollmentMasterVM
                            {
                                Enrollment_Price = DiscountedPrice ?? price ?? 0,
                                TeacherFid = Teacher_Fid,
                                Enrollment_EndDate = Common.StringToDate(en.EndDate),
                                StudentFid = StudentFid,
                                ByManual = ByManual,
                                ByRequest = ByRequest,
                                ApprovedBy = en.UserID,
                                Details = new List<EnrollmentDetailVM>(),
                            };
                            if (en.CourseFid != null) enrlm.Details.AddRange(en.CourseFid.ToList().ConvertAll(x => new EnrollmentDetailVM { CourseID = x }));
                            if (en.SectionFid != null) enrlm.Details.AddRange(en.SectionFid.ToList().ConvertAll(x => new EnrollmentDetailVM { Section_Fid = x }));
                            var f = Save(enrlm, false, en.Expire);
                        }
                    }
                    else
                    {
                        foreach (var StudentFid in Students)
                        {
                            var pack = db.tbl_Package.Find(en.PackageID);
                            var duration = pack.tbl_PackageDuration.Where(x => x.Duration == en.Duration).FirstOrDefault();
                            var CourseIds = pack.tbl_PackageDetail.Select(x => x.CourseID).ToArray();
                            EnrollmentMasterVM enrlm = new EnrollmentMasterVM
                            {
                                Enrollment_Price = DiscountedPrice ?? duration?.Price ?? 0,
                                TeacherFid = pack.tbl_PackageDetail?.FirstOrDefault()?.tbl_Courses?.TeacherFid,
                                Enrollment_EndDate = Common.StringToDate(en.EndDate),
                                StudentFid = StudentFid,
                                PackageDurationFid = duration?.ID,
                                ByManual = ByManual,
                                ByRequest = ByRequest,
                                ApprovedBy = en.UserID,
                                PackageId = en.PackageID,
                                ExamAttemptID = en.ExamAttemptID,
                            };
                            bool f = true;
                            f = f && Save(enrlm, en.Payment == "Installments", en.Expire);
                        }
                    }
                    return true;
                }
                catch
                {
                    throw;
                    return false;
                }
            }
        }
        public bool UpdateStatus(int ID, int Price, string Status)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var En = db.tbl_EnrollmentMaster.Find(ID);
                    En.Enrollment_Price = Price;
                    En.Enrollment_Status = Status;
                    En.IsEmailSent = true;
                    db.SaveChanges();
                    SendMail(db.AspNetUsers.Find(En.StudentFid).Email);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public bool SendMail(string Email)
        {
            try
            {
                IdentityMessage message = new IdentityMessage()
                {
                    Subject = "Accepted",
                    Body = "<h1 style=\"text-align: center; \"><b><font style=\"background-color: rgb(255, 255, 255);\" color=\"#9c00ff\">First Aid Made Easy</font></b></h1><h2 style=\"text-align: center; \"><font color=\"#311873\">Thank You For Choosing Us!&nbsp; Please Visit Our Website To Continue Your Lectures.</font></h2><h2 style=\"text-align: center; \"></font></h2>",
                    Destination = Email
                };
                new EmailService().Send(message);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public List<sp_lstEnrollStatus_Result> GetInvoiceList(string datef, string datet, string StudentID, string status, bool? IsEmailSent = null)
        {

            using (FAMEEntities db = new FAMEEntities())
            {

                DateTime? datefrom = Common.TryStringToDate(datef);
                DateTime? dateto = Common.TryStringToDate(datet);
                return db.sp_lstEnrollStatus(datefrom, dateto, StudentID, 0, status, IsEmailSent).ToList();
            }
        }

        public sp_CurrentPackDetail_Result GetCurrentEnroll(string ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.sp_CurrentPackDetail(ID).FirstOrDefault() ?? new sp_CurrentPackDetail_Result();
            }
        }

        public bool CanCreateTest(string student_Fid)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var cDate = Common.GetCurrentDate();
                var HasEnr = db.tbl_EnrollmentMaster.Where(x => x.StudentFid == student_Fid && x.Enrollment_EndDate >= cDate && x.IsExpired != true).Any();
                var OneYearBefore = cDate.AddMonths(-6);
                var isBook = db.tbl_BookCode.Any(x => x.UsedBy == student_Fid && x.UsedAt > OneYearBefore);

                return HasEnr || isBook;
            }
        }

        /// <summary>
        /// Checks if a student has any active (non-expired) enrollment or a valid book code.
        /// Used by login flow and EnrollmentFilter to block expired students.
        /// </summary>
        public bool HasActiveSubscription(string studentId)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var cDate = Common.GetCurrentDate();
                var hasEnrollment = db.tbl_EnrollmentMaster
                    .Any(x => x.StudentFid == studentId 
                           && x.Enrollment_EndDate >= cDate 
                           && x.IsExpired != true);
                if (hasEnrollment) return true;

                // Also check book codes (valid for 6 months)
                var sixMonthsAgo = cDate.AddMonths(-6);
                var hasBook = db.tbl_BookCode
                    .Any(x => x.UsedBy == studentId && x.UsedAt > sixMonthsAgo);
                return hasBook;
            }
        }


        #endregion

        #region Select Lists AND Extras
        public List<vw_StudetsList> GetStudentsList()
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.vw_StudetsList.OrderBy(x => x.Text).ToList();
            }
        }
        public string GetStudentID(string email)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var id = db.vw_StudetsList.Where(x => x.Text.ToLower() == email.ToLower()).Select(x => x.Value).FirstOrDefault();
                return id;
            }
        }

        public bool Block(string Email, bool IsActive)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var u = db.AspNetUsers.FirstOrDefault(x => x.Email == Email);
                u.IsActive = IsActive;
                db.SaveChanges();
                return true;
            }
        }
        public bool ResetPassword(string Email)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                db.sp_ResetPassword(Email);
                int i = db.SaveChanges();
                return true;
            }
        }
        public bool VerifyEmail(string Email)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                db.AspNetUsers.Where(x => x.Email == Email).FirstOrDefault().EmailConfirmed = true;
                int i = db.SaveChanges();
                return true;
            }
        }


        public bool CopySection(int Toid, int Sid, int Fromid)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    if (Sid > 0)
                    {
                        // Copy a single section with subsections and videos
                        CopySingleSection(db, Sid, Toid);
                    }
                    else
                    {
                        // Copy ALL sections from the source course
                        var course = db.tbl_Courses.Find(Fromid);
                        // Materialize to avoid collection-modified during enumeration
                        var sectionIds = course.tbl_Section.Select(s => s.Section_ID).ToList();
                        foreach (var secId in sectionIds)
                        {
                            CopySingleSection(db, secId, Toid);
                        }
                    }
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Copies a single section along with its subsections and all videos,
        /// preserving the full hierarchy (Section → SubSection → Video).
        /// </summary>
        private void CopySingleSection(FAMEEntities db, int sourceSectionId, int targetCourseId)
        {
            tbl_Section sec = db.tbl_Section.Find(sourceSectionId);
            if (sec == null) return;

            // 1. Create the new section
            var newSec = new tbl_Section()
            {
                Course_Fid = targetCourseId,
                Section_Name = sec.Section_Name,
                Section_Price = sec.Section_Price,
                IsActive = sec.IsActive,
                SortID = sec.SortID,
            };
            db.tbl_Section.Add(newSec);
            db.SaveChanges(); // Save to get the new Section_ID

            // 2. Copy videos that belong directly to this section (no subsection)
            var directVideos = sec.tbl_Video
                .Where(v => v.SectionSub_Fid == null || v.SectionSub_Fid == 0)
                .ToList();
            foreach (var v in directVideos)
            {
                var newVid = new tbl_Video()
                {
                    IsActive = v.IsActive,
                    Course_Fid = targetCourseId,
                    Section_Fid = newSec.Section_ID,
                    SectionSub_Fid = null,
                    Video_Length = v.Video_Length,
                    Video_Name = v.Video_Name,
                    Video_Path = v.Video_Path,
                    Video_ShortDescription = v.Video_ShortDescription,
                    Type = v.Type,
                    SortID = v.SortID,
                    Video_Tags = v.Video_Tags,
                    Difficulty = v.Difficulty,
                };
                db.tbl_Video.Add(newVid);
            }
            db.SaveChanges();

            // 3. Copy subsections and their videos
            var subSections = sec.tbl_SectionSub.ToList();
            foreach (var sub in subSections)
            {
                var newSub = new tbl_SectionSub()
                {
                    SectionSub_Name = sub.SectionSub_Name,
                    Course_Fid = targetCourseId,
                    Section_Fid = newSec.Section_ID,
                    SortID = sub.SortID,
                    IsActive = sub.IsActive,
                };
                db.tbl_SectionSub.Add(newSub);
                db.SaveChanges(); // Save to get the new SubSection ID

                // Copy videos belonging to this subsection
                var subVideos = sub.tbl_Video.ToList();
                foreach (var v in subVideos)
                {
                    var newVid = new tbl_Video()
                    {
                        IsActive = v.IsActive,
                        Course_Fid = targetCourseId,
                        Section_Fid = newSec.Section_ID,
                        SectionSub_Fid = newSub.ID,
                        Video_Length = v.Video_Length,
                        Video_Name = v.Video_Name,
                        Video_Path = v.Video_Path,
                        Video_ShortDescription = v.Video_ShortDescription,
                        Type = v.Type,
                        SortID = v.SortID,
                        Video_Tags = v.Video_Tags,
                        Difficulty = v.Difficulty,
                    };
                    db.tbl_Video.Add(newVid);
                }
                db.SaveChanges();
            }
        }


        #endregion

        #region User Lock Out
        public bool PasswordChanged(string id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var u = db.AspNetUsers.FirstOrDefault(x => x.Id == id);
                u.ShouldChangePassword = false;
                db.SaveChanges();
                return true;
            }
        }
        public AddPhoneNumberViewModel UserMobile(string id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var u = db.tbl_User.FirstOrDefault(x => x.User_AspUser == id);
                return new AddPhoneNumberViewModel()
                {
                    Number = u.User_Mobile,
                    CountryID = u.CountryID.ToString(),
                };
            }
        }
        public bool ChangeUserMobile(string id, string Number)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var u = db.tbl_User.FirstOrDefault(x => x.User_AspUser == id);
                u.User_Mobile = Number;
                db.SaveChanges();
                return true;
            }
        }
        public Tuple<string, int> GenerateVerificationCode(string id, CodeType CodeType, bool Again = false)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var user = db.AspNetUsers.Find(id);
                    if (user.CodeExpiry == null || Again || user.CodeExpiry.Value.AddMinutes(10) < Common.GetCurrentDate() || CodeType.ToString() != user.CodeType)
                    {
                        string r = new Random().Next(0, 1000000).ToString("D6");
                        user.VerificationCode = r;
                        user.CodeExpiry = Common.GetCurrentDate();
                        user.CodeType = CodeType.ToString();
                        db.SaveChanges();
                        return new Tuple<string, int>(r, 0);
                    }
                    var a = user.CodeExpiry.Value.AddMinutes(30).Subtract(Common.GetCurrentDate()).Minutes;
                    return new Tuple<string, int>(user.VerificationCode, a);
                }
                catch { return new Tuple<string, int>("", 0); }
            }
        }
        public string GenVerifCodeSession(string Email, bool Again = false)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var old = (VerifyCodeVM)HttpContext.Current.Session["VerifyCode"];
                    string r = new Random().Next(0, 1000000).ToString("D6");
                    if (old != null && old.Email == Email)
                    {
                        var a = old.Date.AddMinutes(30).Subtract(Common.GetCurrentDate()).Minutes;
                        if (a > 20)
                            r = old.Code;
                    }

                    HttpContext.Current.Session["VerifyCode"] = new VerifyCodeVM
                    {
                        Email = Email,
                        Code = r,
                        Date = Common.GetCurrentDate(),
                    };
                    return r;
                }
                catch { return ""; }
            }
        }
        public bool ConfirmEmail(string Email, string Code)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var user = db.AspNetUsers.FirstOrDefault(x => x.UserName == Email);
                    bool IsValid = user.CodeExpiry.Value.AddMinutes(30) > Common.GetCurrentDate() && Code == user.VerificationCode;
                    if (IsValid)
                    {
                        user.EmailConfirmed = true;
                        db.SaveChanges();
                        return true;
                    }
                    return false;
                }
                catch { return false; }
            }
        }
        public bool ConfirmNewEmail(string Email, string Code)
        {
            try
            {
                var d = (VerifyCodeVM)HttpContext.Current.Session["VerifyCode"];
                if (d != null)
                {
                    d.IsVerified = d.Email == Email && d.Code == Code;
                    HttpContext.Current.Session["VerifyCode"] = d;

                    return d.Email.ToLower().Trim() == Email.ToLower().Trim() && d.Code == Code;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                JzLogger.WriteError(ex, "Session is null");
                return false;
            }
        }
        public bool SaveExpire(UserLockoutVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var user = db.tbl_UserLockout.Find(model.ID) ?? new tbl_UserLockout();
                    user.LockoutDT = Common.StringToDate(model.LockoutDT);
                    user.Remarks = model.Remarks;
                    user.AspUserID = model.AspUserID;
                    user.ID = model.ID;
                    if (model.ID == 0)
                        db.tbl_UserLockout.Add(user);
                    db.SaveChanges();
                    return true;
                }
                catch { return false; }
            }
        }
        public bool DeleteExpire(int ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    db.tbl_UserLockout.Remove(db.tbl_UserLockout.Find(ID));
                    db.SaveChanges();
                    return true;
                }
                catch { return false; }
            }
        }
        public tbl_UserLockout IsExpired(string ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var Date = DateTime.Now.AddHours(5);
                    return db.tbl_UserLockout.Where(x => x.AspUserID == ID && x.LockoutDT < Date).FirstOrDefault();
                }
                catch { return null; }
            }
        }
        public List<UserLockoutVM> GetExpireableStudent()
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    return db.tbl_UserLockout.ToList().ConvertAll(x => new UserLockoutVM
                    {
                        AspUserID = x.AspUserID,
                        LockoutDT = Common.DateToString(x.LockoutDT),
                        Remarks = x.Remarks,
                        UserName = x.AspNetUsers.tbl_User.FirstOrDefault()?.User_Name,
                        UserEmail = x.AspNetUsers.Email,
                        LockoutDate = x.LockoutDT,
                        ID = x.ID
                    });
                }
                catch { return null; }
            }
        }
        public UserLockoutVM GetExpire(int ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var x = db.tbl_UserLockout.Find(ID);
                    return new UserLockoutVM()
                    {
                        AspUserID = x.AspUserID,
                        LockoutDT = Common.DateToString(x.LockoutDT),
                        Remarks = x.Remarks,
                        UserName = x.AspNetUsers.tbl_User.FirstOrDefault()?.User_Name,
                        UserEmail = x.AspNetUsers.Email,
                        LockoutDate = x.LockoutDT,
                        ID = x.ID
                    };
                }
                catch { return null; }
            }
        }


        public int? GetType(string ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var Date = DateTime.Now.AddHours(5);
                    return db.tbl_User.FirstOrDefault(x => x.User_AspUser == ID).Type;
                }
                catch { return 0; }
            }
        }
        //public bool IsAllowed(string ID,string UserID)
        //{
        //    var AmiID = System.Configuration.ConfigurationManager.AppSettings["AmiID"];
        //    return AmiID == UserID || AmiID == ID;
        //    //using (FAMEEntities db = new FAMEEntities())
        //    //{
        //    //    try
        //    //    {
        //    //        return db.tbl_Master.Any(x => x.Master_Value == ID);
        //    //    }
        //    //    catch  { return false; }
        //    //}
        //}

        #endregion

        #region Requests
        public bool AcceptRequest(int id, string UserID, decimal? Price)
        {
            try
            {

                using (FAMEEntities db = new FAMEEntities())
                {
                    var req = db.tbl_Request.Find(id);
                    if (req == null)
                    {
                        throw new Exception($"Request with ID {id} not found.");
                    }
                    IdentityMessage message = new IdentityMessage()
                    {
                        Subject = "Accepted",
                        Body = "<h1 style=\"text-align: center; \"><b><font style=\"background-color: rgb(255, 255, 255);\" color=\"#9c00ff\">First Aid Made Easy</font></b></h1><h2 style=\"text-align: center; \"><font color=\"#311873\">Thank You For Choosing Us!&nbsp; Please Visit Our Website To Continue Your Lectures.</font></h2><h2 style=\"text-align: center; \"></font></h2>",
                        Destination = req.StudentEmail
                    };
                    //==============================
                    //===  Enroll For Section  =====
                    //==============================
                    if (req.SectionID > 0 && req.StudentID != null)
                    {
                        var section = new SectionRepository().GetByID(req.SectionID);
                        if (section == null)
                        {
                            throw new Exception($"Section with ID {req.SectionID} not found.");
                        }
                        EnrollmentMasterVM enrlm = new EnrollmentMasterVM
                        {
                            Enrollment_Price = Price ?? section.Section_Price * (req.Duration / 30),
                            TeacherFid = section.TeacherFid,
                            StudentFid = req.StudentID,
                            Enrollment_EndDate = Common.GetEndDate(req.Duration),
                            ByManual = false,
                            ByRequest = true,
                            ApprovedBy = UserID,
                            Details = new List<EnrollmentDetailVM>() { new EnrollmentDetailVM { Section_Fid = section.Section_ID } }
                        };
                        if (Save(enrlm))
                        {
                            req.IsAccepted = true;
                            db.Entry(req).State = EntityState.Modified;
                            db.SaveChanges();
                            //Common.SendMail(message);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    //==============================
                    //===  Enroll For Package  =====
                    //==============================
                    else if (req.PackageID > 0)
                    {
                        bool f = true;
                        f = f && Enroll(new AddSubscriptioVM
                        {
                            UserID = UserID,
                            PackageID = req.PackageID,
                            Duration = req.Duration,
                            StudentFid = req.StudentID,
                            EnrollmentFor = "Package",
                            Expire = false,
                            Payment = req.Payment,
                            EndDate = string.Format("{0:dd/MM/yyyy}", Common.GetEndDate(req.Duration)),
                            ExamAttemptID = req.ExamAttemptID,
                        }, false, true, Price);


                        req.IsAccepted = true;
                        db.Entry(req).State = EntityState.Modified;
                        db.SaveChanges();
                        //Common.SendMail(message);
                        return true;
                    }
                    //==============================
                    //===  Enroll For Course  =====
                    //==============================
                    else if (req.CourseID > 0)
                    {
                        var course = new CourseRepository().GetByID(req.CourseID ?? 0);
                        if (course == null)
                        {
                            throw new Exception($"Course with ID {req.CourseID} not found.");
                        }
                        EnrollmentMasterVM enrlm = new EnrollmentMasterVM
                        {
                            Enrollment_Price = Price ?? course.Course_Price * (req.Duration / 30),
                            Enrollment_EndDate = Common.GetEndDate(req.Duration),
                            TeacherFid = course.Teacher_Fid,
                            StudentFid = req.StudentID,
                            ByManual = false,
                            ByRequest = true,
                            ApprovedBy = UserID,
                            Details = new List<EnrollmentDetailVM>() { new EnrollmentDetailVM { CourseID = course.Course_Id } }
                        };
                        if (Save(enrlm))
                        {
                            req.IsAccepted = true;
                            db.Entry(req).State = EntityState.Modified;
                            db.SaveChanges();
                            //Common.SendMail(message);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        throw new Exception($"Request {id} has no valid PackageID, SectionID, or CourseID.");
                    }
                }
            }
            catch
            {
                throw;
            }
        }
        public bool AcceptRequest(int id, int Price, int Duration, string Status, string UserID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var req = db.tbl_Request.Find(id);
                //==============================
                //===  Enroll For Section  =====
                //==============================
                if (req.SectionID > 0 && req.StudentID != null)
                {
                    var section = new SectionRepository().GetByID(req.SectionID);
                    EnrollmentMasterVM enrlm = new EnrollmentMasterVM
                    {
                        Enrollment_Price = Price,
                        TeacherFid = section.TeacherFid,
                        StudentFid = req.StudentID,
                        Enrollment_Status = Status,
                        Enrollment_EndDate = Common.GetEndDate(Duration),
                        ByManual = false,
                        ByRequest = true,
                        ByExtension = true,
                        ApprovedBy = UserID,
                        Details = new List<EnrollmentDetailVM>() { new EnrollmentDetailVM { Section_Fid = section.Section_ID } }
                    };
                    if (Save(enrlm, Expire: true))
                    {
                        req.IsAccepted = true;
                        db.Entry(req).State = EntityState.Modified;
                        db.SaveChanges();
                        //Common.SendMail(message);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                //==============================
                //===  Enroll For Package  =====
                //==============================
                else if (req.PackageID > 0)
                {
                    var pack = db.tbl_Package.Find(req.PackageID);
                    var packDurID = db.tbl_PackageDuration.FirstOrDefault(x => x.PackageID == req.PackageID && x.Duration == Duration).ID;
                    EnrollmentMasterVM enrlm = new EnrollmentMasterVM
                    {
                        Enrollment_Price = Price,
                        TeacherFid = pack.tbl_PackageDetail?.FirstOrDefault()?.tbl_Courses?.TeacherFid,
                        Enrollment_EndDate = Common.GetEndDate(Duration),
                        StudentFid = req.StudentID,
                        ByManual = false,
                        ByRequest = true,
                        Enrollment_Status = EnrollStatus.Approved.ToString(),
                        ByExtension = true,
                        ApprovedBy = UserID,
                        PackageDurationFid = packDurID,
                        PackageId = req.PackageID,
                    };

                    bool f = true;
                    f = f && Save(enrlm, req.Payment == "Installments");
                    req.IsAccepted = true;
                    req.Duration = Duration;
                    req.PackageDurationID = packDurID;
                    db.Entry(req).State = EntityState.Modified;
                    db.SaveChanges();
                    SendMail(req.StudentEmail);
                    return true;
                }
                //==============================
                //===  Enroll For Course  =====
                //==============================
                else
                {
                    var course = new CourseRepository().GetByID(req.CourseID ?? 0);
                    EnrollmentMasterVM enrlm = new EnrollmentMasterVM
                    {
                        Enrollment_Price = course.Course_Price * (req.Duration / 30),
                        Enrollment_EndDate = Common.GetEndDate(req.Duration),
                        TeacherFid = course.Teacher_Fid,
                        StudentFid = req.StudentID,
                        ByManual = false,
                        ByRequest = true,
                        ApprovedBy = UserID,
                        Details = new List<EnrollmentDetailVM>() { new EnrollmentDetailVM { CourseID = course.Course_Id } }
                    };
                    if (Save(enrlm))
                    {
                        req.IsAccepted = true;
                        db.Entry(req).State = EntityState.Modified;
                        db.SaveChanges();
                        //Common.SendMail(message);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        public bool AddRequest(RequestVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_Request request = db.tbl_Request.FirstOrDefault(x => x.StudentID == model.StudentID);
                if (request == null)
                {
                    request = new tbl_Request();
                }
                request.CourseID = model.CourseID;
                request.Duration = model.Duration;
                request.SectionID = model.SectionID == 0 ? null : model.SectionID;
                request.StudentID = model.StudentID;
                request.RequestDate = Common.GetCurrentDate();
                request.StudentEmail = model.StudentEmail;
                request.PackageID = model.PackageID;
                request.Payment = model.Payment;
                request.RequestFor = model.RequestFor ?? RequestType.Register.ToString();
                request.TeacherFid = db.tbl_Package.Find(model.PackageID)?.tbl_PackageDetail?.FirstOrDefault()?.tbl_Courses?.TeacherFid;
                request.IsAccepted = model.IsAccepted;
                request.ExamAttemptID = model.ExamAttemptID;

                if (request.RequestID == 0)
                {
                    db.tbl_Request.Add(request);
                }

                db.SaveChanges();

                if (request.IsAccepted == true) { AcceptRequest(request.RequestID, null, 0); }

                return true;
            }
        }
        public List<sp_lstRequests_Result> GetList(DateTime? DateFrom, DateTime? DateTo, int Type, string ReqFor)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var List = db.sp_lstRequests(DateFrom, DateTo, Type, ReqFor).ToList();
                    return List;
                }
            }
            catch
            {
                throw;
            }
        }

        public void DeleteFiles(tbl_User tuser)
        {
            if (tuser != null)
            {
                Common.DeleteFile(tuser.User_Pic);
                Common.DeleteFile(tuser.CNICBack);
                Common.DeleteFile(tuser.CNICFront);
                Common.DeleteFile(tuser.StuCardBack);
                Common.DeleteFile(tuser.StuCardFront);
            }
        }

        public bool DeleteRequest(string id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var tuser = db.tbl_User.FirstOrDefault(x => x.User_AspUser == id);
                    DeleteFiles(tuser);
                    var run = db.sp_DeleteUser(id);
                    return true;

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    ex.Entries.ToList();
                    return true;
                }
            }
        }

        public bool HasPendingRequest(string studentId)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_Request.Any(x => x.StudentID == studentId && x.IsAccepted == null);
            }
        }

        public SubscriptionsVM GetInvoiceDetail(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                DateTime date = Common.GetCurrentDate();
                var x = db.tbl_EnrollmentMaster.Find(id);
                if (x == null) return null;

                var a = new SubscriptionsVM()
                {
                    ID = x.tbl_PackageDuration?.PackageID,
                    Amount = x.tbl_PackageDuration?.Price ?? x.Enrollment_Price,
                    CourseName = x.tbl_PackageDuration?.tbl_Package?.PackageName ?? "Course Enrollment",
                    DEnrollmentDate = x.Enrollment_Date,
                    DExpiryDate = x.Enrollment_EndDate,
                    EnrollmentMethod = x.CouponID == null ? "Payment" : "Coupon",
                    RemainingDays = x.Enrollment_EndDate.HasValue ? Math.Round(x.Enrollment_EndDate.Value.Subtract(Common.GetCurrentDate()).TotalDays) : 0,
                    Status = x.Enrollment_EndDate < date ? "Expired" : (x.IsExpired == true) ? "Override" : "Continue",
                    GivenBy = x.AspNetUsers1?.tbl_User?.FirstOrDefault()?.User_Name,
                    EnrollmentID = x.Enrollment_Id,
                };

                if (x.tbl_PackageDuration != null)
                {
                    a.CorList = db.tbl_Package.FirstOrDefault(p => p.PackageID == (x.tbl_PackageDuration.PackageID ?? 0)).tbl_PackageDetail.Select(y => new SubscriptionsVM
                    {
                        Amount = y.tbl_Courses.Course_Price,
                        CourseName = y.tbl_Courses.Course_Name,
                        ID = y.tbl_Courses.tbl_Video.Count()
                    }).ToList();
                }
                else
                {
                    a.CorList = x.tbl_EnrollmentDetail.Select(y => new SubscriptionsVM
                    {
                        Amount = y.tbl_Courses?.Course_Price ?? 0,
                        CourseName = y.tbl_Courses?.Course_Name ?? y.tbl_Section?.Section_Name,
                        ID = 0
                    }).ToList();
                }

                return a;
            }
        }

        public bool AddExtensionRequest(int enrollId, string studentId, string studentEmail)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var en = db.tbl_EnrollmentMaster.AsNoTracking().FirstOrDefault(x => x.Enrollment_Id == enrollId);
                if (en == null) return false;

                int? PackageDurationFid = en.PackageDurationFid;
                var PackID = db.tbl_PackageDuration.AsNoTracking().FirstOrDefault(x => x.ID == PackageDurationFid)?.PackageID;

                tbl_Request req = new tbl_Request
                {
                    RequestFor = RequestType.Extension.ToString(),
                    PackageID = PackID ?? 0,
                    Duration = 0,
                    StudentID = studentId,
                    Payment = "OneTime",
                    StudentEmail = studentEmail,
                    RequestDate = Common.GetCurrentDate(),
                };

                db.tbl_Request.Add(req);
                db.SaveChanges();
                return true;
            }
        }

        #endregion
    }
}