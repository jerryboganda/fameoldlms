using First_Aid_Made_Easy.DAL;
using Serilog;
using System;
using System.Globalization;
using System.Linq;

namespace First_Aid_Made_Easy.BLL
{
    public static class AmbassadorSettingsHelper
    {
        private static readonly ILogger _logger = Log.ForContext(typeof(AmbassadorSettingsHelper));

        public const string DefaultCurrencySetting = "Ambassador:DefaultCurrency";
        public const string MinPayoutAmountSetting = "Ambassador:MinPayoutAmount";
        public const string CommissionHoldDaysSetting = "Ambassador:CommissionHoldDays";

        public static string GetDefaultCurrency()
        {
            return GetString(DefaultCurrencySetting, "ETB");
        }

        public static decimal GetMinPayoutAmount()
        {
            return GetDecimal(MinPayoutAmountSetting, 500m);
        }

        public static int GetCommissionHoldDays()
        {
            return GetInt(CommissionHoldDaysSetting, 7);
        }

        public static string GetString(string name, string defaultValue)
        {
            try
            {
                using (var db = new FAMEEntities())
                {
                    var value = db.tbl_Settings
                        .Where(s => s.Name == name)
                        .Select(s => s.Value)
                        .FirstOrDefault();

                    return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error reading ambassador setting {SettingName}", name);
                return defaultValue;
            }
        }

        public static int GetInt(string name, int defaultValue)
        {
            var raw = GetString(name, defaultValue.ToString(CultureInfo.InvariantCulture));
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : defaultValue;
        }

        public static decimal GetDecimal(string name, decimal defaultValue)
        {
            var raw = GetString(name, defaultValue.ToString(CultureInfo.InvariantCulture));
            return decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
                ? value
                : defaultValue;
        }

        public static bool Set(string name, string value)
        {
            try
            {
                using (var db = new FAMEEntities())
                {
                    var setting = db.tbl_Settings.FirstOrDefault(s => s.Name == name);
                    if (setting == null)
                    {
                        setting = new tbl_Settings { Name = name };
                        db.tbl_Settings.Add(setting);
                    }

                    setting.Value = value;
                    db.SaveChanges();
                }

                if (name.StartsWith("Feature:", StringComparison.OrdinalIgnoreCase))
                {
                    FeatureFlags.RefreshFlag(name);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error saving ambassador setting {SettingName}", name);
                return false;
            }
        }

        public static bool SetDecimal(string name, decimal value)
        {
            return Set(name, value.ToString(CultureInfo.InvariantCulture));
        }

        public static bool SetInt(string name, int value)
        {
            return Set(name, value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
