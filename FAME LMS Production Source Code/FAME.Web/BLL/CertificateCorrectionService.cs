using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models.Certificate;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    public class CertificateCorrectionService : ICertificateCorrectionService
    {
        private readonly ICertificateAuditService _auditService;
        private readonly ILogger _logger;

        public CertificateCorrectionService(ICertificateAuditService auditService, ILogger logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        public int SubmitRequest(int issueId, string userId, string requestType, string details)
        {
            using (var db = new CertificateDbContext())
            {
                var request = new tbl_CertificateCorrectionRequest
                {
                    IssueId = issueId,
                    UserId = userId,
                    RequestType = requestType,
                    Details = details,
                    Status = "Open",
                    CreatedAt = DateTime.Now
                };

                db.CertificateCorrectionRequests.Add(request);
                db.SaveChanges();

                _auditService.Log(userId, "CorrectionRequested", "Issue", issueId,
                    details: $"Correction request ({requestType}): {details}");

                return request.Id;
            }
        }

        public CertificateCorrectionVM GetById(int id)
        {
            using (var db = new CertificateDbContext())
            {
                var req = db.CertificateCorrectionRequests
                    .Include(r => r.Issue)
                    .FirstOrDefault(r => r.Id == id);
                if (req == null) return null;
                return MapToVM(req, db);
            }
        }

        public List<CertificateCorrectionVM> GetOpenRequests()
        {
            using (var db = new CertificateDbContext())
            {
                return db.CertificateCorrectionRequests
                    .Include(r => r.Issue)
                    .Where(r => r.Status == "Open")
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList()
                    .Select(r => MapToVM(r, db))
                    .ToList();
            }
        }

        public List<CertificateCorrectionVM> GetRequestsForIssue(int issueId)
        {
            using (var db = new CertificateDbContext())
            {
                return db.CertificateCorrectionRequests
                    .Include(r => r.Issue)
                    .Where(r => r.IssueId == issueId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList()
                    .Select(r => MapToVM(r, db))
                    .ToList();
            }
        }

        public List<CertificateCorrectionVM> GetRequestsForUser(string userId)
        {
            using (var db = new CertificateDbContext())
            {
                return db.CertificateCorrectionRequests
                    .Include(r => r.Issue)
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList()
                    .Select(r => MapToVM(r, db))
                    .ToList();
            }
        }

        public void ResolveRequest(int id, string resolvedBy, string notes, bool approved)
        {
            using (var db = new CertificateDbContext())
            {
                var req = db.CertificateCorrectionRequests.Find(id);
                if (req == null) return;

                req.Status = approved ? "Resolved" : "Rejected";
                req.ResolvedBy = resolvedBy;
                req.ResolutionNotes = notes;
                req.ResolvedAt = DateTime.Now;

                db.Entry(req).State = EntityState.Modified;
                db.SaveChanges();

                _auditService.Log(resolvedBy, approved ? "CorrectionResolved" : "CorrectionRejected",
                    "Issue", req.IssueId,
                    details: $"Correction #{id} {(approved ? "resolved" : "rejected")}. Notes: {notes}");
            }
        }

        private CertificateCorrectionVM MapToVM(tbl_CertificateCorrectionRequest req, CertificateDbContext db)
        {
            string resolvedByName = null;
            if (!string.IsNullOrEmpty(req.ResolvedBy))
            {
                resolvedByName = db.AspNetUsers
                    .Where(u => u.Id == req.ResolvedBy)
                    .Select(u => u.UserName)
                    .FirstOrDefault();
            }

            return new CertificateCorrectionVM
            {
                Id = req.Id,
                IssueId = req.IssueId,
                LearnerName = req.Issue?.LearnerName,
                CourseName = req.Issue?.CourseName,
                PublicId = req.Issue?.PublicId,
                RequestType = req.RequestType,
                Details = req.Details,
                Status = req.Status,
                ResolvedByName = resolvedByName,
                ResolutionNotes = req.ResolutionNotes,
                ResolvedAt = req.ResolvedAt,
                CreatedAt = req.CreatedAt
            };
        }
    }
}
