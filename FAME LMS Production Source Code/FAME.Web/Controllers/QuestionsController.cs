using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize]
    public class QuestionsController : Controller
    {
        private readonly IQuestionRepository _questionRepository;
        private readonly IQuestionPaperRepository _questionPaperRepository;
        private readonly IQuestionSystemsRepository _questionSystemsRepository;
        private readonly IQuestionPartitionRepository _questionPartitionRepository;
        private readonly ICourseRepository _courseRepository;

        // Background Job Tracking
        private static ConcurrentDictionary<string, JobResult> _jobs = new ConcurrentDictionary<string, JobResult>();

        public class JobResult
        {
            public string Status { get; set; } // "Processing", "Completed", "Error"
            public List<QuestionVM> Questions { get; set; }
            public string ErrorMessage { get; set; }
            public string DebugLog { get; set; }
        }
        private readonly IGeneralRepository _generalRepository;

        public QuestionsController(
            IQuestionRepository questionRepository,
            IQuestionPaperRepository questionPaperRepository,
            IQuestionSystemsRepository questionSystemsRepository,
            IQuestionPartitionRepository questionPartitionRepository,
            IGeneralRepository generalRepository)
        {
            _questionRepository = questionRepository;
            _questionPaperRepository = questionPaperRepository;
            _questionPaperRepository = questionPaperRepository;
            _questionSystemsRepository = questionSystemsRepository;
            _questionPartitionRepository = questionPartitionRepository;
            _generalRepository = generalRepository;
        }

        #region Question CRUD

        public ActionResult Delete(int id)
        {
            bool f = _questionRepository.DeleteQuestion(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Index(int Type, int? id = 0)
        {
            ViewBag.Systems = _questionSystemsRepository.GetList();
            ViewBag.Partitions = _questionPartitionRepository.GetList();
            ViewBag.Papers = _questionPaperRepository.GetList();
            if (Type > 0 && Type < 4)
            {
                QuestionVM c = new QuestionVM();
                if (id > 0)
                {
                    c = _questionRepository.GetByID(id);
                }
                else
                    c.Type = Type;
                return View(c);
            }
            return RedirectToAction("List", new { Type = 1 });
        }
        public ActionResult Preview(int id)
        {
            var c = _questionRepository.GetByID(id);
            return View(c);
        }
        [HttpPost]
        public ActionResult Index(QuestionVM vid)
        {
            vid.CreatedBy = User.Identity.GetUserId();
            QuestionVM id = _questionRepository.SaveQuestion(vid);
            return Json(id, JsonRequestBehavior.AllowGet);
        }
        public ActionResult List(int Type)
        {
            ViewBag.Systems = _questionSystemsRepository.GetList();
            ViewBag.Partitions = _questionPartitionRepository.GetList();
            QuestionVM model = new QuestionVM { List = new List<sp_lstQuestions_Result>(), Type = Type };
            return View(model);
        }
        public ActionResult FilterQuestions(int Type, int PartitionID, string Tags, string Text, int SystemID)
        {
            QuestionVM model = new QuestionVM
            {
                Type = Type,
                List = _questionRepository.GetList((QuestionType)Type, PartitionID, Tags, Text, "", SystemID)
            };
            return PartialView("_List", model);
        }
        public ActionResult Filter(int Type, string Tags, string Text, int SystemID,int PartitionID)
        {
            QuestionVM model = new QuestionVM { List = _questionRepository.GetList((QuestionType)Type, PartitionID, Tags, Text, "", SystemID), Type = Type };
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region FAQ
        public ActionResult FaqIndex(int? id = 0)
        {
            ViewBag.Categories = _generalRepository.GetList((int)MasterGroup.AgentCategory);
            QuestionVM c = new QuestionVM();
            if (id > 0)
            {
                c = _questionRepository.GetByID(id);
            }
            else
                c.Type = (int)QuestionType.FAQ;
            return View(c);
        }
        public ActionResult FaqList()
        {
            QuestionVM model = new QuestionVM { List = _questionRepository.GetList((QuestionType.FAQ), 0, "", ""), Type = (int)QuestionType.FAQ };
            return View(model);
        }
        #endregion

        #region Import MCQs

        public ActionResult Import()
        {
            ViewBag.Systems = _questionSystemsRepository.GetList();
            ViewBag.Partitions = _questionPartitionRepository.GetList();
            return View();
        }
        [HttpPost]
        public ActionResult Import(HttpPostedFileBase File, string Type, int NoOfQuestions, int SystemID,int PartitionID)
        {
            // $!$@ at start Questions
            // $!$  at start of Options
            // #!#  at End correct Option
            // *!*  at start Explanation
            #region Extract Text
            var path = "~/Images/Docs/" + Common.SavePicSameName(File, "Docs/");
            string file = Server.MapPath(path);

            var text = System.IO.File.ReadAllText(file);
            //Document doc = new Document(file);
            //foreach (Paragraph para in doc.GetChildNodes(NodeType.Paragraph, true))
            //    text += para.ToString(SaveFormat.Text);

            var qs = text.Split(new string[] { "$!$@" }, StringSplitOptions.None);
            var Questions = new List<QuestionVM>();
            #endregion

            #region Extract Questions
            foreach (var q in qs)
            {
                var quest = q.Split(new string[] { "$!$" }, StringSplitOptions.None);
                if (quest.Length > 1)
                {
                    var nq = new QuestionVM
                    {
                        Question = quest[0],
                        SystemID = SystemID,
                        PartitionID = PartitionID,
                        Type = (int)QuestionType.Mcq,
                        Options = new List<OptionVM>()
                    };

                    #region Options
                    int opNo = 0;
                    foreach (var o in quest)
                    {
                        opNo++;
                        if (opNo > 1)
                        {
                            if (opNo == quest.Count())
                            {
                                var lastopAndExp = o.Split(new string[] { "*!*" }, StringSplitOptions.None);
                                if (lastopAndExp.Length > 1)
                                {
                                    nq.AnsExplain = lastopAndExp[1];
                                    nq.Options.Add(new OptionVM
                                    {
                                        OptionText = lastopAndExp[0].Replace("#!#", ""),
                                        IsCorrect = lastopAndExp[0].Contains("#!#")
                                    });
                                }
                            }
                            else
                                nq.Options.Add(new OptionVM
                                {
                                    OptionText = o.Replace("#!#", ""),
                                    IsCorrect = o.Contains("#!#")
                                });
                        }
                    }
                    #endregion

                    nq.IsMultiAns = nq.Options.Where(x => x.IsCorrect).Count() > 1;
                    Questions.Add(nq);
                }
            }
            #endregion

            System.IO.File.Delete(file);
            if (Type.Contains("Save") && NoOfQuestions == Questions.Count)
            {
                _questionRepository.SaveQuestions(Questions);
                ViewBag.IsSaved = "yes";
            };
            if (NoOfQuestions != Questions.Count) ViewBag.IsValid = "Number Of Questions do not match";
            ViewBag.Systems = _questionSystemsRepository.GetList();
            ViewBag.Partitions = _questionPartitionRepository.GetList();
            return View(Questions);
        }

        #endregion

        #region Import PDF MCQs

        /// <summary>
        /// Shows the PDF import form
        /// </summary>
        public ActionResult ImportPdf()
        {
            ViewBag.Systems = _questionSystemsRepository.GetList();
            ViewBag.Partitions = _questionPartitionRepository.GetList();
            return View();
        }

        /// <summary>
        /// Processes uploaded PDF and extracts MCQs
        /// </summary>
        [HttpPost]
        public ActionResult StartPdfImport(HttpPostedFileBase File, int SystemID, int PartitionID)
        {
            if (File == null || File.ContentLength == 0)
            {
                return Json(new { success = false, message = "Please select a file." });
            }

            string jobId = Guid.NewGuid().ToString();
            _jobs[jobId] = new JobResult { Status = "Processing" };

            // Read bytes synchronously
            byte[] fileBytes;
            using (var binaryReader = new BinaryReader(File.InputStream))
            {
                fileBytes = binaryReader.ReadBytes(File.ContentLength);
            }
            string fileName = File.FileName;

            // Start background task
            Task.Run(() =>
            {
                try
                {
                    var parser = new PdfMcqParser();
                    string tempPath = Path.Combine(HttpRuntime.AppDomainAppPath, "Images/Docs", jobId + "_" + fileName);
                    
                    // Ensure dir exists
                    Directory.CreateDirectory(Path.GetDirectoryName(tempPath));
                    System.IO.File.WriteAllBytes(tempPath, fileBytes);

                    var questions = parser.ParsePdfFile(tempPath, SystemID, PartitionID);
                    
                    try { System.IO.File.Delete(tempPath); } catch { }

                    _jobs[jobId] = new JobResult 
                    { 
                        Status = "Completed", 
                        Questions = questions,
                        DebugLog = parser.DebugLog
                    };
                }
                catch (Exception ex)
                {
                    _jobs[jobId] = new JobResult 
                    { 
                        Status = "Error", 
                        ErrorMessage = ex.Message 
                    };
                }
            });

            return Json(new { success = true, jobId = jobId });
        }

        [HttpGet]
        public ActionResult CheckImportStatus(string jobId)
        {
            if (_jobs.TryGetValue(jobId, out var job))
            {
                if (job.Status == "Completed")
                {
                     Session["ImportResults_" + jobId] = job.Questions;
                     Session["DebugLog_" + jobId] = job.DebugLog;
                     return Json(new { status = "Completed", redirectUrl = "/Questions/ReviewImport?jobId=" + jobId }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { status = job.Status, message = job.ErrorMessage }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = "Error", message = "Job not found" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ReviewImport(string jobId)
        {
            var questions = Session["ImportResults_" + jobId] as List<QuestionVM>;
            var debugLog = Session["DebugLog_" + jobId] as string;
            
            if (questions == null)
            {
                return RedirectToAction("ImportPdf");
            }

            ViewBag.DebugLog = debugLog;
            ViewBag.Systems = _questionSystemsRepository.GetList();
            ViewBag.Partitions = _questionPartitionRepository.GetList();
            
            _jobs.TryRemove(jobId, out _);
            
            return View("ImportPdf", questions);
        }

        /// <summary>
        /// Processes uploaded PDF and extracts MCQs (Legacy Sync Method)
        /// </summary>
        [HttpPost]
        public ActionResult ImportPdf(HttpPostedFileBase File, int SystemID, int PartitionID)
        {
            try
            {
                if (File == null || File.ContentLength == 0)
                {
                    ViewBag.Error = "Please select a PDF file to upload.";
                    ViewBag.Systems = _questionSystemsRepository.GetList();
                    ViewBag.Partitions = _questionPartitionRepository.GetList();
                    return View(new List<QuestionVM>());
                }

                // Save uploaded file temporarily
                var uploadPath = Server.MapPath("~/Images/Docs/");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                var fileName = Path.GetFileName(File.FileName);
                var tempPath = Path.Combine(uploadPath, "temp_" + Guid.NewGuid() + "_" + fileName);
                File.SaveAs(tempPath);

                // Parse the PDF
                var parser = new PdfMcqParser();
                var questions = parser.ParsePdfFile(tempPath, SystemID, PartitionID);

                // Clean up temp file
                if (System.IO.File.Exists(tempPath))
                {
                    System.IO.File.Delete(tempPath);
                }

                var parsedCount = questions.Count;
                ViewBag.ParsedCount = parsedCount;
                ViewBag.SystemID = SystemID;
                ViewBag.PartitionID = PartitionID;
                ViewBag.Systems = _questionSystemsRepository.GetList();
                ViewBag.Partitions = _questionPartitionRepository.GetList();
                
                // Show debug log if no questions were parsed
                if (parsedCount == 0)
                {
                    ViewBag.DebugLog = parser.DebugLog;
                }
                
                return View(questions);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error parsing PDF: " + ex.Message;
                ViewBag.Systems = _questionSystemsRepository.GetList();
                ViewBag.Partitions = _questionPartitionRepository.GetList();
                return View(new List<QuestionVM>());
            }
        }

        /// <summary>
        /// Saves parsed questions to database via AJAX
        /// </summary>
        [HttpPost]
        public ActionResult SaveParsedQuestions(List<QuestionVM> questions)
        {
            try
            {
                if (questions == null || questions.Count == 0)
                {
                    return Json(new { success = false, message = "No questions to save." });
                }

                // Set creator for all questions
                foreach (var q in questions)
                {
                    q.CreatedBy = User.Identity.GetUserId();
                }

                // Bulk save
                var result = _questionRepository.SaveQuestions(questions);
                
                var count = questions.Count;
                return Json(new { success = result, message = result ? "Successfully saved " + count + " questions!" : "Failed to save questions." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        #endregion

        public ActionResult UploadImage()
        {
            try
            {
                HttpFileCollectionBase files = Request.Files;
                HttpPostedFileBase file = files[0];
                string fname;
                // Checking for Internet Explorer      
                if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                {
                    string[] testfiles = file.FileName.Split(new char[] { '\\' });
                    fname = testfiles[testfiles.Length - 1];
                }
                else
                {
                    fname = file.FileName;
                }
                fname = "Course" + fname;
                System.IO.File.Delete(Server.MapPath("~/Images/" + fname));
                fname = Path.Combine(Server.MapPath("~/Images/"), fname);
                file.SaveAs(fname);
                return Json("Course" + file.FileName, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return null;
            }
        }
    }
}