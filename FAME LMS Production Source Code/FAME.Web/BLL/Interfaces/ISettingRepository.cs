using First_Aid_Made_Easy.BLL.JzTimer;
using First_Aid_Made_Easy.DAL;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface ISettingRepository
    {
        object SaveEmailSettings(EmailSettingsVM model);
        EmailSettingsVM GetEmailSettings();
        object SaveSMTPSettings(SmtpSettingsVM model);
        SmtpSettingsVM GetSMTPSettings();
        object SaveQuestionSystemSettings(QuestionSystemSettingsVM model);
        QuestionSystemSettingsVM GetQuestionSystemSettings(FAMEEntities db);
        QuestionSystemSettingsVM GetQuestionSystemSettings();
        bool SaveRegisterSettings(RegisterPageSettingsVM model);
        RegisterPageSettingsVM GetRegisterSettings(FAMEEntities db);
        RegisterPageSettingsVM GetRegisterSettings();
        string GetConfigByName(string name);
    }
}
