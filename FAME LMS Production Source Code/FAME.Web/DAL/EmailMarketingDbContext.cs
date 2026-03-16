using System.Data.Entity;

namespace First_Aid_Made_Easy.DAL
{
    /// <summary>
    /// Separate Code-First DbContext for the Email Marketing module.
    /// Uses the same "DefaultConnection" as AmbassadorDbContext.
    /// Follows the same pattern: lightweight POCOs, no EDMX dependencies.
    /// </summary>
    public class EmailMarketingDbContext : DbContext
    {
        public EmailMarketingDbContext()
            : base("name=DefaultConnection")
        {
            Database.SetInitializer<EmailMarketingDbContext>(null);
        }

        // Email Marketing entities
        public virtual DbSet<tbl_EmailSenderAccounts> tbl_EmailSenderAccounts { get; set; }
        public virtual DbSet<tbl_EmailTemplates> tbl_EmailTemplates { get; set; }
        public virtual DbSet<tbl_EmailCampaigns> tbl_EmailCampaigns { get; set; }
        public virtual DbSet<tbl_EmailAudiences> tbl_EmailAudiences { get; set; }
        public virtual DbSet<tbl_EmailCampaignRecipients> tbl_EmailCampaignRecipients { get; set; }
        public virtual DbSet<tbl_EmailCampaignLinks> tbl_EmailCampaignLinks { get; set; }
        public virtual DbSet<tbl_EmailLinkClicks> tbl_EmailLinkClicks { get; set; }
        public virtual DbSet<tbl_EmailUnsubscribes> tbl_EmailUnsubscribes { get; set; }
        public virtual DbSet<tbl_EmailDripCampaigns> tbl_EmailDripCampaigns { get; set; }
        public virtual DbSet<tbl_EmailDripSteps> tbl_EmailDripSteps { get; set; }
        public virtual DbSet<tbl_EmailDripQueue> tbl_EmailDripQueue { get; set; }

        // Lightweight Identity POCO references for audience queries
        public virtual DbSet<EmailMarketingUser> Users { get; set; }
        public virtual DbSet<EmailMarketingUserProfile> UserProfiles { get; set; }

