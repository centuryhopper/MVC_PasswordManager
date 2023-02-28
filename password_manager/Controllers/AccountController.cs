using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Models;
using PasswordManager.Utils;

namespace PasswordManager.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> logger;
    private readonly IConfiguration configuration;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly RoleManager<IdentityRole> roleManager;

    public AccountController(ILogger<AccountController> logger, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.roleManager = roleManager;
    }

    public IActionResult Login()
    {
        // logger.LogWarning("log in page");

        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    public IActionResult Success()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValidateLogin(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var user = await userManager.FindByNameAsync(model.username);

            var password = await userManager.CheckPasswordAsync(user!, model.password);

            // var result = await signInManager.PasswordSignInAsync(model.username, model.password, isPersistent: false, lockoutOnFailure: false);

            if (user is not null && password)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                // var roles = await userManager.GetRolesAsync(user!);
                // if (roles.Contains("Admin"))
                // {
                HttpContext.Session.SetString(SessionVariables.userId, user?.Id!);
                return RedirectToAction(nameof(HomeController.Index), "Home");
                // }
            }
            else
            {
                ModelState.AddModelError(String.Empty, "Incorrect username or password.");

                // Retrieve the list of errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);

                errors.ToList().ForEach(e => logger.LogWarning(e.ErrorMessage));

                TempData["incorrectLogin"] = string.Join("\n", errors.Select(e=>e.ErrorMessage).ToList());

                return RedirectToAction(nameof(Login));
            }
        }

        TempData["incorrectLogin"] = "something failed, redisplaying login...";
        // If we got this far, something failed, redisplay form
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ValidateRegister(RegisterViewModel model)
    {
        logger.LogWarning($"registering {model}");

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
            };

            var result = await userManager.CreateAsync(user, model.password);

            if (result.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);

                // make sure the role exists
                var roleExist = await roleManager.RoleExistsAsync(Roles.REG);
                if (!roleExist)
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole(Roles.REG));
                }

                // lets just assign everyone to admin role for now
                await userManager.AddToRoleAsync(user, Roles.REG);

                var userId = user?.Id!;

                HttpContext.Session.SetString(SessionVariables.userId, userId);

                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        TempData["userAlreadyExists"] = "Something went wrong with registering";

        return RedirectToAction(nameof(Register));

    }
}
