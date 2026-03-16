using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;

namespace First_Aid_Made_Easy.BLL
{
    public class SectionRepository : ISectionRepository
    {
        public List<SectionVM> GetSectionsByCourse(int courseID, string StudentFid)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    DateTime DNow = Common.GetCurrentDate();
                    List<tbl_Section> list = db.tbl_Section.Where(x => x.Course_Fid == courseID && (x.IsActive ?? true)).ToList();
                    List<SectionVM> slist = new List<SectionVM>();
                    foreach (var x in list)
                    {
                        SectionVM sec = new SectionVM();
                        var en = db.tbl_EnrollmentDetail.Where(y => (y.Section_Fid == x.Section_ID || y.CourseID == x.Course_Fid) && y.tbl_EnrollmentMaster.StudentFid == StudentFid && y.IsExpired != true && y.tbl_EnrollmentMaster.IsExpired != true && y.tbl_EnrollmentMaster.Enrollment_EndDate >= DNow).FirstOrDefault();

                        sec.Section_ID = x.Section_ID;
                        sec.Section_Name = x.Section_Name;
                        sec.Section_Price = x.Section_Price;
                        sec.Course_Fid = x.Course_Fid;
                        sec.TeacherFid = x.tbl_Courses.TeacherFid;
                        sec.SortID = x.SortID ?? 0;
                        sec.DurationInMin = x.tbl_Video.Sum(s => s.Video_Length) ?? 0;
                        sec.Lessons = x.tbl_Video.Count;
                        sec.EndDate = en?.tbl_EnrollmentMaster?.Enrollment_EndDate;
                        slist.Add(sec);
                    }
                    return slist.OrderBy(x => x.SortID).ToList();
                }
                catch
                {
                    throw;
                }
            }
        }
        public SectionVM GetByIDForTakeLec(int sectionID, string StudentFid)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    DateTime DNow = Common.GetCurrentDate();
                    tbl_Section x = db.tbl_Section.Find(sectionID);
                    SectionVM sec = new SectionVM();
                    var en = db.tbl_EnrollmentDetail.Where(y => (y.Section_Fid == x.Section_ID || y.CourseID == x.Course_Fid) && y.tbl_EnrollmentMaster.StudentFid == StudentFid && y.IsExpired != true && y.tbl_EnrollmentMaster.IsExpired != true && y.tbl_EnrollmentMaster.Enrollment_EndDate >= DNow).FirstOrDefault()?.tbl_EnrollmentMaster;
                    //======== If Enrollments Have Installments =====

                    sec.Section_ID = x.Section_ID;
                    sec.Section_Name = x.Section_Name;
                    sec.Section_Price = x.Section_Price;
                    sec.Course_Fid = x.Course_Fid;
                    sec.TeacherFid = x.tbl_Courses.TeacherFid;

                    if (en != null)
                    {
                        if (en.tbl_StudentInstallments.Count > 0)
                        {
                            var ins = en.tbl_StudentInstallments.Where(z => z.IsPaid != true).OrderBy(z => z.InstallmentDate).FirstOrDefault();
                            sec.NextInstallment = ins?.InstallmentDate;
                            sec.InstallmentPrice = ins?.InstallmentPrice;
                            sec.PendingInstallment = en.tbl_StudentInstallments.Where(z => z.InstallmentDate > DNow).Count();
                            //======== If Enrollments Have Pending Installments =====
                            if ((en.tbl_StudentInstallments.Any(z => z.InstallmentDate < DNow && z.IsPaid != true)))
                            {
                                sec.isExpired = true;
                            }
                            else
                            {
                                sec.isExpired = false;
                            }
                        }
                        sec.EndDate = en?.Enrollment_EndDate ?? DNow;
                        sec.RemainingDays = en.Enrollment_EndDate.HasValue ? en.Enrollment_EndDate.Value.Subtract(DNow).TotalDays : 0;
                    }
                    else
                    {
                        sec.EndDate = DNow;
                        sec.isExpired = false;
                        sec.RemainingDays = 0;

                    }
                    return sec;
                }
                catch
                {
                    throw;
                }
            }
        }

        public bool DeleteSection(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var sec = db.tbl_Section.Find(id);
                sec.IsActive = false;
                db.SaveChanges();
                return true;
            }
        }

        public int SaveSection(SectionVM section)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    tbl_Section model = new tbl_Section();
                    if (section.Section_ID > 0)
                    {
                        model = db.tbl_Section.Find(section.Section_ID);
                    }
                    model.Section_ID = section.Section_ID;
                    model.Section_Name = section.Section_Name;
                    model.Section_Price = section.Section_Price;
                    model.Course_Fid = section.Course_Fid;
                    if (!(section.Section_ID > 0))
                    {
                        db.tbl_Section.Add(model);
                    }
                    db.SaveChanges();
                    section.Section_ID = model.Section_ID;
                    return section.Section_ID;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public SectionVM GetByID(int? id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    tbl_Section model = db.tbl_Section.Find(id);
                    SectionVM s = new SectionVM()
                    {
                        Section_ID = model.Section_ID,
                        Section_Name = model.Section_Name,
                        Section_Price = model.Section_Price,
                        Course_Fid = model.Course_Fid,
                        TeacherFid = model.tbl_Courses.TeacherFid
                    };
                    return s;
                }
                catch
                {
                    return null;
                }
            }
        }
        public List<SelectListItem> List(string id = "", bool isAdmin = true)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var model = db.tbl_Section.Where(x => x.tbl_Courses.TeacherFid == id || isAdmin).OrderBy(x => x.Section_Name).ToList();

                    return model.ConvertAll(x => new SelectListItem
                    {
                        Text = x.Section_Name + " => " + x.tbl_Courses.Course_Name,
                        Value = x.Section_ID.ToString()
                    });
                }
                catch
                {
                    return null;
                }
            }
        }

    }
}