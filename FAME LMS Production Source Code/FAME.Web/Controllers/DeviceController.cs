using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using System.Net;

namespace First_Aid_Made_Easy.Controllers
{
    [Authorize]
    public class DeviceController : Controller
    {
        private readonly IGeneralRepository _generalRepository;

        public DeviceController(IGeneralRepository generalRepository)
        {
            _generalRepository = generalRepository;
        }

        public ActionResult Index()
        {
            try
            {
                string UserID = User.Identity.GetUserId();
                var LstClientInfo = _generalRepository.GetActiveUserDevices(UserID);
                return View(LstClientInfo);
            }
            catch
            {
                return View();
            }
        }

        public ActionResult DeviceDeleteReqList()
        {
            var model = _generalRepository.GetDeleteReqList();
            return View(model);
        }
        public ActionResult RemoveDevice(string ID, string UserID, int ReqID = 0, bool IsAccepted = true)
        {
            var model = _generalRepository.RemoveDevice(ID, UserID, ReqID, IsAccepted);
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PrevHistory(string id)
        {
            var model = _generalRepository.GetDeleteReqList(id, true);
            return PartialView("_PrevHistory", model);
        }


        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult ResetDevices(string ID)
        {
            return Json(_generalRepository.ResetUserDevices(ID), JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteDeviceRequest(string id, string Notes)
        {
            var UseriD = User.Identity.GetUserId();
            var result = _generalRepository.AddDeviceDeleteRequest(id, UseriD, Notes);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult IsValid(string id)
        {
            if (!Request.IsAuthenticated) return Json(true, JsonRequestBehavior.AllowGet);

            bool? IsValid = _generalRepository.IsDeviceValid(id, User.Identity.GetUserId());
            return Json(IsValid, JsonRequestBehavior.AllowGet);
        }
    }
}