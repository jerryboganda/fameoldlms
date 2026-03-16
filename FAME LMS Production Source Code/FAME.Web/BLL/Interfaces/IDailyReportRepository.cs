using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IDailyReportRepository
    {
        List<DailyReportVM> GetList(string UserID, DateTime? DateFrom, DateTime? DateTo);
        List<DailyReportVM> GetAllReports();
        List<DailyReportVM> GetRecentReports(int count);
        List<DailyReportVM> GetUserRecentReports(int userId, int count);
        int GetTotalReportsCount();
        int GetUserReportsCount(int userId);
        DailyReportVM GetByID(int id);
        bool Create(DailyReportVM model);
        bool Delete(int id);
    }
}