        // Lightweight POCOs for package/course/enrollment queries
        public virtual DbSet<EmailMarketingPackage> Packages { get; set; }
        public virtual DbSet<EmailMarketingCourse> Courses { get; set; }
        public virtual DbSet<EmailMarketingEnrollmentMaster> EnrollmentMasters { get; set; }
        public virtual DbSet<EmailMarketingEnrollmentDetail> EnrollmentDetails { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ── Email Marketing entities ─────────────────────────────
            modelBuilder.Entity<tbl_EmailSenderAccounts>().ToTable("tbl_EmailSenderAccounts").HasKey(e => e.Id);
            modelBuilder.Entity<tbl_EmailTemplates>().ToTable("tbl_EmailTemplates").HasKey(e => e.Id);
            modelBuilder.Entity<tbl_EmailCampaigns>().ToTable("tbl_EmailCampaigns").HasKey(e => e.Id);
            modelBuilder.Entity<tbl_EmailAudiences>().ToTable("tbl_EmailAudiences").HasKey(e => e.Id);
            modelBuilder.Entity<tbl_EmailCampaignRecipients>().ToTable("tbl_EmailCampaignRecipients").HasKey(e => e.Id);
            modelBuilder.Entity<tbl_EmailCampaignLinks>().ToTable("tbl_EmailCampaignLinks").HasKey(e => e.Id);
            modelBuilder.Entity<tbl_EmailLinkClicks>().ToTable("tbl_EmailLinkClicks").HasKey(e => e.Id);
            modelBuilder.Entity<tbl_EmailUnsubscribes>().ToTable("tbl_EmailUnsubscribes").HasKey(e => e.Id);
            modelBuilder.Entity<tbl_EmailDripCampaigns>().ToTable("tbl_EmailDripCampaigns").HasKey(e => e.Id);
            modelBuilder.Entity<tbl_EmailDripSteps>().ToTable("tbl_EmailDripSteps").HasKey(e => e.Id);
            modelBuilder.Entity<tbl_EmailDripQueue>().ToTable("tbl_EmailDripQueue").HasKey(e => e.Id);

            // ── Relationships ────────────────────────────────────────
            modelBuilder.Entity<tbl_EmailCampaigns>()
                .HasRequired(c => c.SenderAccount)
                .WithMany()
                .HasForeignKey(c => c.FromAccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_EmailCampaigns>()
                .HasOptional(c => c.Template)
                .WithMany()
                .HasForeignKey(c => c.TemplateId);

            modelBuilder.Entity<tbl_EmailCampaignRecipients>()
                .HasRequired(r => r.Campaign)
                .WithMany()
                .HasForeignKey(r => r.CampaignId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<tbl_EmailCampaignLinks>()
                .HasRequired(l => l.Campaign)
                .WithMany()
                .HasForeignKey(l => l.CampaignId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<tbl_EmailLinkClicks>()
                .HasRequired(c => c.Link)
                .WithMany()
                .HasForeignKey(c => c.LinkId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<tbl_EmailDripCampaigns>()
                .HasOptional(d => d.SenderAccount)
                .WithMany()
                .HasForeignKey(d => d.FromAccountId);

            modelBuilder.Entity<tbl_EmailDripSteps>()
                .HasRequired(s => s.DripCampaign)
                .WithMany()
                .HasForeignKey(s => s.DripCampaignId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<tbl_EmailDripSteps>()
                .HasOptional(s => s.Template)
                .WithMany()
                .HasForeignKey(s => s.TemplateId);

            modelBuilder.Entity<tbl_EmailDripQueue>()
                .HasRequired(q => q.DripCampaign)
                .WithMany()
                .HasForeignKey(q => q.DripCampaignId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbl_EmailDripQueue>()
                .HasRequired(q => q.Step)
                .WithMany()
                .HasForeignKey(q => q.StepId)
                .WillCascadeOnDelete(false);

            // ── Lightweight Identity POCOs ────────────────────────────
            modelBuilder.Entity<EmailMarketingUser>().ToTable("AspNetUsers").HasKey(u => u.Id);
            modelBuilder.Entity<EmailMarketingUserProfile>().ToTable("tbl_User").HasKey(u => u.User_Id);

            // ── Lightweight Package/Course/Enrollment POCOs ──────────
            modelBuilder.Entity<EmailMarketingPackage>().ToTable("tbl_Package").HasKey(p => p.PackageID);
            modelBuilder.Entity<EmailMarketingCourse>().ToTable("tbl_Courses").HasKey(c => c.Course_Id);
            modelBuilder.Entity<EmailMarketingEnrollmentMaster>().ToTable("tbl_EnrollmentMaster").HasKey(e => e.Enrollment_Id);
            modelBuilder.Entity<EmailMarketingEnrollmentDetail>().ToTable("tbl_EnrollmentDetail").HasKey(e => e.ID);

            base.OnModelCreating(modelBuilder);
        }
    }

    // ─────────────────────────────────────────────────────────────────
    // Lightweight POCO for reading user data in audience queries
    // ─────────────────────────────────────────────────────────────────

    [System.ComponentModel.DataAnnotations.Schema.Table("AspNetUsers")]
    public class EmailMarketingUser
    {
        [System.ComponentModel.DataAnnotations.Key]
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool? IsActive { get; set; }
    }

    [System.ComponentModel.DataAnnotations.Schema.Table("tbl_User")]
    public class EmailMarketingUserProfile
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int User_Id { get; set; }
        public string User_Name { get; set; }
        public string User_FatherName { get; set; }
        public string FatherEmail { get; set; }
        public string User_AspUser { get; set; }   // FK to AspNetUsers.Id
        public string User_Mobile { get; set; }
        public string Institute { get; set; }
        public System.Nullable<int> Type { get; set; }  // University ID
        public string CNIC { get; set; }
        public string ExamType { get; set; }
        public System.Nullable<int> CountryID { get; set; }
        public string City { get; set; }
        public System.Nullable<int> CategoryID { get; set; }
        public System.Nullable<System.DateTime> CreateDT { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────
    // Lightweight POCOs for Package/Course/Enrollment queries
    // ─────────────────────────────────────────────────────────────────

    [System.ComponentModel.DataAnnotations.Schema.Table("tbl_Package")]
    public class EmailMarketingPackage
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int PackageID { get; set; }
        public string PackageName { get; set; }
        public bool? IsActive { get; set; }
        public int? SortID { get; set; }
    }

    [System.ComponentModel.DataAnnotations.Schema.Table("tbl_Courses")]
    public class EmailMarketingCourse
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Course_Id { get; set; }
        public string Course_Name { get; set; }
        public bool? IsActive { get; set; }
        public int? SortID { get; set; }
    }

    [System.ComponentModel.DataAnnotations.Schema.Table("tbl_EnrollmentMaster")]
    public class EmailMarketingEnrollmentMaster
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int Enrollment_Id { get; set; }
        public string StudentFid { get; set; }       // FK → AspNetUsers.Id
        public int? PackageId { get; set; }           // FK → tbl_Package.PackageID
        public bool? IsExpired { get; set; }
        public System.DateTime? Enrollment_EndDate { get; set; }
        public string Enrollment_Status { get; set; }
    }

    [System.ComponentModel.DataAnnotations.Schema.Table("tbl_EnrollmentDetail")]
    public class EmailMarketingEnrollmentDetail
    {
        [System.ComponentModel.DataAnnotations.Key]
        public int ID { get; set; }
        public int? EnrollmentID { get; set; }        // FK → tbl_EnrollmentMaster.Enrollment_Id
        public int? CourseID { get; set; }             // FK → tbl_Courses.Course_Id
        public bool? IsExpired { get; set; }
    }
}
