using System.Data.Entity;

namespace First_Aid_Made_Easy.DAL
{
    public class ExamAttemptContext : DbContext
    {
        static ExamAttemptContext()
        {
            Database.SetInitializer<ExamAttemptContext>(null);
        }

        public ExamAttemptContext() : base("name=FAME_CodeFirst") 
        { 
        }

        public virtual DbSet<tbl_ExamAttempt> tbl_ExamAttempt { get; set; }
        public virtual DbSet<tbl_StudySchedule> tbl_StudySchedule { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ExamAttempt mapping
            modelBuilder.Entity<tbl_ExamAttempt>()
                .ToTable("tbl_ExamAttempt", "dbo")
                .HasKey(e => e.ExamAttemptID);

            modelBuilder.Entity<tbl_ExamAttempt>()
                .Property(e => e.AttemptName).HasMaxLength(100);

            // StudySchedule mapping
            modelBuilder.Entity<tbl_StudySchedule>()
                .ToTable("tbl_StudySchedule", "dbo")
                .HasKey(s => s.StudyScheduleID);

            modelBuilder.Entity<tbl_StudySchedule>()
                .Property(s => s.TopicTitle).HasMaxLength(255);

            modelBuilder.Entity<tbl_StudySchedule>()
                .Property(s => s.FilePath).HasMaxLength(500);

            base.OnModelCreating(modelBuilder);
        }
    }
}
