using First_Aid_Made_Easy.DAL;
using Serilog;
using System;
using System.Globalization;
using System.Linq;
using System.Web;

namespace First_Aid_Made_Easy.BLL
{
    public static class ReferralTrackingHelper
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(ReferralTrackingHelper));
        private const string ReferralCookieName = "fame_ref";
        private const int ReferralCookieExpiryDays = 30;

        public static bool TryAttributeReferral(string userId, string email, string fullName, HttpRequestBase request, HttpResponseBase response, bool isVerified = false)
        {
            try
            {
                if (!FeatureFlags.IsReferralTrackingEnabled)
                {
                    return false;
                }

                var referralInfo = GetReferralFromRequest(request);
                if (referralInfo == null)
                {
                    return false;
                }

                using (var db = new AmbassadorDbContext())
                {
                    var ambassador = db.tbl_Ambassador.FirstOrDefault(a =>
                        a.ReferralCode == referralInfo.ReferralCode &&
                        a.Status == "Active");

                    if (ambassador == null)
                    {
                        _logger.Warning("Referral code {Code} not found or inactive", referralInfo.ReferralCode);
                        ClearReferralCookie(response);
                        return false;
                    }

                    var existingReferral = db.tbl_Referral.FirstOrDefault(r => r.ReferredUserId == userId);
                    if (existingReferral != null)
                    {
                        _logger.Warning("User {UserId} already has a referral record", userId);
                        ClearReferralCookie(response);
                        return false;
                    }

                    var normalizedSource = NormalizeSource(referralInfo.Source);

                    long? clickId = null;
                    if (normalizedSource == "Link" && referralInfo.ClickedAt.HasValue)
                    {
                        var clickWindow = referralInfo.ClickedAt.Value.AddMinutes(-5);
                        var click = db.tbl_ReferralClick
                            .Where(c => c.AmbassadorId == ambassador.Id && c.CreatedAt >= clickWindow)
                            .OrderByDescending(c => c.CreatedAt)
                            .FirstOrDefault();

                        clickId = click?.Id;
                    }

                    var now = DateTime.UtcNow;
                    var referral = new tbl_Referral
                    {
                        AmbassadorId = ambassador.Id,
                        ReferredUserId = userId,
                        RegisteredAt = now,
                        VerifiedAt = isVerified ? now : (DateTime?)null,
                        Status = isVerified ? "Verified" : "Registered",
                        Source = normalizedSource,
                        ClickId = clickId,
                        Notes = string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(fullName)
                            ? null
                            : $"Referred signup: {fullName ?? email}"
                    };

                    db.tbl_Referral.Add(referral);
                    db.SaveChanges();

                    ClearReferralCookie(response);

                    _logger.Information(
                        "Referral attributed: User {UserId} -> Ambassador {AmbassadorId} (Code: {Code}, Source: {Source})",
                        userId,
                        ambassador.Id,
                        referralInfo.ReferralCode,
                        referral.Source);

                    TryAwardRegistrationBonus(db, ambassador.Id, referral.Id);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in referral attribution for user {UserId} - safely ignored", userId);
                return false;
            }
        }

        public static bool TryMarkReferralVerified(string userId)
        {
            try
            {
                using (var db = new AmbassadorDbContext())
                {
                    var referral = db.tbl_Referral.FirstOrDefault(r => r.ReferredUserId == userId);
                    if (referral == null)
                    {
                        return false;
                    }

                    if (referral.VerifiedAt == null)
                    {
                        referral.VerifiedAt = DateTime.UtcNow;
                    }

                    if (referral.Status == "Registered")
                    {
                        referral.Status = "Verified";
                    }

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error marking referral as verified for user {UserId}", userId);
                return false;
            }
        }

        public static bool TryStoreReferralCode(string referralCode, string source, HttpRequestBase request, HttpResponseBase response)
        {
            try
            {
                if (!FeatureFlags.IsReferralTrackingEnabled || string.IsNullOrWhiteSpace(referralCode) || response == null)
                {
                    return false;
                }

                var normalizedCode = referralCode.Trim().ToUpperInvariant();
                using (var db = new AmbassadorDbContext())
                {
                    var isValid = db.tbl_Ambassador.Any(a => a.ReferralCode == normalizedCode && a.Status == "Active");
                    if (!isValid)
                    {
                        return false;
                    }
                }

                SetReferralCookie(response, normalizedCode, NormalizeSource(source), request != null && request.IsSecureConnection);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error storing referral code {Code}", referralCode);
                return false;
            }
        }

        public static string GetCurrentReferralCode(HttpRequestBase request)
        {
            return GetReferralFromRequest(request)?.ReferralCode;
        }

        private static void TryAwardRegistrationBonus(AmbassadorDbContext db, int ambassadorId, int referralId)
        {
            try
            {
                if (!FeatureFlags.IsCommissionProcessingEnabled)
                {
                    return;
                }

                var now = DateTime.UtcNow;
                var rule = db.tbl_CommissionRule
                    .Where(r => r.IsActive && r.AppliesTo == "Registration" && r.EffectiveFrom <= now && (r.EffectiveTo == null || r.EffectiveTo >= now))
                    .OrderByDescending(r => r.Priority)
                    .ThenByDescending(r => r.Id)
                    .FirstOrDefault();

                if (rule == null)
                {
                    return;
                }

                if (!string.Equals(rule.CommissionType, "Flat", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(rule.CommissionType, "Fixed", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (rule.CommissionValue <= 0)
                {
                    return;
                }

                var holdDays = AmbassadorSettingsHelper.GetCommissionHoldDays();
                var availableAt = now.AddDays(holdDays);

                db.tbl_AmbassadorEarning.Add(new tbl_AmbassadorEarning
                {
                    AmbassadorId = ambassadorId,
                    ConversionId = null,
                    Amount = rule.CommissionValue,
                    Currency = AmbassadorSettingsHelper.GetDefaultCurrency(),
                    Type = "RegistrationBonus",
                    Status = holdDays > 0 ? "Pending" : "Available",
                    Description = $"Registration bonus for referral #{referralId}",
                    CreatedAt = now,
                    AvailableAt = availableAt,
                    RuleId = rule.Id
                });

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error awarding registration bonus for referral {ReferralId}", referralId);
            }
        }

        private static ReferralCookieData GetReferralFromRequest(HttpRequestBase request)
        {
            var cookieData = GetReferralFromCookie(request);
            if (cookieData != null)
            {
                return cookieData;
            }

            var refCode = request?["refCode"];
            if (!string.IsNullOrWhiteSpace(refCode))
            {
                return new ReferralCookieData
                {
                    ReferralCode = refCode.Trim().ToUpperInvariant(),
                    Source = "Code",
                    ClickedAt = null
                };
            }

            var manualCode = request?["ReferralCode"];
            if (!string.IsNullOrWhiteSpace(manualCode))
            {
                return new ReferralCookieData
                {
                    ReferralCode = manualCode.Trim().ToUpperInvariant(),
                    Source = "Code",
                    ClickedAt = null
                };
            }

            return null;
        }

        private static ReferralCookieData GetReferralFromCookie(HttpRequestBase request)
        {
            try
            {
                var cookie = request?.Cookies[ReferralCookieName];
                if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value))
                {
                    return null;
                }

                var parts = cookie.Value.Split('|');
                if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
                {
                    return null;
                }

                return new ReferralCookieData
                {
                    ReferralCode = parts[0].Trim().ToUpperInvariant(),
                    Source = parts.Length > 1 ? parts[1] : "Link",
                    ClickedAt = parts.Length > 2 && DateTime.TryParse(parts[2], null, DateTimeStyles.RoundtripKind, out var clickedAt)
                        ? clickedAt
                        : (DateTime?)null
                };
            }
            catch
            {
                return null;
            }
        }

        private static string NormalizeSource(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return "Link";
            }

            switch (source.Trim().ToLowerInvariant())
            {
                case "code":
                case "manual":
                    return "Code";
                default:
                    return "Link";
            }
        }

        private static void SetReferralCookie(HttpResponseBase response, string referralCode, string source, bool secure)
        {
            response.Cookies.Add(new HttpCookie(ReferralCookieName)
            {
                Value = string.Join("|", referralCode, source, DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)),
                Expires = DateTime.UtcNow.AddDays(ReferralCookieExpiryDays),
                HttpOnly = true,
                Secure = secure,
                Path = "/"
            });
        }

        private static void ClearReferralCookie(HttpResponseBase response)
        {
            try
            {
                response?.Cookies.Add(new HttpCookie(ReferralCookieName)
                {
                    Value = string.Empty,
                    Expires = DateTime.UtcNow.AddDays(-1),
                    Path = "/"
                });
            }
            catch
            {
            }
        }

        private class ReferralCookieData
        {
            public string ReferralCode { get; set; }
            public string Source { get; set; }
            public DateTime? ClickedAt { get; set; }
        }
    }
}
