using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class ExamAttemptController : Controller
    {
        private readonly ExamAttemptRepository _repo = new ExamAttemptRepository();

        public ActionResult Index(int? packageId)
        {
            var model = new ExamAttemptVM
            {
                List = _repo.GetAttemptList(packageId)
            };

            using (var db = new FAMEEntities())
            {
                ViewBag.Packages = db.tbl_Package.Where(p => p.IsActive != false)
                    .OrderBy(p => p.PackageName)
                    .Select(p => new SelectListItem { Value = p.PackageID.ToString(), Text = p.PackageName })
                    .ToList();
            }

            ViewBag.SelectedPackageId = packageId;
            return View(model);
        }

        public ActionResult Save(int? id)
        {
            var model = id.HasValue && id.Value > 0 
                ? _repo.GetAttemptByID(id.Value) ?? new ExamAttemptVM { IsActive = true }
                : new ExamAttemptVM { IsActive = true };

            LoadPackages();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(ExamAttemptVM model)
        {
            if (ModelState.IsValid)
            {
                var result = _repo.SaveAttempt(model);
                if (result > 0)
                {
                    TempData["Success"] = "Exam attempt saved successfully.";
                    return RedirectToAction("Index", new { packageId = model.PackageID });
                }
                ModelState.AddModelError("", "Failed to save exam attempt.");
            }

            LoadPackages();
            return View(model);
        }

        public ActionResult Delete(int id)
        {
            var attempt = _repo.GetAttemptByID(id);
            int? packageId = attempt?.PackageID;

            if (_repo.DeleteAttempt(id))
            {
                TempData["Success"] = "Exam attempt deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to delete exam attempt.";
            }

            return RedirectToAction("Index", new { packageId });
        }

        // AJAX endpoint for registration dropdown
        [AllowAnonymous]
        public JsonResult GetByPackageId(int packageId)
        {
            var attempts = _repo.GetActiveAttemptsByPackage(packageId);
            return Json(attempts.Select(a => new { a.ExamAttemptID, a.AttemptName }), JsonRequestBehavior.AllowGet);
        }

        private void LoadPackages()
        {
            using (var db = new FAMEEntities())
            {
                ViewBag.Packages = db.tbl_Package.Where(p => p.IsActive != false)
                    .OrderBy(p => p.PackageName)
                    .Select(p => new SelectListItem { Value = p.PackageID.ToString(), Text = p.PackageName })
                    .ToList();
            }
        }

        #region Assign to Student

        public ActionResult AssignToStudent(int? enrollmentId)
        {
            LoadPackages();
            
            if (enrollmentId.HasValue)
            {
                var enrollment = _repo.GetEnrollmentById(enrollmentId.Value);
                if (enrollment != null)
                {
                    ViewBag.SelectedPackageId = enrollment.PackageId;
                    ViewBag.Students = _repo.GetStudentsByPackage(enrollment.PackageId)
                        .Select(s => new SelectListItem 
                        { 
                            Value = s.EnrollmentId.ToString(), 
                            Text = s.StudentName + " (" + s.StudentEmail + ")",
                            Selected = s.EnrollmentId == enrollmentId
                        }).ToList();
                    ViewBag.Attempts = GetActiveAttemptsByPackage(enrollment.PackageId)
                        .Select(a => new SelectListItem 
                        { 
                            Value = a.ExamAttemptID.ToString(), 
                            Text = a.AttemptName,
                            Selected = a.ExamAttemptID == enrollment.CurrentExamAttemptId
                        }).ToList();
                    return View(enrollment);
                }
            }
            
            return View(new StudentEnrollmentVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignToStudent(StudentEnrollmentVM model)
        {
            if (model.EnrollmentId > 0)
            {
                if (_repo.AssignAttemptToStudent(model.EnrollmentId, model.NewExamAttemptId))
                {
                    TempData["Success"] = "Exam attempt assigned successfully.";
                    return RedirectToAction("AssignToStudent", new { enrollmentId = model.EnrollmentId });
                }
                TempData["Error"] = "Failed to assign exam attempt.";
            }
            else
            {
                TempData["Error"] = "Please select a student.";
            }

            LoadPackages();
            return View(model);
        }

        // AJAX: Get students enrolled in a package
        public JsonResult GetStudentsByPackage(int packageId)
        {
            var students = _repo.GetStudentsByPackage(packageId);
            return Json(students.Select(s => new 
            { 
                s.EnrollmentId, 
                s.StudentName, 
                s.StudentEmail,
                s.CurrentExamAttemptId,
                CurrentAttempt = s.CurrentExamAttemptName ?? "Not Assigned"
            }), JsonRequestBehavior.AllowGet);
        }

        // AJAX: Get exam attempts for a package
        public JsonResult GetAttemptsByPackage(int packageId)
        {
            var attempts = GetActiveAttemptsByPackage(packageId);
            return Json(attempts.Select(a => new { a.ExamAttemptID, a.AttemptName }), JsonRequestBehavior.AllowGet);
        }

        private List<ExamAttemptVM> GetActiveAttemptsByPackage(int packageId)
        {
            return _repo.GetActiveAttemptsByPackage(packageId);
        }

        // POST: Bulk assign exam attempt to all students in a package
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BulkAssignToPackage(int packageId, int? examAttemptId)
        {
            if (packageId <= 0)
            {
                TempData["Error"] = "Please select a package.";
                return RedirectToAction("AssignToStudent");
            }

            int count = _repo.BulkAssignAttemptToPackage(packageId, examAttemptId);
            
            if (count > 0)
            {
                if (examAttemptId.HasValue)
                {
                    TempData["Success"] = $"Successfully assigned exam attempt to {count} enrollments.";
                }
                else
                {
                    TempData["Success"] = $"Successfully removed exam attempt from {count} enrollments.";
                }
            }
            else
            {
                TempData["Error"] = "No students found to update.";
            }

            return RedirectToAction("AssignToStudent");
        }

        #endregion
    }
}
