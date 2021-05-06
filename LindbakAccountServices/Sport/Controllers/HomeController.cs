using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace Sport.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private const string baseurl = "https://sport.inv.no/sport";

        public ActionResult Index()
        {
            return View((User as ClaimsPrincipal).Claims);
        }

        public ActionResult ChangePassword()
        {
            var userId = (User.Identity as ClaimsIdentity).FindFirst("sub")?.Value;

            return Redirect($"{baseurl}/identity/reset?userId={userId}&redirectUrl={HttpUtility.UrlEncode(CurrentUrl)}");

        }

        public ActionResult EditProfile()
        {
            return Redirect($"{baseurl}/profile/?redirectUrl={HttpUtility.UrlEncode(CurrentUrl)}");
        }

        public string CurrentUrl => string.Format("{0}://{1}:{2}", Request.Url.Scheme, Request.Url.Host, Request.Url.Port);

        public ActionResult Logout()
        {
            Request.GetOwinContext()
                   .Authentication
                   .SignOut(HttpContext.GetOwinContext()
                           .Authentication.GetAuthenticationTypes()
                           .Select(o => o.AuthenticationType).ToArray());

            return Redirect("/");
        }
    }
}
