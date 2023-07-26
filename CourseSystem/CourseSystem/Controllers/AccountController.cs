using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            TempData["Error"] = "Entered incorrect email. Please try again.";

            return View(loginViewModel);
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            TempData["Error"] = "Admin hasn't verified your email yet";

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

        var newUserResponse = await _userManager.CreateAsync(newUser, registerViewModel.Password);

        if (!await _roleManager.RoleExistsAsync(AppUserRoles.Admin.ToString()))
            await _roleManager.CreateAsync(new IdentityRole(AppUserRoles.Admin.ToString()));

        if (!await _roleManager.RoleExistsAsync(AppUserRoles.Teacher.ToString()))
            await _roleManager.CreateAsync(new IdentityRole(AppUserRoles.Teacher.ToString()));

        if (!await _roleManager.RoleExistsAsync(AppUserRoles.Student.ToString()))
            await _roleManager.CreateAsync(new IdentityRole(AppUserRoles.Student.ToString()));

        if (newUserResponse.Succeeded)
        {
            var roleResult = await _userManager.AddToRoleAsync(newUser, registerViewModel.Role.ToString());

            if (!roleResult.Succeeded) return View("Error");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Account",
                new { userId = newUser.Id, code = code },
                protocol: HttpContext.Request.Scheme);

            await _emailService.SendUserApproveToAdmin(newUser, callbackUrl);

            if (registerViewModel.Role != AppUserRoles.Admin)
            {
                TempData["Error"] = "Please, wait for registration confirmation from the admin";

                return View(registerViewModel);
            }
            else
            {
                var emailData = new EmailData(new List<string> { registerViewModel.EmailAddress },
                     "Confirm your account",
                     $"Confirm registration, follow the link: <a href='{callbackUrl}'>link</a>");

                var emailSentResult = await _emailService.SendEmailAsync(emailData);

                if (!emailSentResult.IsSuccessful)
                    TempData["Error"] = emailSentResult.Message;

                TempData["Error"] = "To complete your ADMIN registration, check your email and follow the link provided in the email";
                return View(registerViewModel);
            }
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
            var callbackUrl = Url.Action(
                "Detail",
                "User",                
                new { id = user.Id },
                protocol: HttpContext.Request.Scheme);

            await _emailService.SendEmailAboutSuccessfulRegistration(user, callbackUrl);

            var confirmEmailVM = new ConfirmEmailViewModel()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                UserId = userId
            };

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