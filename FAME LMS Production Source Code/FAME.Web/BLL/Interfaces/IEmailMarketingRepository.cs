using First_Aid_Made_Easy.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace First_Aid_Made_Easy.BLL.Interfaces
{
    public interface IEmailMarketingRepository
    {
        // ── Sender Accounts ──────────────────────────────────────
        List<SenderAccountVM> GetSenderAccounts();
        SenderAccountVM GetSenderAccount(int id);
        SenderAccountVM GetDefaultSenderAccount();
        int SaveSenderAccount(SenderAccountVM model, string userId);
        bool DeleteSenderAccount(int id);
        bool SetDefaultSenderAccount(int id);
        Task<bool> TestSenderAccount(int id, string testEmail);
        int ImportExistingSmtpSettings(string userId);
        Task<List<SenderAccountVM>> SyncFromBrevoApi(string apiKey, string userId);
        Task<bool> SendQuickTestEmail(int senderId, string testEmail, string subject, string htmlBody);

        // ── Templates ────────────────────────────────────────────
        List<EmailTemplateVM> GetTemplates(string category = null);
        EmailTemplateVM GetTemplate(int id);
        int SaveTemplate(EmailTemplateVM model, string userId);
        bool DeleteTemplate(int id);

        // ── Audiences / Segments ─────────────────────────────────
        List<AudienceSegmentVM> GetAudiences();
        AudienceSegmentVM GetAudience(int id);
        int SaveAudience(AudienceSegmentVM model, string userId);
        bool DeleteAudience(int id);
        AudiencePreviewVM PreviewAudience(AudienceFilterVM filter, int sampleSize = 10);
        List<AudienceRecipientVM> GetAudienceRecipients(AudienceFilterVM filter);
        int GetAudienceCount(AudienceFilterVM filter);
        List<AudienceRecipientVM> GetRecipientsByPackage(int? packageId, string enrollmentStatus = "All");
        List<AudienceRecipientVM> GetRecipientsByCourse(int? courseId, string enrollmentStatus = "All");

        // ── Packages & Courses (for dropdowns) ──────────────────
        List<System.Web.Mvc.SelectListItem> GetPackagesDropdown();
        List<System.Web.Mvc.SelectListItem> GetCoursesDropdown();

        // ── Campaigns ────────────────────────────────────────────
        List<CampaignVM> GetCampaigns(string status = null, int page = 1, int pageSize = 20);
        CampaignVM GetCampaign(int id);
        CampaignCreateVM GetCampaignForEdit(int id);
        int SaveCampaign(CampaignCreateVM model, string userId);
        bool DeleteCampaign(int id);
        bool UpdateCampaignStatus(int id, string status);
        Task<bool> SendCampaignAsync(int campaignId);
        Task<bool> SendTestEmailAsync(int campaignId, string testEmail);
        bool PauseCampaign(int id);
        bool ResumeCampaign(int id);
        int DuplicateCampaign(int id, string userId);
        Task<bool> ResendToNonOpenersAsync(int campaignId, string newSubject = null);

        // ── Campaign Stats ───────────────────────────────────────
        CampaignStatsVM GetCampaignStats(int id, int recipientPage = 1, int recipientPageSize = 50);
        CampaignDashboardVM GetDashboard();
        List<CampaignLinkStatsVM> GetCampaignLinks(int campaignId);
        List<CampaignRecipientVM> GetCampaignRecipients(int campaignId, string statusFilter = null, int page = 1, int pageSize = 50);

        // ── Tracking ─────────────────────────────────────────────
        void TrackOpen(long recipientId);
        string TrackClick(int linkId, long recipientId, string userAgent, string ipAddress);
        
        // ── Unsubscribe ──────────────────────────────────────────
        bool IsUnsubscribed(string email);
        bool ProcessUnsubscribe(string email, string reason, int? campaignId);
        bool Resubscribe(string email);
        UnsubscribeReportVM GetUnsubscribeReport(int page = 1, int pageSize = 50);

        // ── Drip Campaigns ───────────────────────────────────────
        List<DripCampaignVM> GetDripCampaigns();
        DripCampaignVM GetDripCampaign(int id);
        int SaveDripCampaign(DripCampaignVM model, string userId);
        bool DeleteDripCampaign(int id);
        bool ToggleDripCampaign(int id);
        void EnqueueDripForUser(string userId, string email, string triggerType);

        // ── Utilities ────────────────────────────────────────────
        string GenerateUnsubscribeToken(string email, int? campaignId);
        string DecodeUnsubscribeToken(string token);
        string ProcessMergeTags(string html, AudienceRecipientVM recipient, int? campaignId = null);
    }
}
