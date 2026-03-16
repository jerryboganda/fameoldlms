using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using Rotativa;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;

namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize]
    public class BillingController : Controller
    {
        private readonly IDashBoardRepository _dashBoardRepository;
        private readonly IGeneralRepository _generalRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;

        public BillingController(
            IDashBoardRepository dashBoardRepository,
            IGeneralRepository generalRepository,
            IEnrollmentRepository enrollmentRepository)
        {
            _dashBoardRepository = dashBoardRepository;
            _generalRepository = generalRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        public ActionResult Delete(int id)
        {
            return Json(_generalRepository.DeleteReminder(id), JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Index()
        {
            var id = User.Identity.GetUserId();
            ViewBag.StudentDetail = await _dashBoardRepository.GetStudentDetailAsync(0, id);

            SubscriptionsVM sbs = (await _dashBoardRepository.GetStudentDetailAsync(0, User.Identity.GetUserId())).Subscrption;
            ViewBag.AlreadySent = _enrollmentRepository.HasPendingRequest(id);
            return View(sbs);
        }
        public ActionResult InvoicePDF(int ID)
        {
            var a = _enrollmentRepository.GetInvoiceDetail(ID);
            if (a == null) return HttpNotFound();
            return new ViewAsPdf(a);
        }
        public ActionResult ExtensionRequest(int EnrollID, bool IsAccepted)
        {
            var result = _enrollmentRepository.AddExtensionRequest(EnrollID, User.Identity.GetUserId(), User.Identity.GetUserName());
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}