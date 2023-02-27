using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Models;
using PasswordManager.Services;
using PasswordManager.Utils;

namespace PasswordManager.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ILogger<SettingsController> logger;
        private readonly IAuthenticationService<UserModel> authService;

        public SettingsController(ILogger<SettingsController> logger, IAuthenticationService<UserModel> authService)
        {
            this.logger = logger;
            this.authService = authService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DeleteAccount()
        {
            // call delete account method from service
            await authService.Delete(HttpContext.Session.GetString(SessionVariables.userId)!);

            // redirect client to the login page
            return RedirectToAction("Index", "Login");
        }
    }
}