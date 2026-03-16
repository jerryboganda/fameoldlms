using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models.Certificate;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    /// <summary>Helper class for raw SQL result mapping.</summary>
    public class EnrolledUserRaw
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime? Enrollment_Date { get; set; }
        public int ManualCertCount { get; set; }
    }

    public class ManualCertificateService : IManualCertificateService
    {
        private readonly ILogger _logger;

        public ManualCertificateService(ILogger logger)
        {
            _logger = logger;
        }

        public List<CourseDropdownItem> GetAllCourses()
        {
            using (var fameDb = new FAMEEntities())
            {
                fameDb.Database.CommandTimeout = 30;
                return fameDb.tbl_Courses
                    .Where(c => c.IsActive == true)
                    .OrderBy(c => c.SortID).ThenBy(c => c.Course_Name)
                    .Select(c => new CourseDropdownItem
                    {
                        Id = c.Course_Id,
                        Name = c.Course_Name
                    }).ToList();
            }
        }

        public List<PackageDropdownItem> GetAllPackages()
        {
            using (var fameDb = new FAMEEntities())
            {
                fameDb.Database.CommandTimeout = 30;
                return fameDb.tbl_Package
                    .Where(p => p.IsActive == true || p.IsActive == null)
                    .OrderBy(p => p.SortID).ThenBy(p => p.PackageName)
                    .Select(p => new PackageDropdownItem
                    {
                        Id = p.PackageID,
                        Name = p.PackageName
                    }).ToList();
            }
        }

        public List<EnrolledUserRow> GetUsersByCourse(int courseId)
        {
            using (var fameDb = new FAMEEntities())
            {
                fameDb.Database.CommandTimeout = 120;

                // Get course name
                var courseName = fameDb.tbl_Courses
                    .Where(c => c.Course_Id == courseId)
                    .Select(c => c.Course_Name)
                    .FirstOrDefault() ?? "Unknown Course";

                // Single raw SQL with LEFT JOIN to tbl_ManualCertificate for cert counts.
                // Avoids EF6 .Contains() with thousands of IDs (generates N parameters = timeout).
                var rows = fameDb.Database.SqlQuery<EnrolledUserRaw>(@"
                    SELECT u.Id AS UserId, u.UserName, u.Email,
                           tu.User_Name AS FullName,
                           MIN(em.Enrollment_Date) AS Enrollment_Date,
                           ISNULL(mc.CertCount, 0) AS ManualCertCount
                    FROM tbl_EnrollmentDetail ed
                    INNER JOIN tbl_EnrollmentMaster em ON ed.EnrollmentID = em.Enrollment_Id
                    INNER JOIN AspNetUsers u ON em.StudentFid = u.Id
                    LEFT JOIN tbl_User tu ON tu.User_AspUser = u.Id
                    LEFT JOIN (
                        SELECT UserId, COUNT(*) AS CertCount
                        FROM tbl_ManualCertificate
                        WHERE CourseId = @courseId AND IsActive = 1
                        GROUP BY UserId
                    ) mc ON mc.UserId = u.Id
                    WHERE ed.CourseID = @courseId AND ISNULL(em.IsExpired, 0) != 1
                    GROUP BY u.Id, u.UserName, u.Email, tu.User_Name, mc.CertCount
                    ORDER BY u.UserName",
                    new SqlParameter("@courseId", courseId))
                    .ToList();

                return rows.Select(u => new EnrolledUserRow
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    CourseName = courseName,
                    CourseId = courseId,
                    PackageId = null,
                    PackageName = null,
                    EnrollmentDate = u.Enrollment_Date,
                    ManualCertCount = u.ManualCertCount
                }).ToList();
            }
        }

        public List<EnrolledUserRow> GetUsersByPackage(int packageId)
        {
            using (var fameDb = new FAMEEntities())
            {
                fameDb.Database.CommandTimeout = 120;

                // Get package name
                var packageName = fameDb.tbl_Package
                    .Where(p => p.PackageID == packageId)
                    .Select(p => p.PackageName)
                    .FirstOrDefault() ?? "Unknown Package";

                // Single raw SQL — avoids both EF6 GroupBy-FirstOrDefault (correlated subquery)
                // AND .Contains() with thousands of user IDs (generates N SQL parameters).
                var rows = fameDb.Database.SqlQuery<EnrolledUserRaw>(@"
                    SELECT u.Id AS UserId, u.UserName, u.Email,
                           tu.User_Name AS FullName,
                           MIN(em.Enrollment_Date) AS Enrollment_Date,
                           ISNULL(mc.CertCount, 0) AS ManualCertCount
                    FROM tbl_EnrollmentMaster em
                    INNER JOIN AspNetUsers u ON em.StudentFid = u.Id
                    LEFT JOIN tbl_User tu ON tu.User_AspUser = u.Id
                    LEFT JOIN (
                        SELECT UserId, COUNT(*) AS CertCount
                        FROM tbl_ManualCertificate
                        WHERE PackageId = @packageId AND IsActive = 1
                        GROUP BY UserId
                    ) mc ON mc.UserId = u.Id
                    WHERE em.PackageId = @packageId AND ISNULL(em.IsExpired, 0) != 1
                    GROUP BY u.Id, u.UserName, u.Email, tu.User_Name, mc.CertCount
                    ORDER BY u.UserName",
                    new SqlParameter("@packageId", packageId))
                    .ToList();

                return rows.Select(u => new EnrolledUserRow
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    FullName = u.FullName,
                    Email = u.Email,
                    CourseName = null,
                    CourseId = null,
                    PackageId = packageId,
                    PackageName = packageName,
                    EnrollmentDate = u.Enrollment_Date,
                    ManualCertCount = u.ManualCertCount
                }).ToList();
            }
        }

        public int UploadManualCertificate(string userId, int? courseId, int? packageId,
            string learnerName, string fileName, string filePath, string title, string uploadedBy)
        {
            using (var db = new CertificateDbContext())
            {
                var entity = new tbl_ManualCertificate
                {
                    UserId = userId,
                    CourseId = courseId,
                    PackageId = packageId,
                    LearnerName = learnerName,
                    FileName = fileName,
                    FilePath = filePath,
                    Title = title,
                    UploadedBy = uploadedBy,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                // Denormalize course/package names
                if (courseId.HasValue)
                {
                    using (var fameDb = new FAMEEntities())
                    {
                        entity.CourseName = fameDb.tbl_Courses
                            .Where(c => c.Course_Id == courseId.Value)
                            .Select(c => c.Course_Name)
                            .FirstOrDefault();
                    }
                }
                if (packageId.HasValue)
                {
                    using (var fameDb = new FAMEEntities())
                    {
                        entity.PackageName = fameDb.tbl_Package
                            .Where(p => p.PackageID == packageId.Value)
                            .Select(p => p.PackageName)
                            .FirstOrDefault();
                    }
                }

                db.ManualCertificates.Add(entity);
                db.SaveChanges();

                _logger.Information("Manual certificate uploaded: Id={Id}, UserId={UserId}, Course={CourseId}, Package={PackageId}",
                    entity.Id, userId, courseId, packageId);

                return entity.Id;
            }
        }

        public List<ManualCertificateVM> GetManualCertificatesForUser(string userId)
        {
            using (var db = new CertificateDbContext())
            {
                return db.ManualCertificates
                    .Where(mc => mc.UserId == userId && mc.IsActive)
                    .OrderByDescending(mc => mc.CreatedAt)
                    .Select(mc => new ManualCertificateVM
                    {
                        Id = mc.Id,
                        UserId = mc.UserId,
                        LearnerName = mc.LearnerName,
                        CourseId = mc.CourseId,
                        PackageId = mc.PackageId,
                        CourseName = mc.CourseName,
                        PackageName = mc.PackageName,
                        FileName = mc.FileName,
                        FilePath = mc.FilePath,
                        Title = mc.Title,
                        IsActive = mc.IsActive,
                        CreatedAt = mc.CreatedAt
                    }).ToList();
            }
        }

        public void DeleteManualCertificate(int id, string adminUserId)
        {
            using (var db = new CertificateDbContext())
            {
                var cert = db.ManualCertificates.Find(id);
                if (cert != null)
                {
                    cert.IsActive = false;
                    db.SaveChanges();
                    _logger.Information("Manual certificate soft-deleted: Id={Id} by {Admin}", id, adminUserId);
                }
            }
        }
    }
}
