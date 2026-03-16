using First_Aid_Made_Easy.Models.Certificate;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface ICertificateCorrectionService
    {
        int SubmitRequest(int issueId, string userId, string requestType, string details);
        CertificateCorrectionVM GetById(int id);
        List<CertificateCorrectionVM> GetOpenRequests();
        List<CertificateCorrectionVM> GetRequestsForIssue(int issueId);
        List<CertificateCorrectionVM> GetRequestsForUser(string userId);
        void ResolveRequest(int id, string resolvedBy, string notes, bool approved);
    }
}
