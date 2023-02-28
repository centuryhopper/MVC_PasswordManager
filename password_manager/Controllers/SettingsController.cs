using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Models;
using PasswordManager.Utils;

namespace PasswordManager.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ILogger<SettingsController> logger;
        private readonly UserManager<ApplicationUser> userManager;

        public SettingsController(ILogger<SettingsController> logger, UserManager<ApplicationUser> userManager)
        {
            this.logger = logger;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DeleteAccount()
        {
            var userId = HttpContext.Session.GetString(SessionVariables.userId)!;

            var user = await userManager.FindByIdAsync(userId);

            if (user is null)
            {
                TempData["DeleteAccountError"] = "Error on deleting your own account";
                return RedirectToAction(nameof(Index));
            }

            var result = await userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                // redirect client to the login page
                return RedirectToAction(nameof(AccountController.Login), "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return RedirectToAction(nameof(AccountController.Login), "Account");
        }
    }
}