using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Web.Configuration;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using System.Net.Http;
using System.Windows;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace First_Aid_Made_Easy.Controllers
{
    [Authorize]
    public class ZoomController : Controller
    {
        private readonly IZoomApiRepository _zoomApiRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IUserRepository _userRepository;

        private static readonly HttpClient client = new HttpClient();
        private const int NumberOfRetries = 3;
        private const int DelayOnRetry = 1000;

        public ZoomController(
            IZoomApiRepository zoomApiRepository,
            ICourseRepository courseRepository,
            IPackageRepository packageRepository,
            IUserRepository userRepository)
        {
            _zoomApiRepository = zoomApiRepository;
            _courseRepository = courseRepository;
            _packageRepository = packageRepository;
            _userRepository = userRepository;
        }

        #region Meeting CRUD
        [HttpGet]
        public ActionResult CreateMeeting(long ID = 0)
        {
            MeetingVM model = new MeetingVM();
            if (ID > 0)
                model = _zoomApiRepository.GetByID(ID);
            else model.StartTime = DateTime.UtcNow.AddHours(5);
            string id = User.Identity.GetUserId();
            ViewBag.Courses = _courseRepository.GetList(id, User.IsInRole("Admin"));
            ViewBag.Packages = _packageRepository.GetList();
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> SaveMeeting(long id, string CourseIDs, string PackageIDs, string StartTime, string password)
        {
            string UserID = WebConfigurationManager.AppSettings["userID"];
            var json = Request.Params["data"];
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = new HttpResponseMessage();
            string token = WebConfigurationManager.AppSettings["token"];
            HttpMethod method = HttpMethod.Post;
            string href = "https://api.zoom.us/v2/users/" + UserID + "/meetings";
            if (id > 0)
            {
                href = "https://api.zoom.us/v2/meetings/" + id;
                method = new HttpMethod("PATCH");
            }
            using (var requestMessage = new HttpRequestMessage(method, href))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                requestMessage.Content = content;
                response = await client.SendAsync(requestMessage);
            }
            var responseString = await response.Content.ReadAsStringAsync();
            Meeting data;
            if (id > 0)
            {
                data = JsonConvert.DeserializeObject<Meeting>(json);
                data.id = id;
            }
            else
                data = JsonConvert.DeserializeObject<Meeting>(responseString);
            await _zoomApiRepository.SaveMeetingLocal(CourseIDs, PackageIDs, StartTime, data, password, id == 0, User.Identity.GetUserId());
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListMeeting()
        {
            var id = User.Identity.GetUserId();
            var nList = _zoomApiRepository.GetMeetingList(id);
            return View(nList);
        }

        public async Task<ActionResult> Delete(long id)
        {
            var response = new HttpResponseMessage();
            string token = WebConfigurationManager.AppSettings["token"];
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Delete, "https://api.zoom.us/v2/meetings/" + id))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                response = await client.SendAsync(requestMessage);
            }

            _zoomApiRepository.DeleteMeeting(id);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> GetMeetings()
        {
            //=========== Not Used Now ================
            string UserID = WebConfigurationManager.AppSettings["userID"];
            var response = new HttpResponseMessage();
            string token = WebConfigurationManager.AppSettings["token"];
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://api.zoom.us/v2/users/" + UserID + "/meetings"))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                for (int i = 1; i <= NumberOfRetries; ++i)
                {
                    try
                    {
                        response = await client.SendAsync(requestMessage);
                        break;
                    }
                    catch when (i < NumberOfRetries)
                    {
                        await Task.Delay(DelayOnRetry);
                    }
                }
            }
            var responseString = await response.Content.ReadAsStringAsync();
            Data wow = JsonConvert.DeserializeObject<Data>(responseString);
            return Json(wow, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Meeting (Start / Join / Leave)
        public ActionResult Index()
        {
            // ========= Not Used ===========
            return View();
        }
        public ActionResult StartMeeting(long id)
        {
            _zoomApiRepository.UpdateMeetingStatus(id, MeetStatus.Active.ToString());
            string UserID = User.Identity.GetUserId();
            var profile = _userRepository.GetProfile(UserID);
            var meet = new MeetingVM
            {
                MeetingNo = id,
                Course = profile?.User_Name,
                Password = _zoomApiRepository.GetByID(id)?.Password,
                Role = 1
            };
            return View("Index", meet);
        }
        public ActionResult JoinMeeting(long id)
        {
            string UserID = User.Identity.GetUserId();
            var profile = _userRepository.GetProfile(UserID);
            var meet = new MeetingVM
            {
                MeetingNo = id,
                Course = profile?.User_Name,
                Password = _zoomApiRepository.GetByID(id)?.Password,
                Role = 0
            };
            return View("Index", meet);
        }
        public ActionResult Meeting(string name, string mn, string pwd, string role, string lang, string signature)
        {

            return View();
        }
        public ActionResult Leave(long id)
        {
            var IsHost = User.IsInRole("Admin") || User.IsInRole("Teacher");
            if (IsHost)
            {
                _zoomApiRepository.UpdateMeetingStatus(id, MeetStatus.Completed.ToString());
                return RedirectToAction("ListMeeting");
            }
            return RedirectToAction("MyMeetings", "Student");
        }
        public ActionResult ExternalLinkPage()
        {
            return View();
        }
        #endregion

        #region Meeting Video
        [HttpPost]
        public ActionResult SaveVideo(VideoVM model)
        {
            var v = _zoomApiRepository.SaveVideo(model);
            return Json(v, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetVideo(int ID)
        {
            var v = _zoomApiRepository.GetVideo(ID);
            return PartialView("_VideoIndex", v);
        }
        public ActionResult DeleteVideo(int ID)
        {
            var v = _zoomApiRepository.Delete(ID);
            return Json(v, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UploadImage(int ID)
        {
            HttpFileCollectionBase files = Request.Files;
            HttpPostedFileBase file = files[0];
            string FileName = Common.SavePicSameName(file, "Meet/");
            var f = _zoomApiRepository.SavePic(FileName, ID);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}