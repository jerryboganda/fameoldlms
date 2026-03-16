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
    public class FavouritesRepository : IFavouritesRepository
    {
        public bool AddToFavourits(tbl_FavouriteList model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_FavouriteList table = db.tbl_FavouriteList.FirstOrDefault(x => x.Student_Fid == model.Student_Fid && x.Object_ID == model.Object_ID && x.ObjectType == model.ObjectType);
                if (table == null)
                {
                    table = new tbl_FavouriteList
                    {
                        Student_Fid = model.Student_Fid,
                        Object_ID = model.Object_ID,
                        ObjectType = model.ObjectType,
                    };
                    db.tbl_FavouriteList.Add(table);
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }
        public bool RemoveFavourits(tbl_FavouriteList model)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    tbl_FavouriteList table = db.tbl_FavouriteList.FirstOrDefault(x => x.Student_Fid == model.Student_Fid && x.Object_ID == model.Object_ID && x.ObjectType == model.ObjectType);
                    db.tbl_FavouriteList.Remove(table);
                    db.SaveChanges();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public List<sp_lstFavourites_Result> GetList(string StudentID, string Type = "")
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var data = db.sp_lstFavourites(StudentID, Type).ToList();
                return data;
            }
        }
    }
}