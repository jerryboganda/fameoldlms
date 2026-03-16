
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace First_Aid_Made_Easy.Controllers
{
    public class ApiBaseController : ApiController
    {
        public string JzUserId { get; set; }
        public string JzUserEmail { get; set; }

        public ApiBaseController()
        {
            if (HttpContext.Current.User.Identity is ClaimsIdentity identity)
            {
                JzUserId = identity.FindFirst(ClaimTypes.Name)?.Value;
                JzUserEmail = identity.FindFirst(ClaimTypes.Email)?.Value;
            }
            else
            {
                JzUserId = null;
                JzUserEmail = null;
            }
        }
    }

}
