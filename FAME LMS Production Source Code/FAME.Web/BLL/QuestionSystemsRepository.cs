using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.BLL
{
    public class QuestionSystemsRepository : IQuestionSystemsRepository
    {
        public List<QuestionSystemVM> GetList()
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_QuestionSystem
                    .Select(x => new QuestionSystemVM
                    {
                        ID = x.ID,
                        Name = x.Name,
                        Section = x.Section,
                        IsActive = x.IsActive ?? false
                    })
                    .ToList();
            }
        }


        public bool Delete(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var v = db.tbl_QuestionSystem.Find(id);
                    db.tbl_QuestionSystem.Remove(v);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public QuestionSystemVM Save(QuestionSystemVM d)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_QuestionSystem model = new tbl_QuestionSystem();
                if (d.ID > 0)
                {
                    model = db.tbl_QuestionSystem.Find(d.ID);
                }

                model.ID = d.ID;
                model.Name = d.Name;
                model.Section = d.Section;
                model.IsActive = d.IsActive;

                if (d.ID == 0)
                {
                    model.IsActive = true;
                    db.tbl_QuestionSystem.Add(model);
                }
                db.SaveChanges();
                d.ID = model.ID;
                return d;
            }
        }

        public QuestionSystemVM GetByID(int? id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_QuestionSystem model = db.tbl_QuestionSystem.Find(id);

                QuestionSystemVM s = new QuestionSystemVM()
                {
                    ID = model.ID,
                    Name = model.Name,
                    Section = model.Section,
                    IsActive = model.IsActive ?? false
                };
                return s;
            }
        }
    }
}