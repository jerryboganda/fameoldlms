using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Linq;

using First_Aid_Made_Easy.BLL.Interfaces;

namespace First_Aid_Made_Easy.BLL
{
    public class StudentGroupRepository : IStudentGroupRepository
    {
        #region Question Paper

        public List<StudentGroupVM> GetList(int UniID = 0)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.tbl_StudentGroup.Where(x => UniID == 0 || x.UniversityID == UniID).ToList().ConvertAll(model => new StudentGroupVM
                {
                    Description = model.Description,
                    GroupTitle = model.GroupTitle,
                    Id = model.Id,
                    CreatedBy = model.CreatedBy,
                    Pic = model.Pic,
                    UniversityID = model.UniversityID,
                    UniversityName = Common.Universities.FirstOrDefault(x => x.Value == model.UniversityID)?.Text
                });
            }
        }
        public bool DeleteStudentGroup(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var v = db.tbl_StudentGroup.Find(id);
                    db.tbl_StudentGroup.Remove(v);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public StudentGroupVM Save(StudentGroupVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_StudentGroup table = new tbl_StudentGroup();
                if (model.Id > 0)
                {
                    table = db.tbl_StudentGroup.Find(model.Id);
                    db.tbl_StudentGroupDetail.RemoveRange(table.tbl_StudentGroupDetail);
                }

                table.Description = model.Description;
                table.GroupTitle = model.GroupTitle;
                table.UniversityID = model.UniversityID;
                if (model.Detail != null)
                    table.tbl_StudentGroupDetail = model.Detail.ConvertAll(x => new tbl_StudentGroupDetail
                    {
                        StudentID = x.StudentID,
                        IsLeader = x.IsLeader
                    });
                if (!(model.Id > 0))
                {
                    table.CreatedBy = model.CreatedBy;
                    db.tbl_StudentGroup.Add(table);
                }
                db.SaveChanges();
                model.Id = table.Id;
                return model;
            }
        }

        public StudentGroupVM GetByID(int? id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_StudentGroup model = db.tbl_StudentGroup.Find(id);
                StudentGroupVM s = new StudentGroupVM()
                {
                    Id = model.Id,
                    Detail = model.tbl_StudentGroupDetail.Select(
                        x => new StudentGroupDetailVM
                        {
                            StudentID = x.StudentID,
                            IsLeader = x.IsLeader,
                            StudentEmail = x.AspNetUsers?.Email,
                            StudentName = x.AspNetUsers?.tbl_User.FirstOrDefault()?.User_Name
                        }).ToList(),
                    Description = model.Description,
                    GroupTitle = model.GroupTitle,
                    UniversityID = model.UniversityID,
                };
                return s;
            }
        }

        public bool SavePic(string FileName, int ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var qp = db.tbl_StudentGroup.Find(ID);
                qp.Pic = FileName;
                db.SaveChanges();
                return true;
            }
        }
        #endregion
    }
}