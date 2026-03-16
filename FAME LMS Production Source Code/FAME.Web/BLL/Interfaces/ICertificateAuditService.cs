using First_Aid_Made_Easy.Models.Certificate;
using System.Collections.Generic;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface ICertificateAuditService
    {
        /// <summary>
        /// Log an audit event.
        /// </summary>
        void Log(string actorId, string action, string entityType, int entityId,
                 string beforeHash = null, string afterHash = null,
                 string details = null, string ipAddress = null);

        /// <summary>
        /// Get audit trail for a specific entity.
        /// </summary>
        List<CertificateAuditLogVM> GetAuditTrail(string entityType, int entityId);

        /// <summary>
        /// Get recent audit logs for the admin dashboard.
        /// </summary>
        List<CertificateAuditLogVM> GetRecentLogs(int count = 50);

        /// <summary>
        /// Get verification count (total QR/link scans).
        /// </summary>
        int GetVerificationCount();

        /// <summary>
        /// Compute SHA256 hash of a string (for before/after audit comparison).
        /// </summary>
        string ComputeHash(string content);
    }
}
