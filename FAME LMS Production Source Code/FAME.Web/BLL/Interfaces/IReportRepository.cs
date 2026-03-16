using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IReportRepository
    {
        StudentProgressReportVM GetProgress(string id);
        List<sp_rptReultReport_Result> GetResultReport(int universityId, int paperId, string sectionId, string studentId);
        decimal GetUniversityTotalUsers(int universityId);
        System.Threading.Tasks.Task<List<EmailReportVM>> GetEmailReport(System.DateTime from, System.DateTime to);
        List<sp_rptQuestionStats_Result> GetQuestionStats(int paperId, int systemId);
        List<TrialStudentsVM> GetTrialStudents();
    }
}
