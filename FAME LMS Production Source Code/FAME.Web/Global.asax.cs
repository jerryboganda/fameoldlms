using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.JzTimer;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;


namespace First_Aid_Made_Easy
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private EmailSenderService _emailSenderService;
        private NotificationSenderService _notifSenderService;
        private EmailCampaignSchedulerService _campaignSchedulerService;
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            //GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => new MyIdProvider());
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //new Aspose.Words.License().SetLicense("C:\\Users\\Jahanzaib\\.nuget\\packages\\aspose.words\\22.9.0\\Aspose.Total.lic");
            _emailSenderService = new EmailSenderService();
            _emailSenderService.Start();
            _notifSenderService = new NotificationSenderService();
            _notifSenderService.Start();
            _campaignSchedulerService = new EmailCampaignSchedulerService();
            _campaignSchedulerService.Start();
        }

        protected void Application_BeginRequest()
        {
            if (MaintenanceModeHelper.ShouldHandleRequest(HttpContext.Current))
            {
                MaintenanceModeHelper.WriteMaintenanceResponse(HttpContext.Current);
            }
        }

        protected void Application_End()
        {
            _emailSenderService?.Stop();
            _notifSenderService?.Stop();
            _campaignSchedulerService?.Stop();
        }
    }
}
