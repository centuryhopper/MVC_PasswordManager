using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PasswordManager.Data;
using PasswordManager.Models;
using PasswordManager.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Authentication.Cookies;

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

builder.Services.AddScoped<IDataAccess<AccountModel>, EFService>();
builder.Services.AddScoped<IAuthenticationService<UserModel>, AuthenticationService>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddDistributedMemoryCache();

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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
    options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.LoginPath = "/Login";
    options.Cookie.Name = "CookieMadeByLeo";
});

// builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<PasswordDbContext>();

builder.Services.AddDbContextPool<PasswordDbContext>(
    options =>
    {
        var connStr = getConnectionString(builder);
        options.UseNpgsql(
                connStr
        );
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
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
