using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;

using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IGeneralRepository _generalRepository;

        public ProfileController(IUserRepository userRepository, IGeneralRepository generalRepository)
        {
            _userRepository = userRepository;
            _generalRepository = generalRepository;
        }

        public ActionResult MyProfile(string id)
        {
            LoadViewBags();
            if (User.IsInRole("Student"))
                return View("ProfileStudent", _userRepository.GetProfile(User.Identity.GetUserId()));
            return View(_userRepository.GetProfile(User.Identity.GetUserId()));
        }

        private void LoadViewBags()
        {
            int id = Convert.ToInt32(MasterGroup.Occupation);
            ViewBag.Occupation = _generalRepository.GetList(id).OrderByDescending(x => x.Master_ID).ToList();
        }

        [HttpPost]
        public ActionResult MyProfile(UserVM user)
        {
            if (ModelState.IsValid)
            {
                var f = _userRepository.Save(user);
                TempData["Success"] = "success";
                if (User.IsInRole("Teacher"))
                    return RedirectToAction("Index", "Teacher");
                else if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "Admin");
                else
                    return RedirectToAction("MyProfile", "Profile");
            }
            else
            {
                LoadViewBags();
                if (User.IsInRole("Student"))
                    return View("ProfileStudent", user);
                return View(user);
            }
        }
        public PartialViewResult ChangePassword()
        {
            return PartialView("_ChangePassword");
        }

        [AllowAnonymous]
        public ActionResult CheckProfilePics()
        {
            var list = _userRepository.GetUserPics(new DateTime(2024, 1, 1));
            var paths = new List<string>();
            foreach (var l in list)
            {
                var p = Server.MapPath(l.StartsWith("/") ? "~/Images" : "~/Images/" + l);
                if (System.IO.File.Exists(p))
                {
                    paths.Add(l);
                }
            }
            return Json(paths, 0);
        }
    }
}