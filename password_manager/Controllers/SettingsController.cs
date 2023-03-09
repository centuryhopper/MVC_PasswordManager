using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using password_manager.Models;
using password_manager.Utils;

namespace password_manager.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ILogger<SettingsController> logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IEmailSender emailSender;

        public SettingsController(ILogger<SettingsController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender)
        {
            this.logger = logger;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.emailSender = emailSender;
        }

        [Authorize(Roles=$"{Constants.ADMIN},{Constants.USER},{Constants.AUDITOR},{Constants.MANAGER}")]
        public IActionResult Index(string returnUrl)
        {
            if (!signInManager.IsSignedIn(User))
            {
                return RedirectToAction(nameof(AccountController.Login), "Account", new {returnUrl = returnUrl});
            }
            return View();
        }

        public async Task<IActionResult> DeleteAccount(ConfirmDeleteViewModel model)
        {
            var userId = HttpContext.Session.GetString(Constants.userId)!;

            var user = await userManager.FindByIdAsync(userId);

            var errors = new List<string>{};

            if (user is null)
            {
                TempData["DeleteAccountError"] = "Error on deleting your own account";
                return RedirectToAction(nameof(Index));
            }

            if (await userManager.CheckPasswordAsync(user, model.ConfirmPassword))
            {
                logger.LogWarning("Password is correct");
                var result = await userManager.DeleteAsync(user);

                if (result.Succeeded)
                {
                    // redirect client to the login page
                    TempData[Constants.ACCOUNT_DELETION_CONFIRMATION] = "Your account has been deleted.";
                    return RedirectToAction(nameof(AccountController.Login), "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

            }

            ModelState.AddModelError("", "Incorrect Password!");
            errors.Add(Helpers.GetErrors<SettingsController>(ModelState, logger));

            TempData[Constants.DELETE_ERROR] = string.Join(", ", errors);

            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

        public IActionResult EditUserInfo()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (!HttpContext.User.Identity!.IsAuthenticated)
            {
                return View("AccessDenied");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction(nameof(AccountController.Login), "Account");
                }

                // ChangePasswordAsync changes the user password
                var result = await userManager.ChangePasswordAsync(user,
                    model.CurrentPassword, model.NewPassword);

                // The new password did not meet the complexity rules or
                // the current password is incorrect. Add these errors to
                // the ModelState and rerender ChangePassword view
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    TempData[Constants.CHANGE_PASSWORD_ERROR] = Helpers.GetErrors<SettingsController>(ModelState, logger);
                    return View();
                }

                // Upon successfully changing the password refresh sign-in cookie
                await signInManager.RefreshSignInAsync(user);
                return View("ChangePasswordConfirmation");
            }

            TempData[Constants.CHANGE_PASSWORD_ERROR] = "Failed to change password.";

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Autosave(string autoSaveModel)
        {
            var model = JsonConvert.DeserializeObject<EditUserInfoViewModel>(autoSaveModel)!;
            // logger.LogWarning($"autosaving {model}");

            if
            (
                string.IsNullOrEmpty(model.UserName) ||
                string.IsNullOrEmpty(model.Email) ||
                string.IsNullOrEmpty(model.FirstName) ||
                string.IsNullOrEmpty(model.LastName) ||
                string.IsNullOrEmpty(model.Role)
            )
            {
                logger.LogWarning("all fields should not be empty");
                // TempData[Constants.ERROR_EDIT_USER] = "Please make sure you have entered all the proper fields when editing your password account.";
                return BadRequest(ModelState);
            }

            var user = await userManager.FindByIdAsync(HttpContext.Session.GetString(Constants.userId)!);
            if (user == null)
            {
                TempData[Constants.ERROR_EDIT_USER] = "Your profile isn't not found for some reason :(";
                return NotFound();
            }

            // update user info
            user.UserName = model.UserName;
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            // Remove user from all existing roles
            var existingRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, existingRoles);

            // create the role if it doesn't exist yet
            if (!(await roleManager.RoleExistsAsync(model.Role)))
            {
                await roleManager.CreateAsync(new IdentityRole(model.Role));
            }
            // Add user to new role
            var result = await userManager.AddToRoleAsync(user, model.Role);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // TempData[Constants.EDIT_USER] = "Save successful";

            return Ok();
        }

        public async Task<IActionResult> ToggleTwoFactor()
        {
            var user = await userManager.FindByIdAsync(HttpContext.Session.GetString(Constants.userId)!);
            user!.TwoFactorEnabled = !user.TwoFactorEnabled;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public async Task<IActionResult> LoginTwoStep(string email)
        {
            var user = await userManager.FindByEmailAsync(email);

            var token = await userManager.GenerateTwoFactorTokenAsync(user!, "Email");

            var subject = "Password Manager Login Verification Code";
            var emailBody = $"code: {token}";

            await Helpers.SendEmail<SettingsController>(emailSender, email, subject, emailBody, logger);

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginTwoStep(TwoFactor twoFactor)
        {
            if (!ModelState.IsValid)
            {
                return View("Error", new ErrorViewModel {ErrorTitle="error", ErrorMessage="incorrect verification code"});
            }

            var result = await signInManager.TwoFactorSignInAsync("Email", twoFactor.TwoFactorCode, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
            else
            {
                ModelState.AddModelError("", "Incorrect Code");
                // TempData[Constants.VERIFICATION_ERROR] = Helpers.GetErrors<SettingsController>(ModelState, logger);
                return View();
            }
        }

    }
}