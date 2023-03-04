using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PasswordManager.Data;
using PasswordManager.Models;
using PasswordManager.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using PasswordManager.Utils;

/*
TODO: clean up and document code

TODO: add unit tests for password encrypt/decrypt methods

TODO: add multi-factor auth

*/


// parse the elephantSQL provided string into ASP.net core friendly connection string
string getConnectionString(WebApplicationBuilder builder)
{
    // ElephantSQL formatting
    var optionsBuilder = new DbContextOptionsBuilder<PasswordDbContext>();
    // ElephantSQL formatting
    var uriString = builder.Configuration.GetConnectionString("cloudConnectionString")!;
    var uri = new Uri(uriString);
    var db = uri.AbsolutePath.Trim('/');
    var user = uri.UserInfo.Split(':')[0];
    var passwd = uri.UserInfo.Split(':')[1];
    var port = uri.Port > 0 ? uri.Port : 5432;
    var connStr = string.Format("Server={0};Database={1};User Id={2};Password={3};Port={4}",
        uri.Host, db, user, passwd, port);
    return connStr;
}

var builder = WebApplication.CreateBuilder(args);

// Dependency injection
builder.Services.AddScoped<IDataAccess<AccountModel>, EFService>();

// this singleton is meant to be used in non-controller classes that have a DI for the httpcontext accessor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddTransient<IEmailSender, EmailSender>();


builder.Services.AddDistributedMemoryCache();

// determine how long a user can idle in the application before timing out
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(120);
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// allow client-side apps to fetch data from this api
builder.Services.AddCors(p => p.AddPolicy(name: "client_policy", build =>
{
    build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

#region JWT token auth set up
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
    options =>
{
    var secretKey = builder.Configuration.GetSection("AppSettings:Token").Value!;
    var issuer = builder.Configuration.GetSection("AppSettings:Issuer").Value!;
    var audience = builder.Configuration.GetSection("AppSettings:Audience").Value!;
    // options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(secretKey)),
    };
    options.Events.OnMessageReceived = (context) => {

        if (context.Request.Cookies.ContainsKey(Constants.X_ACCESS_TOKEN))
        {
            context.Token = context.Request.Cookies[Constants.X_ACCESS_TOKEN];
        }

        return Task.CompletedTask;
    };
});
#endregion


#region authentication cookie middleware
// builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
// .AddCookie(options =>
// {
//     options.Cookie.SameSite = SameSiteMode.Strict;
//     options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
//     options.Cookie.HttpOnly = true;
//     // options.LoginPath = "/Account/Login";
//     // options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
//     // options.Cookie.Name = "CookieMadeByLeo";
//     // options.AccessDeniedPath = "/AccessDenied";
// });
#endregion

// builder.Services.AddAuthorization(options =>
// {
//     options.FallbackPolicy = new AuthorizationPolicyBuilder()
//         .RequireAuthenticatedUser()
//         .Build();
// });

// identity framework setup
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // must confirm email
    options.SignIn.RequireConfirmedEmail = true;

    // keep it stupid simple JUST for now
    options.Password.RequiredLength = 1;
    options.Password.RequiredUniqueChars = 1;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;

}).AddEntityFrameworkStores<PasswordDbContext>().AddDefaultTokenProviders();

// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultScheme = IdentityConstants.ApplicationScheme;
//     options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
// })
// .AddIdentityCookies();

// link to postgreSQL db for entity framework
builder.Services.AddDbContextPool<PasswordDbContext>(
    options =>
    {
        var connStr = getConnectionString(builder);
        options.UseNpgsql(
                connStr
        ).EnableSensitiveDataLogging();
    }
);

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
app.UseSession();

app.UseRouting();

app.UseCors(
    "client_policy"
);

app.UseAuthentication();
app.UseAuthorization();

// redirect the authorized access page to this route
// default in asp.net core is /Account/AccessDenied
app.UseStatusCodePagesWithRedirects("/Home/AccessDenied?code={0}");

// app.Map("/AccessDenied", builder =>
// {
//     builder.Run(async context =>
//     {
//         context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
//         await context.Response.WriteAsync("Access Denied :/");
//     });
// });

app.UseCookiePolicy();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
