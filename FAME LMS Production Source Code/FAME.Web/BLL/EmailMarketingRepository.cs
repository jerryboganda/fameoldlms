using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.BLL
{
    public class EmailMarketingRepository : IEmailMarketingRepository
    {
        private static readonly SemaphoreSlim _sendLock = new SemaphoreSlim(5, 5); // Max 5 concurrent sends
        private const int SEND_DELAY_MS = 100; // Throttle between sends

        // =============================================
        // SENDER ACCOUNTS
        // =============================================

        public List<SenderAccountVM> GetSenderAccounts()
        {
            using (var db = new EmailMarketingDbContext())
            {
                return db.tbl_EmailSenderAccounts
                    .Where(a => a.IsActive)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenBy(a => a.AccountName)
                    .Select(a => new SenderAccountVM
                    {
                        Id = a.Id,
                        AccountName = a.AccountName,
                        FromEmail = a.FromEmail,
                        FromName = a.FromName,
                        SmtpHost = a.SmtpHost,
                        SmtpPort = a.SmtpPort,
                        SmtpUsername = a.SmtpUsername,
                        EnableSSL = a.EnableSSL,
                        UseApi = a.UseApi,
                        ApiKey = a.ApiKey,
                        ApiProvider = a.ApiProvider,
                        DailyLimit = a.DailyLimit,
                        SentToday = a.SentToday,
                        IsDefault = a.IsDefault,
                        IsActive = a.IsActive,
                        CreatedAt = a.CreatedAt
                    }).ToList();
            }
        }

        public SenderAccountVM GetSenderAccount(int id)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var a = db.tbl_EmailSenderAccounts.Find(id);
                if (a == null) return null;
                return MapSenderAccount(a);
            }
        }

        public SenderAccountVM GetDefaultSenderAccount()
        {
            using (var db = new EmailMarketingDbContext())
            {
                var a = db.tbl_EmailSenderAccounts.FirstOrDefault(x => x.IsDefault && x.IsActive)
                     ?? db.tbl_EmailSenderAccounts.FirstOrDefault(x => x.IsActive);
                if (a == null) return null;
                return MapSenderAccount(a);
            }
        }

        public int SaveSenderAccount(SenderAccountVM model, string userId)
        {
            using (var db = new EmailMarketingDbContext())
            {
                tbl_EmailSenderAccounts entity;
                if (model.Id > 0)
                {
                    entity = db.tbl_EmailSenderAccounts.Find(model.Id);
                    if (entity == null) return 0;
                    entity.UpdatedAt = DateTime.Now;
                }
                else
                {
                    entity = new tbl_EmailSenderAccounts { CreatedAt = DateTime.Now, CreatedBy = userId };
                    db.tbl_EmailSenderAccounts.Add(entity);
                }

                entity.AccountName = model.AccountName;
                entity.FromEmail = model.FromEmail;
                entity.FromName = model.FromName;
                entity.SmtpHost = model.SmtpHost;
                entity.SmtpPort = model.SmtpPort;
                entity.SmtpUsername = model.SmtpUsername;
                if (!string.IsNullOrEmpty(model.SmtpPassword))
                    entity.SmtpPassword = model.SmtpPassword;
                entity.EnableSSL = model.EnableSSL;
                entity.UseApi = model.UseApi;
                if (!string.IsNullOrEmpty(model.ApiKey))
                    entity.ApiKey = model.ApiKey;
                entity.ApiProvider = model.ApiProvider;
                entity.DailyLimit = model.DailyLimit;
                entity.IsActive = model.IsActive;

                db.SaveChanges();
                return entity.Id;
            }
        }

        public bool DeleteSenderAccount(int id)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var entity = db.tbl_EmailSenderAccounts.Find(id);
                if (entity == null || entity.IsDefault) return false;
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.Now;
                db.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Import the existing SMTP/Brevo settings from tbl_Settings into tbl_EmailSenderAccounts.
        /// Returns the number of senders imported (0 if already imported or no settings found).
        /// </summary>
        public int ImportExistingSmtpSettings(string userId)
        {
            using (var db = new EmailMarketingDbContext())
            {
                // Skip if we already have senders
                if (db.tbl_EmailSenderAccounts.Any()) return 0;

                // Read existing SMTP settings from tbl_Settings via the SettingRepository
                var settingsRepo = new SettingRepository();
                var smtp = settingsRepo.GetSMTPSettings();
                if (smtp == null || string.IsNullOrWhiteSpace(smtp.FromEmail)) return 0;

                var entity = new tbl_EmailSenderAccounts
                {
                    AccountName = string.IsNullOrWhiteSpace(smtp.FromName) ? "Primary Sender" : smtp.FromName,
                    FromEmail = smtp.FromEmail,
                    FromName = smtp.FromName ?? "First Aid Made Easy",
                    SmtpHost = smtp.Host,
                    SmtpPort = int.TryParse(smtp.Port, out var port) ? port : 587,
                    SmtpUsername = smtp.Username,
                    SmtpPassword = smtp.Password,
                    EnableSSL = smtp.SSL,
                    UseApi = smtp.UseApi,
                    ApiKey = smtp.ApiKey,
                    ApiProvider = smtp.UseApi ? "Brevo" : null,
                    DailyLimit = 500,
                    SentToday = 0,
                    IsDefault = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                };
                db.tbl_EmailSenderAccounts.Add(entity);
                db.SaveChanges();
                return 1;
            }
        }

        /// <summary>
        /// Call Brevo GET /v3/senders API to fetch all verified senders and import them.
        /// Skips senders whose email already exists in tbl_EmailSenderAccounts.
        /// </summary>
        public async Task<List<SenderAccountVM>> SyncFromBrevoApi(string apiKey, string userId)
        {
            var imported = new List<SenderAccountVM>();
            if (string.IsNullOrWhiteSpace(apiKey)) return imported;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("api-key", apiKey);
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.GetAsync("https://api.brevo.com/v3/senders");
                if (!response.IsSuccessStatusCode) return imported;

                var json = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(json);
                if (result?.senders == null) return imported;

                using (var db = new EmailMarketingDbContext())
                {
                    var existingEmails = db.tbl_EmailSenderAccounts
                        .Select(a => a.FromEmail.ToLower()).ToList();
                    bool hasDefault = db.tbl_EmailSenderAccounts.Any(a => a.IsDefault);
                    bool isFirst = true;

                    foreach (var s in result.senders)
                    {
                        string email = (string)s.email;
                        string name = (string)s.name;
                        if (string.IsNullOrWhiteSpace(email)) continue;
                        if (existingEmails.Contains(email.ToLower())) continue;

                        var entity = new tbl_EmailSenderAccounts
                        {
                            AccountName = name ?? email.Split('@')[0],
                            FromEmail = email,
                            FromName = name ?? email.Split('@')[0],
                            UseApi = true,
                            ApiKey = apiKey,
                            ApiProvider = "Brevo",
                            DailyLimit = 300,
                            SentToday = 0,
                            IsDefault = !hasDefault && isFirst,
                            IsActive = true,
                            CreatedAt = DateTime.Now,
                            CreatedBy = userId
                        };
                        db.tbl_EmailSenderAccounts.Add(entity);
                        existingEmails.Add(email.ToLower());
                        isFirst = false;
                    }

                    db.SaveChanges();

                    // Return the full refreshed list
                    imported = db.tbl_EmailSenderAccounts
                        .Where(a => a.IsActive)
                        .OrderByDescending(a => a.IsDefault)
                        .ThenBy(a => a.AccountName)
                        .Select(a => new SenderAccountVM
                        {
                            Id = a.Id,
                            AccountName = a.AccountName,
                            FromEmail = a.FromEmail,
                            UseApi = a.UseApi,
                            IsDefault = a.IsDefault,
                            IsActive = a.IsActive
                        }).ToList();
                }
            }
            return imported;
        }

        public bool SetDefaultSenderAccount(int id)
        {
            using (var db = new EmailMarketingDbContext())
            {
                // Remove default from all
                var allAccounts = db.tbl_EmailSenderAccounts.Where(a => a.IsDefault).ToList();
                foreach (var a in allAccounts) a.IsDefault = false;

                var target = db.tbl_EmailSenderAccounts.Find(id);
                if (target == null) return false;
                target.IsDefault = true;
                target.UpdatedAt = DateTime.Now;
                db.SaveChanges();
                return true;
            }
        }

        public async Task<bool> TestSenderAccount(int id, string testEmail)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var account = db.tbl_EmailSenderAccounts.Find(id);
                if (account == null) return false;

                var subject = "FAME LMS - Test Email";
                var body = "<div style='font-family:Arial;padding:20px;'><h2>Test Email</h2><p>This is a test email from FAME LMS Email Marketing System.</p><p>Sender Account: <strong>" + account.AccountName + "</strong></p><p>Sent at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "</p></div>";

                await SendSingleEmailAsync(account, testEmail, subject, body);
                return true;
            }
        }

        public async Task<bool> SendQuickTestEmail(int senderId, string testEmail, string subject, string htmlBody)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var account = db.tbl_EmailSenderAccounts.Find(senderId);
                if (account == null)
                {
                    // Fallback: use the default sender
                    account = db.tbl_EmailSenderAccounts.FirstOrDefault(a => a.IsDefault && a.IsActive)
                           ?? db.tbl_EmailSenderAccounts.FirstOrDefault(a => a.IsActive);
                }
                if (account == null) throw new Exception("No sender accounts configured. Please add a sender first.");

                if (string.IsNullOrWhiteSpace(subject)) subject = "FAME LMS - Test Email";
                if (string.IsNullOrWhiteSpace(htmlBody))
                    htmlBody = "<div style='font-family:Arial;padding:20px;'><h2>Test Email</h2><p>This is a quick test email from FAME LMS.</p></div>";

                await SendSingleEmailAsync(account, testEmail, "[TEST] " + subject, htmlBody);
                return true;
            }
        }

        // =============================================
        // TEMPLATES
        // =============================================

        public List<EmailTemplateVM> GetTemplates(string category = null)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var query = db.tbl_EmailTemplates.Where(t => t.IsActive);
                if (!string.IsNullOrEmpty(category))
                    query = query.Where(t => t.Category == category);

                return query.OrderBy(t => t.TemplateName)
                    .Select(t => new EmailTemplateVM
                    {
                        Id = t.Id,
                        TemplateName = t.TemplateName,
                        Subject = t.Subject,
                        Category = t.Category,
                        ThumbnailPath = t.ThumbnailPath,
                        IsSystem = t.IsSystem,
                        IsActive = t.IsActive,
                        CreatedAt = t.CreatedAt
                    }).ToList();
            }
        }

        public EmailTemplateVM GetTemplate(int id)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var t = db.tbl_EmailTemplates.Find(id);
                if (t == null) return null;
                return new EmailTemplateVM
                {
                    Id = t.Id,
                    TemplateName = t.TemplateName,
                    Subject = t.Subject,
                    HtmlBody = t.HtmlBody,
                    JsonDesign = t.JsonDesign,
                    ThumbnailPath = t.ThumbnailPath,
                    Category = t.Category,
                    IsSystem = t.IsSystem,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt
                };
            }
        }

        public int SaveTemplate(EmailTemplateVM model, string userId)
        {
            using (var db = new EmailMarketingDbContext())
            {
                tbl_EmailTemplates entity;
                if (model.Id > 0)
                {
                    entity = db.tbl_EmailTemplates.Find(model.Id);
                    if (entity == null) return 0;
                    entity.UpdatedAt = DateTime.Now;
                }
                else
                {
                    entity = new tbl_EmailTemplates { CreatedAt = DateTime.Now, CreatedBy = userId };
                    db.tbl_EmailTemplates.Add(entity);
                }

                entity.TemplateName = model.TemplateName;
                entity.Subject = model.Subject;
                entity.HtmlBody = model.HtmlBody;
                entity.JsonDesign = model.JsonDesign;
                entity.ThumbnailPath = model.ThumbnailPath;
                entity.Category = model.Category ?? "Marketing";
                entity.IsActive = true;

                db.SaveChanges();
                return entity.Id;
            }
        }

        public bool DeleteTemplate(int id)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var entity = db.tbl_EmailTemplates.Find(id);
                if (entity == null || entity.IsSystem) return false;
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.Now;
                db.SaveChanges();
                return true;
            }
        }

        // =============================================
        // AUDIENCES / SEGMENTS
        // =============================================

        public List<AudienceSegmentVM> GetAudiences()
        {
            using (var db = new EmailMarketingDbContext())
            {
                return db.tbl_EmailAudiences
                    .Where(a => a.IsActive)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenBy(a => a.SegmentName)
                    .Select(a => new AudienceSegmentVM
                    {
                        Id = a.Id,
                        SegmentName = a.SegmentName,
                        Description = a.Description,
                        FilterJson = a.FilterJson,
                        RecipientCount = a.RecipientCount,
                        IsDefault = a.IsDefault,
                        IsActive = a.IsActive,
                        CreatedAt = a.CreatedAt
                    }).ToList();
            }
        }

        public AudienceSegmentVM GetAudience(int id)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var a = db.tbl_EmailAudiences.Find(id);
                if (a == null) return null;
                var vm = new AudienceSegmentVM
                {
                    Id = a.Id,
                    SegmentName = a.SegmentName,
                    Description = a.Description,
                    FilterJson = a.FilterJson,
                    RecipientCount = a.RecipientCount,
                    IsDefault = a.IsDefault,
                    IsActive = a.IsActive,
                    CreatedAt = a.CreatedAt
                };
                if (!string.IsNullOrEmpty(a.FilterJson))
                    vm.Filter = JsonConvert.DeserializeObject<AudienceFilterVM>(a.FilterJson);
                return vm;
            }
        }

        public int SaveAudience(AudienceSegmentVM model, string userId)
        {
            using (var db = new EmailMarketingDbContext())
            {
                tbl_EmailAudiences entity;
                if (model.Id > 0)
                {
                    entity = db.tbl_EmailAudiences.Find(model.Id);
                    if (entity == null) return 0;
                    entity.UpdatedAt = DateTime.Now;
                }
                else
                {
                    entity = new tbl_EmailAudiences { CreatedAt = DateTime.Now, CreatedBy = userId };
                    db.tbl_EmailAudiences.Add(entity);
                }

                entity.SegmentName = model.SegmentName;
                entity.Description = model.Description;
                entity.FilterJson = model.Filter != null ? JsonConvert.SerializeObject(model.Filter) : model.FilterJson;
                entity.RecipientCount = GetAudienceCount(model.Filter ?? JsonConvert.DeserializeObject<AudienceFilterVM>(model.FilterJson));
                entity.IsActive = true;

                db.SaveChanges();
                return entity.Id;
            }
        }

        public bool DeleteAudience(int id)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var entity = db.tbl_EmailAudiences.Find(id);
                if (entity == null || entity.IsDefault) return false;
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.Now;
                db.SaveChanges();
                return true;
            }
        }

        public AudiencePreviewVM PreviewAudience(AudienceFilterVM filter, int sampleSize = 10)
        {
            var recipients = GetAudienceRecipients(filter);
            return new AudiencePreviewVM
            {
                TotalCount = recipients.Count,
                SampleRecipients = recipients.Take(sampleSize).ToList()
            };
        }

        public List<AudienceRecipientVM> GetAudienceRecipients(AudienceFilterVM filter)
        {
            using (var db = new EmailMarketingDbContext())
            {
                filter = filter ?? new AudienceFilterVM();
                var now = DateTime.Now;
                var normalizedEnrollmentStatus = NormalizeEnrollmentStatus(filter.EnrollmentStatus);

                var usersQuery = db.Users.AsQueryable();
                var profilesQuery = db.UserProfiles.AsQueryable();

                // Filter active users with email
                usersQuery = usersQuery.Where(u => u.Email != null && u.Email != "");

                if (filter.EmailConfirmed.HasValue)
                    usersQuery = usersQuery.Where(u => u.EmailConfirmed == filter.EmailConfirmed.Value);

                if (filter.IsActive.HasValue)
                    usersQuery = usersQuery.Where(u => u.IsActive.HasValue && u.IsActive.Value == filter.IsActive.Value);

                // Join users with profiles
                var query = from u in usersQuery
                            join p in profilesQuery on u.Id equals p.User_AspUser into profiles
                            from p in profiles.DefaultIfEmpty()
                            select new { User = u, Profile = p };

                // Apply profile-level filters
                if (filter.Universities != null && filter.Universities.Any())
                {
                    var uniIds = filter.Universities.Select(x => {
                        int id;
                        return int.TryParse(x, out id) ? id : 0;
                    }).Where(x => x > 0).ToList();
                    if (uniIds.Any())
                        query = query.Where(x => x.Profile != null && x.Profile.Type.HasValue && uniIds.Contains(x.Profile.Type.Value));
                }

                if (filter.ExamTypes != null && filter.ExamTypes.Any())
                    query = query.Where(x => x.Profile != null && filter.ExamTypes.Contains(x.Profile.ExamType));

                if (filter.Countries != null && filter.Countries.Any())
                {
                    var countryIds = filter.Countries.Select(x => {
                        int id;
                        return int.TryParse(x, out id) ? id : 0;
                    }).Where(x => x > 0).ToList();
                    if (countryIds.Any())
                        query = query.Where(x => x.Profile != null && x.Profile.CountryID.HasValue && countryIds.Contains(x.Profile.CountryID.Value));
                }

                if (filter.Cities != null && filter.Cities.Any())
                    query = query.Where(x => x.Profile != null && filter.Cities.Contains(x.Profile.City));

                if (filter.RegisteredAfter.HasValue)
                    query = query.Where(x => x.Profile != null && x.Profile.CreateDT >= filter.RegisteredAfter.Value);

                if (filter.RegisteredBefore.HasValue)
                    query = query.Where(x => x.Profile != null && x.Profile.CreateDT <= filter.RegisteredBefore.Value);

                var packageIds = new List<int>();
                if (filter.Packages != null && filter.Packages.Any())
                {
                    packageIds = filter.Packages.Select(x => {
                        int id;
                        return int.TryParse(x, out id) ? id : 0;
                    }).Where(x => x > 0).ToList();
                }

                var courseIds = new List<int>();
                if (filter.Courses != null && filter.Courses.Any())
                {
                    courseIds = filter.Courses.Select(x => {
                        int id;
                        return int.TryParse(x, out id) ? id : 0;
                    }).Where(x => x > 0).ToList();
                }

                if (packageIds.Any())
                {
                    var packageEnrollmentQuery = db.EnrollmentMasters
                        .Where(e => e.StudentFid != null && e.StudentFid != "" && e.PackageId.HasValue && packageIds.Contains(e.PackageId.Value));
                    packageEnrollmentQuery = ApplyEnrollmentStatusFilter(packageEnrollmentQuery, normalizedEnrollmentStatus, now);
                    var packageStudentIdsQuery = packageEnrollmentQuery.Select(e => e.StudentFid).Distinct();
                    query = query.Where(x => packageStudentIdsQuery.Contains(x.User.Id));
                }

                if (courseIds.Any())
                {
                    var courseEnrollmentQuery = from ed in db.EnrollmentDetails
                                                join em in db.EnrollmentMasters on ed.EnrollmentID equals em.Enrollment_Id
                                                where em.StudentFid != null
                                                   && em.StudentFid != ""
                                                   && ed.CourseID.HasValue
                                                   && courseIds.Contains(ed.CourseID.Value)
                                                select new { Detail = ed, Master = em };

                    switch (normalizedEnrollmentStatus)
                    {
                        case "Active":
                            courseEnrollmentQuery = courseEnrollmentQuery.Where(x =>
                                (x.Detail.IsExpired == null || x.Detail.IsExpired == false) &&
                                (x.Master.IsExpired == null || x.Master.IsExpired == false) &&
                                x.Master.Enrollment_EndDate >= now);
                            break;
                        case "Expired":
                            courseEnrollmentQuery = courseEnrollmentQuery.Where(x =>
                                x.Detail.IsExpired == true ||
                                x.Master.IsExpired == true ||
                                x.Master.Enrollment_EndDate < now);
                            break;
                    }

                    var courseStudentIdsQuery = courseEnrollmentQuery.Select(x => x.Master.StudentFid).Distinct();
                    query = query.Where(x => courseStudentIdsQuery.Contains(x.User.Id));
                }

                if (!packageIds.Any() && !courseIds.Any() && normalizedEnrollmentStatus != "All")
                {
                    var statusEnrollmentQuery = ApplyEnrollmentStatusFilter(
                        db.EnrollmentMasters.Where(e => e.StudentFid != null && e.StudentFid != ""),
                        normalizedEnrollmentStatus,
                        now);
                    var statusStudentIdsQuery = statusEnrollmentQuery.Select(e => e.StudentFid).Distinct();
                    query = query.Where(x => statusStudentIdsQuery.Contains(x.User.Id));
                }

                if (filter.ExcludeUnsubscribed)
                {
                    query = query.Where(x => !db.tbl_EmailUnsubscribes.Any(u => !u.IsResubscribed && u.Email == x.User.Email));
                }

                var results = query.Select(x => new AudienceRecipientVM
                {
                    UserId = x.User.Id,
                    Email = x.User.Email,
                    Name = x.Profile != null ? x.Profile.User_Name : x.User.UserName,
                    University = x.Profile != null ? x.Profile.Institute : null,
                    ExamType = x.Profile != null ? x.Profile.ExamType : null,
                    City = x.Profile != null ? x.Profile.City : null
                }).ToList();

                return DedupeRecipientsByEmail(results);
            }
        }

        public int GetAudienceCount(AudienceFilterVM filter)
        {
            return GetAudienceRecipients(filter).Count;
        }

        /// <summary>
        /// Get all students enrolled in a specific package (or all packages if packageId is null).
        /// Returns distinct recipients with valid emails.
        /// </summary>
        public List<AudienceRecipientVM> GetRecipientsByPackage(int? packageId, string enrollmentStatus = "All")
        {
            using (var db = new EmailMarketingDbContext())
            {
                var now = DateTime.Now;
                var normalizedStatus = NormalizeEnrollmentStatus(enrollmentStatus);

                var enrollmentQuery = db.EnrollmentMasters
                    .Where(e => e.StudentFid != null && e.StudentFid != "" && e.PackageId.HasValue);

                if (packageId.HasValue && packageId.Value > 0)
                    enrollmentQuery = enrollmentQuery.Where(e => e.PackageId == packageId.Value);
                enrollmentQuery = ApplyEnrollmentStatusFilter(enrollmentQuery, normalizedStatus, now);

                var studentIdsQuery = enrollmentQuery.Select(e => e.StudentFid).Distinct();

                var results = (from u in db.Users
                               join p in db.UserProfiles on u.Id equals p.User_AspUser into profiles
                               from p in profiles.DefaultIfEmpty()
                               where studentIdsQuery.Contains(u.Id)
                                     && u.Email != null
                                     && u.Email != ""
                                     && !db.tbl_EmailUnsubscribes.Any(un => !un.IsResubscribed && un.Email == u.Email)
                               select new AudienceRecipientVM
                               {
                                   UserId = u.Id,
                                   Email = u.Email,
                                   Name = p != null ? p.User_Name : u.UserName,
                                   University = p != null ? p.Institute : null,
                                   ExamType = p != null ? p.ExamType : null,
                                   City = p != null ? p.City : null
                               }).ToList();

                return DedupeRecipientsByEmail(results);
            }
        }

        /// <summary>
        /// Get all students enrolled in a specific course (or all courses if courseId is null).
        /// Joins tbl_EnrollmentDetail → tbl_EnrollmentMaster → AspNetUsers.
        /// </summary>
        public List<AudienceRecipientVM> GetRecipientsByCourse(int? courseId, string enrollmentStatus = "All")
        {
            using (var db = new EmailMarketingDbContext())
            {
                var now = DateTime.Now;
                var normalizedStatus = NormalizeEnrollmentStatus(enrollmentStatus);

                var courseEnrollmentQuery = from ed in db.EnrollmentDetails
                                            join em in db.EnrollmentMasters on ed.EnrollmentID equals em.Enrollment_Id
                                            where em.StudentFid != null
                                               && em.StudentFid != ""
                                               && ed.CourseID.HasValue
                                            select new { Detail = ed, Master = em };
                if (courseId.HasValue && courseId.Value > 0)
                    courseEnrollmentQuery = courseEnrollmentQuery.Where(x => x.Detail.CourseID == courseId.Value);

                switch (normalizedStatus)
                {
                    case "Active":
                        courseEnrollmentQuery = courseEnrollmentQuery.Where(x =>
                            (x.Detail.IsExpired == null || x.Detail.IsExpired == false) &&
                            (x.Master.IsExpired == null || x.Master.IsExpired == false) &&
                            x.Master.Enrollment_EndDate >= now);
                        break;
                    case "Expired":
                        courseEnrollmentQuery = courseEnrollmentQuery.Where(x =>
                            x.Detail.IsExpired == true ||
                            x.Master.IsExpired == true ||
                            x.Master.Enrollment_EndDate < now);
                        break;
                }

                var studentIdsQuery = courseEnrollmentQuery.Select(x => x.Master.StudentFid).Distinct();

                var results = (from u in db.Users
                               join p in db.UserProfiles on u.Id equals p.User_AspUser into profiles
                               from p in profiles.DefaultIfEmpty()
                               where studentIdsQuery.Contains(u.Id)
                                     && u.Email != null
                                     && u.Email != ""
                                     && !db.tbl_EmailUnsubscribes.Any(un => !un.IsResubscribed && un.Email == u.Email)
                               select new AudienceRecipientVM
                               {
                                   UserId = u.Id,
                                   Email = u.Email,
                                   Name = p != null ? p.User_Name : u.UserName,
                                   University = p != null ? p.Institute : null,
                                   ExamType = p != null ? p.ExamType : null,
                                   City = p != null ? p.City : null
                               }).ToList();

                return DedupeRecipientsByEmail(results);
            }
        }

        /// <summary>
        /// Get all active packages for the dropdown list.
        /// Includes "All Packages" as the first option.
        /// </summary>
        public List<System.Web.Mvc.SelectListItem> GetPackagesDropdown()
        {
            using (var db = new EmailMarketingDbContext())
            {
                var packages = db.Packages
                    .Where(p => p.IsActive == true)
                    .OrderBy(p => p.SortID)
                    .Select(p => new { p.PackageID, p.PackageName })
                    .ToList();

                var items = new List<System.Web.Mvc.SelectListItem>();
                items.Add(new System.Web.Mvc.SelectListItem { Value = "", Text = "-- Select Package --" });
                items.Add(new System.Web.Mvc.SelectListItem { Value = "0", Text = "All Packages" });
                foreach (var p in packages)
                    items.Add(new System.Web.Mvc.SelectListItem { Value = p.PackageID.ToString(), Text = p.PackageName });
                return items;
            }
        }

        /// <summary>
        /// Get all active courses for the dropdown list.
        /// Includes "All Courses" as the first option.
        /// </summary>
        public List<System.Web.Mvc.SelectListItem> GetCoursesDropdown()
        {
            using (var db = new EmailMarketingDbContext())
            {
                var courses = db.Courses
                    .Where(c => c.IsActive == true)
                    .OrderBy(c => c.SortID)
                    .Select(c => new { c.Course_Id, c.Course_Name })
                    .ToList();

                var items = new List<System.Web.Mvc.SelectListItem>();
                items.Add(new System.Web.Mvc.SelectListItem { Value = "", Text = "-- Select Course --" });
                items.Add(new System.Web.Mvc.SelectListItem { Value = "0", Text = "All Courses" });
                foreach (var c in courses)
                    items.Add(new System.Web.Mvc.SelectListItem { Value = c.Course_Id.ToString(), Text = c.Course_Name });
                return items;
            }
        }

        // =============================================
        // CAMPAIGNS
        // =============================================

        public List<CampaignVM> GetCampaigns(string status = null, int page = 1, int pageSize = 20)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var query = db.tbl_EmailCampaigns.Include(c => c.SenderAccount).AsQueryable();

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(c => c.Status == status);

                return query.OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CampaignVM
                    {
                        Id = c.Id,
                        CampaignName = c.CampaignName,
                        Subject = c.Subject,
                        Status = c.Status,
                        SenderName = c.SenderAccount.FromName,
                        SenderEmail = c.SenderAccount.FromEmail,
                        TotalRecipients = c.TotalRecipients,
                        TotalSent = c.TotalSent,
                        TotalFailed = c.TotalFailed,
                        TotalOpens = c.TotalOpens,
                        TotalClicks = c.TotalClicks,
                        UniqueOpens = c.UniqueOpens,
                        UniqueClicks = c.UniqueClicks,
                        TotalBounces = c.TotalBounces,
                        TotalUnsubscribes = c.TotalUnsubscribes,
                        ScheduledAt = c.ScheduledAt,
                        CompletedAt = c.CompletedAt,
                        CreatedAt = c.CreatedAt,
                        Tags = c.Tags
                    }).ToList();
            }
        }

        public CampaignVM GetCampaign(int id)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var c = db.tbl_EmailCampaigns.Include(x => x.SenderAccount).FirstOrDefault(x => x.Id == id);
                if (c == null) return null;
                return new CampaignVM
                {
                    Id = c.Id,
                    CampaignName = c.CampaignName,
                    Subject = c.Subject,
                    Status = c.Status,
                    SenderName = c.SenderAccount != null ? c.SenderAccount.FromName : "",
                    SenderEmail = c.SenderAccount != null ? c.SenderAccount.FromEmail : "",
                    TotalRecipients = c.TotalRecipients,
                    TotalSent = c.TotalSent,
                    TotalFailed = c.TotalFailed,
                    TotalOpens = c.TotalOpens,
                    TotalClicks = c.TotalClicks,
                    UniqueOpens = c.UniqueOpens,
                    UniqueClicks = c.UniqueClicks,
                    TotalBounces = c.TotalBounces,
                    TotalUnsubscribes = c.TotalUnsubscribes,
                    ScheduledAt = c.ScheduledAt,
                    CompletedAt = c.CompletedAt,
                    CreatedAt = c.CreatedAt,
                    Tags = c.Tags
                };
            }
        }

        public CampaignCreateVM GetCampaignForEdit(int id)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var c = db.tbl_EmailCampaigns.Find(id);
                if (c == null) return null;

                var vm = new CampaignCreateVM
                {
                    Id = c.Id,
                    CampaignName = c.CampaignName,
                    Subject = c.Subject,
                    PreheaderText = c.PreheaderText,
                    FromAccountId = c.FromAccountId,
                    TemplateId = c.TemplateId,
                    HtmlBody = c.HtmlBody,
                    AudienceId = c.AudienceId,
                    TargetingMode = string.IsNullOrEmpty(c.TargetingMode) ? "audience" : c.TargetingMode,
                    PackageId = c.PackageId,
                    CourseId = c.CourseId,
                    SingleEmail = c.SingleEmail,
                    ScheduledAt = c.ScheduledAt,
                    Tags = c.Tags,
                    ABTestEnabled = c.ABTestEnabled,
                    ABTestSplitPct = c.ABTestSplitPct ?? 20,
                    ABTestWinnerMetric = c.ABTestWinnerMetric ?? "OpenRate",
                    EnrollmentStatus = "All"
                };

                if (!string.IsNullOrWhiteSpace(c.AudienceFilterJson))
                {
                    try
                    {
                        var filter = JsonConvert.DeserializeObject<AudienceFilterVM>(c.AudienceFilterJson);
                        vm.EnrollmentStatus = NormalizeEnrollmentStatus(filter?.EnrollmentStatus);
                    }
                    catch
                    {
                        vm.EnrollmentStatus = "All";
                    }
                }

                PopulateCampaignDropdowns(vm, db);
                return vm;
            }
        }

        public int SaveCampaign(CampaignCreateVM model, string userId)
        {
            using (var db = new EmailMarketingDbContext())
            {
                tbl_EmailCampaigns entity;
                if (model.Id > 0)
                {
                    entity = db.tbl_EmailCampaigns.Find(model.Id);
                    if (entity == null) return 0;
                    if (entity.Status != "Draft") return 0; // Can only edit drafts
                    entity.UpdatedAt = DateTime.Now;
                }
                else
                {
                    entity = new tbl_EmailCampaigns
                    {
                        Status = "Draft",
                        CreatedAt = DateTime.Now,
                        CreatedBy = userId
                    };
                    db.tbl_EmailCampaigns.Add(entity);
                }

                entity.CampaignName = model.CampaignName;
                entity.Subject = model.Subject;
                entity.PreheaderText = model.PreheaderText;
                entity.FromAccountId = model.FromAccountId;
                entity.TemplateId = model.TemplateId;
                entity.HtmlBody = model.HtmlBody;
                entity.AudienceId = model.AudienceId;
                entity.Tags = model.Tags;
                entity.ABTestEnabled = model.ABTestEnabled;
                entity.ABTestSplitPct = model.ABTestSplitPct;
                entity.ABTestWinnerMetric = model.ABTestWinnerMetric;

                // Save targeting mode and related fields
                entity.TargetingMode = string.IsNullOrWhiteSpace(model.TargetingMode)
                    ? "audience"
                    : model.TargetingMode.Trim().ToLowerInvariant();
                entity.PackageId = model.PackageId;
                entity.CourseId = model.CourseId;
                entity.SingleEmail = model.SingleEmail;
                entity.AudienceFilterJson = null;

                // If audience selected, snapshot the filter
                if ((entity.TargetingMode == "audience" || string.IsNullOrEmpty(entity.TargetingMode)) && model.AudienceId.HasValue && model.AudienceId > 0)
                {
                    var audience = db.tbl_EmailAudiences.Find(model.AudienceId.Value);
                    if (audience != null)
                        entity.AudienceFilterJson = audience.FilterJson;
                }
                else if (entity.TargetingMode == "audience" && model.CustomFilter != null)
                {
                    entity.AudienceFilterJson = JsonConvert.SerializeObject(model.CustomFilter);
                }
                else if (entity.TargetingMode == "package" || entity.TargetingMode == "course")
                {
                    var normalizedStatus = NormalizeEnrollmentStatus(model.EnrollmentStatus);
                    entity.AudienceFilterJson = JsonConvert.SerializeObject(new AudienceFilterVM
                    {
                        EnrollmentStatus = normalizedStatus
                    });
                }

                // Calculate estimated recipients based on targeting mode
                switch (entity.TargetingMode)
                {
                    case "package":
                        entity.TotalRecipients = GetRecipientsByPackage(model.PackageId, NormalizeEnrollmentStatus(model.EnrollmentStatus)).Count;
                        break;
                    case "course":
                        entity.TotalRecipients = GetRecipientsByCourse(model.CourseId, NormalizeEnrollmentStatus(model.EnrollmentStatus)).Count;
                        break;
                    case "single":
                        entity.TotalRecipients = string.IsNullOrWhiteSpace(model.SingleEmail) ? 0 : 1;
                        break;
                    default: // "audience"
                        if (!string.IsNullOrEmpty(entity.AudienceFilterJson))
                        {
                            try
                            {
                                var filter = JsonConvert.DeserializeObject<AudienceFilterVM>(entity.AudienceFilterJson) ?? new AudienceFilterVM();
                                entity.TotalRecipients = GetAudienceCount(filter);
                            }
                            catch
                            {
                                entity.TotalRecipients = 0;
                            }
                        }
                        else entity.TotalRecipients = 0;
                        break;
                }

                if (model.ScheduledAt.HasValue)
                {
                    entity.ScheduledAt = model.ScheduledAt.Value;
                    entity.Status = "Scheduled";
                }

                db.SaveChanges();
                return entity.Id;
            }
        }

        public bool DeleteCampaign(int id)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var entity = db.tbl_EmailCampaigns.Find(id);
                if (entity == null) return false;
                if (entity.Status == "Sending") return false;
                entity.Status = "Cancelled";
                entity.UpdatedAt = DateTime.Now;
                db.SaveChanges();
                return true;
            }
        }

        public bool UpdateCampaignStatus(int id, string status)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var entity = db.tbl_EmailCampaigns.Find(id);
                if (entity == null) return false;
                entity.Status = status;
                entity.UpdatedAt = DateTime.Now;
                db.SaveChanges();
                return true;
            }
        }

        public async Task<bool> SendCampaignAsync(int campaignId)
        {
            // ─── Quick validation & status update (fast, non-blocking) ───
            string campaignSubject;
            string campaignHtmlBody;
            string targetingMode;
            string enrollmentStatus = "All";
            int? audienceId, packageId, courseId;
            string singleEmail, audienceFilterJson;
            int senderAccountId;

            using (var db = new EmailMarketingDbContext())
            {
                var campaign = db.tbl_EmailCampaigns.Find(campaignId);
                if (campaign == null) return false;
                if (campaign.Status != "Draft" && campaign.Status != "Scheduled") return false;

                var sender = db.tbl_EmailSenderAccounts.Find(campaign.FromAccountId);
                if (sender == null) return false;

                // Capture values for background use (avoid cross-context entity usage)
                campaignSubject = campaign.Subject;
                campaignHtmlBody = campaign.HtmlBody;
                targetingMode = string.IsNullOrEmpty(campaign.TargetingMode) ? "audience" : campaign.TargetingMode;
                audienceId = campaign.AudienceId;
                packageId = campaign.PackageId;
                courseId = campaign.CourseId;
                singleEmail = campaign.SingleEmail;
                audienceFilterJson = campaign.AudienceFilterJson;
                senderAccountId = campaign.FromAccountId;

                if (!string.IsNullOrWhiteSpace(audienceFilterJson))
                {
                    try
                    {
                        var filterSnapshot = JsonConvert.DeserializeObject<AudienceFilterVM>(audienceFilterJson);
                        enrollmentStatus = NormalizeEnrollmentStatus(filterSnapshot?.EnrollmentStatus);
                    }
                    catch
                    {
                        enrollmentStatus = "All";
                    }
                }

                // Set status immediately so UI updates
                campaign.Status = "Sending";
                campaign.SendStartedAt = DateTime.Now;
                db.SaveChanges();
            }

            // ─── Fire-and-forget: build recipients + send in background ───
            _ = Task.Run(async () =>
            {
                try
                {
                    List<AudienceRecipientVM> recipients;

                    // Build recipient list
                    switch (targetingMode)
                    {
                        case "package":
                            recipients = GetRecipientsByPackage(packageId, enrollmentStatus);
                            break;
                        case "course":
                            recipients = GetRecipientsByCourse(courseId, enrollmentStatus);
                            break;
                        case "single":
                            recipients = new List<AudienceRecipientVM>();
                            if (!string.IsNullOrEmpty(singleEmail))
                            {
                                using (var db2 = new EmailMarketingDbContext())
                                {
                                    var recipientVm = new AudienceRecipientVM { Email = singleEmail, Name = singleEmail };
                                    // Use projection to avoid materializing full entity (IsActive NULL safety)
                                    var userData = db2.Users
                                        .Where(u => u.Email == singleEmail)
                                        .Select(u => new { u.Id, u.UserName })
                                        .FirstOrDefault();
                                    if (userData != null)
                                    {
                                        recipientVm.UserId = userData.Id;
                                        var profile = db2.UserProfiles.FirstOrDefault(p => p.User_AspUser == userData.Id);
                                        if (profile != null)
                                        {
                                            recipientVm.Name = profile.User_Name ?? userData.UserName;
                                            recipientVm.University = profile.Institute;
                                            recipientVm.ExamType = profile.ExamType;
                                            recipientVm.City = profile.City;
                                        }
                                    }
                                    recipients.Add(recipientVm);
                                }
                            }
                            break;
                        default: // "audience"
                            AudienceFilterVM filter = null;
                            if (!string.IsNullOrEmpty(audienceFilterJson))
                            {
                                try
                                {
                                    filter = JsonConvert.DeserializeObject<AudienceFilterVM>(audienceFilterJson);
                                }
                                catch
                                {
                                    filter = null;
                                }
                            }
                            filter = filter ?? new AudienceFilterVM { Role = "Student" };
                            recipients = GetAudienceRecipients(filter);
                            break;
                    }

                    tbl_EmailSenderAccounts senderAccount;
                    using (var db = new EmailMarketingDbContext())
                    {
                        // Update total recipients
                        var campaignEntity = db.tbl_EmailCampaigns.Find(campaignId);
                        if (campaignEntity != null)
                        {
                            campaignEntity.TotalRecipients = recipients.Count;
                            db.SaveChanges();
                        }

                        senderAccount = db.tbl_EmailSenderAccounts.Find(senderAccountId);

                        // Batch-insert recipient records (100 per SaveChanges for performance)
                        int batchCount = 0;
                        foreach (var r in recipients)
                        {
                            db.tbl_EmailCampaignRecipients.Add(new tbl_EmailCampaignRecipients
                            {
                                CampaignId = campaignId,
                                RecipientEmail = r.Email,
                                RecipientName = r.Name,
                                UserId = r.UserId,
                                Status = "Queued",
                                MergeData = JsonConvert.SerializeObject(r)
                            });
                            batchCount++;
                            if (batchCount % 100 == 0)
                                db.SaveChanges();
                        }
                        if (batchCount % 100 != 0)
                            db.SaveChanges();

                        // Extract and store tracked links
                        var links = ExtractLinks(campaignHtmlBody);
                        foreach (var url in links)
                        {
                            var trackCode = Guid.NewGuid().ToString("N").Substring(0, 12);
                            db.tbl_EmailCampaignLinks.Add(new tbl_EmailCampaignLinks
                            {
                                CampaignId = campaignId,
                                OriginalUrl = url,
                                TrackingCode = trackCode
                            });
                        }
                        db.SaveChanges();
                    }

                    // ─── Send emails ─────────────────────────────────────
                    int sent = 0, failed = 0;
                    using (var db = new EmailMarketingDbContext())
                    {
                        var recipientEntities = db.tbl_EmailCampaignRecipients
                            .Where(r => r.CampaignId == campaignId && r.Status == "Queued")
                            .ToList();

                        var linkMap = db.tbl_EmailCampaignLinks
                            .Where(l => l.CampaignId == campaignId)
                            .ToDictionary(l => l.OriginalUrl, l => l);

                        foreach (var recipient in recipientEntities)
                        {
                            try
                            {
                                await _sendLock.WaitAsync();
                                try
                                {
                                    var recipientData = !string.IsNullOrEmpty(recipient.MergeData)
                                        ? JsonConvert.DeserializeObject<AudienceRecipientVM>(recipient.MergeData)
                                        : new AudienceRecipientVM { Email = recipient.RecipientEmail, Name = recipient.RecipientName };

                                    var html = ProcessMergeTags(campaignHtmlBody, recipientData, campaignId);

                                    // Inject tracking pixel
                                    var trackingPixel = $"<img src=\"https://firstaidmadeeasy.com.pk/EmailTracking/Open/{recipient.Id}\" width=\"1\" height=\"1\" style=\"display:none;\" />";
                                    html = html.Replace("</body>", trackingPixel + "</body>");
                                    if (!html.Contains("</body>"))
                                        html += trackingPixel;

                                    // Rewrite links for tracking
                                    foreach (var kvp in linkMap)
                                    {
                                        var trackUrl = $"https://firstaidmadeeasy.com.pk/EmailTracking/Click/{kvp.Value.Id}/{recipient.Id}";
                                        html = html.Replace($"href=\"{kvp.Key}\"", $"href=\"{trackUrl}\"");
                                        html = html.Replace($"href='{kvp.Key}'", $"href='{trackUrl}'");
                                    }

                                    await SendSingleEmailAsync(senderAccount, recipient.RecipientEmail, campaignSubject, html);

                                    recipient.Status = "Sent";
                                    recipient.SentAt = DateTime.Now;
                                    sent++;
                                }
                                finally
                                {
                                    _sendLock.Release();
                                }

                                await Task.Delay(SEND_DELAY_MS);
                            }
                            catch (Exception ex)
                            {
                                recipient.Status = "Failed";
                                recipient.ErrorMessage = ex.Message.Length > 500 ? ex.Message.Substring(0, 500) : ex.Message;
                                failed++;
                                JzLogger.WriteError(ex, $"EmailMarketing: Failed to send to {recipient.RecipientEmail}");
                            }

                            // Periodic save every 50 emails
                            if ((sent + failed) % 50 == 0)
                                db.SaveChanges();
                        }

                        db.SaveChanges();

                        // Update campaign stats
                        var finalCampaign = db.tbl_EmailCampaigns.Find(campaignId);
                        if (finalCampaign != null)
                        {
                            finalCampaign.TotalSent = sent;
                            finalCampaign.TotalFailed = failed;
                            finalCampaign.Status = "Sent";
                            finalCampaign.CompletedAt = DateTime.Now;
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Global error handler: mark campaign as Failed so it doesn't stay stuck in "Sending"
                    JzLogger.WriteError(ex, $"EmailMarketing: Campaign {campaignId} background task CRASHED");
                    try
                    {
                        using (var db = new EmailMarketingDbContext())
                        {
                            var c = db.tbl_EmailCampaigns.Find(campaignId);
                            if (c != null)
                            {
                                c.Status = "Failed";
                                c.CompletedAt = DateTime.Now;
                                // Count what actually sent
                                c.TotalSent = db.tbl_EmailCampaignRecipients.Count(r => r.CampaignId == campaignId && r.Status == "Sent");
                                c.TotalFailed = db.tbl_EmailCampaignRecipients.Count(r => r.CampaignId == campaignId && r.Status == "Failed");
                                db.SaveChanges();
                            }
                        }
                    }
                    catch (Exception innerEx)
                    {
                        JzLogger.WriteError(innerEx, $"EmailMarketing: Failed to mark campaign {campaignId} as Failed");
                    }
                }
            });

            return true;
        }

        public async Task<bool> SendTestEmailAsync(int campaignId, string testEmail)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var campaign = db.tbl_EmailCampaigns.Include(c => c.SenderAccount).FirstOrDefault(c => c.Id == campaignId);
                if (campaign == null) return false;

                var sampleRecipient = new AudienceRecipientVM
                {
                    Email = testEmail,
                    Name = "Test User",
                    University = "Test University",
                    ExamType = "FCPS",
                    City = "Lahore"
                };

                var html = ProcessMergeTags(campaign.HtmlBody, sampleRecipient, campaignId);
                await SendSingleEmailAsync(campaign.SenderAccount, testEmail, "[TEST] " + campaign.Subject, html);
                return true;
            }
        }

        public bool PauseCampaign(int id)
        {
            return UpdateCampaignStatus(id, "Paused");
        }

        public bool ResumeCampaign(int id)
        {
            return UpdateCampaignStatus(id, "Sending");
        }

        public int DuplicateCampaign(int id, string userId)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var source = db.tbl_EmailCampaigns.Find(id);
                if (source == null) return 0;

                var clone = new tbl_EmailCampaigns
                {
                    CampaignName = source.CampaignName + " (Copy)",
                    Subject = source.Subject,
                    PreheaderText = source.PreheaderText,
                    FromAccountId = source.FromAccountId,
                    TemplateId = source.TemplateId,
                    HtmlBody = source.HtmlBody,
                    AudienceId = source.AudienceId,
                    AudienceFilterJson = source.AudienceFilterJson,
                    Status = "Draft",
                    Tags = source.Tags,
                    ABTestEnabled = source.ABTestEnabled,
                    ABTestSplitPct = source.ABTestSplitPct,
                    ABTestWinnerMetric = source.ABTestWinnerMetric,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now
                };

                db.tbl_EmailCampaigns.Add(clone);
                db.SaveChanges();
                return clone.Id;
            }
        }

        public async Task<bool> ResendToNonOpenersAsync(int campaignId, string newSubject = null)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var original = db.tbl_EmailCampaigns.Find(campaignId);
                if (original == null || original.Status != "Sent") return false;

                // Get non-openers
                var nonOpeners = db.tbl_EmailCampaignRecipients
                    .Where(r => r.CampaignId == campaignId && r.Status == "Sent" && r.OpenCount == 0)
                    .Select(r => new { r.RecipientEmail, r.RecipientName, r.UserId, r.MergeData })
                    .ToList();

                if (!nonOpeners.Any()) return false;

                // Create new campaign as resend
                var resend = new tbl_EmailCampaigns
                {
                    CampaignName = original.CampaignName + " (Resend)",
                    Subject = newSubject ?? original.Subject,
                    PreheaderText = original.PreheaderText,
                    FromAccountId = original.FromAccountId,
                    TemplateId = original.TemplateId,
                    HtmlBody = original.HtmlBody,
                    AudienceFilterJson = original.AudienceFilterJson,
                    Status = "Draft",
                    TotalRecipients = nonOpeners.Count,
                    CreatedBy = original.CreatedBy,
                    CreatedAt = DateTime.Now
                };
                db.tbl_EmailCampaigns.Add(resend);
                db.SaveChanges();

                return await SendCampaignAsync(resend.Id);
            }
        }

        // =============================================
        // CAMPAIGN STATS
        // =============================================

        public CampaignStatsVM GetCampaignStats(int id, int recipientPage = 1, int recipientPageSize = 50)
        {
            var campaign = GetCampaign(id);
            if (campaign == null) return null;

            return new CampaignStatsVM
            {
                Campaign = campaign,
                TopLinks = GetCampaignLinks(id),
                Recipients = GetCampaignRecipients(id, null, recipientPage, recipientPageSize),
                CurrentPage = recipientPage,
                TotalRecipientPages = (int)Math.Ceiling((double)campaign.TotalRecipients / recipientPageSize),
                OpenTimelineJson = GetOpenTimeline(id),
                ClickTimelineJson = GetClickTimeline(id)
            };
        }

        public CampaignDashboardVM GetDashboard()
        {
            using (var db = new EmailMarketingDbContext())
            {
                var now = DateTime.Now;
                var monthStart = new DateTime(now.Year, now.Month, 1);

                var campaignQuery = db.tbl_EmailCampaigns.AsQueryable();

                var totalCampaigns = campaignQuery.Count();
                var totalEmailsSentThisMonth = campaignQuery
                    .Where(c => c.CompletedAt >= monthStart)
                    .Select(c => c.TotalSent)
                    .DefaultIfEmpty(0).Sum();

                var sentCampaignStats = campaignQuery
                    .Where(c => c.Status == "Sent" && c.TotalSent > 0)
                    .Select(c => new { c.TotalSent, c.UniqueOpens, c.UniqueClicks })
                    .ToList();

                decimal avgOpenRate = 0, avgClickRate = 0;
                if (sentCampaignStats.Any())
                {
                    avgOpenRate = Math.Round(sentCampaignStats.Average(c => c.TotalSent > 0 ? (decimal)c.UniqueOpens / c.TotalSent * 100 : 0), 1);
                    avgClickRate = Math.Round(sentCampaignStats.Average(c => c.TotalSent > 0 ? (decimal)c.UniqueClicks / c.TotalSent * 100 : 0), 1);
                }

                return new CampaignDashboardVM
                {
                    Campaigns = GetCampaigns(),
                    TotalCampaigns = totalCampaigns,
                    TotalEmailsSentThisMonth = totalEmailsSentThisMonth,
                    AverageOpenRate = avgOpenRate,
                    AverageClickRate = avgClickRate,
                    ActiveDrips = db.tbl_EmailDripCampaigns.Count(d => d.IsActive)
                };
            }
        }

        public List<CampaignLinkStatsVM> GetCampaignLinks(int campaignId)
        {
            using (var db = new EmailMarketingDbContext())
            {
                return db.tbl_EmailCampaignLinks
                    .Where(l => l.CampaignId == campaignId)
                    .OrderByDescending(l => l.TotalClicks)
                    .Select(l => new CampaignLinkStatsVM
                    {
                        LinkId = l.Id,
                        Url = l.OriginalUrl,
                        TotalClicks = l.TotalClicks,
                        UniqueClicks = l.UniqueClicks
                    }).ToList();
            }
        }

        public List<CampaignRecipientVM> GetCampaignRecipients(int campaignId, string statusFilter = null, int page = 1, int pageSize = 50)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var query = db.tbl_EmailCampaignRecipients
                    .Where(r => r.CampaignId == campaignId);

                if (!string.IsNullOrEmpty(statusFilter))
                    query = query.Where(r => r.Status == statusFilter);

                return query.OrderByDescending(r => r.SentAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new CampaignRecipientVM
                    {
                        Id = r.Id,
                        Email = r.RecipientEmail,
                        Name = r.RecipientName,
                        Status = r.Status,
                        SentAt = r.SentAt,
                        FirstOpenedAt = r.FirstOpenedAt,
                        FirstClickedAt = r.FirstClickedAt,
                        OpenCount = r.OpenCount,
                        ClickCount = r.ClickCount
                    }).ToList();
            }
        }

        // =============================================
        // TRACKING
        // =============================================

        public void TrackOpen(long recipientId)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var recipient = db.tbl_EmailCampaignRecipients.Find(recipientId);
                if (recipient == null) return;

                recipient.OpenCount++;
                recipient.LastOpenedAt = DateTime.Now;
                if (!recipient.FirstOpenedAt.HasValue)
                {
                    recipient.FirstOpenedAt = DateTime.Now;
                    recipient.Status = "Opened";

                    // Update campaign unique opens
                    var campaign = db.tbl_EmailCampaigns.Find(recipient.CampaignId);
                    if (campaign != null)
                    {
                        campaign.UniqueOpens++;
                        campaign.TotalOpens++;
                    }
                }
                else
                {
                    var campaign = db.tbl_EmailCampaigns.Find(recipient.CampaignId);
                    if (campaign != null)
                        campaign.TotalOpens++;
                }

                db.SaveChanges();
            }
        }

        public string TrackClick(int linkId, long recipientId, string userAgent, string ipAddress)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var link = db.tbl_EmailCampaignLinks.Find(linkId);
                if (link == null) return null;

                // Log click
                db.tbl_EmailLinkClicks.Add(new tbl_EmailLinkClicks
                {
                    LinkId = linkId,
                    RecipientId = recipientId,
                    ClickedAt = DateTime.Now,
                    UserAgent = userAgent?.Length > 500 ? userAgent.Substring(0, 500) : userAgent,
                    IpAddress = ipAddress
                });

                // Update link stats
                link.TotalClicks++;
                var isFirstClick = !db.tbl_EmailLinkClicks.Any(c => c.LinkId == linkId && c.RecipientId == recipientId);
                if (isFirstClick)
                    link.UniqueClicks++;

                // Update recipient
                var recipient = db.tbl_EmailCampaignRecipients.Find(recipientId);
                if (recipient != null)
                {
                    recipient.ClickCount++;
                    if (!recipient.FirstClickedAt.HasValue)
                    {
                        recipient.FirstClickedAt = DateTime.Now;
                        recipient.Status = "Clicked";

                        var campaign = db.tbl_EmailCampaigns.Find(recipient.CampaignId);
                        if (campaign != null)
                        {
                            campaign.UniqueClicks++;
                            campaign.TotalClicks++;
                        }
                    }
                    else
                    {
                        var campaign = db.tbl_EmailCampaigns.Find(recipient.CampaignId);
                        if (campaign != null)
                            campaign.TotalClicks++;
                    }
                }

                db.SaveChanges();
                return link.OriginalUrl;
            }
        }

        // =============================================
        // UNSUBSCRIBE
        // =============================================

        public bool IsUnsubscribed(string email)
        {
            using (var db = new EmailMarketingDbContext())
            {
                return db.tbl_EmailUnsubscribes.Any(u => u.Email == email && !u.IsResubscribed);
            }
        }

        public bool ProcessUnsubscribe(string email, string reason, int? campaignId)
        {
            using (var db = new EmailMarketingDbContext())
            {
                if (db.tbl_EmailUnsubscribes.Any(u => u.Email == email && !u.IsResubscribed))
                    return true; // Already unsubscribed

                db.tbl_EmailUnsubscribes.Add(new tbl_EmailUnsubscribes
                {
                    Email = email,
                    CampaignId = campaignId,
                    Reason = reason,
                    UnsubscribedAt = DateTime.Now
                });

                // Update campaign stats if applicable
                if (campaignId.HasValue)
                {
                    var campaign = db.tbl_EmailCampaigns.Find(campaignId.Value);
                    if (campaign != null)
                        campaign.TotalUnsubscribes++;
                }

                db.SaveChanges();
                return true;
            }
        }

        public bool Resubscribe(string email)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var record = db.tbl_EmailUnsubscribes.FirstOrDefault(u => u.Email == email && !u.IsResubscribed);
                if (record == null) return false;
                record.IsResubscribed = true;
                record.ResubscribedAt = DateTime.Now;
                db.SaveChanges();
                return true;
            }
        }

        public UnsubscribeReportVM GetUnsubscribeReport(int page = 1, int pageSize = 50)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var now = DateTime.Now;
                var monthStart = new DateTime(now.Year, now.Month, 1);

                var query = db.tbl_EmailUnsubscribes.Where(u => !u.IsResubscribed);

                var totalCount = query.Count();
                var thisMonthCount = query.Count(u => u.UnsubscribedAt >= monthStart);

                var records = query
                    .OrderByDescending(u => u.UnsubscribedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new UnsubscribeRecordVM
                    {
                        Email = u.Email,
                        Reason = u.Reason,
                        UnsubscribedAt = u.UnsubscribedAt
                    }).ToList();

                // Join campaign names for the records
                var campaignIds = db.tbl_EmailUnsubscribes
                    .Where(u => !u.IsResubscribed && u.CampaignId.HasValue)
                    .Select(u => new { u.Email, u.CampaignId })
                    .Take(500).ToList();

                // Reason breakdown
                var reasonGroups = query
                    .GroupBy(u => u.Reason ?? "")
                    .Select(g => new { Reason = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .ToList();
                var reasonBreakdown = new Dictionary<string, int>();
                foreach (var rg in reasonGroups)
                    reasonBreakdown[string.IsNullOrEmpty(rg.Reason) ? "No reason given" : rg.Reason] = rg.Count;

                var topReason = reasonGroups.FirstOrDefault()?.Reason ?? "N/A";
                if (string.IsNullOrEmpty(topReason)) topReason = "No reason given";

                // Unsubscribe rate (vs total emails sent)
                var totalDelivered = db.tbl_EmailCampaigns.Sum(c => (int?)c.TotalSent) ?? 1;
                var unsubRate = totalDelivered > 0 ? Math.Round((decimal)totalCount / totalDelivered * 100, 2) : 0;

                // Monthly trend (last 6 months)
                var sixMonthsAgo = now.AddMonths(-6);
                var monthlyData = query
                    .Where(u => u.UnsubscribedAt >= sixMonthsAgo)
                    .ToList()
                    .GroupBy(u => new { u.UnsubscribedAt.Year, u.UnsubscribedAt.Month })
                    .Select(g => new { Month = g.Key.Year + "-" + g.Key.Month.ToString("D2"), Count = g.Count() })
                    .OrderBy(g => g.Month)
                    .ToList();
                var trendJson = Newtonsoft.Json.JsonConvert.SerializeObject(monthlyData);

                return new UnsubscribeReportVM
                {
                    Records = records,
                    TotalUnsubscribed = totalCount,
                    UnsubscribedThisMonth = thisMonthCount,
                    UnsubscribeRate = unsubRate,
                    TopReason = topReason,
                    ReasonBreakdown = reasonBreakdown,
                    MonthlyTrendJson = trendJson,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    CurrentPage = page
                };
            }
        }

        // =============================================
        // DRIP CAMPAIGNS
        // =============================================

        public List<DripCampaignVM> GetDripCampaigns()
        {
            using (var db = new EmailMarketingDbContext())
            {
                return db.tbl_EmailDripCampaigns
                    .OrderByDescending(d => d.CreatedAt)
                    .Select(d => new DripCampaignVM
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        TriggerType = d.TriggerType,
                        IsActive = d.IsActive,
                        TotalEnrolled = d.TotalEnrolled,
                        TotalCompleted = d.TotalCompleted,
                        StepCount = db.tbl_EmailDripSteps.Count(s => s.DripCampaignId == d.Id),
                        TotalSent = db.tbl_EmailDripQueue.Count(q => q.DripCampaignId == d.Id && q.Status == "Sent"),
                        CreatedAt = d.CreatedAt
                    }).ToList();
            }
        }

        public DripCampaignVM GetDripCampaign(int id)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var d = db.tbl_EmailDripCampaigns.Find(id);
                if (d == null) return null;

                var steps = db.tbl_EmailDripSteps
                    .Where(s => s.DripCampaignId == id)
                    .OrderBy(s => s.StepOrder)
                    .Select(s => new DripStepVM
                    {
                        Id = s.Id,
                        StepOrder = s.StepOrder,
                        DelayDays = s.DelayDays,
                        DelayHours = s.DelayHours,
                        Subject = s.Subject,
                        TemplateId = s.TemplateId,
                        HtmlBody = s.HtmlBody,
                        Condition = s.Condition,
                        IsActive = s.IsActive
                    }).ToList();

                return new DripCampaignVM
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    TriggerType = d.TriggerType,
                    TriggerConfig = d.TriggerConfig,
                    FromAccountId = d.FromAccountId,
                    IsActive = d.IsActive,
                    TotalEnrolled = d.TotalEnrolled,
                    TotalCompleted = d.TotalCompleted,
                    CreatedAt = d.CreatedAt,
                    Steps = steps
                };
            }
        }

        public int SaveDripCampaign(DripCampaignVM model, string userId)
        {
            using (var db = new EmailMarketingDbContext())
            {
                tbl_EmailDripCampaigns entity;
                if (model.Id > 0)
                {
                    entity = db.tbl_EmailDripCampaigns.Find(model.Id);
                    if (entity == null) return 0;
                    entity.UpdatedAt = DateTime.Now;

                    // Remove existing steps
                    var existingSteps = db.tbl_EmailDripSteps.Where(s => s.DripCampaignId == model.Id).ToList();
                    db.tbl_EmailDripSteps.RemoveRange(existingSteps);
                }
                else
                {
                    entity = new tbl_EmailDripCampaigns { CreatedAt = DateTime.Now, CreatedBy = userId };
                    db.tbl_EmailDripCampaigns.Add(entity);
                }

                entity.Name = model.Name;
                entity.Description = model.Description;
                entity.TriggerType = model.TriggerType;
                entity.TriggerConfig = model.TriggerConfig;
                entity.FromAccountId = model.FromAccountId;

                db.SaveChanges();

                // Add steps
                if (model.Steps != null)
                {
                    foreach (var step in model.Steps)
                    {
                        db.tbl_EmailDripSteps.Add(new tbl_EmailDripSteps
                        {
                            DripCampaignId = entity.Id,
                            StepOrder = step.StepOrder,
                            DelayDays = step.DelayDays,
                            DelayHours = step.DelayHours,
                            Subject = step.Subject,
                            TemplateId = step.TemplateId,
                            HtmlBody = step.HtmlBody,
                            Condition = step.Condition,
                            IsActive = step.IsActive
                        });
                    }
                    db.SaveChanges();
                }

                return entity.Id;
            }
        }

        public bool DeleteDripCampaign(int id)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var entity = db.tbl_EmailDripCampaigns.Find(id);
                if (entity == null) return false;
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.Now;
                db.SaveChanges();
                return true;
            }
        }

        public bool ToggleDripCampaign(int id)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var entity = db.tbl_EmailDripCampaigns.Find(id);
                if (entity == null) return false;
                entity.IsActive = !entity.IsActive;
                entity.UpdatedAt = DateTime.Now;
                db.SaveChanges();
                return true;
            }
        }

        public void EnqueueDripForUser(string userId, string email, string triggerType)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var activeDrips = db.tbl_EmailDripCampaigns
                    .Where(d => d.IsActive && d.TriggerType == triggerType)
                    .ToList();

                foreach (var drip in activeDrips)
                {
                    var steps = db.tbl_EmailDripSteps
                        .Where(s => s.DripCampaignId == drip.Id && s.IsActive)
                        .OrderBy(s => s.StepOrder)
                        .ToList();

                    var baseTime = DateTime.Now;
                    foreach (var step in steps)
                    {
                        var scheduledAt = baseTime.AddDays(step.DelayDays).AddHours(step.DelayHours);
                        db.tbl_EmailDripQueue.Add(new tbl_EmailDripQueue
                        {
                            DripCampaignId = drip.Id,
                            StepId = step.Id,
                            UserId = userId,
                            Email = email,
                            ScheduledAt = scheduledAt,
                            Status = "Pending"
                        });
                    }

                    drip.TotalEnrolled++;
                }

                db.SaveChanges();
            }
        }

        // =============================================
        // UTILITIES
        // =============================================

        public string GenerateUnsubscribeToken(string email, int? campaignId)
        {
            var data = $"{email}|{campaignId ?? 0}|{DateTime.UtcNow.Ticks}";
            var bytes = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        public string DecodeUnsubscribeToken(string token)
        {
            token = token.Replace('-', '+').Replace('_', '/');
            switch (token.Length % 4)
            {
                case 2: token += "=="; break;
                case 3: token += "="; break;
            }
            var bytes = Convert.FromBase64String(token);
            var data = Encoding.UTF8.GetString(bytes);
            var parts = data.Split('|');
            return parts.Length > 0 ? parts[0] : null;
        }

        public string ProcessMergeTags(string html, AudienceRecipientVM recipient, int? campaignId = null)
        {
            if (string.IsNullOrEmpty(html)) return html;

            var unsubToken = GenerateUnsubscribeToken(recipient.Email, campaignId);
            var unsubLink = $"https://firstaidmadeeasy.com.pk/EmailTracking/Unsubscribe/{unsubToken}";

            html = html.Replace("{{FirstName}}", recipient.Name ?? "Student");
            html = html.Replace("{{Email}}", recipient.Email ?? "");
            html = html.Replace("{{University}}", recipient.University ?? "");
            html = html.Replace("{{ExamType}}", recipient.ExamType ?? "");
            html = html.Replace("{{City}}", recipient.City ?? "");
            html = html.Replace("{{UnsubscribeLink}}", unsubLink);
            html = html.Replace("{{LoginLink}}", "https://firstaidmadeeasy.com.pk/Account/Login");
            html = html.Replace("{{Year}}", DateTime.Now.Year.ToString());
            html = html.Replace("{{Month}}", DateTime.Now.ToString("MMMM"));
            html = html.Replace("{{Date}}", DateTime.Now.ToString("MMMM dd, yyyy"));

            return html;
        }

        // =============================================
        // PRIVATE HELPERS
        // =============================================

        private string NormalizeEnrollmentStatus(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "All";

            switch (raw.Trim().ToLowerInvariant())
            {
                case "active":
                case "trial":
                    return "Active";
                case "expired":
                    return "Expired";
                default:
                    return "All";
            }
        }

        private IQueryable<EmailMarketingEnrollmentMaster> ApplyEnrollmentStatusFilter(
            IQueryable<EmailMarketingEnrollmentMaster> enrollmentQuery,
            string enrollmentStatus,
            DateTime now)
        {
            var normalized = NormalizeEnrollmentStatus(enrollmentStatus);

            switch (normalized)
            {
                case "Active":
                    return enrollmentQuery.Where(e =>
                        (e.IsExpired == null || e.IsExpired == false) &&
                        e.Enrollment_EndDate >= now);
                case "Expired":
                    return enrollmentQuery.Where(e =>
                        e.IsExpired == true || e.Enrollment_EndDate < now);
                default:
                    return enrollmentQuery;
            }
        }

        private List<AudienceRecipientVM> DedupeRecipientsByEmail(List<AudienceRecipientVM> recipients)
        {
            if (recipients == null || recipients.Count == 0) return new List<AudienceRecipientVM>();

            return recipients
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.Email))
                .GroupBy(r => r.Email.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g
                    .OrderBy(r => string.IsNullOrWhiteSpace(r.Name) ? 1 : 0)
                    .ThenBy(r => string.IsNullOrWhiteSpace(r.UserId) ? 1 : 0)
                    .First())
                .ToList();
        }

        private SenderAccountVM MapSenderAccount(tbl_EmailSenderAccounts a)
        {
            return new SenderAccountVM
            {
                Id = a.Id,
                AccountName = a.AccountName,
                FromEmail = a.FromEmail,
                FromName = a.FromName,
                SmtpHost = a.SmtpHost,
                SmtpPort = a.SmtpPort,
                SmtpUsername = a.SmtpUsername,
                EnableSSL = a.EnableSSL,
                UseApi = a.UseApi,
                ApiKey = a.ApiKey,
                ApiProvider = a.ApiProvider,
                DailyLimit = a.DailyLimit,
                SentToday = a.SentToday,
                IsDefault = a.IsDefault,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt
            };
        }

        private async Task SendSingleEmailAsync(tbl_EmailSenderAccounts account, string toEmail, string subject, string htmlBody)
        {
            if (account.UseApi && !string.IsNullOrEmpty(account.ApiKey))
            {
                // Send via Brevo API (same pattern as IdentityConfig.cs)
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("api-key", account.ApiKey);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new
                    {
                        sender = new { name = account.FromName, email = account.FromEmail },
                        to = new[] { new { email = toEmail } },
                        subject = subject,
                        htmlContent = htmlBody
                    };

                    var jsonContent = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", jsonContent);

                    if (!response.IsSuccessStatusCode)
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Brevo API Error ({response.StatusCode}): {error}");
                    }
                }
            }
            else
            {
                // Send via SMTP
                var msg = new MailMessage
                {
                    From = new MailAddress(account.FromEmail, account.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                msg.To.Add(toEmail);

                var smtpClient = new SmtpClient
                {
                    Host = account.SmtpHost,
                    Port = account.SmtpPort ?? 587,
                    EnableSsl = account.EnableSSL,
                    Credentials = new NetworkCredential(account.SmtpUsername ?? account.FromEmail, account.SmtpPassword)
                };

                await smtpClient.SendMailAsync(msg);
            }
        }

        private List<string> ExtractLinks(string html)
        {
            var links = new HashSet<string>();
            if (string.IsNullOrEmpty(html)) return links.ToList();

            var regex = new Regex(@"href\s*=\s*[""'](https?://[^""']+)[""']", RegexOptions.IgnoreCase);
            foreach (Match match in regex.Matches(html))
            {
                var url = match.Groups[1].Value;
                // Don't track unsubscribe links
                if (!url.Contains("EmailTracking/Unsubscribe"))
                    links.Add(url);
            }
            return links.ToList();
        }

        private string GetOpenTimeline(int campaignId)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var data = db.tbl_EmailCampaignRecipients
                    .Where(r => r.CampaignId == campaignId && r.FirstOpenedAt.HasValue)
                    .GroupBy(r => DbFunctions.TruncateTime(r.FirstOpenedAt))
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .OrderBy(g => g.Date)
                    .ToList();
                return JsonConvert.SerializeObject(data);
            }
        }

        private string GetClickTimeline(int campaignId)
        {
            using (var db = new EmailMarketingDbContext())
            {
                var data = db.tbl_EmailCampaignRecipients
                    .Where(r => r.CampaignId == campaignId && r.FirstClickedAt.HasValue)
                    .GroupBy(r => DbFunctions.TruncateTime(r.FirstClickedAt))
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .OrderBy(g => g.Date)
                    .ToList();
                return JsonConvert.SerializeObject(data);
            }
        }

        private void PopulateCampaignDropdowns(CampaignCreateVM vm, EmailMarketingDbContext db)
        {
            vm.SenderAccounts = db.tbl_EmailSenderAccounts
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.IsDefault)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.AccountName + " (" + a.FromEmail + ")",
                    Selected = a.IsDefault
                }).ToList();

            vm.Templates = db.tbl_EmailTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.TemplateName)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.TemplateName + " [" + t.Category + "]"
                }).ToList();
            vm.Templates.Insert(0, new SelectListItem { Value = "", Text = "-- No Template (Blank) --" });

            vm.Audiences = db.tbl_EmailAudiences
                .Where(a => a.IsActive)
                .OrderBy(a => a.SegmentName)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.SegmentName + " (" + a.RecipientCount + " recipients)"
                }).ToList();
            vm.Audiences.Insert(0, new SelectListItem { Value = "", Text = "-- Select Audience --" });

            // Package & Course dropdowns (were missing - BUG FIX)
            var packages = db.Packages
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.SortID)
                .Select(p => new { p.PackageID, p.PackageName })
                .ToList();
            vm.PackagesList = new List<SelectListItem>();
            vm.PackagesList.Add(new SelectListItem { Value = "", Text = "-- Select Package --" });
            vm.PackagesList.Add(new SelectListItem { Value = "0", Text = "All Packages" });
            foreach (var p in packages)
                vm.PackagesList.Add(new SelectListItem { Value = p.PackageID.ToString(), Text = p.PackageName });

            var courses = db.Courses
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.SortID)
                .Select(c => new { c.Course_Id, c.Course_Name })
                .ToList();
            vm.CoursesList = new List<SelectListItem>();
            vm.CoursesList.Add(new SelectListItem { Value = "", Text = "-- Select Course --" });
            vm.CoursesList.Add(new SelectListItem { Value = "0", Text = "All Courses" });
            foreach (var c in courses)
                vm.CoursesList.Add(new SelectListItem { Value = c.Course_Id.ToString(), Text = c.Course_Name });
        }
    }
}
