using First_Aid_Made_Easy.DAL;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    /// <summary>
    /// Feature flag service for safely enabling/disabling features in production.
    /// Uses database-backed flags that can be toggled without redeployment.
    /// Includes in-memory caching with configurable TTL.
    /// 
    /// ZERO-DOWNTIME DEPLOYMENT:
    /// - All new features start DISABLED by default
    /// - Enable via database: UPDATE tbl_Settings SET Value = 'true' WHERE Name = 'Feature:AmbassadorProgram'
    /// - Changes take effect within cache TTL (default: 5 minutes)
    /// </summary>
    public static class FeatureFlags
    {
        private static readonly ConcurrentDictionary<string, CachedValue> _cache = new ConcurrentDictionary<string, CachedValue>();
        private static readonly TimeSpan CacheTTL = TimeSpan.FromMinutes(5);
        private static readonly ILogger _logger = Log.ForContext(typeof(FeatureFlags));

        // Feature flag constants
        public const string AMBASSADOR_PROGRAM_ENABLED = "Feature:AmbassadorProgram";
        public const string AMBASSADOR_REFERRAL_TRACKING = "Feature:AmbassadorReferralTracking";
        public const string AMBASSADOR_COMMISSION_PROCESSING = "Feature:AmbassadorCommissionProcessing";

        /// <summary>
        /// Check if Ambassador Program is enabled (default: false)
        /// </summary>
        public static bool IsAmbassadorProgramEnabled => GetFlag(AMBASSADOR_PROGRAM_ENABLED, false);

        /// <summary>
        /// Check if referral tracking is enabled (default: false)
        /// </summary>
        public static bool IsReferralTrackingEnabled => GetFlag(AMBASSADOR_REFERRAL_TRACKING, false);

        /// <summary>
        /// Check if commission processing is enabled (default: false)
        /// </summary>
        public static bool IsCommissionProcessingEnabled => GetFlag(AMBASSADOR_COMMISSION_PROCESSING, false);

        /// <summary>
        /// Get a feature flag value from database with caching
        /// </summary>
        /// <param name="flagName">The setting name</param>
        /// <param name="defaultValue">Default value if not found (ALWAYS false for safety)</param>
        /// <returns>True if enabled, false otherwise</returns>
        public static bool GetFlag(string flagName, bool defaultValue = false)
        {
            try
            {
                // Check cache first
                if (_cache.TryGetValue(flagName, out var cached))
                {
                    if (DateTime.UtcNow < cached.ExpiresAt)
                    {
                        return cached.Value;
                    }
                }

                // Query database
                using (var db = new FAMEEntities())
                {
                    var setting = db.tbl_Settings.FirstOrDefault(s => s.Name == flagName);
                    var value = setting != null && 
                                !string.IsNullOrEmpty(setting.Value) &&
                                (setting.Value.Equals("true", StringComparison.OrdinalIgnoreCase) || 
                                 setting.Value == "1");

                    // Update cache
                    _cache[flagName] = new CachedValue
                    {
                        Value = value,
                        ExpiresAt = DateTime.UtcNow.Add(CacheTTL)
                    };

                    return value;
                }
            }
            catch (Exception ex)
            {
                // On ANY error, return the safe default (disabled)
                _logger.Error(ex, "Error reading feature flag {Flag}, returning default {Default}", flagName, defaultValue);
                return defaultValue;
            }
        }

        /// <summary>
        /// Force refresh a specific flag from database
        /// </summary>
        public static void RefreshFlag(string flagName)
        {
            _cache.TryRemove(flagName, out _);
            GetFlag(flagName, false); // Re-fetch
        }

        /// <summary>
        /// Clear all cached flags (use after bulk database updates)
        /// </summary>
        public static void ClearCache()
        {
            _cache.Clear();
            _logger.Information("Feature flag cache cleared");
        }

        /// <summary>
        /// Set a feature flag (for admin use)
        /// </summary>
        public static bool SetFlag(string flagName, bool enabled)
        {
            try
            {
                using (var db = new FAMEEntities())
                {
                    var setting = db.tbl_Settings.FirstOrDefault(s => s.Name == flagName);
                    if (setting == null)
                    {
                        setting = new tbl_Settings { Name = flagName };
                        db.tbl_Settings.Add(setting);
                    }
                    setting.Value = enabled ? "true" : "false";
                    db.SaveChanges();

                    // Invalidate cache
                    _cache.TryRemove(flagName, out _);

                    _logger.Information("Feature flag {Flag} set to {Value}", flagName, enabled);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error setting feature flag {Flag}", flagName);
                return false;
            }
        }

        private class CachedValue
        {
            public bool Value { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
