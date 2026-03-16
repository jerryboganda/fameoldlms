using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace First_Aid_Made_Easy.BLL
{
    public static class MaintenanceModeHelper
    {
        private static readonly object SyncRoot = new object();
        private static MaintenanceModeSettings _cachedSettings = MaintenanceModeSettings.Disabled();
        private static DateTime _nextRefreshUtc = DateTime.MinValue;
        private static DateTime _lastWriteUtc = DateTime.MinValue;

        public static MaintenanceModeSettings GetSettings()
        {
            string path = GetSettingsPath();
            DateTime fileWriteUtc = File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;

            if (DateTime.UtcNow < _nextRefreshUtc && fileWriteUtc == _lastWriteUtc)
            {
                return _cachedSettings;
            }

            lock (SyncRoot)
            {
                fileWriteUtc = File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;
                if (DateTime.UtcNow < _nextRefreshUtc && fileWriteUtc == _lastWriteUtc)
                {
                    return _cachedSettings;
                }

                _cachedSettings = LoadSettings(path);
                _lastWriteUtc = fileWriteUtc;
                _nextRefreshUtc = DateTime.UtcNow.AddSeconds(5);
                return _cachedSettings;
            }
        }

        public static bool IsEnabled()
        {
            return GetSettings().Enabled;
        }

        public static bool ShouldHandleRequest(HttpContext context)
        {
            MaintenanceModeSettings settings = GetSettings();
            if (!settings.Enabled || context == null)
            {
                return false;
            }

            HttpRequest request = context.Request;
            if (request != null && request.IsLocal && settings.AllowLocalRequests)
            {
                return false;
            }

            return true;
        }

        public static void WriteMaintenanceResponse(HttpContext context)
        {
            MaintenanceModeSettings settings = GetSettings();
            HttpResponse response = context.Response;

            response.Clear();
            response.StatusCode = 503;
            response.StatusDescription = "Service Unavailable";
            response.TrySkipIisCustomErrors = true;
            response.ContentType = "text/html; charset=utf-8";
            response.Headers["Retry-After"] = (settings.RetryAfterMinutes * 60).ToString(CultureInfo.InvariantCulture);
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            response.Write(RenderHtml(settings, context.Request?.Url?.Host));
            context.ApplicationInstance.CompleteRequest();
        }

        public static string GetSettingsPath()
        {
            return HostingEnvironment.MapPath("~/App_Data/maintenance.json")
                ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "maintenance.json");
        }

        private static MaintenanceModeSettings LoadSettings(string path)
        {
            if (!File.Exists(path))
            {
                return MaintenanceModeSettings.Disabled();
            }

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return MaintenanceModeSettings.Disabled();
                }

                MaintenanceModeSettings settings = JsonConvert.DeserializeObject<MaintenanceModeSettings>(json) ?? new MaintenanceModeSettings();
                return Sanitize(settings);
            }
            catch
            {
                return MaintenanceModeSettings.EnabledByFallback();
            }
        }

        private static MaintenanceModeSettings Sanitize(MaintenanceModeSettings settings)
        {
            if (settings == null)
            {
                settings = new MaintenanceModeSettings();
            }

            if (settings.UntilUtc.HasValue && settings.UntilUtc.Value <= DateTime.UtcNow)
            {
                return MaintenanceModeSettings.Disabled();
            }

            settings.Title = string.IsNullOrWhiteSpace(settings.Title) ? "Scheduled Maintenance" : settings.Title.Trim();

            int retryAfterMinutes = settings.RetryAfterMinutes;
            if (retryAfterMinutes <= 0 && settings.UntilUtc.HasValue)
            {
                retryAfterMinutes = Math.Max(1, (int)Math.Ceiling((settings.UntilUtc.Value - DateTime.UtcNow).TotalMinutes));
            }

            settings.RetryAfterMinutes = retryAfterMinutes <= 0 ? 30 : retryAfterMinutes;
            settings.Message = string.IsNullOrWhiteSpace(settings.Message)
                ? string.Format(CultureInfo.InvariantCulture, "Website is under maintenance. Please try again in {0} minutes.", settings.RetryAfterMinutes)
                : settings.Message.Trim();

            return settings;
        }

        private static string RenderHtml(MaintenanceModeSettings settings, string hostName)
        {
            string title = HttpUtility.HtmlEncode(settings.Title);
            string message = HttpUtility.HtmlEncode(settings.Message);
            string siteName = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(hostName) ? "this website" : hostName);
            string eta = HttpUtility.HtmlEncode(string.Format(CultureInfo.InvariantCulture, "Please try again in about {0} minutes.", settings.RetryAfterMinutes));
            string untilText = settings.UntilUtc.HasValue
                ? "Expected back: " + HttpUtility.HtmlEncode(settings.UntilUtc.Value.ToLocalTime().ToString("dd MMM yyyy hh:mm tt", CultureInfo.InvariantCulture))
                : string.Empty;

            return "<!DOCTYPE html>"
                + "<html><head><meta charset=\"utf-8\" />"
                + "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />"
                + "<title>Maintenance</title>"
                + "<style>"
                + "body{margin:0;font-family:Segoe UI,Arial,sans-serif;background:linear-gradient(135deg,#f6efe7,#dce9f2);color:#173042;min-height:100vh;display:flex;align-items:center;justify-content:center;padding:24px;}"
                + ".card{max-width:680px;width:100%;background:rgba(255,255,255,.94);border:1px solid rgba(23,48,66,.08);border-radius:24px;box-shadow:0 20px 60px rgba(23,48,66,.14);padding:32px 28px;}"
                + ".badge{display:inline-block;padding:8px 12px;border-radius:999px;background:#173042;color:#fff;font-size:12px;letter-spacing:.08em;text-transform:uppercase;margin-bottom:18px;}"
                + "h1{margin:0 0 14px;font-size:34px;line-height:1.1;}"
                + "p{margin:0 0 12px;font-size:18px;line-height:1.6;color:#365267;}"
                + ".meta{margin-top:20px;padding-top:18px;border-top:1px solid rgba(23,48,66,.1);font-size:14px;color:#5d7487;}"
                + "@media (max-width:640px){.card{padding:24px 20px;border-radius:20px;}h1{font-size:28px;}p{font-size:16px;}}"
                + "</style></head><body>"
                + "<main class=\"card\"><div class=\"badge\">Maintenance Mode</div>"
                + "<h1>" + title + "</h1>"
                + "<p>" + message + "</p>"
                + "<p>" + eta + "</p>"
                + (string.IsNullOrWhiteSpace(untilText) ? string.Empty : "<p>" + untilText + "</p>")
                + "<div class=\"meta\">" + siteName + " is temporarily unavailable while we finish maintenance safely.</div>"
                + "</main></body></html>";
        }
    }

    public class MaintenanceModeSettings
    {
        public bool Enabled { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int RetryAfterMinutes { get; set; }
        public bool AllowLocalRequests { get; set; } = true;
        public DateTime? UntilUtc { get; set; }

        public static MaintenanceModeSettings Disabled()
        {
            return new MaintenanceModeSettings
            {
                Enabled = false,
                Title = "Scheduled Maintenance",
                Message = "Website is under maintenance. Please try again in 30 minutes.",
                RetryAfterMinutes = 30,
                AllowLocalRequests = true
            };
        }

        public static MaintenanceModeSettings EnabledByFallback()
        {
            return new MaintenanceModeSettings
            {
                Enabled = true,
                Title = "Scheduled Maintenance",
                Message = "Website is under maintenance. Please try again in 30 minutes.",
                RetryAfterMinutes = 30,
                AllowLocalRequests = true
            };
        }
    }
}
