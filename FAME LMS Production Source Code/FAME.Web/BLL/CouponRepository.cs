using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.BLL
{
    public class CouponRepository : ICouponRepository
    {
        public bool Create(CouponVM model)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    tbl_Coupon table = new tbl_Coupon();
                    if (model.Id > 0)
                    {
                        table = db.tbl_Coupon.Find(model.Id);
                    }
                    table.Id = model.Id;
                    table.IsActive = model.IsActive;
                    table.NoOfUses = model.NoOfUses;
                    table.RemUses = model.NoOfUses;
                    table.SectionID = model.SectionID == 0 ? null : model.SectionID;
                    table.DiscountPer = model.DiscountPer;
                    table.ExpiryDate = Common.StringToDate(model.ExpiryDate);
                    table.CouponSecret = model.CouponSecret;
                    table.ForCourse = model.SectionID == 0;
                    table.CourseFid = model.CourseFid;
                    table.Amount = model.Amount;
                    table.CreatedBy = model.CreatedBy;
                    table.CouponName = model.CouponName;
                    if (model.Id == 0)
                    {
                        table.CreatedDate = Common.GetCurrentDate();
                        db.tbl_Coupon.Add(table);
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                //throw;
                return false;
            }
        }

        public CouponVM GetByID(int id)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var table = db.tbl_Coupon.Find(id);

                    CouponVM model = new CouponVM()
                    {
                        Id = table.Id,
                        IsActive = table.IsActive,
                        NoOfUses = table.NoOfUses,
                        RUses = table.RemUses ?? 0,
                        SectionID = table.SectionID ?? 0,
                        DiscountPer = table.DiscountPer,
                        ExpiryDate = Common.DateToString(table.ExpiryDate),
                        CouponSecret = table.CouponSecret,
                        ForCourse = table.CourseFid != null,
                        CourseFid = table.CourseFid,
                        Amount = table.Amount,
                        CouponName = table.CouponName,
                    };

                    return model;
                }
            }
            catch
            {
                return null;
            }
        }
        public List<CouponVM> GetList(string id)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {

                    var list = db.tbl_Coupon.Where(x => x.CreatedBy == id).OrderByDescending(x => x.CreatedDate).ToList();
                    List<CouponVM> List = list.ConvertAll(x => new CouponVM()
                    {
                        Id = x.Id,
                        IsActive = x.IsActive,
                        NoOfUses = x.NoOfUses,
                        ExpiryDate = Common.DateToString(x.ExpiryDate),
                        CourseFid = x.CourseFid,
                        SectionID = x.SectionID,
                        DiscountPer = x.DiscountPer,
                        CouponName = x.CouponName,
                        RUses = (x.RemUses ?? (x.NoOfUses - x.tbl_EnrollmentMaster.Count() < 0 ? 0 : x.NoOfUses - x.tbl_EnrollmentMaster.Count())),
                        ForName = x.ForCourse ?? false ? "C-" + db.tbl_Courses.Find(x.CourseFid).Course_Name : "S-" + db.tbl_Section.Find(x.SectionID).Section_Name,
                    });
                    return List;
                }
            }
            catch 
            {
                throw;
            }
        }

        public tbl_Coupon IsCouponTrue(int id, string secret, bool isCourse)
        {

            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var DT = Common.GetCurrentDate();
                    var pre = PredicateBuilder.New<tbl_Coupon>();
                    if (isCourse)
                    {
                        pre.And(x => x.CourseFid == id);
                    }
                    if (!isCourse)
                    {
                        pre.And(x => x.SectionID == id);
                    }
                    pre.And(x => x.CouponSecret == secret);
                    pre.And(x => x.IsActive);
                    pre.And(x => x.RemUses > 0);
                    var model = db.tbl_Coupon.Where(pre.Compile()).FirstOrDefault();
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