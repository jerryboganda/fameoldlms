using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using First_Aid_Made_Easy.BLL.Interfaces;

namespace First_Aid_Made_Easy.BLL
{
    public class BookMistakesRepository : IBookMistakesRepository
    {
        public bool Create(tbl_BookMistakes table)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                table.CreatedDT = Common.GetCurrentDate();
                db.tbl_BookMistakes.Add(table);

                db.SaveChanges();
                return true;
            }
        }

        public tbl_BookMistakes GetByID(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_BookMistakes.Find(id);
            }
        }
        public List<tbl_BookMistakes> GetList()
        {
            using (FAMEEntities db = new FAMEEntities())
            {

                return db.tbl_BookMistakes.OrderByDescending(x => x.CreatedDT).ToList();
            }
        }

        public bool Delete(int iD)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var a = db.tbl_BookMistakes.Find(iD);
                db.tbl_BookMistakes.Remove(a);
                db.SaveChanges();
                return true;
            }
        }
    }
}