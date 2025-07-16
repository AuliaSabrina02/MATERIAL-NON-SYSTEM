using SEMB_ERP.Function;
using SEMB_ERP.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Json;
using SEMB_ERP.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var configuration = builder.Configuration;

string connectionString = "Data Source=10.155.152.114;Initial Catalog=SEMB_ERP;Persist Security Info=True;User ID=dt;Password=Dt@123;MultipleActiveResultSets=true";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<ITokenService, TokenService>();

// OPENID CONNECT OAUTH2
builder.Services.AddDataProtection()
    .SetApplicationName("sso")
    .PersistKeysToFileSystem(new DirectoryInfo(configuration["Auth:KeyStorage"]));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(
    options =>
    {
        options.AccessDeniedPath = "/AccessDenied"; // All 403s go here
        //options.LoginPath = "/Account/Login"; // Specify the login page path
        //options.AccessDeniedPath = "/Account/AccessDenied"; // Optional: Specify the access denied page
        options.Cookie.Name = "ping";
        options.Cookie.Path = "/"; // Make cookie accessible for all paths
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Use Always in production
        options.Cookie.HttpOnly = true; // Prevent JavaScript access
        options.SlidingExpiration = true; // Optional: enable sliding expiration
    }
)
.AddOpenIdConnect(options =>
{
    options.ClientId = configuration["Auth:ClientId"];
    options.ClientSecret = configuration["Auth:ClientSecret"];
    options.Authority = configuration["Auth:Authority"];
    options.ResponseType = "code";
    options.Scope.Add("openid");
    options.Scope.Add("profile");

    // Callback URL after authentication
    options.CallbackPath = new PathString(configuration["Auth:CallbackPath"]);

    // Configure what to do upon receiving tokens
    options.SaveTokens = true;

    var auth = new Authentication();
    var codeVerifier = auth.GenerateCodeVerifier();
    var codeChallenge = auth.GenerateCodeChallenge(codeVerifier);
    // Event Handling
    options.Events = new OpenIdConnectEvents
    {
        OnTokenValidated = context =>
        {
            return Task.CompletedTask;
        },
        OnTicketReceived = context =>
        {
            var principal = context.Principal;
            return Task.CompletedTask;
        },
        OnRedirectToIdentityProvider = context =>
        {
            //if (context.Response.StatusCode == StatusCodes.Status401Unauthorized || !context.HttpContext.User.Identity.IsAuthenticated)
            //{
            //    // Redirect to the custom login page instead of Ping SSO
            //    context.Response.Redirect("/Account/Login");
            //    context.HandleResponse(); // Prevent the default redirection to Ping SSO
            //    return Task.CompletedTask;
            //}
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized || (!context.HttpContext.User.Identity.IsAuthenticated && context.HttpContext.Request.Path != "/Home/Login"))
            {
                var actionContext = new ActionContext(
                    context.HttpContext,
                    new RouteData(),
                    new ActionDescriptor()
                );

                // Use the UrlHelperFactory to create a UrlHelper
                var urlHelperFactory = context.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
                var urlHelper = urlHelperFactory.GetUrlHelper(actionContext);

                // Generate the URL to the Login action in the Account controller
                var redirectToUrl = urlHelper.Action("Index", "Home");

                context.Response.Redirect(redirectToUrl);
                context.HandleResponse(); // Prevent the default redirection to Ping SSO
                return Task.CompletedTask;
            }
            var request = context.HttpContext.Request;
            var scheme = request.Scheme; // HTTP or HTTPS
            var host = request.Host.Value; // domain and port
            var pathBase = request.PathBase; // domain and port
            var path = request.Path; // domain and port
            var redirect_url = $"{scheme}://{host}{pathBase}/api/auth/Index?originalPath={pathBase}{path}";
            if (env.IsProduction())
            {
                // Set PKCE parameters in the request
                context.ProtocolMessage.SetParameter("code_challenge", codeChallenge);
                context.ProtocolMessage.SetParameter("code_challenge_method", "S256");

                // Store the code_verifier in the properties for later use during the token request
                //context.Properties.SetParameter("code_verifier", codeVerifier);
                context.HttpContext.Response.Cookies.Append("code_verifier", codeVerifier, new CookieOptions
                {
                    HttpOnly = true, // Optional, improve security
                    Secure = true, // Ensure cookie is sent over HTTPS
                    SameSite = SameSiteMode.None // Adjust as necessary for your application
                });

                context.HttpContext.Response.Cookies.Append("redirect_url", redirect_url, new CookieOptions
                {
                    HttpOnly = true, // Optional, improve security
                    Secure = true, // Ensure cookie is sent over HTTPS
                    SameSite = SameSiteMode.None // Adjust as necessary for your application
                });
            }
            else
            {
                context.ProtocolMessage.State = $"returnUrl=={redirect_url}";
            }

            context.ProtocolMessage.RedirectUri = configuration["Auth:RedirectURI"];
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = async context =>
        {
            // Check if the token is expired
            if (context.Response.StatusCode == 401)
            {
                var refreshToken = context.HttpContext.Request.Cookies["refresh_token"];

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var tokenService = context.HttpContext.RequestServices.GetService<ITokenService>();
                    // Attempt to refresh the access token using the refresh token
                    var isTokenRefreshed = await tokenService.RefreshAccessToken(refreshToken, context.HttpContext);

                    if (isTokenRefreshed)
                    {
                        // Optionally re-execute the request or redirect
                        context.HandleResponse(); // Prevent the default 401 handling
                        context.Response.Redirect(context.Request.Path); // Redirect to the original request path
                    }
                    else
                    {
                        // Handle refresh token failure (e.g., redirect to login or show an error)
                        context.Response.Redirect("/Account/Login/ABC"); // Redirect to login page
                    }
                }
                else
                {
                    // No refresh token available, redirect to login
                    context.Response.Redirect("/Account/Login/CDA");
                }
            }
            //return Task.CompletedTask;
        }
    };
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireRequestor", policy => policy.RequireClaim("semb_erp_role", "requestor"));
    options.AddPolicy("RequireReceiver", policy => policy.RequireClaim("semb_erp_role", "receiver"));
    options.AddPolicy("RequirePlantreceiver", policy => policy.RequireClaim("semb_erp_role", "plant_receiver"));
    options.AddPolicy("RequireAdmin", policy => policy.RequireClaim("semb_erp_role", "admin"));

    options.AddPolicy("RequireRequestorReceiver", policy => policy.RequireClaim("semb_erp_role", "requestor", "receiver"));
    options.AddPolicy("RequireRequestorReceiverPlantreceiver", policy => policy.RequireClaim("semb_erp_role", "requestor", "receiver", "plant_receiver"));
    options.AddPolicy("RequireRequestorReceiverAdmin", policy => policy.RequireClaim("semb_erp_role", "requestor", "receiver", "admin"));
    options.AddPolicy("RequireRequestorPlantreceiver", policy => policy.RequireClaim("semb_erp_role", "requestor", "plant_receiver"));
    options.AddPolicy("RequireRequestorPlantreceiverAdmin", policy => policy.RequireClaim("semb_erp_role", "requestor", "plant_receiver", "admin"));
    options.AddPolicy("RequireRequestorAdmin", policy => policy.RequireClaim("semb_erp_role", "requestor", "admin"));

    options.AddPolicy("RequireReceiverPlantreceiver", policy => policy.RequireClaim("semb_erp_role", "receiver", "plant_receiver"));
    options.AddPolicy("RequireReceiverPlantreceiverAdmin", policy => policy.RequireClaim("semb_erp_role", "receiver", "plant_receiver", "admin"));
    options.AddPolicy("RequireReceiverAdmin", policy => policy.RequireClaim("semb_erp_role", "receiver", "admin"));

    options.AddPolicy("RequirePlantreceiverAdmin", policy => policy.RequireClaim("semb_erp_role", "plant_receiver", "admin"));

    //options.AddPolicy("RequireRequestorReceiver", policy =>
    //    policy.RequireClaim("semb_erp_role", "requestor", "receiver"));
    //options.AddPolicy("RequireRequestorPlantreceiver", policy =>
    //    policy.RequireClaim("semb_erp_role", "requestor", "plant_receiver"));

    options.AddPolicy("RequireAny", policy =>
        policy.RequireClaim("semb_erp_role", "requestor", "receiver","plant_receiver","admin"));
});
// END OPENID CONNECT

builder.Services.AddMvc();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ImportExportFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); //new
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "callback",
    pattern: "callback",
    defaults: new { controller = "Callback", action = "HandleCallback" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
