using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Models;
using PasswordManager.Services;
using PasswordManager.Utils;
// using System.Web.Mvc;

namespace password_manager.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> logger;
        private readonly IAuthenticationService<UserModel> authService;
        private readonly IHttpContextAccessor ctx;

        public LoginController(ILogger<LoginController> logger, IAuthenticationService<UserModel> authService, IHttpContextAccessor ctx)
        {
            this.logger = logger;
            this.authService = authService;
            this.ctx = ctx;
        }

        public async Task<IActionResult> Index()
        {
            await ctx.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            ctx.HttpContext!.Response.Headers["Cache-Control"] = "no-store";

            ctx.HttpContext.Response.Cookies.Delete(".AspNetCore.Session");
            ctx.HttpContext.Response.Cookies.Delete(".AspNetCore.Antiforgery.2AllGjtG7jM");

            ctx.HttpContext.Session.Clear();

            logger.LogWarning("log in page");

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(UserModel model)
        {
            logger.LogWarning($"trying to log in");

            string? val = await authService.Login(model);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.username!)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var props = new AuthenticationProperties();

            await ctx.HttpContext!.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

            if (ModelState.IsValid && !string.IsNullOrEmpty(val))
            {
                ctx.HttpContext!.Session.SetString(SessionVariables.userId, val);
                return RedirectToAction("Index", "Home", new {val=val});
            }

            TempData["incorrectLogin"] = "Incorrect username or password";

            return RedirectToAction("Index");
        }
    }
}