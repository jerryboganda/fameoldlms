using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class StudyScheduleController : Controller
    {
        private readonly ExamAttemptRepository _repo = new ExamAttemptRepository();

        public ActionResult Index(int attemptId)
        {
            var model = new StudyScheduleVM
            {
                ExamAttemptID = attemptId,
                List = _repo.GetScheduleList(attemptId)
            };

            var attempt = _repo.GetAttemptByID(attemptId);
            ViewBag.AttemptName = attempt?.AttemptName ?? "";
            ViewBag.PackageName = attempt?.PackageName ?? "";

            return View(model);
        }

        public ActionResult Save(int attemptId, int? id)
        {
            var model = id.HasValue && id.Value > 0
                ? _repo.GetScheduleByID(id.Value) ?? new StudyScheduleVM { ExamAttemptID = attemptId, IsActive = true, ScheduleType = 1 }
                : new StudyScheduleVM { ExamAttemptID = attemptId, IsActive = true, ScheduleType = 1 };

            var attempt = _repo.GetAttemptByID(attemptId);
            ViewBag.AttemptName = attempt?.AttemptName ?? "";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Save(StudyScheduleVM model)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload for type 3
                if (model.ScheduleType == 3 && model.ScheduleFile != null && model.ScheduleFile.ContentLength > 0)
                {
                    try
                    {
                        string fileName = Path.GetFileName(model.ScheduleFile.FileName);
                        string uniqueName = $"{Guid.NewGuid()}_{fileName}";
                        string uploadPath = Server.MapPath("~/Uploads/StudySchedules");

                        if (!Directory.Exists(uploadPath))
                            Directory.CreateDirectory(uploadPath);

                        string filePath = Path.Combine(uploadPath, uniqueName);
                        model.ScheduleFile.SaveAs(filePath);
                        model.FilePath = $"~/Uploads/StudySchedules/{uniqueName}";
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "File upload failed: " + ex.Message);
                        LoadAttemptInfo(model.ExamAttemptID);
                        return View(model);
                    }
                }

                var result = _repo.SaveSchedule(model);
                if (result > 0)
                {
                    TempData["Success"] = "Study schedule saved successfully.";
                    return RedirectToAction("Index", new { attemptId = model.ExamAttemptID });
                }
                ModelState.AddModelError("", "Failed to save study schedule.");
            }

            LoadAttemptInfo(model.ExamAttemptID);
            return View(model);
        }

        public ActionResult Delete(int id, int attemptId)
        {
            if (_repo.DeleteSchedule(id))
            {
                TempData["Success"] = "Study schedule deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete study schedule.";
            }

            return RedirectToAction("Index", new { attemptId });
        }

        private void LoadAttemptInfo(int attemptId)
        {
            var attempt = _repo.GetAttemptByID(attemptId);
            ViewBag.AttemptName = attempt?.AttemptName ?? "";
        }
    }
}
