using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.BLL
{
    public class NotificationRepository : INotificationRepository
    {
        public bool Create(StudentNotifVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_StudentNotif table = new tbl_StudentNotif();
                if (model.ID > 0)
                {
                    table = db.tbl_StudentNotif.Find(model.ID);
                    if (table.Status != "Pending")
                    {
                        return true;
                    }
                }
                table.MessageTitle = model.MessageTitle;
                table.MaessageBody = model.MaessageBody;
                table.Description = model.Description;
                table.PackageIds = model.PackageIds;
                table.UniversityIds = model.UniversityIds;
                table.IsActive = model.IsActive;
                table.IsPopup = model.IsPopup;
                table.IsPush = model.IsPush;
                table.SendAt = model.SendAt;
                if (!string.IsNullOrEmpty(model.PicturePath))
                    table.PicturePath = model.PicturePath;
                if (!string.IsNullOrEmpty(model.Banner))
                    table.Banner = model.Banner;

                if (model.ID == 0)
                {
                    table.Status = "Pending";
                    table.CreatedDT = Common.GetCurrentDate();
                    table.CreatedBy = model.CreatedBy;
                    db.tbl_StudentNotif.Add(table);
                }
                db.SaveChanges();
                return true;
            }
        }

        public object Delete(int id)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var a = db.tbl_StudentNotif.Find(id);
                    if (a.Status != "Pending")
                    {
                        return false;
                    }
                    db.tbl_StudentNotif.Remove(a);
                    db.SaveChanges();
                    return true;
                }
            }
            catch { return false; }
        }

        public List<sp_GetNotifications_Result> GetNotifications(string id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.sp_GetNotifications(id, null).OrderByDescending(x => x.CreatedDT).ToList();
            }
        }

        public List<sp_GetNotifications_Result> CheckForNotification(string id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.sp_GetNotifications(id, true).ToList();
            }
        }

        public bool HaveSeen(string Sid, int NId)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    if (!db.tbl_SeenNotif.Any(y => NId == y.NotifID && Sid == y.StudentID))
                    {
                        db.tbl_SeenNotif.Add(new tbl_SeenNotif
                        {
                            NotifID = NId,
                            StudentID = Sid
                        });
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch { return false; }
        }

        public bool HaveSeen(string Sid, string NIds)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var ns = NIds.Split(',').ToList().ConvertAll(x => new tbl_SeenNotif { StudentID = Sid, NotifID = Convert.ToInt32(x), });

                foreach (var x in ns)
                {
                    if (!db.tbl_SeenNotif.Any(y => x.NotifID == y.NotifID && x.StudentID == y.StudentID))
                    {
                        db.tbl_SeenNotif.Add(x);
                    }
                }
                db.SaveChanges();
                return true;
            }
        }

        public StudentNotifVM GetMessage()
        {

            using (FAMEEntities db = new FAMEEntities())
            {
                var id = db.tbl_StudentNotif.FirstOrDefault(x => x.CreatedBy == "Welcome")?.ID ?? 0;
                if (id == 0) return new StudentNotifVM();
                return GetByID(id);
            }
        }
        public StudentNotifVM GetByID(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var table = db.tbl_StudentNotif.Find(id);

                StudentNotifVM model = new StudentNotifVM()
                {
                    ID = table.ID,
                    MessageTitle = table.MessageTitle,
                    MaessageBody = table.MaessageBody,
                    PicturePath = table.PicturePath,
                    Banner = table.Banner,
                    Status = table.Status,
                    Description = table.Description,
                    SendAt = table.SendAt,
                    UniversityIds = table.UniversityIds,
                    PackageIds = table.PackageIds,
                    IsActive = table.IsActive ?? false,
                    IsPush = table.IsPush ?? false,
                    IsPopup = table.IsPopup ?? false
                };
                return model;
            }
        }
        public List<StudentNotifVM> GetList()
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    List<tbl_StudentNotif> list = new List<tbl_StudentNotif>();
                    list = db.tbl_StudentNotif.Where(x => x.CreatedBy != "Welcome").OrderByDescending(x => x.CreatedDT).ToList();

                    List<StudentNotifVM> List = list.ConvertAll(x => new StudentNotifVM()
                    {
                        Description = x.Description,
                        MaessageBody = x.MaessageBody,
                        SendAt = x.SendAt,
                        MessageTitle = x.MessageTitle,
                        Status = x.Status,
                        ID = x.ID,
                        IsActive = x.IsActive ?? false,
                        IsPush = x.IsPush ?? false,
                        IsPopup = x.IsPopup ?? false
                    });
                    return List;
                }
            }
            catch
            {
                throw;
            }
        }

        public tbl_StudentNotif IsNotificationTrue(int id, string secret, bool isCourse)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var DT = Common.GetCurrentDate();
                    var pre = PredicateBuilder.New<tbl_StudentNotif>();
                    var model = db.tbl_StudentNotif.Where(pre.Compile()).FirstOrDefault();
                    return model;
                }
            }
            catch
            {
                return null;
            }
        }


    }
}