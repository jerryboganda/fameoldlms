using System.Web.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using System.Configuration;

namespace First_Aid_Made_Easy.Filters
{
    public class JzJwtAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var token = request.Headers["Authorization"]?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                filterContext.Result = new HttpStatusCodeResult(401, "Authorization token is missing.");
                return;
            }

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["config:JwtKey"]);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = ConfigurationManager.AppSettings["config:JwtIssuer"],
                    ValidateAudience = true,
                    ValidAudience = ConfigurationManager.AppSettings["config:JwtAudience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                filterContext.HttpContext.User = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch (Exception)
            {
                filterContext.Result = new HttpStatusCodeResult(401, "Invalid or expired token.");
            }
        }
    }

}