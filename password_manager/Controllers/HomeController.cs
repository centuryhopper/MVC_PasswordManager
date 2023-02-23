using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PasswordManager.Models;
using PasswordManager.Services;
using PasswordManager.Utils;

namespace password_manager.Controllers;


// currently using cookie based authentication
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

    public IActionResult Index()
    {
        // currently the user id is passed in. We will filter the table in the database for all passwords associated with this account, if there are any.
        logger.LogWarning("entering home controller index page");

        logger.LogWarning($"home index ctx user id: {ctx.HttpContext!.Session.GetString(SessionVariables.userId)}");

        if (ctx.HttpContext!.User.Identity!.IsAuthenticated)
        {
            return View(new AccountModel());
        }

        TempData["sessionExpired"] = "The session has expired. Please Log in again.";

        return RedirectToAction("Login", "Account");
    }

    [HttpPost]
    public async Task<ActionResult> AddAccount(AccountModel model)
    {
        try
        {
            await dataAccess.Post(model, ctx.HttpContext!.Session.GetString(SessionVariables.userId)!);
            TempData["addedAccount"] = "account has been successfully added";
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            TempData["errorAddingAccount"] = "an error has occurred in adding an account";
        }
        return RedirectToAction("Index", "Home");
    }

    // Cannot pass model back up for some reason
    public async Task<ActionResult> Delete(string userId, string accountId)
    {
        try
        {
            var model = await dataAccess.Delete(accountId);
            logger.LogWarning("deleted model with id");
        }
        catch (Exception e)
        {
            logger.LogError($"encountered an issue with deleting: {e.Message}");
        }

        return RedirectToAction("Index", "Home");

    }

    public async Task<IActionResult> Edit(string accountId, int idx)
    {
        if (ctx.HttpContext!.User.Identity!.IsAuthenticated)
        {
            logger.LogWarning($"editing user password account: {accountId}");
            var model = await dataAccess.GetOne(accountId);
            return View(new EditViewModel {accountModel = model!, editIdx = idx});
        }

        TempData["sessionExpired"] = "The session has expired. Please Log in again.";

        return RedirectToAction("Login", "Account");
    }

    [HttpPost]
    public async Task<ActionResult> Edit(EditViewModel model)
    {
        logger.LogWarning("Finished edit");

        if
        (
            string.IsNullOrEmpty(model.accountModel.title)         ||
            string.IsNullOrEmpty(model.accountModel.username)      ||
            string.IsNullOrEmpty(model.accountModel.password)      ||
            string.IsNullOrWhiteSpace(model.accountModel.title)    ||
            string.IsNullOrWhiteSpace(model.accountModel.username) ||
            string.IsNullOrWhiteSpace(model.accountModel.password)
        )
        {
            logger.LogWarning("all fields should not be empty");
            TempData["editError"] = "Please make sure you have entered all the proper fields when editing your password account.";
            return RedirectToAction("Index", "Home");
        }

        await dataAccess.Put(model.accountModel);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Autosave(string autoSaveModel)
    {

        AccountModel model = JsonConvert.DeserializeObject<AccountModel>(autoSaveModel)!;
        // logger.LogWarning($"autosaving {model}");

        if
        (
            string.IsNullOrEmpty(model.title)         ||
            string.IsNullOrEmpty(model.username)      ||
            string.IsNullOrEmpty(model.password)      ||
            string.IsNullOrWhiteSpace(model.title)    ||
            string.IsNullOrWhiteSpace(model.username) ||
            string.IsNullOrWhiteSpace(model.password)
        )
        {
            logger.LogWarning("all fields should not be empty");
            TempData["editError"] = "Please make sure you have entered all the proper fields when editing your password account.";
            return BadRequest();
        }

        await dataAccess.Put(model);
        return Ok();
    }

    // get by default so the header really isn't needed but shown for clarity
    [HttpGet]
    public async Task<PartialViewResult> FilterAccounts(string filterTerm)
    {
        var userId = ctx.HttpContext!.Session.GetString(SessionVariables.userId)!;
        return PartialView("_AccountsListView", new AccountListViewModel { accountModels = await dataAccess.FilterBy(userId, filterTerm) as List<AccountModel>, filterTerm = filterTerm });
    }

    public async Task LogOut()
    {
        await ctx.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        ctx.HttpContext!.Response.Headers["Cache-Control"] = "no-store";
        ctx.HttpContext.Response.Cookies.Delete(".AspNetCore.Session");
        ctx.HttpContext.Response.Cookies.Delete(".AspNetCore.Antiforgery.2AllGjtG7jM");
        ctx.HttpContext.Session.Clear();
        logger.LogWarning("logging user out");
        ctx.HttpContext!.Response.Redirect(Url.Action("Login", "Account")!);

        // sign the user out and redirect to landing page
        // return RedirectToAction("Login", "Account");
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
