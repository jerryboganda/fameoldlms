using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using First_Aid_Made_Easy.BLL.Interfaces;

namespace First_Aid_Made_Easy.BLL
{
    public class TutorialRepository : ITutorialRepository
    {
        public List<TutorialVM> GetList()
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_Tutorial.ToList().ConvertAll(x => new TutorialVM
                {
                    Body = x.Body,
                    VideoLink = x.VideoLink,
                    Title = x.Title,
                    TutorialID = x.TutorialID,
                    CategoryID = x.CategoryID,
                    Category = x.tbl_Master?.Master_Value,
                    IsPopular = x.IsPopular ?? false,
                });
            }
        }


        public bool DeleteTutorial(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var v = db.tbl_Tutorial.Find(id);
                    db.tbl_Tutorial.Remove(v);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public TutorialVM SaveTutorial(TutorialVM Tutorial)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_Tutorial model = new tbl_Tutorial();
                if (Tutorial.TutorialID > 0)
                {
                    model = db.tbl_Tutorial.Find(Tutorial.TutorialID);
                }

                model.TutorialID = Tutorial.TutorialID;
                model.Title = Tutorial.Title;
                model.CategoryID = Tutorial.CategoryID;
                model.VideoLink = Tutorial.VideoLink;
                model.Body = Tutorial.Body;
                model.IsPopular = Tutorial.IsPopular;

                if (Tutorial.TutorialID == 0)
                {
                    model.CreatedBy = Tutorial.CreatedBy;
                    db.tbl_Tutorial.Add(model);
                }
                db.SaveChanges();
                Tutorial.TutorialID = model.TutorialID;
                return Tutorial;
            }
        }

        public TutorialVM GetByID(int? id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_Tutorial model = db.tbl_Tutorial.Find(id);

                TutorialVM s = new TutorialVM()
                {
                    TutorialID = model.TutorialID,
                    Title = model.Title,
                    VideoLink = model.VideoLink,
                    CategoryID = model.CategoryID,
                    Body = model.Body,
                    IsPopular = model.IsPopular ?? false,
                    Category = model.tbl_Master?.Master_Value
                };
                return s;
            }
        }
    }
}