using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize]
    public class ReminderController : Controller
    {
        private readonly IGeneralRepository _generalRepository;

        public ReminderController(IGeneralRepository generalRepository)
        {
            _generalRepository = generalRepository;
        }

        public ActionResult Delete(int id)
        {
            return Json(_generalRepository.DeleteReminder(id), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Index()
        {
            var id = User.Identity.GetUserId();
            var rem = new List<ReminderVM>();
            if (User.IsInRole("Student"))
            {
                rem = _generalRepository.GetReminderList(id);
                return View("IndexStudent", rem);
            }
            else
            {
                rem = _generalRepository.GetReminderList(id); // Use same list logically
            }

            return View(rem);
        }
        [HttpPost]
        public ActionResult Save(string Reminder, string Date, string EndDate = "", bool IsAllDay = false)
        {
            var rem = new tbl_Reminder
            {
                Reminder = Reminder,
                UserID = User.Identity.GetUserId(),
                DateFrom = Common.TryStringToDateTime(Date),
                DateTo = Common.TryStringToDateTime(EndDate),
                IsFullDay = IsAllDay,
                CreatedDT = Common.GetCurrentDate(),
            };
            var savedRem = _generalRepository.SaveReminder(rem);
            return Json(new
            {
                id = savedRem.RemID,
                Date = String.Format("{0:yyyy-MM-dd HH:mm}", savedRem.DateFrom),
                DateTo = String.Format("{0:yyyy-MM-dd HH:mm}", savedRem.DateTo),
            }, JsonRequestBehavior.AllowGet);
        }

    }
}