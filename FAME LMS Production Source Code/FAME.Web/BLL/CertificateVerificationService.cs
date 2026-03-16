using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models.Certificate;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.BLL
{
    public class CertificateVerificationService : ICertificateVerificationService
    {
        private readonly ICertificateAuditService _auditService;
        private readonly ILogger _logger;

        // Simple in-memory rate limiter: IP → (count, windowStart)
        private static readonly ConcurrentDictionary<string, RateLimitEntry> _rateLimits
            = new ConcurrentDictionary<string, RateLimitEntry>();
        private const int MaxRequestsPerMinute = 30;

        public CertificateVerificationService(ICertificateAuditService auditService, ILogger logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        public CertificateVerifyVM Verify(string publicId, string ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                return new CertificateVerifyVM { Found = false };

            using (var db = new CertificateDbContext())
            {
                var issue = db.CertificateIssues
                    .Include(i => i.Template)
                    .FirstOrDefault(i => i.PublicId == publicId);

                if (issue == null)
                    return new CertificateVerifyVM { Found = false, PublicId = publicId };

                // Format display name based on privacy level
                string displayName;
                switch (issue.PrivacyLevel)
                {
                    case "InitialsOnly":
                        displayName = GetInitials(issue.LearnerName);
                        break;
                    case "FirstLastInitial":
                        displayName = GetFirstNameLastInitial(issue.LearnerName);
                        break;
                    default: // FullName
                        displayName = issue.LearnerName;
                        break;
                }

                // Check expiry
                var status = issue.Status;
                if (status == "Issued" && issue.ExpiresAt.HasValue && issue.ExpiresAt.Value < DateTime.Now)
                    status = "Expired";

                // Log the verification
                _auditService.Log(null, "Verified", "Issue", issue.Id,
                    ipAddress: ipAddress,
                    details: $"Certificate {publicId} verified. Status: {status}");

                return new CertificateVerifyVM
                {
                    Found = true,
                    PublicId = publicId,
                    DisplayName = displayName,
                    CourseName = issue.CourseName,
                    TemplateType = issue.Template?.Type,
                    Status = status,
                    IssuedAt = issue.IssuedAt,
                    ExpiresAt = issue.ExpiresAt,
                    RevokedAt = issue.RevokedAt,
                    CreditsAwarded = issue.CreditsAwarded,
                    IssuingOrganization = "First Aid Made Easy"
                };
            }
        }

        public bool CheckRateLimit(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress)) return true;

            var now = DateTime.UtcNow;
            var entry = _rateLimits.AddOrUpdate(
                ipAddress,
                _ => new RateLimitEntry { Count = 1, WindowStart = now },
                (_, existing) =>
                {
                    if ((now - existing.WindowStart).TotalMinutes >= 1)
                    {
                        // Reset window
                        return new RateLimitEntry { Count = 1, WindowStart = now };
                    }
                    existing.Count++;
                    return existing;
                });

            return entry.Count <= MaxRequestsPerMinute;
        }

        #region Helpers

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "***";
            var parts = name.Trim().Split(' ');
            return string.Join(".", parts.Select(p => p.Length > 0 ? p[0].ToString().ToUpper() : "")) + ".";
        }

        private string GetFirstNameLastInitial(string name)
        {
            if (string.IsNullOrEmpty(name)) return "***";
            var parts = name.Trim().Split(' ');
            if (parts.Length == 1) return parts[0];
            return parts[0] + " " + parts.Last()[0] + ".";
        }

        private class RateLimitEntry
        {
            public int Count { get; set; }
            public DateTime WindowStart { get; set; }
        }

        #endregion
    }
}
