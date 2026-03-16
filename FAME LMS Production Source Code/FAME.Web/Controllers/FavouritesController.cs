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

namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize]
    public class FavouritesController : Controller
    {
        private readonly IFavouritesRepository _favouritesRepository;

        public FavouritesController(IFavouritesRepository favouritesRepository)
        {
            _favouritesRepository = favouritesRepository;
        }

        public ActionResult Index()
        {
            var id = User.Identity.GetUserId();
            var model = _favouritesRepository.GetList(id);
            return View(model);
        }
        public ActionResult Mcqs()
        {
            var id = User.Identity.GetUserId();
            var model = _favouritesRepository.GetList(id, QuestionType.Mcq.ToString());
            return View(model);
        }
        public ActionResult AddToFavourits(int ID, string Type, bool Remove = false)
        {
            var id = User.Identity.GetUserId();
            var model = new tbl_FavouriteList()
            {
                Student_Fid = id,
                ObjectType = Type,
                Object_ID = ID,
            };
            bool f;
            if (Remove)
                f = _favouritesRepository.RemoveFavourits(model);
            else
                f = _favouritesRepository.AddToFavourits(model);
            return Json(f, JsonRequestBehavior.AllowGet);
        }

    }
}