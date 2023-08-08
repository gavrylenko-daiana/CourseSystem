using BLL.Interfaces;
using Core.Helpers;
using Core.Models;
using Core.EmailTemplates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UI.ViewModels;
using UI.ViewModels.EmailViewModels;

namespace UI.Controllers;

[Authorize]
public class UserController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IEmailService _emailService;

    public UserController(UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IEmailService emailService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if (currentUser == null)
        {
            return View("Error");
        }

        var userViewModel = new AppUser();
        currentUser.MapTo(userViewModel);
      
        return View(userViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            TempData.TempDataMessage("Error", "This user does not exist.");
            // edit path
            return RedirectToAction("Index", "Home");
        }
        
        var userViewModel = new AppUser();
        user.MapTo(userViewModel);

        return View(userViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return View("Error");
        }
        
        var editUserViewModel = new EditUserViewModel();
        user.MapTo(editUserViewModel);

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
            TempData.TempDataMessage("Error", "Failed to edit password");
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

            TempData.TempDataMessage("SuccessMessage", "password has been successfully changed.");

            return View("EditPassword", editUserPasswordViewModel);
            // return RedirectToAction("Index", "User", new { user.Id });
        }
        else
        {
            TempData.TempDataMessage("Error", "You entered incorrect password");
            return View("EditPassword", editUserPasswordViewModel);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditUserViewModel editUserViewModel)
    {
        if (!ModelState.IsValid)
        {
            TempData.TempDataMessage("Error", "Failed to edit profile");
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

        if (user == null)
        {
            return View("Error");
        }

        //Comes to the e-mail Admin, he or she have to confirm the deletion of the account
        var callbackUrl = Url.Action(
            "ConfirmUserDeletion",
            "User",
            new { userId = user.Id },
            protocol: HttpContext.Request.Scheme);

        var deletionSendingResult = await _emailService.SendEmailToAppUsers(EmailType.ConfirmDeletionByAdmin, user, callbackUrl);

        if (!deletionSendingResult.IsSuccessful)
        {
            TempData.TempDataMessage("Error", deletionSendingResult.Message);
        }

        TempData.TempDataMessage("Error", "Wait for confirmation of account deletion from the admin");
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmUserDeletion(string userId)
    {
        if (userId == null)
        {
            return View("Error");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return View("Error");
        }

        //send message to user to delete his or her account
        var actionLink = Url.Action(
            "Delete",
            "Account",
            new { userId = userId },
            protocol: HttpContext.Request.Scheme);

        await _emailService.SendEmailToAppUsers(EmailType.ConfirmDeletionByUser, user, actionLink);
        
        var confirmDeleteVM = new ConfirmUserDeleteViewModel();
        user.MapTo(confirmDeleteVM);

        return View(confirmDeleteVM); //You succesfully confirmed user deletion
    }
}