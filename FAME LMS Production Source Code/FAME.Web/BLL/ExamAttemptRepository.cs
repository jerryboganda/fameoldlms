using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    public class ExamAttemptRepository
    {
        #region Exam Attempts

        public List<ExamAttemptVM> GetAttemptList(int? packageId = null)
        {
            using (var db = new ExamAttemptContext())
            using (var fameDb = new FAMEEntities())
            {
                var query = db.tbl_ExamAttempt.AsQueryable();

                if (packageId.HasValue)
                {
                    query = query.Where(x => x.PackageID == packageId.Value);
                }

                var attempts = query.OrderBy(x => x.PackageID).ThenBy(x => x.SortOrder).ToList();

                var packageIds = attempts.Select(x => x.PackageID).Distinct().ToList();
                var packages = fameDb.tbl_Package
                    .Where(p => packageIds.Contains(p.PackageID))
                    .ToDictionary(p => p.PackageID, p => p.PackageName);

                return attempts.Select(x => new ExamAttemptVM
                {
                    ExamAttemptID = x.ExamAttemptID,
                    PackageID = x.PackageID,
                    AttemptName = x.AttemptName,
                    SortOrder = x.SortOrder ?? 0,
                    IsActive = x.IsActive ?? true,
                    CreatedDT = x.CreatedDT,
                    PackageName = packages.ContainsKey(x.PackageID) ? packages[x.PackageID] : "Unknown"
                }).ToList();
            }
        }

        public List<ExamAttemptVM> GetActiveAttemptsByPackage(int packageId)
        {
            using (var db = new ExamAttemptContext())
            {
                return db.tbl_ExamAttempt
                    .Where(x => x.PackageID == packageId && (x.IsActive ?? true))
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new ExamAttemptVM
                    {
                        ExamAttemptID = x.ExamAttemptID,
                        AttemptName = x.AttemptName
                    }).ToList();
            }
        }

        public ExamAttemptVM GetAttemptByID(int id)
        {
            using (var db = new ExamAttemptContext())
            {
                var attempt = db.tbl_ExamAttempt.Find(id);
                if (attempt == null) return null;

                string packageName = "";
                using (var fameDb = new FAMEEntities())
                {
                    packageName = fameDb.tbl_Package
                        .Where(p => p.PackageID == attempt.PackageID)
                        .Select(p => p.PackageName)
                        .FirstOrDefault();
                }

                return new ExamAttemptVM
                {
                    ExamAttemptID = attempt.ExamAttemptID,
                    PackageID = attempt.PackageID,
                    AttemptName = attempt.AttemptName,
                    SortOrder = attempt.SortOrder ?? 0,
                    IsActive = attempt.IsActive ?? true,
                    PackageName = packageName
                };
            }
        }

        public int SaveAttempt(ExamAttemptVM model)
        {
            using (var db = new ExamAttemptContext())
            {
                tbl_ExamAttempt entity;

                if (model.ExamAttemptID > 0)
                {
                    entity = db.tbl_ExamAttempt.Find(model.ExamAttemptID);
                    if (entity == null) return 0;
                }
                else
                {
                    entity = new tbl_ExamAttempt { CreatedDT = Common.GetCurrentDate() };
                    db.tbl_ExamAttempt.Add(entity);
                }

                entity.PackageID = model.PackageID;
                entity.AttemptName = model.AttemptName;
                entity.SortOrder = model.SortOrder;
                entity.IsActive = model.IsActive;

                db.SaveChanges();
                return entity.ExamAttemptID;
            }
        }

        public bool DeleteAttempt(int id)
        {
            using (var db = new ExamAttemptContext())
            {
                try
                {
                    var attempt = db.tbl_ExamAttempt.Find(id);
                    if (attempt == null) return false;

                    db.tbl_ExamAttempt.Remove(attempt);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion

        #region Study Schedules

        public List<StudyScheduleVM> GetScheduleList(int attemptId)
        {
            using (var db = new ExamAttemptContext())
            {
                var schedules = db.tbl_StudySchedule
                    .Where(x => x.ExamAttemptID == attemptId)
                    .OrderBy(x => x.SortOrder)
                    .ToList();

                // Get attempt info
                var attempt = db.tbl_ExamAttempt.Find(attemptId);
                string attemptName = attempt?.AttemptName ?? "";
                int packageId = attempt?.PackageID ?? 0;

                string packageName = "";
                using (var fameDb = new FAMEEntities())
                {
                    packageName = fameDb.tbl_Package
                        .Where(p => p.PackageID == packageId)
                        .Select(p => p.PackageName)
                        .FirstOrDefault() ?? "";
                }

                return schedules.Select(x => new StudyScheduleVM
                {
                    StudyScheduleID = x.StudyScheduleID,
                    ExamAttemptID = x.ExamAttemptID,
                    ScheduleType = x.ScheduleType,
                    WeekNumber = x.WeekNumber,
                    TopicTitle = x.TopicTitle,
                    TopicDescription = x.TopicDescription,
                    FilePath = x.FilePath,
                    ScheduleDate = x.ScheduleDate,
                    SortOrder = x.SortOrder ?? 0,
                    IsActive = x.IsActive ?? true,
                    AttemptName = attemptName,
                    PackageName = packageName,
                    PackageID = packageId
                }).ToList();
            }
        }

        public List<StudyScheduleVM> GetStudentSchedules(string studentId)
        {
            var result = new List<StudyScheduleVM>();

            using (var db = new ExamAttemptContext())
            using (var fameDb = new FAMEEntities())
            {
                var currentDate = Common.GetCurrentDate();

                // Get student's active enrollments with their selected attempts
                var enrollments = fameDb.tbl_EnrollmentMaster
                    .Where(e => e.StudentFid == studentId &&
                               e.IsExpired != true &&
                               e.Enrollment_EndDate >= currentDate &&
                               e.ExamAttemptID != null)
                    .Select(e => new { e.PackageId, e.ExamAttemptID, e.tbl_Package.PackageName })
                    .Distinct()
                    .ToList();

                if (!enrollments.Any())
                    return result;

                var attemptIds = enrollments.Where(e => e.ExamAttemptID.HasValue)
                                           .Select(e => e.ExamAttemptID.Value)
                                           .ToList();

                // Get all schedules for these attempts
                var schedules = db.tbl_StudySchedule
                    .Where(s => attemptIds.Contains(s.ExamAttemptID) && (s.IsActive ?? true))
                    .OrderBy(s => s.ExamAttemptID).ThenBy(s => s.SortOrder)
                    .ToList();

                // Get attempt names
                var attempts = db.tbl_ExamAttempt
                    .Where(a => attemptIds.Contains(a.ExamAttemptID))
                    .ToDictionary(a => a.ExamAttemptID, a => new { a.AttemptName, a.PackageID });

                foreach (var schedule in schedules)
                {
                    var attemptInfo = attempts.ContainsKey(schedule.ExamAttemptID) 
                        ? attempts[schedule.ExamAttemptID] 
                        : null;

                    var enrollmentInfo = enrollments.FirstOrDefault(e => 
                        e.ExamAttemptID == schedule.ExamAttemptID);

                    result.Add(new StudyScheduleVM
                    {
                        StudyScheduleID = schedule.StudyScheduleID,
                        ExamAttemptID = schedule.ExamAttemptID,
                        ScheduleType = schedule.ScheduleType,
                        WeekNumber = schedule.WeekNumber,
                        TopicTitle = schedule.TopicTitle,
                        TopicDescription = schedule.TopicDescription,
                        FilePath = schedule.FilePath,
                        ScheduleDate = schedule.ScheduleDate,
                        SortOrder = schedule.SortOrder ?? 0,
                        IsActive = schedule.IsActive ?? true,
                        AttemptName = attemptInfo?.AttemptName ?? "",
                        PackageName = enrollmentInfo?.PackageName ?? "",
                        PackageID = attemptInfo?.PackageID ?? 0
                    });
                }
            }

            return result;
        }

        public StudyScheduleVM GetScheduleByID(int id)
        {
            using (var db = new ExamAttemptContext())
            {
                var schedule = db.tbl_StudySchedule.Find(id);
                if (schedule == null) return null;

                var attempt = db.tbl_ExamAttempt.Find(schedule.ExamAttemptID);

                return new StudyScheduleVM
                {
                    StudyScheduleID = schedule.StudyScheduleID,
                    ExamAttemptID = schedule.ExamAttemptID,
                    ScheduleType = schedule.ScheduleType,
                    WeekNumber = schedule.WeekNumber,
                    TopicTitle = schedule.TopicTitle,
                    TopicDescription = schedule.TopicDescription,
                    FilePath = schedule.FilePath,
                    ScheduleDate = schedule.ScheduleDate,
                    SortOrder = schedule.SortOrder ?? 0,
                    IsActive = schedule.IsActive ?? true,
                    AttemptName = attempt?.AttemptName ?? ""
                };
            }
        }

        public int SaveSchedule(StudyScheduleVM model)
        {
            using (var db = new ExamAttemptContext())
            {
                tbl_StudySchedule entity;

                if (model.StudyScheduleID > 0)
                {
                    entity = db.tbl_StudySchedule.Find(model.StudyScheduleID);
                    if (entity == null) return 0;
                }
                else
                {
                    entity = new tbl_StudySchedule { CreatedDT = Common.GetCurrentDate() };
                    db.tbl_StudySchedule.Add(entity);
                }

                entity.ExamAttemptID = model.ExamAttemptID;
                entity.ScheduleType = model.ScheduleType;
                entity.WeekNumber = model.WeekNumber;
                entity.TopicTitle = model.TopicTitle;
                entity.TopicDescription = model.TopicDescription;
                entity.FilePath = model.FilePath;
                entity.ScheduleDate = model.ScheduleDate;
                entity.SortOrder = model.SortOrder;
                entity.IsActive = model.IsActive;

                db.SaveChanges();
                return entity.StudyScheduleID;
            }
        }

        public bool DeleteSchedule(int id)
        {
            using (var db = new ExamAttemptContext())
            {
                try
                {
                    var schedule = db.tbl_StudySchedule.Find(id);
                    if (schedule == null) return false;

                    db.tbl_StudySchedule.Remove(schedule);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion

        #region Student Assignments

        /// <summary>
        /// Get all students enrolled in a specific package with their current exam attempt assignment
        /// Groups by student to avoid duplicates from multiple enrollments
        /// </summary>
        public List<StudentEnrollmentVM> GetStudentsByPackage(int packageId)
        {
            using (var db = new FAMEEntities())
            {
                var currentDate = Common.GetCurrentDate();

                // First, get all active enrollments for this package
                var allEnrollments = db.tbl_EnrollmentMaster
                    .Where(e => e.PackageId == packageId && 
                               e.IsExpired != true && 
                               e.Enrollment_EndDate >= currentDate)
                    .Select(e => new StudentEnrollmentVM
                    {
                        EnrollmentId = e.Enrollment_Id,
                        StudentId = e.StudentFid,
                        StudentName = e.AspNetUsers.UserName ?? e.AspNetUsers.Email,
                        StudentEmail = e.AspNetUsers.Email,
                        PackageId = e.PackageId ?? 0,
                        PackageName = e.tbl_Package.PackageName,
                        CurrentExamAttemptId = e.ExamAttemptID,
                        EnrollmentEndDate = e.Enrollment_EndDate
                    })
                    .ToList();

                // Then group by student in memory to get only one entry per student
                var distinctStudents = allEnrollments
                    .GroupBy(e => e.StudentEmail)
                    .Select(g => g.OrderByDescending(e => e.EnrollmentEndDate).First())
                    .OrderBy(e => e.StudentEmail)
                    .ToList();

                return distinctStudents;
            }
        }

        /// <summary>
        /// Assign an exam attempt to a student's enrollment
        /// </summary>
        public bool AssignAttemptToStudent(int enrollmentId, int? examAttemptId)
        {
            using (var db = new FAMEEntities())
            {
                var enrollment = db.tbl_EnrollmentMaster.Find(enrollmentId);
                if (enrollment == null) return false;

                enrollment.ExamAttemptID = examAttemptId;
                db.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Bulk assign an exam attempt to all active students enrolled in a package
        /// </summary>
        public int BulkAssignAttemptToPackage(int packageId, int? examAttemptId)
        {
            using (var db = new FAMEEntities())
            {
                var currentDate = Common.GetCurrentDate();

                var enrollments = db.tbl_EnrollmentMaster
                    .Where(e => e.PackageId == packageId && 
                               e.IsExpired != true && 
                               e.Enrollment_EndDate >= currentDate)
                    .ToList();

                foreach (var enrollment in enrollments)
                {
                    enrollment.ExamAttemptID = examAttemptId;
                }

                db.SaveChanges();
                return enrollments.Count;
            }
        }

        /// <summary>
        /// Get enrollment details by ID
        /// </summary>
        public StudentEnrollmentVM GetEnrollmentById(int enrollmentId)
        {
            using (var db = new FAMEEntities())
            {
                var e = db.tbl_EnrollmentMaster.Find(enrollmentId);
                if (e == null) return null;

                string currentAttemptName = null;
                if (e.ExamAttemptID.HasValue)
                {
                    using (var examDb = new ExamAttemptContext())
                    {
                        currentAttemptName = examDb.tbl_ExamAttempt
                            .Where(a => a.ExamAttemptID == e.ExamAttemptID)
                            .Select(a => a.AttemptName)
                            .FirstOrDefault();
                    }
                }

                return new StudentEnrollmentVM
                {
                    EnrollmentId = e.Enrollment_Id,
                    StudentId = e.StudentFid,
                    StudentName = e.AspNetUsers.UserName ?? e.AspNetUsers.Email,
                    StudentEmail = e.AspNetUsers.Email,
                    PackageId = e.PackageId ?? 0,
                    PackageName = e.tbl_Package.PackageName,
                    CurrentExamAttemptId = e.ExamAttemptID,
                    CurrentExamAttemptName = currentAttemptName,
                    EnrollmentEndDate = e.Enrollment_EndDate
                };
            }
        }

        #endregion
    }
}
