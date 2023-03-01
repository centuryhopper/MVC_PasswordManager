using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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
    private readonly IEmailSender emailSender;

    public AccountController(ILogger<AccountController> logger, IConfiguration configuration, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.roleManager = roleManager;
        this.emailSender = emailSender;
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

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Find the user by email
        var user = await userManager.FindByEmailAsync(model.Email);

        // If the user is found AND Email is confirmed
        if (user is not null && await userManager.IsEmailConfirmedAsync(user))
        {
            // Generate the reset password token
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            // Build the password reset link
            var passwordResetLink = Url.Action("ResetPassword", "Account",
                    new { email = model.Email, token = token }, protocol: Request.Scheme);

            // Log the password reset link
            // TODO: instead of logging, actually send it to the user provided email
            logger.Log(LogLevel.Warning, passwordResetLink);

            var emailBody = $"Dear {user.UserName}, please click the following link to reset your password: {passwordResetLink}";

            var subject = "Password reset request";

            await Helpers.SendEmail<AccountController>(emailSender, model.Email, subject, emailBody, logger);

            // Send the user to Forgot Password Confirmation view
            return View("ForgotPasswordConfirmation");
        }

        // To avoid account enumeration and brute force attacks, don't
        // reveal that the user does not exist or is not confirmed
        return View("ForgotPasswordConfirmation");

    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string token, string email)
    {
        // If password reset token or email is null, most likely the
        // user tried to tamper the password reset link
        if (token == null || email == null)
        {
            ModelState.AddModelError("", "Invalid password reset token");
        }
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Find the user by email
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                // reset the user password
                var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    logger.LogWarning("Successfully reset password");
                    return View("ResetPasswordConfirmation");
                }
                // Display validation errors. For example, password reset token already
                // used to change the password or password complexity rules not met
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // To avoid account enumeration and brute force attacks, don't
            // reveal that the user does not exist
            return View("ResetPasswordConfirmation");
        }
        // Display validation errors if model state is not valid
        return View(model);
    }

    [AcceptVerbs("Get", "Post")]
    [AllowAnonymous]
    public async Task<IActionResult> IsEmailInUse(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            logger.LogWarning("Email you provided is fine");
            return Json(true);
        }

        return Json($"Email {email} is already in use");
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValidateLogin(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var user = await userManager.FindByNameAsync(model.username)!;

            if (user != null && !user.EmailConfirmed &&
                    (await userManager.CheckPasswordAsync(user, model.password)))
            {
                ModelState.AddModelError(string.Empty, "Email not confirmed yet");
                TempData["incorrectLogin"] = Helpers.GetErrors<AccountController>(ModelState);
                return View(model);
            }

            var result = await signInManager.PasswordSignInAsync(model.username, model.password, isPersistent: false, lockoutOnFailure: false);

            if (user is not null && result.Succeeded)
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

                TempData["incorrectLogin"] = Helpers.GetErrors<AccountController>(ModelState);

                return RedirectToAction(nameof(Login));
            }
        }

        TempData["incorrectLogin"] = "something failed, redisplaying login...";
        // If we got this far, something failed, redisplay form
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (userId is null || token is null)
        {
            TempData["incorrectLogin"] = "couldn't successfully confirm email";
            return RedirectToAction(nameof(Login));
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return View("Error", new ErrorViewModel {ErrorMessage = $"The User ID {userId} is invalid"});
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            return View();
        }

        return View("Error", new ErrorViewModel {ErrorMessage = "Email cannot be confirmed"});

    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> ValidateRegister(RegisterViewModel model)
    {
        logger.LogWarning($"registering {model}");

        var existingUser = await userManager.FindByEmailAsync(model.Email);

        if (existingUser is null && ModelState.IsValid)
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
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                var confirmationLink = Url.Action(nameof(AccountController.ConfirmEmail), "Account",
                    new { userId = user.Id, token = token }, Request.Scheme);

                if (signInManager.IsSignedIn(User) && User.IsInRole(Roles.ADMIN))
                {
                    return RedirectToAction(nameof(ManageController.Index), "Manage");
                }

                // make sure the role exists
                var roleExist = await roleManager.RoleExistsAsync(Roles.REG);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(Roles.REG));
                }

                // lets just assign everyone to admin role for now
                await userManager.AddToRoleAsync(user, Roles.REG);

                var subject = "Password Manager Account Confirmation Link";

                var emailBody = $"Please use this confirmation link to confirm your email address: {confirmationLink}";


                await Helpers.SendEmail<AccountController>(emailSender, model.Email, subject, emailBody, logger);


                return View("Success", new SuccessViewModel { SuccessMessage = "Before you can Login, please confirm your email by clicking on the confirmation link we have emailed you", SuccessTitle = "Registration successful" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        var msg = Helpers.GetErrors<AccountController>(ModelState);

        if (String.IsNullOrEmpty(msg))
        {
            msg = "user with the email you provided already exists.";
        }

        TempData["userAlreadyExists"] = msg;

        return RedirectToAction(nameof(Register));
    }

}
