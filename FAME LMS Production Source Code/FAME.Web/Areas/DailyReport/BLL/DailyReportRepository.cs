using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    public class DailyReportRepository : IDailyReportRepository
    {
        private readonly FAMEEntities db = new FAMEEntities();

        public List<DailyReportVM> GetList(string UserID, DateTime? DateFrom, DateTime? DateTo)
        {
            int userId = int.Parse(UserID);
            return db.tbl_DailyReport.Where(x => x.UserFid == userId && x.Date >= DateFrom && x.Date <= DateTo).ToList()
                .ConvertAll(x => new DailyReportVM
                {
                    ID = x.ID,
                    Body = x.Body,
                    Date = string.Format("{0:yyyy-MM-dd}", x.Date),
                    UserFid = x.UserFid,
                    FilePath = "~/Images/" + x.FilePath,
                    CreatedDT = x.CreatedDT
                }).ToList();
        }

        public List<DailyReportVM> GetAllReports()
        {
            return db.tbl_DailyReport.ToList()
                .ConvertAll(x => new DailyReportVM
                {
                    ID = x.ID,
                    Body = x.Body,
                    Date = string.Format("{0:yyyy-MM-dd}", x.Date),
                    UserFid = x.UserFid,
                    FilePath = "~/Images/" + x.FilePath,
                    CreatedDT = x.CreatedDT
                }).ToList();
        }

        public List<DailyReportVM> GetRecentReports(int count)
        {
            return db.tbl_DailyReport.OrderByDescending(x => x.CreatedDT).Take(count).ToList()
                .ConvertAll(x => new DailyReportVM
                {
                    ID = x.ID,
                    Body = x.Body,
                    Date = string.Format("{0:yyyy-MM-dd}", x.Date),
                    UserFid = x.UserFid,
                    FilePath = "~/Images/" + x.FilePath,
                    CreatedDT = x.CreatedDT
                }).ToList();
        }

        public List<DailyReportVM> GetUserRecentReports(int userId, int count)
        {
            return db.tbl_DailyReport.Where(x => x.UserFid == userId).OrderByDescending(x => x.CreatedDT).Take(count).ToList()
                .ConvertAll(x => new DailyReportVM
                {
                    ID = x.ID,
                    Body = x.Body,
                    Date = string.Format("{0:yyyy-MM-dd}", x.Date),
                    UserFid = x.UserFid,
                    FilePath = "~/Images/" + x.FilePath,
                    CreatedDT = x.CreatedDT
                }).ToList();
        }

        public int GetTotalReportsCount()
        {
            return db.tbl_DailyReport.Count();
        }

        public int GetUserReportsCount(int userId)
        {
            return db.tbl_DailyReport.Count(x => x.UserFid == userId);
        }

        public DailyReportVM GetByID(int id)
        {
            var report = db.tbl_DailyReport.Find(id);
            if (report == null) return null;

            return new DailyReportVM
            {
                ID = report.ID,
                Body = report.Body,
                Date = string.Format("{0:yyyy-MM-dd}", report.Date),
                UserFid = report.UserFid,
                FilePath = "~/Images/" + report.FilePath,
                CreatedDT = report.CreatedDT
            };
        }

        public bool Create(DailyReportVM model)
        {
            if (model.ID == 0)
            {
                var report = new tbl_DailyReport
                {
                    Body = model.Body,
                    Date = model.DateDT,
                    UserFid = model.UserFid,
                    FilePath = model.FilePath,
                    CreatedDT = Common.GetCurrentDate()
                };

                db.tbl_DailyReport.Add(report);
                return db.SaveChanges() > 0;
            }
            else
            {

                var report = db.tbl_DailyReport.Find(model.ID);
                if (report == null) return false;

                report.Body = model.Body;
                report.Date = model.DateDT;
                report.ModifyDT = Common.GetCurrentDate();
                report.FilePath = model.FilePath ?? report.FilePath;

                return db.SaveChanges() > 0;
            }
        }

        public bool Delete(int id)
        {
            try
            {
                var report = db.tbl_DailyReport.Find(id);
                if (report == null) return false;

                db.tbl_DailyReport.Remove(report);
                return db.SaveChanges() > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}