using First_Aid_Made_Easy.DAL;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    public static class PurchaseCommissionHelper
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(PurchaseCommissionHelper));

        public static bool TryAwardPurchaseCommission(string userId, decimal purchaseAmount, string productType = "Subscription", string productName = null)
        {
            try
            {
                if (!FeatureFlags.IsCommissionProcessingEnabled || string.IsNullOrWhiteSpace(userId) || purchaseAmount <= 0)
                {
                    return false;
                }

                using (var context = new AmbassadorDbContext())
                {
                    var referral = context.tbl_Referral
                        .Where(r => r.ReferredUserId == userId)
                        .OrderByDescending(r => r.RegisteredAt)
                        .FirstOrDefault();

                    if (referral == null)
                    {
                        return false;
                    }

                    var ambassador = context.tbl_Ambassador.FirstOrDefault(a => a.Id == referral.AmbassadorId);
                    if (ambassador == null || ambassador.Status != "Active")
                    {
                        return false;
                    }

                    var now = DateTime.UtcNow;
                    var activeRules = context.tbl_CommissionRule
                        .Where(r => r.IsActive && r.EffectiveFrom <= now && (r.EffectiveTo == null || r.EffectiveTo >= now))
                        .OrderByDescending(r => r.Priority)
                        .ThenByDescending(r => r.Id)
                        .ToList();

                    var rule = FindMatchingRule(activeRules, productType, ambassador.Tier, referral.Status == "Converted");
                    if (rule == null)
                    {
                        _logger.Debug("No matching commission rule found for user {UserId} and type {Type}", userId, productType);
                        return false;
                    }

                    var commissionAmount = CalculateCommission(rule, purchaseAmount);
                    if (commissionAmount <= 0)
                    {
                        return false;
                    }

                    var conversion = new tbl_ReferralConversion
                    {
                        ReferralId = referral.Id,
                        EnrollmentId = 0,
                        Amount = purchaseAmount,
                        Currency = AmbassadorSettingsHelper.GetDefaultCurrency(),
                        Status = "Approved",
                        ConvertedAt = now,
                        ApprovedAt = now,
                        ConversionType = NormalizeConversionType(productType)
                    };

                    context.tbl_ReferralConversion.Add(conversion);

                    referral.Status = "Converted";
                    if (referral.VerifiedAt == null)
                    {
                        referral.VerifiedAt = now;
                    }

                    context.SaveChanges();

                    var holdDays = AmbassadorSettingsHelper.GetCommissionHoldDays();
                    context.tbl_AmbassadorEarning.Add(new tbl_AmbassadorEarning
                    {
                        AmbassadorId = ambassador.Id,
                        ConversionId = conversion.Id,
                        Amount = commissionAmount,
                        Currency = AmbassadorSettingsHelper.GetDefaultCurrency(),
                        Type = "Commission",
                        Status = holdDays > 0 ? "Pending" : "Available",
                        Description = string.IsNullOrWhiteSpace(productName)
                            ? $"Commission for {NormalizeConversionType(productType)} purchase"
                            : $"Commission for {productName}",
                        CreatedAt = now,
                        AvailableAt = now.AddDays(holdDays),
                        RuleId = rule.Id
                    });

                    context.SaveChanges();

                    _logger.Information(
                        "Commission awarded: {Amount} {Currency} to ambassador {AmbassadorId} for referred user {UserId}",
                        commissionAmount,
                        AmbassadorSettingsHelper.GetDefaultCurrency(),
                        ambassador.Id,
                        userId);

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in TryAwardPurchaseCommission for user {UserId}", userId);
                return false;
            }
        }

        public static bool WasUserReferred(string userId)
        {
            try
            {
                if (!FeatureFlags.IsAmbassadorProgramEnabled)
                {
                    return false;
                }

                using (var context = new AmbassadorDbContext())
                {
                    return context.tbl_Referral.Any(r => r.ReferredUserId == userId);
                }
            }
            catch
            {
                return false;
            }
        }

        public static string GetReferringAmbassadorName(string userId)
        {
            try
            {
                if (!FeatureFlags.IsAmbassadorProgramEnabled)
                {
                    return null;
                }

                using (var context = new AmbassadorDbContext())
                {
                    var referral = context.tbl_Referral.FirstOrDefault(r => r.ReferredUserId == userId);
                    if (referral == null)
                    {
                        return null;
                    }

                    var ambassador = context.tbl_Ambassador
                        .Where(a => a.Id == referral.AmbassadorId)
                        .Select(a => new { a.FullName, a.AspNetUsers.Email, a.AspNetUsers.UserName })
                        .FirstOrDefault();

                    if (ambassador == null)
                    {
                        return null;
                    }

                    if (!string.IsNullOrWhiteSpace(ambassador.FullName))
                    {
                        return ambassador.FullName;
                    }

                    return !string.IsNullOrWhiteSpace(ambassador.UserName) ? ambassador.UserName : ambassador.Email;
                }
            }
            catch
            {
                return null;
            }
        }

        private static tbl_CommissionRule FindMatchingRule(IEnumerable<tbl_CommissionRule> rules, string productType, string tier, bool alreadyConverted)
        {
            var normalizedType = NormalizeConversionType(productType);
            var appliesTo = new List<string>();

            if (!alreadyConverted)
            {
                appliesTo.Add("FirstPaymentOnly");
                appliesTo.Add("FirstPurchase");
                appliesTo.Add("Subscription");
            }

            appliesTo.Add("AllPayments");
            appliesTo.Add("AllPurchases");
            appliesTo.Add(normalizedType);

            return rules
                .Where(r => appliesTo.Any(a => string.Equals(a, r.AppliesTo, StringComparison.OrdinalIgnoreCase)))
                .Where(r => IsTierEligible(r.MinTier, tier))
                .FirstOrDefault();
        }

        private static decimal CalculateCommission(tbl_CommissionRule rule, decimal purchaseAmount)
        {
            if (string.Equals(rule.CommissionType, "Percentage", StringComparison.OrdinalIgnoreCase))
            {
                return Math.Round(purchaseAmount * (rule.CommissionValue / 100m), 2);
            }

            return rule.CommissionValue;
        }

        private static string NormalizeConversionType(string productType)
        {
            if (string.IsNullOrWhiteSpace(productType))
            {
                return "Subscription";
            }

            switch (productType.Trim().ToLowerInvariant())
            {
                case "firstpaymentonly":
                case "firstpurchase":
                    return "FirstPaymentOnly";
                case "allpayments":
                case "allpurchases":
                    return "AllPayments";
                default:
                    return "Subscription";
            }
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
                case "bronze":
                    return 1;
                case "silver":
                    return 2;
                case "gold":
                    return 3;
                case "platinum":
                    return 4;
                default:
                    return 0;
            }
        }
    }
}
