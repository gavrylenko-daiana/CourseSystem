using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;

namespace UI.Controllers;

public class UserController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public UserController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null) return View("Error");

        var userViewModel = new AppUser()
        {
            Id = currentUser.Id,
            UserName = currentUser.UserName,
            FirstName = currentUser.FirstName,
            LastName = currentUser.LastName,
            Email = currentUser.Email,
            BirthDate = currentUser.BirthDate,
            University = currentUser.University!,
            Telegram = currentUser.Telegram!,
            GitHub = currentUser.GitHub!,
            Role = currentUser.Role,
            UserAssignments = currentUser.UserAssignments,
            UserCourses = currentUser.UserCourses,
            UserGroups = currentUser.UserGroups
        };

        return View(userViewModel);
    }

    [HttpGet]
    [Route("Detail/{id}")]
    public async Task<IActionResult> Detail(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            TempData["Error"] = "This user does not exist.";

            // edit path
            return RedirectToAction("Index", "Home");
        }

        var detailUser = new AppUser()
        {
            Id = user.Id,
            UserName = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            BirthDate = user.BirthDate,
            University = user.University!,
            Telegram = user.Telegram!,
            GitHub = user.GitHub!,
            Role = user.Role,
            UserAssignments = user.UserAssignments,
            UserCourses = user.UserCourses,
            UserGroups = user.UserGroups
        };

        return View(detailUser);
    }
    
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return View("Error");
        }

        var editUserViewModel = new EditUserViewModel()
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            BirthDate = user.BirthDate,
            University = user.University!,
            Telegram = user.Telegram!,
            GitHub = user.GitHub!
        };

        return View(editUserViewModel);
    }
    
    [HttpGet]
    public async Task<IActionResult> EditPassword()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return View("Error");
        }

        var editUserPasswordViewModel = new EditUserPasswordViewModel();

        return View(editUserPasswordViewModel);
    }

    [HttpPost]
    [ActionName("EditPassword")]
    public async Task<IActionResult> EditUserPassword(EditUserPasswordViewModel editUserPasswordViewModel)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Failed to edit password";

            return View("EditPassword", editUserPasswordViewModel);
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return View("Error");
        }

        var checkPassword = await _userManager.CheckPasswordAsync(user, editUserPasswordViewModel.CurrentPassword);

        if (checkPassword)
        {
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, editUserPasswordViewModel.NewPassword);
            await _userManager.UpdateAsync(user);
            
            TempData["SuccessMessage"] = "password has been successfully changed.";

            return View("EditPassword", editUserPasswordViewModel);
            // return RedirectToAction("Index", "User", new { user.Id });
        }
        else
        {
            TempData["Error"] = "You entered incorrect password";

            return View("EditPassword", editUserPasswordViewModel);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditUserViewModel editUserViewModel)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Failed to edit profile";
    
            return View("Edit", editUserViewModel);
        }
    
        var user = await _userManager.GetUserAsync(User);
    
        if (user == null)
        {
            return View("Error");
        }

        user.UserName = editUserViewModel.FirstName + editUserViewModel.LastName;
        user.FirstName = editUserViewModel.FirstName;
        user.LastName = editUserViewModel.LastName;
        user.BirthDate = editUserViewModel.BirthDate;
        user.University = editUserViewModel.University!;
        user.Telegram = editUserViewModel.Telegram!;
        user.GitHub = editUserViewModel.GitHub!;
        user.Email = editUserViewModel.Email;
        
        await _userManager.UpdateAsync(user);
        
        return RedirectToAction("Index", "User", new { user.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null) return View("Error");
        
        //Приходит на почту админу, он должен подтвердить удаление аккаунта
        
        await _signInManager.SignOutAsync();

        return RedirectToAction("Login", "Account");
    }
}