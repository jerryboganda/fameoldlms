using System.Data.Entity;
using System.ComponentModel.DataAnnotations.Schema;

namespace First_Aid_Made_Easy.DAL
{
    // Partial class to extend the auto-generated FAMEEntities context
    // NOTE: Ambassador entities have been moved to AmbassadorDbContext (Code-First)
    // because FAMEEntities uses Database-First (EDMX) which throws
    // UnintentionalCodeFirstException and cannot map Code-First entities.
    public partial class FAMEEntities
    {
        // This method is called when the DbContext is being configured
        partial void OnModelCreatingPartial(DbModelBuilder modelBuilder)
        {
            // Configure tbl_GuidelineVideo entity
            modelBuilder.Entity<tbl_GuidelineVideo>()
                .ToTable("tbl_GuidelineVideo")
                .HasKey(g => g.GuidelineVideoID);

            modelBuilder.Entity<tbl_GuidelineVideo>()
                .Property(g => g.GuidelineVideoID)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<tbl_GuidelineVideo>()
                .Property(g => g.VideoTitle)
                .IsRequired()
                .HasMaxLength(255);

            modelBuilder.Entity<tbl_GuidelineVideo>()
                .Property(g => g.VideoDescription)
                .HasMaxLength(500);

            modelBuilder.Entity<tbl_GuidelineVideo>()
                .Property(g => g.VideoPath)
                .IsRequired()
                .HasMaxLength(500);

            // Configure the foreign key relationship
            modelBuilder.Entity<tbl_GuidelineVideo>()
                .HasRequired(g => g.tbl_Package)
                .WithMany(p => p.tbl_GuidelineVideo)
                .HasForeignKey(g => g.PackageID)
                .WillCascadeOnDelete(true);
        }
    }
}
