using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    /// <summary>
    /// Repository for managing referral tracking - clicks, registrations, and conversions
    /// </summary>
    public class ReferralRepository : IReferralRepository
    {
        private readonly AmbassadorDbContext _context;
        private readonly ILogger _logger;

        public ReferralRepository()
        {
            _context = new AmbassadorDbContext();
            _logger = Log.ForContext<ReferralRepository>();
        }

        #region Click Tracking

        public int TrackClick(string referralCode, string ipAddress, string userAgent, string landingPage, string source)
        {
            try
            {
                var ambassador = _context.tbl_Ambassador.FirstOrDefault(a => a.ReferralCode == referralCode && a.Status == "Active");
                if (ambassador == null)
                {
                    _logger.Warning("Click tracked for invalid referral code: {Code}", referralCode);
                    return 0;
                }

                // Hash IP and UserAgent for privacy
                var ipHash = ComputeHash(ipAddress ?? "");
                var userAgentHash = ComputeHash(userAgent ?? "");

                var click = new tbl_ReferralClick
                {
                    AmbassadorId = ambassador.Id,
                    CreatedAt = DateTime.UtcNow,
                    IPHash = ipHash,
                    UserAgentHash = userAgentHash,
                    LandingPage = landingPage
                };

                _context.tbl_ReferralClick.Add(click);
                _context.SaveChanges();

                _logger.Information("Click tracked for ambassador {AmbassadorId}", ambassador.Id);
                return (int)click.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error tracking click for code {Code}", referralCode);
                return 0;
            }
        }

        private string ComputeHash(string input)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash).Substring(0, 32);
            }
        }

        public int GetClickCount(int ambassadorId, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.tbl_ReferralClick.Where(c => c.AmbassadorId == ambassadorId);
            
            if (from.HasValue)
                query = query.Where(c => c.CreatedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(c => c.CreatedAt <= to.Value);

            return query.Count();
        }

        #endregion

        #region Referral Management

        public tbl_Referral GetById(int id)
        {
            return _context.tbl_Referral
                .Include(r => r.tbl_Ambassador)
                .Include(r => r.AspNetUsers)
                .Include(r => r.tbl_ReferralConversion)
                .FirstOrDefault(r => r.Id == id);
        }

        public tbl_Referral GetByReferredUserId(string userId)
        {
            return _context.tbl_Referral
                .Include(r => r.tbl_Ambassador)
                .FirstOrDefault(r => r.ReferredUserId == userId);
        }

        public List<ReferralVM> GetReferralsByAmbassador(int ambassadorId, string status = null, int page = 1, int pageSize = 20)
        {
            var query = _context.tbl_Referral
                .Include(r => r.tbl_ReferralConversion)
                .Where(r => r.AmbassadorId == ambassadorId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);

            return query
                .OrderByDescending(r => r.RegisteredAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .Select(r => MapToReferralVM(r))
                .ToList();
        }

        public PagedResult<ReferralVM> GetByAmbassador(int ambassadorId, ReferralFilterVM filter)
        {
            var query = _context.tbl_Referral
                .Include(r => r.tbl_ReferralConversion)
                .Include(r => r.AspNetUsers)
                .Where(r => r.AmbassadorId == ambassadorId);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(r => r.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var search = filter.SearchTerm.ToLower();
                query = query.Where(r => r.AspNetUsers.UserName.ToLower().Contains(search) || 
                                         r.AspNetUsers.Email.ToLower().Contains(search));
            }

            if (!string.IsNullOrEmpty(filter.Source))
                query = query.Where(r => r.Source == filter.Source);

            if (filter.FromDate.HasValue)
                query = query.Where(r => r.RegisteredAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(r => r.RegisteredAt <= filter.ToDate.Value);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(r => r.RegisteredAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList()
                .Select(r => MapToReferralVM(r))
                .ToList();

            return new PagedResult<ReferralVM>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public int GetReferralCount(int ambassadorId, string status = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.tbl_Referral.Where(r => r.AmbassadorId == ambassadorId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.Status == status);
            if (from.HasValue)
                query = query.Where(r => r.RegisteredAt >= from.Value);
            if (to.HasValue)
                query = query.Where(r => r.RegisteredAt <= to.Value);

            return query.Count();
        }

        public int GetCountByStatus(int ambassadorId, string status)
        {
            return _context.tbl_Referral.Count(r => r.AmbassadorId == ambassadorId && r.Status == status);
        }

        public Dictionary<string, int> GetAllStatusCounts(int ambassadorId)
        {
            var referrals = _context.tbl_Referral.Where(r => r.AmbassadorId == ambassadorId).ToList();
            return new Dictionary<string, int>
            {
                { "Total", referrals.Count },
                { "Registered", referrals.Count(r => r.Status == "Registered") },
                { "Verified", referrals.Count(r => r.Status == "Verified") },
                { "Converted", referrals.Count(r => r.Status == "Converted") },
                { "Invalid", referrals.Count(r => r.Status == "Invalid") }
            };
        }

        public int CreateReferral(int ambassadorId, string referredUserId, string referredEmail, string referredName, string source, int? clickId = null)
        {
            try
            {
                // Check if user was already referred
                var existing = _context.tbl_Referral.FirstOrDefault(r => r.ReferredUserId == referredUserId);
                if (existing != null)
                {
                    _logger.Warning("User {UserId} was already referred by ambassador {AmbId}", referredUserId, existing.AmbassadorId);
                    return existing.Id;
                }

                var referral = new tbl_Referral
                {
                    AmbassadorId = ambassadorId,
                    ReferredUserId = referredUserId,
                    RegisteredAt = DateTime.UtcNow,
                    Status = "Registered",
                    Source = source,
                    ClickId = clickId
                };

                _context.tbl_Referral.Add(referral);
                _context.SaveChanges();

                _logger.Information("Referral created: Ambassador {AmbId} referred user {UserId}", ambassadorId, referredUserId);
                return referral.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating referral for ambassador {AmbId}", ambassadorId);
                return 0;
            }
        }

        public bool UpdateReferralStatus(int referralId, string status)
        {
            try
            {
                var referral = _context.tbl_Referral.Find(referralId);
                if (referral == null) return false;

                referral.Status = status;
                if (string.Equals(status, "Verified", StringComparison.OrdinalIgnoreCase) && referral.VerifiedAt == null)
                    referral.VerifiedAt = DateTime.UtcNow;
                _context.SaveChanges();

                _logger.Information("Referral {Id} status updated to {Status}", referralId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating referral {Id} status", referralId);
                return false;
            }
        }

        #endregion

        #region Conversion Tracking

        public int RecordConversion(int referralId, string conversionType, decimal? amount = null, int? orderId = null)
        {
            try
            {
                var referral = _context.tbl_Referral.Find(referralId);
                if (referral == null)
                {
                    _logger.Warning("Cannot record conversion - referral {Id} not found", referralId);
                    return 0;
                }

                var conversion = new tbl_ReferralConversion
                {
                    ReferralId = referralId,
                    ConversionType = conversionType,
                    ConvertedAt = DateTime.UtcNow,
                    Amount = amount ?? 0,
                    OrderId = orderId,
                    Status = "Approved",
                    Currency = AmbassadorSettingsHelper.GetDefaultCurrency(),
                    ApprovedAt = DateTime.UtcNow,
                    EnrollmentId = 0
                };

                _context.tbl_ReferralConversion.Add(conversion);

                // Update referral status
                referral.Status = "Converted";

                _context.SaveChanges();

                _logger.Information("Conversion recorded: Referral {ReferralId}, Type {Type}, Amount {Amount}", 
                    referralId, conversionType, amount);
                return conversion.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error recording conversion for referral {Id}", referralId);
                return 0;
            }
        }

        public List<ConversionVM> GetConversionsByAmbassador(int ambassadorId, DateTime? from = null, DateTime? to = null)
        {
            var query = from c in _context.tbl_ReferralConversion
                        join r in _context.tbl_Referral
                            .Include(x => x.AspNetUsers)
                            .Include(x => x.tbl_Ambassador)
                            .Include(x => x.tbl_Ambassador.AspNetUsers)
                            on c.ReferralId equals r.Id
                        where r.AmbassadorId == ambassadorId
                        select new { Conversion = c, Referral = r };

            if (from.HasValue)
                query = query.Where(x => x.Conversion.ConvertedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(x => x.Conversion.ConvertedAt <= to.Value);

            return query
                .OrderByDescending(x => x.Conversion.ConvertedAt)
                .ToList()
                .Select(x => new ConversionVM
                {
                    Id = x.Conversion.Id,
                    ReferralId = x.Referral.Id,
                    StudentName = x.Referral.ReferredName,
                    StudentEmail = x.Referral.ReferredEmail,
                    AmbassadorName = !string.IsNullOrWhiteSpace(x.Referral.tbl_Ambassador.FullName)
                        ? x.Referral.tbl_Ambassador.FullName
                        : x.Referral.tbl_Ambassador.Email,
                    AmbassadorEmail = x.Referral.tbl_Ambassador.Email,
                    ReferredName = x.Referral.ReferredName,
                    ConversionType = x.Conversion.ConversionType,
                    Amount = x.Conversion.Amount,
                    Currency = x.Conversion.Currency,
                    CommissionAmount = _context.tbl_AmbassadorEarning
                        .Where(e => e.ConversionId == x.Conversion.Id)
                        .Sum(e => (decimal?)e.Amount) ?? 0,
                    ConvertedAt = x.Conversion.ConvertedAt,
                    Status = x.Conversion.Status,
                    OrderId = x.Conversion.OrderId
                })
                .ToList();
        }

        public PagedResult<ConversionVM> GetAllConversions(ConversionFilterVM filter)
        {
            var query = from c in _context.tbl_ReferralConversion
                        join r in _context.tbl_Referral
                            .Include(x => x.AspNetUsers)
                            .Include(x => x.tbl_Ambassador)
                            .Include(x => x.tbl_Ambassador.AspNetUsers)
                            on c.ReferralId equals r.Id
                        select new { Conversion = c, Referral = r };

            if (filter.AmbassadorId.HasValue)
                query = query.Where(x => x.Referral.AmbassadorId == filter.AmbassadorId.Value);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(x => x.Conversion.Status == filter.Status);

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(x => x.Conversion.ConvertedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList()
                .Select(x => new ConversionVM
                {
                    Id = x.Conversion.Id,
                    ReferralId = x.Referral.Id,
                    StudentName = x.Referral.ReferredName,
                    StudentEmail = x.Referral.ReferredEmail,
                    AmbassadorName = !string.IsNullOrWhiteSpace(x.Referral.tbl_Ambassador.FullName)
                        ? x.Referral.tbl_Ambassador.FullName
                        : x.Referral.tbl_Ambassador.Email,
                    AmbassadorEmail = x.Referral.tbl_Ambassador.Email,
                    ReferredName = x.Referral.ReferredName,
                    ConversionType = x.Conversion.ConversionType,
                    Amount = x.Conversion.Amount,
                    Currency = x.Conversion.Currency,
                    CommissionAmount = _context.tbl_AmbassadorEarning
                        .Where(e => e.ConversionId == x.Conversion.Id)
                        .Sum(e => (decimal?)e.Amount) ?? 0,
                    ConvertedAt = x.Conversion.ConvertedAt,
                    Status = x.Conversion.Status,
                    OrderId = x.Conversion.OrderId
                })
                .ToList();

            return new PagedResult<ConversionVM>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public bool ApproveConversion(int conversionId, string adminUserId)
        {
            try
            {
                var conversion = _context.tbl_ReferralConversion.Find(conversionId);
                if (conversion == null) return false;

                conversion.Status = "Approved";
                conversion.ApprovedAt = DateTime.UtcNow;
                _context.SaveChanges();

                _logger.Information("Conversion {ConversionId} approved by admin {AdminId}", conversionId, adminUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error approving conversion {ConversionId}", conversionId);
                return false;
            }
        }

        public bool RevokeConversion(int conversionId, string reason, string adminUserId)
        {
            try
            {
                var conversion = _context.tbl_ReferralConversion.Find(conversionId);
                if (conversion == null) return false;

                conversion.Status = "Revoked";
                conversion.RevokedAt = DateTime.UtcNow;
                conversion.RevokeReason = reason;
                _context.SaveChanges();

                _logger.Information("Conversion {ConversionId} revoked by admin {AdminId}. Reason: {Reason}", conversionId, adminUserId, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error revoking conversion {ConversionId}", conversionId);
                return false;
            }
        }

        public int GetConversionCount(int ambassadorId, DateTime? from = null, DateTime? to = null)
        {
            var query = from c in _context.tbl_ReferralConversion
                        join r in _context.tbl_Referral on c.ReferralId equals r.Id
                        where r.AmbassadorId == ambassadorId
                        select c;

            if (from.HasValue)
                query = query.Where(c => c.ConvertedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(c => c.ConvertedAt <= to.Value);

            return query.Count();
        }

        public decimal GetConversionValue(int ambassadorId, DateTime? from = null, DateTime? to = null)
        {
            var query = from c in _context.tbl_ReferralConversion
                        join r in _context.tbl_Referral on c.ReferralId equals r.Id
                        where r.AmbassadorId == ambassadorId
                        select c;

            if (from.HasValue)
                query = query.Where(c => c.ConvertedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(c => c.ConvertedAt <= to.Value);

            return query.Sum(c => (decimal?)c.Amount) ?? 0;
        }

        #endregion

        #region Analytics

        public Dictionary<string, int> GetReferralsBySource(int ambassadorId)
        {
            return _context.tbl_Referral
                .Where(r => r.AmbassadorId == ambassadorId)
                .GroupBy(r => r.Source ?? "Direct")
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public List<ChartDataPointVM> GetDailyReferrals(int ambassadorId, int days = 30)
        {
            var fromDate = DateTime.UtcNow.Date.AddDays(-days);

            var data = _context.tbl_Referral
                .Where(r => r.AmbassadorId == ambassadorId && r.RegisteredAt >= fromDate)
                .GroupBy(r => DbFunctions.TruncateTime(r.RegisteredAt))
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToList();

            return data.Select(d => new ChartDataPointVM
            {
                Label = d.Date?.ToString("MMM dd") ?? "",
                Value = d.Count
            }).ToList();
        }

        public List<ChartDataPointVM> GetMonthlyReferrals(int ambassadorId, int months = 6)
        {
            var fromDate = DateTime.UtcNow.Date.AddMonths(-months);

            var data = _context.tbl_Referral
                .Where(r => r.AmbassadorId == ambassadorId && r.RegisteredAt >= fromDate)
                .ToList() // Execute query first
                .GroupBy(r => new { r.RegisteredAt.Year, r.RegisteredAt.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            return data.Select(d => new ChartDataPointVM
            {
                Label = new DateTime(d.Year, d.Month, 1).ToString("MMM yyyy"),
                Value = d.Count
            }).ToList();
        }

        #endregion

        #region Helpers

        private ReferralVM MapToReferralVM(tbl_Referral r)
        {
            var latestConversion = r.tbl_ReferralConversion?.OrderByDescending(c => c.ConvertedAt).FirstOrDefault();

            return new ReferralVM
            {
                Id = r.Id,
                AmbassadorId = r.AmbassadorId,
                ReferredUserId = r.ReferredUserId,
                ReferredEmail = r.ReferredEmail,
                ReferredName = r.ReferredName,
                ReferredAt = r.ReferredAt,
                Status = r.Status,
                Notes = r.Notes,
                Source = r.Source,
                HasConverted = latestConversion != null,
                ConversionType = latestConversion?.ConversionType,
                ConvertedAt = latestConversion?.ConvertedAt,
                ConversionAmount = latestConversion?.Amount ?? 0,
                Currency = latestConversion?.Currency ?? AmbassadorSettingsHelper.GetDefaultCurrency(),
                Conversion = latestConversion == null ? null : new ConversionVM
                {
                    Id = latestConversion.Id,
                    Amount = latestConversion.Amount,
                    Currency = latestConversion.Currency,
                    Status = latestConversion.Status,
                    ConvertedAt = latestConversion.ConvertedAt,
                    ApprovedAt = latestConversion.ApprovedAt,
                    ConversionType = latestConversion.ConversionType,
                    OrderId = latestConversion.OrderId ?? 0
                }
            };
        }

        #endregion

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
