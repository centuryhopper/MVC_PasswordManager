using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using password_manager.Models;
using PasswordManager.Models;
using PasswordManager.Services;
using PasswordManager.Utils;

namespace password_manager.Controllers;


public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;
    private readonly IHttpContextAccessor ctx;
    private readonly IDataAccess<AccountModel> dataAccess;

    public HomeController(ILogger<HomeController> logger, IHttpContextAccessor ctx, IDataAccess<AccountModel> dataAccess)
    {
        this.logger = logger;
        this.ctx = ctx;
        this.dataAccess = dataAccess;
    }

    public async Task<IActionResult> Index(string val)
    {
        // currently the user id is passed in. We will filter the table in the database for all passwords associated with this account, if there are any.
        logger.LogWarning("entering home controller index page");

        if (ctx.HttpContext!.User.Identity!.IsAuthenticated)
        {
            var lst = await dataAccess.Get(val);
            var accountModel = new AccountModel
            {
                userId = val
            };
            var accountViewModel = new AccountViewModel
            {
                accountModels = lst as List<AccountModel>,
                accountModel = accountModel
            };
            ctx.HttpContext!.Session.SetString(SessionVariables.userId, val);
            ctx.HttpContext.Session.SetInt32(SessionVariables.userIdInt, 100);
            return View(accountViewModel);
        }

        TempData["sessionExpired"] = "The session has expired. Please Log in again.";

        return RedirectToAction("Index", "Login");
    }

    [HttpPost]
    public async Task<ActionResult> AddAccount(AccountViewModel accountViewModel)
    {
        try
        {
            await dataAccess.Post(accountViewModel.accountModel, accountViewModel.accountModel.userId!);
            TempData["addedAccount"] = "account has been successfully added";
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            TempData["errorAddingAccount"] = "an error has occurred in adding an account";
        }
        return RedirectToAction("Index", "Home", new {val=accountViewModel.accountModel.userId!});
    }

    // Cannot pass model back up for some reason
    public async Task<ActionResult> Delete(string userId, string accountId)
    {
        // TODO: make into try catch structure (delete)
        var model = await dataAccess.Delete(accountId);
        logger.LogWarning("deleted model with id");
        return RedirectToAction("Index", "Home", new {val=userId});
    }

    public async Task<ActionResult> Edit(string userId, string accountId)
    {
        // TODO: make into try catch structure (update)
        var model = await dataAccess.GetOne(accountId);
        logger.LogWarning(userId);
        return View(model);
    }

    // get by default so the header really isn't needed but shown for clarity
    [HttpGet]
    public async Task<PartialViewResult> FilterAccounts(string filterTerm)
    {
        var userId = ctx.HttpContext!.Session.GetString(SessionVariables.userId)!;
        return PartialView("_AccountsListView", await dataAccess.FilterBy(userId, filterTerm));
    }

    [HttpPost]
    public async Task<ActionResult> Edit(AccountModel model)
    {
        await dataAccess.Put(model);
        return RedirectToAction("Index", "Home", new {val=model.userId});
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
