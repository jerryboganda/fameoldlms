using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Web.Http;
using First_Aid_Made_Easy.App_Start;


[assembly: OwinStartupAttribute(typeof(First_Aid_Made_Easy.Startup))]
namespace First_Aid_Made_Easy
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.UseCookieAuthentication(new CookieAuthenticationOptions { ExpireTimeSpan = TimeSpan.FromHours(24), });
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.MapSignalR();
            // Configure Autofac DI
            AutofacConfig.RegisterDependencies();


            //var myProvider = new AuthorizationServerProvider();
            //OAuthAuthorizationServerOptions options = new OAuthAuthorizationServerOptions
            //{
            //    AllowInsecureHttp = true,
            //    TokenEndpointPath = new PathString("/token"),
            //    AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
            //    Provider = myProvider
            //};
            //app.UseOAuthAuthorizationServer(options);
            //app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
            //HttpConfiguration config = new HttpConfiguration();
            //WebApiConfig.Register(config);
        }
    }
}
