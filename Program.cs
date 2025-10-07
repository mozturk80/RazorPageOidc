using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(o =>
{
    // ........................................................................
    // The OIDC handler must use a sign-in scheme capable of persisting 
    // user credentials across requests.

    o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // ........................................................................

    // ........................................................................
    // The "openid" and "profile" scopes are required for the OIDC handler 
    // and included by default. 

    //options.Scope.Add("some-scope");
    // ........................................................................

    // ........................................................................
    // The following paths must match the redirect and post logout redirect 
    // paths configured when registering the application with the OIDC provider. 
    // Both the signin and signout paths must be registered as Redirect URIs.
    // The default values are "/signin-oidc" and "/signout-callback-oidc".

    //options.CallbackPath = new PathString("/signin-oidc");
    //options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
    // ........................................................................

    // ........................................................................
    // The RemoteSignOutPath is the "Front-channel logout URL" for remote single 
    // sign-out. The default value is "/signout-oidc".

    //options.RemoteSignOutPath = new PathString("/signout-oidc");
    // ........................................................................

    o.Authority = builder.Configuration["OpenIDConnectSettings:Authority"];
    o.ClientId = builder.Configuration["OpenIDConnectSettings:ClientId"];
    o.ClientSecret = builder.Configuration["OpenIDConnectSettings:ClientSecret"];
    o.ResponseType = "code";
    o.SaveTokens = true;

    // Map incoming JSON keys (adjust to your provider)
    // o.ClaimActions.MapJsonKey("department", "department");

    o.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = async ctx =>
        {
            var identity = (ClaimsIdentity)ctx.Principal!.Identity!;
            identity.AddClaim(new Claim("department", "Finance"));
           
        }
    };

    o.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name",
        RoleClaimType = "role"
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("FinanceOnly", p =>
        p.RequireClaim("department", "Finance"));
});

builder.Services.AddRazorPages();

var app = builder.Build();
//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
