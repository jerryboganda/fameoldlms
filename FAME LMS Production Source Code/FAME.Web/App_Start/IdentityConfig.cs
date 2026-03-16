using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.BLL.JzTimer;
using First_Aid_Made_Easy.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace First_Aid_Made_Easy
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            try
            {

                SmtpSettingsVM model = new SettingRepository().GetSMTPSettings();

                if (model.UseApi && model.ApiKey != null && model.ApiKey != "")
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("api-key", model.ApiKey);
                        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                        var content = new
                        {
                            sender = new { name = model.FromName, email = model.FromEmail },
                            to = new[] { new { email = message.Destination } },
                            subject = message.Subject,
                            htmlContent = message.Body
                        };

                        var response = await client.PostAsJsonAsync("https://api.brevo.com/v3/smtp/email", content);

                        if (response.IsSuccessStatusCode)
                        {
                            JzLogger.WriteVerbose(null, $"Brevo API Email Sent to : {message.Destination}");
                        }
                        else
                        {
                            var errorMessage = await response.Content.ReadAsStringAsync();
                            throw new Exception($"Brevo API Error: {errorMessage}");
                        }
                    }
                }
                else
                {
                    MailMessage msg = new MailMessage
                    {
                        From = new MailAddress(model.FromEmail, model.FromName),
                        Subject = message.Subject,
                        Body = message.Body,
                        IsBodyHtml = true,
                    };
                    msg.To.Add(new MailAddress(message.Destination));
                    SmtpClient smtpClient = new SmtpClient
                    {
                        Host = model.Host,
                        Port = Convert.ToInt32(model.Port),
                        EnableSsl = model.SSL,
                        Credentials = new NetworkCredential(model.Username, model.Password),
                    };
                    await smtpClient.SendMailAsync(msg);
                    JzLogger.WriteVerbose(null, $"SMTP Sent to : {message.Destination}");
                }
            }
            catch (SmtpException smtpEx)
            {
                JzLogger.WriteDebug(smtpEx, $"SMTP Error :");
            }
            catch (Exception ex)
            {
                JzLogger.WriteDebug(ex, $"General Email Error :");
            }
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage();
            var Brand = WebConfigurationManager.AppSettings["SmsBrand"];
            var RapidAPI = WebConfigurationManager.AppSettings["X-RapidAPI-Key"];

            switch (SMSApi.Telesign)
            {
                case SMSApi.Mocean:
                    var key = WebConfigurationManager.AppSettings["mocean-api-key"];
                    var secret = WebConfigurationManager.AppSettings["mocean-api-secret"];
                    request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("https://donald544-mocean-moceanapi-v1.p.rapidapi.com/rest/1/sms"),
                        Headers = {
                            { "X-RapidAPI-Key", "0099682042msh98e1577a9b61e34p1371acjsn82fa52a630bb" },
                            { "X-RapidAPI-Host", "donald544-mocean-moceanapi-v1.p.rapidapi.com" },
                        },
                        Content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "mocean-to", message.Destination },
                            { "mocean-api-key", key },
                            { "mocean-api-secret", secret },
                            { "mocean-from", Brand },
                            { "mocean-text", message.Body },
                        }),
                    };
                    break;
                case SMSApi.Telesign:
                    request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("https://telesign-telesign-send-sms-verification-code-v1.p.rapidapi.com/sms-verification-code"),
                        Headers =
                            {
                                { "X-RapidAPI-Key", RapidAPI },
                                { "X-RapidAPI-Host", "telesign-telesign-send-sms-verification-code-v1.p.rapidapi.com" },
                            },
                        Content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "phoneNumber", message.Destination },
                            { "verifyCode", message.Body },
                            { "appName", Brand },
                        }),
                    };
                    break;
                case SMSApi.m4sms:
                    var id = WebConfigurationManager.AppSettings["SmsID"];
                    var Psw = WebConfigurationManager.AppSettings["SmsPsw"];
                    request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri("http://api.m4sms.com/api/sendsms"),
                        Content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "id", id },
                            { "pass", Psw },
                            { "mobile", message.Destination },
                            { "brandname", Brand },
                            { "msg", message.Body },
                            { "language", "English" },
                        }),
                    };
                    break;
            }

            using (var response = await client.SendAsync(request))
            {
                var body = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                Console.WriteLine(body);
            }
            await Task.FromResult(0);
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<ApplicationDbContext>()));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 0,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }
        public override Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool rememberMe, bool shouldLockout)
        {
            return base.PasswordSignInAsync(userName, password, rememberMe, shouldLockout);
        }
        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }
}
