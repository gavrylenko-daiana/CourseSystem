using BLL.Interfaces;
using Core.Enums;
using Core.Helpers;
using Core.Models;
using Core.EmailTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UI.ViewModels;
using UI.ViewModels.EmailViewModels;

namespace UI.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;

    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        RoleManager<IdentityRole> roleManager, IEmailService emailService, IUserService userService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _emailService = emailService;
        _userService = userService;
    }

    private async Task CreateAppUserRoles()
    {
        if (!await _roleManager.RoleExistsAsync(AppUserRoles.Admin.ToString()))
        {
            await _roleManager.CreateAsync(new IdentityRole(AppUserRoles.Admin.ToString()));
        }

        if (!await _roleManager.RoleExistsAsync(AppUserRoles.Teacher.ToString()))
        {
            await _roleManager.CreateAsync(new IdentityRole(AppUserRoles.Teacher.ToString()));
        }

        if (!await _roleManager.RoleExistsAsync(AppUserRoles.Student.ToString()))
        {
            await _roleManager.CreateAsync(new IdentityRole(AppUserRoles.Student.ToString()));
        }
    }

    private string CreateCallBackUrl(string code, string controllerName, string actionName, object routeValues)
    {
        if (controllerName.IsNullOrEmpty() || actionName.IsNullOrEmpty())
        {
            return Url.ActionLink("Index", "Home", protocol: HttpContext.Request.Scheme) ?? string.Empty;
        }

        if (routeValues == null)
        {
            return Url.ActionLink("Index", "Home", protocol: HttpContext.Request.Scheme) ?? string.Empty;
        }

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
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Logout", "Account");
        }

        var login = new LoginViewModel();

        return View(login);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel loginViewModel)
    {
        if (!ModelState.IsValid)
        {
            TempData.TempDataMessage("Error", "Values must not be empty. Please try again.");

            return View(loginViewModel);
        }

        var userResult = await _userService.GetUserByEmailAsync(loginViewModel.EmailAddress);

        if (!userResult.IsSuccessful)
        {
            ViewData.ViewDataMessage("Error", "Entered incorrect email or password. Please try again.");

            return View(loginViewModel);
        }

        var user = userResult.Data;

        if (!ValidationHelpers.IsValidEmail(loginViewModel.EmailAddress))
        {
            ViewData.ViewDataMessage("Error", "Entered incorrect email. Please try again.");

            return View(loginViewModel);
        }

        if (user.Role != AppUserRoles.Admin)
        {
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = CreateCallBackUrl(code, "Account", "ConfirmEmail", new { userId = user.Id, code = code });

                if (await _userManager.IsInRoleAsync(user, AppUserRoles.Admin.ToString()))
                {
                    await _emailService.SendEmailToAppUsers(EmailType.ConfirmAdminRegistration, user, callbackUrl);
                    
                    TempData.TempDataMessage("Error", "Your ADMIN account is not verified, we sent email for confirmation again");
                }
                else
                {
                    await _emailService.SendEmailToAppUsers(EmailType.AccountApproveByAdmin, user, callbackUrl);
                    
                    TempData.TempDataMessage("Error", "Admin hasn't verified your email yet, we sent email for confirmation again");
                }

                return View(loginViewModel);
            }
        }

        var checkPassword = await _userManager.CheckPasswordAsync(user, loginViewModel.Password);

        if (checkPassword)
        {
            var singInAttempt = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);

            if (singInAttempt.Succeeded)
            {
                return RedirectToAction("Index", "User");
            }
        }

        TempData.TempDataMessage("Error", "Entered incorrect email or password. Please try again.");

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
        if (!ModelState.IsValid)
        {
            return View(registerViewModel);
        }

        var userResult = await _userService.GetUserByEmailAsync(registerViewModel.Email);

        if (userResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", "This email is already in use");

            return View(registerViewModel);
        }

        if (!ValidationHelpers.IsValidEmail(registerViewModel.Email))
        {
            ViewData.ViewDataMessage("Error", "Entered incorrect email. Please try again.");

            return View(registerViewModel);
        }

        var newUser = new AppUser();
        registerViewModel.MapTo(newUser);

        newUser.UserName = registerViewModel.FirstName + registerViewModel.LastName;

        var newUserResponse = await _userManager.CreateAsync(newUser, registerViewModel.Password);

        await CreateAppUserRoles();

        if (newUserResponse.Succeeded)
        {
            var roleResult = await _userManager.AddToRoleAsync(newUser, registerViewModel.Role.ToString());

            if (!roleResult.Succeeded)
            {
                return View("Error");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var callbackUrl = CreateCallBackUrl(code, "Account", "ConfirmEmail", new { userId = newUser.Id, code = code });

            if (registerViewModel.Role != AppUserRoles.Admin)
            {
                await _emailService.SendEmailToAppUsers(EmailType.AccountApproveByAdmin, newUser, callbackUrl);

                TempData.TempDataMessage("Error", "Please, wait for registration confirmation from the admin");

                return View(registerViewModel);
            }
            else
            {
                var emailSentResult = await _emailService.SendEmailToAppUsers(EmailType.ConfirmAdminRegistration, newUser, callbackUrl);

                if (!emailSentResult.IsSuccessful)
                {
                    TempData.TempDataMessage("Error", emailSentResult.Message);
                }

                TempData.TempDataMessage("Error", "To complete your ADMIN registration, check your email and follow the link provided in the email");

                return View(registerViewModel);
            }
        }
        else
        {
            var errorMessages = string.Empty;

            foreach (var error in newUserResponse.Errors)
            {
                errorMessages += $"{error.Description}{Environment.NewLine}";
            }

            TempData.TempDataMessage("Error", errorMessages);

            return View(registerViewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return View("Error");
        }

        var userResult = await _userService.FindByIdAsync(userId);

        if (!userResult.IsSuccessful)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = userResult.Data;
        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var toUserProfileUrl = CreateCallBackUrl(token, "User", "Detail", new { id = user.Id });

            await _emailService.SendEmailToAppUsers(EmailType.UserRegistration, user, toUserProfileUrl);

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
        await _signInManager.SignOutAsync();

        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string userId)
    {
        var userResult = await _userService.FindByIdAsync(userId);

        if (userResult.IsSuccessful)
        {
            await _signInManager.UserManager.DeleteAsync(userResult.Data);
        }

        return View();
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        var forgotPasswordViewModel = new ForgotViewModel();

        return View(forgotPasswordViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotViewModel forgotPasswordBeforeEnteringViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(forgotPasswordBeforeEnteringViewModel);
        }

        var userResult = await _userService.GetUserByEmailAsync(forgotPasswordBeforeEnteringViewModel.Email);

        if (!userResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", "You wrote an incorrect email. Try again!");

            return View(forgotPasswordBeforeEnteringViewModel);
        }

        // Email
        var emailCodeResult = await _emailService.SendCodeToUser(forgotPasswordBeforeEnteringViewModel.Email);

        if (!emailCodeResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", emailCodeResult.Message);

            return View(forgotPasswordBeforeEnteringViewModel);
        }

        return RedirectToAction("CheckEmailCode", new { code = emailCodeResult.Data, email = forgotPasswordBeforeEnteringViewModel.Email });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> SendCodeUser(ForgotEntity forgotEntity)
    {
        var userResult = await _userService.GetCurrentUser(User);

        if (!userResult.IsSuccessful)
        {
            return RedirectToAction("Login", "Account");
        }

        var emailCodeResult = await _emailService.SendCodeToUser(userResult.Data.Email!);

        if (!emailCodeResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", emailCodeResult.Message);

            return RedirectToAction("Login", "Account");
        }

        return RedirectToAction("CheckEmailCode", new { code = emailCodeResult.Data, email = userResult.Data.Email, forgotEntity });
    }

    [HttpGet]
    public IActionResult CheckEmailCode(int code, string email, ForgotEntity forgotEntity)
    {
        var forgotViewModel = new ForgotViewModel()
        {
            Email = email,
            ForgotEntity = forgotEntity
        };

        TempData["Code"] = code;

        return View(forgotViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CheckEmailCode(int code, ForgotViewModel forgotViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(forgotViewModel);
        }

        TempData["ForgotEntity"] = forgotViewModel.ForgotEntity;

        if (code == forgotViewModel.EmailCode)
        {
            if (forgotViewModel.ForgotEntity == ForgotEntity.Password)
            {
                return RedirectToAction("ResetPassword", new { email = forgotViewModel.Email });
            }

            return RedirectToAction("ResetEmail", new { email = forgotViewModel.Email });
        }
        else
        {
            TempData.TempDataMessage("Error", "Invalid code. Please try again.");

            return RedirectToAction("Login", "Account");
        }
    }

    [HttpGet]
    public IActionResult ResetPassword(string email)
    {
        TempData.TempDataMessage("SuccessMessage", "Code is valid. You can reset your password.");

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

        var result = await _userService.UpdatePasswordAsync(newPasswordViewModel.Email, newPasswordViewModel.NewPassword);

        if (!result.IsSuccessful)
        {
            TempData.TempDataMessage("Error", "Failed to reset password.");
        }

        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public IActionResult RegisterNewAdmin()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RegisterNewAdmin(RegisterAdminViewModel registerAdminViewModel)
    {
        var userResult = await _userService.GetUserByEmailAsync(registerAdminViewModel.Email);

        if (userResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"This email already exist");

            return View(registerAdminViewModel);
        }

        var newAdmin = new AppUser();
        registerAdminViewModel.MapTo(newAdmin);
        
        newAdmin.UserName = newAdmin.FirstName + newAdmin.LastName;

        var newUserResponse = await _userManager.CreateAsync(newAdmin, _userService.GenerateTemporaryPassword());

        if (!newUserResponse.Succeeded)
        {
            TempData.TempDataMessage("Error", "Failed to add new admin");

            return View(registerAdminViewModel);
        }

        var sendResult = await _emailService.SendTempPasswordToUser(EmailType.GetTempPasswordToAdmin, newAdmin);

        if (!sendResult.IsSuccessful)
        {
            await _userManager.DeleteAsync(newAdmin);
            
            TempData.TempDataMessage("Error", $"{sendResult.Message}");

            return View(registerAdminViewModel);
        }

        return RedirectToAction("Index", "User");
    }

    [HttpGet]
    public IActionResult ResetEmail(string email)
    {
        TempData.TempDataMessage("SuccessMessage", "Code is valid. You can reset your email.");

        var resetEmail = new NewEmailViewModel()
        {
            Email = email
        };

        return View(resetEmail);
    }

    [HttpPost]
    public async Task<IActionResult> ResetEmail(NewEmailViewModel newEmailViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(newEmailViewModel);
        }

        var currentUserResult = await _userService.GetCurrentUser(User);

        if (!currentUserResult.IsSuccessful)
        {
            return RedirectToAction("Login");
        }

        var user = await _userService.GetUserByEmailAsync(newEmailViewModel.NewEmail);

        if (user != null)
        {
            TempData.TempDataMessage("Error", $"This email already exist");

            return View(newEmailViewModel);
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(currentUserResult.Data);

        var callbackUrl = CreateCallBackUrl(code, "Account", "ConfirmedResetEmail", 
            new
                {
                    userId = currentUserResult.Data.Id, code = code, currentEmail = newEmailViewModel.Email,
                    newEmail = newEmailViewModel.NewEmail
                });

        currentUserResult.Data.Email = newEmailViewModel.NewEmail;

        await _emailService.SendEmailToAppUsers(EmailType.AccountApproveByUser, currentUserResult.Data, callbackUrl);

        TempData.TempDataMessage("Error", "Please, wait for registration confirmation from the admin");

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmedResetEmail(string userId, string code, string currentEmail,
        string newEmail)
    {
        if (userId == null || code == null)
        {
            return View("Error");
        }

        var userResult = await _userService.FindByIdAsync(userId);

        if (!userResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", $"{userResult.Message}");

            return RedirectToAction("Login", "Account");
        }

        var user = userResult.Data;
        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {
            var updateResult = await _userService.UpdateEmailAsync(currentEmail, newEmail);

            if (!updateResult.IsSuccessful)
            {
                TempData.TempDataMessage("Error", $"{updateResult.Message}");

                return View("Login");
            }

            return RedirectToAction("Logout");
        }
        else
        {
            TempData.TempDataMessage("Error", $"Failed to confirm email");

            return View("Error");
        }
    }
}