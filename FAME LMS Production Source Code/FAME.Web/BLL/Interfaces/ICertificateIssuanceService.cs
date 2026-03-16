using First_Aid_Made_Easy.Models.Certificate;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface ICertificateIssuanceService
    {
        // Issue
        CertificateIssueVM IssueCertificate(string userId, int ruleSetId, string issuedBy = null);
        CertificateIssueVM IssueCertificateManual(string userId, int templateId, int courseId, string issuedBy);

        // Query
        CertificateIssueVM GetIssueById(int id);
        CertificateIssueVM GetIssueByPublicId(string publicId);
        List<CertificateIssueListVM> GetIssuesForStudent(string userId, string statusFilter = null, string typeFilter = null, string search = null);
        List<CertificateIssueListVM> GetAllIssues(string statusFilter = null, string search = null, int page = 1, int pageSize = 20);
        int GetTotalIssueCount(string statusFilter = null, string search = null);

        // Actions
        void ApprovePendingIssue(int issueId, string approvedBy);
        void RejectPendingIssue(int issueId, string reason, string rejectedBy);
        void RevokeCertificate(int issueId, string reason, string revokedBy);
        CertificateIssueVM ReIssueCertificate(int originalIssueId, string correctedName, decimal? correctedCredits, string notes, string reIssuedBy);

        // Student certificate center
        StudentCertificateCenterVM GetStudentCertificateCenter(string userId, string search = null, string typeFilter = null, string statusFilter = null);

        // Helpers
        bool HasActiveCertificate(string userId, int courseId, int templateId);
    }
}
