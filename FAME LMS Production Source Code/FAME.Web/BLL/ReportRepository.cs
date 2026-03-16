using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Web;

namespace First_Aid_Made_Easy.BLL
{
    public class ReportRepository : IReportRepository
    {
        public ReportRepository() { }
        public StudentProgressReportVM GetProgress(string id)
        {
            var last15days = DateTime.Now.AddDays(-15);
            using (FAMEEntities db = new FAMEEntities())
            {
                var l = db.tbl_Progress
                .Where(x => x.LastOpenDT >= last15days && x.Student_Fid == id)
                .Select(p => new ScreenTimeReportVM
                {
                    CourseID = p.tbl_Video.Course_Fid,
                    CourseName = p.tbl_Video.tbl_Courses.Course_Name,
                    LastOpenDT = p.LastOpenDT,
                    ScreenTime = p.ScreenTime,
                })
                .OrderByDescending(x => x.LastOpenDT).ToList()
                 .GroupBy(x => new { x.CourseID, x.LastOpenDT.Value.Date })
                 .Select(p => new ScreenTimeReportVM
                 {
                     CourseID = p.Key.CourseID,
                     CourseName = p.FirstOrDefault().CourseName,
                     LastOpenDT = p.Key.Date,
                     ScreenTime = p.Sum(s => s.ScreenTime),
                 }).ToList();

                var re = db.tbl_ResultMaster
                     .Where(x => x.StudentID == id)
                     .OrderByDescending(x => x.Datetime)
                     .Take(15)
                     .Select(r => new ResultReportVM
                     {
                         Date = r.Datetime,
                         Total = r.tbl_ResultDetail.Count(),
                         Obtained = r.tbl_ResultDetail.Where(x => x.IsTrue == true).Count(),
                     })
                     .ToList();

                return new StudentProgressReportVM
                {
                    ScreenTime = l,
                    Results = re
                };
            }
        }
        public List<sp_rptReultReport_Result> GetResultReport(int universityId, int paperId, string sectionId, string studentId)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.sp_rptReultReport(universityId, paperId, sectionId, studentId, null, null).ToList();
            }
        }

        public decimal GetUniversityTotalUsers(int universityId)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return Convert.ToDecimal(db.tbl_User.Where(x => x.Type == universityId).Count());
            }
        }

        public async System.Threading.Tasks.Task<List<EmailReportVM>> GetEmailReport(System.DateTime dateFrom, System.DateTime dateTo)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return await (from emailLog in db.tbl_EmailLogs
                                   join tu in db.tbl_User on emailLog.StudentID equals tu.User_AspUser
                                   join st in db.AspNetUsers on emailLog.StudentID equals st.Id
                                   where emailLog.SentAt >= dateFrom && emailLog.SentAt <= dateTo
                                   select new EmailReportVM
                                   {
                                       SentAt = emailLog.SentAt,
                                       Email = st.Email,
                                       InactiveDays = emailLog.InactiveDays,
                                       Phone = tu.User_Mobile,
                                       StudentName = tu.User_Name,
                                       Type = emailLog.Type,
                                       StudentID = emailLog.StudentID,
                                   }).ToListAsync();
            }
        }

        public List<sp_rptQuestionStats_Result> GetQuestionStats(int paperId, int systemId)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.sp_rptQuestionStats(paperId, systemId, null).ToList();
            }
        }

        public List<TrialStudentsVM> GetTrialStudents()
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_ResultMaster.Where(x => x.StudentID == null && x.TrialStudent != null)
                    .GroupBy(x => x.TrialStudent).Select(y => new TrialStudentsVM
                    {
                        Student = y.Key,
                        Count = y.Count(),
                        LastTest = y.Max(z => z.Datetime)
                    }).OrderByDescending(x => x.LastTest).ToList();
            }
        }
    }

    public class StudentProgressReportVM
    {
        public List<ScreenTimeReportVM> ScreenTime { get; set; }
        public List<ResultReportVM> Results { get; set; }
    }

    public class ScreenTimeReportVM
    {
        public int? CourseID { get; set; }
        public string CourseName { get; set; }
        public DateTime? LastOpenDT { get; set; }
        public int? ScreenTime { get; set; }
    }

    public class ResultReportVM
    {
        public DateTime? Date { get; set; }
        public int? Total { get; set; }
        public int? Obtained { get; set; }
    }
}