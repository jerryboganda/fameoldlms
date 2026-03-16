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
    public class ExamRepository : IExamRepository
    {
        public List<sp_lstExams_Result> GetList(int difficultyLevel, string tags, string Text = "", string CreatedBY = "")
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.sp_lstExams(difficultyLevel, tags, "", CreatedBY).ToList();
            }
        }


        public bool DeleteExam(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var v = db.tbl_Exam.Find(id);
                    db.tbl_Exam.Remove(v);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public ExamVM Save(ExamVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_Exam table = new tbl_Exam();
                if (model.ExamID > 0)
                {
                    table = db.tbl_Exam.Find(model.ExamID);
                    db.tbl_ExamDetail.RemoveRange(table.tbl_ExamDetail);
                }

                table.ExamID = model.ExamID;
                table.Tags = model.Tags;
                table.DifficultyLevel = model.DifficultyLevel;
                table.Description = model.Description;
                table.ExamTitle = model.ExamTitle;
                table.TopicsCovered = model.TopicsCovered;
                table.CourseIDs = model.CourseIDs;
                table.Price = model.Price;
                table.PackageIDs = model.PackageIDs;
                if (model.Papers != null)
                    table.tbl_ExamDetail = model.Papers.ConvertAll(x => new tbl_ExamDetail { PaperID = x, });
                if (!(model.ExamID > 0))
                {
                    table.CreatedBy = model.CreatedBy;
                    db.tbl_Exam.Add(table);
                }
                db.SaveChanges();
                model.ExamID = table.ExamID;
                return model;
            }
        }

        public bool SavePic(string FileName, int ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var qp = db.tbl_Exam.Find(ID);
                qp.Thumbnail = FileName;
                db.SaveChanges();
                return true;
            }
        }
        public ExamVM GetByID(int? id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_Exam model = db.tbl_Exam.Find(id);
                ExamVM s = new ExamVM()
                {
                    ExamID = model.ExamID,
                    Papers = model.tbl_ExamDetail.Select(x => x.PaperID).ToList(),
                    Tags = model.Tags,
                    DifficultyLevel = model.DifficultyLevel,
                    Description = model.Description,
                    ExamTitle = model.ExamTitle,
                    CourseIDs = model.CourseIDs,
                    PackageIDs = model.PackageIDs,
                    Thumbnail = model.Thumbnail,
                    Price = model.Price,
                    TopicsCovered = model.TopicsCovered,
                };
                s.PaperList = db.tbl_QuestionPaper.Where(x => s.Papers.Contains(x.PaperID)).ToList().ConvertAll(x => new QuestionPaperVM
                {
                    PaperTitle = x.PaperTitle,
                    Marks = x.tbl_QuestionPaperDetail.Sum(y => y.tbl_Question.Marks) ?? 0,
                    DifficultyLevel = x.DifficultyLevel,
                    PaperID = x.PaperID,
                });
                return s;
            }
        }
    }
}