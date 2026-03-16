using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using First_Aid_Made_Easy.Controllers;
using First_Aid_Made_Easy.Models;

namespace First_Aid_Made_Easy.BLL
{
    public class AuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            context.Validated();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            ApplicationUser user = new ApplicationUser();
            // Note: AuthController requires DI parameters. Legacy OAuth validation should be reviewed.
            // using (AuthController _repo = new AuthController())
            // {
            //     var a = new { context.UserName, context.Password };
            // 
            //     if (user == null)
            //     {
            //         context.SetError("invalid_grant", "The user name or password is incorrect.");
            //         return;
            //     }
            // }

            var identity = new ClaimsIdentity(context.Options.AuthenticationType);
            identity.AddClaim(new Claim("id", user.Id));

            context.Validated(identity);
        }
    }
}




