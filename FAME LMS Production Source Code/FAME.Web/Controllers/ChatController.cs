using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [Authorize]
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IPackageRepository _packageRepository;

        public ChatController(
            IChatRepository chatRepository,
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            IPackageRepository packageRepository)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _packageRepository = packageRepository;
        }

        // GET: Chat
        public ActionResult Index(string id)
        {
            ViewBag.UserID = User.Identity.GetUserId();
            ChatVM model = new ChatVM();
            var List = _chatRepository.GetList(User.Identity.GetUserId());
            if (string.IsNullOrEmpty(id))
            {
                if (List.Count() > 0)
                {
                    int ConvID = List.FirstOrDefault().ConvID;
                    model = _chatRepository.GetByID(ConvID, User.Identity.GetUserId());
                }
            }
            else
                model = _chatRepository.GetByUserID(id, User.Identity.GetUserId());
            if (!List.Any(x => x.FriendName == model.ConvName))
                List.Add(new DAL.sp_GetConversations_Result
                {
                    ConvID = model.ConvID,
                    FriendID = model.FriendID,
                    FriendName = model.ConvName,
                    FriendPic = model.FriendPic
                });
            model.List = List;
            return View(model);
        }
        public ActionResult GetConversation(int id)
        {
            return PartialView("_SingleConv", _chatRepository.GetByID(id, User.Identity.GetUserId()));
        }
        public ActionResult LoadMore(int id, int ConvID)
        {
            return PartialView("_Messages", _chatRepository.LoadMore(id, ConvID, User.Identity.GetUserId()));
        }
        public ActionResult UploadFile(long ID)
        {
            HttpFileCollectionBase files = Request.Files;
            HttpPostedFileBase file = files[0];
            string FileName = Common.SavePicSameName(file, "Chat/");
            var f = _chatRepository.SaveFile(FileName, ID);
            new Hubs.ChatHub().SendFile(ID, FileName);
            return Json(f, JsonRequestBehavior.AllowGet);
        }

        #region Friends
        public ActionResult SendRequest(string ID)
        {
            return Json(_chatRepository.SendRequest(ID, User.Identity.GetUserId()), JsonRequestBehavior.AllowGet);
        }
        public ActionResult UndoRequest(string ID)
        {
            return Json(_chatRepository.UndoRequest(ID, User.Identity.GetUserId()), JsonRequestBehavior.AllowGet);
        }
        public ActionResult AcceptRequest(int ID, bool IsAccepted)
        {
            return Json(_chatRepository.AcceptRequest(ID, IsAccepted), JsonRequestBehavior.AllowGet);
        }
        public ActionResult FindUser(int ID)
        {
            return Json(_userRepository.GetByID(ID, User.Identity.GetUserId()), JsonRequestBehavior.AllowGet);
        }
        public ActionResult FriendList()
        {
            return View(_chatRepository.GetFrindList(User.Identity.GetUserId()));
        }
        #endregion

        #region Drawer
        public ActionResult StudentDrawer()
        {
            ViewBag.Groups = _chatRepository.GetList(User.Identity.GetUserId()).Where(x => x.FriendID == null).ToList();
            var model = _chatRepository.GetFrindList(User.Identity.GetUserId());
            model.AllUser = model.AllUser.Take(10).ToList();
            model.AllUser.AddRange(model.Instructors.Take(10));
            return View("StudentDrawer", model);
        }
        public ActionResult LoadDrawer(string id)
        {
            var data = _chatRepository.GetByUserID(id, User.Identity.GetUserId());
            ViewBag.Conversation = data;
            string Html = Common.ViewToString(this.ControllerContext, "_ChatDrawer", null);
            return Json(new { Html, data.ConvID, data.ConvName }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadDrawerGroup(int id)
        {
            var data = _chatRepository.GetByID(id, User.Identity.GetUserId());
            ViewBag.Conversation = data;
            string Html = Common.ViewToString(this.ControllerContext, "_ChatDrawer", null);
            return Json(new { Html, data.ConvID, data.ConvName }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Group CRUD

        public ActionResult GroupIndex(int id = 0)
        {
            ChatVM model = new ChatVM();
            LoadCourses();
            if (id > 0)
            {
                model = _chatRepository.GetByID(id, User.Identity.GetUserId());
            }
            return View(model);
        }
        public ActionResult GroupList()
        {
            ChatVM model = new ChatVM() { GroupList = _chatRepository.GroupList() };
            return View(model);
        }
        [HttpPost]
        public ActionResult SaveGroup(ChatVM model)
        {
            if (ModelState.IsValid)
            {
                model.UserID_One = User.Identity.GetUserId();
                _chatRepository.Save(model);
                return RedirectToAction("GroupList");
            }
            return View(model);
        }
        public ActionResult GetGroupInfo(int id)
        {
            var Userid = User.Identity.GetUserId();
            var model = _chatRepository.GetGroupInfo(id, Userid);
            return Json(model , JsonRequestBehavior.AllowGet);
        }
        private void LoadCourses()
        {
            string id = User.Identity.GetUserId();
            List<DAL.tbl_Courses> list = _courseRepository.GetList(id, User.IsInRole("Admin"));
            ViewBag.Courses = list.ConvertAll(x => new
            {
                Value = x.Course_Id,
                Text = x.Course_Name
            });
            var listP = _packageRepository.GetList();
            ViewBag.Packages = listP.ConvertAll(x => new
            {
                Value = x.PackageID,
                Text = x.PackageName
            });
        }

        #endregion
    }
}