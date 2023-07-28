using BLL.Interfaces;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UI.ViewModels;

namespace UI.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailService _emailService;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        RoleManager<IdentityRole> roleManager, IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _emailService = emailService;
    }

    private async Task CreateAppUserRoles()
    {
        if (!await _roleManager.RoleExistsAsync(AppUserRoles.Admin.ToString()))
            await _roleManager.CreateAsync(new IdentityRole(AppUserRoles.Admin.ToString()));

        if (!await _roleManager.RoleExistsAsync(AppUserRoles.Teacher.ToString()))
            await _roleManager.CreateAsync(new IdentityRole(AppUserRoles.Teacher.ToString()));

        if (!await _roleManager.RoleExistsAsync(AppUserRoles.Student.ToString()))
            await _roleManager.CreateAsync(new IdentityRole(AppUserRoles.Student.ToString()));
    }

    private string CreateCallBackUrl(string code, string controllerName, string actionName, object routeValues)
    {
        if (controllerName.IsNullOrEmpty() || actionName.IsNullOrEmpty())
            return Url.ActionLink("Index", "Home", protocol: HttpContext.Request.Scheme) ?? string.Empty;

        if (routeValues == null)
            return Url.ActionLink("Index", "Home", protocol: HttpContext.Request.Scheme) ?? string.Empty;

        var callbackUrl = Url.Action(
            actionName,
            controllerName,
            routeValues,
            protocol: HttpContext.Request.Scheme);

        return callbackUrl;
    }

    [HttpGet]
    public IActionResult Login()
    {
        var login = new LoginViewModel();
        return View(login);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel)
    {
        if (!ModelState.IsValid) return View(loginViewModel);

        var user = await _userManager.FindByEmailAsync(loginViewModel.EmailAddress);

        if (user == null)
        {
            // TempData["Error"] = "Entered incorrect email. Please try again.";
            TempData.TempDataMessage("Error", "Entered incorrect email. Please try again.");

            return View(loginViewModel);
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            var callbackUrl = CreateCallBackUrl(code, "Account", "ConfirmEmail", new { userId = user.Id, code = code });

            if (await _userManager.IsInRoleAsync(user, AppUserRoles.Admin.ToString()))
            {
                await _emailService.SendAdminEmailConfirmation(loginViewModel.EmailAddress, callbackUrl);
                TempData["Error"] = "Your ADMIN account is not verified, we sent email for confirmation again";
            }
            else
            {
                await _emailService.SendUserApproveToAdmin(user, callbackUrl);
                TempData["Error"] = "Admin hasn't verified your email yet, we sent email for confirmation again";
            }

            return View(loginViewModel);
        }

        var checkPassword = await _userManager.CheckPasswordAsync(user, loginViewModel.Password);

        if (checkPassword)
        {
            var singInAttempt = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);

            if (singInAttempt.Succeeded)
            {
                // return RedirectToAction(); Authorize user
                return RedirectToAction("Index", "Home");
            }
        }

        TempData["Error"] = "Entered incorrect password. Please try again.";

        return View(loginViewModel);
    }

    [HttpGet]
    public IActionResult Register()
    {
        var register = new RegisterViewModel();

        return View(register);
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
    {
        if (!ModelState.IsValid) return View(registerViewModel);

        var user = await _userManager.FindByEmailAsync(registerViewModel.EmailAddress);

        if (user != null)
        {
            TempData["Error"] = "This email is already in use";
            return View(registerViewModel);
        }

        var newUser = new AppUser()
        {
            UserName = registerViewModel.FirstName + registerViewModel.LastName,
            Email = registerViewModel.EmailAddress,
            FirstName = registerViewModel.FirstName,
            LastName = registerViewModel.LastName,
            BirthDate = registerViewModel.BirthDate,
            University = registerViewModel.University,
            Role = registerViewModel.Role
        };

        if (registerViewModel.Telegram != null) newUser.Telegram = registerViewModel.Telegram;
        if (registerViewModel.GitHub != null) newUser.GitHub = registerViewModel.GitHub;

        try
        {
            var newUserResponse = await _userManager.CreateAsync(newUser, registerViewModel.Password);

            await CreateAppUserRoles();

            if (newUserResponse.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(newUser, registerViewModel.Role.ToString());

                if (!roleResult.Succeeded) return View("Error");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                var callbackUrl =
                    CreateCallBackUrl(code, "Account", "ConfirmEmail", new { userId = newUser.Id, code = code });

                if (registerViewModel.Role != AppUserRoles.Admin)
                {
                    await _emailService.SendUserApproveToAdmin(newUser, callbackUrl);
                    
                    TempData["Error"] = "Please, wait for registration confirmation from the admin";
                    
                    return View(registerViewModel);
                }
                else
                {
                    var emailSentResult =
                        await _emailService.SendAdminEmailConfirmation(registerViewModel.EmailAddress, callbackUrl);

                    if (!emailSentResult.IsSuccessful)
                        TempData["Error"] = emailSentResult.Message;

                    TempData["Error"] =
                        "To complete your ADMIN registration, check your email and follow the link provided in the email";
                    return View(registerViewModel);
                }
            }
            else
            {
                TempData["Error"] =
                    "Your password must have at least 6 characters. Must have at least 1 capital letter character, 1 digit and 1 symbol to choose from (!@#$%^&*()_+=\\[{]};:<>|./?,-)";
                return View(registerViewModel);
            }
        }
        catch (Exception e)
        {
            TempData["Error"] = $"{e.Message}";
            return View(registerViewModel);
        }

        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return View("Error");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return View("Error");
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var toUserProfileUrl = CreateCallBackUrl(token, "User", "Detail", new { id = user.Id });
            await _emailService.SendEmailAboutSuccessfulRegistration(user, toUserProfileUrl);

            var confirmEmailVM = new ConfirmEmailViewModel();
            user.MapTo(confirmEmailVM);

            return View(confirmEmailVM);
        }
        else
        {
            return View("Error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _signInManager.SignOutAsync();
        }
        catch
        {
            return View("Error");
        }

        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    [Route("Delete/{userId}")]
    public async Task<IActionResult> Delete(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user != null)
                await _signInManager.UserManager.DeleteAsync(user);

            return View();
        }
        catch
        {
            return View("Error");
        }
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        var forgotPasswordViewModel = new ForgotPasswordViewModel();

        return View(forgotPasswordViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordViewModel forgotPasswordBeforeEnteringViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(forgotPasswordBeforeEnteringViewModel);
        }

        var user = await _userManager.FindByEmailAsync(forgotPasswordBeforeEnteringViewModel.Email);

        if (user == null)
        {
            TempData["Error"] = "You wrote an incorrect email. Try again!";

            return View(forgotPasswordBeforeEnteringViewModel);
        }

        // Email
        var emailCode = await _emailService.SendCodeToUser(forgotPasswordBeforeEnteringViewModel.Email);

        return RedirectToAction("CheckEmailCode",
            new { code = emailCode, email = forgotPasswordBeforeEnteringViewModel.Email });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> SendCodeUser()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null) return View("Error");

        var emailCode = await _emailService.SendCodeToUser(user.Email!);

        return RedirectToAction("CheckEmailCode",
            new { code = emailCode, email = user.Email });
    }

    [HttpGet]
    public IActionResult CheckEmailCode(int code, string email)
    {
        var forgotPasswordCodeViewModel = new ForgotPasswordViewModel()
        {
            Email = email,
        };

        TempData["Code"] = code;

        return View(forgotPasswordCodeViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CheckEmailCode(int code,
        ForgotPasswordViewModel forgotPasswordCodeViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(forgotPasswordCodeViewModel);
        }

        if (code == forgotPasswordCodeViewModel.EmailCode)
        {
            return RedirectToAction("ResetPassword", new { email = forgotPasswordCodeViewModel.Email });
        }
        else
        {
            TempData["Error"] = "Invalid code. Please try again.";

            return RedirectToAction("Login", "Account");
        }
    }

    [HttpGet]
    public IActionResult ResetPassword(string email)
    {
        TempData["SuccessMessage"] = "Code is valid. You can reset your password.";

        var resetPassword = new NewPasswordViewModel()
        {
            Email = email
        };

        return View(resetPassword);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(NewPasswordViewModel newPasswordViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(newPasswordViewModel);
        }

        var user = await _userManager.FindByEmailAsync(newPasswordViewModel.Email);
        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, newPasswordViewModel.NewPassword);
        await _userManager.UpdateAsync(user);
        await _signInManager.SignOutAsync();

        return RedirectToAction("Login");
    }
}