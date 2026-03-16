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
    /// Service for calculating and managing ambassador commissions
    /// </summary>
    public class CommissionService : ICommissionService
    {
        private readonly AmbassadorDbContext _context;
        private readonly ILogger _logger;

        public CommissionService()
        {
            _context = new AmbassadorDbContext();
            _logger = Log.ForContext<CommissionService>();
        }

        #region Rule Management

        public List<CommissionRuleVM> GetRules(bool activeOnly = true)
        {
            var query = _context.tbl_CommissionRule.AsQueryable();

            if (activeOnly)
                query = query.Where(r => r.IsActive);

            return query
                .OrderBy(r => r.AppliesTo)
                .ThenBy(r => r.MinTier)
                .ToList()
                .Select(MapToRuleVM)
                .ToList();
        }

        public List<CommissionRuleVM> GetActiveRules(string tier = null)
        {
            var query = _context.tbl_CommissionRule.Where(r => r.IsActive);

            if (!string.IsNullOrEmpty(tier))
                query = query.Where(r => r.MinTier == null || r.MinTier == tier);

            return query
                .OrderBy(r => r.AppliesTo)
                .ThenBy(r => r.MinTier)
                .ToList()
                .Select(MapToRuleVM)
                .ToList();
        }

        public CommissionRuleVM GetRule(int ruleId)
        {
            var rule = _context.tbl_CommissionRule.Find(ruleId);
            return rule != null ? MapToRuleVM(rule) : null;
        }

        public int CreateRule(CommissionRuleVM model)
        {
            try
            {
                var rule = new tbl_CommissionRule
                {
                    RuleName = model.RuleName,
                    CommissionType = model.CommissionType,
                    CommissionValue = model.CommissionValue,
                    AppliesTo = model.AppliesTo,
                    MinTier = model.MinTier,
                    Priority = model.Priority,
                    EffectiveFrom = model.EffectiveFrom,
                    EffectiveTo = model.EffectiveTo,
                    IsActive = model.IsActive,
                    CreatedBy = !string.IsNullOrWhiteSpace(model.CreatedByName)
                        ? model.CreatedByName
                        : _context.AspNetUsers.Select(u => u.Id).FirstOrDefault(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.tbl_CommissionRule.Add(rule);
                _context.SaveChanges();

                _logger.Information("Commission rule created: {Name}", model.RuleName);
                return rule.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error creating commission rule");
                return 0;
            }
        }

        public bool UpdateRule(CommissionRuleVM model)
        {
            try
            {
                var rule = _context.tbl_CommissionRule.Find(model.Id);
                if (rule == null) return false;

                rule.RuleName = model.RuleName;
                rule.CommissionType = model.CommissionType;
                rule.CommissionValue = model.CommissionValue;
                rule.AppliesTo = model.AppliesTo;
                rule.MinTier = model.MinTier;
                rule.IsActive = model.IsActive;
                rule.UpdatedAt = DateTime.UtcNow;

                _context.SaveChanges();

                _logger.Information("Commission rule updated: {Id}", model.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating commission rule {Id}", model.Id);
                return false;
            }
        }

        public bool ToggleRule(int ruleId, bool isActive)
        {
            try
            {
                var rule = _context.tbl_CommissionRule.Find(ruleId);
                if (rule == null) return false;

                rule.IsActive = isActive;
                rule.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();

                _logger.Information("Commission rule {Id} toggled to {Active}", ruleId, isActive);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error toggling commission rule {Id}", ruleId);
                return false;
            }
        }

        #endregion

        #region Commission Calculation

        public decimal CalculateCommission(int ambassadorId, string eventType, decimal orderAmount = 0)
        {
            try
            {
                var ambassador = _context.tbl_Ambassador.Find(ambassadorId);
                if (ambassador == null || ambassador.Status != "Active")
                    return 0;

                var now = DateTime.UtcNow;
                var rules = _context.tbl_CommissionRule
                    .Where(r => r.IsActive && r.AppliesTo == eventType)
                    .Where(r => r.MinOrderAmount == null || orderAmount >= r.MinOrderAmount)
                    .Where(r => r.EffectiveFrom <= now && (r.EffectiveTo == null || r.EffectiveTo >= now))
                    .ToList()
                    .Where(r => IsTierEligible(r.MinTier, ambassador.Tier))
                    .OrderByDescending(r => GetTierRank(r.MinTier))
                    .ThenByDescending(r => r.Priority)
                    .ToList();

                if (!rules.Any())
                {
                    _logger.Debug("No commission rules found for event {Event}, ambassador {AmbId}", eventType, ambassadorId);
                    return 0;
                }

                // Use the first matching rule (most specific)
                var rule = rules.First();
                decimal commission = 0;

                if (rule.CommissionType == "Percentage")
                {
                    commission = orderAmount * (rule.CommissionValue / 100);
                }
                else // Flat Amount
                {
                    commission = rule.CommissionValue;
                }

                _logger.Debug("Calculated commission: {Amount} for ambassador {AmbId}, event {Event}", commission, ambassadorId, eventType);
                return Math.Round(commission, 2);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error calculating commission for ambassador {AmbId}", ambassadorId);
                return 0;
            }
        }

        public int AwardCommission(int ambassadorId, int? referralId, int? conversionId, string earningType, 
            decimal amount, string description)
        {
            try
            {
                if (amount <= 0)
                {
                    _logger.Warning("Cannot award zero or negative commission to ambassador {AmbId}", ambassadorId);
                    return 0;
                }

                var earning = new tbl_AmbassadorEarning
                {
                    AmbassadorId = ambassadorId,
                    ConversionId = conversionId,
                    Type = earningType,
                    Amount = amount,
                    Currency = AmbassadorSettingsHelper.GetDefaultCurrency(),
                    Status = "Pending",
                    Description = description,
                    CreatedAt = DateTime.UtcNow,
                    AvailableAt = DateTime.UtcNow.AddDays(GetHoldPeriodDays()) // Commission hold period
                };

                _context.tbl_AmbassadorEarning.Add(earning);
                _context.SaveChanges();

                _logger.Information("Commission awarded: {Amount} to ambassador {AmbId} for {Type}", 
                    amount, ambassadorId, earningType);
                return earning.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error awarding commission to ambassador {AmbId}", ambassadorId);
                return 0;
            }
        }

        public bool UpdateEarningStatus(int earningId, string status)
        {
            try
            {
                var earning = _context.tbl_AmbassadorEarning.Find(earningId);
                if (earning == null) return false;

                earning.Status = status;
                _context.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error updating earning {Id} status", earningId);
                return false;
            }
        }

        #endregion

        #region Earnings Queries

        public List<EarningVM> GetEarnings(int ambassadorId, string status = null, DateTime? from = null, 
            DateTime? to = null, int page = 1, int pageSize = 20)
        {
            var query = _context.tbl_AmbassadorEarning.Where(e => e.AmbassadorId == ambassadorId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(e => e.Status == status);
            if (from.HasValue)
                query = query.Where(e => e.CreatedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(e => e.CreatedAt <= to.Value);

            return query
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .Select(MapToEarningVM)
                .ToList();
        }

        public BalanceVM GetBalance(int ambassadorId)
        {
            var now = DateTime.UtcNow;

            var earnings = _context.tbl_AmbassadorEarning
                .Where(e => e.AmbassadorId == ambassadorId)
                .ToList();

            var payouts = _context.tbl_PayoutRequest
                .Where(p => p.AmbassadorId == ambassadorId)
                .ToList();

            var totalEarned = earnings.Where(e => e.Status != "Cancelled").Sum(e => e.Amount);
            var pendingEarnings = earnings.Where(e => e.Status == "Pending" && e.AvailableAt > now).Sum(e => e.Amount);
            var availableEarnings = earnings.Where(e => e.Status == "Available" || (e.Status == "Pending" && e.AvailableAt <= now)).Sum(e => e.Amount);
            var totalPaidOut = payouts.Where(p => p.Status == "Paid").Sum(p => p.Amount);
            var pendingPayouts = payouts.Where(p => p.Status == "Requested" || p.Status == "Processing").Sum(p => p.Amount);

            var availableBalance = availableEarnings - totalPaidOut - pendingPayouts;

            // Get minimum payout setting
            var minPayout = GetMinPayoutAmount();

            return new BalanceVM
            {
                TotalEarnings = totalEarned,
                PendingBalance = pendingEarnings,
                AvailableBalance = Math.Max(0, availableBalance),
                TotalPaidOut = totalPaidOut,
                PendingPayouts = pendingPayouts,
                Currency = AmbassadorSettingsHelper.GetDefaultCurrency(),
                MinPayoutAmount = minPayout,
                CanRequestPayout = availableBalance >= minPayout
            };
        }

        public decimal GetTotalEarnings(int ambassadorId, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.tbl_AmbassadorEarning
                .Where(e => e.AmbassadorId == ambassadorId && e.Status != "Cancelled");

            if (from.HasValue)
                query = query.Where(e => e.CreatedAt >= from.Value);
            if (to.HasValue)
                query = query.Where(e => e.CreatedAt <= to.Value);

            return query.Sum(e => (decimal?)e.Amount) ?? 0;
        }

        public List<ChartDataPointVM> GetMonthlyEarnings(int ambassadorId, int months = 6)
        {
            var fromDate = DateTime.UtcNow.Date.AddMonths(-months);

            var data = _context.tbl_AmbassadorEarning
                .Where(e => e.AmbassadorId == ambassadorId && e.CreatedAt >= fromDate && e.Status != "Cancelled")
                .ToList()
                .GroupBy(e => new { e.CreatedAt.Year, e.CreatedAt.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Total = g.Sum(e => e.Amount) })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            return data.Select(d => new ChartDataPointVM
            {
                Label = new DateTime(d.Year, d.Month, 1).ToString("MMM yyyy"),
                Value = (int)d.Total
            }).ToList();
        }

        #endregion

        #region Settings

        public decimal GetMinPayoutAmount()
        {
            return AmbassadorSettingsHelper.GetMinPayoutAmount();
        }

        public int GetHoldPeriodDays()
        {
            return AmbassadorSettingsHelper.GetCommissionHoldDays();
        }

        public int AddAdjustment(int ambassadorId, decimal amount, string type, string description, string adminUserId)
        {
            try
            {
                var earning = new tbl_AmbassadorEarning
                {
                    AmbassadorId = ambassadorId,
                    Type = type,
                    Amount = amount,
                    Currency = AmbassadorSettingsHelper.GetDefaultCurrency(),
                    Status = "Available",
                    Description = $"Admin adjustment: {description}",
                    CreatedAt = DateTime.UtcNow,
                    AvailableAt = DateTime.UtcNow
                };

                _context.tbl_AmbassadorEarning.Add(earning);
                _context.SaveChanges();

                _logger.Information("Adjustment added: Ambassador {AmbassadorId}, Amount {Amount}, Type {Type} by Admin {AdminId}",
                    ambassadorId, amount, type, adminUserId);

                return earning.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error adding adjustment for ambassador {AmbassadorId}", ambassadorId);
                return 0;
            }
        }

        #endregion

        #region Helpers

        private CommissionRuleVM MapToRuleVM(tbl_CommissionRule rule)
        {
            return new CommissionRuleVM
            {
                Id = rule.Id,
                Name = rule.RuleName,
                CommissionType = rule.CommissionType,
                Rate = rule.CommissionType == "Percentage" ? rule.CommissionValue : 0,
                FlatAmount = rule.CommissionType == "Flat" ? rule.CommissionValue : 0,
                AppliesTo = rule.AppliesTo,
                Tier = rule.MinTier,
                IsActive = rule.IsActive,
                Currency = AmbassadorSettingsHelper.GetDefaultCurrency()
            };
        }

        private static bool IsTierEligible(string minTier, string ambassadorTier)
        {
            if (string.IsNullOrWhiteSpace(minTier))
            {
                return true;
            }

            return GetTierRank(ambassadorTier) >= GetTierRank(minTier);
        }

        private static int GetTierRank(string tier)
        {
            switch ((tier ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "bronze": return 1;
                case "silver": return 2;
                case "gold": return 3;
                case "platinum": return 4;
                default: return 0;
            }
        }

        private EarningVM MapToEarningVM(tbl_AmbassadorEarning earning)
        {
            return new EarningVM
            {
                Id = earning.Id,
                AmbassadorId = earning.AmbassadorId,
                ConversionId = earning.ConversionId,
                EarningType = earning.Type,
                Amount = earning.Amount,
                Currency = earning.Currency,
                Status = earning.Status,
                Description = earning.Description,
                EarnedAt = earning.CreatedAt,
                AvailableAt = earning.AvailableAt
            };
        }

        #endregion

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
