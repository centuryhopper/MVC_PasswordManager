using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Models;
using PasswordManager.Services;

namespace password_manager.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ILogger<RegisterController> logger;
        private readonly IAuthenticationService<UserModel> authService;

        public RegisterController(ILogger<RegisterController> logger, IAuthenticationService<UserModel> authService)
        {
            this.logger = logger;
            this.authService = authService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(UserModel model)
        {
            logger.LogWarning($"logged in {model}");

            bool res = await authService.Register(model);

            if (ModelState.IsValid && res)
            {
                return RedirectToAction("Index", "Login");
            }

            TempData["userAlreadyExists"] = "This user already exists. Please try a different username.";

            return RedirectToAction("Index");
        }
    }
}
