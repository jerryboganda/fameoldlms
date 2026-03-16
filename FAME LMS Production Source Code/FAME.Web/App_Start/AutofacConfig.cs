using Autofac;
using First_Aid_Made_Easy.BLL;
using First_Aid_Made_Easy.BLL.Interfaces;
using First_Aid_Made_Easy.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Http;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System.Web;
using First_Aid_Made_Easy.Models;
using Autofac.Integration.Mvc;

using Serilog;

namespace First_Aid_Made_Easy.App_Start
{
    public class AutofacConfig
    {
        public static IContainer RegisterDependencies()
        {
            var builder = new ContainerBuilder();

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/Logs/log.txt"),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 5242880,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

            // Register Serilog ILogger
            builder.Register(c => Log.Logger).As<ILogger>().SingleInstance();

            // Register Repositories
            builder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerRequest();
            builder.RegisterType<CourseRepository>().As<ICourseRepository>().InstancePerRequest();
            builder.RegisterType<EnrollmentRepository>().As<IEnrollmentRepository>().InstancePerRequest();
            builder.RegisterType<SectionRepository>().As<ISectionRepository>().InstancePerRequest();
            builder.RegisterType<SectionSubRepository>().As<ISectionSubRepository>().InstancePerRequest();
            builder.RegisterType<VideoRepository>().As<IVideoRepository>().InstancePerRequest();
            builder.RegisterType<TutorialRepository>().As<ITutorialRepository>().InstancePerRequest();
            builder.RegisterType<BookMistakesRepository>().As<IBookMistakesRepository>().InstancePerRequest();
            builder.RegisterType<QuestionRepository>().As<IQuestionRepository>().InstancePerRequest();
            builder.RegisterType<DashBoardRepository>().As<IDashBoardRepository>().InstancePerRequest();
            builder.RegisterType<PackageRepository>().As<IPackageRepository>().InstancePerRequest();
            builder.RegisterType<ChatRepository>().As<IChatRepository>().InstancePerRequest();
            builder.RegisterType<GeneralRepository>().As<IGeneralRepository>().InstancePerRequest();
            builder.RegisterType<InstallmentRepository>().As<IInstallmentRepository>().InstancePerRequest();
            builder.RegisterType<ZoomApiRepository>().As<IZoomApiRepository>().InstancePerRequest();
            builder.RegisterType<NotificationRepository>().As<INotificationRepository>().InstancePerRequest();
            builder.RegisterType<TicketRepository>().As<ITicketRepository>().InstancePerRequest();
            builder.RegisterType<CouponRepository>().As<ICouponRepository>().InstancePerRequest();
            builder.RegisterType<ExamRepository>().As<IExamRepository>().InstancePerRequest();
            builder.RegisterType<QuestionPaperRepository>().As<IQuestionPaperRepository>().InstancePerRequest();
            builder.RegisterType<QuestionSystemsRepository>().As<IQuestionSystemsRepository>().InstancePerRequest();
            builder.RegisterType<SettingRepository>().As<ISettingRepository>().InstancePerRequest();
            builder.RegisterType<QuestionPartitionRepository>().As<IQuestionPartitionRepository>().InstancePerRequest();
            builder.RegisterType<FavouritesRepository>().As<IFavouritesRepository>().InstancePerRequest();
            builder.RegisterType<ReportRepository>().As<IReportRepository>().InstancePerRequest();
            builder.RegisterType<StudentGroupRepository>().As<IStudentGroupRepository>().InstancePerRequest();
            builder.RegisterType<BlogPostRepository>().As<IBlogPostRepository>().InstancePerRequest();
            builder.RegisterType<DailyReportAuthRepository>().As<IDailyReportAuthRepository>().InstancePerRequest();
            builder.RegisterType<DailyReportRepository>().As<IDailyReportRepository>().InstancePerRequest();
            builder.RegisterType<FileItemRepository>().As<IFileItemRepository>().InstancePerRequest();
            builder.RegisterType<ConfigurationService>().As<IConfigurationService>().InstancePerRequest();
            builder.RegisterType<EnrollmentService>().As<IEnrollmentService>().InstancePerRequest();

            // Register Website Package Management
            builder.RegisterType<WebsitePackageRepository>().As<IWebsitePackageRepository>().InstancePerRequest();

            // Register Ambassador Services
            builder.RegisterType<AmbassadorRepository>().As<IAmbassadorRepository>().InstancePerRequest();
            builder.RegisterType<ReferralRepository>().As<IReferralRepository>().InstancePerRequest();
            builder.RegisterType<CommissionService>().As<ICommissionService>().InstancePerRequest();
            builder.RegisterType<PayoutService>().As<IPayoutService>().InstancePerRequest();
            builder.RegisterType<AmbassadorAuditService>().As<IAmbassadorAuditService>().InstancePerRequest();

            // Register Certificate Services
            builder.RegisterType<CertificateAuditService>().As<ICertificateAuditService>().InstancePerRequest();
            builder.RegisterType<CertificateTemplateService>().As<ICertificateTemplateService>().InstancePerRequest();
            builder.RegisterType<CertificateRenderService>().As<ICertificateRenderService>().InstancePerRequest();
            builder.RegisterType<CertificateIssuanceService>().As<ICertificateIssuanceService>().InstancePerRequest();
            builder.RegisterType<CertificateRuleService>().As<ICertificateRuleService>().InstancePerRequest();
            builder.RegisterType<CertificateVerificationService>().As<ICertificateVerificationService>().InstancePerRequest();
            builder.RegisterType<CertificateCorrectionService>().As<ICertificateCorrectionService>().InstancePerRequest();
            builder.RegisterType<ManualCertificateService>().As<IManualCertificateService>().InstancePerRequest();

            // Register Email Marketing Services
            builder.RegisterType<EmailMarketingRepository>().As<IEmailMarketingRepository>().InstancePerRequest();

            // Register Identity Managers
            builder.Register(c => HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>()).InstancePerRequest();
            builder.Register(c => HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>()).InstancePerRequest();

            // Register All Controllers in the current assembly (MVC)
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            // Register Web API Controllers (using reflection to get all ApiControllers)
            builder.RegisterAssemblyTypes(typeof(MvcApplication).Assembly)
                .Where(t => typeof(ApiController).IsAssignableFrom(t) && t.Name.EndsWith("Controller"))
                .InstancePerRequest();
            
            // Register Autofac MVC Module
            builder.RegisterModule<AutofacWebTypesModule>();

            // Build the container
            var container = builder.Build();

            // Set the MVC Dependency Resolver using the official Autofac Integration
            DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(container));

