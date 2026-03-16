using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using First_Aid_Made_Easy.BLL.Interfaces;


namespace First_Aid_Made_Easy.Controllers
{
    [Authorize(Roles = "SuppAgent,Admin")]
    public class TicketController : Controller
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketController(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        // GET: Ticket
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SaveReply(TicketReplyVM model)
        {
            model.SendBy = User.Identity.GetUserId();
            _ticketRepository.SaveReply(model);
            return RedirectToAction("Reply", new { id = model.Ticket_ID });
        }

        public ActionResult Reply(int id)
        {
            return View(_ticketRepository.GetByID(id, User.Identity.GetUserId()));
        }


        public ActionResult List()
        {
            TicketVM model = new TicketVM { List = _ticketRepository.GetList(Status.Open.ToString(), "", "", "","") };
            return View(model);
        }
        public ActionResult ListForPopular()
        {
            TicketVM model = new TicketVM { List = _ticketRepository.GetList("", "", "", "","") };
            return View(model);
        }
        // ============== For Popularity =======================
        public ActionResult ChagePopular(int ID, bool isPopular)
        {
            return Json(_ticketRepository.ChangePopular(ID, isPopular), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Close(int ID, string Status)
        {
            return Json(_ticketRepository.ChangeStatus(ID, Status), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Filter(string Status, string DateFrom, string DateTo, string StudentID)
        {
            TicketVM model = new TicketVM();
            model.List = _ticketRepository.GetList(Status, DateFrom, DateTo, StudentID, User.Identity.GetUserId());
            return PartialView("_List", model);
        }
    }
}