using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    public class GuidelineVideoRepository
    {
        public List<GuidelineVideoVM> GetList(int? packageId = null)
        {
            List<GuidelineVideoVM> result = new List<GuidelineVideoVM>();
            using (GuidelineContext db = new GuidelineContext())
            {
                var query = db.tbl_GuidelineVideo.AsQueryable();

                if (packageId.HasValue)
                {
                    query = query.Where(x => x.PackageID == packageId.Value);
                }

                var entities = query
                    .Where(x => x.IsActive ?? true)
                    .OrderBy(x => x.SortOrder)
                    .ToList();

                // Get package names manually to avoid EDMX usage for this table
                using (var fameDb = new FAMEEntities())
                {
                    var packageIds = entities.Select(x => x.PackageID).Distinct().ToList();
                    var packages = fameDb.tbl_Package
                        .Where(p => packageIds.Contains(p.PackageID))
                        .ToDictionary(p => p.PackageID, p => p.PackageName);

                    result = entities.Select(x => new GuidelineVideoVM
                    {
                        GuidelineVideoID = x.GuidelineVideoID,
                        PackageID = x.PackageID,
                        VideoTitle = x.VideoTitle,
                        VideoDescription = x.VideoDescription,
                        VideoPath = x.VideoPath,
                        SortOrder = x.SortOrder ?? 0,
                        IsActive = x.IsActive ?? true,
                        CreatedDT = x.CreatedDT,
                        PackageName = packages.ContainsKey(x.PackageID) ? packages[x.PackageID] : "Unknown"
                    }).ToList();
                }
            }
            return result;
        }

        public List<GuidelineVideoVM> GetStudentVideos(string studentId)
        {
            using (GuidelineContext db = new GuidelineContext())
            {
                List<int> packageIds = new List<int>();
                Dictionary<int, string> packageNames = new Dictionary<int, string>();

                // Get current active package IDs for the student using main context
                using (var fameDb = new FAMEEntities())
                {
                    var currentDate = Common.GetCurrentDate();
                    var packages = fameDb.tbl_EnrollmentMaster
                        .Where(e => e.StudentFid == studentId && 
                                   e.IsExpired != true && 
                                   e.Enrollment_EndDate >= currentDate &&
                                   e.PackageId != null)
                        .Select(e => new { e.PackageId.Value, e.tbl_Package.PackageName })
                        .Distinct()
                        .ToList();
                    
                    packageIds = packages.Select(p => p.Value).ToList();
                    packageNames = packages.ToDictionary(p => p.Value, p => p.PackageName);
                }

                if (!packageIds.Any())
                    return new List<GuidelineVideoVM>();

                var videos = db.tbl_GuidelineVideo
                    .Where(x => packageIds.Contains(x.PackageID) && (x.IsActive ?? true))
                    .OrderBy(x => x.PackageID).ThenBy(x => x.SortOrder)
                    .ToList();

                return videos.Select(x => new GuidelineVideoVM
                    {
                        GuidelineVideoID = x.GuidelineVideoID,
                        PackageID = x.PackageID,
                        VideoTitle = x.VideoTitle,
                        VideoDescription = x.VideoDescription,
                        VideoPath = x.VideoPath,
                        PackageName = packageNames.ContainsKey(x.PackageID) ? packageNames[x.PackageID] : ""
                    })
                    .ToList();
            }
        }

        public GuidelineVideoVM GetByID(int id)
        {
            using (GuidelineContext db = new GuidelineContext())
            {
                var video = db.tbl_GuidelineVideo.Find(id);
                if (video == null) return null;

                string packageName = "";
                using(var fameDb = new FAMEEntities()) 
                {
                     packageName = fameDb.tbl_Package.Where(p => p.PackageID == video.PackageID).Select(p => p.PackageName).FirstOrDefault();
                }

                return new GuidelineVideoVM
                {
                    GuidelineVideoID = video.GuidelineVideoID,
                    PackageID = video.PackageID,
                    VideoTitle = video.VideoTitle,
                    VideoDescription = video.VideoDescription,
                    VideoPath = video.VideoPath,
                    SortOrder = video.SortOrder ?? 0,
                    IsActive = video.IsActive ?? true,
                    PackageName = packageName
                };
            }
        }

        public int Save(GuidelineVideoVM model)
        {
            using (GuidelineContext db = new GuidelineContext())
            {
                tbl_GuidelineVideo entity;

                if (model.GuidelineVideoID > 0)
                {
                    entity = db.tbl_GuidelineVideo.Find(model.GuidelineVideoID);
                    if (entity == null) return 0;
                }
                else
                {
                    entity = new tbl_GuidelineVideo { CreatedDT = Common.GetCurrentDate() };
                    db.tbl_GuidelineVideo.Add(entity);
                }

                entity.PackageID = model.PackageID;
                entity.VideoTitle = model.VideoTitle;
                entity.VideoDescription = model.VideoDescription;
                entity.VideoPath = model.VideoPath;
                entity.SortOrder = model.SortOrder;
                entity.IsActive = model.IsActive;

                db.SaveChanges();
                return entity.GuidelineVideoID;
            }
        }

        public bool Delete(int id)
        {
            using (GuidelineContext db = new GuidelineContext())
            {
                try
                {
                    var video = db.tbl_GuidelineVideo.Find(id);
                    if (video == null) return false;

                    db.tbl_GuidelineVideo.Remove(video);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
