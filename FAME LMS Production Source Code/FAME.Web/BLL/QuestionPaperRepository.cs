using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;

using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;

namespace First_Aid_Made_Easy.BLL
{
    public class QuestionPaperRepository : IQuestionPaperRepository
    {
        #region Question Paper

        public List<sp_lstQuestionPapers_Result> GetList(string ForStudent = "", bool? mockTest = null)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                //mockTest = false => no mocktest in sp_lstQuestionPapers_Result
                //mockTest = true => mocktest of that student
                //mockTest = null => all question papers in sp_lstQuestionPapers_Result
                return db.sp_lstQuestionPapers(ForStudent, mockTest).OrderByDescending(x => x.StartDT).ToList();
            }
        }

        public bool DeleteQuestionPaper(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                try
                {
                    var v = db.tbl_QuestionPaper.Find(id);
                    db.tbl_QuestionPaper.Remove(v);
                    db.SaveChanges();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public QuestionPaperVM Save(QuestionPaperVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_QuestionPaper table = new tbl_QuestionPaper();
                if (model.PaperID > 0)
                {
                    table = db.tbl_QuestionPaper.Find(model.PaperID);
                    db.tbl_QuestionPaperDetail.RemoveRange(table.tbl_QuestionPaperDetail);
                    db.tbl_ExamDetail.RemoveRange(table.tbl_ExamDetail);
                }

                table.PaperID = model.PaperID;
                table.Tags = model.Tags;
                table.DifficultyLevel = model.DifficultyLevel;
                table.Description = model.Description;
                table.PaperTitle = model.PaperTitle;
                table.CourseIDs = model.CourseIDs;
                table.SectionIDs = model.SectionIDs;
                table.UniversityID = model.UniversityID;
                table.PackageIDs = model.PackageIDs;
                table.Time = model.Time;
                table.PassPer = model.PassPer;
                table.Notes = model.Notes;
                table.Marks = model.Marks;
                table.ExpiryDT = model.ExpiryDT;
                table.StartDT = model.StartDT;
                table.MockTestType = model.MockTestType;
                table.AnswerDT = model.AnswerDT;
                if (model.Questions != null)
                    table.tbl_QuestionPaperDetail = model.Questions.ConvertAll(x => new tbl_QuestionPaperDetail
                    {
                        QuestionID = x.ID,
                        Type = x.Type
                    });
                if (model.ExamIDs != null)
                    table.tbl_ExamDetail = model.ExamIDs.Split(',').ToList().ConvertAll(x => new tbl_ExamDetail
                    {
                        ExamID = Convert.ToInt32(x),
                        PaperID = table.PaperID,
                    });
                if (!(model.PaperID > 0))
                {
                    table.CreatedBy = model.CreatedBy;
                    db.tbl_QuestionPaper.Add(table);
                }
                db.SaveChanges();
                model.PaperID = table.PaperID;
                return model;
            }
        }

        public QuestionPaperVM GetByID(int? id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_QuestionPaper model = db.tbl_QuestionPaper.Find(id);
                QuestionPaperVM s = new QuestionPaperVM()
                {
                    PaperID = model.PaperID,
                    Questions = model.tbl_QuestionPaperDetail.Select(x => new QuestionsVM { ID = x.QuestionID, Type = x.Type }).ToList(),
                    Tags = model.Tags,
                    DifficultyLevel = model.DifficultyLevel,
                    Description = model.Description,
                    PaperTitle = model.PaperTitle,
                    CourseIDs = model.CourseIDs,
                    SectionIDs = model.SectionIDs,
                    UniversityID = model.UniversityID,
                    PackageIDs = model.PackageIDs,
                    Thumbnail = model.Thumbnail,
                    Notes = model.Notes,
                    Marks = model.Marks,
                    Time = model.Time,
                    MockTestType = model.MockTestType,
                    PassPer = model.PassPer,
                    ExpiryTime = string.Format("{0:yyyy-MM-ddTHH:mm}", model.ExpiryDT),
                    StartTime = string.Format("{0:yyyy-MM-ddTHH:mm}", model.StartDT),
                    AnswerTime = string.Format("{0:yyyy-MM-ddTHH:mm}", model.AnswerDT),
                    ExpiryDT = model.ExpiryDT,
                    TopicsCovered = model.TopicsCovered,
                    ExamIDs = string.Join(",", db.tbl_ExamDetail.Where(x => x.PaperID == model.PaperID).Select(x => x.ExamID).ToArray())
                };
                var qs = s.Questions.Select(x => x.ID).ToList();
                s.QuestionList = db.tbl_Question.Where(x => qs.Contains(x.Question_Id)).ToList().ConvertAll(x => new QuestionVM
                {
                    Question_Id = x.Question_Id,
                    Question = x.Question,
                    Marks = x.Marks,
                    IsMultiAns = x.IsMultiAns ?? false,
                    DifficultyLevel = x.DifficultyLevel,
                    Type = x.Type,
                    AnsExplain = x.AnsExplain,
                    Answer = x.Answer,
                    Options = x.tbl_Options.ToList().ConvertAll(o => new OptionVM
                    {
                        OptionText = o.OptionText,
                        Details = o.OptionDetail,
                        IsCorrect = o.IsCorrect ?? false
                    })
                });
                return s;
            }
        }

        public SolvePaperDto GetQuestionToSolve(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                string t = QuestionType.Mcq.ToString();

                var model = db.tbl_ResultMaster
                    .Include("tbl_ResultDetail.tbl_Question.tbl_Options")
                    .Include("tbl_QuestionPaper")
                    .FirstOrDefault(rm => rm.ID == id);

                if (model == null)
                {
                    System.Diagnostics.Trace.TraceWarning($"GetQuestionToSolve: ResultMaster not found. id={id}");
                    return null;
                }

                var questionIds = model.tbl_ResultDetail.Select(rd => rd.QuestionID).ToList();

                var favouriteQuestions = db.tbl_FavouriteList
                    .AsNoTracking()
                    .Where(fl => questionIds.Contains(fl.Object_ID) && fl.Student_Fid == model.StudentID && fl.ObjectType == t)
                    .Select(fl => fl.Object_ID)
                    .ToList();

                var questions = model.tbl_ResultDetail
                    .Select(r => new PaperQuestionDetail
                    {
                        QuestionId = r.QuestionID ?? 0,
                        IsTrue = r.IsTrue,
                        TimeSpent = r.TimeSpent,
                        Answer = r.Answer,
                        Question = r.tbl_Question != null ? r.tbl_Question.Question : "Question Missing",
                        Marks = r.tbl_Question != null ? r.tbl_Question.Marks : 0,
                        AnsExplain = r.tbl_Question != null ? r.tbl_Question.AnsExplain : "",

                        IsPopular = favouriteQuestions.Contains(r.QuestionID),
                        QuestOptions = r.tbl_Question != null ? r.tbl_Question.tbl_Options.Select(o => new OptionVM
                        {
                            OptionText = o.OptionText,
                            Details = o.OptionDetail,
                            IsCorrect = o.IsCorrect ?? false,
                            IsChecked = o.OptionText != null && r.Answer != null && o.OptionText.Trim() == r.Answer.Trim(),
                            // Optimized percentage calculation to avoid deep recursion if possible, or just set to 0 if problematic
                            CheckedByStudentsPerc = 0 
                        }).ToList() : new List<OptionVM>()
                    })
                    .ToList();

                System.Diagnostics.Trace.TraceInformation(
                    $"GetQuestionToSolve: id={id}, resultDetails={model.tbl_ResultDetail.Count}, questions={questions.Count}, obtained={model.ObtainedMarks}, totalMarks={model.TotalMarks}, passPer={model.tbl_QuestionPaper?.PassPer}");

                var s = new SolvePaperDto
                {
                    Datetime = model.Datetime,
                    ID = model.ID,

                    StudentID = model.StudentID,
                    ObtainedMarks = model.ObtainedMarks ?? 0,
                    QuestionPaperID = model.QuestionPaperID,
                    TotalMarks = model.TotalMarks ?? (model.tbl_QuestionPaper != null ? model.tbl_QuestionPaper.Marks : questions.Count()),
                    Mode = model.Mode,
                    TimeGiven = model.TimeGiven,//in min
                    TimeElapsed = model.TimeElapsed,// in sec
                    PaperTitle = !string.IsNullOrEmpty(model.tbl_QuestionPaper?.PaperTitle) ? model.tbl_QuestionPaper.PaperTitle : "Practice Module " + model.ID,
                    AllowReopen = model.AllowReopen ?? false,
                    AnswerAt = model.tbl_QuestionPaper?.AnswerDT ?? model.Datetime, // Use Start date if end date not set
                    IsComplete = model.ObtainedMarks != null,
                    QuestionCount = questions.Count(),
                    Details = questions ?? new List<PaperQuestionDetail>(),
                    PassPer = model.tbl_QuestionPaper?.PassPer ?? 50
                };

                return s;
            }
        }

        public QuestionPaperVM GetByResultID(int id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var model = db.tbl_ResultMaster
                .AsNoTracking()
                .Include(r => r.tbl_QuestionPaper)
                .Include(r => r.tbl_ResultDetail)
                .FirstOrDefault(r => r.ID == id);

                if (model == null) return null;

                string t = QuestionType.Mcq.ToString();

                var questionIds = model.tbl_ResultDetail
                    .Select(rd => rd.QuestionID)
                    .ToList();

                var favouriteSet = new HashSet<int>(
                    db.tbl_FavouriteList
                      .AsNoTracking()
                      .Where(fl =>
                          fl.Student_Fid == model.StudentID &&
                          fl.ObjectType == t &&
                          questionIds.Contains(fl.Object_ID))
                      .Select(fl => fl.Object_ID ?? 0)
                );

                var questions = db.tbl_Question
                    .AsNoTracking()
                    .Where(q => questionIds.Contains(q.Question_Id))
                    .Select(q => new QuestionVM
                    {
                        Question_Id = q.Question_Id,
                        Question = q.Question,
                        Marks = q.Marks,
                        IsMultiAns = q.IsMultiAns ?? false,
                        DifficultyLevel = q.DifficultyLevel,
                        Type = q.Type,
                        AnsExplain = q.AnsExplain,
                        Answer = q.Answer,
                        IsPopular = favouriteSet.Contains(q.Question_Id),
                        Options = q.tbl_Options.Select(o => new OptionVM
                        {
                            OptionText = o.OptionText,
                            Details = o.OptionDetail,
                            IsCorrect = o.IsCorrect ?? false
                        }).ToList()
                    })
                    .ToList();

                return new QuestionPaperVM
                {
                    PaperTitle = model.tbl_QuestionPaper?.PaperTitle,
                    AnswerDT = model.tbl_QuestionPaper?.AnswerDT,
                    PassPer = model.tbl_QuestionPaper?.PassPer ?? 50,
                    QuestionList = questions
                };
            }
        }


        public bool SavePic(string FileName, int ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var qp = db.tbl_QuestionPaper.Find(ID);
                qp.Thumbnail = FileName;
                db.SaveChanges();
                return true;
            }
        }
        #endregion

        #region Result Card

        public ResultVM SaveResult(ResultVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_ResultMaster table = new tbl_ResultMaster();
                if (model.ID > 0)
                {
                    table = db.tbl_ResultMaster.Find(model.ID);
                    db.tbl_ResultDetail.RemoveRange(table.tbl_ResultDetail);
                }

                table.QuestionPaperID = model.QuestionPaperID;

                if (model.Detail != null)
                    table.tbl_ResultDetail = model.Detail.ConvertAll(x => new tbl_ResultDetail
                    {
                        QuestionID = x.QuestionID,
                        Answer = x.Answer,
                        IsTrue = x.IsTrue,
                    });
                if (!(model.ID > 0))
                {
                    table.StudentID = model.StudentID;
                    table.Mode = model.Mode;
                    table.Datetime = Common.GetCurrentDate();
                    db.tbl_ResultMaster.Add(table);
                }
                db.SaveChanges();
                model.ID = table.ID;
                return model;
            }
        }
        public CreateTestInput AutoCreateResult(CreateTestInput model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_ResultMaster table = new tbl_ResultMaster()
                {
                    Mode = model.Mode,
                    StudentID = model.StudentID,
                    TrialStudent = model.TrialStudent,
                    TimeGiven = Convert.ToInt32(Convert.ToDecimal(model.maxQPerBlock) * (decimal)1.5),
                    Datetime = Common.GetCurrentDate(),
                    tbl_ResultDetail = QuestionIDs(db, model.StudentID, model.Systems, model.QuestionMode).Take(model.maxQPerBlock ?? 0).ToList().ConvertAll(x => new tbl_ResultDetail { QuestionID = x }),
                };
                db.tbl_ResultMaster.Add(table);
                db.SaveChanges();
                model.ID = table.ID;
                return model;
            }
        }

        public int AutoCreateResultFromPaper(int ID, string Mode, string StudentID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var res = db.tbl_ResultMaster.FirstOrDefault(x => x.QuestionPaperID == ID && x.StudentID == StudentID && x.ObtainedMarks == null);
                if (res != null) return res.ID;

                var qs = db.tbl_QuestionPaperDetail.Where(x => x.PaperID == ID).Select(x => x.QuestionID).ToList();
                tbl_ResultMaster table = new tbl_ResultMaster()
                {
                    QuestionPaperID = ID,
                    Mode = Mode,
                    StudentID = StudentID,
                    TimeGiven = db.tbl_QuestionPaper.Find(ID)?.Time,
                    Datetime = Common.GetCurrentDate(),
                    tbl_ResultDetail = qs.ConvertAll(x => new tbl_ResultDetail { QuestionID = x }),
                };
                db.tbl_ResultMaster.Add(table);
                db.SaveChanges();
                return table.ID;
            }
        }

        public List<int> QuestionIDs(FAMEEntities db, string StudentID, string SystemIds, string QuestionMode)
        {
            string result = db.sp_QuestionCountByStudent(StudentID, SystemIds).FirstOrDefault(x => x.QType == QuestionMode).QuestionIDs;
            var l = result.Split(',').Where(x => x != "").ToList().ConvertAll(x => Convert.ToInt32(x));
            l.Shuffle();
            return l;
        }
        public List<ResultVM> ResultList(string Uid, bool MockTest = false)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                List<tbl_ResultMaster> res;
                if (MockTest)
                    res = db.tbl_ResultMaster.Where(x => x.StudentID == Uid && x.tbl_QuestionPaper.MockTestType != null).ToList();
                else
                    res = db.tbl_ResultMaster.Where(x => x.StudentID == Uid && x.tbl_QuestionPaper.MockTestType == null).ToList();
                return res.ConvertAll(x => new ResultVM
                {
                    Mode = x.Mode,
                    TotalMarks = x.tbl_QuestionPaper?.Marks,
                    PaperTitle = x.tbl_QuestionPaper?.PaperTitle,
                    AnswerAt = x.tbl_QuestionPaper?.AnswerDT,
                    Datetime = x.Datetime,
                    IsComplete = x.ObtainedMarks != null,
                    ID = x.ID,
                    AllowReopen = x.AllowReopen ?? false,
                    maxQPerBlock = x.tbl_ResultDetail.Count()
                }).OrderByDescending(x => x.Datetime).ToList();
            }
        }
        public List<ResultVM> InCompleteResultList()
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var c = Common.GetCurrentDate();
                var res = db.tbl_ResultMaster.Where(x => x.ObtainedMarks == null && x.QuestionPaperID != null && x.tbl_QuestionPaper.ExpiryDT >= c).ToList();
                return res.ConvertAll(x => new ResultVM
                {
                    Mode = x.Mode,
                    TotalMarks = x.tbl_QuestionPaper?.Marks,
                    PaperTitle = x.tbl_QuestionPaper?.PaperTitle,
                    Datetime = x.Datetime,
                    IsComplete = x.ObtainedMarks != null,
                    ID = x.ID,
                    AllowReopen = x.AllowReopen ?? false,
                    maxQPerBlock = x.tbl_ResultDetail.Count(),
                    StudentID = x.AspNetUsers.UserName,
                });
            }
        }

        public ResultDetailVM SaveResult(ResultDetailVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var a = db.tbl_ResultDetail.FirstOrDefault(x => x.QuestionID == model.QuestionID && x.ResultID == model.ResultID);
                
                // Fix: Check for null before accessing navigation properties
                if (a != null)
                {
                    if (a.tbl_ResultMaster != null)
                    {
                        a.tbl_ResultMaster.TimeElapsed = model.TimeElapsed;
                    }
                    a.Answer = model.Answer;
                    a.IsTrue = model.IsTrue;
                    a.TimeSpent = model.TimeSpent;
                }
                else
                {
                    // If not found, we might need to update the master directly or find it first
                    var master = db.tbl_ResultMaster.Find(model.ResultID);
                    if (master != null)
                    {
                        master.TimeElapsed = model.TimeElapsed;
                        a = new tbl_ResultDetail
                        {
                            QuestionID = model.QuestionID,
                            Answer = model.Answer,
                            IsTrue = model.IsTrue,
                            ResultID = model.ResultID,
                            TimeSpent = model.TimeSpent,
                        };
                        db.tbl_ResultDetail.Add(a);
                    }
                }

                db.SaveChanges();
                return model;
            }
        }

        public ResultVM GetResultByID(int resultID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                tbl_ResultMaster model = db.tbl_ResultMaster.Find(resultID);
                if (model != null)
                {
                    ResultVM s = new ResultVM()
                    {
                        Datetime = model.Datetime,
                        ID = model.ID,
                        StudentID = model.StudentID,
                        ObtainedMarks = model.ObtainedMarks,
                        QuestionPaperID = model.QuestionPaperID,
                        TotalMarks = model.TotalMarks,
                        Mode = model.Mode,
                        TimeGiven = model.TimeGiven - Common.GetCurrentDate().Subtract(model.Datetime ?? Common.GetCurrentDate()).Minutes,
                        TimeElapsed = model.TimeElapsed,
                        Detail = model.tbl_ResultDetail.ToList().ConvertAll(x => new ResultDetailVM
                        {
                            Answer = x.Answer,
                            IsTrue = x.IsTrue,
                            QuestionID = x.QuestionID,
                            ResultID = x.ResultID,
                        })
                    };
                    return s;
                }
                return new ResultVM();
            }
        }
        public bool RecheckQuestion(int QPID, int QuestionID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var model = db.tbl_ResultDetail.Where(x => x.QuestionID == QuestionID && x.tbl_ResultMaster.QuestionPaperID == QPID).ToList();
                var Answer = db.tbl_Options.FirstOrDefault(x => x.Question_Id == QuestionID && x.IsCorrect == true)?.OptionText;
                foreach (var q in model)
                {
                    q.IsTrue = q.Answer == Answer;
                }
                db.SaveChanges();
                return true;
            }
        }


        public void Completed(int ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var res = db.tbl_ResultMaster.Include("tbl_ResultDetail").FirstOrDefault(x => x.ID == ID);
                if (res != null)
                {
                    // Optimization: Fetch all correct options for the questions in this result in ONE query
                    var questionIds = res.tbl_ResultDetail.Select(d => d.QuestionID).ToList();
                    var correctOptions = db.tbl_Options
                        .Where(o => questionIds.Contains(o.Question_Id) && o.IsCorrect == true)
                        .ToDictionary(o => o.Question_Id, o => o.OptionText);

                    foreach (var detail in res.tbl_ResultDetail)
                    {
                        if (detail.QuestionID.HasValue && correctOptions.TryGetValue(detail.QuestionID.Value, out string correctAnswer))
                        {
                            detail.IsTrue = !string.IsNullOrEmpty(detail.Answer) && 
                                            !string.IsNullOrEmpty(correctAnswer) && 
                                            detail.Answer.Trim().Equals(correctAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            detail.IsTrue = false;
                        }
                    }

                    // Save marks
                    res.ObtainedMarks = res.tbl_ResultDetail.Count(x => x.IsTrue == true);
                    res.TotalMarks = res.tbl_ResultDetail.Count();
                    
                    db.SaveChanges();
                }
            }
        }
        public void DeleteResult(int ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var res = db.tbl_ResultMaster.Find(ID);
                db.tbl_ResultMaster.Remove(res);
                db.SaveChanges();
            }
        }
        public void Suspend(int ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
            }
        }
        #endregion

        #region Test

        public List<sp_QuestionCountByStudent_Result> GetQuestionCount(string SID, string SysID = "")
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.sp_QuestionCountByStudent(SID, SysID).ToList();
            }
        }
        public List<SystemVM> GetSystems(string SID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {

                string sql = "EXEC sp_QuestionSystemCountByStudent @p0";
                var qCount = db.Database.SqlQuery<sp_QuestionSystemCountByStudent_Result>(sql, SID).ToList();


                return qCount.GroupBy(x => x.SystemID).ToList().ConvertAll(x => new SystemVM
                {
                    Section = x.FirstOrDefault().Section ?? "Uncategorized",
                    Name = x.FirstOrDefault().SystemName,
                    ID = x.FirstOrDefault().SystemID,
                    IsAvailable = true,
                    QCount = x.Select(y => new sp_QuestionCountByStudent_Result
                    {
                        NoOfQ = y.NoOfQ,
                        QType = y.QType,
                    }).ToList(),

                }).OrderByDescending(x => x.IsAvailable).ThenByDescending(x => x.QCount.Sum(y => y.NoOfQ)).ToList();

            }
        }
        public List<SystemVM> GetFreeSystems(string Type)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var systems = new SettingRepository().GetQuestionSystemSettings(db);
                var FreeSystems = systems.FreeSystems?.Split(',').ToList().ConvertAll(x => Convert.ToInt32(x));
                var MockSystems = systems.MockTestSystems?.Split(',').ToList().ConvertAll(x => Convert.ToInt32(x));

                string sql = "EXEC sp_QuestionSystemCountByStudent @p0";
                var qCount = db.Database.SqlQuery<sp_QuestionSystemCountByStudent_Result>(sql, "").ToList();


                return qCount.GroupBy(x => x.SystemID).ToList().ConvertAll(x => new SystemVM
                {
                    Name = x.FirstOrDefault().SystemName,
                    ID = x.FirstOrDefault().SystemID,
                    IsAvailable = (Type == "Mock" ? MockSystems.Contains(x.FirstOrDefault().SystemID ?? 0) : FreeSystems.Contains(x.FirstOrDefault().SystemID ?? 0)),
                    QCount = x.Select(y => new sp_QuestionCountByStudent_Result
                    {
                        NoOfQ = y.NoOfQ,
                        QType = y.QType,
                    }).ToList(),

                }).OrderByDescending(x => x.IsAvailable).ThenByDescending(x => x.QCount.Sum(y => y.NoOfQ)).ToList();
            }
        }

        public List<sp_TestsPerformance_Result> TestsPerformance(string id)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return db.sp_TestsPerformance(id).ToList();
            }
        }
        #endregion
        public bool AllowReopen(int ID)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var r = db.tbl_ResultMaster.Find(ID);
                if (r != null)
                {
                    r.AllowReopen = true;
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }
    }
    public static class ExtensionsClass
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}