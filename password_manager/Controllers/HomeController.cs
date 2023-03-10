using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using password_manager.Models;
using password_manager.Services;
using password_manager.Utils;
using System.Text;

namespace password_manager.Controllers;

// currently using jwt token based authentication
public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;
    private readonly IDataAccess<AccountModel> dataAccess;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly IConfiguration configuration;

    public HomeController(ILogger<HomeController> logger, IDataAccess<AccountModel> dataAccess, UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
    {
        this.logger = logger;
        this.dataAccess = dataAccess;
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.configuration = configuration;
    }

    private async Task Refresh()
    {
        if (!(Request.Cookies.TryGetValue(Constants.X_USERNAME, out var _) && Request.Cookies.TryGetValue(Constants.X_REFRESH_TOKEN, out var oldRefreshToken)))
        {
            logger.LogError($"Error. Couldn't generate refresh token");
            return;
        }

        string currentToken = Request.Cookies[Constants.X_ACCESS_TOKEN]!;

        var currentTokenExpiration = DateTime.Parse(TokenManager.ValidateToken(currentToken, configuration.GetSection("AppSettings:Token").Value!, ClaimTypes.Expiration)!);

        if (DateTime.Compare(DateTime.Now, currentTokenExpiration) < 0)
        {
            logger.LogWarning("Token has not expired yet");
            return;
        }

        // var user = await userManager.Users.FirstOrDefaultAsync(i => i.UserName == userName && i.RefreshToken == refreshToken);

        // if (user == null)
        //     return BadRequest();

        var user = (await userManager.FindByIdAsync(HttpContext.Session.GetString(Constants.userId)!))!;

        var (token, refreshToken, dateCreated, expires) = TokenManager.createJwtToken(user, configuration.GetSection("AppSettings:Token").Value!);

        // user.RefreshToken = Guid.NewGuid().ToString();

        // await _userManager.UpdateAsync(user);

        // Overwrite old cookie values
        Response.Cookies.Append(Constants.X_ACCESS_TOKEN, token, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict });
        // Response.Cookies.Append(Constants.X_USERNAME, user.UserName!, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict });
        Response.Cookies.Append(Constants.X_REFRESH_TOKEN, refreshToken, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [Authorize(Roles=$"{Constants.ADMIN},{Constants.USER},{Constants.AUDITOR},{Constants.MANAGER}")]
    public async Task<IActionResult> Index(string returnUrl)
    {
        if (!signInManager.IsSignedIn(User))
        {
            return RedirectToAction(nameof(AccountController.Login), "Account", new {returnUrl = returnUrl});
        }
        // var cookieSize = Encoding.UTF8.GetByteCount("hTy%2BC4dZSUKhUCCANpRm5rmUimoDbXGz9rjRVTJqT0E%3D");

        await Refresh();
        // var isAuthenticated = HttpContext.User.Identity!.IsAuthenticated;
        // var name = HttpContext.User.Identity.Name;
        // var authType = HttpContext.User.Identity.AuthenticationType;

        // logger.LogWarning($"entering home controller index page with {token}");

        var userId = HttpContext.Session.GetString(Constants.userId);

        return View(new AccountModel());

        // TempData[Constants.SESSION_EXPIRED] = "The session has expired. Please Log in again.";

        // return RedirectToAction("Login", "Account");
    }

    [HttpPost]
    public async Task<ActionResult> AddAccount(AccountModel model)
    {
        await Refresh();

        try
        {
            await dataAccess.Post(model, HttpContext.Session.GetString(Constants.userId)!);
            TempData[Constants.ADDED_ACCOUNT] = "account has been successfully added";
        }
        catch (Exception e)
        {
            logger.LogError(e.Message);
            TempData[Constants.ERROR_ADD_ACCOUNT] = "an error has occurred in adding an account";
        }
        return RedirectToAction("Index", "Home");
    }

    // Cannot pass model back up for some reason
    public async Task<ActionResult> Delete(string userId, string accountId)
    {
        await Refresh();

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
        if (HttpContext.User.Identity!.IsAuthenticated)
        {
            logger.LogWarning($"editing user password account: {accountId}");
            await Refresh();
            var model = await dataAccess.GetOne(accountId);
            return View(new EditViewModel { accountModel = model!, editIdx = idx });
        }

        TempData[Constants.SESSION_EXPIRED] = "The session has expired. Please Log in again.";

        return RedirectToAction("Login", "Account");
    }

    [HttpPost]
    public async Task<ActionResult> Edit(EditViewModel model)
    {
        logger.LogWarning("Finished edit");

        if
        (
            string.IsNullOrEmpty(model.accountModel.title) ||
            string.IsNullOrEmpty(model.accountModel.username) ||
            string.IsNullOrEmpty(model.accountModel.password) ||
            string.IsNullOrWhiteSpace(model.accountModel.title) ||
            string.IsNullOrWhiteSpace(model.accountModel.username) ||
            string.IsNullOrWhiteSpace(model.accountModel.password)
        )
        {
            logger.LogWarning("all fields should not be empty");
            TempData[Constants.ERROR_EDIT_ACCOUNT] = "Please make sure you have entered all the proper fields when editing your password account.";
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
            string.IsNullOrEmpty(model.title) ||
            string.IsNullOrEmpty(model.username) ||
            string.IsNullOrEmpty(model.password) ||
            string.IsNullOrWhiteSpace(model.title) ||
            string.IsNullOrWhiteSpace(model.username) ||
            string.IsNullOrWhiteSpace(model.password)
        )
        {
            logger.LogWarning("all fields should not be empty");
            TempData[Constants.ERROR_EDIT_ACCOUNT] = "Please make sure you have entered all the proper fields when editing your password account.";
            return BadRequest();
        }

        await dataAccess.Put(model);
        return Ok();
    }

    // get by default so the header really isn't needed but shown for clarity
    [HttpGet]
    public async Task<PartialViewResult> _AccountsListView(string filterTerm)
    {
        if (filterTerm is null)
            filterTerm = "";
        var userId = HttpContext.Session.GetString(Constants.userId);
        var accountModels = await dataAccess.FilterBy(userId!, filterTerm) as List<AccountModel>;
        return PartialView("_AccountsListView", new AccountListViewModel { accountModels = accountModels!, filterTerm = filterTerm });
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
