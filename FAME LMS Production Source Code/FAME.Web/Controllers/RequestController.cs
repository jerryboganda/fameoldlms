using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize]
    public class RequestController : Controller
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IInstallmentRepository _installmentRepository;

        public RequestController(
            IEnrollmentRepository enrollmentRepository,
            IInstallmentRepository installmentRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _installmentRepository = installmentRepository;
        }


        #region Register Request 


        public ActionResult List(int Type = 1)
        {
            RequestVM model = new RequestVM();
            ViewBag.Type = Type;
            var date = Common.GetCurrentDate();
            model.List = _enrollmentRepository.GetList(date, date, Type, RequestType.Register.ToString());
            return View(model);
        }

        public ActionResult FilterList(DateTime DateFrom, DateTime DateTo, int Type = 1)
        {
            RequestVM model = new RequestVM();
            string id = User.Identity.GetUserId();
            model.List = _enrollmentRepository.GetList(DateFrom, DateTo, Type, RequestType.Register.ToString());
            return PartialView("_List", model);
        }
        #endregion

        #region Extension Request 


        public ActionResult ExtList(int Type = 1)
        {
            RequestVM model = new RequestVM();
            ViewBag.Type = Type;
            var date = Common.GetCurrentDate();
            model.List = _enrollmentRepository.GetList(date, date, Type, RequestType.Extension.ToString());
            return View(model);
        }

        public ActionResult ExtFilterList(DateTime DateFrom, DateTime DateTo, int Type = 1)
        {
            RequestVM model = new RequestVM();
            string id = User.Identity.GetUserId();
            model.List = _enrollmentRepository.GetList(DateFrom, DateTo, Type, RequestType.Extension.ToString());
            return PartialView("_ExtList", model);
        }
        #endregion

        #region Installments

        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult InstallmentList()
        {
            StudentInstallmentVM model = new StudentInstallmentVM();
            string id = User.Identity.GetUserId();
            var date = Common.GetCurrentDateForView();
            model.List = _installmentRepository.GetInstallmentList(date, date, id, "");
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult FilterInstallmentList(string DateFrom, string DateTo, string Semail)
        {
            StudentInstallmentVM model = new StudentInstallmentVM();
            string id = User.Identity.GetUserId();
            model.List = _installmentRepository.GetInstallmentList(DateFrom, DateTo, id, Semail);
            return PartialView("_InstallmentList", model);
        }

        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult InstallmentPaid(int id)
        {
            bool f = _installmentRepository.InstallmentPaid(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Invoices 

        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult InvoiceList()
        {
            var model = _enrollmentRepository.GetInvoiceList("", "", "", "", false);
            return View(model);
        }
        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult FilterInvoiceList(string DateFrom, string DateTo, string Status, bool? IsEmailSent)
        {
            var model = _enrollmentRepository.GetInvoiceList(DateFrom, DateTo, "", Status, IsEmailSent);
            return PartialView("_InvoiceList", model);
        }

        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult InvoiceSend(int id, string Status, int Price)
        {
            bool f = _enrollmentRepository.UpdateStatus(id, Price, Status);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}