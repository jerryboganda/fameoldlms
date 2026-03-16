using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using First_Aid_Made_Easy.Models;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using Microsoft.AspNet.Identity;
using First_Aid_Made_Easy.Areas.FileManager.Models;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Cors;
using First_Aid_Made_Easy.BLL.JzTimer;

namespace First_Aid_Made_Easy.Controllers
{
    [Authorize]
    public class GeneralController : Controller
    {
        private readonly IGeneralRepository _generalRepository;

        public GeneralController(IGeneralRepository generalRepository)
        {
            _generalRepository = generalRepository;
        }

        // GET: General
        public ActionResult AgentCategory()
        {
            ViewBag.List = _generalRepository.GetList((int)MasterGroup.AgentCategory);
            return View();
        }
        [HttpPost]
        public ActionResult SaveAgentCategory(tbl_Master data)
        {
            var f = _generalRepository.CreateAgentCategory(data);
            if (f)
                TempData["Success"] = "Success";
            return RedirectToAction("AgentCategory");

        }
        public ActionResult DeleteAgentCategory(int ID)
        {
            return Json(_generalRepository.DeleteAgentCategory(ID), JsonRequestBehavior.AllowGet);
        }

        #region BookCode

        public ActionResult BookCode()
        {
            ViewBag.List = _generalRepository.GetList();
            return View();
        }
        [HttpPost]
        public ActionResult SaveCode(string Code, string Notes)
        {
            return Json(_generalRepository.Create(new tbl_BookCode
            {
                Notes = Notes,
                BookCode = Code,
                CreatedBy = User.Identity.GetUserId()
            }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult VerifyBookCode(string BookCode)
        {
            return Json(_generalRepository.UseBookCode(BookCode, User.Identity.GetUserId()), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Master Files
        public ActionResult MasterFiles()
        {
            var List = new List<MasterVM>();
            List.Add(_generalRepository.GetMasterByGroup(MasterGroup.DashBoardAd));
            ViewBag.List = List;
            return View();
        }

        [HttpPost]
        public ActionResult SaveMaster(MasterVM data)
        {
            var f = _generalRepository.CreateMaster(data);
            if (f)
                TempData["Success"] = "Success";
            return RedirectToAction("MasterFiles");

        }
        #endregion

        #region Seacrh

        public ActionResult Search(string t)
        {
            var results = _generalRepository.SearchContent(t);
            return PartialView("_SearchResults", results);
        }

        #endregion

        #region Upload Files


        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Upload(HttpPostedFileBase File, string Name)
        {
            string Path = Common.SavePic(File, "Comp/");
            var ParseResult = new CNICParseResult { };
            var processer = new ImageProcessing(Path);
            string text = ""; OperationStats status = OperationStats.Success;
            try
            {

                Path = processer.Compress();
                if (Name == "CNICFrontFile")
                {
                    string OCR_text = processer.VisionOCR();
                    ParseResult = processer.ParseCNIC(OCR_text);
                    ParseResult.PicPath = processer.GetProfileImage();
                    if (!ParseResult.isValid)
                    {
                        status = OperationStats.Invalid;
                        text = "Unable to detect data from this picture. Please upload a new one";
                        processer.DeleteFile();
                        Common.DeleteFile(ParseResult.PicPath);
                        Path = "";
                    }
                }
                if (Name == "CNICBackFile")
                {
                    string OCR_text = processer.VisionOCR();

                    if (!OCR_text.Contains("Registrar General of Pakistan"))
                    {
                        status = OperationStats.Invalid;
                        text = "Not a Valid CNIC Back Pic. Please upload a new one";
                        processer.DeleteFile();
                        Path = "";
                    }
                }
                if (Name == "User_PicFile")
                {
                    if (!processer.HasValidHumanFace())
                    {
                        status = OperationStats.Invalid;
                        text = "Unable to detect a face in this picture. Please upload a new one";
                        processer.DeleteFile();
                        Path = "";
                    }
                }

                //if (Name.Contains("CNIC")) processer.Process();
            }
            catch (Exception ex)
            {
                JzLogger.WriteError(ex, "Error In Upload");
                status = OperationStats.Error;
                text = "Something Went Wrong";
                Common.DeleteFile(Path);
                Path = "";
            }

            return Json(new
            {
                Status = status,
                Message = text,
                Path,
                ParseResult,
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> JustUpload(HttpPostedFileBase File, string Name)
        {
            var a = File.ContentLength;
            string Path = Common.SavePic(File, "Comp/");
            var processer = new ImageProcessing(Path);
            if (Name.Contains("CNIC")) processer.Process();
            if (Name == "User_PicFile") Path = processer.Compress();
            return Json(new
            {
                Status = OperationStats.Success,
                Message = "",
                Path,
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult UploadFS(HttpPostedFileBase File, string p)
        {
            string Path = Common.SavePic(File, "ticket/", p);
            return Json(new
            {
                Status = OperationStats.Success,
                Message = "",
                Path,
            }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult Delete(string Paths)
        {
            if (Paths != null)
            {
                foreach (var Path in Paths.Split(','))
                {
                    Common.DeleteFile(Path);
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        #endregion


    }
}