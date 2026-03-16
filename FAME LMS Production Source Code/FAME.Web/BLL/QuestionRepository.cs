using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    public class QuestionRepository : IQuestionRepository
    {


        public List<QuestionVM> GetQuestionsBySection(int SectionID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                List<tbl_Question> list = db.tbl_Question.Where(x => x.Section_Fid == SectionID).ToList();
                List<QuestionVM> slist = list.ConvertAll(x => new QuestionVM
                {
                    Question_Id = x.Question_Id,
                    Type = x.Type,
                    Answer = x.Answer,
                    Course_Fid = x.Course_Fid,
                    Question = x.Question,
                    Section_Fid = x.Section_Fid,
                    Tags = x.Tags,
                    DifficultyLevel = x.DifficultyLevel,
                    Marks = x.Marks,
                    AnsExplain = x.AnsExplain,
                });
                return slist;
            }
        }
        public List<sp_lstQuestions_Result> GetList(QuestionType type, int PartitionID, string tags, string Text, string CreatedBY = "", int SystemID = 0)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                int Type = (int)type;
                return db.sp_lstQuestions(Type, PartitionID, SystemID, tags, Text, CreatedBY).ToList();
            }
        }
        public List<sp_lstVideoFaqs_Result> GetList(int VideoID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                // CategoryID is used As VideoID for QuestionType.FAQVideo
                return db.sp_lstVideoFaqs(VideoID).ToList();
            }
        }


        public bool DeleteQuestion(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var v = db.tbl_Question.Find(id);
                    db.tbl_Question.Remove(v);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public QuestionVM SaveQuestion(QuestionVM Question)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    tbl_Question model = new tbl_Question();
                    if (Question.Question_Id > 0)
                    {
                        model = db.tbl_Question.Find(Question.Question_Id);
                        db.tbl_Options.RemoveRange(model.tbl_Options);
                    }

                    model.Question_Id = Question.Question_Id;
                    model.Question = Question.Question;
                    model.Answer = Question.Answer;
                    model.CategoryID = Question.CategoryID;
                    model.IsPopular = Question.IsPopular;
                    model.Tags = Question.Tags;
                    model.DifficultyLevel = Question.DifficultyLevel;
                    model.AnsExplain = Question.AnsExplain;
                    model.Section_Fid = Question.Section_Fid;
                    model.Course_Fid = Question.Course_Fid;
                    model.Type = Question.Type;
                    model.Marks = Question.Marks;
                    model.IsMultiAns = Question.IsMultiAns;
                    model.VideoID = Question.VideoID;
                    model.SystemID = Question.SystemID;
                    model.PartitionID = Question.PartitionID;

                    if (Question.Options != null)
                        model.tbl_Options = Question.Options.ConvertAll(x => new tbl_Options
                        {
                            OptionText = x.OptionText,
                            IsCorrect = x.IsCorrect,
                            OptionDetail = x.Details,
                        });
                    if (model.CreatedBy == null) model.CreatedBy = Question.CreatedBy;
                    if (!(Question.Question_Id > 0))
                    {
                        db.tbl_Question.Add(model);
                    }
                    db.SaveChanges();

                    if (Question.PaperID > 0)
                    {
                        if (!db.tbl_QuestionPaperDetail.Any(x => x.PaperID == Question.PaperID && x.QuestionID == model.Question_Id))
                        {
                            db.tbl_QuestionPaperDetail.Add(new tbl_QuestionPaperDetail()
                            {
                                PaperID = Question.PaperID,
                                QuestionID = model.Question_Id,
                                Type = Question.Type,
                            });
                            db.SaveChanges();
                        }
                    }
                    Question.Question_Id = model.Question_Id;
                    return Question;
                }
                catch
                {
                    return new QuestionVM
                    {
                        Section_Fid = Question.Section_Fid,
                        Course_Fid = Question.Course_Fid
                    };
                }
            }
        }


        public bool SaveQuestions(List<QuestionVM> Questions)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                List<tbl_Question> model = Questions.ConvertAll(Question => new tbl_Question
                {
                    Question = Question.Question,
                    Answer = Question.Answer,
                    IsPopular = Question.IsPopular,
                    Tags = Question.Tags,
                    DifficultyLevel = Question.DifficultyLevel,
                    AnsExplain = Question.AnsExplain,
                    Type = Question.Type,
                    Marks = Question.Marks,
                    IsMultiAns = Question.IsMultiAns,
                    SystemID = Question.SystemID,
                    PartitionID = Question.PartitionID,
                    tbl_Options = Question.Options.ConvertAll(x => new tbl_Options
                    {
                        OptionText = x.OptionText,
                        IsCorrect = x.IsCorrect,
                        OptionDetail = x.Details,
                    })
                });
                db.tbl_Question.AddRange(model);
                db.SaveChanges();
                return true;
            }
        }

        public List<QuestionVM> AllMcqs(int? Type)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var list = db.tbl_Question.Where(x => x.Type == Type).ToList();

                    var s = list.ConvertAll(model => new QuestionVM()
                    {
                        Question_Id = model.Question_Id,
                        Options = model.tbl_Options.ToList().ConvertAll(x => new OptionVM
                        {
                            Details = x.OptionDetail,
                            IsCorrect = x.IsCorrect ?? false,
                            OptionText = x.OptionText
                        }).ToList(),
                        Type = model.Type,
                        Answer = model.Answer,
                        Course_Fid = model.Course_Fid,
                        Question = model.Question,
                        Section_Fid = model.Section_Fid,
                        Tags = model.Tags,
                        AnsExplain = model.AnsExplain,
                        DifficultyLevel = model.DifficultyLevel,
                        Marks = model.Marks,
                    });


                    return s;
                }
                catch
                {
                    return null;
                }
            }
        }
        public QuestionVM GetByID(int? id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_Question model = db.tbl_Question.Find(id);

                QuestionVM s = new QuestionVM()
                {
                    Question_Id = model.Question_Id,
                    Options = model.tbl_Options.ToList().ConvertAll(x => new OptionVM
                    {
                        Details = x.OptionDetail,
                        IsCorrect = x.IsCorrect ?? false,
                        OptionText = x.OptionText
                    }).ToList(),
                    Type = model.Type,
                    Answer = model.Answer,
                    Course_Fid = model.Course_Fid,
                    Question = model.Question,
                    Section_Fid = model.Section_Fid,
                    Tags = model.Tags,
                    AnsExplain = model.AnsExplain,
                    DifficultyLevel = model.DifficultyLevel,
                    Marks = model.Marks,
                    CategoryID = model.CategoryID,
                    IsPopular = model.IsPopular ?? false,
                    IsMultiAns = model.IsMultiAns ?? false,
                    SystemID = model.SystemID,
                    PartitionID = model.PartitionID,
                };


                return s;
            }
        }
        public int GetCourseFid(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    tbl_Section model = db.tbl_Section.Find(id);
                    return model.Course_Fid;
                }
                catch
                {
                    return 0;
                }
            }
        }

        #region Comment / Notes / Tags
        public bool PostComment(tbl_Comment comment)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    db.tbl_Comment.Add(comment);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public sp_GetReview_Result SaveReview(int ID, string Type, string UserID, int Stars)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    db.tbl_Ratings.Add(new tbl_Ratings
                    {
                        ObjectID = ID,
                        ObjectType = Type,
                        Stars = Stars,
                        UserID = UserID
                    });
                    db.SaveChanges();
                    return db.sp_GetReview(ID, Type, "").FirstOrDefault();
                }
                catch
                {
                    return null;
                }
            }
        }
        public sp_GetReview_Result GetReview(int ID, string Type, string UserID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.sp_GetReview(ID, Type, UserID).FirstOrDefault() ?? new sp_GetReview_Result();
            }
        }
        public bool SaveNotesTags(tbl_Notes comment)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var model = db.tbl_Notes.FirstOrDefault(x => x.FID == comment.FID && x.StudentID == comment.StudentID && x.Type == comment.Type);
                    if (model != null)
                        db.tbl_Notes.Remove(model);
                    db.tbl_Notes.Add(comment);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }
        public Tuple<string, string> GetNotesTags(int QuestionID, string Uid)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var notes = db.tbl_Notes.FirstOrDefault(x => x.FID == QuestionID && x.StudentID == Uid && x.Type == NotesType.Notes.ToString())?.Notes;
                var tags = db.tbl_Notes.FirstOrDefault(x => x.FID == QuestionID && x.StudentID == Uid && x.Type == NotesType.Tags.ToString())?.Notes;
                return new Tuple<string, string>(notes, tags);
            }
        }
        public List<sp_lstComments_Result> GetComments(int QuestionID = 0, int VideoID = 0, bool? isApproved = null)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var list = db.sp_lstComments(QuestionID, VideoID, isApproved).ToList();
                return list;
            }
        }
        public List<sp_lstApproveComments_Result> GetApproveComments(string isApproved = "All")
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var list = db.sp_lstApproveComments(isApproved, "").ToList();
                return list;
            }
        }
        public bool ApproveComments(int ID, string ApprovedBy, bool IsApproved)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var cmnt = db.tbl_Comment.Find(ID);
                cmnt.IsApproved = IsApproved;
                cmnt.ApprovedBy = ApprovedBy;
                cmnt.ApprovedDT = Common.GetCurrentDate();
                db.SaveChanges();
                return true;
            }
        }
        #endregion
    }
}