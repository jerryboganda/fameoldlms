using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using First_Aid_Made_Easy.Models.Certificate;

namespace First_Aid_Made_Easy.DAL
{
    // ─────────────────────────────────────────────────────────────────
    // Lightweight POCO for cross-referencing AspNetUsers from the
    // Certificate context, without pulling in EDMX-generated types.
    // ─────────────────────────────────────────────────────────────────

    [Table("AspNetUsers")]
    public class CertificateUser
    {
        [Key]
        public string Id { get; set; }

        public string Email { get; set; }

        public string UserName { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Separate Code-First DbContext for the Certificate module.
    /// Uses lightweight CertificateUser POCO to avoid collisions with
    /// the EDMX-generated AspNetUsers entity.
    /// Tables are created via SQL script – NO migrations.
    /// </summary>
    public class CertificateDbContext : DbContext
    {
        public CertificateDbContext()
            : base("name=DefaultConnection")
        {
            Database.SetInitializer<CertificateDbContext>(null);
        }

        // Certificate entities
        public virtual DbSet<tbl_CertificateAsset> CertificateAssets { get; set; }
        public virtual DbSet<tbl_CertificateTemplate> CertificateTemplates { get; set; }
        public virtual DbSet<tbl_CertificateTemplateVersion> CertificateTemplateVersions { get; set; }
        public virtual DbSet<tbl_CertificateRuleSet> CertificateRuleSets { get; set; }
        public virtual DbSet<tbl_CertificateIssue> CertificateIssues { get; set; }
        public virtual DbSet<tbl_CertificateAuditLog> CertificateAuditLogs { get; set; }
        public virtual DbSet<tbl_CertificateCorrectionRequest> CertificateCorrectionRequests { get; set; }
        public virtual DbSet<tbl_ManualCertificate> ManualCertificates { get; set; }
        public virtual DbSet<tbl_GoogleSheet> GoogleSheets { get; set; }

        // Identity table – lightweight POCO, NOT EDMX class
        public virtual DbSet<CertificateUser> AspNetUsers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ── Certificate entities ─────────────────────────────────
            modelBuilder.Entity<tbl_CertificateAsset>()
                .ToTable("tbl_CertificateAsset").HasKey(e => e.Id);

            modelBuilder.Entity<tbl_CertificateTemplate>()
                .ToTable("tbl_CertificateTemplate").HasKey(e => e.Id);

            modelBuilder.Entity<tbl_CertificateTemplateVersion>()
                .ToTable("tbl_CertificateTemplateVersion").HasKey(e => e.Id);

            modelBuilder.Entity<tbl_CertificateRuleSet>()
                .ToTable("tbl_CertificateRuleSet").HasKey(e => e.Id);

            modelBuilder.Entity<tbl_CertificateIssue>()
                .ToTable("tbl_CertificateIssue").HasKey(e => e.Id);

            modelBuilder.Entity<tbl_CertificateAuditLog>()
                .ToTable("tbl_CertificateAuditLog").HasKey(e => e.Id);

            modelBuilder.Entity<tbl_CertificateCorrectionRequest>()
                .ToTable("tbl_CertificateCorrectionRequest").HasKey(e => e.Id);

            modelBuilder.Entity<tbl_ManualCertificate>()
                .ToTable("tbl_ManualCertificate").HasKey(e => e.Id);

            modelBuilder.Entity<tbl_GoogleSheet>()
                .ToTable("tbl_GoogleSheet").HasKey(e => e.Id);

            // ── Identity POCO ────────────────────────────────────────
            modelBuilder.Entity<CertificateUser>()
                .ToTable("AspNetUsers").HasKey(u => u.Id);

            // ── Sever FK navigation to EDMX types ────────────────────
            // The FK columns (CourseId, PackageId) remain as plain ints;
            // we do NOT define navigation properties to tbl_Courses or
            // tbl_Package because those are EDMX-managed entities.

            base.OnModelCreating(modelBuilder);
        }
    }
}
