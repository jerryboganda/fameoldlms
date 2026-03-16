using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.Filters;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using First_Aid_Made_Easy.BLL.Interfaces;


namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize]
    public class EnrollmentController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IDashBoardRepository _dashBoardRepository;
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentController(
            ICourseRepository courseRepository,
            ICouponRepository couponRepository,
            IEnrollmentRepository enrollmentRepository,
            ISectionRepository sectionRepository,
            IPackageRepository packageRepository,
            IDashBoardRepository dashBoardRepository,
            IEnrollmentService enrollmentService)
        {
            _courseRepository = courseRepository;
            _couponRepository = couponRepository;
            _enrollmentRepository = enrollmentRepository;
            _sectionRepository = sectionRepository;
            _packageRepository = packageRepository;
            _dashBoardRepository = dashBoardRepository;
            _enrollmentService = enrollmentService;
        }

        #region Enrollment Falto
        [Authorize]
        public ActionResult Enroll(int id, string Method, string EnrollFor, string Secret)
        {
            var UID = User.Identity.GetUserId();
            if (Method == "CP")//Coupon
            {
                if (EnrollFor == "CR")//Course
                {
                    var course = _courseRepository.GetByID(id);
                    var coupon = _couponRepository.IsCouponTrue(id, Secret, true);
                    if (coupon.DiscountPer == 100)
                    {
                        EnrollmentMasterVM enrlm = new EnrollmentMasterVM
                        {
                            CouponFid = coupon.Id,
                            Enrollment_Price = course.Course_Price,
                            TeacherFid = course.Teacher_Fid,
                            Enrollment_EndDate = coupon.ExpiryDate,
                            StudentFid = UID,
                            ApprovedBy = User.Identity.GetUserId(),
                            Details = new List<EnrollmentDetailVM>() { new EnrollmentDetailVM { CourseID = course.Course_Id } }
                        };
                        var flag = _enrollmentRepository.Save(enrlm);
                        if (flag)
                        {
                            return Json("Success", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json("Error", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(coupon.DiscountPer, JsonRequestBehavior.AllowGet);
                    }
                }
                else //section
                {
                    var section = _sectionRepository.GetByID(id);
                    var coupon = _couponRepository.IsCouponTrue(id, Secret, false);

                    if (coupon.DiscountPer == 100)
                    {
                        EnrollmentMasterVM enrlm = new EnrollmentMasterVM();
                        enrlm.CouponFid = coupon.Id;
                        enrlm.Enrollment_Price = section.Section_Price;
                        enrlm.TeacherFid = section.TeacherFid;
                        enrlm.Enrollment_EndDate = coupon.ExpiryDate;
                        enrlm.Details = new List<EnrollmentDetailVM>() { new EnrollmentDetailVM { Section_Fid = section.Section_ID } };
                        enrlm.StudentFid = UID;
                        enrlm.ApprovedBy = User.Identity.GetUserId();
                        var flag = _enrollmentRepository.Save(enrlm);
                        if (flag)
                        {
                            return Json("Success", JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json("Error", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return Json(coupon.DiscountPer, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else if (Method == "EP")//EasyPaisa
            {
                return RedirectToAction("SectionEnroll", new { id = id, Secret = Secret });
            }
            else if (Method == "JC")//JazzCash
            {
                return RedirectToAction("SectionEnroll", new { id = id, Secret = Secret });
            }
            else
            {
                return RedirectToAction("SectionEnroll", new { id = id, Secret = Secret });
            }
        }


        [HttpPost]
        public ActionResult Pay(FormCollection formCollection)
        {
            JavaScriptSerializer JS = new JavaScriptSerializer();
            if (!string.IsNullOrEmpty(formCollection["Response"]))
            {
                string Pr = formCollection["Response"];
                CheckOutResponseModel checkOutResponseModel = JS.Deserialize<CheckOutResponseModel>
                    (Pr);
                Session["C3DSecureID"] = checkOutResponseModel.C3DSecureID;
                Session["GateWayCode"] = checkOutResponseModel.GateWayCode;
                Session["pp_Amount"] = checkOutResponseModel.pp_Amount;
                Session["pp_BankID"] = checkOutResponseModel.pp_BankID;
                Session["pp_BillReference"] = checkOutResponseModel.pp_BillReference;
                Session["pp_Description"] = checkOutResponseModel.pp_Description;
                Session["pp_Frequency"] = checkOutResponseModel.pp_Frequency;
                Session["pp_InstrToken"] = checkOutResponseModel.pp_InstrToken;
                Session["pp_Language"] = checkOutResponseModel.pp_Language;
                Session["pp_MerchantID"] = checkOutResponseModel.pp_MerchantID;
                Session["pp_ProductID"] = checkOutResponseModel.pp_ProductID;
                Session["pp_RetreivalReferenceNo"] = checkOutResponseModel.pp_RetreivalReferenceNo;
                Session["pp_ReturnURL"] = checkOutResponseModel.pp_ReturnURL;
                Session["pp_SubMerchantID"] = checkOutResponseModel.pp_SubMerchantID;
                Session["pp_TxnCurrency"] = checkOutResponseModel.pp_TxnCurrency;
                Session["pp_TxnDateTime"] = checkOutResponseModel.pp_TxnDateTime;
                Session["pp_TxnExpiryDate"] = checkOutResponseModel.pp_TxnExpiryDateTime;
                Session["pp_TxnRefNo"] = checkOutResponseModel.pp_TxnRefNo;
                Session["pp_TxnType"] = checkOutResponseModel.pp_TxnType;
                Session["pp_Version"] = checkOutResponseModel.pp_Version;
                Session["ppmbf_1"] = checkOutResponseModel.ppmbf_1;
                Session["ppmbf_2"] = checkOutResponseModel.ppmbf_2;
                Session["ppmbf_3"] = checkOutResponseModel.ppmbf_3;
                Session["ppmbf_4"] = checkOutResponseModel.ppmbf_4;
                Session["ppmbf_5"] = checkOutResponseModel.ppmbf_5;
                Session["ppmbf_6"] = checkOutResponseModel.ppmbf_6;
                Session["ppmpf_1"] = checkOutResponseModel.ppmpf_1;
                Session["ppmpf_2"] = checkOutResponseModel.ppmpf_2;
                Session["ppmpf_3"] = checkOutResponseModel.ppmpf_3;
                Session["ppmpf_4"] = checkOutResponseModel.ppmpf_4;
                Session["ppmpf_5"] = checkOutResponseModel.ppmpf_5;
                Session["ppmpf_6"] = checkOutResponseModel.ppmpf_6;
                Session["ResponseCode"] = checkOutResponseModel.ResponseCode;
                Session["ResponseMessage"] = checkOutResponseModel.ResponseMessage;
                Session["SummaryStatus"] = checkOutResponseModel.SummaryStatus;
                Session["pp_SecureHash"] = checkOutResponseModel.pp_SecureHash;
                Session["DoShow"] = true;
            }
            else
            {
                Session["C3DSecureID"] = formCollection["C3DSecureID"];
                Session["GateWayCode"] = formCollection["GateWayCode"];
                Session["pp_Amount"] = formCollection["pp_Amount"];
                Session["pp_BankID"] = formCollection["pp_BankID"];
                Session["pp_BillReference"] = formCollection["pp_BillReference"];
                Session["pp_Description"] = formCollection["pp_Description"];
                Session["pp_Frequency"] = formCollection["pp_Frequency"];
                Session["pp_InstrToken"] = formCollection["pp_InstrToken"];
                Session["pp_Language"] = formCollection["pp_Language"];
                Session["pp_MerchantID"] = formCollection["pp_MerchantID"];
                Session["pp_ProductID"] = formCollection["pp_ProductID"];
                Session["pp_RetreivalReferenceNo"] = formCollection["pp_RetreivalReferenceNo"];
                Session["pp_ReturnURL"] = formCollection["pp_ReturnURL"];
                Session["pp_SubMerchantID"] = formCollection["pp_SubMerchantID"];
                Session["pp_TxnCurrency"] = formCollection["pp_TxnCurrency"];
                Session["pp_TxnDateTime"] = formCollection["pp_TxnDateTime"];
                Session["pp_TxnExpiryDate"] = formCollection["pp_TxnExpiryDateTime"];
                Session["pp_TxnRefNo"] = formCollection["pp_TxnRefNo"];
                Session["pp_TxnType"] = formCollection["pp_TxnType"];
                Session["pp_Version"] = formCollection["pp_Version"];
                Session["ppmbf_1"] = formCollection["ppmbf_1"];
                Session["ppmbf_2"] = formCollection["ppmbf_2"];
                Session["ppmbf_3"] = formCollection["ppmbf_3"];
                Session["ppmbf_4"] = formCollection["ppmbf_4"];
                Session["ppmbf_5"] = formCollection["ppmbf_5"];
                Session["ppmbf_6"] = formCollection["ppmbf_6"];
                Session["ppmpf_1"] = formCollection["ppmpf_1"];
                Session["ppmpf_2"] = formCollection["ppmpf_2"];
                Session["ppmpf_3"] = formCollection["ppmpf_3"];
                Session["ppmpf_4"] = formCollection["ppmpf_4"];
                Session["ppmpf_5"] = formCollection["ppmpf_5"];
                Session["ppmpf_6"] = formCollection["ppmpf_6"];
                Session["pp_ResponseCode"] = formCollection["pp_ResponseCode"];
                Session["pp_ResponseMessage"] = formCollection["pp_ResponseMessage"];
                Session["SummaryStatus"] = formCollection["SummaryStatus"];
                Session["pp_SecureHash"] = formCollection["pp_SecureHash"];
                Session["DoShow"] = true;
            }
            return View();
        }
        #endregion

        #region Select Lists AND Copy Section 

        public ActionResult CopySection()
        {
            LoadCourses();
            return View();
        }

        [HttpPost]
        public ActionResult CopySection(int Toid, int Sid, int Fromid)
        {
            bool f = _enrollmentRepository.CopySection(Toid, Sid, Fromid);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult GetStudentID(string email)
        {
            var ID = _enrollmentRepository.GetStudentID(email);
            return Json(ID, JsonRequestBehavior.AllowGet);
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
            ViewBag.Section = _sectionRepository.List(User.Identity.GetUserId(), User.IsInRole("Admin"));
        }


        private void LoadPackage()
        {
            var listP = _packageRepository.GetList();
            ViewBag.Packages = listP.ConvertAll(x => new
            {
                Value = x.PackageID,
                Text = x.PackageName
            });
            ViewBag.Durations = new List<SelectListItem>
                {
                    new SelectListItem()
                };
        }
        #endregion

        #region Add Subsctiptions
        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult Index()
        {
            var model = new EnrollmentMasterVM();
            LoadCourses();
            LoadPackage();
            model.EndDate = Common.GetCurrentDateForView();
            return View(model);
        }


        //[Authorize(Roles = "Teacher,Admin")]
        //public ActionResult SaveSubscription(string[] StudentFid, int Duration, int Section_Fid, int CourseFid)
        //{
        //    bool f = new EnrollmentRepository().Enroll(StudentFid, Duration, Section_Fid, CourseFid, User.Identity.GetUserId());
        //    return Json(f, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult SaveSubscription(AddSubscriptioVM model)
        {
            model.UserID = User.Identity.GetUserId();
            bool f = _enrollmentService.ProcessEnrollment(model, true, false, model.Price);
            return Json(f, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetSubscription(string id)
        {
            var model = await _dashBoardRepository.GetStudentDetailAsync(0, id);
            return PartialView("_Enrollments", model.Subscrption);
        }
        #endregion

        #region Accept Request

        public ActionResult AcceptRequest(int id, decimal price)
        {
            try
            {
                bool f = _enrollmentService.ApproveEnrollmentRequest(id, User.Identity.GetUserId(), price);
                return Json(new { success = f }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null)
                {
                    msg += " Inner: " + ex.InnerException.Message;
                    if (ex.InnerException.InnerException != null)
                    {
                        msg += " Inner 2: " + ex.InnerException.InnerException.Message;
                    }
                }
                return Json(new { success = false, message = msg }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AcceptExtRequest(int id, int Price, int Duration, string Status)
        {
            bool f = _enrollmentService.ExtendEnrollment(id, Price, Duration, Status, User.Identity.GetUserId());
            return Json(f, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteRequest(string id)
        {
            bool f = _enrollmentRepository.DeleteRequest(id);
            return Json(f, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Block Users

        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult Blocking()
        {
            return View();
        }

        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult Expiring(int id = 0)
        {
            ViewBag.List = _enrollmentRepository.GetExpireableStudent();
            var model = new UserLockoutVM() { LockoutDT = Common.DateToString(DateTime.Now) };
            if (id > 0) model = _enrollmentRepository.GetExpire(id);
            return View(model);
        }


        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult Block(string Email, string Act)
        {
            bool f;
            if (Act == "Reset Password For")
                f = _enrollmentRepository.ResetPassword(Email);
            else if (Act == "Verify Email For")
                f = _enrollmentRepository.VerifyEmail(Email);
            else
            {
                var IsActive = Act == "UnBlock";
                f = _enrollmentService.ToggleStudentBlockStatus(Email, IsActive);
            }
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult SaveExpire(UserLockoutVM model)
        {
            var f = _enrollmentRepository.SaveExpire(model);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Teacher,Admin")]
        public ActionResult DeleteExpire(int ID)
        {
            var f = _enrollmentRepository.DeleteExpire(ID);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region SMS Sending
        [AllowAnonymous]
        public ActionResult SendSmsBranded(string dest, string body)
        {
            return Json(SMS.SendSmsBranded(dest, body), JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}