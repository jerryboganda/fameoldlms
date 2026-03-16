using Aspose.Words;
using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using Newtonsoft.Json;
using RazorEngine;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace First_Aid_Made_Easy.BLL.JzTimer
{

    public class NotificationSenderService
    {
        private Timer _timer;
        private readonly HttpClient _httpClient;
        public NotificationSenderService()
        {
            _httpClient = new HttpClient();
            var fcmToken = System.Web.Configuration.WebConfigurationManager.AppSettings["FcmToken"];
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + fcmToken);
        }


        public void Start()
        {
            _timer = new Timer(StartSendingNotification, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
        }

        private async void StartSendingNotification(object state)
        {
            using (FAMEEntities db = new FAMEEntities())
            {
                var dNow = Common.GetCurrentDate();
                var a = db.tbl_StudentNotif.Where(x => x.Status == "Pending" && x.IsPush == true && x.IsActive == true && x.SendAt <= dNow).ToList();

                if (a.Count > 0)
                {
                    foreach (var item in a)
                    {
                        try
                        {
                            var devices = db.sp_GetDevicesNotification(item.UniversityIds ?? "", item.PackageIds ?? "").Select(x => x.DeviceID).ToList();
                            if (devices.Count > 0)
                            {
                                if (devices.Count > 1000)
                                {
                                    var devicespartitions = Common.Partition(devices, GetPartitionCount(devices.Count));
                                    foreach (var dvs in devicespartitions)
                                    {
                                        await SendNotification(dvs, item);
                                    }
                                }
                                else
                                {
                                    await SendNotification(devices, item);
                                }
                            }
                            item.Status = "Sent";
                            await db.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            JzLogger.WriteInformation("Error Notification : " + ex.Message + " => " + ex.InnerException.Message);
                        }
                    }
                }
            }
        }
        private int GetPartitionCount(int deviceCount)
        {
            if (deviceCount <= 2000) return 2;
            if (deviceCount <= 3000) return 3;
            if (deviceCount <= 4000) return 4;
            if (deviceCount <= 5000) return 5;
            if (deviceCount <= 6000) return 6;
            return 5;
        }
        private async Task SendNotification(List<string> devices, tbl_StudentNotif notif)
        {
            var payload = new
            {
                registration_ids = devices,
                notification = new
                {
                    title = notif.MessageTitle,
                    body = notif.Description,
                    click_action = "SHINY_PUSH_NOTIFICATION_CLICK",
                },
                data = new
                {
                    notif.ID,
                    notif.PicturePath,
                    notif.MessageTitle,
                    notif.MaessageBody,
                    notif.Description,
                }
            };

            var response = await _httpClient.PostAsJsonAsync("https://fcm.googleapis.com/fcm/send", payload);
            if (response.IsSuccessStatusCode)
            {
                JzLogger.WriteInformation("Sent => " + JsonConvert.SerializeObject(payload));
            }
            JzLogger.WriteInformation("Response => " + response.Content.ReadAsStringAsync().Result);
        }

        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, 0);
        }
    }

}
