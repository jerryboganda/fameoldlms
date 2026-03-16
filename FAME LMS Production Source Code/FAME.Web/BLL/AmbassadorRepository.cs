using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace First_Aid_Made_Easy.BLL
{
    public class AmbassadorRepository : IAmbassadorRepository
    {
        private readonly IAmbassadorAuditService _auditService;

        public AmbassadorRepository(IAmbassadorAuditService auditService)
        {
            _auditService = auditService;
        }

        #region Ambassador CRUD

        public AmbassadorVM GetById(int id)
        {
            using (var db = new AmbassadorDbContext())
            {
                var ambassador = db.tbl_Ambassador
                    .Include(a => a.AspNetUsers)
                    .Include(a => a.AspNetUsers1) // ApprovedBy
                    .FirstOrDefault(a => a.Id == id);

                if (ambassador == null) return null;

                return MapToVM(ambassador);
            }
        }

        public AmbassadorVM GetByUserId(string userId)
        {
            using (var db = new AmbassadorDbContext())
            {
                var ambassador = db.tbl_Ambassador
                    .Include(a => a.AspNetUsers)
                    .Include(a => a.AspNetUsers1)
                    .FirstOrDefault(a => a.UserId == userId);

                if (ambassador == null) return null;

                return MapToVM(ambassador);
            }
        }

        public AmbassadorVM GetByReferralCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;

            using (var db = new AmbassadorDbContext())
            {
                var ambassador = db.tbl_Ambassador
                    .Include(a => a.AspNetUsers)
                    .FirstOrDefault(a => a.ReferralCode == code.ToUpper());

                if (ambassador == null) return null;

                return MapToVM(ambassador);
            }
        }

        public PagedResultVM<AmbassadorListItemVM> GetList(AmbassadorFilterVM filter)
        {
            using (var db = new AmbassadorDbContext())
            {
                var query = db.tbl_Ambassador
                    .Include(a => a.AspNetUsers)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    var term = filter.SearchTerm.ToLower();
                    query = query.Where(a =>
                        a.AspNetUsers.Email.ToLower().Contains(term) ||
                        ((a.FullName ?? a.AspNetUsers.UserName).ToLower().Contains(term)) ||
                        a.ReferralCode.ToLower().Contains(term) ||
                        (a.University != null && a.University.ToLower().Contains(term)));
                }

                if (!string.IsNullOrEmpty(filter.Status))
                    query = query.Where(a => a.Status == filter.Status);

                if (!string.IsNullOrEmpty(filter.Tier))
                    query = query.Where(a => a.Tier == filter.Tier);

                if (filter.FromDate.HasValue)
                    query = query.Where(a => a.CreatedAt >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(a => a.CreatedAt <= filter.ToDate.Value);

                // Get total count
                var totalCount = query.Count();

                // Apply sorting
                switch (filter.SortBy?.ToLower())
                {
                    case "email":
                        query = filter.SortDir == "asc" 
                            ? query.OrderBy(a => a.AspNetUsers.Email)
                            : query.OrderByDescending(a => a.AspNetUsers.Email);
                        break;
                    case "status":
                        query = filter.SortDir == "asc"
                            ? query.OrderBy(a => a.Status)
                            : query.OrderByDescending(a => a.Status);
                        break;
                    case "tier":
                        query = filter.SortDir == "asc"
                            ? query.OrderBy(a => a.Tier)
                            : query.OrderByDescending(a => a.Tier);
                        break;
                    default:
                        query = filter.SortDir == "asc"
                            ? query.OrderBy(a => a.CreatedAt)
                            : query.OrderByDescending(a => a.CreatedAt);
                        break;
                }

                // Apply pagination
                var items = query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList()
                    .Select(a => MapToListItemVM(a, db))
                    .ToList();

                return new PagedResultVM<AmbassadorListItemVM>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize
                };
            }
        }

        public List<AmbassadorListItemVM> GetActiveAmbassadors()
        {
            using (var db = new AmbassadorDbContext())
            {
                return db.tbl_Ambassador
                    .Include(a => a.AspNetUsers)
                    .Where(a => a.Status == "Active")
                    .OrderBy(a => a.AspNetUsers.Email)
                    .ToList()
                    .Select(a => MapToListItemVM(a, db))
                    .ToList();
            }
        }

        public List<AmbassadorListItemVM> GetPendingApplications()
        {
            using (var db = new AmbassadorDbContext())
            {
                return db.tbl_Ambassador
                    .Include(a => a.AspNetUsers)
                    .Where(a => a.Status == "Pending")
                    .OrderBy(a => a.CreatedAt)
                    .ToList()
                    .Select(a => MapToListItemVM(a, db))
                    .ToList();
            }
        }

        #endregion

        #region Ambassador Lifecycle

        public int Apply(string userId, AmbassadorApplicationVM model, string fullName = null)
        {
            using (var db = new AmbassadorDbContext())
            {
                // Check if already applied
                if (db.tbl_Ambassador.Any(a => a.UserId == userId))
                    throw new InvalidOperationException("You have already applied to be an ambassador.");

                var referralCode = GenerateReferralCode();

                var ambassador = new tbl_Ambassador
                {
                    UserId = userId,
                    ReferralCode = referralCode,
                    Status = "Pending",
                    Tier = "Bronze",
                    University = model.University,
                    Country = model.Country,
                    PhoneNumber = model.PhoneNumber,
                    FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim(),
                    ApplicationNotes = model.ApplicationNotes,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                db.tbl_Ambassador.Add(ambassador);
                db.SaveChanges();

                _auditService.Log(ambassador.Id, "ApplicationSubmitted", "Ambassador", ambassador.Id.ToString(),
                    null, "Pending", userId);

                return ambassador.Id;
            }
        }

        public bool Approve(int ambassadorId, string approvedBy)
        {
            using (var db = new AmbassadorDbContext())
            {
                var ambassador = db.tbl_Ambassador.Find(ambassadorId);
                if (ambassador == null || ambassador.Status != "Pending")
                    return false;

                var oldStatus = ambassador.Status;
                ambassador.Status = "Active";
                ambassador.ApprovedBy = approvedBy;
                ambassador.ApprovedAt = DateTime.Now;
                ambassador.UpdatedAt = DateTime.Now;

                // Add Ambassador role to user
                var ambassadorRole = db.AspNetRoles.FirstOrDefault(r => r.Name == "Ambassador");
                if (ambassadorRole != null)
                {
                    var user = db.AspNetUsers.Find(ambassador.UserId);
                    if (user != null && !user.AspNetRoles.Any(r => r.Name == "Ambassador"))
                    {
                        user.AspNetRoles.Add(ambassadorRole);
                    }
                }

                db.SaveChanges();

                _auditService.LogStatusChange(ambassadorId, oldStatus, "Active", approvedBy);

                return true;
            }
        }

        public bool Reject(int ambassadorId, string reason, string rejectedBy)
        {
            using (var db = new AmbassadorDbContext())
            {
                var ambassador = db.tbl_Ambassador.Find(ambassadorId);
                if (ambassador == null || ambassador.Status != "Pending")
                    return false;

                var oldStatus = ambassador.Status;
                ambassador.Status = "Rejected";
                ambassador.SuspendedReason = reason;
                ambassador.UpdatedAt = DateTime.Now;

                db.SaveChanges();

                _auditService.LogStatusChange(ambassadorId, oldStatus, "Rejected", rejectedBy);

                return true;
            }
        }

        public bool Suspend(int ambassadorId, string reason, string suspendedBy)
        {
            using (var db = new AmbassadorDbContext())
            {
                var ambassador = db.tbl_Ambassador.Find(ambassadorId);
                if (ambassador == null || ambassador.Status != "Active")
                    return false;

                var oldStatus = ambassador.Status;
                ambassador.Status = "Suspended";
                ambassador.SuspendedReason = reason;
                ambassador.UpdatedAt = DateTime.Now;

                db.SaveChanges();

                _auditService.LogStatusChange(ambassadorId, oldStatus, "Suspended", suspendedBy);

                return true;
            }
        }

        public bool Reactivate(int ambassadorId, string reactivatedBy)
        {
            using (var db = new AmbassadorDbContext())
            {
                var ambassador = db.tbl_Ambassador.Find(ambassadorId);
                if (ambassador == null || ambassador.Status != "Suspended")
                    return false;

                var oldStatus = ambassador.Status;
                ambassador.Status = "Active";
                ambassador.SuspendedReason = null;
                ambassador.UpdatedAt = DateTime.Now;

                db.SaveChanges();

                _auditService.LogStatusChange(ambassadorId, oldStatus, "Active", reactivatedBy);

                return true;
            }
        }

        public bool UpdateTier(int ambassadorId, string tier, string updatedBy)
        {
            if (!new[] { "Bronze", "Silver", "Gold", "Platinum" }.Contains(tier))
                return false;

            using (var db = new AmbassadorDbContext())
            {
                var ambassador = db.tbl_Ambassador.Find(ambassadorId);
                if (ambassador == null)
                    return false;

                var oldTier = ambassador.Tier;
                ambassador.Tier = tier;
                ambassador.UpdatedAt = DateTime.Now;

                db.SaveChanges();

                _auditService.Log(ambassadorId, "TierUpdated", "Ambassador", ambassadorId.ToString(),
                    oldTier, tier, updatedBy);

                return true;
            }
        }

        public bool UpdateProfile(int ambassadorId, AmbassadorApplicationVM model)
        {
            using (var db = new AmbassadorDbContext())
            {
                var ambassador = db.tbl_Ambassador.Find(ambassadorId);
                if (ambassador == null)
                    return false;

                ambassador.University = model.University;
                ambassador.Country = model.Country;
                ambassador.PhoneNumber = model.PhoneNumber;
                ambassador.UpdatedAt = DateTime.Now;

                db.SaveChanges();

                return true;
            }
        }

        #endregion

        #region Dashboard

        public AmbassadorDashboardVM GetDashboard(int ambassadorId)
        {
            var ambassador = GetById(ambassadorId);
            if (ambassador == null) return null;

            using (var db = new AmbassadorDbContext())
            {
                var dashboard = new AmbassadorDashboardVM
                {
                    Ambassador = ambassador,
                    Stats = GetStats(ambassadorId),
                    RecentReferrals = GetRecentReferrals(ambassadorId, 5, db),
                    RecentEarnings = GetRecentEarnings(ambassadorId, 5, db),
                    RecentPayouts = GetRecentPayouts(ambassadorId, 5, db),
                    ClicksChartData = GetChartData(ambassadorId, "clicks", 30),
                    ConversionsChartData = GetChartData(ambassadorId, "conversions", 30)
                };

                return dashboard;
            }
        }

        public DashboardStatsVM GetStats(int ambassadorId)
        {
            using (var db = new AmbassadorDbContext())
            {
                var now = DateTime.Now;
                var monthStart = new DateTime(now.Year, now.Month, 1);

                var stats = new DashboardStatsVM
                {
                    // Clicks
                    TotalClicks = db.tbl_ReferralClick.Count(c => c.AmbassadorId == ambassadorId),
                    ClicksThisMonth = db.tbl_ReferralClick.Count(c => 
                        c.AmbassadorId == ambassadorId && c.CreatedAt >= monthStart),

                    // Referrals
                    TotalReferrals = db.tbl_Referral.Count(r => r.AmbassadorId == ambassadorId),
                    PendingReferrals = db.tbl_Referral.Count(r => 
                        r.AmbassadorId == ambassadorId && r.Status == "Registered"),
                    ConvertedReferrals = db.tbl_Referral.Count(r => 
                        r.AmbassadorId == ambassadorId && r.Status == "Converted"),
                    ReferralsThisMonth = db.tbl_Referral.Count(r => 
                        r.AmbassadorId == ambassadorId && r.RegisteredAt >= monthStart),

                    // Conversions this month
                    ConversionsThisMonth = db.tbl_ReferralConversion
                        .Where(c => c.tbl_Referral.AmbassadorId == ambassadorId && c.ConvertedAt >= monthStart)
                        .Count(),

                    // Earnings
                    TotalEarnings = db.tbl_AmbassadorEarning
                        .Where(e => e.AmbassadorId == ambassadorId && e.Status != "Revoked")
                        .Sum(e => (decimal?)e.Amount) ?? 0,

                    PendingEarnings = db.tbl_AmbassadorEarning
                        .Where(e => e.AmbassadorId == ambassadorId && e.Status == "Pending")
                        .Sum(e => (decimal?)e.Amount) ?? 0,

                    AvailableBalance = db.tbl_AmbassadorEarning
                        .Where(e => e.AmbassadorId == ambassadorId && e.Status == "Available")
                        .Sum(e => (decimal?)e.Amount) ?? 0,

                    PaidOutTotal = db.tbl_PayoutRequest
                        .Where(p => p.AmbassadorId == ambassadorId && p.Status == "Paid")
                        .Sum(p => (decimal?)p.Amount) ?? 0,

                    EarningsThisMonth = db.tbl_AmbassadorEarning
                        .Where(e => e.AmbassadorId == ambassadorId && 
                               e.Status != "Revoked" && e.CreatedAt >= monthStart)
                        .Sum(e => (decimal?)e.Amount) ?? 0,

                    Currency = AmbassadorSettingsHelper.GetDefaultCurrency()
                };

                return stats;
            }
        }

        public ChartDataVM GetChartData(int ambassadorId, string chartType, int days = 30)
        {
            var chartData = new ChartDataVM { ChartType = "line" };
            var endDate = DateTime.Now.Date;
            var startDate = endDate.AddDays(-days);
            var endDateExclusive = endDate.AddDays(1); // Pre-compute for LINQ to Entities compatibility

            using (var db = new AmbassadorDbContext())
            {
                var dates = Enumerable.Range(0, days + 1)
                    .Select(d => startDate.AddDays(d))
                    .ToList();

                if (chartType == "clicks")
                {
                    var clickData = db.tbl_ReferralClick
                        .Where(c => c.AmbassadorId == ambassadorId &&
                                   c.CreatedAt >= startDate && c.CreatedAt <= endDateExclusive)
                        .GroupBy(c => DbFunctions.TruncateTime(c.CreatedAt))
                        .Select(g => new { Date = g.Key, Count = g.Count() })
                        .ToDictionary(x => x.Date.Value, x => x.Count);

                    foreach (var date in dates)
                    {
                        chartData.Labels.Add(date.ToString("MMM dd"));
                        chartData.Values.Add(clickData.ContainsKey(date) ? clickData[date] : 0);
                    }
                }
                else if (chartType == "conversions")
                {
                    var conversionData = db.tbl_ReferralConversion
                        .Where(c => c.tbl_Referral.AmbassadorId == ambassadorId &&
                                   c.ConvertedAt >= startDate && c.ConvertedAt <= endDateExclusive)
                        .GroupBy(c => DbFunctions.TruncateTime(c.ConvertedAt))
                        .Select(g => new { Date = g.Key, Count = g.Count() })
                        .ToDictionary(x => x.Date.Value, x => x.Count);

                    foreach (var date in dates)
                    {
                        chartData.Labels.Add(date.ToString("MMM dd"));
                        chartData.Values.Add(conversionData.ContainsKey(date) ? conversionData[date] : 0);
                    }
                }
            }

            return chartData;
        }

        #endregion

        #region Admin Dashboard

        public AdminAmbassadorDashboardVM GetAdminDashboard()
        {
            using (var db = new AmbassadorDbContext())
            {
                var now = DateTime.Now;
                var monthStart = new DateTime(now.Year, now.Month, 1);

                // Build chart data
                var chartLabels = new List<string>();
                var referralData = new List<int>();
                var conversionData = new List<int>();
                for (int i = 5; i >= 0; i--)
                {
                    var date = now.AddMonths(-i);
                    var ms = new DateTime(date.Year, date.Month, 1);
                    var me = ms.AddMonths(1);
                    chartLabels.Add(date.ToString("MMM yyyy"));
                    referralData.Add(db.tbl_Referral.Count(r => r.RegisteredAt >= ms && r.RegisteredAt < me));
                    conversionData.Add(db.tbl_ReferralConversion.Count(c => c.ConvertedAt >= ms && c.ConvertedAt < me && c.Status == "Approved"));
                }

                // Tier distribution
                var tierLabels = new List<string> { "Bronze", "Silver", "Gold", "Platinum" };
                var tierData = tierLabels.Select(t => db.tbl_Ambassador.Count(a => a.Tier == t && a.Status == "Active")).ToList();

                // Top performers this month
                var topPerformers = db.tbl_Referral
                    .Where(r => r.RegisteredAt >= monthStart)
                    .GroupBy(r => r.AmbassadorId)
                    .Select(g => new { AmbassadorId = g.Key, Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .Take(5)
                    .ToList()
                    .Select(g =>
                    {
                        var amb = db.tbl_Ambassador.Include(a => a.AspNetUsers).FirstOrDefault(a => a.Id == g.AmbassadorId);
                        return new TopPerformerVM
                        {
                            AmbassadorId = g.AmbassadorId,
                            Name = amb != null ? ResolveDisplayName(amb) : "Unknown",
                            Referrals = g.Count,
                            Earnings = db.tbl_AmbassadorEarning
                                .Where(e => e.AmbassadorId == g.AmbassadorId && e.CreatedAt >= monthStart)
                                .Sum(e => (decimal?)e.Amount) ?? 0
                        };
                    })
                    .ToList();

                // Recent audit log
                var recentActivity = db.tbl_AmbassadorAuditLog
                    .OrderByDescending(a => a.PerformedAt)
                    .Take(10)
                    .ToList()
                    .Select(a => new RecentActivityVM
                    {
                        Description = a.Action + " - " + a.EntityType,
                        Timestamp = a.PerformedAt,
                        Icon = a.Action.Contains("Approve") ? "check_circle" : a.Action.Contains("Create") ? "add_circle" : "info",
                        IconBgClass = a.Action.Contains("Approve") ? "bg-success" : a.Action.Contains("Create") ? "bg-primary" : "bg-info"
                    })
                    .ToList();

                return new AdminAmbassadorDashboardVM
                {
                    TotalAmbassadors = db.tbl_Ambassador.Count(),
                    ActiveAmbassadors = db.tbl_Ambassador.Count(a => a.Status == "Active"),
                    PendingApplications = db.tbl_Ambassador.Count(a => a.Status == "Pending"),
                    TotalReferrals = db.tbl_Referral.Count(),
                    ConvertedReferrals = db.tbl_ReferralConversion.Count(c => c.Status == "Approved"),
                    TotalCommissionsPaid = db.tbl_AmbassadorEarning
                        .Where(e => e.Status == "PaidOut")
                        .Sum(e => (decimal?)e.Amount) ?? 0,
                    PendingCommissions = db.tbl_AmbassadorEarning
                        .Where(e => e.Status == "Pending" || e.Status == "Available")
                        .Sum(e => (decimal?)e.Amount) ?? 0,
                    PendingPayoutRequests = db.tbl_PayoutRequest
                        .Count(p => p.Status == "Requested"),
                    Currency = AmbassadorSettingsHelper.GetDefaultCurrency(),
                    ChartLabels = chartLabels,
                    ReferralData = referralData,
                    ConversionData = conversionData,
                    TierLabels = tierLabels,
                    TierData = tierData,
                    TopPerformers = topPerformers,
                    RecentActivity = recentActivity
                };
            }
        }

        public AdminStatsVM GetAdminStats()
        {
            using (var db = new AmbassadorDbContext())
            {
                var now = DateTime.Now;
                var monthStart = new DateTime(now.Year, now.Month, 1);

                return new AdminStatsVM
                {
                    TotalAmbassadors = db.tbl_Ambassador.Count(),
                    ActiveAmbassadors = db.tbl_Ambassador.Count(a => a.Status == "Active"),
                    PendingApplications = db.tbl_Ambassador.Count(a => a.Status == "Pending"),
                    SuspendedAmbassadors = db.tbl_Ambassador.Count(a => a.Status == "Suspended"),

                    TotalReferrals = db.tbl_Referral.Count(),
                    TotalConversions = db.tbl_ReferralConversion.Count(c => c.Status == "Approved"),

                    TotalCommissionsPaid = db.tbl_AmbassadorEarning
                        .Where(e => e.Status == "PaidOut")
                        .Sum(e => (decimal?)e.Amount) ?? 0,

                    PendingPayouts = db.tbl_PayoutRequest
                        .Where(p => p.Status == "Requested" || p.Status == "Approved")
                        .Sum(p => (decimal?)p.Amount) ?? 0,

                    TotalRevenue = db.tbl_ReferralConversion
                        .Where(c => c.Status == "Approved")
                        .Sum(c => (decimal?)c.Amount) ?? 0,

                    NewAmbassadorsThisMonth = db.tbl_Ambassador.Count(a => a.CreatedAt >= monthStart),
                    ConversionsThisMonth = db.tbl_ReferralConversion
                        .Count(c => c.ConvertedAt >= monthStart && c.Status == "Approved"),
                    CommissionsThisMonth = db.tbl_AmbassadorEarning
                        .Where(e => e.CreatedAt >= monthStart && e.Status != "Revoked")
                        .Sum(e => (decimal?)e.Amount) ?? 0,

                    Currency = AmbassadorSettingsHelper.GetDefaultCurrency()
                };
            }
        }

        public List<AmbassadorListItemVM> GetTopAmbassadors(int count = 10)
        {
            using (var db = new AmbassadorDbContext())
            {
                var ambassadorIds = db.tbl_Referral
                    .Where(r => r.Status == "Converted")
                    .GroupBy(r => r.AmbassadorId)
                    .OrderByDescending(g => g.Count())
                    .Take(count)
                    .Select(g => g.Key)
                    .ToList();

                return db.tbl_Ambassador
                    .Include(a => a.AspNetUsers)
                    .Where(a => ambassadorIds.Contains(a.Id) && a.Status == "Active")
                    .ToList()
                    .Select(a => MapToListItemVM(a, db))
                    .OrderByDescending(a => a.ConvertedReferrals)
                    .ToList();
            }
        }

        #endregion

        #region Validation

        public bool IsValidReferralCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return false;

            using (var db = new AmbassadorDbContext())
            {
                return db.tbl_Ambassador.Any(a => 
                    a.ReferralCode == code.ToUpper() && a.Status == "Active");
            }
        }

        public bool CanApply(string userId)
        {
            using (var db = new AmbassadorDbContext())
            {
                // Check if already applied
                if (db.tbl_Ambassador.Any(a => a.UserId == userId))
                    return false;

                // Check if user exists and is verified
                var user = db.AspNetUsers.Find(userId);
                return user != null && user.EmailConfirmed;
            }
        }

        public bool IsActiveAmbassador(string userId)
        {
            using (var db = new AmbassadorDbContext())
            {
                return db.tbl_Ambassador.Any(a => 
                    a.UserId == userId && a.Status == "Active");
            }
        }

        public string GenerateReferralCode()
        {
            using (var db = new AmbassadorDbContext())
            {
                string code;
                int attempts = 0;

                do
                {
                    code = GenerateRandomCode(8);
                    attempts++;
                } while (db.tbl_Ambassador.Any(a => a.ReferralCode == code) && attempts < 10);

                return code;
            }
        }

        #endregion

        #region Private Helpers

        private AmbassadorVM MapToVM(tbl_Ambassador ambassador)
        {
            return new AmbassadorVM
            {
                Id = ambassador.Id,
                UserId = ambassador.UserId,
                Email = ambassador.AspNetUsers?.Email,
                FullName = ResolveDisplayName(ambassador),
                ReferralCode = ambassador.ReferralCode,
                Status = ambassador.Status,
                Tier = ambassador.Tier,
                University = ambassador.University,
                Country = ambassador.Country,
                PhoneNumber = ambassador.PhoneNumber,
                ApplicationNotes = ambassador.ApplicationNotes,
                ApprovedByName = ambassador.AspNetUsers1?.UserName,
                ApprovedAt = ambassador.ApprovedAt,
                RejectionReason = ambassador.Status == "Rejected" ? ambassador.SuspendedReason : null,
                SuspendedReason = ambassador.SuspendedReason,
                CreatedAt = ambassador.CreatedAt,
                UpdatedAt = ambassador.UpdatedAt
            };
        }

        private AmbassadorListItemVM MapToListItemVM(tbl_Ambassador ambassador, AmbassadorDbContext db)
        {
            var ambassadorId = ambassador.Id;

            return new AmbassadorListItemVM
            {
                Id = ambassador.Id,
                Email = ambassador.AspNetUsers?.Email,
                FullName = ResolveDisplayName(ambassador),
                ReferralCode = ambassador.ReferralCode,
                Status = ambassador.Status,
                Tier = ambassador.Tier,
                University = ambassador.University,
                CreatedAt = ambassador.CreatedAt,
                TotalReferrals = db.tbl_Referral.Count(r => r.AmbassadorId == ambassadorId),
                ConvertedReferrals = db.tbl_Referral.Count(r => 
                    r.AmbassadorId == ambassadorId && r.Status == "Converted"),
                TotalEarnings = db.tbl_AmbassadorEarning
                    .Where(e => e.AmbassadorId == ambassadorId && e.Status != "Revoked")
                    .Sum(e => (decimal?)e.Amount) ?? 0,
                AvailableBalance = db.tbl_AmbassadorEarning
                    .Where(e => e.AmbassadorId == ambassadorId && e.Status == "Available")
                    .Sum(e => (decimal?)e.Amount) ?? 0
            };
        }

        private List<ReferralVM> GetRecentReferrals(int ambassadorId, int count, AmbassadorDbContext db)
        {
            return db.tbl_Referral
                .Include(r => r.AspNetUsers)
                .Where(r => r.AmbassadorId == ambassadorId)
                .OrderByDescending(r => r.RegisteredAt)
                .Take(count)
                .ToList()
                .Select(r => new ReferralVM
                {
                    Id = r.Id,
                    AmbassadorId = r.AmbassadorId,
                    ReferredUserId = r.ReferredUserId,
                    ReferredUserEmail = r.AspNetUsers?.Email,
                    ReferredUserName = r.AspNetUsers?.UserName,
                    RegisteredAt = r.RegisteredAt,
                    VerifiedAt = r.VerifiedAt,
                    Source = r.Source,
                    Status = r.Status
                })
                .ToList();
        }

        private List<EarningVM> GetRecentEarnings(int ambassadorId, int count, AmbassadorDbContext db)
        {
            return db.tbl_AmbassadorEarning
                .Where(e => e.AmbassadorId == ambassadorId)
                .OrderByDescending(e => e.CreatedAt)
                .Take(count)
                .ToList()
                .Select(e => new EarningVM
                {
                    Id = e.Id,
                    AmbassadorId = e.AmbassadorId,
                    Amount = e.Amount,
                    Currency = e.Currency,
                    Status = e.Status,
                    Type = e.Type,
                    Description = e.Description,
                    CreatedAt = e.CreatedAt,
                    AvailableAt = e.AvailableAt
                })
                .ToList();
        }

        private List<PayoutRequestVM> GetRecentPayouts(int ambassadorId, int count, AmbassadorDbContext db)
        {
            return db.tbl_PayoutRequest
                .Include(p => p.tbl_PayoutMethod)
                .Where(p => p.AmbassadorId == ambassadorId)
                .OrderByDescending(p => p.RequestedAt)
                .Take(count)
                .ToList()
                .Select(p => new PayoutRequestVM
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Status = p.Status,
                    RequestedAt = p.RequestedAt,
                    ProcessedAt = p.ProcessedAt,
                    TransactionRef = p.TransactionRef,
                    PayoutMethod = p.tbl_PayoutMethod != null ? new PayoutMethodVM
                    {
                        MethodType = p.tbl_PayoutMethod.MethodType,
                        MaskedAccountNumber = p.tbl_PayoutMethod.MaskedAccountNumber
                    } : null
                })
                .ToList();
        }

        private List<PayoutRequestVM> GetPendingPayoutRequests(int count, AmbassadorDbContext db)
        {
            return db.tbl_PayoutRequest
                .Include(p => p.tbl_Ambassador)
                .Include(p => p.tbl_Ambassador.AspNetUsers)
                .Include(p => p.tbl_PayoutMethod)
                .Where(p => p.Status == "Requested")
                .OrderBy(p => p.RequestedAt)
                .Take(count)
                .ToList()
                .Select(p => new PayoutRequestVM
                {
                    Id = p.Id,
                    AmbassadorId = p.AmbassadorId,
                    AmbassadorEmail = p.tbl_Ambassador?.AspNetUsers?.Email,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Status = p.Status,
                    RequestedAt = p.RequestedAt,
                    PayoutMethod = p.tbl_PayoutMethod != null ? new PayoutMethodVM
                    {
                        MethodType = p.tbl_PayoutMethod.MethodType,
                        MaskedAccountNumber = p.tbl_PayoutMethod.MaskedAccountNumber
                    } : null
                })
                .ToList();
        }

        private ChartDataVM GetAdminConversionsChart(int months)
        {
            var chartData = new ChartDataVM { ChartType = "bar" };
            using (var db = new AmbassadorDbContext())
            {
                var now = DateTime.Now;
                for (int i = months - 1; i >= 0; i--)
                {
                    var date = now.AddMonths(-i);
                    var monthStart = new DateTime(date.Year, date.Month, 1);
                    var monthEnd = monthStart.AddMonths(1);

                    chartData.Labels.Add(date.ToString("MMM yyyy"));
                    chartData.Values.Add(db.tbl_ReferralConversion
                        .Count(c => c.ConvertedAt >= monthStart && 
                                   c.ConvertedAt < monthEnd && 
                                   c.Status == "Approved"));
                }
            }
            return chartData;
        }

        private ChartDataVM GetAdminPayoutsChart(int months)
        {
            var chartData = new ChartDataVM { ChartType = "bar" };
            using (var db = new AmbassadorDbContext())
            {
                var now = DateTime.Now;
                for (int i = months - 1; i >= 0; i--)
                {
                    var date = now.AddMonths(-i);
                    var monthStart = new DateTime(date.Year, date.Month, 1);
                    var monthEnd = monthStart.AddMonths(1);

                    chartData.Labels.Add(date.ToString("MMM yyyy"));
                    chartData.Values.Add(db.tbl_PayoutRequest
                        .Where(p => p.ProcessedAt >= monthStart && 
                                   p.ProcessedAt < monthEnd && 
                                   p.Status == "Paid")
                        .Sum(p => (decimal?)p.Amount) ?? 0);
                }
            }
            return chartData;
        }

        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Removed confusing chars
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static string ResolveDisplayName(tbl_Ambassador ambassador)
        {
            if (!string.IsNullOrWhiteSpace(ambassador?.FullName))
            {
                return ambassador.FullName;
            }

            if (!string.IsNullOrWhiteSpace(ambassador?.AspNetUsers?.UserName) &&
                !string.Equals(ambassador.AspNetUsers.UserName, ambassador.AspNetUsers.Email, StringComparison.OrdinalIgnoreCase))
            {
                return ambassador.AspNetUsers.UserName;
            }

            return ambassador?.AspNetUsers?.Email ?? "Unknown";
        }

        #endregion
    }
}
