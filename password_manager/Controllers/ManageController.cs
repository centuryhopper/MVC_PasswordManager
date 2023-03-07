using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using password_manager.Models;
using password_manager.Utils;

namespace password_manager.Controllers;

public class ManageController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly ILogger<ManageController> logger;

    public ManageController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<ManageController> logger)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.logger = logger;
    }

    [Authorize(Roles = Constants.ADMIN)]
    public async Task<IActionResult> Index(string returnUrl)
    {
        if (!signInManager.IsSignedIn(User))
        {
            return RedirectToAction(nameof(AccountController.Login), "Account", new {returnUrl = returnUrl});
        }
        var users = await (from user in userManager.Users select user).ToListAsync();
        var roles = new List<IEnumerable<string>>(users.Count);
        foreach (var user in users)
        {
            var role = await userManager.GetRolesAsync(user);
            roles.Add(role.AsEnumerable());
        }

        return View(new ManageUsersViewModel<ApplicationUser> { Users = users, Roles = roles });
    }
}

