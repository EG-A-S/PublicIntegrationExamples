using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Web.Helpers;
using System.IdentityModel.Tokens;
using System.Collections.Generic;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security.Cookies;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security;
using System.Security.Claims;

[assembly: OwinStartup(typeof(Sport.Startup))]

namespace Sport
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // todo: replace with serilog
            //LogProvider.SetCurrentLogProvider(new DiagnosticsTraceLogProvider());

            AntiForgeryConfig.UniqueClaimTypeIdentifier = "sub";
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });


            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = "https://intersportdev.inv.no/intersport/identity",

                ClientId = "WebShop",
                ResponseType = "id_token token",
                Scope = "openid email profile member",
                RedirectUri = "https://localhost:44326",
                PostLogoutRedirectUri = "https://localhost:44326",
                //RedirectUri = "https://lrslindbaksporttest.azurewebsites.net",
                //PostLogoutRedirectUri = "https://lrslindbaksporttest.azurewebsites.net",
                SignInAsAuthenticationType = "Cookies",
                UseTokenLifetime = true,

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    SecurityTokenValidated = n =>
                   {
                       // keep the id_token for logout
                       n.AuthenticationTicket.Identity.AddClaim(new Claim("id_token", n.ProtocolMessage.IdToken));

                       // keep access token for api requests
                       n.AuthenticationTicket.Identity.AddClaim(new Claim("access_token", n.ProtocolMessage.AccessToken));

                       return Task.FromResult(0);
                   },

                    RedirectToIdentityProvider = n =>
                    {
                        // Pass id_token for safe logout
                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                        {
                            var idTokenHint = n.OwinContext.Authentication.User.FindFirst("id_token");

                            if (idTokenHint != null)
                            {
                                n.ProtocolMessage.IdTokenHint = idTokenHint.Value;
                            }
                        }

                        return Task.FromResult(0);
                    }
                }
            });
        }
    }
}
