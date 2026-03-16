using First_Aid_Made_Easy.DAL;
using First_Aid_Made_Easy.Models;
using First_Aid_Made_Easy.BLL;
using RazorEngine;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace First_Aid_Made_Easy.BLL.JzTimer
{

    public class EmailSenderService
    {
        private Timer _timer;
        private int _isProcessing;

        public void Start()
        {
            DateTime now = Common.GetCurrentDate();
            DateTime next8AM = now.Date.AddHours(8);
            if (now > next8AM)
            {
                next8AM = next8AM.AddDays(1);
            }
            TimeSpan timeUntil8AM = next8AM - now;
            _timer = new Timer(SendEmail, null, timeUntil8AM, TimeSpan.FromHours(24));
            //_timer = new Timer(SendEmail, null, TimeSpan.Zero, TimeSpan.FromHours(24));
            JzLogger.WriteInformation("Email Service Will Start after." + timeUntil8AM);
        }

        private void SendEmail(object state)
        {
            if (MaintenanceModeHelper.IsEnabled())
            {
                return;
            }

            if (Interlocked.Exchange(ref _isProcessing, 1) == 1)
            {
                return;
            }

            _ = SendEmailAsync();
        }

        private async Task SendEmailAsync()
        {
            try
            {
                JzLogger.WriteInformation("Email Service Started.");

                using (FAMEEntities db = new FAMEEntities())
                {
                    EmailSettingsVM settings = new SettingRepository().GetEmailSettings();
                    var students = db.sp_GetStudentsWithLastOnline(settings.AfterDays, settings.ParentAfterDays).ToList();
                    foreach (sp_GetStudentsWithLastOnline_Result student in students)
                    {
                        EmailType MessageType = student.Days == settings.AfterDays ? EmailType.FirstMessage :
                                            student.Days == settings.WarningAfterDays ? EmailType.Warning :
                                            student.Days == settings.ParentAfterDays ? EmailType.Parent : EmailType.None;
                        try
                        {
                            if (student != null && MessageType != EmailType.None)
                            {
                                #region Get Email Data To Send
                                var model = new EmailReportVM() { Settings = settings, Student = student };

                                if (settings.SendLoginInfo)
                                {
                                    model.LogInfo = await (from login in db.tbl_UserLogins
                                                           join deviceJoin in db.tbl_UserDevices on login.DeviceID equals deviceJoin.DeviceID into device
                                                           from selectedDevice in device.DefaultIfEmpty()
                                                           where login.UserID == student.Id
                                                           orderby login.DateTime descending
                                                           select new DeviceInfoVM
                                                           {
                                                               DateTime = login.DateTime,
                                                               UserAgent = selectedDevice.UserAgent,
                                                           })
                                                     .Take(5)
                                                     .ToListAsync();
                                }

                                if (settings.SendTestAttempts)
                                {
                                    model.Result = await db.tbl_ResultMaster.Where(x => x.StudentID == student.Id)
                                        .Select(x => new ResultInfoVM
                                        {
                                            Datetime = x.Datetime,
                                            Mode = x.Mode,
                                            ObtainedMarks = x.tbl_ResultDetail.Where(d => d.IsTrue == true).Count(),
                                            TotalMarks = x.tbl_ResultDetail.Count(),
                                            QuestionPaper = x.tbl_QuestionPaper.PaperTitle
                                        })
                                        .ToListAsync();
                                }

                                if (settings.SendLectureReport)
                                {
                                    try
                                    {
                                        DateTime currentDate = DateTime.UtcNow.Date;
                                        DateTime fiveDaysAgo = currentDate.AddDays(-5); // Last 5 days including today

                                        model.Watch = await db.tbl_Progress
                                            .Where(p => p.Student_Fid == student.Id && p.LastOpenDT >= fiveDaysAgo)
                                            .GroupBy(p => DbFunctions.TruncateTime(p.LastOpenDT))
                                            .Select(group => new WatchInfoVM
                                            {
                                                DateTime = group.Key,
                                                Watched = group.Sum(p => p.ScreenTime)
                                            })
                                            .OrderByDescending(group => group.DateTime)
                                            .ToListAsync();
                                    }
                                    catch (Exception ex)
                                    {
                                        model.Watch = new System.Collections.Generic.List<WatchInfoVM>();
                                        JzLogger.WriteError(ex, "tbl_Progress Error.");
                                    }
                                }

                                #endregion

                                #region Parse Data to Html
                                string templatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Settings/EmailTemplate.cshtml");
                                string template = File.ReadAllText(templatePath);
                                string renderedContent = Razor.Parse(template, model);
                                #endregion

                                #region Send Email
                                await Common.SendMail(new MessageVM
                                {
                                    Body = renderedContent,
                                    Subject = "FAME Activity Report",
                                    Destination = MessageType == EmailType.Parent ? student.FatherEmail : student.Email
                                });
                                #endregion

                                #region Save Log
                                db.tbl_EmailLogs.Add(new tbl_EmailLogs
                                {
                                    SentAt = Common.GetCurrentDate(),
                                    StudentID = student.Id,
                                    InactiveDays = student.Days,
                                    Type = (int)MessageType
                                });

                                await db.SaveChangesAsync();

                                #endregion
                            }
                        }
                        catch (Exception ex)
                        {
                            JzLogger.WriteError(ex, "Email Sending Error.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                JzLogger.WriteError(ex, "EmailSenderService failed to load email work queue.");
            }
            finally
            {
                Interlocked.Exchange(ref _isProcessing, 0);
            }
        }

        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, 0);
            _timer?.Dispose();
        }
    }

}
