using System;
using System.Collections.Generic;
using First_Aid_Made_Easy.Models;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    /// <summary>
    /// Service interface for audit logging in the Ambassador Portal
    /// </summary>
    public interface IAmbassadorAuditService : IDisposable
    {
        #region Logging

        /// <summary>
        /// Log a generic ambassador action
        /// </summary>
        void LogAction(int? ambassadorId, string action, string details, string performedBy, string ipAddress = null);

        /// <summary>
        /// Log method (alias for LogAction for compatibility)
        /// </summary>
        void Log(int? ambassadorId, string action, string details, string performedBy, string ipAddress = null);

        /// <summary>
        /// Log method with entity change tracking
        /// </summary>
        void Log(int? ambassadorId, string action, string entityType, string entityId, string oldValue, string newValue, string performedBy);

        /// <summary>
        /// Log application submitted
        /// </summary>
        void LogApplicationSubmitted(int ambassadorId, string userId, string ipAddress = null);

        /// <summary>
        /// Log application approved
        /// </summary>
        void LogApplicationApproved(int ambassadorId, string adminUserId, string ipAddress = null);

        /// <summary>
        /// Log application rejected
        /// </summary>
        void LogApplicationRejected(int ambassadorId, string adminUserId, string reason, string ipAddress = null);

        /// <summary>
        /// Log status change
        /// </summary>
        void LogStatusChange(int ambassadorId, string oldStatus, string newStatus, string performedBy, string ipAddress = null);

        /// <summary>
        /// Log tier change
        /// </summary>
        void LogTierChange(int ambassadorId, string oldTier, string newTier, string performedBy, string ipAddress = null);

        /// <summary>
        /// Log payout request
        /// </summary>
        void LogPayoutRequested(int ambassadorId, decimal amount, int requestId, string ipAddress = null);

        /// <summary>
        /// Log payout processed
        /// </summary>
        void LogPayoutProcessed(int ambassadorId, decimal amount, int requestId, string adminUserId, string ipAddress = null);

        /// <summary>
        /// Log payout rejected
        /// </summary>
        void LogPayoutRejected(int ambassadorId, decimal amount, int requestId, string reason, string adminUserId, string ipAddress = null);

        /// <summary>
        /// Log commission awarded
        /// </summary>
        void LogCommissionAwarded(int ambassadorId, decimal amount, string earningType, int? referralId = null);

        /// <summary>
        /// Log referral created
        /// </summary>
        void LogReferralCreated(int ambassadorId, int referralId, string referredEmail);

        /// <summary>
        /// Log conversion recorded
        /// </summary>
        void LogConversionRecorded(int ambassadorId, int conversionId, string conversionType, decimal amount);

        /// <summary>
        /// Log login
        /// </summary>
        void LogLogin(int ambassadorId, string ipAddress);

        /// <summary>
        /// Log payout method added
        /// </summary>
        void LogPayoutMethodAdded(int ambassadorId, string methodType, string ipAddress = null);

        /// <summary>
        /// Log payout method removed
        /// </summary>
        void LogPayoutMethodRemoved(int ambassadorId, int methodId, string ipAddress = null);

        #endregion

        #region Retrieval

        /// <summary>
        /// Get audit log with optional filters
        /// </summary>
        List<First_Aid_Made_Easy.BLL.AuditLogEntry> GetAuditLog(int? ambassadorId = null, string action = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 50);

        /// <summary>
        /// Get recent activity
        /// </summary>
        List<First_Aid_Made_Easy.BLL.RecentActivityVM> GetRecentActivity(int count = 10);

        #endregion
    }
}
