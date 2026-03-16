using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.BLL.JzTimer
{
    public static class Settings
    {
        public readonly static string Email_Name = "Settings.Email";
        public readonly static string SMTP_Name = "Settings.SMTP";
        public readonly static string Q_Sys = "Settings.QuestionSystem";
        public readonly static string _Register = "Settings.Register";
        public static class SMTP
        {
            public readonly static string FromEmail = SMTP_Name + ".FromEmail";
            public readonly static string FromName = SMTP_Name + ".FromName";
            public readonly static string MailAccount = SMTP_Name + ".MailAccount";
            public readonly static string MailPassword = SMTP_Name + ".MailPassword";
            public readonly static string Host = SMTP_Name + ".Host";
            public readonly static string Port = SMTP_Name + ".Port";
            public readonly static string SSL = SMTP_Name + ".SSL";
            public readonly static string UseApi = SMTP_Name + ".UseApi";
            public readonly static string ApiKey = SMTP_Name + ".ApiKey";
        }
        public static class Email
        {
            public readonly static string SendLoginInfo = Email_Name + ".SendLoginInfo";
            public readonly static string SendLectureReport = Email_Name + ".SendLectureReport";
            public readonly static string SendTestAttempts = Email_Name + ".SendTestAttempts";

            public readonly static string AfterDays = Email_Name + ".AfterDays";
            public readonly static string WarningAfterDays = Email_Name + ".WarningAfterDays";
            public readonly static string ParentAfterDays = Email_Name + ".ParentAfterDays";

            public readonly static string FirstDayMessage = Email_Name + ".FirstDayMessage";
            public readonly static string WarningMessage = Email_Name + ".WarningMessage";
            public readonly static string ParentMessage = Email_Name + ".ParentMessage";

            public readonly static string Universities = Email_Name + ".Universities";
            public readonly static string Packages = Email_Name + ".Packages";
        }

        public static class QSys
        {
            public readonly static string FreeSystems = Q_Sys + ".FreeSystems";
            public readonly static string MockTestSystems = Q_Sys + ".MockTestSystems";
            public readonly static string TrialTestId = Q_Sys + ".TrialTestId";
        }

        public static class Register
        {
            public readonly static string AllowedPortals = _Register + ".AllowedPortals";
            public readonly static string DefaultPortal = _Register + ".DefaultPortal";
            public readonly static string CanChange = _Register + ".CanChange";
            public readonly static string TutorialLink = _Register + ".TutorialLink";

            public readonly static string PaymentInfo = _Register + ".PaymentInfo";
            public readonly static string AdminContacts = _Register + ".AdminContacts";
        }
    }
}