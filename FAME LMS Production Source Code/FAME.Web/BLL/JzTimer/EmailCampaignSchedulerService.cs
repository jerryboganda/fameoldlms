using First_Aid_Made_Easy.DAL;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;

namespace First_Aid_Made_Easy.BLL.JzTimer
{
    /// <summary>
    /// Background service that processes scheduled email campaigns and drip queue.
    /// Polls every 60 seconds, same pattern as EmailSenderService.
    /// </summary>
    public class EmailCampaignSchedulerService
    {
        private Timer _timer;
        private bool _isProcessing;
        private readonly object _lock = new object();

        public void Start()
        {
            // Poll every 60 seconds
            _timer = new Timer(ProcessQueue, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60));
            JzLogger.WriteVerbose(null, "EmailCampaignSchedulerService started");
        }

        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, 0);
            _timer?.Dispose();
            JzLogger.WriteVerbose(null, "EmailCampaignSchedulerService stopped");
        }

        private void ProcessQueue(object state)
        {
            lock (_lock)
            {
                if (_isProcessing) return;
                _isProcessing = true;
            }

            try
            {
                ProcessScheduledCampaigns();
                ProcessDripQueue();
                ResetDailySendCounts();
            }
            catch (Exception ex)
            {
                JzLogger.WriteError(ex, "EmailCampaignSchedulerService.ProcessQueue");
            }
            finally
            {
                _isProcessing = false;
            }
        }

        /// <summary>
        /// Find campaigns with Status='Scheduled' and ScheduledAt <= now, then trigger send.
        /// </summary>
        private void ProcessScheduledCampaigns()
        {
            try
            {
                using (var db = new EmailMarketingDbContext())
                {
                    var now = DateTime.Now;
                    var scheduled = db.tbl_EmailCampaigns
                        .Where(c => c.Status == "Scheduled" && c.ScheduledAt <= now)
                        .ToList();

                    if (!scheduled.Any()) return;

                    var repo = new EmailMarketingRepository();
                    foreach (var campaign in scheduled)
                    {
                        try
                        {
                            JzLogger.WriteVerbose(null, $"EmailCampaignScheduler: Sending scheduled campaign #{campaign.Id} '{campaign.CampaignName}'");
                            var task = repo.SendCampaignAsync(campaign.Id);
                            task.Wait(); // Block until queued (actual sends happen on background thread)
                        }
                        catch (Exception ex)
                        {
                            JzLogger.WriteError(ex, $"EmailCampaignScheduler: Failed to send campaign #{campaign.Id}");
                            campaign.Status = "Failed";
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                JzLogger.WriteError(ex, "EmailCampaignScheduler.ProcessScheduledCampaigns");
            }
        }

        /// <summary>
        /// Process pending drip queue items whose ScheduledAt <= now.
        /// </summary>
        private void ProcessDripQueue()
        {
            try
            {
                using (var db = new EmailMarketingDbContext())
                {
                    var now = DateTime.Now;
                    var pendingItems = db.tbl_EmailDripQueue
                        .Include(q => q.Step)
                        .Include(q => q.DripCampaign)
                        .Where(q => q.Status == "Pending" && q.ScheduledAt <= now)
                        .Take(100) // Process max 100 at a time
                        .ToList();

                    if (!pendingItems.Any()) return;

                    var repo = new EmailMarketingRepository();

                    foreach (var item in pendingItems)
                    {
                        try
                        {
                            // Get sender account
                            var senderAccountId = item.DripCampaign.FromAccountId;
                            tbl_EmailSenderAccounts senderAccount = null;

                            if (senderAccountId.HasValue)
                                senderAccount = db.tbl_EmailSenderAccounts.Find(senderAccountId.Value);
                            else
                                senderAccount = db.tbl_EmailSenderAccounts.FirstOrDefault(a => a.IsDefault && a.IsActive);

                            if (senderAccount == null)
                            {
                                item.Status = "Failed";
                                item.ErrorMessage = "No sender account configured";
                                continue;
                            }

                            // Get email body
                            var htmlBody = item.Step.HtmlBody;
                            if (item.Step.TemplateId.HasValue)
                            {
                                var template = db.tbl_EmailTemplates.Find(item.Step.TemplateId.Value);
                                if (template != null)
                                    htmlBody = template.HtmlBody;
                            }

                            if (string.IsNullOrEmpty(htmlBody))
                            {
                                item.Status = "Failed";
                                item.ErrorMessage = "No email body configured for this step";
                                continue;
                            }

                            // Check if user unsubscribed
                            if (db.tbl_EmailUnsubscribes.Any(u => u.Email == item.Email && !u.IsResubscribed))
                            {
                                item.Status = "Cancelled";
                                item.ErrorMessage = "User unsubscribed";
                                continue;
                            }

                            // Process merge tags
                            var recipient = new Models.AudienceRecipientVM
                            {
                                UserId = item.UserId,
                                Email = item.Email,
                                Name = item.Email // Will be replaced by merge tags if available
                            };
                            htmlBody = repo.ProcessMergeTags(htmlBody, recipient);

                            // Send the email (using reflection to access private method, or create a public wrapper)
                            // For now, use Common.SendMail as a fallback
                            var msg = new Models.MessageVM
                            {
                                Subject = item.Step.Subject,
                                Body = htmlBody,
                                Destination = item.Email
                            };
                            var sendTask = Common.SendMail(msg);
                            sendTask.Wait();

                            item.Status = "Sent";
                            item.SentAt = DateTime.Now;

                            JzLogger.WriteVerbose(null, $"EmailCampaignScheduler: Drip email sent to {item.Email} (Drip #{item.DripCampaignId}, Step #{item.StepId})");
                        }
                        catch (Exception ex)
                        {
                            item.Status = "Failed";
                            item.ErrorMessage = ex.Message.Length > 500 ? ex.Message.Substring(0, 500) : ex.Message;
                            JzLogger.WriteError(ex, $"EmailCampaignScheduler: Drip send failed for {item.Email}");
                        }
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                JzLogger.WriteError(ex, "EmailCampaignScheduler.ProcessDripQueue");
            }
        }

        /// <summary>
        /// Reset daily send counts at midnight.
        /// </summary>
        private void ResetDailySendCounts()
        {
            try
            {
                using (var db = new EmailMarketingDbContext())
                {
                    var today = DateTime.Today;
                    var accountsToReset = db.tbl_EmailSenderAccounts
                        .Where(a => a.LastResetDate == null || a.LastResetDate < today)
                        .ToList();

                    if (!accountsToReset.Any()) return;

                    foreach (var account in accountsToReset)
                    {
                        account.SentToday = 0;
                        account.LastResetDate = today;
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                JzLogger.WriteError(ex, "EmailCampaignScheduler.ResetDailySendCounts");
            }
        }
    }
}
