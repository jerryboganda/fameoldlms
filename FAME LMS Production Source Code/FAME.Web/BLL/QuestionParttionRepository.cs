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
    public class QuestionPartitionRepository : IQuestionPartitionRepository
    {
        public List<QuestionSystemVM> GetList()
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_QuestionPartition
                    .Select(x => new QuestionSystemVM
                    {
                        ID = x.ID,
                        Name = x.Name,
                        IsActive = x.IsActive ?? false,
                        IsForStudent = x.IsForStudent ?? false,
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
                    var v = db.tbl_QuestionPartition.Find(id);
                    db.tbl_QuestionPartition.Remove(v);
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
                tbl_QuestionPartition model = new tbl_QuestionPartition();
                if (d.ID > 0)
                {
                    model = db.tbl_QuestionPartition.Find(d.ID);
                }

                model.ID = d.ID;
                model.Name = d.Name;
                model.IsActive = d.IsActive;
                model.IsForStudent = d.IsForStudent;

                if (d.ID == 0)
                {
                    model.IsActive = true;
                    db.tbl_QuestionPartition.Add(model);
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
                tbl_QuestionPartition model = db.tbl_QuestionPartition.Find(id);

                QuestionSystemVM s = new QuestionSystemVM()
                {
                    ID = model.ID,
                    Name = model.Name,
                    IsActive = model.IsActive ?? false,
                    IsForStudent = model.IsForStudent ?? false,
                };
                return s;
            }
        }
    }
}