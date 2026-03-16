using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.BLL.JzTimer;

using First_Aid_Made_Easy.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching; // For caching
using System.Web;
using System.Web.Configuration;

namespace First_Aid_Made_Easy.BLL
{
    public class SettingRepository : ISettingRepository
    {
        private readonly ObjectCache _cache = MemoryCache.Default;  // Cache object

        // Cache expiration time
        private readonly TimeSpan CacheExpiration = TimeSpan.FromHours(24); // Adjust as needed

        public object SaveEmailSettings(EmailSettingsVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                SaveConfigByName(Settings.Email.SendLectureReport, model.SendLectureReport, db);
                SaveConfigByName(Settings.Email.SendLoginInfo, model.SendLoginInfo, db);
                SaveConfigByName(Settings.Email.SendTestAttempts, model.SendTestAttempts, db);

                SaveConfigByName(Settings.Email.FirstDayMessage, model.FirstDayMessage, db);
                SaveConfigByName(Settings.Email.WarningMessage, model.WarningMessage, db);
                SaveConfigByName(Settings.Email.ParentMessage, model.ParentMessage, db);

                SaveConfigByName(Settings.Email.AfterDays, model.AfterDays, db);
                SaveConfigByName(Settings.Email.WarningAfterDays, model.WarningAfterDays, db);
                SaveConfigByName(Settings.Email.ParentAfterDays, model.ParentAfterDays, db);

                SaveConfigByName(Settings.Email.Universities, model.Universities, db);
                SaveConfigByName(Settings.Email.Packages, model.Packages, db);

                db.SaveChanges();

                // Clear cache for email settings after saving
                _cache.Remove(Settings.Email_Name);

                return true;
            }
        }

        public EmailSettingsVM GetEmailSettings()
        {
            // Try to get from cache first
            var cachedSettings = _cache[Settings.Email_Name] as EmailSettingsVM;
            if (cachedSettings != null)
            {
                return cachedSettings;
            }

            using (FAMEEntities db = new FAMEEntities())
            {
                var db_tbl_Settings = db.tbl_Settings.Where(x => x.Name.Contains(Settings.Email_Name)).ToList();
                var ret = new EmailSettingsVM()
                {
                    SendLectureReport = Convert.ToBoolean(GetConfigByName(Settings.Email.SendLectureReport, db_tbl_Settings) ?? true),
                    SendLoginInfo = Convert.ToBoolean(GetConfigByName(Settings.Email.SendLoginInfo, db_tbl_Settings) ?? true),
                    SendTestAttempts = Convert.ToBoolean(GetConfigByName(Settings.Email.SendTestAttempts, db_tbl_Settings) ?? true),

                    AfterDays = Convert.ToInt32(GetConfigByName(Settings.Email.AfterDays, db_tbl_Settings) ?? 1),
                    WarningAfterDays = Convert.ToInt32(GetConfigByName(Settings.Email.WarningAfterDays, db_tbl_Settings) ?? 2),
                    ParentAfterDays = Convert.ToInt32(GetConfigByName(Settings.Email.ParentAfterDays, db_tbl_Settings) ?? 3),

                    FirstDayMessage = Convert.ToString(GetConfigByName(Settings.Email.FirstDayMessage, db_tbl_Settings)),
                    WarningMessage = Convert.ToString(GetConfigByName(Settings.Email.WarningMessage, db_tbl_Settings)),
                    ParentMessage = Convert.ToString(GetConfigByName(Settings.Email.ParentMessage, db_tbl_Settings)),

                    Packages = Convert.ToString(GetConfigByName(Settings.Email.Packages, db_tbl_Settings)),
                    Universities = Convert.ToString(GetConfigByName(Settings.Email.Universities, db_tbl_Settings)),
                };

                // Cache the result for future requests
                _cache.Set(Settings.Email_Name, ret, DateTimeOffset.Now.Add(CacheExpiration));

                return ret;
            }
        }