            // Set the Web API Dependency Resolver
            GlobalConfiguration.Configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            return container;
        }
    }

    public class AutofacWebApiDependencyResolver : System.Web.Http.Dependencies.IDependencyResolver
    {
        private readonly ILifetimeScope _container;

        public AutofacWebApiDependencyResolver(ILifetimeScope container)
        {
            _container = container;
        }

        public System.Web.Http.Dependencies.IDependencyScope BeginScope()
        {
            return new AutofacWebApiDependencyScope(_container.BeginLifetimeScope("AutofacWebRequest"));
        }

        public object GetService(Type serviceType)
        {
            return _container.ResolveOptional(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            Type enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            if (_container.TryResolve(enumerableType, out object result))
            {
                return (IEnumerable<object>)result;
            }
            return Enumerable.Empty<object>();
        }

        public void Dispose()
        {
        }
    }

    public class AutofacWebApiDependencyScope : System.Web.Http.Dependencies.IDependencyScope
    {
        private readonly ILifetimeScope _scope;

        public AutofacWebApiDependencyScope(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public object GetService(Type serviceType)
        {
            return _scope.ResolveOptional(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            Type enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            if (_scope.TryResolve(enumerableType, out object result))
            {
                return (IEnumerable<object>)result;
            }
            return Enumerable.Empty<object>();
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
