using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Areas.FileManager.Models;
using Microsoft.AspNet.Identity;

namespace First_Aid_Made_Easy.Areas.FileManager.Controllers
{
    [Authorize]
    public class StMainController : Controller
    {
        private readonly IFileItemRepository _fileRepository;
        private readonly IDashBoardRepository _dashBoardRepository;
        private string RootPath = "~/File-Repository/";

        public StMainController(IFileItemRepository fileRepository, IDashBoardRepository dashBoardRepository)
        {
            _fileRepository = fileRepository;
            _dashBoardRepository = dashBoardRepository;
        }

        // GET: FileManager/Main
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Update(string path)
        {
            try
            {
                path = path.Trim('/');

                var PackIDs = _dashBoardRepository.GetEnrollmentList("", "", User.Identity.GetUserId(), 0, 0, null, "All", "", "", "").Select(x => x.PackageID).Where(x => x.HasValue).Select(x => x.Value).ToList();
                // get current files & folders
                var i = await _fileRepository.GetListForPackageAsync(path, PackIDs);
                var items = i.Select(x => new FileItemModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Path = x.Path,
                    MimeType = x.MimeType,
                    CDate = x.CDate,
                    MDate = x.MDate,
                    IsFolder = x.IsFolder
                }).ToList();

                return Json(new OperationResult
                {
                    Status = OperationStats.Success,
                    Items = items
                }, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                throw;
            }
        }

        public async Task<FileResult> Download(int id)
        {
            var file = await _fileRepository.GetByIDAsync(id);
            if (file == null) return null;
            string path = Server.MapPath(string.Concat(file.Path.Replace("ROOT", RootPath), '/', file.Name));
            byte[] fileBytes;
            if (file.IsFolder)
            {
                string zipPath = Server.MapPath(RootPath + file.Name + ".zip");
                ZipFile.CreateFromDirectory(path, zipPath);
                path = zipPath;
                fileBytes = System.IO.File.ReadAllBytes(path);
                System.IO.File.Delete(path);
            }
            else
                fileBytes = System.IO.File.ReadAllBytes(path);

            string fileName = file.Name;
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);

        }

        private List<ModelErrorCollection> GetErrors(ModelStateDictionary modelState)
        {
            return modelState.Select(x => x.Value.Errors)
                .Where(y => y.Count > 0)
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}