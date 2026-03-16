using System.Data.Entity;

namespace First_Aid_Made_Easy.DAL
{
    public class GuidelineContext : DbContext
    {
        static GuidelineContext()
        {
            // Disable EF's default model initialization to avoid conflicts with EDMX
            Database.SetInitializer<GuidelineContext>(null);
        }

        public GuidelineContext() : base("name=FAME_CodeFirst") 
        { 
        }

        public virtual DbSet<tbl_GuidelineVideo> tbl_GuidelineVideo { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Explicitly map to avoid any EF metadata confusion
            modelBuilder.Entity<tbl_GuidelineVideo>()
                .ToTable("tbl_GuidelineVideo", "dbo")
                .HasKey(g => g.GuidelineVideoID);

            modelBuilder.Entity<tbl_GuidelineVideo>()
                .Property(g => g.GuidelineVideoID)
                .HasColumnName("GuidelineVideoID");

            modelBuilder.Entity<tbl_GuidelineVideo>()
                .Property(g => g.PackageID)
                .HasColumnName("PackageID");

            modelBuilder.Entity<tbl_GuidelineVideo>()
                .Property(g => g.VideoTitle)
                .HasColumnName("VideoTitle")
                .HasMaxLength(255);

            modelBuilder.Entity<tbl_GuidelineVideo>()
                .Property(g => g.VideoDescription)
                .HasColumnName("VideoDescription")
                .HasMaxLength(500);

            modelBuilder.Entity<tbl_GuidelineVideo>()
                .Property(g => g.VideoPath)
                .HasColumnName("VideoPath")
                .HasMaxLength(500);

            modelBuilder.Entity<tbl_GuidelineVideo>()
                .Property(g => g.SortOrder)
                .HasColumnName("SortOrder");

            modelBuilder.Entity<tbl_GuidelineVideo>()
                .Property(g => g.IsActive)
                .HasColumnName("IsActive");

            modelBuilder.Entity<tbl_GuidelineVideo>()
                .Property(g => g.CreatedDT)
                .HasColumnName("CreatedDT");

            base.OnModelCreating(modelBuilder);
        }
    }
}
