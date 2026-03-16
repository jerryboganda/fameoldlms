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
    [Authorize]
    public class BookMistakesController : Controller
    {
        private readonly IBookMistakesRepository _bookMistakesRepository;

        public BookMistakesController(IBookMistakesRepository bookMistakesRepository)
        {
            _bookMistakesRepository = bookMistakesRepository;
        }



        public ActionResult View(int id = 0)
        {
            var model = new tbl_BookMistakes();
            if (id > 0)
            {
                model = _bookMistakesRepository.GetByID(id);
            }
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Student()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult SaveMistake(tbl_BookMistakes model)
        {
            var id = _bookMistakesRepository.Create(model);
            return Json(id, 0);
        }



        public ActionResult Delete(int ID)
        {
            bool f = _bookMistakesRepository.Delete(ID);
            return Json(f,0);
        }


        public ActionResult List()
        {
            var model = _bookMistakesRepository.GetList();
            return View(model);
        }


    }
}