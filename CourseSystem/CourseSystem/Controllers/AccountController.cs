using Core.Enums;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;

namespace UI.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    
    public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
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
            TempData["Error"] = "Entered incorrect username or email. Please try again.";
            return View(loginViewModel);
        }

        var checkPassword = await _userManager.CheckPasswordAsync(user, loginViewModel.Password);

        if (checkPassword)
        {
            var singInAttempt = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);

            if (singInAttempt.Succeeded)
            {
                // return RedirectToAction(); Authorize user
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
            TempData["Error"] = "This username is already in use";
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
            
            // var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            // var callbackUrl = Url.Action(
            //     "ConfirmEmail",
            //     "Account",
            //     new { userId = newUser.Id, code = code },
            //     protocol: HttpContext.Request.Scheme);

            // await _emailService.SendEmailAsync(registerViewModel.EmailAddress, "Confirm your account",
            //     $"Confirm registration, follow the link: <a href='{callbackUrl}'>link</a>");

            TempData["Error"] = "To complete your registration, check your email and follow the link provided in the email";
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
            return RedirectToAction("Index", "Home");
        else
            return View("Error");
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
}