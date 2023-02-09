using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using password_manager.Models;
using PasswordManager.Utils;

namespace password_manager.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;
    private readonly IHttpContextAccessor ctx;

    public HomeController(ILogger<HomeController> logger, IHttpContextAccessor ctx)
    {
        this.logger = logger;
        this.ctx = ctx;
    }

    public IActionResult Index(string val)
    {
        // currently the user id is passed in. We will filter the table in the database for all passwords associated with this account, if there are any.

        logger.LogWarning("entering home controller index page");

        if (ctx.HttpContext!.User.Identity!.IsAuthenticated)
        {
            ViewBag.val = val;
            ctx.HttpContext!.Session.SetString(SessionVariables.userId, "this userId is from the session state");
            ctx.HttpContext.Session.SetInt32(SessionVariables.userIdInt, 100);
            return View();
        }

        TempData["sessionExpired"] = "The session has expired. Please Log in again.";

        return RedirectToAction("Index", "Login");
    }

    public async Task LogOut()
    {
        await ctx.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        ctx.HttpContext!.Response.Headers["Cache-Control"] = "no-store";

        ctx.HttpContext.Response.Cookies.Delete(".AspNetCore.Session");
        ctx.HttpContext.Response.Cookies.Delete(".AspNetCore.Antiforgery.2AllGjtG7jM");

        ctx.HttpContext.Session.Clear();
        logger.LogWarning("logging user out");
        ctx.HttpContext!.Response.Redirect("/Login");

        // sign the user out and redirect to landing page
        // return RedirectToAction("Index", "Login");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
