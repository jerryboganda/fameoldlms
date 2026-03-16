using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Models;
using Google.Rpc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    public class AuthV2Controller : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IPackageRepository _packageRepository;
        private readonly IGeneralRepository _generalRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IInstallmentRepository _installmentRepository;

        public AuthV2Controller(
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            IUserRepository userRepository,
            IPackageRepository packageRepository,
            IGeneralRepository generalRepository,
            IEnrollmentRepository enrollmentRepository,
            IInstallmentRepository installmentRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userRepository = userRepository;
            _packageRepository = packageRepository;
            _generalRepository = generalRepository;
            _enrollmentRepository = enrollmentRepository;
            _installmentRepository = installmentRepository;
        }

        public ApplicationSignInManager SignInManager => _signInManager;
        public ApplicationUserManager UserManager => _userManager;

        #region Authentication
        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var email = model.Email.ToLower();
                var user = await UserManager.FindByEmailAsync(email);

                if (user == null)
                {
                    return Json(new { success = false, message = "Invalid login attempt." }, JsonRequestBehavior.AllowGet);
                }

                var result = await SignInManager.PasswordSignInAsync(email, model.Password, model.RememberMe, shouldLockout: false);

                if (result != SignInStatus.Success)
                {
                    return HandleSignInFailure(result);
                }

                var canLoginResult = await CanLogin(user.Id);
                if (!canLoginResult.Item1)
                {
                    return Json(new { success = false, message = canLoginResult.Item2 }, JsonRequestBehavior.AllowGet);
                }

                var userProfile = _userRepository.GetProfile(user.Id);
                var jwtToken = GenerateJwtToken(user);


                var responseData = new
                {
                    User = new
                    {
                        userProfile.User_FatherName,
                        userProfile.User_Mobile,
                        userProfile.OccupationID,
                        userProfile.CountryID,
                        userProfile.MockTestType,
                        userProfile.YearOfMBBS,
                        user.Email,
                        userProfile.User_Id,
                        userProfile.User_Name,
                        userProfile.User_Pic,
                        userProfile.Institute,
                        userProfile.JobLocation,
                        userProfile.City,
                        userProfile.CNIC,
                        userProfile.Type,
                        userProfile.FatherProfession,
                        userProfile.SponserProfession,
                        userProfile.StuCardFront,
                        userProfile.StuCardBack,
                        userProfile.CNICFront,
                        userProfile.CNICBack,
                        ID = userProfile.User_AspUser,
                        IsRequestAccepted = Common.IsRequestAccepted(userProfile.User_AspUser),
                    },
                    Token = jwtToken
                };

                return Json(new { success = true, data = responseData }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private ActionResult HandleSignInFailure(SignInStatus status)
        {
            string message;
            switch (status)
            {
                case SignInStatus.LockedOut:
                    message = "Account is not verified yet.";
                    break;
                case SignInStatus.RequiresVerification:
                    message = "Email is not verified yet.";
                    break;
                case SignInStatus.Failure:
                    message = "Your email or password is wrong.";
                    break;
                default:
                    message = "An unknown error occurred.";
                    break;
            }

            return Json(new
            {
                success = false,
                message
            }, JsonRequestBehavior.AllowGet);
        }

        private string GenerateJwtToken(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["config:JwtKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Id),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(ConfigurationManager.AppSettings["config:JwtExpireDays"])),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = ConfigurationManager.AppSettings["config:JwtIssuer"],
                Audience = ConfigurationManager.AppSettings["config:JwtAudience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
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
                if (await UserManager.IsEmailConfirmedAsync(id))
                {
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
                else
                {
                    return (false, "Email Is Not Verified Yet");
                }
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
                await UserManager.SendEmailAsync(user.Id, "Varification Code", message);
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
            await UserManager.EmailService.SendAsync(message);
            return View("DisplayEmail");
        }
        [AllowAnonymous]
        public async Task<ActionResult> SendEmailNewUser(string Email, bool Again = false)
        {
            var code = _enrollmentRepository.GenVerifCodeSession(Email, Again);
            IdentityMessage message = new IdentityMessage()
            {
                Subject = "Verification Code",
                Body = Common.GetMessage(code),
                Destination = Email
            };
            await UserManager.EmailService.SendAsync(message);
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
}
