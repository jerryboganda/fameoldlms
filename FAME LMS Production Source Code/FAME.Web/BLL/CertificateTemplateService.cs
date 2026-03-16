using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models.Certificate;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    public class CertificateTemplateService : ICertificateTemplateService
    {
        private readonly ICertificateAuditService _auditService;
        private readonly ILogger _logger;

        public CertificateTemplateService(ICertificateAuditService auditService, ILogger logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        #region CRUD

        public tbl_CertificateTemplate GetById(int id)
        {
            using (var db = new CertificateDbContext())
            {
                return db.CertificateTemplates.Find(id);
            }
        }

        public CertificateTemplateVM GetTemplateForEdit(int id)
        {
            using (var db = new CertificateDbContext())
            {
                var t = db.CertificateTemplates.Find(id);
                if (t == null) return null;

                var creator = db.AspNetUsers.Find(t.CreatedBy);
                var assets = db.CertificateAssets
                    .Where(a => a.IsActive)
                    .OrderBy(a => a.AssetType).ThenBy(a => a.Name)
                    .Select(a => new CertificateAssetVM
                    {
                        Id = a.Id,
                        AssetType = a.AssetType,
                        Name = a.Name,
                        FilePath = a.FilePath,
                        SignatoryName = a.SignatoryName,
                        SignatoryTitle = a.SignatoryTitle,
                        IsActive = a.IsActive,
                        CreatedAt = a.CreatedAt
                    }).ToList();

                return new CertificateTemplateVM
                {
                    Id = t.Id,
                    Name = t.Name,
                    Type = t.Type,
                    Status = t.Status,
                    Version = t.Version,
                    LayoutJson = t.LayoutJson,
                    ThumbnailPath = t.ThumbnailPath,
                    Orientation = t.Orientation,
                    PageSize = t.PageSize,
                    Description = t.Description,
                    CreatedBy = t.CreatedBy,
                    CreatedByName = creator?.UserName ?? "Unknown",
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    AvailableAssets = assets
                };
            }
        }

        public List<CertificateTemplateListVM> GetAllTemplates(string statusFilter = null)
        {
            using (var db = new CertificateDbContext())
            {
                var query = db.CertificateTemplates.AsQueryable();
                if (!string.IsNullOrEmpty(statusFilter))
                    query = query.Where(t => t.Status == statusFilter);

                return query.OrderByDescending(t => t.UpdatedAt)
                    .Select(t => new CertificateTemplateListVM
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Type = t.Type,
                        Status = t.Status,
                        Version = t.Version,
                        ThumbnailPath = t.ThumbnailPath,
                        Orientation = t.Orientation,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    }).ToList();
            }
        }

        public int SaveTemplate(CertificateTemplateVM model, string userId)
        {
            using (var db = new CertificateDbContext())
            {
                tbl_CertificateTemplate entity;

                if (model.Id > 0)
                {
                    // Update existing
                    entity = db.CertificateTemplates.Find(model.Id);
                    if (entity == null) throw new InvalidOperationException("Template not found.");

                    var beforeJson = JsonConvert.SerializeObject(entity);
                    var beforeHash = _auditService.ComputeHash(beforeJson);

                    entity.Name = model.Name;
                    entity.Type = model.Type;
                    entity.LayoutJson = model.LayoutJson;
                    entity.Orientation = model.Orientation;
                    entity.PageSize = model.PageSize;
                    entity.Description = model.Description;
                    entity.UpdatedBy = userId;
                    entity.UpdatedAt = DateTime.Now;

                    db.Entry(entity).State = EntityState.Modified;
                    db.SaveChanges();

                    var afterHash = _auditService.ComputeHash(JsonConvert.SerializeObject(entity));
                    _auditService.Log(userId, "TemplateUpdated", "Template", entity.Id,
                        beforeHash, afterHash, $"Template '{entity.Name}' updated");
                }
                else
                {
                    // Create new
                    entity = new tbl_CertificateTemplate
                    {
                        Name = model.Name,
                        Type = model.Type,
                        Status = "Draft",
                        Version = 1,
                        LayoutJson = model.LayoutJson ?? "{}",
                        Orientation = model.Orientation ?? "Landscape",
                        PageSize = model.PageSize ?? "A4",
                        Description = model.Description,
                        CreatedBy = userId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    db.CertificateTemplates.Add(entity);
                    db.SaveChanges();

                    // Create initial version snapshot
                    var version = new tbl_CertificateTemplateVersion
                    {
                        TemplateId = entity.Id,
                        Version = 1,
                        LayoutJson = entity.LayoutJson,
                        ChangeNotes = "Initial version",
                        CreatedBy = userId,
                        CreatedAt = DateTime.Now
                    };
                    db.CertificateTemplateVersions.Add(version);
                    db.SaveChanges();

                    _auditService.Log(userId, "TemplateCreated", "Template", entity.Id,
                        details: $"Template '{entity.Name}' created");
                }

                return entity.Id;
            }
        }

        public void UpdateTemplateStatus(int id, string status, string userId)
        {
            using (var db = new CertificateDbContext())
            {
                var entity = db.CertificateTemplates.Find(id);
                if (entity == null) throw new InvalidOperationException("Template not found.");

                var oldStatus = entity.Status;
                entity.Status = status;
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.Now;

                db.Entry(entity).State = EntityState.Modified;
                db.SaveChanges();

                _auditService.Log(userId, "TemplateStatusChanged", "Template", id,
                    details: $"Status changed from '{oldStatus}' to '{status}'");
            }
        }

        public void DeleteTemplate(int id, string userId)
        {
            using (var db = new CertificateDbContext())
            {
                var entity = db.CertificateTemplates.Find(id);
                if (entity == null) return;

                // Soft delete → set to Archived
                entity.Status = "Archived";
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.Now;

                db.Entry(entity).State = EntityState.Modified;
                db.SaveChanges();

                _auditService.Log(userId, "TemplateArchived", "Template", id,
                    details: $"Template '{entity.Name}' archived");
            }
        }

        #endregion

        #region Versioning

        public tbl_CertificateTemplateVersion GetVersion(int templateId, int version)
        {
            using (var db = new CertificateDbContext())
            {
                return db.CertificateTemplateVersions
                    .FirstOrDefault(v => v.TemplateId == templateId && v.Version == version);
            }
        }

        public tbl_CertificateTemplateVersion GetLatestVersion(int templateId)
        {
            using (var db = new CertificateDbContext())
            {
                return db.CertificateTemplateVersions
                    .Where(v => v.TemplateId == templateId)
                    .OrderByDescending(v => v.Version)
                    .FirstOrDefault();
            }
        }

        public List<tbl_CertificateTemplateVersion> GetVersionHistory(int templateId)
        {
            using (var db = new CertificateDbContext())
            {
                return db.CertificateTemplateVersions
                    .Where(v => v.TemplateId == templateId)
                    .OrderByDescending(v => v.Version)
                    .ToList();
            }
        }

        public int CreateVersion(int templateId, string changeNotes, string userId)
        {
            using (var db = new CertificateDbContext())
            {
                var template = db.CertificateTemplates.Find(templateId);
                if (template == null) throw new InvalidOperationException("Template not found.");

                var newVersionNum = template.Version + 1;

                var version = new tbl_CertificateTemplateVersion
                {
                    TemplateId = templateId,
                    Version = newVersionNum,
                    LayoutJson = template.LayoutJson,
                    ChangeNotes = changeNotes,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now
                };
                db.CertificateTemplateVersions.Add(version);

                template.Version = newVersionNum;
                template.UpdatedBy = userId;
                template.UpdatedAt = DateTime.Now;
                db.Entry(template).State = EntityState.Modified;

                db.SaveChanges();

                _auditService.Log(userId, "VersionCreated", "Template", templateId,
                    details: $"Version {newVersionNum} created. Notes: {changeNotes}");

                return version.Id;
            }
        }

        #endregion

        #region Assets

        public List<CertificateAssetVM> GetAssets(string assetType = null)
        {
            using (var db = new CertificateDbContext())
            {
                var query = db.CertificateAssets.Where(a => a.IsActive);
                if (!string.IsNullOrEmpty(assetType))
                    query = query.Where(a => a.AssetType == assetType);

                return query.OrderBy(a => a.AssetType).ThenBy(a => a.Name)
                    .Select(a => new CertificateAssetVM
                    {
                        Id = a.Id,
                        AssetType = a.AssetType,
                        Name = a.Name,
                        FilePath = a.FilePath,
                        SignatoryName = a.SignatoryName,
                        SignatoryTitle = a.SignatoryTitle,
                        IsActive = a.IsActive,
                        CreatedAt = a.CreatedAt
                    }).ToList();
            }
        }

        public CertificateAssetVM GetAssetById(int id)
        {
            using (var db = new CertificateDbContext())
            {
                var a = db.CertificateAssets.Find(id);
                if (a == null) return null;
                return new CertificateAssetVM
                {
                    Id = a.Id,
                    AssetType = a.AssetType,
                    Name = a.Name,
                    FilePath = a.FilePath,
                    SignatoryName = a.SignatoryName,
                    SignatoryTitle = a.SignatoryTitle,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                };
            }
        }

        public int SaveAsset(string assetType, string name, string filePath, string signatoryName, string signatoryTitle, string userId)
        {
            using (var db = new CertificateDbContext())
            {
                var asset = new tbl_CertificateAsset
                {
                    AssetType = assetType,
                    Name = name,
                    FilePath = filePath,
                    SignatoryName = signatoryName,
                    SignatoryTitle = signatoryTitle,
                    UploadedBy = userId,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                db.CertificateAssets.Add(asset);
                db.SaveChanges();

                _auditService.Log(userId, "AssetUploaded", "Asset", asset.Id,
                    details: $"{assetType} asset '{name}' uploaded");

                return asset.Id;
            }
        }

        public void DeleteAsset(int id, string userId)
        {
            using (var db = new CertificateDbContext())
            {
                var asset = db.CertificateAssets.Find(id);
                if (asset == null) return;
                asset.IsActive = false;
                db.Entry(asset).State = EntityState.Modified;
                db.SaveChanges();

                _auditService.Log(userId, "AssetDeleted", "Asset", id,
                    details: $"{asset.AssetType} asset '{asset.Name}' deactivated");
            }
        }

        #endregion

        #region Dashboard

        public CertificateAdminDashboardVM GetAdminDashboard()
        {
            using (var db = new CertificateDbContext())
            {
                var vm = new CertificateAdminDashboardVM
                {
                    TotalTemplates = db.CertificateTemplates.Count(t => t.Status != "Archived"),
                    ActiveTemplates = db.CertificateTemplates.Count(t => t.Status == "Active"),
                    TotalIssued = db.CertificateIssues.Count(i => i.Status == "Issued"),
                    PendingApprovals = db.CertificateIssues.Count(i => i.Status == "PendingApproval"),
                    RevokedCount = db.CertificateIssues.Count(i => i.Status == "Revoked"),
                    RenderFailedCount = db.CertificateIssues.Count(i => i.Status == "RenderFailed"),
                    OpenCorrectionRequests = db.CertificateCorrectionRequests.Count(c => c.Status == "Open"),
                    TotalVerifications = db.CertificateAuditLogs.Count(a => a.Action == "Verified"),

                    RecentTemplates = db.CertificateTemplates
                        .Where(t => t.Status != "Archived")
                        .OrderByDescending(t => t.UpdatedAt)
                        .Take(5)
                        .Select(t => new CertificateTemplateListVM
                        {
                            Id = t.Id,
                            Name = t.Name,
                            Type = t.Type,
                            Status = t.Status,
                            Version = t.Version,
                            ThumbnailPath = t.ThumbnailPath,
                            Orientation = t.Orientation,
                            CreatedAt = t.CreatedAt,
                            UpdatedAt = t.UpdatedAt
                        }).ToList(),

                    RecentIssues = db.CertificateIssues
                        .OrderByDescending(i => i.CreatedAt)
                        .Take(10)
                        .Select(i => new CertificateIssueListVM
                        {
                            Id = i.Id,
                            PublicId = i.PublicId,
                            LearnerName = i.LearnerName,
                            CourseName = i.CourseName,
                            Status = i.Status,
                            IssuedAt = i.IssuedAt,
                            ThumbnailPath = i.ThumbnailPath
                        }).ToList(),

                    PendingCorrections = db.CertificateCorrectionRequests
                        .Where(c => c.Status == "Open")
                        .OrderByDescending(c => c.CreatedAt)
                        .Take(10)
                        .Select(c => new CertificateCorrectionVM
                        {
                            Id = c.Id,
                            IssueId = c.IssueId,
                            RequestType = c.RequestType,
                            Details = c.Details,
                            Status = c.Status,
                            CreatedAt = c.CreatedAt
                        }).ToList()
                };

                return vm;
            }
        }

        #endregion
    }
}
