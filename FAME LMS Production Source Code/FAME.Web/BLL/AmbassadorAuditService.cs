using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    /// <summary>
    /// Service for logging ambassador-related audit events
    /// </summary>
    public class AmbassadorAuditService : IAmbassadorAuditService
    {
        private readonly AmbassadorDbContext _context;
        private readonly ILogger _logger;

        public AmbassadorAuditService()
        {
            _context = new AmbassadorDbContext();
            _logger = Serilog.Log.ForContext<AmbassadorAuditService>();
        }

        public void LogAction(int? ambassadorId, string action, string details, string performedBy, string ipAddress = null)
        {
            try
            {
                var audit = new tbl_AmbassadorAuditLog
                {
                    AmbassadorId = ambassadorId,
                    Action = action,
                    EntityType = "Ambassador",
                    NewValue = details,
                    PerformedBy = performedBy,
                    PerformedAt = DateTime.UtcNow,
                    IPHash = ipAddress
                };

                _context.tbl_AmbassadorAuditLog.Add(audit);
                _context.SaveChanges();

                _logger.Debug("Audit logged: {Action} for ambassador {AmbId}", action, ambassadorId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error logging audit action: {Action}", action);
            }
        }

        public void Log(int? ambassadorId, string action, string details, string performedBy, string ipAddress = null)
        {
            LogAction(ambassadorId, action, details, performedBy, ipAddress);
        }

        public void Log(int? ambassadorId, string action, string entityType, string entityId, string oldValue, string newValue, string performedBy)
        {
            try
            {
                var audit = new tbl_AmbassadorAuditLog
                {
                    AmbassadorId = ambassadorId,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    OldValue = oldValue,
                    NewValue = newValue,
                    PerformedBy = performedBy,
                    PerformedAt = DateTime.UtcNow
                };

                _context.tbl_AmbassadorAuditLog.Add(audit);
                _context.SaveChanges();

                _logger.Debug("Audit logged: {Action} for entity {EntityType}:{EntityId}", action, entityType, entityId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error logging audit action: {Action}", action);
            }
        }

        public void LogApplicationSubmitted(int ambassadorId, string userId, string ipAddress = null)
        {
            LogAction(ambassadorId, "APPLICATION_SUBMITTED", "Ambassador application submitted", userId, ipAddress);
        }

        public void LogApplicationApproved(int ambassadorId, string adminUserId, string ipAddress = null)
        {
            LogAction(ambassadorId, "APPLICATION_APPROVED", "Ambassador application approved", adminUserId, ipAddress);
        }

        public void LogApplicationRejected(int ambassadorId, string adminUserId, string reason, string ipAddress = null)
        {
            LogAction(ambassadorId, "APPLICATION_REJECTED", $"Ambassador application rejected. Reason: {reason}", adminUserId, ipAddress);
        }

        public void LogStatusChange(int ambassadorId, string oldStatus, string newStatus, string performedBy, string ipAddress = null)
        {
            LogAction(ambassadorId, "STATUS_CHANGED", $"Status changed from {oldStatus} to {newStatus}", performedBy, ipAddress);
        }

        public void LogTierChange(int ambassadorId, string oldTier, string newTier, string performedBy, string ipAddress = null)
        {
            LogAction(ambassadorId, "TIER_CHANGED", $"Tier changed from {oldTier} to {newTier}", performedBy, ipAddress);
        }

        public void LogPayoutRequested(int ambassadorId, decimal amount, int requestId, string ipAddress = null)
        {
            LogAction(ambassadorId, "PAYOUT_REQUESTED", $"Payout request #{requestId} for ETB {amount:N0}", null, ipAddress);
        }

        public void LogPayoutProcessed(int ambassadorId, decimal amount, int requestId, string adminUserId, string ipAddress = null)
        {
            LogAction(ambassadorId, "PAYOUT_PROCESSED", $"Payout request #{requestId} (ETB {amount:N0}) marked as paid", adminUserId, ipAddress);
        }

        public void LogPayoutRejected(int ambassadorId, decimal amount, int requestId, string reason, string adminUserId, string ipAddress = null)
        {
            LogAction(ambassadorId, "PAYOUT_REJECTED", $"Payout request #{requestId} (ETB {amount:N0}) rejected. Reason: {reason}", adminUserId, ipAddress);
        }

        public void LogCommissionAwarded(int ambassadorId, decimal amount, string earningType, int? referralId = null)
        {
            var refText = referralId.HasValue ? $" (referral #{referralId})" : "";
            LogAction(ambassadorId, "COMMISSION_AWARDED", $"Commission of ETB {amount:N0} awarded for {earningType}{refText}", "SYSTEM");
        }

        public void LogReferralCreated(int ambassadorId, int referralId, string referredEmail)
        {
            LogAction(ambassadorId, "REFERRAL_CREATED", $"New referral #{referralId} - {referredEmail}", "SYSTEM");
        }

        public void LogConversionRecorded(int ambassadorId, int conversionId, string conversionType, decimal amount)
        {
            LogAction(ambassadorId, "CONVERSION_RECORDED", $"Conversion #{conversionId} - {conversionType} (ETB {amount:N0})", "SYSTEM");
        }

        public void LogLogin(int ambassadorId, string ipAddress)
        {
            LogAction(ambassadorId, "LOGIN", "Ambassador portal login", null, ipAddress);
        }

        public void LogPayoutMethodAdded(int ambassadorId, string methodType, string ipAddress = null)
        {
            LogAction(ambassadorId, "PAYOUT_METHOD_ADDED", $"New payout method added: {methodType}", null, ipAddress);
        }

        public void LogPayoutMethodRemoved(int ambassadorId, int methodId, string ipAddress = null)
        {
            LogAction(ambassadorId, "PAYOUT_METHOD_REMOVED", $"Payout method #{methodId} removed", null, ipAddress);
        }

        public List<AuditLogEntry> GetAuditLog(int? ambassadorId = null, string action = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 50)
        {
            var query = _context.tbl_AmbassadorAuditLog.AsQueryable();

            if (ambassadorId.HasValue)
                query = query.Where(a => a.AmbassadorId == ambassadorId);
            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);
            if (from.HasValue)
                query = query.Where(a => a.PerformedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(a => a.PerformedAt <= to.Value);

            return query
                .OrderByDescending(a => a.PerformedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .Select(a => new AuditLogEntry
                {
                    Id = a.Id,
                    AmbassadorId = a.AmbassadorId,
                    Action = a.Action,
                    Details = a.Details,
                    PerformedBy = a.PerformedBy,
                    PerformedAt = a.PerformedAt,
                    IpAddress = a.IpAddress
                })
                .ToList();
        }

        public List<RecentActivityVM> GetRecentActivity(int count = 10)
        {
            var activities = _context.tbl_AmbassadorAuditLog
                .OrderByDescending(a => a.PerformedAt)
                .Take(count)
                .ToList();

            return activities.Select(a => new RecentActivityVM
            {
                Description = FormatActivityDescription(a),
                Timestamp = a.PerformedAt,
                Icon = GetActivityIcon(a.Action),
                IconBgClass = GetActivityIconBgClass(a.Action)
            }).ToList();
        }

        private string FormatActivityDescription(tbl_AmbassadorAuditLog log)
        {
            switch (log.Action)
            {
                case "APPLICATION_SUBMITTED":
                    return "New ambassador application submitted";
                case "APPLICATION_APPROVED":
                    return "Ambassador application approved";
                case "APPLICATION_REJECTED":
                    return "Ambassador application rejected";
                case "PAYOUT_REQUESTED":
                    return log.Details;
                case "PAYOUT_PROCESSED":
                    return log.Details;
                case "COMMISSION_AWARDED":
                    return log.Details;
                case "REFERRAL_CREATED":
                    return log.Details;
                case "CONVERSION_RECORDED":
                    return log.Details;
                default:
                    return log.Details ?? log.Action;
            }
        }

        private string GetActivityIcon(string action)
        {
            switch (action)
            {
                case "APPLICATION_SUBMITTED": return "assignment";
                case "APPLICATION_APPROVED": return "check_circle";
                case "APPLICATION_REJECTED": return "cancel";
                case "PAYOUT_REQUESTED": return "payments";
                case "PAYOUT_PROCESSED": return "paid";
                case "PAYOUT_REJECTED": return "money_off";
                case "COMMISSION_AWARDED": return "monetization_on";
                case "REFERRAL_CREATED": return "person_add";
                case "CONVERSION_RECORDED": return "trending_up";
                case "STATUS_CHANGED": return "swap_horiz";
                case "TIER_CHANGED": return "upgrade";
                default: return "info";
            }
        }

        private string GetActivityIconBgClass(string action)
        {
            switch (action)
            {
                case "APPLICATION_APPROVED": return "bg-success";
                case "APPLICATION_REJECTED": return "bg-danger";
                case "PAYOUT_PROCESSED": return "bg-success";
                case "PAYOUT_REJECTED": return "bg-danger";
                case "COMMISSION_AWARDED": return "bg-info";
                case "REFERRAL_CREATED": return "bg-primary";
                case "CONVERSION_RECORDED": return "bg-success";
                default: return "bg-secondary";
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }

    public class AuditLogEntry
    {
        public long Id { get; set; }
        public int? AmbassadorId { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public string PerformedBy { get; set; }
        public DateTime PerformedAt { get; set; }
        public string IpAddress { get; set; }
    }

    public class RecentActivityVM
    {
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string Icon { get; set; }
        public string IconBgClass { get; set; }
    }
}
