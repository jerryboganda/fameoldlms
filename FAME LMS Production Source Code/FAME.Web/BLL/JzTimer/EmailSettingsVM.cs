using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.BLL.JzTimer
{
    public class QuestionSystemSettingsVM
    {

        public string FreeSystems { get; set; }
        public string MockTestSystems { get; set; }
        public int? TrialTestId { get; set; }
    }
    public class RegisterPageSettingsVM
    {
        public int? DefaultPortal { get; set; }
        public string AllowedPortals { get; set; }
        public string TutorialLink { get; set; }
        public bool CanChangePortal { get; set; }
        [AllowHtml]
        public string AdminContacts { get; set; }
        [AllowHtml]
        public string PaymentInfo { get; set; }
    }
    public class SystemSettings
    {
        public RegisterPageSettingsVM RegisterPageSettings { get; set; }
    }
    public class SmtpSettingsVM
    {
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Port {  get; set; }
        public string Host {  get; set; }
        public bool SSL { get; set; } = true;
        public bool UseApi { get; set; } = true;
        public string ApiKey { get; set; }
    }
    public class EmailSettingsVM
    {
        public bool SendLoginInfo { get; set; } = true;
        public bool SendLectureReport { get; set; } = true;
        public bool SendTestAttempts { get; set; } = true;

        public int AfterDays { get; set; } = 1;
        public int WarningAfterDays { get; set; } = 2;
        public int ParentAfterDays { get; set; } = 3;

        public string FirstDayMessage { get; set; }
        public string WarningMessage { get; set; }
        public string ParentMessage { get; set; }

        public string Universities { get; set; }
        public string Packages { get; set; }


    }
}