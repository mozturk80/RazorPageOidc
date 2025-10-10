using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


// Enable logging to Debug window
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(o =>
{
    o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    o.Authority = builder.Configuration["OpenIDConnectSettings:Authority"];
    o.ClientId = builder.Configuration["OpenIDConnectSettings:ClientId"];
    o.ClientSecret = builder.Configuration["OpenIDConnectSettings:ClientSecret"];
    o.ResponseType = "code";
    o.SaveTokens = true;

#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
    var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
    var logger = loggerFactory.CreateLogger("OIDC");

    o.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = ctx =>
        {
            logger.LogInformation("Redirecting to Identity Provider: {Url}", ctx.ProtocolMessage.IssuerAddress);
            return Task.CompletedTask;
        },
        OnMessageReceived = ctx =>
        {
            logger.LogInformation("OIDC Message Received: {Message}", ctx.ProtocolMessage);
            return Task.CompletedTask;
        },
        OnTokenValidated = ctx =>
        {
            logger.LogInformation("Token Validated for user: {Name}", ctx.Principal.Identity?.Name);
            var identity = (ClaimsIdentity)ctx.Principal.Identity;
            //add nonce information

            identity.AddClaim(new Claim("department", "Finance"));
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = ctx =>
        {
            logger.LogError(ctx.Exception, "Authentication Failed");
            return Task.CompletedTask;
        },
        OnRemoteFailure = ctx =>
        {
            logger.LogError(ctx.Failure, "Remote Failure during OIDC");
            return Task.CompletedTask;
        },
        OnTicketReceived = ctx =>
        {
            logger.LogInformation("Authentication Ticket Received");
            return Task.CompletedTask;
        },
        OnAuthorizationCodeReceived = ctx =>
        {
            logger.LogInformation("Authorization Code Received");
            return Task.CompletedTask;
        },
        OnTokenResponseReceived = ctx =>
        {
            logger.LogInformation("Token Response Received");
            return Task.CompletedTask;
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
