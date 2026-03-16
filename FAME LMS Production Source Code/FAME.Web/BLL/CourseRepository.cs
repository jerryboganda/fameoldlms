using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    public class CourseRepository : ICourseRepository
    {
        //For Index Page
        public List<CourseVM> GetOurCourses()
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_Courses
                    .Where(x => x.IsActive == true)
                    .Take(4)
                    .Select(x => new CourseVM
                    {
                        Course_Id = x.Course_Id,
                        Course_Description = x.Course_Description,
                        Course_Name = x.Course_Name,
                        Course_Pic = x.Course_Pic,
                        Teacher_Fid = x.TeacherFid,
                        Teacher_Name = db.tbl_User.Where(y => y.User_AspUser == x.TeacherFid).Select(y => y.User_Name).FirstOrDefault(),
                        Teacher_Pic = db.tbl_User.Where(y => y.User_AspUser == x.TeacherFid).Select(y => y.User_Pic).FirstOrDefault(),
                        Details = x.tbl_CourseDetails.Select(z => new CourseDetailVM
                        {
                            CourseDetail_Body = z.CourseDetail_Body
                        }).ToList(),
                        DurationInMin = (x.tbl_Video.Sum(y => y.Video_Length) ?? 0),
                        Lessons = x.tbl_Video.Count
                    })
                    .ToList();
            }
        }
        public List<CourseVM> GetList(string UserID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var listvm = db.tbl_Courses
                    .Where(x => x.IsActive == true)
                    .Select(x => new CourseVM
                    {
                        Course_Id = x.Course_Id,
                        Course_Description = x.Course_Description,
                        Course_Name = x.Course_Name,
                        Course_Pic = x.Course_Pic,
                        Teacher_Fid = x.TeacherFid,
                        Teacher_Name = db.tbl_User.Where(y => y.User_AspUser == x.TeacherFid).Select(y => y.User_Name).FirstOrDefault(),
                        Teacher_Pic = db.tbl_User.Where(y => y.User_AspUser == x.TeacherFid).Select(y => y.User_Pic).FirstOrDefault(),
                        Details = x.tbl_CourseDetails.Select(z => new CourseDetailVM
                        {
                            CourseDetail_Body = z.CourseDetail_Body
                        }).ToList(),
                        DurationInMin = (x.tbl_Video.Sum(y => y.Video_Length) ?? 0),
                        Lessons = x.tbl_Video.Count
                    })
                    .ToList();

                if (!string.IsNullOrEmpty(UserID))
                {
                    var cids = (from d in db.tbl_EnrollmentDetail
                                join m in db.tbl_EnrollmentMaster on d.EnrollmentID equals m.Enrollment_Id
                                join s in db.tbl_Section on d.Section_Fid equals s.Section_ID
                                where m.StudentFid == UserID
                                select (int?)s.Course_Fid).Distinct().ToList();

                    var currentDate = Common.GetCurrentDate();
                    listvm.Where(x => cids.Contains(x.Course_Id)).ToList().ForEach(c => c.EndDate = currentDate);
                }
                return listvm;
            }
        }

        public int SaveCourse(CourseVM course)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_Courses model = new tbl_Courses();
                if (course.Course_Id > 0)
                {
                    model = db.tbl_Courses.Find(course.Course_Id);
                    List<tbl_CourseDetails> list = db.tbl_CourseDetails.Where(x => x.Course_Fid == course.Course_Id).ToList();
                    db.tbl_CourseDetails.RemoveRange(list);
                }
                model.Course_Id = course.Course_Id;
                model.Course_Description = course.Course_Description;
                model.Course_Name = course.Course_Name;
                model.Course_Price = course.Course_Price;
                model.Course_Pic = course.Course_Pic;
                model.TeacherFid = course.Teacher_Fid;
                model.tbl_CourseDetails = course.Details.ConvertAll(a => new tbl_CourseDetails()
                {
                    CourseDetail_ID = a.CourseDetail_ID,
                    CourseDetail_Body = a.CourseDetail_Body,
                    Course_Fid = a.Course_Fid
                });
                if (!(course.Course_Id > 0))
                {
                    db.tbl_Courses.Add(model);
                }
                db.SaveChanges();
                return model.Course_Id;
            }
        }
        public MyCoursesVM GetListByStudent(string student_Fid)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var cDate = Common.GetCurrentDate();
                var Slist = db.sp_MySections(student_Fid).ToList();
                var list = db.sp_MyCourses(student_Fid).ToList();
                var packList = db.tbl_EnrollmentMaster
                .Where(x => x.StudentFid == student_Fid &&
                            x.Enrollment_EndDate >= cDate && x.IsExpired != true &&
                            x.tbl_Package.HasType == true && x.PackageId > 0)
                            .GroupBy(x => x.PackageId)
                            .Select(g => g.FirstOrDefault())
                            .Select(x => new PackageVM
                            {
                                PackageID = x.PackageId.Value,
                                Duration = x.tbl_PackageDuration.Duration ?? 0,
                                CreatedDT = x.Enrollment_EndDate.Value
                            })
                            .ToList();

                MyCoursesVM c = new MyCoursesVM
                {
                    CorList = list.ConvertAll(x => new CourseVM
                    {
                        Course_Description = x.Course_Description,
                        Course_Name = x.Course_Name,
                        Course_Id = x.Course_Id,
                        Course_Pic = x.Course_Pic,
                        Lessons = x.Videos ?? 0,
                        Views = x.Views ?? 0,
                        DurationInMin = x.VideosDur ?? 0,
                        WatchedVideos = x.WatchedVideos ?? 0,
                    }),
                    SectionList = Slist.ConvertAll(x => new SectionVM()
                    {
                        Section_ID = x.Section_ID,
                        Section_Name = x.Section_Name,
                        Course_Name = x.Course_Name,
                        Course_Pic = x.Course_Pic,
                        Lessons = x.Videos ?? 0,
                        Views = x.Views ?? 0,
                        Course_Fid = x.Course_Fid,
                        DurationInMin = x.VideosDur ?? 0,
                        WatchedVideos = x.WatchedVideos ?? 0,
                    }),
                };

                if (packList.Count() > 0)
                {
                    c.PackageList = new List<PackageVM>();
                    foreach (var p in packList)
                    {
                        var package = db.tbl_Package.Where(x => x.PackageID == p.PackageID).Select(x => new PackageVM
                        {
                            PackageID = x.PackageID,
                            PackageName = x.PackageName,
                            PackageDescription = x.PackageDescription,
                            PackagePrice = x.PackagePrice,
                            Duration = p.Duration,
                            Detail = x.tbl_PackageDetail.Select(d => new PackageDetailVM()
                            {
                                CourseName = d.tbl_Courses.Course_Name,
                                CoursePic = d.tbl_Courses.Course_Pic,
                                CourseID = d.CourseID,
                                Paper = d.Paper,
                                Type = d.Type,
                                DurationInMin = d.tbl_Courses.tbl_Video.Sum(v => v.Video_Length)
                            }).ToList(),
                        }).FirstOrDefault();
                        package.EndDate = Common.DateToString(p.CreatedDT);
                        c.PackageList.Add(package);
                    }
                }

                return c;
            }
        }

        public List<CourseVM> GetListVM(string ID, bool IsAdmin)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                List<tbl_Courses> list = db.tbl_Courses.Include("tbl_Video").Where(x => (x.IsActive ?? true) && (x.TeacherFid == ID || IsAdmin)).ToList();
                return list.ConvertAll(x => new CourseVM
                {
                    Course_Id = x.Course_Id,
                    Course_Description = x.Course_Description,
                    Course_Name = x.Course_Name,
                    Course_Pic = x.Course_Pic,
                    Teacher_Fid = x.TeacherFid,
                    Lessons = x.tbl_Video.Count
                });
            }
        }

        public List<tbl_Courses> GetList(string id = "", bool isAdmin = true)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_Courses.Where(x => x.IsActive != false && x.TeacherFid == id || isAdmin).ToList();
            }
        }
        public CourseVM GetByID(int id, string Uid)
        {
            using (var db = new FAMEEntities())
            {
                DateTime currentDate = Common.GetCurrentDate();

                var course = db.tbl_Courses
                    .Include("tbl_Section.tbl_Video")
                    .Include("tbl_CourseDetails")
                    .Include("tbl_Video")
                    .FirstOrDefault(c => c.Course_Id == id);

                if (course == null) return null;

                // Fetch all progress records for this user to avoid N+1 queries
                var progressLookup = db.tbl_Progress
                    .Where(p => p.Student_Fid == Uid)
                    .ToDictionary(p => p.Video_Fid, p => new { p.isCompleted, p.ScreenTime });

                var courseVM = new CourseVM
                {
                    Course_Id = course.Course_Id,
                    Course_Description = course.Course_Description,
                    Course_Name = course.Course_Name,
                    Course_Pic = course.Course_Pic,
                    Course_Price = course.Course_Price ?? 0,
                    Teacher_Fid = course.TeacherFid,
                    SortID = course.SortID ?? 0,
                    Details = course.tbl_CourseDetails.Select(y => new CourseDetailVM
                    {
                        CourseDetail_Body = y.CourseDetail_Body
                    }).ToList(),
                    DurationInMin = course.tbl_Video.Sum(y => y.Video_Length) ?? 0,
                    Lessons = course.tbl_Video.Count,
                    SectionList = course.tbl_Section
                        .Where(y => y.IsActive != false)
                        .Select(y => new SectionVM
                        {
                            Section_ID = y.Section_ID,
                            Section_Name = y.Section_Name,
                            Section_Price = y.Section_Price,
                            Course_Fid = y.Course_Fid,
                            TeacherFid = course.TeacherFid,
                            SortID = y.SortID ?? 0,
                            DurationInMin = y.tbl_Video.Sum(s => s.Video_Length) ?? 0,
                            Lessons = y.tbl_Video.Count,
                            EndDate = db.tbl_EnrollmentDetail
                                .Where(ed => ed.Section_Fid == y.Section_ID && ed.tbl_EnrollmentMaster.StudentFid == Uid && ed.IsExpired != true && ed.tbl_EnrollmentMaster.IsExpired != true && ed.tbl_EnrollmentMaster.Enrollment_EndDate >= currentDate)
                                .Select(ed => ed.tbl_EnrollmentMaster.Enrollment_EndDate)
                                .FirstOrDefault(),
                            VideoList = y.tbl_Video
                                .Where(v => v.SectionSub_Fid == null)
                                .OrderBy(v => v.SortID)
                                .Select(v => new VideoVM
                                {
                                    Video_Id = v.Video_Id,
                                    Video_Name = v.Video_Name,
                                    Video_Path = v.Video_Path,
                                    Video_Length = v.Video_Length,
                                    Video_ShortDescription = v.Video_ShortDescription,
                                    Type = v.Type,
                                    SortID = v.SortID ?? 0,
                                    Views = 0,
                                    IsWatched = db.tbl_Progress.Where(p => p.Video_Fid == v.Video_Id && p.Student_Fid == Uid).Select(p => p.isCompleted).FirstOrDefault() ?? false,
                                    ScreenTime = db.tbl_Progress.Where(p => p.Video_Fid == v.Video_Id && p.Student_Fid == Uid).Select(p => p.ScreenTime).FirstOrDefault() ?? 0
                                }).ToList(),
                            SectionSubList = y.tbl_SectionSub.Where(x => x.IsActive != false)
                                .Select(sb => new SectionSubVM
                                {
                                    ID = sb.ID,
                                    Section_Fid = sb.Section_Fid,
                                    SortID = sb.SortID ?? 0,
                                    Course_Fid = sb.Course_Fid,
                                    SectionSub_Name = sb.SectionSub_Name,
                                    DurationInMin = sb.tbl_Video.Sum(s => s.Video_Length) ?? 0,
                                    Lessons = sb.tbl_Video.Count,
                                    VideoList = sb.tbl_Video
                                        .OrderBy(v => v.SortID)
                                        .Select(v => new VideoVM
                                        {
                                            Video_Id = v.Video_Id,
                                            Video_Name = v.Video_Name,
                                            Video_Path = v.Video_Path,
                                            Video_Length = v.Video_Length,
                                            Video_ShortDescription = v.Video_ShortDescription,
                                            Type = v.Type,
                                            SortID = v.SortID ?? 0,
                                            Views = 0,
                                            IsWatched = db.tbl_Progress.Where(p => p.Video_Fid == v.Video_Id && p.Student_Fid == Uid).Select(p => p.isCompleted).FirstOrDefault() ?? false,
                                            ScreenTime = db.tbl_Progress.Where(p => p.Video_Fid == v.Video_Id && p.Student_Fid == Uid).Select(p => p.ScreenTime).FirstOrDefault() ?? 0
                                        }).ToList()
                                }).ToList()
                        }).ToList()
                };

                var enrollment = db.tbl_EnrollmentMaster
                    .Where(e => (db.tbl_EnrollmentDetail.Any(ed => ed.EnrollmentID == e.Enrollment_Id && ed.CourseID == id) ||
                                 db.tbl_PackageDetail.Any(pd => pd.PackageID == e.PackageId && pd.CourseID == id))
                                 && e.StudentFid == Uid && e.IsExpired != true && e.Enrollment_EndDate >= currentDate)
                    .OrderByDescending(e => e.Enrollment_EndDate)
                    .FirstOrDefault();

                if (enrollment != null)
                {
                    var nextInstallment = enrollment.tbl_StudentInstallments
                        .Where(i => i.InstallmentDate > currentDate)
                        .OrderBy(i => i.InstallmentDate)
                        .FirstOrDefault();

                    courseVM.NextInstallment = nextInstallment?.InstallmentDate;
                    courseVM.InstallmentPrice = nextInstallment?.InstallmentPrice;
                    courseVM.PendingInstallment = enrollment.tbl_StudentInstallments.Count(i => i.InstallmentDate > currentDate);

                    if (enrollment.tbl_StudentInstallments.Any(i => i.InstallmentDate < currentDate && i.IsPaid != true))
                    {
                        enrollment = null;
                    }
                }

                courseVM.EndDate = enrollment?.Enrollment_EndDate ?? currentDate;
                courseVM.isExpired = enrollment == null;
                courseVM.RemainingDays = enrollment?.Enrollment_EndDate?.Subtract(currentDate).TotalDays ?? 0;

                return courseVM;
            }
        }
        public CourseVM GetDetailsByID(int id)
        {
            using (var db = new FAMEEntities())
            {
                var course = db.tbl_Courses
                    .Include("tbl_Section.tbl_Video")
                    .Include("tbl_Section.tbl_SectionSub.tbl_Video")
                    .Include("tbl_CourseDetails")
                    .Include("tbl_Chapter")
                    .FirstOrDefault(c => c.Course_Id == id);

                if (course == null) return null;

                var courseVM = new CourseVM
                {
                    Course_Id = course.Course_Id,
                    Course_Description = course.Course_Description,
                    Course_Name = course.Course_Name,
                    Course_Pic = course.Course_Pic,
                    Course_Price = course.Course_Price ?? 0,
                    Teacher_Fid = course.TeacherFid,
                    SortID = course.SortID ?? 0,
                    Details = course.tbl_CourseDetails.Select(detail => new CourseDetailVM
                    {
                        CourseDetail_Body = detail.CourseDetail_Body
                    }).ToList(),
                    DurationInMin = course.tbl_Video.Sum(video => video.Video_Length) ?? 0,
                    Lessons = course.tbl_Video.Count,
                    ChapterList = course.tbl_Chapter.Select(x => new ChapterVM
                    {
                        ChapterID = x.ChapterID,
                        Title = x.Title,
                        SortID = x.SortID,
                    }).ToList(),
                    SectionList = course.tbl_Section
                        .Where(section => section.IsActive != false)
                        .Select(section => new SectionVM
                        {
                            Section_ID = section.Section_ID,
                            Section_Name = section.Section_Name,
                            Section_Price = section.Section_Price,
                            Course_Fid = section.Course_Fid,
                            TeacherFid = course.TeacherFid,
                            SortID = section.SortID ?? 0,
                            DurationInMin = section.tbl_Video.Sum(video => video.Video_Length) ?? 0,
                            Lessons = section.tbl_Video.Count,
                            VideoList = section.tbl_Video
                                .Where(video => video.SectionSub_Fid == null)
                                .OrderBy(video => video.SortID)
                                .Select(video => new VideoVM
                                {
                                    Video_Id = video.Video_Id,
                                    Video_Name = video.Video_Name,
                                    Video_Path = video.Video_Path,
                                    Video_Length = video.Video_Length,
                                    Video_ShortDescription = video.Video_ShortDescription,
                                    Type = video.Type,
                                    SortID = video.SortID ?? 0
                                }).ToList(),
                            SectionSubList = section.tbl_SectionSub.Where(x => x.IsActive != false)
                                .Select(subSection => new SectionSubVM
                                {
                                    ID = subSection.ID,
                                    SortID = subSection.SortID ?? 0,
                                    Section_Fid = subSection.Section_Fid,
                                    Course_Fid = subSection.Course_Fid,
                                    SectionSub_Name = subSection.SectionSub_Name,
                                    VideoList = subSection.tbl_Video
                                        .OrderBy(video => video.SortID)
                                        .Select(video => new VideoVM
                                        {
                                            Video_Id = video.Video_Id,
                                            Video_Name = video.Video_Name,
                                            Video_Path = video.Video_Path,
                                            Video_Length = video.Video_Length,
                                            Video_ShortDescription = video.Video_ShortDescription,
                                            Type = video.Type,
                                            SortID = video.SortID ?? 0
                                        }).ToList()
                                }).ToList()
                        }).ToList()
                };

                return courseVM;
            }
        }

        public CourseVM GetByID(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var x = db.tbl_Courses.Find(id);
                CourseVM corvm = new CourseVM()
                {
                    Course_Id = x.Course_Id,
                    Course_Description = x.Course_Description,
                    Course_Name = x.Course_Name,
                    Course_Pic = x.Course_Pic,
                    Course_Price = x.Course_Price ?? 0,
                    Teacher_Fid = x.TeacherFid,
                    Teacher_Name = Common.GetUserNameByASpUserID(x.TeacherFid).User_Name,
                    Teacher_Pic = db.tbl_User.Where(y => y.User_AspUser == x.TeacherFid).Select(y => y.User_Pic).FirstOrDefault(),
                    Details = db.tbl_CourseDetails.Where(y => y.Course_Fid == x.Course_Id).ToList().ConvertAll(z => new CourseDetailVM
                    {
                        CourseDetail_Body = z.CourseDetail_Body
                    }),
                    DurationInMin = (x.tbl_Video.Sum(y => y.Video_Length) ?? 0),
                    Lessons = x.tbl_Video.Count
                };
                return corvm;
            }
        }
        #region Chapter CRUD

        public ChapterVM GetChapterByID(int ChapterID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var x = db.tbl_Chapter.Find(ChapterID);
                return new ChapterVM
                {
                    Course_Id = x.Course_Id,
                    ChapterID = x.ChapterID,
                    Content = x.ChapterContent,
                    Title = x.Title,
                    SortID = x.SortID,
                };
            }
        }
        public int SaveChapter(ChapterVM x)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var model = new tbl_Chapter();
                if (x.ChapterID > 0)
                    model = db.tbl_Chapter.Find(x.ChapterID);
                model.Course_Id = x.Course_Id;
                model.ChapterID = x.ChapterID;
                model.ChapterContent = x.Content;
                model.Title = x.Title;
                model.SortID = x.SortID;
                if (x.ChapterID == 0)
                    db.tbl_Chapter.Add(model);
                db.SaveChanges();
                return model.ChapterID;
            }
        }

        public bool DeleteChapter(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var v = db.tbl_Chapter.Find(id);
                    db.tbl_Chapter.Remove(v);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public bool CopyVideos(int sectionId, int? SectionSubID, string videoIds)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var ids = videoIds.Split(',')
                                      .Select(id => int.TryParse(id.Trim(), out var vid) ? vid : (int?)null)
                                      .Where(id => id.HasValue)
                                      .Select(id => id.Value)
                                      .ToList();

                    if (!ids.Any())
                    {
                        return false;
                    }

                    var vids = db.tbl_Video.Where(v => ids.Contains(v.Video_Id)).ToList();
                    var sec = db.tbl_Section.Find(sectionId);
                    if (!vids.Any())
                    {
                        return false;
                    }

                    foreach (var video in vids)
                    {
                        var newVideo = new tbl_Video
                        {
                            Video_Name = video.Video_Name,
                            Video_ShortDescription = video.Video_ShortDescription,
                            Video_Length = video.Video_Length,
                            Video_Path = video.Video_Path,
                            Section_Fid = sectionId,
                            Course_Fid = sec.Course_Fid,
                            SectionSub_Fid = SectionSubID,
                            IsActive = video.IsActive,
                            Type = video.Type,
                            SortID = video.SortID,
                            Video_Tags = video.Video_Tags,
                            Difficulty = video.Difficulty
                        };

                        db.tbl_Video.Add(newVideo);
                    }

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SaveSorting(SortingVM d)
        {
            int sr = 0;
            using (var db = new FAMEEntities())
            {
                if (d.Type == "Section")
                {
                    var l = db.tbl_Section.Where(x => d.Ids.Contains(x.Section_ID)).ToList();
                    foreach (var i in d.Ids)
                    {
                        sr++;
                        var g = l.FirstOrDefault(x => x.Section_ID == i);
                        if (g != null)
                        {
                            g.SortID = sr;
                            db.Entry(g).State = EntityState.Modified;
                        }
                    }
                }
                else
                {
                    var l = db.tbl_Video.Where(x => d.Ids.Contains(x.Video_Id)).ToList();
                    foreach (var i in d.Ids.Where(x => x != null))
                    {
                        sr++;
                        var g = l.FirstOrDefault(x => x.Video_Id == i);
                        if (g != null)
                        {
                            g.SortID = sr;
                            db.Entry(g).State = EntityState.Modified;
                        }
                    }
                }
                db.SaveChanges();
                return true;
            }
        }

        #endregion
    }
}