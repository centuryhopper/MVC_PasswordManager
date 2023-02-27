// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Security.Claims;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Authentication;
// using Microsoft.AspNetCore.Authentication.Cookies;
// using Microsoft.AspNetCore.Mvc;
// using PasswordManager.Models;
// using PasswordManager.Services;
// using PasswordManager.Utils;

// namespace password_manager.Controllers
// {
//     public class AccountController : Controller
//     {
//         private readonly ILogger<AccountController> logger;
//         private readonly IAuthenticationService<UserModel> authService;
//         private readonly IHttpContextAccessor ctx;

//         public AccountController(ILogger<AccountController> logger, IAuthenticationService<UserModel> authService, IHttpContextAccessor ctx)
//         {
//             this.logger = logger;
//             this.authService = authService;
//             this.ctx = ctx;
//         }

//         public async Task<IActionResult> Login()
//         {
//             await ctx.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

//             ctx.HttpContext!.Response.Headers["Cache-Control"] = "no-store";

//             ctx.HttpContext.Response.Cookies.Delete(".AspNetCore.Session");
//             ctx.HttpContext.Response.Cookies.Delete(".AspNetCore.Antiforgery.2AllGjtG7jM");

//             ctx.HttpContext.Session.Clear();

//             logger.LogWarning("log in page");

//             return View();
//         }

//         public IActionResult Register()
//         {
//             return View();
//         }

//         [HttpPost]
//         public async Task<ActionResult> ValidateLogin(UserModel model)
//         {
//             logger.LogWarning($"trying to log in");

//             string? val = await authService.Login(model);

//             var claims = new List<Claim>
//             {
//                 new Claim(ClaimTypes.Name, model.username!)
//             };
//             var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
//             var principal = new ClaimsPrincipal(identity);
//             var props = new AuthenticationProperties();

//             await ctx.HttpContext!.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);

//             if (ModelState.IsValid && !string.IsNullOrEmpty(val))
//             {
//                 ctx.HttpContext!.Session.SetString(SessionVariables.userId, val);

//                 logger.LogWarning($"login ctx user id: {ctx.HttpContext!.Session.GetString(SessionVariables.userId)}");

//                 return RedirectToAction("Index", "Home");
//             }

//             TempData["incorrectLogin"] = "Incorrect username or password";

//             return RedirectToAction(nameof(Login));
//         }

//         [HttpPost]
//         public async Task<ActionResult> ValidateRegister(UserModel model)
//         {
//             logger.LogWarning($"logged in {model}");

//             bool res = await authService.Register(model);

//             if (ModelState.IsValid && res)
//             {
//                 return RedirectToAction(nameof(Login));
//             }

//             TempData["userAlreadyExists"] = "This user already exists. Please try a different username.";

//             return RedirectToAction(nameof(Register));
//         }
//     }
// }