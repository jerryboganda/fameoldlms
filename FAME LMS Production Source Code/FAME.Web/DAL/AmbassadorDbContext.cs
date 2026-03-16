using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace First_Aid_Made_Easy.DAL
{
    // ─────────────────────────────────────────────────────────────────
    // Lightweight POCO classes that map to ASP.NET Identity tables.
    // These replace the EDMX-generated AspNetUsers / AspNetRoles so
    // that EF Code-First does NOT discover the 25+ EDMX navigation
    // properties (tbl_Package, tbl_Courses, etc.) that would crash
    // the model with "has no key defined" errors.
    // ─────────────────────────────────────────────────────────────────

    [Table("AspNetUsers")]
    public class AmbassadorUser
    {
        public AmbassadorUser()
        {
            AspNetRoles = new HashSet<AmbassadorRole>();
        }

        [Key]
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Many-to-many roles via AspNetUserRoles junction table.
        /// Named "AspNetRoles" to match existing BLL code (user.AspNetRoles).
        /// </summary>
        public virtual ICollection<AmbassadorRole> AspNetRoles { get; set; }
    }

    [Table("AspNetRoles")]
    public class AmbassadorRole
    {
        public AmbassadorRole()
        {
            AspNetUsers = new HashSet<AmbassadorUser>();
        }

        [Key]
        public string Id { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Back-reference for the many-to-many relationship.
        /// </summary>
        public virtual ICollection<AmbassadorUser> AspNetUsers { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Separate Code-First DbContext for the Ambassador module.
    /// Uses lightweight AmbassadorUser / AmbassadorRole POCOs to avoid
    /// pulling in any EDMX-generated entity types.
    /// </summary>
    public class AmbassadorDbContext : DbContext
    {
        public AmbassadorDbContext()
            : base("name=DefaultConnection")
        {
            Database.SetInitializer<AmbassadorDbContext>(null);
        }

        // Ambassador entities
        public virtual DbSet<tbl_Ambassador> tbl_Ambassador { get; set; }
        public virtual DbSet<tbl_Referral> tbl_Referral { get; set; }
        public virtual DbSet<tbl_ReferralClick> tbl_ReferralClick { get; set; }
        public virtual DbSet<tbl_ReferralConversion> tbl_ReferralConversion { get; set; }
        public virtual DbSet<tbl_CommissionRule> tbl_CommissionRule { get; set; }
        public virtual DbSet<tbl_AmbassadorEarning> tbl_AmbassadorEarning { get; set; }
        public virtual DbSet<tbl_PayoutMethod> tbl_PayoutMethod { get; set; }
        public virtual DbSet<tbl_PayoutRequest> tbl_PayoutRequest { get; set; }
        public virtual DbSet<tbl_AmbassadorAuditLog> tbl_AmbassadorAuditLog { get; set; }

        // Identity tables – lightweight POCOs, NOT EDMX classes
        public virtual DbSet<AmbassadorUser> AspNetUsers { get; set; }
        public virtual DbSet<AmbassadorRole> AspNetRoles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ── Ambassador entities ──────────────────────────────────
            modelBuilder.Entity<tbl_Ambassador>().ToTable("tbl_Ambassador").HasKey(a => a.Id);
            modelBuilder.Entity<tbl_Referral>().ToTable("tbl_Referral").HasKey(r => r.Id);
            modelBuilder.Entity<tbl_ReferralClick>().ToTable("tbl_ReferralClick").HasKey(r => r.Id);
            modelBuilder.Entity<tbl_ReferralConversion>().ToTable("tbl_ReferralConversion").HasKey(r => r.Id);
            modelBuilder.Entity<tbl_CommissionRule>().ToTable("tbl_CommissionRule").HasKey(r => r.Id);
            modelBuilder.Entity<tbl_AmbassadorEarning>().ToTable("tbl_AmbassadorEarning").HasKey(e => e.Id);
            modelBuilder.Entity<tbl_PayoutMethod>().ToTable("tbl_PayoutMethod").HasKey(p => p.Id);
            modelBuilder.Entity<tbl_PayoutRequest>().ToTable("tbl_PayoutRequest").HasKey(p => p.Id);
            modelBuilder.Entity<tbl_AmbassadorAuditLog>().ToTable("tbl_AmbassadorAuditLog").HasKey(a => a.Id);

            // ── Identity POCO classes ────────────────────────────────
            modelBuilder.Entity<AmbassadorUser>().ToTable("AspNetUsers").HasKey(u => u.Id);
            modelBuilder.Entity<AmbassadorRole>().ToTable("AspNetRoles").HasKey(r => r.Id);

            // Many-to-many: Users ↔ Roles via AspNetUserRoles
            modelBuilder.Entity<AmbassadorUser>()
                .HasMany(u => u.AspNetRoles)
                .WithMany(r => r.AspNetUsers)
                .Map(m =>
                {
                    m.ToTable("AspNetUserRoles");
                    m.MapLeftKey("UserId");
                    m.MapRightKey("RoleId");
                });

            // ── Sever references to EDMX entity types ────────────────
            // tbl_ReferralConversion.tbl_EnrollmentMaster → EDMX type (no key)
            modelBuilder.Entity<tbl_ReferralConversion>()
                .Ignore(e => e.tbl_EnrollmentMaster);

            // tbl_CommissionRule.tbl_Package → EDMX type (no key)
            modelBuilder.Entity<tbl_CommissionRule>()
                .Ignore(e => e.tbl_Package);

            base.OnModelCreating(modelBuilder);
        }
    }
}
