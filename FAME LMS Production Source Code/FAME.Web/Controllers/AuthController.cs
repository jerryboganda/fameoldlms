using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using Google.Rpc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    public class AuthController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IGeneralRepository _generalRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IInstallmentRepository _installmentRepository;

        public AuthController(
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            IUserRepository userRepository,
            IPackageRepository packageRepository,
            IGeneralRepository generalRepository,
            IEnrollmentRepository enrollmentRepository,
            IInstallmentRepository installmentRepository)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            _userRepository = userRepository;
            _packageRepository = packageRepository;
            _generalRepository = generalRepository;
            _enrollmentRepository = enrollmentRepository;
            _installmentRepository = installmentRepository;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public async Task<ApplicationUser> FindUser(string userName, string password)
        {
            return await UserManager.FindAsync(userName, password);
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        #region Authentication

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email.ToLower());
            var result = await SignInManager.PasswordSignInAsync(model.Email.ToLower(), model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    {
                        var can = await CanLogin(user.Id);
                        if (!can.Item1)
                        {
                            return Json(new { success = can.Item1, can.Item2 }, JsonRequestBehavior.AllowGet);
                        }

                        var model2 = _userRepository.GetProfile(user.Id);
                        var data = new
                        {
                            model2.User_FatherName,
                            model2.User_Mobile,
                            model2.OccupationID,
                            model2.CountryID,
                            model2.MockTestType,
                            model2.YearOfMBBS,
                            user.Email,
                            model2.User_Id,
                            model2.User_Name,
                            model2.User_Pic,
                            model2.Institute,
                            model2.JobLocation,
                            model2.City,
                            model2.CNIC,
                            model2.Type,
                            model2.FatherProfession,
                            model2.SponserProfession,
                            model2.StuCardFront,
                            model2.StuCardBack,
                            model2.CNICFront,
                            model2.CNICBack,
                            ID = model2.User_AspUser,
                            IsRequestAccepted = Common.IsRequestAccepted(model2.User_AspUser),
                        };
                        return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
                    }
                case SignInStatus.LockedOut:
                    return Json(new { success = false, Message = "Account Is Not Verified Yet" }, JsonRequestBehavior.AllowGet);
                case SignInStatus.RequiresVerification:
                    return Json(new { success = false, Message = "Email Is Not Verified Yet" }, JsonRequestBehavior.AllowGet);
                case SignInStatus.Failure:
                default:
                    return Json(new { success = false, Message = ("Your Email Or Password Is Wrong") }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            var d = (VerifyCodeVM)Session["VerifyCode"] ?? new VerifyCodeVM();
            model.EmailConfirmed = d.Email == model.Email && d.IsVerified;
            string Error = "";
            var Package = _packageRepository.GetByID(model.PackageID);
            var Duration = Package?.Durations?.FirstOrDefault(x => x.Duration == model.Duration)?.DurationChar;
            if (ModelState.IsValid && Duration != null)
            {


                var user = await UserManager.FindByEmailAsync(model.Email);
                IdentityResult result = new IdentityResult();
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = model.Email.ToLower(),
                        Email = model.Email.ToLower(),
                        EmailConfirmed = model.EmailConfirmed,
                        RegisteredFrom = model.RegisteredFrom,
                        PhoneNumber = model.User_Mobile,
                    };
                    result = await UserManager.CreateAsync(user, model.Password);
                }
                else
                {
                    _generalRepository.UpdateAspNetUser(new DAL.AspNetUsers
                    {
                        Id = user.Id,
                        RegisteredFrom = model.RegisteredFrom,
                        PhoneNumber = model.User_Mobile,
                    });
                }


                model.User_AspUser = user.Id;
                model.StudentID = user.Id;
                model.StudentEmail = model.Email;

                if (result.Succeeded || user != null)
                {
                    _ = _userRepository.Save(model);
                    try
                    {
                        var resultenr = _enrollmentRepository.AddRequest(model);
                    }
                    catch (Exception ex) { Error = (ex.Message); }
                    var resultr = await UserManager.AddToRoleAsync(user.Id, "Student");
                    return Json(new
                    {
                        success = true,
                        message = "Account Created Successfully.",
                        data = new UserInfo
                        {
                            User_Name = model.User_Name,
                            Email = model.Email,
                            User_Mobile = model.User_Mobile,
                            Institute = model.Institute,
                            City = model.City,
                            PackageName = Package.PackageName,
                            Duration = Duration
                        }
                    }, JsonRequestBehavior.AllowGet);
                }
                Error = result.Errors.FirstOrDefault();
            }

            if (Duration == null)
                Error = ("Package does not contain such duration");
            return Json(new { success = false, message = Error }, JsonRequestBehavior.AllowGet);
        }

        private async Task<(bool, string)> CanLogin(string id)
        {
            if (!UserManager.IsInRole(id, "Admin"))
            {
                if (!(await UserManager.IsEmailConfirmedAsync(id)))
                {
                    return (false, "Email Is Not Verified Yet");
                }

                if (_installmentRepository.IsPendingInstallment(id))
                {
                    return (false, "Your Account Is Blocked Due To Pending Installment");
                }

                #region tbl_UserLockout
                var Exp = _enrollmentRepository.IsExpired(id);
                if (Exp != null)
                {
                    return (false, Exp.Remarks);
                }

                #endregion
            }

            return (true, "");

        }
        [HttpGet]
        public async Task<ActionResult> CanUserLogin(string id)
        {
            var can = await CanLogin(id);
            return Json(new { success = can.Item1, message = can.Item2 }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Confirm Email

        [AllowAnonymous]
        public ActionResult ConfirmEmail(string Email, string code)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(code))
            {
                return Json(new { success = false, message = "Email And Code Is Required", data = false }, JsonRequestBehavior.AllowGet);
            }

            var result = _enrollmentRepository.ConfirmEmail(Email, code);
            return Json(new { success = result, message = result ? "Email Confirmed Successfuly" : "Code Is Wrong Or Expired.", data = result }, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult ConfirmNewEmail(string Email, string code)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(code))
            {
                return Json(new { success = false, message = "Email And Code Is Required", data = false }, JsonRequestBehavior.AllowGet);
            }

            var result = _enrollmentRepository.ConfirmNewEmail(Email, code);

            return Json(new { success = result, message = result ? "Email Confirmed Successfuly" : "Code Is Wrong Or Expired.", data = result }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Manage Password
        [HttpPost]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            List<string> Error = new List<string>();
            if (ModelState.IsValid && !string.IsNullOrEmpty(model.ID))
            {
                var result = await UserManager.ChangePasswordAsync(model.ID, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(model.ID);
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return Json(new { success = true, message = "Password Changed Successfully", data = true }, JsonRequestBehavior.AllowGet);
                }
                AddErrors(result);
            }

            foreach (ModelState modelState in ViewData.ModelState.Values)
                foreach (ModelError error in modelState.Errors)
                    Error.Add(error.ErrorMessage);
            return Json(new { success = false, message = Error.FirstOrDefault(), data = false }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null) return Json(new { success = false, message = "Email Does Not Exist.", data = false }, JsonRequestBehavior.AllowGet);
            if (!(await UserManager.IsEmailConfirmedAsync(user.Id))) return Json(new { success = false, data = false, message = "Email is Not Verified." }, JsonRequestBehavior.AllowGet);

            string code = _enrollmentRepository.GenerateVerificationCode(user.Id, CodeType.Email).Item1;
            var message = Common.GetMessage(code);
            if (model.Type == "SMS")
            {
                var no = Common.GetUserNameByASpUserID(user.Id);
                SMS.SMSMain(new IdentityMessage()
                {
                    Destination = no.CountryID.ToString() + no.User_Mobile,
                    Body = "FAME Verfication Code Is  " + code,
                });
            }
            else
            {
                var sent = await Common.TrySendMail(new MessageVM
                {
                    Destination = user.Email,
                    Subject = "Varification Code",
                    Body = message
                });

                if (!sent)
                    return Json(new { success = false, message = "Unable to send verification email right now.", data = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, message = "Email sent with 6-digit code", data = true }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var Error = new List<string>();
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null)
                    return Json(new { success = false, message = "Email Does Not Exist.", data = false }, JsonRequestBehavior.AllowGet);

                bool isCodeValid = _enrollmentRepository.ConfirmEmail(model.Email, model.Code);
                if (!isCodeValid)
                    return Json(new { success = false, message = "Invalid Code", data = false }, JsonRequestBehavior.AllowGet);

                var result = await UserManager.ResetPasswordAsync(user.Id, await UserManager.GeneratePasswordResetTokenAsync(user.Id), model.Password);
                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "Password Reset Successfully", data = true }, JsonRequestBehavior.AllowGet);
                }
                AddErrors(result);
            }
            foreach (ModelState modelState in ViewData.ModelState.Values)
                foreach (ModelError error in modelState.Errors)
                    Error.Add(error.ErrorMessage);
            return Json(new { success = false, message = Error.FirstOrDefault(), data = false });
        }
        #endregion

        #region Send Email

        [AllowAnonymous]
        public async Task<ActionResult> SendEmailAgain(string email)
        {
            var id = UserManager.FindByEmail(email)?.Id;
            if (id != null)
            {
                await SendEmailAgainP(id, true);
                return Json(new { success = true, message = "Email Sent To your Account for verification." }, 0);
            }
            else
            {
                return Json(new { success = false, message = "Email Not Found" }, 0);

            }
        }
        [AllowAnonymous]
        public async Task<ActionResult> SendEmailAgainP(string Id, bool Again)
        {
            var code = _enrollmentRepository.GenerateVerificationCode(Id, CodeType.Email, Again).Item1;
            var mail = UserManager.FindById(Id).Email;
            IdentityMessage message = new IdentityMessage()
            {
                Subject = "Verification Code",
                Body = Common.GetMessage(code),
                Destination = mail
            };
            var sent = await Common.TrySendMail(new MessageVM { Destination = mail, Subject = message.Subject, Body = message.Body });
            if (!sent)
            {
                TempData["Error"] = "Unable to send verification email right now.";
            }
            return View("DisplayEmail");
        }
        [AllowAnonymous]
        public async Task<ActionResult> SendEmailNewUser(string Email, bool Again = false)
        {
            var code = _enrollmentRepository.GenVerifCodeSession(Email, Again);
            var sent = await Common.TrySendMail(new MessageVM
            {
                Subject = "Verification Code",
                Body = Common.GetMessage(code),
                Destination = Email
            });

            if (!sent)
                return Json(new { success = false, message = "Unable to send verification email right now.", data = false }, 0);

            return Json(new { success = true, message = "Email Sent To your Account for verification.", data = true }, 0);
        }

        #endregion

        #region Helpers
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }


        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }


        #endregion
    }
    public class UserInfo
    {
        public string User_Name { get; set; }
        public string Email { get; set; }
        public string User_Mobile { get; set; }
        public string Institute { get; set; }
        public string City { get; set; }
        public string PackageName { get; set; }
        public string Duration { get; set; }
    }
}
