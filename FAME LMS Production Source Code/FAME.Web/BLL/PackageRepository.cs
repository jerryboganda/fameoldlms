using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using First_Aid_Made_Easy.Areas.FileManager.Controllers;
using First_Aid_Made_Easy.Areas.FileManager.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.BLL
{
    public class PackageRepository : IPackageRepository
    {
        public bool Create(PackageVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_Package table = new tbl_Package();
                if (model.PackageID > 0)
                {
                    table = db.tbl_Package.Find(model.PackageID);
                    var list = table.tbl_PackageDetail.ToList();
                    db.tbl_PackageDetail.RemoveRange(list);
                }
                table.PackageID = model.PackageID;
                table.PackageName = model.PackageName;
                table.SortID = model.SortID ?? 99;
                table.PackageDescription = model.PackageDescription;
                table.PackagePrice = model.PackagePrice;
                table.Duration = model.Duration;
                table.HasType = model.HasType;
                if (model.PackageID == 0) // New package — ensure it's active
                    table.IsActive = true;

                // MainController fm = new MainController(); // Removed - not used

                if (model.HasType == true)
                {
                    table.tbl_PackageDetail = model.Detail?.ConvertAll(x => new tbl_PackageDetail()
                    {
                        PackageID = table.PackageID,
                        CourseID = x.CourseID,
                        Paper = x.Paper,
                        Type = x.Type
                    });
                }
                else
                {
                    table.tbl_PackageDetail = model.CourseIds?.ToList().ConvertAll(x => new tbl_PackageDetail()
                    {
                        PackageID = table.PackageID,
                        CourseID = Convert.ToInt32(x),
                    });
                }
                if (model.PackageID == 0)
                {
                    table.CreatedDT = Common.GetCurrentDate();
                    table.CreatedBy = model.CreatedBy;
                    db.tbl_Package.Add(table);
                }
                db.SaveChanges();
                // _ = fm.CreateForPackage(HttpContext.Current.Server, model.PackageName.Trim(), table.PackageID); // Temporarily disabled - needs refactoring
                return true;
            }
        }

        public PackageDurationVM GetDuration(int? id)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var d = db.tbl_PackageDuration.Where(x => x.ID == id).FirstOrDefault();
                    PackageDurationVM model = new PackageDurationVM
                    {
                        ID = d.ID,
                        Duration = d.Duration,
                        DurationS = d.Duration.ToString(),
                        PackageID = d.PackageID,
                        Price = d.Price
                    };
                    return model;
                }
            }
            catch { return null; }
        }
        public bool SaveDuration(PackageDurationVM model)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {

                    tbl_PackageDuration table = new tbl_PackageDuration();
                    if (model.ID > 0)
                    {
                        table = db.tbl_PackageDuration.Where(x => x.ID == model.ID).FirstOrDefault();
                    }

                    table.ID = model.ID;
                    table.Duration = model.Duration;
                    table.Price = model.Price;
                    table.PackageID = model.PackageID;
                    if (model.ID == 0)
                    {
                        db.tbl_PackageDuration.Add(table);
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch { return false; }
        }

        public InstallmentVM CheckInstallments(int id)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var pack = db.tbl_Installments.Where(x => x.PackageDurationID == id).ToList();
                    InstallmentVM model = new InstallmentVM
                    { List = new List<InstallmentVM>() };
                    int? numbr = 0, TotalDuration = 0;
                    foreach (var x in pack.OrderBy(x => x.Duration))
                    {
                        numbr++;
                        TotalDuration += x.Duration;
                        model.List.Add(new InstallmentVM
                        {
                            DurationS = getInstText(x.Duration, numbr, x.InstallmentPrice, TotalDuration)
                        });
                    }
                    return model;
                }
            }
            catch { return null; }
        }

        public bool SaveInstallments(InstallmentVM model)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    List<tbl_Installments> List = new List<tbl_Installments>();
                    List = db.tbl_Installments.Where(x => x.PackageDurationID == model.PackageDurationID).ToList();
                    db.tbl_Installments.RemoveRange(List);

                    List = model.List.ConvertAll(x => new tbl_Installments()
                    {
                        PackageDurationID = model.PackageDurationID,
                        InstallmentPrice = x.InstallmentPrice,
                        Duration = x.Duration,
                    });

                    db.tbl_Installments.AddRange(List);
                    db.SaveChanges();
                    return true;
                }
            }
            catch { return false; }
        }

        public InstallmentVM GetInstallments(int id)
        {

            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var list = db.tbl_Installments.Where(x => x.PackageDurationID == id).ToList();
                    var PackDur = db.tbl_PackageDuration.Find(id);
                    var VM = new InstallmentVM
                    {
                        PackageID = PackDur.PackageID,
                        PackageDurationID = PackDur.ID,
                        List = list.ConvertAll(x => new InstallmentVM()
                        {
                            Duration = x.Duration,
                            PackageDurationID = x.PackageDurationID,
                            InstallmentID = x.InstallmentID,
                            InstallmentPrice = x.InstallmentPrice,
                            DurationS = x.Duration.ToString()
                        }),
                        PackageDuration = new PackageDurationVM()
                        {
                            Price = PackDur.Price,
                            Duration = PackDur.Duration,
                            DurationS = getText(PackDur.Duration).Replace("--- Rs.", ""),
                        }
                    };
                    return VM;
                }
            }
            catch { return new InstallmentVM(); }
        }

        public object Deactivate(int id)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var pack = db.tbl_Package.Find(id);
                    pack.IsActive = false;
                    db.Entry(pack).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
            }
            catch { return false; }
        }

        public List<PackageDurationVM> getDurations(int id)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    return db.tbl_PackageDuration.Where(x => x.PackageID == id).ToList().ConvertAll(x => new PackageDurationVM()
                    {
                        ID = x.ID,
                        Duration = x.Duration,
                        Price = x.Price
                    });
                }
            }
            catch { return new List<PackageDurationVM>(); }
        }

        public PackageVM GetByID(int id)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var table = db.tbl_Package.Include("tbl_PackageDetail").FirstOrDefault(x => x.PackageID == id);

                    PackageVM model = new PackageVM()
                    {
                        PackageID = table.PackageID,
                        PackageName = table.PackageName,
                        PackageDescription = table.PackageDescription,
                        PackagePrice = table.PackagePrice,
                        Duration = table.Duration ?? 0,
                        SortID = table.SortID,
                        HasType = table.HasType ?? false,
                        Durations = table.tbl_PackageDuration.ToList().ConvertAll(x => new PackageDurationVM()
                        {
                            ID = x.ID,
                            Duration = x.Duration,
                            Price = x.Price,
                            PackageID = x.PackageID,
                            DurationS = x.Duration.ToString(),
                            DurationChar = getText(x.Duration),
                            InstallmentsExist = x.tbl_Installments.Count > 0
                        }),
                        Detail = table.tbl_PackageDetail.ToList().ConvertAll(d => new PackageDetailVM()
                        {
                            CourseName = d.tbl_Courses?.Course_Name,
                            CoursePic = d.tbl_Courses?.Course_Pic,
                            CourseID = d.CourseID,
                            Paper = d.Paper,
                            Type = d.Type,
                        }),
                    };
                    if (string.IsNullOrEmpty(model.EndDate))
                    {
                        model.EndDate = Common.GetCurrentDateForView();
                    }
                    model.CourseIds = table.tbl_PackageDetail.Select(x => x.CourseID.ToString()).ToArray();
                    return model;
                }
            }
            catch
            {
                return null;
            }
        }
        public List<PackageVM> GetList()
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    List<tbl_Package> list = new List<tbl_Package>();
                    list = db.tbl_Package.Where(x => x.IsActive != false).OrderBy(x => x.SortID).ToList();

                    List<PackageVM> List = list.ConvertAll(x => new PackageVM()
                    {
                        PackageName = x.PackageName,
                        PackageID = x.PackageID,
                        PackageDescription = x.PackageDescription,
                        SortID = x.SortID,
                        Courses = x.tbl_PackageDetail.Count(),
                        Detail = x.tbl_PackageDetail.ToList().ConvertAll(d => new PackageDetailVM()
                        {
                            CourseName = d.tbl_Courses?.Course_Name
                        }),
                        PackagePrice = x.tbl_PackageDuration.FirstOrDefault(d => d.Duration == 1)?.Price ?? 0,
                    });
                    return List;
                }
            }
            catch
            {
                throw;
            }
        }
        public List<PackageVM> GetNameList()
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var List = db.tbl_Package.Where(x => x.IsActive != false).OrderBy(x => x.SortID).Select(x => new PackageVM()
                {
                    PackageName = x.PackageName,
                    PackageID = x.PackageID
                }).ToList();
                return List;
            }
        }

        public tbl_Package IsPackageTrue(int id, string secret, bool isCourse)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var DT = Common.GetCurrentDate();
                    var pre = PredicateBuilder.New<tbl_Package>();
                    var model = db.tbl_Package.Where(pre.Compile()).FirstOrDefault();
                    return model;
                }
            }
            catch
            {
                return null;
            }
        }

        public string getText(int? Duration)
        {
            if (Duration == null)
                return "N/A";
            if (Duration == 0)
                return "1st Days --- Rs.";
            else if (Duration < 30)
            {
                return Duration + " Days --- Rs.";
            }
            else if (Duration < 360)
            {
                return (int)(Duration / 30) + " Month --- Rs.";
            }
            else
                return (int)(Duration / 360) + " Year --- Rs.";
        }
        public string getInstText(int? Duration, int? numbr, decimal? Price, int? TotalDuration)
        {
            string intNumb = "";
            switch (numbr)
            {
                case 1: intNumb = "1. First"; break;
                case 2: intNumb = "2. Second"; break;
                case 3: intNumb = "3. Third"; break;
                case 4: intNumb = "4. Fourth"; break;
                case 5: intNumb = "5. Fifth"; break;
                case 6: intNumb = "6. Sixth"; break;
                case 7: intNumb = "7. Seventh"; break;
            }
            intNumb += " Installment of PKR " + string.Format("{0:n0}", Price);

            if (TotalDuration == null)
                return "N/A";
            if (TotalDuration == 0)
                return intNumb + " at the Beginnig";
            else if (TotalDuration < 30)
            {
                return intNumb + " after " + TotalDuration + " Days.";
            }
            else if (TotalDuration < 360)
            {
                return intNumb + " after " + (int)(TotalDuration / 30) + " Month.";
            }
            else
                return intNumb + " after " + (int)(TotalDuration / 360) + " Year.";
        }
    }
}