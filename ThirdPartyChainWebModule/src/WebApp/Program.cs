using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

var securitySection = builder.Configuration.GetSection("security");

builder.Services
    .AddAuthentication(options =>
    {
        // This is a server-side web app and use cookies to preserve login and session across http requests
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        // Use OpenID with EG Cloud Chain to authenticate user
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    { 
        // Base uri to EG Cloud Chain identity provider as specified by EG
        options.Authority = securitySection["authority"];
        // Client Id as specified by EG
        options.ClientId = securitySection["clientId"];

        // Use OpenID Connect Authorization Code Flow with Proof Key for Code Exchange (PKCE)
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true; // This is the default, but added here to signal its importance.

        // Get permission claims from UserInfoEndpoint
        options.GetClaimsFromUserInfoEndpoint = true;
        // Request tenant scope to include lrstid claim with EG Tenant ID for identity and api access tokens
        options.Scope.Add("tenant");
        
        // ASP.NET OIDC middleware only include a predefined set of claims in the ASP.NET user context. To be able to
        // authorize access based on permissions from Cloud Chain they must be added to list of actions that populate
        // the user context.
        options.ClaimActions.Add(new JsonKeyClaimAction("permission", "string", "permission"));
        
        // (Optional) Use JWT claim name conventions instead of Microsoft names which make looking at claims less confusing
        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "role";
    });

builder.Services
    .AddAuthorization(options =>
    {
        // Create an authorization policy that require an authenticated user with permission claim "CashSettlement.Access"
        options.AddPolicy(
            "CashSettlementUser", 
            policy => policy
                .RequireAuthenticatedUser()
                // EG tenant ID as specified by EG for single-tenant web application
                .RequireClaim("lrstid", securitySection["validEGTenantID"])
                // Permission name as specified in manifest in format <module code>.<permission code>
                // These permissions are not scoped to a specific store. When this becomes a requirement the
                // integration must be updated to request user permissions from an API 
                .RequireClaim("permission", "CashSettlement.Access"));
    });

// Adding server-side page rendering framework
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/SignOut");
});

var app = builder.Build();


// Configure application pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app
    .MapRazorPages()
    // Use the "CashSettlementUser" policy as base authorization for all pages 
    .RequireAuthorization("CashSettlementUser");

app.Run();