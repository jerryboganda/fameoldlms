using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.BLL.JzTimer;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Filters;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    [BrowserFilter]
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly ISettingRepository _settingRepository;

        public SettingsController(ISettingRepository settingRepository)
        {
            _settingRepository = settingRepository;
        }

        public ActionResult EmailSettings()
        {
            EmailSettingsVM model = _settingRepository.GetEmailSettings();
            return View(model);
        }

        public ActionResult SaveEmailSettings(EmailSettingsVM model)
        {
            var f = _settingRepository.SaveEmailSettings(model);
            return Json(f, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SMTPSettings()
        {
            SmtpSettingsVM model = _settingRepository.GetSMTPSettings();
            return View(model);
        }

        public ActionResult SaveSMTPSettings(SmtpSettingsVM model)
        {
            var f = _settingRepository.SaveSMTPSettings(model);
            return Json(f, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Settings()
        {
            SystemSettings model = new SystemSettings { RegisterPageSettings = _settingRepository.GetRegisterSettings() };
            return View(model);
        }

        [HttpPost]
        public ActionResult SaveSettings(SystemSettings t)
        {
            var model = _settingRepository.SaveRegisterSettings(t.RegisterPageSettings);
            return Json(true, 0);
        }

    }
}