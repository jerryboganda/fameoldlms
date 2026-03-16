using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using OfficeOpenXml;
using SendGrid.Helpers.Mail.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using First_Aid_Made_Easy.BLL.Interfaces;

namespace First_Aid_Made_Easy.BLL
{

    public class TeamVM
    {
        public string Name { get; set; }
        public string Text { get; set; }
        public string Image { get; set; }
        public string Text2 { get; set; }

    }
    public class CountryVM
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string CodeS { get; set; }

    }
    public class SelectListVM
    {
        public int Value { get; set; }
        public string Text { get; set; }
        public bool InDDL { get; set; }
    }
    public class UniversityVM
    {
        public int Value { get; set; }
        public int? PackageID { get; set; }
        public int? Duration { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public string UrlAction { get; set; }
        public bool InDDL { get; set; } = true;
        public bool CanCreateTest { get; set; } = false;
        public bool AutoAccept { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
    public static class Common
    {
        #region User Info
        public static bool IsRequestAccepted(string id)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var rt = RequestType.Register.ToString();
                    var IsAccepted = db.tbl_Request.FirstOrDefault(y => y.StudentID == id && y.RequestFor == rt)?.IsAccepted ?? false;
                    return IsAccepted;
                }
            }
            catch
            {
                return false;
            }
        }
        public static tbl_User GetUserNameByASpUserID(string id)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    var user = db.tbl_User.Include("AspNetUsers").Where(y => y.User_AspUser == id).FirstOrDefault();
                    return user;
                }
            }
            catch
            {
                return null;
            }
        }
        public static string SavePicSameName(HttpPostedFileBase File, string SubFolder)
        {
            if (File != null)
            {
                var ser = HttpContext.Current.Server;
                var ext = Path.GetExtension(File.FileName);
                var FileName = Path.GetFileNameWithoutExtension(File.FileName);
                var Filename = FileName; int? c = 0;
                var Dic = "~/Images/" + SubFolder;
                while (System.IO.File.Exists(ser.MapPath(Dic + Filename + ext)))
                {
                    c++;
                    Filename = FileName + c;
                }
                File.SaveAs(ser.MapPath(Dic + Filename + ext));
                return Filename + ext;
            }
            return null;
        }
        public static string SavePic(HttpPostedFileBase File, string SubFolder, string OldPic = null)
        {

            var ser = HttpContext.Current.Server;
            DeleteFile(OldPic);

            if (File != null)
            {
                var Dir = "~/Images/" + SubFolder;
                var AbsDir = ser.MapPath(Dir);
                var FileName = Guid.NewGuid() + Path.GetExtension(File.FileName);
                if (!Directory.Exists(AbsDir))
                { Directory.CreateDirectory(AbsDir); }
                File.SaveAs(AbsDir + FileName);
                return SubFolder + FileName;
            }
            return null;
        }
        public static bool DeleteFile(string Path)
        {
            if (Path != null)
            {
                Path = HttpContext.Current.Server.MapPath(Path.StartsWith("/") ? "~/Images" : "~/Images/" + Path);
                if (File.Exists(Path))
                {
                    File.Delete(Path);
                    return true;
                }
            }
            return false;
        }
        public static string GetUserPicByASpUserID(string id)
        {
            try
            {
                using (FAMEEntities db = new FAMEEntities())
                {
                    return db.tbl_User.Where(y => y.User_AspUser == id).Select(y => y.User_Pic).FirstOrDefault();
                }
            }
            catch
            {
                return "";
            }
        }
        #endregion

        #region Encode Decode
        [Obsolete("Use SecurityHelper.RobustEncode instead for better protection.")]
        public static string Encode(string str)
        {
            byte[] bt = Encoding.ASCII.GetBytes(str);
            string str2 = Convert.ToBase64String(bt);
            byte[] bt2 = Encoding.ASCII.GetBytes(str2);

            return Convert.ToBase64String(bt2);
        }
        [Obsolete("Use SecurityHelper.RobustDecode instead.")]
        public static string Decoding(string str)
        {
            byte[] bt = Convert.FromBase64String(str);
            string str2 = Encoding.ASCII.GetString(bt);
            byte[] bt2 = Convert.FromBase64String(str2);

            return Encoding.ASCII.GetString(bt2);
        }

        #endregion

        #region Dropdown Lists


        private static IConfigurationService Config => DependencyResolver.Current.GetService<IConfigurationService>();

        [Obsolete("Use IConfigurationService via DI instead.")]
        public static List<SelectListItem> MockTestTypesDdl => Config.GetMockTestTypes();

        public static string GetDisplayName(Enum enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var displayAttribute = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;
            return displayAttribute?.GetName() ?? enumValue.ToString();
        }

        [Obsolete("Use IConfigurationService via DI instead.")]
        public static List<SelectListItem> ExamTypes => Config.GetExamTypes();

        [Obsolete("Use IConfigurationService via DI instead.")]
        public static List<SelectListItem> Roles => Config.GetRoles();

        [Obsolete("Use IConfigurationService via DI instead.")]
        public static List<UniversityVM> Universities => Config.GetUniversities();

        [Obsolete("Use IConfigurationService via DI instead.")]
        public static List<CountryVM> Country => Config.GetCountries();

        [Obsolete("Use IConfigurationService via DI instead.")]
        public static List<SelectListItem> DurationInst => Config.GetDurationInstList();

        [Obsolete("Use IConfigurationService via DI instead.")]
        public static List<SelectListItem> Duration => Config.GetDurationList();

        [Obsolete("Use IConfigurationService via DI instead.")]
        public static List<SelectListItem> Difficulty => Config.GetDifficultyList();

        [Obsolete("Use IConfigurationService via DI instead.")]
        public static List<SelectListItem> Status => Config.GetStatusList();
        public static string DifficultyS(string d)
        {
            switch (d)
            {
                case "1": return "Easy";
                case "2": return "Medium";
                case "3": return "Hard";
                default: return "N/A";
            }
        }
        public static string DifficultyS(int? d)
        {
            switch (d)
            {
                case 1: return "Easy";
                case 2: return "Medium";
                case 3: return "Hard";
                default: return "N/A";
            }
        }

        public static int? GetProp(string Name)
        {
            var v = WebConfigurationManager.AppSettings[Name];
            if (v == null) return null;
            return Convert.ToInt32(v);
        }

        #endregion

        #region Date Handeling
        public static DateTime StringToDate(string viewDateTime)
        {
            //Formating should happen here.
            return DateTime.ParseExact(viewDateTime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
        }
        public static DateTime? TryStringToDate(string viewDateTime)
        {
            try
            {
                return DateTime.ParseExact(viewDateTime, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }
        public static DateTime? TryMM_DDToDate(string viewDateTime)
        {
            try
            {
                return DateTime.ParseExact(viewDateTime, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }
        public static DateTime? RemStringToDate(string viewDateTime)
        {
            return DateTime.ParseExact(viewDateTime, "yyyy-M-d", CultureInfo.InvariantCulture);
        }
        public static DateTime? TryStringToDateTime(string viewDateTime)
        {
            try
            {
                return DateTime.ParseExact(viewDateTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }
            catch
            {
                try
                {
                    return DateTime.ParseExact(viewDateTime, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                }
                catch
                {
                    return null;
                }
            }
        }
        public static string DateToString(DateTime? dbDateTime)
        {
            return string.Format("{0:dd/MM/yyyy}", dbDateTime);
        }
        public static string GetCurrentDateForView()
        {
            DateTime dt = DateTime.UtcNow.AddHours(5);
            return string.Format("{0:dd/MM/yyyy}", dt);
        }
        public static DateTime GetCurrentDate()
        {
            DateTime dt = DateTime.UtcNow.AddHours(5);
            return dt;
        }
        public static DateTime GetEndDate(int Duration)
        {
            DateTime dt = DateTime.UtcNow.AddHours(5);

            if (Duration < 30)
                return dt.AddDays(Duration);
            else
                return dt.AddMonths(Duration / 30);
        }
        #endregion

        #region Email
        public static async Task SendMail(MessageVM message)
        {
            if (message.Destination == "") return;
            MailMessage msg = new MailMessage
            {
                From = new MailAddress(WebConfigurationManager.AppSettings["mailAccount"], "FAME LMS"), /**/
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true,
            };
            if (message.Destinations?.Count > 0)
                foreach (var m in message.Destinations)
                    msg.To.Add(m);
            else
                msg.To.Add(message.Destination);

            SmtpClient smtpClient = new SmtpClient
            {
                Host = WebConfigurationManager.AppSettings["host"],
                Port = Convert.ToInt32(WebConfigurationManager.AppSettings["port"]),
                EnableSsl = Convert.ToBoolean(WebConfigurationManager.AppSettings["ssl"]),
                Credentials = new NetworkCredential(WebConfigurationManager.AppSettings["mailAccount"], WebConfigurationManager.AppSettings["mailPassword"]),
            };
            await smtpClient.SendMailAsync(msg);
        }

        public static async Task<bool> TrySendMail(MessageVM message)
        {
            try
            {
                JzLogger.WriteVerbose(null, $"TrySendMail starting for {message?.Destination}");
                await SendMail(message);
                JzLogger.WriteVerbose(null, $"TrySendMail succeeded for {message?.Destination}");
                return true;
            }
            catch (Exception ex)
            {
                JzLogger.WriteError(ex, $"Failed to send email to {message?.Destination}");
                return false;
            }
        }

        public static string GetMessage(string code)
        {
            StreamReader str = new StreamReader(HttpContext.Current.Server.MapPath("~/Content/templates/verify-email.html"));
            string MailText = str.ReadToEnd();
            str.Close();
            return MailText.Replace("[uniqueCode]", code);

        }
        public static string GetSMS(string code)
        {
            return "\" FAME \" Your 6 digit Code is " + code;

        }
        #endregion

        #region Formating
        public static string FL(string text, int length)
        {
            if (text != null && text.Length > length)
                text = text.Substring(0, length - 3) + "...";
            return text;
        }
        public static string HrsMins(decimal? Minutes)
        {
            decimal Mins = Minutes ?? 0;
            decimal hrs = Math.Floor((Mins) / 60);
            var mins = (Mins % 60);
            if (hrs > 0 && hrs < 100 && mins > 0)
                return hrs + "hrs " + mins + "mins";
            if (hrs == 0)
                return mins + "mins";
            else
                return hrs + "hrs";
        }
        public static string HrsMins(int? Mins) => HrsMins(Convert.ToDecimal(Mins));
        public static string Hrs(decimal Mins) => Math.Floor(Mins / 60) + "hrs ";

        #endregion

        public static String ViewToString(ControllerContext controllerContext, String viewName, Object model)
        {
            if (model != null)
            {
                controllerContext.Controller.ViewData.Model = model;
            }
            using (var sw = new StringWriter())
            {
                var ViewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var ViewContext = new ViewContext(controllerContext, ViewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, sw);
                ViewResult.View.Render(ViewContext, sw);
                ViewResult.ViewEngine.ReleaseView(controllerContext, ViewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
        public static byte[] StringToExcel(string Content)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var excelPackage = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells["A1"].LoadFromText(Content, new ExcelTextFormat() { });
                return excelPackage.GetAsByteArray();
            }
        }

        public static void ExportExcel(string file, string fileName)
        {
            HttpContext.Current.Response.ClearContent();
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment; filename=" + fileName + ".xls");
            HttpContext.Current.Response.ContentType = "application/excel";
            HttpContext.Current.Response.Write(file);
            HttpContext.Current.Response.End();
        }

        public static List<T>[] Partition<T>(List<T> list, int totalPartitions)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            if (totalPartitions < 1)
                throw new ArgumentOutOfRangeException("totalPartitions");

            List<T>[] partitions = new List<T>[totalPartitions];

            int maxSize = (int)Math.Ceiling(list.Count / (double)totalPartitions);
            int k = 0;

            for (int i = 0; i < partitions.Length; i++)
            {
                partitions[i] = new List<T>();
                for (int j = k; j < k + maxSize; j++)
                {
                    if (j >= list.Count)
                        break;
                    partitions[i].Add(list[j]);
                }
                k += maxSize;
            }

            return partitions;
        }

        public static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLowerInvariant();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", ""); // Remove invalid chars
            str = Regex.Replace(str, @"\s+", " ").Trim();  // Convert multiple spaces into one space
            str = Regex.Replace(str, @"\s", "-");          // Replace spaces with hyphens
            return str;
        }
        internal static object GetUserNameByASpUserID(object createdBy)
        {
            throw new NotImplementedException();
        }
    }

    #region SMS
    public static class SMS
    {
        //=========================================
        //========== LifeTime SMS Integration =====
        //=========================================
        public static string SMSMain(IdentityMessage message)
        {
            string apiToken = WebConfigurationManager.AppSettings["LifetimeSmsToken"]; //Previously hardcoded
            string apiSecret = WebConfigurationManager.AppSettings["LifetimeSmsSecret"]; //Previously hardcoded
            string toNumber = message.Destination;
            string Masking = "FAME";
            string MessageText = message.Body;

            string jsonResponse = SendSMSPOST(apiToken, apiSecret, toNumber, Masking, MessageText);
            return jsonResponse;
        }
        public static string SendSMSPOST(string apiToken, string apiSecret, string toNumber, string Masking, string MessageText)
        {

            String api = "https://lifetimesms.com/json";
            String parameters = "api_token=" + apiToken + "&api_secret=" + apiSecret + "&to=" + toNumber + "&from=" + Masking + "&message=" + MessageText;
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(api);
            httpWebRequest.Accept = "application/json";
            httpWebRequest.ContentType = " application/x-www-form-urlencoded";
            httpWebRequest.Method = "POST";
            using (var streamWriter = new System.IO.StreamWriter(httpWebRequest.GetRequestStream()))
            {
                parameters = "api_token=" + apiToken + "&api_secret=" + apiSecret + "&to=" + toNumber + "&from=" + Masking + "&message=" + MessageText;

                streamWriter.Write(parameters);
                streamWriter.Flush();
                streamWriter.Close();
            }


            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return result.ToString();

            }
        }
        public static void SmsLTS()
        {
            //Send sms
            string apiToken = WebConfigurationManager.AppSettings["LtsExampleToken"]; 
            string apiSecret = WebConfigurationManager.AppSettings["LtsExampleSecret"]; 
            string toNumber = ""; //Your cell phone number with country code
            string Masking = "OneTech"; //Your Company Brand Name
            string MessageText = "Dear Customer Your Order Has been booked";

            String api = "https://lifetimesms.com/json";
            String parameters = "api_token=" + apiToken + "&api_secret=" + apiSecret + "&to=" + toNumber + "&from=" + Masking + "&message=" + MessageText;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(api);
            var httpResponse = (HttpWebResponse)req.GetResponse();

        }
        //=========================================
        //========== Send PK integration ==========
        //=========================================
        public static string MainSendPK()
        {
            string MyApiKey = WebConfigurationManager.AppSettings["SendPkApiKey"]; //Previously hardcoded
            string toNumber = "923400027805"; //Recepient cell phone number with country code
            string Masking = "First Aid Made Easy"; //Your Company Brand Name
            string MessageText = "SMS Sent using .Net";
            string jsonResponse = SendSMS(Masking, toNumber, MessageText, WebConfigurationManager.AppSettings["SendPkUser"], WebConfigurationManager.AppSettings["SendPkPass"], MyApiKey);
            return jsonResponse;
        }
        public static string SendSMS(string Masking, string toNumber, string MessageText, string MyUsername, string MyPassword, string MyApiKey)
        {
            String URI = "https://sendpk.com" +
            "/api/sms.php?" +
            "api_key=" + MyApiKey +
            "&sender=" + Masking +
            "&mobile=" + toNumber +
            "&message=" + Uri.UnescapeDataString(MessageText); // Visual Studio 10-15
            try
            {
                WebRequest req = WebRequest.Create(URI);
                WebResponse resp = req.GetResponse();
                var sr = new System.IO.StreamReader(resp.GetResponseStream());
                return sr.ReadToEnd().Trim();
            }
            catch (WebException ex)
            {
                var httpWebResponse = ex.Response as HttpWebResponse;
                if (httpWebResponse != null)
                {
                    switch (httpWebResponse.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            return "404:URL not found :" + URI;
                        case HttpStatusCode.BadRequest:
                            return "400:Bad Request";
                        default:
                            return httpWebResponse.StatusCode.ToString();
                    }
                }
            }
            return null;
        }

        //=========================================
        //========== Send Branded Integration =====
        //=========================================
        public static string SendSmsBranded(string dest, string body)
        {
            var id = WebConfigurationManager.AppSettings["SmsID"];
            var Psw = WebConfigurationManager.AppSettings["SmsPsw"];
            var Brand = WebConfigurationManager.AppSettings["SmsBrand"];
            string link = "http://api.m4sms.com/api/sendsms?id=" + id +
                        "&pass=" + Psw + "&mobile=" + dest +
                        "&brandname=" + Brand + "&msg=" + body +
                        "&language=English";
            WebRequest req = WebRequest.Create(link);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            string result = null;
            using (Stream str = resp.GetResponseStream())
            {
                StreamReader sr = new StreamReader(str);
                result = sr.ReadToEnd();
                sr.Close();
            }
            return result;
        }
    }
    #endregion

    public class Compression
    {
        private bool Compress(string ImagePath, int Quality, string OutPutDirectory)
        {
            using (Bitmap bitmap = new Bitmap(ImagePath))
            {
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder encoder = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters myencoders = new EncoderParameters(1);
                EncoderParameter myencoder = new EncoderParameter(encoder, Quality);
                myencoders.Param[0] = myencoder;
                bitmap.Save("", jpgEncoder, myencoders);
                return true;
            }
        }
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] encoder = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in encoder)
                if (codec.FormatID == format.Guid) return codec;
            return null;
        }
    }
}
