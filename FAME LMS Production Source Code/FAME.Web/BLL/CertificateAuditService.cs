using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models.Certificate;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace First_Aid_Made_Easy.BLL
{
    public class CertificateAuditService : ICertificateAuditService
    {
        private readonly ILogger _logger;

        public CertificateAuditService(ILogger logger)
        {
            _logger = logger;
        }

        public void Log(string actorId, string action, string entityType, int entityId,
                        string beforeHash = null, string afterHash = null,
                        string details = null, string ipAddress = null)
        {
            try
            {
                using (var db = new CertificateDbContext())
                {
                    var entry = new tbl_CertificateAuditLog
                    {
                        ActorId = actorId,
                        Action = action,
                        EntityType = entityType,
                        EntityId = entityId,
                        BeforeHash = beforeHash,
                        AfterHash = afterHash,
                        Details = details,
                        IpAddress = ipAddress,
                        CreatedAt = DateTime.Now
                    };
                    db.CertificateAuditLogs.Add(entry);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Audit logging should never break the main flow
                _logger.Error(ex, "Failed to write certificate audit log: {Action} on {EntityType} {EntityId}",
                    action, entityType, entityId);
            }
        }

        public List<CertificateAuditLogVM> GetAuditTrail(string entityType, int entityId)
        {
            using (var db = new CertificateDbContext())
            {
                var logs = db.CertificateAuditLogs
                    .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToList();

                return logs.Select(a =>
                {
                    string actorName = null;
                    if (!string.IsNullOrEmpty(a.ActorId) && a.ActorId != "SYSTEM")
                    {
                        actorName = db.AspNetUsers
                            .Where(u => u.Id == a.ActorId)
                            .Select(u => u.UserName)
                            .FirstOrDefault() ?? a.ActorId;
                    }
                    else
                    {
                        actorName = a.ActorId ?? "System";
                    }

                    return new CertificateAuditLogVM
                    {
                        Id = a.Id,
                        ActorName = actorName,
                        Action = a.Action,
                        EntityType = a.EntityType,
                        EntityId = a.EntityId,
                        Details = a.Details,
                        CreatedAt = a.CreatedAt
                    };
                }).ToList();
            }
        }

        public List<CertificateAuditLogVM> GetRecentLogs(int count = 50)
        {
            using (var db = new CertificateDbContext())
            {
                return db.CertificateAuditLogs
                    .OrderByDescending(a => a.CreatedAt)
                    .Take(count)
                    .Select(a => new CertificateAuditLogVM
                    {
                        Id = a.Id,
                        ActorName = a.ActorId,
                        Action = a.Action,
                        EntityType = a.EntityType,
                        EntityId = a.EntityId,
                        Details = a.Details,
                        CreatedAt = a.CreatedAt
                    }).ToList();
            }
        }

        public int GetVerificationCount()
        {
            using (var db = new CertificateDbContext())
            {
                return db.CertificateAuditLogs.Count(a => a.Action == "Verified");
            }
        }

        public string ComputeHash(string content)
        {
            if (string.IsNullOrEmpty(content)) return null;
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}