        public object SaveSMTPSettings(SmtpSettingsVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                SaveConfigByName(Settings.SMTP.FromEmail, model.FromEmail, db);
                SaveConfigByName(Settings.SMTP.FromName, model.FromName, db);
                SaveConfigByName(Settings.SMTP.MailAccount, model.Username, db);
                SaveConfigByName(Settings.SMTP.MailPassword, model.Password, db);
                SaveConfigByName(Settings.SMTP.Host, model.Host, db);
                SaveConfigByName(Settings.SMTP.Port, model.Port, db);
                SaveConfigByName(Settings.SMTP.SSL, model.SSL, db);
                SaveConfigByName(Settings.SMTP.UseApi, model.UseApi, db);
                SaveConfigByName(Settings.SMTP.ApiKey, model.ApiKey, db);

                db.SaveChanges();

                // Clear cache for SMTP settings after saving
                _cache.Remove(Settings.SMTP_Name);

                return true;
            }
        }

        public SmtpSettingsVM GetSMTPSettings()
        {
            // Try to get from cache first
            var cachedSettings = _cache[Settings.SMTP_Name] as SmtpSettingsVM;
            if (cachedSettings != null)
            {
                return cachedSettings;
            }

            using (FAMEEntities db = new FAMEEntities())
            {
                var db_tbl_Settings = db.tbl_Settings.Where(x => x.Name.Contains(Settings.SMTP_Name)).ToList();
                var ret = new SmtpSettingsVM()
                {
                    FromEmail = Convert.ToString(GetConfigByName(Settings.SMTP.FromEmail, db_tbl_Settings) ?? WebConfigurationManager.AppSettings["mailAccount"]),
                    FromName = Convert.ToString(GetConfigByName(Settings.SMTP.FromName, db_tbl_Settings) ?? "First Aid Made Easy - By Dr Hafiz Atif"),
                    SSL = Convert.ToBoolean(GetConfigByName(Settings.SMTP.SSL, db_tbl_Settings) ?? true),
                    Username = Convert.ToString(GetConfigByName(Settings.SMTP.MailAccount, db_tbl_Settings) ?? WebConfigurationManager.AppSettings["mailAccount"]),
                    Password = Convert.ToString(GetConfigByName(Settings.SMTP.MailPassword, db_tbl_Settings) ?? WebConfigurationManager.AppSettings["mailPassword"]),
                    Port = Convert.ToString(GetConfigByName(Settings.SMTP.Port, db_tbl_Settings) ?? WebConfigurationManager.AppSettings["port"]),
                    Host = Convert.ToString(GetConfigByName(Settings.SMTP.Host, db_tbl_Settings) ?? WebConfigurationManager.AppSettings["host"]),
                    UseApi = Convert.ToBoolean(GetConfigByName(Settings.SMTP.UseApi, db_tbl_Settings) ?? WebConfigurationManager.AppSettings["UseApi"]),
                    ApiKey = Convert.ToString(GetConfigByName(Settings.SMTP.ApiKey, db_tbl_Settings) ?? WebConfigurationManager.AppSettings["SmtpApikey"]),
                };

                // Cache the result for future requests
                _cache.Set(Settings.SMTP_Name, ret, DateTimeOffset.Now.Add(CacheExpiration));

                return ret;
            }
        }

        public object SaveQuestionSystemSettings(QuestionSystemSettingsVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                SaveConfigByName(Settings.QSys.FreeSystems, model.FreeSystems, db);
                SaveConfigByName(Settings.QSys.MockTestSystems, model.MockTestSystems, db);
                SaveConfigByName(Settings.QSys.TrialTestId, model.TrialTestId, db);

                db.SaveChanges();

                // Clear cache for question system settings after saving
                _cache.Remove(Settings.Q_Sys);

                return true;
            }
        }

        public QuestionSystemSettingsVM GetQuestionSystemSettings(FAMEEntities db)
        {
            // Try to get from cache first
            var cachedSettings = _cache[Settings.Q_Sys] as QuestionSystemSettingsVM;
            if (cachedSettings != null)
            {
                return cachedSettings;
            }

            var db_tbl_Settings = db.tbl_Settings.Where(x => x.Name.Contains(Settings.Q_Sys)).ToList();
            var ret = new QuestionSystemSettingsVM()
            {
                FreeSystems = Convert.ToString(GetConfigByName(Settings.QSys.FreeSystems, db_tbl_Settings)),
                MockTestSystems = Convert.ToString(GetConfigByName(Settings.QSys.MockTestSystems, db_tbl_Settings)),
                TrialTestId = GetConfigByName<int?>(Settings.QSys.TrialTestId, db_tbl_Settings),
            };

            // Cache the result for future requests
            _cache.Set(Settings.Q_Sys, ret, DateTimeOffset.Now.Add(CacheExpiration));

            return ret;
        }

        public QuestionSystemSettingsVM GetQuestionSystemSettings()
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return GetQuestionSystemSettings(db);
            }
        }
        public bool SaveRegisterSettings(RegisterPageSettingsVM model)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                // Save each setting in the database
                SaveConfigByName(Settings.Register.DefaultPortal, model.DefaultPortal, db);
                SaveConfigByName(Settings.Register.AllowedPortals, model.AllowedPortals, db);
                SaveConfigByName(Settings.Register.CanChange, model.CanChangePortal, db);
                SaveConfigByName(Settings.Register.TutorialLink, model.TutorialLink, db);
                SaveConfigByName(Settings.Register.PaymentInfo, model.PaymentInfo, db);
                SaveConfigByName(Settings.Register.AdminContacts, model.AdminContacts, db);

                // Commit changes to the database
                db.SaveChanges();

                // Clear cache for Register Page settings to ensure they are reloaded after saving
                _cache.Remove(Settings._Register);

                return true;
            }
        }
        public RegisterPageSettingsVM GetRegisterSettings(FAMEEntities db)
        {
            // Try to get from cache first
            var cachedSettings = _cache[Settings._Register] as RegisterPageSettingsVM;
            if (cachedSettings != null)
            {
                return cachedSettings;
            }

            // If not in cache, fetch from database
            var db_tbl_Settings = db.tbl_Settings.Where(x => x.Name.Contains(Settings._Register)).ToList();
            var ret = new RegisterPageSettingsVM()
            {
                DefaultPortal = GetConfigByName<int?>(Settings.Register.DefaultPortal, db_tbl_Settings),
                AllowedPortals = GetConfigByName<string>(Settings.Register.AllowedPortals, db_tbl_Settings),
                CanChangePortal = GetConfigByName<bool>(Settings.Register.CanChange, db_tbl_Settings),
                TutorialLink = GetConfigByName<string>(Settings.Register.TutorialLink, db_tbl_Settings),
                PaymentInfo = GetConfigByName<string>(Settings.Register.PaymentInfo, db_tbl_Settings),
                AdminContacts = GetConfigByName<string>(Settings.Register.AdminContacts, db_tbl_Settings),
            };

            // Cache the result for future requests
            _cache.Set(Settings._Register, ret, DateTimeOffset.Now.Add(CacheExpiration));

            return ret;
        }

        public RegisterPageSettingsVM GetRegisterSettings()
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                return GetRegisterSettings(db);
            }
        }

        // Similar changes can be made to other methods like SaveRegisterSettings and GetRegisterSettings

        private T GetConfigByName<T>(string name, List<tbl_Settings> db_tbl_Settings)
        {
            var setting = db_tbl_Settings.FirstOrDefault(x => x.Name == name);
            if (setting != null)
            {
                if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // Handle nullable types
                    if (setting.Value == null)
                    {
                        return default(T);
                    }
                    else
                    {
                        var underlyingType = Nullable.GetUnderlyingType(typeof(T));
                        return (T)Convert.ChangeType(setting.Value, underlyingType);
                    }
                }
                else
                {
                    return (T)Convert.ChangeType(setting.Value, typeof(T));
                }
            }
            else
            {
                return default(T);
            }
        }

        private object SaveConfigByName(string Name, object value, FAMEEntities db)
        {
            var a = db.tbl_Settings.FirstOrDefault(x => x.Name == Name) ?? new tbl_Settings() { Name = Name };
            a.Value = value?.ToString();
            if (a.ID == 0) db.tbl_Settings.Add(a);
            return true;
        }

        private object GetConfigByName(string Name, List<tbl_Settings> db_tbl_Settings)
        {
            var a = db_tbl_Settings.FirstOrDefault(x => x.Name == Name)?.Value;
            return a;
        }

        public string GetConfigByName(string Name)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var a = db.tbl_Settings.FirstOrDefault(x => x.Name == Name)?.Value;
                return a;
            }
        }
    }
}
