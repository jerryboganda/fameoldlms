using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;

namespace First_Aid_Made_Easy.BLL
{
    public class SectionSubRepository : ISectionSubRepository
    {
        public bool DeleteSection(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var sec = db.tbl_SectionSub.Find(id);
                sec.IsActive = false;
                db.SaveChanges();
                return true;
            }
        }

        public int SaveSection(SectionSubVM section)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    tbl_SectionSub model = new tbl_SectionSub();
                    if (section.ID > 0)
                    {
                        model = db.tbl_SectionSub.Find(section.ID);
                    }
                    model.ID = section.ID;
                    model.SectionSub_Name = section.SectionSub_Name;
                    model.Course_Fid = section.Course_Fid;
                    model.Section_Fid = section.Section_Fid;
                    if (!(section.ID > 0))
                    {
                        db.tbl_SectionSub.Add(model);
                    }
                    db.SaveChanges();
                    section.ID = model.ID;
                    return section.ID;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public SectionSubVM GetByID(int? id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    tbl_SectionSub model = db.tbl_SectionSub.Find(id);
                    SectionSubVM s = new SectionSubVM()
                    {
                        ID = model.ID,
                        SectionSub_Name = model.SectionSub_Name,
                        Course_Fid = model.Course_Fid,
                        Section_Fid = model.Section_Fid,
                        SortID = model.SortID,
                    };
                    return s;
                }
                catch
                {
                    return null;
                }
            }
        }
        public List<SelectListItem> List(string id = "", bool isAdmin = true)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var model = db.tbl_SectionSub.Where(x => x.tbl_Courses.TeacherFid == id || isAdmin).OrderBy(x => x.SectionSub_Name).ToList();

                    return model.ConvertAll(x => new SelectListItem
                    {
                        Text = x.SectionSub_Name + " => " + x.tbl_Courses.Course_Name,
                        Value = x.ID.ToString()
                    });
                }
                catch
                {
                    return null;
                }
            }
        }

    }
}