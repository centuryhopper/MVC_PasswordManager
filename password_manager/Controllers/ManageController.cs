using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Models;
using PasswordManager.Utils;

namespace PasswordManager.Controllers;

public class ManageController : Controller
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly ILogger<ManageController> logger;

    public ManageController(UserManager<ApplicationUser> userManager, ILogger<ManageController> logger)
    {
        this.userManager = userManager;
        this.logger = logger;
    }

    [Authorize(Roles = Constants.ADMIN)]
    public async Task<IActionResult> Index()
    {
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

