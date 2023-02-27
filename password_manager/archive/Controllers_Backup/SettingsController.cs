// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using PasswordManager.Models;
// using PasswordManager.Services;
// using PasswordManager.Utils;

// namespace password_manager.Controllers
// {
//     public class SettingsController : Controller
//     {
//         private readonly ILogger<SettingsController> logger;
//         private readonly IAuthenticationService<UserModel> authService;
//         private readonly IHttpContextAccessor ctx;

//         public SettingsController(ILogger<SettingsController> logger, IAuthenticationService<UserModel> authService, IHttpContextAccessor ctx)
//         {
//             this.logger = logger;
//             this.authService = authService;
//             this.ctx = ctx;
//         }

//         public IActionResult Index()
//         {
//             if (ctx.HttpContext!.User.Identity!.IsAuthenticated)
//             {
//                 return View();
//             }

//             TempData["sessionExpired"] = "The session has expired. Please Log in again.";

//             return RedirectToAction("Login", "Account");
//         }

//         public async Task<IActionResult> DeleteAccount()
//         {
//             // call delete account method from service
//             await authService.Delete(ctx.HttpContext!.Session.GetString(SessionVariables.userId)!);

//             // redirect client to the login page
//             return RedirectToAction("Index","Login");
//         }
//     }
// }