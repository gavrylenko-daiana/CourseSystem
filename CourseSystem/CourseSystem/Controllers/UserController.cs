using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;

namespace UI.Controllers;

public class UserController : Controller
{
    private readonly UserManager<AppUser> _userManager;

    public UserController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
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
            University = currentUser.University,
            Telegram = currentUser.Telegram,
            GitHub = currentUser.GitHub,
            Role = currentUser.Role,
            UserAssignments = currentUser.UserAssignments,
            UserCourses = currentUser.UserCourses,
            UserGroups = currentUser.UserGroups
        };

        return View(userViewModel);
    }

    [HttpGet]
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
            University = user.University,
            Telegram = user.Telegram,
            GitHub = user.GitHub,
            Role = user.Role,
            UserAssignments = user.UserAssignments,
            UserCourses = user.UserCourses,
            UserGroups = user.UserGroups
        };

        return View(detailUser);
    }
}