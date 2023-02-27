using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PasswordManager.Data;
using PasswordManager.Models;
using PasswordManager.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

/*

TODO: clean up and document code
TODO: add unit tests for password encrypt/decrypt methods
TODO: add authorizations and roles to crud controllers (Home in this case)
TODO: add identity framework to application


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

// JWT token auth set up
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
});

// authentication cookie middleware
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.LoginPath = "/Login";
    options.Cookie.Name = "CookieMadeByLeo";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    // options.AccessDeniedPath = "/AccessDenied";
});

// identity framework setup
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<PasswordDbContext>().AddDefaultTokenProviders();

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

app.UseCookiePolicy();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
